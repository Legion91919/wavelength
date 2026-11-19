---
name: build-mobile-firebase
description: Build Expo / React Native apps on Firebase in a monorepo - apps/ and libs/ layout, one-way layer dependencies, a Result type for use cases, Firestore query and cost rules, security rules tests, Cloud Functions for anything trusted (payments, moderation), emulators and EAS builds. Use for any Expo, React Native, Firebase or Firestore work.
---

# Build: mobile with Expo and Firebase

## Layout
```
apps/mobile          Expo app: screens, navigation, platform glue only
apps/desktop         optional admin or ops app (Electron or web)
libs/core            domain types, Result type, pure rules (no Firebase imports)
libs/features/<x>    use cases + repository interfaces per feature
libs/data            Firebase implementations of the repositories
functions/           Cloud Functions: anything that must be trusted
```
Dependencies only point **inward**: apps → features → core, and data → features/core. Screens never import Firebase directly. Enforce it with an ESLint boundaries rule (`eslint-plugin-boundaries` or `import/no-restricted-paths`).

## Use cases return a Result
```ts
type Result<T, E = AppError> = { ok: true; value: T } | { ok: false; error: E };
```
Use cases never throw for expected failures (validation, not found, permission). Screens handle both branches, so every error state gets designed.

## Firestore rules of thumb
- Model data for the queries the screens make. Duplicate data on purpose and keep the copies consistent in a Function.
- Every list query has a `limit()` and cursor pagination. There's no unbounded `onSnapshot`.
- Listeners are attached on screen focus and detached on blur. Count reads per screen in development.
- Counters and aggregates are updated by Functions (or `count()` queries), never computed by fetching everything.
- Composite indexes live in `firestore.indexes.json` and are committed.

## Trust boundary
The client is untrusted. Payments, payouts, ratings, moderation, verification and role changes all happen in Cloud Functions. **Security rules** deny by default and are tested with `@firebase/rules-unit-testing` against the emulator in CI.

## Payments (any provider)
Verify every webhook signature and amount on the server, make handlers idempotent (store the provider's transaction ID), keep payment state in its own collection with an append-only status history, and keep sandbox and production keys in separate environments.

## Dev loop
- Emulators for Auth, Firestore, Functions and Storage. Seed them with a script that generates **invented** data. Never use exports of production data on a dev machine unless they're anonymised.
- `tsc --noEmit` across the workspace, Jest for libs and functions, Maestro or Detox for the main flows.
- EAS Build profiles: `development`, `preview`, `production`. Version numbers come from CI, not by hand.

## Agent notes
- The root `CLAUDE.md`/`AGENTS.md` stays short. Move architecture laws, Firestore rules and style rules into `.agentic/architecture.md` and `.agentic/quality.md` (see the `project-context` skill).
- Keep logs, crash dumps and build output out of the repo root (git-ignore them, or have scripts write to `scratch/`).
