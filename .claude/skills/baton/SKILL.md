---
name: baton
description: Orchestrate a project with the Baton model - Frame, Map, Decide, Sketch, then a slice loop (test, build, prove, demo), then Pass the Baton to the owner. Use when starting from a brief or idea, when asked to "build this properly", or to check which leg a project is on.
---

# Baton: run the whole race

Every agent picks up the same baton, the project's hidden `.agentic/` folder. It reads the baton, does one leg, writes what it learned back into it, and hands it on. The last leg hands the baton to the people who will own the system.

```
 brief                                                                      owner
   │                                                                          ▲
   ▼                                                                          │
 FRAME ─▶ MAP ─▶ DECIDE ─▶ SKETCH ─▶ ┌────────── SLICE LOOP ──────────┐ ─▶ PASS THE BATON
                                     │ failing test ▶ build ▶ prove ▶ demo │
                                     └───────────────┬────────────────┘
                                                     │ lessons.md feeds the next race
```

## Rule zero
Nothing gets built until three things exist: the frame (`product.md`), a decision (ADR-0001) and a sketch (tokens plus one chosen screen or API contract). The agent setup is the first thing you ship.

## The legs

| Leg | Skill | Runner (subagent) | You may hand on when… |
|---|---|---|---|
| Frame | `frame` | `framer` | the problem fits in one sentence, risks are ranked, `.agentic/` exists |
| Map | `map-domain` | `domain-mapper` | users, data and traits have evidence; flows are numbered; slice 1 has acceptance criteria |
| Decide | `decide-architecture` | `architect` | the advisor has run, ADR-0001 lists rejected options, and there's a context diagram |
| Sketch | `sketch-ui` | `ui-sketcher` | three variants were compared, one was chosen, and all four states are drawn |
| Slice loop | `blueprint`, then `build-dotnet` / `build-web` / `build-mobile-firebase`, `data-flows`, `ai-in-product` | `dotnet-builder`, `web-builder`, `data-builder`, `ai-builder` (or an outside executor such as Copilot following a blueprint) | each slice is demoable and its tests are green |
| Prove | `prove-it` | `prover`, then `critic` | CI gates are on and green, and the critic reports no blockers |
| Pass the baton | `pass-the-baton` | `handoff-writer` | `arch-advisor check <path> --min 80` passes and an owner has deployed it once |

Map, Decide and Sketch inform each other: run them close together and revisit earlier legs when a later one finds a gap. Data and AI work happens *inside* slices, never as a separate phase at the end.

## How to orchestrate
1. Run `detect_stack` on the path. For a new project, run `scaffold_agentic_context`.
2. Give each runner three things: its skill, the files it may touch, and the condition for handing on.
3. Check that condition yourself before the next leg starts. If it isn't met, send the work back with the specific gap.
4. Add one line to `.agentic/changelog.md` per completed leg or slice, and overwrite `.agentic/next-step.md` before stopping.
5. Stop for a human on anything hard to undo or that costs money (architecture style, data store, cloud, paid services).

## Dropped batons (common failures)
- Skipping the sketch for "backend-only" work. The API contract *is* the sketch, so design it first.
- AGENTS.md growing into a novel. Knowledge goes into the right `.agentic/` file instead.
- Building the whole backlog before the first demo. Run one slice at a time.
