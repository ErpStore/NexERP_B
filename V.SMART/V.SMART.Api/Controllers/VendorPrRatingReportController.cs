using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using V.SMART.Api.Authorization;
using V.SMART.Api.Contracts;
using V.SMART.Api.Middleware;
using V.SMART.Api.Reporting;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;

namespace V.SMART.Api.Controllers
{
    /// <summary>M2-B08 analytical-report endpoint for the <c>vendor-pr-rating</c> slug. See
    /// <see cref="HsnSummaryReportController"/>'s remarks for why this is its own controller.</summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/reports/vendor-pr-rating")]
    [Authorize]
    [RequireScreen(ScreenName)]
    [Tags("Reports")]
    public class VendorPrRatingReportController : ControllerBase
    {
        /// <summary>Byte-identical to the seeded Screens.ScreenName (ApplicationDbContext.cs:1325)
        /// and to PrPoRatings.razor:513's own <c>ScreenName</c> override.</summary>
        private const string ScreenName = "PR PO Rating Report";

        private readonly IReportExecutor _executor;

        public VendorPrRatingReportController(IReportExecutor executor)
        {
            _executor = executor;
        }

        [HttpGet(Name = "getVendorPrRatingReport")]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<object>>> Get([FromQuery] VendorPrRatingReportQuery query)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var entry = ReportRegistry.Find("vendor-pr-rating")
                ?? throw new InvalidOperationException("vendor-pr-rating is not registered in ReportRegistry.");

            var parameters = new[]
            {
                new SqlParameter("@fromDate", (object?)query.FromDate ?? DBNull.Value),
                new SqlParameter("@toDate", (object?)query.ToDate ?? DBNull.Value),
                new SqlParameter("@vendorcode", (object?)query.VendorCode ?? DBNull.Value),
            };

            var rows = await entry.Execute(_executor, parameters);

            var page = rows.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

            return Ok(new PagedResult<object>(page, rows.Count, query.PageNumber, query.PageSize));
        }
    }
}
