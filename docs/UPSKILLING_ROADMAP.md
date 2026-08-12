# V.SMART NexGen ERP — Developer Upskilling Roadmap
### For: Developer transitioning from WPF/Desktop → Blazor Server Web Development

---

## How to Use This Document
Work through the phases **in order**. Each phase builds on the previous one. After every concept, there is a **"Verify in THIS repo"** section — go find that exact code in the codebase to cement understanding. Do not skip to Phase 3 if Phase 1 is unclear.

---

# PHASE 0 — Mental Model Reset (1–2 days)
> The single most important shift from WPF to web.

## The Fundamental Difference

| WPF Mental Model | Blazor Server Mental Model |
|---|---|
| App runs entirely on the user's machine | UI renders in the browser; logic runs on the SERVER |
| `Window` object lives in memory as long as the app runs | A "circuit" (SignalR WebSocket connection) lives as long as the tab is open |
| UI updates happen via `Dispatcher.Invoke` + data binding | UI updates happen via `StateHasChanged()` after async work |
| User navigates between `Window`s / `Page`s via code | User navigates between URL routes (e.g., `/sales/enquiry`) |
| One instance per user — no isolation needed | Hundreds of users share one server — every user gets a **Scoped** service instance |
| App crash = one user affected | Server crash = all users affected — stability is critical |

## What is a "Circuit"?
When a browser opens your Blazor app, it opens a **persistent WebSocket connection** (SignalR) to the server. That connection is called a **circuit**. Your services, state, and UI updates all travel through it. When the user closes the tab → circuit closes → all `Scoped` service state is lost. This is why the custom `CustomAuthStateProvider` stores the user's claims in memory and they disappear on page refresh or tab close.

---

# PHASE 1 — Web & HTTP Fundamentals (3–5 days)
> You cannot debug web problems without understanding how HTTP works.

## 1.1 HTTP Basics

Every web request has:
- **URL**: `https://yourapp.com/sales/enquiry` — identifies the resource
- **Method**: GET (read), POST (create/submit), PUT (update), DELETE (remove)
- **Headers**: metadata — content type, auth tokens, cookies
- **Body**: the data payload (on POST/PUT)
- **Status Code**: 200 OK, 404 Not Found, 401 Unauthorized, 500 Server Error

**In Blazor Server**, your pages don't make HTTP calls to themselves — they call C# services directly (because everything runs on the server). HTTP is relevant for:
- The initial page load (GET request for the HTML shell)
- SignalR connection for UI updates
- External API calls (E-Invoice API in this repo)
- File uploads/downloads

## 1.2 How Blazor Server Actually Works

```
Browser                     Server (ASP.NET Core)
  |                                |
  |--- GET /sales/enquiry -------> |
  |<-- HTML shell (empty) -------- |   ← First load: just a shell
  |                                |
  |--- WebSocket (SignalR) ------> |   ← Circuit opens
  |<-- Rendered HTML diff -------- |   ← Server renders component, sends diff
  |                                |
  |--- User clicks button -------> |   ← Event travels over WebSocket
  |<-- HTML diff (updated UI) ---- |   ← Server handles event, re-renders, sends diff
```

**Key insight**: The browser never has your C# code. It only has the rendered HTML output and the SignalR connection. This is totally different from React/Angular where JavaScript runs in the browser.

## 1.3 What is HTML/CSS at a minimum level?
You don't need to be a CSS expert, but you must recognize:

```html
<!-- div = a block container (like a Grid row in WPF) -->
<div class="row">
  <!-- span = inline text (like a TextBlock) -->
  <span>Hello</span>
  
  <!-- button with a click event -->
  <button onclick="...">Click me</button>
  
  <!-- input field (like a TextBox) -->
  <input type="text" value="..." />
</div>
```

In this repo, raw HTML is minimal because **MudBlazor** provides pre-built components. But you'll still see HTML in `.razor` files.

**Practice**: Open `Pages/SalesAndLabour_pages/SalesEnquiry_Pages/EnquirySalesList.razor` and identify every HTML element vs every MudBlazor component (`<Mud*>`).

---

# PHASE 2 — C# for Web (3–4 days)
> You know C# from WPF. Here is what changes for web.

## 2.1 async/await — Non-negotiable
In WPF you could sometimes get away with synchronous code. In Blazor Server, **synchronous database calls block the entire circuit** (and can block other users on the same thread pool). Every DB or I/O operation must be `async`.

```csharp
// ❌ WPF style — blocks the UI thread
var data = _db.SalesEnquiries.ToList();

// ✅ Blazor Server style — yields the thread back while waiting
var data = await _db.SalesEnquiries.ToListAsync();
```

**Rule**: If a method does any I/O (database, file, network), its return type is `Task<T>` or `Task`, and you `await` it.

## 2.2 Dependency Injection (DI)
In WPF you likely used `new MyService()` or a simple IoC container. In ASP.NET Core, DI is built-in and everything goes through it.

```csharp
// Program.cs — registering a service
builder.Services.AddScoped<ISalesService, SalesService>();

// In a Blazor page — receiving the service (NOT new-ing it up)
@inject ISalesService SalesService
```

Three lifetimes you must understand:
| Lifetime | Created | Destroyed | Use For |
|---|---|---|---|
| `Singleton` | App startup | App shutdown | Config, caches, thread-safe shared state |
| `Scoped` | Per browser circuit | Circuit closes (tab closed) | DbContext, services needing per-user state |
| `Transient` | Every time injected | When the consumer is disposed | Lightweight stateless utilities |

**In this repo**: All business services and repositories are `Scoped`. This means each user's browser tab gets its own instances — critical for multi-tenancy.

## 2.3 Interfaces and Why They Matter Here
Every service in this repo has an interface (`IXxxService`) and a concrete class (`XxxService`). This enables:
1. **Testability** — you can mock the interface in unit tests
2. **DI** — the DI container resolves `ISalesService` → `SalesService` without the page knowing about the concrete class
3. **Swap implementations** — you can add a `MockSalesService` for development without changing pages

```csharp
// The page only knows about the interface — not the concrete class
@inject ISalesService SalesService  // ← interface

// The DI container handles: ISalesService → new SalesService(unitOfWork, mapper)
```

## 2.4 Expression Trees (used in Repository)
The generic `Repository<T>` uses `Expression<Func<T, bool>>` for type-safe queries. This is how LINQ where-clauses become SQL.

```csharp
// This C# lambda expression...
Expression<Func<SalesEnquiry, bool>> predicate = x => x.TenantId == 1 && x.IsActive;

// ...gets compiled into SQL by EF Core:
// WHERE TenantId = 1 AND IsActive = 1
```

**Verify in repo**: Open `Repository/IRepository/IRepository.cs` — look at `GetAllAsync(Expression<Func<T, bool>> predicate)`.

---

# PHASE 3 — Blazor Fundamentals (1–2 weeks)
> This is the core skill. Spend the most time here.

## 3.1 What is a Razor Component?
A `.razor` file = HTML template + C# logic in one file. Think of it like a WPF `UserControl` where the XAML and code-behind are merged.

```razor
<!-- WPF equivalent: UserControl with TextBlock and Button -->
@* This is a Blazor component *@

<h3>Hello, @_name!</h3>              @* HTML + C# expression *@
<button @onclick="SayHello">Click</button>

@code {
    private string _name = "World";   // state (like ViewModel properties)
    
    private void SayHello()           // event handler (like ICommand)
    {
        _name = "Blazor";
        // StateHasChanged() is called automatically after events
    }
}
```

## 3.2 Component Lifecycle — The Most Important Thing to Learn

```
OnInitialized()           ← Called once on creation (sync, before render)
OnInitializedAsync()      ← Called once on creation (async) — PUT YOUR DB CALLS HERE
OnParametersSet()         ← Called when parent passes new [Parameter] values
OnParametersSetAsync()    ← Async version
ShouldRender()            ← Return false to skip re-render (performance optimization)
OnAfterRender(firstRender)← After the component renders to the DOM — PUT JS INTEROP HERE
OnAfterRenderAsync()      ← Async version
Dispose()                 ← Called when component is removed — PUT CLEANUP HERE
```

**The golden rule**: Load data in `OnInitializedAsync`. Do JS interop in `OnAfterRenderAsync`. Everything else is event handlers.

```csharp
protected override async Task OnInitializedAsync()
{
    _isLoading = true;
    _enquiries = await EnquiryService.GetAllAsync(TenantId);
    _isLoading = false;
    // No need to call StateHasChanged() — lifecycle methods trigger re-render automatically
}
```

## 3.3 Parameters — How Components Talk to Each Other

```razor
@* Parent passes data down via [Parameter] *@
<CustomerSelection CustomerId="@_selectedId" 
                   OnCustomerSelected="HandleCustomerSelected" />

@* Child component *@
@code {
    [Parameter] public int CustomerId { get; set; }          // data in
    [Parameter] public EventCallback<int> OnCustomerSelected { get; set; }  // event out
    
    private async Task SelectCustomer(int id)
    {
        await OnCustomerSelected.InvokeAsync(id);  // notify parent
    }
}
```

**WPF equivalent**: `[Parameter]` = `DependencyProperty`. `EventCallback` = routed event.

**Verify in repo**: Open `Components/CustomerSelection.razor` and find its `[Parameter]` properties.

## 3.4 Two-Way Data Binding

```razor
@* One-way binding (read only) *@
<p>@_name</p>

@* One-way event binding *@
<button @onclick="Save">Save</button>

@* Two-way binding — @bind is shorthand for value= + @onchange= *@
<MudTextField @bind-Value="_name" Label="Name" />

@* Equivalent long form: *@
<MudTextField Value="_name" ValueChanged="(v) => { _name = v; }" Label="Name" />
```

## 3.5 StateHasChanged — When to Call It

After **lifecycle methods and event handlers**, Blazor automatically re-renders. You only need to call `StateHasChanged()` manually when:
- You update state from a **background Task** (e.g., a timer callback or SignalR message)
- You update state from inside a `Task.Run()`
- You need to force a re-render mid-method (e.g., show a loading spinner before a long operation)

```csharp
// Example: loading spinner pattern
private async Task LoadData()
{
    _isLoading = true;
    StateHasChanged();           // force render NOW to show spinner
    
    _data = await Service.GetDataAsync();
    
    _isLoading = false;
    // StateHasChanged() called automatically when method returns
}
```

## 3.6 Routing in Blazor

```razor
@page "/sales/enquiry"          ← This component responds to this URL
@page "/sales/enquiry/{Id:int}" ← Route parameter (like query string but in the URL)

@code {
    [Parameter] public int Id { get; set; }  ← Bound from URL automatically
}
```

Navigate programmatically:
```csharp
@inject NavigationManager Nav

Nav.NavigateTo("/sales/enquiry");
Nav.NavigateTo($"/sales/enquiry/{id}");
```

**WPF equivalent**: `NavigationService.Navigate(new EnquiryPage(id))` → `Nav.NavigateTo($"/sales/enquiry/{id}")`

## 3.7 Cascading Parameters — App-Wide Data

For data that every component needs (current user, theme), Blazor uses cascading values instead of passing them down every component tree.

```razor
@* In MainLayout.razor — inject once at the top *@
<CascadingValue Value="@_currentUser">
    @Body
</CascadingValue>

@* In any deep child component — receive it *@
[CascadingParameter] private UserSession CurrentUser { get; set; }
```

**Verify in repo**: Look at `Layout/MainLayout.razor` — it injects `UserSession` which is a `Scoped` singleton per circuit, effectively acting as cascading state.

---

# PHASE 4 — This Repo's Architecture (2–3 days)
> Read this section while having the actual code open side by side.

## The 5-Layer Architecture

```
┌─────────────────────────────────────────────────────┐
│  LAYER 1: Razor Pages  (.razor files)               │
│  Location: V.SMART.Shared/Pages/                    │
│  Role: UI only — display data, capture user input   │
│  Never: direct DB calls, business logic             │
└──────────────────────┬──────────────────────────────┘
                       │ calls via @inject IXxxService
┌──────────────────────▼──────────────────────────────┐
│  LAYER 2: Business Service  (IXxxService/XxxService) │
│  Location: BusinessLayer/BusinessService/           │
│  Role: Business rules, validation, orchestration    │
│  Never: direct DbContext, UI concerns               │
└──────────────────────┬──────────────────────────────┘
                       │ calls via IUnitOfWork
┌──────────────────────▼──────────────────────────────┐
│  LAYER 3: Unit of Work  (UnitOfWork.cs)             │
│  Location: Repository/UnitOfWork.cs                 │
│  Role: Groups all repos, wraps SaveChangesAsync     │
│  Think of it as: the "transaction boundary"         │
└──────────────────────┬──────────────────────────────┘
                       │ exposes IXxxRepository
┌──────────────────────▼──────────────────────────────┐
│  LAYER 4: Repository  (IXxxRepository/XxxRepository) │
│  Location: Repository/[Module]Repository/           │
│  Role: Data access — CRUD + complex queries         │
│  Never: business logic, SaveChanges (UoW handles)   │
└──────────────────────┬──────────────────────────────┘
                       │ uses EF Core DbContext
┌──────────────────────▼──────────────────────────────┐
│  LAYER 5: Data / EF Core  (ApplicationDbContext)    │
│  Location: Data/ApplicationDbContext.cs             │
│  Role: EF Core entities + DbContext configuration   │
│  Think of it as: the "database API"                 │
└─────────────────────────────────────────────────────┘
```

## 4.1 Layer 5 — EF Core (the Database API)

Entity Framework Core maps C# classes to database tables. You write C# — EF generates SQL.

```csharp
// Data/SalesAndLabour/SalesEnquiry/SalesEnquiry.cs
public class SalesEnquiry                      // ← maps to "SalesEnquiries" table
{
    public int Id { get; set; }               // ← Primary Key (auto-increment)
    public int TenantId { get; set; }         // ← Multi-tenant filter column
    public string EnquiryNo { get; set; }
    public DateTime EnquiryDate { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; }    // ← Navigation property (FK relationship)
    public List<SalesEnquirySub> Items { get; set; } = new(); // ← 1:N child rows
}

// ApplicationDbContext.cs
public DbSet<SalesEnquiry> SalesEnquiries { get; set; }  // ← makes this table queryable
```

**LINQ queries become SQL automatically:**
```csharp
// This C# LINQ query...
var results = await _db.SalesEnquiries
    .AsNoTracking()                              // don't track for update (read-only)
    .Include(e => e.Customer)                   // JOIN Customers table
    .Include(e => e.Items)                      // JOIN SalesEnquiryItems table
    .Where(e => e.TenantId == tenantId          // WHERE TenantId = @tenantId
             && e.EnquiryDate >= fromDate)      //   AND EnquiryDate >= @from
    .OrderByDescending(e => e.EnquiryDate)      // ORDER BY EnquiryDate DESC
    .ToListAsync();                             // execute async

// ...generates this SQL:
// SELECT e.*, c.*, i.*
// FROM SalesEnquiries e
// JOIN Customers c ON e.CustomerId = c.Id
// JOIN SalesEnquiryItems i ON i.EnquiryId = e.Id
// WHERE e.TenantId = @tenantId AND e.EnquiryDate >= @from
// ORDER BY e.EnquiryDate DESC
```

**`AsNoTracking()` rule**: Use it on every read-only query. Without it, EF tracks the entity for change detection (overhead you don't need when just displaying data).

## 4.2 Layer 4 — Generic Repository Pattern

The `Repository<T>` class in this repo provides common operations for ANY entity:

```csharp
// IRepository<T> — the contract
Task<T> GetAsync(int id);
Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> predicate);
Task<IEnumerable<T>> GetAllWithIncludeAsync(predicate, params includes);
Task<T> CreateAsync(T entity);
Task<T> UpdateAsync(T entity);
Task<bool> DeleteAsync(int id);
Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);

// Usage in a specific repository:
public class SalesEnquiryRepository : Repository<SalesEnquiry>, ISalesEnquiryRepository
{
    public SalesEnquiryRepository(ApplicationDbContext db, ILoggingService logs) 
        : base(db, logs) { }   // ← calls the generic base
    
    // Add module-specific methods here that can't be handled generically
    public async Task<List<SalesEnquiry>> GetByCustomerAsync(int customerId, int tenantId)
        => await GetAllWithIncludeAsync(
               e => e.CustomerId == customerId && e.TenantId == tenantId,
               e => e.Customer, e => e.Items);
}
```

## 4.3 Layer 3 — Unit of Work Pattern

The Unit of Work aggregates all repositories and manages the database transaction.

```csharp
// UnitOfWork.cs exposes all repositories as properties
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _db;
    
    public ISalesEnquiryRepository SalesEnquiries { get; }
    public ICustomerRepository Customers { get; }
    // ... all other repos
    
    public async Task<int> SaveAsync()    // ← ONE save for all changes
        => await _db.SaveChangesAsync();
}

// In a business service — the correct pattern:
public async Task SaveEnquiryAsync(SalesEnquiryViewModel vm)
{
    var entity = _mapper.Map<SalesEnquiry>(vm);
    
    if (vm.Id == 0)
        await _unitOfWork.SalesEnquiries.CreateAsync(entity);  // stage the insert
    else
        await _unitOfWork.SalesEnquiries.UpdateAsync(entity);  // stage the update
    
    await _unitOfWork.SaveAsync();  // ← ONE call — commits everything to DB
}
```

**Why UoW?** If you save a Sales Enquiry header and then saving the line items fails, you want BOTH to roll back. UoW wraps them in a single `SaveChangesAsync()` call = one database transaction.

## 4.4 Layer 2 — Business Service Pattern

```csharp
// Interface — defines the contract (what the layer can do)
// Location: IBusinessService/ISalesService/IEnquirySalesService.cs
public interface IEnquirySalesService
{
    Task<List<SalesEnquiryViewModel>> GetAllAsync(int tenantId);
    Task<SalesEnquiryViewModel?> GetByIdAsync(int id);
    Task<bool> SaveAsync(SalesEnquiryViewModel vm, int userId);
    Task<bool> CancelAsync(int id, string reason, int userId);
}

// Concrete implementation
// Location: BusinessService/SalesService/EnquirySalesService.cs
public class EnquirySalesService : IEnquirySalesService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;
    
    public EnquirySalesService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }
    
    public async Task<bool> SaveAsync(SalesEnquiryViewModel vm, int userId)
    {
        // Business rules live here (not in the page, not in the repo)
        if (vm.Items.Count == 0)
            throw new BusinessException("Enquiry must have at least one item.");
        
        if (await _uow.SalesEnquiries.AnyAsync(e => e.EnquiryNo == vm.EnquiryNo && e.Id != vm.Id))
            throw new BusinessException("Enquiry number already exists.");
        
        var entity = _mapper.Map<SalesEnquiry>(vm);
        entity.CreatedBy = userId;
        entity.CreatedDate = DateTime.Now;
        
        await _uow.SalesEnquiries.CreateAsync(entity);
        await _uow.SaveAsync();
        return true;
    }
}
```

## 4.5 Layer 1 — Razor Pages (UI Layer)

The page only handles UI concerns — loading data to display and capturing input. All business logic delegates to the service.

```razor
@page "/sales/enquiry"
@inherits BaseUserRightsComponent          ← gets CanView, CanCreate, CanEdit, CanDelete
protected override string ScreenName => "SalesEnquiry";

@inject IEnquirySalesService EnquiryService
@inject ISnackbar Snackbar
@inject NavigationManager Nav

@if (!CanView) { <RedirectToLogin /> return; }

<!-- MudBlazor grid -->
<MudDataGrid T="SalesEnquiryViewModel" Items="@_enquiries" Loading="@_loading">
    ...
</MudDataGrid>

@code {
    private List<SalesEnquiryViewModel> _enquiries = new();
    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadRightsAsync();                              // from BaseUserRightsComponent
        _enquiries = await EnquiryService.GetAllAsync(TenantId);
        _loading = false;
    }

    private async Task Save(SalesEnquiryViewModel vm)
    {
        try
        {
            await EnquiryService.SaveAsync(vm, UserId);      // delegate to service
            Snackbar.Add("Saved successfully", Severity.Success);
        }
        catch (BusinessException ex)
        {
            Snackbar.Add(ex.Message, Severity.Error);        // show error in UI
        }
    }
}
```

## 4.6 AutoMapper — ViewModel ↔ Entity Mapping

Never expose raw EF entities to the UI. Use ViewModels mapped with AutoMapper.

```csharp
// Entity (maps to DB table — in Data/ folder)
public class SalesEnquiry { public int Id; public int CustomerId; public Customer Customer; }

// ViewModel (what the Razor page binds to — in ViewModels/ folder)
public class SalesEnquiryViewModel { public int Id; public int CustomerId; public string CustomerName; }

// AutoMapper profile (in Mappings/ folder)
public class SalesProfile : Profile
{
    public SalesProfile()
    {
        CreateMap<SalesEnquiry, SalesEnquiryViewModel>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name));
        
        CreateMap<SalesEnquiryViewModel, SalesEnquiry>();  // reverse map for saves
    }
}

// In the service:
var viewModels = _mapper.Map<List<SalesEnquiryViewModel>>(entities);
```

## 4.7 The BaseUserRightsComponent — Screen-Level Authorization

Every list/form page in this repo `@inherits BaseUserRightsComponent`. This gives the page:
- `CanView` — can the user see this page at all?
- `CanCreate` — can they add new records?
- `CanEdit` — can they modify records?
- `CanDelete` — can they delete records?

These flags come from the `UserRights` table in the database, loaded via `LoadRightsAsync()` in `OnInitializedAsync`.

```razor
@* Usage pattern you'll see everywhere *@
@if (!CanView) { <RedirectToLogin /> return; }

<button @onclick="Add" disabled="@(!CanCreate)">Add New</button>
<button @onclick="Edit" disabled="@(!CanEdit)">Edit</button>
```

## 4.8 Multi-Tenancy — How the Database Context is Resolved

```
Browser Request (Host: tenant1.yourapp.com)
         ↓
TenantProvider.GetCurrentTenant()
         ↓  reads Host header
MasterDbContext → looks up TenantInfo by hostname
         ↓  finds connection string for tenant1
TenantDbContextFactory.CreateDbContext()
         ↓  creates ApplicationDbContext with tenant1's SQL Server connection
All queries run against tenant1's database (or tenant1's rows in a shared DB)
```

This is why `ITenantProvider` and `ITenantDbContextFactory` are registered as `Scoped` — each browser circuit (user session) gets its own resolved tenant and DbContext.

---

# PHASE 5 — MudBlazor UI Components (1 week)
> This repo uses MudBlazor 8.x exclusively. Learn these in priority order.

## Priority 1 — Used on Every Page

| Component | WPF Equivalent | Key Props |
|---|---|---|
| `MudText` | `TextBlock` | `Typo` (h1–h6, body1, body2, caption) |
| `MudTextField<T>` | `TextBox` | `@bind-Value`, `Label`, `Required`, `Variant` |
| `MudSelect<T>` | `ComboBox` | `@bind-Value`, `T="int"`, `<MudSelectItem>` |
| `MudButton` | `Button` | `Variant`, `Color`, `OnClick`, `Disabled` |
| `MudIconButton` | `Button` with icon | `Icon`, `Color`, `Size` |
| `MudDataGrid<T>` | `DataGrid` | `Items`, `Loading`, `T=`, `<Columns>` |
| `MudForm` | `StackPanel` + validation | `@ref`, `Validate()`, `IsValid` |
| `MudDialog` | `Window` (modal) | Via `IDialogService` or `BsModal` component |
| `MudSnackbar` | Toast notification | Via `ISnackbar.Add(message, severity)` |

## Priority 2 — Layout

```razor
<MudGrid>                    ← 12-column grid (like Grid ColumnDefinitions)
    <MudItem xs="12" md="6"> ← xs=mobile(full width), md=desktop(half width)
        <MudTextField ... />
    </MudItem>
    <MudItem xs="12" md="6">
        <MudSelect ... />
    </MudItem>
</MudGrid>
```

## Priority 3 — The MudDataGrid in Depth

```razor
<MudDataGrid T="SalesEnquiryViewModel" 
             Items="@_enquiries"          ← bind your List<T> here
             Loading="@_loading"          ← shows loading spinner
             Dense="true"                 ← compact rows
             Hover="true"                 ← hover highlight
             Striped="true">              ← alternating row colors
    <Columns>
        <PropertyColumn Property="x => x.EnquiryNo" Title="Enquiry #" Sortable="true" />
        <PropertyColumn Property="x => x.CustomerName" Title="Customer" />
        <PropertyColumn Property="x => x.EnquiryDate" Title="Date" Format="dd/MM/yyyy" />
        
        <TemplateColumn Title="Actions" Sortable="false">
            <CellTemplate>
                <MudIconButton Icon="@Icons.Material.Filled.Edit"
                               OnClick="@(() => Edit(context.Item))"
                               Disabled="@(!CanEdit)" />
                <MudIconButton Icon="@Icons.Material.Filled.Delete"
                               Color="Color.Error"
                               OnClick="@(() => Delete(context.Item.Id))"
                               Disabled="@(!CanDelete)" />
            </CellTemplate>
        </TemplateColumn>
    </Columns>
</MudDataGrid>
```

## Priority 4 — Forms with Validation

```razor
<MudForm @ref="_form">
    <MudTextField @bind-Value="_vm.EnquiryNo"
                  Label="Enquiry Number"
                  Required="true"
                  RequiredError="Enquiry number is required" />
    
    <MudDatePicker @bind-Date="_vm.EnquiryDate"
                   Label="Enquiry Date"
                   Required="true" />
    
    <MudButton OnClick="Save" Color="Color.Primary">Save</MudButton>
</MudForm>

@code {
    private MudForm _form = default!;
    
    private async Task Save()
    {
        await _form.Validate();
        if (!_form.IsValid) return;
        
        await EnquiryService.SaveAsync(_vm, UserId);
    }
}
```

---

# PHASE 6 — Advanced Topics (ongoing)

## 6.1 Entity Framework Core Migrations

When you change an entity (add a property, rename a column), you create a migration:

```powershell
# In Package Manager Console — target V.SMART.Shared project
Add-Migration AddEnquiryPriorityField -Project V.SMART.Shared -StartupProject V.SMART.Web

# Apply migration to the database
Update-Database -Project V.SMART.Shared -StartupProject V.SMART.Web
```

Migrations create files in `V.SMART.Shared/Migrations/` — **commit these to source control**.

## 6.2 JavaScript Interop (use sparingly)
MudBlazor handles 95% of JS needs. When you genuinely need JS:

```csharp
@inject IJSRuntime JS

// Call a JS function from C#
await JS.InvokeVoidAsync("printPage");
var result = await JS.InvokeAsync<string>("getClipboard");
```

```javascript
// In wwwroot/js/common.js
window.printPage = () => window.print();
```

**Verify in repo**: Search for `_JS.InvokeAsync` in `Pages/` to see real usage.

## 6.3 SignalR / Real-time Features
Blazor Server already uses SignalR under the hood. If you need to push updates to multiple users simultaneously (e.g., production dashboard), you use `IHubContext<T>` — but only when truly needed.

## 6.4 E-Invoice API Integration
This repo has a full E-Invoice (GST) API integration in `E_Invoice/` and `BusinessLayer/BusinessService/EInvoiceAPIService/`. Study this as an example of an external HTTP API call pattern using `HttpClient` + `BouncyCastle` encryption.

---

# PHASE 7 — Development Workflow & Tools (ongoing)

## Daily Tools
| Tool | Purpose | Shortcut |
|---|---|---|
| VS Code + C# Dev Kit | Primary IDE | — |
| SQL Server Management Studio (SSMS) | Query the database directly | — |
| Browser DevTools (F12) | Inspect HTML output, network requests, JS errors | F12 |
| Blazor error page | Shows server-side errors in the browser | — |

## Debugging Blazor Server
1. Set breakpoints in C# code normally — the debugger attaches to the server process
2. Browser F12 → Console tab shows SignalR connection errors
3. `DetailedErrors = true` in `Program.cs` (already set) shows full stack traces in browser
4. Use `ILogger<T>` in services to write structured logs

## Recommended Learning Order

| Week | Focus | Resources |
|---|---|---|
| Week 1 | HTTP basics + HTML/CSS basics + C# async/await | Microsoft Learn: "Web development for beginners" |
| Week 2 | Blazor fundamentals (components, lifecycle, binding) | Microsoft Docs: "Blazor tutorial" |
| Week 3 | EF Core + LINQ + Repository pattern | Microsoft Docs: "EF Core getting started" |
| Week 4 | MudBlazor components | mudblazor.com/docs — work through each component |
| Week 5 | Study THIS repo — trace one full module top to bottom | Open SalesEnquiry end-to-end |
| Week 6+ | Build a new feature from scratch using the patterns above | Pick a small new screen |

## Tracing a Full Feature End-to-End (Homework Exercise)

Trace the **Sales Enquiry** feature by opening these files in order:

1. `Data/SalesAndLabour/SalesEnquiry/` — understand the entity
2. `Repository/IRepository/ISalesAndLabourRepository/ISalesEnquiry/` — understand the repo interface
3. `Repository/SalesAndLabourRepository/SalesEnquiry/` — understand the repo implementation
4. `Repository/UnitOfWork.cs` — find where `SalesEnquiries` is exposed
5. `BusinessLayer/BusinessService/IBusinessService/ISalesService/IEnquirySalesService.cs` — service interface
6. `BusinessLayer/BusinessService/SalesService/` — service implementation
7. `ViewModels/SalesAndLabourViewModel/` — the ViewModel
8. `Pages/SalesAndLabour_pages/SalesEnquiry_Pages/EnquirySalesList.razor` — the UI
9. `V.SMART.Web/Program.cs` — find where all these services are registered

After this exercise, you will understand every layer of the architecture completely.

---

*Document version: 1.0 | Created: 2026-08-01*
*Update this document as you discover new patterns in the codebase.*
