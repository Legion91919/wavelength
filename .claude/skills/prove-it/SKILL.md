---
name: prove-it
description: Make every claim of "done" provable - test layers, CI gates (build, tests, coverage ratchet, lint, secret and dependency scans, privacy guard, readiness score), SLOs, telemetry and safe rollout. Use when setting up CI, deciding what done means, reviewing coverage, or before a release or hand-off.
---

# Prove it

"It works on my machine" is a claim. A green pipeline that anyone can re-run is proof.

## Layers of proof
| Layer | Proves | Tooling (.NET / web) | When |
|---|---|---|---|
| Rules | the domain logic is right | xUnit / Vitest | every push |
| Seams | each adapter and the database really work | WebApplicationFactory + Testcontainers | every push |
| Contracts | APIs, data feeds and AI outputs keep their shape | OpenAPI diff, JSON Schema tests | every push |
| Journeys | a real user can finish each flow | Playwright + axe | every PR |
| Accuracy | AI output is good enough | eval runner on a labelled set | when prompts or models change |
| Boundaries | modules don't reach into each other | NetArchTest / reflection tests | every push |

## The pipeline (copy `templates/ci/dotnet-ci.yml`)
1. Build with warnings as errors.
2. Tests with coverage, failing below the bar. Start at 70% and only ever raise it.
3. Format and lint.
4. Secret scan (gitleaks), vulnerable-package check, privacy guard.
5. `arch-advisor check . --min 80`.
6. Release: deploy to a staging slot, smoke test, then swap, or ramp behind a feature flag.

## Running in production
- Put SLIs and SLOs in `.agentic/quality.md`: availability, p95 latency, data freshness for jobs, accuracy for AI.
- Trace with OpenTelemetry from the edge through workers, the database and outside calls.
- Alert when the error budget is burning fast, not on every single error.

## The critic
Before merging, hand the change to the `critic` subagent. It works from a fresh context and has never seen the work, so it can't go easy on its own output. Blockers get fixed and suggestions become tickets.

## Hand on when
Every gate is on and green, SLOs are written down, and the critic found no blockers.
