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

## No task active — `M2-C05-01` closed `Completed`, and three are now `Ready`

`M2-C05-01` (server-paged `DataGrid` core) is **`Completed`**: merged to `master` as `bf2b4cd`
via [PR #2](https://github.com/ErpStore/NexERP_B/pull/2) with all CI green, and **signed off by
the repository owner on 2026-08-25** — the authority
[KB-088 § Who may set COMPLETED](workflow.md#who-may-set-completed) requires.

**Full record:** [`tasks/M2-C05-01.md`](tasks/M2-C05-01.md) § Execution Record.
Tracker: row 164, footnotes ⁷⁰ and ⁷⁴.

### What the sign-off released — the point of the task

Three rows moved `Blocked` → `Ready`, because `M2-C05-01` was their **only** prerequisite. Each
was re-checked against all five parts of the *"can actually be done"* test, not assumed:

| Ready | Type | Priority | Why it is worth picking |
|---|---|---|---|
| [`M2-C05-03`](tasks/M2-C05-03.md) — states + export | Frontend | P1, 2 d | **The unblocker.** It is `M2-D01`'s last outstanding blocker, and `M2-D01` is G2 criterion 1's path. Its `#empty` / `#error` / `#toolbar` seams are already typed in the merged component. |
| [`M2-C06`](tasks/M2-C06.md) — `RecordPickerDialog` | Frontend | P0, 1 wk | Highest priority of the three. Consumes the grid's **detached** query mode, which exists precisely so a dialog does not write the page's URL. |
| [`M2-C05-02`](tasks/M2-C05-02.md) — column preferences | Frontend | P1, 3 d | The `columnVisibility` seam is typed and honoured, marked `TODO(M2-C05-02)`. |

**Recommendation, not a decision:** `M2-C05-03` first. It is the shortest (2 d), its seams are
already in place, and it is the only one of the three that unblocks anything downstream —
finishing it leaves `M2-D01` (Currency end-to-end) `Ready`, which is the first ERP screen and
the thing G2 criterion 1 actually measures.

Three further rows lost `M2-C05-01` as a blocker but keep a second one: `M2-C07` waits only on
`M2-C10`; `M2-C09` only on `M2-B08`; `M2-D01` only on `M2-C05-03`.

### The session's other outcome — CI is trustworthy again

**R-76 is resolved** (register entry `~~R-76~~`). It was three defects, not one: the overlay
leak between spec files, a racy assertion, and **two real PrimeNG defects in
`ComboboxComponent`** that the flake had been pointing at — a late search response reopening a
dismissed panel, and an overlay enter-confirmation resurrecting one (recorded as **R-77**, the
upstream defect, contained locally and watched on PrimeNG upgrades).

Both guards were **proven by removal** before being trusted. `npm run test:ci` went from *two
clean runs in five* to **six consecutive 356/356**. Until this landed, a red CI could not be read
as a regression signal; it now can.

### Blocked on the owner, unchanged

- **Every `Backend`, `Security` and `Database` task is unrunnable in this environment.**
  `global.json` pins .NET SDK `10.0.400`; only `10.0.111` is obtainable, and the 4xx binaries
  come solely from `builds.dotnet.microsoft.com`, which the network policy denies at CONNECT.
  Three options for the owner in [`failure-log.md` § M2-B08 · attempt 1](failure-log.md).
  This is why `M2-B08` is `Blocked` (footnote ⁷³) and why `M2-C09` behind it cannot move.
- **`M0-06`** (`Ready`) still fails part 5 — `migration/M0-06-remove-default-admin` is unmerged.
- **`M0-11`** (`Ready`) is a `Product Decision`, owner-only.
- **`M2-A03`** needs the CI job marked a *required* status check in GitHub branch protection —
  the one thing keeping G2 criterion 3 at half-met. Owner: Vivek.
- **`M2-C10`** (blocking `M2-C07`) needs an owner decision on its criterion: a live
  `[Authorize]`d endpoint, or relax it to static analysis.
- **Q-38**, **Q-71**, **Q-82**, **Q-83**, **R-43** and `M0-04` credential rotation are unchanged.

### Environment note for the next session on this workstation

`node` here is **v22.22.2**; Angular CLI 22.1.5 requires `^22.22.3 || ^24.15.0 || >=26.0.0` and
refuses to run at all, so `lint`, `test:ci` and `build` fail before doing any work while
`typecheck` (plain `tsc`) passes. `package.json`'s `engines` and `frontend/nexgen-web/.nvmrc`
already say Node 24, so the repository is not wrong. Everything this session ran used **Node
v24.19.0 / npm 11.17.0** (`nvm install 24`).
