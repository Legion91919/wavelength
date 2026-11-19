# Components (search + now playing)

Build order follows the screen top to bottom. Every component uses token CSS variables only.

| Component | Props | Loading | Empty | Error | Success |
|---|---|---|---|---|---|
| `SearchBox` | `value`, `onChange(q)`, `busy` | spinner hint "Searching..." | placeholder "Search tracks, artists, albums" | field stays usable | query echoed, results below |
| `SearchResults` | `state: idle\|loading\|empty\|error\|success`, `result?: SearchResult`, `nowPlayingId?`, `onPlay(track)`, `onPlayNext(track)`, `onRetry()` | 4 skeleton rows, `aria-live=polite` | "No matches for ..." with accent tip | `role=alert` panel + Try again | grouped sections with counts and timing |
| `TrackRow` | `track`, `query`, `isPlaying`, `onPlay`, `onPlayNext` | n/a (parent shows skeleton) | n/a | n/a | title, artist, album, duration, Play, + Next; highlighted match |
| `ArtistRow` / `AlbumRow` | `artist` / `album`, `query`, `onPlayAll(tracks)` | n/a | n/a | n/a | name, label, track count, action button |
| `NowPlayingBar` | `current?: Track`, `isPlaying`, `positionSeconds`, `countedState: idle\|pending\|counted\|failed`, `queueLength`, `onToggle`, `onNext`, `onRestart`, `onOpenQueue` | "Loading audio..." while the element buffers | "Nothing playing yet" | audio error text + Skip | title, artist, label, progress, counted tick |
| `QueuePanel` | `items: Track[]`, `open`, `onRemove(index)`, `onClose` | n/a (client state) | "Nothing queued. Use + Next on any track." | n/a | ordered list with remove buttons |
| `AudioEngine` (hook `usePlayer`) | `src`, `durationSeconds`, `onReport(event)` | buffering flag | no track | `error` from the audio element | position, listened seconds, threshold/end reports |

Play reporting: `usePlayer` accumulates *listened* seconds (seeks ignored), posts `POST /api/plays` once at 30 s (`reason: threshold`) and once when the play ends (`reason: ended` or `skipped`), always with the same client-generated `playId`.
