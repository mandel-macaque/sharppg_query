#!/usr/bin/env bash
##########################################################################
# Bootstrap script for Cake Frosting build on Unix (macOS / Linux)
##########################################################################
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Restore dotnet local tools (Husky, …)
dotnet tool restore

# Run the Cake Frosting build project, forwarding all arguments
dotnet run --project build -- "$@"
