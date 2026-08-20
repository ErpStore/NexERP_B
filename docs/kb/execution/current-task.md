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

## Active task — `M2-A08` — row-level scoping + account gates (Q-05…Q-08)

**Task file:** [`tasks/M2-A08.md`](tasks/M2-A08.md). **Status:** `Ready` in KB-081, `Not
Started` (attempt 0 of 3), no branch yet. **The runner itself is `STOPPED`** (owner
`/migration-stop`, fulfilled 2026-08-20 at the `M2-B12-01` boundary) — this task is the next
dependency-ready candidate, written here for whoever resumes the run; it has **not** been
dispatched. See [`runner-state.md`](runner-state.md) (KB-093).

### Why `M2-A08`, now

Hard prerequisite `M2-A01-03` is `Completed` and merged (`ed559ad`), so `M2-A08` is genuinely
`Ready`. Candidates considered and excluded: `M2-C00`/`M2-A07`/`M2-B12-01` (validated `PASS`
but `Needs Review`, unmerged — a Hard prerequisite must be `Completed`, not `Needs Review`,
CLAUDE.md § Standing constraints); `M2-C01`/`M2-A03`/`M2-B12-02` (`Blocked`); `M2-A02` (`Ready`
but gated on unanswered **Q-28**); `M0-01-03` (`Ready` in KB-081 but its remaining work is an
owner-only, human-executed rebuild drill its own task file forbids an AI session from
performing). Among the remaining P0 candidates, `M2-A08` ranks first: it is a recorded
**Hard** dependency of `M2-D01` ([`dependency-graph.md:145`](dependency-graph.md)), the vertical
slice — no other `Ready` P0 candidate (`M2-B04`) has a tracked dependent.

**It gates the vertical slice `M2-D01`.** It closes four open questions — Q-05, Q-06, Q-07,
Q-08 — three of which are enforced today **only inside Blazor `@code`**, and the API
reproduces none of them.

### What it does

Three things, in order — do not skip to the third:

1. **Investigate** — produce `file:line` evidence for all four gates (QR expiry, trial/device
   expiry, row-level `StateCodesCsv` scoping), classify each Confirmed/Inferred/Unknown per
   KB-002, record negative results explicitly. **Q-05 and Q-06 are already answered** in
   [`open-questions.md`](../open-questions.md) (`QrExpiryDate` checked post-query in two
   duplicated Razor copies, not the query; `TrialDays`/`ExpiryDate` enforced only in
   `Login.razor:271-275` with three carve-outs, and `GetUserTrialAsync` is dead code) — verify
   those citations still hold before relying on them; the task file itself is dated
   2026-08-12 and is a hypothesis, not fact (CLAUDE.md § Authority order). Q-07/Q-08 are open.
2. **Decide, then enforce server-side** — implement in the API whatever the investigation
   proves is real: row filtering and account-level gates. Screen-right enforcement is
   `M2-A01`'s job, already done; this task covers what `[RequireScreen]`/`[RequireRight]`
   does not.
3. **Test** — including negative tests proving a scoped caller cannot see out-of-scope rows,
   and that each account gate refuses at the point it is supposed to refuse.

**Where a gate turns out never to have been enforced, switching it on is a product decision,
not an engineering one** — surface it with evidence and a named owner rather than silently
enabling it. A gate that *is* enforced today must be ported with its carve-outs exactly.

### Coordinate, do not guess

`M2-A04` (login, not yet started) owns `POST /api/v1/auth/login` — the trial and device gates
belong on that path; decide which task lands them and record it. `M2-A06`'s error shapes
govern the status code a scope/gate refusal returns. Full dependency table:
[`tasks/M2-A08.md` § Dependencies](tasks/M2-A08.md).

### Carried forward from `M2-B12-01` (document numbering, closed `Needs Review`)

Not directly relevant to `M2-A08`'s scope, but live in the repository for whichever task picks
it up next: [`docs/kb/modules/document-numbering.md`](../modules/document-numbering.md)
(KB-100, new) and Q-37/Q-38/Q-39/Q-40 in `open-questions.md`. `M2-B12-02` (verify unique
constraints in a live DB) stays `Blocked` until `M2-B12-01` is reviewed and merged.

---

## Nothing else awaits review beyond what is already recorded

`M2-C00` (`migration/M2-C00-kb050-angular-rewrite`, `b3c0e6e`), `M2-A07`
(`migration/M2-A07-me-endpoint`, `61da4bd`) and now `M2-B12-01`
(`migration/M2-B12-01-inv-012-numbering`, `8a54f96`) are all validated `PASS` and awaiting
owner merge. **`M2-A02` must settle `Q-28` before it starts.** An API-only administrator holds
**zero** `UserRight` rows, because `AuthController.Login` never calls
`SyncRightsForUserAsync`. Annotate `CurrencyController` before that is answered and the
administrator authenticates successfully into an empty UI — the R-40 failure mode, moved to
the API side.

## Ready and unclaimed after `M2-A08`

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-B04` | Decouple `IApprovalService` | 1 wk | P0, zero tracked dependents |
| `M2-B01` | API versioning → `/api/v1` | 1 d | P1 |
| `M2-B05` | Typed `ScreenCodes` constants (R-10) | 2 d | P1 |
| `M2-B06` | File upload / download endpoints | 1 wk | P1 |
| `M2-B09` | Reference-data endpoints + caching | 3 d | P1, released by `M2-B02` |
| `M2-B11` | Health checks + structured logging (R-23) | 3 d | P2 |
| `M0-01-03` | Deployment script + rebuild runbook | 1 d | P0 in KB-081, but owner-only human rebuild drill — not autonomously startable |

**A session may now run more than one of these.** The standing rule is *"pick the next task
that can actually be done"*, with a five-part test in [`CLAUDE.md`](../../../CLAUDE.md) §
Standing constraints. **One task, one branch, cut from `master`** is unchanged, as is *never
merge, never push*.
