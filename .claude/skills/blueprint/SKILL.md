---
name: blueprint
description: Planner/executor split for multi-agent work - one agent writes a precise blueprint for a single task, a second agent or tool (Copilot, Codex, Cursor, Kimi, a cheaper model) carries it out, and the planner reviews the result against it. Also keeps the .agentic/next-step.md resume point. Use when handing a slice to another agent or tool, when token budget matters, or at the start and end of every session.
---

# Blueprint: plan once, execute cheaply, review strictly

A strong model is best spent on thinking and reviewing. Typing out the code can go to a faster or cheaper executor, as long as the instructions leave nothing to guess.

## The loop
1. **Planner** reads the baton (`.agentic/`) and writes `.agentic/blueprints/<nn>-<task>.md` in the format below.
2. **Executor** follows it literally: no extra features, no files outside the list.
3. **Executor** runs the checks the blueprint names (typecheck, tests, lint) and pastes the output at the bottom of the blueprint.
4. **Planner** reviews the diff against the blueprint's acceptance checks. If it passes, tick it in `backlog.md`. If not, append a "Round 2" section with the specific fixes.

## Blueprint format
```markdown
# Blueprint NN: <task>
Goal: <one sentence, user-visible outcome>
Slice: <link to the backlog item>

## Files
- create: path/to/new/file.ts   (what it contains)
- change: path/to/existing.ts   (exact function or section, and the change)
- do not touch: <anything tempting but out of scope>

## Steps
1. … (small enough that each step compiles)

## Contracts
<types, function signatures, API shapes, copied exactly>

## Acceptance checks
- [ ] command: `npm run typecheck` passes
- [ ] command: `npm test -- <pattern>` passes
- [ ] behaviour: <what a person sees>

## Executor output
<pasted by the executor>
```

## Rules for good blueprints
- Name every file. If the executor has to search, the blueprint isn't finished.
- Give contracts as code, not prose.
- One blueprint is one slice or less, and should take under an hour to execute.
- State what *not* to do (common traps: touching generated files, adding dependencies, changing styles).

## The resume point: `.agentic/next-step.md`
At the **end** of every session, overwrite it with what you're doing, what's done, the next concrete action and anything blocking you. At the **start** of every session, read it first. A paused line of work gets its own file (`next-step.<topic>.paused.md`), so it doesn't get mixed into the current one.

## Telling other tools about the setup
Copilot, Codex and Cursor read `AGENTS.md`. `.github/copilot-instructions.md` should just point there and describe the executor role, rather than repeating the rules.
