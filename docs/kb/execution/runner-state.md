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
| **Status** | `STOPPED` — clean, expected end. `M2-C04-01` closed this session at `Needs Review` (validated `PASS`); the task lifecycle forbids starting the next task in the same session ([KB-088 §1](workflow.md#1-task-lifecycle)), so the run stops at this boundary rather than opening `M2-A06`. |
| **Stop reason** | Task boundary, not a blocker. `M2-C04-01` validated `PASS` — verdict `PASS`, `failureCategory: none`, `scopeOk: true`, all sixteen acceptance criteria `MET`, no regressions. The coverage regression that stopped attempt 1 (`npm run coverage` against `vitest.config.ts:38`'s `branches: 100` floor) is closed honestly: branches now measure 100 % and the floor itself was not touched. Nothing further is owed from an autonomous session; the branch awaits the repository owner's review and merge (KB-088 "Who may set COMPLETED"). |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-20 — `M2-C04-01` attempt 2's coverage fix (`9f886a6`, on top of the preserved WIP `5313c46`) independently validated `PASS`. Full record: [`tasks/M2-C04-01.md` § Execution Record (2026-08-20)](tasks/M2-C04-01.md#execution-record-2026-08-20); `task-tracker.md` footnote ²². Session close-out moved the task `Blocked` → `Needs Review` and left the branch `migration/M2-C04-01-design-tokens` unmerged for owner review. |
| **Current task** | None — `M2-C04-01` is closed for this session (`Needs Review`, not runner-selectable). See **Next ready task** below for what an owner or the next run should open. |
| **Current phase** | n/a — no task is in flight. |
| **Current agent** | n/a — no agent is live |
| **Current model** | n/a |
| **Attempt** | `M2-C04-01`: 1 of 3 used this session, 0 escalations, per the final validator's own accounting (the lost attempt-2 dispatch recorded in the prior state did not consume budget, consistent with the `M0-12-01` precedent — KB-081 footnote ¹²). Task is now closed; the counter does not carry forward to whatever opens next. |
| **Escalations** | 0 |
| **Last validation** | `M2-C04-01`, tip `9f886a6` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. All sixteen acceptance criteria independently re-checked and `MET`, including a from-scratch WCAG recomputation (110 pairs, 0 failing, both themes) and re-runs of `typecheck`/`lint`/`test`/`build`/`coverage` all green (`branches 100 %`). No regressions found; `V.SMART/` untouched. Full evidence: `tasks/M2-C04-01.md` § Execution Record (2026-08-20). |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, closed `Needs Review`. |
| **Classification** | `M2-C04-01` — complexity **HIGH**, risk **MEDIUM** (as previously recorded; unchanged by this close-out — see the task file for the full classification). |
| **Models this run** | Implement: `opus`. Validate: `opus`. (HIGH-complexity routing, [KB-091 §5.1](autonomous-runner.md#51-the-routing-table)) |
| **Next ready task** | **`M2-A06`** (Exception middleware → `ProblemDetails` + correlation ids), selected per [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule): P0, `Ready` (Hard prerequisite is `G0`, already passed), and the highest downstream-unblocking count among the `Ready` P0 candidates — it is a Hard prerequisite for `M2-B02` (→ `M2-B03` → `M2-B10`), `M2-B06` and `M2-B11`. It was recorded as a tied candidate with `M2-C04-01` in the prior state; with `M2-C04-01` now closed, it stands alone at the top of the ranking. Other `Ready` P0 candidates considered and ranked below it on downstream unblocking: `M2-B04`, `M2-B12-01`, `M2-C10`, `M0-01-03`; `M2-A01-02` is nominally `Ready` but its spec contradicts current reality (see `current-task.md`) and was not selected. `current-task.md` has been rewritten to point at `M2-A06`. Not started — the next session opens it. |
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
