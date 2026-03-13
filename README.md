# sharppg_query
C# bindings for the libpg_query from postgreSQL to be able to parse SQL and its AST

## Overview

**SharpPgQuery** is a C# library that wraps [libpg_query](https://github.com/pganalyze/libpg_query) — the PostgreSQL parser extracted as a standalone C library — to provide access to the Abstract Syntax Tree (AST) of any SQL statement from managed .NET code.

The API follows patterns established by the [Roslyn SDK](https://github.com/dotnet/roslyn) (`SyntaxTree`, `SyntaxNode`, `Diagnostic`, …) to give .NET developers a familiar and productive experience. Memory usage is kept small through lazy node materialisation backed by `System.Text.Json`'s pooled `JsonDocument`.

### Targets

| Framework       | Purpose                                       |
|-----------------|-----------------------------------------------|
| `netstandard2.0`| Roslyn analyzer hosts, older .NET runtimes    |
| `net10.0`       | Modern .NET applications                      |

### Supported OS

| Platform      | Architecture |
|---------------|--------------|
| Linux         | x64          |
| macOS         | x64, arm64   |

---

## Installing libpg_query

The NuGet package **embeds** the compiled `libpg_query` native library for each supported platform. The library is **not committed to this repository** — it is built from source as part of the `Pack` pipeline.

If you are consuming the published NuGet package, no manual installation is required.

### Building from source (for contributors / CI)

Use the `BuildNative` Cake Frosting target to automatically clone and compile libpg_query on the current platform:

```bash
./build.sh --target=BuildNative
```

This will:
1. Clone [libpg_query](https://github.com/pganalyze/libpg_query) into `native/src/libpg_query/` (skipped if already present)
2. Compile the static archive with `make`
3. Link it into a shared library placed at `native/{rid}/libpg_query.{so,dylib}`

Subsequent runs are **idempotent** — if the library already exists, the task skips immediately.

You can also build the shared library manually if you prefer, and place it at the expected path:

```
native/
  linux-x64/libpg_query.so
  osx-x64/libpg_query.dylib
  osx-arm64/libpg_query.dylib
```

#### Linux (manual)

```bash
git clone --depth 1 https://github.com/pganalyze/libpg_query.git
cd libpg_query
make

# Build a shared library (.so) from the static archive
gcc -shared -fPIC -o libpg_query.so \
    -Wl,--whole-archive libpg_query.a \
    -Wl,--no-whole-archive -lm

# Copy to the expected native directory
mkdir -p ../native/linux-x64
cp libpg_query.so ../native/linux-x64/
```

#### macOS (manual)

```bash
git clone --depth 1 https://github.com/pganalyze/libpg_query.git
cd libpg_query
make

# Build a dynamic library (.dylib)
gcc -dynamiclib -o libpg_query.dylib \
    -Wl,-all_load libpg_query.a \
    -lm

# Copy to the expected native directory (adjust rid as appropriate)
mkdir -p ../native/osx-arm64   # or osx-x64
cp libpg_query.dylib ../native/osx-arm64/
```

---

## Quick start

```csharp
using SharpPgQuery;
using SharpPgQuery.Syntax;

// Parse a SQL query
using var tree = PgQuery.ParseSql("SELECT id, name FROM users WHERE active = true");

if (tree.HasErrors)
{
    foreach (var diag in tree.GetDiagnostics())
        Console.WriteLine(diag);
    return;
}

var root = tree.GetRoot();

// Walk all nodes
foreach (var node in root.DescendantNodes())
{
    if (node.Kind == PgSyntaxKind.SelectStatement)
        Console.WriteLine("Found a SELECT statement");
}

// Normalize (replace literals with $n placeholders)
string normalized = PgQuery.NormalizeSql("SELECT * FROM t WHERE id = 42");
// → "SELECT * FROM t WHERE id = $1"
Console.WriteLine(normalized);
```

---

## Building

The project uses [Cake Frosting](https://cakebuild.net/docs/running-builds/runners/cake-frosting) as its build system.
The Frosting runner is a regular C# console project located in `build/`, so no global tool installation is required.

### Prerequisites

- [.NET SDK 8+](https://dot.net/) (the build project targets `net8.0`; the library targets `netstandard2.0` and `net10.0`)
- C toolchain (`gcc`/`clang`, `make`) — only needed to build the native library from source

### Common targets

```bash
# Build the native library from source (run once before anything else)
./build.sh --target=BuildNative

# Run tests (default target – also runs BuildNative if native lib is missing)
./build.sh

# Compile the solution only
./build.sh --target=Build

# Run the xUnit test suite
./build.sh --target=RunTests

# Create the NuGet package (builds native lib + C# code, then packs)
./build.sh --target=Pack

# Debug configuration
./build.sh --target=RunTests --configuration=Debug

# Generate the DocFX site
./build.sh --target=Docs
```

On Windows use `build.ps1` with the same arguments.

You can also invoke the Frosting runner directly:

```bash
dotnet run --project build -- --target=RunTests
```

---

## Project structure

```
sharppg_query/
├── build/                    # Cake Frosting build project
│   ├── Build.csproj
│   ├── Program.cs
│   ├── BuildContext.cs
│   └── Tasks/                # One file per Cake task (includes Docs target)
├── build.sh / build.ps1      # Bootstrap scripts (Unix / Windows)
├── SharpPgQuery.slnx         # Solution file
├── dotnet-tools.json         # Husky local tool
├── .husky/
│   └── task-runner.json      # Git-hook task definitions (Husky.Net)
├── native/
│   ├── src/                  # (gitignored) libpg_query source, cloned by BuildNative
│   ├── linux-x64/            # (gitignored) libpg_query.so — built by BuildNative
│   ├── osx-x64/              # (gitignored) libpg_query.dylib — built by BuildNative
│   └── osx-arm64/            # (gitignored) libpg_query.dylib — built by BuildNative
├── docs/                     # DocFX project (generated site in docs/_site)
└── src/
    ├── SharpPgQuery/         # Main library
    │   ├── PgQuery.cs        # Static entry points
    │   ├── PgSyntaxTree.cs   # Abstract tree base
    │   ├── PgSqlSyntaxTree.cs# Concrete SQL tree
    │   ├── PgQueryException.cs
    │   ├── Diagnostics/      # PgDiagnostic, PgDiagnosticSeverity
    │   ├── Syntax/           # PgSyntaxNode, PgSyntaxNodeList, PgSyntaxKind
    │   └── Native/           # P/Invoke layer (PgQueryNativeMethods)
    └── SharpPgQuery.Tests/   # xUnit test suite
```

---

## Git hooks (Husky)

Git hooks are managed by [Husky.Net](https://alirezanet.github.io/Husky.Net/). The `.husky/task-runner.json` file is where hooks are configured. To activate them after cloning:

```bash
dotnet tool restore
dotnet husky install
```

---

## NuGet package

The published package embeds the native `libpg_query` libraries under `runtimes/{rid}/native/` so consumers do not need to install anything separately:

| Runtime identifier | Library file         |
|--------------------|----------------------|
| `linux-x64`        | `libpg_query.so`     |
| `osx-x64`          | `libpg_query.dylib`  |
| `osx-arm64`        | `libpg_query.dylib`  |

---

## License

This project is licensed under the [MIT License](LICENSE).

`libpg_query` is copyright PostgreSQL Global Development Group and licensed under the PostgreSQL License. See [libpg_query/LICENSE](https://github.com/pganalyze/libpg_query/blob/main/LICENSE) for details.
