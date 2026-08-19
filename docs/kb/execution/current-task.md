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
last_verified: 2026-08-19
dependencies: [KB-081, KB-082, KB-088]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task: **none.** Gate G0 still bars M2, and no M0 task is selectable.

Nothing is in progress. The last session closed `M2-A01-01` (below) under a one-off,
owner-authorised gate exception; **that exception is spent and does not transfer.** No task
satisfies the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).

**Do not open `M2-A01-02`, or any other M2 task.** Every one of them declares `G0` — directly
or transitively — and G0 has **zero of seven exit criteria ticked**
([KB-080 §7](README.md#7-m0--stabilise)). `M2-A01-01` was exempted only because it produces
documentation and changes no behaviour. `M2-A01-02` writes the authorization filter, and
[KB-105 §9](../architecture/server-side-authorization-spec.md) lists verification for it that
**cannot run until `M0-12-01` creates a test project** — which is itself `Blocked`.

## What a human must clear, before anything else can move

Four blockers, three of them the repository owner's. This is the whole critical path.

| # | Blocker | Status | Owner |
|---|---|---|---|
| 1 | **`M0-12-01`** — create the test project and wire it into CI | `Blocked`¹² — **two** dispatches (2026-08-18), both returned no result. **1 of 3 attempts left** (corrected 2026-08-19 — see below). Do not spend it on a third identical re-dispatch before someone inspects the agent-dispatch layer (**Q-21**) | Whoever administers the runner's dispatch layer; fallback **Vivek** |
| 2 | **`M0-01-03`** — rebuild drill | `Needs Review`¹ — repo-side work merged; `db/REBUILD-DRILL-LOG.md` is a skeleton, every field `TBD`. A hard G0 exit criterion | **Vivek** — needs a disposable SQL Server instance |
| 3 | **`M0-07`** — CI green on `master` | `Completed`⁷ as a task, but the G0 box stays **unticked**: never run on a hosted runner, `master` does not carry the workflow, `ci/warning-baseline.json` is still `provisional`, no required status check exists (**Q-20**) | **Vivek** — GitHub org admin |
| 4 | **`M0-04`** — rotate the exposed credentials | `Blocked`⁴ — owner never identified. Also blocks `M0-05` (purge secrets from history), whose other prerequisite `M0-03` is `Completed` | Unidentified ops/infra person |

Blocked transitively behind `M0-12-01`: `M0-12`, `M0-12-02`, `M0-13`, `M0-09`, `M0-10`,
`M0-06`, `M0-11`. Parent containers, never worked directly: `M0-01`, `M0-12`.

Full detail and candidate owners: [`runner-state.md`](runner-state.md) (KB-093);
[`task-tracker.md`](task-tracker.md) (KB-081) footnotes 1, 4, 7, 12, 13.

> **Bookkeeping inconsistency — RESOLVED 2026-08-19, on `master` in `50a97c9` and `56d8389`.**
> `runner-state.md` claimed `M0-12-01` had spent 3 of 4 attempts. **Both numbers were wrong.**
> **Numerator:** the "3" counted the 2026-08-18 pass that made *no dispatch at all* — it recorded
> `0 of 4 used this pass` and `n/a — no dispatch made this pass` — as though it were an attempt.
> `task-tracker.md` footnote 12, `failure-log.md` (two entries) and `tasks/M0-12-01.md` (two
> Execution Records) all agree on **two**, and KB-093's own tie-break rule — *"If this file and
> KB-081 disagree, KB-081 wins and this file is corrected"* — settles it.
> **Denominator:** the budget is **3, not 4**, per [KB-091 §6.4](autonomous-runner.md#64-retry-rules)
> — *"Attempt 3 fails → **`BLOCKED`**. Stop. … Do not attempt a fourth."* — and
> `.claude/workflows/migration-runner.js:43` (`maxRetries: 2, // 2 retries = up to 3 implementation
> attempts`), confirmed live by the 2026-08-19 run reporting `maxRetries: 2`. No configured value
> anywhere is 4; every "of 4" in this KB was agent prose.
> **Net: `M0-12-01` has used 2 of 3 — exactly one attempt remains.** The standing instruction is
> unchanged and now correctly grounded: do not re-dispatch until **Q-21** is answered, because a
> third failure ends the budget outright.

## Most recently closed: `M2-A01-01` — Implementation spec from ADR-004

`Needs Review` on `migration/M2-A01-01-authorization-spec`. Documentation only; no `.cs`,
`.razor`, `.csproj` or `.json` file touched. `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj`
→ **0 errors, 6,695 warnings**, matching the [KB-086](M0-15-build-baseline.md) baseline exactly,
which is how "nothing under `V.SMART/` changed" was proven. Executed under the gate exception
described in [KB-081 footnote 13](task-tracker.md) and
[KB-105 §12](../architecture/server-side-authorization-spec.md).

**Deliverable:** [`architecture/server-side-authorization-spec.md`](../architecture/server-side-authorization-spec.md),
`doc_id` **KB-105** (not `KB-016` as the task file suggested — the INDEX allocation rule
reserves `KB-1xx` for task-produced contract specs; the task file's own instruction to verify
against the INDEX is what produced this). Full record:
[`tasks/M2-A01-01.md` § Execution Record (2026-08-18)](tasks/M2-A01-01.md#execution-record-2026-08-18).

### Discoveries a future session must reuse rather than re-derive

- **`Program.cs` line numbers have shifted again.** In `V.SMART/V.SMART.Api/Program.cs`,
  `UseAuthentication()` is at **`:121`** and `UseAuthorization()` at **`:122`** — the
  `M2-A01-01` task file says `:114`/`:115`. `JwtTokenService`'s claim list is at `:29-35`, not
  `:25-31`. Both API and Web `Program.cs` are shared composition roots; **always re-read
  before citing a line number in either.**
- **`RightsHelper`'s screen-name match is LINQ-to-Objects, not SQL.**
  `UserRightsRepository.cs:27` calls `ToListAsync()` before `RightsHelper.cs:8` runs, so the
  comparison is ordinal and case-sensitive. Pushing it into SQL would make it
  collation-dependent and silently *widen* access. This single fact decides the filter's
  design ([KB-105 §D-1](../architecture/server-side-authorization-spec.md)).
- **`Screens.ScreenName` is `nvarchar(max)`** — SQL Server cannot index it as a key column, so
  no uniqueness is enforceable on it as declared.
- **`CurrentUserService.GetUserIdAsync()` returns `0`** for a missing or unparseable claim
  (`:59-65`). Anything on the API side that needs a user id must read the claim directly.
- **No code path writes a `Screens` row** — the 152 seeded rows are the entire runtime
  catalogue (Confirmed negative result; the full list is KB-105 Appendix A).
- **`M2-B05` must not "correct" the seed's typos.** `Id = 82` is `"Sub-Contrect GRN"`; `Id =
  107` is `"Advaceadjustment"`. They are the canonical matching strings.

### Three new questions, one of which blocks a specific task

| ID | In one line | Blocks |
|---|---|---|
| **Q-27** | Do duplicate `(UserId, ScreenId)` rows exist in live tenant DBs? Nothing in the model prevents them, and the rights query has no `OrderBy`, so Blazor's `FirstOrDefault` is **already non-deterministic today** | Nothing immediately — decides whether the filter faithfully reproduces correct behaviour or a latent bug |
| **Q-28** | An API-only user acquires **no** `UserRight` rows: `AuthController.Login` never calls `SyncRightsForUserAsync`, and the Blazor path does so only for `UserId == 1` | **`M2-A02`** — the filter would 403 an administrator out of the vertical slice on its first request |
| **Q-29** | All five `UserRight` write sites are in the **Blazor** host, none in the API, so cross-process cache invalidation does not exist and the ≈60 s TTL is the only staleness bound | Scope of `M2-A01-03` |

`Q-28` does **not** block `M2-A01-02`; it blocks `M2-A02`. All three are in
[`open-questions.md`](../open-questions.md).

## Other open blockers, unchanged by this session

- **`Needs Review`** — implemented, validated, committed on its own branch, awaiting a human
  review-and-merge step no autonomous session may perform
  ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)): `M0-01-03`, `M0-02`,
  and now `M2-A01-01`.
- **`Blocked` on an unscheduled human**, not on any task: `M0-04`.
