# 0001. Architecture style

- Status: Accepted
- Date: 2026-11-19

## Context

Independent labels earn little from global platforms and get no clear numbers on who listens, so they can't pay artists fairly or plan tours. Launch traits: MediaDelivery, ReadHeavy, PersonalData, MoneyMovement, SeveralBusinessAreas, ExternalPartnerIntegrations, SmallTeam. At global scale the advisor recommends Microservices.

## Decision

Launch as a modular monolith (Catalogue, Playback, Playlists, Royalties) with audio behind a CDN, play events written idempotently, and royalties calculated from an append-only play ledger. Evolution trigger: Move play-event ingestion into a separate pipeline when events pass ~5,000/second; extract Search as a cached read service when it needs its own scaling (reference #5).

## Alternatives considered

- Microservices with an event stream from day one: the global-scale answer, too heavy for the launch catalogue.
- Counting plays in the client and uploading daily totals: easy, but can't be audited and invites fraud.

## Consequences

- Module boundaries match the future services, so extraction is a deployment change, not a rewrite.
- One deployable and one database for the launch team to run.
