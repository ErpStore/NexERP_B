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
last_verified: 2026-08-21
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
| M0-01-03 | M0 | — deployment script + rebuild runbook | Database | **Needs Review**¹ ²¹ ³⁰ *(drill §§2–6 executed and passing; §7 and a named operator still outstanding; on `migration/M0-01-03-rebuild-drill` `34b5e32`, unmerged)* | P0 | M0-01-02 | 1 d | G0 |
| M0-02 | M0 | Confirm stored-procedure drift across tenants (Q-14) | Investigation | **Completed**⁶ | P1 | M0-01-02 | 1 d | G0 |
| M0-12 | M0 | Test project + calculation tests *(parent)* | Testing | Not Started | P0 | M0-07 | 3 d | G0 |
| M0-12-01 | M0 | — create the test project and wire it into CI | Testing | **Completed**¹² | P0 | M0-07 | 0.5 d | G0 |
| M0-12-02 | M0 | — characterisation tests for `CalculationService` | Testing | **Completed**¹⁴ | P0 | M0-12-01 | 2.5 d | G0 |
| M0-13 | M0 | Characterisation tests for `StockManagerService` | Testing | **Completed**¹³ | P0 | M0-12-01 | 3 d | G0 |
| M0-09 | M0 | Fix the two unreachable delete guards (R-08) | Backend | **Completed**¹⁵ | P1 | M0-12-01 | 0.5 d | G0 |
| M0-10 | M0 | Audit all `CanDelete…Async` guards (INV-025) | Investigation | **Needs Review**²⁹ *(close-out claims `PASS` after attempt 3; on `migration/M0-10-candelete-guard-audit` `fc8e0c0`, unmerged; that branch's own row still reads `Ready`)* | P1 | M0-09 | 2 d | G0 |
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
| M2-A01-02 | M2 | — implement `[RequireScreen]` / `[RequireRight]` | Security | **Completed**²⁵ | P0 | M2-A01-01 | 3 d | G2 |
| M2-A01-03 | M2 | — per-request rights resolution + caching | Security | **Completed**²⁷ | P0 | M2-A01-02 | 2 d | G2 |
| M2-A02 | M2 | Apply to `CurrencyController` + denial tests | Security | **Ready** (gated on **Q-28** — see footnote ²⁷) | P0 | M2-A01-03 | 1 d | G2 |
| M2-A03 | M2 | Permission-matrix test harness (CI gate) | Testing | Blocked | P0 | M2-A02 | 3 d | G2 |
| M2-A04 | M2 | Refresh tokens + revocation | Security | Blocked | P0 | M2-A01-02 | 3–5 d | G2 |
| M2-A05 | M2 | Cross-origin SPA tenant resolution + real CORS | Security | Blocked | P0 | M2-A04 | 3–5 d | G2 |
| M2-A06 | M2 | Exception middleware → `ProblemDetails` | Backend | **Completed**²³ | P0 | G0 | 3–5 d | G2 |
| M2-A07 | M2 | `GET /api/v1/me` | Backend | **Needs Review**³⁷ *(validated `PASS`; on `migration/M2-A07-me-endpoint` `61da4bd`, unmerged)* | P0 | M2-A01-03 | 2 d | G2 |
| M2-A08 | M2 | Row-level scoping + account gates (Q-05…Q-08) | Security | **Needs Review**²⁹ *(validated `PASS`; on `migration/M2-A08-row-scope-and-account-gates` `bca92fd`, unmerged; ⚠ a second branch `migration/M2-A08-row-level-scoping` also claims this task)* | P0 | M2-A01-03 | 3 d | G2 |

### M2-B — API structure

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M2-B07 | M2 | Shared `AddVSmartDomain()` DI extension | Backend | **Completed**²⁰ | P0 | G0 | 3 d | G2 |
| M2-B04 | M2 | Decouple `IApprovalService` + 13 `Pages` refs | Backend | **Needs Review**²⁸ *(validated `PASS` on attempt 2; on `migration/M2-B04-decouple-pages-references` `5ca1c10`, unmerged)* | P0 | M2-B07 | 1 wk | G2 |
| M2-B01 | M2 | API versioning → `/api/v1` | Backend | **Completed**³³ | P1 | M2-B07 | 1 d | G2 |
| M2-B02 | M2 | Paging / sort / filter contract | Backend | **Completed**²⁴ | P0 | M2-A06 | 1 wk | G2 |
| M2-B03 | M2 | Codify the controller template | Documentation | Blocked | P0 | M2-A02, M2-B02 | 2 d | G2 |
| M2-B05 | M2 | Typed `ScreenCodes` constants (R-10) | Backend | **Blocked**³¹ *(⛔ premise falsified — needs re-specification by the owner; no code written, no branch)* | P1 | M2-B07 | 2 d | G2 |
| M2-B06 | M2 | File upload / download endpoints | Backend | **Completed**³² ³⁵ *(merged to `master` 2026-08-21, `65d9666`)* | P1 | M2-A06, M2-B01 | 1 wk | G2 |
| M2-B08 | M2 | Report + print endpoints (ADR-005) | Backend | Blocked | P1 | **M2-B07**, M2-A01-03, G0 | 1 wk | G2 |
| M2-B09 | M2 | Reference-data endpoints + caching | Backend | **Needs Review**³⁴ *(implemented; on `migration/M2-B09-reference-endpoints` `d1175db`, unmerged)* | P1 | **M2-B07**, M2-B02, M2-B01 | 3 d | G2 |
| M2-B10 | M2 | OpenAPI + TypeScript client generation in CI | DevOps | Blocked | P0 | M2-B03 | 3 d | G2 |
| M2-B11 | M2 | Health checks + structured logging (R-23) | DevOps | **Needs Review**³⁶ *(validated `PASS` on attempt 2 of 4; on `migration/M2-B11-health-checks-logging` `12dad11`, unmerged)* | P2 | M2-A06 | 3 d | G2 |
| M2-B12 | M2 | Document numbering hardening *(parent)* | Backend | Not Started *(parent — never worked directly)* | P0 | M2-B07 | 1 wk | G2 |
| M2-B12-01 | M2 | — INV-012 numbering investigation | Investigation | **Blocked**²⁹ *(escalation budget exhausted, owner **Vivek**; on `migration/M2-B12-01-inv-012-numbering` `407d0ba`, unmerged — the earlier `PASS` was premature)* | P0 | M2-B07 | 2 d | G2 |
| M2-B12-02 | M2 | — verify unique constraints in a live DB (Q-10) | Database | Blocked | P0 | M2-B12-01 | 1 d | G2 |
| M2-B12-03 | M2 | — race-safe allocation + idempotency (R-12) | Backend | Blocked | P0 | M2-B12-02 | 3 d | G2 |

### M2-C — React foundation

| Task ID | Milestone | Task | Type | Status | Priority | Depends On | Estimate | Gate |
|---|---|---|---|---|---|---|---|---|
| M2-C00 | M2 | Rewrite KB-050 frontend architecture for Angular | Documentation | **Needs Review**²⁶˒³⁸ *(validated `PASS`; on `migration/M2-C00-kb050-angular-rewrite` `b3c0e6e`, unmerged)* | P0 | G0 | 2 d | G2 |
| M2-C01 | M2 | Angular CLI + TS strict + lint + test + CI | Frontend | **Blocked**²⁶ *(re-scoped for Angular; `M2-C00` is `Needs Review`, not `Completed`)* | P0 | M2-C00 | 3 d | G2 |
| M2-C11 | M2 | **Adopt** the Angular pilot as the app baseline | DevOps | Blocked²⁶ | P2 | M2-C00 | 0.5 d | G2 |
| M2-C10 | M2 | Decimal handling — no float money arithmetic | Frontend | Blocked²⁶ | P0 | M2-C01 | 2 d | G2 |
| M2-C02 | M2 | Auth: login, refresh, guards, permission store | Frontend | Blocked | P0 | M2-C01, M2-A04, M2-A07 | 1 wk | G2 |
| M2-C04 | M2 | Design-system primitives *(parent)* | Frontend | Not Started *(parent — never worked directly)* | P0 | M2-C01 | 2 wks | G2 |
| M2-C04-01 | M2 | — tokens, theme, light/dark | Frontend | Blocked²⁶ *(re-scoped; React implementation superseded)* | P0 | M2-C01 | 3 d | G2 |
| M2-C04-02 | M2 | — form controls + validation display | Frontend | Blocked²⁶ | P0 | M2-C04-01 | 4 d | G2 |
| M2-C04-03 | M2 | — modal, drawer, toast, states | Frontend | Blocked²⁶ | P0 | M2-C04-01 | 3 d | G2 |
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
| M2-D01 | M2 | Currency end-to-end in Angular | Frontend | Blocked | P0 | M2-C05-03, M2-A02, M2-B10 | 3 d | G2 |
| M2-D02 | M2 | Customer Master *(parent)* | Migration | Blocked | P0 | M2-D01 | 1.5 wks | G2 |
| M2-D02-01 | M2 | — `@code` triage + logic extraction | Backend | Blocked | P0 | M2-D01 | 4 d | G2 |
| M2-D02-02 | M2 | — `CustomersController` + API tests | Backend | Blocked | P0 | M2-D02-01 | 3 d | G2 |
| M2-D02-03 | M2 | — Angular screens + component tests | Frontend | Blocked | P0 | M2-D02-02 | 4 d | G2 |
| M2-D03 | M2 | Blazor ↔ Angular parity test | Testing | Blocked | P0 | M2-D02-03 | 3 d | G2 |

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
| M2 | 53 | **6** *(`M2-A01-01`¹⁸ — under a deliberate G0 gate exception, `M2-A01-02`²⁵, `M2-A01-03`²⁷, `M2-A06`²³, `M2-B07`²⁰, `M2-B02`²⁴ — **all six are backend**. Was 7 until 2026-08-20: `M2-C01`¹⁹ and `M2-C04-01`²² were `Completed` in React and are **superseded** by [ADR-007](../decisions/ADR-007-angular-stack.md), footnote ²⁶. Total rises 52 → 53 with the new `M2-C00`. Recount is by `grep` over the M2 rows, never by adjusting the previous number)* | G2 | ⬜ Not met |
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

> ### ✅ The waived half is now SATISFIED — 2026-08-20, on `master`
>
> `master` was pushed `20be92f..e63716e` (41 commits) and the workflow ran **green**, owner-confirmed. That was the **first hosted execution** of the `frontend` job: 150 Vitest tests *and* the `branches: 100` coverage gate, on a runner rather than a workstation.
>
> **Read what changed, precisely.** The criterion said *"green on the **branch**"*; what is now proven is *"green on **`master`**, after merge"*. That is the same guarantee arriving later, not the original criterion being met — an execution session still cannot produce a hosted run, so the waiver was correct when it was taken. It is retired by evidence, not withdrawn as a mistake.
>
> Retired alongside it: `M2-A06`'s *"not verified, and not verifiable from an execution session — that the new CI step is green on a hosted runner."* `Test - V.SMART.Api.Tests` ran green on the same push, its first execution anywhere but this workstation.
>
> **Still open, and unaffected:** **Q-20**'s remaining half. CI running green is not CI *gating* merges — there is still no required status check, and this very push reported `Bypassed rule violations … Changes must be made through a pull request`. Green CI that nothing enforces is a smoke alarm with the battery out.

> **Read the waiver as a waiver.** Criterion 10's second half — *"the `frontend` job ... is green on the branch"* — was **never satisfied at the time**. It needs a push, which an execution session may not make. Waived by the owner on the `M0-07` (`d79e1a4`) and `M0-12-02` (`a83f1e2`) precedent. Everything locally verifiable was independently re-run before merging: `typecheck`, `lint --max-warnings=0`, `test`, `build` all exit 0; entry chunk **289.69 kB / 90.90 kB gzipped**; `git status` clean after a build; `dotnet test` still **79 passed**. Scope confirmed: no `node_modules/`, `dist/`, `playwright-report/` or `test-results/` committed; `package-lock.json` committed; `frontend/vsmart-erp/` untouched; exactly one component library (`@mantine/core`); the pre-existing CI job intact.

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

²² **M2-C04-01: `Completed` and merged (`56b8ae2`, 2026-08-20) — all sixteen acceptance criteria `MET`, no waiver.**

> **The first M2 task to close with nothing waived.** `M2-C01` and `M2-B07` each carried an unmet criterion an execution session structurally could not reach. This one did not.
>
> **Re-verified independently before merging, and again on `master` after:** `npm run coverage` **150 passed, branches 100 %**, exit 0; `typecheck`, `lint`, `build` all exit 0; entry bundle **91.59 kB gzip** against KB-050's `< 250 kB` budget; `dotnet test` **84 passed** — unchanged, and it could not have moved, since no file under `V.SMART/` or `tests/` is in the diff.
>
> **The coverage regression was fixed by raising coverage, not by lowering the floor.** Attempt 1 added ~700 partly-covered lines under a `branches: 100` threshold and broke `npm run coverage` (exit 0 → 1). `vitest.config.ts` is **byte-identical to `master`** — verified, not assumed — and its own comment reads *"Thresholds are set to the MEASURED starting value (M2-C01), so the number can only ever be raised. Do not lower these."* Part of the fix **deleted** branches rather than testing them: `ThemeToggle`'s arrow-key ring indexed a tuple with a computed number, which under `noUncheckedIndexedAccess` forced two guards the type system already makes unreachable. Unreachable branches cannot be covered, only ignored.
>
> **Eight KB-051 colours were raised, not shipped as specified.** They failed WCAG at the values KB-051 gave: `--border` light **1.18:1**, `--text-disabled` light **2.26:1**, `--success`/`--warning` **4.38:1**, and a `--focus-ring` specified as a *40 % wash* that cannot reach 3:1 against any light background (WCAG 2.2 §2.4.11 requires it to). **The thresholds were never lowered to fit the palette.** The validator re-derived this from scratch with its own WCAG implementation — 110 pairs, 0 failing, both themes.
>
> **The 12 px workhorse type scale is an owner decision, not a default.** `--text-sm: 12px` (table body, form inputs), `--text-base: 14px`, 30 px compact rows — confirmed by Vivek 2026-08-20, weighed explicitly against the "more user-friendly than the reference ERPs" goal and kept. **The reasoning, so it is not re-litigated:** density *is* the usability feature in a data-heavy ERP; rows-per-screen is what an operator entering line items all day actually feels, and larger type costs visible rows on the highest-frequency task in the system. `M2-C04-02`, `M2-C05-01` and `M2-C07` inherit this scale and should not reopen it.
>
> **Still owed at review, and genuinely not automatable:** both themes at **200 % zoom with `prefers-reduced-motion` enabled**. `jsdom` applies no stylesheet, so no test can cover it. This is a review step, **not an unmet acceptance criterion** — recorded here so it is not mistaken for one, and not silently dropped either.

Pre-merge record follows. Branch `migration/M2-C04-01-design-tokens`, tip `9f886a6` at validation.

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

---

> ### ⚠ Gap found at owner review, 2026-08-20 — the 21 new API tests never run in CI
>
> `tests/V.SMART.Api.Tests/` is **not in `NexGen-ERP---2025-master.sln`** and **not invoked by
> `.github/workflows/ci.yml`**, which runs exactly one test project:
> `dotnet test tests/V.SMART.Shared.Tests/...`. Verified, not inferred — the solution lists five
> projects and this is not among them; the CI file's own "WHAT IS DELIBERATELY NOT HERE" comment
> still reads *"Any test project other than `tests/V.SMART.Shared.Tests`."*
>
> **All 21 tests pass locally** (re-run at owner review: `Failed: 0, Passed: 21`). Nothing is
> broken. The gap is that **nothing on a hosted runner would notice if they broke** — they can rot
> silently, and the error contract they pin is the one every future controller inherits.
>
> **This was not recorded anywhere** — not in the task file, not in the execution record, not in
> the close-out. The acceptance criteria did not ask for `.sln`/CI wiring, so the `PASS` is
> correct and the eighteen criteria genuinely are `MET`; this is a gap in *what the task asked
> for*, not a validation failure. `M0-12-01` set the precedent that a new test project gets wired
> into CI in the same change that creates it.
>
> **✅ FIXED BEFORE MERGE**, on owner instruction (*"wire it into CI then merge"*), commit
> `a499989`. `tests/V.SMART.Api.Tests/` is now in the solution (`dotnet sln list` → six projects)
> and has its own `Test - V.SMART.Api.Tests` step in `ci.yml`'s `build` job — a **separate** step,
> so the job log names which suite failed without anyone opening a `.trx`. The
> "WHAT IS DELIBERATELY NOT HERE" comment was rewritten: it asserted no test project other than
> Shared exists, which the merge made false. It now states the standing rule — **every new test
> project gets its own step in the change that creates it**, because a suite CI does not run is a
> suite that rots without anyone noticing.
>
> `ci.yml` was parsed with a real YAML parser, not eyeballed: `build` / `frontend` /
> `frontend-e2e` jobs intact, both `Test` steps present and in order. A hand-indented step that
> fails to parse would break the entire workflow.
>
> **✅ Now verified, 2026-08-20.** `master` pushed `20be92f..e63716e` and the workflow ran **green**, owner-confirmed. `Test - V.SMART.Api.Tests` executed on a hosted runner for the first time and passed. The step is real, not merely well-formed.

²³ **M2-A06: `Completed` and merged (`76eca5d`, 2026-08-20) — all eighteen acceptance criteria `MET`, no waiver.**

> **Re-verified before merging and again on `master` after:** `dotnet build V.SMART.Api` **0 errors / 6,694 warnings** (baseline 6,695); `dotnet test V.SMART.Api.Tests` **21 passed**; `dotnet test V.SMART.Shared.Tests` **84 passed**, no regression.
>
> **BR-SO-001 was honoured** — the one thing here that could have destroyed behaviour while reading as an improvement in a diff. `ApiProblems.BusinessRuleRefusal` carries the service's own message into `title` **verbatim**: *"not reworded, not prefixed, not truncated. Those strings are product UX written by the domain team."*
>
> **The validator probed the running host rather than trusting the build.** It started the API with a throwaway JWT secret and an unreachable `MasterDb` and confirmed over HTTP: `application/problem+json` with matching `X-Correlation-Id`/`traceId` on 401, 404, 503 and a CORS preflight 204; **no connection string** in the unresolved-tenant body (R-01); a caller-supplied correlation header ignored. The implementer had explicitly reported *not* doing this — the validator did not take that as settled.
>
> **A deliberate breaking change ships here:** `DELETE /api/currencies/{id}`'s refusal moves **`400` → `409`** per ADR-002 §4. Intended, and the only contract change in the diff.
>
> **Two defects flagged forward rather than quietly fixed:** `ExceptionHandlingMiddleware`'s `Response.Clear()` discards CORS headers on error responses (→ **`M2-A05`**, R-24), and `/swagger/index.html` returns no correlation header (Development-only, no API endpoint affected).
>
> **INV-040 `Complete`:** business-rule refusals are signalled by **tuple return, not exception** — 79 delete-guard methods across 61 service files. The binding convention for every later controller is a controller helper (`ProblemResults.BusinessRuleProblem`), **not** a domain exception.

Pre-merge record follows. Branch `migration/M2-A06-problem-details`, tip `f69891a` at validation.

**Original close-out text:** M2-A06: `Needs Review` — implemented and independently validated `PASS` on
`migration/M2-A06-problem-details` (`f69891a`), 2026-08-20. Not merged; per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) only the repository owner
may set it `Completed`.**

All eighteen acceptance criteria independently re-checked `MET` (two — updating `M2-A02`'s
tests, and the `M2-A03` permission harness — correctly marked *not applicable* / *not
checkable*, since neither prerequisite has landed). `dotnet build V.SMART.Api --no-incremental`:
**0 errors, 6,694 warnings** (baseline 6,695). `dotnet test tests/V.SMART.Api.Tests/…`: **21
passed, 0 failed** (new project, created by this task). `dotnet test
tests/V.SMART.Shared.Tests/…`: **84 passed, 0 failed** — no regression. `git diff --stat
HEAD~1 HEAD -- V.SMART/V.SMART.Shared V.SMART/V.SMART.Web V.SMART/V.SMART/`: empty — protected
trees untouched. Full record: [`tasks/M2-A06.md` § Execution Record
(2026-08-20)](tasks/M2-A06.md#execution-record-2026-08-20).

**Business-rule refusal signalling is now decided and recorded (INV-040, `Complete`):** a
controller helper (`ProblemResults.BusinessRuleProblem`), not a domain exception, binding on
every one of the 60–80 controllers still to come. Two new open questions were raised and
recorded, not guessed at: **Q-34** (a refusal tuple sometimes carries 404/500 semantics that
`409` cannot distinguish — not determinable from source) and **Q-35** (the `503`-for-
unresolved-tenant and ignore-caller-correlation-header design choices had no prior KB
position).

**Two gaps found only during this close-out's independent validation, not reported by the
implementer, now recorded in the repository rather than left to be rediscovered:** (1)
`GET /swagger/index.html` (Development only) returns `200` with no `X-Correlation-Id`, because
`UseSwagger`/`UseSwaggerUI` sit ahead of `UseErrorContract()` in `Program.cs` — no API endpoint
is affected (`docs/kb/api/api-overview.md`); (2) `ExceptionHandlingMiddleware`'s
`context.Response.Clear()` discards CORS headers on an error response, an inherent consequence
of the task's own required "before `UseCors`" ordering — flagged forward to **M2-A05**
(`docs/kb/risks/technical-debt-register.md` R-24).

**Releases, once reviewed and merged:** `M2-B02` (→ `M2-B03` → `M2-B10`), `M2-B06`, `M2-B11` —
all list `M2-A06` as a Hard prerequisite. None moves to `Ready` on `Needs Review` alone.

²⁴ **M2-B02: `Completed` and merged (`feec964`, 2026-08-20) — all eighteen acceptance criteria `MET`, no waiver.**

> **Third consecutive M2 task to close with nothing waived.**
>
> **Re-verified before merging and again on `master` after:** `dotnet build V.SMART.Web` **0 errors** — built *deliberately*, because this task touches `V.SMART.Shared`, the **live Blazor app's business layer**, not just the API; `dotnet build V.SMART.Api` **0 errors**; `dotnet test V.SMART.Api.Tests` **56 passed** (21 → +35); `dotnet test V.SMART.Shared.Tests` **84 passed**, no regression.
>
> **The shared-layer change is genuinely additive — checked, not taken on assertion.** `ICurrencyService` gains a 4-arg overload; the 3-arg member keeps its signature and delegates with `sort: null`. An empty term list returns `query.OrderByDescending(x => x.CurrId)` — exactly the previous ordering path — and **`CurrencyList.razor` is not in the diff**, so the live Blazor caller still binds to the 3-arg overload by named arguments. The running UI is untouched.
>
> **Two pieces of engineering worth keeping visible.** `ApplyOrder` appends `ThenByDescending(CurrId)` whenever the sort key is not unique — paging over a non-unique key lets SQL Server break ties differently per query, so rows silently repeat or vanish between pages; that is a correctness bug most paging implementations ship with. And **an unknown sort field throws**, deliberately opposite to `CurrencyFilterBuilder`'s silent catch-all, with the reason recorded: *"a request that silently sorts nothing while answering 200 is worse than one that fails."* The allow-list is an explicit `switch` over string literals, never reflection, so the sortable set is a reviewable compile-time fact.
>
> **The one stated limitation was verified, not accepted.** The record says the `toDate` 23:59 boundary could not be exercised against real SQL because every dev-tenant `Currency` row has a null `CreatedDate`. Checked directly against the local `SQLEXPRESS` tenant (read-only, integrated auth): `SELECT COUNT(*), COUNT(CreatedDate) FROM Currency` → **`3, 0`**. Three rows, none with a `CreatedDate`. **The limitation is real and was reported honestly** — the boundary was tested one level below HTTP through the untouched `CurrencyFilterBuilder` predicate instead.
>
> **A convention that outlives this task**, [ADR-002 §2a](../decisions/ADR-002-rest-api-layer.md): `[FromQuery]` on a record binds by CLR property name and Swashbuckle emits it verbatim, so every bound property needs an explicit `[FromQuery(Name = "camelCase")]` or the OpenAPI document — and the TypeScript client `M2-B10` generates from it — **silently drifts to PascalCase**. Binding on `M2-B03`'s controller template.
>
> **A pre-existing defect found in passing, recorded not fixed:** **Q-36** — `CurrencyList.razor:758-760` sets a `Status` filter key `CurrencyFilterBuilder` has no case for, so **that dropdown has been filtering nothing**. Out of scope here.
>
> **Releases only `M2-B09`.** `M2-B03` still needs `M2-A02` (`Blocked`); `M2-C05`/`M2-C05-01` still need `M2-C04-02`, which is `Ready` but not done.

Pre-merge record follows. Branch `migration/M2-B02-paging-contract`, tip `c603115` at validation.

**Original close-out text:** M2-B02: `Needs Review` — implemented and independently validated `PASS` on
`migration/M2-B02-paging-contract` (`c603115`), 2026-08-20. Not merged; per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) only the repository owner
may set it `Completed`.**

All eighteen acceptance criteria independently re-checked `MET` (the `toDate` 23:59-boundary
criterion `MET` with a stated limit — proven through the real, untouched `CurrencyFilterBuilder`
predicate one level below HTTP, not against a live SQL Server round trip, because every
`Currency` row in the reachable dev tenant has a null `CreatedDate`). `dotnet build
V.SMART.Api --no-incremental`: **0 errors, 6,695 warnings** (KB-083 baseline, no new warnings).
`dotnet test tests/V.SMART.Api.Tests/…`: **56 passed, 0 failed**. `dotnet test
tests/V.SMART.Shared.Tests/…`: **84 passed, 0 failed** — no regression. `dotnet build
V.SMART.Web`: **0 errors** — Blazor host intact, `CurrencyList.razor:344-348`'s three-argument
call still binds unchanged. `CurrencyFilterBuilder` diffed byte-identical against its pre-change
location (`CurrencyService.cs:157-186` → `:180-209`). A live pre/post comparison of
`GET api/currencies?pageNumber=1&pageSize=10` against the same tenant database returned a
**byte-identical** body (`md5` equal, `cmp` silent).

**One retry inside the same attempt, not a fresh dispatch.** The first validation pass found the
implementation sound but `FAIL`ed it on two defects: the OpenAPI document had regressed from
camelCase to PascalCase query-parameter names (contradicting `ADR-002` §2a and `api-overview.md`,
which this task itself must keep in sync — `M2-B10` generates its TypeScript client from this
document), and the `toDate` 23:59 criterion was unverified rather than failing. Both were fixed
on the same branch: explicit `[FromQuery(Name = …)]` wire names sourced from `const` fields on
`PagedQuery`/`CurrencyQuery`, and two new boundary tests in `PagedContractTests.cs`. Full record:
`docs/kb/execution/failure-log.md` § "M2-B02" and [`tasks/M2-B02.md` § Execution Record
(2026-08-20)](tasks/M2-B02.md#execution-record-2026-08-20).

**INV-041 (`Complete`):** no service in `V.SMART.Shared/BusinessLayer/` takes a `sort`
parameter; the chosen mechanism is an additive 4-argument `SearchWithDynamicFilterAsync`
overload, not a reserved filter-dictionary key (rejected — every `*FilterBuilder` silently
ignores an unrecognised key, `_ => query`) and not controller-side sorting (rejected — `Skip`/
`Take` run first). **Q-36 raised, not guessed at:** `CurrencyList.razor:758-760` sets a `Status`
filter key `CurrencyFilterBuilder` has no case for, so that dropdown already filters nothing in
production — the in-the-wild evidence for the filter-dictionary rejection above, not this
task's to fix.

**Behaviour changes, both prescribed by the task's own contract, not accidental:** default
`pageSize` moves `10 → 20` (`ADR-002` §2a, `KB-040`); an unparseable `fromDate`/`toDate` now
returns `400` instead of being silently discarded by the old `string?` re-parse.

**Releases, once reviewed and merged:** `M2-B03` (→ `M2-B10`), `M2-B09`, `M2-C05`, `M2-C05-01` —
all list `M2-B02` as a Hard prerequisite. None moves to `Ready` on `Needs Review` alone.

²⁵ **M2-A01-02: `Completed` and merged (`ed559ad`, 2026-08-20) — all acceptance criteria `MET`, no waiver.**

> **Fourth consecutive M2 task to close with nothing waived.** Re-verified before merging and again on `master` after: `dotnet build V.SMART.Api` **0 errors**; `dotnet test V.SMART.Api.Tests` **104 passed** (56 → +48); `dotnet test V.SMART.Shared.Tests` **84 passed**. Scope: `V.SMART.Api` only — no `Shared`, no Web, no MAUI, no `bin/`/`obj/`, **and no controller annotated**, which the task forbids.
>
> ### The D-5 / R-40 contradiction was not resolved by guessing
>
> This was the whole risk of the task, so it was checked directly rather than taken from the report — a plausible-looking compromise here would have baked an **undeclared superuser into the new API's security model**, and it would have read as reasonable in a diff.
>
> - `grep` of `V.SMART.Api/Authorization/` for `UserId == 1`, `IsAdmin`, `Administrator`, `superuser`, `bypass`, `.Role` → **zero matches**.
> - **KB-105's D-5 still reads *"No `Administrator` bypass. None. Anywhere."* verbatim.** The spec *was* touched, but **additively** — an implementation-status block recording two deliberate departures, which also corrects its own stale `Program.cs` line numbers. D-5 was **not** softened to fit the code.
> - `T13_an_Administrator_with_no_row_is_denied` pins it: an identity carrying a `Role=Administrator` claim against an empty rights set is denied.
>
> **Why it did not fire — which matters more than that it didn't.** R-40's bypass lives in `Login.razor`'s **login** path, not in `RightsHelper` or the rights check. The filter reads `UserRight` rows and nothing else, so an administrator with no rows is denied, correctly. **The contradiction was never this task's to hit.** It stays live for **`M2-A02`**, and sharper there: an API-only administrator holds **zero rows**, because `AuthController.Login` never calls `SyncRightsForUserAsync` (**Q-28**). Implement `M2-A02` before settling Q-28 and the administrator authenticates into an empty UI.
>
> ### One security-relevant departure — recorded, not hidden
>
> **D-4 is only partly implemented.** An authenticated action on a controller carrying **no** `[RequireScreen]` at all is presently **allowed through** rather than refused, at request time *and* at startup. The reasoning is sound — enforcing it now would make the host refuse to start over `CurrencyController`'s five unannotated endpoints, and this task requires all six to behave exactly as before — and the half-annotated directions (T-11, T-12) **are** enforced, as is D-6's catalogue check.
>
> **But until `M2-A02` closes it, the filter is opt-in, not deny-by-default at the controller level** — the opposite of what "deny by default" implies. Tracked against **R-03** ([KB-060](../risks/technical-debt-register.md)). This belongs in front of whoever writes `M2-A02`, not in a footnote nobody re-reads.
>
> **Also latent and deployment-conditional:** the globally registered filter constructs `IUserRightsProvider` — and therefore the tenant `DbContext` — via DI on **every** request reaching MVC, including unannotated actions.
>
> **Releases `M2-A01-03` only** (per-request rights caching). `M2-A02` and everything behind it wait on that.

Pre-merge record follows. Branch `migration/M2-A01-02-require-screen-right`, tip `9a6b3c2` at validation.

**Original close-out text:** M2-A01-02: `Needs Review` — implemented and independently validated `PASS` on
`migration/M2-A01-02-require-screen-right` (`9a6b3c2`), 2026-08-20, attempt 1 of 3, 0
escalations. Not merged; per [KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)
only the repository owner may set it `Completed`.**

All twenty-two acceptance criteria independently re-checked `MET` (including the six extra
types `NoScreenRightAttribute`, `ScreenRightSet`, `ScreenCatalogue` and
`ScreenRightStartupValidator` that KB-105 §2 mandates beyond the task file's own stale
six-file list — the task file itself defers to "the exact names, namespaces and signatures
fixed by `M2-A01-01`"). `dotnet build V.SMART.Api --no-incremental`: **0 errors, 6,694
warnings** (KB-083 baseline, no new warnings). `dotnet test tests/V.SMART.Api.Tests`: **104
passed, 0 failed**. `dotnet test tests/V.SMART.Shared.Tests`: **84 passed, 0 failed** — no
regression. `dotnet build V.SMART.Web`: **0 errors** — Blazor host intact,
`RightsHelper.cs`/`BaseUserRightsComponent.cs` untouched. `git diff --stat master...HEAD`: 17
files, 1445 insertions(+), 2 deletions(-); nothing under `V.SMART.Shared/`, `V.SMART.Web/`,
`V.SMART/`, `Controllers/`, `Auth/`, `Migrations/`.

**Live host verification, not merely inferred from tests.** Started the API against local
`SQLEXPRESS` (`NexGenErpDb_Master`, tenant `Id 1`/`Hostname localhost`): host starts (the new
`ScreenRightStartupValidator` accepts the real controller set); unauthenticated `GET
/api/currencies` → `401` `application/problem+json`, unchanged; anonymous `POST
/api/auth/login` with bad credentials → `401` with the pre-existing message, unchanged;
authenticated `GET /api/currencies?pageNumber=1&pageSize=2` with a minted JWT → `200` with the
`M2-B02` paged body — the filter is dormant on an unannotated controller, as designed.
`ScreenCatalogue`'s 152 names diffed against the live `Screens` seed
(`ApplicationDbContext.cs:1152-1327`): identical, including the canonical seed typos.

**Three deliberate spec-vs-task-file departures, all traced to KB-105 lines, none guessed:**
(1) ten authorization types created, not the task file's six — KB-105 §2 is more detailed and
the task file defers to it; (2) `IUserRightsProvider.GetAsync(int tenantId, int userId,
CancellationToken ct) : Task<ScreenRightSet>`, not the task file's
`GetRightsAsync(int userId) : Task<IReadOnlyList<UserRight>>` — KB-105 §2.6, `tenantId`
explicit for `M2-A01-03`'s cache key, detached projection to avoid a `DbContext`-lifetime bug;
(3) an unusable `"UserId"`/`"TenantId"` claim denies with `401`, not `403` — KB-105 D-3 is
stricter than the task file's gloss. Full reasoning: [`tasks/M2-A01-02.md` § Execution Record
(2026-08-20)](tasks/M2-A01-02.md#execution-record-2026-08-20).

**A new, not-previously-recorded finding from independent review, latent and
deployment-conditional, not a regression today:** the globally registered filter constructs
`IUserRightsProvider` (→ `IUnitOfWork` → the tenant `DbContext`) via DI on every request that
reaches MVC's pipeline, even on unannotated actions where it never calls `GetAsync`. Verified
live that this cannot break anything today — `UseAuthorization()` middleware rejects a
tokenless caller before MVC builds the filter pipeline (401, not a DI-construction 503), and
`AuthController.Login` already constructs `IUnitOfWork` itself. It becomes relevant once
`M2-A02` annotates a real controller with an unresolvable tenant; a lazy provider injection
would remove it if that surfaces as an actual problem. Recorded so `M2-A02` does not
rediscover it as a mystery.

**Not merged, no controller annotated, so R-03 (`KB-060`) stays open** — mechanism exists,
enforcement does not yet. Closing tasks: `M2-A02`, `M2-A03`. Once reviewed and merged, this
releases exactly one real dependent, `M2-A01-03` (per-request rights caching) — no other task
lists `M2-A01-02` as a Hard prerequisite.

²⁶ **The M2-C tree was re-scoped from React to Angular on 2026-08-20 — [ADR-007](../decisions/ADR-007-angular-stack.md).**

> **Owner decision.** His background is C# and WPF with no frontend experience; the runner writes the screens but **he** reviews and maintains them. Angular's component-plus-service shape, constructor DI and typed Reactive Forms map onto MVVM and XAML in a way React's hooks model does not. The finding that reopened it: **ADR-003 never evaluated Angular at all** — every rationale it recorded was a choice *within* React. Full reasoning in ADR-007.
>
> ### What changed in this tree
>
> | Task | Was | Now |
> |---|---|---|
> | **`M2-C00`** *(new)* | — | **`Ready`** — rewrite [KB-050](../frontend-new/react-architecture.md) for Angular. **Gates the whole tree**: until it lands there is no authoritative structure to specify against |
> | `M2-C01` | `Completed`¹⁹ (React scaffold, merged `12f172f`) | **`Ready`**, re-scoped to Angular CLI. Now depends on `M2-C00` |
> | `M2-C04-01` | `Completed`²² (design tokens, merged `56b8ae2`) | `Blocked`, re-scoped. **`tokens.css` ports nearly verbatim** — plain CSS custom properties, carrying the eight WCAG contrast corrections and the 12 px type-scale decision |
> | `M2-C11` | "**Archive** the Angular pilot" | "**Adopt** the Angular pilot as the app baseline" — it has 9 components including auth service, route guard and HTTP interceptor |
> | `M2-C10`, `M2-C04-02`, `M2-C04-03` | `Ready` | `Blocked` on `M2-C00` |
> | `M2-D01`, `M2-D02-03`, `M2-D03` | "in React" / "React screens" / "Blazor ↔ React parity" | Angular |
>
> ### Footnotes ¹⁹ and ²² are NOT withdrawn
>
> `M2-C01` and `M2-C04-01` **were** implemented, independently verified and merged. `M2-C01`'s frontend CI job ran **green on a hosted runner**. That happened, and the record of it stands unedited. **A superseded task is not a failed one**, and rewriting those footnotes to read "Angular" would falsify the history this repository exists to keep.
>
> ### All 26 `M2-C*`/`M2-D*` task files carry a ⛔ STOP banner — they were not rewritten
>
> That is ~25,000 lines of specification with 1,300+ React-specific references. Rewriting it now would mean **writing Angular detail against a KB-050 that does not exist yet**, for tasks months away, which is how specifications go stale — a risk this repository already names: *"A task file's Current Implementation section is a hypothesis, not fact."*
>
> Each banner states what survives (acceptance-criteria *intent*, the ERP behaviour being reproduced, `file:line` evidence about the **existing Blazor** code, business rules, dependencies — none of which was ever a React decision) and what must be re-derived (every stack-specific instruction, `frontend/nexgen-web/` path, component API and command). **It also tells a runner to stop and report rather than infer**, because re-specifying is an owner-level documentation change.
>
> Tasks get fully re-specified **as they come up**, against a real KB-050 — not in bulk against a guess.
>
> ### Cost, stated plainly
>
> Two merged tasks discarded and ~12 specs to re-derive: **1–2 weeks of real waste.** No backend work is affected — **all five remaining `Completed` M2 tasks are backend**, framework-neutral by design, which is the property that made this switch affordable at all. Deferring it would not have made it cheaper: `M2-C05`/`C07`/`C08` are 6–7 weeks by ADR-003's own estimate and all still ahead.
²⁷ **M2-A01-03: `Completed` and merged (2026-08-20) — all acceptance criteria `MET`, no waiver.** 

> **The cache key is tenant-scoped**, which was the one way this task could have introduced a cross-tenant data leak: `screenrights:v1:{tenantId}:{userId}`, with the hazard named in the code — *"two tenants can both have `UserId = 1` and a tenant-blind key would serve one tenant's rights to another."* Under database-per-tenant (KB-014) that is the whole risk, and it is closed. **Fail-closed on a missing tenant:** the filter resolves `tenantId` via `TryGetPositiveIntClaim` and **denies** when the claim is absent or unusable — it does not default to `0`. **No negative caching:** a throwing rights query writes nothing and propagates to the `M2-A06` handler, so a database fault can never be recorded as "no rights" (KB-105 §7.3). **The TTL cap is enforced in code:** default 60 s, max 300 s, `0` disables and restores exact `M2-A01-02` behaviour — the cap exists because the Blazor and API hosts are separate processes, so a `UserRight` write in Blazor cannot invalidate the API's in-process cache; only TTL expiry catches it. Verified before merging: `dotnet build V.SMART.Api` **0 errors**; `V.SMART.Api.Tests` **117 passed** (104 → +13); `V.SMART.Shared.Tests` **84 passed**.
>
> **Releases `M2-A02`, `M2-A07`, `M2-A08`. `M2-A02` is gated on Q-28** — an API-only administrator holds **zero** `UserRight` rows because `AuthController.Login` never calls `SyncRightsForUserAsync`. Annotating a controller before that is settled produces an administrator who authenticates into an empty UI.
>
> **Footnote renumbered ²⁶ → ²⁷ on merge** (`569c9e6`) — `master` had claimed ²⁶ for the ADR-007 Angular re-scope an hour earlier. Sixth cross-branch id collision.

Pre-merge record follows.

**Original close-out text:** M2-A01-03: `Needs Review`, not `Completed` — implemented and independently validated
`PASS` on `migration/M2-A01-03-rights-cache`, tip `0fde6fb`, 2026-08-20, attempt 2 of 3, 0
escalations.** Not merged; per [KB-088 "Who may set
COMPLETED"](workflow.md#who-may-set-completed) only the repository owner may set it
`Completed`.

All eighteen acceptance criteria independently re-checked `MET`. `dotnet build V.SMART.Api
--no-incremental`: **0 errors, 6,694 warnings** (baseline). `dotnet test
tests/V.SMART.Api.Tests`: **117 passed, 0 failed** (104 → +13, the new
`UserRightsCacheTests.cs`). `dotnet test tests/V.SMART.Shared.Tests`: **84 passed, 0 failed** —
no regression. `git diff --stat master...HEAD`: 13 files, +630/-27; nothing under
`V.SMART.Shared/`, `V.SMART.Web/`, `V.SMART/V.SMART/`, no migration, no secret touched.

**Attempt 1 regressed the test suite and was repaired in attempt 2.** `a78c51e` added the
non-default `Invalidate(int, int)` member to `IUserRightsProvider` without updating the two
test stand-ins in `ScreenRightAuthorizationFilterTests.cs`, so that project stopped compiling
(`CS0535` × 2) and 104 previously-green tests ran zero. `0fde6fb` implemented `Invalidate` on
both stand-ins (no default interface implementation, deliberately, so a future real provider
cannot silently skip eviction) and added the 13 cache tests. Re-validated clean; see
[`tasks/M2-A01-03.md` § Execution Record
(2026-08-20)](tasks/M2-A01-03.md#execution-record-2026-08-20) for the full record.

**One deliberate departure from KB-105 §8.2:** no `SizeLimit` cap on the shared
`IMemoryCache` — recorded as **R-41** ([KB-060](../risks/technical-debt-register.md)), not a
wiring gap.

**Corrected a stale KB-105 sentence found during re-verification**, not introduced by this
task: §7.4 read "three of the five `UserRight` write sites are in the Blazor host",
contradicting its own §8.4 table, F-7 and Q-29, all of which say five. Fixed to "five of
five" with the re-grep evidence attached.

**Does not release `M2-A02`, `M2-A07` or `M2-A08` yet** — their Hard prerequisite must be
genuinely `Completed`, not `Needs Review`
([KB-082](dependency-graph.md#ready-task-selection-rule) step 1). They remain `Blocked` until
this branch is reviewed and merged.

²⁸ **M2-B04: `Needs Review` on `migration/M2-B04-decouple-pages-references` (`5ca1c10`,
unmerged) — attempt 2 validated `PASS` on 2026-08-21 after attempt 1 stopped with no
implementer result.** The retry alone cleared it, as the `environment` category predicted; no
escalation to Vivek was needed and none was raised. Attempt 2 committed the implementation
(`2f61390`) and the independent validation record (`5ca1c10`): all 15 `V.SMART.Shared.Pages`
`using` directives gone from non-UI code (`grep` → **0 hits** outside `/Pages/`), the one
load-bearing case (`FundTransFilterVM.cs` — typed as the Razor component `Bank`, not the EF
entity `Banks`) retyped and verified behaviour-neutral against every `filter.` reference in
`FundTransRepository.cs:46-92`, and a two-fact architecture guard added at
`tests/V.SMART.Shared.Tests/Architecture/NoPagesReferenceFromDomainTests.cs` (reflection +
source scan) which the validator **attacked with two independent seeded violations** rather
than trusting. Builds and suites re-derived by the validator, each against its *matching*
baseline form: `V.SMART.Api` 0 errors / 6694 warnings, `V.SMART.Web` 0 errors / 6697, CI form
6693 with `compare-warnings.sh` → `Gate: PASSED (equal to baseline)`; Shared tests 86 passed
(84 + 2 new), Api tests 117 passed. Attempt 1's `6695` was never an anomaly — it was the plain
build's own baseline, compared to the wrong form.

**Two gates remain open for the reviewer, and `PASS` does not close them.** (a) Acceptance
criterion 9, the **manual approval-workflow regression**, is `NOT CHECKABLE` here — it needs
`V.SMART.Web` running against a tenant database as a user holding `UserAuthority` rows, and no
session may acquire a credential (Q-14 / R-01 / Q-32). (b) The **MAUI head was not built** —
unverified, not passing (KB-083). Also note the finding that reframes the task: the headline
`IApprovalService`/`Authorization.razor` dependency was **dead text** — `Authorization.razor`
contains zero `static` and declares no type, so the `using static` import set was provably
empty. M2-B04 removed a documentation-level architectural violation and installed a guard; it
did **not** sever a real compile-time coupling. One deviation: no
`scripts/check-no-pages-references.sh`, because the task makes it conditional on
`tests/V.SMART.Shared.Tests/` not existing, and it exists. Raised **Q-55** rather than deleting
the now-unused `FundTransFilter.Bank` property. Full account, including attempt 1's
transcript-traced failure: [`tasks/M2-B04.md` § Execution Record
(2026-08-21)](tasks/M2-B04.md#execution-record-2026-08-21).

**Attempt 1, for the record (2026-08-21).** `Failure category: environment`, not
`business-rule`/`architecture`/`acceptance-criterion`. The `migration-implementer` agent
(`opus`) died mid-response — "API Error: The response stopped arriving" — after doing most of
the real work: 14 of 15 non-Razor `source_files` had their `V.SMART.Shared.Pages` `using`
removed, the one load-bearing case (`FundTransFilterVM.cs:27`, typed as the Razor component
`Bank` instead of the EF entity `Banks`) was found and fixed, and `dotnet build
V.SMART.Api` came back clean — `0 Error(s)`, `6695 Warning(s)` — before it tried and failed
(shell heredoc quoting bug, no code effect) to write the architecture guard test and then lost
its connection. It was blocked on a retry, not on a human decision — the same `environment`
category as `M0-12-01` attempt 1 (`failure-log.md`), where the retry alone cleared it, and it
cleared this one too. Attempt 2 resumed from the surviving working-tree diff rather than
re-deriving it, and wrote the guard test with `Write`/`Edit` instead of the Bash heredoc whose
quoting bug had cost attempt 1 that file. **2 of 3 attempts used, 0 escalations.**

²⁹ **Branch-state census, 2026-08-21 — four rows above were `Ready` on `master` while finished
or blocked work already existed on a branch.** This is not bookkeeping pedantry: it is the exact
condition that produced **two independent branches for `M2-A08`**
(`migration/M2-A08-row-scope-and-account-gates`, which validated `PASS`, and
`migration/M2-A08-row-level-scoping`, still live in `wt-M2-A08` doing different work — INV-028 →
KB-120 and the Q-05…Q-08 answers). A session cutting from `master` reads `Ready` and starts
work that already exists.

Each state below was read from the branch itself — `git log --oneline -2 <branch>` plus that
branch's own tracker row — not inherited from a summary:

| Task | Branch | Tip | Actual state |
|---|---|---|---|
| `M2-B12-01` | `migration/M2-B12-01-inv-012-numbering` | `407d0ba` | 🚩 **`Blocked`, owner Vivek.** Escalation budget exhausted: validated `FAIL` at `fa4a2ad`, crossed `escalate_after_failures: 2`, the one permitted escalation was spent on a diagnosis committed as `8a54f96`, **and that fix has never been re-validated**. 2 of 3 attempts used. |
| `M2-A08` | `…-row-scope-and-account-gates` | `bca92fd` | `Needs Review`, validated `PASS`. |
| `M2-A08` ⚠ | `…-row-level-scoping` | `6e6633a` | Second branch, same task id, different content, live worktree. **Owner decides which is the real `M2-A08`.** |
| `M2-B01` | `migration/M2-B01-api-versioning` | `045a7f4` | Close-out claims validated `PASS`, **11 of 12 criteria met, criterion 4 partial**. |
| `M0-10` | `migration/M0-10-candelete-guard-audit` | `fc8e0c0` | Close-out claims `Needs Review` after attempt 3, regression repaired. |

**`M2-B01` and `M0-10` never updated their own tracker rows off `Ready`**, so even reading their
branches directly, the tracker is not where their status lives — the commit subject and the task
file are. That is worth fixing at merge.

**A false `PASS` was corrected in the course of this census.** The runner state carried into this
session recorded `M2-B12-01` as validated `PASS` and awaiting merge; it had already propagated
into more than one file, and this session propagated it onto `master` once before checking.
`M2-B12-01`'s own tip commit says the opposite in its subject line — *"corrects a premature
PASS"* — and its runner-state adds that **no genuine `PASS` of `58e7bee` exists anywhere in this
repository**. It was caught only because `git stash list` incidentally printed that commit
subject next to the branch name.

**The rule this earns, which belongs with footnote ²¹'s:** a status inherited from a sibling
branch is a *claim*, not a fact, and it decays exactly as fast as the branches move. Confirming
it costs one `git log` per branch. `git worktree list` belongs in the same check — three sibling
worktrees were live this session and the tracker cannot see any of them.

³⁰ **M0-01-03: the rebuild drill ran on 2026-08-21 — §§2–6 passing, §7 and the named operator
still outstanding.** Branch `migration/M0-01-03-rebuild-drill` (`34b5e32`), unmerged. Full
record: `db/REBUILD-DRILL-LOG.md`, and *Execution Record (2026-08-21)* in
[`tasks/M0-01-03.md`](tasks/M0-01-03.md).

**The task file's own step 7 was stale and blocked this task twice.** It says *"You cannot
execute it — there is no SQL Server instance reachable from this session and no credential to
use if there were."* Both clauses were false: `MSSQL$SQLEXPRESS` was running and reachable by
**Windows integrated authentication**, so no credential was needed at all. Footnote ²¹ recorded
exactly this on 2026-08-19 and moved the task `Needs Review` → `Ready`, **but nobody updated
the task file**, so the next session read the old premise as current and the autonomous runner
stopped on it again. *Same failure shape footnote ²¹ was written about: a negative result is a
claim about the search, not about the world, and it decays.*

**What the drill established.** An empty database becomes a working tenant database from
repository artefacts alone in about a minute: `MasterDbContext` applied, one `Tenants` row,
**108 migrations in ~50 s** producing 197 tables, 150 `Screens` and the `Administrator` user,
then **91 stored procedures in 2.16 s, 0 failed**, idempotent on a second run.
`db/deploy-stored-procedures.ps1` loses its `UNVERIFIED` banner on evidence and its ordering
assumption moves from *Inferred* to **Confirmed**. R-04's "add a deployment step" half closes.
One failure, fixed in the runbook: `--connection` alone cannot work, because M0-03-01 replaced
the design-time factories' hardcoded credential with a fail-fast resolver that throws before
`dotnet ef` applies it — so the runbook's old *"a step that succeeds without a connection
string silently used the hardcoded one"* warning is now obsolete, and that is an improvement.

**🚩 The drill's most valuable output is a security finding that has nothing to do with the
drill — R-65, and it lands on `M2-A02`.** `V.SMART.Api/Authorization/ScreenCatalogue.cs`
compiles **152** screen names; the rebuilt database **and** the live development database both
hold **150**, with `ScreenCode` 114 and 115 absent. The two phantoms are **`Bill Paid List`**
and **`Bill Pending List`**. `ScreenRightStartupValidator` accepts both, so a controller
annotated `[RequireScreen("Bill Paid List")]` **passes startup validation and then denies every
request forever, in every tenant, silently** — precisely the lockout KB-105 warns about at its
own `:130`. Three KB-105 facts recorded as **Confirmed** are corrected: they were derived from
the `HasData` seed block, and the seeded state is not the migrated state once later migrations
`DeleteData` seed rows. **`M2-A02` must not begin against this catalogue.** Owner **Vivek**.

**Also raised:** **Q-65** — `20260324053747_AddnewTemperveryTable` is the only migration file
with no `.Designer.cs`, so EF has never applied it to any database; ⚠ do not resolve it by
generating the Designer, because its `Up()` renames ~100 tables. And **"219 migrations" was a
file count** — 109 migration classes, 108 applicable, +1 for `MasterDb` (KB-012, R-30
corrected).

**Why it is not `Completed`.** Two acceptance criteria are genuinely open, not merely
unverified: runbook **§7** (start the Blazor host, log in, run one report, print one document —
the *"and the app runs against it"* half of G0 criterion 1) was **not attempted**, and the task
requires **a named person** to execute the drill, which an autonomous session is not. The
instance was also not a *fresh, empty* SQL Server — the drill created two throwaway databases
on the pre-existing development instance and wrote to nothing else, which leaves that half of
the G0 wording unevidenced too. **Both drill databases were deliberately left in place** so a
named operator can run §7 without repeating §§2–6; `db/REBUILD-DRILL-LOG.md` names them and
gives the two `DROP DATABASE` statements.

³¹ **M2-B05: `Ready` → `Blocked` 2026-08-21. Its central premise is false, and no code was
written.** Owner **Vivek**; the task needs re-specifying, not retrying. Full evidence:
**INV-044** in [`investigation-registry.md`](../investigation-registry.md), the ⛔ banner on
[`tasks/M2-B05.md`](tasks/M2-B05.md), and the corrected **R-10** in
[`technical-debt-register.md`](../risks/technical-debt-register.md).

The task exists to *"replace the magic integer literals currently passed as `screenCode`"*.
**There are none.** The screen code is resolved at runtime from the database by screen name —
`GetScreenCodeByScreenNameAsync`, **166** call sites across **61** Razor pages. Of **244**
stock-call expressions inspected, **zero** pass an integer literal in the `screenCode`
position, and the only `screenCode = <integer>` assignment in the repository is commented out.
Its literal-replacement deliverable and Implementation Steps 8–10 — including the one the task
file itself calls *"the single most important verification step"* — have no subject, and
generating the constants class alone would commit a file no call site uses.

**The risk R-10 describes is real; it named the wrong parameter.** `AddOrUpdateStockAsync`'s
**second** argument is `storeId`, and **55 call sites pass a bare `6` or `7`** — confirmed as
`REJECTION STORE` and `REWORK STORE` against both a rebuilt-from-source and the live
development database, with all 9 `Stores` rows migration-seeded and identical between them.
Filed as **R-66**, and the obvious candidate for whatever M2-B05 is re-cut into. Note the
asymmetry that makes it worse than R-10 as written: `screenCode` is looked up by name and
therefore *cannot* be got wrong, while `storeId` sits at position 2 beside `itemId`, unnamed
and unchecked, encoding a business assumption in 55 places.

**How this was missed until now.** R-10 was marked `Confirmed` without a call site being
opened — the same shape as `M0-01-03`'s stale "no SQL Server is reachable" (footnote ³⁰) and
KB-105's seed-derived "152 screens" (**R-65**). *Three times in two sessions, a claim about the
source stood in for a claim about the running system.* Reading the signature is not reading the
call site; reading the seed is not reading the database.

**What survives re-specification:** M2-B05's *secondary* value. ADR-004's `[RequireScreen("…")]`
still takes a hand-typed string and `ScreenCatalogue.cs` still hard-codes two screen names no
database contains (**R-65**). A generated, database-derived catalogue would serve that and fix
R-65 together — but that belongs with **M2-A02**.

³² **M2-B06: `Ready` → `Blocked` 2026-08-21 — it has a hard dependency on `M2-B01` that its
own `depends_on` does not declare.** No code written, no branch. This was the last candidate in
the pool, and finding it exhausts the selectable set entirely.

**The conflict.** M2-B06's *API Changes* table mandates *"plural kebab-case under `/api/v1`"*
and every endpoint it specifies is `/api/v1/...`. **`master` has no `/api/v1`.** Its two
controllers are `[Route("api/auth")]` and `[Route("api/currencies")]`. The version prefix, and
the `ApiRoutes.V1` constant that owns it, exist **only** on the unmerged
`migration/M2-B01-api-versioning` branch — whose own doc comment states the rule this task
would have to break: *"no controller author writes the version string by hand."*

A branch cut from `master` therefore has three options, all bad:

| Option | Why it fails |
|---|---|
| Hard-code `api/v1/files` | Writes the version string by hand — the exact thing `ApiRoutes` exists to prevent — and desynchronises from M2-B01 |
| Use `api/files`, matching master | Violates M2-B06's own acceptance criteria and ADR-002 §6 |
| Recreate `ApiRoutes.cs` | Duplicates a **new file** that already exists on M2-B01's branch — the worst kind of merge collision |

**M2-B06 becomes genuinely `Ready` the moment `M2-B01` merges**, and needs no re-specification
— unlike `M2-B05` (footnote ³¹), this task is sound, it is only mis-sequenced. Add `M2-B01` to
its `depends_on`.

**Note what did *not* block it.** M2-B06 names React 13 times and has a `## React Changes`
section, which on `CLAUDE.md`'s literal ADR-007 test makes it stale. It is not: those hits are
boilerplate plus prose describing the consuming client, and its deliverable — replacing
`IBrowserFile`/`IFileOpener` with HTTP file endpoints — is stack-agnostic and survives the
Angular switch untouched. That test needs tightening; see `failure-log.md`, *"ADR-007 staleness
test is unusable as written"*. **Do not re-block this task on the grep when M2-B01 lands.**

**One thing to settle before it runs.** Its security section requires `[RequireScreen]` on
every endpoint. Suitable seeded screens do exist for the Excel endpoints — `Excel Upload` (97)
and `Master Upload` (25) — but there is **no generic "Files" screen**, and the task says only
that download *"requires the right on the screen that owns the file"*, which is a design the
implementer would have to invent. That interacts with **R-65** (two catalogue names exist in no
database) and with the still-open fact that `[RequireScreen]` is **opt-in, not
deny-by-default** — an unannotated controller is currently allowed through, which `M2-A02` is
meant to close.

³³ **M2-B01: `Completed` and merged to `master` 2026-08-21** (`ae9d2c8`, `--no-ff`), on the
owner's explicit instruction. Validated `PASS`, **11 of 12 acceptance criteria MET**.

**Re-verified on `master` after the merge, not taken from the branch's own report:**
`dotnet build V.SMART.Api --no-incremental` → **0 errors, 6694 warnings**, matching KB-083's
`--no-incremental` row exactly (delta 0); `tests/V.SMART.Api.Tests` **117 passed**;
`tests/V.SMART.Shared.Tests` **84 passed**. Both controllers now read
`[Route($"{ApiRoutes.V1}/auth")]` and `[Route($"{ApiRoutes.V1}/currencies")]`.

*(The Shared suite is 84 here, not the 86 that `M2-B04`'s record cites — the two extra are that
branch's architecture-guard tests, still unmerged. Not a regression.)*

**Criterion 4 is `PARTIAL` and was accepted knowingly.** `POST /api/v1/auth/login` reaches and
executes the action, and `POST /api/auth/login` now returns **404**, so the routing content —
the only thing this task changes — is fully proven. The login **success branch** was never
exercised, because it needs a valid dev-tenant credential no session may obtain (Q-14 / R-01 /
Q-32). The diff contains **zero lines** inside `AuthController`'s body, `JwtTokenService`, or
anything else on the token path. **To close it:** one valid dev-tenant credential, then a single
`POST /api/v1/auth/login` expecting `200` with a `token` field.

**Releases three tasks at once** — `M2-B06`, `M2-B09` and `M2-B11` were all blocked on the
`/api/v1` route surface and the `ApiRoutes.V1` constant this branch introduces. `M2-B06`'s
block was footnote ³²'s undeclared dependency; `M2-B09` and `M2-B11` were step-2 same-file
conflicts on `Program.cs`/`CurrencyController.cs` with this branch while it was in flight.

**Housekeeping:** the worktree `C:/Kumar/NexGen-ERP---2025-master/wt-M2-B01` and the branch
`migration/M2-B01-api-versioning` are now merged and can be removed
(`git worktree remove wt-M2-B01 && git branch -d migration/M2-B01-api-versioning`). Left in
place — removing another session's worktree is not this session's call.

³⁴ **M2-B09: implemented 2026-08-21, `Needs Review` on `migration/M2-B09-reference-endpoints`
(`d1175db`), unmerged.** The first task this run could execute, and only because the owner
merged `M2-B01` — every route it specifies is under `/api/v1`, which did not exist on `master`
until then. Closes KB-041 item **B6** and the output-caching third of **C1**. New doc:
**KB-124**. Full record: [`tasks/M2-B09.md` § Execution Record](tasks/M2-B09.md).

**Premise verified before writing code**, after three false premises earlier in this run — it
holds: `GetIGST`/`GetGST` really are `FirstOrDefault` returning `0` for an unlisted rate, and
all five `ICommonService` methods exist as specified.

**Two traps found and disarmed, either of which would have shipped silently:**

1. **ASP.NET Core's default output-cache policy declines to cache authenticated responses**, and
   all six endpoints are `[Authorize]`. Composing on it would have produced a cache that
   **stores nothing** — working endpoints, green tests, meaningless measurements, discovered
   only by someone profiling the API months later. The policy is hand-written and a test asserts
   the opt-in explicitly.
2. **`UseOutputCache()` placement is load-bearing in both directions.** After
   `UseAuthentication` because the cache key is a claim — placed earlier, `HttpContext.User` is
   still anonymous, the policy fails closed on every request, and the cache again stores
   nothing. After `UseAuthorization` so a cache hit is still an authorized request.

**The key is the tenant, and the policy fails closed.** Five of the six lists come from a
per-tenant `DbContext`; a URL-only key serves tenant A's data to tenant B. Missing, empty,
unparseable, non-positive or unauthenticated → caching is disabled rather than degraded to an
unkeyed entry.

**Measured, not assumed** — against the M0-01-03 drill database *and* the live one, which agree
exactly: 40 states, 49 uoms, 3 currencies, **150** screens, **0** terms; 300 queries in **16 ms**
(≈0.05 ms each). **KB-124 states plainly that the database cost is negligible** and the cache is
justified by request-count collapsing, not by expensive queries — so no later reader cites a
reason that was never true. Two findings: `/reference/terms` returns an empty array because
`TermsAndConditions` holds zero rows in both databases, and `/reference/screens` returns **150**,
disagreeing with `ScreenCatalogue.cs`'s 152 — that is **R-65**, a defect in the catalogue, not
in this endpoint.

**Verification:** Api build 0 errors / **6694** warnings (= baseline), warning gate
**`PASSED`** at 6693, Web build 0 errors / **6697** (= baseline), **162** Api tests (117 → 162)
and **84** Shared tests, no regression. `CommonConstants.cs`, `CommonService.cs`,
`ICommonService.cs`, `ApplicationDbContext.cs` and `CurrencyController.cs` confirmed
**unchanged by diff**; nothing under `V.SMART.Web/`, the MAUI head or `Migrations/` touched.
`Program.cs` is the only modified production file — two additive lines, placed away from
`M2-A08`'s pending insertions so that merge stays trivial.

**Two criteria are not met, and say so:** no Blazor screen was opened (the mechanical argument
is strong — `V.SMART.Web` builds at its exact baseline and nothing it consumes changed — but it
is an argument, not an observation), and there is **no end-to-end two-tenant HTTP test**. The
cache key and fail-closed behaviour are proven at the policy level, which is where a
cross-tenant leak would originate, but no test runs two tenants through a live host; that needs
a `WebApplicationFactory` harness this project does not have. **That is the residual risk of the
task**, recorded in KB-124 §6.

**R-15 is `partially resolved`, not closed** — correct at the API boundary, still coercing
in-process across **105** call sites of `GetIGST`/`GetGST`, and the `CalculationService`
disagreement (170 on one path, 0 on the other) is untouched.

³⁵ **M2-B06: implemented 2026-08-21, `Needs Review` on `migration/M2-B06-file-endpoints`.**
Footnote ³² was right that the task was sound and only mis-sequenced: once `M2-B01` merged it
needed no re-specification and was executed as written.

**Read the branch as two commits, deliberately.** `e9b143b` is **~979 lines this session did not
author** — found uncommitted in the working tree at session start, written 16:04–16:18 IST minutes
before, almost certainly by an earlier runner session killed mid-implementation (the M2-B06 Select
bookkeeping was written at 16:04 and no commit followed). Both live peer Claude sessions were asked
and ruled themselves out; no Visual Studio was running. The owner chose to adopt it. It was verified
before committing — build at baseline, stream copy present, every must-not-change file byte-unchanged
— but a reviewer should read it as unattributed code that passed inspection, not as reasoned-through
work. Everything after it is this session's.

**Delivered:** `POST /api/v1/files`, `GET /api/v1/files/{id:int}`, the three
`/api/v1/currencies/{export,import,import-template}` endpoints as the ADR-005 reference
implementation on **one** resource, and `ApiFileUploadService` — the `IFileUploadService`
implementation `V.SMART.Api` had none of, registered as a host registration beside
`AddVSmartDomain()` rather than inside it. `ICompanyService`/`CompanyService` no longer reference
`IBrowserFile`; the adaptation moved to the single Razor call site, `CompanyUpsert.razor:1105`.

**Verified:** `V.SMART.Api` **0 errors / 6694 warnings** and `V.SMART.Web` **0 / 6697** — both the
exact baselines, the Razor change adding none; `tests/V.SMART.Api.Tests` **117 → 148 passed**;
`tests/V.SMART.Shared.Tests` **84 passed**, no regression. All **seven** required negative tests
pass and are reported individually, plus a byte-identity round trip.

**One hazard that had to be designed around.** `CompanyService` used to call
`OpenReadStream(maxAllowedSize: maxFileSize)` *after* its own size check. Moving the stream to the
caller inverts the order, and `OpenReadStream` **throws** when the file exceeds the limit it is
handed — which would have replaced the screen's "File size is too large" toast with an exception,
an observable Blazor behaviour change. The call site opens with
`Math.Max(e.File.Size, maxFileSize)` so the open never throws and the service still owns the real
check.

**Two criteria openly unmet, and they are the same two `M2-B09` closed with.** No Blazor screen was
opened, so the manual upload/download regression is an argument (nothing touched but one call site;
`WebFileUploadService` byte-unchanged; both hosts at baseline) rather than an observation. And there
is **no end-to-end HTTP test against two real tenant databases** — N2/N6/N7 are proved at the policy
and unit level; the wire-level proof needs a tenant-DB credential no session may acquire
(**Q-14 / R-01 / Q-32**).

**A live defect was found and deliberately left in place → R-67.** `SaveCorresFileAsync` creates the
file and leaves the stream copy commented out (`WebFileUploadService.cs:100-104`), returning the
path as though it succeeded, so **every Blazor correspondence and drawing upload lands as 0 bytes**.
It has been survivable only because `Correspondence.Image` holds a second copy and the two download
screens disagree about which to read. Fixing it changes live behaviour and needs its own task.

**Also recorded:** **INV-045** (the file-handling investigation, including the negative result that
no blob storage, CDN or virus scanning exists anywhere), and a storage half added to **Q-16** —
uploads live on a local filesystem, and a containerised or multi-instance deployment loses or splits
them silently.


³⁶ **M2-B11: `Ready` → `Needs Review` 2026-08-21.** Health checks (`GET /health/live`,
`GET /health/ready`) and a new `ILogger`-based `StructuredLoggingService` land on
`migration/M2-B11-health-checks-logging` (`7b4b86c`, plus two follow-ups: `81ad961` fixed a
`CS8767` nullability mismatch the warning-gate ratchet caught on attempt 1, `12dad11` corrected
the Execution Record's own warning-gate measurement and the retention wording). `ILoggingService`
is byte-identical (`git diff` empty); `FileLoggingService` is kept and still the Blazor/MAUI
registration; the new implementation is wired only in `V.SMART.Api`. **Validated `PASS` on
attempt 2 of 4** (attempt 1 failed the CI warning ratchet at 6694 vs. the 6693 gate baseline —
category `implementation-error`, fixed same session, no loop). Independent-validator run:
`dotnet build V.SMART.Api` 0 errors, gate `tools/compare-warnings.sh` measured 6693 = baseline,
**PASSED**; `dotnet test tests/V.SMART.Api.Tests` 179 passed (148 → 179); `dotnet test
tests/V.SMART.Shared.Tests` 84 passed; `V.SMART.Web` 0 errors / 6697 (its exact baseline).
Runtime probes against a real local SQL Server confirmed `/health/live` returns 200 with the
master DB unreachable and touches no database (`Predicate = _ => false`); `/health/ready`
returns 200 with both `master-db` and `tenant-db` healthy, and 503 naming `master-db` when it is
down; a credential grep of every emitted `diagnostics-*.json` found zero hits for `Password`,
`TenantInfo`, `ConnectionString` or a server/database name. Produced **KB-113**
([observability.md](../architecture/observability.md)) and **INV-046** (494 `LogUserAction`
call sites, all in `V.SMART.Shared`, zero in `V.SMART.Api` — so no audit event was observed at
runtime, only proved at unit level; the `#if ANDROID || WINDOWS || MACCATALYST` `_basePath`
branch confirmed dead on both TFMs). **R-23 is marked resolved for `V.SMART.Api` only** — still
open for the Blazor and MAUI hosts, which keep `FileLoggingService` unchanged. **Criteria not
met, stated rather than glossed:** the tenant-unreachable-while-master-healthy 503 was proved
only at unit level, not over HTTP (writing a bogus `Tenants` row was out of scope); no
`LogUserAction` event was observed at runtime because no code path in `V.SMART.Api` calls it;
the Blazor host was not started; the MAUI head was not built (unverified per KB-083). Full
record: [`tasks/M2-B11.md` § Execution Record](tasks/M2-B11.md#execution-record-2026-08-21-branch-migrationm2-b11-health-checks-logging).
Nothing depends on `M2-B11` in the dependency graph, so no other row is released by this close.
³⁷ **M2-A07: `Needs Review` — implemented and independently validated `PASS` on
`migration/M2-A07-me-endpoint` (`61da4bd`, one commit on top of `master` tip `8b1a261`), attempt
1 of 4 as the validator reported it, 0 escalations, `scopeOk: true`.** Not merged; per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) only the repository owner
may set it `Completed`.

> `GET /api/v1/me` returns `userId`, `userName`, `tenantId`, role and the full screen-rights
> map, read only through `IUserRightsProvider` (never the repository directly). Role is sourced
> from the JWT `ClaimTypes.Role` claim — never `CurrentUserService.GetUserRoleAsync()` (R-18)
> — and `ERPAdmin` (R-31) is not propagated; the API model carries role as an opaque string.
> `UserAuthority` and four `User` UI flags are deliberately deferred, each independently
> justified in **INV-042** (`Complete`, 7 findings). Absent `UserRight` rows are omitted, not
> padded to 152 `false` entries; duplicates collapse first-match-wins, matching the filter's own
> `FirstOrDefault`. Route is the literal `/api/v1/me`, recorded for `M2-B01`.
>
> **Independently re-verified, including two checks the implementer had reported as
> not-checkable from a test project.** The validator started the real API host and confirmed
> over the wire: no token → `401` `application/problem+json`; a real `UserId=1`/`TenantId=1`
> token → `200` with exactly the five documented members and **150** rights keys, matching
> `SELECT s.ScreenName FROM UserRights r JOIN Screens s ON s.Id=r.ScreenId WHERE r.UserId=1`
> against the live tenant database exactly (150 vs 150, `diff` empty). Body scanned for
> `password`/`connectionstring`/`server=`: none. `dotnet build V.SMART.Api`: **0 errors, 6,695
> warnings** — exact baseline. `dotnet test tests/V.SMART.Api.Tests`: **148/148** (117 → +31).
> `dotnet test tests/V.SMART.Shared.Tests`: **84/84**, no regression. `git diff --stat HEAD~1
> HEAD`: 9 files, +1184/-13; nothing under `V.SMART.Shared/`, `V.SMART.Web/`, `V.SMART/V.SMART/`,
> `V.SMART.Api/Auth/`, `V.SMART.Api/Authorization/`; no EF migration.
>
> **Two criteria are recorded `NOT MET`/`NOT CHECKABLE` rather than silently dropped — the
> `M2-A06` precedent (this footnote table, ²³).** (1) *"`CanCreate = false` ⇒ `403` from `POST
> /api/currencies`, proven by test"* is not satisfiable today: `CurrencyController` carries no
> `[RequireScreen]`/`[RequireRight]` — that annotation is `M2-A02`'s, gated on `Q-28`. The
> substitute test feeds one `ScreenRightSet` to both `MeController` and the real filter and
> asserts agreement, the strongest available today. (2) *"On the `M2-A03` exempt allow-list ...
> and the harness passes"* — the exemption mechanism itself, `[NoScreenRight(justification)]`,
> is shipped and was observed accepted by `ScreenRightStartupValidator` at boot on the live
> host; there is no separate harness to run because `M2-A03` is `Blocked`.
>
> **New risk raised by the implementer: R-43** — `tests/V.SMART.Api.Tests` has no
> `Microsoft.AspNetCore.Mvc.Testing`/`WebApplicationFactory`, so every HTTP-level claim in this
> task (401 without a token, the JWT inbound claim map, tenant isolation over the wire) is
> asserted by declaration inside a controller test, not observed as a real response — which is
> exactly why the validator went to the live host itself for the two checks above. **New risk
> found by the validator: R-44** — probing a token whose `TenantId` claim names no `Tenants` row
> returned `200` with a *different* tenant's rights, because `TenantProvider.cs:46-58`'s
> host-based fallback and `UserRightsProvider`'s claimed-tenant cache key compose badly. Not an
> `M2-A07` regression — both files are pre-existing and were out of scope — but it contradicts
> `UserRightsProvider.cs:17-22`'s unqualified cross-tenant claim. Tracked as **Q-37**, routed to
> `M2-A02`/`M2-A08`.
>
> Full record: [`tasks/M2-A07.md` § Execution Record
> (2026-08-20)](tasks/M2-A07.md#execution-record-2026-08-20).
³⁸ **M2-C00: `Needs Review` 2026-08-20 — implemented across 3 attempts, independently validated `PASS`, not merged.**

> Rewrote [KB-050](../frontend-new/react-architecture.md) for Angular and re-specified `M2-C01`
> in the same change, per scope. Branch `migration/M2-C00-kb050-angular-rewrite`, cut from
> `master` tip `ec70620`, merge-base `8b1a261` (fully caught up with `master`, including the
> `be818b9` ADR-007 sweep and the criterion-3 relaxation both landed mid-task). Attempt 1 failed
> validation on the stack-table criterion (since relaxed on `master` — the criterion was wrong,
> not the document); attempts 2 and 3 corrected line-citation drift and re-anchored against
> ADR-007 as it grew from 197 to 225 lines mid-task. **Validator verdict: `PASS`**, all nine
> acceptance criteria `MET`, `scopeOk: true`, diff confirmed docs-only (9 Markdown files),
> `dotnet build V.SMART.Api` unchanged at the 6695-warning/0-error baseline. Full record:
> [`tasks/M2-C00.md` § Validation close-out (2026-08-20)](tasks/M2-C00.md#validation-close-out-2026-08-20).
>
> **`M2-C01`'s row reads `Blocked`, not `Ready`, despite the ADR-007 re-scope note (footnote ²⁶)
> having marked it `Ready` in anticipation.** Its Hard prerequisite is `M2-C00` at `Completed`,
> and `Needs Review` does not satisfy that
> ([KB-082](dependency-graph.md#ready-task-selection-rule) step 1) — the same rule already applied
> to `M2-A02`/`M2-A07`/`M2-A08` under footnote ²⁷. It becomes genuinely `Ready` the moment this
> branch is reviewed and merged; no further work is needed on it before then.
>
> **Only the repository owner may set `Completed`**
> ([KB-088 § Who may set COMPLETED](workflow.md#who-may-set-completed)).
