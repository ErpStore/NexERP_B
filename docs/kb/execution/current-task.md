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
dependencies: [KB-081, KB-082, KB-088, KB-107]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task — `M2-A06`, Exception middleware → `ProblemDetails` + correlation ids

**Task file:** [`tasks/M2-A06.md`](tasks/M2-A06.md).

**Status:** `Ready`. Not yet started — this file was rewritten by `M2-C04-01`'s close-out
(2026-08-20) to point here; no branch exists yet.

### Why this task, now

`M2-C04-01` (design tokens, theme, light/dark) closed this session validated `PASS` and moved
to `Needs Review` — not `Completed`, so it does not release `M2-C04-02`/`M2-C04-03`/`M2-C03`
yet ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)). Per the
[Ready-task selection rule](dependency-graph.md#ready-task-selection-rule): `M2-A06` is P0,
its Hard prerequisite is `G0` (already passed), and among the `Ready` P0 candidates it has the
highest downstream-unblocking count — it is a **Hard** prerequisite for `M2-B02`
(→ `M2-B03` → `M2-B10`), `M2-B06` and `M2-B11`. It was recorded as a tied candidate with
`M2-C04-01` while that task was still open; with `M2-C04-01` closed, `M2-A06` stands alone at
the top of the ranking.

Other `Ready` P0/P1 candidates considered and ranked below it (fewer downstream unblocks, or
off the critical path): `M2-B04` (decouple `IApprovalService`), `M2-B12-01` (numbering
investigation), `M2-C10` (decimal handling), `M0-01-03` (rebuild drill, carried M0 debt),
`M0-10` (R-08 audit, P1). `M2-A01-02` is nominally `Ready` but its spec (KB-105 decision D-5)
contradicts current reality (R-40, the `UserId == 1` bypass) — see the warning this file
carried previously, now folded into `M2-A01-02`'s own task file rather than repeated here;
do not open it blind.

### What this task does

Give the API one error contract. Add global exception-handling middleware emitting
`application/problem+json` for every failure, attach a correlation id to every request and
response, and map status codes per **ADR-002 §4** — most importantly, **a business-rule
refusal is `409`, with the service's existing message string carried into `title` verbatim**
(BR-SO-001 — these strings are product UX, never replaced with generic text). Closes **R-24**.

Applies to the two existing controllers (`CurrencyController`, `AuthController`) and their six
endpoints. A **deliberate breaking change**: `DELETE /api/currencies/{id}`'s refusal moves
from `400` to `409`.

### Read before starting

| Doc | Why |
|---|---|
| `docs/kb/decisions/ADR-002-rest-api-layer.md` §4 | The status-code table this task implements |
| `docs/kb/api/api-readiness-assessment.md` (KB-041) | Item A5 and the *Standard error contract* — the target JSON |
| `docs/kb/api/api-overview.md` (KB-040) | Every current response shape (reuse INV-008, `Complete`) |
| `docs/kb/business-rules/business-rule-inventory.md` (KB-030) | BR-SO-001 |
| `docs/kb/architecture/server-side-authorization-spec.md` (KB-105) | The `403` body already defined by `M2-A01-01` — converge on it, do not invent a second shape |
| `docs/kb/architecture/multi-tenancy.md` (KB-014) | Problem 4 — the silent tenant-resolution failure this middleware must render usefully |
| `docs/kb/risks/technical-debt-register.md` (KB-060) | R-24 (closes here), R-19, R-20, R-23 |

### Coordination constraints — read before touching `Program.cs`

- **Same-file conflicts:** `M2-A05` and `M2-B01` both edit
  `V.SMART/V.SMART.Api/Program.cs`. Neither is in flight as of this close-out, but check
  `git branch --no-merged master` before starting — M2 runs multiple branches in parallel and
  this has collided before (see `runner-state.md` process note).
- **`M2-A02`'s tests assert the current (pre-this-task) error shapes deliberately.** `M2-A02`
  is `Blocked` (not yet implemented), so there is nothing to update yet — but if it lands
  before this task starts, its assertions must be updated in the same change as this task's
  contract change, not left asserting the old shapes.
- **The `403` shape must match `M2-A01-02`'s filter exactly.** `M2-A01-02` is `Ready` but not
  implemented (and currently blocked in practice by the D-5/R-40 contradiction above). This
  task should still converge on the `403` shape **KB-105 already specifies**, so whichever of
  the two lands first does not have to be revisited by the other.

### One bounded investigation this task must run

How does the existing service layer signal a business-rule refusal? `CurrencyService` uses a
**tuple return** (`(bool success, string message, ...)`), not an exception — confirmed at
`CurrencyController.cs:64,77,87`. Confirm whether this convention holds more widely, decide how
`409` gets produced (controller helper / result type / thrown domain exception), and record the
decision — every later controller copies it. Reuse INV-008 for the response inventory; do not
re-derive it.

### Do not

Rewrite `CurrencyController.cs` beyond error returns (no route change, no M2-B02 filter DTO).
Edit any business-service message string — they are product UX. Touch
`V.SMART/V.SMART.Shared/**`, `V.SMART.Web/**`, `V.SMART/**` MAUI host, or migrations. Reorder
the pipeline beyond inserting the new middleware first. Fix R-19 (`UserRepository.LoginAsync`
swallowing exceptions) — out of scope, lives in `V.SMART.Shared`, affects live Blazor. Start a
second task after this one.

---

## Carried forward from `M2-C04-01`'s close-out

- **`M2-C04-01` is `Needs Review`, not `Completed`.** Validated `PASS` on 2026-08-20 (branch
  `migration/M2-C04-01-design-tokens`, tip `9f886a6`) — all sixteen acceptance criteria `MET`,
  the coverage regression that stopped attempt 1 closed honestly (branches 100 %, floor
  untouched). One manual step is still owed at review: both themes at 200 % zoom, with
  `prefers-reduced-motion` enabled — not automatable, `jsdom` applies no stylesheet. Full
  record: [`tasks/M2-C04-01.md` § Execution Record (2026-08-20)](tasks/M2-C04-01.md#execution-record-2026-08-20).
  `M2-C04-02`, `M2-C04-03` and `M2-C03` stay `Blocked` until this is reviewed and merged.
- **`UserThemePreference.IsDarkMode` is a bare `bool`, cannot represent `system`.** Recorded as
  an INV-006 amendment and **Q-33** (owner: product + backend, needed by M3-3). No entity
  change was made. Not relevant to `M2-A06`, but recorded here so it is not re-discovered.
- **The theme layer's byte cost is recorded** in `frontend/nexgen-web/README.md` against
  KB-050's `< 250 KB gzip` target: entry JS 91.59 kB gzip, well inside budget.

## Ready and unclaimed once `M2-A06` closes

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
Listed for whoever plans the task after this one — not to be started now.

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-B04` | Decouple `IApprovalService` + 13 `Pages` refs | 1 wk | Largest single extraction in M2-B |
| `M2-B12-01` | INV-012 numbering investigation | 2 d | Investigation-only; unblocks the numbering chain |
| `M2-B01` | API versioning → `/api/v1` | 1 d | Same-file conflict with `M2-A05` — check nothing is in flight |
| `M2-B05` | Typed `ScreenCodes` constants (R-10) | 2 d | Feeds `M2-A01-02`'s filter |
| `M2-C10` | Decimal handling — no float money arithmetic | 2 d | P0 correctness, blocks `M2-C07` |
| `M2-C11` | Archive the Angular pilot | 0.5 d | P2 housekeeping |
| `M0-01-03` | Deployment script + rebuild runbook | 1 d | Closes a G0 exception; hardware no longer blocks it |
| `M0-10` | R-08 compute-one/test-another guards audit | — | M0 debt carried into M2 |
| `M2-A01-02` | `[RequireScreen]` / `[RequireRight]` | 3 d | **Do not open blind** — spec contradicts reality, see the warning above |

`M2-B12`, `M2-C04`, `M2-C05`, `M2-A01` and `M2-D02` are **parent containers** and are never
worked directly.

## What M2 inherits from M0 — "gate passed" is not "clean slate"

G0 passed **with three exceptions**, all owner-deferred, none with a date set
([KB-107 §1](M0-milestone-review.md)):

1. **Criterion 1 — no rebuild drill.** `M0-01-03` is `Ready` (a SQL Server instance was on
   this workstation the whole time — see below); the drill itself has not run.
2. **Criterion 2 — secrets still in history** (`M0-05`, `Blocked` behind `M0-04`).
3. **Criterion 3 — production credentials unrotated, in a public repository** (R-01). `M0-04`
   is `Blocked` on an unidentified owner and now also gated on **Q-32** (the `Tenants` table
   stores the same `sa` credential in plaintext — rotating without answering Q-32 first breaks
   every tenant row that embeds it).

## Two standing process notes for M2

- **Check `git branch --no-merged master` before allocating any KB/INV/Q id.** Six id
  collisions have already happened across parallel M2 branches — `grep`-before-claim cannot
  see a sibling branch.
- **`master` requires pull requests**, but no required status check gates merges yet — that is
  the open half of **Q-20**. The owner holds bypass rights; prefer a PR regardless.

## Baselines as of this file

| | |
|---|---|
| `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` | **84 passed, 0 failed** |
| `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj --no-incremental` | **0 errors, 6,694 warnings** (baseline 6,695) |
| `frontend/nexgen-web` — `npm run typecheck && lint && test -- --run && build && coverage` | All exit 0 as of `M2-C04-01` tip `9f886a6` (2026-08-20); coverage branches **100 %** |
| CI on `master` | green |

The authoritative, continuously-updated command table is
[KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands).
