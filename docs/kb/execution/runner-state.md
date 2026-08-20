---
doc_id: KB-093
title: Autonomous Runner State
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-20
dependencies: [KB-089, KB-091, KB-092, KB-081]
---

# Autonomous Runner State

**Machine-owned. Small by design.** This is the runner's own control state — whether a run is
live, what it is on, how many attempts it has spent, and why it stopped. It is written at
every state transition by `.claude/workflows/migration-runner.js`, so a run that is killed
mid-task resumes from this file rather than from a conversation.

It is **not** a status report. Those live elsewhere and this file never duplicates them:

| Question | Read instead |
|---|---|
| What is every task's status? | [`task-tracker.md`](task-tracker.md) (KB-081) — the authority |
| What is the active task, in detail? | [`current-task.md`](current-task.md) (KB-089) |
| Why did an attempt fail, and what was tried? | [`failure-log.md`](failure-log.md) (KB-092) |
| What are the routing and retry rules? | [`autonomous-runner.md`](autonomous-runner.md) (KB-091) |

If this file and KB-081 disagree about a task's status, **KB-081 wins** and this file is
corrected.

---

## State

| Field | Value |
|---|---|
| **Status** | `STOPPED` — clean task-boundary stop. **`M2-B02` implemented and independently validated `PASS`** on `migration/M2-B02-paging-contract` (`c603115`), 2026-08-20, after one retry-within-attempt (an OpenAPI casing regression and an unverified boundary criterion, both fixed on the same branch — see `failure-log.md`). **Now `Completed` and merged** (`feec964`, owner-instructed 2026-08-20), and `master` pushed at `8392a64`. The merge released **`M2-B09`** to `Ready` — and **only** `M2-B09`: `M2-B03` still waits on `M2-A02`, `M2-C05`/`M2-C05-01` on `M2-C04-02`. Its Hard-dependents (`M2-B03`, `M2-B09`, `M2-C05`, `M2-C05-01`) therefore stay `Blocked`. The next dependency-ready task (`M2-A01-02`) has been selected and written into `current-task.md` but not started, per this project's "one task, one session" rule. |
| **Stop reason** | Clean task-boundary stop. `M2-B02`'s validation, documentation and close-out are complete and committed; the next task is selected, not started. |
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-20 autonomous run through this task boundary). |
| **Last transition** | 2026-08-20 — `M2-B02` closed out: `Needs Review`, validated `PASS` on attempt 1 of 3 (one in-attempt retry, no new implementer dispatch), 0 escalations, `scopeOk: true`. `task-tracker.md`, `tasks/M2-B02.md` (Execution Record) and `current-task.md` updated; `current-task.md` now points to `M2-A01-02`. Working tree at close-out: `migration/M2-B02-paging-contract` tip `c603115`, only `runner-state.md`/`failure-log.md` dirty (orchestrator-owned bookkeeping). |
| **Current task** | `M2-A01-02` — implement `[RequireScreen]`/`[RequireRight]` (the authorization filter M2-A01-01 specified). Not yet started; no branch exists yet. |
| **Current phase** | `READY` — selected, not yet `IN_PROGRESS`. |
| **Current agent** | n/a — not yet dispatched |
| **Current model** | n/a — not yet dispatched |
| **Attempt** | `M2-A01-02`: 0 of 3 used. |
| **Escalations** | 0 |
| **Last validation** | `M2-B02`, tip `c603115` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. All eighteen acceptance criteria independently re-checked `MET` (the `toDate` 23:59-boundary criterion `MET` with a stated LINQ-vs-T-SQL limit, not against a live SQL Server round trip — every `Currency` row in the reachable dev tenant has a null `CreatedDate`). Re-run by the validator: `dotnet build V.SMART.Api --no-incremental` 0 errors/6,695 warnings (KB-083 baseline); `dotnet test tests/V.SMART.Api.Tests` 56/56 passed; `dotnet test tests/V.SMART.Shared.Tests` 84/84 passed, no regression; `dotnet build V.SMART.Web` 0 errors (Blazor host intact); a live pre/post comparison of `GET api/currencies?pageNumber=1&pageSize=10` against the same tenant database returned a byte-identical body. One retry inside the same attempt: the first validation pass found an OpenAPI casing regression (PascalCase query parameters vs. the required camelCase contract) and the `toDate` criterion unverified rather than failing; both were fixed on the branch (explicit `[FromQuery(Name = …)]` wire names; two new boundary tests) and re-validated `PASS`. Full evidence: `tasks/M2-B02.md` § Execution Record (2026-08-20), `failure-log.md` § "M2-B02". No validation has run yet for `M2-A01-02`. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, `Needs Review`, then owner-merged and `Completed`. `M2-B12-01` — was selected, superseded before being started. `M2-B02` — implemented, validated `PASS` after one in-attempt retry, `Needs Review`, not yet merged. `M2-A01-02` — now selected, not yet started. |
| **Classification** | `M2-B02` (closed) — complexity **HIGH**, risk **HIGH**, per [KB-091 §4](autonomous-runner.md#4-classifying-a-task): base MEDIUM from `task_type: Backend`, raised to HIGH on `estimate: 1 wk` (≥3 d) and `source_files` spanning two projects; risk HIGH on the KB-091 §4.3 trigger "changes behaviour a live Blazor user can observe" (the sort-mechanism design touches `V.SMART.Shared/Repository/Repository.cs`, shared with the live Blazor host). `M2-A01-02` (next) — `task_type: Security`, `estimate: 3 d`, `business_rules: [BR-AUTH-002]` non-empty, `source_files` confined to `V.SMART.Api/` plus three read-only `V.SMART.Shared` references (`RightsHelper.cs`, `UserRightsRepository.cs`, entity classes) — classification not yet run, will be determined at dispatch, but the populated `business_rules` and the `Program.cs`/`AuthController`-adjacent surface both point toward HIGH by the same pattern as `M2-A06`. |
| **Models this run** | `M2-B02`: Implement `opus`, Validate `opus` (HIGH complexity/risk routing, per [KB-091 §5.1](autonomous-runner.md#51-the-routing-table)/[§5.2](autonomous-runner.md#52-floors-that-override-the-table)). `M2-A01-02`: not yet routed. |
| **Next ready task** | `M2-A01-02`, selected per the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) — see `current-task.md` for the full ranking against the other six genuinely `Ready` P0 candidates (`M0-01-03`, `M2-B04`, `M2-B12-01`, `M2-C04-02`, `M2-C04-03`, `M2-C10`) and why `M2-A01-02` won the tie against `M2-B12-01` on step 3 (critical-path placement) after both tied on step 2 (each unblocks exactly one real dependent — `M2-A01-03` and `M2-B12-02` respectively). `M2-B02`'s dependents (`M2-B03`/`M2-B09`/`M2-C05`/`M2-C05-01`) remain `Blocked` — its `Needs Review` status does not release a Hard-dependent successor. |
| **Process note — id allocation** | **Four cross-branch id collisions have now occurred, all on 2026-08-19** — six KB/INV/Q ids, `M2-C01`'s footnote ¹⁸, and a `Q-31` double-claim caught during `M2-B07`'s merge bookkeeping (`Q-31` was already held by `M2-B07` itself; the new question became **Q-32**). Every one was caught by hand at merge, which is not a control. `grep`-before-claim cannot see a sibling branch, and it cannot see an id claimed earlier in the same session. `git branch --no-merged master` must be checked before claiming any id. This recurs until the allocation rule itself changes. |

### Status values

| Status | Means |
|---|---|
| `RUNNING` | A run is live and processing a task |
| `STOPPED` | No run is live. A clean, expected end — budget reached, or no ready task |
| `BLOCKED` | A run halted needing a human. **Stop reason and owner are mandatory** |
| `STOP_REQUESTED` | A human asked the current run to finish its task and stop. The runner checks this at the top of each task and exits cleanly |

---

## Requesting a stop

Set **Status** to `STOP_REQUESTED` and record who asked and why. The runner reads this file at
the start of every task, so the request takes effect at the next task boundary — the in-flight
task finishes its validate/record cycle rather than being abandoned half-implemented.

That is the safe stop. Killing the run mid-task is also safe for the repository — every
transition is written here before the next begins — but it leaves a task part-implemented on
its branch, which someone has to reconcile.

---

## Pre-run flags for `M0-15`

Both were `safetyStop` conditions ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks))
and a run will halt on them rather than measure something unreproducible. **Both are now
resolved (2026-08-17). Kept as history — re-check them, do not assume they stay clear.**

- ✅ **Branch point — RESOLVED 2026-08-17 by re-cutting from `master`.** The branch had been cut
  from `migration/M0-08-gitignore-build-output`; this stopped the first run. It was reset to
  `master` and the three non-M0-08 commits were cherry-picked back, dropping only `e0a7092`
  (M0-08), which remains safe on its own branch. New history:
  `31cfa95` (master) → `998f7d0` → `7905c83` → `fece832`.
  Verified: `git merge-base HEAD master` → `31cfa95`, **identical to master's tip**;
  `git merge-base --is-ancestor e0a7092 HEAD` → false.
  Pre-re-cut state is preserved at tag `backup/M0-15-pre-recut-2026-08-17` (`ef861c3`).
  **Side effect:** M0-08's `Needs Review` status travelled with `e0a7092`, so KB-081 on this
  branch again lists M0-08 as `Ready`. It self-corrects when M0-08 is reviewed and merged.
- ✅ **Dirty working tree — RESOLVED 2026-08-17.**
  `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs` and
  `V.SMART/V.SMART.Web/appsettings.json` were stashed as
  `PRE-M0-15: local tenant DB debugging …` (stash commit `6dbf4b47b8ff`) — local tenant-DB
  debugging work, not part of any task: null/empty-tenant guards in the factory, and a
  `MasterDb` connection string repointed at a local `.\SQLEXPRESS` / `NexGenErpDb_Master`.
  Recoverable with `git stash apply`; the stash holds full file contents, not just a diff.
  `V.SMART.Api/` remains untracked **by design** — see the untracked-directory checkout trap in
  `CLAUDE.md`; never stash or clean it.

No flag is open as of 2026-08-17, so a run may open M0-15. Re-verify both before each run —
they are cheap to check (`git merge-base HEAD master`, `git status --porcelain`) and expensive
to get wrong, since the whole point of M0-15 is a baseline someone else can reproduce.
