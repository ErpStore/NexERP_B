---
doc_id: KB-089
title: Current Task
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: active
confidence: n/a
last_verified: 2026-08-20
dependencies: [KB-081, KB-082, KB-088, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task — `M2-B04` — decouple `IApprovalService` and the other `Pages`-referencing files

**Task file:** [`tasks/M2-B04.md`](tasks/M2-B04.md). **Status:** `Not Started` (tracker says
`Ready`), attempt 0 of 3, no branch yet. **Type:** Backend. **Priority:** P0. **Estimate:** 1 wk.
**Depends on:** `M2-B07` — `Completed` and merged (`ffbb1dd`, 2026-08-19), so this Hard
prerequisite is genuinely satisfied, not just `Needs Review`.

### What it does

Removes every compile-time dependency from non-UI `V.SMART.Shared` code onto the
`V.SMART.Shared.Pages` namespace, and installs an automated guard that fails the build if one
is reintroduced. The headline case: `IApprovalService.cs:8` and `ApprovalService.cs:14`
`using static`-import a Razor page type (`Authorization.razor`) directly into a business-layer
interface/service — Confirmed, re-verified 2026-08-12. After this task, `BusinessLayer/`,
`Data/`, `Repository/`, `Mappings/`, `ViewModels/` and `E_Invoice/` contain zero references to
`V.SMART.Shared.Pages`, so the approval workflow (and the other `source_files` this task
lists — `EinvoiceDatabaseService.cs`, `StockAddIssPosition.cs`, `MaintenanceProcessService.cs`,
`GetRatingsService.cs`, `FundTrans.cs`, `ReceiptsSub.cs`, `MaterialReq.cs`, `PerformaInv.cs`,
`JsonSerializer.cs`, `ProductionIssuAssyMapping.cs`, `PurchaseSCNRepository.cs`,
`FundTransFilterVM.cs`) can be exposed over HTTP without dragging a Razor component into the
API's activation path. Serves **R-11** (KB-060) and **A6** of KB-041's P0 blocker list.
**Re-verify every cited `file:line` before trusting it** — this task file's `last_verified` is
2026-08-12, older than most of the tasks landed since.

### Why this task, not one of the others

Selected per the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule):
P0, `Ready`, its one Hard prerequisite (`M2-B07`) is genuinely `Completed` and merged (not
merely `Needs Review`), no unanswered Information dependency, not a parent container, not
gated on an unscheduled human step, no sibling branch open on it, no same-file conflict with
any branch currently outstanding.

**Excluded from the candidate set, and why:**

- `M2-C00`, `M2-A07`, `M2-A08` — all implemented, validated `PASS`, but `Needs Review` and
  **unmerged**. A `Needs Review` prerequisite does not release a `Hard` dependent
  (selection rule step 1), and none of these three is itself unblocked further work — they
  are done, awaiting the owner's merge.
- `M2-B12-01` — its own branch (`migration/M2-B12-01-inv-012-numbering`) already ran INV-012
  and closed **`BLOCKED`** ("escalation budget exhausted"), unmerged. The tracker on this
  branch still stale-reads it as `Ready`; do not re-select it without reconciling that branch
  first (read `tasks/M2-B12-01.md` and its branch tip `407d0ba` before touching it again).
- `M2-A02` — `Ready` but gated on unanswered **Q-28** (an API-only administrator would
  authenticate into an empty UI; `AuthController.Login` never calls
  `SyncRightsForUserAsync`).
- `M2-C01`, `M2-C04-02`, and the rest of the `M2-C*`/`M2-D*` tree — every file carries a ⛔
  STOP banner pending Angular re-specification against [ADR-007](../decisions/ADR-007-angular-stack.md).
  `M2-C00` (the task that lifts the banner tree-wide) is done but unmerged.
- **`M0-01-03`** — tracker footnote ²¹ reopened it to `Ready` because the SQL Server instance
  that used to block it is now confirmed present (`MSSQL$SQLEXPRESS`, `.\SQLEXPRESS`). **But
  the task file's own Implementation Steps (step 7) are explicit: "Hand the drill to a human.
  You cannot execute it."** All repository-side deliverables already landed on
  `migration/M0-01-03-sp-deployment-and-rebuild-runbook` (unmerged); what remains is a real
  tenant-database rebuild drill, which the task itself reserves for a **named human**, not an
  AI session — hardware being reachable does not change that instruction. Per
  [dependency-graph.md § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
  step 1, bullet 4, a task "blocked on a human step nobody has scheduled" is `BLOCKED` with a
  named owner, and surfacing it is the useful action — so it was **not** selected here.
  **If the owner wants to schedule the drill, or explicitly authorizes an AI session to run it
  against the confirmed local SQL Server Express instance, say so and it becomes selectable.**

### Ready and unclaimed after `M2-B04`

| Task | What | Est. | Priority | Note |
|---|---|---|---|---|
| `M2-B01` | API versioning → `/api/v1` | 1 d | P1 | Released by `M2-B07` |
| `M2-B05` | Typed `ScreenCodes` constants (R-10) | 2 d | P1 | Released by `M2-B07` |
| `M2-B06` | File upload / download endpoints | 1 wk | P1 | Released by `M2-A06` |
| `M2-B09` | Reference-data endpoints + caching | 3 d | P1 | Released by `M2-B07`, `M2-B02` |
| `M2-B11` | Health checks + structured logging (R-23) | 3 d | P2 | Released by `M2-A06` |

**A session may run more than one of these.** Per CLAUDE.md's five-part "can actually be done"
test: **One task, one branch, cut from `master`** is unchanged, as is *never merge, never
push*.

---

## Awaiting owner merge — nothing further to do until reviewed

`M2-C00` (`migration/M2-C00-kb050-angular-rewrite`, `b3c0e6e`), `M2-A07`
(`migration/M2-A07-me-endpoint`, `61da4bd`) and `M2-A08`
(`migration/M2-A08-row-scope-and-account-gates`, `0706263`) are all implemented, validated
`PASS`, and sitting on their own unmerged branches. Merging any of them releases real work:
`M2-C00` unblocks the entire `M2-C` tree, `M2-A07` and `M2-A08` (once `M2-A01-03` is also
merged) unblock `M2-C02` and `M2-D01` respectively.

## Carried forward from `M2-A08` — read before touching row scope, auth gates, or Leads

- **Row-level scoping is real on exactly one entity — `Leads` — and nowhere else.**
  `User.StateCodesCsv` does **not** filter customers, vendors, items or any other query
  anywhere in `V.SMART/` (negative grep, independently reproduced by two sessions). Do not
  assume `StateCodesCsv` scopes anything beyond `LeadService.GetAllLoadLeadsAsync`. Evidence
  and the corrected `Q-08` answer: [`open-questions.md`](../open-questions.md) and
  [KB-108](../architecture/row-scope-and-account-gates.md) §2.
- **The row-scope mechanism to reuse, not reinvent:** query-level composition via
  `V.SMART.Api/Authorization/RowScopeQueryExtensions.ApplyRowScope`, backed by `RowScope`,
  `IRowScopeProvider`, `ScopedEntityCatalogue`, `[RowScoped]`/`[NoRowScope]`, and a startup
  validator that refuses to boot over an undeclared scoped entity. `M2-B03`'s controller
  template and `M2-B08`'s report/print endpoints should apply this pattern, not the
  in-memory/opt-in pattern `LeadService` uses today (KB-108 §5).
- **Two undocumented Leads scope leaks were found and are not yet fixed anywhere:**
  `LeadsList.razor:470-484` exempts `UserId == 1` from row scope entirely, and
  `LeadsList.razor:396-401` computes the **paging total from the unscoped query** for every
  user regardless of their own scope. Neither was in scope for `M2-A08` to fix (Blazor
  `Pages/`/`BusinessLayer/` were off-limits); whichever task first exposes a scoped Leads
  endpoint (`M2-B03`-shaped, likely a future `M2-D`-tree task) must not reproduce either leak.
- **Trial gate is enforced on `POST /api/auth/login`**; device gate is **not** — the evaluator
  exists (`V.SMART.Api/Auth/AccountGates.cs`, tested) but nothing calls it. **Q-38**: no task
  currently owns wiring it in, because `M2-A04` (the assumed owner) is itself `Blocked`. If a
  future task touches the login path, read Q-38 and KB-108 §4/§8 first.
- **Q-37** (open): is the trial gate's `!IsDesktop` exemption (`Login.razor:271`) deliberate
  licensing policy or an oversight? Owner-only; do not guess before any desktop-hosted API
  deployment.
- `GetUserByQrToken` now excludes expired tokens at the query (the fix ships on the unmerged
  `M2-A08` branch) — once merged, any new caller can trust the repository method directly
  instead of re-checking `QrExpiryDate` itself.

## Carried forward — still relevant (from `M2-A01-02`, merged `ed559ad`)

- **`V.SMART/V.SMART.Api/Authorization/` exists**, all ten types KB-105 §2 specifies —
  `Right`, `[RequireScreen]`, `[RequireRight]`, `[NoScreenRight]`, `IUserRightsProvider` (now
  cached, per `M2-A01-03`, `Needs Review`/unmerged), `ScreenRightAuthorizationFilter`,
  `ScreenRightSet`, `ScreenCatalogue`, `ScreenRightStartupValidator`, registered in
  `Program.cs`. **No controller is annotated yet** — still `M2-A02`'s job, and `M2-A02` is
  gated on `Q-28`.
- **⚠ THE FILTER IS OPT-IN, NOT DENY-BY-DEFAULT — `M2-A02` must close this.** An authenticated
  action on a controller carrying no `[RequireScreen]` at all is allowed through today, both
  at request time and at startup. Tracked against **R-03** (KB-060).
- **R-40 / D-5 contradiction** (`UserId == 1` auto-granted all rights by `Login.razor`, vs.
  KB-105 D-5 "no Administrator bypass") is confirmed **not hit** by the current filter — the
  bypass lives only in the Blazor login path. Still unresolved for `M2-A02`/API logins: an
  API-only administrator holds zero `UserRight` rows unless `Q-28` is settled.
