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
dependencies: [KB-081, KB-082, KB-088, KB-107]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## No active task — `M2-C01` closed, next task not yet selected

**`M2-C01` is `Completed` and merged (`12f172f`, 2026-08-19).** `frontend/nexgen-web/` is on
`master`: Vite 6 + React 19 + TypeScript `strict`, Mantine 7, TanStack Query, Vitest + RTL +
MSW, a Playwright scaffold, and the KB-050 feature-sliced skeleton. The canonical frontend
commands (`npm ci`, `typecheck`, `lint`, `format:check`, `test`, `coverage`, `build`, `e2e`)
are established and recorded in
[KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands).

**Read the close as a waiver, not a pass.** 14 of 15 acceptance criteria were met. The 15th —
criterion 10's second half, *"the `frontend` job … is green on the branch"* — was **never
satisfied**; it needs a push, which an execution session may not make. The owner waived it on
the `M0-07` (`d79e1a4`) and `M0-12-02` (`a83f1e2`) precedent. Full record:
[`task-tracker.md`](task-tracker.md) footnote ¹⁹.

---

## Ready and unclaimed — six tasks

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).

| Task | What | Est. | Why you might take it first |
|---|---|---|---|
| **`M2-B07`** | Shared `AddVSmartDomain()` DI extension | 3 d | **Unblocks the most** — `B01`, `B04`, `B05`, `B08`, `B09` all wait on it. The DI seam everything in M2-B hangs off |
| **`M2-A06`** | Exception middleware → `ProblemDetails` | 3–5 d | Unblocks `B02`, `B06`, `B11`. Establishes the error contract every controller then relies on |
| **`M2-C04-01`** | Design tokens, theme, light/dark | 3 d | The first task where "make the UI genuinely better" becomes concrete decisions. Blocks `C04-02`, `C04-03`, `C03` |
| **`M2-C10`** | Decimal handling — no float money arithmetic | 2 d | P0 correctness. Blocks `C07`. Cheap, self-contained, and wrong-by-default if deferred |
| **`M2-C11`** | Archive the Angular pilot | 0.5 d | P2 housekeeping. The smallest unit of real progress available |
| **`M2-A01-02`** | Implement `[RequireScreen]` / `[RequireRight]` | 3 d | Spec merged ([KB-105](../architecture/server-side-authorization-spec.md)) — but **read the warning below before opening it** |

**`M2-C04` is a parent container** and is never worked directly — its implementable scope lives
entirely in `M2-C04-01/02/03`. Same for `M2-C05` and `M2-A01`.

---

## Before `M2-A01-02` is opened — the spec contradicts reality

[KB-105](../architecture/server-side-authorization-spec.md)'s decision **D-5** states there is
**no Administrator bypass, anywhere**. That was written on 2026-08-18 and is **wrong as
written**.

`M0-06` subsequently found **R-40**: `UserId == 1` is an undeclared superuser.
`Login.razor:345-349` auto-grants it all 152 screen rights on every login; no `UserRight` rows
are seeded at all; and rights are deny-by-default (`RightsHelper.cs:7-20`). So a replacement
administrator created with any other `UserId` authenticates successfully and then **sees
nothing**.

**This is not a thing to discover mid-implementation.** Either D-5 changes, or the bypass is
removed and a rights-provisioning path replaces it — and that second option overlaps `M0-06`,
which is itself `Blocked` on Q-25/Q-26. Reconcile before writing the filter, not after.

Related and still unanswered: **Q-28** — an API-only user acquires no `UserRight` rows at all,
because `AuthController.Login` never calls `SyncRightsForUserAsync`. That blocks `M2-A02`
rather than `M2-A01-02`, but it is the same root cause.

---

## What M2 inherits from M0 — "gate passed" is not "clean slate"

G0 passed **with three exceptions**, all owner-deferred, **none with a date set**
([KB-107 §1](M0-milestone-review.md)):

1. **Criterion 1 — no rebuild drill.** No evidence a tenant database can be reconstructed from
   source control alone. Blocked on a disposable SQL Server, not on work. Surfaces at **M6**,
   the point of maximum cost to discover it does not work.
2. **Criterion 2 — secrets still in history.**
3. **Criterion 3 — production credentials still unrotated, in a public repository** (R-01). The
   only remedy is rotation (`M0-04`); purging history cannot retract what is already cloned.

Still-open M0 work carried in: `M0-06` (`Blocked` on Q-25/Q-26), `M0-10` (`Ready`), `M0-11`
(`Ready` — writes `ADR-006` recording the answered Q-01), `M0-01-03` (`Needs Review`).

## Two process notes for M2

- **Check `git branch --no-merged master` before allocating any id.** `M2-A01-01` and `M0-06`
  collided on **six** ids, because `grep`-before-claim only sees merged work and cannot see a
  sibling branch. M2 runs more branches in parallel than M0 did, so this will recur. `M0-06`
  still owes `KB-104 → KB-106` and `INV-035 → INV-038`, and its `KB-104` is cited in an
  `ApplicationDbContext.cs` source comment that must change with it.
- **`master` requires pull requests.** The 2026-08-19 push reported
  `Bypassed rule violations … Changes must be made through a pull request`, and succeeded only
  because the owner holds bypass rights. Prefer a PR. No required status check gates merges yet
  — the open half of **Q-20**.

## Baselines as of this file

| | |
|---|---|
| `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` | **79 passed, 0 failed** |
| `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj --no-incremental` | **0 errors, 6,694 warnings** (baseline 6,695) |
| CI on `master` | **green** |

`CLAUDE.md` still says *"`dotnet test` finds nothing — no test project exists until M0-12-01
creates one."* **That sentence is stale.** The authoritative command table is
[KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands).
