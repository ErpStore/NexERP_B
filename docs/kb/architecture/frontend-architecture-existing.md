---
doc_id: KB-015
title: Existing UI Architecture (As-Is)
module: architecture
source_files:
  - V.SMART/V.SMART.Shared/Routes.razor
  - V.SMART/V.SMART.Shared/Layout/MainLayout.razor
  - V.SMART/V.SMART.Shared/Layout/NavMenu.razor
  - V.SMART/V.SMART.Shared/Components/
  - V.SMART/V.SMART.Shared/Shared/BaseUserRightsComponent.cs
  - V.SMART/V.SMART.Shared/Pages/
  - frontend/vsmart-erp/
entities: [UserColumnPreference, UserThemePreference]
api_endpoints: []
database_tables: [UserColumnPreference, UserThemePreference, PrintSetting]
business_rules: []
status: complete
confidence: confirmed
last_verified: 2026-08-26
dependencies: [KB-010, KB-013]
---

# Existing UI Architecture (As-Is)

> This document describes **what exists**. The proposed Angular architecture is in
> [`frontend-new/react-architecture.md`](../frontend-new/react-architecture.md).

## Stack

| Concern | Technology |
|---|---|
| Rendering | Blazor Server — `AddInteractiveServerComponents()`, `AddInteractiveServerRenderMode()`. All UI state lives on the server in a SignalR circuit. |
| Component library | MudBlazor 8.11 (`MudNavMenu`, `MudNavGroup`, `MudNavLink`, `MudIcon`, dialogs, inputs) |
| Also present | Bootstrap 5 CSS (`wwwroot/bootstrap/`), hand-written `app.css`, `Microsoft.AspNetCore.Components.QuickGrid` |
| Charts | Blazor-ApexCharts 6.1 |
| Client storage | `Blazored.LocalStorage` |
| Interop JS | `wwwroot/js/{common,autoSelect,isNumericWithDot,notification}.js` |
| Validation | `DataAnnotations` on ViewModels + `Microsoft.AspNetCore.Components.DataAnnotations.Validation` |

Mixing MudBlazor and Bootstrap means two overlapping design languages in one app —
a consistency problem the redesign removes by construction.

### List grids are QuickGrid, not MudBlazor — Confirmed, re-measured 2026-08-25 (M2-C05-01)

A widely repeated assumption is wrong and is corrected here rather than left to be
rediscovered. **`MudDataGrid` and `MudTable` appear in zero `.razor` files.**

| Measurement | Result | Command |
|---|---|---|
| `.razor` files containing `<QuickGrid` | **93** | `grep -rl '<QuickGrid' --include=*.razor V.SMART` |
| `.razor` occurrences of `MudDataGrid` | **0** | `grep -rn 'MudDataGrid' --include=*.razor V.SMART` |
| `.razor` occurrences of `MudTable` | **0** | `grep -rn 'MudTable' --include=*.razor V.SMART` |
| MudBlazor package version | **8.11.0** | `V.SMART/V.SMART.Shared/V.SMART.Shared.csproj:62` |

MudBlazor **is** referenced and **is** used — for navigation, dialogs and inputs, per the table
above. It is simply not what renders a list. Every list screen is
`Microsoft.AspNetCore.Components.QuickGrid` inside hand-written Bootstrap 5 markup; the
reference implementation is
`V.SMART/V.SMART.Shared/Pages/CashFlow_Pages/Payments_Pages/PaymentList.razor:134-238`.

Two consequences the migration depends on:

- **`<QuickGrid Items="vm.AsQueryable()">` materialises the whole collection in the Blazor
  circuit before rendering** (`PaymentList.razor:134`). KB-050's 10,000-row / 60 fps target is
  therefore unreachable in the current architecture — not a tuning problem.
- **Column metadata already has a canonical shape**, `V.SMART/V.SMART.Shared/ViewModels/GridColumn.cs:3-13`:
  `Title`, `Field`, `IsVisible`, `IsDate`, `Width` (default `"120px"`), `Align` (a Bootstrap
  *class name*, `"text-center"`, not a value) and `IsDetailColumn`. `M2-C05-01`'s TypeScript
  column model mirrors those concepts deliberately, so `M2-C05-02` can round-trip persisted
  preferences without a translation table that drifts.

Recorded as INV-053 in [`investigation-registry.md`](../investigation-registry.md); it refines
INV-006 rather than competing with it.

## Routing

`Routes.razor`:

```razor
<CascadingAuthenticationState>
  <Router AppAssembly="@typeof(MainLayout).Assembly">
    <Found>
      <AuthorizeRouteView RouteData DefaultLayout="MainLayout">
        <NotAuthorized><RedirectToLogin /></NotAuthorized>
      </AuthorizeRouteView>
    </Found>
    <NotFound>…</NotFound>
  </Router>
</CascadingAuthenticationState>
```

**440 `@page` directives across 333 components** (many components declare 2–4 routes:
create, create-with-parent-id, update, details). Full list in
[`frontend-new/page-map.md`](../frontend-new/page-map.md).

### Route naming conventions (Confirmed)

| Pattern | Example | Meaning |
|---|---|---|
| `/{entity}List` or `/{entity}` | `/mfgPOList`, `/customer` | list screen |
| `/{entity}/create` | `/MfgPO/create` | new document |
| `/{entity}/create/{ParentId:int}` | `/MfgPO/create/{CustId:int}` | new document pre-bound to a parent |
| `/{entity}/update/{Id:int}` | `/MfgPO/update/{PoId:int}` | edit |
| `/{entity}/details/{Id:int}` | `/mfgPO/details/{PoId:int}` | read-only view |
| `/{module}-home` | `/sales-home`, `/purchase-home` | module landing page |
| `/{module}-master` | `/inventory-master`, `/hr-master` | master-data landing page |

Route casing is inconsistent (`/MfgPO/create` vs `/mfgPO/details` vs `/mfgPOList`) —
worth normalising in the new app.

## Layout and navigation

| File | LOC | Role |
|---|---|---|
| `Layout/MainLayout.razor` (+`.css`) | — | app shell |
| `Layout/NavMenu.razor` (+`.css`) | **888** | sidebar |
| `Layout/BlankLayout.razor` | — | login / QR login |

`NavMenu` has two modes: a **mini icon rail** (drawer closed) and a full `MudNavMenu`
tree (drawer open), with a separate collapsed **production-user menu** when
`ShowProductionMenu` is set. Navigation groups are gated by
`<AuthorizeView Roles="Administrator,ERPAdmin,User">`.

> Note: `"ERPAdmin"` appears in the `AuthorizeView` role list but **does not exist** in
> the `UserRole` enum (`Administrator`, `User` only). Dead role reference. **Confirmed.**

### Navigation tree (the canonical module map)

32 groups. Full tree with routes in [`frontend-new/page-map.md`](../frontend-new/page-map.md).

```
Home · Dashboard
Master ├ Admin · Inventory · General · Account · Human Resource · Settings
Assembly Costing
Sales ├ Manufacturing Work ├ Labour Work
Out Sourcing ├ Purchase ├ Sub Contract
Production / Shop Floor ├ Assembly ├ Component
Planning
Inventory / Stock
Inspection / QC
Maintenance
Human Resource
Cash Flow / Accounts
Utilities
Reports ├ Issue Summary · Accounts · Track Reports · Analysis Reports · Rating
        · Pending Report · History
```

## Page anatomy

Almost every page follows one of three shapes: **List**, **Upsert**, **Details**.

Measured: 333 pages, 321,661 LOC, **~184,000 LOC (57%) inside `@code` blocks**.
Mean ≈ 987 LOC/page.

Largest pages (LOC):

| Page | LOC |
|---|---|
| `SalesAndLabour_pages/LabourDcOut_Pages/LabourDcOutgoingUpsert.razor` | 6,528 |
| `Master_Module_pages/Items_Pages/ItemUpsert.razor` | 4,731 |
| `OutSourcing_Module_pages/PurchOrSubConPO_Pages/PurchPOUpsert.razor` | 4,620 |
| `SalesAndLabour_pages/MfgInv_Pages/MfgInvUpsert.razor` | 4,431 |
| `SalesAndLabour_pages/SalesPo_Pages/MfgPOUpsert.razor` | 4,383 |
| `SalesAndLabour_pages/ExpInv_Pages/ExpInvUpsert.razor` | 4,100 |
| `ProductionModule_pages/ProductionComp_Pages/ProductionIssueCompUpsert.razor` | 3,912 |
| `OutSourcing_Module_pages/SubContractDCOut_Pages/SubContractDCOutUpsert.razor` | 3,911 |

### What is actually inside an `@code` block

Traced in `MfgPOUpsert.razor` (4,383 LOC; `@code` starts at line 2,002). Method inventory:

**Presentation only — safe to discard:**
`OpenModal`, `CloseModal`, `HandleModalConfirmation`, `OpenAttachmentButton`,
breadcrumb setup, modal title/colour/button-text state, `IsQuoteModalVisible`,
column definitions (`QuoteColumns`/`QuoteFields`/`QuoteHiddenColumns`).

**Data loading — becomes an API call:**
`OnInitializedAsync`, `LoadExistingPo`, `InitializeNewPo`, `SearchCustomers`,
`SearchItems`, `GetPendingQuoteCountAsync`.

**⚠️ Business logic — MUST be extracted before the page is deleted:**

| Method | Behaviour |
|---|---|
| `ApplyCustomerSelectionAsync` (80 LOC) | cascades customer → currency, terms, consignee, cost centre, tax mode |
| `OnItemChanged` (75 LOC) | item selection → last unit price, HSN, UOM, assembly existence check |
| `OnQtyChange` / `UpdateQuantities` | quantity vs already-transacted balance arithmetic |
| `IsItemAlreadySelected` | duplicate-line prevention |
| `ValidateRowAsync` / `ValidateLastRowAsync` | line-level validation before add/save |
| `AskToShortCloseAsync` / `ShortClosePo` | short-close workflow |
| `CancelItem` (68 LOC) / `CancelPO` (70 LOC) | cancellation with downstream-transaction checks and mandatory reason |
| `OnItemCancelChanged` (78 LOC) | per-line cancel with quantity revert |
| `ResetSlno`, `DeleteAndResequenceAsync` orchestration | line renumbering |
| `UpdateDueDate` | delivery-date derivation |

**This pattern repeats across all ~65 Upsert pages.** Extracting it is the largest single
work item in the migration and is the reason document modules are rated High/Very High
complexity in [`frontend-new/feature-mapping.md`](../frontend-new/feature-mapping.md).

## Shared components (22, `Components/`)

| Component | Purpose | Angular equivalent |
|---|---|---|
| `BsModal.razor` | confirm dialog, optional reason textbox | `<ConfirmDialog reasonRequired>` |
| `DetailsModal.razor` | generic picker grid over `List<Dictionary<string,object>>` + column/field/hidden lists, multi-select callback | `<RecordPickerDialog>` — **highest-value component to rebuild first** |
| `MasterModal.razor` | inline create of a master record without leaving the form | `<QuickCreateDialog>` |
| `CustomerSelection.razor`, `VendorSelection.razor` | party typeahead + detail cascade | `<PartyPicker>` |
| `ColumnMenu.razor` | per-user column show/hide → `UserColumnPreference` | DataGrid column manager |
| `PageHeader.razor` | title + breadcrumbs (`PageHeader.BreadcrumbItem`) | `<PageHeader>` |
| `ProcessingOverlay.razor` (+`.css`) | busy overlay | `<BusyOverlay>` |
| `UnsavedChangesModal.razor` | dirty-form guard | router `beforeLeave` blocker |
| `SmartBackButton.razor` | context-aware back | breadcrumb/back |
| `ExcelUpload.razor` | template-driven import | `<ExcelImportDialog>` |
| `TrimmedInputText.razor` | trimming text input | form primitive |
| `AuthorizationToggle.razor` | approve/reject control | `<ApprovalActions>` |
| `CorrespondenceStatus.razor` | attachment count badge | `<AttachmentBadge>` |
| `TDSEntryModal.razor`, `RolItemDetailModel.razor`, `RejectionReworkDcModel.razor`, `ExportInvModel.razor`, `EwayDcListModel.razor`, `EwaySettingModel.razor`, `MyCompanyDetails.razor` | domain dialogs | per-module |
| `Underconstruction.razor` | placeholder | — |

The `DetailsModal` + `MasterModal` + `*Selection` trio is how the whole ERP does
"pull lines from an upstream document" — reproducing this interaction well is the single
biggest UX lever in the new frontend.

> **Correction, 2026-08-26 (M2-C06, INV-054):** the "trio" framing is **inaccurate for
> `MasterModal`**. It is 45 lines of modal chrome with a content slot — parameters
> `IsVisible`, `Title`, `MaxWidth`, `ChildContent`, `OnClose` and nothing else
> (`MasterModal.razor:32-39`) — with no table, no search and no selection. It maps to
> M2-C04-03's generic `app-modal`, **not** to `RecordPickerDialog`. It is referenced by
> **133** files, so mis-scoping it would be expensive. The `*Selection` pages are routable
> pages that return via `ReturnUrl` (`CustomerSelection.razor:1-2`), not dialogs. Only
> `DetailsModal` is `RecordPickerDialog`'s territory.

### `DetailsModal.razor` — the 33-call-site survey (Confirmed, 2026-08-26, M2-C06)

Full evidence and the classification table: [INV-054](../investigation-registry.md).
Headlines, because they change what the replacement must do:

- **33 files, but 41 instances** — five files render it more than once.
- **All 41 are multi-select pulls from an upstream document. None is a single-record
  master pick.** Master picking is done by the routable `CustomerSelection` /
  `VendorSelection` pages. `RecordPickerDialog`'s single-select mode is therefore **new
  capability, not migrated behaviour**.
- **`HiddenColumns` is passed by all 41, never omitted** — a per-*screen* static list of
  technical id columns (`Ref*SubId`, item and cost-centre ids) which the selection
  handlers then read out of the returned rows. **Hidden does not mean absent.**
- **`HeaderContent` is passed by 4 of 33**: three a Stock/All scope filter
  (`SubContractDCOutUpsert.razor:53-61`, handler `:2360-2372`), one a colour legend
  (`JobOrderUpsert_pages.razor:40-48`).
- **The conditional cell highlighting has exactly one consumer**, and the flags behind it
  are computed in Razor `@code` (`JobOrderUpsert_pages.razor:2494-2522`) — an unextracted
  BOM-difference calculation, `<W>-03` work.
- **Selection order is load-bearing**: 34 files append the returned rows in iteration
  order and 48 renumber afterwards (`MfgPOUpsert.razor:4014` → `:4072` → `:4077`), so the
  ticking sequence *is* the line order of the document being built.
- **Pre-selection is dead code**: `["Selected"]` is written `false` in 75 places and `true`
  in none.
- **Candidate sets already come from ~75 dedicated server-side service methods**, with
  eligibility already server-side — but **none of them pages, sorts or searches, and none
  is exposed by `V.SMART.Api`**. That is per-wave backend work, ~75 methods plus a
  controller each, and it belongs in the M3-5 estimate.

**Defect, recorded and deliberately not fixed** (`DetailsModal.razor:144-169`, with the
guard at `:156-168`): `ConfirmSelection` tests the result of `.ToList()` for `null` — which
cannot be `null` — and throws `InvalidOperationException("Please select  Dc from the
list.")`, while the `catch` immediately rethrows. The genuinely reachable case, an **empty**
selection, is not handled at all, and the Update button is always enabled (`:90`). The
Blazor component keeps serving all 33 call sites unchanged until each is migrated in its
module wave; the replacement disables its confirm button instead.

## State management (as-is)

There is effectively **no client state management** — Blazor Server keeps component state
on the server. State that does exist:

| State | Mechanism | Scope |
|---|---|---|
| Auth principal | `CustomAuthStateProvider` field | circuit |
| Current user | `CurrentUserService` (cached username/userId) | scoped |
| Tenant | `TenantProvider._cached` | scoped |
| Theme | `ThemeStateService` + `UserThemePreference` | scoped + persisted |
| Grid columns | `BaseUserRightsComponent.Columns` + `UserColumnPreference` | component + persisted |
| Screen rights | `BaseUserRightsComponent.userRights` | component, reloaded per page |
| Local storage | `Blazored.LocalStorage` | browser |

**Implication:** there is no existing client-state design to port. The Angular app starts
from a clean sheet — which is why typed services over `HttpClient` plus signals are proposed
([ADR-007](../decisions/ADR-007-angular-stack.md)) rather than a heavyweight global store.
*(This sentence previously proposed TanStack Query + Zustand, under the superseded ADR-003.
The observation it rests on — that Blazor holds screen rights in a per-page component and
nothing else survives as client state — is unchanged and is what actually matters here.)*

## Forms and validation

- ViewModels carry `DataAnnotations` (`[Required]`, `[StringLength]`, `[RegularExpression]`,
  `[EmailAddress]`, `[Phone]`) — see `CurrencyVM.cs`.
- `EditForm` + `DataAnnotationsValidator` renders messages.
- **Cross-field and cross-row rules live in `@code`**, not in attributes
  (`ValidateRowAsync`, `IsItemAlreadySelected`, quantity-balance checks).

For the new frontend: the attribute layer is mechanically translatable to Zod; the
`@code` layer is not, and must be extracted to the server first.

## Reporting and export from the UI

- **Print:** `@inject ReportService reportService` → `Generate_Report(...)` → `byte[]` →
  base64 → JS interop opens/downloads the PDF (`pdfBase64` field in `MfgPOUpsert.razor`).
- **Excel export:** `ExcelExportService.ExportListToExcel<T>(list, sheetName, lockColumns)`
  and `ExportPendingListToExcel<T>`.
- **Excel import:** `ExcelUpload.razor` + `IExcelTemplateService.CreateTemplateAsync(uploadType)`
  (`UploadTypes` constants).

All three are server-side already and become plain HTTP endpoints.

## The Angular 19 pilot (`frontend/vsmart-erp/`)

A **learning spike, not a product**. Angular 19.2 + PrimeNG 19.1 + PrimeFlex, standalone
components, signals.

Implemented: `login`, `currency-list`, `currency-form`, `shell`, `auth.service`,
`auth.guard`, `auth.interceptor`. That is **1 of ~150 screens**.

Its source comments are explicitly tutorial-style ("LEARNING — Signals for auth state",
"LEARNING — Observables vs async/await"), confirming it was built to teach the team SPA
concepts by mirroring `CurrencyController`.

**Value to retain from it:**
- It proves the `AuthController` → `CurrencyController` → SPA path works end to end.
- Its `LoginResponse` shape (`token, username, userId, tenantId, role`) is the de-facto
  contract already implemented server-side.

**Value to retain — reversed on 2026-08-20.** This section previously read *"none of the code.
The user's decision is React; the Angular project should be archived, not converted."*
[ADR-007](../decisions/ADR-007-angular-stack.md) **reversed that**: Angular is the decision, and
the pilot's auth service, route guard and HTTP interceptor become the starting point rather than
landfill. `M2-C11` changed from *archive* to *adopt*.

Three qualifications, all still true and none softened by the reversal:

- **The pilot is Angular 19.2; the target is 22.x** — three majors apart. ADR-007 recommends
  `ng new` on 22 with the ~500 lines of auth wiring ported across, rather than three chained
  `ng update` migrations to preserve scaffolding the CLI regenerates for free.
- **Its `localStorage` JWT is XSS-exposed and must not be copied.** Token storage is `M2-C02`'s
  decision, against ADR-004.
- **Its `dist/` and `.angular/cache/` are committed build output** and should still be removed
  (risk R-14). Adopting the pilot's *code* does not mean adopting its *repository hygiene*.
