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

## No task selectable — `M2-B10` closed this session, nothing behind it is ready

`M2-B10` (OpenAPI polish + generated TypeScript client in CI) was implemented and
independently validated **`PASS`** on attempt 1 of 1, `scopeOk: true`, `failureCategory:
none`, 0 escalations, and closed **`Needs Review`** (not `Completed` — owner integration
required, [KB-088 § Who may set COMPLETED](workflow.md#who-may-set-completed)) on
`migration/M2-B10-openapi-typescript-client` (tip `195daf3`, base `master` `c2a9140`).

**Full record:** [`tasks/M2-B10.md`](tasks/M2-B10.md) § Execution Record (2026-08-24).
Tracker: [`task-tracker.md`](task-tracker.md) row 135, footnote ⁶⁷. Runner bookkeeping:
[`runner-state.md`](runner-state.md) `Status` row (session close-out).

### What landed

All three deliverables, at the corrected Angular locations (the task file was written for
the pre-ADR-007 React stack and self-corrected in its own "Execution note" section):

- **OpenAPI polish** — all 18 actions across 6 controllers (Auth, Currency, CurrencyExcel,
  Files, Me, Reference) carry an explicit operation id, a resource tag, and
  `[ProducesResponseType]` for every status they can return, per
  [KB-114 §11](../api/controller-conventions.md). This closes the gap the previous session
  had flagged (`CurrencyController` `GetAll`-only, `AuthController.Login` none).
- **Committed spec** — `api/openapi.json`, produced by one reproducible command
  (`bash tools/generate-api-client.sh`), recorded in
  [KB-083](prompt-template.md#verified-repository-commands).
- **Generated client** — `ng-openapi-gen` 1.0.5, generated into
  `frontend/nexgen-web/src/app/core/api/generated/` (the path `M2-C01` reserved). `decimal?`
  → `number | null`, flagged to `M2-C10`, not resolved silently — recorded in
  [KB-112](../api/generated-client.md) / INV-051.
- **CI job** — `api-contract` added to the single `.github/workflows/ci.yml`, drift-checked
  and proven to fail on a deliberate contract break and on a hand-edited generated file, both
  reverted after observing the failure.

### Next dependency-ready candidate — none

Re-ran the five-part "can actually be done" test
([KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)) against
every row currently `Ready` on the tracker:

- **`M0-06`** — fails part 5: sibling branch `migration/M0-06-remove-default-admin` still
  exists, unmerged.
- **`M0-11`** — fails part 2: `task_type: Product Decision`, owner-only, never
  self-selectable.

No other row reads `Ready`. In particular:

- **`M2-D01`** (Currency end-to-end in Angular) names `M2-B10` as a Hard prerequisite, but
  `M2-B10` is `Needs Review`, not `Completed` and merged — merging it releases `M2-D01`'s
  `M2-B10` half (its other two prerequisites, `M2-C05-03` and `M2-A02`, are also not both
  `Completed`-and-merged yet).
- **`M2-A03`** stays `Blocked` on the named GitHub branch-protection gap (owner: Vivek),
  unaffected by this task.

**Nothing is selectable. This close-out starts no new task**, per its own instruction.

### Carried forward — still true

- **`M0-06`** (`Ready`) still fails part 5: sibling branch `migration/M0-06-remove-default-admin`
  still exists, unmerged.
- **`M0-11`** (`Ready`) still fails part 2: `task_type: Product Decision`, owner-only, never
  self-selectable.
- **`M2-A03`** (`Blocked`) still needs a human to mark the `api-contract`/`build` CI job a
  *required* status check on `master` in GitHub repository settings, or to accept the
  criterion as a standing manual gate, or to re-scope the criterion into a successor task.
  Owner: Vivek.
- **Q-71** (open-questions.md) is still open: whether/when to switch the production fail-open
  direction on an unannotated controller (`ScreenRightAuthorizationFilter.cs:69-72`,
  `ScreenRightStartupValidator.cs:83-88`). Untouched by `M2-B10`.
- **R-43** (no `WebApplicationFactory` host in `tests/V.SMART.Api.Tests`) is still open —
  401/403 proofs across the suite stop at the policy/`ObjectResult` level, not over the wire.
- **`M2-C10`'s decimal wire format is now measured**, not merely reserved: `decimal?` →
  `number | null` per the real committed `api/openapi.json` (INV-051, KB-112). `M2-C10`
  itself remains `Blocked` on its own separate criterion — a reachable DB + credential for a
  live `[Authorize]`d endpoint, or relaxing that criterion to static analysis; an owner
  decision, unaffected by this task.
- Outstanding owner decisions unrelated to `M2-B10` (`M0-04` credential rotation, `Q-38`,
  merging `migration/M2-A02-currency-authorization` and other `Needs Review` branches) are
  unchanged — see `task-tracker.md` § Current state.
- **Unmerged branches worth a reviewer's attention, none to be merged by a session:**
  `migration/M2-B10-openapi-typescript-client` (this task, `PASS`, `Needs Review`),
  `migration/M2-A02-currency-authorization` (`PASS`, `Needs Review`),
  `migration/M2-A03-permission-matrix-harness` (`FAIL`/`environment`, `Blocked` on a human),
  `migration/M2-A09-screen-catalogue-phantoms` (`PASS`, `Needs Review`),
  `migration/M2-A10-api-rights-seeding` (`PASS`, `Needs Review`), `migration/M0-06-remove-default-admin`
  (unknown validation state — the reason `M0-06` is excluded from selection).
