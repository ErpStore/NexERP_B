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
| **Status** | `RUNNING` — autonomous run started 2026-08-20 by owner instruction (`/migration-runner`). **Three branches are validated `PASS` and awaiting owner merge, and none is on `master`:** `M2-C00` (`migration/M2-C00-kb050-angular-rewrite`, `b3c0e6e`), `M2-A07` (`migration/M2-A07-me-endpoint`, `61da4bd`) and now `M2-A08` (`migration/M2-A08-row-scope-and-account-gates`, `0706263`). **`M2-B12-01`'s sibling branch concluded `BLOCKED`** (`migration/M2-B12-01-inv-012-numbering`, tip `407d0ba`, "escalation budget exhausted") — also unmerged, not selectable, and its tracker row here still stale-reads `Ready` until reconciled. None of the four releases its dependents until merged ([selection rule](dependency-graph.md#ready-task-selection-rule) step 1). 
| **Stop reason** | n/a — running. 
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-20 autonomous run through `M2-B02`, `M2-A01-02`, `M2-A01-03`, `M2-A08`, and now selecting `M0-01-03`). |
| **Last transition** | 2026-08-20 — `M2-A08` closed `Needs Review` (validated `PASS`, unmerged, tip `0706263`); re-applied the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) and selected `M0-01-03`, writing it into `current-task.md`. Not yet started — no branch exists yet. |
| **Current task** | None yet — awaiting the Select phase of this run. `M0-01-03` is the next candidate; not yet dispatched. |
| **Current phase** | Idle — awaiting selection. 
| **Current agent** | n/a — not yet dispatched |
| **Current model** | Implement/Validate routing not yet computed for `M0-01-03` — TBD at dispatch. |
| **Attempt** | n/a — no task in flight. Last closed: `M2-A08`, **1 of 3** used, `PASS`, now `Needs Review`, unmerged. |
| **Escalations** | 0 |
| **Last validation** | `M2-A08`, tip `0706263` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. All acceptance criteria independently re-checked `MET`, with two items reported as observations rather than failures: the "empty scope → `200`" criterion is provable only at the query/`PagedResult` level, since no scoped endpoint exists yet by this task's own scope; and `RowScopeStartupValidatorTests.The_APIs_own_actions_all_pass_today` exercises stub action descriptors, not the API's live action table. Re-run by the validator: `dotnet build V.SMART.Api` 0 errors/6,695 warnings (baseline, matches KB-086); `dotnet test tests/V.SMART.Api.Tests` 174/174 passed; `dotnet test tests/V.SMART.Shared.Tests` 88/88 passed; `dotnet build V.SMART.Web` 0 errors (Blazor host intact); `git diff --stat -- V.SMART/V.SMART.Shared/Pages/ V.SMART/V.SMART.Shared/BusinessLayer/` produced no output, the task's hard stop. `JwtTokenService.cs` unchanged; the only `V.SMART.Shared/` change is the one `GetUserByQrToken` query (+23/-1); no EF migration. Q-08's `Inferred` claim corrected by a negative grep the validator independently reproduced. Full evidence: `tasks/M2-A08.md` § Execution Record (2026-08-20) — validation close-out. No validation has run yet for `M0-01-03`. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, `Needs Review`, then owner-merged and `Completed`. `M2-B12-01` — implemented (INV-012 run) then closed `BLOCKED`, escalation budget exhausted, on its own unmerged branch; not on `master`. `M2-B02` — implemented, validated `PASS` after one in-attempt retry, `Completed` and merged (`feec964`), released `M2-B09` only. `M2-A01-02` — implemented, validated `PASS`, `Completed` and merged (`ed559ad`), released `M2-A01-03`. `M2-A01-03` — implemented across 2 attempts, validated `PASS`, closed `Needs Review` (unmerged, `0fde6fb`), released `M2-A02`/`M2-A07`/`M2-A08` on the tracker (pending merge). `M2-A08` — implemented and validated in one pass, `PASS` on attempt 1 of 3, closed `Needs Review` (unmerged, `0706263`) — releases `M2-D01` once merged. |
| **Classification** | `M2-A01-02`/`M2-A01-03` (closed) — complexity **HIGH**, risk **HIGH** (task_type Security, `business_rules` populated). `M2-A08` (closed) — complexity **HIGH**, risk **HIGH**, same grounds: `task_type: Security`, `business_rules: [BR-AUTH-001, BR-AUTH-002, BR-TEN-002]`, two-project `source_files` (`V.SMART.Shared` + `V.SMART.Api`). `M0-01-03` (next) — not yet classified; `task_type: Database`, small estimate (1 d) — to be computed at dispatch. |
| **Models this run** | `M2-A01-02`/`M2-A01-03`: Implement `opus`, Validate `opus`. `M2-A08`: same HIGH/HIGH routing (Security-task floor, [KB-091 §5.2](autonomous-runner.md#52-floors-that-override-the-table)). `M0-01-03`: TBD at dispatch. |
| **Next ready task** | **`M0-01-03`** — deployment script + rebuild runbook. Selected per the [selection rule](dependency-graph.md#ready-task-selection-rule): P0, genuinely `Ready` (footnote ²¹ — the SQL Server Express instance that blocked it is confirmed present), on the M0 critical path (`M0-01-01 → M0-01-02 → M0-01-03`), 1-day estimate, no sibling branch open on it. Excluded from the candidate set: `M2-C00`, `M2-A07`, `M2-A08` (all `Needs Review`, unmerged, do not release dependents), `M2-B12-01` (sibling branch already concluded `BLOCKED`), `M2-C01`/`M2-C04-02` and the rest of the `M2-C*`/`M2-D*` tree (⛔ STOP banners, [ADR-007](../decisions/ADR-007-angular-stack.md) re-specification pending), `M2-A02` (`Ready` but gated on unanswered **Q-28**), `M2-B03`/`M2-B08` (`Blocked` on unmerged prerequisites). Remaining P0/P1 `Ready` pool after `M0-01-03`: `M2-B04`, `M2-B01`, `M2-B05`, `M2-B06`, `M2-B09`, `M2-B11`. |
| **Process note — id allocation** | **Four cross-branch id collisions have now occurred, all on 2026-08-19** — six KB/INV/Q ids, `M2-C01`'s footnote ¹⁸, and a `Q-31` double-claim caught during `M2-B07`'s merge bookkeeping (`Q-31` was already held by `M2-B07` itself; the new question became **Q-32**). Every one was caught by hand at merge, which is not a control. `grep`-before-claim cannot see a sibling branch, and it cannot see an id claimed earlier in the same session. `git branch --no-merged master` must be checked before claiming any id. This recurs until the allocation rule itself changes. `M2-A08` claimed **KB-108** (not KB-100, already taken by `M2-B12-01`) and **Q-37**/**Q-38**, each re-grepped before claiming — no collision this time. |

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
