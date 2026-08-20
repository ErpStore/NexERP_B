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
| **Status** | `STOPPED` — no run is live. **`M2-A07` (`GET /api/v1/me`) implemented and independently validated `PASS`** on `migration/M2-A07-me-endpoint` (`61da4bd`), closed `Needs Review`, **not merged**. It does **not** release `M2-C02` until merged ([selection rule](dependency-graph.md#ready-task-selection-rule) step 1: Hard prerequisites need `Completed`, not `Needs Review`). 
| **Stop reason** | n/a — not running. 
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-20 autonomous run through `M2-B02`, `M2-A01-02`, `M2-A01-03`, `M2-A06`, `M2-C04-01`, and now `M2-A07`). |
| **Last transition** | 2026-08-20 — `M2-A07` closed `Needs Review` (validated `PASS`, unmerged, on top of `master` tip `8b1a261`, which already carries the `ADR-007` Angular pivot and `M2-A01-03`'s merge). Re-applied the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule): `M2-C00` remains the top-ranked candidate (P0, gates the entire 20-task `M2-C` tree; no other `Ready` candidate — `M2-A08`, `M2-B12-01`, `M2-B04`, `M2-B09`, `M0-01-03` — comes close on downstream unblocking), so `current-task.md` is unchanged in its active-task pointer, updated only to record `M2-A07`'s outcome. |
| **Current task** | None — `M2-C00` remains `Ready` and unclaimed. 
| **Current phase** | Idle — awaiting selection. 
| **Current agent** | n/a — not yet dispatched |
| **Current model** | Implement/Validate routing not yet computed for `M2-C00`. |
| **Attempt** | n/a — no task in flight. Last closed: `M2-A07`, reported as **1 of 4** by the validator (the correct denominator remains **three** per KB-091 §6.4 / `migration-runner.js` `maxRetries: 2` — the reporting quirk noted here since footnote-tracking began recurs), `PASS`, 0 escalations, `Needs Review` (unmerged). |
| **Escalations** | 0 |
| **Last validation** | `M2-A07`, tip `61da4bd` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. Sixteen acceptance criteria checked: fourteen `MET` (including two the validator observed **over the wire** against a live host that the implementer had reported as not-checkable — no-token `401`, and a real token's `200` with 150 rights keys matching the tenant database exactly); one `NOT MET AS WRITTEN` and unsatisfiable until `M2-A02` annotates `CurrencyController` (gated on `Q-28`); one `MET` on its written reason but with no harness to run it against (`M2-A03` `Blocked`) — both treated per the `M2-A06` precedent, not as failures. `dotnet build V.SMART.Api`: 0 errors, 6,695 warnings (exact baseline, re-run by the validator). `dotnet test tests/V.SMART.Api.Tests`: 148/148 (117→+31). `dotnet test tests/V.SMART.Shared.Tests`: 84/84, no regression. `git diff --stat HEAD~1 HEAD`: 9 files, +1184/-13, nothing outside `V.SMART.Api/Controllers/MeController.cs` + tests + docs. Full evidence: `tasks/M2-A07.md` § Execution Record (2026-08-20). New risks raised: **R-43** (API test project has no HTTP-level test capability — the implementer's own finding) and **R-44** (a live-probed cross-tenant read path through `TenantProvider`'s host-based fallback composing with `UserRightsProvider`'s claimed-tenant cache key — the validator's own finding, tracked with **Q-37**). No validation has run yet for `M2-C00`. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, `Needs Review`, then owner-merged and `Completed`. `M2-B12-01` — selected once, superseded before being started (twice now); not started, still `Ready` and unclaimed. `M2-B02` — implemented, validated `PASS` after one in-attempt retry, `Completed` and merged (`feec964`), released `M2-B09` only. `M2-A01-02` — implemented, validated `PASS`, `Completed` and merged (`ed559ad`), released `M2-A01-03`. `M2-A01-03` — implemented across 2 attempts, validated `PASS`, `Completed` and merged (`edcf126`), released `M2-A02` (gated on Q-28), `M2-A07`, `M2-A08`. `M2-A07` — implemented, validated `PASS`, closed `Needs Review` (unmerged, `61da4bd`) — does **not** yet release `M2-C02` pending merge. |
| **Classification** | `M2-A01-02` (closed) — complexity **HIGH**, risk **HIGH** (task_type Security, `business_rules` populated, `Program.cs` in `source_files`). `M2-A01-03` (closed) — complexity **HIGH**, risk **HIGH**, same grounds plus `business_rules: [BR-AUTH-002, BR-TEN-001, BR-TEN-002]` and two-project `source_files`. `M2-A07` (closed) — `task_type: Backend`, `business_rules: [BR-AUTH-002, BR-TEN-002]` populated, `Program.cs`-adjacent `source_files` → **HIGH**/**HIGH** by the same grounds. `M2-C00` (next candidate) — not yet classified; `task_type: Documentation`, `business_rules: []` — to be computed at dispatch. |
| **Models this run** | `M2-A01-02`: Implement `opus`, Validate `opus`. `M2-A01-03`: Implement `opus`, Validate `opus` (same HIGH/HIGH routing). `M2-A07`: Implement/Validate routing not recorded by this close-out session (it only ran the close-out, not the implement/validate dispatch) — see `tasks/M2-A07.md` if it is needed. `M2-C00`: TBD at dispatch. |
| **Next ready task** | **`M2-C00`** — re-applied the [selection rule](dependency-graph.md#ready-task-selection-rule) after `M2-A07` closed: P0, gates all 20 frontend `M2-C*` tasks, no other `Ready` candidate comes close on downstream unblocking. Written into `current-task.md`, unchanged from before this close-out. Also `Ready` and unclaimed: `M2-A08` (P0, released by `M2-A01-03` — read `R-44`/`Q-37` first), `M2-B12-01`, `M2-B04`, `M2-B09`, `M0-01-03`; and `M2-A02`, `Ready` but **gated on Q-28** — an API-only administrator holds zero `UserRight` rows. 
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
