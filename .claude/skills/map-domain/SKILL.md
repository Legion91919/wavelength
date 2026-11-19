---
name: map-domain
description: Second leg of the Baton model - map users, jobs, data and failure modes, pick architecture traits with evidence, number the key flows and write acceptance criteria for slice 1. Use after framing and before deciding architecture or sketching screens.
---

# Map the domain

## Questions to answer (and where the answer goes)

| Question | File |
|---|---|
| Who are the users, how often do they use it, what job are they doing? | `product.md` → Users |
| What does success look like in numbers? | `product.md` → Outcomes |
| What data comes in, from where, in what shape, how often? | `product.md` → Constraints, and `architecture.md` later |
| What must never go wrong? (double payment, data leak, wrong number in a report) | `product.md` → Constraints, `quality.md` |
| Which traits apply? | `product.md` → Traits |
| What are the key flows? | `ux/flows.md` |
| What is the first slice? | `backlog.md` → Now |

## Getting real answers
- Ask about the **last time** the problem happened, not about the ideal system. "Walk me through last quarter's report" beats "what features do you want?".
- Ask **what happens when it goes wrong**. That's where the rules, audit and edge cases live.
- Ask **who else touches the data**. That's where integrations hide.
- Ask for **one real (anonymised) example** of every input document or file.

## Picking traits
Call `list_traits` on the arch-advisor MCP server. Pick only traits that are *facts about the problem* and that you can defend with a sentence from discovery. Write the sentence next to each trait in `product.md`:

```
- DocumentIngestion: "applications arrive as scanned PDFs by email, ~200 a month"
- RegulatoryAudit: "investors and auditors ask where every number came from"
- SmallTeam: "3 engineers"
```

## Flows
Number each key job as a flow in `ux/flows.md`. Each step names the screen or API call it uses. Keep to the 3–5 flows that carry most of the value.

## First slice
The smallest end-to-end path that proves the riskiest assumption. Write it as:

```
## Now
- [ ] Slice 1: <user> can <do thing> and sees <result>
  - Given … When … Then …   (3–6 acceptance criteria)
  - Test notes: what proves it works
```

## Done when
`product.md` has no `TODO(map)` left in Problem/Users/Outcomes/Traits, flows are numbered, and slice 1 has acceptance criteria.
