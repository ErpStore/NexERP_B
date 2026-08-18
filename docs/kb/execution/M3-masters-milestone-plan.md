---
doc_id: KB-088
title: M3-Masters — Milestone Plan and Effort Calculation (Master Module Vertical Slice)
module: execution
source_files:
  - NexGen-ERP---2025-master.sln
  - V.SMART/V.SMART.Shared/Pages/Master_Module_pages/
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/MasterService/
  - V.SMART/V.SMART.Shared/ViewModels/MasterViewModel/
  - V.SMART/V.SMART.Web/Program.cs
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: proposal
confidence: measured
last_verified: 2026-08-18
dependencies: [KB-001, KB-041, KB-070, KB-071, KB-080, KB-081, ADR-001, ADR-002, ADR-003, ADR-004]
---

# M3-Masters — Milestone Plan and Effort Calculation

> **Proposal.** Scope: take the **Master module only** from Blazor Server to
> React 19 + ASP.NET Core Web API, module-by-module, **reusing the existing business
> layer without rewriting it** (ADR-001).
>
> All scale figures below are **measured from the working tree on 2026-08-18**, not
> estimated. Effort figures are engineering-effort ranges, not commitments.

---

## 0. Baseline correction — read this first

KB-001, KB-041 and KB-071 state that `V.SMART.Api` exists with 2 controllers / 6
endpoints, and that `CurrencyController` is the working proof that a controller can wrap
an untouched business service.

**That project is not in this repository.**

| Check | Result |
|---|---|
| `NexGen-ERP---2025-master.sln` references `V.SMART\V.SMART.Api\V.SMART.Api.csproj` | ✅ yes |
| `V.SMART/V.SMART.Api/` exists on disk | ❌ **no** |
| `git log --all -- 'V.SMART/V.SMART.Api'` | ❌ **empty — never committed** |
| Any `*Controller.cs` in the tree | ❌ **none** |

Consequences, all of which are priced into this plan:

1. **The solution does not build as checked out** — a referenced project is missing.
   This is task M0-15 (toolchain/build baseline) and it is the true first task.
2. **The HTTP surface starts at 0%, not 10%.** There is no proven controller → service
   path. Establishing one is now a milestone task (M3F-B02), not a given.
3. The Angular pilot in `frontend/vsmart-erp` points at `http://localhost:5144`, an API
   that does not exist in source control. It proves nothing and stays archived
   (ADR-003).

> **Action:** KB-001, KB-041 and KB-071 must be corrected. Tracked as **INV-035**.

---

## 1. Measured scope of the Master module

### 1.1 Frontend surface

| Metric | Value |
|---|---|
| Sub-masters (page folders under `Pages/Master_Module_pages/`) | **38** |
| Razor pages | **78** |
| Routes (`@page`) | **130** |
| Distinct permission screens (`ScreenName`) | **41** of 148 product-wide |
| Razor LOC | **54,715** |
| — of which inside `@code` blocks | **32,781 (60%)** |
| — of which markup | 21,934 (40%) |

### 1.2 Backend surface (reused, not rewritten)

| Metric | Value |
|---|---|
| Master business-service files | **64** (13,350 LOC) |
| Master ViewModels (DTO source) | **36** (2,444 LOC) |
| Master repository files | **122** |
| Master entity classes | **72** |
| AutoMapper profiles (master-related) | **37** of 148 |
| Master services referencing UI types | **5** of 64 — see §1.4 |
| **Stored procedures used by Master services** | **0** ✅ |

### 1.3 The single most useful finding

**The Master module uses zero stored procedures.**

`grep -o 'Sp_[A-Za-z0-9_]*' BusinessLayer/BusinessService/MasterService/` returns nothing.

R-04 (82 of 94 stored procedures have no DDL in source control) is a **Critical** risk and
a P0 gate on M0 — but it gates **reports and transactional modules**, not this one. M0-01
therefore does **not** block M3-Masters and the two can run in parallel. This removes
roughly 4–5 days from the critical path and is the strongest argument for making Masters
the first module.

### 1.4 The five services that need decoupling

These reference `IBrowserFile` / `IJSRuntime` / MudBlazor / `Microsoft.AspNetCore.Components`
and must be decoupled before a controller can call them (R-11):

- `MasterService/AdminService/UserService.cs`
- `MasterService/CompanyService.cs`
- `MasterService/GeneralService/CustomerService.cs`
- `IBusinessService/IMasterServices/ICompanyService.cs`
- `InspectionService/MasterInspectionService.cs`

**59 of 64 master services are already clean** and can be wrapped as-is. That is the good
news that makes ADR-001 hold.

---

## 2. Workspace layout — one repository, two workspaces

You asked whether to create a new workspace for the new UI. **Yes for the workspace, no
for a separate repository.**

```
NexERP_B/
├── NexGenErp.sln                  ← NEW solution (the old .sln is broken; see §0)
├── src/
│   ├── NexGenErp.Api/             ← NEW  ASP.NET Core Web API (starts empty)
│   ├── NexGenErp.Shared/          ← EXISTING V.SMART.Shared — business logic KEPT
│   ├── NexGenErp.Web/             ← EXISTING Blazor Server — stays live in production
│   └── NexGenErp.Maui/            ← EXISTING MAUI hybrid
├── web/                           ← NEW React workspace (pnpm)
│   ├── apps/erp/                  ←   Vite + React 19 + TS strict
│   └── packages/
│       ├── ui/                    ←   design system (NexGen brand)
│       ├── api-client/            ←   GENERATED from OpenAPI — never hand-edited
│       └── config/
├── tests/
│   ├── NexGenErp.Api.Tests/
│   └── e2e/                       ←   Playwright
├── db/                            ← EXISTING stored procedures
└── docs/                          ← EXISTING knowledge base
```

**Why one repository and not two:**

1. **Contract drift is the #1 failure mode of this migration.** The OpenAPI → TypeScript
   client must regenerate in the *same commit* that changes a controller, so a breaking
   change is a build failure, not a runtime surprise found in QA. Across two repos this
   requires version pinning and a release dance, and it will be skipped under deadline.
2. **Parity testing needs both sides.** Every master is verified by running the same input
   through Blazor and React and comparing persisted rows. That test cannot live in one repo.
3. **Atomic rollback.** Feature flags flip per module; a bad module reverts as one commit.

`pnpm` workspaces and the .NET solution coexist fine. CI runs two jobs off one checkout.

**Rebranding rename** (`V.SMART` → `NexGenErp`) touches 1,891 files and is almost entirely
mechanical. Do it **once, before M3F starts** — renaming after 40 React screens reference
the old namespaces is needless pain. Budget 1.5–2 weeks including build fixes, MAUI
application id, FastReport template logos, and the 16 files carrying `bhargavi`/`BSPL`.

---

## 3. Milestone structure

Three milestones. **M3F is paid once for the whole programme**; every module after Masters
skips it.

```
M0 (in progress) ──┐
                   ├──► M3F Foundation ──► M3M Masters build ──► G3 gate
M0-01 (SPs) ───────┘        (once)            (38 sub-masters)
   └─ NOT a blocker for Masters (§1.3) — runs in parallel
```

### M3F — Foundation · Gate G2

Everything that must exist before the first master screen can ship.

#### M3F-A · Rebrand and workspace (prerequisite)

| ID | Task | Est. |
|---|---|---|
| M3F-A01 | Fix the broken solution; create `NexGenErp.sln`, restore/build green | 1–2 d |
| M3F-A02 | Namespace + assembly rename `V.SMART` → `NexGenErp` (1,891 files) | 4–6 d |
| M3F-A03 | Brand assets: logo, favicon, splash, MAUI app id, FastReport templates | 2–3 d |
| M3F-A04 | Repo restructure to §2 layout; pnpm workspace; CI two-job pipeline | 2–3 d |
| | **Subtotal** | **9–14 d** |

#### M3F-B · Backend platform

| ID | Task | Est. |
|---|---|---|
| M3F-B01 | Create `NexGenErp.Api` project (does not exist — §0) | 1–2 d |
| M3F-B02 | `AddNexGenDomain()` DI extension — 242 registrations shared across hosts (R-26) | 3–5 d |
| M3F-B03 | **Vertical slice: `CurrencyController`** — proves controller → untouched service | 2–3 d |
| M3F-B04 | JWT + refresh tokens + revocation + `GET /api/v1/me` (user, tenant, rights) | 8–10 d |
| M3F-B05 | **`[RequireScreen]` / `[RequireRight]` authorization filter (ADR-004)** | 8–12 d |
| M3F-B06 | Typed `ScreenCodes` constants — 148 screens (R-10) | 2 d |
| M3F-B07 | `ProblemDetails` error contract + exception middleware + correlation ids | 4–5 d |
| M3F-B08 | Tenant resolution for a cross-origin SPA + real CORS config | 4–6 d |
| M3F-B09 | Decouple the 5 UI-coupled master services (§1.4, R-11) | 3–4 d |
| M3F-B10 | File upload/download + Excel import/export endpoints | 6–8 d |
| M3F-B11 | OpenAPI polish + TypeScript client generation wired into CI | 3–4 d |
| | **Subtotal** | **44–61 d** |

> **M3F-B05 is the milestone's highest-risk item and is non-negotiable.** Today
> authorization exists only in `BaseUserRightsComponent`, inherited by 296 of 333 pages —
> no business service or repository checks permissions. In Blazor Server that is fragile
> but contained, because there is no HTTP surface to bypass. **The moment a REST API
> exists, every endpoint is open to any authenticated user until this filter lands.**
> No controller ships before it.

#### M3F-C · Frontend platform

| ID | Task | Est. |
|---|---|---|
| M3F-C01 | Vite + React 19 + TS strict, ESLint/Prettier, Vitest, Playwright, CI | 3 d |
| M3F-C02 | NexGen design tokens, theme, light/dark | 5–7 d |
| M3F-C03 | Design-system primitives (`packages/ui`) | 10–12 d |
| M3F-C04 | App shell — header, permission-filtered nav (148 screens), breadcrumbs, ⌘K | 8–10 d |
| M3F-C05 | Auth — login, silent refresh, route guards, permission store, `PermissionGate` | 5–7 d |
| M3F-C06 | **`DataGrid`** — server paging, sort, filter, column preferences¹, export | 8–10 d |
| M3F-C07 | **`RecordPickerDialog`** — replaces the Blazor `DetailsModal` | 5 d |
| M3F-C08 | **`MasterCrudShell`** — one list+form scaffold that all 38 masters configure | 5–7 d |
| | **Subtotal** | **49–61 d** |

¹ `ColumnPreferenceVM` / `ShowColumn()` in `BaseUserRightsComponent` is an existing
per-user, per-screen column-visibility feature. Users have it today; dropping it is a
visible regression. It must be carried into `DataGrid`.

> **Deliberately deferred to M4:** `LineItemGrid` and `DocumentEditor` (~4–6 weeks
> combined). Masters are header-only forms — they need neither. Deferring them is the main
> reason Masters is the correct first module and saves ~4 weeks off this milestone versus
> the KB-070 Phase 2 scope.

**M3F total: 102–136 person-days.**

**Gate G2 (exit criteria).** Solution builds; brand rename complete; `CurrencyController`
serving a React Currency screen end-to-end — login, tenant resolution, permission-gated
CRUD, server paging, validation, `ProblemDetails`, Excel export — with **at least one
endpoint proven to return 403 for a user lacking the right**, and the Blazor app untouched
and still live.

---

### M3M — Masters build · Gate G3

38 sub-masters, tiered by measured complexity. Each follows the same six-step loop:

```
1. extract business rules from @code   →  docs/kb/business-rules/
2. lift logic into the service layer   →  NexGenErp.Shared (unit-tested)
3. controller + DTOs + validation      →  NexGenErp.Api
4. [RequireScreen]/[RequireRight]      →  + permission-matrix test
5. React screens via MasterCrudShell   →  web/apps/erp
6. parity test vs Blazor               →  same input, compare persisted rows
```

#### Tier A — Simple reference masters · 15 sub-masters · ~9,450 LOC

State · UOM · ScreenManagement · HolidayList · ProjectTypeMaster · Income · Expense ·
CurrencyToday · Bank · Factor · TermsAndConditions · Category · RawMaterial ·
RejectionMaster · Currency

Thin CRUD, little logic in `@code`. Highly repetitive once `MasterCrudShell` exists.

**2.5–5 d each → 38–75 d**

#### Tier B — Moderate masters · 17 sub-masters · ~23,000 LOC

ItemRateUpdation · LeaveApplication · EmployeeLeaveBalance · Machine · HSNMaster ·
Process · LeaveType · ShiftAllocation · Company · Store · Grouping · MasterUpload ·
Candidate · CostCenter · Customer · VendorSupplier · Employee

Real validation, cross-entity lookups, GST/HSN rules, Excel upload paths, address/contact
sub-collections.

**6–11 d each → 102–187 d**

#### Tier C — Hard masters · 6 sub-masters · ~21,300 LOC

| Sub-master | Razor LOC | Why hard |
|---|---|---|
| Items | 5,838 | `ItemUpsert.razor` is 4,731 LOC; `ItemService` 1,414 LOC |
| BOM | 4,337 | recursive assembly structure; `AssemblyDefService` 1,674 LOC |
| Identity | 4,278 | users, roles, login, seeded-admin removal (R-09) |
| BOMLabour | 3,383 | `AssemblyDefLabourService` 1,839 LOC — largest master service |
| Labour Cost Management | 2,956 | costing/rollup logic |
| UserRights | 487 | the **148 screen × 5 right** permission matrix editor |

`UserRights` is small in LOC and large in consequence — it administers the system that
M3F-B05 enforces, and needs the full matrix test.

**18–35 d each → 108–210 d**

**M3M total: 248–472 person-days** (expected ~339).

**Gate G3 (exit criteria).** All 38 sub-masters live in React for a pilot tenant behind
per-tenant feature flags; permission-matrix test green for all 41 master screens; parity
tests pass against Blazor; zero fallbacks to the Blazor UI for one full week of pilot use.

---

## 4. Effort calculation

### 4.1 Person-days

| Milestone | Optimistic | Expected | Pessimistic |
|---|---:|---:|---:|
| M0 remaining (excl. M0-01, parallel) | 18 | 20 | 22 |
| M3F-A Rebrand + workspace | 9 | 11 | 14 |
| M3F-B Backend platform | 44 | 52 | 61 |
| M3F-C Frontend platform | 49 | 55 | 61 |
| M3M Masters build (38) | 248 | 339 | 472 |
| **Total** | **368** | **477** | **630** |

### 4.2 Calendar

Assumes backend and frontend tracks run in parallel through M3F, then the team fans out
across sub-masters. QA at 0.5–1 FTE throughout.

| Team | M3F elapsed | M3M elapsed | **Total to Masters live** |
|---|---|---|---|
| 2 devs | 11–15 wks | 25–47 wks | **8–14 months** |
| 4 devs (2 BE / 2 FE) | 6–8 wks | 13–24 wks | **4.5–7.5 months** |
| 6 devs (3 BE / 3 FE) | 4–6 wks | 8–16 wks | **3–5 months** |

**Recommended: 4 developers + 1 QA → ~5–6 months for the Master module.**

### 4.3 Where the money goes

| | Share of M3 effort |
|---|---|
| One-time foundation (M3F) — never paid again | **~26%** |
| Tier C masters (6 of 38) | **~31%** |
| Tier B masters (17 of 38) | ~29% |
| Tier A masters (15 of 38) | ~11% |
| M0 remainder | ~4% |

Six sub-masters carry a third of the cost. **Sequence Tier A first** — it validates
`MasterCrudShell` cheaply and gets visible screens in front of users in week 8–10.
Attempt Items or BOM first and the shell will be redesigned mid-flight.

### 4.4 Extrapolation to the full programme

Masters is 54,715 of 321,661 Razor LOC (**17%**), but only ~14% of difficulty-weighted
effort — the 65 transactional document editors (3,000–6,500 LOC each) are materially
harder per screen and additionally require `LineItemGrid` + `DocumentEditor`.

| | Estimate |
|---|---|
| Remaining after Masters | 255 pages · 385 routes · ~267k Razor LOC (~151k in `@code`) |
| Full programme (all modules, React + API) | **~1,900–2,900 person-days** ≈ 8–13 person-years |
| With 6 developers | **~18–30 months** |

This is **above** the 11–14 month figure in KB-070. Two reasons, both measurable:

1. KB-070's Phase 3 waves name ~18 masters. The tree contains **38**.
2. KB-070 assumed a 10%-built API. It is 0% (§0).

**Treat M3-Masters as the calibration run.** It is the first real measurement of `@code`
extraction cost (R-06), which is the dominant variable in the whole programme. Re-baseline
the Phase 4 numbers at Gate G3 and not before.

---

## 5. Risks specific to this milestone

| # | Risk | Mitigation |
|---|---|---|
| MR-1 | Controllers ship before M3F-B05 and every endpoint is public (R-03) | G2 gate: a proven 403. No controller merges without a `[RequireScreen]` test |
| MR-2 | `MasterCrudShell` is designed against Tier A and cannot express Items/BOM | Spike `ItemUpsert` requirements during M3F-C08; do not finalise the shell API until that spike lands |
| MR-3 | `@code` extraction reveals rules that only exist in the UI, with no test to protect them | Extract *before* rebuilding (KB-070 principle 3); characterisation tests first |
| MR-4 | Column-preference regression (§M3F-C06 note ¹) | Explicit acceptance criterion on `DataGrid` |
| MR-5 | 38 sub-masters is 2× what KB-070 planned; timelines quoted from KB-070 are wrong | This document supersedes KB-070 §Phase 3.1–3.3 for Master scope |
| MR-6 | Rebrand deferred until after React work begins | M3F-A02 is a hard prerequisite of M3F-B01 |

---

## 6. Open questions for the product owner

| # | Question | Blocks |
|---|---|---|
| Q-20 | Are all 38 sub-masters in scope, or can dormant ones (e.g. `ProjectTypeMaster`, `Factor`) be retired rather than migrated? Each retirement saves 2.5–5 d | M3M sizing |
| Q-21 | Is the MAUI hybrid app in scope for the React rebuild, or does it stay on Blazor? | M3F-A03, programme scope |
| Q-22 | Confirm team size and start date so §4.2 resolves to dates | scheduling |
| Q-23 | Does the licence position on the acquired codebase permit redistribution under a new brand? No `LICENSE` file exists in this repository | **the entire programme** |

---

## 7. Recommended first three tasks

1. **M3F-A01** — fix the solution so it builds (blocks literally everything).
2. **M3F-A02** — the `V.SMART` → `NexGenErp` rename, while the tree is still small.
3. **M3F-B03** — `CurrencyController` + a React Currency screen: the smallest possible
   end-to-end proof, on the one master with an existing service, ViewModel and repository
   already isolated (`CurrencyService.cs`, `CurrencyVM.cs`, `CurrencyRepository.cs`).

Do not start Tier B or C work until Gate G2 passes.
