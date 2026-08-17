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
last_verified: 2026-08-17
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
| **Status** | `RUNNING` |
| **Stop reason** | n/a — run in progress |
| **Run started** | 2026-08-17 |
| **Last transition** | 2026-08-17 — `M0-08` validated `PASS` (attempt 1 of 3) and is closed out as `Needs Review`⁵, not `Completed` (integration/merge is a human step per KB-088 "Who may set COMPLETED"; see [`tasks/M0-08.md` § Execution Record](tasks/M0-08.md#execution-record-2026-08-17)). Re-applying the ready-task selection rule against the tracker as it now stands: candidates are tasks with every Hard prerequisite genuinely `Completed`, not a parent container, and not blocked on an unscheduled human step. `M0-04` stays excluded (`Blocked`⁴ on an unidentified human owner, unchanged). `M0-08` is no longer a candidate — it is `Needs Review`, not `Ready`. `M0-03` is a parent container (skipped). `M0-07`'s Hard prerequisites (`M0-15`, `M0-08`) are both `Needs Review`, not `Completed`, so it stays `Blocked`. The only remaining candidate is `M0-02` (P1, `depends_on: [M0-01-02]`, which is `Completed`; not a parent; not blocked on a human step — Q-14 is what M0-02 itself answers, not a prerequisite it is waiting on). **`M0-02` selected**, uncontested (sole candidate, no rank tie-break needed). |
| **Current task** | `M0-02` — Confirm stored-procedure drift across tenant databases (Q-14) |
| **Current phase** | Not yet opened — selection only, per this session's instruction not to implement |
| **Current agent** | n/a (selection only) |
| **Current model** | n/a (not yet proceeding) |
| **Attempt** | 0 of 3 (`max_retries: 2`) |
| **Escalations** | 0 of 1 |
| **Last validation** | `M0-08` attempt 1 — `PASS` (2026-08-17, this run). See [`failure-log.md`](failure-log.md). |
| **Tasks processed this run** | 1 (`M0-08` — validated `PASS`, closed out `Needs Review`; `M0-02` chosen next, not yet opened) |
| **Classification** | `M0-02`: `task_type: Investigation` → base complexity to be assessed when the task is actually opened; not pre-classified here since this session's instruction is selection only, not implementation. |
| **Models this run** | Not yet assigned for `M0-02` — classification deferred to the session that opens it. |
| **Blocked on** | Nothing for `M0-02`. `M0-04` remains separately `Blocked`⁴ on identification of a named person with production SQL Server and GST e-Invoice/e-Way gateway access — unresolved, carried forward, not this run's active task. `M0-07` remains `Blocked` pending `M0-15` and `M0-08` both reaching `Completed` (i.e. reviewed and merged). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. Once named, that person must participate in-session or rotation must remain Blocked pending their availability. |
| **Next ready task** | To be re-derived when `M0-02` completes, per the ready-task selection rule against the tracker at that time. |

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
