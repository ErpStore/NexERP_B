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
last_verified: 2026-08-12
dependencies: [KB-010, KB-013]
---

# Existing UI Architecture (As-Is)

> This document describes **what exists**. The proposed React architecture is in
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

| Component | Purpose | React equivalent |
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

**Value not to retain:** none of the code. The user's decision is React; the Angular
project should be archived, not converted. Its `dist/` and `.angular/cache/` are committed
build output and should be removed from the repo (risk R-14).
