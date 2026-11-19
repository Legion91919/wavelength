# Handover

What the client needs to own this without us.

- [ ] Source, infrastructure-as-code and pipelines in the client's own accounts
- [ ] Every secret rotated and owned by the client
- [ ] Architecture and decisions explained (`architecture.md`, `decisions/`)
- [ ] Runbook rehearsed by a client engineer
- [ ] Known issues and next steps listed below
- [ ] AI agent setup documented so their team can keep using it

## Known issues
- Slice 1 stores are in memory: the ledger and play sessions are lost on restart (slice 2).
- Play sessions never expire yet; memory grows with plays (slice 2 adds expiry).
- No authentication: any caller can read any label's statement.
- Demo audio clips (6-9 s) loop until the track's catalogue length; gapless preload of the next track is not built.
- Web API client is hand-written, not generated from OpenAPI; no Playwright e2e yet.

## Recommended next steps
- Slice 2 persistence + outbox (see `next-step.md`), then OIDC and per-label authorisation.