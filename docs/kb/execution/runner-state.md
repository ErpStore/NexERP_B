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
| **Status** | `STOPPED` — resuming `M2-C00`. **Criterion 3 was RELAXED on `master` (`8b1a261`) and merged into this branch**: it no longer demands byte-equality with ADR-007's table, only substantive agreement with no contradiction, and it names additional clarifying detail as **allowed**. **Validate attempt 2 against the corrected criterion — do not re-edit KB-050 to satisfy the old one.** 
| **Stop reason** | n/a — not running. 
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-20 autonomous run through `M2-B02`, `M2-A01-02`, `M2-A01-03`, and now into `M2-C00`). |
| **Last transition** | 2026-08-20 — the prior `RUNNING` pointer at `M2-C04-02` was cleared (`ec70620`) because ADR-007 blocked that task behind `M2-C00`; re-applied the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) over the `Ready` pool in `task-tracker.md`. `M2-C00` is P0 and gates the entire 20-task `M2-C` tree — no other `Ready` candidate (`M2-A07`, `M2-A08`, `M2-B12-01`, `M2-B09`, `M2-B04`, `M0-01-03`) comes close on downstream unblocking, and `M2-A02` is `Ready` but gated on unanswered `Q-28`. `current-task.md` already pointed at `M2-C00` from the prior session's write; this run confirmed it against the tracker and re-selected the same task. Working tree confirmed clean, `HEAD` at `master`'s tip (`ec70620`) — no safety-stop condition. |
| **Current task** | **`M2-C00` — resume for VALIDATION, not re-implementation.** Attempt 1 validated `FAIL` on criterion 3; attempt 2's correction pass **is committed** (`6d0aebb`, `421646a`) but was **never validated** — the owner stopped the run before the validator ran. The work is done; what is missing is the verdict. 
| **Current phase** | `TESTING` — implementation complete on the branch, awaiting an independent validation pass. 
| **Current agent** | n/a — not yet dispatched |
| **Current model** | Implement `sonnet`, Validate `sonnet` (MEDIUM/LOW routing — see Classification row). |
| **Attempt** | `M2-C00`: **0 of 3** — selected, not started. Last closed task: `M2-A01-03`, **2 of 3** used, `PASS`, now `Completed` and merged. *(The count read "2 of 4" — the retry-budget denominator wrong for the sixth time. It is **three**: KB-091 §6.4 "Attempt 3 fails → BLOCKED … Do not attempt a fourth", and `migration-runner.js` `maxRetries: 2`.)* |
| **Escalations** | 0 |
| **Last validation** | `M2-A01-03`, tip `0fde6fb` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. All eighteen acceptance criteria independently re-checked `MET`: cache resolves through `IMemoryCache`; key `screenrights:v1:{tenantId}:{userId}`, tenant-scoped and prefixed; TTL from `Authorization:RightsCacheSeconds`, default 60 s, **absolute** expiration; explicit `Invalidate(tenantId, userId)` eviction, and the count of in-process `UserRight` write sites needing it is genuinely 0 (all five write sites confirmed in the Blazor host only); the cross-process gap is documented in three places; a zero-TTL bypass exists for `M2-A03`'s harness; a cache miss returns exactly what the repository produces, no reordering; a failing query is never cached. Re-run by the validator: `dotnet build V.SMART.Api --no-incremental` 0 errors/6,694 warnings (baseline); `dotnet test tests/V.SMART.Api.Tests` 117/117 passed (104→117, the growth is this task's cache suite); `dotnet test tests/V.SMART.Shared.Tests` 84/84 passed, no regression. `JwtTokenService.cs` unchanged, no controller annotated, nothing under `V.SMART.Shared/`/`V.SMART.Web/`/`V.SMART/V.SMART/` modified, no secret or migration touched. **Attempt 1 (`a78c51e`) regressed the test suite** — added `Invalidate` to `IUserRightsProvider` without updating two test stand-ins, `CS0535` × 2, 104 tests ran zero — diagnosed as `implementation-error`, fixed in attempt 2 (`0fde6fb`), re-validated clean. Full evidence: `tasks/M2-A01-03.md` § Execution Record (2026-08-20). No validation has run yet for `M2-C04-02`. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, `Needs Review`, then owner-merged and `Completed`. `M2-B12-01` — selected once, superseded before being started (twice now); not started, still `Ready` and unclaimed. `M2-B02` — implemented, validated `PASS` after one in-attempt retry, `Completed` and merged (`feec964`), released `M2-B09` only. `M2-A01-02` — implemented, validated `PASS`, `Completed` and merged (`ed559ad`), released `M2-A01-03`. `M2-A01-03` — implemented across 2 attempts, validated `PASS`, closed `Needs Review` (unmerged, `0fde6fb`) — does **not** yet release `M2-A02`/`M2-A07`/`M2-A08` pending merge. |
| **Classification** | `M2-A01-02` (closed) — complexity **HIGH**, risk **HIGH** (task_type Security, `business_rules` populated, `Program.cs` in `source_files`). `M2-A01-03` (closed) — complexity **HIGH**, risk **HIGH**, same grounds plus `business_rules: [BR-AUTH-002, BR-TEN-001, BR-TEN-002]` and two-project `source_files`. `M2-C00` (current) — `task_type: Documentation` → base **LOW** ([KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)); one raise applies under §4.2 (the task specifies auth flow/permission rendering and its `source_files` names `auth.service.ts`, `auth.guard.ts`, `auth.interceptor.ts` — "touches authentication, authorisation" applies even though no code is written), giving complexity **MEDIUM**. `estimate: 2 d` (< 3 d, no raise), `depends_on: [G0]` (1 dependency, no raise), `business_rules: []` (no raise), `source_files` are all frontend paths, not spanning the four .NET projects (no raise). Risk: **LOW** — Documentation-type, writes nothing but KB prose (KB-050), no schema/secrets/`Program.cs`/`appsettings*`, `business_rules` empty, no live-observable behaviour change, matching the explicit LOW row in [KB-091 §4.3](autonomous-runner.md#43-risk). |
| **Models this run** | `M2-A01-02`: Implement `opus`, Validate `opus`. `M2-A01-03`: Implement `opus`, Validate `opus` (same HIGH/HIGH routing). `M2-C00`: Implement `sonnet`, Validate `sonnet` (MEDIUM complexity row of [KB-091 §5.1](autonomous-runner.md#51-the-routing-table); risk LOW sets no floor). |
| **Next ready task** | `M2-C00` is now the selected/current task (see above), so "next" refers to what follows it. After `M2-C00` lands, `M2-C01` (re-scoped to Angular, `Ready`) is unblocked, and the remaining 25 `M2-C*`/`M2-D*` tasks stay `Blocked` pending their own re-specification (out of `M2-C00`'s scope). Other still-`Ready`, independent candidates a session could pick instead: `M2-A07`, `M2-A08`, `M2-B12-01`, `M2-B09`, `M2-B04`, `M0-01-03`; and `M2-A02`, which is `Ready` but **gated on Q-28** — an API-only administrator holds zero `UserRight` rows. 
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
