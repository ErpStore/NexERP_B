---
doc_id: KB-070
title: Phased Migration Strategy (Proposal)
module: migration
source_files: []
status: proposal
confidence: n/a
last_verified: 2026-08-12
dependencies: [KB-020, KB-041, KB-050, KB-052, KB-060]
---

# Phased Migration Strategy

> **Proposal.** Estimates are engineering-effort ranges for a small team (2–3 backend,
> 2–3 frontend, 1 QA), not commitments. They assume the existing Blazor app keeps running
> and serving users throughout.

## Guiding principles

1. **Strangler-fig, not big bang.** The Blazor app stays in production until the React app
   reaches parity per module. Both run against the same database and the same services.
2. **The backend is extended, never rewritten.** New controllers over existing services.
3. **Extract before you rebuild.** Business logic comes out of `@code` into services
   *first*, verified against the running Blazor app, *then* the React screen is built.
4. **Server is authoritative.** Calculations, validation, permissions, numbering.
5. **Migrate along the dependency graph.** Masters → transactional documents → reports.
6. **Every module ships behind a per-tenant, per-module feature flag.**

---

## Phase 0 — Stabilise (2–3 weeks, starts immediately, parallel to everything)

Not migration work. It is the safety net without which nothing else is responsible.

| Task | Risk addressed |
|---|---|
| Rotate and externalise all secrets; purge git history | R-01, R-02 |
| Script all 94 stored procedures into `db/stored-procedures/` | **R-04** |
| Stand up CI: build + analyzers + (soon) tests | R-05 |
| Fix the seeded default administrator credential | R-09 |
| Fix R-08 (delete-guard copy-paste bugs) | R-08 |
| Decide R-07 (silent stock under-issue) as a product question | R-07 / Q-01 |
| Add `.gitignore` entries; remove committed build output | R-14 |
| Integration tests for `ICalculationService` and `IStockManagerService` | R-05 |

**Exit criteria.** No secrets in the repo; a fresh tenant database can be built from
source; CI green; the two highest-consequence services have characterisation tests.

---

## Phase 1 — Repository understanding (largely complete)

| Task | Status |
|---|---|
| Architecture analysis | ✅ [`architecture/`](../architecture/) |
| Module inventory + dependency graph | ✅ [`modules/module-inventory.md`](../modules/module-inventory.md) |
| API surface + readiness | ✅ [`api/`](../api/) |
| Business-rule seed + template | 🟡 [`business-rules/`](../business-rules/) — **partial by design** |
| Knowledge base + investigation registry | ✅ this directory |
| **Per-module business-rule extraction (INV-012 … INV-020)** | ⬜ **runs just ahead of each module's migration** |

Per-module rule extraction is deliberately *not* done up front. Doing it all now would
produce documentation that goes stale before it is used. It is scheduled one module ahead
of implementation.

---

## Phase 2 — Foundation (6–8 weeks)

### Backend

| Task | Est. |
|---|---|
| Server-side authorization filter (`[RequireScreen]` / `[RequireRight]`) — **ADR-004** | 1–2 wks |
| `ProblemDetails` error contract + exception middleware + correlation ids | 1 wk |
| Refresh tokens, revocation, `GET /api/v1/me` (user + tenant + full rights) | 1 wk |
| Tenant resolution for a cross-origin SPA; real CORS config | 3–5 d |
| Decouple `IApprovalService` and the other 13 `Pages`-referencing files (R-11) | 1 wk |
| Shared `AddVSmartDomain()` DI extension across all hosts (R-26) | 3 d |
| Typed `ScreenCodes` constants (R-10) | 2 d |
| File upload/download, report (PDF), Excel export/import endpoints | 1 wk |
| Reference-data endpoints (GST rates, UOM, states, screens, terms) | 3 d |
| OpenAPI polish + TypeScript client generation in CI | 3 d |

### Frontend

| Task | Est. |
|---|---|
| Vite + React 19 + TS strict, ESLint/Prettier, Vitest, Playwright, CI | 3 d |
| App shell: header, sidebar (permission-filtered), breadcrumbs, ⌘K palette, theme | 1.5 wks |
| Auth: login, refresh, guards, permission store, `PermissionGate` | 1 wk |
| Design-system primitives (see [`design-system.md`](../frontend-new/design-system.md)) | 2 wks |
| **`DataGrid`** — server-paged, column prefs, export, all states | 1.5 wks |
| **`RecordPickerDialog`** — the `DetailsModal` replacement | 1 wk |
| **`LineItemGrid`** — keyboard-first editable grid | 2 wks |
| **`DocumentEditor`** shell (header + lines + totals + commands) | 2 wks |
| Report framework (`ReportPage` from a declarative definition) | 1 wk |

**Exit criteria (vertical slice).** Currency and Customer Master fully working in React —
login, tenant, permission-gated CRUD, server paging, validation, error contract, Excel
export — with the Blazor app untouched and still live.

---

## Phase 3 — Core modules (12–16 weeks)

Order follows the dependency graph. Each module: extract `@code` logic → build controllers
→ build React screens → parity test → feature-flag on for a pilot tenant.

| Wave | Modules | Why here | Est. |
|---|---|---|---|
| 3.1 | Masters — Accounts, General (Customer, Vendor, Terms, States, Machines) | Lowest logic density; every other module depends on them | 3 wks |
| 3.2 | Masters — Inventory (Item, BOM, BOM Labour, Process, Store, HSN, Raw Material) | Unlocks every document module; Item and BOM are genuinely hard | 4 wks |
| 3.3 | Masters — Admin & Settings (Users, **Permission matrix**, Screens, General Settings, Print, Company) | Needed to administer the new app itself | 2 wks |
| 3.4 | **Approvals inbox** (`/approvals`) | High user value, low dependency, exercises the workflow-command pattern early | 1.5 wks |
| 3.5 | Sales: Leads → Enquiry → Feasibility → Quotation → **Sales Order** | Sales Order is the `DocumentEditor` reference implementation | 4 wks |
| 3.6 | Reports framework + first 10 reports | Read-only, parallelisable, immediate perceived value | 2 wks (parallel) |
| 3.7 | Dashboard | | 1.5 wks |

**Exit criteria.** A pilot tenant runs masters, the sales pipeline through Sales Order,
approvals, and core reports entirely in React.

---

## Phase 4 — Advanced modules (16–22 weeks)

| Wave | Modules | Note | Est. |
|---|---|---|---|
| 4.1 | Out Sourcing + Purchase (Requisition → Enquiry → Quotation → Price Comparison → PO → GRN → SCN → Invoice → Debit Note) | SCN writes stock — must follow `IStockManagerService` hardening | 5 wks |
| 4.2 | Inventory / Stock (Issue Request, MIN, STN, Inter-Store, Tool Crib, Stock Position) | Highest correctness risk in the product | 4 wks |
| 4.3 | Planning (Job Order, Route Card, RC Release, Estimation, Requirement Analysis) | | 4 wks |
| 4.4 | Production (Assembly + Component issue/return/SCN) + **shop-floor Production Log UI** | Log gets a bespoke touch-first interface | 4 wks |
| 4.5 | Manufacturing Work (DC, Tax Invoice, Export Invoice) + **e-Invoice / e-Way Bill** | External integrations, statutory correctness — allow contingency | 4 wks |
| 4.6 | Sub Contract (DC-Out, GRN, SCN, Invoice) | `SubConGRNService` is 5,631 LOC | 3 wks |
| 4.7 | Labour Work (GRN, SCN, DC-Out, Invoice, Credit Note) | `LabourDcOutgoingService` 6,112 LOC + a 6,528-LOC page — **the single largest item** | 4 wks |
| 4.8 | Accounts / Cash Flow (Payments, Receipts, Advance Adjustment, Service Bills, Bank) | TDS and allocation logic | 3 wks |
| 4.9 | HR (Leave, Attendance, Payroll, Staff Loan, Letters) | Payroll needs careful parity testing | 3 wks |
| 4.10 | Inspection / QC, Maintenance, Utilities | Lower complexity | 2 wks |
| 4.11 | Remaining ~30 reports | | 2 wks (parallel) |

---

## Phase 5 — Testing and hardening (6–8 weeks, but starts in Phase 2)

Testing is **not** a terminal phase; only the hardening sweep is.

| Activity | When |
|---|---|
| Unit tests for extracted business logic | with each extraction |
| API integration tests per controller | with each controller |
| Component tests (RTL) for design-system primitives | Phase 2 |
| E2E (Playwright) for each module's critical path | with each module |
| **Permission matrix testing** — every endpoint × every right combination, automated | Phase 2 onward, mandatory gate |
| **Parity testing** — same input through Blazor and React, compare persisted rows and totals | per module |
| Performance: 10k-row grids, 200-line documents, concurrent document creation (R-12) | Phase 5 |
| Security: pen test focused on tenant isolation, IDOR on `{id}` routes, JWT handling, XSS | Phase 5 |
| Accessibility: axe in CI + manual keyboard pass per screen | continuous |
| Load test against a production-sized tenant; index review (R-13) | Phase 5 |

---

## Phase 6 — Production migration (4–6 weeks)

| Step | Detail |
|---|---|
| Deployment topology | React static build on CDN/nginx; API containerised; Blazor app retained; per-tenant subdomain routing preserved |
| Monitoring | Structured logs (replacing R-23), APM traces, error tracking, uptime checks, per-tenant dashboards |
| Rollout | Per-tenant, per-module feature flags. Pilot tenant first (smallest by volume), then staged |
| Rollback | Flags flip back to Blazor per module within minutes. Both UIs share one database, so **no data migration and no rollback data problem** — the decisive advantage of the strangler approach |
| User migration | No credential migration: same `Users` table, same hashes. Per-role training, side-by-side period, in-app guided tour |
| Legacy decommissioning | Only after ≥ 1 full financial period with zero fallbacks on a module. Retire Blazor routes module by module; retire the MAUI app **only** once the responsive/shop-floor React screens are proven (Q-11) |

---

## Timeline summary

| Phase | Duration | Cumulative |
|---|---|---|
| 0 — Stabilise | 2–3 wks (parallel) | — |
| 1 — Understanding | done + rolling | — |
| 2 — Foundation | 6–8 wks | 2 mo |
| 3 — Core modules | 12–16 wks | 5–6 mo |
| 4 — Advanced modules | 16–22 wks | 9–12 mo |
| 5 — Hardening sweep | 6–8 wks (mostly overlapped) | 10–13 mo |
| 6 — Production migration | 4–6 wks | **11–14 months** |

**The dominant variable is Phase 4**, and within it the `@code` extraction (R-06). A
per-module triage in Phase 3 will materially sharpen the Phase 4 estimate — treat the
current Phase 4 range as provisional until wave 3.5 (Sales Order) is complete, because it
is the first real measurement of extraction cost.

## Explicit non-goals

- No database schema change.
- No replacement of EF Core, AutoMapper, FastReport, or the stored procedures.
- No rewrite of any business service.
- No conversion of the Angular pilot — archive it.
- No new ERP functionality during the migration. Feature freeze on the Blazor app for
  anything a migrating module touches.
