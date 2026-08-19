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
| **Stop reason** | `M0-12-01` attempt 3 of 3 implemented real work (commit `9557de2`, branch `migration/M0-12-01-test-project`) and the validator returned `FAIL`: 10 of 11 acceptance criteria are met, but criterion 6 requires observing a deliberately-failing test turn a GitHub Actions run red, which requires **pushing** the branch. Pushing is forbidden to an execution session (`CLAUDE.md` "Never merge or push without an explicit instruction in the current conversation"; runner dispatched with `allowMerge=false`), and no local substitute exists (no `gh`, no `act`, no docker on this workstation). Retry budget is exhausted (3 of 3) and a fourth dispatch would reproduce the identical commit and stop at the identical wall — this is an authority gap, not a defect, so it is not diagnosed further. |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — attempt 3 validated `FAIL` (`failureCategory: environment`) on criterion 6 only; debugger concurred (`disposition: blocked`); close-out session recorded the outcome and returned `BLOCKED` with `nextTaskId: ""`. |
| **Current task** | `M0-12-01` — Create the test project and wire it into CI — **`Blocked`** on the repository owner. See [`tasks/M0-12-01.md` § Execution Record (2026-08-19)](tasks/M0-12-01.md#execution-record-2026-08-19). |
| **Current phase** | `BLOCKED` |
| **Current agent** | n/a — no run is live |
| **Current model** | n/a this cycle — attempt 3 used Implement `opus`, Validate `opus`, Diagnose `opus` (Classification: `task_type: Testing` → base MEDIUM, raised to **HIGH** because 4 tasks — `M0-12-02`, `M0-13`, `M0-09`, `M0-06` — name `M0-12-01` in `depends_on`, per KB-091 §4.2) |
| **Attempt** | **3 of 3 used — budget exhausted.** Attempt 1 (2026-08-18): both agents `529 Overloaded`, nothing implemented. Attempt 2 (2026-08-18): empty implementer return, nothing implemented. Attempt 3 (2026-08-19): real implementation, validated `FAIL` on criterion 6 alone (push authority). Per [KB-091 §6.4](autonomous-runner.md#64-retry-rules) three failed attempts → `BLOCKED`, stop, do not attempt a fourth. A fourth dispatch would not help: the blocker is authority (push), not code quality, and the same commit would recur. |
| **Escalations** | 0 of 1 (this cycle) — the debugger classified this as `environment`/`blocked`, not something a diagnose-and-retry cycle can fix, so no escalation to a stronger model was applied |
| **Last validation** | `M0-12-01` — verdict `FAIL`, attempt 3 of 3. 10 of 11 acceptance criteria independently re-verified `MET` (test project builds/targets net9.0/references Shared; `.sln` has all 6 platform config rows; `dotnet test` → 11 discovered, 11 passed, 0 failed; `dotnet build V.SMART.Api` → 0 errors, 6,695 warnings, at baseline; `git status --porcelain` clean of `bin/`/`obj/`/`.vs/`/`*.user`; INV-031 complete with Confirmed/Inferred/Unknown tags; INV-031 states EnsureCreated applies HasData seeds and the StockAdd FK requirement; KB-083's stale sentence removed and replaced with a measured command row; `git diff --stat` shows zero files under `V.SMART/`; the "fixture could not be built" fallback is not applicable — the fixture built). Criterion 6 (CI observed red on push) `NOT MET` — never performed, no branch pushed. Full record: [`tasks/M0-12-01.md` § Execution Record (2026-08-19)](tasks/M0-12-01.md#execution-record-2026-08-19). The most recent `PASS` remains `M0-14` (Vivek sign-off, merge `275c6e2`). |
| **Tasks processed this run** | `M0-12-01` — attempted, `BLOCKED` (not `Completed`; a human decision is required before the branch can be pushed and the last criterion checked) |
| **Classification** | `task_type: Testing` → base `MEDIUM` ([KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)). Raise applied: 4 tasks (`M0-12-02`, `M0-13`, `M0-09`, `M0-06`) name `M0-12-01` in their `depends_on` — ≥3 tasks naming this one ([KB-091 §4.2](autonomous-runner.md#42-raise-one-level-for-each-of-these-that-is-true)) → **complexity HIGH**. No other raise applies: `estimate` is 0.5 d (not ≥3 d); `depends_on` names only 1 task (`M0-07`); `business_rules: []` in frontmatter; `source_files` all sit under the single project `V.SMART.Shared`; the task does not modify authn/authz/tenancy/numbering/calculation logic. **Risk: MEDIUM (default).** |
| **Models this run** | Per [KB-091 §5.1](autonomous-runner.md#51-the-routing-table) at complexity HIGH: Investigate `opus`, Implement `opus`, Validate `opus`, Diagnose (first failure) `opus`, Diagnose (escalated) `opus`. |
| **Blocked on** | **Nothing that blocks selection.** `M0-12-01` is `Completed` and merged (`bdee81f`) on the owner's instruction; Q-22 resolved as option (A) and criterion 6 was verified on a hosted runner. Four tasks are now `Ready` — `M0-12-02`, `M0-13`, `M0-09`, `M0-06` — and the runner may open one. Still human-blocked, and still barring **G0** (so M2 remains shut): `M0-04` `Blocked`⁴ (unidentified owner, production SQL / GST gateway access — also blocks `M0-05`); `M0-01-03` `Needs Review` (repo-side work merged; only a human-executed rebuild drill remains, a hard G0 exit criterion). |
| **Owner to unblock M0-12-01** | **Vivek (repository owner)** — same named owner as the identical gap on `M0-07`'s CI criterion (Q-20). Needs to choose option A or B above; see Q-22 in [`open-questions.md`](../open-questions.md). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. Once rotated, also unblocks `M0-05` (purge secrets from git history), whose other Hard prerequisite (`M0-03`) is already `Completed`. |
| **Owner to unblock M0-01-03** | Repository owner — needs to run/record the rebuild drill against a real, disposable SQL Server instance (`db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`); see `tasks/M0-01-03.md`. This is a hard G0 exit criterion. |
| **Next ready task** | **Four are `Ready`** following the `M0-12-01` merge (`bdee81f`): **`M0-12-02`** (characterisation tests for `CalculationService`, P0, 2.5 d), **`M0-13`** (characterisation tests for `StockManagerService`, P0, 3 d), **`M0-09`** (unreachable delete guards, R-08, P1, 0.5 d), **`M0-06`** (seeded default Administrator credential, P1, 1 d). `M0-10` stays `Blocked` behind `M0-09`, `M0-11` behind `M0-13`. **`M0-12-02` and `M0-13` are the two G0 actually names** — the characterisation tests the gate asks for — so P0 order is not merely nominal here. Apply the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) rather than assuming this list is still current. |

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
