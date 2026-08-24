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

## ▶ `M2-A03` — Automated permission-matrix test harness

**Selected 2026-08-24** (tip `22eb745` on `master`, tree clean), not yet dispatched. This
supersedes the previous "no task is currently selectable" note below, which the repository
owner overtook by merging `migration/M2-A02-currency-authorization`,
`migration/M2-A09-screen-catalogue-phantoms` and `migration/M2-A10-api-rights-seeding` to
`master` (`a7bbc34`, `--no-ff` one at a time, `M2-A09` first) — that note was never rewritten
by the merge commits themselves, which only touched `task-tracker.md` rows/footnotes and
`runner-state.md`'s `Status`/`Current task` lines.

**Full spec:** [`tasks/M2-A03.md`](tasks/M2-A03.md).

### Why this task, not `M2-B03`

`task-tracker.md` rows 113/130 both read `Ready`, released by the same merge:

- **`M2-A03`** — `Testing`, P0, 3 d, `depends_on: [M2-A02]` (`Completed`, merged).
- **`M2-B03`** — `Documentation`, P0, 2 d, `depends_on: [M2-A02, M2-B02]` (both `Completed`,
  merged).

Both cleared the five-part "can actually be done" test (KB-082 § Ready-task selection rule):
no unmet Hard prerequisite, not a `Product Decision`, no unanswered open question gates either
(`open-questions.md` grepped — `Q-71` names `M2-A03` only as a *candidate owner* for a
separate, still-undecided direction-switch, not as something `M2-A03`'s own task file depends
on), no ⛔ banner, no sibling branch on either's `source_files`
(`git branch --no-merged master`: only `M0-03-*`, `M0-04`, `M0-06`, `M0-08`, `M2-A08`,
`M2-B12-01`, `M2-C10` remain, none touching `CurrencyController.cs`, `AuthController.cs`,
`Program.cs`, `ApplicationDbContext.cs`, `RightsHelper.cs`, `UserRight.cs`, `Contracts/`, or
the two ADR files either task names).

Both P0 → rank step 1 ties. Both name exactly one dependent (`M2-A03` → `M5-05`; `M2-B03` →
`M2-B10`) → rank step 2 ties. **Rank step 3 (critical path) breaks the tie**: the documented
critical path (`dependency-graph.md` § *Project critical path*) runs
`... → M2-A01-03 → M2-A02 → M2-A03 → M2-C05-01 → ...` — `M2-B03` does not appear in it.
**`M2-A03` wins.**

### Classification (KB-091 §4)

No explicit `complexity`/`risk` override in `tasks/M2-A03.md` frontmatter. `task_type: Testing`
→ base MEDIUM (§4.1); raised to the HIGH ceiling by `estimate: 3 d` (≥3 d), non-empty
`business_rules: [BR-AUTH-002]`, `source_files` spanning two projects (`V.SMART.Api`,
`V.SMART.Shared`), and the task explicitly extending the authorization surface `M2-A02` built
(§4.2). Risk **HIGH** per §4.3 (`business_rules` populated). Per §5.1 HIGH-complexity routing
plus the §5.2 item 2 risk-HIGH floor: **Investigate `opus`, Implement `opus`, Validate `opus`.**

### Carried forward — still true, not re-derived this pass

- **`M0-06`** (`Ready`) still fails part 5: sibling branch `migration/M0-06-remove-default-admin`
  still exists, unmerged (`git branch --no-merged master`, re-checked 2026-08-24).
- **`M0-11`** (`Ready`) still fails part 2: `task_type: Product Decision`, owner-only, never
  self-selectable.
- Outstanding owner decisions from the prior close-out (`M0-04` credential rotation,
  `M2-C10`'s environment, `Q-38`) are unchanged by this pass — see `task-tracker.md` § Current
  state for detail; none of them gates `M2-A03`.
- `M2-A03`'s own note in `tasks/M2-A03.md` (unverified this pass beyond the frontmatter read
  above): it is the task responsible for proving the authorization matrix **over the wire**
  (`R-43` — no `WebApplicationFactory` host exists yet in `tests/V.SMART.Api.Tests`, so
  `M2-A02`'s 401/403 proof stopped at the policy/`ObjectResult` level). Whoever implements this
  task should expect to need that host, not assume it already exists.
- **`AuthController.cs`'s `AdministratorUserId` const (`= 1`)** is still the whole safety
  property for API-side rights seeding (`M2-A10`, merged) — not this task's surface, but listed
  among `M2-A03`'s `source_files` (`AuthController.cs`) because the harness may exercise the
  login path. Do not generalise or "fix" that gate as part of this task; it is out of scope.

---

## Previous note (superseded by the above)

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

At that point in time no task was selectable — a person-level stall on owner merge review.
The owner has since merged `M2-A02`, `M2-A09` and `M2-A10` (`a7bbc34`), which is exactly the
event this note said would change the answer, and released `M2-A03` and `M2-B03` — see above.

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
  proofs only. `M2-A03` (selected above) is the task named as owning that wire-level proof.
