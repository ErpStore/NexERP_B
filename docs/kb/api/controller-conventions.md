---
doc_id: KB-114
title: Controller Conventions — the frozen API contract every controller implements
module: api
source_files:
  - V.SMART/V.SMART.Api/ApiRoutes.cs
  - V.SMART/V.SMART.Api/Controllers/CurrencyController.cs
  - V.SMART/V.SMART.Api/Controllers/AuthController.cs
  - V.SMART/V.SMART.Api/Contracts/PagedQuery.cs
  - V.SMART/V.SMART.Api/Contracts/PagedResult.cs
  - V.SMART/V.SMART.Api/Contracts/SortSpecification.cs
  - V.SMART/V.SMART.Api/Contracts/CurrencyQuery.cs
  - V.SMART/V.SMART.Api/Contracts/FilterDictionaryAdapter.cs
  - V.SMART/V.SMART.Api/Middleware/ProblemResults.cs
  - V.SMART/V.SMART.Api/Middleware/ApiProblems.cs
  - V.SMART/V.SMART.Api/Middleware/ProblemTypes.cs
  - V.SMART/V.SMART.Api/Authorization/RequireScreenAttribute.cs
  - V.SMART/V.SMART.Api/Authorization/RequireRightAttribute.cs
  - V.SMART/V.SMART.Api/Authorization/NoScreenRightAttribute.cs
  - V.SMART/V.SMART.Api/Authorization/ScreenCatalogue.cs
  - V.SMART/V.SMART.Api/Authorization/ScreenRightStartupValidator.cs
entities: []
api_endpoints: []
database_tables: [Screens, UserRights]
business_rules: [BR-SO-001, BR-AUTH-002]
status: active
confidence: confirmed
last_verified: 2026-08-24
dependencies: [ADR-002, ADR-004, ADR-005, KB-011, KB-030, KB-040, KB-041, KB-105, KB-108]
---

# Controller Conventions — the frozen API contract

**Open this before writing any controller. Copy §2, change the nouns, then run §11 against
what you wrote.**

This document implements [ADR-002](../decisions/ADR-002-rest-api-layer.md) and
[ADR-004](../decisions/ADR-004-server-side-authorization.md). It does not supersede them and
it does not contradict them; where this document is more specific, it is because a decision
was taken here that they left open, and that decision is marked as such.

Everything below is **Confirmed against code on 2026-08-24** unless it says otherwise. Every
`file:line` was re-read at the line cited.

---

## 1. Scope and status — this contract is FROZEN

**The contract described here is frozen as of task `M2-B03`.**

[KB-080 §9](../execution/README.md#risks) records the reason as a risk mitigation in the plan
itself: *"Freeze the contract at M2-B03; treat later changes as breaking and version them."*
The chain is strictly sequential —

```
M2-A06 (error contract) → M2-B02 (paging contract) → M2-B03 (this document) → M2-B10 (TS client)
```

— because [M2-B10](../execution/tasks/M2-B10.md) generates the TypeScript client from the
OpenAPI document. A client generated from an unsettled contract has to be regenerated, and
every Angular component built on it reworked. That cost is why B10 must not start early, and
it is equally why **this document must not be quietly amended later.**

After this point:

- a change to any shape described here is a **breaking API change**. It requires a new route
  version (a second constant beside `ApiRoutes.V1`, `V.SMART/V.SMART.Api/ApiRoutes.cs:29`), not
  an edit in place;
- adding a **new** resource that follows these rules is not a change to the contract and needs
  nothing from this document;
- adding a genuinely new *kind* of endpoint (one this document has no shape for) requires an
  addendum here **and** an ADR-002 addendum, in the same change, before the endpoint merges;
- a controller that diverges is either fixed, or it carries a written exception in §12 of this
  document. There is no third option.

**What this document is not.** It is not a description of the Angular client, not a report
contract ([ADR-005](../decisions/ADR-005-reporting-and-printing.md) and `M2-B08` own that),
and not an authorization design ([KB-105](../architecture/server-side-authorization-spec.md)
owns that). It is the shape of a controller.

---

## 2. The reference controller — complete, and compiled

The block below is a **whole file**. It was compiled inside `V.SMART.Api` on 2026-08-24 —
`dotnet build V.SMART/V.SMART.Api/V.SMART.Api.csproj` reported **0 Error(s)** (first pass, full
tree: 6,695 warnings, the documented MudBlazor baseline, unchanged; the re-compile after the
`Search` action was added was incremental and reported 0 errors, 2 pre-existing `NU1608`
restore warnings) — and then deleted, because `M2-B03` ships no code. It is written over
`IMfgPoService`, a **real** service, so every call, every tuple shape and every return type
below is one the compiler accepted.

**Two differences between the block below and the file that was compiled, and no others:**
the compiled types were prefixed `Scratch…` (`ScratchSalesOrdersController`,
`ScratchSalesOrderQuery`, `ScratchSalesOrderFilterAdapter`) so they could not collide with a
future real controller, and `ForSalesOrder` sat in a file-local static class rather than on
`V.SMART.Api.Contracts.FilterDictionaryAdapter`, which `M2-B03` was not allowed to modify. In
your controller, the adapter method belongs **on `FilterDictionaryAdapter`**, one explicit
method per resource, exactly as `FilterDictionaryAdapter.ForCurrency`
(`V.SMART/V.SMART.Api/Contracts/FilterDictionaryAdapter.cs:42`) is.

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using V.SMART.Api.Contracts;
using V.SMART.Api.Middleware;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Services.ReportViewer;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;

namespace V.SMART.Api.Controllers
{
    /// <summary>The typed query DTO. One per resource; it derives from <see cref="PagedQuery"/>.</summary>
    public sealed record SalesOrderQuery : PagedQuery
    {
        public const string CustomerParameter = "customer";
        public const string CreatedByParameter = "createdBy";
        public const string FromDateParameter = "fromDate";
        public const string ToDateParameter = "toDate";

        /// <summary>
        /// Empty until IMfgPoService gains the four-argument SearchWithDynamicFilterAsync
        /// overload in its own module's wave (KB-011). Empty means every sort= is a 400, which is
        /// the loud failure; declaring names the service cannot honour would be the silent one.
        /// </summary>
        public static readonly IReadOnlyList<string> Sortable = Array.Empty<string>();

        protected override IReadOnlyList<string> SortableFields => Sortable;

        [FromQuery(Name = CustomerParameter)]
        public string? Customer { get; init; }

        [FromQuery(Name = CreatedByParameter)]
        public string? CreatedBy { get; init; }

        [FromQuery(Name = FromDateParameter)]
        public DateTime? FromDate { get; init; }

        [FromQuery(Name = ToDateParameter)]
        public DateTime? ToDate { get; init; }
    }

    /// <summary>
    /// In the real thing this is one more method on
    /// V.SMART.Api.Contracts.FilterDictionaryAdapter. Keys are the resource's own
    /// *FilterBuilder switch labels - MfgPoService.cs:387,404,409,414.
    /// </summary>
    public static class SalesOrderFilterAdapter
    {
        public static Dictionary<string, object>? ForSalesOrder(SalesOrderQuery query)
        {
            ArgumentNullException.ThrowIfNull(query);

            var filters = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(query.Customer))
                filters["Customer"] = query.Customer;

            if (!string.IsNullOrWhiteSpace(query.CreatedBy))
                filters["CreatedBy"] = query.CreatedBy;

            if (query.FromDate.HasValue)
                filters["FromDate"] = query.FromDate.Value.ToString(
                    FilterDictionaryAdapter.FilterDateFormat,
                    System.Globalization.CultureInfo.InvariantCulture);

            if (query.ToDate.HasValue)
                filters["ToDate"] = query.ToDate.Value.ToString(
                    FilterDictionaryAdapter.FilterDateFormat,
                    System.Globalization.CultureInfo.InvariantCulture);

            return filters.Count > 0 ? filters : null;
        }
    }

    [ApiController]
    [Route($"{ApiRoutes.V1}/sales-orders")]
    [Authorize]
    [RequireScreen(ScreenName)]
    public class SalesOrdersController : ControllerBase
    {
        /// <summary>Seeded Screens.ScreenName, byte for byte (ApplicationDbContext.cs:1151-1325).</summary>
        private const string ScreenName = "Sales Order";

        private const string PdfContentType = "application/pdf";

        /// <summary>The typeahead query-string parameter, and its hard row cap (§9).</summary>
        private const string TypeaheadParameter = "q";

        private const int TypeaheadCap = 20;

        /// <summary>The resource's own *FilterBuilder switch label - MfgPoService.cs:346.</summary>
        private const string SalesOrderNumberFilterKey = "Po";

        private const string PrintTemplate = "<resource>.frx";
        private const string PrintParameter = "<parameter>";
        private const string PrintProcedure = "<procedure>";

        private readonly IMfgPoService _salesOrders;
        private readonly ReportService _reports;

        public SalesOrdersController(IMfgPoService salesOrders, ReportService reports)
        {
            _salesOrders = salesOrders;
            _reports = reports;
        }

        [HttpGet]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(PagedResult<MfgPoVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<MfgPoVM>>> GetAll([FromQuery] SalesOrderQuery query)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var (items, totalCount) = await _salesOrders.SearchWithDynamicFilterAsync(
                query.PageNumber,
                query.PageSize,
                SalesOrderFilterAdapter.ForSalesOrder(query));

            return Ok(new PagedResult<MfgPoVM>(items, totalCount, query.PageNumber, query.PageSize));
        }

        [HttpGet("{id:int}")]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(MfgPoVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<MfgPoVM>> GetById(int id)
        {
            var vm = await _salesOrders.GetPoByPoIdAsync(id);
            if (vm is null)
                return this.NotFoundProblem("Sales Order not found.");

            return Ok(vm);
        }

        /// <summary>
        /// Typeahead. Unpaged, and capped at TypeaheadCap rows - state the cap here, in the
        /// summary the generated client shows. Never a mode of the list endpoint (§9).
        /// </summary>
        [HttpGet("search")]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(IEnumerable<MfgPoVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<IEnumerable<MfgPoVM>>> Search(
            [FromQuery(Name = TypeaheadParameter), Required, MinLength(1)] string q)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var (items, _) = await _salesOrders.SearchWithDynamicFilterAsync(
                1,
                TypeaheadCap,
                new Dictionary<string, object> { [SalesOrderNumberFilterKey] = q });

            return Ok(items);
        }

        /// <summary>
        /// Creates a Sales Order. Before copying this action, run §9's duplicate-key
        /// verification for your resource: the refusal must already live inside the service.
        /// For Sales Order it does NOT - UpsertPoAsync (MfgPoService.cs:985) never calls
        /// IsDuplicatePoAsync (MfgPoService.cs:964); the check is in the page
        /// (MfgPOUpsert.razor:3745-3753). See caveat 3 under this block.
        /// </summary>
        [HttpPost]
        [RequireRight(Right.Create)]
        [ProducesResponseType(typeof(MfgPoVM), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MfgPoVM>> Create([FromBody] MfgPoVM vm)
        {
            if (vm.PoId != 0)
                ModelState.AddModelError(nameof(MfgPoVM.PoId), "A new Sales Order must not carry an id. Use PUT to update.");

            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var created = await _salesOrders.UpsertPoAsync(vm);

            return CreatedAtAction(nameof(GetById), new { id = created.PoId }, created);
        }

        [HttpPut("{id:int}")]
        [RequireRight(Right.Edit)]
        [ProducesResponseType(typeof(MfgPoVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<MfgPoVM>> Update(int id, [FromBody] MfgPoVM vm)
        {
            if (id != vm.PoId)
                ModelState.AddModelError(nameof(MfgPoVM.PoId), "The id in the route and the id in the body must match.");

            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var updated = await _salesOrders.UpsertPoAsync(vm);

            return Ok(updated);
        }

        [HttpDelete("{id:int}")]
        [RequireRight(Right.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            var (canDelete, message) = await _salesOrders.CanDeleteSalesOrderAsync(id);
            if (!canDelete)
                return this.BusinessRuleProblem(message);

            var deleted = await _salesOrders.DeletePOByPOIdAsync(id);
            if (!deleted)
                return this.NotFoundProblem("Sales Order not found.");

            return NoContent();
        }

        [HttpPost("{id:int}/short-close")]
        [RequireRight(Right.Edit)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> ShortClose(int id)
        {
            var order = await _salesOrders.GetPoByPoIdAsync(id);
            if (order is null)
                return this.NotFoundProblem("Sales Order not found.");

            await _salesOrders.UpsertSalesOrderShortCloseAsync(order);

            return NoContent();
        }

        [HttpGet("{id:int}/print")]
        [RequireRight(Right.View)]
        [Produces(PdfContentType)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Print(int id)
        {
            var pdf = await _reports.Generate_Report(id, PrintTemplate, PrintParameter, false, ScreenName, PrintProcedure);
            if (pdf is null || pdf.Length == 0)
                return this.NotFoundProblem("Sales Order not found.");

            return File(pdf, PdfContentType, $"sales-order-{id}.pdf");
        }
    }
}
```

### Four honest caveats on the reference, so you do not copy a wrong thing

Every action above is the settled *shape*. Four of them additionally depend on something that
is true per resource, and the reference is written over `IMfgPoService` — a real service with
real gaps — precisely so those dependencies are visible rather than assumed. **Caveat 3 is the
one that silently loses a business rule if you skip it.**

1. **The print action is a stub, and its three constants are placeholders.** The real
   `.frx` template, parameter and stored-procedure names per document, plus the whole print
   route contract, are [M2-B08](../execution/tasks/M2-B08.md)'s (`KB-110`). Take the values
   from the resource's own Blazor print call site; do not guess them. The shape above —
   `GET /{id:int}/print`, `[RequireRight(Right.View)]`, `application/pdf`,
   `FileContentResult` — is the part that is settled. `ReportService` is a concrete class, not
   an interface, and is registered by `AddVSmartDomain()`
   (`V.SMART/V.SMART.Shared/DependencyInjection/ServiceCollectionExtensions.cs:324`); inject it
   as-is rather than inventing an interface for it.
2. **`ShortClose` compiles, but it is not yet correct for Sales Order specifically**, and this
   is the most instructive thing in the file. `UpsertSalesOrderShortCloseAsync` takes the whole
   VM and persists the `ShortClose` flag it is given — and the *decision to flip that flag*
   currently lives in the Razor page:
   `MfgPOUpsert.razor:3294` does `MfgPoVMs.ShortClose = !MfgPoVMs.ShortClose;` immediately
   before calling the service at `:3295`. Load-and-call, as written above, would therefore
   persist the flag unchanged. **This is exactly the ADR-002 §3 anti-pattern in the wild**: the
   business decision is on the client. The endpoint may only ship once that decision has moved
   into the service (that module's `@code`-extraction wave). The template's rule stands
   regardless: *one endpoint, one service call, the whole sequence server-side.* See §7.
3. **`Create` compiles, and for Sales Order it would create a duplicate the live Blazor screen
   refuses.** `MfgPOUpsert.razor:3745-3753` calls
   `IsDuplicatePoAsync(PONo, Suffix, PoId, CustId)` for a new PO and, on `true`, refuses the
   save — *"PO No '…' with Suffix '…' already exists."* — while
   `MfgPoService.UpsertPoAsync` (`MfgPoService.cs:985`) never calls it; the only other
   references to the method in that file are its own declaration (`:964`) and its error log
   (`:980`). So `Create` as written above is the correct *shape* and the wrong *behaviour for
   this resource*: same class of defect as caveat 2, and the same fix — the check moves into
   the service, not into the controller (§9's duplicate-key decision explains why the
   controller is the wrong place). **Do not copy `Create` for any resource without running
   §9's per-resource duplicate-key verification first.** Where the service already refuses
   internally — `CurrencyService.cs:108` and `:152` return
   `(false, "Currency name already exists.", null)` — there is nothing to do and `Create` is
   correct as written.
4. **The typeahead is served from the paged search, because this resource has no dedicated
   `Search…Async(string)`.** `IMfgPoService` exposes `SearchCustomersAsync` and
   `SearchItemsAsync` — *other* resources' typeaheads consumed by the Sales Order screen —
   but nothing that searches Sales Orders by number, so `Search` above calls
   `SearchWithDynamicFilterAsync(1, TypeaheadCap, …)` with the resource's own filter key
   (`"Po"`, `MfgPoService.cs:346`, which does `PONo.StartsWith`) and discards the count. That
   is the documented fallback in §9, not a second contract: the route, the cap, the bare-array
   response and the `[ProducesResponseType]` set are identical either way. Where the service
   *does* expose a typeahead of its own, call it directly.

---

## 3. Route conventions

| Rule | Form | Evidence |
|---|---|---|
| Version prefix | `[Route($"{ApiRoutes.V1}/…")]` — **never** a literal `"api/v1"` | `ApiRoutes.cs:29` (`const string V1 = "api/v1"`) |
| Resource segment | plural **kebab-case**: `sales-orders`, `purchase-orders`, `currencies` | ADR-002 §2; `ApiRoutes.cs:21-23` |
| Id constraint | always `{id:int}` — never a bare `{id}` | `CurrencyController.cs:70,94,108` |
| Collection | `GET /` · `POST /` | ADR-002 §2 |
| Item | `GET|PUT|DELETE /{id:int}` | ADR-002 §2 |
| Workflow command | `POST /{id:int}/{verb}` — kebab-case verb (`short-close`, `cancel`, `approve`) | ADR-002 §3 |
| Print | `GET /{id:int}/print` → `application/pdf` | ADR-002 §2 |
| Typeahead | `GET /search?q=…` — see §9 | decided here |
| Sub-collection | `GET /{id:int}/{child-plural}` | ADR-002 §2 (by extension) |

`[ApiController]` is mandatory: it is what makes model binding produce the automatic 400
through M2-A06's `InvalidModelStateResponseFactory`
(`V.SMART/V.SMART.Api/Middleware/ErrorContractExtensions.cs:21-25`).

A literal `"api/v1"` in a controller is a **review reject** — the whole point of the constant
is that the version string exists in exactly one place when a `v2` is needed
([KB-040 §Versioning](api-overview.md#versioning-m2-b01)).

Two controllers may share a route prefix when their action templates cannot collide —
`CurrencyController` (`""`, `{id:int}`) and `CurrencyExcelController` (`export`, `import`,
`import-template`) both sit on `api/v1/currencies` (`CurrencyExcelController.cs:38`). Prefer
one controller per resource; a second is justified only when the first cannot carry the
attributes the second needs.

---

## 4. Authorization

**ADR-004's Non-negotiable, quoted:** *"No controller ships without `[RequireScreen]` +
`[RequireRight]` and its permission-matrix test. This is the one rule in this knowledge base
with no exceptions."*

| Placement | Attribute | Notes |
|---|---|---|
| class | `[Authorize]` | authentication. Present even when every action is also screen-gated |
| class | `[RequireScreen("<seeded screen name>")]` | exactly one screen per controller; `AllowMultiple = false` (`RequireScreenAttribute.cs:13`). *A controller that needs two screens is two controllers.* |
| action | `[RequireRight(Right.View\|Create\|Edit\|Delete)]` | exactly one right; `AllowMultiple = false` (`RequireRightAttribute.cs:10`), because `RightsHelper` offers no combining rule (KB-105 D-4) |
| action or class | `[NoScreenRight("justification")]` | the auditable opt-out for an authenticated endpoint that legitimately has no screen (`NoScreenRightAttribute.cs:13-21`). **Not** a substitute for `[AllowAnonymous]`: authentication still applies |
| action | `[AllowAnonymous]` | login only |

`Right` has **four** members, not five — `IsHide` is deliberately not expressible as a required
right (`Authorization/Right.cs:13-19`).

**Deny by default.** A missing `UserRight` row means no right (ADR-004 §1). Rights are read per
request from the tenant database with a short cache, never from the JWT (ADR-004 §2).

**Verb → right mapping** (follow it unless the screen genuinely differs):

| Action | Right |
|---|---|
| `GET` list, `GET /{id}`, `GET /search`, `GET /{id}/print` | `Right.View` |
| `POST /` | `Right.Create` |
| `PUT /{id}` | `Right.Edit` |
| `POST /{id}/{verb}` (workflow command) | `Right.Edit` |
| `DELETE /{id}` | `Right.Delete` |

### Finding the right screen name

The screen name is a **free-text string matched ordinally and case-sensitively** against
`Screens.ScreenName` (`RequireScreenAttribute.cs:4-8`, KB-105 D-1). A one-character slip
silently denies every call to the controller in every tenant.

1. The names are seeded at
   `V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs:1151-1325` — **152 rows**, `Id ==
   ScreenCode` for every one.
2. The compile-time copy the API validates against is
   `V.SMART/V.SMART.Api/Authorization/ScreenCatalogue.cs:35` — **150 names**. Two seeded rows
   (`ScreenCode` 114 and 115) are deleted by a later migration and are deliberately absent, so
   naming either fails at startup instead of denying forever (M2-A09, R-65).
3. Cross-check the Blazor page for the same screen: both Currency pages declare
   `ScreenName = "Currency"` (`CurrencyList.razor:252`, `CurrencyUpsert.razor:135`), which is
   how you confirm you picked the screen the users actually have rights on — note
   `"Currency"` (`ScreenCatalogue.cs:41`) and `"Currency Today"` (`:57`) are different screens.
4. **The seed's own misspellings are canonical** — `"Sub-Contrect GRN"`, `"Advaceadjustment"`
   (KB-105 F-8). Do not "correct" them.
5. `[RequireScreen("…", Seeded = false)]` exists for a screen deliberately absent from the
   catalogue and requires written justification at review (`RequireScreenAttribute.cs:25-32`).

`M2-B05` will replace the hand-copied catalogue with a generated one (R-10). When it lands,
use its typed constants instead of a string literal; until then the literal is correct and the
startup validator is the guard.

### The startup validator will refuse to boot a misannotated controller

`ScreenRightStartupValidator.Validate` (`Authorization/ScreenRightStartupValidator.cs:44`)
throws, naming every offender, when:

1. an action declares `[RequireRight]` but its controller declares no `[RequireScreen]`;
2. a controller declares `[RequireScreen]` and one of its actions declares neither
   `[RequireRight]` nor `[NoScreenRight]` and is not anonymous;
3. a declared screen name is absent from `ScreenCatalogue.SeededScreenNames` and the attribute
   did not set `Seeded = false`.

The reverse direction — an authenticated action on a controller with **no** `[RequireScreen]`
at all — is **not** yet enforced at runtime (`ScreenRightStartupValidator.cs:33-42`); it is
**Q-71**, and `M2-A03`'s permission-matrix harness enforces it as a *test* failure instead. So
a missing annotation cannot reach `master`, but the host would still serve it. Do not rely on
the host to catch you.

### Row scope

If any action returns rows of an entity registered in
`V.SMART/V.SMART.Api/Authorization/ScopedEntityCatalogue.cs` (today: `Leads`, and only
`Leads`), the action must declare either `[RowScoped(typeof(T))]` and apply
`ApplyRowScope`, or `[NoRowScope("justification")]`.
`RowScopeStartupValidator.Validate` (`Authorization/RowScopeStartupValidator.cs:46`) refuses to
start otherwise. A single out-of-scope row is answered with the **same 404 body** a missing row
gets, via `this.OutOfScopeProblem()` (`Middleware/ProblemResults.cs:62`) — a 403, or a
distinguishable 404, would confirm the row exists (KB-108 decision P8). For every other
resource this section is a no-op; do not add scoping where none exists today.

---

## 5. List endpoints — the M2-B02 contract

Three pieces, all in `V.SMART/V.SMART.Api/Contracts/`. The reference implementation is
`CurrencyController.GetAll` (`CurrencyController.cs:52-68`) with `CurrencyQuery.cs`.

**Request.** One `[FromQuery]` typed query record per resource, deriving from `PagedQuery`
(`Contracts/PagedQuery.cs:33`). Never `Dictionary<string, object>` at the boundary.

- `pageNumber` — default **1**, `[Range(1, int.MaxValue)]` (`PagedQuery.cs:58-61`).
- `pageSize` — default **20** (`:46`), maximum **100** (`:55`). The maximum is a
  denial-of-service bound, not a preference.
- `sort` — comma-separated camel-case fields, `-` prefix for descending
  (`sort=-createdDate,currName`). Parsed and allow-list-validated by
  `SortSpecification.TryParse` (`Contracts/SortSpecification.cs:38`).
- **Every property declares `[FromQuery(Name = …)]` from a `const`**, because the binder and
  Swashbuckle otherwise emit the C# property name into the OpenAPI document and M2-B10
  generates from that document (ADR-002 §2a).
- `IValidatableObject` member names are the **wire** names, never `nameof(Property)` — the
  member name becomes the `errors` key verbatim (`PagedQuery.cs:88-92`).
- Date filters are `DateTime?`, never `string?` (`CurrencyQuery.cs:64,69`).

**Sortable allow-list.** Per resource, explicit, derived from what the list screen shows —
never reflection (`CurrencyQuery.cs:45-48`). An unknown field is a **400 listing the permitted
values** (`SortSpecification.cs:107-113`).

**Sort reaching the service.** Only `ICurrencyService` currently has the four-argument
`SearchWithDynamicFilterAsync(int, int, Dictionary<string, object>?, string? sort)` overload
(`ICurrencyService.cs:32`); the other 133 declarations do not (KB-011, INV-041). Two legal
states, no third:

- the service **has** the overload → pass `query.ToServiceSort()` (`PagedQuery.cs:109`) and
  declare the real `Sortable` list, as `CurrencyController.cs:61-65` does;
- the service **does not** → call the three-argument member and declare
  `Sortable = Array.Empty<string>()`, so any `sort=` is a loud 400. Adding the overload is that
  module's own wave (pair the `*FilterBuilder` with a `*SortBuilder` that **throws** on an
  unknown field; `CurrencyService.cs:227-322` is the reference).

Never smuggle sort through the filter dictionary: every `*FilterBuilder.ApplyFilter` ends in
`_ => query`, so it would answer 200 and sort nothing (ADR-002 §2a, option 2, rejected).

**Filters.** One explicit adapter method per resource on `FilterDictionaryAdapter`. Keys are
the resource's `*FilterBuilder` switch labels — PascalCase entity property names, not the wire
names — and each key must be quoted against its `file:line`, because a wrong key fails
**silently** (`FilterDictionaryAdapter.cs:19-23`). Return `null` when nothing is filtered
(`:62`). Dates go over as `yyyy-MM-dd` invariant strings (`:34`).

**Response.** `PagedResult<T>(Items, TotalCount, PageNumber, PageSize)`
(`Contracts/PagedResult.cs:31`) — one generic type for the whole API. Never a controller-local
paged record: 60–80 of them become 60–80 interfaces in the generated client. There is
deliberately **no `totalPages`**. `TotalCount` is the count of the **filtered, unpaged** query.

---

## 6. The error contract

Every error body is `application/problem+json`. The single producer is
`V.SMART/V.SMART.Api/Middleware/ApiProblems.cs`; controllers reach it only through the
`ProblemResults` extension methods.

| Status | When | Body | Produce it with |
|---|---|---|---|
| 400 | model binding / `DataAnnotations` failure | `errors` dictionary keyed by field | automatic, or `this.ValidationProblemResult()` (`ProblemResults.cs:90`) |
| 401 | unauthenticated / expired token | minimal — deliberately uninformative | `this.UnauthenticatedProblem(title)` (`:74`) |
| 403 | screen-right denial | KB-105 §7.1 shape, `screen` + `right` extension members | the filter; never a controller |
| 404 | not found | minimal | `this.NotFoundProblem(message)` (`:48`) |
| **409** | **business-rule refusal** | `title` = the service's message **verbatim** | `this.BusinessRuleProblem(message)` (`:44`) |
| 500 | unhandled | `traceId` only — no message, type or stack, in any environment | the middleware; never a controller |

Additional statuses this API already defines, for the endpoints that need them: **413** upload
too large (`ProblemResults.cs:86`), **403** account gate — expired trial, device, platform
(`:69`), **400/503** tenant unresolved (`:82`). Every body carries `type` (a stable URI under
`https://api.v-smart.local/problems/`, `Middleware/ProblemTypes.cs:17`), `instance` and
`traceId` (`ApiProblems.cs:27-44`).

### 409 carries the service's message verbatim. Paraphrasing it is a defect.

When a delete guard or a create/update refusal returns `(false, message)`, that string is
**product UX written by the domain team**. ADR-002 §4: *"strings like 'Cannot delete this Sales
Order as a Sales DC transaction exists.' are product UX written by the domain team. They are
surfaced unchanged, never replaced with generic text."*

This is what preserves **BR-SO-001** — *a Sales Order cannot be deleted once any downstream
document exists* (KB-030, Confirmed). The rule lives in the service; the API's whole job is not
to damage it on the way out.

Concretely, all four of these are defects, and a reviewer should treat them as such:

- rewording the message ("This record is in use");
- prefixing or wrapping it ("Business rule violation: …");
- truncating it;
- **parsing** it to decide a different status code.

`ApiProblems.BusinessRuleRefusal` (`ApiProblems.cs:52`) puts the string straight into `title`
and nothing else touches it. The boolean is the signal; the string is payload. The known cost
of that rule is recorded rather than guessed at: a refusal tuple that actually carries
not-found or infrastructure semantics is reported as 409 — **Q-34** in
[`open-questions.md`](../open-questions.md). Do not "fix" that in your controller by
inspecting the string.

### 400 versus 409 — the distinction that produced the original mess

> **400 means the request was malformed. 409 means the request was well-formed and the domain
> refused it.**

Apply it like this:

| Ask | Then |
|---|---|
| Could the client have known the request was invalid without the database? | **400** |
| Did the answer require domain state — other rows, a document chain, a status? | **409** |
| Is it a missing/oversized/unparseable field, a bad `sort`, a route/body id mismatch? | **400** |
| Is it "you may not do this *now*, because of what else exists"? | **409** |

This is not academic. `CurrencyController` used to answer **400** for a delete-guard refusal;
M2-A06 changed it to 409 and recorded the change as deliberately breaking
(`CurrencyController.cs:112-114`). It also once returned two different 400 body shapes from one
endpoint — `ValidationProblem(ModelState)` in some actions and `BadRequest(new { message })` in
others — which is the inconsistency [R-24](../risks/technical-debt-register.md) recorded.
**Both are now fixed** (R-24 CLOSED 2026-08-20, M2-A06), and this document is what stops them
recurring: the anti-patterns are named below, so they can be grepped for.

### Never write these in a controller

| Anti-pattern | Why | Use instead |
|---|---|---|
| `BadRequest(new { message })` | a second, undocumented error shape; the generated client cannot type it | `this.ValidationProblemResult()` |
| `NotFound()` / `NotFound(object)` | no `type`, no `traceId` | `this.NotFoundProblem("…")` |
| `Conflict(...)` / `StatusCode(409, …)` | bypasses the verbatim-title rule | `this.BusinessRuleProblem(message)` |
| `Forbid()` / a hand-built 403 body | the 403 shape is frozen by KB-105 §7.1 and has one producer | the authorization filter |
| `try/catch` around the service call | the middleware owns 500 and the `traceId`; a catch here hides it | nothing — let it throw |
| `throw new Exception("…")` to signal a refusal | 1,107 `InvalidOperationException` sites in `BusinessLayer` already mix refusals with infrastructure faults (`ProblemResults.cs:22-28`) | return the tuple's message as 409 |

---

## 7. Workflow commands

ADR-002 §3: anything that is a business operation — cancel, short-close, approve, reject,
release, post — is a single `POST /{id}/{verb}` that runs the **entire** sequence server-side.
The client collects input and calls one endpoint. **The client never orchestrates a
multi-step business operation.**

```csharp
[HttpPost("{id:int}/short-close")]
[RequireRight(Right.Edit)]
// … ProducesResponseType set, including 409 …
public async Task<IActionResult> ShortClose(int id) { … }
```

- **Verb, not noun**, kebab-case, in the route: `short-close`, `cancel`, `approve`.
- **`Right.Edit`** unless the command has its own screen.
- Returns **204** when the command carries no payload, or **200** with the updated VM when the
  client needs the new state. Pick one per command and declare it.
- A refusal from the command's own guard is **409 with the message verbatim** — the same rule
  as delete. BR-SO-003's mandatory cancellation reason is *input*, collected by the client and
  posted in the command body; it is not orchestration.

**The anti-pattern, named.** `MfgPOUpsert.razor` sequences the business operation from the UI:
it decides the flag (`:3294`) then calls the service (`:3295`); elsewhere it checks
transactions (`:2180`) and separately reverts quantities and updates status (`:3414`). Four
client-side steps that must be one server-side call. A controller that reproduces that shape —
two mutating service calls, or a decision taken in the action body — has moved the defect from
Blazor to the API rather than removed it. Where the existing service still requires the client
to have decided, the endpoint waits for that module's extraction wave; it does not ship with
the decision in the controller.

---

## 8. Payloads

- **Use the existing `…VM` ViewModels, unchanged.** No parallel DTO hierarchy (ADR-002 §2).
  They already carry `DataAnnotations`, which is what produces the 400 for free.
- The **only** new types a controller may introduce are the M2-B02 query record (§5) and, where
  a command needs input, a small command record declared on the controller — as
  `AuthController` declares `LoginRequest`/`LoginResponse` (`AuthController.cs:65-74`).
- Do **not** add a property to a VM for the API's benefit; it is shared with Blazor Server.
- Casing is the ASP.NET Core default camel-case JSON policy. Do not configure it per
  controller.
- Decimals: `M2-C10` owns wire representation (INV-032). Until it lands, do not round or format
  in a controller.

---

## 9. Service method → HTTP verb

Derived from [KB-011 §Business service conventions](../architecture/backend-architecture.md#business-service-conventions-observed-consistent)
(INV-002) and settled here. **This table is the mapping; you should not have to think.**

| Service convention | Endpoint | Success | Notes |
|---|---|---|---|
| `SearchWithDynamicFilterAsync(int, int, Dictionary<string,object>?[, string? sort])` | `GET /` | 200 `PagedResult<T>` | typed query record + `FilterDictionaryAdapter` (§5) |
| `Get…ByIdAsync(int)` | `GET /{id:int}` | 200 `TVm` | `null` → 404 |
| `Upsert…Async(TVm)` | **`POST /` and `PUT /{id:int}`** | 201 + `Location` / 200 | **decided below** |
| `CreateAsync(TVm)` / `UpdateAsync(int, TVm)` → `(bool, string, TVm?)` | `POST /` / `PUT /{id:int}` | 201 + `Location` / 200 | `false` → 409, message verbatim |
| `CanDelete…Async(int)` → `(bool, string)` | consulted **before** `DELETE` | — | `false` → **409**, message verbatim (BR-SO-001) |
| `Delete…ByIdAsync(int)` | `DELETE /{id:int}` | 204 | `false` → 404 |
| `IsDuplicate…Async(…)` | **never an endpoint**, and **never called from a controller** | — | a 409 out of create/update **only where the service itself refuses** — **verify per resource, decided below** |
| `Search…Async(string)` typeahead | **`GET /search?q=…`** | 200 `IReadOnlyList<TVm>` | **decided below**; where no such method exists, the documented paged-search fallback |
| `GetNext…NoAsync(…)` | **not an endpoint** | — | document numbering is allocated server-side inside the create transaction ([M2-B12](../execution/tasks/M2-B12.md)) |
| `Generate_Report(…)` → `byte[]` | `GET /{id:int}/print` | 200 `application/pdf` | contract owned by M2-B08 |

### Decision — `Upsert…Async` is one method behind two verbs

`Upsert…Async(TVm)` is the dominant shape: **129 files** under
`V.SMART.Shared/BusinessLayer/` match `public.*UpsertAsync|Task<.*> Upsert` (measured
2026-08-24). The three-element `(bool Success, string Message, TVm? Entity)` create/update
split that `CurrencyController` uses is **unique to `CurrencyService`**
(`ICurrencyService.cs:35-36`) and was added for the API (INV-040). So the awkward case is the
common one.

**Decided: `POST /` and `PUT /{id:int}` both call the single `Upsert…Async`. The verbs are
distinguished at the controller, not in the service.**

- `POST /` — the body must carry **id `0`**. A non-zero id is a **400** with `errors` keyed by
  the id field. On success: `CreatedAtAction(nameof(GetById), new { id = created.<Id> },
  created)` → **201** with `Location`.
- `PUT /{id:int}` — the route id and the body id must be **equal**. Unequal is a **400**. On
  success: **200** with the returned VM.

**Why.** Splitting the service into `CreateAsync`/`UpdateAsync` would mean editing 129 live
business services that Blazor Server is running against — forbidden, and out of all proportion
to the gain. The id guards are HTTP semantics, not business logic: they say what the *verb*
promised, and they cost no round trip. `CurrencyService`'s split is honoured where it exists —
use whichever the interface offers, never both.

**One thing you must verify per resource before shipping `PUT`:** whether that service's
`Upsert…Async` *inserts* when handed an unknown non-zero id. The API preserves service
behaviour and does not add a pre-check by default. If an insert-on-unknown-id would be wrong
for your resource, add the `Get…ByIdAsync` existence check — it is a permitted extra read
(§10 T5) — and say so in the controller's task. Do not change the service to find out.

### Decision — a duplicate-key refusal is the **service's** to make, and you must verify it per resource

`IsDuplicate…Async` is **not** an endpoint and — this is the part the first draft of this
document got wrong — **a controller must not call it either**. But "it is a 409 out of
create/update" is only true for some services. The family has **43 declarations under 16
distinct names** in `V.SMART.Shared/BusinessLayer/` (measured 2026-08-24), and the check sits
in **three different places** depending on the service. All three are Confirmed:

| # | Where the check lives | Evidence | What the API does today |
|---|---|---|---|
| **a** | **Inside the service, as a refusal tuple** | `CurrencyService.cs:108` and `:152` → `(false, "Currency name already exists.", null)` | Correct already: the controller returns `BusinessRuleProblem(message)` → **409**, message verbatim. Nothing to do. |
| **b** | **Inside the service, as a `throw`** | `LeadService.cs:206` checks `IsDuplicateLeadsNameAsync`, `:209` throws `InvalidOperationException("Leads name already exists.")` | **Wrong status, silently.** The API deliberately does not map that exception type — `ProblemResults.cs:18-31` records why (1,107 `InvalidOperationException` throw sites mix refusals with infrastructure faults) — so it surfaces as **500**. |
| **c** | **Only in the Razor page** | `MfgPOUpsert.razor:3745-3753` calls `IsDuplicatePoAsync` and refuses the save; `MfgPoService.cs:985` `UpsertPoAsync` never calls it | **The rule is lost.** A `POST` copied from §2 creates the duplicate the live screen refuses. |

**Decided, and this is the obligation the frozen contract imposes on every controller author:**

1. **Before shipping `POST`/`PUT` for a resource, grep that resource's service and its Blazor
   upsert page for `IsDuplicate`.** Record which of a/b/c you found, in the controller's task.
   Finding nothing is a valid answer and is also recorded.
2. **Shape (a) — ship it.** Nothing to add.
3. **Shape (b) — do not ship the endpoint and do not "fix" it in the controller.** Catching the
   exception in an action violates T6, and re-classifying `InvalidOperationException` as 409 in
   the middleware would turn every infrastructure fault in `BusinessLayer` into a business
   refusal. Raise it; the status mapping is [Q-81](../open-questions.md).
4. **Shape (c) — the check must move into the service before the endpoint ships.** This is the
   standing project rule, not a new one: business logic in Razor `@code` is *extracted into a
   server-side service*, never reimplemented at the edge. It belongs to that module's
   `@code`-extraction wave, exactly like caveat 2's `ShortClose`.

**Why the controller may not simply call `IsDuplicate…Async` itself** — the tempting shortcut,
and the reason it is banned. The method returns a **bare `bool`** (`MfgPoService.cs:964`); it
authors no message. The only message that exists is **UX text in the page**
(`MfgPOUpsert.razor:3750`). A controller doing the check would therefore have to *compose* the
409 `title`, which §6 and [ADR-002 §4](../decisions/ADR-002-rest-api-layer.md#4-error-contract-applicationproblemjson-everywhere)
forbid and §10 T8 makes a checklist failure — and it would leave the rule unenforced for
Blazor Server, which is still live and still the authority. One rule, one place, and that
place is the service.

### Decision — typeahead is its own endpoint

**Decided: `GET /api/v1/{resource}/search?q=…`, not a mode of the list endpoint.**

The list endpoint's response is `PagedResult<T>` and its contract is frozen. A typeahead that
returned a bare array from the same route would give the generated TypeScript client two
response shapes for one operation, which is precisely the drift M2-B10 exists to remove. A
separate action gives one operation, one response type, one set of
`[ProducesResponseType]`. `search` cannot collide with `""` or `{id:int}`.

- `q` is required, minimum length 1; violations are the automatic 400.
- Unpaged, but **capped** — return at most the service's own limit, or 20, whichever is
  smaller. State the cap in the XML doc comment.
- `[RequireRight(Right.View)]`.

**Where the service has no `Search…Async(string)` of its own** — common; `IMfgPoService` has
typeaheads for *customers* and *items* but none for Sales Orders — serve the same endpoint from
`SearchWithDynamicFilterAsync(1, cap, …)` with exactly **one** filter key, the resource's own
identifying one, and discard the count (§2's `Search`, and `MfgPoService.cs:346` for the key).
The route, the cap, the bare-array response and the `[ProducesResponseType]` set are unchanged;
this is a fallback implementation, not a second contract. Do **not** add a `Search…Async` to a
live service just to satisfy the template.

### Negative results — deliberately not defined

Recorded so no future session invents them per controller:

- **Bulk create/update/delete: there is no service convention for it, so this template defines
  no endpoint shape for it.** A resource that genuinely needs one raises it as an ADR-002
  addendum first.
- **`GetNext…NoAsync` is not an endpoint.** Exposing it would let a client allocate a document
  number outside the create transaction, which is how duplicates happen. M2-B12 owns numbering.
- **No idempotency keys yet.** Deferred to [M2-B12-03](../execution/tasks/M2-B12-03.md). Do not
  invent a per-controller scheme; when it lands it will be a cross-cutting filter.
- **No `PATCH`.** No service method takes a partial VM.
- **No soft-delete endpoint.** `Delete…ByIdAsync` is what exists.

---

## 10. What "thin" means, testably

ADR-002 §2 says *"Controllers are thin: bind → authorize → one service call → map. No business
logic."* As written that is unfalsifiable at review. These criteria are not.

**An action body conforms when all of the following hold:**

| # | Criterion |
|---|---|
| T1 | No `if`, `switch` or ternary **on a domain value** — a VM field, an amount, a status, a date. Branching on a service's success/`CanDelete` boolean, or on `null`, is not a domain branch |
| T2 | No loop (`for`, `foreach`, LINQ `Select`/`Where`/`Sum`) over domain data |
| T3 | No arithmetic on money, quantity, tax, rate or any date-derived business value |
| T4 | **At most one mutating service call.** Two writes in one action is orchestration and belongs in the service |
| T5 | Reads limited to the action's own read, plus exactly two permitted extras: the delete guard (`CanDelete…Async` before `Delete…Async`) and loading the aggregate a command or upsert-existence check requires by signature |
| T6 | No `try`/`catch` — the middleware owns 500 and the `traceId` |
| T7 | No `DbContext`, repository or `IUnitOfWork` use. Business services only (the documented exception is `AuthController`, §12) |
| T8 | No string building of a user-facing business message. Every 409 title comes from the service |
| T9 | The permitted controller-side checks are exactly: `ModelState.IsValid`; route-id vs body-id equality; `null` → 404; refusal boolean → 409; row scope |

`CurrencyController.Delete` (`CurrencyController.cs:110-124`) is the worked example of T5's
first exception: two awaits, no domain branch, both required by the service's shape.

---

## 11. OpenAPI annotations — M2-B10 depends on this section

[M2-B10](../execution/tasks/M2-B10.md) generates the TypeScript client from the committed
`api/openapi.json`, which `tools/generate-openapi.sh` writes out of the compiled assembly with
`dotnet swagger tofile` — not from `/swagger/v1/swagger.json`, because Swagger UI is gated to
`Development` (KB-112). **A status code you do not declare does not exist for the client**:
the generated method has no type for it, and the Angular error handling has nothing to branch
on. This is the section most likely to be skipped and the one with the largest downstream cost.

Declare, on **every** action:

| Action | Required `[ProducesResponseType]` |
|---|---|
| `GET /` (list) | `PagedResult<TVm>` 200 · `ValidationProblemDetails` 400 · `ProblemDetails` 401, 403 |
| `GET /{id:int}` | `TVm` 200 · `ProblemDetails` 401, 403, 404 |
| `GET /search` | `IEnumerable<TVm>` 200 · `ValidationProblemDetails` 400 · `ProblemDetails` 401, 403 |
| `POST /` | `TVm` 201 · `ValidationProblemDetails` 400 · `ProblemDetails` 401, 403, 409 |
| `PUT /{id:int}` | `TVm` 200 · `ValidationProblemDetails` 400 · `ProblemDetails` 401, 403, 404, 409 |
| `DELETE /{id:int}` | 204 · `ProblemDetails` 401, 403, 404, 409 |
| `POST /{id}/{verb}` | 204 (or `TVm` 200) · `ProblemDetails` 401, 403, 404, 409 |
| `GET /{id:int}/print` | `FileContentResult` 200 + `[Produces("application/pdf")]` · `ProblemDetails` 401, 403, 404 |

Rules:

- **400 is typed `ValidationProblemDetails`**, everything else `ProblemDetails`. They are
  different schemas — only the 400 carries `errors`.
- Declare 401 and 403 on every authenticated action even though the filter produces them; the
  client needs the type.
- Do not declare a status the action cannot return. An over-declared 409 on a `GET` is as much
  a defect as a missing one — it produces dead branches in the generated client.
- `[Produces("…")]` only where the media type is not JSON (print, export). **Amended 2026-08-24
  (M2-B10) — do not use `[Produces]` for this.** `ProducesAttribute` constrains the output
  formatters for the WHOLE action, including its `problem+json` error responses, so it is a
  behaviour change and not metadata. Declare the media type on the response instead:
  `[ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK, "application/…")]`,
  which is what `CurrencyExcelController` and `FilesController` now carry.
- The XML doc comment on the action becomes the operation summary. Write it for the client
  developer, not for yourself.

### Added 2026-08-24 by M2-B10 — two more obligations, both machine-checked

The client is now generated from this document's output, so two things that used to be
implicit are mandatory and are asserted by
`tests/V.SMART.Api.Tests/OpenApiConformanceTests.cs`:

- **An explicit operation id on every action**, written as the route name:
  `[HttpGet("{id:int}", Name = "getCurrencyById")]`. It becomes the generated TypeScript
  method name (`currencies.getCurrencyById({ id })`). Convention: camelCase
  `verb + Resource`, unique across the whole API. **Renaming one is a breaking change to
  every SPA call site** — it is API surface, not an implementation detail, and it must not
  change merely because a C# method was renamed.
- **An explicit `[Tags("…")]` on every controller**, so the client groups by RESOURCE and a
  class rename cannot silently regroup it. Two controllers serving one resource share one
  tag: `CurrencyController` and `CurrencyExcelController` are both `[Tags("Currency")]`.

After any of this changes, run `bash tools/generate-api-client.sh` and commit the regenerated
`api/openapi.json` and client with the controller change. CI fails on drift. The whole
procedure, the generator comparison and the `decimal` → `number` finding are in
[KB-112](generated-client.md).

---

## 12. Conformance checklist

Run this against a new controller before approving it. Every item is objectively checkable by
reading the file; no judgement calls.

**Routing**

1. `[ApiController]` on the class.
2. Route composed from `ApiRoutes.V1`; **no literal `"api/v1"`** anywhere in the file.
3. Resource segment is plural kebab-case.
4. Every id route parameter is `{id:int}`.
5. Command routes are `POST /{id:int}/{kebab-verb}`.

**Authorization**

6. `[Authorize]` on the class.
7. `[RequireScreen("…")]` on the class, and the string is present in
   `ScreenCatalogue.SeededScreenNames` (or `Seeded = false` with a written justification).
8. Every action has exactly one of `[RequireRight(...)]`, `[NoScreenRight("justification")]`,
   `[AllowAnonymous]`.
9. Rights follow §4's verb table.
10. If any action returns a `ScopedEntityCatalogue` entity, it declares `[RowScoped]` or
    `[NoRowScope("justification")]`.
11. The controller has its row in the permission matrix
    (`tests/V.SMART.Api.Tests/PermissionMatrix/`) — ADR-004 §6, non-negotiable.

**Contract**

12. List action takes one `[FromQuery]` record deriving from `PagedQuery`, and returns
    `PagedResult<T>`. No controller-local paged record.
13. Every query property declares `[FromQuery(Name = …)]` from a `const`.
14. `Sortable` is either the real allow-list (service has the sort overload) or
    `Array.Empty<string>()` (it does not). Never a list the service cannot honour.
15. Filter keys are quoted against the resource's `*FilterBuilder` `file:line`.
16. Payloads are existing `…VM` types; no new DTO where a VM exists.
17. `POST` rejects a non-zero body id; `PUT` rejects a route/body id mismatch. Both as 400.
18. `POST` returns `CreatedAtAction` with a `Location`; `DELETE` returns 204.

**Errors**

19. Every error return goes through a `ProblemResults` helper. `grep` the file for
    `BadRequest(`, `NotFound(`, `Conflict(`, `StatusCode(`, `Forbid(` — **zero hits**.
20. Delete calls `CanDelete…Async` first and returns `BusinessRuleProblem(message)` on refusal.
21. No message string is reworded, prefixed or truncated between the service and `title`.
22. No `try`/`catch` in any action.
22a. **Duplicate-key verification recorded** (§9). The controller's task states the result of
    grepping the resource's service *and* its Blazor upsert page for `IsDuplicate` — shape
    (a), (b), (c) or none. `POST`/`PUT` may only ship on (a) or none. `grep` the controller
    for `IsDuplicate` — **zero hits**; the check belongs to the service.

**OpenAPI**

23. Every action declares the full `[ProducesResponseType]` set from §11, an explicit operation
    id (route `Name`) and an explicit controller `[Tags("…")]` (M2-B10).
24. 400 is typed `ValidationProblemDetails`; the rest `ProblemDetails`.

**Thin**

25. Every action satisfies T1–T9 (§10).

---

## 13. The two worked examples, verified

The template is only real once two independent controllers pass it
([KB-080 §9 Definition of Done](../execution/README.md#definition-of-done)). Both were checked
item by item on 2026-08-24 against the code as it then stood.

### 13.1 `CurrencyController` — conforms

`V.SMART/V.SMART.Api/Controllers/CurrencyController.cs`, 126 lines, read in full.

| Items | Result |
|---|---|
| 1–5 routing | **Pass.** `[ApiController]` `:11`; `[Route($"{ApiRoutes.V1}/currencies")]` `:12`; plural kebab; `{id:int}` at `:70,94,108`. No command routes |
| 6–9 authorization | **Pass.** `[Authorize]` `:13`; `[RequireScreen("Currency")]` `:21`, present at `ScreenCatalogue.cs:41`; `[RequireRight]` on all five actions `:53,71,81,95,109`; rights match §4 |
| 10 row scope | **N/A.** `Currency` is not in `ScopedEntityCatalogue` |
| 11 permission matrix | **Pass.** `tests/V.SMART.Api.Tests/CurrencyAuthorizationTests.cs` and `PermissionMatrix/` |
| 12–18 contract | **Pass.** `CurrencyQuery`/`PagedResult<CurrencyVM>` `:56,67`; wire names from `const` (`CurrencyQuery.cs:26-35`); real `Sortable` (`:45-48`) because the service has the sort overload; filter keys quoted to `CurrencyService.cs:193-207`; `CurrencyVM` payload; `CreatedAtAction` `:91` |
| 17 id guards | **Partial — the one divergence, see below** |
| 19–22 errors | **Pass.** Only `ProblemResults` helpers: `ValidationProblemResult()` `:59,85,99`, `NotFoundProblem` `:76,121`, `BusinessRuleProblem` `:89,103,117`. `CanDelete` before delete `:115-117`. No `try`/`catch` |
| 22a duplicate-key | **Pass — shape (a).** `CurrencyService.cs:108` (create) and `:152` (update) do the check inside the service and return `(false, "Currency name already exists.", null)`, which the controller surfaces as a 409 at `:89,103`. Zero `IsDuplicate` hits in the controller |
| 23–24 OpenAPI | **Divergence, see below** |
| 25 thin | **Pass.** Delete is T5's documented two-await exception |

**Divergence D-1 — id guards (item 17).** `Create` (`:82`) does not reject a body carrying a
non-zero `CurrId`, and `Update` (`:96`) does not compare the route id with the body id. It is
not a hole in this controller — `ICurrencyService` has a genuine `CreateAsync`/`UpdateAsync(int
id, …)` split (`ICurrencyService.cs:35-36`), so the route id is what the service acts on and
the body id is ignored rather than trusted. **Resolution: no template change and no exception.**
The template's rule (§9) exists for the 129 `Upsert…Async` services where the body id *is* what
the service acts on. Adding the guards here would be an improvement, not a correction — raise
it as a follow-up on the Currency module, not in `M2-B03`.

**Divergence D-2 — `[ProducesResponseType]` coverage (items 23–24). CLOSED 2026-08-24 by
[M2-B10](../execution/tasks/M2-B10.md)**, which was the follow-up this paragraph asked for:
all five Currency actions, and `AuthController.Login`, now declare their full status set, so
the generated client has the 404 on `GET /currencies/{id}` and the 409 on `DELETE` that the
document used to lack. One judgement recorded there rather than repeated: `PUT` declares **no**
404, because `UpdateAsync` reports a missing row through the same refusal tuple as a
business-rule refusal and the action answers 409. The original finding, for the record: only
`GetAll` declared them (`:54-55`), omitting 401/403; `GetById`, `Create`, `Update` and `Delete`
declared none.
**Resolution at M2-B03: no template change — the template was right and the controller was
behind it.**
This matters concretely: M2-B10 generates from the OpenAPI document, so today's document has no
404 for `GET /currencies/{id}` and no 409 for `DELETE`. Raised as a follow-up for whoever owns
Currency next; it is out of `M2-B03`'s scope, which ships no code.

**Stale comment, noted not fixed.** `CurrencyController.cs:17` cites `ScreenCatalogue.cs:37`
for `"Currency"`; the name is now at `ScreenCatalogue.cs:41`. Harmless drift in a comment,
inside a file `M2-B03` may not touch.

### 13.2 `AuthController` — documented exception

`V.SMART/V.SMART.Api/Controllers/AuthController.cs`, 171 lines, read in full. It exposes one
endpoint: `POST /api/v1/auth/login` (`:76-78`).

**It is not a resource controller and must not be forced into the template.** ADR-002 §5 writes
the token endpoint exactly this way. The exception is granted for these items and no others:

| Item | Status | Justification |
|---|---|---|
| 3 plural kebab resource | **Exception.** `auth` is a singular non-collection segment | ADR-002 §5 writes `POST /api/v1/auth/login`; `ApiRoutes.cs:21-24` names `auth` as the one non-collection segment |
| 6 `[Authorize]`, 7 `[RequireScreen]` | **Exception.** Neither is present | Login is the endpoint that *establishes* identity. Gating it on a screen right would deadlock authentication. `[AllowAnonymous]` `:77` is the correct and audited marker, and `ScreenRightStartupValidator.cs:60-63` skips anonymous actions by design |
| 12–18 resource contract | **N/A.** No collection, no id, no VM payload | The `LoginRequest`/`LoginResponse` records (`:65-74`) are §8's permitted command records |
| 22a duplicate-key | **N/A.** No `POST`/`PUT` over a resource, and zero `IsDuplicate` hits in the file | The check applies to create/update of a keyed resource |
| 23–24 OpenAPI | **Met since 2026-08-24 (M2-B10).** `Login` declares 200 `LoginResponse`, 400 `ValidationProblemDetails`, 401 and 403 `ProblemDetails`, plus an operation id (`login`) and `[Tags("Auth")]` | Was *not met*: its 400 (tenant unresolved), 401 and 403 (account gate) were invisible to the generated client. Closed with D-2 |
| T7 (no `IUnitOfWork`) | **Exception.** It injects `IUnitOfWork` (`:29`) and calls `Users.LoginAsync` (`:92`) | Authentication is not a business service; there is no `IAuthService` and inventing one would mean editing `V.SMART.Shared`. This exception is granted to `AuthController` **only** — a resource controller touching `IUnitOfWork` is a reject |

**Items it does pass, and which every controller must:** 1, 2 (`[Route($"{ApiRoutes.V1}/auth")]`
`:13`), 8 (`[AllowAnonymous]`), 19–22 — every error goes through a helper
(`TenantUnresolvedProblem` `:85`, `UnauthenticatedProblem` `:94`, `AccountGateProblem` `:109`),
and no error is a bare `BadRequest`.

**Also carried by this controller, and out of scope here but not to be "tidied":** the
`AdministratorUserId = 1` rights-seeding gate (`:27`, *"Do not generalise, do not make
configurable"*), the trial gate (`:107-109`), and the deliberately swallowed seeding failure
(`:149-169`). All three are decided behaviour with recorded reasoning.

### 13.3 The rest of the API surface

Four more controllers exist that predate this document but postdate the task that specified it:
`CurrencyExcelController`, `FilesController`, `MeController`, `ReferenceController`. They were
**not** checked item by item here — `M2-B03`'s acceptance names two — but all four already
carry `[RequireScreen]`/`[RequireRight]` or `[NoScreenRight]`, and `MeController` and
`ReferenceController` are the worked examples of the `[NoScreenRight]` opt-out. Treat them as
additional precedent, not as verified conformance.

---

## 14. Where the rest lives

| You need | Read |
|---|---|
| Why the API is REST, and the decisions this document implements | [ADR-002](../decisions/ADR-002-rest-api-layer.md) |
| Why authorization is server-side, and the non-negotiable rule | [ADR-004](../decisions/ADR-004-server-side-authorization.md) |
| What endpoints exist today, and their exact bodies | [KB-040](api-overview.md) |
| What is still to build, and the estimates | [KB-041](api-readiness-assessment.md) |
| Screen-right semantics, the 403 body, the traps | [KB-105](../architecture/server-side-authorization-spec.md) |
| Row scope and account gates | [KB-108](../architecture/row-scope-and-account-gates.md) |
| Whether a document can be deleted, and which guard enforces it | [KB-061](../risks/delete-guard-audit.md) — **read before writing any `DELETE`** |
| Service conventions in the business layer | [KB-011](../architecture/backend-architecture.md) |
| Reference data and caching | [KB-124](reference-data-and-caching.md) |
| Report and print contract | `M2-B08` / `KB-110` (not yet written) |
