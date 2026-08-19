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

## Active task: **M0-06** — Remove the seeded default Administrator credential

Full spec: [`tasks/M0-06.md`](tasks/M0-06.md). Type Security, P1, estimate 1 d, Gate G0.

**Run State: `Blocked` on the repository owner — NOT a runner candidate again until Q-26 is
answered.** Attempt 2 of 3, 1 escalation. Validator verdict **`FAIL`**, `scopeOk: true`,
`failureCategory: architecture`. Implemented on `migration/M0-06-remove-default-admin`
(`5b12573`, `4fb8781`) — branch not merged, not pushed.

### What happened

Everything engineering-scoped was completed and independently re-verified: the hard-coded
seed is removed from `ApplicationDbContext.cs` (single hunk), six new tests plus an amended
`DbFixtureTests` assertion are in the suite (`dotnet test` → 85/85 passed), `dotnet build
V.SMART.Api --no-incremental` → 6,694 warnings / 0 errors (under the 6,695 baseline), the
published PBKDF2 hash is confined to 109 pre-existing migration files, `UserRepository.cs`
and the `Screens`/`Restrict`-loop seed are untouched, and a runbook exists
([KB-104](security/default-admin-removal-runbook.md)) with a named owner and a read-only
per-tenant diagnostic. 14 of the task's 16 acceptance criteria are `MET`.

**Acceptance criterion 2 — "no default administrator credential is seeded into a newly
created tenant database" — is `NOT MET`, and cannot be closed from inside a migration.**
`InitialCreate.cs:7562` still inserts `UserId=1` / `"Administrator"` / the published hash,
and it always will: migration history may never be edited (`tasks/M0-06.md`'s own "Files
That Must Not Change" / criterion 11), and replaying migrations is the *only*
tenant-provisioning path this repository supports — nothing in `V.SMART/` calls `Migrate()`,
`MigrateAsync()` or `EnsureCreated()` (Q-02, still `Unknown`). A migration `Up()` cannot tell
a freshly provisioned database from a live tenant, so any DELETE against `Users` either
strikes an existing tenant whose only administrator may be this account (Q-25, `Unknown` —
the task file forbids shipping that) or never fires on a fresh database, leaving the
criterion unmet either way — and it would also **succeed and silently cascade**, since all
three FKs to `Users` are `Cascade`, not `Restrict` as the task file assumed
(`InitialCreate.cs:7196-7200`, `:7232-7236`), destroying `UserRight`/`UserAuthority`/
`UserThemePreference` rows.

The task's own `Dependencies` table names *"a deployment owner"* as an unsatisfied **Hard**
dependency it may not silently resolve on its own authority. This was escalated as **Q-26**
(`open-questions.md`), with three options (A: define tenant provisioning and make the KB-104
runbook step mandatory; B: authorise guarded DML accepting the lock-out risk; C: re-scope
criterion 2 to the model-only property and re-home the replay gap) — none of which an AI
session may choose. Q-25 (is `UserId=1` some tenant's only administrator?) is a separate,
also-`Unknown` prerequisite to B.

**Owner: Vivek** (repository/deployment owner). Full record:
[`tasks/M0-06.md` § Execution Record (2026-08-19)](tasks/M0-06.md#execution-record-2026-08-19);
[`failure-log.md` § M0-06 · attempt 1](failure-log.md#m0-06--attempt-1--2026-08-19) and its
diagnosis entry; [`task-tracker.md`](task-tracker.md) footnote 16;
[`runner-state.md`](runner-state.md) (KB-093).

### Discovered along the way, recorded not acted on

**R-40 (new, High): `UserId == 1` is an undeclared superuser.** `Login.razor:345-349`
auto-grants it all 152 screen rights on every login; no `UserRight` rows are seeded at all;
rights are deny-by-default (`RightsHelper.cs:7-20`). A replacement administrator created with
any other `UserId` authenticates and then sees nothing — a lockout by a route entirely
different from the one this task warned about. Recorded in
[`technical-debt-register.md`](risks/technical-debt-register.md) (KB-060); feeds directly
into Q-26 option B/C.

### What a future session resuming this needs to do

**Do not re-implement M0-06 with the same spec — it will reproduce the identical wall.**
The next action is the owner answering Q-25 and Q-26 (`open-questions.md`), not more
engineering. Once answered:
- If option A or C: close criterion 2 by re-scoping it (C) or by making the KB-104 runbook
  step mandatory and enforced (A), and merge the existing branch.
- If option B: implement the guarded DML the owner accepts, on top of the existing branch.
- Either way, the deferred Option-A runtime bootstrap component (no task id yet — proposed as
  `M0-06-02` in R-09 open item 4) needs to be registered in `task-tracker.md` and given a task
  file once Q-26 decides its shape.

### Not part of M0-06, carried forward in case relevant

An **unreported instance of the compute-one/test-another guard defect** exists at
`MfgPoService.cs:613-615` (`CanSalesOrderItemCancelCheckAsync` — `hasCR` computed, `hasRc`
tested), found by the `M0-09` validator, not fixed, and not `M0-06`'s concern. Recorded under
R-08 in [`technical-debt-register.md`](risks/technical-debt-register.md) (KB-060) and as a
scope note on `INV-025` in [`investigation-registry.md`](investigation-registry.md) (KB-003),
for `M0-10` to pick up once it runs.

## No other task is runner-selectable this run

`M0-10` (audit all `CanDelete…Async` guards, INV-025) names `M0-09` as a Hard prerequisite
and the selection rule requires that prerequisite to be genuinely `Completed`, not
`Needs Review`. It stays `Blocked` until `M0-09`'s branch (`migration/M0-09-delete-guard-fix`,
`8e3b19d`, validated `PASS`) is reviewed and merged. With `M0-06` now closed `Blocked` too,
**no dependency-ready, human-unblocked task remains this run.**

## Most recently closed: `M0-09` — Fix the two unreachable delete guards (R-08)

**`Completed` and merged (`47b2d2e`, 2026-08-19)** on the owner's in-conversation instruction.
Re-verified on `master` after the merge: `dotnet test` **79 passed, 0 failed**;
`dotnet build V.SMART.Api --no-incremental` **0 errors, 6,694 warnings** (baseline 6,695).
Full record:
[`tasks/M0-09.md` § Execution Record (2026-08-19)](tasks/M0-09.md#execution-record-2026-08-19),
[`task-tracker.md`](task-tracker.md) footnote 15.

**Not `Completed`** — awaiting the repository owner's review and merge, same standing
convention as every other `PASS`-validated task this milestone
([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)). **Unblocks nothing
yet**: `M0-10` stays `Blocked` until this branch is merged.

## Other open blockers, unaffected by this change

- **`Needs Review`** — implemented, validated, committed on its own branch, awaiting a
  human review-and-merge/sign-off step that no autonomous session may perform on its own
  authority ([KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed)):
  `M0-01-03`, `M0-09`.
- **`Blocked` on an unscheduled human**, not on any task: `M0-04` (unidentified owner —
  tracker footnote 4), and now `M0-06` (repository owner, Q-25/Q-26 — tracker footnote 16).
- **`Blocked`, transitively:** `M0-11` (Q-01 product decision, released by `M0-13`'s merge
  but not runner-selectable — needs the owner, not a task), `M0-10` (behind `M0-09`'s
  merge), `M0-05` (behind `M0-04`).
- **A parent container**, never worked directly: `M0-01`, `M0-12`.

Full detail on why each is blocked and who the candidate owner is:
[`runner-state.md`](runner-state.md) (KB-093) § *Blocked on* / *Owner to unblock ...* rows,
and [`task-tracker.md`](task-tracker.md) (KB-081) footnotes 1, 4, 13, 15, 16.

> **This does not open M2.** Gate G0 still has zero of seven exit criteria ticked.
> `M0-01-03`'s rebuild drill, `M0-07`'s CI branch-protection criterion, `M0-04`'s credential
> rotation, and now `M0-06`'s Q-25/Q-26 decision remain human-owned. G0 needs all of those
> plus `M0-09`/`M0-10` before M2 can open.
