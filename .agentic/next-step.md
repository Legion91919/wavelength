# Next step

- **Doing:** nothing in progress; slice 1 is complete.
- **Done so far:** Sketch leg, slice 1 (API + web), design dossier (`docs/DESIGN.md`). 78 .NET + 10 vitest tests green.
- **Next action:** start slice 2. Add a Postgres-backed `RoyaltyLedger` and play-session store behind the same module-internal types, with a transactional outbox for `TrackPlayQualified`; first write a failing test that a qualified play survives an app restart.
- **Blocked by:** nothing (NuGet access needed for EF Core in this workspace).
