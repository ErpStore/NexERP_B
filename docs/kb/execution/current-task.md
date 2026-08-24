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

## ▶ Active task: `M2-A10` — Seed administrator rights on the API login path, mirroring Blazor (Q-28)

Full spec: [`tasks/M2-A10.md`](tasks/M2-A10.md). Owner decision already made: **option A** of
[KB-109](../decisions/KB-109-q28-r65-decision-brief.md), 2026-08-24 — call
`SyncRightsForUserAsync` from `AuthController.Login`, gated on `user.UserId == 1` (exactly
Blazor's condition), as a narrow follow-up. Option B (seed every user) was explicitly rejected
— it would grant delete on 150 screens to a view-only clerk.

**Why it's selectable now.** `depends_on: [M2-A01-03]` only, which is `Completed` and merged.
`task_type: Security`, not a `Product Decision`. No open question gates it — Q-28 is the one
that named this task as its own resolution and is now `ANSWERED` (`open-questions.md`). No ⛔
banner; `status: Ready`, `last_verified: 2026-08-24` in its own frontmatter. `git branch
--no-merged master` (checked 2026-08-24, this session) shows no branch touching
`AuthController.cs`, `UserRightService.cs`, `Login.razor` or
`AuthControllerRightsSeedingTests.cs` — the branches present
(`M0-03-01`/`M0-03-02`/`M0-04`/`M0-06`/`M0-08`, `M2-A02`, `M2-A08`, `M2-A09`, `M2-B12-01`,
`M2-C10`) touch none of them. `M2-A02`'s unmerged branch touches only `Controllers/` (a
different controller) and `tests/`; `M2-A09`'s (just closed this session, unmerged) touches
only `Authorization/`. No file overlap with either.

**One-line summary of the work.** `Login.razor:345-349` calls `SyncRightsForUserAsync` when
`user.UserId == 1`; `AuthController.Login` (`AuthController.cs:39-59`) has no equivalent, so an
administrator who has only ever authenticated through the API holds zero `UserRight` rows and
gets 403 from every annotated endpoint once server-side enforcement is live. Add the same
gated call to `AuthController.Login`. **The `UserId == 1` gate is the whole safety property —
do not generalise it**: `SyncRightsForUserAsync` writes all four operation rights `true`
(`UserRightService.cs:66-71`), so calling it for any other user is a silent privilege
escalation (this was option B, and the owner rejected it). Decide and justify what happens to
the login response if seeding throws (Blazor logs and continues; mirror that or explain why
not). `Login.razor` and `UserRightService.cs` must end up byte-unchanged — this task calls the
existing method, it does not touch it or its caller on the Blazor side. Full acceptance
criteria, scope boundaries and the negative-test requirement (criterion 1: prove the call is
**absent** for a non-`1` user, not just that no rows appear) are in the task file — do not
re-derive them here.

## Carried forward from `M2-A09`'s close-out (2026-08-24)

- `M2-A09` (delete the two phantom screen names from `ScreenCatalogue`, R-65) is **implemented
  and independently validated `PASS`**, closed `Needs Review` on
  `migration/M2-A09-screen-catalogue-phantoms` (tip `c3c595e`) — **not merged**. It releases no
  other task (nothing names it in `depends_on`); its value is the fix itself. See
  [`tasks/M2-A09.md` § Execution Record (2026-08-24)](tasks/M2-A09.md#execution-record-2026-08-24)
  and `task-tracker.md` footnote ⁶⁰.
- `ScreenCatalogue.cs` now holds **150** names, not 152 — if this task's tests need a real
  screen name for a positive case, the surviving 150 are the current source of truth (do not
  reuse `"Bill Pending List"` / `"Bill Paid List"`, now gone).
- `M2-A02` (`CurrencyController` screen-right enforcement) remains `Needs Review`, unmerged, on
  `migration/M2-A02-currency-authorization` (tip `634d30c`). `M2-A03` and `M2-B03` stay
  `Blocked` until it merges. Not relevant to `M2-A10`'s file set, but still the reason those two
  tasks are not yet pickable.
- **Q-71** (open-questions.md, raised by `M2-A02`'s close-out) is still open: whether some task
  should now flip `ScreenRightAuthorizationFilter.cs` / `ScreenRightStartupValidator.cs`'s
  dormant "unannotated controller is an error" direction. Candidate owner `M2-A03`; not this
  task's to act on.

## What a future session should do here

- Read [`tasks/M2-A10.md`](tasks/M2-A10.md) in full before starting — it is short (0.5 d
  estimate) and self-contained.
- Do not touch `Login.razor` or `UserRightService.cs` — criteria 4 and 5 require them
  byte-unchanged. This task calls the existing method from the API controller only.
- Do not generalise the `UserId == 1` gate, make it configurable, or "fix" it to cover
  zero-rights users generally — that is option B, explicitly rejected by the owner in KB-109.
  If the magic-number nature of `UserId == 1` itself needs addressing, raise it as a new open
  question; do not act on it here.
- After `M2-A10` closes, the pool empties again on dependency grounds unless the owner has
  merged `M2-A02`, `M2-A09` or `M2-A10` by then — re-run the five-part test against
  `task-tracker.md`'s `Ready`/`Blocked` rows rather than assume nothing changed. In particular,
  merging `M2-A02` would move `M2-A03` and `M2-B03` from `Blocked` to selectable.
