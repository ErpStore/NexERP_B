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
| **Stop reason** | Clean end. `M2-B07`'s one unmet criterion was waived by the owner and the branch merged to `master` as `ffbb1dd`. No run is live. |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — **`M2-B07` `BLOCKED` → `Completed`.** The owner waived the render criterion on the `M2-C01` / `M0-12-02` / `M0-07` precedent and instructed the merge in-conversation. Verified independently *before* merging: `dotnet test` **84 passed** (79 + 5 new), `dotnet build V.SMART.Api` **0 errors**, and `V.SMART.Web` serving `GET /` → **200 with zero DI resolution errors**. Post-merge on `master`: 84 passed, 0 errors. |
| **Current task** | None. `M2-B07` closed. |
| **Current phase** | Idle — awaiting the next selection. |
| **Current agent** | n/a |
| **Current model** | n/a |
| **Attempt** | n/a. **Corrected:** the close-out recorded "3 of 3 exhausted"; the true count is **two** real implement/validate cycles plus **one dispatch lost to `ENOTFOUND`**, which per the `M0-12-01` precedent does not consume budget. The task closed on an owner waiver, **not on budget exhaustion**, with one attempt still in hand. |
| **Escalations** | 0 |
| **Last validation** | `M2-B07` — validator verdict `FAIL`, `failureCategory: environment`, every mechanical criterion `MET`. **The verdict was never overturned.** The task closed because the owner waived the one unmet criterion, which is a different thing from the validator passing it. Recorded as a waiver in `task-tracker.md` footnote ²⁰. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged |
| **Classification** | n/a — no task selected |
| **Models this run** | Implement: `opus`. Validate: `opus`. |
| **Blocked on** | Nothing, for selection purposes — nine M2 tasks are `Ready`, plus `M0-01-03` and `M0-10`. Standing owner-owned items: `M0-04` `Blocked` (no identified owner, **and now gated on Q-32** — rotating `sa` would break every `Tenants` row that embeds it); `M0-06` `Blocked` on Q-25/Q-26; `M0-11` `Ready` but a `Product Decision`, owner-only and never runner-selectable ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)); `M2-A01-02` nominally `Ready` but its spec contradicts reality — see `current-task.md`. |
| **Next ready task** | Not pre-selected — the runner selects per [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule). Newly released by `M2-B07`: **`M2-B04`**, **`M2-B01`**, **`M2-B05`**, **`M2-B12-01`**. Already `Ready`: `M2-A06`, `M2-C04-01`, `M2-C10`, `M2-C11`, `M2-A01-02` (blocked in practice). Carried M0 debt: **`M0-01-03`** (`Ready` as of 2026-08-19 — the rebuild drill is no longer hardware-blocked, see footnote ²¹) and `M0-10`. `M2-B12` and `M2-C04` are **parent containers** and are never worked directly. |
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
