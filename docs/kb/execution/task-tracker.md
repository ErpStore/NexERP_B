---
doc_id: KB-081
title: Master Progress Tracker
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-13
dependencies: [KB-080, KB-082]
---

# Master Progress Tracker

The single place task status is recorded. Update it as the **last step** of every task,
after the diff is reviewed and committed.

**Statuses:** `Not Started` · `Ready` · `Blocked` · `In Progress` · `Needs Review` ·
`Completed`

- `Ready` = every prerequisite is `Completed` and the task can be opened now.
- `Blocked` = a prerequisite is incomplete, **or** an external answer is missing.
- `Needs Review` = the work is done and committed on its branch, awaiting review.
- **Completed tasks are never deleted.** The record of what was done is the point.

**Parent tasks** (e.g. `M0-03`) are containers. A parent becomes `Completed` only when all
its children are `Completed` — it is never worked directly.

---

## M0 — Stabilise · Gate G0

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M0-00 | M0 | Establish a clean version-control baseline | DevOps | **Completed** | P0 | — | 0.5 d | G0 |
| M0-15 | M0 | Toolchain and build baseline | DevOps | **Ready** | P0 | M0-00 | 0.5 d | G0 |
| M0-08 | M0 | `.gitignore` + remove committed build output | DevOps | **Ready** | P1 | M0-00 | 0.5 d | G0 |
| M0-07 | M0 | CI pipeline: restore → build → analyzers | DevOps | Blocked | P0 | M0-15, M0-08 | 2 d | G0 |
| M0-04 | M0 | Rotate the exposed credentials | Security | **Ready** | P0 | — | 1 d | G0 |
| M0-03 | M0 | Externalise configuration secrets *(parent)* | Security | Blocked | P0 | M0-00 | 1 d | G0 |
| M0-03-01 | M0 | — `appsettings.json` → environment / user-secrets | Security | Blocked | P0 | M0-00 | 0.5 d | G0 |
| M0-03-02 | M0 | — hardcoded connection strings in C# | Security | Blocked | P0 | M0-03-01 | 0.5 d | G0 |
| M0-03-03 | M0 | — fail-fast startup validation | Security | Blocked | P0 | M0-03-02 | 0.5 d | G0 |
| M0-05 | M0 | Purge secrets from git history | Security | Blocked | P0 | M0-03, M0-04 | 1 d | G0 |
| M0-01 | M0 | Capture DDL for all 94 stored procedures *(parent)* | Database | **In Progress** | P0 | — | 4–5 d | G0 |
| M0-01-01 | M0 | — reconcile the 94-name inventory vs the 13 scripted | Database | **Completed** | P0 | — | 1 d | G0 |
| M0-01-02 | M0 | — script the missing procedures from a live tenant DB | Database | **Blocked** *(half A done; needs DBA)* | P0 | M0-01-01 | 2 d | G0 |
| M0-01-03 | M0 | — deployment script + rebuild runbook | Database | Blocked | P0 | M0-01-02 | 1 d | G0 |
| M0-02 | M0 | Confirm stored-procedure drift across tenants (Q-14) | Investigation | Blocked | P1 | M0-01-02 | 1 d | G0 |
| M0-12 | M0 | Test project + calculation tests *(parent)* | Testing | Blocked | P0 | M0-07 | 3 d | G0 |
| M0-12-01 | M0 | — create the test project and wire it into CI | Testing | Blocked | P0 | M0-07 | 0.5 d | G0 |
| M0-12-02 | M0 | — characterisation tests for `CalculationService` | Testing | Blocked | P0 | M0-12-01 | 2.5 d | G0 |
| M0-13 | M0 | Characterisation tests for `StockManagerService` | Testing | Blocked | P0 | M0-12-01 | 3 d | G0 |
| M0-09 | M0 | Fix the two unreachable delete guards (R-08) | Backend | Blocked | P1 | M0-12-01 | 0.5 d | G0 |
| M0-10 | M0 | Audit all `CanDelete…Async` guards (INV-025) | Investigation | Blocked | P1 | M0-09 | 2 d | G0 |
| M0-06 | M0 | Remove the seeded default Administrator credential | Security | Blocked | P1 | M0-12-01 | 1 d | G0 |
| M0-14 | M0 | Gate `DetailedErrors` on `IsDevelopment()` | Security | Blocked | P2 | M0-03-01 | 0.5 d | G0 |
| M0-11 | M0 | **Product decision** — silent FIFO under-issue (Q-01) | Product Decision | Blocked | P0 | M0-13 | decision | G0 |

## M1 — Repository Understanding · Gate G1 ✅

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M1-01 | M1 | Architecture, data, auth, tenancy, UI analysis | Investigation | **Completed** | P0 | — | — | G1 ✅ |
| M1-02 | M1 | Module inventory + dependency graph | Investigation | **Completed** | P0 | — | — | G1 ✅ |
| M1-03 | M1 | API surface + readiness assessment | Investigation | **Completed** | P0 | — | — | G1 ✅ |
| M1-04 | M1 | Business-rule seed (12 rules) + template | Investigation | **Completed** | P0 | — | — | G1 ✅ |
| M1-05 | M1 | Knowledge base + investigation registry + 5 ADRs | Documentation | **Completed** | P0 | — | — | G1 ✅ |
| M1-06 | M1 | Per-module rule extraction (INV-012…020, 024…028) | Investigation | *Rolling* | P1 | per wave | per wave | — |

M1-06 is intentionally never "Completed" — it is a recurring obligation discharged one wave
ahead of each migration ([KB-080 §8](README.md#8-m1--repository-understanding)).

## M2 — Foundation · Gate G2

### M2-A — Security and contract

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M2-A01 | M2 | Server-side screen-right authorization *(parent)* | Security | Blocked | P0 | G0 | 1–2 wks | G2 |
| M2-A01-01 | M2 | — implementation spec from ADR-004 | Architecture | Blocked | P0 | G0 | 2 d | G2 |
| M2-A01-02 | M2 | — implement `[RequireScreen]` / `[RequireRight]` | Security | Blocked | P0 | M2-A01-01 | 3 d | G2 |
| M2-A01-03 | M2 | — per-request rights resolution + caching | Security | Blocked | P0 | M2-A01-02 | 2 d | G2 |
| M2-A02 | M2 | Apply to `CurrencyController` + denial tests | Security | Blocked | P0 | M2-A01-03 | 1 d | G2 |
| M2-A03 | M2 | Permission-matrix test harness (CI gate) | Testing | Blocked | P0 | M2-A02 | 3 d | G2 |
| M2-A04 | M2 | Refresh tokens + revocation | Security | Blocked | P0 | M2-A01-02 | 3–5 d | G2 |
| M2-A05 | M2 | Cross-origin SPA tenant resolution + real CORS | Security | Blocked | P0 | M2-A04 | 3–5 d | G2 |
| M2-A06 | M2 | Exception middleware → `ProblemDetails` | Backend | Blocked | P0 | G0 | 3–5 d | G2 |
| M2-A07 | M2 | `GET /api/v1/me` | Backend | Blocked | P0 | M2-A01-03 | 2 d | G2 |
| M2-A08 | M2 | Row-level scoping + account gates (Q-05…Q-08) | Security | Blocked | P0 | M2-A01-03 | 3 d | G2 |

### M2-B — API structure

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M2-B07 | M2 | Shared `AddVSmartDomain()` DI extension | Backend | Blocked | P0 | G0 | 3 d | G2 |
| M2-B04 | M2 | Decouple `IApprovalService` + 13 `Pages` refs | Backend | Blocked | P0 | M2-B07 | 1 wk | G2 |
| M2-B01 | M2 | API versioning → `/api/v1` | Backend | Blocked | P1 | M2-B07 | 1 d | G2 |
| M2-B02 | M2 | Paging / sort / filter contract | Backend | Blocked | P0 | M2-A06 | 1 wk | G2 |
| M2-B03 | M2 | Codify the controller template | Documentation | Blocked | P0 | M2-A02, M2-B02 | 2 d | G2 |
| M2-B05 | M2 | Typed `ScreenCodes` constants (R-10) | Backend | Blocked | P1 | M2-B07 | 2 d | G2 |
| M2-B06 | M2 | File upload / download endpoints | Backend | Blocked | P1 | M2-A06 | 1 wk | G2 |
| M2-B08 | M2 | Report + print endpoints (ADR-005) | Backend | Blocked | P1 | **M2-B07**, M2-A01-03, G0 | 1 wk | G2 |
| M2-B09 | M2 | Reference-data endpoints + caching | Backend | Blocked | P1 | **M2-B07**, M2-B02 | 3 d | G2 |
| M2-B10 | M2 | OpenAPI + TypeScript client generation in CI | DevOps | Blocked | P0 | M2-B03 | 3 d | G2 |
| M2-B11 | M2 | Health checks + structured logging (R-23) | DevOps | Blocked | P2 | M2-A06 | 3 d | G2 |
| M2-B12 | M2 | Document numbering hardening *(parent)* | Backend | Blocked | P0 | M2-B07 | 1 wk | G2 |
| M2-B12-01 | M2 | — INV-012 numbering investigation | Investigation | Blocked | P0 | M2-B07 | 2 d | G2 |
| M2-B12-02 | M2 | — verify unique constraints in a live DB (Q-10) | Database | Blocked | P0 | M2-B12-01 | 1 d | G2 |
| M2-B12-03 | M2 | — race-safe allocation + idempotency (R-12) | Backend | Blocked | P0 | M2-B12-02 | 3 d | G2 |

### M2-C — React foundation

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M2-C01 | M2 | Vite + React 19 + TS strict + lint + test + CI | Frontend | Blocked | P0 | G0 | 3 d | G2 |
| M2-C11 | M2 | Archive the Angular pilot | DevOps | Blocked | P2 | M2-C01 | 0.5 d | G2 |
| M2-C10 | M2 | Decimal handling — no float money arithmetic | Frontend | Blocked | P0 | M2-C01 | 2 d | G2 |
| M2-C02 | M2 | Auth: login, refresh, guards, permission store | Frontend | Blocked | P0 | M2-C01, M2-A04, M2-A07 | 1 wk | G2 |
| M2-C04 | M2 | Design-system primitives *(parent)* | Frontend | Blocked | P0 | M2-C01 | 2 wks | G2 |
| M2-C04-01 | M2 | — tokens, theme, light/dark | Frontend | Blocked | P0 | M2-C01 | 3 d | G2 |
| M2-C04-02 | M2 | — form controls + validation display | Frontend | Blocked | P0 | M2-C04-01 | 4 d | G2 |
| M2-C04-03 | M2 | — modal, drawer, toast, states | Frontend | Blocked | P0 | M2-C04-01 | 3 d | G2 |
| M2-C03 | M2 | App shell: header, sidebar, breadcrumbs, ⌘K | Frontend | Blocked | P0 | M2-C02, M2-C04-01 | 1.5 wks | G2 |
| M2-C05 | M2 | `DataGrid` *(parent)* | Frontend | Blocked | P0 | M2-C04-02, M2-B02 | 1.5 wks | G2 |
| M2-C05-01 | M2 | — server-paged table core | Frontend | Blocked | P0 | M2-C04-02, M2-B02 | 4 d | G2 |
| M2-C05-02 | M2 | — column preferences + persistence | Frontend | Blocked | P1 | M2-C05-01 | 3 d | G2 |
| M2-C05-03 | M2 | — empty / loading / error states + export | Frontend | Blocked | P1 | M2-C05-01 | 2 d | G2 |
| M2-C06 | M2 | `RecordPickerDialog` | Frontend | Blocked | P0 | M2-C05-01 | 1 wk | G2 |
| M2-C07 | M2 | `LineItemGrid` — keyboard-first editable grid | Frontend | Blocked | P0 | M2-C05-01, M2-C10 | 2 wks | G2 |
| M2-C08 | M2 | `DocumentEditor` shell *(parent)* | Frontend | Blocked | P0 | M2-C07 | 2 wks | G2 |
| M2-C08-01 | M2 | — layout: header + lines + totals + commands | Frontend | Blocked | P0 | M2-C07 | 4 d | G2 |
| M2-C08-02 | M2 | — server-authoritative totals wiring | Frontend | Blocked | P0 | M2-C08-01 | 3 d | G2 |
| M2-C08-03 | M2 | — workflow command pattern | Frontend | Blocked | P0 | M2-C08-01 | 3 d | G2 |
| M2-C09 | M2 | `ReportPage` framework | Frontend | Blocked | P1 | M2-C05-01, M2-B08 | 1 wk | G2 |

### M2-D — Vertical slice

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M2-D01 | M2 | Currency end-to-end in React | Frontend | Blocked | P0 | M2-C05-03, M2-A02, M2-B10 | 3 d | G2 |
| M2-D02 | M2 | Customer Master *(parent)* | Migration | Blocked | P0 | M2-D01 | 1.5 wks | G2 |
| M2-D02-01 | M2 | — `@code` triage + logic extraction | Backend | Blocked | P0 | M2-D01 | 4 d | G2 |
| M2-D02-02 | M2 | — `CustomersController` + API tests | Backend | Blocked | P0 | M2-D02-01 | 3 d | G2 |
| M2-D02-03 | M2 | — React screens + component tests | Frontend | Blocked | P0 | M2-D02-02 | 4 d | G2 |
| M2-D03 | M2 | Blazor ↔ React parity test | Testing | Blocked | P0 | M2-D02-03 | 3 d | G2 |

## M3 — Core Modules · Gate G3

Each wave expands to 14 tasks per the module pattern ([KB-080 §10](README.md#10-module-migration-task-pattern)).
Task files are generated at wave start — see [KB-080 §11](README.md#11-m3--core-modules).

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M3-1-01 | M3 | Wave M3-1 business-rule investigation *(exemplar written)* | Investigation | Blocked | P0 | G2 | 3 d | G3 |
| M3-1-02…14 | M3 | Masters — Accounts, General | Migration | Not Started | P0 | M3-1-01 | 3 wks | G3 |
| M3-2-01…14 | M3 | Masters — Inventory (Item, BOM, Process, Store, HSN) | Migration | Not Started | P0 | M3-1-14 | 4 wks | G3 |
| M3-3-01…14 | M3 | Masters — Admin & Settings, permission matrix | Migration | Not Started | P0 | M3-2-14 | 2 wks | G3 |
| M3-4-01…14 | M3 | Approvals inbox | Migration | Not Started | P1 | M3-3-14 | 1.5 wks | G3 |
| M3-5-01…14 | M3 | Sales: Leads → … → **Sales Order** | Migration | Not Started | P0 | M3-3-14, M2-C08 | 4 wks | G3 |
| M3-6-01…06 | M3 | Report framework + first 10 reports | Frontend | Not Started | P1 | M2-C09 | 2 wks | G3 |
| M3-7-01…08 | M3 | Dashboard | Frontend | Not Started | P2 | M3-1-14 | 1.5 wks | G3 |
| M3-8-01…03 | M3 | Feature-flag infrastructure | DevOps | Not Started | P0 | G2 | 1 wk | G3 |
| M3-9-01 | M3 | Re-baseline M4 from measured M3-5 cost | Documentation | Not Started | P0 | M3-5-14 | 2 d | G3 |

## M4 — Advanced Modules · Gate G4

**All M4 estimates are provisional until M3-9-01.** Execution order differs from the wave
ids: Inventory (M4-2) precedes Purchase (M4-1) — see [KB-080 §12](README.md#12-m4--advanced-modules).

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M4-2-01…14 | M4 | Inventory / Stock | Migration | Not Started | P0 | G3, M0-11 applied | 4 wks | G4 |
| M4-1-01…14 | M4 | Out Sourcing + Purchase | Migration | Not Started | P0 | M4-2-14 | 5 wks | G4 |
| M4-3-01…14 | M4 | Planning | Migration | Not Started | P0 | M4-1-14 | 4 wks | G4 |
| M4-4-01…14 | M4 | Production + shop-floor Production Log | Migration | Not Started | P0 | M4-3-14 | 4 wks | G4 |
| M4-5-01…14 | M4 | Manufacturing Work + e-Invoice / e-Way | Migration | Not Started | P0 | M4-4-14 | 4 wks | G4 |
| M4-6-01…14 | M4 | Sub Contract | Migration | Not Started | P1 | M4-5-14 | 3 wks | G4 |
| M4-7-01…14 | M4 | Labour Work — largest single item | Migration | Not Started | P1 | M4-6-14 | 4 wks | G4 |
| M4-8-01…14 | M4 | Accounts / Cash Flow | Migration | Not Started | P1 | G3 | 3 wks | G4 |
| M4-9-01…14 | M4 | HR incl. Payroll | Migration | Not Started | P2 | G3 | 3 wks | G4 |
| M4-10-01…14 | M4 | Inspection / QC, Maintenance, Utilities | Migration | Not Started | P2 | G3 | 2 wks | G4 |
| M4-11-01…08 | M4 | Remaining ~30 reports | Frontend | Not Started | P2 | M3-6-06 | 2 wks | G4 |

## M5 — Hardening · Gate G5

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M5-01 | M5 | Unit tests for extracted business rules | Testing | *Continuous* | P0 | each `<W>-03` | — | G5 |
| M5-02 | M5 | API integration tests per controller | Testing | *Continuous* | P0 | each `<W>-06` | — | G5 |
| M5-03 | M5 | Component tests for design-system primitives | Testing | Blocked | P0 | M2-C04 | — | G5 |
| M5-04 | M5 | E2E per module critical path | Testing | *Continuous* | P0 | each `<W>-10` | — | G5 |
| M5-05 | M5 | Permission-matrix testing (merge-blocking) | Testing | Blocked | P0 | M2-A03 | — | G5 |
| M5-06 | M5 | Parity testing per module | Testing | *Continuous* | P0 | each `<W>-11` | — | G5 |
| M5-07 | M5 | Performance: grids, documents, concurrency | Testing | Not Started | P1 | G4 | 2 wks | G5 |
| M5-08 | M5 | Security: tenant isolation, IDOR, JWT, XSS | Security | Not Started | P0 | G4 | 2 wks | G5 |
| M5-09 | M5 | Accessibility: axe in CI + keyboard pass | Testing | *Continuous* | P1 | M2-C04 | — | G5 |
| M5-10 | M5 | Load test + live index review (INV-026) | Testing | Not Started | P1 | G4 | 2 wks | G5 |

## M6 — Production Migration · Gate G6

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M6-01 | M6 | Deployment topology (Q-16) | DevOps | Not Started | P0 | G4 | 1 wk | G6 |
| M6-02 | M6 | Monitoring: logs, APM, error tracking | DevOps | Not Started | P0 | M6-01 | 1 wk | G6 |
| M6-03 | M6 | Staged rollout by feature flag (Q-12) | Migration | Not Started | P0 | M6-02 | 2 wks | G6 |
| M6-04 | M6 | Rollback drill in production | Migration | Not Started | P0 | M6-03 | 3 d | G6 |
| M6-05 | M6 | User migration + training | Migration | Not Started | P1 | M6-03 | 1 wk | G6 |
| M6-06 | M6 | Per-tenant EF migration rollout (Q-02) | DevOps | Not Started | P0 | M6-01 | 1 wk | G6 |
| M6-07 | M6 | Decommission Blazor routes | Migration | Not Started | P1 | M6-04 + ≥1 financial period | 2 wks | G6 |
| M6-08 | M6 | Decide the MAUI app's future (Q-11) | Product Decision | Not Started | P2 | M6-07 | decision | G6 |

---

## Rollup

| Milestone | Tasks | Completed | Gate | Gate status |
|---|---|---|---|---|
| M0 | 24 | 2 | G0 | ⬜ Not met |
| M1 | 6 | 5 (+1 rolling) | G1 | ✅ Passed 2026-08-12 |
| M2 | 52 | 0 | G2 | ⬜ Not met |
| M3 | ~100 | 0 | G3 | ⬜ Not met |
| M4 | ~150 | 0 | G4 | ⬜ Not met |
| M5 | 10 | 0 | G5 | ⬜ Not met |
| M6 | 8 | 0 | G6 | ⬜ Not met |

**Currently `Ready`:** M0-04, M0-15, M0-08.

**M0-01-02: `Blocked` — half A done (2026-08-13), half B needs a human DBA.**
Delivered `db/tools/Export-StoredProcedures.ps1` (marked UNVERIFIED — no database reachable
from this execution environment), `db/tools/list-deployed-procedures.sql`,
`db/tools/verify-capture.sh` (a database-free reconciliation harness — proven correct against
7 synthetic failure modes plus a synthetic valid capture), `db/RUNBOOK-capture-stored-procedures.md`,
and `db/stored-procedures/CAPTURE-STATUS.md` pre-filled with all 82 `missing` names. R-04 and
INV-027 amended to record the tooling without claiming the risk is closed — **it is not**:
`db/stored-procedures/` contains 0 `.sql` files. **Action needed:** a named DBA with `VIEW
DEFINITION` on a nominated tenant database must run `db/RUNBOOK-capture-stored-procedures.md`
(half B), then a follow-up session verifies the delivered capture with
`db/tools/verify-capture.sh` (half C) and closes INV-027/R-04.

**M0-01-01: `Completed` (2026-08-13).** Repository-side stored-procedure reconciliation —
see [KB-102](../architecture/stored-procedure-inventory.md) and
[`db/stored-procedures/manifest.csv`](../../../db/stored-procedures/manifest.csv). 11
`scripted`, 1 `case_mismatch`, 82 `missing`, 1 `unreferenced`; both arithmetic closures
verified (94 referenced, 13 declared). R-04 and INV-027 amended. `dotnet build` regression
guard **not run** — no .NET SDK in this execution environment; no file under `V.SMART/` was
touched, so the risk that check guards against does not apply here.

**M0-00: `Completed` (2026-08-13).**
`migration/M0-00-vcs-baseline` merged to `master` via PR #1 (`5fcb2b1`); the follow-up
visibility-correction branch `fix/M0-00a-correct-repo-visibility-finding` merged to
`master` directly (`661482a`). Repository visibility resolved: reported public (flawed
test) → corrected to private (INV-034) → **owner deliberately made it public**, verified
rigorously, Q-19 answered. The last open acceptance criterion — `master` branch
protection — was closed by the repo owner in the GitHub UI on 2026-08-13 and reconfirmed
via the authenticated GitHub API: `list_branches` on `ErpStore/NexERP_B` returns
`{"name":"master","protected":true}`. All acceptance criteria met. M0-15 and M0-08 move
to `Ready`.

M3/M4 counts are the 14-step pattern multiplied across waves and will firm up as each wave's
task files are generated.
