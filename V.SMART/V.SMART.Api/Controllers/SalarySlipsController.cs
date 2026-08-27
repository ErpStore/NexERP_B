using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using V.SMART.Api.Middleware;
using V.SMART.Api.Reporting;
using V.SMART.Shared.Services.ReportViewer;

namespace V.SMART.Api.Controllers
{
    /// <summary>
    /// M2-B08 print-only stub. Deliberately the entry that exercises
    /// <see cref="PrintGenerator.GenerateSalarySlipReport"/> rather than
    /// <c>ReportService.Generate_Report</c> — proving the registry's second generator branch,
    /// not just the dominant one. See <see cref="PurchaseOrdersController"/>'s remarks for why
    /// this is its own controller.
    /// </summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/salary-slips")]
    [Authorize]
    [RequireScreen(ScreenName)]
    [Tags("Salary")]
    public class SalarySlipsController : ControllerBase
    {
        /// <summary>Byte-identical to the seeded Screens.ScreenName (ApplicationDbContext.cs:1313)
        /// and to SalaryDetails.razor:310's literal <c>"Salary"</c> argument.</summary>
        private const string ScreenName = "Salary";

        private const string PdfContentType = "application/pdf";

        private readonly ReportService _reports;

        public SalarySlipsController(ReportService reports)
        {
            _reports = reports;
        }

        [HttpGet("{id:int}/print", Name = "printSalarySlip")]
        [RequireRight(Right.View)]
        [Produces(PdfContentType)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Print(int id)
        {
            var entry = PrintRegistry.Find("salary-slips")
                ?? throw new InvalidOperationException("salary-slips is not registered in PrintRegistry.");

            if (entry.Generator != Reporting.PrintGenerator.GenerateSalarySlipReport)
                throw new InvalidOperationException("salary-slips registry entry no longer names GenerateSalarySlipReport; this controller must be updated to match.");

            var pdf = await _reports.GenerateSalarySlipReport(
                id, entry.Template, entry.Parameter, cancel: false, entry.ScreenName, entry.ProcedureName);

            if (pdf is null || pdf.Length == 0)
                return this.NotFoundProblem("Salary record not found.");

            return File(pdf, PdfContentType, $"salary-slip-{id}.pdf");
        }
    }
}
