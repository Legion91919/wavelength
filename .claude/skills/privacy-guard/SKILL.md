---
name: privacy-guard
description: Keep private projects private while still learning from them - rules for turning private work into generic, publishable skills and lessons, plus the pre-commit privacy guard that blocks private names, secrets and personal data. Use before committing to any public repo, when writing lessons from private work, or when making a repo public.
---

# Privacy guard

## The rule
Public repos contain **generic tools, generic skills and invented demo domains**. Nothing that lets someone recognise a private project, client, person or dataset.

## Turning private work into a public lesson
| Private observation | Public lesson |
|---|---|
| "In <private game> the shop screen overlapped on iPads" | "Check every UI screen at tablet 3:4; anchors break on wide screens" |
| "Client X's monthly totals never matched" | "Reconcile totals against the source after every load; fail loudly" |

Strip: names, places, story, art, numbers, dates, people, internal URLs, file paths.

## The guard (`tools/privacy_guard.py`)
- Blocks terms listed in a **private denylist** (default `~/.agentic/denylist.txt`, or a `_private/denylist.txt` next to your repos). Store entries as hashes (`python tools/privacy_guard.py --hash "Some Name"` prints a `sha256:` line), so even the denylist never contains the real names. The denylist itself is never committed.
- Blocks common secrets (private keys, AWS/Azure/GitHub/OpenAI/Anthropic-style keys, connection strings with passwords) and South African ID numbers / email addresses outside an allow-list.
- Install as a pre-commit hook: `python tools/privacy_guard.py --install` (in each public repo).
- Run over everything before making a repo public: `python tools/privacy_guard.py --all`.

## Local-only data
The local RAG index, fine-tuning datasets, model weights and the master plan live outside public repos (a `_private/` folder beside them, `~/.agentic/`). The kit's `.gitignore` also blocks `.private/` and `*.private.*` as a safety net.
