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

## Active task — `M2-C04-01`, `Blocked` after attempt 1, resume rather than restart

**Task file:** [`tasks/M2-C04-01.md`](tasks/M2-C04-01.md) — Design tokens, theme, light/dark.
**Branch:** `migration/M2-C04-01-design-tokens`, tip **`cdb147a`**. **Do not start from a clean
tree or a fresh branch** — attempt 1's full implementation, which passed all sixteen
acceptance criteria on independent validator re-check, already exists there.

### Run State

| Field | Value |
|---|---|
| Status | `Blocked` — attempt 1 of 3 stopped, retry budget not exhausted |
| What happened | Attempt 1 (`cdb147a`) implemented the full token/theme layer and passed all sixteen acceptance criteria on independent validator re-check (contrast recomputed from scratch: 0 failing pairs, both themes; `typecheck`/`lint`/`test`/`build` all green). It still validated `FAIL` — category **regression** — because `npm run coverage`, a verified KB-083 command (`prompt-template.md:366`, "exit 0 … branches 100 %"), now exits 1: `vitest.config.ts:38` still pins `branches: 100` and the ~700 new lines under `shared/theme/**` are only partly branch-covered. The validator's disposition was `retry`, same model, no escalation trigger. The runner then dispatched `migration-debugger` for that retry, and it **returned no result to the orchestrator** — but its process left real, uncommitted edits on disk, matching the `M0-12-01`/`M2-B07` precedent that an empty return does not mean an empty disk |
| Spot-check evidence | `git log --oneline -3` on the branch still shows `cdb147a` as tip (no second **commit** exists), but `git status --porcelain` is **not** clean: `ThemeToggle.tsx` and `theme.test.tsx` carry an uncommitted diff. It replaces `ThemeToggle.tsx`'s index-arithmetic `move(delta)` with a total `RING` lookup keyed by `ColorSchemePreference`, removing exactly two of the branches attempt 1's coverage report named as uncovered in that file; `theme.test.tsx` gains matching imports and a density-attribute reset. It does **not** touch `ThemeProvider.tsx`, `density.ts` or `useColorScheme.ts` — also named uncovered — so it is partial even at face value, and it was **not reviewed, reconciled or validated** this session (no `coverage`/`test`/`lint`/`build` re-run against it). Left as-is, unstaged, deliberately not committed unreviewed and not discarded |
| Not yet done | Reviewing the uncommitted diff against attempt 1's diagnosis; covering the remaining missed branches in `ThemeProvider.tsx`, `density.ts`, `useColorScheme.ts` (or lowering the threshold and correcting the KB-083 row in the same commit); then re-running `npm run coverage` plus all sixteen acceptance criteria and re-validating |
| Next step | Re-dispatch the implementer/debugger on this branch at this tip. It should **review the uncommitted working-tree diff first** — decide whether to build on it or discard it — rather than regenerating from `cdb147a` blind; the committed implementation already passes every acceptance criterion |
| Escalation condition | This is blocked-on-a-task (a resumable retry), not blocked-on-a-human-decision — do not wait for an owner decision to retry. If a **second consecutive** no-result attempt occurs, that repetition is the signal worth escalating to **Vivek** (repository owner), per the `M0-12-01` precedent (`task-tracker.md` footnote ¹²) |
| Full record | `tasks/M2-C04-01.md` § Execution Record (2026-08-19); `failure-log.md` § M2-C04-01 · attempt 1 · validation, and § M2-C04-01 · attempt 2 · dispatch; `runner-state.md`; `task-tracker.md` footnote ²² |

---

## Ready and unclaimed, once `M2-C04-01` closes — eight tasks, plus two M0 carry-overs

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
These are **not** the task to pick up next — `M2-C04-01` above is, since it already has an
open attempt and validated (nearly-passing) work on its branch. Listed here for whoever plans
the *following* task.

| Task | What | Est. | Why you might take it first |
|---|---|---|---|
| **`M2-B01`** | API versioning → `/api/v1` | 1 d | **Newly released.** Cheapest real progress on the backend; every later controller depends on the route shape |
| **`M2-B05`** | Typed `ScreenCodes` constants (R-10) | 2 d | **Newly released.** Feeds `M2-A01-02`'s filter and kills a class of string-typo bug |
| **`M2-B04`** | Decouple `IApprovalService` + 13 `Pages` refs | 1 wk | **Newly released.** The largest single piece of Blazor→service extraction in M2-B |
| **`M2-B12-01`** | INV-012 numbering investigation | 2 d | **Newly released.** Investigation-only; unblocks the document-numbering chain |
| **`M2-A06`** | Exception middleware → `ProblemDetails` | 3–5 d | Unblocks `B02`, `B06`, `B11`. Establishes the error contract every controller relies on |
| **`M2-C10`** | Decimal handling — no float money arithmetic | 2 d | P0 correctness. Blocks `C07`. Wrong-by-default if deferred |
| **`M2-C11`** | Archive the Angular pilot | 0.5 d | P2 housekeeping. Smallest unit of real progress available |
| **`M2-A01-02`** | Implement `[RequireScreen]` / `[RequireRight]` | 3 d | **Read the warning below before opening it** |
| **`M0-01-03`** | Deployment script + rebuild runbook | 1 d | Closes a G0 exception that was deferred on a false premise |
| **`M0-10`** | R-08 compute-one/test-another guards | — | M0 debt carried into M2 |

**`M2-B12`, `M2-C04`, `M2-C05` and `M2-A01` are parent containers** and are never worked
directly — their implementable scope lives entirely in their `-0n` children. `M2-C04-02`,
`M2-C04-03` and `M2-C03` stay `Blocked` on `M2-C04-01` above, not listed as ready.

---

## The rebuild drill was never blocked on hardware

**A SQL Server Express instance has been on this workstation the whole time** — `MSSQL$SQLEXPRESS`
running, carrying `NexGenErpDb_Master` and a 197-table `NexGenErpDb`, reachable with
`Server=.\SQLEXPRESS;Trusted_Connection=True`. Found during `M2-B07`, 2026-08-19.

Three consecutive sessions recorded that no database existed, and
[KB-107](M0-milestone-review.md) made "obtain a disposable SQL Server" its single headline
recommendation on the strength of that. Nothing in the repository points at the instance — both
hosts ship `"MasterDb": ""` and both user-secrets stores still hold
`Database=DoesNotExist_M0-03-01-LocalTest` from `M0-03-01`'s fail-fast test — so each session
**inferred absence from a config default and recorded the inference as fact.**

**The lesson is not about SQL Server.** A negative result needs the same `file:line`-grade
evidence as a positive one; *"I could not find X"* is a claim about the search, not about X.

`M0-01-03` is therefore `Ready` (footnote ²¹). Use a **throwaway database** on this instance —
**not `NexGenErpDb`**, which holds the only provisioned user and its 150 `UserRights` rows.

**And a new blocker for `M0-04`:** the `Tenants` row stores its connection string in plaintext
**with `sa` credentials** — **Q-32**. Rotating that password without answering Q-32 first would
break every tenant row that embeds it. `M0-04` must not be executed before it is answered.


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
   source control alone. ~~Blocked on a disposable SQL Server, not on work.~~ **Corrected 2026-08-19: a SQL Server was here all along — this is blocked on running the drill, which is work. `M0-01-03` is now `Ready`.** Surfaces at **M6**,
   the point of maximum cost to discover it does not work.
2. **Criterion 2 — secrets still in history.**
3. **Criterion 3 — production credentials still unrotated, in a public repository** (R-01). The
   only remedy is rotation (`M0-04`); purging history cannot retract what is already cloned.

Still-open M0 work carried in: `M0-06` (`Blocked` on Q-25/Q-26), `M0-10` (`Ready`), `M0-11`
(`Ready` — writes `ADR-006` recording the answered Q-01), `M0-01-03` (**`Ready`** as of 2026-08-19).

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
| `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` | **84 passed, 0 failed** (79 + 5 from `M2-B07`) |
| `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj --no-incremental` | **0 errors, 6,694 warnings** (baseline 6,695) |
| CI on `master` | **green** |

`CLAUDE.md` still says *"`dotnet test` finds nothing — no test project exists until M0-12-01
creates one."* **That sentence is stale.** The authoritative command table is
[KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands).
