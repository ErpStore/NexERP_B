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
last_verified: 2026-08-24
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## ▶ No task is currently selectable

`M2-A10` (seed administrator rights on the API login path, mirroring Blazor, Q-28) closed
**`Needs Review`** this session (2026-08-24) — independently validated `PASS` on the final of 3
validation-attempt passes, `scopeOk: true`, `failureCategory: none`, 1 escalation across the
run. Branch `migration/M2-A10-api-rights-seeding` (tip `02a4633`), left unmerged, unpushed, for
owner review. `AuthController.Login` now calls `SeedAdministratorRightsAsync(user.UserId)`,
gated on `UserId == 1` exactly as `Login.razor:345-349` is, and continues (logs, does not fail)
if seeding throws. Full API suite 318/318, Shared suite 90/91 (1 pre-existing unrelated skip).
`Login.razor` and `UserRightService.cs` confirmed byte-identical to `master`. Full record:
[`tasks/M2-A10.md`](tasks/M2-A10.md) § Execution Record (2026-08-24),
[`task-tracker.md`](task-tracker.md) footnote ⁶¹, [`runner-state.md`](runner-state.md).

**Re-checked against the five-part "can actually be done" test** ([KB-082 § Ready-task
selection rule](dependency-graph.md#ready-task-selection-rule)) against every `Ready` row left
in `task-tracker.md` after this close-out.

| Task | Ready? | Why it fails the five-part test |
|---|---|---|
| `M0-06` — remove the seeded default Administrator credential | `Ready` | Fails part 5: a sibling branch already exists (`migration/M0-06-remove-default-admin`, confirmed via `git branch --no-merged master` 2026-08-24, this session). |
| `M0-11` — Product decision: silent FIFO under-issue (Q-01) | `Ready` | Fails part 2: `task_type: Product Decision`, owner-only, never self-selectable. |

Nothing else in the tracker carries a `Ready` row. `M2-A02` and `M2-A09` are `Needs Review`
(implemented, independently validated `PASS`, both unmerged) — a `Needs Review` branch does not
satisfy part 1 of the test for anything that depends on it, and neither has a task naming it in
`depends_on` regardless. Everything else is `Blocked`, `In Progress`, `Not Started`, or already
`Completed`. This is a **person-level** stall, not an execution-capacity one — see
`task-tracker.md` § Current state (2026-08-24) for the outstanding owner decisions, in order of
how much each unblocks:

1. **`M0-04`** — rotate the exposed credentials (deferred to end-of-milestone 2026-08-19).
   Unblocks `M2-A04` → `M2-A05` → `M2-C02` → `M2-C03`, and G0 criteria 2/3.
2. **`M2-C10`'s environment** — a reachable DB + credential, or relax its "MEASURED wire
   format" criterion to static analysis. Unblocks `M2-C10`, then `M2-C07`.
3. **Owner review and merge of unmerged `PASS`/`Needs Review` branches.** Several sit ready for
   review; merging any of them may release further `Blocked` tasks. Merging `M2-A02` in
   particular moves `M2-A03` and `M2-B03` from `Blocked` to selectable — the largest single
   release currently sitting on the owner's desk. `M2-A09` and `M2-A10` release no other task
   (nothing names either in `depends_on`) but are still worth merging for their own fixes. See
   `task-tracker.md` § "Unmerged branches still carrying work" for the current list.
4. **Q-38** — what `M2-C11` is *for*, now `M2-C01` has built the workspace it existed to
   adopt. Unblocks `M2-C11`.
5. **`M0-06`'s own branch** (`migration/M0-06-remove-default-admin`) — reviewed and merged (or
   abandoned) would clear the sibling-branch block on that row, but nothing else depends on it.

### What a future session should do here

- **Do not re-run Select** against the same two `Ready` rows without a state change — nothing
  about them has changed since this close-out. Check `git branch --no-merged master` and
  `task-tracker.md` § Current state first; if one of the decisions above has been made,
  re-derive selectability from that, not from this file's stale snapshot.
- **Offer the owner a documentation-only task ahead of an unmet gate** rather than stalling, per
  standing guidance — e.g. re-specifying or investigating something that does not need a
  `Ready` row to proceed, if one exists and is worth doing.
- If the owner merges any unmerged `Needs Review`/`PASS` branch (`M2-A02`, `M2-A09`, `M2-A10`,
  or others), re-run the five-part test — a merge is the only event that changes this file's
  answer. Merging `M2-A02` in particular is the one to watch: it releases `M2-A03` and `M2-B03`
  immediately.

### Carried forward from `M2-A10`'s close-out, for whoever next touches auth or rights seeding

- **`AuthController.cs`'s `AdministratorUserId` const (`= 1`) is the whole safety property for
  API-side rights seeding.** `SyncRightsForUserAsync` writes all four operation rights `true`
  for every screen the user lacks a row for (`UserRightService.cs:66-71`); calling it for any
  user other than `UserId == 1` is a silent privilege escalation. This was option B in KB-109
  and the owner rejected it. Do not generalise the gate, make it configurable, or "fix" it to
  cover zero-rights users generally.
- **`UserId == 1` as the definition of "administrator" is still an unevidenced magic number**
  (flagged by KB-109, deliberately left open by `M2-A10`). If a future task needs to act on
  this, it needs its own open question — `M2-A10` was forbidden from touching it.
- **Documentation drift is a recurring failure mode on this branch's own validation history**:
  a corrected factual claim ("Blazor aborts sign-in on a seeding failure" — false;
  `Login.razor:337` authenticates before seeding runs at `:345-349`, so a throw there only loses
  the post-login navigation) was written in four places and fixed in only two on the first
  attempt. If a similar cross-cutting claim needs stating again, prefer one statement in
  [KB-013](../architecture/auth-and-permissions.md) with the others linking to it, not four
  independent copies.
- R-73 (`docs/kb/risks/technical-debt-register.md`, added on the `M2-A10` branch) records an
  unverified premise — a stale-empty-rights-cache interaction — that no test exercises. It is
  Inferred, not observed; whoever next touches rights-caching should check it rather than
  assume it.
- **R-43** (no `WebApplicationFactory` host in `tests/V.SMART.Api.Tests`) is still open and is
  why neither `M2-A02` nor `M2-A10` can prove their HTTP-level behaviour (401/403/200 over the
  wire, or that seeding actually reaches SQL Server) — both are controller/`ObjectResult`-level
  proofs only. Candidate owner unclear; raise if a task starts needing it.
