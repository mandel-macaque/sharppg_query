# Introduction

SharpPgQuery wraps the PostgreSQL parser extracted as [`libpg_query`](https://github.com/pganalyze/libpg_query) and exposes the resulting syntax tree to .NET applications.

The library mirrors Roslyn concepts (`SyntaxTree`, `SyntaxNode`, `Diagnostic`, …) so that the SQL parsing workflow feels familiar to C# developers. The NuGet package ships platform-specific builds of `libpg_query` for Linux `x64` and macOS (`x64` and `arm64`), so consumers do not need to compile the native dependency themselves.

For contributors, the `BuildNative` Cake target will clone and build `libpg_query` locally so the managed project can be compiled and tested.
