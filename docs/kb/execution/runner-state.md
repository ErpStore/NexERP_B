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
| **Status** | `BLOCKED` |
| **Stop reason** | `M0-12-02` implemented (`050f06b`) and validated `FAIL`/`failureCategory: environment` — 11 of 12 acceptance criteria objectively `MET`; criterion 8's second half ("the suite passes in CI on the branch") requires a `git push` that `CLAUDE.md` § Standing constraints and this dispatch's `allow_push=false` both forbid. The branch is absent from `origin` (`git ls-remote --heads origin` — 8 refs, none matching), so no hosted Actions run can exist. A diagnosis pass reproduced the same result independently and agrees: `disposition: blocked`. This is the same wall already recorded for `M0-07` and `M0-12-01` (Q-20/Q-22). Needs the repository owner's decision — see `task-tracker.md` footnote 14. |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — `M0-12-02` implemented (attempt 1 of 3, 0 escalations), validated `FAIL`/`environment` by the validator, then re-confirmed by an independent diagnosis pass (`disposition: blocked`, no fix applied, no code/test file touched). Close-out recorded the outcome as `Blocked` on the repository owner — session stopped at task boundary; not resumed automatically because `KB-091 §8` triggers 5 and 7 both require a human decision. |
| **Current task** | `M0-12-02` — Characterisation tests for `CalculationService`. See [`tasks/M0-12-02.md`](tasks/M0-12-02.md). Blocked; not resolvable by re-dispatch. |
| **Current phase** | `TESTING`/`REVIEW` → stopped, awaiting owner decision (push authorisation, or waive criterion 8's CI half) |
| **Current agent** | n/a — run stopped at task boundary |
| **Current model** | n/a — see Models this run |
| **Attempt** | 1 of 3 (not exhausted; further attempts would not help — same wall, not a code defect) |
| **Escalations** | 0 |
| **Last validation** | `M0-12-02` — verdict `FAIL`, attempt 1 of 3, 0 escalations, `failureCategory: environment`, `scopeOk: true`. 11 of 12 acceptance criteria `MET` (`dotnet test` → 73/73 passing twice; `dotnet build V.SMART.Api --no-incremental` → 0 errors/6,694 warnings; `git diff --stat` zero files under `V.SMART/`; KB-030/KB-060/KB-004/KB-003 all updated in-commit). Criterion 8's "in CI on the branch" half unmet — branch never pushed. Full record: [`tasks/M0-12-02.md` § Execution Record (2026-08-19)](tasks/M0-12-02.md#execution-record-2026-08-19); [`failure-log.md` § M0-12-02 · attempt 1](failure-log.md#m0-12-02--attempt-1--2026-08-19) and its diagnosis entry. |
| **Tasks processed this run** | `M0-12-02` — implemented, validated `FAIL`/`environment`, recorded `Blocked` on the repository owner |
| **Classification** | `task_type: Testing` → base `MEDIUM` ([KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)). Raises ([KB-091 §4.2](autonomous-runner.md#42-raise-one-level-for-each-of-these-that-is-true)): `estimate` is `2.5 d` (<3 d, no raise); `depends_on` names only 1 task (no raise); `source_files` all sit under one project, `V.SMART.Shared` (no raise); `business_rules: [BR-CALC-001, BR-CALC-002]` is non-empty → **one raise**; the task touches **calculation logic** (`CalculationService`) → **a second raise**. Two raises on a `MEDIUM` base caps at **complexity HIGH** (no level above HIGH). **Risk: HIGH** — `business_rules` populated triggers the HIGH row in [KB-091 §4.3](autonomous-runner.md#43-risk) regardless of task_type. |
| **Models this run** | Per [KB-091 §5.1](autonomous-runner.md#51-the-routing-table) at complexity HIGH: Investigate `opus`, Implement `opus`, Validate `opus`, Diagnose (first failure) `opus`, Diagnose (escalated) `opus`. |
| **Blocked on** | `M0-12-02` is now itself human-blocked (see Stop reason above) — an owner decision on pushing the branch, or waiving criterion 8's CI half. Also still human-blocked, and still barring **G0** (so M2 remains shut): `M0-04` `Blocked`⁴ (unidentified owner, production SQL / GST gateway access — also blocks `M0-05`); `M0-01-03` `Needs Review` (repo-side work merged; only a human-executed rebuild drill remains, a hard G0 exit criterion). `M0-13` is `Completed`/merged, so `M0-11` (Q-01 product decision) is released to the owner rather than blocked on a task. |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. Once rotated, also unblocks `M0-05` (purge secrets from git history), whose other Hard prerequisite (`M0-03`) is already `Completed`. |
| **Owner to unblock M0-01-03** | Repository owner — needs to run/record the rebuild drill against a real, disposable SQL Server instance (`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`); see `tasks/M0-01-03.md`. This is a hard G0 exit criterion. |
| **Owner to unblock M0-11** | Repository owner — `M0-13` is now `Completed` and merged (`9b57552`), so `M0-11` (the Q-01 product decision) is released; it needs an owner decision, not runner work — no runner may self-select a `Product Decision` task ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)). |
| **Owner to unblock M0-12-02** | Repository owner (Vivek) — choose (A) authorise pushing `migration/M0-12-02-calculationservice-characterisation` and observe CI green, the Q-22 route, or (B) waive criterion 8's "in CI" half per the M0-07 precedent (`d79e1a4`). See `task-tracker.md` footnote 14. |
| **Next ready task** | `M0-09` (P1, 0.5 d) and `M0-06` (P1, 1 d) remain `Ready`, independent of each other and of `M0-12-02`. `M0-10` stays `Blocked` behind `M0-09`. `M0-12-02` is not a Hard prerequisite for either, so this run could select one of them next, but per the close-out instruction for this session `nextTaskId` is left empty — a human should decide whether to resume with `M0-09`/`M0-06` or resolve `M0-12-02` first. Apply the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) rather than assuming this list is still current. |

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
