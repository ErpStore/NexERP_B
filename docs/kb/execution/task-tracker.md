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
| M0-01-03 | M0 | — deployment script + rebuild runbook | Database | **Needs Review**¹ | P0 | M0-01-02 | 1 d | G0 |
| M0-02 | M0 | Confirm stored-procedure drift across tenants (Q-14) | Investigation | **Completed**⁶ | P1 | M0-01-02 | 1 d | G0 |
| M0-12 | M0 | Test project + calculation tests *(parent)* | Testing | Not Started | P0 | M0-07 | 3 d | G0 |
| M0-12-01 | M0 | — create the test project and wire it into CI | Testing | **Blocked**¹² | P0 | M0-07 | 0.5 d | G0 |
| M0-12-02 | M0 | — characterisation tests for `CalculationService` | Testing | Blocked | P0 | M0-12-01 | 2.5 d | G0 |
| M0-13 | M0 | Characterisation tests for `StockManagerService` | Testing | Blocked | P0 | M0-12-01 | 3 d | G0 |
| M0-09 | M0 | Fix the two unreachable delete guards (R-08) | Backend | Blocked | P1 | M0-12-01 | 0.5 d | G0 |
| M0-10 | M0 | Audit all `CanDelete…Async` guards (INV-025) | Investigation | Blocked | P1 | M0-09 | 2 d | G0 |
| M0-06 | M0 | Remove the seeded default Administrator credential | Security | Blocked | P1 | M0-12-01 | 1 d | G0 |
| M0-14 | M0 | Gate `DetailedErrors` on `IsDevelopment()` | Security | **Completed**¹⁰ | P2 | M0-03-01 | 0.5 d | G0 |
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
| M0 | 24 | 12 | G0 | ⬜ Not met |
| M1 | 6 | 5 (+1 rolling) | G1 | ✅ Passed 2026-08-12 |
| M2 | 52 | 0 | G2 | ⬜ Not met |
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

**Currently `Ready`:** none, as of the M0-02 deferral merge (2026-08-18). Every M0 task is now
`Completed`, `Blocked` on a named human, or `Needs Review` and therefore not re-selectable:
M0-02 is `Needs Review`⁶ (Q-14 explicitly deferred by Vivek, its named owner); M0-03 is a
`Completed` parent container, never worked directly; M0-03-01/02/03 and M0-14 are `Completed`;
M0-01-03 is `Needs Review`¹, awaiting a human-executed rebuild drill; M0-04 is `Blocked`⁴ on an
unidentified credential owner; M0-07 is `Blocked`⁷ on `origin` push plus GitHub org admin
rights; M0-05 stays `Blocked` because M0-04 has not run; everything downstream of
M0-07/M0-12-01 stays `Blocked` transitively. No task satisfies the *Ready-task selection rule*
— **the runner cannot open anything until a human clears M0-04, M0-07 or M0-01-03's drill.**
**Active task:** none — see [`current-task.md`](current-task.md), which now records this as
the hand-off state rather than pointing at an in-progress task. Selection rule for what
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

⁴ **M0-04: `Blocked` on a human, not on a task.** The 2026-08-17 run opened it and stopped at
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
Independently validated **PASS** on attempt 1 of 4, 0 escalations, `scopeOk: true`,
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

**Owner to unblock:** whoever administers the autonomous runner / agent-dispatch
infrastructure for this project. No such person is named anywhere in the repository; in their
absence, the repository owner (**Vivek**) is the fallback contact, consistent with every other
`Blocked`-on-a-human row in this table that lacks a more specific named owner (compare `M0-04`
footnote 4). This is a **runner-health** question, not a task-specification question — no
change to `tasks/M0-12-01.md` is indicated by anything found this session.
