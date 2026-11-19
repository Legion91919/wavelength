# User flows

## Flow 1: search, play, queue next, count the play (slice 1)
1. Listener opens Wavelength. Screen: `screens/search-now-playing.html` (now-playing bar empty state).
2. Types "meridien" in `SearchBox`. Client calls `GET /api/search?q=meridien` (debounced 200 ms). `SearchResults` shows loading, then grouped Tracks / Artists / Albums (accents optional), or empty / error.
3. Presses **Play** on a track. `NowPlayingBar` loads the clip from `/cdn/audio/<file>` into the HTML audio element and the client generates a `playId` (UUID).
4. Presses **+ Next** on another track. It goes to the top of `QueuePanel` (client state).
5. At 30 listened seconds the client sends `POST /api/plays { playId, trackId, listenedSeconds: 30, reason: "threshold" }`. Playback records it, publishes `TrackPlayQualified` once per `playId`, and Royalties appends one ledger entry for the track's label. Bar shows "Counted for <label>".
6. When the track ends (or is skipped) the client sends the same `playId` with `reason: "ended"|"skipped"`. No second royalty entry. The queue advances to the next track with a new `playId`.

## Flow 2: label manager checks a statement (slice 1, API only)
1. `GET /api/labels` lists labels.
2. `GET /api/labels/{labelId}/statement?from=2026-11-01&to=2026-11-30` returns qualifying plays per track, the total and a reconciliation block that must equal `GET /api/labels/{labelId}/ledger?from=...&to=...` to the cent. (Screen comes in a later slice.)
