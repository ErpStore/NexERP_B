using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using V.SMART.Api.Middleware;
using V.SMART.Api.Reporting;
using V.SMART.Shared.Services.ReportViewer;

namespace V.SMART.Api.Controllers
{
    /// <summary>M2-B08 print-only stub — see <see cref="PurchaseOrdersController"/>'s remarks for why
    /// this is its own controller rather than a shared one.</summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/job-orders")]
    [Authorize]
    [RequireScreen(ScreenName)]
    [Tags("Job Order")]
    public class JobOrdersController : ControllerBase
    {
        /// <summary>Byte-identical to the seeded Screens.ScreenName (ApplicationDbContext.cs:1207)
        /// and to JobOrderDetails.razor:220's own <c>ScreenName</c> override.</summary>
        private const string ScreenName = "Job Order";

        private const string PdfContentType = "application/pdf";

        private readonly ReportService _reports;

        public JobOrdersController(ReportService reports)
        {
            _reports = reports;
        }

        [HttpGet("{id:int}/print", Name = "printJobOrder")]
        [RequireRight(Right.View)]
        [Produces(PdfContentType)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Print(int id)
        {
            var entry = PrintRegistry.Find("job-orders")
                ?? throw new InvalidOperationException("job-orders is not registered in PrintRegistry.");

            var pdf = await _reports.Generate_Report(
                id, entry.Template, entry.Parameter, cancel: false, entry.ScreenName, entry.ProcedureName);

            if (pdf is null || pdf.Length == 0)
                return this.NotFoundProblem("Job Order not found.");

            return File(pdf, PdfContentType, $"job-order-{id}.pdf");
        }
    }
}
