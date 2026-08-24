using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using V.SMART.Api.Contracts;
using V.SMART.Api.Middleware;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;

namespace V.SMART.Api.Controllers
{
    [ApiController]
    [Route($"{ApiRoutes.V1}/currencies")]
    [Authorize]
    // M2-A02 - the first resource controller whose screen right is enforced by the API and not
    // only by the Blazor UI (ADR-004 section 1; risk R-03). The literal is byte-identical to the seeded
    // Screens.ScreenName at ApplicationDbContext.cs:1155 (Id = 5, ScreenCode = 5), to
    // ScreenCatalogue.cs:37, and to the ScreenName both Blazor Currency pages declare
    // (CurrencyList.razor:252, CurrencyUpsert.razor:135). It is NOT the distinct "Currency Today"
    // screen (ScreenCatalogue.cs:53). Matching is ordinal and case-sensitive (KB-105 D-1), so a
    // one-character slip here would silently deny every Currency call in every tenant.
    [RequireScreen("Currency")]
    // M2-B10 — the OpenAPI tag is declared, not inherited from the class name, so the generated
    // client groups by RESOURCE. CurrencyExcelController carries the same tag on purpose: both
    // serve /api/v1/currencies, and a caller should not have to know the server split them.
    [Tags("Currency")]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        /// <summary>
        /// M2-B02 — the reference implementation of the paged list contract (ADR-002 §2 and its
        /// M2-B02 addendum). Every future list endpoint copies this shape: one
        /// <c>[FromQuery]</c> typed query record in, one <see cref="PagedResult{T}"/> out, the
        /// service's filter dictionary produced by <see cref="FilterDictionaryAdapter"/> and
        /// never seen on the wire.
        ///
        /// <para>The controller-local paged-response record it replaces is deleted: 60–80
        /// copies of it would become 60–80 interfaces in the generated TypeScript client. The
        /// JSON is unchanged — the four property names were already ADR-002's.</para>
        ///
        /// <para><b>Behaviour change to note:</b> the default <c>pageSize</c> is now the
        /// contract-wide 20, where this endpoint alone previously defaulted to 10. Callers that
        /// send <c>pageSize</c> explicitly are unaffected.</para>
        ///
        /// <para>Validation is declarative on <see cref="CurrencyQuery"/>, so an invalid
        /// <c>pageNumber</c>/<c>pageSize</c>/<c>sort</c>/date range is rejected by M2-A06's
        /// <c>InvalidModelStateResponseFactory</c> before this method runs. The explicit
        /// <c>ModelState</c> guard below is the belt-and-braces path for a caller that invokes the
        /// action directly (unit tests), and matches <c>Create</c>/<c>Update</c>.</para>
        /// </summary>
        [HttpGet(Name = "getCurrencies")]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(PagedResult<CurrencyVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<CurrencyVM>>> GetAll([FromQuery] CurrencyQuery query)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var (items, totalCount) = await _currencyService.SearchWithDynamicFilterAsync(
                query.PageNumber,
                query.PageSize,
                FilterDictionaryAdapter.ForCurrency(query),
                query.ToServiceSort());

            return Ok(new PagedResult<CurrencyVM>(items, totalCount, query.PageNumber, query.PageSize));
        }

        /// <summary>
        /// One currency by id. Answers 404 in the canonical problem shape when the row does not
        /// exist.
        /// </summary>
        [HttpGet("{id:int}", Name = "getCurrencyById")]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(CurrencyVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CurrencyVM>> GetById(int id)
        {
            var vm = await _currencyService.GetByIdAsync(id);
            if (vm == null)
                return this.NotFoundProblem("Currency not found.");
            return Ok(vm);
        }

        /// <summary>
        /// Creates a currency. A duplicate name is the service's refusal, surfaced as 409 with the
        /// service's message verbatim in <c>title</c> (ADR-002 §4); a malformed body is 400 with the
        /// per-field <c>errors</c> dictionary.
        /// </summary>
        [HttpPost(Name = "createCurrency")]
        [RequireRight(Right.Create)]
        [ProducesResponseType(typeof(CurrencyVM), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CurrencyVM>> Create([FromBody] CurrencyVM vm)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var (success, message, currency) = await _currencyService.CreateAsync(vm);
            if (!success)
                return this.BusinessRuleProblem(message);

            return CreatedAtAction(nameof(GetById), new { id = currency!.CurrId }, currency);
        }

        /// <summary>
        /// Updates the currency identified by the route id. The route id is what the service acts
        /// on; any <c>currId</c> in the body is ignored.
        /// </summary>
        [HttpPut("{id:int}", Name = "updateCurrency")]
        [RequireRight(Right.Edit)]
        [ProducesResponseType(typeof(CurrencyVM), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        // No 404 here: UpdateAsync reports a missing row through the same (success, message)
        // refusal path as a business-rule refusal, so this action answers 409, never 404.
        // Declaring a status the action cannot return is as much a defect as omitting one -- it
        // puts a dead branch in the generated client (KB-114 s11).
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CurrencyVM>> Update(int id, [FromBody] CurrencyVM vm)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var (success, message, currency) = await _currencyService.UpdateAsync(id, vm);
            if (!success)
                return this.BusinessRuleProblem(message);

            return Ok(currency);
        }

        /// <summary>
        /// Deletes a currency. A delete guard's refusal is 409 carrying the service's message
        /// verbatim (BR-SO-001), not 400.
        /// </summary>
        [HttpDelete("{id:int}", Name = "deleteCurrency")]
        [RequireRight(Right.Delete)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Delete(int id)
        {
            // M2-A06 — ADR-002 §4: a delete guard's refusal is 409, not 400, and the service's
            // message is carried into ProblemDetails.title VERBATIM (BR-SO-001). Deliberate
            // breaking change: this endpoint answered 400 before this task.
            var (canDelete, message) = await _currencyService.CanDeleteCurrencyAsync(id);
            if (!canDelete)
                return this.BusinessRuleProblem(message);

            var deleted = await _currencyService.DeleteCurrencyByCurrIdAsync(id);
            if (!deleted)
                return this.NotFoundProblem("Currency not found.");

            return NoContent();
        }
    }
}
