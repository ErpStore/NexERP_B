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
last_verified: 2026-08-19
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
| **Status** | `RUNNING` |
| **Stop reason** | n/a — run is live |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — `M0-13` implemented (commit `9d8d7be`, attempt 1 of 3, 0 escalations) and validated `PASS` (`scopeOk: true`, `failureCategory: none`, all 12 acceptance criteria `MET`). Close-out recorded the outcome as `Needs Review` — a human must review/merge before it counts as `Completed` ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)) — so `M0-11` (its sole downstream dependent) stays `Blocked`. Selection rule re-applied for the next task: of the remaining candidates (`M0-12-02`, `M0-09`, `M0-06`), `M0-12-02` is the only P0, so it ranks first regardless of downstream/critical-path tie-breaks. `M0-12-02` selected as the next active task. |
| **Current task** | `M0-12-02` — Characterisation tests for `CalculationService`. See [`tasks/M0-12-02.md`](tasks/M0-12-02.md). |
| **Current phase** | `READY` → about to be dispatched for investigation/implementation |
| **Current agent** | n/a — selection only, no agent dispatched yet this cycle |
| **Current model** | n/a — see Models this run |
| **Attempt** | 0 of 3 |
| **Escalations** | 0 of 1 (this cycle) |
| **Last validation** | `M0-13` — verdict `PASS`, attempt 1 of 3, 0 escalations. 12 of 12 acceptance criteria `MET` (all 16 BR-STK-001/002 statements covered by 25 named tests; FIFO/RcSubID/StoreId/re-issue/exception-message criteria each independently re-checked against source; `dotnet test` → 36 discovered, 36 passed, 0 skipped, run twice for flakiness; `git diff --stat` zero files under `V.SMART/`; `dotnet build V.SMART.Api` → 0 errors, 6,694 warnings, at baseline; KB-030/KB-060/KB-004/investigation-registry all updated in-commit). No regressions found. Full record: [`tasks/M0-13.md` § Execution Record (2026-08-19)](tasks/M0-13.md#execution-record-2026-08-19). |
| **Tasks processed this run** | `M0-13` — implemented, validated `PASS`, recorded `Needs Review` (not `Completed` — awaiting owner review/merge) |
| **Classification** | `task_type: Testing` → base `MEDIUM` ([KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)). Raise applied: `estimate` is `2.5 d` (<3 d, so **no** raise from estimate); `business_rules: [BR-CALC-001, BR-CALC-002]` is non-empty → one raise ([KB-091 §4.2](autonomous-runner.md#42-raise-one-level-for-each-of-these-that-is-true)). One raise on a `MEDIUM` base → **complexity HIGH**. No raise from `depends_on` (names only 1 task, `M0-12-01`) or from `source_files` (all under `V.SMART.Shared`). **Risk: HIGH** — `business_rules` populated triggers the HIGH row in [KB-091 §4.3](autonomous-runner.md#43-risk) regardless of task_type. |
| **Models this run** | Per [KB-091 §5.1](autonomous-runner.md#51-the-routing-table) at complexity HIGH: Investigate `opus`, Implement `opus`, Validate `opus`, Diagnose (first failure) `opus`, Diagnose (escalated) `opus`. |
| **Blocked on** | **Nothing that blocks `M0-12-02`.** Its sole Hard prerequisite `M0-12-01` is `Completed` and merged (`bdee81f`); `dotnet test` works; it is a genuinely independent sibling of `M0-13` (different source files — `CalculationService.cs`, not `StockManagerService.cs`). Still human-blocked, and still barring **G0** (so M2 remains shut): `M0-04` `Blocked`⁴ (unidentified owner, production SQL / GST gateway access — also blocks `M0-05`); `M0-01-03` `Needs Review` (repo-side work merged; only a human-executed rebuild drill remains, a hard G0 exit criterion); `M0-13` `Needs Review`¹³ (validated, awaiting owner review/merge). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. Once rotated, also unblocks `M0-05` (purge secrets from git history), whose other Hard prerequisite (`M0-03`) is already `Completed`. |
| **Owner to unblock M0-01-03** | Repository owner — needs to run/record the rebuild drill against a real, disposable SQL Server instance (`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`); see `tasks/M0-01-03.md`. This is a hard G0 exit criterion. |
| **Owner to unblock M0-11** | Repository owner — needs to review and merge `migration/M0-13-stockmanagerservice-characterisation` (`9d8d7be`) before `M0-13` can read as `Completed` and release `M0-11`, the product decision on Q-01. |
| **Next ready task after M0-12-02** | `M0-09` (P1, 0.5 d) and `M0-06` (P1, 1 d) remain `Ready`, independent of each other and of `M0-12-02`. `M0-10` stays `Blocked` behind `M0-09`; `M0-11` stays `Blocked` behind `M0-13` until it is reviewed and merged. Apply the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) rather than assuming this list is still current. |

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
