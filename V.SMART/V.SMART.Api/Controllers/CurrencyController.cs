using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;

namespace V.SMART.Api.Controllers
{
    [ApiController]
    [Route("api/currencies")]
    [Authorize]
    public class CurrencyController : ControllerBase
    {
        private readonly ICurrencyService _currencyService;

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        public record PagedCurrencyResponse(
            List<CurrencyVM> Items,
            int TotalCount,
            int PageNumber,
            int PageSize);

        [HttpGet]
        public async Task<ActionResult<PagedCurrencyResponse>> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? currName = null,
            [FromQuery] string? createdBy = null,
            [FromQuery] string? fromDate = null,
            [FromQuery] string? toDate = null)
        {
            var filters = new Dictionary<string, object>();
            if (!string.IsNullOrWhiteSpace(currName)) filters["CurrName"] = currName;
            if (!string.IsNullOrWhiteSpace(createdBy)) filters["CreatedBy"] = createdBy;
            if (!string.IsNullOrWhiteSpace(fromDate)) filters["FromDate"] = fromDate;
            if (!string.IsNullOrWhiteSpace(toDate)) filters["ToDate"] = toDate;

            var (items, totalCount) = await _currencyService.SearchWithDynamicFilterAsync(
                pageNumber,
                pageSize,
                filters.Count > 0 ? filters : null);

            return Ok(new PagedCurrencyResponse(items, totalCount, pageNumber, pageSize));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CurrencyVM>> GetById(int id)
        {
            var vm = await _currencyService.GetByIdAsync(id);
            if (vm == null)
                return NotFound(new { message = "Currency not found." });
            return Ok(vm);
        }

        [HttpPost]
        public async Task<ActionResult<CurrencyVM>> Create([FromBody] CurrencyVM vm)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var (success, message, currency) = await _currencyService.CreateAsync(vm);
            if (!success)
                return BadRequest(new { message });

            return CreatedAtAction(nameof(GetById), new { id = currency!.CurrId }, currency);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CurrencyVM>> Update(int id, [FromBody] CurrencyVM vm)
        {
            if (!ModelState.IsValid)
                return ValidationProblem(ModelState);

            var (success, message, currency) = await _currencyService.UpdateAsync(id, vm);
            if (!success)
                return BadRequest(new { message });

            return Ok(currency);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var (canDelete, message) = await _currencyService.CanDeleteCurrencyAsync(id);
            if (!canDelete)
                return BadRequest(new { message });

            var deleted = await _currencyService.DeleteCurrencyByCurrIdAsync(id);
            if (!deleted)
                return NotFound(new { message = "Currency not found." });

            return NoContent();
        }
    }
}
