#!/usr/bin/env bash
##########################################################################
# Bootstrap script for Cake build on Unix (macOS / Linux)
##########################################################################
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Restore dotnet local tools (Cake, Husky, …)
dotnet tool restore

# Forward all arguments to Cake
dotnet dotnet-cake build.cake "$@"
