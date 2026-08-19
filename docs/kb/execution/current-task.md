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

## Active task — `M2-B07`, `Blocked` on the repository owner after attempt 3 of 3

**Task file:** [`tasks/M2-B07.md`](tasks/M2-B07.md) — Shared `AddVSmartDomain()` DI extension.
**Branch:** `migration/M2-B07-add-vsmart-domain`, tip **`5cb1901`**. **Do not start from a
clean tree or a fresh branch, and do not re-dispatch an implementer** — the retry budget (3 of
3) is spent and every mechanical acceptance criterion is already `MET`.

### Run State

| Field | Value |
|---|---|
| Status | `Blocked` — attempt 3 of 3 exhausted; blocked on a **human decision**, not on engineering |
| What happened | Attempts 1–3 landed a 655-line `AddVSmartDomain()` extension in `V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs`, called once from each of the three hosts, preserving the exact 249-registration union (mechanically set-diffed, not eyeballed). Every acceptance criterion in `tasks/M2-B07.md` is `MET` except one: *"the Blazor app starts and three screens from three different modules render without a DI resolution error."* |
| The one open criterion — and this close-out's correction | Attempt 3 concluded no database was provisioned on this workstation and both hosts 500'd for that reason. **That conclusion was wrong.** This close-out session found a SQL Server Express instance with `NexGenErpDb_Master` and a 197-table tenant database already on this workstation; pointing `ConnectionStrings__MasterDb` at it makes `V.SMART.Web` render `/` at `200` with **zero** DI resolution errors (`grep -c "Unable to resolve service"` → 0). The three named module screens `302` to `/access-denied` instead — server-side screen-right authorization (ADR-004/M2-A01-01) correctly refusing an unauthenticated request, identical on `master`. The real gap is a **signed-in interactive Blazor circuit**: the one provisioned ERP user's password is hashed and owner-held, and no session may acquire or reuse a credential |
| Not yet done | Nothing code-related — build, test, `ValidateOnBuild`, and registration-union parity are all `MET` and re-verified. What remains is purely: sign in as the ERP user and open three screens, or waive that check |
| Next step | **Do not re-dispatch.** Wait for Vivek. Either (A) he signs in as the one provisioned user with `ConnectionStrings__MasterDb` → `DESKTOP-FIIBE97\SQLEXPRESS` / `NexGenErpDb_Master` and opens three screens from three different modules (five minutes), or (B) he waives the render half on the recorded evidence (whole-graph `ValidateOnBuild` passing at startup, zero `Unable to resolve service`, branch/`master` parity on every route tried) |
| Escalation condition | Already escalated — this **is** the escalation. Retry budget exhausted (3 of 3); do not spend a fourth attempt without an explicit new instruction |
| Full record | `tasks/M2-B07.md` § Execution Record (2026-08-19) — close-out, attempt 3 of 3; `failure-log.md` § M2-B07 · attempt 3 · 2026-08-19; `runner-state.md`; `task-tracker.md` footnote ²⁰ |

---

## Ready and unclaimed, once `M2-B07` closes — five tasks

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
These are **not** the task to pick up next — `M2-B07` above is, since it already has an
open attempt and unvalidated work on its branch. Listed here for whoever plans the *following*
task.

| Task | What | Est. | Why you might take it first |
|---|---|---|---|
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
  still owes `KB-104 → KB-106` and, per its own branch, `INV-035` and `INV-036` — but `INV-036`
  is **already taken on `master`** (M0-13's testing recipe), so that branch's ids will need
  renumbering at merge regardless. `M2-B07` claimed `INV-039` on 2026-08-19 (`investigation-registry.md`),
  deliberately skipping `INV-035`/`INV-038` to leave `M0-06`'s reserved range alone — the next
  free id after that is `INV-040`. `M0-06`'s `KB-104` is cited in an `ApplicationDbContext.cs`
  source comment that must change with it.
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
