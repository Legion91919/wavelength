---
name: design-dossier
description: Write a project's design dossier (docs/DESIGN.md) and README design summary - brief, requirements with capacity estimates, every architecture option compared with compare_architectures, the decision and why, C4 diagrams, communication (sequence) flows, data flow, data model, scaling path, failure modes, security, observability, test strategy and interview-style Q&A. Use after the Decide leg, before a review or hand-off, or when asked to explain or justify a design.
---

# Design dossier

One document a reviewer, a client or an interviewer can read in 15 minutes and understand **what** was built, **why this way**, and **what would change at 100x**.

## Inputs
`.agentic/product.md`, `.agentic/architecture.md`, `.agentic/decisions/`, the code (modules, endpoints, data stores).

## Steps
1. Run `compare_architectures` with the product's traits (and again with its global-scale traits, if it has them). Paste the comparison table and use its reasons in section 4.
2. Write `docs/DESIGN.md` with the sections below. Every diagram is Mermaid and must match the code (module names, endpoints, stores).
3. Put a short **Design at a glance** section at the top of `README.md` (brief in 2–3 lines, chosen architecture and the reason in one paragraph, a link to `docs/DESIGN.md`, a context diagram).
4. Check: every module in the code appears in a diagram; every alternative has a "why not"; every number has a source or an assumption.

## `docs/DESIGN.md` sections
1. **Brief**: client, problem, users, outcomes (from `product.md`).
2. **Requirements**: functional (the slices); non-functional with numbers (latency, availability, freshness, data retention, privacy).
3. **Capacity estimate**: back-of-envelope sums (users, requests per second at peak, storage per year, bandwidth). State assumptions; round generously.
4. **Options considered**: the compare table, then 2–4 lines per serious option on why it won or lost. Include the global-scale answer and the evolution trigger from ADR-0001.
5. **Architecture**: C4 context and container diagrams; modules, what each owns, and the contracts between them.
6. **Communication flows**: a sequence diagram for each slice and for the riskiest flow (retry, offline, failure). Show sync vs async, and where idempotency keys and outbox messages are used.
7. **Data flow**: where data enters, is transformed, stored and leaves (flowchart). Mark personal data and money.
8. **Data model**: an ER diagram of the main entities, with ownership per module and the consistency rule for each (strong, eventual).
9. **Scaling path**: what breaks first and what you'd do (cache, read model, partition, extract service, CDN), in order.
10. **Failure modes**: table of failure → effect → mitigation (dependency down, duplicate request, poison message, region outage, hot key).
11. **Security and privacy**: authN/authZ, data protection (POPIA/GDPR), secrets, threat highlights per flow.
12. **Observability**: SLIs/SLOs, key metrics, traces, the alert that would wake someone.
13. **Testing strategy**: what each test layer covers here, and how acceptance criteria map to tests.
14. **Interview questions**: 6–10 likely questions ("How would you scale the feed to 10M users?", "What if the payment webhook arrives twice?") with crisp answers grounded in this design.

## Style
Plain words, short sentences, numbers over adjectives. Diagrams are small and readable. Nothing in the dossier that the code contradicts.
