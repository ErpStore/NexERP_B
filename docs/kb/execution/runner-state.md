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
| **Stop reason** | `M2-C01` implemented in full (14 of 15 acceptance criteria independently re-verified `MET`) but cannot reach the 15th: `.github/workflows/ci.yml`'s `frontend` job "green on the branch" requires a push, which `CLAUDE.md` § Standing constraints forbids without an explicit in-conversation instruction, and no substitute (local run, `gh`, `act`, Docker, an alternate Node toolchain) is available on this workstation. Needs an owner decision, not a retry — see `Owner to unblock M2-C01` below. |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — close-out session. Two implementation/validation attempts ran on `migration/M2-C01-react-app-skeleton` (`4ac7241`, `8fb8e6d`, `d5182f6`), both validator verdicts `FAIL`/`failureCategory: environment` on the identical criterion. This session recorded the outcome per KB-084's close-out checklist: `tasks/M2-C01.md` Execution Record + frontmatter (`status: Blocked`), `task-tracker.md` row set `Blocked` (footnote ¹⁸, owner Vivek), this file, and `current-task.md`'s Run State. `failure-log.md` (attempt 2 entry) and `open-questions.md` (Q-30) were already recorded by the prior attempt and were not duplicated. No implementation work was done in this session — record-outcome-only, per instruction. |
| **Current task** | `M2-C01` — Vite + React 19 + TS strict + lint + test + CI. See [`tasks/M2-C01.md`](tasks/M2-C01.md#execution-record-2026-08-19). `Blocked`, not started/not resumed further. |
| **Current phase** | Blocked — awaiting owner decision (publish the branch, or waive the "green on the branch" criterion half). |
| **Current agent** | n/a — no agent dispatched this session (record-outcome-only) |
| **Current model** | n/a — no further implement/validate cycle until the owner decides |
| **Attempt** | 2 of 3 (attempt 2's validator verdict is final for this stop; see [`failure-log.md`](failure-log.md#m2-c01--attempt-2--2026-08-19)) |
| **Escalations** | 0 |
| **Last validation** | `M2-C01` — verdict `FAIL`, attempt 2 of 3, 0 escalations, `scopeOk: true`, `failureCategory: environment`. 14/15 criteria `MET`, one `NOT MET`/`NOT CHECKABLE` (criterion 10, second half). Full record: [`tasks/M2-C01.md` § Execution Record (2026-08-19)](tasks/M2-C01.md#execution-record-2026-08-19); [`failure-log.md`](failure-log.md#m2-c01--attempt-2--2026-08-19); `task-tracker.md` footnote ¹⁸. |
| **Tasks processed this run** | `M2-C01` — implemented, validated `FAIL` (environment), recorded `Blocked` (awaiting owner decision) |
| **Classification** | Unchanged from selection: `task_type: Frontend` → `HIGH` complexity (MEDIUM + one raise for `estimate: 3 d`), `MEDIUM` risk. See prior transition entry above for the full derivation; not recomputed, since the blocker is not a classification/routing question. |
| **Models this run** | Implement: `opus`. Validate: `opus`. Both already spent on the two prior attempts; no further model is routed — a stronger model cannot obtain push authority, a hosted runner, or a Node 22 toolchain. |
| **Blocked on** | **The repository owner (Vivek)**, on `M2-C01` specifically: authorise publishing `migration/M2-C01-react-app-skeleton` (preferably as a PR, per Q-20) and confirm the `frontend` CI job green, **or** waive the "green on the branch" half as was done for `M0-07` (`d79e1a4`). Until one of those happens, no M2-C task that depends on `M2-C01` (`M2-C02`, `M2-C03`, `M2-C04-*`, `M2-C05-*`, `M2-C10`, `M2-C11`) should be started — the branch is unmerged, so its work is not actually on `master` for them to build on. Elsewhere, unrelated to `M2-C01`: `M2-A01-02`'s spec-vs-reality contradiction (D-5 vs. R-40) still unresolved; `M0-04` `Blocked` (unidentified owner); `M0-01-03` `Needs Review` (rebuild drill, hard G0 exit criterion, still owed); `M0-06` `Blocked` on Q-25/Q-26; `M0-11` `Ready` but a `Product Decision` (owner-only, not runner-selectable). |
| **Owner to unblock M2-C01** | Repository owner (Vivek) — the only person who can authorise a push per `CLAUDE.md`, or waive the "green on the branch" acceptance-criterion half. See `task-tracker.md` footnote ¹⁸ and `tasks/M2-C01.md` § Execution Record for the two options. |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. Once rotated, also unblocks `M0-05` (purge secrets from git history), whose other Hard prerequisite (`M0-03`) is already `Completed`. |
| **Owner to unblock M0-01-03** | Repository owner — needs to run/record the rebuild drill against a real, disposable SQL Server instance (`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`); see `tasks/M0-01-03.md`. This is a hard G0 exit criterion, carried into M2 as an owner-deferred exception. |
| **Owner to unblock M0-06** | Blocked on Q-25/Q-26 (see `open-questions.md`) — not runner-selectable until answered. |
| **Owner to unblock M0-11** | Repository owner — `M0-13` is `Completed` and merged, so `M0-11` (the Q-01 product decision) is released; it needs an owner decision, not runner work — no runner may self-select a `Product Decision` task ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)). |
| **Next ready task** | None selected — this session's instruction was record-outcome-only, not select-next. Other `Ready`, unclaimed M2 candidates untouched by `M2-C01`'s block, for whoever picks up next: `M2-B07` (P0, 3 d — unblocks the most M2-B work, no same-file conflict with `M2-C01`), `M2-A06` (P0, 3–5 d — unblocks B02/B06/B11), `M2-A01-02` (P0, 3 d — blocked in practice by the D-5/R-40 spec contradiction, so not a genuine alternative right now). |

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
