# Decision: search + now-playing screen

Reviewed in headless Chrome at 1440 px and 390 px. Scores 1 (poor) to 5 (great).

| Variant | Main job speed | Clarity | Accessibility | Tone | Total |
|---|---|---|---|---|---|
| A: dense list (mixed table, fixed bar, queue sidebar) | 5 | 4 | 3 | 3 | 15 |
| B: cards (hero search, cover tiles, floating pill + drawer) | 3 | 3 | 2 | 5 | 13 |
| C: queue-first (big now-playing stage, search on the side) | 3 | 4 | 4 | 4 | 15 |

- **A** is fastest from keyboard and shows the most results, but the mixed-type table is hard to scan, rows are too short as tap targets on a phone and the keyboard hints are meaningless there. Feels like an admin tool.
- **B** has the warmest feel, but one search shows three results per screen, play buttons hide on covers, and at 390 px the floating pill crushes the title into five lines and the queue drawer covers results.
- **C** makes "play next" explicit and shows royalty progress clearly, with large targets; on a phone the artwork pushes search below the fold, which hurts the main job.

## Pick: A's layout with C's actions
- A's structure: search in the header, results in a single scroll, fixed now-playing bar at the bottom, queue as a right panel on desktop.
- Results grouped into **Tracks / Artists / Albums** sections instead of one mixed table (clarity).
- C's explicit per-row **Play** and **+ Next** buttons at 44 px, and C's "counted for label" signal in the now-playing bar (text + tick, never colour alone).
- Below 1024 px the queue becomes a bottom sheet opened from a "Queue n" button in the bar; below 640 px durations and the restart button drop out so titles fit.

Final: `screens/search-now-playing.html` (state switcher: loading, empty, error, success).
