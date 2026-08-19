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

## Active task: **none selected yet — M2 is open.** Pick one of four.

**Gate G0 PASSED WITH EXCEPTIONS on 2026-08-19.** Milestone review:
[KB-107](M0-milestone-review.md). M2 — Foundation is open for the first time.

Four tasks are `Ready`, all P0, none needing a human first:

| Task | What | Est. | Why you might take it first |
|---|---|---|---|
| **`M2-B07`** | Shared `AddVSmartDomain()` DI extension | 3 d | **Unblocks the most** — `B01`, `B04`, `B05`, `B08`, `B09` all wait on it. The DI seam everything in M2-B hangs off |
| **`M2-C01`** | Vite + React 19 + TS strict + lint + test + CI | 3 d | Touches nothing the backend touches — can run **in parallel** with `M2-B07`, no same-file conflict |
| **`M2-A06`** | Exception middleware → `ProblemDetails` | 3–5 d | Unblocks `B02`, `B06`, `B11`. Establishes the error contract every controller then relies on |
| **`M2-A01-02`** | Implement `[RequireScreen]` / `[RequireRight]` | 3 d | Spec already written and merged ([KB-105](../architecture/server-side-authorization-spec.md)). **Read the warning below first** |

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).

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
