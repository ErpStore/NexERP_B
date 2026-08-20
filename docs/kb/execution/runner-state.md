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
| **Status** | `STOPPED` — **`M2-A06` is `Completed` and merged** (`76eca5d`, owner-instructed 2026-08-20) after validating `PASS`, and `master` was pushed at `e63716e` with **CI green** — the first hosted run of `V.SMART.Api.Tests` and the frontend suite. That merge released **`M2-B02`, `M2-B06` and `M2-B11`** to `Ready`, so `current-task.md`'s pointer at `M2-B12-01` was chosen against a candidate set that has since grown: **re-apply the [selection rule](dependency-graph.md#ready-task-selection-rule) rather than inherit it** — `M2-B02` is P0 and unblocks a longer chain (`M2-B03` → `M2-B10`). No branch is cut for any of them. |
| **Stop reason** | Clean task-boundary stop. `M2-A06`'s validation, documentation and close-out are complete and committed; the next dependency-ready task (`M2-B12-01`) has been selected and written into `current-task.md` but not started, per this project's "one task, one session" rule. |
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-20 autonomous run through this task boundary). |
| **Last transition** | 2026-08-20 — `M2-A06` closed out: `Needs Review`, validated `PASS` on attempt 1 of 3, 0 escalations, `scopeOk: true`. `task-tracker.md`, `tasks/M2-A06.md` (Execution Record) and `current-task.md` updated; `current-task.md` now points to `M2-B12-01`. Working tree at close-out: `migration/M2-A06-problem-details` tip `f69891a`, only `runner-state.md` dirty (this file). |
| **Current task** | `M2-B12-01` — INV-012: document numbering + financial-year investigation. Not yet started; no branch exists yet. |
| **Current phase** | `READY` — selected, not yet `IN_PROGRESS`. |
| **Current agent** | n/a — not yet dispatched |
| **Current model** | n/a — not yet dispatched |
| **Attempt** | `M2-B12-01`: 0 of 3 used. |
| **Escalations** | 0 |
| **Last validation** | `M2-A06`, tip `f69891a` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. All eighteen acceptance criteria independently re-checked and `MET` (two — updating `M2-A02`'s tests, the `M2-A03` harness — correctly marked not applicable/not checkable, neither prerequisite has landed). Re-run by the validator: `dotnet build V.SMART.Api --no-incremental` 0 errors/6,694 warnings; `dotnet test tests/V.SMART.Api.Tests` 21/21 passed; `dotnet test tests/V.SMART.Shared.Tests` 84/84 passed, no regression; both protected-tree diffs empty. The validator also independently probed the running host over real HTTP — something the implementer had explicitly reported as not done. Two gaps found only during validation and now recorded in the KB: no `X-Correlation-Id` on `/swagger/index.html` (Development-only), and `ExceptionHandlingMiddleware`'s `Response.Clear()` discarding CORS headers on error (flagged forward to M2-A05). Full evidence: `tasks/M2-A06.md` § Execution Record (2026-08-20). No validation has run yet for `M2-B12-01`. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, `Needs Review`, not yet merged. `M2-B12-01` — selected, not yet started. |
| **Classification** | `M2-A06` (closed) — complexity **HIGH**, risk **HIGH**, per [KB-091 §4](autonomous-runner.md#4-classifying-a-task): base MEDIUM from `task_type: Backend`, raised to HIGH on `estimate: 3–5 d`, non-empty `business_rules`, `source_files` spanning two projects, and touching `AuthController`/BR-AUTH-002. Risk HIGH from editing `Program.cs` plus populated `business_rules`. `M2-B12-01` (next) — `task_type: Investigation`, `estimate: 2 d`, `business_rules: []`, `depends_on: [M2-B07]` only; classification not yet run — will be determined when the task is dispatched. |
| **Models this run** | `M2-A06`: Implement `opus`, Validate `opus` (HIGH-complexity routing, risk floor forced `opus` for validation per [KB-091 §5.2](autonomous-runner.md#52-floors-that-override-the-table)). `M2-B12-01`: not yet routed. |
| **Next ready task** | `M2-B12-01`, selected per the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) — see `current-task.md` for the full ranking against the other three genuinely `Ready` P0 candidates (`M2-B04`, `M2-C10`, `M0-01-03`) and why `M2-B12-01` won the tie against `M2-C10` (finishing it makes `M2-B12-02` immediately `Ready`; finishing `M2-C10` does not make `M2-C07` `Ready`, since `M2-C05-01` still blocks it). `M2-B02`/`M2-B06`/`M2-B11` remain `Blocked` — `M2-A06`'s `Needs Review` status does not release a Hard-dependent successor. |
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
