---
doc_id: KB-089
title: Current Task
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-21
dependencies: [KB-081, KB-082, KB-088, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## ▶ No active task — the candidate pool is genuinely empty

**`M2-B11` closed `Needs Review` 2026-08-21** (branch `migration/M2-B11-health-checks-logging`,
tip `12dad11`; validated `PASS` on attempt 2 of 4, 0 escalations, independently re-derived).
Nothing in the dependency graph names `M2-B11` as a prerequisite, so its close **releases no
other task**. Select ran afterward and found no dependency-ready candidate — see *Why every
`Ready`/`Blocked` task is excluded* below. This is a clean [KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)
stop, not a failure: the constraint is the merge queue, not execution capacity.

**The one thing that would change this: an owner merging any of the branches in the table
below.** Each merge is very likely to release at least one further task (M2-B01-shaped rows
release M2-B09; M2-A01-03-shaped rows release M2-A02/M2-A07/M2-A08's dependents; M2-C00 releases
the whole M2-C tree). Re-run Select the moment any of them lands.

---

## `M2-B11` — closed 2026-08-21, `Needs Review`

Health checks (`GET /health/live`, `GET /health/ready`) covering the master DB and a configurable
tenant subset, plus a new `ILogger`-based `StructuredLoggingService` implementing the unchanged
`ILoggingService` contract with named audit properties and a `TenantInfo` redaction policy — R-23.
`FileLoggingService` is kept as the Blazor/MAUI registration; the new sink is wired only in
`V.SMART.Api`. Attempt 1 failed the CI warning-gate ratchet (`CS8767` in the new
`TenantInfoDestructuringPolicy`, one over the 6693 baseline); attempt 2 fixed it (annotation
only, `81ad961`) and passed. R-23 is marked **resolved for `V.SMART.Api` only** — still open for
the Blazor and MAUI hosts. Two criteria stated as unmet rather than glossed: the
tenant-unreachable-while-master-healthy 503 was proved only at unit level (writing a bogus
`Tenants` row was out of scope), and no `LogUserAction` event was observed over HTTP (no call
site reachable from `V.SMART.Api` — adding one is out of scope). Full record:
[`tasks/M2-B11.md` § Execution Record](tasks/M2-B11.md#execution-record-2026-08-21-branch-migrationm2-b11-health-checks-logging)
and its close-out addendum; tracker footnote ³⁶; [`runner-state.md`](runner-state.md).

**Produced:** [KB-113](../architecture/observability.md) (health-check contract, audit-event
schema, sink deferral pending Q-16, retention policy, redaction policy); **INV-046** (494
`LogUserAction` call sites, all in `V.SMART.Shared`, zero in `V.SMART.Api` or `V.SMART.Web`; the
`#if ANDROID || WINDOWS || MACCATALYST` `_basePath` branch confirmed dead on both TFMs).

---

## The merge queue — nine branches unmerged (census, kept current)

Nothing below is missing a prerequisite; nothing is being re-derived. Until these land on
`master`, [selection rule](dependency-graph.md#ready-task-selection-rule) step 1 keeps every one
of their dependents `Blocked`, because a prerequisite that is `Needs Review` is not `Completed`.
**Never merge or push from an execution session** ([`CLAUDE.md`](../../../CLAUDE.md) § Standing
constraints) — this table is for the owner to act on.

| Task | Branch | Tip | State on that branch |
|---|---|---|---|
| `M2-B11` | `migration/M2-B11-health-checks-logging` | `12dad11` | `Needs Review`, validated `PASS` (attempt 2 of 4) |
| `M2-B04` | `migration/M2-B04-decouple-pages-references` | `5ca1c10` | `Needs Review`, validated `PASS` (attempt 2) |
| `M2-A08` | `migration/M2-A08-row-scope-and-account-gates` | `bca92fd` | `Needs Review`, validated `PASS` |
| `M2-A08` ⚠ | `migration/M2-A08-row-level-scoping` | `6e6633a` | **A second branch for the same task** (INV-028 → KB-120, answering Q-05…Q-08). **Needs an owner decision on which branch is the real `M2-A08`.** |
| `M2-A07` | `migration/M2-A07-me-endpoint` | `e3bc96c` | `Needs Review`, validated `PASS` |
| `M2-C00` | `migration/M2-C00-kb050-angular-rewrite` | `b3c0e6e` | `Needs Review`, validated `PASS` — releases the whole `M2-C` tree |
| `M2-B09` | `migration/M2-B09-reference-endpoints` | `d1175db` | `Needs Review` — six cached reference endpoints, R-15 boundary fix |
| `M2-B06` | `migration/M2-B06-file-endpoints` | (merged) | **`Completed`, merged to `master` `65d9666`** — no longer in this queue |
| `M0-10` | `migration/M0-10-candelete-guard-audit` | `fc8e0c0` | `Needs Review` after attempt 3, regression repaired |
| `M0-01-03` | `migration/M0-01-03-rebuild-drill` | `34b5e32` | `Needs Review` — drill §§2–6 executed and passing; §7 and a named operator outstanding |
| `M2-B12-01` | `migration/M2-B12-01-inv-012-numbering` | `407d0ba` | 🚩 **`Blocked` — escalation budget exhausted, awaiting Vivek.** Not `PASS`. |

**Note `M2-B01` has already merged** (`ae9d2c8`, `--no-ff`, 2026-08-21) and is `Completed` — it
is why `M2-B06`, `M2-B09` and `M2-B11` became selectable this run.

---

## Why every `Ready`/`Blocked` task is excluded right now

| Task | Why not |
|---|---|
| `M0-06` | `Ready`, P1, but **already has a branch** (`migration/M0-06-remove-default-admin`) — five-part test part 5 excludes it. |
| `M0-11` | A **`Product Decision`** (Q-01, silent FIFO under-issue) — never self-selectable; surfacing it to the owner *is* the action. |
| `M2-A02` | `Ready`, P0, but gated on **two things**: the unanswered **Q-28** (an API-only administrator holds zero `UserRight` rows because `AuthController.Login` never calls `SyncRightsForUserAsync`), and **R-65** (two phantom screen names — `Bill Paid List`, `Bill Pending List` — pass `ScreenRightStartupValidator` but would deny every request forever, silently, if either were annotated). Owner: **Vivek** for both. |
| `M2-B05` | `Blocked` — its premise (magic `screenCode` literals) was falsified by INV-044; the real defect is 55 bare `storeId` literals (R-66). Needs owner re-specification onto R-66, not a retry. |
| `M2-C01` | `Blocked` behind `M2-C00`'s merge. |
| `M2-B04`, `M2-A08`, `M2-C00`, `M2-A07`, `M2-B09`, `M0-10`, `M0-01-03` | Done and unmerged — see the merge-queue table above. |
| `M2-B12-01` | `Blocked`, not done — escalation budget exhausted, named owner **Vivek**; the escalated fix at `8a54f96` has never been re-validated. |

---

## Standing blockers worth reading before picking anything up

- **R-65** (`ScreenCatalogue.cs` — two phantom screen names) blocks `M2-A02`. Owner **Vivek**.
  [`technical-debt-register.md`](../risks/technical-debt-register.md).
- **Q-28** (API-only administrators hold zero `UserRight` rows) also blocks `M2-A02`.
  [`open-questions.md`](../open-questions.md).
- **`M0-01-03`** needs a **named operator** to run runbook §7 (start `V.SMART.Web`, log in, run
  one report, print one document) and sign the drill log — an accountability requirement, not a
  technical one. The two throwaway drill databases are left in place for this. See
  [`tasks/M0-01-03.md`](tasks/M0-01-03.md).
- **`M2-A08`** has two competing branches; the owner needs to pick one before either merges.
- **Three sibling worktrees may still be live** (`wt-M0-10`, `wt-M2-A08`, `wt-M2-B01`) —
  `git worktree list` belongs in Select alongside the tracker, which cannot see them.

## Also true right now

- **R-67** — `SaveCorresFileAsync` (`WebFileUploadService.cs:100-104`) writes a zero-byte file
  and reports success; every Blazor correspondence/drawing upload has been landing empty. Found
  by M2-B06, deliberately left unfixed (out of scope), survivable only because
  `Correspondence.Image` holds a second copy.
- **Q-16** now has a storage half (M2-B06) and an observability half (M2-B11): uploaded files
  and the log sink both currently live on local disk/filesystem with no durability guarantee
  under an unknown deployment topology.
