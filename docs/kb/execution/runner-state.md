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
| **Status** | `BLOCKED` — `M2-B07` attempt 3 of 3 is exhausted. This **is** a stop the run must sit at: the retry budget is spent and the remaining gap needs a decision only the repository owner can make. |
| **Stop reason** | The one unmet acceptance criterion — *"the Blazor app starts and three screens from three different modules render without a DI resolution error"* — needs a **signed-in interactive Blazor Server circuit**. The single provisioned ERP user's password is hashed and owner-held; no session may acquire or reuse a credential ([`CLAUDE.md`](../../CLAUDE.md) standing constraint). This close-out session also **corrected** attempt 3's own diagnosis: the prior conclusion "no database is provisioned on this workstation" was wrong — a SQL Server Express instance with `NexGenErpDb_Master` and a 197-table tenant database already exists here, and with it `V.SMART.Web` renders `/` at `200` with zero DI resolution errors; the three named module screens correctly `302` to `/access-denied` under server-side screen-right authorization (ADR-004/M2-A01-01), identical to `master`. Every other acceptance criterion is `MET`. Full record: [`tasks/M2-B07.md` § Execution Record (2026-08-19) — close-out, attempt 3 of 3](tasks/M2-B07.md#execution-record-2026-08-19--close-out-attempt-3-of-3-session-ends-blocked), [`failure-log.md` § M2-B07 · attempt 3](failure-log.md#m2-b07--attempt-3--2026-08-19). |
| **Blocked-on kind** | **Blocked on a human — Vivek (repository owner).** Not blocked on a task: retry budget (3 of 3) is spent, so no further re-dispatch is authorised without a decision. Needed from Vivek: either (A) sign in as the one provisioned user in a browser with `ConnectionStrings__MasterDb` pointed at `DESKTOP-FIIBE97\SQLEXPRESS` / `NexGenErpDb_Master` and open three screens from three different modules, or (B) waive the render half on the recorded evidence (whole-graph `ValidateOnBuild` passing at startup, zero `Unable to resolve service` in the host log, branch/`master` parity on every route tried). |
| **Run started** | 2026-08-19 |
| **Last transition** | 2026-08-19 — close-out session recorded attempt 3's exhaustion and moved status `RUNNING` → `BLOCKED`. `M2-B07` stays the active task in `current-task.md`; `nextTaskId` is empty by design — no task may be selected while `M2-B07` sits blocked-on-a-human at exhausted budget. |
| **Current task** | `M2-B07` — Shared `AddVSmartDomain()` DI extension |
| **Current phase** | `TESTING`/`REVIEW` boundary — every mechanical acceptance criterion is `MET` (build, test, `ValidateOnBuild`, registration-set parity); the task cannot advance to `REVIEW` while one criterion is unresolved. |
| **Current agent** | none — no attempt is in flight; none authorised until Vivek responds |
| **Current model** | n/a |
| **Attempt** | 3 of 3 — exhausted |
| **Escalations** | 1 (this close-out) |
| **Last validation** | `M2-B07` attempt 3 — validator verdict `FAIL`, `failureCategory: environment`, 16 of 17 criteria `MET` or `NOT CHECKABLE` for reasons independent of code. Before that, `M2-C01` — validator verdict `FAIL`, `failureCategory: environment`, 14/15 `MET`, waived by the owner (`task-tracker.md` footnote ¹⁹). |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01` — all `Completed` and merged. `M2-B07` selected and attempted 3 of 3 times; ends this run `Blocked` on the repository owner. |
| **Classification** | `M2-B07` — `task_type: Backend` (base `MEDIUM`) → raised to **`complexity: HIGH`**: `estimate` is 3 d (raise), 7 tasks (`M2-B01`, `B04`, `B05`, `B06`, `B08`, `B09`, `B12-03`) name it in `depends_on` (raise, ≥3), `source_files` spans all four projects — `V.SMART.Shared`, `V.SMART.Web`, `V.SMART.Api`, `V.SMART` (raise), and `risk` is `HIGH` (raise) — MEDIUM + ≥2 raises caps at HIGH. **`risk: HIGH`**: `source_files` includes `V.SMART.Api/Program.cs`, `V.SMART.Web/Program.cs` and `V.SMART/MauiProgram.cs`, which the KB-091 §4.3 table names directly as a HIGH-risk trigger (composition-root changes affect every controller's DI resolution). No frontmatter override present in the task file — both values are derived, not stated. |
| **Models this run** | Implement: `opus` (HIGH complexity). Validate: `opus` (HIGH complexity and HIGH risk both force it). |
| **Blocked on** | `M2-B07`'s exhausted retry budget and the credential/interactive-session gap — see Stop reason and Blocked-on kind rows above. Not blocked for *selection* purposes otherwise: remaining `Ready` M2 candidates, available to a human who explicitly chooses to work around `M2-B07` rather than wait for it, are `M2-A06`, `M2-C04-01`, `M2-C10`, `M2-C11`; `M2-A01-02` nominally `Ready` but its spec contradicts reality — see `current-task.md`, do not select until reconciled. Standing owner-owned items, none of which block the runner: `M0-04` `Blocked` (no identified owner with production access); `M0-01-03` `Needs Review` (rebuild drill, a deferred G0 exception); `M0-06` `Blocked` on Q-25/Q-26; `M0-11` `Ready` but a `Product Decision`, owner-only and never runner-selectable ([KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks)). |
| **Next ready task** | None selected by this close-out — `nextTaskId` is deliberately empty. `M2-B07` remains the active task per `current-task.md` until Vivek responds; a future run resumes there rather than restarting elsewhere. `M2-C04` is a **parent container** and is never worked directly. |
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
