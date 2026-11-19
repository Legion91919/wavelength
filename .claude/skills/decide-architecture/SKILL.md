---
name: decide-architecture
description: Choose and record a solution architecture with the arch-advisor MCP server - traits to style, patterns, risks and evolution triggers, compared against 18 reference systems, then an ADR and a C4 context diagram. Use when choosing an architecture, comparing monolith vs modular monolith vs microservices, or justifying a design to a client.
---

# Decide the architecture

## The rule
Pick the **simplest architecture that survives the known forces**, and write down when you'd change it. Most business systems are a modular monolith; microservices are earned by independent scaling needs *and* a team that can run them.

## Steps
1. Read `.agentic/product.md` → Traits. If traits are missing, run the `map-domain` skill first.
2. Call `recommend_architecture` with those traits. Read the style, patterns, risks and closest reference systems.
3. **Apply judgement** on top of the deterministic answer. Things the advisor can't know:
   - The client's existing skills and cloud (an Azure/.NET shop shouldn't get a Go/AWS design).
   - Hard deadlines (a 6-week MVP is never microservices).
   - Who will run it after handover.
4. Call `write_adr` for the style decision. **Alternatives must include at least one simpler and one more complex option, with why each lost.**
5. Update `.agentic/architecture.md`: style and why, a Mermaid C4 context diagram, modules with what they own, cross-cutting concerns (auth, logging, tracing, config, errors).
6. One ADR per hard-to-reverse decision after that (database, cloud, auth provider, messaging).

## Reference systems (call `list_reference_examples` for the full table)
Static site · employee tool · e-commerce · online gambling · streaming · internet provider · food delivery · car rental · banking · hotel/airline booking · clinic · IoT · multi-tenant SaaS · chat · **document extraction with review · multi-source reporting warehouse · piece-by-piece replacement · lending back office**.

Explain the recommendation by analogy: *"This is closest to a lending back office: money workflows that run for years, so a modular monolith with an append-only ledger and sagas for drawdowns."*

## Module boundaries (modular monolith)
- One folder/project per module; a module exposes a small public interface, everything else is `internal`.
- No module reads another module's tables. Enforce with an architecture test (NetArchTest or a reflection test).
- Modules talk in-process through interfaces or in-process events; the outbox pattern when an event must survive a crash.

## Done when
ADR-0001 exists with rejected alternatives, `architecture.md` has a diagram and module list, and the risks from the advisor each have a mitigation or an explicit "accepted".
