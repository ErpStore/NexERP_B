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
    /// <summary>
    /// M2-B08 analytical-report endpoint for the <c>hsn-summary</c> slug. One controller per
    /// slug, not a shared <c>ReportsController</c> action, for the same reason the print entries
    /// are split — <see cref="RequireScreenAttribute"/> is class-level, and each slug in
    /// <see cref="ReportRegistry"/> can carry a different screen.
    /// </summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/reports/hsn-summary")]
    [Authorize]
    [RequireScreen(ScreenName)]
    [Tags("Reports")]
    public class HsnSummaryReportController : ControllerBase
    {
        /// <summary>Byte-identical to the seeded Screens.ScreenName (ApplicationDbContext.cs:1324)
        /// and to HSNSACSummaryReport.razor:532's own <c>ScreenName</c> override.</summary>
        private const string ScreenName = "HSNSummary Report";

        private readonly IReportExecutor _executor;

        public HsnSummaryReportController(IReportExecutor executor)
        {
            _executor = executor;
        }

        [HttpGet(Name = "getHsnSummaryReport")]
        [RequireRight(Right.View)]
        [ProducesResponseType(typeof(PagedResult<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<PagedResult<object>>> Get([FromQuery] HsnSummaryReportQuery query)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            var entry = ReportRegistry.Find("hsn-summary")
                ?? throw new InvalidOperationException("hsn-summary is not registered in ReportRegistry.");

            var parameters = new[]
            {
                new SqlParameter("@ReportType", query.ReportType),
                new SqlParameter("@FromDate", (object?)query.FromDate ?? DBNull.Value),
                new SqlParameter("@ToDate", (object?)query.ToDate ?? DBNull.Value),
            };

            var rows = await entry.Execute(_executor, parameters);

            // In-memory paging (ReportRegistry remarks) — the procedure accepts no @Skip/@Take.
            var page = rows.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

            return Ok(new PagedResult<object>(page, rows.Count, query.PageNumber, query.PageSize));
        }
    }
}
