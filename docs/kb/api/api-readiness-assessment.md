---
doc_id: KB-041
title: API Readiness Assessment (Proposal)
module: api
source_files:
  - V.SMART/V.SMART.Api/Controllers/CurrencyController.cs
  - V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/
  - V.SMART/V.SMART.Web/Program.cs
entities: []
api_endpoints: []
database_tables: []
business_rules: []
status: proposal
confidence: n/a
last_verified: 2026-08-20
dependencies: [KB-011, KB-013, KB-014, KB-040]
---

# API Readiness Assessment

> **This document is a proposal.** Current behaviour is in
> [`api-overview.md`](api-overview.md).

## The question

> Can the existing backend support a new React frontend without modification?

**No — but the required work is additive, and no business logic needs rewriting.**

## Why the backend is well positioned

| Property | Evidence | Consequence |
|---|---|---|
| Services take/return **ViewModels**, not entities — **mostly**; see the caveat below | `IMfgPoService`, `ICurrencyService`; `ViewModels/` (274 files) | JSON-serialisable contracts already exist for most modules |
| ViewModels carry `DataAnnotations` | `CurrencyVM.cs` | `ModelState` validation works free; also translatable to Zod |
| Services are **scoped DI**, host-agnostic — **now realised, with six named exceptions; see below** | `AddVSmartDomain()` in `V.SMART/V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs`, called by all three hosts (M2-B07) | drop into `V.SMART.Api` unchanged |
| Only ~3% of business files touch UI types | 14 reference `Pages`, 19 reference Blazor/MudBlazor | small, enumerable decoupling backlog |
| Explicit transactions already used | 302 `BeginTransaction*` sites | write endpoints are atomic without new work |
| Consistent method-name conventions | `SearchWithDynamicFilterAsync`, `Get…ByIdAsync`, `Upsert…Async`, `CanDelete…Async` | controllers can be templated |
| Reports already return `byte[]` | `ReportService.Generate_Report` | PDF endpoints are trivial |
| **A working proof exists** | `CurrencyController` wraps `ICurrencyService` with zero service changes | the pattern is validated, not theoretical |

### Update: the "drop in unchanged" claim is now realised — with six exceptions

**Added 2026-08-19 (Confirmed, M2-B07; branch `migration/M2-B07-add-vsmart-domain`, not yet
merged).** The premise this document rested on has been tested rather than asserted.
`V.SMART.Api` previously registered exactly **one** business service (`ICurrencyService`) and
no `IRepository<>` open generic, so no second controller could have activated. It now calls
the single shared `AddVSmartDomain()` (`V.SMART/V.SMART.Api/Program.cs:87`) and can resolve
the whole domain graph by constructor injection.

**Six registrations remain unresolvable in the API**, because the host seams they need have no
API implementation. This is expected and is closed by M2-B06 / M2-B08, not by M2-B07:

| Unresolvable in `V.SMART.Api` | Missing seam | Closed by |
|---|---|---|
| `ReportService` | `IPathProvider` | M2-B08 |
| `IUserService` | `IPathProvider`, `IJSRuntime` | M2-B08 |
| `IGSTITCService` | `IPathProvider` | M2-B08 |
| `IUserThemePreferenceService` | `IJSRuntime` | — |
| `ICompanyService` | `IFileUploadService`, bare `HttpClient` | M2-B06 |
| `IItemService` | `IFileUploadService` | M2-B06 |

**`V.SMART.Api` still has no `IPathProvider` implementation** — inventing one was explicitly
out of M2-B07's scope. `IJSRuntime` is a Blazor concept and has no meaningful web-API
implementation at all; `IUserThemePreferenceService` will need a different approach.

The guarantee is enforced by
`tests/V.SMART.Shared.Tests/DependencyInjection/AddVSmartDomainTests.cs`, which validates the
full graph with `BuildServiceProvider(validateScopes: true, validateOnBuild: true)`.


### Caveat: the ViewModel boundary is not universal

**Added 2026-08-12 (Confirmed).** The first row above was generalised from two sampled
services. It does not hold everywhere.

Of **139** `I*Service.cs` interfaces under `BusinessLayer/`, **21 mention no ViewModel type
in any signature** — they take and return EF entities or primitives:

```
ISummaryAndGraphsService  IAppVersionService      IDashboardService
IEinvoiceDatabaseService  IEWayDatabaseService    IDefectInfoService
IHSNService               IStockManagerService    IBankService
IUserRightService         ICompanyService         ILeaveApplicationService
IProcessService           IBiometricExcelSettingService  IHRMasterService
IInspectionSettingsService IProdLogSettingsService ISalaryHeadPrintSettingService
IStoreMappingService      IUserThemePreferenceService    (+1)
```

Concrete examples: `IBankService` exposes `Task<Banks> GetBankAsync(int)` and
`Task<bool> UpsertBankAsync(Banks)`; `ITermsAndConditionsService` exposes
`Task<TermsAndConditions> CreateAsync(TermsAndConditions)` — **entities**. And
`BankVM`, `StateVM`, `CurrencyTodayVM` and `ProjectTypeMasterVM` **do not exist at all**.

**21 is a floor, not the count.** `ITermsAndConditionsService` is absent from that list only
because one of its eight methods returns a `TermsAndConditionsVM`. Mixed interfaces —
entities on the write path, ViewModels on the search path — are the more common shape, and
have not been enumerated.

**Consequences for the API work.** Two of the affected services are load-bearing:
`IStockManagerService` (the highest-correctness-risk service in the product) and
`IUserRightService` (needed to administer the permission matrix in M3-3). For each affected
module, a controller cannot simply be templated over the existing signature:

1. A ViewModel must be authored before the endpoint exists, or
2. the entity is serialised directly — which exposes EF navigation properties, risks
   circular references and lazy-loading on serialise, and opens over-posting on the write
   path.

**Option 1 is required**; option 2 must not be used. This does **not** change the verdict —
the backend is still reusable additively, and no business logic needs rewriting — but it
adds a per-module cost that item **B1** did not account for. Each wave's `<W>-05` (API
contract) task must check its services' signatures first and, where a ViewModel is missing,
author one *and* an AutoMapper profile before `<W>-06`.

## What must be built

### P0 — blockers (nothing ships without these)

| # | Item | Why | Est. |
|---|---|---|---|
| A1 | **Server-side screen-right authorization** — an authorization filter/attribute resolving `UserRight × Screens` per request, applied to every endpoint | Today authorization is UI-only (BR-AUTH-002). Without it every endpoint is open to any authenticated user | 1–2 wks |
| A2 | **Secrets out of source control** — connection strings and `Jwt:Secret` to environment/Key Vault; rotate the exposed SA and `bspl` credentials | R-01, R-02 | 2–3 days |
| A3 | **Tenant resolution for a cross-origin SPA** — tenant in the login request; JWT claim thereafter; real CORS origin list | Host-based resolution breaks for an SPA | 3–5 days |
| A4 | **Refresh tokens + revocation** | 8-hour non-revocable JWT is unacceptable for an ERP | 3–5 days |
| A5 | ~~**Global exception handling → `ProblemDetails`**, correlation ids~~ — **DELIVERED by M2-A06 (2026-08-20)**. Request logging is **not** delivered: a real log sink remains M2-B11 / R-23 | No error contract existed | 3–5 days |
| A6 | **Decouple `IApprovalService` from the `Authorization` Razor page** (`using static …Pages…`), plus the other 13 `Pages`-referencing files | Business layer cannot ship without the UI assembly otherwise | 1 wk |

### P1 — required for a complete product

| # | Item | Est. |
|---|---|---|
| B1 | **~60–80 resource controllers** over existing services, following one template | 8–12 wks (mechanical, parallelisable) |
| B2 | **Extract `@code` business logic into services** — ~184k LOC to triage; the real number needing extraction is far smaller, but must be assessed per screen | **the dominant cost; per-module** |
| B3 | **File upload/download endpoints** — replace `IBrowserFile` and local-path `IFileOpener` | 1 wk |
| B4 | **Report endpoints** — `GET /api/reports/…` → `application/pdf`; Excel export/import endpoints | 1 wk |
| B5 | **Permission bootstrap endpoint** — `GET /api/me` returning user, tenant, role, and the full `UserRight` set so React can render correctly | 2 days |
| B6 | **Reference-data endpoints** — GST rates, screen catalogue, UOM, states, currencies, terms | 3 days |
| B7 | **Typed screen-code constants** replacing the magic integers passed to `IStockManagerService` | 2 days |
| B8 | **Approval endpoints** enforcing `UserAuthority` server-side | 1 wk |
| B9 | **Server-side sort/filter/paging contract** consistent across all list endpoints | 1 wk |
| B10 | **OpenAPI → TypeScript client generation** in CI | 3 days |

### P2 — hardening

| # | Item |
|---|---|
| C1 | Rate limiting, response compression, output caching for reference data |
| C2 | Health checks + structured logging sink (replace flat files) |
| C3 | API versioning (`/api/v1`) |
| C4 | Integration tests, starting with `IStockManagerService` and `ICalculationService` |
| C5 | Idempotency keys on document-create endpoints (document numbering race, R-12) |

## Explicitly out of scope

- Rewriting any business service.
- Changing the database schema.
- Replacing FastReport or the 94 stored procedures.
- Replacing EF Core, AutoMapper, or the Repository/UoW pattern.
- Converting the Angular pilot.

## Controller template (to be adopted as the standard)

```csharp
[ApiController]
[Route("api/v1/sales-orders")]
[Authorize]
[RequireScreen("Sales Order")]                 // A1: resolves UserRight × Screens
public class SalesOrdersController(IMfgPoService svc) : ControllerBase
{
    [HttpGet]  [RequireRight(Right.View)]
    public Task<ActionResult<PagedResult<MfgPoVM>>> Search([FromQuery] SalesOrderQuery q) => …;

    [HttpGet("{id:int}")] [RequireRight(Right.View)]
    public Task<ActionResult<MfgPoVM>> Get(int id) => …;

    [HttpPost] [RequireRight(Right.Create)]
    public Task<ActionResult<MfgPoVM>> Create([FromBody] MfgPoVM vm) => …;

    [HttpPut("{id:int}")] [RequireRight(Right.Edit)]
    public Task<ActionResult<MfgPoVM>> Update(int id, [FromBody] MfgPoVM vm) => …;

    [HttpDelete("{id:int}")] [RequireRight(Right.Delete)]
    public Task<IActionResult> Delete(int id) => …;   // CanDelete… then Delete…

    // Document-specific commands — these carry the BR-SO-003 orchestration
    [HttpPost("{id:int}/cancel")]      [RequireRight(Right.Edit)]
    [HttpPost("{id:int}/short-close")] [RequireRight(Right.Edit)]
    [HttpGet ("{id:int}/print")]       [RequireRight(Right.View)]   // application/pdf
}
```

Two rules for every controller:
1. **Thin.** Bind, authorize, call one service method, map the result. No business logic.
2. **Commands are explicit.** Anything that is a workflow step (cancel, short-close,
   approve, release, post) gets its own `POST /{id}/{verb}` endpoint that runs the *entire*
   server-side sequence. The client never orchestrates a multi-step business operation.

## Standard error contract (as-is for `V.SMART.Api` since M2-A06, 2026-08-20)

**No longer proposed.** Implemented by M2-A06 under
`V.SMART/V.SMART.Api/Middleware/`; the authoritative description of what shipped is
[`api-overview.md` § *Error contract*](api-overview.md#error-contract-m2-a06).

Two corrections to the sketch below, so it is not copied wrongly:

- the `type` base that shipped is **`https://api.v-smart.local/problems/`**, taken from
  KB-105 §7.1, which had already frozen the `403` body. The
  `https://api.vsmart/errors/…` in the example is superseded;
- `errors` appears on `400` only, and `detail` is omitted when null — a body never carries
  both `errors` and a business-rule `title`.

`application/problem+json` everywhere:

```json
{
  "type": "https://api.v-smart.local/problems/business-rule",
  "title": "Cannot delete this Sales Order as a Sales DC transaction exists.",
  "status": 409,
  "detail": "…",
  "instance": "/api/v1/sales-orders/1234",
  "traceId": "…",
  "errors": { "CurrName": ["Currency Name cannot exceed 100 characters"] }
}
```

Mapping:
- `400` model-binding / `DataAnnotations` failures → `errors` dictionary
- `409` business-rule refusal — **carry the service's existing `Message` string verbatim
  into `title`**; those strings are product UX (BR-SO-001)
- `403` screen-right denial
- `404` not found
- `500` unhandled, with `traceId` only

## Verdict

The backend needs a **new HTTP surface and a new authorization layer**, not new business
logic. The dominant risk is not the API work (mechanical, estimable) — it is item **B2**,
extracting business logic out of 333 Razor pages. Every module estimate in
[`frontend-new/feature-mapping.md`](../frontend-new/feature-mapping.md) is driven by that.
