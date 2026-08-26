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

## `M2-C06` selected — `RecordPickerDialog`, the `DetailsModal` replacement

**Full spec:** [`tasks/M2-C06.md`](tasks/M2-C06.md). Tracker: `task-tracker.md` row 167,
footnote ⁷⁴. Not yet dispatched — **attempt 0**.

### Why this task, and the correction that put it here

The previous session's own notes described `M2-C05-01` as implemented but **unmerged**. That
was true when written; it is not true now. `git log --first-parent` on this session's `master`
tip (`df1d740`) shows `bf2b4cd` **"Merge M2-C05-01: implement the server-paged DataGrid core"**
on the first-parent line, and all 18 files it delivered are present at `HEAD`
(`frontend/nexgen-web/src/app/shared/components/data-grid/`). No session's bookkeeping ever
caught the merge — the task-tracker row and `M2-C05-01`'s own frontmatter both still read
`Needs Review`. **Corrected in `task-tracker.md` footnote ⁷⁴.**

Three rows named `M2-C05-01` as their sole real blocker: `M2-C05-02`, `M2-C05-03` and `M2-C06`.
All three are now `Ready`. `M2-C06` is **P0**; the other two are **P1** — priority is the first
rank criterion (KB-082 § Ready-task selection rule), so `M2-C06` wins outright, not on a tie.
`M2-C05-02` and `M2-C05-03` are `tiedCandidates` — both genuinely selectable, both Frontend, no
overlap with each other, ranked below `M2-C06` on priority alone.

**One thing the next session must not miss:** `M2-C06` and `M2-C05-03` share two files —
`DetailsModal.razor` and `ExcelExportService.cs` (both are *reference* source, not files either
task edits in Angular, but both name them in `source_files`). Do not dispatch both at once —
finish or branch one before starting the other, per *Same-file conflicts — never parallelise*
in `dependency-graph.md`.

`M2-C07` and `M2-C09` do **not** release from this correction — each still names a second,
genuinely-`Blocked` prerequisite (`M2-C10`, `M2-B08`) untouched by it.

### Classification (KB-091 §4)

`task_type: Frontend` → base **MEDIUM**. One raise applies: `estimate: 1 wk` (≥ 3 d) →
**complexity HIGH**. No other §4.2 raise applies — `depends_on` names only one task and only
zero tasks name `M2-C06` back; `business_rules: []`; `source_files` all sit under
`V.SMART.Shared` (one project, referenced for behaviour parity only — this task edits no
`.razor`, no backend, no `Program.cs`); it does not touch authn/authz/tenancy/numbering/calc
logic. **Risk MEDIUM** (default) — not Security/Product Decision, no schema/secrets/`Program.cs`/
`appsettings*`, `business_rules` empty, and it changes nothing a live Blazor user can observe
(`DetailsModal.razor` itself is explicitly Out of Scope — "the Blazor app must keep working
unchanged"). Per §5.1, complexity HIGH routes **Investigate `opus`, Implement `opus`,
Validate `opus`** regardless of risk (risk MEDIUM does not add a floor beyond what complexity
already selects).

**Not a safety stop:** working tree clean at `master` `df1d740`, branch cuts fresh from there,
no sibling branch touches any of `M2-C06`'s `source_files` (`git branch --no-merged master`
checked this session), not a `Product Decision`, no DBA/credential/environment need — the task
is pure Angular-side work with no server dependency beyond the already-merged `M2-C05-01`
(`DataGrid`) it composes. `requiresHuman`: false.

### What the task is

Build `RecordPickerDialog` (`frontend/nexgen-web/src/app/shared/components/record-picker-dialog/`):
a searchable, **server-paged**, multi-select record picker over an upstream document or master
list, composing `M2-C05-01`'s `DataGrid`, returning selections in the order the user ticked
them (`DetailsModal.razor`'s behaviour 5 — this ordering controls line order in the document
being built downstream, so it is load-bearing, not cosmetic). It replaces
`DetailsModal.razor`, referenced by 33 page files, none of which migrate in this task — they
keep using the Blazor component until their module wave. Full spec, the 13-row behaviour table
with `file:line` evidence, and the 33-call-site survey requirement: `tasks/M2-C06.md`.

### Carried forward — still true, untouched by this correction

- **`M0-04`** (credential rotation runbook) closed `Blocked` on a separate, **unmerged** branch
  (`migration/M0-04-credential-rotation-runbook`) — its own designed terminal state, since no
  human with production access participated. That branch's KB bookkeeping (task-tracker footnote
  ⁷¹ *there*, `runner-state.md` halt record) never reached `master`; this file's account of it
  is limited to what is confirmed on `master` (`task-tracker.md` row still reads `Blocked`⁴
  here). Do not re-dispatch it from this branch's tip without first checking whether that branch
  should be merged — that is a merge decision, not a selection one, and this session made no
  such merge.
- **`M0-06`** (`Ready`) still fails part 5: `migration/M0-06-remove-default-admin` unmerged.
- **`M0-11`** (`Ready`) still fails part 2: `task_type: Product Decision`, owner-only.
- **`M2-A03`** (`Needs Review`) still needs a human to make the CI job a *required* status check
  on `master`. Owner: Vivek.
- **`M2-B08`**, **`M2-B12-01`**, **`M2-C10`** stay `Blocked` on environment/escalation-budget
  grounds already recorded — unaffected by this correction.
- **Q-71, Q-81, Q-82, Q-83, Q-84** and **R-43, R-76 (now resolved on `master`), R-77** are
  untouched by this pass.

### Environment note carried from the last session that measured it

`node` here was **v22.22.2** as of 2026-08-25; Angular CLI 22.1.5 requires
`^22.22.3 || ^24.15.0 || >=26.0.0`. `nvm install 24` (Node v24.19.0 / npm 11.17.0) was used to
run `lint`/`test:ci`/`build`. Re-verify at the start of the next session rather than assuming —
this file records what was true then, not a repository change.
