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
| **Stop reason** | `M0-06` escalated per KB-091 §6.3 trigger 2 (architecture/design decision required) and trigger 7 (validator `failureCategory: architecture`). Acceptance criterion 2 (*"no default administrator credential is seeded into a newly created tenant database"*) cannot be met from inside a migration under this task's own constraints — `InitialCreate.cs:7562` re-inserts the seeded credential on every migration replay (the only tenant-provisioning path the repository supports; nothing calls `Migrate()`/`MigrateAsync()`/`EnsureCreated()`), migration history may never be edited, and a migration `Up()` cannot distinguish a freshly provisioned database from a live tenant whose only administrator may be this account (Q-25, Unknown). The task's own `Dependencies` table names *"a deployment owner"* as an unsatisfied **Hard** dependency it may not silently resolve on its own authority. Escalated as **Q-26** with three options; no AI session may choose among them. |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — `M0-06` implemented on `migration/M0-06-remove-default-admin` (`5b12573` seed removal + runbook + tests; `4fb8781` Q-26 escalation), validated **`FAIL`** (`scopeOk: true`, `failureCategory: architecture`), diagnosed, and correctly escalated rather than retried — a same-spec retry would reproduce the identical structural wall. 14 of 16 acceptance criteria independently re-verified `MET` (85/85 tests; `dotnet build V.SMART.Api --no-incremental` 6,694 warnings/0 errors, under the 6,695 baseline; hash confined to 109 pre-existing migration files; `UserRepository.cs`, the `Screens` seed and the FK `Restrict` loop untouched). Criterion 2 is `NOT MET` on the migration-replay path. Closed out this session as `Blocked`¹⁶, not `Needs Review` — the gap is a decision, not a diff awaiting review. `task-tracker.md` (footnote 16 + rollup note), `tasks/M0-06.md` (Execution Record already present, unchanged), `failure-log.md` (validator + diagnosis entries), `open-questions.md` (Q-25, Q-26, already recorded by `4fb8781`), `technical-debt-register.md` (R-09, R-40, already recorded by `5b12573`/`4fb8781`) all consistent. `current-task.md` (KB-089) updated to point at this blocked state rather than re-selecting a new task, per this session's record-outcome-only instruction. |
| **Current task** | `M0-06` — Remove the seeded default Administrator credential. See [`tasks/M0-06.md`](tasks/M0-06.md). Implemented, validated `FAIL`/`architecture`, escalated, closed `Blocked`. Not re-selectable until the owner answers Q-26. |
| **Current phase** | `ESCALATED` → closed out `BLOCKED`. This close-out session's instruction was record-outcome-only; no further implementation was attempted. |
| **Current agent** | n/a — no agent dispatched this session |
| **Current model** | n/a — this session recorded the outcome only; see `Models this run` below for the routing already used on `M0-06`'s attempt |
| **Attempt** | 2 of 3 |
| **Escalations** | 1 |
| **Last validation** | `M0-06` — verdict **`FAIL`**, attempt 2 of 3, 1 escalation, `scopeOk: true`, `failureCategory: architecture`. Full record: [`tasks/M0-06.md` § Execution Record (2026-08-19)](tasks/M0-06.md#execution-record-2026-08-19); [`failure-log.md` § M0-06 · attempt 1](failure-log.md#m0-06--attempt-1--2026-08-19) and its diagnosis entry; `task-tracker.md` footnote 16. |
| **Tasks processed this run** | `M0-06` — implemented, validated `FAIL`, diagnosed, escalated (Q-26), closed `Blocked` on the repository owner. |
| **Classification** | `M0-06` — **complexity `HIGH`**, **risk `HIGH`**, per [KB-091 §4](autonomous-runner.md#4-classifying-a-task). Base complexity from `task_type: Security` is already `HIGH` (§4.1); the §4.2 raises (`business_rules: [BR-AUTH-001, BR-AUTH-002]` non-empty; touches `ApplicationDbContext.cs`/`Migrations/`; is an authentication/authorisation surface) don't move it further — HIGH is the ceiling. Risk `HIGH` under §4.3 on three independent grounds: `task_type: Security`; a credential/secret surface; `business_rules` populated. The initial `requiresHuman: false` call held for the *engineering* half of the task (which was in fact completed and validated); it is the *outcome*, not the up-front classification, that surfaced the human-decision dependency — recorded here so a future session does not read `requiresHuman: false` as a claim that no human step was ever needed. `safetyStop`: false throughout — no untracked-directory or branch-cut trap was hit. |
| **Models this run** | `M0-06` (HIGH complexity, HIGH risk) per [KB-091 §5](autonomous-runner.md#5-model-routing): Investigate `opus`, Implement `opus`, Validate `opus` (also forced by the `risk: HIGH` floor at §5.2 rule 2), Diagnose (first failure) `opus`, Diagnose (escalated) `opus` — all as used. |
| **Blocked on** | `M0-06` itself is now `Blocked` on the repository owner deciding **Q-26** (tenant-provisioning path) and answering **Q-25** (is `UserId=1` some tenant's only administrator?) — see Stop reason above. Also still human-blocked, and still barring **G0** (so M2 remains shut): `M0-04` `Blocked`⁴ (unidentified owner, production SQL / GST gateway access — also blocks `M0-05`); `M0-01-03` `Needs Review` (repo-side work merged; only a human-executed rebuild drill remains, a hard G0 exit criterion); `M0-09` `Needs Review` (awaiting owner review/merge — blocks `M0-10` until merged); `M0-11` `Blocked` on the owner (Q-01 product decision, released by `M0-13`'s merge but not runner-selectable). |
| **Owner to unblock M0-06** | Repository/deployment owner (Vivek) — answer Q-25 (per-tenant diagnostic in [KB-104 § 3](security/default-admin-removal-runbook.md#3)) and decide Q-26's option A/B/C (`open-questions.md`). Until then `M0-06` stays `Blocked` and is not runner-selectable; its branch `migration/M0-06-remove-default-admin` is not merged. |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. Once rotated, also unblocks `M0-05` (purge secrets from git history), whose other Hard prerequisite (`M0-03`) is already `Completed`. |
| **Owner to unblock M0-01-03** | Repository owner — needs to run/record the rebuild drill against a real, disposable SQL Server instance (`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`); see `tasks/M0-01-03.md`. This is a hard G0 exit criterion. |
| **Owner to unblock M0-09** | Repository owner — review and merge `migration/M0-09-delete-guard-fix` (`8e3b19d`), validated `PASS`. Once merged, `M0-10` (INV-025, delete-guard audit) becomes `Ready`. |
| **Owner to unblock M0-11** | Repository owner — `M0-13` is `Completed` and merged (`9b57552`), so `M0-11` (the Q-01 product decision) is released; it needs an owner decision, not runner work — no runner may self-select a `Product Decision` task ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)). |
| **Next ready task** | **None.** `M0-06` closed `Blocked`, not `Ready`. `M0-10` (P1, 2 d) stays `Blocked` behind `M0-09`'s review/merge — the only other candidate the `M0-12-01` merge released. No dependency-ready, human-unblocked task remains this run; see `current-task.md` for the full accounting. |

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
