# Backlog

Vertical slices, smallest valuable first.

## Now
- [x] Sketch leg: tokens + three variants of the slice-1 screen (`sketch-ui`)
- [x] Slice 1: A listener searches, plays a track and queues the next one, and each play over 30 seconds is counted once for the label's royalties
  - Given a search term, then matching tracks, artists and albums are returned
  - Given a play that lasts 31 seconds, then exactly one royalty play is recorded for the label
  - Given the same play event sent twice, then it's still counted once
  - Given a play of 10 seconds, then no royalty play is recorded
  - Test notes: one test per criterion; seeded invented data. (Mapped in `docs/DESIGN.md` section 13.)

## Next
- [ ] Slice 2: persistence (Postgres via EF Core) behind the same repository interfaces, transactional outbox for `TrackPlayQualified`, play-session expiry
- [ ] CI green with all gates

## Later
- [ ] Listener accounts (OIDC) and label-manager authorisation on statements
- [ ] Gapless playback: preload the next queue item
- [ ] Offline play-report buffer on the client
- [ ] Label statement screen
