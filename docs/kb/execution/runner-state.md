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
| **Status** | `RUNNING` — `M2-A01-02` implemented and independently validated `PASS` on `migration/M2-A01-02-require-screen-right` (`9a6b3c2`), 2026-08-20, attempt 1 of 3, 0 escalations. **`Needs Review`, not merged** — per [KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) only the repository owner may set it `Completed`; no owner instruction to merge has been given in this session. Its only Hard-dependent, `M2-A01-03`, therefore stays `Blocked`. `M2-B12-01` selected as the next task. |
| **Stop reason** | n/a — run live. |
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-20 autonomous run through `M2-B02`, `M2-A01-02`, and now into `M2-B12-01`). |
| **Last transition** | 2026-08-20 — `M2-A01-02` close-out recorded (task file execution record, `task-tracker.md` footnote ²⁵, KB-105/KB-040/KB-060/INV-037 documentation updates); `M2-B12-01` selected per the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule) and written into `current-task.md`. Not yet started — no branch exists yet. |
| **Current task** | `M2-B12-01` — INV-012 document-numbering + financial-year investigation (documentation only; no C# file changes). Not yet started; no branch exists yet. |
| **Current phase** | `READY` — selected, dispatch pending. |
| **Current agent** | n/a — not yet dispatched |
| **Current model** | Not yet classified for `M2-B12-01`; see [KB-091 §4](autonomous-runner.md#4-classifying-a-task) at dispatch. |
| **Attempt** | `M2-A01-02`: 1 of 3 used (closed, `PASS`). `M2-B12-01`: 0 of 3 used. |
| **Escalations** | 0 |
| **Last validation** | `M2-A01-02`, tip `9a6b3c2` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. All twenty-two acceptance criteria independently re-checked `MET`, including the four extra KB-105 §2 types (`NoScreenRightAttribute`, `ScreenRightSet`, `ScreenCatalogue`, `ScreenRightStartupValidator`) beyond the task file's own stale six-file list. Re-run by the validator: `dotnet build V.SMART.Api --no-incremental` 0 errors/6,694 warnings (KB-083 baseline, no new warnings); `dotnet test tests/V.SMART.Api.Tests` 104/104 passed; `dotnet test tests/V.SMART.Shared.Tests` 84/84 passed, no regression; `dotnet build V.SMART.Web` 0 errors (Blazor host intact); live host verification against local `SQLEXPRESS` confirmed the six existing endpoints respond unchanged (401 unauthenticated, 401 bad login, 200 authenticated paged GET) and that the filter is dormant on the unannotated `CurrencyController`. `ScreenCatalogue`'s 152 names diffed identical against the live `Screens` seed. Three deliberate spec-vs-task-file departures (type list, provider signature, 401-not-403 on an unusable claim) all traced to KB-105 lines, none guessed. One new, not-previously-recorded finding: the globally registered filter eagerly constructs `IUserRightsProvider`/`IUnitOfWork` via DI on every request reaching MVC's pipeline, latent and deployment-conditional, not a regression today — recorded in `tasks/M2-A01-02.md` § Execution Record, `task-tracker.md` footnote ²⁵, and `KB-060` R-03. Full evidence: `tasks/M2-A01-02.md` § Execution Record (2026-08-20). No validation has run yet for `M2-B12-01`. |
| **Tasks processed this run** | `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, `Needs Review`, then owner-merged and `Completed`. `M2-B12-01` — was selected once before, superseded before being started; now selected again. `M2-B02` — implemented, validated `PASS` after one in-attempt retry, `Completed` and merged (`feec964`), released `M2-B09` only. `M2-A01-02` — implemented, validated `PASS`, `Needs Review`, not yet merged. |
| **Classification** | `M2-A01-02` (closed) — complexity **HIGH**, risk **HIGH**: base HIGH from `task_type: Security` ([KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)), independently also raised by `estimate: 3 d` (≥3 d), `business_rules: [BR-AUTH-002]` non-empty, and the task touching authorization. Risk HIGH per [§4.3](autonomous-runner.md#43-risk): `task_type: Security`, `business_rules` populated, `source_files` includes `Program.cs`. `M2-B12-01` (current) — not yet classified; `task_type: Investigation`, `estimate: 2 d`, `business_rules: []` — likely base MEDIUM or lower per [§4.1](autonomous-runner.md#41-base-complexity-from-task_type), to be confirmed at dispatch. |
| **Models this run** | `M2-A01-02`: Implement `opus`, Validate `opus` (HIGH complexity/risk routing; risk HIGH also forces `opus` for validation per [KB-091 §5.2](autonomous-runner.md#52-floors-that-override-the-table) rule 2, independent of complexity). `M2-B12-01`: not yet routed. |
| **Next ready task** | `M2-B12-01` — INV-012 document-numbering + financial-year investigation. Selected per the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule): with `M2-A01-02` closed (`Needs Review`, not merged, so `M2-A01-03` stays `Blocked`), the remaining genuinely `Ready` P0 candidates are `M0-01-03`, `M2-B04`, `M2-B12-01`, `M2-C04-02`, `M2-C04-03`, `M2-C10` — all tie at step 1 (P0). At step 2 (most downstream unblocking that actually fires), only `M2-B12-01` unblocks a real dependent (`M2-B12-02`, which names only `M2-B12-01` in `depends_on`); `M2-C04-02`'s and `M2-C10`'s apparent dependents still need `M2-B02`/`M2-C05-01` respectively, neither ready, and `M0-01-03`/`M2-B04`/`M2-C04-03` have zero dependents in the tracker. `M2-B12-01` wins outright at step 2 — no tie-break needed. See `current-task.md` for the full ranking. |
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
