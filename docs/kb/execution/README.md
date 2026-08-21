---
doc_id: KB-080
title: ERP Migration — Master Execution Plan
module: execution
source_files:
  - NexGen-ERP---2025-master.sln
  - V.SMART/V.SMART.Api/Program.cs
  - V.SMART/V.SMART.Web/appsettings.json
  - V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: proposal
confidence: n/a
last_verified: 2026-08-21
dependencies: [KB-001, KB-002, KB-003, KB-005, KB-020, KB-030, KB-040, KB-041, KB-050, KB-051, KB-052, KB-053, KB-060, KB-070, KB-071, KB-081, KB-082, KB-083, KB-084]
---

# ERP MIGRATION — MASTER EXECUTION PLAN

> **Proposal / living document.** This converts the migration strategy
> ([KB-070](../migration/migration-strategy.md)) and milestone tracker
> ([KB-071](../migration/milestones.md)) into executable tasks, each with an independent
> fresh-session prompt.
>
> **This document does not authorise implementation.** It is the roadmap. Work starts when
> a task is opened deliberately, one at a time.

## How this plan is organised

The required hierarchy is `PROJECT → MILESTONE → TASK`. It is paginated across files rather
than held in one, because a single file containing 25 detail sections for every task would be
unusable:

| Level | Lives in |
|---|---|
| Project, milestones, task summaries, graphs, gates | **this document** |
| **The one active task** | **[KB-089](current-task.md)** — start here |
| **How a session runs, start to finish** | **[KB-088](workflow.md)** |
| Full task specification | `tasks/<TASK-ID>.md`, one file per task |
| Master progress tracker | [KB-081](task-tracker.md) |
| Dependency graph, critical path, next-task selection | [KB-082](dependency-graph.md) |
| Generation rules + verified commands | [KB-083](prompt-template.md) |
| Milestone review / handoff / DoD templates | [KB-084](review-templates.md) |
| Template for new task files | [KB-090](task-template.md) |

**This document is ~55 KB. Deep-link to the section you need; do not read it end to end.**

Working loop — **the repository is the persistent memory; the conversation is temporary
execution context**:

```
NEW session → "Read CLAUDE.md and docs/kb/execution/current-task.md.
               Execute the current task. When it closes, pick the next task that can actually be done."
   → it executes ONLY that task → test → review the diff → commit
   → it updates KB-081 and rewrites current-task.md for the next task → STOP
   → ANOTHER new session repeats, with nothing pasted in
```

> **Superseded 2026-08-16.** The earlier loop was "open `tasks/<ID>.md`, copy the
> ~150-line prompt at the bottom, paste it into a new session." Those prompt blocks are now
> obsolete — their invariant content lives once in `CLAUDE.md` — and no prompt is copied
> any more. See [KB-083 § The superseded model](prompt-template.md#the-superseded-model).

---

## 1. Project Context

V.SMART / NexGen ERP is a mature multi-tenant discrete-manufacturing ERP for the Indian
market (GST e-Invoice, e-Way Bill, ITC-04, TDS/TCS). Its document spine is
enquiry → quotation → sales order → job order → route card → production → despatch →
invoice, plus purchase, subcontract and labour-work chains.

Measured, not estimated ([KB-001](../00-executive-summary.md)):

| | |
|---|---|
| Projects | 4 (`Shared`, `Web`, `Api`, MAUI) |
| Business services | 285 (128,518 LOC) |
| Razor pages / routes | 333 / 440 |
| EF entity sets | 196 |
| Stored procedures referenced | 94 |
| Logic inside Razor `@code` | ~184,000 LOC (57% of 321,661 Razor LOC) |
| Existing API | 2 controllers, 6 endpoints |
| Tests / CI | none |

**The objective is frontend modernisation, not an ERP rewrite.**

## 2. Existing Architecture

```
Blazor Server UI (V.SMART.Web)        MAUI Hybrid (V.SMART)
            │                                  │
            └──────────── in-process DI ────────┘
                          │
        V.SMART.Shared — 285 business services, ViewModels,
        Repository + UnitOfWork, EF Core 9, AutoMapper,
        FastReport, 94 stored procedures
                          │
              SQL Server — database per tenant
```

Target, per [ADR-001](../decisions/ADR-001-keep-existing-backend.md) and
[ADR-002](../decisions/ADR-002-rest-api-layer.md):

```
Angular SPA ──HTTP──► V.SMART.Api (ASP.NET Core Web API, .NET 9)
                          │
                          ▼
              THE SAME V.SMART.Shared services
                          │
                          ▼
              THE SAME SQL Server databases
```

Blazor keeps serving users throughout. Both UIs run against one database, which is why
rollback carries no data-migration problem.

## 3. Migration Principles

1. **Strangler-fig, never big bang.**
2. **The backend is extended, never rewritten.**
3. **Extract before rebuild** — business logic leaves `@code` into services *first*,
   verified against the running Blazor app, *then* the Angular screen is built.
4. **The server is authoritative** for calculations, validation, permissions, numbering.
5. **Migrate along the dependency graph** — masters → documents → reports.
6. **Every module ships behind a per-tenant, per-module feature flag.**
7. **Testing attaches to the task that introduces the behaviour**, never deferred to M5.
8. **No new ERP functionality** during migration without a separately approved task.

## 4. Source-of-Truth Rules

Per [KB-002](../source-of-truth-rules.md), authority order on conflict:

1. **Current source code** — authoritative.
2. **Database schema / EF migrations** — authoritative for storage, recognising that
   migrations contain superseded snapshots.
3. **The knowledge base** — authoritative for interpretation until code contradicts it.
4. **Older prose documentation** — hypothesis only. `docs/ARCHITECTURE.md` is superseded
   and contains known factual errors.

Every significant finding is classified `Confirmed` / `Inferred` / `Unknown`. An inference
is never presented as fact. Business rules require `file:line` evidence.

## 5. Knowledge Base / RAG Rules

The KB remains metadata-driven ([KB-005](../INDEX.md)). Every document carries
`doc_id, title, module, source_files, entities, api_endpoints, database_tables,
business_rules, status, confidence, last_verified, dependencies`.

Retrieval filters on metadata **before** semantic search, and **never mixes `status:
complete/partial` (as-is) with `status: proposal` (plan)** — answering "how does X work?"
from a proposal document is this KB's worst failure mode.

Every task declares what it **reads** and what it **updates**:

| Task declares | Meaning |
|---|---|
| Required Existing Knowledge | KB docs / ADRs / INV ids to read first |
| Investigation Requirements | New investigation · Reuse · Update · None |
| Documentation Updates | exact docs + frontmatter fields to touch |
| Investigation Registry Updates | whether an INV row is added or amended |

**Anti-repetition protocol** (mandatory, in every prompt): search the registry → reuse if
Complete and not stale → investigate only the gap if Partial → investigate and record if
absent or contradicted → record negative results too.

## 6. Milestone Overview

| ID | Milestone | Est. | Gate | Status |
|---|---|---|---|---|
| **M0** | Stabilise | 2–3 wks | G0 | ⚠️ **PASSED WITH EXCEPTIONS** 2026-08-19 — criteria 1, 2, 3 deferred by owner. Review: [KB-107](M0-milestone-review.md) |
| **M1** | Repository Understanding | — | G1 | ✅ Complete (rolling) |
| **M2** | Foundation | 6–8 wks | G2 | **OPEN** 2026-08-19 |
| **M3** | Core Modules | 12–16 wks | G3 | Blocked by G2 |
| **M4** | Advanced Modules | 16–22 wks | G4 | Blocked by G3 |
| **M5** | Hardening | 6–8 wks (overlapped) | G5 | Runs from M2 |
| **M6** | Production Migration | 4–6 wks | G6 | Blocked by G4 |

Order is unchanged from [KB-071](../migration/milestones.md). Existing task ids are
preserved; new work is added as new ids or as children (`M0-03-02`), never by renumbering.

### Findings from this planning pass that changed M0

Three facts were confirmed on 2026-08-12 while validating that the plan's commands and
paths were real. All three are recorded as **INV-029**.

1. **RESOLVED 2026-08-12 (INV-034 — see [KB-085](M0-00-baseline-decisions.md#repository-visibility-correction-inv-034)): the repository is public, by the owner's deliberate choice, made the same day.**
   Timeline: the original claim below — "`git ls-remote` succeeds without authentication →
   the repository is public" — was wrong when made. Windows Git Credential Manager
   (`credential.helper = manager`, configured system-wide) silently authenticated every
   git operation with the repo owner's cached GitHub credentials, so `git ls-remote`
   *appeared* to succeed anonymously without ever actually testing anonymous access.
   Re-tested with the credential helper explicitly disabled
   (`git -c credential.helper= ls-remote ...`): git demanded a username and failed —
   proving the repository was, at that point, **private**. The repo owner (Kumar) was then
   informed of this and **deliberately set the repository to public**. Re-verified with the
   same rigorous method (not the original flawed one): `git -c credential.helper=
   ls-remote` now succeeds with no auth demanded, and an unauthenticated REST call now
   returns `200`. **The repository is genuinely public now, and the exposed credentials
   (R-01, R-02) must be treated as published and reachable by anyone on the internet** —
   rotation (M0-04) and the history purge (M0-05) are urgent for real, not hypothetically.
   Q-19 ([open-questions.md](../open-questions.md)) is recorded as Answered.

   *(Original 2026-08-12 claim, preserved for the record but superseded by the correction
   above):* "The exposed credentials are published on the public internet. `git ls-remote`
   against `https://github.com/ErpStore/NexERP_B.git` succeeds without authentication → the
   repository is public." The SA password, the production host `154.61.76.112,1533`,
   and the `bspl` production credential are all present in the single committed commit
   `c12c5b2`, in **four files**, including hardcoded C# — not only `appsettings.json`:
   - `V.SMART/V.SMART.Web/appsettings.json`
   - `V.SMART/V.SMART.Shared/Data/MigrationData/ApplicationDbContextFactory.cs`
   - `V.SMART/V.SMART.Shared/Data/MigrationData/MasterDbContextFactory.cs`
   - `V.SMART/V.SMART/MauiProgram.cs`

   `bspl` additionally appears in `EinvoiceDatabaseService.cs` and `EWayDatabaseService.cs`.
   These are still committed-in-history credentials (R-01/R-02) requiring rotation and a
   history purge; the "published" escalation specifically is retracted. Rotation is not a
   week-1 task; it is the first action of the project, and it is not complete until the
   *hardcoded C#* is fixed too — the register's action item mentions only configuration.
   *(Confirmed. The JWT secret string was **not** found in `HEAD`, so `V.SMART.Api/appsettings.json`
   appears to be uncommitted — it is still exposed locally and must still be rotated.)*

2. **Large parts of the project are not in source control at all.** `git status --porcelain`
   returns 37 entries; `git log` shows exactly one commit, "Add project files." Of 2,162
   tracked paths, these have **zero**:

   | Untracked | Consequence |
   |---|---|
   | `V.SMART/V.SMART.Api/` — the whole Web API project | **the backend the Angular app is being built on is not in source control** |
   | `docs/` — the whole knowledge base | all analysis, ADRs and this plan exist on one disk |
   | `frontend/`, `.github/` | CI cannot run until `.github/` is committed |
   | `NexGen-ERP---2025-master.sln` | the only `.sln` in `HEAD` is `Bhargavi V.SMART ERP - 2025.sln`, **deleted** on disk |

   These are untracked, not gitignored (`git check-ignore` exits non-zero). This also
   retracts **R-14**, which claimed the opposite problem — that build output *was* committed
   — and which was marked *Confirmed* without having been traced. See
   [KB-002](../source-of-truth-rules.md#conflicts-found-between-this-knowledge-base-and-code).
   A task-per-branch workflow cannot start on top of any of this. → new task **M0-00**.

3. **The toolchain does not match the target framework.** Projects target `net9.0`; only
   the .NET **10** SDK is installed (10.0.300, 10.0.302). The build nevertheless succeeds:
   `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` → **0 errors, 6,695 warnings,
   ~3 min**. That warning count is the CI baseline — CI cannot use `-warnaserror` until it
   is reduced. → new task **M0-15**.

   **Resolved 2026-08-17 by M0-15 (see [KB-086](M0-15-build-baseline.md)):** the whole-solution
   build (`dotnet build NexGen-ERP---2025-master.sln`, including the MAUI head) **succeeds** —
   0 errors, 13,367 warnings, ~4–4.5 min — reproducibly, but only from a clean `obj`; a dirty
   `obj` produced 2 file-lock/permission errors unrelated to the code. Whether it succeeds on a
   workload-free CI runner (the default assumption for a hosted GitHub Actions runner) is
   **Unknown** — untestable in that session without a workload-free environment. M0-15
   recommends CI build `V.SMART.Api` and `V.SMART.Web` explicitly rather than the solution, so
   CI does not depend on either uncertainty; the MAUI head is then unbuilt in CI, a trade-off
   relevant to Q-11 (the MAUI app's future). The SDK is now pinned via a root `global.json`
   (`10.0.400`, `rollForward: latestFeature`) after the installed SDK set was observed to drift
   (10.0.300/10.0.302 → 10.0.300/10.0.400) on the same machine with no repository change. The
   warning baseline's dominant codes are the `CS86xx` nullable-reference family, not `MUD0002`
   as originally described — `MUD0002` is 130 occurrences, 1.94% of the 6,695 total.

---

## 7. M0 — Stabilise

### Description
Make the repository safe to build on. This is not migration work; it is the safety net
without which no later milestone is responsible.

### Objective
A fresh environment can be rebuilt from source control, secrets are rotated and
externalised, CI is green, and the two highest-consequence services have characterisation
tests pinning their current behaviour.

### Scope
Version-control hygiene, secret rotation and externalisation, stored-procedure capture,
CI, two confirmed defect fixes, one product decision, characterisation tests.

### Out of Scope
Any API controller. Any Angular code. Any business-behaviour change (M0-11's decision is
*recorded* here and *applied* only after M0-13 pins current behaviour). Any schema change.

### Prerequisites
None. M0 starts immediately.

### Dependencies
- **Business:** a product owner to answer Q-01 (M0-11); an owner to approve rotation windows.
- **Infrastructure:** credentials for a live tenant database (M0-01, M0-02); GitHub admin
  rights to rewrite history and audit repo visibility (M0-05); CI runner (M0-07).
- **Technical:** none beyond the current toolchain.
- **Testing:** M0-12 creates the first test project; everything test-related depends on it.

### Tasks

| Task | Name | Type | P | Depends on | Est. | File |
|---|---|---|---|---|---|---|
| **M0-00** | Establish a clean version-control baseline | DevOps | P0 | — | 0.5 d | [↗](tasks/M0-00.md) |
| **M0-15** | Toolchain and build baseline | DevOps | P0 | M0-00 | 0.5 d | [↗](tasks/M0-15.md) |
| **M0-04** | Rotate the exposed credentials | Security | P0 | — | 1 d | [↗](tasks/M0-04.md) |
| **M0-03** | Externalise configuration secrets *(parent)* | Security | P0 | M0-00 | 1 d | [↗](tasks/M0-03.md) |
| M0-03-01 | — `appsettings.json` → environment / user-secrets | Security | P0 | M0-00 | 0.5 d | [↗](tasks/M0-03-01.md) |
| M0-03-02 | — hardcoded connection strings in C# | Security | P0 | M0-03-01 | 0.5 d | [↗](tasks/M0-03-02.md) |
| M0-03-03 | — fail-fast startup validation | Security | P0 | M0-03-02 | 0.5 d | [↗](tasks/M0-03-03.md) |
| **M0-05** | Purge secrets from git history | Security | P0 | M0-03, M0-04 | 1 d | [↗](tasks/M0-05.md) |
| **M0-01** | Capture DDL for all 94 stored procedures *(parent)* | Database | P0 | — | 4–5 d | [↗](tasks/M0-01.md) |
| M0-01-01 | — reconcile the 94-name inventory against the 13 scripted | Database | P0 | — | 1 d | [↗](tasks/M0-01-01.md) |
| M0-01-02 | — script the missing procedures from a live tenant DB | Database | P0 | M0-01-01 | 2 d | [↗](tasks/M0-01-02.md) |
| M0-01-03 | — deployment script + rebuild documentation | Database | P0 | M0-01-02 | 1 d | [↗](tasks/M0-01-03.md) |
| **M0-02** | Confirm stored-procedure drift across tenants (Q-14) | Investigation | P1 | M0-01-02 | 1 d | [↗](tasks/M0-02.md) |
| **M0-08** | `.gitignore` + remove committed build output | DevOps | P1 | M0-00 | 0.5 d | [↗](tasks/M0-08.md) |
| **M0-07** | CI pipeline: restore → build → analyzers | DevOps | P0 | M0-15, M0-08 | 2 d | [↗](tasks/M0-07.md) |
| **M0-12** | Test project + `ICalculationService` characterisation tests *(parent)* | Testing | P0 | M0-07 | 3 d | [↗](tasks/M0-12.md) |
| M0-12-01 | — create the test project and wire it into CI | Testing | P0 | M0-07 | 0.5 d | [↗](tasks/M0-12-01.md) |
| M0-12-02 | — characterisation tests for `CalculationService` | Testing | P0 | M0-12-01 | 2.5 d | [↗](tasks/M0-12-02.md) |
| **M0-13** | `IStockManagerService` characterisation tests | Testing | P0 | M0-12-01 | 3 d | [↗](tasks/M0-13.md) |
| **M0-09** | Fix the two unreachable delete guards (R-08) | Backend | P1 | M0-12-01 | 0.5 d | [↗](tasks/M0-09.md) |
| **M0-10** | Audit all `CanDelete…` guards (INV-025) — **delivered 2026-08-21, output [KB-061](../risks/delete-guard-audit.md)**; the "`…Async`" in this name is a trap, see §7 note | Investigation | P1 | M0-09 | 2 d | [↗](tasks/M0-10.md) |
| **M0-06** | Remove the seeded default Administrator credential | Security | P1 | M0-12-01 | 1 d | [↗](tasks/M0-06.md) |
| **M0-14** | Gate `DetailedErrors` on `IsDevelopment()` | Security | P2 | M0-03-01 | 0.5 d | [↗](tasks/M0-14.md) |
| **M0-11** | Product decision — silent FIFO under-issue (Q-01) | Product Decision | P0 | M0-13 | decision | [↗](tasks/M0-11.md) |

### Parallel Work

**Can run in parallel** (disjoint files, no shared state):
- `M0-01-*` (database scripting, touches only `db/stored-procedures/`) runs alongside
  everything else in M0. It needs a DBA, not a developer.
- `M0-04` (rotation) is an ops action outside the repository and blocks nothing until M0-05.
- After `M0-12-01`: `M0-12-02`, `M0-13`, and `M0-09` touch different files and parallelise.
- `M0-06` and `M0-14` are independent single-file changes.

**Must remain sequential:**
- `M0-00 → M0-08 → M0-07`. All three rewrite repository-wide state (`.gitignore`, tracked
  file set, CI config); running them concurrently guarantees conflicts.
  **M0-08's scope changed (2026-08-17): removal → verification + prevention + enforcement.**
  Its title still says "remove committed build output", but R-14's "committed" claim was
  false (KB-060 R-14, INV-029 amendment): `git ls-files` has never contained a build-output,
  IDE-state or dependency path, so there was nothing to remove and no history rewrite to do —
  which matters, because acting on R-14 as written would have collided with M0-05, the only
  task authorised to rewrite history. The real work, and what M0-08 delivered, is (a)
  re-auditing *after* M0-00 first tracked `frontend/`, since `git add frontend/` on a tree
  containing `node_modules/` is exactly how the risk becomes real, (b) hoisting the ignore
  rules from `frontend/vsmart-erp/.gitignore` (which M2-C11 deletes) into the **root**
  `.gitignore`, and (c) `tools/check-no-build-output.sh`, the guard **M0-07 must wire in** as
  a CI step (`bash tools/check-no-build-output.sh`).
- `M0-03-01 → M0-03-02 → M0-03-03`. Same configuration surface, three passes.
- `M0-03` + `M0-04` → `M0-05`. Purging history before rotation leaves live credentials
  exposed in forks and clones; purging before externalisation loses the working config.
- `M0-13 → M0-11`. The decision must be taken with current behaviour already pinned by
  tests, or the change is invisible.
- `M0-09 → M0-10`. The audit uses the fix as its reference pattern.
  **2026-08-19:** M0-09's fix is implemented on `migration/M0-09-delete-guard-fix` (two
  identifiers in `MfgPoService.CanDeleteSalesOrderAsync`, pinned by
  `tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs`; suite 79/79 green).
  **M0-10's reference pattern therefore exists**: a `CanDelete…` guard that computes one
  boolean and tests another, proven unreachable by a test that seeds *only* the document the
  guard is supposed to catch. Task status is recorded in KB-081, not here.
  **2026-08-21 — M0-10 delivered** on `migration/M0-10-candelete-guard-audit`. Output:
  **[KB-061](../risks/delete-guard-audit.md)**, a 79-row inventory of every
  `(bool CanDelete, string Message)` guard, including every one judged correct. **The
  sequencing paid off and the premise did not survive it:** using M0-09's fix as the
  reference pattern, the audit found the defect class **eradicated** — one surviving
  instance across 93 guards, and it was already recorded in KB-060. The population is **79
  across 61 files**, not the "~40" or "63"/"64" published; scope guard work by **return
  shape**, because both the `Async` suffix *and* the `CanDelete` prefix miss real guards.
  Five follow-ups are **proposed, not created** — `M0-10a` (fix the surviving instance,
  0.5 d, P1), `M0-10b` (14 uncalled guards, 1 d, P2), `M0-10c` (null-handling convention
  plus the three dereference-before-null-check guards, 1.5 d, P2), `M0-10d` (29 unguarded
  delete paths — analysis first, 3–5 d, P1), `M0-10e` (three commented-out Cash Flow guards,
  0.5 d, P2). New risks **R-60…R-64**; new questions **Q-60…Q-64**. **All acceptance
  criteria are MET.** Criterion 9 (*"at least one defect verified empirically"*) **is MET**:
  a proving test in `tests/V.SMART.Shared.Tests/Services/MfgPoServiceDeleteGuardTests.cs`
  was run against unmodified `MfgPoService.cs` and observed
  `Actual: Tuple (True, "Item can be safely Cancell.")` where the guard's own message
  promises a refusal (KB-061 §3.1, §7). It is committed **`Skip`-ped**, because an audit may
  not repair the defect, and is `M0-10a`'s acceptance test. *Attempt 1 recorded this
  criterion NOT MET on the false premise that no test project existed on the branch —
  `tests/V.SMART.Shared.Tests/` has existed since `9557de2` (M0-12-01) and the very file
  cited above since `8e3b19d` (M0-09).* Task status remains KB-081's to record, and only the
  owner sets `Completed`.

**Must NOT be parallelised despite appearing unrelated:**
- `M0-14` and `M0-03-01` both edit `V.SMART.Web/appsettings.json`.
- `M0-06` and `M0-13` both risk touching `ApplicationDbContext.cs` seed data.

### Critical Path

```
M0-00 → M0-08 → M0-07 → M0-12-01 → M0-13 → M0-11 ─┐
                                                   ├→ G0
M0-01-01 → M0-01-02 → M0-01-03 ────────────────────┤
M0-04 ─┬→ M0-05 ─────────────────────────────────── ┘
M0-03 ─┘
```

The binding constraint is **M0-01-02** (scripting 94 procedures) if DBA access is slow, and
otherwise **M0-12-01 → M0-13 → M0-11**.

### Expected Deliverables
- Clean working tree, protected `master`, documented branch convention.
- `db/stored-procedures/` with all 94 procedures + a deployment script + a rebuild runbook.
- No secrets in the working tree or in history; rotated credentials; fail-fast startup.
- `.github/workflows/ci.yml` green, with a recorded warning baseline.
- A test project with passing characterisation tests for `CalculationService` and
  `StockManagerService`.
- R-08 fixed; the `CanDelete…Async` audit recorded as INV-025.
- Q-01 answered and recorded; Q-14 answered or explicitly deferred with reason.

### Risks
| Risk | Mitigation |
|---|---|
| No live-database access → M0-01 stalls, blocking G0 | Escalate on day 1; it is the longest-lead item |
| History rewrite breaks other clones/forks of a public repo | Coordinate; assume the secrets are already harvested and rotate regardless |
| Characterisation tests encode a bug as expected behaviour | That is the intent — they pin *current* behaviour; M0-11's change is applied deliberately afterwards |
| 6,695 warnings make analyzer CI noisy | Baseline first, fail only on new warnings |

### Exit Gate — G0
- [~] A fresh, empty SQL Server can be brought to a working tenant database **from source
      control alone** (EF migrations + all 94 procedures) and the app runs against it.
      *(**DEFERRED by the repository owner, 2026-08-19.** No disposable SQL Server instance is
      available. Everything the drill needs already exists and is committed —
      `db/RUNBOOK-rebuild-tenant-database.md`, `db/deploy-stored-procedures.ps1`, and
      `db/REBUILD-DRILL-LOG.md` as an unfilled skeleton — so this is blocked on **hardware, not
      work**. `M0-01-03` stays `Needs Review`. **This is the most consequential of the three
      deferrals:** it is the only evidence that a working tenant database can be reconstructed
      from source control alone. Until it is run, every environment is a snowflake and no one
      knows whether the 94 procedures plus EF migrations actually reconstitute a working system.
      It costs little now and a great deal at **M6**, when a production environment must be
      built from scratch. The same instance would also settle the three behaviours `M0-13` could
      not verify (FIFO tie-break on identical `AddDate`, `RcSubID` null equality, `[Precision]`
      rounding).)*
- [~] No connection string or JWT secret in the working tree **or** in `git grep … HEAD`.
      *(**DEFERRED to the end of the milestone by the repository owner, 2026-08-19.** Depends
      entirely on `M0-05`, which depends entirely on `M0-04`. Not met, and deliberately not
      counted as met — see the **G0 deferral** note below.)*
- [~] Exposed credentials rotated, confirmed by the person with production access.
      *(**DEFERRED to the end of the milestone by the repository owner, 2026-08-19.**
      Production SQL / GST e-Invoice gateway access is not available now; the owner will
      schedule it. `M0-04` stays `Blocked`. **The exposure is live meanwhile** — R-01 records
      live database credentials in a public repository's history, and the KB's own assessment
      is that "the values are compromised regardless". See the **G0 deferral** note below.)*
- [x] Repository visibility deliberately decided and recorded.
      *(**MET.** Public, by deliberate owner decision 2026-08-12, recorded in
      [KB-085](M0-00-baseline-decisions.md#repository-visibility-correction-inv-034) via INV-034
      after an earlier finding was corrected twice. The criterion asks that the choice be
      deliberate and recorded — it is; it does not ask that the repository be private.)*
- [x] CI green on `master`, running on every push, with a recorded warning baseline.
      *(**MET 2026-08-19.** `master` was pushed on the owner's explicit in-conversation
      instruction (`44e3614..20be92f`, 37 commits) — the first time `origin/master` has carried
      `.github/workflows/ci.yml` — and the **run on `master` is green**, owner-confirmed. The
      workflow triggers `on: push: branches: ['**']`, so "on every push" holds. **Two caveats,
      recorded rather than glossed:** (1) `ci/warning-baseline.json` still carries
      `"provisional": true` — per [KB-087](ci-pipeline.md) the runner's numbers supersede local
      ones, so the baseline should be regenerated from this green run's warning total before
      the flag is cleared; (2) **no required status check is attached** — the push reported
      `Bypassed rule violations … Changes must be made through a pull request`, so a
      pull-request rule exists and the owner holds bypass rights, but CI does not gate merges.
      That second half is the part of **Q-20** still open.)*
- [x] `CalculationService` and `StockManagerService` characterisation tests passing in CI.
      *(**MET 2026-08-19.** Both suites exist and now run on a hosted runner: `StockManagerService`
      — 25 tests (M0-13); `CalculationService` — 30 plus 7 for the `CommonConstants` GST rate
      lists (M0-12-02); plus M0-12-01's 11 and M0-09's 6. `master` was pushed on the owner's
      explicit instruction (`44e3614..20be92f`, 37 commits) and the **CI run on `master` is
      green**, owner-confirmed — so all **79** tests are covered by a hosted run, not only a
      workstation. Supersedes the earlier note that no hosted run covered them.)*
- [x] Q-01 answered and recorded in [open-questions.md](../open-questions.md).
      *(**MET 2026-08-19.** The repository owner decided: **preserve but surface** — the API
      reproduces today's allocation exactly, but the shortfall is returned to the caller and
      shown, rather than being silent. Decided against a **pinned** baseline: `M0-13` had
      already fixed the behaviour in green characterisation tests (`S13`–`S16`), so this was a
      choice about measured code, not unpinned code. The criterion asks that Q-01 be *answered
      and recorded* — it is. **Two things it does not close:** `M0-11` still owes `ADR-006`,
      the written brief, which now records an accepted decision rather than proposing options;
      and the **implementation** of surfacing is deferred by the owner until after Milestone 2
      and has no task id yet. Neither is a G0 exit condition.)*

### G0 deferral — criteria 2 and 3, decided by the repository owner 2026-08-19

**Owner decision, in his words:** *"currently we dont have Needs production SQL / GST gateway
access this we will plan it at end completing the milestone"*, and — when told that deferring
those two still leaves G0 short — *"G0 closes with 2 and 3 deferred"*.

**What this means, stated so nobody later reads G0 as fully passed:**

- Criteria **2** (no secrets in tree or history) and **3** (credentials rotated) are **not
  met**. They are marked `[~]`, not `[x]`. `M0-04` and `M0-05` remain `Blocked`.
- G0 may be declared passed for the purpose of **unbarring M2** with those two outstanding.
  This is a scope decision by the owner, not a finding that the criteria were satisfied.
- **The security exposure is live while they wait.** [R-01](../risks/technical-debt-register.md)
  records live database credentials committed to source control, in a repository that is
  **public** by deliberate decision (KB-085/INV-034). The KB's own assessment is that "the
  values are compromised regardless", and `M0-05` cannot fix that alone — purging history from
  a public repository does not retract what is already cloned, forked or cached. **Rotation
  (`M0-04`) is the only actual remedy**, and it is the deferred item.
- The owner was told this before deciding. Recorded here because a deferral whose risk is
  written down is a schedule choice; one whose risk is not is an accident waiting to be
  rediscovered.

**Still genuinely open, and not covered by this deferral:** criterion **1** only — the rebuild
drill (`M0-01-03`), which needs a disposable SQL Server. Criterion **7** closed on 2026-08-19
when the owner answered Q-01 (*preserve but surface*). **One criterion now stands between the
project and G0, and it is the only one that cannot be settled by a decision — it needs
hardware.**

### Definition of Done
All G0 boxes ticked **and** the milestone review recorded per
[KB-084](review-templates.md): planned vs actual duration, variance, outstanding risks,
lessons learned. Tasks being marked Completed is not sufficient.

**Amended 2026-08-19:** criteria 2 and 3 are deferred by owner decision (above) and will not be
ticked at G0. The milestone review must record them as carried debt into M2, with `M0-04`/`M0-05`
still open, rather than silently omitting them.

---

## 8. M1 — Repository Understanding

### Status
**Complete** (gate G1 passed 2026-08-12). 27 KB documents, module inventory and dependency
graph, API readiness assessment, 5 ADRs, 37-item risk register, investigation registry,
18 open questions.

### Remaining Rolling Investigations
Deliberately deferred — extracting all rules now would produce documentation that goes
stale before use. Each runs **one wave ahead** of its migration, as task `01` of that wave:

| INV | Topic | Runs as |
|---|---|---|
| INV-012 | Document numbering + financial-year suffixes | M2-B12-01 (blocks R-12) |
| INV-016 | Costing / labour-cost rules | M3-2-01 |
| INV-013 | Balance-quantity derivation across `Ref*SubId` chains | M3-5-01 |
| ~~INV-028~~ | Row-level scoping via `User.StateCodesCsv` — **complete 2026-08-12**, premise disproved; see Q-08 and R-38 | M2-A08 (done) |
| INV-017 | Route-card operation sequencing and WIP | M4-3-01 |
| INV-015 | e-Invoice / e-Way payload construction | M4-5-01 |
| INV-018 | Subcontract material reconciliation | M4-6-01 |
| INV-019 | Labour DC outgoing rules | M4-7-01 |
| INV-020 | TDS and advance adjustment | M4-8-01 |
| INV-014 | Payroll calculation | M4-9-01 |
| INV-024 | `@code` triage per module | task `02` of every wave |
| INV-026 | Live index inventory vs the EF model | M5-10 |

---

## 9. M2 — Foundation

### Description
Build the API's security and contract foundations, the Angular shell and design-system
primitives, then prove the whole path end-to-end on two modules.

### Objective
Currency **and** Customer Master fully working in Angular through the Web API, with
permissions enforced **server-side**, while Blazor remains untouched and live.

### Scope
Server-side authorization, error contract, refresh tokens, tenant resolution for a
cross-origin SPA, API structural conventions, the Angular app skeleton and its core
primitives, and one vertical slice.

### Out of Scope
Any module beyond Currency and Customer Master. Any business-logic change. Report content
(the *framework* is in scope, the 40 reports are not). Feature-flag rollout (M3-8).

### Prerequisites
**Gate G0 must have passed.** Not negotiable: without stored-procedure DDL there is no
reproducible environment, and without characterisation tests there is no way to prove the
API preserves behaviour.

### Dependencies
- **Technical:** M2-B07 (`AddVSmartDomain()`) is a hard prerequisite for every controller
  — `V.SMART.Api/Program.cs` registers only `ICurrencyService` while `V.SMART.Web` registers
  242 services, so any second controller fails at runtime on DI resolution.
- **Business:** Q-09 (is `/register` still wanted?), Q-13 (feature freeze?).
- **Infrastructure:** a CORS origin list, which needs the deployment topology (Q-16).
- **Testing:** the permission-matrix harness (M2-A03) becomes a merge-blocking CI gate.

### Tasks

#### M2-A — Security and contract *(must land before any new controller)*

| Task | Name | Type | P | Depends on | Est. | File |
|---|---|---|---|---|---|---|
| **M2-A01** | Server-side screen-right authorization *(parent)* | Security | P0 | G0 | 1–2 wks | [↗](tasks/M2-A01.md) |
| M2-A01-01 | — implementation spec from ADR-004 | Architecture | P0 | G0 | 2 d | [↗](tasks/M2-A01-01.md) |
| M2-A01-02 | — implement `[RequireScreen]` / `[RequireRight]` | Security | P0 | M2-A01-01 | 3 d | [↗](tasks/M2-A01-02.md) |
| M2-A01-03 | — per-request rights resolution + caching | Security | P0 | M2-A01-02 | 2 d | [↗](tasks/M2-A01-03.md) |
| **M2-A02** | Apply the filter to `CurrencyController` + denial tests | Security | P0 | M2-A01-03 | 1 d | [↗](tasks/M2-A02.md) |
| **M2-A03** | Automated permission-matrix test harness | Testing | P0 | M2-A02 | 3 d | [↗](tasks/M2-A03.md) |
| **M2-A04** | Refresh tokens + revocation; shorter access tokens | Security | P0 | M2-A01-02 | 3–5 d | [↗](tasks/M2-A04.md) |
| **M2-A05** | Tenant resolution for a cross-origin SPA + real CORS | Security | P0 | M2-A04 | 3–5 d | [↗](tasks/M2-A05.md) |
| **M2-A06** | Exception middleware → `ProblemDetails` + correlation ids | Backend | P0 | G0 | 3–5 d | [↗](tasks/M2-A06.md) |
| **M2-A07** | `GET /api/v1/me` — user, tenant, role, full rights | Backend | P0 | M2-A01-03 | 2 d | [↗](tasks/M2-A07.md) |
| **M2-A08** | Investigate + enforce row-level scoping and account gates | Security | P0 | M2-A01-03 | 3 d | [↗](tasks/M2-A08.md) |

#### M2-B — API structure

| Task | Name | Type | P | Depends on | Est. | File |
|---|---|---|---|---|---|---|
| **M2-B07** | Shared `AddVSmartDomain()` DI extension | Backend | P0 | G0 | 3 d | [↗](tasks/M2-B07.md) |
| **M2-B04** | Decouple `IApprovalService` + 13 `Pages`-referencing files | Backend | P0 | M2-B07 | 1 wk | [↗](tasks/M2-B04.md) |
| **M2-B01** | API versioning → `/api/v1` | Backend | P1 | M2-B07 | 1 d | [↗](tasks/M2-B01.md) |
| **M2-B02** | Server-side paging / sort / filter contract | Backend | P0 | M2-A06 | 1 wk | [↗](tasks/M2-B02.md) |
| **M2-B03** | Codify the controller template + conventions | Documentation | P0 | M2-A02, M2-B02 | 2 d | [↗](tasks/M2-B03.md) |
| **M2-B05** | Typed `ScreenCodes` constants (R-10) | Backend | P1 | M2-B07 | 2 d | [↗](tasks/M2-B05.md) |
| **M2-B06** | File upload / download endpoints | Backend | P1 | M2-A06 | 1 wk | [↗](tasks/M2-B06.md) |
| **M2-B08** | Report + print endpoints (ADR-005) | Backend | P1 | **M2-B07**, M2-A01-03, G0 | 1 wk | [↗](tasks/M2-B08.md) |
| **M2-B09** | Reference-data endpoints + output caching | Backend | P1 | **M2-B07**, M2-B02 | 3 d | [↗](tasks/M2-B09.md) |
| **M2-B10** | OpenAPI polish + TypeScript client generation in CI | DevOps | P0 | M2-B03 | 3 d | [↗](tasks/M2-B10.md) |
| **M2-B11** | Health checks + structured logging (R-23) | DevOps | P2 | M2-A06 | 3 d | [↗](tasks/M2-B11.md) |
| **M2-B12** | Document numbering hardening *(new — parent)* | Backend | P0 | M2-B07 | 1 wk | [↗](tasks/M2-B12.md) |
| M2-B12-01 | — INV-012: numbering + financial-year investigation | Investigation | P0 | M2-B07 | 2 d | [↗](tasks/M2-B12-01.md) |
| M2-B12-02 | — verify unique constraints in a live tenant DB (Q-10) | Database | P0 | M2-B12-01 | 1 d | [↗](tasks/M2-B12-02.md) |
| M2-B12-03 | — race-safe allocation + idempotency keys (R-12) | Backend | P0 | M2-B12-02 | 3 d | [↗](tasks/M2-B12-03.md) |

> **M2-B12 is new.** [KB-071](../migration/milestones.md) scheduled INV-012 into M2 as
> "blocks R-12" but gave it no task. A race-safe numbering allocation must exist before the
> first document-creating endpoint, because an API raises concurrency well above what
> Blazor Server produces today.

#### M2-C — Angular foundation

| Task | Name | Type | P | Depends on | Est. | File |
|---|---|---|---|---|---|---|
| **M2-C01** | Angular CLI + TS strict + lint + test + CI | Frontend | P0 | G0 | 3 d | [↗](tasks/M2-C01.md) |
| **M2-C11** | Archive the Angular pilot | DevOps | P2 | M2-C01 | 0.5 d | [↗](tasks/M2-C11.md) |
| **M2-C10** | Decimal handling — no float money arithmetic | Frontend | P0 | M2-C01 | 2 d | [↗](tasks/M2-C10.md) |
| **M2-C02** | Auth: login, refresh, guards, permission store | Frontend | P0 | M2-C01, M2-A04, M2-A07 | 1 wk | [↗](tasks/M2-C02.md) |
| **M2-C04** | Design-system primitives *(parent)* | Frontend | P0 | M2-C01 | 2 wks | [↗](tasks/M2-C04.md) |
| M2-C04-01 | — design tokens, theme, light/dark | Frontend | P0 | M2-C01 | 3 d | [↗](tasks/M2-C04-01.md) |
| M2-C04-02 | — form controls + validation display | Frontend | P0 | M2-C04-01 | 4 d | [↗](tasks/M2-C04-02.md) |
| M2-C04-03 | — feedback: modal, drawer, toast, states | Frontend | P0 | M2-C04-01 | 3 d | [↗](tasks/M2-C04-03.md) |
| **M2-C03** | App shell: header, permission-filtered sidebar, ⌘K | Frontend | P0 | M2-C02, M2-C04-01 | 1.5 wks | [↗](tasks/M2-C03.md) |
| **M2-C05** | `DataGrid` *(parent)* | Frontend | P0 | M2-C04-02, M2-B02 | 1.5 wks | [↗](tasks/M2-C05.md) |
| M2-C05-01 | — server-paged table core | Frontend | P0 | M2-C04-02, M2-B02 | 4 d | [↗](tasks/M2-C05-01.md) |
| M2-C05-02 | — column preferences + persistence | Frontend | P1 | M2-C05-01 | 3 d | [↗](tasks/M2-C05-02.md) |
| M2-C05-03 | — empty / loading / error states + export | Frontend | P1 | M2-C05-01 | 2 d | [↗](tasks/M2-C05-03.md) |
| **M2-C06** | `RecordPickerDialog` (`DetailsModal` replacement) | Frontend | P0 | M2-C05-01 | 1 wk | [↗](tasks/M2-C06.md) |
| **M2-C07** | `LineItemGrid` — keyboard-first editable grid | Frontend | P0 | M2-C05-01, M2-C10 | 2 wks | [↗](tasks/M2-C07.md) |
| **M2-C08** | `DocumentEditor` shell *(parent)* | Frontend | P0 | M2-C07 | 2 wks | [↗](tasks/M2-C08.md) |
| M2-C08-01 | — layout: header + lines + totals + command bar | Frontend | P0 | M2-C07 | 4 d | [↗](tasks/M2-C08-01.md) |
| M2-C08-02 | — server-authoritative totals wiring | Frontend | P0 | M2-C08-01 | 3 d | [↗](tasks/M2-C08-02.md) |
| M2-C08-03 | — workflow command pattern (`POST /{id}/{verb}`) | Frontend | P0 | M2-C08-01 | 3 d | [↗](tasks/M2-C08-03.md) |
| **M2-C09** | `ReportPage` framework | Frontend | P1 | M2-C05-01, M2-B08 | 1 wk | [↗](tasks/M2-C09.md) |

#### M2-D — Vertical slice

| Task | Name | Type | P | Depends on | Est. | File |
|---|---|---|---|---|---|---|
| **M2-D01** | Currency end-to-end in Angular | Frontend | P0 | M2-C05-03, M2-A02, M2-B10 | 3 d | [↗](tasks/M2-D01.md) |
| **M2-D02** | Customer Master *(parent)* | Migration | P0 | M2-D01 | 1.5 wks | [↗](tasks/M2-D02.md) |
| M2-D02-01 | — `@code` triage + business-logic extraction | Backend | P0 | M2-D01 | 4 d | [↗](tasks/M2-D02-01.md) |
| M2-D02-02 | — `CustomersController` + API tests | Backend | P0 | M2-D02-01 | 3 d | [↗](tasks/M2-D02-02.md) |
| M2-D02-03 | — Angular list + editor screens + component tests | Frontend | P0 | M2-D02-02 | 4 d | [↗](tasks/M2-D02-03.md) |
| **M2-D03** | Blazor ↔ Angular parity test for Customer Master | Testing | P0 | M2-D02-03 | 3 d | [↗](tasks/M2-D03.md) |

### Parallel Work

**Parallel streams** — M2-A/M2-B (backend) and M2-C (frontend) are genuinely independent
until M2-C02, which needs `GET /api/v1/me` (M2-A07) and refresh tokens (M2-A04). Run them
as two teams; M2-C01, M2-C04-*, M2-C10 and M2-C11 need no backend at all.

**Must remain sequential:**
- `M2-A01-01 → -02 → -03 → M2-A02 → M2-A03`. One security surface, built once.
- `M2-B07` before every controller task. It is the DI precondition.
- `M2-A06 → M2-B02 → M2-B03 → M2-B10`. Error contract → paging contract → template →
  generated client. Generating a client from an unsettled contract wastes the whole chain.
- `M2-C05-01 → M2-C06 / M2-C07 → M2-C08-*`. The grid underpins the picker, the line grid
  and the document editor.
- `M2-D01 → M2-D02-01 → -02 → -03 → M2-D03`.

**Must NOT be parallelised despite appearing unrelated:**
- `M2-A05` and `M2-B01` both rewrite routing/middleware in `V.SMART.Api/Program.cs`.
- `M2-A04` and `M2-A05` both change the token shape and `JwtTokenService`.
- `M2-B01` (versioning) touches every controller route — land it while there are two
  controllers, not sixty.
- `M2-C03` and `M2-C02` share the permission store.

### Critical Path

```
G0 → M2-B07 → M2-A01-01 → M2-A01-02 → M2-A01-03 → M2-A02 → M2-A03
                                                      │
     M2-A06 → M2-B02 → M2-B03 → M2-B10 ───────────────┤
                                                      ▼
   M2-C01 → M2-C04-01 → M2-C04-02 → M2-C05-01 → M2-C05-03 → M2-D01
                                                              │
                                    M2-D02-01 → -02 → -03 → M2-D03 → G2
```

`M2-C07`/`M2-C08` (line-item grid, document editor) are **not** on the M2 critical path —
Customer Master is a master, not a document — but they are on **M3's** critical path and
must complete inside M2 or M3-5 stalls.

### Expected Deliverables
- Every endpoint authorized server-side; a permission-matrix harness blocking merges.
- One documented controller template; `ProblemDetails` everywhere; `/api/v1`.
- A generated TypeScript client, produced in CI, never hand-written.
- An Angular app with shell, auth, design-system primitives, `DataGrid`, `RecordPickerDialog`,
  `LineItemGrid`, `DocumentEditor` shell, `ReportPage` framework.
- Currency and Customer Master live in Angular; Blazor untouched.
- A passing parity test.

### Risks
| Risk | Mitigation |
|---|---|
| The authorization filter is designed against `Screens`/`UserRight` semantics that turn out to be richer than documented | M2-A01-01 is a spec task producing an ADR-004 implementation note before any code |
| ~~Q-08 row-level scoping is real → every list endpoint leaks data~~ **Resolved 2026-08-12: it does not.** `StateCodesCsv` scopes `Leads` only, in the service layer | Replaced by two real risks: scoping is **opt-in** (`GetAllLeadsAsync` returns everything unscoped on the same interface), and **R-38** — trial, device and QR gates live only in Blazor `@code` while `AuthController.cs:39-59` skips all three. M2-A08 stays P0, gated before M2-D01 |
| The contract chain (A06→B02→B03→B10) settles late, forcing frontend rework | Freeze the contract at M2-B03; treat later changes as breaking and version them |
| `DocumentEditor` under-specified because no document module has migrated yet | Accept: build the shell in M2, harden it in M3-5 against a real document |

### Exit Gate — G2
- [ ] Currency **and** Customer Master fully working in Angular: login, tenant resolution,
      permission-gated CRUD, server paging, validation, error contract, Excel export.
- [ ] The Blazor app is untouched and still live against the same database.
- [ ] A user with no rights on a screen is refused by the **API**, not just the UI —
      proven by the permission-matrix harness in CI.
- [ ] The TypeScript client is generated from OpenAPI in CI.
- [ ] Parity test M2-D03 passes.
- [ ] Controller template and error contract documented and adopted.

### Definition of Done
G2 ticked, milestone review recorded, and the controller template demonstrably followed by
both existing controllers — the template is only real once it has two independent users.

---

## 10. Module Migration Task Pattern

Every M3/M4 wave instantiates this 14-step pattern. `<W>` is the wave id (e.g. `M3-5`).

| Step | Task | Type | Typical est. |
|---|---|---|---|
| `<W>-01` | Business-rule investigation (the wave's `INV-0xx`) | Investigation | 2–4 d |
| `<W>-02` | `@code` triage — presentation / data / business (INV-024) | Investigation | 2–3 d |
| `<W>-03` | Extract business logic into services | Backend | 3 d – 3 wks |
| `<W>-04` | Verify extracted services against the running Blazor app | Testing | 2–3 d |
| `<W>-05` | API contract definition | Architecture | 1–2 d |
| `<W>-06` | Controller implementation | Backend | 2–5 d |
| `<W>-07` | API integration + permission tests | Testing | 2–3 d |
| `<W>-08` | Angular screens | Frontend | 4 d – 2 wks |
| `<W>-09` | Component tests | Testing | 1–2 d |
| `<W>-10` | E2E critical path | Testing | 1–2 d |
| `<W>-11` | Blazor ↔ Angular parity test | Testing | 2–3 d |
| `<W>-12` | Feature flag | DevOps | 0.5 d |
| `<W>-13` | Pilot-tenant validation | Migration | 2–3 d |
| `<W>-14` | KB + investigation-registry update | Documentation | 1 d |

Steps 03 and 08 split into child tasks per screen or per service when the module is large;
`M4-7` (Labour Work — a 6,112-LOC service and a 6,528-LOC page) will need several of each.
Combine steps only where the module is genuinely small — never to reduce the task count.

**Ordering rule:** step 03 must complete and step 04 must pass **before** step 08 starts.
Building the Angular screen against un-extracted logic is how ERP behaviour gets silently
reimplemented in TypeScript, which principle 3 forbids.

---

## 11. M3 — Core Modules

### Description
Migrate along the dependency graph: masters first, then the sales pipeline through Sales
Order, plus approvals, reports and the dashboard.

### Objective
A pilot tenant runs masters, the sales pipeline through Sales Order, approvals and core
reports entirely in Angular, with Blazor available as a per-module fallback.

### Scope / Out of Scope
In: waves 3.1–3.7 below, feature-flag infrastructure, M4 re-baselining.
Out: every M4 module; the remaining ~30 reports; decommissioning any Blazor route.

### Prerequisites
Gate G2. Additionally `M2-C07` and `M2-C08` must be complete before wave `M3-5`.

### Waves

| Wave | Modules | Tasks | Est. |
|---|---|---|---|
| **M3-1** | Masters — Accounts, General: **11 masters / 18 Razor files**¹ — Vendor, Machine, Terms & Conditions, State, Expense, Income, Bank, Currency², Currency Today, Project Type, Cost Centre | M3-1-01 … -14 | 3 wks |
| **M3-2** | Masters — Inventory (Item, BOM, BOM Labour, Process, Store, HSN, Raw Material) | M3-2-01 … -14 | 4 wks |
| **M3-3** | Masters — Admin & Settings (Users, permission matrix, Screens, General/Print/Company) | M3-3-01 … -14 | 2 wks |
| **M3-4** | Approvals inbox | M3-4-01 … -14 | 1.5 wks |
| **M3-5** | Sales: Leads → Enquiry → Feasibility → Quotation → **Sales Order** | M3-5-01 … -14 | 4 wks |
| **M3-6** | Report framework + first 10 reports *(parallel)* | M3-6-01 … -06 | 2 wks |
| **M3-7** | Dashboard | M3-7-01 … -08 | 1.5 wks |
| **M3-8** | Feature-flag infrastructure (per tenant, per module) | M3-8-01 … -03 | 1 wk |
| **M3-9** | Re-baseline M4 from measured M3-5 extraction cost | M3-9-01 | 2 d |

¹ **Membership verified 2026-08-12** against `NavMenu.razor:180-188` ("General Master") and
`:190-197` ("Account Master") plus the service folders — an earlier draft of this table
listed 5 of the 11. All pages are under `Pages/Master_Module_pages/`; measured **10,295 LOC,
5,688 inside `@code` (55.2%)**, consistent with R-06. Customer Master is excluded — already
migrated by M2-D02. Three screens that appear in those nav groups are **not** in this wave:
Contract Review Master (its files live under `SalesService`/`SalesAndLabour_pages` → M3-5),
Rejection Master (`SettingsService` → M3-3), and Master Upload (a cross-cutting import
utility with no service). `/myCompany` → M3-3, where KB-053 and KB-020 disagree.

² **Currency is in scope for rule extraction but out of scope for rebuild** — M2-D01 already
ships it in Angular. Its 134-line `@code` block was never triaged and no `BR-CURR-*` rule
exists, so M3-1-01 must still cover it.

**Convention deviations found in this wave** (they generalise, so expect them elsewhere):
`ITermsAndConditionsService` and `IBankService` take and return **EF entities, not
ViewModels**, and `BankVM`/`StateVM`/`CurrencyTodayVM`/`ProjectTypeMasterVM` **do not exist**
— see the caveat in [KB-041](../api/api-readiness-assessment.md#caveat-the-viewmodel-boundary-is-not-universal).
**13 of the 18 pages inject `IUnitOfWork` directly** rather than going through a service,
which raises this wave's extraction cost above a pure-master baseline. Four in-scope screens
(Bank, State, Currency Today, Project Type) have **no delete guard at all**.

### Why M3 prompts are not written yet

**This is deliberate, and it is required by the project's own rules.** A task prompt's
*Business Rules to Preserve* section is the output of that wave's `<W>-01` investigation.
For M3-2 that is INV-016; for M3-5 it is INV-013. Those investigations have not run
([KB-003](../investigation-registry.md) lists them as *Scheduled*). Writing the prompts now
would mean inventing business rules and file paths — violating
[KB-002](../source-of-truth-rules.md) and the instruction never to present inference as
fact.

**The rule:** at the start of each wave, generate that wave's 14 task files from
[KB-083](prompt-template.md), using the completed `<W>-01` and `<W>-02` outputs as the
source for *Current Implementation*, *Business Rules to Preserve*, and *Relevant Files*.
Only `<W>-01` can be written in advance, because its input is the current repository — see
[tasks/M3-1-01.md](tasks/M3-1-01.md), the worked exemplar.

### Parallel Work
- `M3-6` (reports) is read-only and parallelises with every other wave.
- `M3-8` (feature flags) is independent and must finish before any `<W>-12`.
- Within a wave, `-01`/`-02` (investigation) can overlap the previous wave's `-08`…`-14`.
- `M3-1` and `M3-2` **cannot** overlap: Item and BOM master screens depend on
  Accounts/General master pickers.

### Critical Path
```
G2 → M3-1 → M3-2 → M3-3 → M3-5 → M3-9 → G3
                      └→ M3-4 (parallel after M3-3)
     M3-6, M3-7, M3-8 run parallel throughout
```

### Exit Gate — G3
- [ ] Pilot tenant operating masters, sales pipeline through Sales Order, approvals and
      core reports in Angular, in production, Blazor available as fallback.
- [ ] Permission matrix administered from the Angular app itself (M3-3).
- [ ] Parity tests green for every wave.
- [ ] M4 estimates re-baselined against actual M3-5 extraction effort (M3-9).
- [ ] Zero fallbacks to Blazor for migrated modules over the milestone's final two weeks.

### Definition of Done
G3 ticked, milestone review recorded, and **M4's estimates updated in
[KB-081](task-tracker.md)** — M3 is not done while M4 still carries provisional numbers.

---

## 12. M4 — Advanced Modules

### Description
Full functional parity across the remaining modules. **Every estimate here is provisional
until M3-9.**

### Objective
Every module in [KB-020](../modules/module-inventory.md) available in Angular; all 440 legacy
routes mapped or explicitly retired.

### Waves

| Wave | Modules | Tasks | Est. (provisional) |
|---|---|---|---|
| **M4-2** | Inventory / Stock — Issue Request, MIN, STN, Inter-Store, Tool Crib | M4-2-01 … -14 | 4 wks |
| **M4-1** | Out Sourcing + Purchase — Requisition → … → GRN → SCN → Invoice → Debit Note | M4-1-01 … -14 | 5 wks |
| **M4-3** | Planning — Job Order, Route Card, RC Release, Estimation | M4-3-01 … -14 | 4 wks |
| **M4-4** | Production + shop-floor Production Log UI | M4-4-01 … -14 | 4 wks |
| **M4-5** | Manufacturing Work + e-Invoice / e-Way Bill | M4-5-01 … -14 | 4 wks |
| **M4-6** | Sub Contract | M4-6-01 … -14 | 3 wks |
| **M4-7** | Labour Work — **largest single item** | M4-7-01 … -14 | 4 wks |
| **M4-8** | Accounts / Cash Flow | M4-8-01 … -14 | 3 wks |
| **M4-9** | HR — Leave, Attendance, Payroll, Staff Loan | M4-9-01 … -14 | 3 wks |
| **M4-10** | Inspection / QC, Maintenance, Utilities | M4-10-01 … -14 | 2 wks |
| **M4-11** | Remaining ~30 reports *(parallel)* | M4-11-01 … -08 | 2 wks |

> **Sequencing change from KB-070, with reason.** KB-070 lists Purchase (4.1) before
> Inventory (4.2), while noting that "SCN writes stock — must follow `IStockManagerService`
> hardening". Those two statements conflict. **M4-2 (Inventory) is therefore scheduled
> first**, so that stock hardening — including whatever M0-11 decides about R-07 — lands
> before the first module that writes stock through the API. Wave *ids* are unchanged so
> that no existing reference breaks; only execution order moves. Recorded here rather than
> silently applied.

### Prerequisites
Gate G3, and M0-11's decision applied to `StockManagerService` before M4-2-03.

### Parallel Work
`M4-11` (reports) parallelises throughout. `M4-9` (HR) and `M4-10` (QC/Maintenance) are
weakly coupled to the manufacturing spine and can run alongside `M4-5`…`M4-8` with a
separate pair. `M4-1` must follow `M4-2`. `M4-5` must follow `M4-4`.

### Critical Path
```
G3 → M4-2 → M4-1 → M4-3 → M4-4 → M4-5 → M4-6 → M4-7 → G4
     (M4-8, M4-9, M4-10, M4-11 parallel)
```

### Exit Gate — G4
- [ ] Every module in [KB-020](../modules/module-inventory.md) available in Angular.
- [ ] All 440 legacy routes mapped to an Angular route or explicitly retired with a recorded
      reason ([KB-053](../frontend-new/page-map.md)).
- [ ] e-Invoice and e-Way Bill verified against the gateway sandbox **and** one live
      document per tenant.
- [ ] Payroll parity verified across a full pay cycle.
- [ ] Document-numbering race (R-12) closed; idempotency keys on create endpoints.

### Definition of Done
G4 ticked and milestone review recorded. Any route retired without an Angular replacement
carries a written, product-owner-approved reason.

---

## 13. M5 — Hardening

### Description
Testing is continuous from M2. Only the final sweep is a discrete block. Listing it as a
terminal phase would misrepresent when the work happens.

| Task | Activity | When | File |
|---|---|---|---|
| M5-01 | Unit tests for every extracted business rule | with each `<W>-03` | — |
| M5-02 | API integration tests per controller | with each `<W>-06` | — |
| M5-03 | Component tests for design-system primitives | M2 | — |
| M5-04 | E2E per module critical path | with each `<W>-10` | — |
| M5-05 | Permission-matrix testing — **merge-blocking CI gate** | M2 onward | [↗](tasks/M2-A03.md) |
| M5-06 | Parity testing per module | with each `<W>-11` | — |
| M5-07 | Performance: 10k-row grids, 200-line documents, concurrent creates | M5 | generated at M5 |
| M5-08 | Security: tenant isolation, IDOR on `{id}` routes, JWT, XSS | M5 | generated at M5 |
| M5-09 | Accessibility: axe in CI + manual keyboard pass | continuous | generated at M5 |
| M5-10 | Load test on a production-sized tenant; live index review (INV-026) | M5 | generated at M5 |

M5-01…M5-06 have **no standalone task files by design** — they are steps inside the module
pattern (§10). Giving them separate files would let a wave ship untested and defer its tests
to a phase that arrives months later.

M5-07…M5-10 prompts are generated at M5 start, for the same reason as §11: their inputs are
the system as it exists after M4, plus production topology (Q-16) and tenant volumes (Q-12),
none of which are known now.

### Exit Gate — G5
- [ ] Permission-matrix and parity suites green in CI, blocking merge.
- [ ] Pen-test findings closed, or accepted in writing by a named owner.
- [ ] Performance targets met against production-sized data.
- [ ] No axe-critical violations.

---

## 14. M6 — Production Migration

| Task | Step | Depends on |
|---|---|---|
| M6-01 | Deployment topology; resolve Q-16 | G4 |
| M6-02 | Monitoring: structured logs, APM, error tracking, per-tenant dashboards | M6-01 |
| M6-03 | Staged rollout by per-tenant/per-module flag, smallest tenant first (Q-12) | M6-02 |
| M6-04 | Rollback drill in production | M6-03 |
| M6-05 | User migration — no credential migration; training; side-by-side | M6-03 |
| M6-06 | EF migration rollout procedure per tenant (Q-02) | M6-01 |
| M6-07 | Decommission Blazor routes module by module | M6-04, ≥1 financial period |
| M6-08 | Decide and execute the MAUI app's future (Q-11) | M6-07 |

M6 prompts are generated at M6 start. Every one of these tasks is parameterised by answers
that do not exist yet — deployment topology (Q-16), the production tenant list (Q-12), and
the per-tenant EF rollout procedure (Q-02).

### Exit Gate — G6
- [ ] All tenants on Angular for all modules.
- [ ] One full financial period with zero module-level fallbacks.
- [ ] Rollback drill executed successfully at least once in production.
- [ ] Blazor routes retired; the decommissioning decision recorded as a new ADR.

---

## 15. Complete Task Dependency Graph
See [KB-082](dependency-graph.md).

## 16. Critical Path
See [KB-082 §Critical Path](dependency-graph.md#project-critical-path). In summary:

```
M0-00 → M0-08 → M0-07 → M0-12-01 → M0-13 → M0-11 → G0
      → M2-B07 → M2-A01-* → M2-A02 → M2-A03
      → M2-C01 → M2-C04-01 → M2-C05-01 → M2-D01 → M2-D02-* → M2-D03 → G2
      → M3-1 → M3-2 → M3-3 → M3-5 → M3-9 → G3
      → M4-2 → M4-1 → M4-3 → M4-4 → M4-5 → M4-6 → M4-7 → G4
      → M5 sweep → G5 → M6-01 → M6-03 → M6-04 → M6-07 → G6
```

## 17. Parallel Execution Plan
See [KB-082 §Parallel Execution](dependency-graph.md#parallel-execution-plan).

## 18. Master Progress Tracker
See [KB-081](task-tracker.md) for the status of every task, and [KB-089](current-task.md) for
the one that is active now.

Lifecycle: `PLANNED → READY → IN_PROGRESS → IMPLEMENTATION → TESTING → REVIEW → COMPLETED`,
with `BLOCKED` as an orthogonal flag ([KB-088 §1](workflow.md#1-task-lifecycle)). KB-081's
tables use the equivalent legacy names — `Not Started` · `Ready` · `Blocked` · `In Progress` ·
`Needs Review` · `Completed` — and the mapping is at the top of that document.

A task is never `COMPLETED` merely because the code was written. Completed tasks are never
deleted.

## 19. Git Strategy

The repository root is `NexGen-ERP---2025-master/` (the parent directory is not part of the
project). Remote: `https://github.com/ErpStore/NexERP_B.git`. Default branch `master`.

| Rule | |
|---|---|
| Branch per task | `migration/<TASK-ID>-<slug>` — e.g. `migration/M2-C05-01-datagrid-core` |
| Commit subject | `<TASK-ID>: <imperative summary>` — e.g. `M2-C05-01: Implement server-paged DataGrid core` |
| Scope | One task per branch. Never mix two task ids in one branch. |
| Merge | Never merge or push to `master` from an execution session. Leave the branch for review. |
| Rollback | Revert the branch's merge commit. Each task is independently reversible **because** it is single-scope. |
| Protection | `master` protected, requiring one review. **The required *CI* status check is still outstanding.** M0-00 protected `master` but deliberately added no required check because no CI existed; M0-07 (2026-08-17) created the check — job name `Restore, build and gate analyzer warnings` in `.github/workflows/ci.yml` — but an execution session has no GitHub admin rights and cannot push, so it has never run and is not yet required. **Action for a human with admin rights:** after the first green run on `master`, add that check to `master`'s branch protection. Rollback ordering matters — remove the required check *before* ever deleting the workflow, or `master` becomes unmergeable. See [KB-087](ci-pipeline.md) §8. |

**Precondition (M0-00): resolved 2026-08-12.** The working tree previously had 37
uncommitted entries against a single-commit history. M0-00 gave each entry a human-decided
disposition on `migration/M0-00-vcs-baseline`: 7 groups committed (G1, G4, G5, G6, G7, G8,
G9), 2 groups deferred because they carry live credentials (G2 to M0-03-01, G3 to M0-03/
M0-04 — see [KB-085](M0-00-baseline-decisions.md) for the full per-group log). A
branch-per-task workflow can now start from `master` once this branch is reviewed and
merged. `pre-M0-00-baseline` tags the original single commit (`c12c5b2`) for rollback.

Every task file states its branch name, commit subject, expected changed files, and
rollback approach.

## 20. Milestone Review Template
See [KB-084 §Milestone Review](review-templates.md#milestone-review).

## 21. Task Completion / Handoff Template
See [KB-084 §Task Handoff](review-templates.md#task-handoff).

## 22. Definition of Done

**A task is Done when:** its acceptance criteria are objectively met; verification commands
pass; the required tests exist and pass; documentation and (if applicable) the investigation
registry are updated; the diff has been reviewed; and it is committed on its own branch.

**A milestone is Done when:** every task is Done, **its exit gate passes**, and the
milestone review is recorded. Tasks all being marked Completed is explicitly *not*
sufficient — the gate is the completion authority.

**The project is Done when:** G6 passes.

## 23. Open Questions / Decisions

Blocking questions from [KB-004](../open-questions.md), with the task that consumes each:

| Q | Question | Needed by | Owner |
|---|---|---|---|
| **Q-19** *(new)* | Is the public visibility of `ErpStore/NexERP_B` intended? | **M0-04, immediately** | repo owner |
| Q-01 | Is the silent stock under-issue a bug or relied-upon? | M0-11 | product owner |
| Q-14 | Do stored procedures differ between tenant databases? | M0-02 | DBA |
| Q-10 | Do document-number columns carry unique constraints? | M2-B12-02 | DBA |
| ~~Q-05…Q-08~~ | **ANSWERED 2026-08-12** — see R-38. All four gates turned out to live in Blazor `@code`, and the API bypasses them. Q-08's premise was wrong in both directions: `StateCodesCsv` scopes **Leads only**, in the service layer, and **opt-in**. What remains is a *product decision per gate*, not an investigation | M2-A08 | product owner |
| Q-16 | Deployment topology and TLS termination? | M2-A05, M6-01 | ops |
| Q-09 | Is `/register` still wanted? | M2-C02 | product owner |
| Q-13 | Is there a feature freeze on Blazor during migration? | M3-1 | product owner |
| Q-12 | Which tenants are in production, and their data volumes? | M6-03 | ops |
| Q-02 | How are EF migrations rolled out per tenant? | M6-06 | ops |
| Q-11 | What is the MAUI app's future? | M6-08 | product owner |

**Decisions taken in this plan** (recorded, not silently applied):
1. M0-00 and M0-15 added from INV-029 evidence.
2. M2-B12 added — INV-012 was scheduled into M2 with no task.
3. M4 execution order changed so Inventory precedes Purchase, resolving an internal
   contradiction in KB-070. Wave ids unchanged.
4. M3/M4 task prompts are generated at wave start, not now — see §11.

## 24. Recommended First Task

**M0-00 — Establish a clean version-control baseline.** ([tasks/M0-00.md](tasks/M0-00.md))

It is the only task with no prerequisites that unblocks the rest of M0, it requires no
external access, and every other task assumes a branch-per-task workflow that cannot exist
on a tree with 37 uncommitted changes.

**Run M0-04 (rotate the exposed credentials) in parallel, starting immediately.** It is an
ops action outside the repository, it blocks nothing, and the credentials are currently
published on a public GitHub repository. Treat them as already compromised.
