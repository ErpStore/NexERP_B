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
last_verified: 2026-08-26
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Selected: `M2-D01` — Currency end-to-end in Angular

Full spec: [`tasks/M2-D01.md`](tasks/M2-D01.md). Attempt 0 — **not yet dispatched.**

### How this pass found it (select-only, this session)

Starting point: `master` tip `39a9e11` (tree clean, `git status --porcelain` empty), one commit
past the state the inherited pointer described. `39a9e11` is Vivek's owner-instructed
`--no-ff` merge of `migration/M2-C05-03-grid-states-and-export` — confirmed with `git log -1
39a9e11` (author `kumarag595@outlook.com`, message names the verified merged-result checks).

The inherited note (footnote ⁷⁶, `task-tracker.md`) checked what that merge released and found
only `M2-C05-02` (the same-file conflict with `M2-C05-03`'s branch was gone). It did not check
`M2-D01`, whose `depends_on: [M2-C05-03, M2-A02, M2-B10]` also names `M2-C05-03`. Re-running the
five-part test on `M2-D01` directly:

1. **All three Hard prerequisites `Completed` and merged** — `M2-C05-03` (`39a9e11`, this
   session's own verification above), `M2-A02` (`task-tracker.md:112`, merged 2026-08-24 on
   owner instruction), `M2-B10` (`task-tracker.md:135`, merged 2026-08-25 on owner instruction).
2. **Not a `Product Decision`** — `task_type: Frontend` (`tasks/M2-D01.md` frontmatter).
3. **Not gated by an open question** — grepped `open-questions.md` for `M2-D01`, no hit.
4. **No ⛔ banner** — `tasks/M2-D01.md` was re-specified for Angular by `M2-C12-05` on
   2026-08-22; the banner that stood there was removed in that same change.
5. **No sibling branch touches its files** — checked `git diff --stat master...<branch>` for
   every currently unmerged branch (`migration/M0-04-credential-rotation-runbook`,
   `migration/M0-06-remove-default-admin`, `migration/M2-B12-01-inv-012-numbering`,
   `migration/M2-B12-02-verify-unique-constraints`, `migration/M2-C06-record-picker-dialog`,
   `migration/M2-C10-decimal-handling`, `integration/2026-08-25-session-merges`) against
   `Currency`/currency — no hit in any.

`M2-D01` clears all five parts. `M2-C05-02` also clears all five parts now (footnote ⁷⁷'s
sibling correction), but ranks below `M2-D01` on rank step 1 (P0 beats P1) — `M2-C05-02` is
this pass's `tiedCandidates`-adjacent runner-up, not a tie (priority alone settles it, no tie to
report per rank step 4 of `dependency-graph.md`).

`M0-06` (fails part 5, closed `Blocked` on an unmerged branch) and `M0-11` (fails part 2,
`Product Decision`) remain excluded for the same reasons as every prior pass.

Corrected `task-tracker.md`: `M2-D01` row `Blocked` → `Ready` (footnote ⁷⁷, new); `M2-C05-02`
row's rationale updated to name the real remaining ranking reason rather than repeat the
now-stale "sole blocker `M2-C05-01`" phrasing (footnote ⁷⁴ retained, ⁷⁷ appended).

### Classification (KB-091 §4 — task file carries no explicit `complexity`/`risk` override)

- **Base**: `task_type: Frontend` → MEDIUM.
- **Raises** (need only one to reach HIGH, this task clears three):
  - `estimate: 3 d` ≥ 3 d.
  - `depends_on` names 3 tasks (`M2-C05-03`, `M2-A02`, `M2-B10`).
  - `source_files` spans two of the four .NET projects (`V.SMART.Api/Controllers/
    CurrencyController.cs`; `V.SMART.Shared/...` ViewModels, BusinessLayer, Data, Pages).
- **Complexity: HIGH** (MEDIUM + 2+ raises caps at HIGH).
- **Risk**: not Security/Product Decision; no database-schema change authorised or needed
  beyond the existing `Currency` table; no secrets/`Program.cs`/`appsettings*`;
  `business_rules: []`; the task adds an Angular equivalent and does not change what a live
  Blazor user observes (`CurrencyList.razor`/`CurrencyUpsert.razor` are reference-only, left
  running) → **Risk: MEDIUM** (default).
- **Routing** (KB-091 §5.1, complexity HIGH): Investigate, Implement and Validate all route to
  `opus`. Risk MEDIUM does not add a further floor beyond what HIGH complexity already selects
  (§5.2 item 2 only forces `opus` at risk HIGH, which this task isn't, but HIGH complexity
  already puts Validate on `opus` regardless).

### Safety / human-decision check

Not a safety stop: tree clean, `master` tip verified, no dirty working tree, branch to be cut
fresh from `master` at `39a9e11`. Not `requiresHuman`: no DBA/credential/environment need
disclosed by `tasks/M2-D01.md`, not a `Product Decision`, no architecture decision pending
(`ADR-007` already governs the stack).

### Carried forward — still true, untouched by this pass

- **`M2-C05-02`** is genuinely `Ready` and dependency-ready (see above) — the natural next pick
  once `M2-D01` closes, unless something else outranks it by then.
- **`M2-C06`** (`Needs Review`, `migration/M2-C06-record-picker-dialog`, tip `a47d016`) releases
  nothing and no task names it as a Hard prerequisite — reviewable at leisure, not urgent to
  merge.
- **`M0-04`** (credential rotation runbook) closed `Blocked` on a separate, **unmerged** branch
  (`migration/M0-04-credential-rotation-runbook`) — its own designed terminal state, since no
  human with production access participated. Do not re-dispatch it without first checking
  whether that branch should be merged — a merge decision, not a selection one.
- **`M0-06`** (fails part 5, unmerged `Blocked` branch) and **`M0-11`** (fails part 2, `Product
  Decision`, owner-only) remain excluded, unchanged from every prior pass.
- **`M2-A03`** (`Needs Review`) still needs a human to make the CI job a *required* status check
  on `master`. Owner: Vivek.
- **`M2-B08`**, **`M2-B12-01`**, **`M2-C10`** stay `Blocked` on environment/escalation-budget
  grounds already recorded — untouched by this pass.
- **Q-71, Q-81, Q-82, Q-83, Q-84, Q-91, Q-92, Q-93** and **R-43, R-76, R-77, R-78** are
  untouched by this pass.
