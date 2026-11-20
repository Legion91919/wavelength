#!/usr/bin/env bash
# Runs every test suite in this repo, the same way CI does.
set -euo pipefail
cd "$(dirname "$0")/.."
echo "==> dotnet test Wavelength.sln -c Release --nologo"
dotnet test Wavelength.sln -c Release --nologo
echo "==> (cd web && npx vitest run)"
(cd web && npx vitest run)
echo "==> (cd web && npm run build)"
(cd web && npm run build)
echo 'All tests passed.'
