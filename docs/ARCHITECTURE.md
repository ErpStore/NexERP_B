# V.SMART NexGen ERP — Architecture & Understanding Document

> **Template Instructions**: Use this document as a Copilot feed-in prompt. Paste sections into Copilot Chat with `@workspace` to get module-level analysis. Sections marked `[TODO: ANALYZE]` are prompts you run against the codebase.

---

## 1. Solution Overview

| Property | Value |
|---|---|
| Solution Name | NexGen-ERP---2025-master |
| Target Framework | .NET 9 |
| UI Technology | Blazor Server (web) + MAUI Hybrid (desktop) |
| Component Library | MudBlazor 8.x |
| ORM | Entity Framework Core 9 — Code First |
| Database | SQL Server (multi-tenant) |
| Authentication | Custom `AuthenticationStateProvider` (Claims-based, in-circuit) |
| Multi-tenancy | `ITenantProvider` + `ITenantDbContextFactory` per-request scoping |
| Reporting | FastReport.OpenSource + PDF Simple Export |
| Architecture Style | Layered: Repository Pattern + Unit of Work + Business Service Layer |

---

## 2. Project Structure

```
NexGen-ERP---2025-master.sln
├── V.SMART/                        # MAUI Hybrid host (desktop/mobile shell)
│   ├── App.xaml / MauiProgram.cs
│   └── Services/                   # Desktop-specific: FileOpener, PathProvider, FileUpload
│
├── V.SMART.Shared/                 # ★ Core library — all business logic lives here
│   ├── Authentication/             # CustomAuthStateProvider (Claims)
│   ├── BusinessLayer/
│   │   └── BusinessService/
│   │       ├── IBusinessService/   # Interfaces for all services
│   │       └── [ModuleService]/    # Concrete implementations
│   ├── Components/                 # Reusable MudBlazor Razor components
│   ├── Data/                       # EF Core entities + ApplicationDbContext
│   ├── E_Invoice/                  # GST E-Invoice API integration
│   ├── Layout/                     # App shell layout components
│   ├── Mappings/                   # AutoMapper profiles
│   ├── Migrations/                 # EF Core migration history
│   ├── Pages/                      # All Razor page components by module
│   ├── Repository/                 # Repository + UnitOfWork implementations
│   ├── Services/                   # Cross-cutting services (multi-company, report viewer)
│   ├── Shared/                     # Shared Blazor components
│   ├── Utility_Constants/          # Enums, constants, helper utilities
│   └── ViewModels/                 # AutoMapper DTOs / form view models
│
├── V.SMART.Web/                    # ASP.NET Core Blazor Server host (web deployment)
│   ├── Program.cs                  # DI registration + middleware pipeline
│   ├── Components/                 # Web-specific component overrides
│   └── Services/                   # Web-specific: WebFileOpener, WebPathProvider
│
└── Existing Store Procedures/      # Legacy SQL Server SPs (reference/migration guide)
    └── StoredProcedures/
        ├── Sp_Print_*.sql          # Print/label stored procedures
        └── Sp_Inv*.sql
```

---

## 3. Data Layer — Entities & DbContext

### DbContext Files
| File | Purpose |
|---|---|
| `Data/ApplicationDbContext.cs` | Main EF context — all transactional entities + Identity |
| `Data/MasterDbContext.cs` | Master/configuration data context |

### Module Entity Folders
| Folder | Key Entities |
|---|---|
| `Data/SalesAndLabour/` | SalesEnquiry, SalesPO, SalesDC, SalesInvoice, LabourDC, LabourGRN, LabourSCN, PerformaInvoice, CreditNote, Leads, Feasibility, ContractReview, Export |
| `Data/OutSourcing/` | PurchaseEnquiry, PurchasePO, PurchaseGRN, PurchaseSCN, Purchase_Invoice, SubContractDC, SubContractGRN, SubContractSCN, SubContractInvoice, Debit_Note |
| `Data/Planning/` | Estimation, AssyJobOrder, ComponentRouteCard |
| `Data/Production/` | DailyProductionLog, ProductionComponent, ProductionIssueWOAssy, ProductionReturnGrnAssy, ProductionSCNAssembly |
| `Data/Inventory(Stock)/` | StockAddition/Issuance, InterStoreTransfer, MaterialIssueNote, StockIssueRequest, ToolCrib |
| `Data/AccountsModule/` | [TODO: ANALYZE] Ledger, Journal, Payments, Receipts |
| `Data/HumanResource/` | OfferLetter, AppointmentLetter, Attendance, Salary, StaffLoan |
| `Data/Maintenance/` | BreakdownMaintenance, CalibrationHistory, MaintenanceProcess, MaintenanceSchedule |
| `Data/Inspection/` | IncomingInspection, FinalInspection, MasterInspection |
| `Data/Master/` | Company, Admin, General, Accounts, Inventory, HR Masters, ScreenManagement, Rejection |
| `Data/ServiceBills/` | ServiceBills, ServiceBillsSub |

### Multi-Tenancy Pattern
```
TenantInfo (entity) ─→ ITenantProvider ─→ ITenantDbContextFactory
                                              └─→ ApplicationDbContext (per-tenant connection string)
```
Every entity query must include `.Where(x => x.TenantId == tenantId)`.

---

## 4. Repository Layer

### Pattern
- `Repository.cs` — generic base: `GetAll`, `GetById`, `Add`, `Update`, `Delete`
- `UnitOfWork.cs` — aggregates all module repositories, wraps `SaveChangesAsync`
- All repos implement interfaces in `IRepository/`

### Module Repositories (verified)
AccountsRepository, HumanResourceRepository, InspectionRepository, InventoryStockRepository, MaintenanceRepository, MasterRepository, OutSourcingRepository, PlanningRepository, ProductionRepository, ReportRepository, SalesAndLabourRepository, ServiceBillsRepository, SettingsRepository, UtilitiesRepository

---

## 5. Business Service Layer

### Pattern
```csharp
// Interface: IBusinessService/IModuleService/IXxxService.cs
public interface ISalesService {
    Task<List<SalesEnquiryViewModel>> GetEnquiriesAsync(int tenantId);
    Task<bool> SaveEnquiryAsync(SalesEnquiryViewModel vm, int userId);
}

// Concrete: ModuleService/XxxService.cs
public class SalesService : ISalesService {
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    // ...
}
```

### Service Modules (verified)
AccountsService, DashboardService, EInvoiceAPIService, HumanResourceService (+ Attendance, EmployeeLoan, Payroll sub-services), InspectionService, InventoryService, LabourServices, LeadService, MaintenanceService, MasterService (+ AccountsService, AdminService, GeneralService, HRMasterService, InventoryService sub-services), OutSourcingService, PlanningService, ProductionService, ReportService (+ AccountReport, AnalysisReport, GSTITC, POTrack, Rating, TaxDetails, TrackReport), SalesService, ServiceBillService, SettingsService

---

## 6. Authentication & Authorization

### Mechanism
- **No ASP.NET Core cookie/JWT middleware** — Blazor Server circuit-scoped in-memory auth
- `CustomAuthStateProvider` stores `ClaimsPrincipal` in circuit memory
- Login sets claims; logout clears them; circuit disconnect = auto-logout

### Claims Available
| Claim | Type |
|---|---|
| Username | `ClaimTypes.Name` |
| UserId | Custom: `"UserId"` |
| Role | `ClaimTypes.Role` |
| QR Login flag | Custom: `"IsQrLogin"` |
| Production user flag | Custom: `"IsProductionUser"` |

### Authorization Pattern in Pages
```razor
<AuthorizeView Roles="Admin,Manager">
    <Authorized><!-- admin content --></Authorized>
    <NotAuthorized><!-- redirect or message --></NotAuthorized>
</AuthorizeView>
```
Or use the existing `<AuthorizationToggle>` component.

---

## 7. UI Layer — MudBlazor Patterns

### Standard Page Template
```razor
@page "/sales/enquiry"
@inject ISalesService SalesService
@inject ISnackbar Snackbar

<MudText Typo="Typo.h5">Sales Enquiry</MudText>

<MudDataGrid T="SalesEnquiryViewModel" Items="@_enquiries" Loading="@_loading">
    <Columns>
        <PropertyColumn Property="x => x.EnquiryNo" Title="Enquiry No" />
        <PropertyColumn Property="x => x.CustomerName" Title="Customer" />
        <TemplateColumn>
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Edit" OnClick="@(() => Edit(context.Item))" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>

@code {
    private List<SalesEnquiryViewModel> _enquiries = new();
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        _enquiries = await SalesService.GetEnquiriesAsync(TenantId);
        _loading = false;
    }
}
```

### Reusable Components Available
| Component | Purpose |
|---|---|
| `BsModal.razor` | MudDialog wrapper for all modals |
| `MasterModal.razor` | Master data selection modal |
| `DetailsModal.razor` | Detail record popup |
| `CustomerSelection.razor` | Customer picker component |
| `ColumnMenu.razor` | Grid column visibility toggle |
| `ExcelUpload.razor` | Excel file upload handler |
| `ExportInvModel.razor` | Invoice export modal |
| `EwayDcListModel.razor` | E-way bill DC list |
| `CorrespondenceStatus.razor` | Status indicator |
| `AuthorizationToggle.razor` | Role-based visibility toggle |

---

## 8. Reporting Architecture

- **FastReport.OpenSource** generates PDF/print reports
- Report templates (.frx files) are in `wwwroot/` or `Resources/`
- `ReportViewerService` in `V.SMART.Shared/Services/ReportViewer/` handles rendering
- Legacy stored procedures in `Existing Store Procedures/` feed report data
- Pattern: Service calls SP → populates DataTable → passes to FastReport template → renders PDF

### Existing Print SPs
| SP | Report |
|---|---|
| `Sp_Print_CompanyDetails` | Company header |
| `Sp_InvDetailsLabelPrint` | Inventory label |
| `Sp_Print_GRNDetailsLabelPrint` | GRN label |
| `Sp_Print_LabourDC/GRN/Inv/SCN` | Labour module documents |
| `Sp_Print_MFGDC/MfgInv/MfgQuote` | Manufacturing documents |
| `Sp_Print_PurchaseOrder` | Purchase order |
| `Sp_Print_PerformaInvoice` | Proforma invoice |
| `Sp_Print_SubConDcOut` | Subcontract DC |

---

## 9. Key Business Workflows

### Sales Order Flow
```
Lead → Sales Enquiry → Feasibility → Contract Review → Sales PO → Sales DC → Sales Invoice
                                                                          ↓
                                                              Proforma Invoice (pre-billing)
                                                                          ↓
                                                                  Credit Note (returns)
```

### Purchase / Outsourcing Flow
```
Material Requisition → Purchase Enquiry → Purchase PO → Purchase GRN → Purchase Invoice
                                       → Subcontract PO → SubContract DC Out → SubContract GRN → SubContract Invoice
                                                                                               ↓
                                                                                          Debit Note
```

### Production Flow
```
Assembly Job Order → Component Route Card → Daily Production Log
                                          → Production Component Issue (WO/Assy)
                                          → Production SCN Assembly
                                          → Production Return GRN Assembly
```

### HR Flow
```
Lead → Offer Letter → Appointment Letter → Attendance → Payroll/Salary → Staff Loan
```

---

## 10. Database Strategy

### Current State
- SQL Server (version: [TODO: confirm from connection string])
- EF Core Code-First migrations in `V.SMART.Shared/Migrations/`
- Legacy stored procedures for reports (not for CRUD — CRUD is EF Core)
- Multi-tenant: separate connection strings per tenant OR shared DB with TenantId column (verify `TenantDbContextFactory`)

### Recommended Migration Path
1. **Keep EF Core Code-First** — do not migrate to raw SQL
2. **Migrate legacy SPs** to EF Core + FastReport queries where possible
3. **Add indexes** on `TenantId`, `CreatedDate`, foreign keys (verify in `ApplicationDbContext.OnModelCreating`)
4. **Add soft delete** pattern: `IsDeleted` flag + global query filter in DbContext
5. **Audit trail**: Add `CreatedBy`, `CreatedDate`, `ModifiedBy`, `ModifiedDate` base entity

### Hosting Options (Cost-Optimized for SMB)

| Tier | Option | Monthly Cost (est.) | Best For |
|---|---|---|---|
| **Starter** | Single VPS (Hetzner/DigitalOcean 4GB) + SQL Server Express or PostgreSQL | $20–40 | Single tenant, <20 users |
| **Growth** | Azure App Service B2/B3 + Azure SQL Basic/Standard | $80–150 | Multi-tenant, 20–100 users |
| **Scale** | Azure App Service + Azure SQL General Purpose + Azure Blob Storage | $200–400 | Multi-tenant, 100+ users, SLA needed |
| **On-Premise** | Windows Server + SQL Server Standard (one-time) | Hardware cost | Client prefers own infra |

**Recommendation for this client**: Azure App Service (B2) + Azure SQL Standard S2 ($150–180/month) gives managed patching, auto-backup, SSL, and scales without DevOps overhead.

---

## 11. Copilot Analysis Prompts

Copy-paste these into Copilot Chat with `@workspace`:

### Module Analysis
```
@workspace Analyze the Sales & Labour module. List:
1. All EF Core entities in Data/SalesAndLabour/ with their key properties
2. The repository methods available in SalesAndLabourRepository
3. The business service methods in LabourServices and SalesService
4. The Razor pages in Pages/SalesAndLabour_pages/
5. Identify any business rules embedded in the service layer
```

### Business Logic Extraction
```
@workspace In the [ModuleName]Service.cs, identify all business validation rules,
calculations, and state transitions. Express them as plain English business rules
I can document, then suggest how to unit-test each rule.
```

### New Feature Scaffolding
```
@workspace Following the exact patterns in this codebase (Repository + UnitOfWork +
BusinessService + MudBlazor Razor page), scaffold a complete [FeatureName] feature for
the [ModuleName] module. Include: entity, repository interface + impl, service interface
+ impl, ViewModel, AutoMapper mapping, Razor page with MudDataGrid and MudForm,
and Program.cs DI registrations.
```

### Database Schema Understanding
```
@workspace Analyze ApplicationDbContext.cs and list all DbSet properties grouped
by business module. For each module, identify the primary aggregate root entity
and its child/detail entities. Show the relationship pattern (1:1, 1:N, M:N).
```

### UI Modernization
```
@workspace Review the Razor page at Pages/[Module]_Pages/[Page].razor.
Identify any non-MudBlazor HTML, any direct DbContext calls bypassing services,
any missing loading states, and any missing authorization checks.
Suggest the corrected version following our coding standards.
```

---

## 12. Identified Technical Debt & Improvement Areas

> Fill this section as you analyze modules with Copilot.

| Item | Location | Priority | Notes |
|---|---|---|---|
| Inconsistent namespace (IQSMART vs V.SMART) | `Program.cs` lines 6-9 | High | Some using directives reference `IQSMART.Shared.*` — likely renamed project |
| Custom AuthStateProvider not persisted | `Authentication/` | Medium | Circuit disconnect = logout; consider adding DB session persistence |
| [TODO: ANALYZE] Direct DbContext in pages? | `Pages/` | High | Run Copilot scan prompt |
| [TODO: ANALYZE] Missing soft delete? | `Data/` | Medium | Check base entity pattern |
| [TODO: ANALYZE] Missing audit fields? | `Data/` | Medium | Check `CreatedBy/Date` on entities |
| [TODO: ANALYZE] N+1 query risks | `Repository/` | High | Check for missing `Include()` |

---

## 13. Security Checklist

- [ ] SQL: All `FromSqlRaw` calls use `SqlParameter` (no string interpolation)
- [ ] Auth: All mutation pages have `[Authorize]` or `<AuthorizeView>`
- [ ] Multi-tenant: All queries filter by `TenantId`
- [ ] File upload: Extension + MIME + size validation in `MauiFileUploadService` / `WebFileUploadService`
- [ ] Secrets: Connection strings in `appsettings.json` (not committed) or Azure Key Vault
- [ ] E-Invoice API keys: Encrypted storage via `BouncyCastle.Cryptography`
- [ ] HTTPS: Enforced in `Program.cs` (`UseHttpsRedirection`, HSTS)
- [ ] Error details: `DetailedErrors = true` only in Development environment

---

*Last updated: 2026-08-01 | Analyzed by: GitHub Copilot + Senior Architect review*
