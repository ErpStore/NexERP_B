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
| **Status** | `BLOCKED` |
| **Stop reason** | `M0-02` analysis half cannot run: `db/drift/` contains zero per-tenant fingerprint CSVs (Confirmed). Answering Q-14 requires a DBA with `VIEW DEFINITION` on ≥2 tenant databases plus a working tenant list (Q-12 unanswered), and this session may not acquire or reuse any database credential. The tooling half is complete and committed (`c1ab752`); the task is `Blocked` awaiting per-tenant fingerprints, exactly as the task specification prescribes for this case. |
| **Run started** | 2026-08-17 |
| **Last transition** | 2026-08-17 — `M0-02` tooling half implemented, committed on `migration/M0-02-sp-drift-across-tenants` (`c1ab752`), verified (synthetic-fixture harness tests, secret scan, build regression guard), and closed out: task file, `task-tracker.md`, investigation registry (INV-030 `Partial`), `open-questions.md` (Q-14 explicitly undecided), KB-103 all updated. Run halts here — no ready task remains (see below). |
| **Current task** | `M0-02` — Confirm stored-procedure drift across tenant databases (Q-14) |
| **Current phase** | `Blocked` — tooling half `Needs Review` (committed, unmerged); analysis half not started, awaiting DBA fingerprints |
| **Current agent** | n/a (session ended) |
| **Current model** | n/a |
| **Attempt** | 1 of 3 (`max_retries: 2`) |
| **Escalations** | 0 of 1 |
| **Last validation** | `M0-02` attempt 1 — validation did not complete (`verdict: none`, `"validation did not complete"`); the session stopped at the tooling-half handoff point per the task's own decision rule before a full validator pass ran. See [`failure-log.md`](failure-log.md) if a retry is warranted — this is an expected `Blocked` outcome, not a validation failure requiring a retry. |
| **Tasks processed this run** | 1 (`M0-02`, tooling half only, ended `Blocked`) |
| **Classification** | `M0-02`: `task_type: Investigation`, `complexity: MEDIUM`, `risk: LOW` — no frontmatter override; derived from a database-free tooling build (shell comparison harness + runbook + KB updates), 1 hard dependency already `Completed`, no `business_rules`, read-only source files, and an expected two-session shape (tooling now, analysis only once a DBA drops fingerprint CSVs). |
| **Models this run** | Not yet recorded — to be captured by the autonomous-runner state machine as the implementing session runs |
| **Blocked on** | `M0-02`: a DBA with `VIEW DEFINITION` on ≥2 tenant databases, plus a working tenant list (Q-12 unanswered) — see the task file's Execution record. Separately: `M0-04` `Blocked` on an unidentified human owner (production SQL/GST gateway access); `M0-07` `Blocked` pending `M0-15` and `M0-08` reaching `Completed` (both currently `Needs Review`, unmerged); `M0-03-02` `Blocked` pending `M0-03-01` review/merge. |
| **Owner to unblock M0-02** | DBA — first candidate operator **PavanKunar** (ran the M0-01-02 capture); migration lead must also resolve the baseline-tenant label ambiguity (see task file). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. |
| **Next ready task** | **None.** Re-derived at `M0-02`'s close-out per [`dependency-graph.md` § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule): `M0-02` is now `Blocked`, not `Ready`; `M0-03` is a parent container; `M0-04` is `Blocked` on an unidentified human owner; `M0-08`/`M0-03-01`/`M0-15` are `Needs Review`, not `Ready`. The tracker's `Ready` column is empty. `current-task.md` is left pointing at `M0-02` so a human or a later run resumes it rather than restarting, per the explicit close-out instruction for this session. |

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
