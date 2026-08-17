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
| **Stop reason** | **Branch-point safety stop before starting M0-15** (KB-091 §8). `migration/M0-15-build-baseline` was cut from `migration/M0-08-gitignore-build-output`, not from `master`: `e0a7092` (M0-08) is an ancestor of HEAD, and `git merge-base HEAD master` is `31cfa95` — an earlier commit. M0-08 is **`Needs Review`** (committed, unmerged), not `Completed`, per [KB-081 line 65](task-tracker.md). A build baseline measured on this unreviewed mixture is not reproducible, which is the precise failure M0-15 exists to prevent. Pre-documented as a safety stop in this file and in `current-task.md` before the run. |
| **Run started** | — |
| **Last transition** | 2026-08-17 — safety stop at selection phase |
| **Current task** | `M0-15` — Toolchain and build baseline (`READY` per KB-081; dependency-ready but **not start-ready** — blocked by the branch-point safety stop above) |
| **Current phase** | SELECT (selection completed; no implementation phase opened) |
| **Current agent** | — |
| **Current model** | — |
| **Attempt** | 0 of 3 (`max_retries: 2`) |
| **Escalations** | 0 of 1 |
| **Last validation** | none |
| **Tasks completed this run** | 0 |
| **Next task** | `M0-15` remains top-ranked once unblocked (P0, only dependency M0-00 is Completed). Do not silently switch to `M0-02` (P1, needs DBA access) — that would evade rather than resolve the stop. |
| **Blocked on** | M0-08's review status. M0-15 cannot produce a reproducible baseline while its branch sits on unmerged, unreviewed M0-08 work. Resolve by one of: (a) review and merge M0-08, move it to `Completed` in KB-081, then re-cut M0-15 from `master`; (b) re-cut `migration/M0-15-build-baseline` from `master` now, leaving M0-08 to separate review — this drops commits `461295c` and `bf42db1` (INFRA runner checkpoints) from the branch; (c) explicitly accept a baseline of known-compromised reproducibility and record that decision. |
| **Owner to unblock** | Repo maintainer / lead engineer. This is a branch-management decision, **not an implementation step of M0-15**, so no task session can take it unilaterally. Do not silently retarget to `M0-02` — that evades the stop rather than resolving it. |

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

Both are `safetyStop` conditions ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks))
and a run will halt on them rather than measure something unreproducible. **One is now resolved;
one still stands.**

- 🔴 **Branch point — OPEN.** `migration/M0-15-build-baseline` was cut from
  `migration/M0-08-gitignore-build-output`, not from `master`. A build baseline measured on an
  unreviewed mixture is not reproducible, which is exactly what M0-15 exists to prevent.
  Re-verified 2026-08-17: `git merge-base --is-ancestor e0a7092 HEAD` → true;
  `git merge-base HEAD master` → `31cfa95`; `git branch --contains e0a7092` lists
  `migration/M0-08-gitignore-build-output`. M0-08 is `Needs Review` at
  [KB-081 line 65](task-tracker.md). **This is what stopped the 2026-08-17 run.**
- ✅ **Dirty working tree — RESOLVED 2026-08-17.**
  `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs` and
  `V.SMART/V.SMART.Web/appsettings.json` were stashed as
  `PRE-M0-15: local tenant DB debugging …` (stash commit `6dbf4b47b8ff`) — local tenant-DB
  debugging work, not part of any task: null/empty-tenant guards in the factory, and a
  `MasterDb` connection string repointed at a local `.\SQLEXPRESS` / `NexGenErpDb_Master`.
  Recoverable with `git stash apply`; the stash holds full file contents, not just a diff.
  `V.SMART.Api/` remains untracked **by design** — see the untracked-directory checkout trap in
  `CLAUDE.md`; never stash or clean it.

Reconcile the open flag before starting a run, or the runner will stop here and record it again.
