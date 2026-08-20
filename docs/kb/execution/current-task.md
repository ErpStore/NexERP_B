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
dependencies: [KB-081, KB-082, KB-088, KB-107]
---

# Current Task

> **This file holds exactly one task — the active one.** It is a *pointer plus the minimum
> needed to start*, never a copy of the knowledge base. Follow the references.
>
> Procedure: [`workflow.md`](workflow.md) (KB-088). Full spec: the task file linked below.
> Status authority for all other tasks: [`task-tracker.md`](task-tracker.md) (KB-081).

## Active task — `M2-B12-01`, INV-012 — document numbering + financial-year investigation

**Task file:** [`tasks/M2-B12-01.md`](tasks/M2-B12-01.md).

**Status:** `Ready`. Not yet started — this file was rewritten by `M2-A06`'s close-out
(2026-08-20) to point here; no branch exists yet.

### Why this task, now

`M2-A06` (exception middleware → `ProblemDetails` + correlation ids) closed this session
validated `PASS` and moved to `Needs Review` — not `Completed`, so per
[KB-088 "Who may set COMPLETED"](workflow.md#who-may-set-completed) it does **not** release
`M2-B02` / `M2-B06` / `M2-B11`, which all list it as a Hard prerequisite. Those stay `Blocked`.

Applying the [Ready-task selection rule](dependency-graph.md#ready-task-selection-rule)
against the four genuinely `Ready` P0 candidates (`M2-B04`, `M2-B12-01`, `M2-C10`,
`M0-01-03`):

- **Downstream unblocking is the deciding factor, and it is a real tie broken by what
  actually moves.** `M2-B12-01` and `M2-C10` each have exactly one direct dependent in the
  tracker (`M2-B12-02` and `M2-C07` respectively) — `M2-B04` and `M0-01-03` have none. But
  `M2-B12-02`'s **only** Hard prerequisite is `M2-B12-01`, so finishing this task makes a real
  task `Ready` immediately. `M2-C07` also needs `M2-C05-01`, which is nowhere close to
  `Ready` — finishing `M2-C10` alone unblocks nothing. That breaks the tie in
  `M2-B12-01`'s favor.
- Neither `M2-B12-01` nor `M2-C10` sits on the stated critical path
  ([KB-082 § Project critical path](dependency-graph.md#project-critical-path)); estimates
  are identical (2 d each), so those tie-break steps do not distinguish them either — the
  "makes something else Ready" reasoning above is what decided it, recorded here so a future
  session does not have to re-derive it.

`M2-B04` (P0, 1 wk, decouple `IApprovalService`) and `M0-01-03` (P0, 1 d, rebuild drill —
genuinely `Ready` now that a local `SQLEXPRESS` instance was found, see tracker footnote ²¹)
remain valid `Ready` candidates for whoever plans after this one; they were ranked below
`M2-B12-01` only on the downstream-unblocking step above, not excluded for any other reason.

### What this task does

**Documentation only — no C# file changes.** Runs **INV-012**, the scheduled-but-never-run
investigation into how V.SMART allocates document numbers, and produces
`docs/kb/modules/document-numbering.md` (**TO BE CREATED**, `doc_id: KB-100`). Four required
deliverables: (1) a `file:line` call-site inventory grouped by mechanism (raw-SQL last-number
read, lock-free LINQ read, allocation-table read-modify-write — three mechanisms already
Confirmed on 2026-08-12, reproduce and reconcile rather than copy), (2) a format catalogue of
every document series' user-visible string and suffix rule, (3) the financial-year rule
including a known duplicate implementation, (4) corrections to **R-12** (technical-debt
register) — two of its four factual claims are already known wrong (re-verified 2026-08-12:
37 of 38 raw-SQL sites *do* carry `WITH (UPDLOCK, ROWLOCK)`, and it is 36 files / 38 sites,
not "~20"). R-12 stays `Inferred (high confidence)` at the end of this task — only
`M2-B12-02`'s duplicate census can upgrade it; do not upgrade or downgrade it here.

This is the first of a three-task tree (`M2-B12-01` → `M2-B12-02` → `M2-B12-03`) that
produces race-safe, idempotent document numbering (R-12) before the API's first
document-create endpoint — a **Hard** prerequisite on the dependency graph
(`M2-B12-03 → first document-create endpoint`).

### Read before starting

The task file's own *Required Existing Knowledge* section is authoritative
([`tasks/M2-B12-01.md`](tasks/M2-B12-01.md#required-existing-knowledge)) — in particular
KB-083 (prompt template, evidence format — binding on every finding this task produces),
KB-002 (Confirmed/Inferred/Unknown), KB-003 (INV-012's own row, anti-repetition), KB-060 R-12
(the risk being corrected), KB-004 Q-10 (the question this hands to `M2-B12-02`), and
KB-005 §*doc_id allocation* (**KB-100+** is the range for task-produced artefacts — `grep`
before claiming).

### Do not

Touch any C# file — this task writes documentation only. Run INV-015 (e-Invoice/e-Way payload
construction) — record the coupling as a question, do not investigate `E_Invoice/**`; that is
explicitly out of scope and scheduled for Phase 4.5. Upgrade or downgrade R-12's confidence
rating — only `M2-B12-02`'s duplicate census can do that. Start `M2-B12-02` or any other task
after this one.

---

## Carried forward from `M2-A06`'s close-out

- **`M2-A06` is `Needs Review`**, implemented and independently validated `PASS` on
  `migration/M2-A06-problem-details` (`f69891a`), all eighteen acceptance criteria `MET`. Not
  merged — awaiting owner review. Full record:
  [`tasks/M2-A06.md` § Execution Record (2026-08-20)](tasks/M2-A06.md#execution-record-2026-08-20)
  and `task-tracker.md` footnote ²³.
- **The API now has one error contract.** `V.SMART/V.SMART.Api/Middleware/` — global
  exception handling, correlation ids, a single `ProblemDetails` factory
  (`ApiProblems.cs`), registered by `UseErrorContract()` before `UseCors` in `Program.cs`.
  `M2-B02`/`M2-B06`/`M2-B11` all build on it and stay `Blocked` until this is merged
  (`Needs Review` does not release a Hard-dependent successor per the selection rule).
- **INV-040 (`Complete`):** business-rule refusals are signalled by tuple return, not
  exception — 79 delete-guard methods across 61 service files. The binding convention for
  every future controller: a controller helper (`ProblemResults.BusinessRuleProblem`), not a
  domain exception. Relevant to any later task writing a second controller.
- **Two open questions raised, not guessed at:** **Q-34** (a refusal tuple sometimes carries
  404/500 semantics that a blanket `409` mapping cannot distinguish — undecidable from
  source) and **Q-35** (the `503`-for-unresolved-tenant and ignore-caller-correlation-header
  design choices had no prior KB position). Neither blocks `M2-B12-01`.
- **Two gaps found during close-out review, now recorded, not `M2-B12-01`'s concern:**
  `/swagger/index.html` returns no `X-Correlation-Id` header (Development-only, no API
  endpoint affected — `api-overview.md`); `ExceptionHandlingMiddleware`'s
  `Response.Clear()` discards CORS headers on an error response, flagged forward to
  **M2-A05** (`technical-debt-register.md` R-24).
- **`task-tracker.md`'s M2 rollup row is known stale** on `M2-C01`/`M2-C04-01`'s own
  `Completed` status (footnote text says `Completed` and merged; the rollup summary line still
  counts them as `Needs Review`). Not corrected during this close-out — out of scope for a
  session closing `M2-A06`. Whoever next touches that row should reconcile it.

## Ready and unclaimed once `M2-B12-01` closes

Selection rule: [KB-082 § Ready-task selection rule](dependency-graph.md#ready-task-selection-rule).
Listed for whoever plans the task after this one — not to be started now.

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-B04` | Decouple `IApprovalService` + 13 `Pages` refs | 1 wk | Largest single extraction in M2-B; 0 direct dependents, ranked below `M2-B12-01`/`M2-C10` this round on that basis only |
| `M2-C10` | Decimal handling — no float money arithmetic | 2 d | P0 correctness, blocks `M2-C07` — but `M2-C07` also needs `M2-C05-01` (not `Ready`), so this alone unblocks nothing yet |
| `M0-01-03` | Deployment script + rebuild runbook | 1 d | Closes a G0 exception; a local `SQLEXPRESS` instance is confirmed available (tracker footnote ²¹) — genuinely actionable, not blocked on an unscheduled human |
| `M2-B01` | API versioning → `/api/v1` | 1 d | P1. Same-file conflict with `M2-A05` — check nothing is in flight before opening |
| `M2-B05` | Typed `ScreenCodes` constants (R-10) | 2 d | P1. Feeds `M2-A01-02`'s filter |
| `M2-C11` | Archive the Angular pilot | 0.5 d | P2 housekeeping |
| `M0-10` | R-08 compute-one/test-another guards audit | — | P1. M0 debt carried into M2 |
| `M2-A01-02` | `[RequireScreen]` / `[RequireRight]` | 3 d | **Do not open blind** — spec contradicts reality (D-5/R-40, the `UserId == 1` bypass); see the task's own file |

`M2-B12`, `M2-C04`, `M2-C05`, `M2-A01` and `M2-D02` are **parent containers** and are never
worked directly.

## What M2 inherits from M0 — "gate passed" is not "clean slate"

G0 passed **with three exceptions**, all owner-deferred, none with a date set
([KB-107 §1](M0-milestone-review.md)):

1. **Criterion 1 — no rebuild drill.** `M0-01-03` is `Ready` (a SQL Server instance was on
   this workstation the whole time — see above); the drill itself has not run.
2. **Criterion 2 — secrets still in history** (`M0-05`, `Blocked` behind `M0-04`).
3. **Criterion 3 — production credentials unrotated, in a public repository** (R-01). `M0-04`
   is `Blocked` on an unidentified owner and now also gated on **Q-32** (the `Tenants` table
   stores the same `sa` credential in plaintext — rotating without answering Q-32 first breaks
   every tenant row that embeds it).

## Two standing process notes for M2

- **Check `git branch --no-merged master` before allocating any KB/INV/Q id.** Several id
  collisions have already happened across parallel M2 branches — `grep`-before-claim cannot
  see a sibling branch. `docs/kb/modules/document-numbering.md` claims **KB-100** — re-`grep`
  before writing it in case a sibling branch already took it.
- **`master` requires pull requests**, but no required status check gates merges yet — that is
  the open half of **Q-20**. The owner holds bypass rights; prefer a PR regardless.

## Baselines as of this file

| | |
|---|---|
| `dotnet test tests/V.SMART.Shared.Tests/V.SMART.Shared.Tests.csproj` | **84 passed, 0 failed** |
| `dotnet test tests/V.SMART.Api.Tests/V.SMART.Api.Tests.csproj` | **21 passed, 0 failed** (new project, `M2-A06`; not yet wired into CI) |
| `dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj --no-incremental` | **0 errors, 6,694 warnings** (baseline 6,695) |
| `frontend/nexgen-web` — `npm run typecheck && lint && test -- --run && build && coverage` | All exit 0 as of `M2-C04-01` tip `9f886a6` (2026-08-20); coverage branches **100 %** |
| CI on `master` | green |

`M2-B12-01` writes no code and needs none of the above to run — it is a source-reading
investigation. The authoritative, continuously-updated command table is
[KB-083 § Verified repository commands](prompt-template.md#verified-repository-commands).
