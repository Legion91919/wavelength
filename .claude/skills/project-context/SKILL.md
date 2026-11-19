---
name: project-context
description: Create, retrofit and maintain the hidden .agentic/ context folder so agents understand a project without being told every document - stack detection, scaffolding, keeping AGENTS.md short and routing new knowledge to the right file. Use when opening an unfamiliar repo, adding agent support to an existing project, or after a change that new agents would need to know about.
---

# Project context: the hidden `.agentic/` convention

## Why
One growing instruction file becomes noise. Instead, every project has:
- a **short root `AGENTS.md`** (under 80 lines: purpose, hard rules, "read `.agentic/README.md` first"), which Copilot, Cursor, Codex and Gemini read
- **`CLAUDE.md`** containing just `@AGENTS.md`
- a **hidden `.agentic/` folder** with one file per question (map in `.agentic/README.md`)

Agents load only the file their task needs. Humans get a clean root.

## Retrofit an existing project
1. `detect_stack` on the path (reads file names only, safe for private projects).
2. `scaffold_agentic_context` (skips files that already exist).
3. Move knowledge out of any long existing AGENTS.md/README into the right `.agentic/` file. Keep the "Hard rules" in AGENTS.md.
4. Fill `product.md`, `architecture.md` and `runbook.md` from the code (build commands, entry points, folders). Mark guesses `TODO(verify):`.
5. On Windows: `attrib +h .agentic` to hide it in Explorer. Do **not** git-ignore it; it's the project's memory and must be committed.

## Keep it current (every change)
| You learned… | Write it in |
|---|---|
| How to build/run/deploy, an env var, an incident fix | `runbook.md` |
| A hard-to-reverse choice | new ADR via `write_adr` |
| A new module or boundary | `architecture.md` |
| Something reusable on the next project | `lessons.md` (generic wording only) |
| What changed | `changelog.md` (one line) |
| Where you stopped | `next-step.md` (overwrite every session) |

## Never
- Put secrets, client names or personal data in `.agentic/`.
- Let AGENTS.md pass 80 lines (the readiness check warns).
