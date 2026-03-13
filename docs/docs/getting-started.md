# Getting Started

## Install the package

Add the NuGet package to your project:

```shell
dotnet add package SharpPgQuery
```

## Parse SQL

```csharp
using SharpPgQuery;
using SharpPgQuery.Syntax;

using var tree = PgQuery.ParseSql("SELECT id, name FROM users WHERE active = true");

if (tree.HasErrors)
{
    foreach (var diag in tree.GetDiagnostics())
        Console.WriteLine(diag);
    return;
}

PgSqlSyntaxTree root = tree.GetRoot();

foreach (var node in root.DescendantNodes())
{
    if (node.Kind == PgSyntaxKind.SelectStatement)
        Console.WriteLine("Found a SELECT statement");
}
```

## Normalize SQL

`PgQuery.NormalizeSql` replaces literal values with numbered parameters while preserving structure:

```csharp
string normalized = PgQuery.NormalizeSql("SELECT * FROM t WHERE id = 42");
// normalized == "SELECT * FROM t WHERE id = $1"
```
