---
name: build-dotnet
description: Build C#/.NET backends the senior way - Clean Architecture or modular monolith layout, minimal APIs, EF Core migrations, ProblemDetails errors, OpenTelemetry, idempotency, and test-first vertical slices. Use for any C#/.NET API, worker or library work.
---

# Build: C#/.NET

## Layout (modular monolith, Clean Architecture inside each module)
```
src/
  <App>.Api/                    # composition root: endpoints, auth, DI, OpenTelemetry
  Modules/<Module>/
    <Module>.Domain/            # entities, value objects, domain rules. No EF, no HTTP.
    <Module>.Application/       # use cases (commands/queries), ports (interfaces)
    <Module>.Infrastructure/    # EF Core DbContext, adapters, outbox
  <App>.Worker/                 # background jobs, queue consumers
tests/
  <Module>.Domain.Tests/        # fast unit tests for rules
  <Module>.Integration.Tests/   # Testcontainers (Postgres/SQL Server), real HTTP via WebApplicationFactory
  Architecture.Tests/           # module boundaries, layer rules
```
Small single-area app: drop `Modules/`, keep Domain/Application/Infrastructure/Api.

## Defaults
- .NET 8 LTS (or current LTS), `Nullable` enabled, `TreatWarningsAsErrors`, analyzers on (`Directory.Build.props`).
- Minimal APIs grouped per module (`MapGroup`), typed results, validation at the edge (FluentValidation or manual).
- Errors as **ProblemDetails** (RFC 9457); never leak stack traces.
- Money is `decimal` in a `Money` value object with currency. Never `double`.
- Time is `TimeProvider` (injectable, testable), stored as UTC `DateTimeOffset`.
- EF Core: migrations checked in, one DbContext per module with its own schema, no lazy loading.
- Commands that move money or create records accept an **`Idempotency-Key`** header; store key + response hash.
- Events that must reach other modules go through a **transactional outbox**.
- Config via `IOptions<T>` with validation on start; secrets from Key Vault / user-secrets, never appsettings.
- **OpenTelemetry** traces + metrics + logs from day one; propagate `traceparent`.
- Health checks: `/health/live`, `/health/ready`.

## Slice loop (per backlog ticket)
1. Write the acceptance test (integration test hitting the endpoint) → red.
2. Write domain unit tests for the rules → red.
3. Implement domain → application → infrastructure → endpoint until green.
4. `dotnet format --verify-no-changes`, `dotnet test`, then update `.agentic/` docs in the same change.

## Review checklist
- [ ] No module touches another module's DbContext or tables
- [ ] Every money/record-creating command is idempotent
- [ ] Every external call has a timeout, retry with jitter (Polly / `Microsoft.Extensions.Http.Resilience`) and a circuit breaker
- [ ] Logs carry correlation IDs, no personal data
- [ ] New config keys documented in `.env.example` / `runbook.md`
