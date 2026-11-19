---
name: build-web
description: Build Angular or React frontends from the UX hand-off - design tokens as CSS variables, typed API clients generated from OpenAPI, all four screen states, accessibility and Playwright end-to-end tests. Use for any Angular, React or web UI work.
---

# Build: web (Angular or React)

## Start from the design, not from a blank page
1. Read `.agentic/ux/decision.md`, the final screen HTML and `ux/components.md`.
2. Turn `ux/design-tokens.json` into CSS custom properties (`:root { --color-accent: … }`). Components use variables only, never hard-coded colours.
3. Build components in the order listed in `components.md`, each with loading / empty / error / success states.

## Defaults
| Concern | Angular | React |
|---|---|---|
| Version | current LTS, standalone components, signals | React 18+/19 with Vite, TypeScript strict |
| State | signals + services | TanStack Query for server state, `useState`/Zustand for UI state |
| API client | generated from the backend OpenAPI (NSwag / openapi-typescript) | same |
| Forms | Reactive Forms | React Hook Form + Zod |
| Tests | Jest/Vitest for logic, Playwright e2e | Vitest + Testing Library, Playwright e2e |

- Generated API client lives in `src/api/generated/`, never hand-edited. Regenerate in CI and fail on drift.
- Numbers: format with `Intl.NumberFormat`, tabular figures, right-aligned in tables.
- Auth: OIDC (MSAL / angular-auth-oidc-client), tokens never in localStorage when avoidable.

## Accessibility (non-negotiable)
- Semantic HTML first; every input has a label; focus visible; keyboard path through every flow.
- Run axe (via `@axe-core/playwright`) in e2e; zero serious violations.

## E2E
One Playwright test per flow in `ux/flows.md`, named after the flow (`flow-2-review-low-confidence-fields.spec.ts`). Run against the real API with seeded invented data.

## Done when
Every flow has a passing e2e test, axe is clean, and the UI matches the chosen screen at 1440px and 390px.
