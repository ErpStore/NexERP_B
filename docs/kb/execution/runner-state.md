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
| **Status** | `STOPPED` |
| **Stop reason** | Clean end of task cycle — `M0-03-02` finished, validated `PASS`, and closed out to `Needs Review`. No ready task requires resuming this run automatically; the next candidate (`M0-14`) is written to `current-task.md` for the next session/run to open. |
| **Run started** | 2026-08-18 |
| **Last transition** | 2026-08-18 — `M0-03-02` closed out. The resumed implementation attempt (commit `e6e5295`, `migration/M0-03-02-hardcoded-connection-strings-csharp`, cut from `master@0a20d62`) was independently validated `PASS` (attempt 1 of 3, 0 escalations, `scopeOk: true`, no regressions). Task file status set to `Needs Review` (not `Completed` — KB-088 "Who may set COMPLETED" requires human review/merge first). `task-tracker.md` (KB-081) updated with the honest status and footnote ⁸. Investigation registry and `technical-debt-register.md` R-01 were already amended by the implementer with `file:line` evidence and `last_verified: 2026-08-18` — verified present, not re-derived. `current-task.md` (KB-089) rewritten to point at `M0-14` (Ready, P2, same Hard prerequisite `M0-03-01`, no shared-file conflict with the now-closed `M0-03-02`). Note: a stale, differently-named branch `migration/M0-03-02-hardcoded-connection-strings` (no `-csharp`) still exists with old committed work cut from a pre-M0-15-recut point — **superseded, do not merge from or resume it**; the reviewable work is on the `-csharp` branch, commit `e6e5295`. |
| **Current task** | `M0-14` — Gate `DetailedErrors` on `IsDevelopment()` (not yet started) |
| **Current phase** | n/a — no task open |
| **Current agent** | n/a |
| **Current model** | n/a |
| **Attempt** | n/a — `M0-03-02` finished at 1 of 3 attempts, 0 escalations; `M0-14` has not been opened |
| **Escalations** | 0 of 1 (carried from `M0-03-02`'s completed cycle; resets when `M0-14` opens) |
| **Last validation** | `M0-03-02` — `PASS`, attempt 1 of 3, 0 escalations, `scopeOk: true`, `failureCategory: none`. Full validator verdict recorded in the session that closed this task; summarised in [`tasks/M0-03-02.md` § Execution Record](tasks/M0-03-02.md#execution-record-2026-08-18). |
| **Tasks processed this run** | 1 finished this cycle (`M0-03-02`, resumed → committed → validated `PASS` → closed to `Needs Review`) |
| **Classification** | `M0-03-02`: `task_type: Security`, `priority: P0`, `estimate: 0.5 d`, `complexity: MEDIUM`, `risk: MEDIUM` — no explicit `complexity`/`risk` frontmatter override, derived from: 1 hard dependency (`M0-03-01`) already `Completed`; `business_rules: []`; 6 `source_files` spanning three projects (`V.SMART.Shared`, `V.SMART` MAUI, plus `MauiProgram.cs` and two e-Invoice/e-Way integration services) — mechanical literal-to-config replacement, not new logic, hence not `HIGH` complexity, but multi-project reach and the removal of live-looking credential literals (SA password, production host, e-Invoice/e-Way gateway secrets) make it security-sensitive enough for `risk: MEDIUM` rather than `LOW` — a mishandled secret scan or a broken MAUI config read would be a real regression. |
| **Models this run** | Not yet recorded — to be captured as the implementing session runs |
| **Blocked on** | Not applicable to `M0-14` (dependency `M0-03-01` is `Completed`, no external blocker). Still open elsewhere: `M0-02` `Blocked`⁶ on a DBA (≥2 tenant databases, `VIEW DEFINITION`) plus Q-12 (tenant list, unanswered); `M0-04` `Blocked`⁴ on an unidentified human owner (production SQL / GST gateway access); `M0-07` `Blocked`⁷ on a person with `origin` push access and GitHub org admin rights on branch protection — implementation already committed on `migration/M0-07-ci-pipeline`, do not re-implement; `M0-03-03` `Blocked` on `M0-03-02` reaching `Completed` (currently `Needs Review`, not merged). |
| **Owner to unblock M0-02** | DBA — first candidate operator **PavanKunar** (ran the M0-01-02 capture); migration lead must also resolve the baseline-tenant label ambiguity (see task file). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. |
| **Owner to unblock M0-07** | Repository owner / DevOps — needs `origin` push access and GitHub org admin rights on branch protection. Work is committed and unmerged on `migration/M0-07-ci-pipeline`; resume validation from that commit once pushed. |
| **Owner to unblock M0-03-03** | Cleared 2026-08-18 — `M0-03-02` was reviewed and approved by Vivek and merged to `master` (`ec2f0f3`, merging `e6e5295` + `b62440c`). `M0-03-03` is no longer blocked on that merge. |
| **Next ready task** | `M0-14` — the only `Ready` task in the tracker as of this close-out (P2, Hard prerequisite `M0-03-01` already `Completed`, no shared-file conflict with the now-closed `M0-03-02` per `dependency-graph.md` § Same-file conflicts). Written to `current-task.md`. Not started. |

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
