# GitHub Copilot Instructions — V.SMART NexGen ERP

## Project Identity
This is **V.SMART NexGen ERP**, a multi-tenant, manufacturing-focused ERP system.
- Solution: `NexGen-ERP---2025-master.sln`
- Runtime: **.NET 9**, C# latest
- UI Framework: **Blazor Server** (interactive server-side rendering) + **MAUI Hybrid** (desktop)
- Component Library: **MudBlazor 8.x** — always use MudBlazor components, never raw HTML Bootstrap equivalents
- ORM: **Entity Framework Core 9** with SQL Server
- Reports: **FastReport.OpenSource** for PDF/print output
- Charts: **Blazor-ApexCharts**
- Auth: Custom `AuthenticationStateProvider` with Claims (no JWT/cookie middleware)

---

## Project Structure — Know These Paths

| Path | Purpose |
|---|---|
| `V.SMART.Shared/Data/` | EF Core entities, `ApplicationDbContext`, `MasterDbContext` |
| `V.SMART.Shared/Repository/` | Repository pattern implementations + `UnitOfWork.cs` |
| `V.SMART.Shared/BusinessLayer/BusinessService/` | Business logic services (IService + concrete) |
| `V.SMART.Shared/Pages/` | All Blazor `.razor` page components by module |
| `V.SMART.Shared/ViewModels/` | AutoMapper-mapped DTOs and view models |
| `V.SMART.Shared/Mappings/` | AutoMapper profile configurations |
| `V.SMART.Shared/Components/` | Reusable Blazor components (modals, grids, etc.) |
| `V.SMART.Shared/Authentication/` | `CustomAuthStateProvider` with Claims |
| `V.SMART.Web/Program.cs` | DI registration, middleware pipeline, Blazor Server host |
| `Existing Store Procedures/` | Legacy SQL Server stored procedures for reference |

---

## Business Modules in This ERP

1. **Sales & Labour** — Leads, Enquiry, Feasibility, Sales PO, DC, Invoice, Labour DC/GRN/SCN/Invoice, Performa Invoice, Credit Note, Export
2. **Outsourcing** — Purchase Enquiry, PO, GRN, SCN, Invoice, Subcontract DC-Out, Subcontract GRN/SCN/Invoice, Debit Note, Material Requisition
3. **Planning** — Estimation, Assembly Job Order, Component Route Card
4. **Production** — Daily Production Log, Production Component, Issue WO Assy, Return GRN Assy, SCN Assembly
5. **Inventory/Stock** — Stock Addition/Issuance, Inter-Store Transfer, Material Issue Note, Stock Issue Request, Tool Crib
6. **Accounts** — Full accounts module with cash flow, GST ITC, tax reports, account reports
7. **Human Resources** — Offer Letter, Appointment Letter, Attendance, Payroll, Staff Loan, Salary
8. **Maintenance** — Breakdown Maintenance, Calibration History, Maintenance Process, Maintenance Schedule
9. **Inspection** — Incoming Inspection, Final Inspection, Master Inspection
10. **Masters** — Company, Admin, General, Accounts Master, Inventory Master, HR Master, Screen Management, Rejection
11. **Reports** — Analysis, Track Reports, PO Track, Rating, Tax Details, GST ITC
12. **Dashboard** — KPI widgets using ApexCharts
13. **Settings & Utilities** — General Settings, E-Invoice API integration, Correspondence

---

## Architectural Patterns — Always Follow These

### Layer Order (strict, never skip layers)
```
Razor Page (.razor) → IBusinessService → BusinessService → IRepository → Repository → DbContext (EF Core)
```

### Dependency Injection Pattern
- All services registered as `Scoped` in `Program.cs`
- Always inject the **interface**, never the concrete class
- DI registration format in `Program.cs`:
  ```csharp
  builder.Services.AddScoped<IMyService, MyService>();
  builder.Services.AddScoped<IMyRepository, MyRepository>();
  ```

### Repository Pattern
- `Repository.cs` is the generic base repository
- `UnitOfWork.cs` aggregates all repositories and wraps `DbContext` transactions
- All database writes go through `UnitOfWork`, never direct `DbContext` saves in pages

### Business Service Pattern
```csharp
// Interface lives in: BusinessLayer/BusinessService/IBusinessService/
public interface IMyModuleService { Task<List<MyDto>> GetAllAsync(int tenantId); }

// Concrete lives in: BusinessLayer/BusinessService/MyModuleService/
public class MyModuleService : IMyModuleService { ... }
```

### Multi-Tenancy
- `ITenantProvider` resolves the current tenant from `IHttpContextAccessor`
- `ITenantDbContextFactory` creates the scoped `ApplicationDbContext` per tenant
- Every data query must filter by `TenantId` — never return cross-tenant data
- `TenantInfo` entity holds tenant configuration

### ViewModels / DTOs
- AutoMapper profiles live in `V.SMART.Shared/Mappings/`
- Create a `XxxViewModel` for every form/list, map from entity using AutoMapper
- Never expose raw EF entities to Razor pages

### Authentication
- `CustomAuthStateProvider` stores user Claims in-memory (Blazor Server circuit scope)
- Claims available: `ClaimTypes.Name`, `UserId`, `ClaimTypes.Role`, `IsQrLogin`, `IsProductionUser`
- Use `[Authorize]` and `<AuthorizeView>` / `<AuthorizationToggle>` component for UI guards

---

## Coding Standards — Enforce These on Every Suggestion

### C# Style
- Use `async/await` throughout — no `.Result` or `.Wait()`
- Prefer `record` types for immutable DTOs
- Use C# 12+ features: primary constructors, collection expressions
- All nullable reference types enabled (`<Nullable>enable</Nullable>`) — annotate properly
- Method names: `PascalCase`; local variables: `camelCase`; private fields: `_camelCase`

### Blazor / MudBlazor Style
- Use `MudDataGrid` for all tabular data (not `QuickGrid` or HTML tables)
- Use `MudDialog` wrapped in the existing `BsModal.razor` component for modals
- Use `MudForm` + `MudTextField`, `MudSelect`, `MudDatePicker` for all forms
- Always use `@inject` for services in pages, not constructor injection
- Lifecycle: use `OnInitializedAsync` to load data, `StateHasChanged()` only when needed
- Error handling: use `MudSnackbar` for user-facing messages; log to `ILogger`
- Loading states: show `MudProgressLinear` or `MudSkeleton` during async loads

### Razor Page Structure (follow this order)
```razor
@page "/module/feature"
@using V.SMART.Shared.ViewModels.MyModule
@inject IMyService MyService
@inject ISnackbar Snackbar
@inject NavigationManager Nav

<PageTitle>Feature Name</PageTitle>

<!-- MudBlazor layout here -->

@code {
    // 1. Parameters
    // 2. Injected services (already done via @inject)
    // 3. Private fields / state
    // 4. OnInitializedAsync
    // 5. Event handlers
    // 6. Helper methods
}
```

### EF Core / Database
- Use `AsNoTracking()` for all read-only queries
- Use `Include()` for eager loading — no lazy loading (it is disabled)
- Stored procedure calls: use `FromSqlRaw` or `ExecuteSqlRawAsync` with parameterized queries only
- Never use string interpolation in SQL — always use `SqlParameter`
- Migrations live in `V.SMART.Shared/Migrations/`

---

## Prompt Patterns for Common Tasks

When I ask about a module, analyze it using this mental model:
1. What are the EF Core entities? (look in `Data/<Module>/`)
2. What does the Repository do? (look in `Repository/<Module>Repository/`)
3. What business logic exists in the Service? (look in `BusinessLayer/BusinessService/<Module>Service/`)
4. What does the Razor page do? (look in `Pages/<Module>_Pages/`)

When I say "add a feature to [module]", generate:
- Entity change (if schema changes)
- Repository method (interface + implementation)
- Business service method (interface + implementation)
- ViewModel update
- AutoMapper profile update
- Razor page update (MudBlazor)
- DI registration in `Program.cs`

---

## What NOT to Generate
- Do NOT generate jQuery or vanilla JS — use Blazor interop via `IJSRuntime` sparingly
- Do NOT use `HttpClient` calls to self (this is Blazor Server, services are injected directly)
- Do NOT generate REST API controllers unless explicitly asked
- Do NOT use `Bootstrap` classes directly — use MudBlazor props/classes
- Do NOT use `Thread.Sleep` or blocking async patterns
- Do NOT store sensitive data (passwords, connection strings) in code — use `appsettings.json` + environment variables
- Do NOT call `DbContext` directly from Razor pages — always go through the service layer
- Do NOT generate cross-tenant queries — always filter by `TenantId`

---

## Security Requirements
- All inputs validated with DataAnnotations on ViewModels
- SQL: parameterized queries only — zero string interpolation in `FromSqlRaw`
- File uploads: validate extension + MIME type + size (see `MauiFileUploadService.cs` pattern)
- Role checks on every action that mutates data
- E-Invoice API credentials stored in encrypted settings, not plaintext

---

## Key NuGet Packages Reference
| Package | Use |
|---|---|
| `MudBlazor 8.x` | All UI components |
| `Microsoft.EntityFrameworkCore.SqlServer 9.x` | ORM + SQL Server |
| `AutoMapper 16.x` | Entity↔ViewModel mapping |
| `Blazored.LocalStorage` | Browser localStorage (settings persistence) |
| `FastReport.OpenSource` | PDF/print report generation |
| `Blazor-ApexCharts` | Dashboard charts |
| `ClosedXML` / `EPPlus` | Excel export/import |
| `BouncyCastle.Cryptography` | Encryption (E-Invoice) |
| `QRCoder` / `ZXing.Net` | QR code generation/scanning |
