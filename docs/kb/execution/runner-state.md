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
last_verified: 2026-08-16
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
| **Status** | `STOPPED` |
| **Stop reason** | Never started — the orchestration mechanism was installed but no run has been launched |
| **Run started** | — |
| **Last transition** | 2026-08-16 — runner installed |
| **Current task** | `M0-15` — Toolchain and build baseline (`READY`, not yet opened by a run) |
| **Current phase** | — |
| **Current agent** | — |
| **Current model** | — |
| **Attempt** | 0 of 3 (`max_retries: 2`) |
| **Escalations** | 0 of 1 |
| **Last validation** | none |
| **Tasks completed this run** | 0 |
| **Next task** | selected at completion time by [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) — not pre-computed, because status moves |
| **Blocked on** | — |
| **Owner to unblock** | — |

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
and a run will halt on them rather than measure something unreproducible:

- **Branch point.** `migration/M0-15-build-baseline` was cut from `migration/M0-08-…`, not from
  `master`. A build baseline measured on an unreviewed mixture is not reproducible, which is
  exactly what M0-15 exists to prevent.
- **Dirty working tree.** `V.SMART/V.SMART.Shared/Services/MultiCompanyService/TenantDbContextFactory.cs`
  and `V.SMART/V.SMART.Web/appsettings.json` are modified; `V.SMART.Api/` is untracked. M0-15's
  first implementation step requires a clean tree.

Reconcile both against M0-00's documented quarantine list before starting a run, or the runner
will stop here and record it.
