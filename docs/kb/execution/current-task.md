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
last_verified: 2026-08-25
dependencies: [KB-081, KB-082, KB-088, KB-091, KB-092, KB-093, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## `M2-C05-01` closed `Needs Review` — the `DataGrid` core is built

`M2-C05-01` (server-paged `DataGrid` core) was implemented and closed **`Needs Review`** (not
`Completed` — owner integration required,
[KB-088 § Who may set COMPLETED](workflow.md#who-may-set-completed)) on branch
`claude/unblocked-task-execution-pjyouv`, unmerged.

**Full record:** [`tasks/M2-C05-01.md`](tasks/M2-C05-01.md) § Execution Record (2026-08-25).
Tracker: [`task-tracker.md`](task-tracker.md) row 164, footnote ⁷⁰.

### The selection correction that matters more than the task

**The tracker said `Blocked`. It was wrong, and had been for days.** The row read
`Blocked`⁴⁶ with the note *"real blockers are `M2-C04-02`, `M2-B02`"* — but `M2-B02` reached
`Completed` and merged (`feec964`) on **2026-08-20** and `M2-C04-02` on **2026-08-23**, and
nothing moved the row. The task file's own frontmatter still read `status: Not Started`, which
is the tell.

Three consecutive sessions reported *"nothing is dependency-ready"* from that stale row. The
five-part *"can actually be done"* test was re-run against the **prerequisites themselves**
rather than the status column, and it passes all five.

> **Carry this forward: a `Blocked` row whose named blockers have since merged is a stale row,
> not a blocked task.** Re-derive readiness from the prerequisites. The same check should be run
> over every other `Blocked` row before the next session concludes there is nothing to do — this
> session only re-derived the rows it needed.

### What landed

Eighteen files in `frontend/nexgen-web/src/app/shared/components/data-grid/`, exported through
the `shared/components` barrel (which nothing imports eagerly, so the initial bundle is
unchanged at **571.20 kB raw / 136.72 kB transfer**, byte-identical to the baseline).

- **`DataGridComponent<TRow>`** over PrimeNG `p-table` in `lazy` mode — controlled, not
  self-driving: it renders the state it is given and emits the state the user asked for next.
  No `pSortableColumn`, no `p-paginator`, no PrimeNG filter directive, so PrimeNG never holds a
  second opinion about the page number.
- **`DataGridQueryState`** — page / size / sort / filter as signals, route-bound (the URL *is*
  the state) or detached (M2-C06's dialog must not write the URL). `switchMap`ped requests, the
  previous page retained until the next resolves, `ProblemDetails` exposed untouched.
- **One adapter module knows the wire formats** — and there are two, deliberately different:
  M2-B02's API contract (`pageNumber`, `pageSize`, `sort=-field`) and the browser URL
  (`page`, `size`, `sort=field:dir`).
- **45 tests**, covering all 15 the task file lists, including the `axe` scan on a populated and
  an empty grid in both themes.
- **Seams typed and reachable** for `M2-C05-02` (`columnVisibility`) and `M2-C05-03` (`#empty`,
  `#error`, `#toolbar`), each marked `TODO(<task id>)`.

**The `p-table` measurement was run first, as the task file requires, and it passed:** 10,000
rows → **35 rendered `<tr>`**, **16.7 ms median frame** (60 fps) in headless Chromium. Full
method and table in
[KB-050 § Performance targets](../frontend-new/react-architecture.md#performance-targets),
recorded as **INV-052**. Had it failed, escalation was required rather than a silent fallback.

### Two things a reviewer must read before merging

1. **R-76 — `test:ci` is intermittently red, and this branch makes it fire more often.**
   `feedback/busy-overlay.component.spec.ts` leaves a PrimeNG `BlockUI` mask attached to
   `document.body`; spec files share one jsdom document, so its `role="progressbar"` and
   `role="status"` descendants break later files' global role queries. **Proven pre-existing:**
   this branch's tree was stashed so the tree was exactly `master` at `e9a8e7a`, and five
   consecutive full runs gave two clean and three red. But because which files share a worker
   depends on the file count, adding three spec files raises the hit rate — on the final tree,
   three consecutive runs each had one or two failures, **a different pre-existing test each
   time, never one of this task's**. The frontend CI job is blocking
   (`.github/workflows/ci.yml:311-313`), so expect red that is not this branch's defect. The
   one-line harness fix is written out in
   [KB-060 R-76](../risks/technical-debt-register.md); it was **not** applied here because a
   second task's spec file is not this branch's to edit.
2. **INV-052 — M2-B10 generates the paged envelope per resource, never generically.** OpenAPI
   3.0 has no generics, so a grid generic over `TRow` cannot import `CurrencyVMPagedResult`. The
   adapter declares a structurally identical `DataGridPage<TRow>` instead. A limit of OpenAPI,
   not a defect in M2-B10.

### What this releases, and what it does not

`M2-C05-01` is the highest-fan-out frontend task in M2 — `M2-C05-02`, `M2-C05-03`, `M2-C06`,
`M2-C07` and `M2-C09` all name it as a Hard prerequisite. **None of them is released yet**: it
is `Needs Review`, and a `Needs Review` branch is not a satisfied Hard prerequisite. Merging it
releases `M2-C05-02` and `M2-C05-03` immediately, and `M2-C05-03` in turn is one of `M2-D01`'s
three prerequisites (the other two, `M2-A02` and `M2-B10`, are already `Completed` and merged).

### Carried forward — still true

- **`M0-06`** (`Ready`) still fails part 5: `migration/M0-06-remove-default-admin` exists on
  `origin`, unmerged.
- **`M0-11`** (`Ready`) still fails part 2: `task_type: Product Decision`, owner-only.
- **`M2-A03`** (`Needs Review`) still needs a human to mark the CI job a *required* status check
  on `master`, or to accept the criterion as a standing manual gate. Owner: Vivek.
- **Q-71**, **Q-82**, **R-43**, **M0-04** credential rotation and **Q-38** are untouched by this
  task.
- **Unmerged branches worth a reviewer's attention:** this one, plus
  `migration/M2-A03-permission-matrix-harness`, `migration/M0-06-remove-default-admin`,
  `migration/M0-00-vcs-baseline`, `migration/M0-07-ci-pipeline` and
  `migration/M0-12-01-test-project` (`git ls-remote --heads origin`, 2026-08-25).

### Environment note for the next session on this workstation

`node` here is **v22.22.2**; Angular CLI 22.1.5 requires `^22.22.3 || ^24.15.0 || >=26.0.0` and
refuses to run at all, so `lint`, `test:ci` and `build` fail before doing any work while
`typecheck` (plain `tsc`) passes. `package.json`'s `engines` and `frontend/nexgen-web/.nvmrc`
already say Node 24, so the repository is not wrong. Everything above was run under **Node
v24.19.0 / npm 11.17.0** (`nvm install 24`). No repository file was changed for this.
