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
last_verified: 2026-08-18
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
| **Status** | `BLOCKED` |
| **Stop reason** | `M0-12-01: implementer returned no result` |
| **Run started** | 2026-08-18 |
| **Last transition** | 2026-08-18 — implementer dispatched for `M0-12-01` returned no result; validator returned no verdict; task set `Blocked` and the run halted rather than re-dispatching automatically. Full record: [`tasks/M0-12-01.md` § Execution Record (2026-08-18)](tasks/M0-12-01.md#execution-record-2026-08-18); attempt logged in [`failure-log.md`](failure-log.md#m0-12-01--attempt-1--2026-08-18); tracker footnote 12 in [`task-tracker.md`](task-tracker.md). |
| **Current task** | `M0-12-01` — Create the test project and wire it into CI — `Blocked` |
| **Current phase** | `IN_PROGRESS` (dispatched) → `BLOCKED` (empty implementer return; no validation could run) |
| **Current agent** | n/a — the dispatched implementer produced no output; no agent identity to record |
| **Current model** | Implement: `opus`; Validate: `opus`; Investigate: `opus` (see Classification — unchanged from selection) |
| **Attempt** | 1 of 4 used |
| **Escalations** | 0 of 1 (this cycle) — no escalation trigger applied; there was no failure content to classify |
| **Last validation** | `M0-12-01` — verdict `none` ("validation did not complete"), attempt 1 of 4, 0 escalations. Not a `PASS`/`FAIL` — the implementer produced nothing to validate. Full record: [`tasks/M0-12-01.md` § Execution Record (2026-08-18)](tasks/M0-12-01.md#execution-record-2026-08-18). The most recent actual `PASS` remains `M0-14` (Vivek sign-off, merge `275c6e2`). |
| **Tasks processed this run** | 0 implemented — `M0-12-01` was selected and dispatched this cycle but produced no implementation |
| **Classification** | `task_type: Testing` → base `MEDIUM` ([KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)). Raise applied: 4 tasks (`M0-12-02`, `M0-13`, `M0-09`, `M0-06`) name `M0-12-01` in their `depends_on` — ≥3 tasks naming this one ([KB-091 §4.2](autonomous-runner.md#42-raise-one-level-for-each-of-these-that-is-true)) → **complexity HIGH**. No other raise applies: `estimate` is 0.5 d (not ≥3 d); `depends_on` names only 1 task (`M0-07`); `business_rules: []` in frontmatter; `source_files` all sit under the single project `V.SMART.Shared`; the task does not modify authn/authz/tenancy/numbering/calculation logic. **Risk: MEDIUM (default)** — unchanged from selection; this classification was never exercised because no implementation occurred. |
| **Models this run** | Per [KB-091 §5.1](autonomous-runner.md#51-the-routing-table) at complexity HIGH: Investigate `opus`, Implement `opus`, Validate `opus`, Diagnose (first failure) `opus`, Diagnose (escalated) `opus`. |
| **Blocked on** | `M0-12-01` is blocked on a human — see *Owner to unblock M0-12-01* below. Other known human-blocked items remain open in parallel and are unaffected: `M0-04` `Blocked`⁴ (unidentified owner, production SQL / GST gateway access — also blocks `M0-05`); `M0-01-03` `Needs Review` (repo-side work merged; only a human-executed rebuild drill remains, a hard G0 exit criterion). |
| **Owner to unblock M0-12-01** | **Vivek** (repository owner / migration lead — the named owner who has signed off every other human gate this milestone). Needs to check the runner's dispatch/agent-invocation layer for this cycle — an implementer that returns nothing is a tooling symptom, not a task-content problem — before a retry is attempted, in case the fault is systemic. The task specification itself is unchanged and valid; once the dispatch fault is understood (or simply not reproduced), re-open by re-dispatching the implementer. Attempts used: 1 of 4. |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. Once rotated, also unblocks `M0-05` (purge secrets from git history), whose other Hard prerequisite (`M0-03`) is already `Completed`. |
| **Owner to unblock M0-01-03** | Repository owner — needs to run/record the rebuild drill against a real, disposable SQL Server instance (`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`); see `tasks/M0-01-03.md`. This is a hard G0 exit criterion. |
| **Next ready task** | None. `M0-12-01` reverts from `Ready` to `Blocked` on the human above; `M0-12-02`, `M0-13`, `M0-09`, `M0-06` (and transitively `M0-10`, `M0-11`) stay `Blocked` behind it. Re-derive per the *Ready-task selection rule* once `M0-12-01` is unblocked and re-dispatched successfully. |

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
