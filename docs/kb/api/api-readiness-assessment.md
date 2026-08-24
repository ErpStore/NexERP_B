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
last_verified: 2026-08-24
dependencies: [KB-011, KB-013, KB-014, KB-040]
---

# API Readiness Assessment

> **This document is a proposal.** Current behaviour is in
> [`api-overview.md`](api-overview.md).

## The question

> Can the existing backend support a new Angular frontend without modification?

**No — but the required work is additive, and no business logic needs rewriting.**

## Why the backend is well positioned

| Property | Evidence | Consequence |
|---|---|---|
| Services take/return **ViewModels**, not entities — **mostly**; see the caveat below | `IMfgPoService`, `ICurrencyService`; `ViewModels/` (274 files) | JSON-serialisable contracts already exist for most modules |
| ViewModels carry `DataAnnotations` | `CurrencyVM.cs` | `ModelState` validation works free; also translatable to Zod |
| Services are **scoped DI**, host-agnostic — **now realised, with six named exceptions; see below** | `AddVSmartDomain()` in `V.SMART/V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs`, called by all three hosts (M2-B07) | drop into `V.SMART.Api` unchanged |
| Only ~3% of business files touch UI types | ~~14 reference `Pages`~~ — the true figure was **15 files / 16 `using` directives**, and all are **gone as of 2026-08-21 (M2-B04)**; a CI-enforced architecture guard keeps it at zero. 19 reference Blazor/MudBlazor — still outstanding | small, enumerable decoupling backlog |
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
| A6 | ~~**Decouple `IApprovalService` from the `Authorization` Razor page** (`using static …Pages…`), plus the other `Pages`-referencing files~~ — **DELIVERED by M2-B04 (2026-08-21)**. The count was **16 `using` directives across 15 non-UI files**, not 13 others (`Data/AccountsModule/FundTrans.cs:11,12` carried two). 15 imported nothing and were deleted; the one load-bearing case was `ViewModels/AccountsViewModel/FundTransFilterVM.cs`, whose `Bank` property was typed against the Razor component rather than the EF entity `Banks` (`Data/Master/Accounts_Module/Banks.cs:6`) and was retyped. No type had to move. Guarded by `tests/V.SMART.Shared.Tests/Architecture/NoPagesReferenceFromDomainTests.cs`, which runs in CI. **Not delivered, and not in scope:** the assembly is still one project containing both the domain and `Pages/` | Business layer cannot ship without the UI assembly otherwise | 1 wk |

### P1 — required for a complete product

| # | Item | Est. |
|---|---|---|
| B1 | **~60–80 resource controllers** over existing services, following one template — **the template exists and is frozen: [KB-114 `controller-conventions.md`](controller-conventions.md) (M2-B03, 2026-08-24)**. The 8–12 week estimate assumed a template complete enough to follow without re-deriving decisions; that is now what it is measured against | 8–12 wks (mechanical, parallelisable) |
| B2 | **Extract `@code` business logic into services** — ~184k LOC to triage; the real number needing extraction is far smaller, but must be assessed per screen | **the dominant cost; per-module** |
| B3 | **File upload/download endpoints** — replace `IBrowserFile` and local-path `IFileOpener` | ~~1 wk~~ **DELIVERED (M2-B06, 2026-08-21)** |
| B4 | **Report endpoints** — `GET /api/reports/…` → `application/pdf`; Excel export/import endpoints | 1 wk |
| B5 | ~~**Permission bootstrap endpoint** — returning user, tenant, role and the full `UserRight` set so the SPA can render correctly~~ — **DELIVERED by M2-A07 (2026-08-20)** as `GET /api/v1/me`. Note the URL: this row used to name `/api/me`, while ADR-004 §3, KB-105 and KB-080 §9 all said `/api/v1/me`; the endpoint ships at the latter and this row was the outlier. Contract in [KB-040](api-overview.md) | 2 days |
| B6 | ✅ **DELIVERED (M2-B09, 2026-08-21)** — ~~Reference-data endpoints — GST rates, screen catalogue, UOM, states, currencies, terms~~. All six live under `/api/v1/reference`, tenant-keyed output cached. See [KB-124](reference-data-and-caching.md). | 3 days |
| B7 | **Typed screen-code constants** replacing the magic integers passed to `IStockManagerService` | 2 days |
| B8 | **Approval endpoints** enforcing `UserAuthority` server-side | 1 wk |
| B9 | **Server-side sort/filter/paging contract** consistent across all list endpoints | 1 wk — **contract delivered by M2-B02 (2026-08-20) and written up for controller authors in [KB-114 §5](controller-conventions.md); rollout ongoing.** See below |
| B10 | ✅ **DELIVERED (M2-B10, 2026-08-24)** — ~~OpenAPI → TypeScript client generation in CI~~. `api/openapi.json` is committed and covers all 18 operations; the Angular client is generated by `ng-openapi-gen` into `frontend/nexgen-web/src/app/core/api/generated/`; the `api-contract` job in `.github/workflows/ci.yml` regenerates both, fails on drift and type-checks the SPA against the result. One command for developers and CI: `bash tools/generate-api-client.sh`. Generator comparison, the `decimal` → `number` finding flagged to M2-C10, and the whole procedure: [KB-112](generated-client.md) | 3 days |

**B3 status (M2-B06, 2026-08-21).** Delivered. `POST /api/v1/files` and
`GET /api/v1/files/{id:int}` replace `IBrowserFile` and the JS-interop `IFileOpener`;
`V.SMART.Api` gains the `IFileUploadService` implementation it had none of, registered as a host
registration beside `AddVSmartDomain()` and never inside it. `ICompanyService`/`CompanyService` no
longer reference `IBrowserFile` — the adaptation moved to the one Razor call site
(`CompanyUpsert.razor:1105`). Full contract and security controls:
[`api-overview.md`](api-overview.md).

Two things a reader should carry forward rather than assume closed:

- **The Excel endpoints are a reference implementation, not a rollout.** They exist on
  `currencies` only. Item **B4**'s Excel half is therefore *proven*, not *done*; rolling the
  pattern out is per-module work (KB-080 §10). B4's report half is untouched.
- **`IFileOpener` is not deleted and must not be.** The two Blazor hosts still use it. It becomes
  dead only when the last Razor page is retired, far beyond M2.

**A live defect was found and deliberately not fixed:** `SaveCorresFileAsync` writes a **zero-byte
file** and reports success (`WebFileUploadService.cs:100-104`) — the stream copy is commented out.
Every Blazor correspondence/drawing upload has been landing empty. Recorded as **R-67**; the API
path copies correctly and is tested for byte identity. Fixing the Blazor path changes live
behaviour and needs its own task.

**B9 status (M2-B02, 2026-08-20).** The contract itself is delivered and proven on one endpoint:
`PagedResult<T>`, `PagedQuery`, a per-resource typed query record, `SortSpecification` (syntax +
allow-list) and `FilterDictionaryAdapter` in `V.SMART/V.SMART.Api/Contracts/`, applied to
`GET api/v1/currencies`. The rules — sort syntax, `pageSize` maximum 100, allow-list, 400 conditions
— are in [ADR-002 §2a](../decisions/ADR-002-rest-api-layer.md). **"Consistent across all list
endpoints" is not yet true**: `SearchWithDynamicFilterAsync` is declared 134 times across
`V.SMART.Shared/BusinessLayer/` and exactly one of them (`CurrencyService`) has the sort-aware
overload. The remaining 133 convert inside their own module's migration wave
([KB-080 §10](../execution/README.md#10-module-migration-task-pattern), step 06), never in one
sweep. Read B9 as *contract done, rollout ~1/134*.

### P2 — hardening

| # | Item |
|---|---|
| C1 | Rate limiting, response compression, ~~output caching for reference data~~ — **the output-caching third is DELIVERED (M2-B09)**; rate limiting and response compression remain. See [KB-124](reference-data-and-caching.md) §2. |
| C2 | ~~Health checks + structured logging sink (replace flat files)~~ — **DELIVERED 2026-08-21 by [M2-B11](../execution/tasks/M2-B11.md).** `GET /health/live` (anonymous, runs **no** check, touches no database) and `GET /health/ready` (master DB + a configurable subset of tenant DBs, each reported individually, `503` on failure), both outside `/api/v1` because they are infrastructure. Bodies are an **allow-list** — status, check name, duration, and for the tenant check an opaque `tenant-{Id}` — never a description, exception, connection string, server or database name; the endpoints are anonymous, so that is a security property, not tidiness. Logging is Serilog to compact-JSON rolling files, split on an `EventType` discriminator into an **audit** stream (3650-day retention) and a **diagnostic** stream (14-day), enriched with the M2-A06 correlation id, and configurable to a second sink from `appsettings.json`. **Two caveats a reader must not miss:** the *sink choice itself is deferred pending Q-16* (criteria written down), and the structured implementation is registered in **`V.SMART.Api` only** — the Blazor and MAUI hosts deliberately keep `FileLoggingService`. See [KB-113](../architecture/observability.md) |
| C3 | ~~API versioning (`/api/v1`)~~ — **DELIVERED 2026-08-21 by [M2-B01](../execution/tasks/M2-B01.md).** All six endpoints moved under `/api/v1`; the prefix lives in the single constant `V.SMART/V.SMART.Api/ApiRoutes.cs` (`ApiRoutes.V1`) and no controller writes it literally. No versioning package was added — one version needs no negotiation, and `Asp.Versioning.Mvc` would complicate the Swagger document M2-B10 generates the client from. **Pulled forward out of P2 into M2 deliberately:** versioning touches every controller's route attribute, so its cost grows with controller count while its value does not ([KB-080 §9](../execution/README.md), "land it while there are two controllers, not sixty"). See [KB-040 § *Versioning*](api-overview.md#versioning-m2-b01) |
| C4 | Integration tests, starting with `IStockManagerService` and `ICalculationService` |
| C5 | Idempotency keys on document-create endpoints (document numbering race, R-12) |

## Explicitly out of scope

- Rewriting any business service.
- Changing the database schema.
- Replacing FastReport or the 94 stored procedures.
- Replacing EF Core, AutoMapper, or the Repository/UoW pattern.
- Converting the Angular pilot.

## Controller template (to be adopted as the standard) — **SUPERSEDED 2026-08-24 by KB-114**

> ⛔ **Do not implement from the sketch below.** It was turned into a real, complete and
> **compiled** specification by `M2-B03`:
> **[`controller-conventions.md`](controller-conventions.md) (KB-114)** — route shape,
> authorization attributes and how to find the screen name, the paging/sort/filter contract,
> the error contract, the workflow-command pattern, payloads, the service-method → verb
> mapping (including the decided `Upsert…Async` answer), the `[ProducesResponseType]` set
> M2-B10 depends on, an objective definition of "thin", and a conformance checklist.
> **That document is frozen**: a change to the contract after M2-B03 is a breaking, versioned
> change. The sketch is kept only as history — it predates `/api/v1`, `PagedResult<T>`,
> `ProblemResults` and the `[ProducesResponseType]` obligation.

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
