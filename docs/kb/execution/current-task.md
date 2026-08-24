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

## ▶ Active task: `M2-A09` — Remove the two phantom screen names from `ScreenCatalogue` (R-65)

Full spec: [`tasks/M2-A09.md`](tasks/M2-A09.md). Owner decision already made: **option A**
of [KB-109](../decisions/KB-109-q28-r65-decision-brief.md), 2026-08-24 — delete the two
phantom names, do not generate the catalogue from the database (that is option B, deferred to
`M2-B10`), do not make the validator query the database at startup (option C, rejected).

**Why it's selectable now.** `depends_on: [M2-A01-03]` only, which is `Completed` and merged.
`task_type: Security`, not a `Product Decision`. No open question gates it (grepped
`open-questions.md` for `M2-A09`, no hit). No ⛔ banner; `status: Ready`,
`last_verified: 2026-08-24` in its own frontmatter. `git branch --no-merged master` (checked
2026-08-24, this session) shows no branch touching `ScreenCatalogue.cs`,
`ScreenRightStartupValidator.cs` or `ScreenRightStartupValidatorTests.cs` — the only unmerged
branch that overlaps *anything* in `V.SMART/V.SMART.Api/Authorization/` is none; the sole
overlap risk in that directory is `M2-A02`'s branch, which touches only `Controllers/` and
`tests/`, not `Authorization/`. Ranked ahead of the other newly-`Ready` sibling, `M2-A10`
(Security, P1, same `depends_on`), on priority — `M2-A09` is P0.

**One-line summary of the work.** `ApplicationDbContext.cs` seeds 152 `Screens` rows; two were
later deleted by migration (`ScreenCode` 114/115), so every real database holds 150.
`ScreenCatalogue.cs:146-147` still lists the two phantoms — `"Bill Pending List"` and
`"Bill Paid List"`. `ScreenRightStartupValidator` checks a declared `[RequireScreen(...)]` name
against this catalogue, not against the database, so today it would wave through an annotation
naming a phantom screen and produce a **silent, permanent 403 for every user**, with no boot
warning. Delete the two entries; add a test proving the validator now **rejects** a phantom
name (run it against the pre-fix catalogue too, and say so — a test that passes both ways
proves nothing); confirm at least one real, surviving screen name still passes. Full acceptance
criteria, out-of-scope boundaries and doc-update list are in the task file — do not re-derive
them here.

**Blast radius today is zero** — no endpoint currently carries either phantom annotation. This
converts a latent trap into a loud boot failure, nothing more.

## Carried forward from `M2-A02`'s close-out (2026-08-24)

- `M2-A02` (`CurrencyController` screen-right enforcement) is **implemented and independently
  validated `PASS`**, closed `Needs Review` on `migration/M2-A02-currency-authorization` (tip
  `634d30c`) — **not merged**. `M2-A03` and `M2-B03` stay `Blocked` until it is merged to
  `master`; a `Needs Review` branch does not satisfy a Hard prerequisite. See
  [`tasks/M2-A02.md` § Execution Record (2026-08-24)](tasks/M2-A02.md#execution-record-2026-08-24)
  and `task-tracker.md` footnote ⁵⁹.
- **Q-71** was raised by that close-out (`open-questions.md`): whether some task should now
  switch on `ScreenRightAuthorizationFilter.cs:58-72` /
  `ScreenRightStartupValidator.cs:33-42,83-88`'s dormant "an authenticated action on a
  controller with no `[RequireScreen]` is an error" direction, now that every API endpoint is
  annotated or explicitly exempt. Candidate owner `M2-A03`; decision owner the repository
  owner. Not `M2-A09`'s to act on — `M2-A09` touches only the catalogue's *contents*, not the
  validator's unannotated-controller policy — but whoever next reads
  `ScreenRightStartupValidator.cs` for `M2-A09` will see the same dormant comment and should not
  mistake it for this task's scope.
- `M2-A02`'s executing session independently reconfirmed the R-65 gate text (KB-109) names only
  `"Bill Pending List"` / `"Bill Paid List"` as the phantom entries — the same two names
  `M2-A09` targets. No new phantom was found.

## What a future session should do here

- Read [`tasks/M2-A09.md`](tasks/M2-A09.md) in full before starting — it is short (0.5 d
  estimate) and self-contained.
- After `M2-A09` closes, `M2-A10` (Seed administrator rights on the API login path, Q-28, P1,
  same `depends_on: [M2-A01-03]`) is the other sibling already `Ready` and independent — no
  file overlap with `M2-A09` (`AuthController.cs`-adjacent only) or with `M2-A02`'s unmerged
  branch.
- If the owner merges `migration/M2-A02-currency-authorization` before this task is picked up,
  re-run the five-part test for `M2-A03` and `M2-B03` — that merge is the event that would
  change their answer.
