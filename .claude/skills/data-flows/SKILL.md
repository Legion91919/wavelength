---
name: data-flows
description: Data work inside a slice - feeds landed raw then cleaned then modelled, re-runnable jobs, data contracts, lineage, reconciliation and honest dashboards. Use when importing files or APIs, combining data from several sources, or building reports.
---

# Data flows

## Layers (medallion, keep it boring)
```
raw/       exactly what arrived (file, API response), immutable, with source + received_at
clean/     typed, validated, de-duplicated; rejects go to a quarantine table with the reason
model/     business-shaped tables for reporting (facts + dimensions), built only from clean/
```

## Rules
1. **Every job is re-runnable.** Same input → same output. Use `MERGE`/upsert on natural keys, never blind `INSERT`.
2. **Land raw first, transform second.** A bad file can be replayed after the fix without asking the source again.
3. **Data contracts.** Each source has a schema (JSON Schema / C# record / Pydantic) and contract tests that run on sample files in CI.
4. **Lineage.** Every model row can say which raw record(s) it came from (`source_file`, `source_row`, `job_run_id`).
5. **Reconcile.** After each load, check counts and totals against the source (e.g. sum of loan balances = the total on the lender's statement). A mismatch fails the run loudly.
6. **Time.** Store event time and load time separately; reports use event time.
7. **Small-team stack first.** Postgres or SQL Server + scheduled .NET Worker / Python job is enough until it isn't. DuckDB is great for local analysis and tests. Reach for Spark/Databricks only when one machine can't cope.

## Dashboards
- Every chart answers one named question ("Which loans breach covenant this month?").
- Show freshness ("data as of 06:00 today") on every dashboard.
- Numbers on the dashboard must match the model tables exactly; test this.

## Observability
Per job run: rows read, written, rejected, duration, status, emitted as metrics and stored in a `job_runs` table.

## Done when
Jobs re-run cleanly, contract tests pass on sample files, reconciliation checks pass, and lineage is queryable.
