using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using V.SMART.Api.Reporting;

namespace V.SMART.Api.Controllers
{
    /// <summary>The catalogue-response shape for <c>GET /api/v1/reports</c> — one row per
    /// registered slug. Each report's own data endpoint lives on its own controller (see
    /// <see cref="ReportsController"/> remarks); this only lists what exists.</summary>
    /// <param name="Slug">The value to substitute into <c>GET /api/v1/reports/{slug}</c>.</param>
    /// <param name="DisplayName">Human-readable name.</param>
    /// <param name="Parameters">The typed parameters the report's own endpoint accepts.</param>
    /// <param name="ScreenName">The screen right that gates the report's data endpoint — declared
    /// here so a client can decide whether to even offer the report before trying it, but the
    /// data endpoint enforces it independently regardless of what the client does with this.</param>
    /// <param name="Paging">Always <c>"in-memory"</c> for every entry today — see the remark on
    /// <see cref="ReportRegistry"/>. Present on every row rather than assumed, so a client cannot
    /// mistake this for a server-paged list.</param>
    public sealed record ReportCatalogueEntry(
        string Slug,
        string DisplayName,
        IReadOnlyList<ReportParameterDescriptor> Parameters,
        string ScreenName,
        string Paging);

    /// <summary>
    /// M2-B08's report catalogue — what <see cref="ReportRegistry"/> exposes, published as its
    /// own endpoint per the task's Target Result item 3 ("<c>GET /api/v1/reports</c> is what
    /// M2-C09 reads to build 40 screens from one framework — it is a deliverable, not a nicety").
    /// </summary>
    /// <remarks>
    /// <b>Why <c>[NoScreenRight]</c> here, when the task says every report/print action carries
    /// <c>[RequireScreen]</c>/<c>[RequireRight]</c>.</b> This action returns registry
    /// <i>metadata</i> only — slugs, display names, parameter shapes, which screen gates each one
    /// — and executes no stored procedure and returns no report row. Nothing here can leak report
    /// data: each report's own endpoint (e.g. <see cref="HsnSummaryReportController"/>) still
    /// independently enforces its own <c>[RequireScreen]</c> when the client actually calls it.
    /// This is the documented, auditable exception the attribute exists for (KB-105 §2.4), not a
    /// gap — the alternative (gating the catalogue on some one arbitrary screen) would hide
    /// report existence from users who could otherwise legitimately discover, then be correctly
    /// refused, a report they lack rights to.
    /// </remarks>
    [ApiController]
    [Route($"{ApiRoutes.V1}/reports")]
    [Authorize]
    [Tags("Reports")]
    public class ReportsController : ControllerBase
    {
        [HttpGet(Name = "getReportCatalogue")]
        [NoScreenRight("Metadata only - lists slugs/parameters/gating screens, executes no procedure and returns no report row. Each report's own endpoint still enforces its own RequireScreen independently (KB-105 SS2.4).")]
        [ProducesResponseType(typeof(IReadOnlyList<ReportCatalogueEntry>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public ActionResult<IReadOnlyList<ReportCatalogueEntry>> GetCatalogue()
        {
            var catalogue = ReportRegistry.Entries
                .Select(e => new ReportCatalogueEntry(
                    e.Slug,
                    e.DisplayName,
                    e.Parameters,
                    e.ScreenName,
                    Paging: "in-memory"))
                .ToList();

            return Ok(catalogue);
        }
    }
}
