##########################################################################
# Bootstrap script for Cake build on Windows (PowerShell)
##########################################################################
[CmdletBinding()]
Param(
    [string]$Target        = "Default",
    [string]$Configuration = "Release",
    [string]$Artifacts     = "./artifacts"
)

$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

# Restore dotnet local tools (Cake, Husky, …)
dotnet tool restore

# Forward arguments to Cake
dotnet dotnet-cake build.cake --target=$Target --configuration=$Configuration --artifacts=$Artifacts @args
