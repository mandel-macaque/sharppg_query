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

The NuGet package **already bundles** the compiled `libpg_query` native library for the supported platforms, so no manual installation is required when consuming the package.

If you are building the project from source, or want to use a system-wide installation, follow the instructions below.

### Linux

```bash
# Option A – build from source (recommended, ensures the correct version)
git clone --depth 1 https://github.com/pganalyze/libpg_query.git
cd libpg_query
make

# Build a shared library (.so) from the static archive produced by make
gcc -shared -fPIC -o libpg_query.so \
    -Wl,--whole-archive libpg_query.a \
    -Wl,--no-whole-archive -lm

# Install system-wide (optional)
sudo install -m 644 libpg_query.so /usr/local/lib/libpg_query.so
sudo ldconfig
```

### macOS

```bash
# Option A – build from source
git clone --depth 1 https://github.com/pganalyze/libpg_query.git
cd libpg_query
make

# Build a dynamic library (.dylib)
gcc -dynamiclib -o libpg_query.dylib \
    -Wl,-all_load libpg_query.a \
    -lm

# Install system-wide (optional)
sudo install -m 644 libpg_query.dylib /usr/local/lib/libpg_query.dylib

# Option B – Homebrew (if a formula is available)
brew install libpg_query
```

Copy the resulting shared library into the appropriate folder so it is bundled into the NuGet package:

```
native/
  linux-x64/libpg_query.so
  osx-x64/libpg_query.dylib
  osx-arm64/libpg_query.dylib
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

The project uses [Cake](https://cakebuild.net/) as its build system.

### Prerequisites

- [.NET SDK 10](https://dot.net/) (or any SDK listed in `global.json`)
- C toolchain (`gcc`/`clang`, `make`) — only needed to build the native library from source

### Common targets

```bash
# Run tests (default)
./build.sh

# Build only
./build.sh --target=Build

# Create the NuGet package
./build.sh --target=Pack

# Run tests AND pack
./build.sh --target=Publish

# Release configuration
./build.sh --target=Test --configuration=Release
```

On Windows use `build.ps1` with the same arguments.

---

## Project structure

```
sharppg_query/
├── build.cake                # Cake build script
├── build.sh / build.ps1      # Cake bootstrap scripts
├── SharpPgQuery.slnx         # Solution file
├── .config/
│   └── dotnet-tools.json     # Cake + Husky local tools
├── .husky/
│   └── task-runner.json      # Git-hook task definitions (Husky.Net)
├── native/
│   ├── linux-x64/            # libpg_query.so (Linux x86-64)
│   ├── osx-x64/              # libpg_query.dylib (macOS Intel)
│   └── osx-arm64/            # libpg_query.dylib (macOS Apple Silicon)
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
