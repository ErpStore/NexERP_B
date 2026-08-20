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

## Active task — `M2-A07` — `GET /api/v1/me` (user, tenant, role, full rights)

**Task file:** [`tasks/M2-A07.md`](tasks/M2-A07.md). **Status:** `Ready`, not yet started, no
branch yet. Type **Backend**, priority **P0**, estimate **2 d**, gate **G2**.

### Why this task, now

`M2-C00` (rewrite KB-050 for Angular) closed 2026-08-20: implemented across 3 attempts,
**independently validated `PASS`**, status `Needs Review` — done and correct, but **not merged**,
so it does not yet release `M2-C01` or anything else downstream. Full record:
[`tasks/M2-C00.md` § Validation close-out](tasks/M2-C00.md#validation-close-out-2026-08-20).

`M2-A01-03` (tenant-scoped rights cache) **merged** to `master` at `edcf126`, genuinely
releasing `M2-A02` (gated on `Q-28`), `M2-A07` and `M2-A08`.

Candidates for this selection, per the
[Ready-task selection rule](dependency-graph.md#ready-task-selection-rule): `M2-A07`, `M2-A08`,
`M2-B12-01`, `M2-B09`, `M2-B04`, `M0-01-03` (`M2-A02` excluded — `Q-28` is an unanswered
Information dependency; `M2-C01` excluded — its Hard prerequisite `M2-C00` is `Needs Review`,
not `Completed`, despite the tracker's re-scope note having anticipated it as `Ready`).

By downstream-unblocking count, `M2-A07` (releases `M2-C02`) and `M2-B12-01` (releases
`M2-B12-02`) tie at 1 dependent each — both P0, both 2 d, neither named on the stated critical
path. **Genuinely tied and independent** per selection-rule step 4; either could run without
conflicting with the other. **Selected: `M2-A07`**, continuing the M2-A auth/rights thread
`M2-A01-03` just closed. If a second session has capacity, `M2-B12-01` (INV-012 document
numbering, Investigation type, zero frontend/backend overlap) is an equally valid parallel pick.

### What it does

Add a bootstrap endpoint returning the authenticated caller's identity, tenant, role and
**complete screen-rights map** (the 152 × 5 matrix), so the SPA client can render a
permission-correct navigation and gate its controls **client-side, as a UX affordance only** —
the server re-checks independently on every request per ADR-004 §3; this endpoint does not
relax the `M2-A01` filter.

Read rights through the **same** `IUserRightsProvider` `M2-A01-02`/`M2-A01-03` built — two
independent readers would eventually disagree. `M2-A03`'s exempt allow-list must name this
endpoint explicitly (it needs authentication but no screen right). Decide and record whether it
ships at `/api/v1/me` (first versioned route) or `/api/me` pending `M2-B01`. Full detail,
dependencies and open sub-decisions (the `role` reconciliation across `UserRole`/`NavMenu`'s
stray `ERPAdmin`) are in the task file.

### Read this before touching the task file — it has not been re-specified for Angular

`M2-A07` is **Backend**, not one of the 26 `M2-C*`/`M2-D*` files carrying a STOP banner, and its
core work — a server-side endpoint — is framework-neutral. But the task file's prose still says
**"React client"**, **"React sidebar"**, and cites `docs/kb/frontend-new/react-architecture.md`
by its old identity in several places (`:49`, `:57`, `:74`, `:106`, `:121`, `:215`, `:253-259`,
`:346`, `:519`, `:558`, `:704`, `:801`). That file **is** KB-050 — same `doc_id`, same filename,
now rewritten for Angular by `M2-C00`. Read every "React" in `M2-A07.md` as "the SPA client
(Angular per ADR-007)" and every citation into `react-architecture.md` as pointing at the
**current** (Angular) content, not a React one. **Do not** treat this as licence to rewrite the
task file wholesale — that is a re-specification this task was never asked to do; note the
substitution and proceed. If anything in KB-050 as rewritten actually contradicts what `M2-A07`
assumes about the response shape, record it as a new open question rather than guessing.

### Do not

Write React/TypeScript code — this is a backend-only endpoint. Weaken the `M2-A01` filter or
treat this endpoint as authoritative for permissions — it is presentation-only (ADR-004 §3).
Re-specify `M2-A07.md`'s React prose beyond substituting "the SPA client" for "React client" in
your own reasoning; a full rewrite is out of scope unless the owner asks for it.

---

## Nothing else awaits review from the merged side

`M2-A01-03` (tenant-scoped per-request rights cache) is `Completed` and merged (`edcf126`,
2026-08-20). It releases `M2-A02` (still gated on `Q-28` — an API-only administrator holds zero
`UserRight` rows because `AuthController.Login` never calls `SyncRightsForUserAsync`), `M2-A07`
(this task), and `M2-A08`.

`M2-C00` (KB-050 Angular rewrite) is validated `PASS` but sits `Needs Review`, unmerged, on
`migration/M2-C00-kb050-angular-rewrite`. It is ready for the repository owner to review and
merge. Once merged, `M2-C01` becomes a genuine `Ready` candidate.

## Ready and unclaimed, for a session with parallel capacity

| Task | What | Est. | Note |
|---|---|---|---|
| `M2-A08` | Row-level scoping + account gates (Q-05…Q-08) | 3 d | P0, released by `M2-A01-03` |
| `M2-B12-01` | INV-012 document numbering | 2 d | Documentation-only; releases `M2-B12-02`; tied with `M2-A07` on the selection rule |
| `M2-B09` | Reference-data endpoints + caching | 3 d | P1, released by `M2-B07`/`M2-B02` |
| `M2-B04` | Decouple `IApprovalService` | 1 wk | Zero tracked dependents |
| `M0-01-03` | Deployment script + rebuild runbook | 1 d | Carried M0 debt; no longer hardware-blocked |

**A session may run more than one of these** (standing rule since 2026-08-20, `CLAUDE.md` §
Standing constraints) — but **one task, one branch, cut from `master`**, and **never merge,
never push** are unchanged.

---

## Carried forward — still relevant (from `M2-A01-03`, merged `edcf126`)

- **`V.SMART/V.SMART.Api/Authorization/IUserRightsProvider`** now caches per tenant+user
  (`screenrights:v1:{tenantId}:{userId}`, TTL from `Authorization:RightsCacheSeconds`, default
  60 s absolute, explicit `Invalidate(tenantId, userId)`, zero-TTL bypass for `M2-A03`'s
  harness). `M2-A07` must read through this same provider, not a second query path.
- **`Q-28` (open):** an API-only administrator authenticated via `AuthController.Login` holds
  **zero** `UserRight` rows, because that path never calls `SyncRightsForUserAsync`. Blocks
  `M2-A02`, not `M2-A07` directly — but `M2-A07`'s response for such a caller will legitimately
  show an empty rights map until `Q-28` is settled; do not treat that as this task's bug.
- **`Q-37` (open):** what `M2-C11` is for now that ADR-007 inverted it (archive vs. adopt the
  Angular pilot). Does not block `M2-A07`.
- **R-42** (KB-060): `file:line` citations into prose documents rot silently when the cited
  document is edited — observed twice now, most recently in `M2-C00`. `M2-A07`'s stale "React"
  prose (above) is the same failure mode one level up: a *word*, not just a citation, going
  stale when the underlying document's framework changed. Grep the target document before
  trusting an old citation's surrounding words, not just its line range.
