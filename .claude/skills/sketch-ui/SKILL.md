---
name: sketch-ui
description: Variant-first UI sketching (prompt, variants, pick, refine, in the spirit of tools like Google Stitch) - a 5-line brief, design tokens, three genuinely different HTML variants per key screen, a scored review and a hand-off with all states. Use when designing screens, dashboards, portals or forms, or before any frontend code.
---

# Sketch the UI

Fast, visual, iterative: **prompt → variants → pick → refine → hand to frontend**. Everything lands in `.agentic/ux/` so the frontend engineer and future agents build from the same source.

## 1. Brief (5 lines, `ux/brief.md`)
```
Users:       who, on what device, how often, how expert
Main job:    the one thing the screen must make fast
Data:        what's on screen (real field names from discovery)
Tone:        e.g. "institutional, calm, dense but readable"
Must avoid:  e.g. "decorative charts, more than 2 accent colours"
```

## 2. Tokens (`ux/design-tokens.json`)
Colour, type scale, spacing, radius. Check text/background contrast ≥ 4.5:1 (WCAG AA). Financial UIs: tabular numbers (`font-variant-numeric: tabular-nums`), right-aligned amounts, one accent colour, red/green never the only signal.

## 3. Three variants per key screen (`ux/screens/<screen>-a|b|c.html`)
Each variant is a **single self-contained HTML file** using only the tokens (as CSS variables), realistic invented data and no external assets. Make them *genuinely different*:
- **A: Dense table-first** (power users, keyboard).
- **B: Card/summary-first** (executives, glanceable).
- **C: Workflow-first** (a guided queue, e.g. "12 documents need review").

## 4. Review
Open each variant in a browser at desktop 1440px and mobile 390px widths (use the browser tools to screenshot if available). Score each 1–5 on: main job speed, clarity of numbers, accessibility, fits tone. Write the scores and the pick in `ux/decision.md`. Mixing is fine ("B's summary strip on top of A's table").

## 5. Refine and hand off
- Produce `ux/screens/<screen>.html` (final) and update `ux/flows.md` so every flow step names its screen.
- List components the frontend needs (`ux/components.md`): name, props, states (loading, empty, error, success).
- Every screen designs its **empty, loading and error states**, not just the happy path.

## Optional tools
- If a Stitch, Figma or design MCP server is connected, use it for steps 3–4 and export the result into `ux/screens/`.
- For Unity games use the `unity-game` skill's UI rules and check phone and tablet aspect ratios.

## Done when
Tokens pass contrast, the key screen has a chosen final version with all four states, and components are listed.
