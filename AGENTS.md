# AGENTS.md

Wavelength: Independent labels earn little from global platforms and get no clear numbers on who listens, so they can't pay artists fairly or plan tours.

## Read first
Everything about this project lives in the hidden `.agentic/` folder.
Start with `.agentic/README.md`; it says which file answers which question.
Load only the files your task needs.

## Hard rules
- Work in thin vertical slices: one ticket from `.agentic/backlog.md` at a time.
- No change without a test that fails first. Run the full test suite before you finish.
- A decision that is hard to reverse gets an ADR in `.agentic/decisions/`.
- Never commit secrets, real client data or personal data. Use `.env.example`.
- Do not grow this file. New knowledge goes in the right `.agentic/` file; history goes in `.agentic/changelog.md`.
- When you learn something that would help on the next project, add it to `.agentic/lessons.md`.

## What is live
- Slice 1: search (`/api/search`), play events (`POST /api/plays`, 30 s rule, once per play ID), label statement + ledger (`/api/labels/{id}/statement|ledger`), minimal playlists. In-memory stores; demo data seeded in Development.
- Web: search + now-playing bar + queue (`web/`, port 5205 → API 5105).
- Design: `.agentic/ux/screens/search-now-playing.html`; dossier `docs/DESIGN.md`.
