#!/usr/bin/env bash
# Runs once when the Codespace (or local dev container) is created.
set -euo pipefail
cd "$(dirname "$0")/.."
dotnet restore Wavelength.sln
(cd web && npm ci --no-audit --no-fund)
echo "Setup done. Running every test once so you start from green..."
bash scripts/test-all.sh || echo "Some tests failed: run scripts/test-all.sh to see the details."
