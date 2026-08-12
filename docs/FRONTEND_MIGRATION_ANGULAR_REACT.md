# Frontend Migration Plan: Blazor → Angular or React
### Keeping the existing backend business logic intact

---

## Direct Answer

**Yes — 95% of the business logic can be kept exactly as-is.**

The architecture was already designed with clean layer separation:

```
What you KEEP (zero changes needed)
────────────────────────────────────────────────────────
  Data/              ← EF Core entities — 100% pure C#
  Repository/        ← Repository + UnitOfWork — 100% pure C#
  BusinessLayer/     ← Business services — ~95% pure C#
  ViewModels/        ← DTOs — 100% pure C#
  Mappings/          ← AutoMapper profiles — 100% pure C#

What you ADD (new project in the same solution)
────────────────────────────────────────────────────────
  V.SMART.Api/       ← New ASP.NET Core Web API project
                       (thin controllers that call existing services)

What you REPLACE
────────────────────────────────────────────────────────
  V.SMART.Web/       ← Blazor Server host  →  Angular or React SPA
  V.SMART.Shared/Pages/      ← Razor pages  →  Angular/React components
  CustomAuthStateProvider    ← Blazor auth  →  JWT Bearer tokens
  CurrentUserService (partial) ← Blazor claims → IHttpContextAccessor claims
```

---

## The 5 Small Blazor Dependencies to Fix in Business Services

These are the only things in the business layer that reference Blazor. All are quick fixes.

### Fix 1 — Remove `using static MudBlazor.Icons` (17 files)
These are just icon name string constants. None of the actual business logic uses them — they were accidentally imported alongside other things.
```csharp
// DELETE these lines from the affected service files:
using static MudBlazor.Icons;
using MudBlazor;
using MudBlazor.Interfaces;
```
These lines appear in: `EnquirySalesService.cs`, `StockManagerService.cs`, `LabourDcOutgoingService.cs`, `PurchaseGRNService.cs`, `ProductionLogService.cs`, and 12 others. None of the *logic* in those files uses any MudBlazor type.

### Fix 2 — Replace `IBrowserFile` with `IFormFile`
One interface method in `ICompanyService` accepts `IBrowserFile` (Blazor file upload type).

```csharp
// BEFORE — Blazor-specific
using Microsoft.AspNetCore.Components.Forms;
Task<(bool success, string filePath, string fileUrl)> UploadFileAsync(IBrowserFile file, ...);

// AFTER — Web API compatible
using Microsoft.AspNetCore.Http;
Task<(bool success, string filePath, string fileUrl)> UploadFileAsync(IFormFile file, ...);
```

### Fix 3 — Replace `CurrentUserService` with an interface

`CurrentUserService` currently depends on `AuthenticationStateProvider` (Blazor-specific).
Create `ICurrentUserService` with two implementations:

```
ICurrentUserService
 ├── BlazorCurrentUserService  ← uses AuthenticationStateProvider (existing)
 └── ApiCurrentUserService     ← uses IHttpContextAccessor (new, for Web API)
```

```csharp
// New interface — ICurrentUserService.cs (add to Services/)
public interface ICurrentUserService
{
    Task<string> GetUsernameAsync();
    Task<int> GetUserIdAsync();
}

// New Web API implementation — ApiCurrentUserService.cs
public class ApiCurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _http;
    public ApiCurrentUserService(IHttpContextAccessor http) => _http = http;

    public Task<string> GetUsernameAsync()
        => Task.FromResult(_http.HttpContext?.User?.Identity?.Name ?? string.Empty);

    public Task<int> GetUserIdAsync()
    {
        var claim = _http.HttpContext?.User?.FindFirst("UserId")?.Value;
        return Task.FromResult(int.TryParse(claim, out var id) ? id : 0);
    }
}
```

Register in `V.SMART.Api/Program.cs`:
```csharp
builder.Services.AddScoped<ICurrentUserService, ApiCurrentUserService>();
```

### Fix 4 — Remove `Microsoft.AspNetCore.Components` from services
Two services import `Microsoft.AspNetCore.Components` for `NavigationManager` or `ComponentBase`. Remove these — business services must never navigate or depend on UI components.

### Fix 5 — ReportService (skip for Web API)
`ReportService` uses FastReport to generate PDFs server-side. This **still works** in a Web API — the controller calls `ReportService` and returns `File(pdfBytes, "application/pdf")`. No changes needed.

---

## New Project: V.SMART.Api

Add a new ASP.NET Core Web API project to the solution. It references `V.SMART.Shared` and adds nothing but thin API controllers.

### Project structure
```
V.SMART.Api/
├── Program.cs                    ← JWT auth, CORS, DI registration (reuse from V.SMART.Web)
├── Controllers/
│   ├── AuthController.cs         ← POST /api/auth/login  →  returns JWT token
│   ├── SalesEnquiryController.cs ← GET/POST /api/sales/enquiry
│   ├── PurchaseOrderController.cs
│   └── ... (one controller per module)
├── V.SMART.Api.csproj
└── appsettings.json
```

### Example controller (Sales Enquiry)

```csharp
[ApiController]
[Route("api/sales/enquiry")]
[Authorize]
public class SalesEnquiryController : ControllerBase
{
    private readonly IEnquirySalesService _service;

    public SalesEnquiryController(IEnquirySalesService service)
        => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tenantId = GetTenantId();
        var result = await _service.GetAllAsync(tenantId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
        => Ok(await _service.GetByIdAsync(id));

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SalesEnquiryViewModel vm)
    {
        var userId = GetUserId();
        await _service.SaveAsync(vm, userId);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.CancelAsync(id, GetUserId());
        return Ok();
    }

    private int GetTenantId()
        => int.Parse(User.FindFirst("TenantId")?.Value ?? "0");

    private int GetUserId()
        => int.Parse(User.FindFirst("UserId")?.Value ?? "0");
}
```

### Authentication — switch from circuit auth to JWT
```csharp
// V.SMART.Api/Program.cs — add JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
```

### Multi-tenancy — resolve from JWT claim instead of hostname
The `TenantProvider` currently resolves the tenant from the HTTP Host header. For the API, also support a `TenantId` claim in the JWT token:

```csharp
// Update TenantProvider.cs — add claim-based resolution
// ✅ 3. API mode (from JWT claim)
var tenantIdClaim = _httpContextAccessor?.HttpContext?.User?.FindFirst("TenantId")?.Value;
if (!string.IsNullOrEmpty(tenantIdClaim) && int.TryParse(tenantIdClaim, out var tenantId))
{
    _cached = _masterDb.Tenants.FirstOrDefault(t => t.Id == tenantId);
    if (_cached != null) return _cached;
}
```

---

## Angular vs React — Recommendation for This Project

| Factor | Angular | React |
|---|---|---|
| Language | TypeScript (strongly typed) | JavaScript/TypeScript |
| Coming from C# | **Better fit** — DI, decorators, modules, services pattern mirrors C# | Steeper mental leap |
| Built-in HTTP | `HttpClient` module (built-in, no extra libraries) | `fetch` or `axios` (manual setup) |
| Built-in routing | `RouterModule` (built-in) | `react-router-dom` (external) |
| Forms | `ReactiveFormsModule` — very similar to data annotations | Manual or `react-hook-form` (external) |
| State management | Services (singleton/scoped) — identical concept to C# DI | `useState`, Redux, Zustand, etc. |
| Learning curve | Higher upfront, then very productive | Lower upfront, complexity grows |
| UI component libraries | **PrimeNG** or **Angular Material** (enterprise-grade, MudBlazor equivalent) | Ant Design, Material UI |
| Long-term maintainability | Google-backed, opinionated = consistent team code | More freedom = more inconsistency risk |

**Recommendation: Angular + PrimeNG**

Reasons specific to your situation:
- C# developer — Angular's TypeScript + DI + services pattern maps 1:1 to what you already know
- PrimeNG provides the same rich data grid, forms, dialogs, and charts that MudBlazor gives you today
- Angular's `HttpClient` + interceptors handle JWT tokens and tenant headers exactly like ASP.NET Core middleware
- Large ERP with many modules benefits from Angular's module/lazy-loading system

---

## Migration Execution Plan

### Phase 1 — API Layer (2–3 weeks, no frontend changes)
1. Create `V.SMART.Api` project in the solution
2. Apply Fix 1–5 from above in `V.SMART.Shared`
3. Add `AuthController` with JWT login endpoint
4. Add API controllers for **one module** (e.g., Sales Enquiry) end-to-end
5. Test with Postman/Swagger — confirm business logic works identically
6. Keep `V.SMART.Web` (Blazor) running in parallel — zero disruption to client

### Phase 2 — Angular Project Setup (1 week)
1. `ng new vsmart-erp --routing --style=scss`
2. Install PrimeNG: `npm install primeng primeicons primeflex`
3. Set up `HttpClient` with JWT interceptor and tenant header interceptor
4. Build Auth module (login page + route guards)
5. Build the shared layout (sidebar, topbar, breadcrumbs) matching current design

### Phase 3 — Module-by-Module Migration (ongoing)
Migrate one module at a time. Suggested order (simplest → most complex):
1. Masters (Company, Customer, Supplier, Item) — read-heavy, minimal business logic
2. Sales Enquiry → Sales PO → Sales DC → Sales Invoice (the main revenue flow)
3. Outsourcing (Purchase PO chain)
4. Planning + Production
5. HR, Inventory, Accounts, Reports

For each module:
1. Expose API endpoints in `V.SMART.Api`
2. Build Angular service (`enquiry.service.ts`) that calls the API
3. Build Angular component (`enquiry-list.component.ts` + `enquiry-form.component.ts`)
4. Remove the equivalent Blazor page from `V.SMART.Web` (or keep both during transition)

### Phase 4 — Decommission Blazor (after all modules migrated)
1. Remove `V.SMART.Web` project
2. Remove `V.SMART.Shared/Pages/` folder (Razor pages no longer needed)
3. Remove MudBlazor NuGet packages from `V.SMART.Shared.csproj`
4. `V.SMART.Shared` becomes a pure business logic library (no UI dependency)

---

## What the Final Architecture Looks Like

```
┌─────────────────────────────────────────────────────────┐
│  Angular SPA (browser)                                   │
│  Components → Services → HttpClient → API calls         │
└────────────────────────┬────────────────────────────────┘
                         │ HTTPS + JWT Bearer Token
┌────────────────────────▼────────────────────────────────┐
│  V.SMART.Api  (ASP.NET Core Web API — new project)      │
│  API Controllers → calls existing IXxxService           │
│  JWT Auth + CORS + Swagger                              │
└────────────────────────┬────────────────────────────────┘
                         │ (same DI injection as before)
┌────────────────────────▼────────────────────────────────┐
│  V.SMART.Shared  (unchanged — your business logic)      │
│  IBusinessService → Repository → UnitOfWork → EF Core  │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│  SQL Server  (unchanged — same schema, same data)       │
└─────────────────────────────────────────────────────────┘
```

The business logic, database schema, stored procedures, multi-tenancy, and AutoMapper mappings are **completely unchanged**. Only the presentation layer changes.

---

## Effort Estimate

| Task | Effort |
|---|---|
| Fix 5 Blazor dependencies in V.SMART.Shared | 1–2 days |
| Create V.SMART.Api with full DI + JWT + Swagger | 2–3 days |
| Add API controllers for all 15+ modules | 3–4 days (thin controllers, logic already written) |
| Angular project setup + auth + layout shell | 1 week |
| Migrate each module's UI (list + form) | 2–4 days per module |
| Total (15 modules) | ~12–16 weeks (one developer) |

*Note: The API work is fast because you're not writing business logic — just exposing what already exists. Most time is spent rebuilding the UI in Angular.*
