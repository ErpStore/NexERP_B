---
doc_id: KB-071
title: Milestone Tracker — Phase-by-Phase Execution Plan (Proposal)
module: migration
source_files: []
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: proposal
confidence: n/a
last_verified: 2026-08-26
dependencies: [KB-020, KB-041, KB-050, KB-052, KB-060, KB-070, ADR-001, ADR-002, ADR-003, ADR-004, ADR-005]
---

# Milestone Tracker

> **Proposal / living document.** [KB-070](migration-strategy.md) is the *strategy* — why the
> phases exist and in what order. This is the *execution tracker* — the concrete, checkable
> unit of work for each phase, the gate that closes it, and where we currently are.
>
> **Superseded for day-to-day execution by [KB-080](../execution/README.md).** That plan
> decomposes these milestones into individually executable tasks, each with its own
> fresh-session prompt, plus a dependency graph ([KB-082](../execution/dependency-graph.md))
> and a live status tracker ([KB-081](../execution/task-tracker.md)). This document remains
> the authority on *milestone scope and exit gates*; KB-081 is the authority on *task
> status*. Three tasks were added there from evidence gathered on 2026-08-12 (INV-029):
> **M0-00** (clean version-control baseline), **M0-15** (toolchain/build baseline) and
> **M2-B12** (document-numbering hardening — INV-012 was scheduled into M2 with no task).
>
> Rules for this document:
> - A milestone is **not** complete until its **exit gate** passes. Partial completion is
>   recorded as a percentage in the task table, never by moving the gate.
> - Every task has a stable id (`M2-B03`). Ids are never renumbered — only added.
> - When a milestone closes, record the **actual** duration and the delta against estimate.
>   Phase 3.5 is the first real measurement of `@code` extraction cost and re-baselines
>   Phase 4.
> - Update `last_verified` on every edit.

## Backend platform — settled

The backend for the Angular product is **ASP.NET Core Web API (.NET 9)**, in the existing
`V.SMART/V.SMART.Api` project. It is **extended, not created and not rewritten**
([ADR-001](../decisions/ADR-001-keep-existing-backend.md),
[ADR-002](../decisions/ADR-002-rest-api-layer.md)).

Precisely what that means, because it is easy to misread:

| | Today | Target |
|---|---|---|
| Where business logic lives | `V.SMART.Shared` class library (285 services, 128,518 LOC) | **unchanged — same library, same code** |
| How Blazor Server reaches it | direct in-process DI call, **no HTTP** | unchanged (Blazor keeps running during migration) |
| How Angular will reach it | — | HTTP → `V.SMART.Api` controllers → **the same services** |
| State of `V.SMART.Api` | exists, .NET 9, JWT + Swagger, **2 controllers / 6 endpoints** | ~60–80 controllers over the same services |

So "are we using ASP.NET Core Web API?" — **yes**, and it already exists; it is roughly 10%
built. `CurrencyController` is the working proof that a controller can wrap an untouched
business service ([KB-041](../api/api-readiness-assessment.md)). The work is writing the
missing HTTP surface and the missing authorization layer, not writing business logic.

**Caveat carried into M2:** `V.SMART.Api/Program.cs` registers only `ICurrencyService`,
while `V.SMART.Web/Program.cs` has 242 registrations. Until the shared
`AddVSmartDomain()` extension exists (M2-B07 / R-26), every new controller will fail at
runtime with a DI resolution error. That is the first structural task of M2, not an
afterthought.

---

## Position

> **Status refreshed 2026-08-26.** [KB-081](../execution/task-tracker.md) remains the authority
> on task status; this is a milestone-level summary and will go stale again.

| | |
|---|---|
| **Completed** | M1 — Repository understanding |
| **Current** | **M2 — Foundation**, **33 of 62** tasks `Completed` (53%). M0 is 17 of 24, gate **passed with exceptions**. Across the whole backlog: 55 `Completed`, 28 `Blocked`, 3 `Needs Review`, 2 `Ready`, 14 `Not Started`, 7 `Continuous`. |
| **Blocked** | M3–M6, behind G2. Within M2, the security chain stops at **`M2-A04` — refresh tokens and a token revocation list**, and the whole Angular application chain stops behind **`M2-C02` — the auth foundation: login, token refresh, route guards and the permission store**. |
| **Nothing is self-selectable** | Of the two `Ready` rows, **`M0-11`** is a *product decision* (whether a stock issue may silently take less than requested under FIFO — `Q-01`) and is owner-only; **`M0-06`** (remove the seeded default `Administrator` account) is finished on an unmerged branch. Ten consecutive runner passes on 2026-08-26 found nothing to do. |

### The one change that would restart the most work

**Rotate `Jwt:Secret`** — the signing key for API access tokens. It is `M0-04`'s criterion C-4,
and it is the *only* part of that credential-rotation programme the Angular chain waits on.

The key's historical value is published in this repository's git history, so any token signed
with it is forgeable. That is why **`M2-A04` (refresh tokens + revocation) is correctly
`Blocked`**: a refresh token and a revocation list signed with a known key manufacture the
*appearance* of hardened sessions without the substance — a forged refresh token appears on no
revocation list. Building it first would be worse than today's short-lived access tokens.

**It is a much smaller job than `M0-04` as a whole.** `M0-04` also covers the SQL Server logins,
every tenant's connection string, the GST e-Invoice gateway account and a vendor re-key — none
of which this chain needs. C-4 alone is: generate a new secret of at least 32 bytes, set it in
the API's deployment environment, restart. The groundwork is already done —
`V.SMART.Api/appsettings.json:37` holds `"Secret": ""` since `M0-03` externalised it, and
`StartupConfigurationValidator` already fails closed on a null, empty, short or known-default
value. It needs **one person with API deployment-config access** — not the DBA, not the gateway
vendor.

**What it releases, in order:** `M2-A04` (refresh tokens) → `M2-A05` (CORS and tenant resolution
for a cross-origin SPA) → **`M2-C02` (auth foundation)** → and behind that the six tasks whose
dependency tables name `M2-C02`: `M2-C03` (the app shell — header, permission-filtered sidebar),
`M2-D01` (the Currency screen end-to-end vertical slice), `M2-C05-02` (grid column preferences),
`M2-C08`/`M2-C08-01` (master-data screens), `M2-C09` and `M2-D02-03`.

### Other decisions the owner still owns

| Decision | What it is | Releases |
|---|---|---|
| **`Q-01`** | Should a stock issue be allowed to silently issue less than requested when FIFO layers run short? | `M0-11`, the only `Ready` P0 row |
| **`Q-85`** | Should a `decimal` cross the HTTP wire as a JSON string rather than a number? Money currently arrives as an IEEE-754 double, losing precision at `JSON.parse` before any application code sees it. | `M2-C10` (banning float money arithmetic in the SPA) |
| **A named DBA** | KB-101's runbook needs one name; the read-only census script is written and waiting | `Q-10` (do live tenant databases carry unique constraints on document numbers?) → `R-12` (the numbering race) → `M2-B12-03` (race-safe allocation) |
| **`Q-25`/`Q-26`** | Is the seeded `Administrator` any tenant's only admin, and how must a newly provisioned tenant avoid the published credential? | `M0-06`'s unmet criterion |
| **`Q-84`** | Who redacts the SA and production passwords still sitting at `HEAD` inside `docs/kb/`? | `M0-05` (purging secrets from git history) |
| **`C-7`** | The AES key and IV protecting every tenant's GST gateway credential are hardcoded and public; the vendor must re-key | `M0-04`'s gateway half |


### Milestone map

| ID | Milestone | Est. | Gate | Status |
|---|---|---|---|---|
| **M0** | Stabilise — safety net | 2–3 wks | G0 | ⚠️ **Passed with exceptions** 2026-08-19 — 17/24 done; criteria **2 and 3 unsatisfied**, deferred by owner. `M0-04`/`M0-05` `Blocked` |
| **M1** | Repository understanding | — | G1 | ✅ Complete (rolling) |
| **M2** | Foundation — API + Angular shell + vertical slice | 6–8 wks | G2 | 🔄 **OPEN** — **33/62 (53%)**. The backend API and the Angular component library are well advanced; the **application** layer (auth, shell, screens) is stopped behind `M2-C02` — see *Position* above. |
| **M3** | Core modules — masters → sales order | 12–16 wks | G3 | ⬜ Blocked by G2 |
| **M4** | Advanced modules | 16–22 wks | G4 | ⬜ Blocked by G3 |
| **M5** | Hardening sweep | 6–8 wks (overlapped) | G5 | ⬜ Runs from M2 |
| **M6** | Production migration | 4–6 wks | G6 | ⬜ Blocked by G4 |

> **The "do not start M2 frontend work before G0 passes" rule below was overridden by the owner
> on 2026-08-19**, who closed G0 with criteria 2 and 3 deferred specifically to unbar M2. The
> paragraph is kept because its *reason* still stands and is still unresolved: the stored-procedure
> DDL gap is what makes a fresh environment unbuildable. The rule was **waived, not satisfied**.
>
> **Correction, 2026-08-26.** This paragraph previously also cited `M2-C10` (the task banning
> float arithmetic on money in the SPA) as blocked by the same missing-database gap. **That was
> wrong and is withdrawn.** `M2-C10` was re-diagnosed on 2026-08-25: its wire format was already
> measured and committed by `M2-B10` (`api/openapi.json` plus the generated TypeScript client),
> so no live database was ever needed. Its real blocker is **`Q-85`** — whether money should
> cross the wire as a string. See `task-tracker.md` footnote ⁸⁵.

---

## M0 — Stabilise

**Goal.** Make the repository safe to build on. No migration work happens here.

**Why first.** Every later milestone depends on: being able to rebuild an environment from
source, having CI, and having characterisation tests for the two services whose behaviour
must not drift.

| ID | Task | Addresses | Est. | Status |
|---|---|---|---|---|
| M0-01 | Capture DDL for all 94 stored procedures into `db/stored-procedures/`, one file per proc, with a deployment script | R-04, INV-027 | 4–5 d | ⬜ |
| M0-02 | Confirm whether procs have drifted between tenant databases; if so, record per-tenant variants | Q-14 | 1 d | ✅ |
| M0-03 | Move connection strings and `Jwt:Secret` to environment / Key Vault; remove from `appsettings.json` | R-01, R-02 | 1 d | ✅ |
| M0-04 | **Rotate** the exposed SA password and the `bspl` production credential; rotate the JWT secret | R-01 | 1 d | ⬜ |
| M0-05 | Purge secrets from git history (or accept and document the exposure if history rewrite is refused) | R-01 | 1 d | ⬜ |
| M0-06 | Remove the seeded default Administrator hash; force first-run password set | R-09 | 1 d | ⬜ |
| M0-07 | CI pipeline: restore → build → analyzers → (later) test, on every push | R-05 | 2 d | ✅ |
| M0-08 | `.gitignore` fixes; remove committed `dist/`, `.angular/cache/`, `bin/`, `obj/` | R-14 | 0.5 d | ✅ |
| M0-09 | Fix the two unreachable delete guards — `MfgPoService.cs:504` (`hasInvoice` ← `hasExpInvoice`), `:525` (`hasRc` ← `hasCR`) | R-08 | 0.5 d | ✅ |
| M0-10 | Audit all ~40 `CanDelete…Async` methods for the same copy-paste pattern | R-08, INV-025 | 2 d | ✅ |
| M0-11 | **Product decision** on the silent FIFO under-issue (`StockManagerService.cs:209-233`) — bug or relied-upon? | R-07, Q-01 | decision | ⬜ |
| M0-12 | Characterisation tests for `ICalculationService` — the 9-step totals/tax algorithm, item-wise and header-wise, TCS, round-off | R-05 | 3 d | ⬜ |
| M0-13 | Characterisation tests for `IStockManagerService` — FIFO allocation, partial balance, multi-batch, `StockIssueTrack` | R-05 | 3 d | ✅ |
| M0-14 | Turn off unconditional `DetailedErrors` in production config | R-19 | 0.5 d | ✅ |

> **M0-12 and M0-13 are characterisation tests, not correctness tests.** They pin down what
> the code *does today*, including behaviour we may consider wrong. They exist so that any
> change during migration is visible. M0-11's outcome is applied *after* the tests capture
> the current behaviour, as a deliberate, recorded change.

> **Where the six unticked M0 rows actually stand**, and two of them are not what the tick-box
> suggests. [KB-081](../execution/task-tracker.md) is the authority; this table is parent-level
> and predates `M0-00` and `M0-15`, both of which are ✅ there.
>
> - **`M0-01`** — children `M0-01-01`/`-01-02` are ✅; `M0-01-03` (rebuild drill) is merged but
>   `Needs Review`, waiting on a **named operator** to sign runbook §7. Not a technical blocker.
> - **`M0-04`** *(rotate credentials)* and **`M0-05`** *(purge history)* — `Blocked`, deferred to
>   end-of-milestone by the owner 2026-08-19. **These are G0 criteria 2 and 3**, and they are the
>   single highest-leverage item outstanding: `M0-04` gates `M2-A04` → `M2-A05` → `M2-C02`.
> - **`M0-06`** — `Ready`, but a branch already exists for it.
> - **`M0-11`** — a `Product Decision` (silent FIFO under-issue, Q-01). Owner-only; no runner may
>   self-select it.
> - **`M0-12`** — **its bookkeeping disagrees with itself.** Both children `M0-12-01` and
>   `M0-12-02` are ✅ in KB-081, yet the parent row there still reads `Not Started`. Left ⬜ here
>   rather than silently resolved, because correcting a parent's status is KB-081's call.

### Exit gate G0

- [ ] A fresh, empty SQL Server can be brought to a working tenant database **from source
      control alone** — schema (EF migrations) + all 94 stored procedures — and the app runs
      against it.
- [ ] `git grep` for connection strings and JWT secrets over the working tree returns nothing.
- [ ] Exposed credentials rotated, confirmed by the person with production access.
- [ ] CI is green on `main` and runs on every push.
- [ ] `ICalculationService` and `IStockManagerService` have passing characterisation tests
      in CI.
- [ ] Q-01 answered and recorded in [open-questions.md](../open-questions.md).

---

## M1 — Repository understanding ✅

**Complete**, with one deliberate exception. Delivered: 27 KB documents, module inventory
and dependency graph, API surface + readiness assessment, 5 ADRs, 37-item risk register,
investigation registry, 18 open questions.

**Deliberately incomplete:** per-module business-rule extraction (INV-012 … INV-020). Doing
it all now produces documentation that goes stale before use. Each runs **one module ahead**
of its migration wave, as scheduled in
[investigation-registry.md](../investigation-registry.md). Treat this as a recurring task
inside M3 and M4, not as M1 debt.

### Exit gate G1 — passed 2026-08-12

- [x] Architecture, data model, auth, tenancy, existing UI documented with `file:line` evidence.
- [x] Every module inventoried with dependencies and migration complexity.
- [x] The "can the backend serve Angular unmodified?" question answered with evidence.
- [x] As-is and proposal documentation kept in separate directories.

---

## M2 — Foundation

**Goal.** One module works end-to-end in Angular through the Web API, with server-enforced
permissions — proving every architectural decision before they are applied 60 more times.

**This is the milestone that de-risks the project.** If the vertical slice is awkward, it is
far cheaper to change the pattern here than in M4.

### M2-A — Security and contract (must land before any controller)

| ID | Task | Addresses | Est. | Status |
|---|---|---|---|---|
| M2-A01 | `[RequireScreen]` / `[RequireRight]` authorization filter resolving `UserRight × Screens` per request, with caching | **ADR-004, P0** | 1–2 wks | ✅ |
| M2-A02 | Apply the filter to `CurrencyController` and prove denial with a permission-less user | ADR-004 | 1 d | ⬜ |
| M2-A03 | Automated permission-matrix test harness — every endpoint × every right combination | ADR-004 | 3 d | ⬜ |
| M2-A04 | Refresh tokens + revocation; shorten access-token lifetime from 480 min | A4, R-03 | 3–5 d | ⬜ |
| M2-A05 | Tenant resolution for a cross-origin SPA: tenant in login request, JWT claim thereafter; real CORS origin list (replace the `AngularDev` → `localhost:4200` policy) | A3 | 3–5 d | ⬜ |
| M2-A06 | Global exception middleware → `application/problem+json`, correlation ids, request logging | A5 | 3–5 d | ✅ |
| M2-A07 | `GET /api/v1/me` — user, tenant, role, and the full `UserRight` set for client-side rendering | B5 | 2 d | ✅ |
| M2-A08 | Resolve Q-05 … Q-08 (QR expiry, trial/expiry, device binding, `StateCodesCsv` row scoping) and enforce whatever is real, **server-side** | Q-05…Q-08, INV-028 | 3 d | ✅ |

> **M2-A08 is not optional polish.** If `User.StateCodesCsv` is real row-level security and
> is only applied in Razor pages today, then every list endpoint leaks other states' data
> the moment it ships. It must be settled before M2-C.

### M2-B — API structure

| ID | Task | Addresses | Est. | Status |
|---|---|---|---|---|
| M2-B01 | API versioning — move to `/api/v1` | C3 | 1 d | ✅ |
| M2-B02 | Server-side paging/sort/filter contract, uniform across all list endpoints | B9 | 1 wk | ✅ |
| M2-B03 | Controller template + conventions codified and documented (thin controller; commands as `POST /{id}/{verb}`) | ADR-002 | 2 d | ⬜ |
| M2-B04 | Decouple `IApprovalService` from the `Authorization` Razor page, plus the other 13 `Pages`-referencing business files | A6, R-11 | 1 wk | ✅ |
| M2-B05 | Typed `ScreenCodes` constants replacing the magic integers passed to `IStockManagerService` | B7, R-10 | 2 d | ⬜ |
| M2-B06 | File upload/download endpoints replacing `IBrowserFile` and local-path `IFileOpener` | B3 | 1 wk | ✅ |
| M2-B07 | **Shared `AddVSmartDomain()` DI extension** used by Web, Api and MAUI hosts | R-26 | 3 d | ✅ |
| M2-B08 | Report endpoints — `GET /{resource}/{id}/print` → PDF; `GET /reports/{slug}` → JSON; export | B4, ADR-005 | 1 wk | ⬜ |
| M2-B09 | Reference-data endpoints (GST rates, UOM, states, screens, terms, currencies) with output caching | B6 | 3 d | ✅ |
| M2-B10 | OpenAPI polish + TypeScript client generation wired into CI | B10 | 3 d | ⬜ |
| M2-B11 | Health checks + structured logging sink replacing the flat-file logger | C2, R-23 | 3 d | ✅ |

### M2-C — Angular foundation

| ID | Task | Est. | Status |
|---|---|---|---|
| M2-C01 | Angular 22 CLI + TS strict; ESLint/Prettier; Jest-or-Vitest + Angular Testing Library; Playwright; CI | 3 d | ✅ |
| M2-C02 | Auth: login, token refresh, route guards, permission store, `PermissionGate` | 1 wk | ⬜ |
| M2-C03 | App shell — header, permission-filtered sidebar, breadcrumbs, ⌘K palette, light/dark | 1.5 wks | ⬜ |
| M2-C04 | Design-system primitives per [design-system.md](../frontend-new/design-system.md) | 2 wks | ⬜ |
| M2-C05 | **`DataGrid`** — server-paged, column preferences, export, empty/loading/error states | 1.5 wks | ⬜ |
| M2-C06 | **`RecordPickerDialog`** — the `DetailsModal` replacement | 1 wk | ⬜ |
| M2-C07 | **`LineItemGrid`** — keyboard-first editable grid | 2 wks | ⬜ |
| M2-C08 | **`DocumentEditor`** shell — header + lines + totals + commands | 2 wks | ⬜ |
| M2-C09 | `ReportPage` framework driven by declarative report definitions | 1 wk | ⬜ |
| M2-C10 | Decimal handling (`decimal.js`) — **never** float arithmetic on money or quantity | 2 d | ⬜ |
| M2-C11 | Archive `frontend/vsmart-erp` (the Angular pilot) | 0.5 d | ⬜ |

> **Tasks that exist in [KB-081](../execution/task-tracker.md) but have no row above.** This
> table is parent-level and predates them; KB-081 is the authority.
>
> - **`M2-C00`** ✅ — rewrote [KB-050](../frontend-new/react-architecture.md) for Angular and
>   re-specified `M2-C01` in the same change. It is the reason `M2-C01` was executable.
> - **`M2-C04-01`** ✅ design tokens, theming, light/dark · **`M2-C04-02`** ✅ form layout,
>   controls and validation display · **`M2-C04-03`** in progress — modal, drawer, toast, states.
>   `M2-C04` stays ⬜ until all three land.
> - **`M2-C12`** ✅ *(and `M2-C12-01…05`)* — **re-specified all 25 superseded `M2-C`/`M2-D` task
>   files for Angular.** ADR-007 replaced React on 2026-08-20 and left every downstream task
>   carrying a `⛔ STOP — this specification is superseded` banner, so the dependency graph said
>   they were ready while the task files said they were not. **Zero banners now remain.** Without
>   it nothing below `M2-C01` was implementable, whatever the gates said.
>
> **Frontend reality check, 2026-08-23:** the app is an Angular 22 + PrimeNG workspace at
> `frontend/nexgen-web/` with a token-driven design system and **215 tests across 29 files**
> (from 6 across 2 at scaffold). It renders one placeholder route — no ERP screen exists yet.
> That is `M2-D`'s job.

### M2-D — Vertical slice

| ID | Task | Est. | Status |
|---|---|---|---|
| M2-D01 | Currency: controller (exists) + Angular list/form, permission-gated | 3 d | ⬜ |
| M2-D02 | Customer Master: extract `@code` logic → controller → Angular screens | 1.5 wks | ⬜ |
| M2-D03 | Parity test — create/edit/delete a customer through Blazor and through Angular, compare persisted rows | 3 d | ⬜ |

### Exit gate G2

- [ ] Currency **and** Customer Master fully working in Angular: login, tenant resolution,
      permission-gated CRUD, server paging, validation, error contract, Excel export.
- [ ] The Blazor app is **untouched and still live** against the same database.
- [ ] A user with no rights on a screen is refused by the **API**, not just by the UI —
      verified by the permission-matrix harness in CI.
- [ ] The TypeScript client is generated from OpenAPI in CI, not hand-written.
- [ ] Parity test M2-D03 passes.
- [ ] Controller template and error contract documented and adopted.

---

## M3 — Core modules

**Goal.** A pilot tenant runs masters, the sales pipeline through Sales Order, approvals,
and core reports entirely in Angular.

**Per-wave recipe** — every wave, without exception:

1. Run the wave's INV-0xx business-rule extraction **one wave ahead**.
2. Triage the module's `@code` (INV-024): presentation / data-fetch / **business**.
3. Extract the business slice into services; verify against the running Blazor app.
4. Build controllers from the M2-B03 template.
5. Build Angular screens.
6. Parity test, then feature-flag on for the pilot tenant.

| ID | Wave | Modules | Est. | Status |
|---|---|---|---|---|
| M3-1 | 3.1 | Masters — Accounts, General (Customer, Vendor, Terms, States, Machines) | 3 wks | ⬜ |
| M3-2 | 3.2 | Masters — Inventory (Item, BOM, BOM Labour, Process, Store, HSN, Raw Material) | 4 wks | ⬜ |
| M3-3 | 3.3 | Masters — Admin & Settings (Users, **permission matrix**, Screens, General/Print/Company settings) | 2 wks | ⬜ |
| M3-4 | 3.4 | Approvals inbox — exercises the workflow-command pattern early | 1.5 wks | ⬜ |
| M3-5 | 3.5 | Sales: Leads → Enquiry → Feasibility → Quotation → **Sales Order** | 4 wks | ⬜ |
| M3-6 | 3.6 | Report framework + first 10 reports (parallel) | 2 wks | ⬜ |
| M3-7 | 3.7 | Dashboard | 1.5 wks | ⬜ |
| M3-8 | — | Feature-flag infrastructure: per-tenant, per-module | 1 wk | ⬜ |
| M3-9 | — | **Re-baseline M4** from measured M3-5 extraction cost | 2 d | ⬜ |

> **M3-5 is the reference implementation** for `DocumentEditor`, and the first honest
> measurement of `@code` extraction cost — `MfgPOUpsert.razor` alone is 4,383 LOC with
> ~2,380 in `@code` (quantity balancing, row validation, short-close, cancellation with
> mandatory reason). Until M3-5 completes, **every M4 estimate is provisional.**

### Exit gate G3

- [ ] Pilot tenant operating masters, sales pipeline through Sales Order, approvals and
      core reports in Angular, in production, with Blazor available as a per-module fallback.
- [ ] Permission matrix administered from the Angular app itself (M3-3).
- [ ] Parity tests green for every wave.
- [ ] M4 estimates re-baselined against actual M3-5 extraction effort (M3-9).
- [ ] Zero fallbacks to Blazor for migrated modules over the final two weeks of the milestone.

---

## M4 — Advanced modules

**Goal.** Full functional parity. Estimates below are **provisional until G3**.

| ID | Wave | Modules | Note | Est. | Status |
|---|---|---|---|---|---|
| M4-1 | 4.1 | Out Sourcing + Purchase (Requisition → … → GRN → SCN → Invoice → Debit Note) | SCN writes stock — must follow M4-2's hardening, or sequence carefully | 5 wks | ⬜ |
| M4-2 | 4.2 | Inventory / Stock (Issue Request, MIN, STN, Inter-Store, Tool Crib, Stock Position) | **highest correctness risk in the product** | 4 wks | ⬜ |
| M4-3 | 4.3 | Planning (Job Order, Route Card, RC Release, Estimation, Requirement Analysis) | | 4 wks | ⬜ |
| M4-4 | 4.4 | Production + **shop-floor Production Log UI** (bespoke touch-first) | depends on Q-11 (MAUI future) | 4 wks | ⬜ |
| M4-5 | 4.5 | Manufacturing Work (DC, Tax Invoice, Export Invoice) + **e-Invoice / e-Way Bill** | statutory; allow contingency | 4 wks | ⬜ |
| M4-6 | 4.6 | Sub Contract (DC-Out, GRN, SCN, Invoice) | `SubConGRNService` 5,631 LOC | 3 wks | ⬜ |
| M4-7 | 4.7 | Labour Work (GRN, SCN, DC-Out, Invoice, Credit Note) | **largest single item** — 6,112-LOC service + 6,528-LOC page | 4 wks | ⬜ |
| M4-8 | 4.8 | Accounts / Cash Flow (Payments, Receipts, Advance Adjustment, Service Bills, Bank) | TDS and allocation | 3 wks | ⬜ |
| M4-9 | 4.9 | HR (Leave, Attendance, Payroll, Staff Loan, Letters) | payroll parity testing | 3 wks | ⬜ |
| M4-10 | 4.10 | Inspection / QC, Maintenance, Utilities | | 2 wks | ⬜ |
| M4-11 | 4.11 | Remaining ~30 reports (parallel) | | 2 wks | ⬜ |

### Exit gate G4

- [ ] Every module in [module-inventory.md](../modules/module-inventory.md) available in Angular.
- [ ] All 440 legacy routes either mapped to an Angular route or explicitly retired with a
      recorded reason ([page-map.md](../frontend-new/page-map.md)).
- [ ] e-Invoice and e-Way Bill verified against the gateway sandbox **and** one live document
      per tenant.
- [ ] Payroll parity verified across a full pay cycle.
- [ ] Idempotency keys on document-create endpoints; document-numbering race (R-12) closed.

---

## M5 — Hardening

**Runs continuously from M2.** Only the final sweep is a discrete block. Listing it as a
terminal phase would be a lie about when the work happens.

| ID | Activity | When | Status |
|---|---|---|---|
| M5-01 | Unit tests for every extracted business rule | with each extraction | ⬜ |
| M5-02 | API integration tests per controller | with each controller | ⬜ |
| M5-03 | Component tests (RTL) for design-system primitives | M2 | ⬜ |
| M5-04 | E2E (Playwright) per module critical path | with each module | ⬜ |
| M5-05 | **Permission-matrix testing — mandatory CI gate** | M2 onward | ⬜ |
| M5-06 | Parity testing per module | per module | ⬜ |
| M5-07 | Performance: 10k-row grids, 200-line documents, concurrent document creation | M5 | ⬜ |
| M5-08 | Security: tenant isolation, IDOR on `{id}` routes, JWT handling, XSS | M5 | ⬜ |
| M5-09 | Accessibility: axe in CI + manual keyboard pass per screen | continuous | ⬜ |
| M5-10 | Load test against a production-sized tenant; live index review (R-13, INV-026) | M5 | ⬜ |

### Exit gate G5

- [ ] Permission-matrix and parity suites green in CI, blocking merge.
- [ ] Pen test findings closed or accepted in writing.
- [ ] Performance targets met against production-sized data.
- [ ] No axe-critical violations.

---

## M6 — Production migration

| ID | Step | Status |
|---|---|---|
| M6-01 | Deployment topology: Angular static build on CDN/nginx; API containerised; Blazor retained; per-tenant subdomain routing preserved (resolve Q-16) | ⬜ |
| M6-02 | Monitoring: structured logs, APM traces, error tracking, uptime, per-tenant dashboards | ⬜ |
| M6-03 | Staged rollout by per-tenant/per-module flag — smallest tenant first (needs Q-12) | ⬜ |
| M6-04 | Rollback drill: flip a module back to Blazor in production and confirm no data issue | ⬜ |
| M6-05 | User migration: no credential migration (same `Users` table, same hashes); per-role training; side-by-side period; in-app guided tour | ⬜ |
| M6-06 | EF migration rollout procedure per tenant, documented and automated (Q-02) | ⬜ |
| M6-07 | Decommission Blazor routes module by module — only after ≥1 full financial period with zero fallbacks | ⬜ |
| M6-08 | Decide and execute the MAUI app's future (Q-11) | ⬜ |

### Exit gate G6

- [ ] All tenants on Angular for all modules.
- [ ] One full financial period with zero module-level fallbacks.
- [ ] Rollback drill executed successfully at least once in production.
- [ ] Blazor routes retired; the decommissioning decision recorded as an ADR.

---

## Cumulative timeline

| Milestone | Duration | Cumulative |
|---|---|---|
| M0 | 2–3 wks | — |
| M1 | done + rolling | — |
| M2 | 6–8 wks | ~2 mo |
| M3 | 12–16 wks | 5–6 mo |
| M4 | 16–22 wks | 9–12 mo |
| M5 | 6–8 wks (mostly overlapped) | 10–13 mo |
| M6 | 4–6 wks | **11–14 months** |

Assumes 2–3 backend, 2–3 frontend, 1 QA. **The dominant variable is M4**, and within it the
`@code` extraction. M3-9 is the point at which this table stops being a guess.

## Standing constraints (apply to every milestone)

1. No database schema change.
2. No rewrite of any business service; no replacement of EF Core, AutoMapper, FastReport, or
   the 94 stored procedures.
3. Business logic is extracted from `@code` into services **before** the Angular screen is
   built — never reimplemented in TypeScript.
4. The server is authoritative for calculations, validation, permissions and numbering. The
   client may mirror a calculation for responsiveness but never owns the result.
5. Feature freeze on the Blazor app for anything a migrating module touches (needs Q-13).
6. Every milestone closes on its gate, not on its estimate.
