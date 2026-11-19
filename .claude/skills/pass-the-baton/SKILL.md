---
name: pass-the-baton
description: Final leg of the Baton model - make a project fully ownable by someone else: code, infrastructure, secrets, runbook and the agent setup itself, verified by the arch-advisor readiness score. Use before delivery, when a project changes hands, or when asked "could someone else run this tomorrow?".
---

# Pass the baton

You're done when the owner doesn't need you anymore. That includes the agents: the owner inherits the skills, subagents and MCP setup that built the system, so their team can keep working the same way.

## Steps
1. Run `check_handover_readiness` (or `arch-advisor check <path> --min 80`). Clear every FAIL, then as many WARNs as you can.
2. Finish `.agentic/handover.md`:
   - Repos, infrastructure-as-code and pipelines live in the owner's accounts
   - Secrets are rotated and owned by them (list them by name only)
   - An owner engineer has had the architecture and the ADRs walked through with them
   - The runbook has been rehearsed: they deploy and roll back once while you watch
   - Known issues with severity and a suggested fix
   - The top five next steps, one line each with the reason
3. Write up the agent setup: which skills, subagents and MCP servers exist, how to run them, and the house rules (`docs/safe-ai-usage.md`).
4. Clear out dead code and stale branches. The README should get a stranger from clone to running app in 15 minutes.
5. Send a short closing note: what they have, how to run it, what's next.

## Done when
Readiness is 80% or higher with no FAIL, and someone other than you has deployed it.
