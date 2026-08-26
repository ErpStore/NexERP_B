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
    /// <summary>M2-B08 analytical-report endpoint for the <c>sales-track</c> slug. See
    /// <see cref="HsnSummaryReportController"/>'s remarks for why this is its own controller.</summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/reports/sales-track")]
    [Authorize]
    [RequireScreen(ScreenName)]
    [Tags("Reports")]
    public class SalesTrackReportController : ControllerBase
    {
        /// <summary>Byte-identical to the seeded Screens.ScreenName (ApplicationDbContext.cs:1270)
        /// and to SalesTrack.razor:1732's own <c>ScreenName</c> override.</summary>
        private const string ScreenName = "Sales Track Report";

        private readonly IReportExecutor _executor;

        public SalesTrackReportController(IReportExecutor executor)
        {
            _executor = executor;
        }

        [HttpGet(Name = "getSalesTrackReport")]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<object>>> Get([FromQuery] SalesTrackReportQuery query)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var entry = ReportRegistry.Find("sales-track")
                ?? throw new InvalidOperationException("sales-track is not registered in ReportRegistry.");

            var parameters = new[]
            {
                new SqlParameter("@FromDate", (object?)query.FromDate ?? DBNull.Value),
                new SqlParameter("@ToDate", (object?)query.ToDate ?? DBNull.Value),
                new SqlParameter("@CustomerId", (object?)query.CustomerId ?? DBNull.Value),
            };

            var rows = await entry.Execute(_executor, parameters);

            var page = rows.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

            return Ok(new PagedResult<object>(page, rows.Count, query.PageNumber, query.PageSize));
        }
    }
}
