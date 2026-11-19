---
name: ai-in-product
description: Put LLM features inside an application safely - extraction, classification, ranking and summaries with schema-checked output, per-field confidence, a human review queue, an eval set in CI, versioned prompts and a cost budget. Use when a product feature calls a model (not for AI coding tools).
---

# AI in the product

## The shape of every AI feature
```
input → [deterministic pre-processing] → model call (structured output) → validate → confidence?
          high → auto-accept (logged)         low / invalid → human review queue → correction stored → eval set grows
```

## Rules
1. **Structured output only.** Define the result as a JSON Schema / C# record. Reject anything that doesn't validate; retry once with the validation error, then route to a person.
2. **Confidence per field**, not per document. Thresholds are config, tuned from the eval set.
3. **Traceability.** Store input reference (doc + page + bounding box where possible), model name + version, prompt version, raw output, final value, who approved it.
4. **Prompts are code.** Versioned files in `prompts/` with an ID; changes go through review and must not drop the eval score.
5. **Eval set before launch.** 30–100 labelled real-shaped (but invented/anonymised) examples. Metric per field (exact match, numeric tolerance). CI runs the eval on prompt or model changes and fails below threshold.
6. **Deterministic first.** Use rules/regex/parsers for what they do well (dates, ISINs, totals that must sum); the model handles the messy rest.
7. **Budget.** Track tokens, cost and latency per call; set a monthly cap and alerts.
8. **Data protection.** No client data to a provider without a contract allowing it. Offer a local-model path (Ollama) for sensitive deployments; keep the model behind an interface (`IExtractionModel`) so it can be swapped.

## Recipes
- **Documents in, records out**: keep the original → classify the type → extract with that type's schema → cross-check (totals add up, dates in range) → review queue → write the record with a link to the page.
- **Ranking a queue**: a transparent rules score (weights in config) decides the order; the model only writes the one-paragraph *why*. Never show a number nobody can explain.
- **Narratives over numbers**: figures always come from the data layer; the model writes words around them and is told never to compute.

## Done when
Eval passes its threshold in CI, low-confidence and invalid outputs reach a person, and every accepted value is traceable to its source.
