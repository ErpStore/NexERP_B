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
| **Stop reason** | M0-04 requires a human: Actual credential rotation (production SQL login, GST e-Invoice/e-Way gateway re-keying, production deployment) requires a named person with production access not identified anywhere in the repository. This session can deliver the runbook, credential inventory, and human verification checklist, but the rotation itself must end as Blocked, naming the owner, unless a human with that access participates in-session. |
| **Run started** | 2026-08-17 |
| **Last transition** | 2026-08-17 — Run halted. Task `M0-04` opened and investigated; human-executable portion blocks further progress. Deliverables (runbook, credential inventory, verification checklist) can be prepared and reviewed, but credential rotation and production deployment require a named person with production SQL Server and GST gateway access. That person is not yet identified in the repository. |
| **Current task** | `M0-04` — Rotate the exposed credentials |
| **Current phase** | `BLOCKED` (opened, human dependency identified) |
| **Current agent** | Halted pending human participation |
| **Current model** | n/a (not yet proceeding) |
| **Attempt** | 1 of 3 (`max_retries: 2`) |
| **Escalations** | 0 of 1 |
| **Last validation** | `M0-03-01` attempt 2 — `PASS` (2026-08-17, prior run). See [`failure-log.md`](failure-log.md). |
| **Tasks processed this run** | 0 (M0-04 selected and opened, human blocker identified; no full task completion this run) |
| **Classification** | `M0-04`: `task_type: Security` → base complexity `HIGH`. Raised further (though already at ceiling): `source_files` spans four projects (`V.SMART.Shared`, `V.SMART.Web`, `V.SMART.Api`, `V.SMART`); touches authentication/secrets/`appsettings*.json`. `risk: HIGH` (Security type; secrets/credentials; `appsettings*.json`). No level exists above `HIGH`. `risk: HIGH` forces `opus` for validation regardless of complexity; complexity `HIGH` also routes investigate/implement to `opus` per §5.1. `requiresHuman: true` — the task's own objective states most of its work (SQL login rotation, GST gateway re-keying, production deployment) is outside repository scope and requires a named person with production SQL / gateway access not currently identified anywhere in the repository; the AI-executable deliverables are the runbook, inventory, and verification checklist. `safetyStop: false` — nothing blocks *starting* the task; KB-091 §8 items 5 and 7 describe the expected partial-`Blocked` outcome for the rotation-execution portion, not a start-time block, and match the task's own documented completion conditions (docs deliverables can reach `Needs Review` even while rotation itself is `Blocked` on a named owner). |
| **Models this run** | `M0-04`: opus (risk HIGH forces opus for validation; complexity HIGH routes investigate/implement to opus too). |
| **Blocked on** | Identification of named person with production SQL Server and GST e-Invoice/e-Way gateway access required to execute credential rotation and production deployment. |
| **Owner to unblock** | Unknown. Must be identified from operations/infrastructure team. Once named, that person must participate in-session or rotation must remain Blocked pending their availability. |
| **Next ready task** | To be re-derived when M0-04 completes (either delivery of AI-executable docs and Blocked-pending-human-execution, or human participation enables full rotation). Current candidates: `M0-08`, `M0-02` (both P1); no other Ready tasks identified. |

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
