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

## Active task: **`M2-C01`** — Vite + React 19 + TS strict + lint + test + CI

**Selected by the repository owner, 2026-08-19** — the first task of M2 and the first React
code in this repository. Full spec: [`tasks/M2-C01.md`](tasks/M2-C01.md). Type Frontend, P0,
estimate 3 d, Gate G2.

Creates `frontend/nexgen-web/` as a Vite 6 + React 19 + TypeScript `strict` workspace, and
**establishes the canonical frontend commands** (`npm ci`, `typecheck`, `lint`, `test`,
`build`, `e2e`) that every later M2-C task cites.

**Verified available on this workstation, 2026-08-19:** `node v24.19.0`, `npm 11.17.0`.

**Do not touch `frontend/vsmart-erp/`** — that is the archived Angular pilot, and archiving it
is `M2-C11`'s job, not this one.

**One criterion cannot be met from an execution session:** *"`.github/workflows/ci.yml`
contains a `frontend` job … **and it is green on the branch**"*. Green-on-the-branch requires a
push, which `CLAUDE.md` forbids without an explicit in-conversation instruction. Add the job,
verify every command locally, and record that half as `NOT MET` with the reason — the same wall
`M0-12-02` hit, resolved there by an owner decision, not by retrying.

---

## Run State — `BLOCKED`, 2026-08-19

**This is exactly the wall predicted above, now hit and recorded.** `M2-C01` is fully
implemented on `migration/M2-C01-react-app-skeleton` (`4ac7241`, `8fb8e6d`, `d5182f6`) and
14 of 15 acceptance criteria are independently re-verified `MET`. The 15th — CI's `frontend`
job "green on the branch" — is `NOT MET, NOT CHECKABLE`: no GitHub Actions run exists or can
be produced without a push (`git ls-remote --heads origin` does not list this branch; `gh` is
not installed on this workstation), and pushing is forbidden absent an explicit
in-conversation instruction.

**This file still points at `M2-C01` deliberately** — do not select a new active task without
reading this section first. A later session resumes here, it does not restart the
investigation or the scaffold.

**What unblocks it — one owner decision, two options, nothing else will do:**
- **A** — the repository owner authorises publishing `migration/M2-C01-react-app-skeleton`
  (preferably as a PR, per Q-20) and reads the `frontend` job green on a hosted runner.
- **B** — the owner waives the "green on the branch" half for this task and re-homes it,
  exactly as was done for `M0-07` (`d79e1a4`).

Full record: [`tasks/M2-C01.md` § Execution Record
(2026-08-19)](tasks/M2-C01.md#execution-record-2026-08-19),
[`failure-log.md` § M2-C01 · attempt 2](failure-log.md#m2-c01--attempt-2--2026-08-19),
`task-tracker.md` footnote ¹⁸, and [`runner-state.md`](runner-state.md) (Status `BLOCKED`).

**Until this is resolved, do not start any task that depends on `M2-C01`** — `M2-C02`,
`M2-C03`, `M2-C04-*`, `M2-C05-*`, `M2-C10`, `M2-C11` all build on work that is not yet on
`master`. `M2-B07` and `M2-A06` remain genuine, unrelated `Ready` alternatives (see the table
below); `M2-A01-02` is a `Ready` alternative in name only — its own D-5/R-40 contradiction is
still unresolved.

---

**Gate G0 PASSED WITH EXCEPTIONS on 2026-08-19.** Milestone review:
[KB-107](M0-milestone-review.md). M2 — Foundation is open for the first time.

The other three tasks are `Ready` and unclaimed:

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
