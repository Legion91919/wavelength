---
name: frame
description: First leg of the Baton model - turn an email, spec, call notes or a loose idea into a framed problem with ranked risks, open questions and a scaffolded .agentic/ folder. Use at the very start of any project, or when someone pastes a brief and asks how you'd approach it.
---

# Frame

A good frame makes the rest of the race obvious. A bad one means you'll build the wrong thing very efficiently.

## Steps
1. **Read for the problem, then read for the traps.** On a second read, look for words that change everything: "regulated", "audit", "old system", "spreadsheets", "time zones", "they need to own it".
2. **One sentence**: `<who> can't <do what> because <why>, and it costs <impact>.` Every empty slot becomes a question for the Map leg.
3. **Who wins, who worries.** For each person involved, write what they gain and what they're afraid of (the sponsor fears cost, users fear extra clicks, compliance fears gaps in the record, ops fears 3am calls).
4. **Hard edges.** Write down the law and policy, where data must live, systems you can't change, budget, deadline and team size.
5. **Rank the risks** by "would kill the project" × "how likely". For the top three, name the cheapest test that would settle it: a spike, a question, a clickable sketch.
6. **Scaffold** with `scaffold_agentic_context` (arch-advisor MCP), using the one sentence as the purpose.
7. **Seed `.agentic/product.md`** with what you know. Mark gaps as `TODO(map):`.
8. **Name the first demo**: the thinnest end-to-end path that settles the biggest risk.

## Hand the human this
```
Problem:     <one sentence>
Who wins:    <users> by <measurable change>
Biggest risks: 1. … → settle by …   2. …   3. …
First demo:  <thin path>, in <N> days
Questions:   <5 at most, sharpest first>
```

## Ownership from day one
Write down now who will own the code, cloud accounts, secrets and agent setup at the end, in `.agentic/handover.md`. That way the last leg is a formality, not a scramble.

## Privacy
A brief from a real organisation never goes into a public repo. Demos use invented companies.
