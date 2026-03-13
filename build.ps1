##########################################################################
# Bootstrap script for Cake Frosting build on Windows (PowerShell)
##########################################################################
[CmdletBinding()]
Param(
    [string]$Target        = "Default",
    [string]$Configuration = "Release",
    [string]$Artifacts     = "./artifacts"
)

$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

# Restore dotnet local tools (Husky, …)
dotnet tool restore

# Run the Cake Frosting build project
dotnet run --project build -- --target=$Target --configuration=$Configuration --artifacts=$Artifacts @args
