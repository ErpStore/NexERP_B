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
| **Stop reason** | M0-07: blocked during diagnosis (environment): Six M0-07 acceptance criteria are satisfiable only by pushing the branch to origin, a GitHub-hosted Actions run, a merge to master, and GitHub org admin rights on branch protection — all of which the runner's hard constraints and this workstation forbid or lack (no gh CLI, branch absent from origin, master carries no ci.yml); the pipeline, gate and docs themselves contain no defect. |
| **Run started** | 2026-08-17 |
| **Last transition** | 2026-08-17 — M0-07 attempt 1 validated `FAIL` (5 criteria unmet, 1 not checkable, all environment-caused; see the validator verdict recorded in `failure-log.md`), diagnosed as `blocked`/`environment` (not a same-spec-retry candidate per KB-091 §6.4), and closed out: `tasks/M0-07.md` gained an Execution Record and its frontmatter `status` was set to `Blocked`; `task-tracker.md` row and footnote⁷ updated; this file's Status set to `BLOCKED`. Work is committed on `migration/M0-07-ci-pipeline` (`5106929`), unmerged, not pushed. |
| **Current task** | `M0-07` — CI pipeline: restore → build → analyzers |
| **Current phase** | `BLOCKED` — pipeline/gate/docs built and locally verified; six criteria unmet pending human action (push + hosted-runner run + merge + branch-protection admin) |
| **Current agent** | none — awaiting human decision (options A/B/C in `failure-log.md`; option B taken here) |
| **Current model** | Not recorded this pass (close-out only; no implementer/validator model dispatched) |
| **Attempt** | 1 of 3 (`max_retries: 2`) |
| **Escalations** | 0 of 1 |
| **Last validation** | `FAIL` (environment) — see the validator verdict transcribed in `failure-log.md`, M0-07 attempt 1 |
| **Tasks processed this run** | 1 (M0-07, attempt 1, closed out `BLOCKED`) |
| **Classification** | `M0-07`: `task_type: DevOps`, `complexity: MEDIUM`, `risk: LOW` — no frontmatter complexity/risk override present. Derived from: `estimate: 2 d`; 5 `source_files` (solution + 4 `.csproj` + the verified-commands doc), all build/CI configuration, none of them application/business code; 2 Hard `depends_on` (`M0-15`, `M0-08`), both genuinely `Completed`; `business_rules: []`; no database or API surface touched; first CI pipeline for the repo (non-trivial to get green given the ~6,695-warning baseline and the untracked `V.SMART.Api/` directory trap) but fully reversible (a `.yml`/config-only change) and blocks no production data path. |
| **Models this run** | Not yet recorded — to be captured by the autonomous-runner state machine as the implementing session runs |
| **Blocked on (other tasks, informational)** | `M0-02` `Blocked` on a DBA with `VIEW DEFINITION` on ≥2 tenant databases plus a working tenant list (Q-12 unanswered) — unchanged. `M0-04` `Blocked` on an unidentified human owner (production SQL/GST gateway access). `M0-03-03` `Blocked` pending `M0-03-02`. `M0-05` `Blocked` pending `M0-03` + `M0-04`. `M0-07` `Blocked` (this run) on Q-20 + a human with push/merge/branch-protection-admin rights — see below. `M0-12`/`M0-12-01`/`M0-12-02`/`M0-13`/`M0-09`/`M0-06` remain `Blocked` pending `M0-07`. |
| **Owner to unblock M0-02** | DBA — first candidate operator **PavanKunar** (ran the M0-01-02 capture); migration lead must also resolve the baseline-tenant label ambiguity (see `tasks/M0-02.md`). |
| **Owner to unblock M0-04** | Unknown. Must be identified from operations/infrastructure team. |
| **Owner to unblock M0-07** | Migration lead / repository admin with GitHub organisation admin rights on `ErpStore` — not yet named in the KB. Needs to: (1) confirm Q-20 (hosted-runner minutes available?), (2) push `migration/M0-07-ci-pipeline`, (3) observe one green Actions run and regenerate `ci/warning-baseline.json` from the runner's own numbers, (4) merge to `master`, (5) add the CI check to branch protection. |
| **Next ready task (after M0-07)** | `M0-07` did not resolve this run (`BLOCKED`, environment). Re-derive the candidate set at the next session's start against `task-tracker.md`; `M0-03-02` (Ready, P0, off the critical path, no file overlap with `M0-07`) is the likely next candidate per the Ready-task selection rule, unless a human unblocks `M0-07` or `M0-02` first. |

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
