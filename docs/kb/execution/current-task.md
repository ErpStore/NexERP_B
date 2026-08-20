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
last_verified: 2026-08-20
dependencies: [KB-081, KB-082, KB-088, KB-060]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task — `M2-B12-01` — INV-012 document-numbering + financial-year investigation

**Task file:** [`tasks/M2-B12-01.md`](tasks/M2-B12-01.md).

**Status:** `Ready`. Not yet started — no branch exists yet.

### Why this task, now

`M2-A01-02` (implement `[RequireScreen]`/`[RequireRight]`) closed this session validated
`PASS` and moved to `Needs Review` — not `Completed`, so per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) it did **not** release its
only Hard-dependent, `M2-A01-03` (per-request rights caching), which stays `Blocked`.

Applying the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the genuinely `Ready` P0 candidates remaining once `M2-A01-02` is removed from the
pool — `M0-01-03`, `M2-B04`, `M2-B12-01`, `M2-C04-02`, `M2-C04-03`, `M2-C10`:

- **Step 1 (P0/P1/P2)** is a tie — all six are P0.
- **Step 2 (most downstream unblocking that actually fires)** decides it outright.
  `M2-B12-01`'s only dependent, `M2-B12-02`, names *only* `M2-B12-01` in `depends_on` —
  finishing this task makes a real task `Ready`. No other candidate clears that bar:
  `M2-C04-02`'s two dependents (`M2-C05`, `M2-C05-01`) also need `M2-B02`, which is
  `Completed` and merged now but still requires `M2-C04-02` itself, so they stay blocked on
  `M2-C04-02` regardless; `M2-C10`'s dependent `M2-C07` also needs `M2-C05-01`, nowhere near
  `Ready`; `M0-01-03`, `M2-B04`, `M2-C04-03` have zero dependents in the tracker at all.
  `M2-B12-01` wins at step 2 — no further tie-break step is needed. (Last time this file was
  written, `M2-B12-01` tied with `M2-A01-02` at step 2 and lost the step-3 critical-path
  tie-break; with `M2-A01-02` now closed, `M2-B12-01` is the unique step-2 winner.)

### What this task does

**Documentation only — no C# file changes.** Produces
`docs/kb/modules/document-numbering.md` (**TO BE CREATED**, `doc_id: KB-100`): a complete,
`file:line`-evidenced inventory of how V.SMART allocates document numbers (36 repository
files, 38 raw-SQL numbering sites, the `CommonService.GenerateAutoRunningNoAsync` allocator,
7 lock-free LINQ sites, 4 allocation-table read-modify-write methods) and a format catalogue
of every document series' user-visible shape plus its financial-year rule. Corrects R-12
(KB-060) — re-verification already found two of its four factual claims wrong (see the task
file's own table) — and closes INV-012. **No database access is required or expected**; that
is `M2-B12-02`'s job. Full detail, acceptance criteria and the fresh-session execution prompt:
[`tasks/M2-B12-01.md`](tasks/M2-B12-01.md).

### Read before starting

The task file's own *Required Existing Knowledge* / source-file list is authoritative. Key
points already established, so as not to re-derive them:

- **R-12's own register text is wrong on two of four claims** — do not trust it verbatim.
  `37/38` raw-SQL numbering statements *do* carry `WITH (UPDLOCK, ROWLOCK)`, and there is no
  `~20` repositories, there are 36. `UPDLOCK`/`ROWLOCK` is close to decorative here (row lock,
  not range lock; released outside an explicit transaction) — say so in plain words in the
  new document so `M2-B12-03`'s reviewer doesn't assume the code is already protected.
- **R-12 must stay `Inferred (high confidence)` at the end of this task** — reading code
  proves a race is *possible*, not that one has occurred. Only `M2-B12-02`'s duplicate census
  can upgrade or downgrade the classification. Do not do either here.
- **Do not run INV-015** (e-Invoice/e-Way payload construction) even though document numbers
  feed into it — record the coupling as a question, do not investigate `E_Invoice/**`. That is
  scope creep against a Phase-4.5 concern.
- `M2-B07` (Hard, tree-level prerequisite) is `Completed` — confirmed in the tracker. It
  rewrites only the three `Program.cs` files, not `Repository/**` or `BusinessLayer/**`, so
  the sequencing risk the task file names is small but the task still declares it.

### Do not

Start `M2-B12-02` (the live-DB duplicate census) or `M2-B12-03` (race-safe allocation) in this
session. Do not investigate `E_Invoice/**`/INV-015. Do not upgrade or downgrade R-12's
confidence rating — that is `M2-B12-02`'s call.

---

## Carried forward from `M2-A01-02`'s close-out

- **`M2-A01-02` is `Completed` and merged** (`ed559ad`, 2026-08-20) — implemented on
  `migration/M2-A01-02-require-screen-right` (`9a6b3c2`), validated `PASS` on attempt 1 of 3,
  0 escalations. **The D-5/R-40 contradiction was verified as genuinely not-hit at review**, not
  taken on report: `grep` of `V.SMART.Api/Authorization/` for `UserId == 1` / `IsAdmin` /
  `Administrator` / `bypass` / `.Role` returns **zero matches**, and KB-105's D-5 still reads
  *"No `Administrator` bypass. None. Anywhere."* verbatim — the spec was extended, not softened.
  Full record:
  [`tasks/M2-A01-02.md` § Execution Record (2026-08-20)](tasks/M2-A01-02.md#execution-record-2026-08-20)
  and `task-tracker.md` footnote ²⁵. **`M2-A01-03` is now `Ready`.** `M2-A02`,
  `M2-A03`, `M2-A04`, `M2-A07` and `M2-A08` remain `Blocked` behind it.
- **`V.SMART/V.SMART.Api/Authorization/` now exists**, all ten types KB-105 §2 specifies, with
  `Right`, `[RequireScreen]`, `[RequireRight]`, `[NoScreenRight]`, `IUserRightsProvider` (no
  cache), `ScreenRightAuthorizationFilter`, `ScreenRightSet`, `ScreenCatalogue`, and
  `ScreenRightStartupValidator`, registered in `Program.cs`. **No controller is annotated** —
  `M2-A02`'s job. R-03 (KB-060) stays open with that noted.
- **⚠ THE FILTER IS OPT-IN, NOT DENY-BY-DEFAULT — `M2-A02` must close this.** `D-4` is only
  partly implemented: an authenticated action on a controller carrying **no** `[RequireScreen]`
  at all is **allowed through**, at request time and at startup. The reasoning is sound —
  enforcing it now would have made the host refuse to start over `CurrencyController`'s five
  unannotated endpoints, which this task was forbidden to change — and the *half*-annotated
  directions (T-11, T-12) **are** enforced, as is D-6's catalogue check. But the gap is the
  opposite of what "deny by default" implies, so it is stated here rather than left in a
  footnote: **today, an unannotated controller is unprotected.** Tracked against R-03 (KB-060);
  `M2-A02` closes it in the same change that annotates the first controller.
- **A latent, deployment-conditional DI-eagerness finding**, not a regression today, recorded
  for `M2-A02` to watch: the globally registered filter constructs `IUserRightsProvider` (and
  therefore the tenant `DbContext`) via DI on every request reaching MVC's pipeline, even on
  unannotated actions. See `KB-060` R-03 close-out addendum and `task-tracker.md` footnote
  ²⁵ for the detail and why it is safe today.
- **Q-27** (duplicate `(UserId, ScreenId)` `UserRight` rows in a live tenant database) remains
  **Unknown** — `INV-037`'s amendment confirms the 152-screen catalogue matches exactly, but
  the duplicate-row question was not queried in this session's reachable dev tenant.
- **R-40 / D-5 contradiction** (`UserId == 1` auto-granted all 152 rights by
  `Login.razor:345-349`, vs. KB-105 D-5 "no Administrator bypass") was **not hit** by
  `M2-A01-02` — the filter correctly denies a `UserId == 1` caller with zero `UserRight` rows
  (`T13` in the filter's test suite), because the bypass lives in the Blazor login path, not
  in `RightsHelper`/the new filter. Still unresolved for `M2-A02`: an API-only administrator
  will hold zero rows unless `Q-28` (login never calls `SyncRightsForUserAsync` on the API
  path) is settled first. Both remain open questions for `M2-A02`, not this task.

## Ready and unclaimed once `M2-B12-01` closes

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
Listed for whoever plans the task after this one — not to be started now.

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-B12-02` | Verify unique constraints / duplicate numbers in a live tenant DB (Q-10) | 1 d | Released by this task; needs its `(table, number column, scope column)` inventory |
| `M0-01-03` | SP deployment script + rebuild runbook | 1 d | `Needs Review`, blocked on a human-executed rebuild drill |
| `M2-B04` | Decouple `IApprovalService` + 13 `Pages` refs | 1 wk | `Ready`, zero tracked dependents |
| `M2-C04-02` | Form controls + validation display | 4 d | `Ready`; its dependents (`M2-C05`, `M2-C05-01`) additionally need this task itself, not `M2-B02` any more |
| `M2-C04-03` | Modal, drawer, toast, states | 3 d | `Ready`, zero tracked dependents |
| `M2-C10` | Decimal handling — no float money arithmetic | 2 d | `Ready`; its dependent `M2-C07` also needs `M2-C05-01`, nowhere near `Ready` |
| `M2-B09` | Reference-data endpoints + caching | 3 d | `Ready`, P1 — released by `M2-B02`'s merge |
| `M2-A01-02` (once merged) | releases `M2-A01-03` | — | Awaiting owner review |
