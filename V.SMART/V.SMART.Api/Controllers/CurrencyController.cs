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
        [HttpGet]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(PagedResult<CurrencyVM>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
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

        [HttpGet("{id:int}")]
        [RequireRight(Right.View)]
        public async Task<ActionResult<CurrencyVM>> GetById(int id)
        {
            var vm = await _currencyService.GetByIdAsync(id);
            if (vm == null)
                return this.NotFoundProblem("Currency not found.");
            return Ok(vm);
        }

        [HttpPost]
        [RequireRight(Right.Create)]
        public async Task<ActionResult<CurrencyVM>> Create([FromBody] CurrencyVM vm)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var (success, message, currency) = await _currencyService.CreateAsync(vm);
            if (!success)
                return this.BusinessRuleProblem(message);

            return CreatedAtAction(nameof(GetById), new { id = currency!.CurrId }, currency);
        }

        [HttpPut("{id:int}")]
        [RequireRight(Right.Edit)]
        public async Task<ActionResult<CurrencyVM>> Update(int id, [FromBody] CurrencyVM vm)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var (success, message, currency) = await _currencyService.UpdateAsync(id, vm);
            if (!success)
                return this.BusinessRuleProblem(message);

            return Ok(currency);
        }

        [HttpDelete("{id:int}")]
        [RequireRight(Right.Delete)]
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
