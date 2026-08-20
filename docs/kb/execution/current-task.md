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
dependencies: [KB-081, KB-082, KB-088, KB-105]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task — `M2-A01-02` — implement `[RequireScreen]` / `[RequireRight]`

**Task file:** [`tasks/M2-A01-02.md`](tasks/M2-A01-02.md).

**Status:** `Ready`. Not yet started — no branch exists yet.

### Why this task, now

`M2-B02` (server-side paging/sort/filter contract) closed this session validated `PASS` and
moved to `Needs Review` — not `Completed`, so per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) it did **not** release its
Hard-dependents (`M2-B03`, `M2-B09`, `M2-C05`, `M2-C05-01`); they stay `Blocked` until the
branch is reviewed and merged.

Applying the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the genuinely `Ready` P0 candidates (`M0-01-03`, `M2-A01-02`, `M2-B04`, `M2-B12-01`,
`M2-C04-02`, `M2-C04-03`, `M2-C10` — the last two became `Ready` on `M2-C04-01`'s merge and
were not in the candidate set the last time this file was written):

- **Step 1 (P0/P1/P2)** is a tie — all seven are P0.
- **Step 2 (most downstream unblocking that actually fires)** narrows it to two.
  `M2-A01-02`'s only dependent, `M2-A01-03`, names *only* `M2-A01-02` in `depends_on` — finishing
  it makes a real task `Ready`. `M2-B12-01`'s only dependent, `M2-B12-02`, is the same shape.
  Every other candidate's apparent dependents are false unblocks, same reasoning this file used
  last time for `M2-C10`/`M2-C07`: `M2-C04-02`'s two dependents (`M2-C05`, `M2-C05-01`) also need
  `M2-B02`, still `Needs Review` not merged; `M2-C10`'s dependent `M2-C07` also needs
  `M2-C05-01`, nowhere near `Ready`; `M0-01-03`, `M2-B04`, `M2-C04-03` have zero dependents in
  the tracker at all.
- **Step 3 (critical path) breaks the `M2-A01-02` / `M2-B12-01` tie.** The stated critical path
  ([KB-082 § Project critical path](dependency-graph.md#project-critical-path)) is
  `… → M2-A01-01 → M2-A01-02 → M2-A01-03 → M2-A02 → M2-A03 → …`. `M2-B12-01` is not on it.
  `M2-A01-02` wins outright — no further tie-break step is needed.

**Read the caution before starting, do not open this blind.** The tracker has flagged this
task since `M2-A01-01` closed: **R-40** (`docs/kb/execution/M0-milestone-review.md` §4) records
that `UserId == 1` is auto-granted **all 152 screen rights** by `Login.razor:345-349`, which
directly **contradicts** `docs/kb/architecture/server-side-authorization-spec.md`'s decision
**D-5 — "No `Administrator` bypass. None. Anywhere."** (`server-side-authorization-spec.md:487`).
The spec's own truth table (`:335`, row T-13) asserts no bypass exists because `RightsHelper`
never reads a role — that is Confirmed for the *Blazor* rights-check path, but does not account
for the login-time auto-seed. This is not this task's contradiction to resolve by guessing:
if implementing `[RequireScreen]`/`[RequireRight]` strictly per D-5 (deny by default, no
bypass) means `UserId == 1` gets 403'd through the API despite always passing in Blazor, **stop
and record it** — the task's own *Investigation Requirements* section says exactly this:
*"If, while implementing, the code contradicts the specification … stop, record the
contradiction with `file:line` evidence, and report it. Do not silently redesign around it."*
Related: **Q-28** (`open-questions.md`) — an API-only administrator can hold zero `UserRight`
rows, since `AuthController.Login` never calls `SyncRightsForUserAsync`. Q-28 is scoped to
**block `M2-A02`, not this task** (`task-tracker.md` footnote ¹⁸ says so explicitly) — but the
same seeding gap is almost certainly *why* R-40's bypass exists, so read both before writing
the filter.

### What this task does

Implements, in `V.SMART/V.SMART.Api/`, the authorization mechanism `M2-A01-01` specified: a
`Right` enum, a controller-level `[RequireScreen]` attribute, an action-level `[RequireRight]`
attribute, and an `IAsyncAuthorizationFilter` resolving the caller's `UserRight` rows and
short-circuiting `403`. Caching is deliberately **not** implemented here — that is `M2-A01-03`.
No controller is annotated in this task — that is `M2-A02`. Full detail, acceptance criteria and
the fresh-session execution prompt: [`tasks/M2-A01-02.md`](tasks/M2-A01-02.md).

### Read before starting

The task file's own *Required Existing Knowledge* section is authoritative
([`tasks/M2-A01-02.md`](tasks/M2-A01-02.md#required-existing-knowledge)) — in particular
`docs/kb/architecture/server-side-authorization-spec.md` (KB-105, the primary input, created by
`M2-A01-01`), ADR-004, ADR-002 §4 (the `403` body must be `application/problem+json` — and note
`M2-A06`'s `ProblemDetails` middleware is now `Completed` and merged, upgrading that dependency
from Soft to something worth reusing directly rather than duplicating body-construction code),
KB-013, KB-014, KB-040, KB-060 (R-03, R-18, R-26), KB-030 BR-AUTH-002. Its *Dependencies* table
also flags: `M2-B07`'s `AddVSmartDomain()` is **Information only** here (the filter's only
domain dependency is `IUnitOfWork`, already registered at `Program.cs:100`) — verify that is
still true before starting, since if the design has grown a second service dependency M2-B07
becomes Hard and this task blocks (R-26).

### Do not

Start `M2-A01-03` (caching) or `M2-A02` (applying the filter to a controller) in this session.
Redesign around the D-5/R-40 contradiction without recording it first. Pre-empt `M2-B01`
(`/api/v1` routing) or `M2-B05` (`ScreenCodes` constants, R-10) — screen names stay string
literals here.

---

## Carried forward from `M2-B02`'s close-out

- **`M2-B02` is `Completed` and merged** (`feec964`, 2026-08-20), from `migration/M2-B02-paging-contract`
  (`c603115`). Validated `PASS` — all eighteen acceptance criteria `MET`, including the
  `toDate` 23:59 boundary (verified one level below HTTP, through the real, untouched
  `CurrencyFilterBuilder` predicate — a real SQL Server round trip is still blocked by every
  dev-tenant `Currency` row having a null `CreatedDate`). **That limitation was independently
  confirmed at review**, read-only against the local `SQLEXPRESS` tenant:
  `SELECT COUNT(*), COUNT(CreatedDate) FROM Currency` → **`3, 0`** — three rows, none with a
  `CreatedDate`. The constraint is real, not an excuse. Full record:
  [`tasks/M2-B02.md` § Execution Record (2026-08-20)](tasks/M2-B02.md#execution-record-2026-08-20)
  and `task-tracker.md` footnote ²⁴. The merge releases **`M2-B09`** to `Ready`. **`M2-B03`, `M2-C05` and `M2-C05-01` stay `Blocked`** on their *other* prerequisites — `M2-A02` and `M2-C04-02` respectively, neither of which is done
  — not on `M2-B02` any more.
- **A binding convention for every future `[FromQuery]` query DTO**, recorded in
  [ADR-002 §2a](decisions/ADR-002-rest-api-layer.md): `[FromQuery]` on a record binds by CLR
  property name and Swashbuckle emits it verbatim, so every bound property needs an explicit
  `[FromQuery(Name = "camelCaseName")]` or the OpenAPI document (and the generated TypeScript
  client `M2-B10` produces) silently drifts to PascalCase. Not relevant to `M2-A01-02` directly
  (it adds no query DTO), but binding on `M2-B03`'s controller template.
- **INV-041 (`Complete`):** no business service takes a `sort` parameter; the chosen mechanism
  for `M2-B02` was an additive overload, not a filter-dictionary key or controller-side sort —
  see the ADR for the full rejected-options table if a later task considers either again.
- **Q-36 raised, not `M2-A01-02`'s concern:** `CurrencyList.razor:758-760` sets a `Status`
  filter key `CurrencyFilterBuilder` has no case for, so that dropdown already filters nothing
  in production, silently.

## Ready and unclaimed once `M2-A01-02` closes

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
Listed for whoever plans the task after this one — not to be started now.

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-B12-01` | INV-012 document-numbering investigation | 2 d | Lost the tie to `M2-A01-02` only on critical-path placement; unblocks `M2-B12-02` immediately |
| `M2-B04` | Decouple `IApprovalService` + 13 `Pages` refs | 1 wk | Largest single extraction in M2-B; 0 direct dependents |
| `M2-C10` | Decimal handling — no float money arithmetic | 2 d | P0 correctness; its dependent `M2-C07` also needs `M2-C05-01`, not `Ready`, so finishing this alone unblocks nothing yet |
| `M2-C04-02` | Form controls + validation display | 4 d | P0; its dependents (`M2-C05`, `M2-C05-01`) also need `M2-B02` merged |
| `M2-C04-03` | Modal, drawer, toast, states | 3 d | P0; 0 direct dependents in the tracker |
| `M0-01-03` | Deployment script + rebuild runbook | 1 d | Closes a G0 exception; a local `SQLEXPRESS` instance is confirmed available (tracker footnote ²¹) |
| `M2-B01` | API versioning → `/api/v1` | 1 d | P1. Same-file conflict with `M2-A05` — check nothing is in flight before opening |
| `M2-B05` | Typed `ScreenCodes` constants (R-10) | 2 d | P1. Feeds `M2-A01-02`'s successor work |
| `M2-B06` | File upload / download endpoints | 1 wk | P1, released by `M2-A06`'s merge |
| `M2-B11` | Health checks + structured logging (R-23) | 3 d | P2, released by `M2-A06`'s merge; shares the middleware pipeline with `M2-A06` — check nothing else is mid-edit there |
| `M2-C11` | Archive the Angular pilot | 0.5 d | P2 housekeeping |
| `M0-10` | R-08 compute-one/test-another guards audit | 2 d | P1. M0 debt carried into M2 |

`M2-B12`, `M2-C04`, `M2-C05`, `M2-A01` and `M2-D02` are **parent containers** and are never
worked directly.

## What M2 inherits from M0 — "gate passed" is not "clean slate"

G0 passed **with three exceptions**, all owner-deferred, none with a date set
([KB-107 §1](M0-milestone-review.md)):

1. **Criterion 1 — no rebuild drill.** `M0-01-03` is `Ready` (a SQL Server instance was on
   this workstation the whole time); the drill itself has not run.
2. **Criterion 2 — secrets still in history** (`M0-05`, `Blocked` behind `M0-04`).
3. **Criterion 3 — production credentials unrotated, in a public repository** (R-01). `M0-04`
   is `Blocked` on an unidentified owner and now also gated on **Q-32** (the `Tenants` table
   stores the same `sa` credential in plaintext — rotating without answering Q-32 first breaks
   every tenant row that embeds it).

## Two standing process notes for M2

- **Check `git branch --no-merged master` before allocating any KB/INV/Q id.** Several id
  collisions have already happened across parallel M2 branches — `grep`-before-claim cannot
  see a sibling branch.
- **`master` requires pull requests**, but no required status check gates merges yet — that is
  the open half of **Q-20**. The owner holds bypass rights; prefer a PR regardless.

## Baselines as of this file

| | |
|---|---|
| `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` | **84 passed, 0 failed** |
| `dotnet test tests/V.SMART.Api.Tests/V.SMART.Api.Tests.csproj` | **56 passed, 0 failed** on `migration/M2-B02-paging-contract` (`c603115`; `master` itself is still at 21, since `M2-B02` is unmerged) |
| `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj --no-incremental` | **0 errors, 6,695 warnings on `master`** (baseline); `6,695` again on the `M2-B02` branch — no new warnings |
| `frontend/nexgen-web` — `npm run typecheck && lint && test -- --run && build && coverage` | All exit 0 as of `M2-C04-01` tip `9f886a6` (2026-08-20); coverage branches **100 %** |
| CI on `master` | **green**, owner-confirmed 2026-08-20 at `e63716e` — `V.SMART.Shared.Tests` (84), `V.SMART.Api.Tests` (21, pre-`M2-B02`) and the frontend Vitest run (150) plus its `branches: 100` coverage gate |
| **CI does not gate merges** | Still true — the open half of **Q-20**. Green CI that nothing enforces is a smoke alarm with the battery out |

`M2-A01-02` writes only new files under `V.SMART.Api/Authorization/` plus a `Program.cs`
registration — it needs a running `dotnet build` and, per its own *Testing* section, whatever
harness it specifies; it does not need the frontend suite or the tenant database. The
authoritative, continuously-updated command table is
[KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands).
