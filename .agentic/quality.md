# Quality

## Definition of done
- Acceptance criteria met and demonstrated.
- Unit tests for rules, integration tests for each adapter, one end-to-end test per key flow.
- CI green: build, tests, lint, dependency and secret scan, web build + vitest.
- Docs in `.agentic/` updated in the same change.

## Service levels
| SLI | SLO |
|---|---|
| API success rate (non-5xx) | 99.9% over 30 days |
| p95 search latency (`GET /api/search`) | < 300 ms |
| p95 play report latency (`POST /api/plays`) | < 200 ms |
| Qualified plays recorded once (ledger appends = qualified events) | 100% |
| Statements reconciled with the ledger to the cent | 100% (any miss pages on-call) |
| Audio start time (first byte from CDN) | p95 < 500 ms |

## Current evidence (slice 1)
- 78 .NET tests (rules, real-HTTP acceptance, module boundaries), 10 vitest tests. See `docs/DESIGN.md` section 13.
