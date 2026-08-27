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
last_verified: 2026-08-27
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Selected: `M2-C03` — App shell: header, sidebar, breadcrumbs, ⌘K

Full spec: [`tasks/M2-C03.md`](tasks/M2-C03.md). Not yet dispatched.

**Owner override, 2026-08-27: `M2-C03` explicitly chosen over the ranking below.** This pass's
own five-part test and downstream-unblocking ranking (kept below for the record) put `M2-D01`
ahead of `M2-C03`. Presented with both `Ready` options, the owner picked `M2-C03` directly
("Pick M2-C03") — an explicit, in-conversation instruction that overrides the ranking
heuristic, which exists to pick *a* defensible option absent one, not to bind the owner's own
choice. `M2-C03` independently clears every part of the five-part test on its own merits (see
below); the owner's reason for preferring it over `M2-D01` was not asked for and is not
assumed here. `M2-D01` remains `Ready` and unstarted, its own pre-existing unmerged branch
(`migration/M2-D01-currency-end-to-end`) still unresolved — both carried forward for whoever
picks up that task next.

### How this pass found it, 2026-08-27, master tip `3a30401`

`M2-C02` (the prior selection below) was implemented, merged (`6298732`) and marked
`Completed` this session, both on explicit owner instruction ("Yeah merged", then separately
"Yes complete it"; see [`task-tracker.md`](task-tracker.md) footnotes ¹⁰⁶–¹⁰⁷). That clears
rule 1 of the five-part test for two direct dependents at once:

- **`M2-C03`** — Hard prerequisites `M2-C02`, `M2-C04-01` both `Completed` and merged.
- **`M2-D01`** — all seven Hard prerequisites (`M2-C05-03`, `M2-A02`, `M2-B10`, `M2-C02`,
  `M2-A07`, `M2-A06`, `M2-B01`) `Completed` and merged.

Both clear the rest of the five-part test (neither is `Product Decision`; `open-questions.md`
has no hit for either; neither task file carries a ⛔ banner — `M2-D01`'s was re-specified for
Angular by `M2-C12-05`, `M2-C03`'s by `M2-C12-02`).

**Ranked `M2-D01` ahead of `M2-C03`** on step 2 of the Ready-task-selection rule (most
downstream unblocking): `M2-D01` directly gates `M2-D02-01` (`task-tracker.md:182`) and,
transitively, the rest of the `M2-D02*`/`M2-D03` chain — the actual document-editor/Sales
Order spine this migration is building toward. **`M2-C03` currently gates nothing in any
task's `depends_on`** — grepped `task-tracker.md` for every row naming `M2-C03` as a
dependency and found none, confirming the earlier finding at `task-tracker.md:3286` ("`M2-C03`
was cited as blocking `M2-D01` but appears in no Dependencies table") still holds. `M2-C03` is
not excluded — it stays the next candidate after `M2-D01`, just ranked second, and nothing
currently needs it to unblock.

**One thing to check before dispatching, not yet done by this pass:** `M2-D01` was already
dispatched once before, on `migration/M2-D01-currency-end-to-end`, and stopped `Blocked` on
arrival when its `depends_on` was found incomplete (footnote ⁷⁸). That branch still exists,
unmerged. Before cutting a fresh branch per the "one task, one branch" rule, the next session
picking this up should check `git log migration/M2-D01-currency-end-to-end` for how much real
implementation (if any) that branch holds versus just the investigation that found the
`depends_on` gap — reusing it may be more honest than abandoning real work, but re-cutting
fresh is also legitimate if it never got past investigation. Not resolved here because this
pass's instruction was to close `M2-C02`, not to dispatch the next task.

**Downstream not yet released:** `M2-D02-01` still needs `M2-D01` itself `Completed`, not
`Ready`.

---

## Superseded pointer, retained for lineage — `M2-C02` closed (2026-08-27, master tip `3a30401`)

Full spec: [`tasks/M2-C02.md`](tasks/M2-C02.md). **Superseded — implemented, merged
(`6298732`) and marked `Completed`** on owner instruction, both 2026-08-27. Original selection
narrative below, retained for lineage only:

### How this pass found it, 2026-08-27, master tip `e96e333`

`M2-A04` and `M2-C07` were both merged this session (`72a5758`) and then marked `Completed`
on explicit owner instruction ("looks fine ... move to next task"; see
[`task-tracker.md`](task-tracker.md) footnote ¹⁰⁴). That genuinely clears rule 1 of the
five-part test (`Completed` **and** merged, not merely `Ready`/`Needs Review`) for two direct
dependents at once:

- **`M2-C02`** — Hard prerequisites `M2-C01`, `M2-A04`, `M2-A07` are all now `Completed`.
- **`M2-C08-01`** — Hard prerequisite `M2-C07` is now `Completed`. (`M2-C08` itself, the
  parent, stays `Not Started` — parents are never worked directly, so never `Ready`.)

Both cleared the rest of the five-part test (neither is a `Product Decision`; neither is
blocked on an unanswered `Q-nn`; neither task file carries a ⛔ banner; no sibling branch is
open on either's files). **Ranked `M2-C02` ahead of `M2-C08-01`** on step 2 of the
Ready-task-selection rule (most downstream unblocking): `M2-C02` gates `M2-C03`, `M2-D01`
and transitively the whole `M2-D02*`/`M2-D03` chain, plus it was already named as the real
blocker behind `M2-C05-02`'s own close-out — a materially larger release than `M2-C08-01`,
which only gates its two `M2-C08-02`/`-03` siblings. Also on the critical path (KB-082): the
document-editor/M3-5 Sales Order spine needs the auth slice before it needs the document
shell. `M2-C08-01` remains the next candidate after `M2-C02` closes, not excluded — just
ranked second.

**Downstream not yet released:** `M2-C03` still needs `M2-C02` itself `Completed`, not
`Ready`; same for `M2-D01`. `M2-C08-02`/`-03` need `M2-C08-01` `Completed`, not `Ready`.

---

## Superseded pointer, retained for lineage — no task selected (2026-08-26, master tip `ab9a348`)

**This pointer is stale as of this session and has been corrected.** The prior text below
("Selected: `M2-C05-02`") described a pass at master tip `2281740`. Master has since advanced
through `4a8b61a` (that `M2-C05-02` selection recorded), `c0341db` (dispatch found `M2-C05-02`
itself `Blocked` — its corrected `depends_on` names `M2-C02`, which is `Blocked`, plus two
implementation-time findings: the needed endpoint pair does not exist and no real fixture
capture was possible), `5f23c51` (a select-only pass confirming nothing clears the five-part
test), and `ab9a348` (owner-instructed Q-102 fix: added missing Hard `depends_on` entries to
33 task files; the commit's own message confirms "No task changed status" and every task that
gained a new not-`Completed` dependency was already `Blocked`).

This session re-verified rather than trusted that chain: `git status --porcelain --branch` on
`master` is clean; `task-tracker.md` still shows exactly two `**Ready**` rows, unchanged —
`M0-06` (line 84) and `M0-11` (line 86) — and both still fail the five-part "can actually be
done" test:

1. **`M0-06`** — fails part 5: `git log --oneline master..migration/M0-06-remove-default-admin`
   still shows the branch's unmerged tip `5c9b34c`, closed `Blocked` on `Q-25`/`Q-26`.
2. **`M0-11`** — fails part 2: `task_type: Product Decision` (`Q-01`), owner-only.

No other tracker row reads `Ready`. **`nextTaskId` is empty.** See
[`runner-state.md`](runner-state.md) (KB-093) for the full stop record and the standing human
decisions that would unblock the tree (`M2-C02` unblock, `Q-25`/`Q-26`, `Q-01`, and the
`M0-04`/`M0-06` branch merge-or-reject decisions).

--- prior pointer, superseded, retained for lineage ---

## Selected: `M2-C05-02` — Column preferences + persistence

Full spec: [`tasks/M2-C05-02.md`](tasks/M2-C05-02.md). **Superseded — dispatched and closed
`Blocked`** (`c0341db`; its corrected `depends_on` names `M2-C02`, itself `Blocked`). Do not
re-dispatch until `M2-C02` completes and merges. Original selection narrative below, retained
for lineage only:

### How this pass found it (select-only, this session)

Starting point: `master` tip `2281740`, tree clean (`git status --porcelain` empty,
`git branch --show-current` = `master`). This is two commits past the pointer inherited from
the prior pass (`39a9e11`, which had selected `M2-D01`): `e978c12` recorded that selection, and
`2281740` ("Correct `M2-D01`'s `depends_on` on master — 3 of 7 Hard deps were listed") is on
master.

The inherited `current-task.md` pointer ("`M2-D01`, attempt 0, not yet dispatched") was stale.
`git log --all` shows `M2-D01` was in fact dispatched, on its own branch
(`migration/M2-D01-currency-end-to-end`), and stopped on arrival: its own *Dependencies* table
(`tasks/M2-D01.md:244-250`) declares seven Hard rows, but `depends_on` had listed only three
(`M2-C05-03`, `M2-A02`, `M2-B10`), all `Completed`/merged, which is why the prior pass's
five-part test passed it. The other four — `M2-C02`, `M2-A07`, `M2-A06`, `M2-B01` — were never
checked; three are `Completed` but **`M2-C02` is `Blocked`**, and it supplies
`PermissionService`, `requireScreen()` and `*appHasRight`, verified absent on disk
(`frontend/nexgen-web/src/app/core/auth/`, `core/http/`, `layout/shell/` each hold only a
`.gitkeep`). That branch closed `Blocked` (`99885fe`, footnotes ⁷⁸/⁷⁹) but is **unmerged**, so
master's `task-tracker.md` row for `M2-D01` still read `Ready` until this pass. Master's own
`2281740` independently confirms the same finding by correcting `depends_on` to all seven Hard
rows — with the corrected list, `M2-D01` now fails part 1 of the five-part test
(`M2-C02` not `Completed`), so it is excluded without needing to merge the close-out branch.

Re-ran the five-part test against every row `task-tracker.md` marks `Ready`:

1. **`M0-06`** — fails part 5: sibling branch `migration/M0-06-remove-default-admin` is already
   open (unmerged), and itself closed `Blocked` on `Q-25`/`Q-26` (`5c9b34c`).
2. **`M0-11`** — fails part 2: `Product Decision` (`Q-01`), owner-only.
3. **`M2-D01`** — fails part 1: corrected `depends_on` now names `M2-C02`, which is `Blocked`.
4. **`M2-C05-02`** — clears all five parts:
   1. Sole Hard prerequisite `M2-C05-01` is `Completed` and merged
      (`task-tracker.md:164`; `bf2b4cd` is on master's first-parent line).
   2. `task_type: Frontend`, not `Product Decision`.
   3. Grepped `open-questions.md` for `M2-C05-02` — no hit.
   4. `tasks/M2-C05-02.md` carries no ⛔ banner — re-specified for Angular by `M2-C12-03` on
      2026-08-22, banner removed in that same change.
   5. Checked `git diff --stat master...<branch> -- <column-preference source files>` for
      every currently unmerged branch (`migration/M0-04-credential-rotation-runbook`,
      `migration/M0-06-remove-default-admin`, `migration/M2-B12-01-inv-012-numbering`,
      `migration/M2-B12-02-verify-unique-constraints`, `migration/M2-C06-record-picker-dialog`,
      `migration/M2-C10-decimal-handling`, `migration/M2-D01-currency-end-to-end`,
      `integration/2026-08-25-session-merges`) — no hit in any. `M2-C05-03`, the branch that
      previously conflicted on the same files, is already merged (ancestor of master `HEAD`).

`M2-C05-02` is the only row that clears all five parts this pass and is this pass's selection.

### Classification (KB-091 §4 — task file carries no explicit `complexity`/`risk` override)

- **Base**: `task_type: Frontend` → MEDIUM.
- **Raises** (only one applies):
  - `estimate: 3 d` ≥ 3 d — yes.
  - `depends_on`/reverse count ≥ 3 — no (one dependency, `M2-C05-01`).
  - `business_rules` non-empty — no (`[]`).
  - `source_files` spans 2+ of the four .NET projects — no, all under `V.SMART/V.SMART.Shared/`.
  - touches auth/tenancy/document numbering/calculation logic — no.
  - `risk` HIGH — no.
- **Complexity: HIGH** (MEDIUM + 1 raise).
- **Risk: MEDIUM** (default — not Security/Product Decision, no schema change, no
  secrets/`Program.cs`/`appsettings*`, `business_rules: []`; does not change what a live Blazor
  user observes — `ColumnMenu.razor`/the Blazor path is untouched and reference-only, and both
  existing persistence mechanisms stay byte-compatible per the task file's own framing).
- **Routing** (KB-091 §5.1, complexity HIGH): Investigate, Implement and Validate all route to
  `opus`.

### Safety / human-decision check

Not a safety stop: tree clean, `master` tip verified at `2281740`, branch to be cut fresh from
`master`. Not `requiresHuman`: no DBA/credential/environment need disclosed by
`tasks/M2-C05-02.md`, not a `Product Decision`, no pending architecture decision
(`ADR-007` already governs the stack).

### Carried forward — still true, untouched by this pass

- **`M2-D01`** now correctly excluded on master (`depends_on` fixed by `2281740`); its actual
  root blocker is `M0-04` (credential rotation, owner-only) → `M2-A04` → `M2-C02` → `M2-D01`.
  Do not re-dispatch until `M2-C02` is `Completed` and merged.
- **`M0-04`** (credential rotation runbook) closed `Blocked` on a separate, **unmerged** branch
  (`migration/M0-04-credential-rotation-runbook`) — its own designed terminal state, since no
  human with production access participated. A merge decision, not a selection one.
- **`M0-06`** (fails part 5, unmerged `Blocked` branch) and **`M0-11`** (fails part 2, `Product
  Decision`, owner-only) remain excluded, unchanged from every prior pass.
- **`M2-A03`** — closed **`Completed`** 2026-08-26: the owner added the required status check
  ("Restore, build and gate analyzer warnings", the `ci.yml` `build` job) to `master` branch
  protection in session. See tracker footnote ⁸².
- **`M2-B08`**, **`M2-B12-01`**, **`M2-C10`** stay `Blocked` on environment/escalation-budget
  grounds already recorded — untouched by this pass.
- `Q-71, Q-81, Q-82, Q-83, Q-84, Q-91, Q-92, Q-93, Q-97` and `R-43, R-76, R-77, R-78, R-79`
  are untouched by this pass (`Q-97`, per the `M2-D01` branch's close-out, records the gap that
  the five-part test walks only `depends_on` and not a task file's own narrative Dependencies
  table — worth checking when it lands on master).
