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
dependencies: [KB-089, KB-091, KB-092]
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
| **Stop reason** | Clean end of `M0-14`'s cycle: it validated `PASS` and is closed out below as `Needs Review`, but re-running the *Ready-task selection rule* against `task-tracker.md` finds **no** `Ready` candidate. Every remaining M0 task is either `Needs Review` awaiting human review-and-merge (`M0-01-03`, `M0-03-03`, `M0-14` itself) or `Blocked` on an unscheduled human (`M0-02` on a DBA; `M0-04` on an unidentified owner; `M0-07` on repository-owner/DevOps GitHub access), or transitively `Blocked` behind those (`M0-12*`, `M0-13`, `M0-09`, `M0-10`, `M0-06`, `M0-11`). `M0-03` is a parent container, never worked directly. This is a genuine stop, not a defect — surfacing the three named human owners is the useful action per KB-082's selection rule. |
| **Run started** | 2026-08-18 |
| **Last transition** | 2026-08-18 — `M0-14` validated `PASS` (attempt 1 of 3, 0 escalations) and closed out: task file status corrected `Completed` → `Needs Review` (per KB-088 "Who may set COMPLETED" — the implementing session may not self-close a task with an outstanding human review-and-merge step), `task-tracker.md` updated with footnote 10, KB-060 R-20 and KB-003 INV-029 already carried the resolution/negative-finding from the implementing session's own commit (`db41ebc`) and needed no further edit. Re-ran the *Ready-task selection rule*: no candidate is `Ready` — `M0-14` was the sole one this cycle and it is now `Needs Review`, not re-selectable. `current-task.md` rewritten to record the blocked/no-active-task state rather than pointing at a task to execute. |
| **Current task** | none — no task is `Ready`. See *Stop reason*. |
| **Current phase** | n/a |
| **Current agent** | n/a — run is blocked, awaiting a human |
| **Current model** | n/a |
| **Attempt** | n/a — no task open |
| **Escalations** | 0 of 1 (this cycle) |
| **Last validation** | `M0-14` — `PASS`, attempt 1 of 3, 0 escalations, `scopeOk: true`, `failureCategory: none`. Full record: [`tasks/M0-14.md` § Execution Record (2026-08-18)](tasks/M0-14.md#execution-record-2026-08-18). |
| **Tasks processed this run** | 1 — `M0-14` selected, implemented, validated `PASS`, and closed out as `Needs Review` |
| **Classification** | `M0-14` (this cycle, now closed): `task_type: Security`, `priority: P2`, `complexity: LOW`, `risk: LOW` — confirmed accurate in hindsight: the actual diff was exactly the single conditional assignment and one dead-key deletion predicted, no merge conflict occurred, both builds stayed at baseline warning counts. |
| **Models this run** | Recorded in `M0-14`'s Execution Record: implementing session per its own report; validator per the PASS verdict supplied for this close-out. |
| **Blocked on** | No task is `Ready`. `M0-02` `Blocked`⁶ on a DBA (≥2 tenant databases, `VIEW DEFINITION`) plus Q-12 (tenant list, unanswered); `M0-04` `Blocked`⁴ on an unidentified human owner (production SQL / GST gateway access); `M0-07` `Blocked`⁷ on a person with `origin` push access and GitHub org admin rights on branch protection — implementation already committed on `migration/M0-07-ci-pipeline`, do not re-implement; `M0-01-03`, `M0-03-03` and `M0-14` are each `Needs Review`, awaiting a human review-and-merge step before any task depending on them (or their own re-selection) becomes possible. |
| **Owner to unblock M0-02** | DBA — first candidate operator **PavanKunar** (ran the M0-01-02 capture); migration lead must also resolve the baseline-tenant label ambiguity (see task file). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. |
| **Owner to unblock M0-07** | Repository owner / DevOps — needs `origin` push access and GitHub org admin rights on branch protection. Work is committed and unmerged on `migration/M0-07-ci-pipeline`; resume validation from that commit once pushed. |
| **Owner to unblock M0-01-03 review** | Repository owner — needs to review and merge; see `tasks/M0-01-03.md`. |
| **Owner to unblock M0-03-03 review** | Repository owner — needs to review branch `migration/M0-03-03-startup-config-validation` (commit `34be11a`, already merged into `master` at `028e834` as an integration step, but not yet given the explicit named human sign-off this project's `Completed` status requires) and record sign-off. |
| **Owner to unblock M0-14 review** | Repository owner — needs to review branch `migration/M0-14-gate-detailed-errors` (commit `db41ebc`) and merge/sign off. |
| **Next ready task** | None. Re-derive per the *Ready-task selection rule* once one of the human owners above acts — most likely to unblock next: sign-off on `M0-01-03` or `M0-03-03` (pure review, no new work), or `M0-04`/`M0-07`/`M0-02` access being granted. |

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
