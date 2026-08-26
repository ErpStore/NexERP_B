---
doc_id: KB-089
title: Current Task
module: execution
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: [BR-SO-001]
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

## `M2-C05-03` selected — Empty / loading / error states + export

**Full spec:** [`tasks/M2-C05-03.md`](tasks/M2-C05-03.md). Tracker: `task-tracker.md` row 166,
footnote ⁷⁴ (release) and ⁷⁵ (the `M2-C06` close-out that selected it). Not yet dispatched —
**attempt 0**.

### Why this task

`M2-C06` (`RecordPickerDialog`) closed `Needs Review` this session, independently validated
`PASS` (attempt 2 of 5, `scopeOk: true`, `failureCategory: none`). It releases nothing further
— no task names `M2-C06` as a Hard prerequisite. The pool that opened when `M2-C05-01` was
found to be already merged (2026-08-26, `task-tracker.md` footnote ⁷⁴) still has two members:
`M2-C05-02` and `M2-C05-03`, both `Ready`, both P1, both `Frontend`, no file overlap between
them.

**Priority is tied (P1/P1), so rank step 2 (most downstream unblocking) decides:**
`M2-D01` names `M2-C05-03` as one of three Hard prerequisites
(`depends_on: [M2-C05-03, M2-A02, M2-B10]`, `task-tracker.md` line 179) — `M2-C05-03` sits on
the critical path `M2-C05-01 → M2-C05-03 → M2-D01 → M2-D02… → G2`
(`dependency-graph.md:212`). Nothing in the tracker names `M2-C05-02` as a `depends_on` at all;
its value is column-preference persistence, not a release. **`M2-C05-03` wins on rank.**
`M2-C05-02` is a genuinely selectable `tiedCandidate`, not disqualified.

**Five-part "can actually be done" test, re-verified this session:**
1. Sole Hard prerequisite `M2-C05-01` is `Completed` **and merged** (`bf2b4cd` on `master`'s
   first-parent line, confirmed 2026-08-26).
2. `task_type: Frontend`, not `Product Decision`.
3. `open-questions.md` grepped for `M2-C05-03` — no hit.
4. No ⛔ banner — `M2-C12-03` re-specified it for Angular on 2026-08-22 and removed the banner
   in the same change.
5. `git branch --no-merged master` (re-run this session) touches none of its `source_files`.
   `M2-C05-03` and `M2-C06` name two files in common in their frontmatter —
   `V.SMART/V.SMART.Shared/Components/DetailsModal.razor` and
   `V.SMART/V.SMART.Shared/Services/ExcelExportService.cs` — but both are **read-only reference
   citations** in each task's `source_files`, not files either task edits: `M2-C06`'s own
   branch diff touches nothing under `V.SMART/` (verified by its independent validator). No
   real conflict.

### What the task is

Complete `DataGrid` (M2-C05-01) with its five non-happy-path states — loading (first),
loading (refetch), empty (no data), empty (filtered), and error, including a 403
permission-denied inline state and a 409 business-rule message rendered **verbatim** — and
wire **server-side** list export (a toolbar action that fetches bytes from a server endpoint;
the client never builds a spreadsheet, CSV or PDF, per
[ADR-005](../decisions/ADR-005-reporting-and-printing.md)). Composes the M2-C04-03 feedback
primitives; does not re-create them. `BR-SO-001` is cited, not implemented client-side — see
the task file's *Business Rules* section before writing any code. Full spec, the five-state
table and the export contract: `tasks/M2-C05-03.md`.

### Carried forward from the `M2-C06` close-out — still true, untouched by this session

- **`M2-C05-02`** (`Ready`, P1) is the `tiedCandidate` — selectable and independent, just
  ranked below `M2-C05-03`. Available if `M2-C05-03` turns out blocked for a reason this
  session did not find.
- **`M0-04`** (credential rotation runbook) stays `Blocked` on its own separate, **unmerged**
  branch (`migration/M0-04-credential-rotation-runbook`) — its own designed terminal state, no
  human with production access has participated. Do not re-dispatch from that branch's tip
  without a merge decision, which no session has made.
- **`M0-06`** (`Ready`) still fails part 5: `migration/M0-06-remove-default-admin` unmerged.
- **`M0-11`** (`Ready`) still fails part 2: `task_type: Product Decision`, owner-only.
- **`M2-A03`** (`Needs Review`) still needs a human to make the CI job a *required* status
  check on `master`. Owner: Vivek.
- **`M2-B08`**, **`M2-B12-01`**, **`M2-C10`** stay `Blocked` on environment/escalation-budget
  grounds already recorded.
- **`M2-C07`/`M2-C09` do not release** — each still names a second, genuinely-`Blocked`
  prerequisite (`M2-C10`, `M2-B08`).
- **New from `M2-C06`'s investigation (INV-054), relevant to this task:** the ~75 candidate-set
  service methods behind the 33 legacy `DetailsModal` call sites are already server-side but
  none pages, sorts, searches, or is exposed by `V.SMART.Api` — a finding for the M3-5
  estimate, not this task. **Q-91** (search-parameter wire name), **Q-92** (should server
  search match hidden columns), **Q-93** (any call site functionally single-pick) were raised
  by `M2-C06` and remain open; none gates `M2-C05-03`. **R-78** (`DataGrid` needs a
  `disabledRowIds`/`getCellState` capability) is a change request against `M2-C05-01`, not
  this task, but is worth reading before touching the grid's template slots — it documents
  exactly which hooks `DataGrid` is currently missing. **R-70** now has three duplicated jsdom
  fixture copies (`form/`, `overlay/`, and `M2-C06`'s own `record-picker-dialog/`) — three
  is the point at which promoting one to `angular.json`'s `setupFiles` should become a task of
  its own; not this one to fix.
- **Q-71, Q-81, Q-82, Q-83, Q-84** and **R-43, R-77** are untouched by this session.

### Environment note carried from the last session that measured it

`node` was **v22.22.2** as of 2026-08-25 on this workstation; Angular CLI 22.1.5 requires
`^22.22.3 || ^24.15.0 || >=26.0.0`. `nvm install 24` (Node v24.19.0 / npm 11.17.0) was used for
`lint`/`test:ci`/`build` in the `M2-C06` session and worked cleanly. Re-verify at the start of
the next session rather than assuming.
