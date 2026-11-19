# Runbook

## Prerequisites
.NET 8 SDK, Node 22, (optional) ffmpeg to regenerate demo audio.

## Run the API (port 5105, seeded demo data in Development)
```bash
dotnet build src/Wavelength.Api/Wavelength.Api.csproj
ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5105 dotnet run --project src/Wavelength.Api --no-build
curl localhost:5105/health/ready
curl "localhost:5105/api/search?q=meridien"
```
Config: `Royalties:RatePerPlay` (decimal, default 0.05), `Royalties:Currency` (default ZAR), `Wavelength:SeedDemoData` (true in Development).

## Run the web app (port 5205, proxies /api, /cdn, /health to 5105)
```bash
cd web && npm ci && npm run dev    # http://localhost:5205  (?q=meridien pre-fills the search)
```

## Run the tests
```bash
dotnet test                                  # CI (needs NuGet)
# offline workspace without NuGet:
/home/claude/tools/run-xunit-offline.sh tests/Wavelength.Tests src/Wavelength.Api/Wavelength.Api.csproj \
  src/Wavelength.SharedKernel/Wavelength.SharedKernel.csproj src/Modules/*/Wavelength.*.csproj
cd web && npx vitest run && npm run build
```

## Demo audio
`tools/generate-audio.sh` regenerates the six invented clips in `src/Wavelength.Api/wwwroot/cdn/audio` (~360 KB). Clips loop in the player until the track's catalogue duration.

## Deploy
Not yet: one container for the API, `web/dist` and `wwwroot/cdn` behind a CDN. Written with slice 2.

## Monitor and alerts
`/health/live`, `/health/ready`. Page on any statement with `reconciliation.reconciled = false` (see `quality.md`).

## Common incidents
- **Plays not counted:** check `POST /api/plays` responses for 400/404/409; a 500 means the Royalties consumer failed and the client should retry with the same play ID.
- **Ledger empty after restart:** expected in slice 1 (in-memory). Fixed by slice 2 persistence.
