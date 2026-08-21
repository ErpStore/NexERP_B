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
last_verified: 2026-08-21
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
| **Status** | `BLOCKED` — resumed 2026-08-21 by owner instruction (`/migration-runner`); closed `M2-B04`; halted at Select with an empty candidate set; the owner then **authorised `M0-01-03`'s executable half**, which ran and closed `Needs Review`. Now halted again, at the same place and for the same reason: **nothing else can be selected**. This is a [KB-091 §8](autonomous-runner.md#8-safety-limits--the-runner-stops-and-asks) stop, not a failure. Owner: **Vivek**. **The `M0-01-03` stop turned out to be softer than §8 item 5 implied** — its environment half was simply stale (see **Stop reason**), and running it produced **R-65**, a silent-lockout defect that blocks `M2-A02`. `M2-B04` is **closed `Needs Review`**: attempt 2 validated `PASS` (`5ca1c10`) and cleared the attempt-1 `environment` stop with no escalation. **Six branches carry a claimed `PASS` and await owner merge, none on `master`** — `M2-B04` (`…-decouple-pages-references`, `5ca1c10`), `M2-A08` (`…-row-scope-and-account-gates`, `bca92fd`), `M2-A07` (`…-me-endpoint`, `e3bc96c`), `M2-C00` (`…-kb050-angular-rewrite`, `b3c0e6e`), `M2-B01` (`…-api-versioning`, `045a7f4`, 11 of 12 criteria, criterion 4 partial) and `M0-10` (`…-candelete-guard-audit`, `fc8e0c0`, attempt 3) — **and a seventh task, `M2-B12-01`, is `Blocked` on Vivek with its escalation budget exhausted.** None is selectable; none releases its dependents until merged ([selection rule](dependency-graph.md#ready-task-selection-rule) step 1). **The merge queue is now the binding constraint on this project, not execution capacity.** Every state in this row was read from the branch itself this session — see **Process note — inherited status** below for why that mattered. |
| **Stop reason (final, after `M0-01-03`)** | **The merge queue, now seven branches deep.** `M0-01-03` was run to the limit of what a session can honestly do and closed `Needs Review`; what remains on it is a **named operator** and runbook **§7**, neither of which is a technical blocker. Nothing else is selectable. **Correction worth carrying:** the original §8 item 5 stop on `M0-01-03` rested on the *task file's* claim that no SQL Server was reachable and no credential existed. Both were false — `MSSQL$SQLEXPRESS` was running and reachable by **Windows integrated auth**, so no credential was ever needed. Footnote ²¹ recorded that on 2026-08-19; the task file was never updated, and the runner stopped on the stale premise. **Running it produced R-65**, a silent-lockout defect in `ScreenCatalogue.cs` that blocks `M2-A02` and would not have been found any other way. |
| **Stop reason (initial, at Select)** | **No task could be done autonomously, and the reason was the merge queue rather than execution capacity.** Five branches are validated `PASS` and unmerged, so none of their dependents is released. Of what remains: `M0-01-03` (P0, the rank winner) needs the rebuild **drill executed**, and its own step 7 hands that to a human — KB-091 §8 item 5. The environment half of that block **is now false** (tracker footnote ²¹: `MSSQL$SQLEXPRESS` is running, reachable by Windows integrated auth, `sqlcmd` and the `SqlServer` module both present — re-verified this session), so what actually remains unavailable is a **named operator** to sign the drill log and the **UI smoke test** (start the Blazor host, log in, run one report, print one document through `Sp_Print_CompanyDetails`). `M2-B09` is the only other unclaimed `Ready` task and it is dropped at selection step 2: it shares **`V.SMART.Api/Program.cs` and `Controllers/CurrencyController.cs`** with `M2-B01`, which is live in `wt-M2-B01`. Nothing here is a defect; it is a queue that needs the owner. |
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-21 autonomous run through `M2-B02`, `M2-A01-02`, `M2-A01-03`, `M2-B12-01`, `M2-A08`, `M2-B04`, and now into `M0-01-03`). |
| **Last transition** | 2026-08-21 — `M2-B04` closed `Needs Review` after attempt 2's `PASS`; the four bookkeeping files still carried attempt 1's close-out text (written 11:13–11:20, before attempt 2 committed at 12:19/12:37) and were reconciled in this transition. Re-applied the [selection rule](dependency-graph.md#ready-task-selection-rule): step 2 dropped `M2-B09` on a same-file conflict with the live `M2-B01` worktree, leaving **`M0-01-03`** as the only candidate — which then hit the KB-091 §8 item 5 stop. Run halted at Select, nothing dispatched. |
| **Current task** | None in flight. `M0-01-03` **ran and closed `Needs Review`** on `migration/M0-01-03-rebuild-drill` (`34b5e32`, unmerged): runbook §§2–6 executed and passing — 108 migrations in ~50 s, 197 tables, 150 `Screens`, 91 stored procedures in 2.16 s with 0 failures and idempotent on re-run. **Still open on it:** runbook §7 (the UI smoke test — the *"and the app runs against it"* half of G0 criterion 1) and the **named-operator** requirement, neither of which an autonomous session can satisfy. Both drill databases were left in place so an operator can run §7 without repeating §§2–6. |
| **Current phase** | Idle — halted at Select, after completing `M0-01-03`'s executable half. |
| **Current agent** | n/a — not dispatched. |
| **Current model** | n/a — nothing dispatched. Had `M0-01-03` run, it would route to `opus`. |
| **Attempt** | `M0-01-03`: **1 of 3** used, 0 escalations, no validator dispatched (the owner scoped the run directly), closed `Needs Review` with two acceptance criteria openly unmet. `M2-B04` (closed): **2 of 3** used, 0 escalations, verdict `PASS`, status `Needs Review`. `M2-B12-01`: **2 of 3** used, **1 of 1 escalations spent**, verdict `FAIL` at `fa4a2ad`, status **`Blocked`** — *not* `PASS`, and the escalated fix at `8a54f96` has never been re-validated. `M2-A08`: **1 of 3** used, 0 escalations, `PASS`, `Needs Review` on `…-row-scope-and-account-gates`. |
| **Escalations** | 0 |
| **Last validation** | `M2-B04`, tip `5ca1c10` — verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`, attempt 2 of 3. Re-derived by the validator, each build compared against its *matching* baseline form: `V.SMART.Api` **0 errors / 6694 warnings**, `V.SMART.Web` **0 / 6697**, CI form **6693** with `tools/compare-warnings.sh` → `Gate: PASSED (equal to baseline)`, exit 0; `tests/V.SMART.Shared.Tests` **86 passed** (84 + 2 new), `tests/V.SMART.Api.Tests` **117 passed**; `grep` for `V.SMART.Shared.Pages` outside `/Pages/` → **0 hits**. Attempt 1's `6695` was never an anomaly — it was the plain build's own baseline compared to the CI-form number. **The guard was attacked, not trusted:** the validator seeded two independent violations in different namespaces (an unused `using`, and a fully-qualified member with no `using` at all) and confirmed the documented reflection blindness is real and honestly stated. **Two gates stay open and `PASS` does not close them:** acceptance criterion 9 (manual approval-workflow regression) is `NOT CHECKABLE` without a tenant-DB credential (Q-14/R-01/Q-32), and the MAUI head was not built. **Finding that reframes the task:** the headline `IApprovalService` → `Authorization.razor` dependency was **dead text** — that file contains zero `static` and declares no type — so M2-B04 removed a documentation-level violation and installed a guard; it did not sever a real compile-time coupling. Previous validation: `M2-A01-03`, tip `0fde6fb` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`. All eighteen acceptance criteria independently re-checked `MET`: cache resolves through `IMemoryCache`; key `screenrights:v1:{tenantId}:{userId}`, tenant-scoped and prefixed; TTL from `Authorization:RightsCacheSeconds`, default 60 s, **absolute** expiration; explicit `Invalidate(tenantId, userId)` eviction, and the count of in-process `UserRight` write sites needing it is genuinely 0 (all five write sites confirmed in the Blazor host only); the cross-process gap is documented in three places; a zero-TTL bypass exists for `M2-A03`'s harness; a cache miss returns exactly what the repository produces, no reordering; a failing query is never cached. Re-run by the validator: `dotnet build V.SMART.Api --no-incremental` 0 errors/6,694 warnings (baseline); `dotnet test tests/V.SMART.Api.Tests` 117/117 passed (104→117, the growth is this task's cache suite); `dotnet test tests/V.SMART.Shared.Tests` 84/84 passed, no regression. `JwtTokenService.cs` unchanged, no controller annotated, nothing under `V.SMART.Shared/`/`V.SMART.Web/`/`V.SMART/V.SMART/` modified, no secret or migration touched. **Attempt 1 (`a78c51e`) regressed the test suite** — added `Invalidate` to `IUserRightsProvider` without updating two test stand-ins, `CS0535` × 2, 104 tests ran zero — diagnosed as `implementation-error`, fixed in attempt 2 (`0fde6fb`), re-validated clean. Full evidence: `tasks/M2-A01-03.md` § Execution Record (2026-08-20). |
| **Tasks processed this run** | Current run (started 2026-08-19, spans to 2026-08-21): `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, owner-merged and `Completed`. `M2-B02` — implemented, validated `PASS`, `Completed` and merged, released `M2-B09`. `M2-A01-02` — implemented, validated `PASS`, `Completed` and merged, released `M2-A01-03`. `M2-A01-03` — implemented across 2 attempts, validated `PASS`, `Completed` and merged, released `M2-A02`/`M2-A07`/`M2-A08`. `M2-B12-01` — implemented across 2 attempts, validated `PASS`, `Needs Review` (unmerged). `M2-A08` — implemented, validated `PASS`, `Needs Review` (unmerged). `M2-B04` — attempt 1 stopped with no implementer result, attempt 2 validated `PASS`, `Needs Review` (unmerged). `M0-01-03` — selected 2026-08-21, **not started**, safety stop. |
| **Classification** | `M2-A01-02` (closed) — complexity **HIGH**, risk **HIGH** (task_type Security, `business_rules` populated, `Program.cs` in `source_files`). `M2-A01-03` (closed) — complexity **HIGH**, risk **HIGH**, same grounds plus `business_rules: [BR-AUTH-002, BR-TEN-001, BR-TEN-002]` and two-project `source_files`. `M2-B04` — `task_type: Backend`, `business_rules: [BR-APPR-001]`, `estimate: 1 wk` (≥3 d raises complexity independent of task_type per [KB-091 §4.1](autonomous-runner.md#41-base-complexity-from-task_type)); routed to `opus` for both attempts; closed `PASS` on attempt 2. `M0-01-03` (current) — `task_type: Database`, `business_rules: []`, `estimate: 1 d`, priority **P0**. Complexity **MEDIUM** (1 d is below the ≥3 d raise, but the work executes DDL against a live SQL Server instance that holds real development databases), risk **HIGH** on that ground alone → `opus`. |
| **Models this run** | `M2-A01-02`: Implement `opus`, Validate `opus`. `M2-A01-03`: Implement `opus`, Validate `opus`. `M2-B12-01`: Implement `opus`, Validate `opus` (both attempts). `M2-A08`: Implement `opus`, Validate `opus`. `M2-B04`: Implement `opus` (attempt 1 stopped `environment`-category before reaching validate; attempt 2 same route, `PASS`), Validate `opus`. `M0-01-03`: Implement `opus`, Validate `opus`. |
| **Next ready task** | **None — the candidate set is empty and only the owner can refill it.** `M0-01-03` is now also `Needs Review`, making it the **seventh** unmerged branch. The single highest-value action available is **merging the queue**; the second is deciding **R-65** before `M2-A02` starts, and **M2-A08**'s two competing branches. *(Previously: `M0-01-03` — selected by the [selection rule](dependency-graph.md#ready-task-selection-rule), its only prerequisite `M0-01-02` being `Completed` and merged with all repository-side artefacts on `master`.)* **Excluded, with reasons:** `M2-B04`, `M2-B12-01`, `M2-A08`, `M2-C00`, `M2-A07` — all done, `PASS`, unmerged (step 1); `M2-A02` — `Ready` but gated on the unanswered **Q-28** (step 1, information dependency); `M2-C01` — `Blocked` behind `M2-C00`'s merge; `M2-B01`, `M0-10` — each already has a live sibling worktree (step 2, and the five-part test's part 5); `M2-B09` — `Ready` and P1, but **dropped at step 2, not merely outranked**: its `source_files` name `V.SMART/V.SMART.Api/Program.cs` and `V.SMART/V.SMART.Api/Controllers/CurrencyController.cs`, and `M2-B01` — live in `wt-M2-B01` — names *both* of those same two files. Two sessions editing `Program.cs` in parallel produce a merge, not progress. `M2-B09` becomes the obvious next pick the moment `M2-B01` lands. |
| **Process note — inherited status** | 🚩 **A `PASS` recorded in this file was false, and this session propagated it once before checking.** The inherited state listed `M2-B12-01` as validated `PASS` and awaiting merge. That branch's own tip is *"Record close-out — BLOCKED, escalation budget exhausted, **corrects a premature PASS**"*, and its runner-state states that the earlier `PASS` was claimed for tip `58e7bee` whose own failure-log entry recorded `FAIL` — *"no genuine `PASS` of `58e7bee` exists anywhere in this repository."* It was caught only because `git stash list` incidentally printed that commit subject. The same check then found `M2-B01` and `M0-10`, two finished branches the inherited state did not mention at all. **A status inherited from a sibling branch is a claim, not a fact.** `git log --oneline -2 <branch>` costs nothing and is now part of the Select phase, alongside `git worktree list`. |
| **Process note — concurrency** | **Three sibling worktrees were live when this run resumed** — `wt-M0-10` (`migration/M0-10-candelete-guard-audit`), `wt-M2-A08` (`migration/M2-A08-row-level-scoping`) and `wt-M2-B01` (`migration/M2-B01-api-versioning`) — none of them this session's. `git worktree list` is therefore part of selection, not a curiosity: the tracker alone cannot see them. Note also that **`M2-A08` now has two branches** (`migration/M2-A08-row-level-scoping` and `migration/M2-A08-row-scope-and-account-gates`), which is duplicated work by two sessions on one task and needs an owner decision about which to keep. |
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
