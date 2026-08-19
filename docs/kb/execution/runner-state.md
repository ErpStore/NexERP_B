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
| **Stop reason** | `M2-B07` attempt 1 — the implementer agent returned **no result** (no diff, no text, no tool output). The validator correspondingly returned `{"verdict": "none", "note": "validation did not complete"}`. The attempt did leave real, substantial work uncommitted in the working tree; a separate close-out session preserved it as-is in commit `a071716` on `migration/M2-B07-add-vsmart-domain` and ran spot-check builds only (0 errors, at-baseline warnings on `V.SMART.Api` and `V.SMART.Web`; the MAUI head's non-Android targets clean, its Android target's single error attributable to the close-out session's own build timeout). No test run, no `ValidateOnBuild`, no acceptance criterion checked. Full record: `tasks/M2-B07.md` § Execution Record (2026-08-19), `failure-log.md` § M2-B07 · attempt 1 · 2026-08-19. |
| **Blocked-on kind** | **Blocked on a task, not on a human.** Attempts used: 1 of 4 — retry budget is not exhausted. This does not need an owner decision to unblock, it needs a re-dispatch. Per the `M0-12-01` precedent (`task-tracker.md` footnote ¹²), a *single* no-result attempt is not itself the escalation signal — a second consecutive no-result attempt on the same task would be. If that happens, escalate to **Vivek** (repository owner) as the named human owner, the same as `M0-12-01`. |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — `M2-B07` attempt 1 stopped (implementer no-result) and the run halted `BLOCKED` rather than self-retrying, because this close-out is a separate session from the one that dispatched attempt 1 and retry dispatch is the next session's job, not this one's. |
| **Current task** | `M2-B07` — Shared `AddVSmartDomain()` DI extension |
| **Current phase** | `IMPLEMENTATION` — attempt 1 stopped short of a report; work-in-progress preserved on branch, not validated |
| **Current agent** | n/a — this session is close-out only, not a dispatch |
| **Current model** | n/a |
| **Attempt** | 1 of 4 |
| **Escalations** | 0 |
| **Last validation** | `M2-B07` — validator verdict `none`, `"validation did not complete"`. Before that, `M2-C01` — validator verdict `FAIL`, `failureCategory: environment`, 14/15 `MET`, waived by the owner (`task-tracker.md` footnote ¹⁹). |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01` — all `Completed` and merged. `M2-B07` selected and attempted (attempt 1, stopped, `Blocked`). |
| **Classification** | `M2-B07` — `task_type: Backend` (base `MEDIUM`) → raised to **`complexity: HIGH`**: `estimate` is 3 d (raise), 7 tasks (`M2-B01`, `B04`, `B05`, `B06`, `B08`, `B09`, `B12-03`) name it in `depends_on` (raise, ≥3), `source_files` spans all four projects — `V.SMART.Shared`, `V.SMART.Web`, `V.SMART.Api`, `V.SMART` (raise), and `risk` is `HIGH` (raise) — MEDIUM + ≥2 raises caps at HIGH. **`risk: HIGH`**: `source_files` includes `V.SMART.Api/Program.cs`, `V.SMART.Web/Program.cs` and `V.SMART/MauiProgram.cs`, which the KB-091 §4.3 table names directly as a HIGH-risk trigger (composition-root changes affect every controller's DI resolution). No frontmatter override present in the task file — both values are derived, not stated. |
| **Models this run** | Implement: `opus` (HIGH complexity). Validate: `opus` (HIGH complexity and HIGH risk both force it). |
| **Blocked on** | `M2-B07` attempt 1's no-result stop — see Stop reason and Blocked-on kind rows above. Not blocked for *selection* purposes otherwise: remaining `Ready` M2 candidates once `M2-B07` closes are `M2-A06`, `M2-C04-01`, `M2-C10`, `M2-C11`; `M2-A01-02` nominally `Ready` but its spec contradicts reality — see below, do not select until reconciled. Standing owner-owned items, none of which block the runner: `M0-04` `Blocked` (no identified owner with production access); `M0-01-03` `Needs Review` (rebuild drill, a deferred G0 exception); `M0-06` `Blocked` on Q-25/Q-26; `M0-11` `Ready` but a `Product Decision`, owner-only and never runner-selectable ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)). |
| **Next ready task** | Re-dispatch `M2-B07` (attempt 2 of 4) on `migration/M2-B07-add-vsmart-domain` at tip `a071716` first — it is the current task and its branch already exists with real, unvalidated progress; starting a different task while it sits `Blocked`-on-a-task would abandon that work needlessly. Only if a second consecutive no-result attempt escalates this to human-blocked should the runner fall through to the next `Ready` candidate per [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule). `M2-C04` is a **parent container** and is never worked directly. |
| **Process note — id allocation** | **Three cross-branch id collisions occurred on 2026-08-19** (six KB/INV/Q ids, then `M2-C01`'s tracker footnote ¹⁸ vs. `M2-A01-01`'s). Every one was caught by hand at merge. `grep`-before-claim cannot see a sibling branch; `git branch --no-merged master` must be checked before claiming any id. M2 runs more parallel branches than M0 did, so this recurs until the allocation rule itself changes. |

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
