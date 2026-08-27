---
doc_id: ADR-005
title: Keep FastReport and stored-procedure reporting; expose over HTTP
module: decisions
status: accepted
confidence: n/a
last_verified: 2026-08-27
dependencies: [KB-011, KB-052, KB-060]
---

# ADR-005 — Reporting and printing strategy

**Status:** Accepted · **Date:** 2026-08-12

## Context

Two independent reporting mechanisms exist, both entirely server-side:

1. **Printed documents** — 104 FastReport `.frx` templates across 5 tenant folders
   (`default`, `acucom…`, `sns…`, `srinuenggind…`, `sharadaelectrou1…`), rendered to PDF by
   `ReportService.Generate_Report`, with per-tenant template override, per-screen
   `PrintSetting` (watermark, logo, ISO logo, copy count, copy name), and the tenant's own
   connection string injected into the report.
2. **Analytical/list reports** — 94 stored procedures executed through
   `ReportExecutor.ExecuteAsync<T>` into keyless entity types, surfaced on ~40 report
   screens.

Both already return data or bytes with no UI coupling. Reproducing them in Angular would mean
rebuilding 104 pixel-accurate statutory documents (invoices, e-way bills, delivery
challans) plus 94 non-trivial SQL reports — for no functional gain and considerable
compliance risk.

## Decision

**Keep both mechanisms unchanged. Expose them over HTTP.**

```
GET /api/v1/{resource}/{id}/print?template={name}      → application/pdf
GET /api/v1/reports/{slug}?<typed parameters>          → paged JSON
GET /api/v1/reports/{slug}/export?format=xlsx|pdf|csv  → file
```

> **Correction 2026-08-12 (Confirmed).** "Paged JSON" is not free — **`ReportExecutor`
> performs no paging at all.** `ExecuteAsync<T>` (`:35-39`) materialises the entire result
> set with `ToListAsync()`; a `@Skip`/`@Take` variant exists at `:48-86` but is **commented
> out**. `SetCommandTimeout(300)` at `:25`, and a second `CommandTimeout = 300` on the
> `DataTable` overload at `:107`.
>
> So paging must be added — in the stored procedure, in a wrapping query, or by accepting
> full materialisation and paging in memory (which does nothing for the timeout). **M2-B08
> must decide and record which**, because M2-C09's grid design depends on the answer, and
> the 300-second ceiling is a real operational limit, not a theoretical one.

The Angular client renders PDFs from a blob URL and never generates them.

**The ~40 report screens are built from one declarative `ReportPage` framework** — a report
definition supplies parameters, columns, the procedure slug, and export options. Forty
screens become forty configuration objects.

Excel export/import likewise stays server-side (`ExcelExportService`,
`IExcelTemplateService`) behind endpoints.

### Mandatory prerequisite

**R-04 must be fixed first:** only 12 of the 94 stored procedures the application calls have
DDL in source control (13 `.sql` files exist, but one — `Sp_Print_PurchaseOrder.sql` — is
dead code; see R-04). All 94 must be scripted into `db/stored-procedures/` with a deployment step
before this decision is safe to depend on. Until then, reporting cannot be rebuilt in a
fresh environment at all.

## Consequences

**Positive.** Zero risk to statutory document layouts. ~40 report screens collapse into one
framework. Per-tenant template overrides and print settings keep working untouched. Report
work can proceed in parallel with module migration because it is read-only.

**Negative.** FastReport `.frx` files remain editable only in the FastReport designer — a
specialist skill outside the Angular team. The 300-second report timeout will block an HTTP
request thread just as it blocks a Blazor circuit today; long reports should move to a
background-job + download pattern if they prove problematic (there is currently **no**
background infrastructure at all). Reports bypass the service layer and query the database
directly, so **`UserRight` does not gate report data** — report endpoints must carry
`[RequireScreen]`/`[RequireRight]` per ADR-004, and any row-level scoping
(`User.StateCodesCsv`, Q-08) must be handled explicitly.

**Neutral.** Blazor-ApexCharts is replaced by Recharts for the dashboard — that is
presentation, not reporting, and carries no compliance risk.

## Implementation notes (M2-B08, 2026-08-27)

**`ReportService` has three public entry points, not one.** `Generate_Report` (78 callers) is
dominant, but `GenerateSalarySlipReport` (2 callers, identical signature) and
`Generate_Attendance_Report` (1 caller, **different** signature — `(year, monthId, weekNo,
fileName, procedureName)`, no `id`, no `screenName`) both exist. The first two share one route
shape (`GET /{resource}/{id:int}/print`, distinguished by a `Generator` field in the seed
registry); the third does not fit that shape at all and is deliberately left unregistered —
its own route, if one is ever needed, is a decision for whoever registers it, not inherited
from this ADR.

**Report paging is in-memory, not server-side, and this is a permanent property of the
design, not a gap to close later.** `ReportExecutor.ExecuteAsync<T>` runs a stored procedure
that returns its full result set; none of the procedures accept `@Skip`/`@Take`
(`ReportExecutor.cs:48-86` is a commented-out attempt, abandoned before M2-B08 and not
revived). Adding such parameters to a procedure is a schema change, which this ADR's own
"neither engine changes" premise forbids. Every report catalogue entry states
`"paging": "in-memory"` so a client cannot mistake this for the server-paged contract
[ADR-002 §2a](ADR-002-rest-api-layer.md) establishes for ordinary resource lists.

Full contract, the seeded print/report maps and the template-coverage matrix:
[KB-110](../api/report-and-print-endpoints.md).
