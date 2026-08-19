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
| **Status** | `STOPPED` |
| **Stop reason** | Clean end of session — `M0-09` close-out complete. This session's instruction was record-outcome-only ("do NOT implement anything and do NOT start another task"), so it stops at this task boundary by design, not because of a blocker. |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — `M0-09` implemented on `migration/M0-09-delete-guard-fix` (`8e3b19d`), validated `PASS` (`scopeOk: true`, `failureCategory: none`, all 12 acceptance criteria `MET`, independently re-derived including a separate-worktree reproduction of the pre-fix red state), and closed out `Needs Review` — not `Completed`, per [KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed). `docs/kb/execution/tasks/M0-09.md`, `task-tracker.md` (footnote 15), `current-task.md`, `technical-debt-register.md` and `investigation-registry.md` all updated in this close-out session. Selected `M0-06` as the next dependency-ready task per the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule): of the four tasks the `M0-12-01` merge released, `M0-12-02` and `M0-13` are `Completed`, `M0-09` just closed `Needs Review` (not re-selectable), leaving `M0-06` as the sole genuinely `Ready` candidate — no tie-break needed. `M0-06`'s ordering prerequisite on `M0-13` (both risk touching `ApplicationDbContext.cs` seed data) is satisfied — `M0-13` is `Completed` and merged. |
| **Current task** | `M0-06` — Remove the seeded default Administrator credential. See [`tasks/M0-06.md`](tasks/M0-06.md). Not yet started. |
| **Current phase** | Selected, not dispatched — this close-out session's instruction was record-outcome-only. |
| **Current agent** | n/a — not yet dispatched |
| **Current model** | n/a — not yet classified for this task |
| **Attempt** | 0 of 3 |
| **Escalations** | 0 |
| **Last validation** | `M0-09` — verdict `PASS`, attempt 1 of 3, 0 escalations, `scopeOk: true`, `failureCategory: none`. Full record: [`tasks/M0-09.md` § Execution Record (2026-08-19)](tasks/M0-09.md#execution-record-2026-08-19); `task-tracker.md` footnote 15. No validation yet for `M0-06`. |
| **Tasks processed this run** | `M0-09` — implemented, validated `PASS`, recorded `Needs Review` (awaiting owner review/merge) |
| **Classification** | Not yet computed for `M0-06` — leave to the session that dispatches it. `M0-06` is `task_type: Security`, `business_rules: [BR-AUTH-001, BR-AUTH-002]` non-empty, and touches `ApplicationDbContext.cs`/`Migrations/`, which are exactly the surfaces [KB-091 §4.2](autonomous-runner.md#42-raise-one-level-for-each-of-these-that-is-true) and §4.3 flag for raises — expect at least `HIGH` complexity and `HIGH` risk, but compute it properly rather than reusing this note as the classification. |
| **Models this run** | n/a — no model routing decided for `M0-06` yet |
| **Blocked on** | Nothing blocks `M0-06` itself. Still human-blocked elsewhere, and still barring **G0** (so M2 remains shut): `M0-04` `Blocked`⁴ (unidentified owner, production SQL / GST gateway access — also blocks `M0-05`); `M0-01-03` `Needs Review` (repo-side work merged; only a human-executed rebuild drill remains, a hard G0 exit criterion); `M0-09` `Needs Review` (this session's work, awaiting owner review/merge — blocks `M0-10` until merged); `M0-11` `Blocked` on the owner (Q-01 product decision, released by `M0-13`'s merge but not runner-selectable). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. Once rotated, also unblocks `M0-05` (purge secrets from git history), whose other Hard prerequisite (`M0-03`) is already `Completed`. |
| **Owner to unblock M0-01-03** | Repository owner — needs to run/record the rebuild drill against a real, disposable SQL Server instance (`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`); see `tasks/M0-01-03.md`. This is a hard G0 exit criterion. |
| **Owner to unblock M0-09** | Repository owner — review and merge `migration/M0-09-delete-guard-fix` (`8e3b19d`), validated `PASS`. Once merged, `M0-10` (INV-025, delete-guard audit) becomes `Ready`. |
| **Owner to unblock M0-11** | Repository owner — `M0-13` is `Completed` and merged (`9b57552`), so `M0-11` (the Q-01 product decision) is released; it needs an owner decision, not runner work — no runner may self-select a `Product Decision` task ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)). |
| **Next ready task** | `M0-06` (P1, 1 d), selected above. `M0-10` stays `Blocked` behind `M0-09` until `M0-09` is reviewed and merged to genuinely `Completed`. |

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
