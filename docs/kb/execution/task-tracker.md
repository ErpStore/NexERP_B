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
last_verified: 2026-08-19
dependencies: [KB-080, KB-082, KB-088, KB-089]
---

# Master Progress Tracker

The single place task status is recorded, for **every** task. Update it as the **last step**
of every task, after the diff is reviewed and committed.

> **Looking for what to work on right now?** Read
> [`current-task.md`](current-task.md) (KB-089) instead — it holds the one active task and
> the minimum needed to execute it. This document is the *status of everything*; that one is
> *the thing to do next*. When they disagree, this document is authoritative on status and
> `current-task.md` must be corrected.
>
> Procedure for opening, completing and handing over a task:
> [`workflow.md`](workflow.md) (KB-088).

**Lifecycle:** `PLANNED → READY → IN_PROGRESS → IMPLEMENTATION → TESTING → REVIEW →
COMPLETED`, with `BLOCKED` as an orthogonal flag rather than a phase
([KB-088 §1](workflow.md#1-task-lifecycle)).

The tables below use this document's original vocabulary. They are the same states:

| Canonical | Used in the tables below |
|---|---|
| `PLANNED` | `Not Started` |
| `READY` | `Ready` |
| `IN_PROGRESS` / `IMPLEMENTATION` / `TESTING` | `In Progress` |
| `REVIEW` | `Needs Review` |
| `COMPLETED` | `Completed` |
| *(flag)* `BLOCKED` | `Blocked` |

- `Ready` = every prerequisite is `Completed` and the task can be opened now.
- `Blocked` = a prerequisite is incomplete, **or** an external answer is missing. Record
  which, and who can unblock it — blocked-on-a-task and blocked-on-a-human are different
  problems.
- `Needs Review` = the work is done and committed on its branch, awaiting review. This is the
  normal end state of an execution session; `Completed` requires integration
  ([KB-088 §3](workflow.md#who-may-set-completed)).
- **Completed tasks are never deleted.** The record of what was done is the point.

**Parent tasks** (e.g. `M0-03`) are containers. A parent becomes `Completed` only when all
its children are `Completed` — it is never worked directly.

---

## M0 — Stabilise · Gate G0

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M0-00 | M0 | Establish a clean version-control baseline | DevOps | **Completed** | P0 | — | 0.5 d | G0 |
| M0-15 | M0 | Toolchain and build baseline | DevOps | **Completed**² | P0 | M0-00 | 0.5 d | G0 |
| M0-08 | M0 | `.gitignore` + remove committed build output | DevOps | **Completed**⁵ | P1 | M0-00 | 0.5 d | G0 |
| M0-07 | M0 | CI pipeline: restore → build → analyzers | DevOps | **Completed**⁷ | P0 | M0-15, M0-08 | 2 d | G0 |
| M0-04 | M0 | Rotate the exposed credentials | Security | **Blocked**⁴ | P0 | — | 1 d | G0 |
| M0-03 | M0 | Externalise configuration secrets *(parent)* | Security | **Completed**¹¹ | P0 | M0-00 | 1 d | G0 |
| M0-03-01 | M0 | — `appsettings.json` → environment / user-secrets | Security | **Completed**³ | P0 | M0-00 | 0.5 d | G0 |
| M0-03-02 | M0 | — hardcoded connection strings in C# | Security | **Completed**⁸ | P0 | M0-03-01 | 0.5 d | G0 |
| M0-03-03 | M0 | — fail-fast startup validation | Security | **Completed**⁹ | P0 | M0-03-02 | 0.5 d | G0 |
| M0-05 | M0 | Purge secrets from git history | Security | Blocked | P0 | M0-03, M0-04 | 1 d | G0 |
| M0-01 | M0 | Capture DDL for all 94 stored procedures *(parent)* | Database | **In Progress** | P0 | — | 4–5 d | G0 |
| M0-01-01 | M0 | — reconcile the 94-name inventory vs the 13 scripted | Database | **Completed** | P0 | — | 1 d | G0 |
| M0-01-02 | M0 | — script the missing procedures from a live tenant DB | Database | **Completed** | P0 | M0-01-01 | 2 d | G0 |
| M0-01-03 | M0 | — deployment script + rebuild runbook | Database | **Ready**¹ ²¹ | P0 | M0-01-02 | 1 d | G0 |
| M0-02 | M0 | Confirm stored-procedure drift across tenants (Q-14) | Investigation | **Completed**⁶ | P1 | M0-01-02 | 1 d | G0 |
| M0-12 | M0 | Test project + calculation tests *(parent)* | Testing | Not Started | P0 | M0-07 | 3 d | G0 |
| M0-12-01 | M0 | — create the test project and wire it into CI | Testing | **Completed**¹² | P0 | M0-07 | 0.5 d | G0 |
| M0-12-02 | M0 | — characterisation tests for `CalculationService` | Testing | **Completed**¹⁴ | P0 | M0-12-01 | 2.5 d | G0 |
| M0-13 | M0 | Characterisation tests for `StockManagerService` | Testing | **Completed**¹³ | P0 | M0-12-01 | 3 d | G0 |
| M0-09 | M0 | Fix the two unreachable delete guards (R-08) | Backend | **Completed**¹⁵ | P1 | M0-12-01 | 0.5 d | G0 |
| M0-10 | M0 | Audit all `CanDelete…Async` guards (INV-025) | Investigation | **Ready** | P1 | M0-09 | 2 d | G0 |
| M0-06 | M0 | Remove the seeded default Administrator credential | Security | **Ready** | P1 | M0-12-01 | 1 d | G0 |
| M0-14 | M0 | Gate `DetailedErrors` on `IsDevelopment()` | Security | **Completed**¹⁰ | P2 | M0-03-01 | 0.5 d | G0 |
| M0-11 | M0 | **Product decision** — silent FIFO under-issue (Q-01) | Product Decision | **Ready**¹⁷ | P0 | M0-13 | decision | G0 |

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
| M2-A01 | M2 | Server-side screen-right authorization *(parent)* | Security | **In Progress** | P0 | G0 | 1–2 wks | G2 |
| M2-A01-01 | M2 | — implementation spec from ADR-004 | Architecture | **Completed**¹⁸ | P0 | G0 *(exception)* | 2 d | G2 |
| M2-A01-02 | M2 | — implement `[RequireScreen]` / `[RequireRight]` | Security | **Ready** | P0 | M2-A01-01 | 3 d | G2 |
| M2-A01-03 | M2 | — per-request rights resolution + caching | Security | Blocked | P0 | M2-A01-02 | 2 d | G2 |
| M2-A02 | M2 | Apply to `CurrencyController` + denial tests | Security | Blocked | P0 | M2-A01-03 | 1 d | G2 |
| M2-A03 | M2 | Permission-matrix test harness (CI gate) | Testing | Blocked | P0 | M2-A02 | 3 d | G2 |
| M2-A04 | M2 | Refresh tokens + revocation | Security | Blocked | P0 | M2-A01-02 | 3–5 d | G2 |
| M2-A05 | M2 | Cross-origin SPA tenant resolution + real CORS | Security | Blocked | P0 | M2-A04 | 3–5 d | G2 |
| M2-A06 | M2 | Exception middleware → `ProblemDetails` | Backend | **Ready** | P0 | G0 | 3–5 d | G2 |
| M2-A07 | M2 | `GET /api/v1/me` | Backend | Blocked | P0 | M2-A01-03 | 2 d | G2 |
| M2-A08 | M2 | Row-level scoping + account gates (Q-05…Q-08) | Security | Blocked | P0 | M2-A01-03 | 3 d | G2 |

### M2-B — API structure

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M2-B07 | M2 | Shared `AddVSmartDomain()` DI extension | Backend | **Completed**²⁰ | P0 | G0 | 3 d | G2 |
| M2-B04 | M2 | Decouple `IApprovalService` + 13 `Pages` refs | Backend | **Ready** | P0 | M2-B07 | 1 wk | G2 |
| M2-B01 | M2 | API versioning → `/api/v1` | Backend | **Ready** | P1 | M2-B07 | 1 d | G2 |
| M2-B02 | M2 | Paging / sort / filter contract | Backend | Blocked | P0 | M2-A06 | 1 wk | G2 |
| M2-B03 | M2 | Codify the controller template | Documentation | Blocked | P0 | M2-A02, M2-B02 | 2 d | G2 |
| M2-B05 | M2 | Typed `ScreenCodes` constants (R-10) | Backend | **Ready** | P1 | M2-B07 | 2 d | G2 |
| M2-B06 | M2 | File upload / download endpoints | Backend | Blocked | P1 | M2-A06 | 1 wk | G2 |
| M2-B08 | M2 | Report + print endpoints (ADR-005) | Backend | Blocked | P1 | **M2-B07**, M2-A01-03, G0 | 1 wk | G2 |
| M2-B09 | M2 | Reference-data endpoints + caching | Backend | Blocked | P1 | **M2-B07**, M2-B02 | 3 d | G2 |
| M2-B10 | M2 | OpenAPI + TypeScript client generation in CI | DevOps | Blocked | P0 | M2-B03 | 3 d | G2 |
| M2-B11 | M2 | Health checks + structured logging (R-23) | DevOps | Blocked | P2 | M2-A06 | 3 d | G2 |
| M2-B12 | M2 | Document numbering hardening *(parent)* | Backend | Not Started *(parent — never worked directly)* | P0 | M2-B07 | 1 wk | G2 |
| M2-B12-01 | M2 | — INV-012 numbering investigation | Investigation | **Ready** | P0 | M2-B07 | 2 d | G2 |
| M2-B12-02 | M2 | — verify unique constraints in a live DB (Q-10) | Database | Blocked | P0 | M2-B12-01 | 1 d | G2 |
| M2-B12-03 | M2 | — race-safe allocation + idempotency (R-12) | Backend | Blocked | P0 | M2-B12-02 | 3 d | G2 |

### M2-C — React foundation

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M2-C01 | M2 | Vite + React 19 + TS strict + lint + test + CI | Frontend | **Completed**¹⁹ | P0 | G0 | 3 d | G2 |
| M2-C11 | M2 | Archive the Angular pilot | DevOps | **Ready** | P2 | M2-C01 | 0.5 d | G2 |
| M2-C10 | M2 | Decimal handling — no float money arithmetic | Frontend | **Ready** | P0 | M2-C01 | 2 d | G2 |
| M2-C02 | M2 | Auth: login, refresh, guards, permission store | Frontend | Blocked | P0 | M2-C01, M2-A04, M2-A07 | 1 wk | G2 |
| M2-C04 | M2 | Design-system primitives *(parent)* | Frontend | Not Started *(parent — never worked directly)* | P0 | M2-C01 | 2 wks | G2 |
| M2-C04-01 | M2 | — tokens, theme, light/dark | Frontend | **Needs Review**²² | P0 | M2-C01 | 3 d | G2 |
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
| M0 | 24 | 12 | G0 | ⬜ Not met |
| M1 | 6 | 5 (+1 rolling) | G1 | ✅ Passed 2026-08-12 |
| M2 | 52 | 1 *(M2-B07; also 2 `Needs Review` — M2-A01-01 gate exception¹⁸, M2-C04-01²²)* | G2 | ⬜ Not met |
| M3 | ~100 | 0 | G3 | ⬜ Not met |
| M4 | ~150 | 0 | G4 | ⬜ Not met |
| M5 | 10 | 0 | G5 | ⬜ Not met |
| M6 | 8 | 0 | G6 | ⬜ Not met |

**M0-03-01: `Completed` 2026-08-17.** Reviewed, signed off by the repository owner, and
merged to `master` (`f55db52`). See note ³ above and
[`tasks/M0-03-01.md` § Execution Record](tasks/M0-03-01.md#execution-record-2026-08-17) for
the full record. `M0-03-02` and `M0-14` are now `Ready` — their prerequisite is satisfied.

**M0-03-02: `Completed` 2026-08-18.** Implemented on
`migration/M0-03-02-hardcoded-connection-strings-csharp` (`e6e5295`), validated `PASS` on
attempt 1 of 3, 0 escalations, `scopeOk: true`, no regressions; reviewed by Vivek and merged
to `master` (`ec2f0f3` + `7fbb768`). Full record:
[`tasks/M0-03-02.md` § Execution Record](tasks/M0-03-02.md#execution-record-2026-08-18).
`M0-03-03` was unblocked by this and is itself now `Completed`. An older, superseded branch of
the same name (no `-csharp` suffix) still exists, cut from a pre-M0-15-recut point — **do not
merge it**.

**Currently `Ready`: `M0-06` (P1, 1 d) and `M0-10` (P1, 2 d), as of the `M0-09` merge
(`47b2d2e`, 2026-08-19).** All four tasks released by the `M0-12-01` merge are now
`Completed`: `M0-13`¹³ (`3f6dfa8`), `M0-12-02`¹⁴ (`a83f1e2`), `M0-09`¹⁵ (`47b2d2e`) — and
`M0-09`'s merge in turn released **`M0-10`**. `dotnet test` re-run on `master` after the
merge: **79 passed, 0 failed** — the suite has gone 0 → 11 → 36 → 73 → 79 in a single day.
`dotnet build V.SMART.Api --no-incremental`: **0 errors, 6,694 warnings**, at the 6,695
baseline.

> **`M0-10` matters more than its `Investigation` label suggests.** `M0-09` fixed two
> compute-one/test-another guards, and its validator found **a third, unreported instance of
> the identical defect** at `MfgPoService.cs:613-615` (`CanSalesOrderItemCancelCheckAsync`
> computes `hasCR`, tests `hasRc`) — correctly left unfixed as out of scope, and recorded under
> R-08 / INV-025. **The bug class is therefore confirmed wider than the two instances anyone
> had catalogued**, and `M0-10` is the task that audits the rest. It is no longer a
> speculative sweep; it has a concrete lead and evidence the pattern repeats.

> **`M0-12-02` closed at 11 of 12 criteria, with the twelfth *waived*, not met.** Criterion 8's
> second half — *"the suite passes in CI on the branch"* — requires pushing the branch so a
> hosted Actions run exists, which an execution session may not do. The owner waived it
> in-conversation on 2026-08-19, on the **`M0-07` precedent** (signed off `Completed` with the
> identical gap open, `d79e1a4`). The reasoning, recorded so it is not mistaken for an
> oversight: `M0-12-01` had already proven this pipeline runs this suite end to end — green,
> **red at the `Test - V.SMART.Shared.Tests` step**, green again — so criterion 8 would have
> re-tested the *pipeline* rather than this task. **Both G0 characterisation tasks are now
> done.**

> **`M0-11` is released, and it is now blocked on *you*, not on a task.** Its sole Hard
> prerequisite `M0-13` is `Completed`, so the dependency is genuinely clear. But `M0-11` is a
> **Product Decision** — the Q-01 call on silent FIFO under-issue — and rule 1 of the
> [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) excludes a task
> "blocked on a human step nobody has scheduled… **surfacing it to that owner is itself the
> useful action**". So it stays `Blocked`, with **Vivek** as the named owner, and no runner may
> self-select it. What changed is the *reason*: it is no longer waiting on engineering work.
> `M0-13`'s 25 tests pin the current behaviour — including R-07's silent under-allocation,
> asserted deliberately as-is rather than fixed — so the decision is now made against a fixed
> baseline instead of a moving one.

`M0-12-01` is `Completed`: the owner cleared the Q-21 gate, authorised the push, and instructed
the merge, all in-conversation on 2026-08-19. All 11 acceptance criteria are met — criterion 6
was verified on a hosted runner by the green → red → green loop (`dec5790` green, `821e923`
**red at the `Test - V.SMART.Shared.Tests` step**, `e642797` green), with the runner's own log
showing `Failed: 1, Passed: 11, Total: 12` at the red step. `dotnet test` was re-run on `master`
after the merge: **11 passed, 0 failed.** Full record in footnote 12; **Q-22 resolved** as
option (A).

Of the four tasks the `M0-12-01` merge released, **`M0-12-02` and `M0-13` are the ones G0
actually asks for** — they are the characterisation tests the gate names; `M0-13` is now
implemented (`Needs Review`¹³), as is `M0-12-02` (`Completed`¹⁴) and `M0-09`
(`Needs Review`¹⁵). `M0-06` (1 d, P1) is the only one of the four still `Ready`.

Every other M0 task remains `Completed`, `Blocked` on a named human, or
`Needs Review` and therefore not re-selectable:
---

### ¹⁸ M2-A01-01 — `Needs Review`, executed 2026-08-18 under a **deliberate G0 gate exception**

**This is the first M2 task to be worked, and G0 has not passed.** Recording it here so the
deviation is visible rather than silent.

`M2-A01-01` declares `depends_on: [G0]`, and [KB-080 §9](README.md#9-m2--foundation) states
*"Gate G0 must have passed. Not negotiable."* Zero of G0's seven exit criteria were ticked on
2026-08-18. The task was nonetheless opened by the **explicit in-session decision of the
repository owner (Vivek)**, after the four G0 blockers (`M0-01-03`'s rebuild drill, `M0-04`,
`M0-07`'s hosted-runner/branch-protection gap, `M0-12-01`) were laid out to him.

**Rationale.** `M2-A01-01` produces documentation only and changes no behaviour. The two
things G0 guarantees — a reproducible environment from stored-procedure DDL, and
characterisation tests proving behaviour preservation — are prerequisites for *changing*
behaviour. Every input this task needed already existed in the working tree.

**The exception is confined to `M2-A01-01`.** It does **not** transfer to `M2-A01-02` or any
other M2 task. The moment code is written against the specification, G0's rationale applies
in full: [KB-105](../architecture/server-side-authorization-spec.md) §9 lists verification
that cannot even run until `M0-12-01` creates a test project, and `M0-12-01` is `Blocked`¹².
**Every other M2 task stays `Blocked` on G0.** M2's completed count stays at 0 in the rollup
below — `Needs Review` is not `Completed` ([KB-088 § Who may set COMPLETED](workflow.md#who-may-set-completed)).

**Delivered on `migration/M2-A01-01-authorization-spec`**, documentation only, no `V.SMART/`
file touched: [KB-105](../architecture/server-side-authorization-spec.md) (new,
`status: proposal`), plus INV-037 and an INV-004 amendment in
[`investigation-registry.md`](../investigation-registry.md), three new questions
(**Q-27, Q-28, Q-29**) in [`open-questions.md`](../open-questions.md), and routing entries in
[`INDEX.md`](../INDEX.md). Full record:
[`tasks/M2-A01-01.md` § Execution Record (2026-08-18)](tasks/M2-A01-01.md#execution-record-2026-08-18).

**`Q-28` blocks `M2-A02`.** The task found that `AuthController.Login` never calls
`SyncRightsForUserAsync` while the Blazor login path does, and only for `UserId == 1` — so a
user who has only ever authenticated through the API can hold zero `UserRight` rows and, once
the filter is live, would be 403'd from every annotated endpoint. That must be settled before
the filter is applied to `CurrencyController`, or the vertical slice will fail on its first
request. It does **not** block `M2-A01-02`.

---

**Currently `Ready`:** none, as of the M0-02 deferral merge (2026-08-18). Every M0 task is now
`Completed`, `Blocked` on a named human, or `Needs Review` and therefore not re-selectable:
M0-02 is `Needs Review`⁶ (Q-14 explicitly deferred by Vivek, its named owner); M0-03 is a
`Completed` parent container, never worked directly; M0-03-01/02/03 and M0-14 are `Completed`;
M0-01-03 is `Needs Review`¹, awaiting a human-executed rebuild drill; M0-04 is `Blocked`⁴ on an
unidentified credential owner; M0-07 is `Blocked`⁷ on `origin` push plus GitHub org admin
rights; M0-05 stays `Blocked` because M0-04 has not run; M0-13 is `Needs Review`¹³, awaiting
owner review/merge; M0-09 is `Needs Review`¹⁵, likewise awaiting owner review/merge; `M0-10`
stays `Blocked` behind `M0-09` and `M0-11` behind `M0-13` (genuinely `Completed`, not merely
`Reviewed`, is what the selection rule requires). **The G0 exit gate still needs a human** for
M0-04, M0-01-03's drill, and the branch-protection half of M0-07 (Q-20) — so merging
`M0-12-02`, `M0-09` and completing `M0-06` would **not** clear G0 on its own, and **M2 stays
barred**.
**Active task:** none — see [`current-task.md`](current-task.md). Selection rule for what
becomes active next: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).

**M0-15: `Completed` 2026-08-17.** Reviewed, signed off by the repository owner, and merged to
`master` (`854551f`); the branch has since been deleted. Originally committed on
`migration/M0-15-build-baseline`
(`fd9ae21`). Produced `docs/kb/execution/M0-15-build-baseline.md` (KB-086):
0 errors on Api (6,695 warnings), Web (6,698 warnings, previously unmeasured) and Shared
(13,341 warnings) from two clean runs each; the solution build (previously unverified)
succeeds reproducibly from a clean `obj` (0 errors, 13,367 warnings) but is **Unknown**
without MAUI workloads — no workload-free environment was available to test it. Corrected
this task's own premise that the warning baseline is "largely `MUD0002`" — measured,
`MUD0002` is 130/6,695 (1.94%); `CS86xx` nullable-reference warnings dominate. Pinned the SDK
via a new root `global.json` (`10.0.400`, `rollForward: latestFeature`) after observing the
installed SDK set drift on the same machine since INV-029's 2026-08-12 measurement. Full
record: [`tasks/M0-15.md` § Execution Record](tasks/M0-15.md#execution-record-2026-08-17).
Independently re-validated: PASS on attempt 1 of 3, no scope violations, no regressions. M0-15
remains a **hard** prerequisite for M0-07 until it is reviewed and merged — see
[`dependency-graph.md`](dependency-graph.md) § Ready-task selection rule on why `REVIEW` does
not unblock a merge-dependent successor.

**M0-00: `Completed` 2026-08-14.** `migration/M0-00-vcs-baseline` merged to `master` via
PR #1 (`5fcb2b1`); the follow-up visibility-correction branch
`fix/M0-00a-correct-repo-visibility-finding` merged to `master` directly (`661482a`).
Repository visibility resolved: reported public (flawed test) → corrected to private
(INV-034) → **owner deliberately made it public**, verified rigorously, Q-19 answered. The
one item this document previously recorded as still open — `master` requiring a pull
request — is now also satisfied: **re-checked via the public unauthenticated GitHub API,
2026-08-14: `master` branch protection is `true`** (`GET
/repos/ErpStore/NexERP_B/branches/master` → `"protected": true`, `"protection": {
"enabled": true }`), corroborated the same day by a real push
(`migration/M0-01-03-sp-deployment-and-rebuild-runbook` → `master`, merge commit
`661f042`) receiving `remote: Bypassed rule violations for refs/heads/master: - Changes
must be made through a pull request.` — i.e. a require-PR rule is configured and was
bypassed by an account with override rights, not absent. **This directly contradicts this
document's 2026-08-12 note that `"protected": false`** — the owner must have enabled
protection sometime between the two checks; the exact date/time isn't independently known
and isn't asserted here beyond "before 2026-08-14." All M0-00 acceptance criteria in
[tasks/M0-00.md](tasks/M0-00.md) are now met. M0-15 and M0-08 move to `Ready`.

¹ **M0-01-03: `Needs Review`, not `Blocked`, despite being incomplete.** All repository-side
deliverables are done and already merged to `master` (same merge commit `661f042` above) —
`db/deploy-stored-procedures.ps1`, `db/RUNBOOK-rebuild-tenant-database.md`, the 13 legacy
procedures relocated into `db/stored-procedures/relocated-legacy/`, and the KB updated. What
remains is a human-executed rebuild drill against a real, disposable SQL Server instance
(`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`) — an external prerequisite this
task's own constraints forbid an AI session from performing. G0 exit criterion 1 is **not**
met until that drill runs and succeeds. See
[tasks/M0-01-03.md](tasks/M0-01-03.md#execution-record-2026-08-13) for the full record.

M3/M4 counts are the 14-step pattern multiplied across waves and will firm up as each wave's
task files are generated.

² **M0-15: `Completed` 2026-08-17** — reviewed and signed off by the repository owner; merged to `master` via `854551f`. Prior record follows. All deliverables are done and committed on its own
branch (`fd9ae21`, unmerged) — the baseline document, the `global.json` pin, and every KB-083 /
KB-003 update the task specified are landed. What remains is the human review-and-merge step;
see the note above and [tasks/M0-15.md § Execution Record](tasks/M0-15.md#execution-record-2026-08-17)
for the full record.

³ **M0-03-01: `Completed` 2026-08-17** — reviewed and signed off by the repository owner; merged to `master` via `f55db52`. Prior record follows. Validated `PASS` on attempt 2 of 3 (1
escalation on attempt 1, resolved by re-reading acceptance criterion 8 literally — see
[tasks/M0-03-01.md § Execution Record](tasks/M0-03-01.md#execution-record-2026-08-17)). All
deliverables are committed on `migration/M0-03-01-appsettings-secrets` (`2f1a8cf`), unmerged
— note this is a **shorter branch name** than the one the task's own spec and git strategy
name (`migration/M0-03-01-externalise-appsettings-secrets`); the work is on the shorter one.
`V.SMART/V.SMART.Api/appsettings.json` and `V.SMART.Api.csproj` were already sanitised on
disk by an unknown prior actor before this task ran (the directory is untracked, so git has
no provenance) and are correctly **not** part of this branch's diff. `M0-03-02` stays
`Blocked` — its Hard prerequisite is this task at `Completed`, and `Needs Review` does not
satisfy that per the *Ready-task selection rule*'s "not `REVIEW`" clause.

⁴ **M0-04: `Blocked` — DEFERRED to the end of the milestone by the owner, 2026-08-19.**
The owner confirmed production SQL / GST e-Invoice gateway access is not available now and
will be scheduled at the end of the milestone. `M0-05` (purge secrets from history) is
deferred with it, being blocked on nothing else. **G0 criteria 2 and 3 are correspondingly
deferred** — see [KB-080 § G0 deferral](README.md#g0-deferral--criteria-2-and-3-decided-by-the-repository-owner-2026-08-19).

> **The exposure is live meanwhile, and this is not a bookkeeping detail.** R-01 records live
> database credentials committed to source control, in a repository that is **public** by
> deliberate decision (KB-085 / INV-034). The KB's own assessment is that *"the values are
> compromised regardless"*. `M0-05` cannot fix that on its own: purging history from a public
> repository does not retract what is already cloned, forked or cached. **Rotation — `M0-04`,
> the deferred item — is the only actual remedy.** The owner was told this before deciding.

Original record follows. The 2026-08-17 run opened it and stopped at
classification without cutting a branch or attempting work — `tasksAttempted: 0`. Its blocking
dependency is organisational, not technical: the actual rotation needs a named person with
production SQL Server access and access to the GST e-Invoice / e-Way gateway account, and **no
such person is identified anywhere in the repository**. Who unblocks it is itself the open
question; it must be answered from the operations/infrastructure team, and that person has to
participate in-session or the rotation stays blocked pending their availability.

The status is recorded here so the *Ready-task selection rule* stops re-selecting it — while it
read `Ready` at P0, every run picked it and stopped in the same place. Move it back to `Ready`
once an owner is named.

**Part of this task is not blocked.** The task file's own objective splits it: the rotation
runbook (`docs/runbooks/credential-rotation.md`, which does not exist yet), the credential-usage
inventory, and the human verification checklist are all deliverable without production access,
and the task file offers `Needs Review` for those documents alone as a legitimate end state. The
run did not produce them — it halted on the blocked half. That documentation remains available
work for whoever picks this up.

⁵ **M0-08: `Completed` 2026-08-17** — reviewed and signed off by the repository owner; merged to `master` via `f873a9a`. Prior record follows. Validated `PASS` on attempt 1 of 3. All
deliverables are committed on `migration/M0-08-build-output-guard` (`963909f`, cut from
`master` @ `4994fcf`), unmerged — build-output/IDE-state/dependency-directory audit is clean
(2,452 tracked paths, guard pattern produces no output), `tools/check-no-build-output.sh` is
committed and exits 0 on the tree / 1 on a deliberately-added violation (proven in a scratch
repo), root `.gitignore` covers the relevant patterns independent of the nested
`frontend/vsmart-erp/.gitignore`, R-14 is corrected in place (not history-rewritten — none was
needed), and INV-029 carries the negative-result amendment. `dotnet build
V.SMART/V.SMART.Api/V.SMART.Api.csproj` → 0 errors, 6,695 warnings (baseline-exact). A
superseded earlier attempt also exists at `migration/M0-08-gitignore-build-output`
(`e0a7092`) — left unmerged and untouched; a reviewer should pick one branch to merge. Full
record: [tasks/M0-08.md § Execution Record](tasks/M0-08.md#execution-record-2026-08-17).
`M0-07` is now **`Ready`** — both its Hard prerequisites (`M0-15`, `M0-08`) reached `Completed`
on 2026-08-17 when the repository owner signed them off, so the *Ready-task selection rule*'s
"not `REVIEW`" clause no longer excludes it. It is the top P0 candidate, and clearing it
unblocks nine further tasks behind `M0-12-01`.

⁶ **M0-02: `Completed` 2026-08-18 — Q-14 explicitly deferred by Vivek, then signed off and merged. `Completed` here means the *task* closed, NOT that Q-14 was answered.** Was `Blocked` on a human, not on a task; that block is closed by decision rather than by evidence. Committed on
`migration/M0-02-sp-drift-across-tenants` (`c1ab752`), merged via `8f358ed`; the deferral
itself on `migration/M0-02-defer-q14` (`f4d9482`), merged via `71f2f56`. The **tooling half is
complete**: `db/tools/list-deployed-procedures.sql` extended with `hash_raw` +
`hash_normalised` (Query B, `FINGERPRINT_QUERY_VERSION 2`), `db/tools/compare-tenant-fingerprints.sh`
(classifies `identical`/`cosmetic`/`divergent`/`missing_in_tenant`/`extra_in_tenant`, fails
loudly on header mismatch/malformed row/`NULL` hash/duplicate row — each exercised against
synthetic fixtures, no fabricated CSV committed), `db/RUNBOOK-tenant-drift-check.md`,
`db/drift/README.md`. `db/drift/` holds **zero** tenant fingerprint CSVs, so the **analysis
half could not run** — per the task's own decision rule this is the expected first-pass
outcome, not a failure. Q-14 is recorded in `docs/kb/open-questions.md` as **explicitly
undecided** (not "no drift"); INV-030 is `Partial` (`docs/kb/investigation-registry.md`).
**Blocked on:** a DBA with `VIEW DEFINITION` on ≥2 tenant databases, plus a working tenant
list (Q-12 unanswered) — a session may not acquire or reuse a credential. **Owner:** DBA —
first candidate operator **PavanKunar** (ran the M0-01-02 capture); the migration lead must
also decide which database the "baseline" label denotes, given the `IQSMARTDEMO_DB_2025-26`
→ `NexGenErpDb` provenance caveat in `db/stored-procedures/CAPTURE-STATUS.md`. Full record:
[`tasks/M0-02.md` § Execution record](tasks/M0-02.md#execution-record--2026-08-17-tooling-half).
To resume: hand `db/RUNBOOK-tenant-drift-check.md` to the DBA, drop the resulting CSVs into
`db/drift/`, then re-open at the task's Implementation Steps §9 — do not re-derive the
tooling.

**Sign-off, 2026-08-18.** **Vivek** — who is also Q-14's named owner — signed off M0-02 and the
deferral is merged (`71f2f56`). The task moves `Needs Review` → `Completed`; the M0 rollup goes
from 10 to 11. Nothing depends on M0-02, so no other row moves.

**Read this closure precisely, because it is easy to misread.** `Completed` means the *task*
discharged its obligation — [KB-080 §7](README.md) accepts "Q-14 answered **or explicitly
deferred with reason**", and this took the second path. It does **not** mean the question was
answered. **Zero tenants were fingerprinted and zero compared, so stored-procedure drift is
`undecided` — never "no drift".** A single fingerprint compared against nothing classifies
every procedure `identical`; that is arithmetic, not evidence. INV-030 correctly remains
`Partial` and KB-103 §4 correctly remains `TBD`; neither should be "finished" by anyone tidying
up.

**Risk accepted while deferred:** `db/stored-procedures/` stays a single artefact set *by
assumption*, and `db/deploy-stored-procedures.ps1` has no per-tenant path — so a deployment can
overwrite one tenant's customised procedure with another tenant's, silently, with no test to
catch it (INV-023). The captured DDL's provenance compounds it: it originated in the demo
database `IQSMARTDEMO_DB_2025-26`, so it may describe no production tenant at all. **Reopen on
any CSV landing in `db/drift/`, or on any per-tenant report or statutory-document surprise in
the field.**

**Closed by deferral, 2026-08-18.** **Vivek** (repository owner / migration lead), as the
**named owner**, explicitly deferred Q-14 rather than schedule DBA time. That is a valid close
for the G0 deliverable, which [KB-080 §7](README.md) states as "Q-14 answered **or explicitly
deferred with reason**". **Zero tenants were fingerprinted and zero compared, so drift is
undecided — this is emphatically not a finding of "no drift".** Risk knowingly accepted:
`db/stored-procedures/` stays a single artefact set *by assumption* and
`db/deploy-stored-procedures.ps1` has no per-tenant path, so a deployment can overwrite one
tenant's customised procedure with another's, silently, with no test to catch it. Reopen on any
CSV landing in `db/drift/` or any per-tenant report surprise in the field; the tooling is
complete and must not be re-derived. Full record:
[`tasks/M0-02.md` § Deferral](tasks/M0-02.md#deferral--2026-08-18-q-14-explicitly-deferred).

⁷ **M0-07: `Completed` 2026-08-18 — signed off by Vivek, with one acceptance criterion
explicitly carried forward (see below).** It had been `Blocked` on six acceptance criteria that
could not be satisfied from this workstation. Vivek cleared five of them on 2026-08-18:

| Criterion | State |
|---|---|
| Branch pushed to `origin` | ✅ `migration/M0-07-ci-pipeline` pushed |
| A GitHub-hosted Actions run executes | ✅ run [`32158375284`](https://github.com/ErpStore/NexERP_B/actions/runs/32158375284) on `772fea3` |
| That run goes **green** | ✅ `conclusion: success`, all ten steps, 16:05:37→16:12:54 UTC (~7m17s) |
| Both analyzer warning gates pass on a runner | ✅ `Analyzer warning gate — V.SMART.Api` and `— V.SMART.Web` both `success` |
| Merged to `master` | ✅ this merge |
| Branch protection requires the check | ❌ **still outstanding** — a GitHub settings action |

The hosted run also settles what local verification could not: the gate passes against the
committed baseline as measured by the runner, after `M0-03-02`, `M0-03-03` and `M0-14` had
already landed on `master`. The hygiene guard (`check-no-build-output.sh`, from `M0-08`) is now
actually enforced by CI rather than only documented.

**Sign-off, 2026-08-18 — and the one criterion carried forward, not met.** Vivek signed off
M0-07 with **five of six** acceptance criteria satisfied. The sixth — *branch protection
requires the CI check as a required status* — is **still not done**, and `Completed` here does
not assert otherwise. It is a GitHub settings action no session can perform.

**Correction, same day — where that setting lives is NOT confirmed.** An earlier version of this
footnote stated that a **ruleset** already existed on `master` and that only adding the check to
it remained. That was an inference from GitHub's push output ("Bypassed rule violations … Changes
must be made through a pull request"), and the API contradicts it:
`GET /repos/ErpStore/NexERP_B/rulesets` returns `[]`, including with `?includes_parents=true`, so
**no ruleset is visible at all** — the repository owner confirms none was created.

What *is* Confirmed: `GET /repos/ErpStore/NexERP_B/branches/master` reports `"protected": true`,
so protection exists by some mechanism. Which mechanism is **Unknown** from an unauthenticated
session — `/branches/master/protection` and `/orgs/ErpStore/rulesets` both return `401`. Most
likely **classic branch protection** (Settings → **Branches**), possibly an org-level ruleset
that an unauthenticated caller cannot enumerate. **Check Settings → Branches first, not
Settings → Rules.** Recorded rather than quietly amended, because M0-07's outstanding criterion
depends on knowing where the control actually is.

**Consequence while it stays undone:** CI runs and reports, but nothing *enforces* it. A red
build does not block a merge, and `master` can still be pushed directly. The pipeline is
advisory until that setting lands.

**What this sign-off unblocks:** `M0-12-01` (create the test project and wire it into CI) moves
`Blocked` → `Ready` — the first `Ready` task since M0-14 closed. Behind it, still `Blocked` on
`M0-12-01` itself: `M0-12-02`, `M0-13`, `M0-09`, `M0-06`, and transitively `M0-10` and `M0-11`.

The earlier diagnosis was explicit that **the pipeline, the warning gate and the documentation
contain no defect** — only their verification was blocked. The green run confirms that.

The work is committed and unmerged on `migration/M0-07-ci-pipeline` (four commits, ~1,500
lines): the restore/build workflow, the analyzer warning gate with a committed baseline,
`tools/compare-warnings.ps1` and `tools/compare-warnings.sh`, plus the task and KB records.

**Blocked on:** a person with push access to `origin` and GitHub org admin rights on branch
protection. **Owner:** repository owner / DevOps. Once they push the branch and let Actions
run, validation can resume from the same commit — do not re-implement.

Recorded here so the *Ready-task selection rule* stops re-selecting it: while this row read
`Ready` at P0 it was the top candidate, so every run picked it and stopped in the same place.
Note the branch carries its own copy of this status change; reconcile when it is merged.

⁸ **M0-03-02: `Completed` 2026-08-18 — reviewed, approved by Vivek, and merged.** It sat at `Needs Review` until that sign-off, because per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed), this project requires
human review and merge before a task can leave `Needs Review` — an autonomous session may
never set `Completed` itself. All in-scope deliverables are done and committed on
`migration/M0-03-02-hardcoded-connection-strings-csharp` (`e6e5295`): both design-time
factories and the MAUI host read connection strings from configuration with no default
fallback and an actionable throw when unset; the two e-Invoice/e-Way gateway-credential
comments are deleted with `GetUserNameandPassword`'s signature unchanged in both services;
`git grep -n "Password="/"154.61"/"User Id=sa" -- "V.SMART/"` are all empty; the Api build is
0 errors / 6,694 warnings (baseline 6,695, one fewer); KB-060 R-01 and the investigation
registry are amended with `file:line` evidence. What remains is the human review-and-merge
step and, per the task's own scope limit, a MAUI-workload-capable build of
`V.SMART/V.SMART/V.SMART.csproj`, which no session in this environment can perform. See
[tasks/M0-03-02.md § Execution Record](tasks/M0-03-02.md#execution-record-2026-08-18) for the
full record.

**Sign-off, 2026-08-18.** **Vivek** reviewed M0-03-02 and approved it ("looks fine"), and it
was merged to `master`: `ec2f0f3` (implementation, `e6e5295` + runner state `b62440c`) and
`7fbb768` (knowledge-base close-out). That is the human review-and-merge step footnote 8
describes, so the task moves `Needs Review` → `Completed` and `M0-03-03` moves `Blocked` →
`Ready`, its Hard prerequisite now satisfied. **One verification gap is carried forward, not
closed:** a MAUI-workload-capable build of `V.SMART/V.SMART/V.SMART.csproj` was never run —
no session in this environment can perform one — so `MauiProgram.cs`'s configuration read is
verified by inspection and by the `V.SMART.Shared` build, not by building the MAUI host. The
close-out commit `3378656` was originally made directly on `master`, contrary to the branch
rule; it was moved to `kb/M0-03-02-closeout` and merged through review instead.

⁹ **M0-03-03: `Completed` 2026-08-18 — reviewed, approved by Vivek, and merged.** Implemented and committed on
`migration/M0-03-03-startup-config-validation` (`34be11a`), merged to `master` 2026-08-18.  That commit's parent is
`d4ba526` — `master`'s tip at the time this task opened, not `0a20d62` as the branch's own
close-out records first stated; `0a20d62` is only a transitive ancestor, corrected here.
Independently validated **PASS** on attempt 1 of 3, 0 escalations, `scopeOk: true`,
`failureCategory: none` — twelve of thirteen acceptance criteria `MET`, the thirteenth (MAUI
head build) correctly declared not checkable in this environment. No regressions found across
the full branch diff (12 files, additive-or-delegating only).
`V.SMART.Shared/Services/StartupConfigurationValidator.cs` (new) is the single place that
decides whether `ConnectionStrings:MasterDb` and, for the API,
`Jwt:Secret`/`Jwt:Issuer`/`Jwt:Audience` are acceptable; both hosts call it before any
dependent registration; the known-defaults deny-list is stored as SHA-256 digests only, six of
seven independently re-derived from git history by the validator. Full record:
[`tasks/M0-03-03.md` § Execution Record](tasks/M0-03-03.md#execution-record-2026-08-18--close-out-reconciled-to-master).
The implementing session had incorrectly self-set `status: Completed` in the task file; this
close-out corrects it to `Needs Review` per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) — the task's completion
conditions include a human review-and-merge step, which no session may perform on its own
authority. `M0-14` remains the only other `Ready` P0/P2 candidate — see the *Currently `Ready`*
note above, now current again.

One open item carried forward for **M0-04**'s author (not a defect in this task): one of the
seven deny-list digests
(`af0dd1d6b96a33ecd30ef530a7c245e9d91358a0ffa6ca255052f507edfdaba3`, at
`StartupConfigurationValidator.cs:60`) could not be reproduced from git history — its
provenance (a pre-M0-03-01 `V.SMART.Api/appsettings.json` value that was never committed,
per INV-029's untracked-directory finding) is stated honestly in the code comment. A digest
cannot leak the value it covers; M0-04 is positioned to confirm or replace it against the real
historical value during rotation.

¹⁰ **M0-14: `Completed` 2026-08-18 — reviewed, approved by Vivek, and merged.** Implemented and committed on
`migration/M0-14-gate-detailed-errors` (`db41ebc`, cut on top of `master@028e834`, i.e. after
both `M0-03-01` and `M0-03-03` had already landed on `master` — no same-file merge was
actually needed in practice, despite the task file's conflict-risk warning). Independently
validated **PASS** on attempt 1 of 3, 0 escalations, `scopeOk: true`, `failureCategory: none`
— all eleven acceptance criteria `MET`; the two-environment manual runtime check was correctly
reported as not performed (no reachable SQL Server instance in this environment, independently
confirmed by the validator), which the task file's own fallback instruction treats as an
acceptable outcome, not a gap. `Program.cs:198`'s `options.DetailedErrors` now derives from
`builder.Environment.IsDevelopment()`; the dead `"DetailedError"` key at
`appsettings.json:14` was deleted after `git grep` proved it unbound. KB-060 R-20 marked
resolved with exact evidence; KB-003 INV-029 amended with the negative-binding finding. Full
record:
[`tasks/M0-14.md` § Execution Record](tasks/M0-14.md#execution-record-2026-08-18).
The implementing session had incorrectly self-set `status: Completed` in the task file; this
close-out corrects it to `Needs Review` per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) — the task's completion
conditions include a human review-and-merge step, which no session may perform on its own
authority. No task in the tracker is `Ready` after this close-out — see the *Currently
`Ready`* note above.

**Sign-off, 2026-08-18.** **Vivek** reviewed M0-14 and approved it ("looks fine"), and it was
merged to `master` as `275c6e2`. That is the human review-and-merge step this footnote
describes, so the task moves `Needs Review` → `Completed`; the M0 rollup goes from 7 to 8. No
task depends on M0-14, so nothing else moves with it. **The queue stays empty:** M0's remaining
work is blocked on three human owners — M0-04 (production SQL / GST gateway credential owner,
still unidentified), M0-07 (`origin` push + GitHub org admin), and M0-01-03 (the rebuild drill
against a disposable SQL Server, a hard G0 exit criterion). `M0-03-03` is merged but still
awaits its own sign-off, and `M0-02`'s Q-14 deferral is committed on
`migration/M0-02-defer-q14` and still unmerged.

¹¹ **M0-03: `Completed` 2026-08-18 — the parent closes with its last child.** All three
children are `Completed` and merged: `M0-03-01` (`f55db52`), `M0-03-02` (`ec2f0f3` + `7fbb768`)
and `M0-03-03` (`028e834`, signed off 2026-08-18). The parent was left reading `Ready`, which
was actively misleading — a container is never worked directly, so a `Ready` P0 row invited
every selection pass to consider a task nothing could execute.

**Its Combined Acceptance Criteria were verified against the merged code on `master`, not
assumed from the children's status** (2026-08-18, all ten):
`git grep "Password=" -- "V.SMART/"` returns nothing outside `Migrations/`; no JWT secret
literal remains; both `appsettings.json` files carry empty-valued keys only; both design-time
factories resolve through `DesignTimeConnectionString.Resolve` (environment variable →
configuration → actionable throw); `MauiProgram.cs` reads configuration with the three
commented alternatives gone; `StartupConfigurationValidator.Validate` is wired into
`V.SMART.Api/Program.cs:25` (`requireJwt: true`) and `V.SMART.Web/Program.cs:186`
(`requireJwt: false`), enforcing a 32-byte minimum and a known-default digest deny-list;
`docs/CONFIGURATION.md` documents the developer keys; the Api build is 0 errors; and R-01
carries `file:line` evidence for all five C# files.

**What this does not do:** M0-03 closes the *working-tree* half of G0's secrets criterion
only. The **history** half is `M0-05`, which stays `Blocked` — its other prerequisite `M0-04`
(rotation) is still blocked on an unidentified owner. The committed credentials remain live
and exposed in git history until both run.

¹² **M0-12-01: `Ready` — attempt 1 aborted on a transient API error, 2026-08-18. Not blocked on
anything; just re-run.** (The close-out below was written as `Blocked` "on a human"; that
diagnosis was wrong and is corrected at the end of this footnote.) `M0-07` had already
satisfied this task's sole Hard prerequisite and it was correctly selected `Ready` and
dispatched (attempt 1 of 3, `opus`). The dispatched implementer **returned no result at all** —
no diff, no text, no tool output — so the validator also returned no verdict
(`{"verdict": "none", "note": "validation did not complete"}`). Verified at close-out: no
`migration/M0-12-01-*` branch exists, no `tests/` directory exists at the repository root,
`git status --porcelain` is clean, and the `master` tip is unchanged at `d79e1a4`. **Nothing
was implemented; nothing needs to be reverted.** This is not a specification defect (contrast
`M0-03-01`, which failed validation with a concrete, quoted exception) and not a missing
credential (contrast `M0-02`/`M0-04`) — it is an execution/dispatch anomaly with no artefact to
diagnose. Full record: [`tasks/M0-12-01.md` § Execution Record (2026-08-18)](tasks/M0-12-01.md#execution-record-2026-08-18);
attempt logged in [`failure-log.md`](failure-log.md#m0-12-01--attempt-1--2026-08-18).
**Root cause — CONFIRMED, and it is not a dispatch defect.** The close-out above recorded the
cause as "unknown" and asked the repository owner to audit the runner's agent-invocation layer.
That was wrong, and is corrected here: the workflow's own completion record shows **both**
agents for this cycle terminated on a transient upstream API error —

```
[investigate:M0-12-01] failed: API Error: 529 Overloaded
[implement:M0-12-01]   failed: API Error: 529 Overloaded
```

— i.e. 2 of the run's 5 agents errored server-side (`agents_error: 2`). The implementer returned
nothing because it never ran to completion, not because dispatch mis-fired. **No owner action is
required and no tooling audit is warranted.** The correct response is simply to **re-run the
runner**; the task specification is unchanged and needs no re-work. Attempts used: 1 of 3, so
three remain. Status is therefore restored to `Ready`, not left `Blocked` on a human, because
nothing human-held is in the way.
**Blocks:** `M0-12-02`, `M0-13`, `M0-09`, `M0-06`, and transitively `M0-10` and `M0-11` — the
same four-plus-two tasks M0-07's completion was about to unblock.

**Update, 2026-08-18 — attempt 2 repeated the exact same failure; status moves back to
`Blocked`, this time on a human, not on nothing.** The re-dispatch this footnote recommended
was carried out: attempt 2 of 3, `opus`, same classification. **The implementer again
returned no result at all** — no diff, no text, no tool output — and the validator again
recorded `{"verdict": "none", "note": "validation did not complete"}`. Re-verified at
close-out: still no `migration/M0-12-01-*` branch, still no `tests/` directory, `git
status --porcelain` clean, `master` tip unchanged. Nothing was implemented on either attempt.

This footnote's own attempt-1 text named the exact falsification condition that has now been
met: *"If attempt 2 fails the same way, that repetition is the signal worth investigating — a
single 529 is not."* Two consecutive empty implementer returns, same task, same model, same
complexity classification, is that repetition. This close-out session cannot independently
re-confirm a second `529 Overloaded` — the workflow's agent-completion log that let the first
misdiagnosis get corrected is visible only from inside the run that produced it, not from a
close-out session reading the repository afterward. **So this is recorded honestly as
`Blocked` on a human — specifically, someone who can read the runner's dispatch/agent-invocation
logs for both cycles and confirm or rule out a systemic (not transient) cause — rather than
re-dispatched a third time on the same unverified assumption.** Attempts used: **2 of 3 — one remains**, held in reserve until the cause is checked.
*(Denominator corrected 2026-08-19: the budget is 3, not 4 — [KB-091 §6.4](autonomous-runner.md#64-retry-rules),
"Attempt 3 fails → BLOCKED. Stop. … Do not attempt a fourth", matching `migration-runner.js:43`
`maxRetries: 2, // 2 retries = up to 3 implementation attempts`. Earlier text here read "of 4",
which no authority supports.)* Full record:
[`tasks/M0-12-01.md` § Execution Record (2026-08-18) — Attempt 2](tasks/M0-12-01.md#execution-record-2026-08-18--attempt-2-repeated-empty-return);
attempt logged in [`failure-log.md`](failure-log.md#m0-12-01--attempt-2--2026-08-18). See also
open question **Q-21** in [`open-questions.md`](../open-questions.md).

**Update, 2026-08-19 — Q-21 answered. The *investigation* the gate asked for is complete; the
*decision* it asked for is still the owner's, and the task stays `Blocked` until he makes it.**
The paragraph above is wrong on one point of fact: the agent-completion log is **not**
visible only from inside its own run. The per-agent transcripts persist on disk at
`~/.claude/projects/<project>/<sessionId>/subagents/workflows/<runId>/agent-<agentId>.jsonl`,
and reading them settles the question outright — **every agent in both attempts ended on
`"apiErrorStatus":529, "error":"server_error"`.** Attempt 1's `migration-investigator` recorded
`529` at 16:41:00Z (`req_011CeAYN4EMJrAe6z7CZ1qX8`) **after 158,887 bytes of successful tool
work**; its `migration-implementer` at 16:44:18Z (`req_011CeAYdkQF6u4n5sSMXvwoi`) died on its
first call. Attempt 2 repeated the pattern across three agents. An investigator that reads
158 KB of source before dying was dispatched correctly and was running normally, which rules
out the systemic-dispatch hypothesis this footnote was holding the task for. Corroborated the
same day by two runner invocations dispatching 4 of 4 agents with `agents_error: 0` and
`agents_empty_result: 0`.

> **Gate CLEARED 2026-08-19 by the repository owner.** Sequence, recorded because the
> distinction matters: a session took the evidence above, moved `M0-12-01` to `Ready` and
> dispatched it **on its own authority**; the harness safety classifier stopped that run,
> correctly, because this footnote's gate reserves the confirmation for **a human**, and
> performing the check does not confer authority to declare it satisfied. The flip was
> withdrawn, the evidence was put to the owner, and **Vivek cleared it in his own words** —
> *"yes, the 529 evidence clears the gate — run it"*. `M0-12-01` is `Ready` **on his
> authority**. The precedent this sets is narrow: an AI session may *gather* the evidence a
> human-owned gate asks for, but only the named human may declare the gate passed.
>
> **Still undecided, and deliberately not assumed:** whether the two `529` aborts consumed
> retry budget at all (the KB-091 §6.4 reading below). The conservative count governs —
> **2 of 3 used, one remains.** If attempt 3 also dies on infrastructure without producing
> work, halt and put *that* question to the owner rather than declaring the task `Blocked` for
> good.

> **Open interpretation, flagged not applied.** [KB-091 §6.4](autonomous-runner.md#64-retry-rules)
> counts *validation failures* ("Attempt 1 fails → `DIAGNOSING`… Attempt 2 fails → `ESCALATED`").
> Neither M0-12-01 attempt produced work or a validation verdict — both aborted on infrastructure
> before implementing anything. On that reading the two `529` aborts should not consume the
> retry budget at all, and the task still has its full three. **This session did not apply that
> reading** — it is recorded as a question for the owner, and the conservative count (one
> attempt left) governs. If a third attempt also dies on a `529`, this is the paragraph to
> revisit before declaring the task `Blocked` for good.

**Owner to unblock (gate cleared): none — cleared 2026-08-19 by Vivek.** The original wording
of this row, which sought a dispatch-layer administrator, is retained below for history:
whoever administers the autonomous runner / agent-dispatch
infrastructure for this project. No such person is named anywhere in the repository; in their
absence, the repository owner (**Vivek**) is the fallback contact, consistent with every other
`Blocked`-on-a-human row in this table that lacks a more specific named owner (compare `M0-04`
footnote 4). This is a **runner-health** question, not a task-specification question — no
change to `tasks/M0-12-01.md` is indicated by anything found this session.

**Update, 2026-08-19 — attempt 3 of 3, dispatched after Vivek cleared the gate above, produced
real work and moved the task back to `Blocked`, this time on a content decision, not a
dispatch mystery.** The implementer committed `9557de2` on
`migration/M0-12-01-test-project`: the test project (`tests/V.SMART.Shared.Tests/`, `.csproj` +
6 source files), the `.sln` registration (19 lines, all 6 platform configs), the CI test step
in `.github/workflows/ci.yml`, `INV-031` (`Complete`, 8 findings, each `Confirmed`/`Inferred`/
`Unknown`-tagged), and the KB-083/KB-060 documentation updates. This close-out session
independently re-ran the evidence rather than trusting the report: `dotnet test` → 11
discovered, 11 passed, 0 failed; `dotnet build V.SMART.Api` → 0 errors, 6,695 warnings
(baseline, no new warnings); `git status --porcelain` clean of build output; `git diff --stat
HEAD~1 HEAD -- V.SMART/` empty. **10 of the 11 acceptance criteria are `MET`.**

**What is not met — criterion 6, and why it cannot be met from inside an execution session.**
The criterion requires observing a deliberately-failing test turn a live GitHub Actions run
red, on a pushed branch, with the run identifier recorded. Task step 14
(`tasks/M0-12-01.md:289-291`) instructs exactly this — "push the branch, confirm CI goes red" —
but `CLAUDE.md` § Standing constraints is explicit: *"Never merge or push without an explicit
instruction in the current conversation"*, and the runner dispatches with `allowMerge=false`.
No local substitute exists (`gh`, `act`, docker — none installed on this workstation). The
branch has never been pushed (`git branch -r` shows no
`origin/migration/M0-12-01-test-project`; `git rev-parse --abbrev-ref @{u}` reports no
upstream configured), so the workflow has never executed on a hosted runner and no run
identifier exists. **This is the identical gap already carried, and accepted, for `M0-07`'s
own CI criterion** — see [`ci-pipeline.md`](ci-pipeline.md) §8 and this footnote's earlier
text above (`M0-07` `Blocked`⁷) — and `M0-07` was signed off `Completed` with that gap open
(`d79e1a4`). Criterion 6 inherits an existing, already-precedented gap; it does not create a
new category of problem.

**Attempts used: 3 of 3 — budget exhausted** ([KB-091 §6.4](autonomous-runner.md#64-retry-rules)).
A fourth dispatch would not help: the wall is push authority, not code, so it would reproduce
`9557de2` and stop identically. Both the validator (`failureCategory: environment`) and an
independent debugger pass (`disposition: blocked`) agree. **Status: `Blocked` on the
repository owner** — a decision, not something further investigation or implementation can
resolve. **Blocks, transitively, unchanged:** `M0-12-02`, `M0-13`, `M0-09`, `M0-06`, `M0-10`,
`M0-11`.

**Owner to unblock: Vivek**, choosing one of two options (recorded as **Q-22** in
[`open-questions.md`](../open-questions.md)):

- **A.** Explicitly authorise pushing `migration/M0-12-01-test-project` in-conversation; then
  break one smoke assertion, observe red, revert, and record the run identifier. Best paired
  with resolving Q-20 (hosted-runner availability) first, or the push may hit the same
  unanswered organisational question `M0-07` already flagged.
- **B.** Waive criterion 6 for `M0-12-01`, re-homing it onto whichever task first pushes a
  branch — consistent with the `M0-07` precedent already accepted into `Completed`.

Full record: [`tasks/M0-12-01.md` § Execution Record
(2026-08-19)](tasks/M0-12-01.md#execution-record-2026-08-19);
[`failure-log.md` § M0-12-01 · attempt 3](failure-log.md#m0-12-01--attempt-3--2026-08-19).

**M0-13: `Needs Review` 2026-08-19.** Implemented on
`migration/M0-13-stockmanagerservice-characterisation` (`9d8d7be`), attempt 1 of 3, 0
escalations. Validator verdict **`PASS`**, `scopeOk: true`, `failureCategory: none` — all 12
acceptance criteria `MET`, no regressions found. 25 new tests (suite 11 → 36, all green, run
twice) pin all 16 statements of BR-STK-001 and BR-STK-002/R-07, including the R-07 drift
asserted numerically on both the create and update paths and the statement-16 asymmetry (100
against 0 throws; 100 against 1 succeeds and drifts by 99). `git diff --stat master...HEAD`
shows zero files under `V.SMART/`; `dotnet build V.SMART.Api` still 0 errors / 6,694 warnings
(at baseline). KB-030, KB-060, KB-004 (Q-01) and the investigation registry (INV-011
annotated, new row INV-036) were all updated in the same commit. **R-07 stays open — it is
pinned, not fixed**, exactly as the task required. Full record:
[`tasks/M0-13.md` § Execution Record (2026-08-19)](tasks/M0-13.md#execution-record-2026-08-19).

Not `Completed`: this task's own completion conditions include no human step, but this
project's standing convention ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed))
reserves that status for the repository owner once the branch is reviewed and merged — the
same convention already applied to M0-03, M0-08, M0-12-01 and every other `PASS`-validated
task in this milestone. **Unblocks nothing yet**: `M0-11` (Product Decision, Q-01) names
`M0-13` as a Hard prerequisite and the
[Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) requires that
prerequisite to be genuinely `COMPLETED`, not `REVIEW` — `M0-11` stays `Blocked` until the
owner reviews and merges this branch.

¹⁴ **M0-12-02: `Completed` and merged (`a83f1e2`, 2026-08-19) — 11 of 12 acceptance criteria
`MET`, the twelfth WAIVED by the repository owner.**

> **Read the waiver as a waiver, not as a pass.** Criterion 8's second half — *"the suite
> passes in CI on the branch"* — was **never satisfied**. It requires pushing the branch so a
> hosted Actions run exists, which `CLAUDE.md` forbids an execution session to do. The owner
> waived it in-conversation on the **`M0-07` precedent** (`d79e1a4`, signed off `Completed`
> with the identical gap open). Justification recorded so a later reader does not mistake this
> for sloppiness: `M0-12-01` had already proven this pipeline runs this suite end to end —
> green, **red at the `Test - V.SMART.Shared.Tests` step**, green again — so criterion 8 would
> have re-tested the *pipeline*, not this task. **If a future task needs per-branch CI
> evidence, this waiver is not a precedent for skipping it** — it rests specifically on
> `M0-12-01` having already demonstrated the gate works.

The paragraph below is the pre-merge close-out, retained verbatim for the record.

**Pre-merge state (2026-08-19):** implemented on
`migration/M0-12-02-calculationservice-characterisation` (`050f06b`), attempt 1 of 3, 0
escalations. Validator verdict `FAIL`, `failureCategory: environment`, `scopeOk: true`. 37
new tests (suite 36 → 73, all green, run twice: `Failed: 0, Passed: 73`) pin every row of
BR-CALC-001 (19) and BR-CALC-002 (3), both tax branches with a three-line/three-rate
item-wise case, both `.5` midpoints, a negative `RoundOff`, the two silent early returns with
twelve fields asserted unmutated, the fixed-vs-percentage header-discount asymmetry, and the
unlisted-GST-rate/R-15 pair — all independently re-derived and re-checked against source, not
taken from the implementer's report. `git diff --stat master...HEAD` shows zero files under
`V.SMART/`; `dotnet build V.SMART.Api --no-incremental` → 0 errors, 6,694 warnings, at
baseline. KB-030, KB-060, KB-004 (new **Q-23**, **Q-24**) and KB-003 (INV-011 annotated) were
all updated in the same commit.

**What is not met — criterion 8's second half, and why it cannot be met from inside an
execution session.** *"the suite passes in CI on the branch"* requires pushing
`migration/M0-12-02-calculationservice-characterisation` to `origin` so `ci.yml` executes on
a hosted runner. `git ls-remote --heads origin` lists eight branches and this one is not
among them; `CLAUDE.md` § Standing constraints forbids pushing without an explicit
in-conversation instruction, and this dispatch carried `allow_push=false`. This is the
identical gap already carried for **M0-07** (signed off `Completed` with it open, `d79e1a4`)
and resolved for **M0-12-01** only once the owner explicitly authorised the push (**Q-22**).
Nothing about the branch itself is in question — `ci.yml:183-190` runs the whole test project
with no filter, and its `$LASTEXITCODE` re-raise was already confirmed working on a hosted
runner by M0-12-01's `821e923`.

**Attempts used: 1 of 3** — not exhausted, but a same-spec retry rebuilds `050f06b` and stops
at the identical wall, so no further attempt was spent chasing it
([KB-091 §6.4](autonomous-runner.md#64-retry-rules)). **Status: `Blocked` on the repository
owner** — a decision, not something further investigation or implementation can resolve.
**Blocks, transitively, unchanged:** nothing new — `M0-12-02` was not itself a Hard
prerequisite for any other task; `M0-09` and `M0-06` remain independently `Ready`.

**Owner to unblock: Vivek**, choosing one of two options:

- **A.** Explicitly authorise pushing `migration/M0-12-02-calculationservice-characterisation`
  and observe the `Test - V.SMART.Shared.Tests` step green — the exact route already taken
  for `M0-12-01` under Q-22.
- **B.** Waive the "in CI" half of criterion 8 and re-home it, consistent with the `M0-07`
  precedent already accepted into `Completed` (`d79e1a4`).

Full record: [`tasks/M0-12-02.md` § Execution Record
(2026-08-19)](tasks/M0-12-02.md#execution-record-2026-08-19);
[`failure-log.md` § M0-12-02 · attempt 1](failure-log.md#m0-12-02--attempt-1--2026-08-19) and
its diagnosis entry.

¹⁵ **M0-09: `Completed` and merged (`47b2d2e`, 2026-08-19)** on the owner's in-conversation
instruction. Re-verified on `master` after the merge: `dotnet test` **79 passed, 0 failed**;
`dotnet build V.SMART.Api --no-incremental` **0 errors, 6,694 warnings** (baseline 6,695).
**This released `M0-10`.** Pre-merge record follows. Implemented on
`migration/M0-09-delete-guard-fix` (`8e3b19d`), attempt 1 of 3, 0 escalations. Validator
verdict **`PASS`**, `scopeOk: true`, `failureCategory: none` — every acceptance criterion
`MET`, independently re-derived rather than taken on trust (the validator reproduced the
pre-fix red state itself in a separate detached worktree at `3549571`). `MfgPoService.cs`
changed exactly two identifiers: `hasInvoice` → `hasExpInvoice` (`:504`) and `hasRc` →
`hasCR` (`:525`); no `Message` string, guard order, or query changed. Two new tests
(`MfgPoServiceDeleteGuardTests.cs`) were observed to fail pre-fix and pass post-fix; the
existing Tax Invoice/Route Card guard tests pass throughout. Suite: 79/79 green (73 → 79).
`dotnet build V.SMART.Api` (CI form, `--no-incremental`): 0 errors, 6,693 warnings — at the
6,695 baseline. `git diff --stat master...HEAD` touches one file under `V.SMART/`, two lines,
plus KB-030, KB-060, KB-080 (this file's predecessor version), KB-003, and two new test
files. KB-030's BR-SO-002 was rewritten from live defect to fixed, naming the commit; KB-060's
R-08 was marked resolved on its first action item only, with the second (the wider
`CanDelete…` audit) explicitly left open as `M0-10` / `INV-025`.

**Not `Completed`**, for the same standing reason as `M0-13` and every other `PASS`-validated
task this milestone ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)):
the branch is implemented and validated but not yet reviewed and merged by the repository
owner. **Unblocks nothing yet** — `M0-10` names `M0-09` as a Hard prerequisite and the
[Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) requires that
prerequisite to be genuinely `Completed`, not `Needs Review`, so `M0-10` stays `Blocked`
until this branch is reviewed and merged.

**One validator-found lead, not acted on and not part of this task's scope**: an unreported
instance of the identical compute-one/test-another pattern exists at
`MfgPoService.cs:613-615`, inside `CanSalesOrderItemCancelCheckAsync` — `hasCR` is computed
at `:613` but the guard at `:614-615` tests `hasRc` (the Route Card boolean from `:608`), so
the Contract-Review branch of that line-cancel check is unreachable for the same reason
BR-SO-002's delete guard was. This is pre-existing, outside M0-09's authorised two-line
surface, and does not fail this task. It is recorded as a scope note on `INV-025` (see
`investigation-registry.md`) and as a new bullet under R-08 in
`docs/kb/risks/technical-debt-register.md` (KB-060), both updated in this close-out, so
`M0-10` picks it up without re-deriving it.

Full record: [`tasks/M0-09.md` § Execution Record
(2026-08-19)](tasks/M0-09.md#execution-record-2026-08-19).

¹⁷ **M0-11: `Blocked` → `Ready`, 2026-08-19 — the human step it was waiting for has happened.**
This task was never blocked on engineering. It was blocked on a product decision only the
repository owner could take, and he took it: **Q-01 is answered — *preserve but surface*.** The
API will reproduce today's allocation behaviour exactly (a short issue still succeeds, still
allocates what is available) but the shortfall is returned to the caller and shown, instead of
being silent.

**What changes about this task:** its deliverable is unchanged — `ADR-006-fifo-under-issue.md`
— but it is now written to **record an accepted decision**, not to propose two options for
someone to choose between. The acceptance criteria still apply in full, including arguing both
options in good faith and mapping every behavioural statement to an `M0-13` test by exact name;
a decision brief that omits the rejected option is not a brief, it is an announcement. Note the
task file's own criterion — *"Option B addresses **visibility**, not merely 'leave it as it
is'"* — anticipated exactly this answer.

**What this task does NOT cover, and must not silently absorb:** the owner deferred the
*implementation* of surfacing until after Milestone 2. That work has **no task id yet** and is
not part of `M0-11`, `M0-13`, or any current M2 task. When it is scheduled it must address two
things beyond the obvious: (a) `StockManagerService.cs:154-155` commits an orphan `StockIssue`
row for the full quantity **before** allocation is attempted, so even a *refused* issue leaves
a row that does not match reality; (b) tests `S13`–`S16` pin the current behaviour and must be
updated in the same commit as any change, or they will correctly go red.

**G0 criterion 7 is met** by the answer being recorded in `open-questions.md`; it does not wait
on `ADR-006`. See [KB-080 § Exit Gate — G0](README.md#exit-gate--g0).

¹⁹ **M2-C01: `Completed` and merged (`12f172f`, 2026-08-19) — 14 of 15 acceptance criteria met, the 15th WAIVED.**

> **Read the waiver as a waiver.** Criterion 10's second half — *"the `frontend` job ... is green on the branch"* — was **never satisfied**. It needs a push, which an execution session may not make. Waived by the owner on the `M0-07` (`d79e1a4`) and `M0-12-02` (`a83f1e2`) precedent. Everything locally verifiable was independently re-run before merging: `typecheck`, `lint --max-warnings=0`, `test`, `build` all exit 0; entry chunk **289.69 kB / 90.90 kB gzipped**; `git status` clean after a build; `dotnet test` still **79 passed**. Scope confirmed: no `node_modules/`, `dist/`, `playwright-report/` or `test-results/` committed; `package-lock.json` committed; `frontend/vsmart-erp/` untouched; exactly one component library (`@mantine/core`); the pre-existing CI job intact.

**Footnote renumbered from ¹⁸ to ¹⁹ on merge** — ¹⁸ had just been assigned to `M2-A01-01`, and this branch was cut before that landed. The same cross-branch allocation defect as the six-id KB/INV/Q collision; see [INDEX.md](../INDEX.md) § doc_id allocation.

Pre-merge record follows.
engineering.** Two implementation attempts on `migration/M2-C01-react-app-skeleton`
(`4ac7241`, `8fb8e6d`, `d5182f6`) built `frontend/nexgen-web/` as a Vite 6 + React 19 +
TypeScript-strict workspace and independently re-verified 14 of 15 acceptance criteria `MET`
against commands actually run — `npm ci`, `typecheck`, `lint`, `format:check`, `test`,
`coverage`, `build`, `e2e` all exit 0; only `@mantine/core` is present in the dependency tree;
every ADR-003 major matches; `src/` matches KB-050; no `V.SMART/` or `frontend/vsmart-erp/`
file touched; KB-083 updated. The sole `NOT MET`: criterion 10 (`tasks/M2-C01.md:373-374`),
*"`.github/workflows/ci.yml` contains a `frontend` job … **and it is green on the branch**"* —
the job exists and is well-formed, but no GitHub Actions run can exist without a push, which
`CLAUDE.md` forbids absent an explicit in-conversation instruction (`git ls-remote --heads
origin` does not list this branch; `gh` is not installed on this workstation). This is the
identical wall `M0-07`, `M0-12-01` and `M0-12-02` already hit. **Owner: Vivek (repository
owner) — the only person who can authorise publishing the branch (option A) or waive the
"green on the branch" half as was done for `M0-07` (`d79e1a4`) and offered for `M0-12-02`
(option B).** Full record: [`tasks/M2-C01.md` § Execution Record
(2026-08-19)](tasks/M2-C01.md#execution-record-2026-08-19),
[`failure-log.md`](failure-log.md#m2-c01--attempt-2--2026-08-19).

²⁰ **M2-B07: `Completed` and merged (`ffbb1dd`, 2026-08-19) — every mechanical criterion met, the render criterion WAIVED.**

> **Read the waiver as a waiver.** *"The Blazor app starts and three screens from three different modules render without a DI resolution error"* was **never satisfied**. It needs a signed-in interactive Blazor Server circuit; the one provisioned ERP user's password is hashed and owner-held, and no session may acquire or reuse a credential (Q-14 / R-01). The three screens `302` to `/access-denied` under server-side screen-right authorization — **identically on `master`**, so this is not a regression the task introduced. Waived by the owner on the `M2-C01` (`12f172f`), `M0-12-02` (`a83f1e2`) and `M0-07` (`d79e1a4`) precedent.
>
> **What verified the DI graph instead** — independently re-run before merging, not taken from the run's report: `dotnet test` **84 passed, 0 failed** (79 + 5 new); `dotnet build V.SMART.Api` **0 errors**; and `V.SMART.Web` started, resolved its tenant, queried EF and served `GET /` → **200 with zero DI resolution errors**. The 5 new tests in `tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs` call `BuildServiceProvider(validateScopes: true, validateOnBuild: true)` over the whole graph with host seams supplied — **a stricter check than rendering three screens.**
>
> **It carries a debt, accepted deliberately.** `V.SMART.Api` now opts out of `ValidateOnBuild` (`31a10ba`). `WebApplicationBuilder` enables it automatically in Development, and seven seam-coupled registrations aborted API startup — a **runtime** failure that no compile check catches, and which the first attempt shipped undetected. The opt-out keeps `ValidateScopes` on, names the seven registrations as *measured* rather than assumed, and carries a `REMOVE THIS BLOCK` marker tied to `M2-B06`/`M2-B08`. **This task introduced that loosening.** Tracked in [technical-debt-register.md](../risks/technical-debt-register.md).
>
> **Attempt accounting, corrected.** Two real implement/validate cycles, plus one dispatch lost to an `ENOTFOUND` transport failure that — per the `M0-12-01` precedent — does not consume budget. The pre-merge record below says "3 of 3 exhausted"; **the task closed on an owner waiver, not on budget exhaustion**, and one attempt remained.

Pre-merge record follows.

Every mechanical acceptance criterion in `tasks/M2-B07.md` is `MET`: `AddVSmartDomain()` exists
in `V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs` and is called exactly
once from each of `V.SMART.Api/Program.cs`, `V.SMART.Web/Program.cs` and
`V.SMART/MauiProgram.cs`; per-host domain registrations dropped from 242/243 to 7/8;
`ApplicationDbContext` is still registered through `ITenantDbContextFactory`, never
`AddDbContext`; the union of 249 distinct registrations is preserved exactly, verified by a
mechanical set-diff, not eyeballing; `AddVSmartDomainTests.cs` passes
`BuildServiceProvider(validateScopes: true, validateOnBuild: true)` over the whole graph (5/5
green, 84/84 in the suite); `V.SMART.Api` and `V.SMART.Web` build at 0 errors, at or under their
recorded warning baselines; runtime parity against a `master` worktree is byte-identical on
every route and status code tried. R-26 is `RESOLVED` in
`docs/kb/risks/technical-debt-register.md` with this evidence; R-40 records the API's
`ValidateOnBuild = false` opt-out, deliberately scoped and due for removal at M2-B06/M2-B08.

**One criterion stays unmet, and the reason for that changed between attempt 3 and this
close-out.** Attempt 3 concluded no database was provisioned on the workstation and both hosts
500'd for that reason. This close-out session found that conclusion **wrong**: a SQL Server
Express instance with `NexGenErpDb_Master` and a 197-table tenant database already exists here,
and pointing the connection string at it makes `V.SMART.Web` render `/` at `200` with zero DI
resolution errors. The three named module screens instead `302` to `/access-denied` —
server-side screen-right authorization (ADR-004/M2-A01-01) correctly refusing an
unauthenticated request, identical on `master`. The real gap is a **signed-in interactive
Blazor circuit**: the one provisioned ERP user's password is hashed and owner-held, and no
session may acquire or reuse a credential. **Owner: Vivek** — either (A) sign in as that user in
a browser and open three screens from three different modules, five minutes, or (B) waive the
render half on the recorded evidence (whole-graph `ValidateOnBuild` passing at startup, zero
`Unable to resolve service` in the host log, branch/`master` parity on every route). Attempts
used: 3 of 3. Full record: [`tasks/M2-B07.md` § Execution Record (2026-08-19) — close-out,
attempt 3 of 3](tasks/M2-B07.md#execution-record-2026-08-19--close-out-attempt-3-of-3-session-ends-blocked),
[`failure-log.md`](failure-log.md#m2-b07--attempt-3--2026-08-19).

²¹ **M0-01-03: `Needs Review` → `Ready` 2026-08-19 — the premise that blocked it was false.**

This task has sat unfinished all milestone because the rebuild drill needs a SQL Server to
rebuild *onto*, and three consecutive sessions recorded that no such server was available.
[KB-107](M0-milestone-review.md) built its closing recommendation on that: *"obtain a
disposable SQL Server … nothing else on this list is blocked on so little."*

**A SQL Server Express instance has been installed on the development workstation the whole
time.** Confirmed independently during `M2-B07`, 2026-08-19: `MSSQL$SQLEXPRESS` running,
carrying `NexGenErpDb_Master` and a 197-table `NexGenErpDb`, reachable with
`Server=.\SQLEXPRESS;Trusted_Connection=True` — Windows integrated auth, **no credential
acquired or reused**.

**Why nobody saw it.** Both hosts ship `"MasterDb": ""`
(`V.SMART/V.SMART.Web/appsettings.json:10`, `V.SMART/V.SMART.Api/appsettings.json:9`) and both
user-secrets stores still hold `Database=DoesNotExist_M0-03-01-LocalTest`, left over from
`M0-03-01`'s fail-fast test. Every session read an empty default, found nothing configured, and
**inferred absence from a config default** — then wrote that inference down as fact, where the
next session read it as settled. It was never entered as `Unknown` in
[`open-questions.md`](../open-questions.md); it became `Confirmed` purely by repetition.

**The process lesson, which outlives this task:** a negative result needs the same
`file:line`-grade evidence as a positive one. *"I could not find X"* is a claim about the
search, not about X. `CLAUDE.md` already says never to write an inference so that it reads as
fact — this is what it costs when that slips.

**What this does and does not mean.** The drill is now *runnable*; it has **not been run**.
`db/REBUILD-DRILL-LOG.md` is still a skeleton with every field `TBD`, and G0 criterion 1 is
still **DEFERRED** — but deferred on work, not on hardware, and its owner-agreed deferral now
rests on a stated reason that no longer holds. Use a throwaway database on this instance;
**do not run the drill against `NexGenErpDb`**, which holds the only provisioned user and 150
`UserRights` rows.

**Related, and a genuine blocker for something else:** the `Tenants` row on this instance stores
its connection string in plaintext with `sa` credentials — **Q-32**, which must be answered
before `M0-04`'s rotation is executed, or rotation will break every tenant row that embeds the
password.

²² **M2-C04-01: `Needs Review` 2026-08-20 — validated `PASS`, awaiting owner review and merge.**
Branch `migration/M2-C04-01-design-tokens`, tip `9f886a6`. Not merged, not pushed.

**History, for context.** Attempt 1 (`cdb147a`, 2026-08-19) implemented the full token/theme
layer under `frontend/nexgen-web/src/shared/theme/` and passed all sixteen acceptance criteria
on independent validator re-check, but validated `FAIL`/`regression` on `npm run coverage`
(`vitest.config.ts:38` pins `branches: 100`; the new code was only partly branch-covered). The
retry dispatched for that regression (`migration-debugger`) returned no result to the
orchestrator, but its process left a real, uncommitted, partial fix on disk — preserved
unmodified as `5313c46` with a WIP disclosure, per the `M0-12-01`/`M2-B07` precedent that an
empty agent return does not mean an empty disk.

**Resolution, 2026-08-20 (`9f886a6`).** The partial fix was reviewed, completed and extended:
`ThemeProvider.tsx`, `density.ts` and `useColorScheme.ts` gained coverage for their five
remaining reachable branches (hostile-`localStorage` paths, corrupt-density fallback, no-
`matchMedia` degradation), without touching `vitest.config.ts`'s floor. `npm run coverage`
now exits 0 at **branches 100 %** (statements 95.90 %, functions 86.95 %, lines 95.90 %),
closing the regression honestly rather than by lowering the gate —
`git diff --stat aaae3a0 HEAD -- frontend/nexgen-web/vitest.config.ts` is empty. An independent
validator then re-ran all sixteen acceptance criteria plus the coverage/build/lint/typecheck
commands against this tip and found every one **MET** or exit 0: verdict `PASS`,
`failureCategory: none`, `scopeOk: true`, no regressions found. Full evidence:
[`tasks/M2-C04-01.md` § Execution Record (2026-08-20)](tasks/M2-C04-01.md#execution-record-2026-08-20).
**Attempts used: 1 of 3, 0 escalations** (per the runner's own accounting for this session; the
lost attempt-2 dispatch above did not consume budget, consistent with the `M0-12-01` precedent).

**Why `Needs Review`, not `Completed`.** Per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed), only the repository owner
may set a task `Completed`, and one manual step is explicitly owed at review and cannot be
automated: a pass in both themes at 200 % zoom and with `prefers-reduced-motion` enabled —
`jsdom` computes no layout and applies no stylesheet, so the reduced-motion and focus-ring
commitments are verified at the stylesheet-text level only. Everything else the task's
acceptance criteria ask for is independently confirmed.

**Releases, once reviewed and merged:** `M2-C04-02` and `M2-C04-03` (both list this as a Hard
prerequisite), and — together with `M2-C02` — `M2-C03`. None of the three move to `Ready` on
`Needs Review` alone, per the *Ready-task selection rule*'s "not `REVIEW`" clause; they stay
`Blocked` until this is merged.
