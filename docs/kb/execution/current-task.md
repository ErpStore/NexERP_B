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

## ⛔ `M2-A03` — Automated permission-matrix test harness — BLOCKED, needs a human

**Not resumable by another execution session as-is.** Implemented and independently validated
this session (2026-08-24) on `migration/M2-A03-permission-matrix-harness` (tip `21dc055`, base
`13ee72a`). Verdict: **`FAIL`, `failureCategory: environment`, `scopeOk: true`**. 17 of 18
acceptance criteria are objectively met. The sole unmet one is GitHub repository configuration,
not a code defect, and **no execution session can fix it** — re-dispatching the implementer or
the validator at any model will reproduce this entry verbatim.

**Full spec:** [`tasks/M2-A03.md`](tasks/M2-A03.md) § Execution Record (2026-08-24) has the
full account. Runner bookkeeping: [`runner-state.md`](runner-state.md) `Status` row and
`selection_note`. Tracker: [`task-tracker.md`](task-tracker.md) row 113, footnote ⁶³.

### Run State — what stopped it

- **Unmet criterion**: `tasks/M2-A03.md`'s acceptance criterion "The harness runs in CI on
  every push and pull request as a **required** job." The "runs on every push/PR" half is true
  and observed (`.github/workflows/ci.yml:56-61,213-219`). The "**required**" half is GitHub
  branch-protection configuration — it has no representation anywhere in this git tree.
- **Why it can't be checked or fixed here**: `gh api repos/ErpStore/NexERP_B/branches/master/protection`
  → `gh: command not found` (no `gh` CLI on this workstation). Even with `gh` present, setting
  branch protection requires push/admin access this session does not have, and the branch
  itself is not on `origin` (`git ls-remote --heads origin` does not list it) — there is no PR
  for a required check to attach to yet.
- **Category is deliberately `environment`, not `acceptance-criterion`** — same class and same
  disposition as the `M0-07` attempt-1 stop (`failure-log.md:305-379`).
- **Attempts used: 1 of 4. Escalations: 0.** Not retried, because a retry cannot change the
  outcome (KB-091 §8 triggers 5 and 7).
- **Everything else this task built is sound**, independently re-verified this session:
  `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` → 0 errors; `dotnet test
  tests/V.SMART.Api.Tests/V.SMART.Api.Tests.csproj` → 470/470 passed; the harness alone
  106/106; the generated matrix 60/60 (10 gated endpoints × 6 rights fixtures — the real
  surface is 6 controllers / 18 actions, 3x the task file's stale premise). No file under
  `V.SMART/` was touched (`git diff --stat 13ee72a..HEAD -- V.SMART/` empty).

### To resume this task, a human needs to do one of

1. **Mark the `build` job (or a job containing it) a required status check on `master`** in
   the GitHub repository settings (or via an authenticated `gh` from a machine that has it),
   then have a session re-verify and close the task `Needs Review`/`Completed`.
2. **Accept the criterion as a standing manual gate**, the way `M0-07`'s equivalent criterion
   was accepted, and close the task on that basis.
3. **Move "required for merge" into a separate owner-owned successor task** and re-scope
   `M2-A03`'s acceptance criteria to what a repository-only session can prove.

**Owner: Vivek** (repository owner; only he may decide among the above or set a task
`Completed`).

### Not to do

- Do not re-dispatch the implementer or validator on `M2-A03` expecting a different result —
  the blocking condition is external and unchanged.
- Do not soften, delete, or silently mark the criterion met.
- Do not merge or push `migration/M2-A03-permission-matrix-harness` — leave it for owner
  review.

### Next dependency-ready candidate (not started — this close-out's scope is M2-A03 only)

`task-tracker.md` row 130, **`M2-B03`** (`Documentation`, P0, `depends_on: [M2-A02, M2-B02]`,
both `Completed` and merged) clears the five-part "can actually be done" test as of this
session: no unmet Hard prerequisite, not a `Product Decision`, no open question gates it, no
⛔ banner, no sibling branch on its `source_files` (`git branch --no-merged master` re-checked
2026-08-24). It was **not started** in this session — only recorded as the honest next
candidate, per this close-out's explicit instruction to record the outcome of `M2-A03` and
start nothing else.

### Carried forward — still true

- **`M0-06`** (`Ready`) still fails part 5: sibling branch `migration/M0-06-remove-default-admin`
  still exists, unmerged.
- **`M0-11`** (`Ready`) still fails part 2: `task_type: Product Decision`, owner-only, never
  self-selectable.
- **Q-71** (open-questions.md) is still open: whether/when to switch the production fail-open
  direction on an unannotated controller (`ScreenRightAuthorizationFilter.cs:69-72`,
  `ScreenRightStartupValidator.cs:83-88`). `M2-A03`'s harness makes the condition a
  build/test-time failure but did not touch `Authorization/**`, so the production gap itself
  is unchanged and remains Q-71's to answer.
- **R-43** (no `WebApplicationFactory` host in `tests/V.SMART.Api.Tests`) is still open. The
  401/403 proofs in this harness (and in `M2-A02`/`M2-A10` before it) stop at the
  policy/`ObjectResult` level, not over the wire.
- **`AuthController.cs`'s `AdministratorUserId` const (`= 1`)** is still the whole safety
  property for API-side rights seeding (`M2-A10`, merged). Do not generalise or "fix" that
  gate incidentally while touching `AuthController.cs` for any reason.
- Outstanding owner decisions unrelated to `M2-A03` (`M0-04` credential rotation, `M2-C10`'s
  environment, `Q-38`) are unchanged — see `task-tracker.md` § Current state.
