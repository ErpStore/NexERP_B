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

> **⚠ The selection note this file carried into 2026-08-21 was written by a session that then
> died, and one of its assertions was false by the time the next session read it.** It ended
> *"No safety-stop condition applies: working tree clean, branch will cut fresh from `master`."*
> The working tree was **not** clean. Between that sentence being written (16:04 IST) and the next
> session starting (~16:23), the same run implemented ~979 lines of `M2-B06` into the working tree
> and was killed before committing any of it. The next session therefore opened on a `RUNNING`
> state claiming *attempt 0, not yet dispatched* over a tree that already held most of the
> implementation.
>
> **This is the second time in this run that a claim inherited from bookkeeping has been wrong**
> (see **Process note — inherited status**), and the failure mode is the same both times: the
> record was written *before* the action it describes finished. The correction is not "read more
> carefully" — it is that **`git status --porcelain` belongs at the top of every run, before the
> state file is believed at all.** A `RUNNING` row and a dirty tree together mean a killed run, not
> a fresh one.
>
> **What the Select phase itself got right, and is preserved:** candidates were `M2-B06` (P1, 1 wk,
> `depends_on: [M2-A06, M2-B01]`, both `Completed` and merged) and `M2-B11` (P2, 3 d,
> `depends_on: [M2-A06]`); both passed the five-part "can actually be done" test; rank step 1
> settled it on priority. Classification `task_type: Backend` (base MEDIUM), raised to **HIGH** by
> `estimate: 1 wk` and a three-project `source_files`; risk **MEDIUM**. `M2-B06` was the correct
> pick and it has now been executed and closed `Needs Review`.

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
| **Status** | `STOPPED` — **`M2-B11` closed `Needs Review` 2026-08-21, validated `PASS` on attempt 2 of 4 by an independent validator, no escalations.** Select re-ran afterward and found **no ready task**: `M2-B11` releases nothing in the dependency graph, and every other candidate is excluded by the same reasons recorded below (**Next ready task**). This is a clean, expected stop — budget was not exhausted, the pool is genuinely empty pending owner merges. Owner: **Vivek**. **Eight branches now carry a claimed `PASS`/`Needs Review` and await owner merge** — `M2-B11` (`…-health-checks-logging`, `12dad11`, this task), `M2-B09` (`…-reference-endpoints`, `d1175db`), `M2-B04` (`…-decouple-pages-references`, `5ca1c10`), `M2-A08` (`…-row-scope-and-account-gates`, `bca92fd`), `M2-A07` (`…-me-endpoint`, `e3bc96c`), `M2-C00` (`…-kb050-angular-rewrite`, `b3c0e6e`), `M0-10` (`…-candelete-guard-audit`, `fc8e0c0`) and `M0-01-03` (`34b5e32`, awaiting a named operator) — **plus `M2-B12-01` `Blocked` on Vivek with its escalation budget exhausted.** |
| **Stop reason (final, after `M2-B11`)** | **`M2-B11` was the only candidate and it closed clean; nothing behind it is ready.** Health checks (`/health/live`, `/health/ready`) and a new `ILogger`-based `StructuredLoggingService` land on `migration/M2-B11-health-checks-logging`. Attempt 1 failed the CI warning-gate ratchet (6694 measured vs. 6693 baseline, a `CS8767` nullability mismatch in the new `TenantInfoDestructuringPolicy`); attempt 2 fixed it with an annotation-only change (`81ad961`) and re-measured at 6693 = baseline, gate `PASSED`. Independent validator verdict **`PASS`**, `scopeOk: true`, no regressions. `ILoggingService.cs` is byte-identical; `FileLoggingService` is kept as the Blazor/MAUI registration; the new sink is wired only in `V.SMART.Api`. R-23 marked resolved **for `V.SMART.Api` only**. Two criteria stated as unmet rather than glossed: no `LogUserAction` event was observed over HTTP (no call site exists in `V.SMART.Api` — out of scope to add one) and the Blazor host was not started. Nothing in the dependency graph names `M2-B11` as a prerequisite, so this close releases no other task. Full record: tracker footnote ³⁶, `failure-log.md`, `tasks/M2-B11.md` § Execution Record (Close-out). |
| **Stop reason (after `M2-B09`)** | **The owner merged `M2-B01`, which refilled the pool, and `M2-B09` was executed to completion.** It closed `Needs Review` on `migration/M2-B09-reference-endpoints` (`d1175db`) — six cached reference endpoints, the R-15 boundary fix, KB-124, **162** Api tests (117 → 162), every build at its exact baseline, scope verified by diff. Two criteria openly unmet: no Blazor screen opened, and no end-to-end two-tenant HTTP test (proven at the policy level instead — the residual risk, recorded in KB-124 §6). **Remaining pool after it:** `M2-B06` (P1, 1 wk, released by the same merge, not yet started) and `M2-B11` (P2, 3 d). Both are genuinely selectable — this is the first time in this run that has been true. |
| **Stop reason (after `M2-B06`)** | **The candidate set is empty on every path, and one merge fixes most of it.** `M2-B06` was the last candidate: sound task, but it specifies every endpoint under `/api/v1` and **`master` has no `/api/v1`** — the prefix and the `ApiRoutes.V1` constant exist only on the unmerged `migration/M2-B01-api-versioning` branch. Its `depends_on` did not declare `M2-B01`, so neither selection step saw it: step 1 checks *declared* prerequisites, step 2 checks *file* overlap, and this collision is on a route surface. `depends_on` corrected to `[M2-A06, M2-B01]`; **`Blocked`, no re-specification needed**, unlike `M2-B05`. **Merging `M2-B01` alone releases `M2-B06`, `M2-B09` and `M2-B11`.** Full account: tracker footnote ³², `failure-log.md`. |
| **Stop reason (after `M2-B05`)** | **`M2-B05` was selected cleanly and then falsified at Investigate — `Blocked`, category `specification`, owner Vivek, no code written and no branch.** It won Select legitimately: P1, 2 d, prerequisite merged, **zero file overlap** with any of the seven unmerged branches, no `⛔` banner. Its own step 2 says to re-verify rather than trust the document; doing so showed the task's premise is false. There are **no `screenCode` literals** — the code resolves the screen code at runtime from the database by name (`GetScreenCodeByScreenNameAsync`, **166** sites, **61** Razor pages; **0** literals in **244** inspected stock calls). The real magic numbers are **55 bare `6`/`7` `storeId` literals** (`REJECTION STORE`/`REWORK STORE`) → **R-66**. **R-10 was `Confirmed` from a signature, not a call site**, and this task was sized at 2 days on that. Full account: **INV-044**, tracker footnote ³¹, `failure-log.md`. |
| **Stop reason (after `M0-01-03`)** | **The merge queue, now seven branches deep.** `M0-01-03` was run to the limit of what a session can honestly do and closed `Needs Review`; what remains on it is a **named operator** and runbook **§7**, neither of which is a technical blocker. Nothing else is selectable. **Correction worth carrying:** the original §8 item 5 stop on `M0-01-03` rested on the *task file's* claim that no SQL Server was reachable and no credential existed. Both were false — `MSSQL$SQLEXPRESS` was running and reachable by **Windows integrated auth**, so no credential was ever needed. Footnote ²¹ recorded that on 2026-08-19; the task file was never updated, and the runner stopped on the stale premise. **Running it produced R-65**, a silent-lockout defect in `ScreenCatalogue.cs` that blocks `M2-A02` and would not have been found any other way. |
| **Stop reason (initial, at Select)** | **No task could be done autonomously, and the reason was the merge queue rather than execution capacity.** Five branches are validated `PASS` and unmerged, so none of their dependents is released. Of what remains: `M0-01-03` (P0, the rank winner) needs the rebuild **drill executed**, and its own step 7 hands that to a human — KB-091 §8 item 5. The environment half of that block **is now false** (tracker footnote ²¹: `MSSQL$SQLEXPRESS` is running, reachable by Windows integrated auth, `sqlcmd` and the `SqlServer` module both present — re-verified this session), so what actually remains unavailable is a **named operator** to sign the drill log and the **UI smoke test** (start the Blazor host, log in, run one report, print one document through `Sp_Print_CompanyDetails`). `M2-B09` is the only other unclaimed `Ready` task and it is dropped at selection step 2: it shares **`V.SMART.Api/Program.cs` and `Controllers/CurrencyController.cs`** with `M2-B01`, which is live in `wt-M2-B01`. Nothing here is a defect; it is a queue that needs the owner. |
| **Run started** | 2026-08-19 (spans the 2026-08-19→2026-08-21 autonomous run through `M2-B02`, `M2-A01-02`, `M2-A01-03`, `M2-B12-01`, `M2-A08`, `M2-B04`, and now into `M0-01-03`). |
| **Last transition** | 2026-08-21 — **`M2-B11` closed `Needs Review`.** Independent validator ran attempt 1 against `7b4b86c` and returned `FAIL` (CI warning ratchet, 6694 vs. 6693 baseline). Diagnosis: `implementation-error`, `CS8767` from a missing `[NotNullWhen(true)]` on `TenantInfoDestructuringPolicy.TryDestructure`'s `out` parameter — Serilog's `IDestructuringPolicy` declares it, this implementation omitted it. Fixed (`81ad961`, annotation only), re-measured at the gate's own baseline (6693), re-validated attempt 2 → `PASS`. A documentation-only follow-up (`12dad11`) corrected the task file's own Execution Record, which had quoted the wrong (plain-build) warning figure, and corrected `KB-113`/`R-23`'s retention wording (`retainedFileCountLimit` is a file count, not a day-span). Select then re-ran and produced an **empty candidate set** — nothing in the dependency graph names `M2-B11`, so no task is released. Run halted, nothing dispatched. |
| **Current task** | **None.** `M2-B11` closed this transition; Select found no dependency-ready candidate. See **Next ready task** below and `current-task.md`, which has been rewritten to record the empty pool rather than point at a specific task. |
| **Current phase** | Select completed 2026-08-21 after `M2-B11`'s close-out — candidate set empty. Nothing dispatched. |
| **Current agent** | n/a — not dispatched. |
| **Current model** | n/a — nothing selected. |
| **Attempt** | `M2-B11` (closed): **2 of 4** used, 0 escalations. Attempt 1 `FAIL` (`implementation-error`, CI warning ratchet), attempt 2 `PASS` after an annotation-only fix. **Independently validated** — the validator re-ran every acceptance criterion and every command itself rather than trusting the implementer's report. `M2-B06` (closed): **1 of 3** used, 0 escalations, closed `Needs Review` with two acceptance criteria openly unmet. **No independent validator was dispatched** — the verification below was run directly in the session, which is a weaker guarantee than an independent re-derivation and is recorded as such. `M0-01-03`: **1 of 3** used, 0 escalations, no validator dispatched, `Needs Review`. `M2-B04` (closed): **2 of 3** used, 0 escalations, verdict `PASS`, `Needs Review`. `M2-B12-01`: **2 of 3** used, **1 of 1 escalations spent**, verdict `FAIL` at `fa4a2ad`, status **`Blocked`** — *not* `PASS`. `M2-A08`: **1 of 3** used, 0 escalations, `PASS`, `Needs Review`. |
| **Escalations** | 0 |
| **Last validation** | `M2-B11` — **independently validated, `PASS`, attempt 2 of 4.** `dotnet build V.SMART.Api --no-incremental` **6694 warnings / 0 errors** (plain build — the wrong measurement for the ratchet, recorded as such); the gate measurement (`dotnet restore` → `dotnet build --no-restore --no-incremental` → `tools/compare-warnings.sh`) → **6693 warnings, measured = baseline, Gate: PASSED, exit 0**; `dotnet test tests/V.SMART.Api.Tests` **179 passed / 0 failed** (148 → 179); `dotnet test tests/V.SMART.Shared.Tests` **84 passed / 0 failed**; `V.SMART.Web` **0 errors / 6697 warnings**, its exact baseline. Runtime: `/health/live` 200 with the master DB down and no database touched; `/health/ready` 200 with master+tenant healthy, 503 naming `master-db` when it is down; a credential grep of every emitted `diagnostics-*.json` found zero hits for `Password`/`TenantInfo`/`ConnectionString`/a server or database name. `git diff -- ILoggingService.cs` empty. All 20 acceptance criteria in `tasks/M2-B11.md` re-checked directly by the validator. Two criteria openly unmet, stated not glossed: the tenant-unreachable-while-master-healthy 503 proved only at unit level (writing a bogus `Tenants` row was out of scope), and no `LogUserAction` event was observed at runtime (no call site exists in `V.SMART.Api`). Previous validation: `M2-B06` — **self-verified, not independently validated.** `dotnet build V.SMART.Api --no-incremental` **0 errors / 6694 warnings** (the exact M2-B04-verified baseline); `dotnet build V.SMART.Web --no-incremental` **0 / 6697** (its exact baseline — the Razor call-site adaptation adds no warning); `dotnet test tests/V.SMART.Api.Tests` **148 passed / 0 failed** (117 → 148, the growth being this task's file-endpoint suite); `dotnet test tests/V.SMART.Shared.Tests` **84 passed / 0 failed**, no regression from the `CompanyService` signature change. **All seven required negative tests pass and are reported individually** — traversal, cross-tenant indistinguishability, 413, disallowed extension, unknown id, 401, 403 — with N6/N7 proved at the **policy level** (attributes present; `ScreenRightAuthorizationFilterTests` proves the filter denies) rather than over the wire. **Round trip passes byte-identical**, which is the assertion `WebFileUploadService` would fail. Scope verified by diff: `WebFileUploadService.cs`, `WebFileOpener.cs`, `MauiFileUploadService.cs`, `DesktopFileOpener.cs`, `ExcelExportService.cs`, `ExcelTemplateService.cs`, `IExcelTemplateService.cs`, `IFileOpener.cs`, `IFileUploadService.cs` and `Migrations/**` are all byte-unchanged. Previous validation: `M2-B04`, tip `5ca1c10` — validator verdict **`PASS`**, `failureCategory: none`, `scopeOk: true`, attempt 2 of 3. |
| **Tasks processed this run** | Current run (started 2026-08-19, spans to 2026-08-21): `M0-12-01`, `M0-13`, `M0-12-02`, `M0-09`, `M2-A01-01`, `M2-C01`, `M2-B07` — all `Completed` and merged. `M2-C04-01` — implemented, validated `PASS`, `Completed` and merged. `M2-A06` — implemented, validated `PASS`, owner-merged and `Completed`. `M2-B02` — implemented, validated `PASS`, `Completed` and merged, released `M2-B09`. `M2-A01-02` — implemented, validated `PASS`, `Completed` and merged, released `M2-A01-03`. `M2-A01-03` — implemented across 2 attempts, validated `PASS`, `Completed` and merged, released `M2-A02`/`M2-A07`/`M2-A08`. `M2-B12-01` — 2 attempts, **`Blocked`**, escalation budget exhausted (unmerged). `M2-A08` — implemented, validated `PASS`, `Needs Review` (unmerged). `M2-B04` — attempt 1 stopped with no implementer result, attempt 2 validated `PASS`, `Needs Review` (unmerged). `M0-01-03` — runbook §§2–6 executed and passing, `Needs Review` (unmerged), §7 and a named operator outstanding. `M2-B09` — implemented, `Needs Review` (unmerged). **`M2-B06` — API half adopted from a killed run and verified, second half implemented this session, `Needs Review` (unmerged).** `M2-B05` — selected, then **`Blocked`**: premise falsified, awaiting owner re-specification. **`M2-B11` — health checks + Serilog-shaped structured logging, 2 attempts (1 `FAIL` on the CI warning ratchet, fixed, 1 `PASS`), independently validated, `Needs Review` (unmerged).** |
| **Classification** | `M2-B06` (closed) — `task_type: Backend` (base MEDIUM), `business_rules: []`, `estimate: 1 wk` (≥3 d, raise 1), `source_files` spans `V.SMART.Shared`, `V.SMART.Web` and `V.SMART` (raise 2) → complexity **HIGH**; risk **MEDIUM**. **The risk assessment was wrong in one respect and it is worth recording why.** It read *"no observable live-Blazor behaviour change — Blazor keeps its own file-handling code"*, which is true of the endpoints but **not** of acceptance criterion 5: removing `IBrowserFile` from `ICompanyService` necessarily edits a live Razor page, and the naive edit would have converted a *"File size is too large"* toast into a thrown `IOException` (`OpenReadStream` throws when the file exceeds the limit it is handed). The hazard was caught and neutralised at the call site, but a classification that says "no observable Blazor change" about a task whose own criteria mandate a Razor edit is a classification that would not have flagged it. `M2-A01-02`, `M2-A01-03` (closed) — complexity **HIGH**, risk **HIGH**. `M2-B04` — `task_type: Backend`, `business_rules: [BR-APPR-001]`, `estimate: 1 wk`; `opus` both attempts, `PASS` on attempt 2. `M0-01-03` — `task_type: Database`, complexity **MEDIUM**, risk **HIGH** (DDL against a live instance) → `opus`. `M2-B11` — `task_type: DevOps`, `business_rules: []`, `estimate: 3 d`, `source_files` spans `V.SMART.Shared` and `V.SMART.Api` → complexity **MEDIUM**; risk **MEDIUM** (touches the middleware pipeline shared with `M2-A06`, and a credential-leak surface via `TenantInfo`, but no business logic and no schema change). |
| **Models this run** | `M2-B11`: Implement `opus`, Validate `opus` (both attempts). `M2-B06`: **no agent dispatched** — classified `opus`/`opus` (complexity HIGH) but executed directly in the session after the owner scoped it at the adopt/discard decision, so no independent validator re-derived the result. `M2-A01-02`: Implement `opus`, Validate `opus`. `M2-A01-03`: Implement `opus`, Validate `opus`. `M2-B12-01`: Implement `opus`, Validate `opus` (both attempts). `M2-A08`: Implement `opus`, Validate `opus`. `M2-B04`: Implement `opus` (attempt 1 stopped `environment`-category before reaching validate; attempt 2 same route, `PASS`), Validate `opus`. `M0-01-03`: Implement `opus`, Validate `opus`. |
| **Next ready task** | **None.** `M2-B11` closed and, per the dependency graph, nothing names it as a prerequisite — its close releases no other task. The pool stays empty unless the merge queue moves. **Excluded this round, with reasons:** `M0-06` — `Ready` and P1, but **already has a branch** (`migration/M0-06-remove-default-admin`), so the five-part test's part 5 excludes it; `M0-11` — a **`Product Decision`**, never self-selectable, and surfacing it to the owner *is* the action; `M2-A02` — `Ready` and P0, but gated on the unanswered **Q-28** (an API-only administrator holds zero `UserRight` rows because `AuthController.Login` never calls `SyncRightsForUserAsync`) **and** on **R-65**, whose two phantom screen names would deny every request forever, silently, if either were annotated; `M2-B05` — `Blocked`, premise falsified, awaiting owner re-specification onto **R-66**; `M2-C01` — `Blocked` behind `M2-C00`'s merge; every other task is finished-and-unmerged, awaiting owner review/merge. **`current-task.md` has been rewritten to record this empty state rather than point at a specific task.** |
| **Process note — an orphaned working tree** | 🚩 **A run was killed mid-implementation and left ~979 lines of uncommitted work that no live session would claim.** At the start of 2026-08-21's second session the tree held a near-complete `M2-B06` API implementation — `FilesController.cs`, `CurrencyExcelController.cs`, two contracts, four service files, `Middleware/*` and `Program.cs` — with mtimes of 16:04–16:18 IST, minutes old, while `runner-state.md` said `RUNNING`, *attempt 0, not yet dispatched*. **How it was resolved without guessing:** `ListAgents` found two live peer Claude sessions and **both were asked directly**; both ruled themselves out with specifics (different repositories entirely). `Get-Process devenv` returned nothing, eliminating a human in Visual Studio. `git worktree list` showed the three known sibling worktrees, none on these files. That left a dead runner session as the only consistent explanation, which the 16:04 bookkeeping write corroborates. **The decision was put to the owner, not taken:** adopt, halt-and-report, discard-and-restart, or skip to `M2-B11`. The owner chose **adopt**. The adopted work was then committed **separately** (`e9b143b`) and **verified rather than trusted** — build re-run at baseline, the stream copy confirmed present, every must-not-change file confirmed byte-unchanged, the line-102 defect confirmed still in place. **Three things this should change:** (1) `git status --porcelain` runs *before* this file is believed, every run — a `RUNNING` row over a dirty tree means a killed run; (2) `ListAgents` plus a direct question is a cheap and conclusive concurrency check, and it belongs beside `git worktree list` in Select; (3) an implementer that writes files before committing anything leaves no record of its own existence — the only evidence it ran at all was filesystem mtimes. |
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
