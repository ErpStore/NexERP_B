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
| **Stop reason** | No dependency-ready task remains: runner-state.md's Status is BLOCKED, not STOP_REQUESTED, so this is not a stop request — proceeded to selection. current-task.md holds no in-progress task (it explicitly records "No task is Ready"), so there is nothing to resume. Re-checked task-tracker.md (KB-081, the status authority) and found it had advanced past runner-state.md's prior snapshot: M0-03-02 and M0-14 are now both Completed (owner Vivek signed off and merged both, per footnotes 8 and 10), and M0-03-03 was already implemented, validated PASS, and merged to master (commit 34be11a) — only its own review sign-off is outstanding. Applied the Ready-task selection rule (KB-082): a candidate needs every Hard prerequisite genuinely COMPLETED (not REVIEW), must not be a parent container, and must not be blocked on an unscheduled human. M0-05 depends on the M0-03 parent, which is not fully Completed (M0-03-03 is still Needs Review) plus M0-04 (Blocked). M0-12*, M0-13, M0-09, M0-10, M0-06, M0-11 are all transitively Blocked behind M0-07/M0-12-01. M0-02, M0-04, M0-07 are Blocked on named-but-unscheduled human owners. M0-01-03 and M0-03-03 are Needs Review, not re-selectable (their work is already done and merged in substance; only human sign-off remains). No candidate satisfies the rule, so no task is Ready this cycle. Corrected docs/kb/execution/runner-state.md (KB-093) to reflect the tracker's more current state (M0-03-02 and M0-14 now Completed with sign-off recorded; M0-03-03 already merged awaiting sign-off) since the file was stale relative to the tracker, which is the authority when they disagree. Did not touch current-task.md's own "no task is Ready" framing since it remains accurate, and did not implement anything. |
| **Run started** | 2026-08-18 |
| **Last transition** | 2026-08-18 — selection re-run found no new work: confirmed via `task-tracker.md` footnotes 8 and 10 that `M0-03-02` and `M0-14` reached `Completed` (Vivek's sign-off, commits `ec2f0f3`/`7fbb768` and `275c6e2`/`b6b2c6f`), that `M0-03-03` is `Needs Review` (already merged to `master`, only sign-off outstanding), and that no other task became a Hard-prerequisite-satisfied candidate as a result — `M0-05` still needs the whole `M0-03` parent `Completed`, which `M0-03-03` blocks. `current-task.md` and this file were stale relative to the tracker (the status authority) and are corrected here; no code was touched. |
| **Current task** | none — no task is `Ready`. See *Stop reason*. |
| **Current phase** | n/a |
| **Current agent** | n/a — run is blocked, awaiting a human |
| **Current model** | n/a |
| **Attempt** | n/a — no task open |
| **Escalations** | 0 of 1 (this cycle) |
| **Last validation** | `M0-14` — `PASS`, attempt 1 of 3, 0 escalations, `scopeOk: true`, `failureCategory: none`. Full record: [`tasks/M0-14.md` § Execution Record (2026-08-18)](tasks/M0-14.md#execution-record-2026-08-18). Now `Completed` (Vivek sign-off, merge `275c6e2`). |
| **Tasks processed this run** | 0 — this cycle only re-derived selection state; no task was `Ready` to open |
| **Classification** | n/a this cycle — no task selected |
| **Models this run** | n/a this cycle |
| **Blocked on** | No task is `Ready`. `M0-02` `Blocked`⁶ on a DBA (≥2 tenant databases, `VIEW DEFINITION`) plus Q-12 (tenant list, unanswered); `M0-04` `Blocked`⁴ on an unidentified human owner (production SQL / GST gateway access); `M0-07` `Blocked`⁷ on a person with `origin` push access and GitHub org admin rights on branch protection — implementation already committed on `migration/M0-07-ci-pipeline`, do not re-implement; `M0-01-03` and `M0-03-03` are each `Needs Review` (both already merged in substance — `M0-01-03`'s repo-side deliverables and `M0-03-03`'s branch are on `master`), awaiting only the human review-and-merge/sign-off step before either can close or unblock a downstream task. |
| **Owner to unblock M0-02** | DBA — first candidate operator **PavanKunar** (ran the M0-01-02 capture); migration lead must also resolve the baseline-tenant label ambiguity (see task file). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. |
| **Owner to unblock M0-07** | Repository owner / DevOps — needs `origin` push access and GitHub org admin rights on branch protection. Work is committed and unmerged on `migration/M0-07-ci-pipeline`; resume validation from that commit once pushed. |
| **Owner to unblock M0-01-03 review** | Repository owner — needs to run/record the rebuild drill (`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`); see `tasks/M0-01-03.md`. |
| **Owner to unblock M0-03-03 review** | Repository owner — needs to review branch `migration/M0-03-03-startup-config-validation` (commit `34be11a`, already merged into `master`) and record sign-off, the same way Vivek did for `M0-03-02` and `M0-14`. |
| **Next ready task** | None. Re-derive per the *Ready-task selection rule* once one of the human owners above acts — most likely to unblock next: sign-off on `M0-03-03` (pure review, no new work — would also complete the `M0-03` parent), or `M0-04`/`M0-07`/`M0-02` access being granted. |

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
