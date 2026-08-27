using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using V.SMART.Api.Middleware;
using V.SMART.Api.Reporting;
using V.SMART.Shared.Services.ReportViewer;

namespace V.SMART.Api.Controllers
{
    /// <summary>
    /// M2-B08 print-only stub. This is not the Purchase Order resource controller — no CRUD
    /// exists here; a future module-wave task (M3/M4) that builds one is expected to fold this
    /// action in, at the same route, and delete this file. Placed on its own resource-shaped
    /// controller rather than a shared print controller because <see cref="RequireScreenAttribute"/>
    /// is class-level only (KB-105 §2.2) and this route needs a different screen than the other
    /// two seeded print entries.
    /// </summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/purchase-pos")]
    [Authorize]
    [RequireScreen(ScreenName)]
    [Tags("Purchase Order")]
    public class PurchaseOrdersController : ControllerBase
    {
        /// <summary>Byte-identical to the seeded Screens.ScreenName (ApplicationDbContext.cs:1234)
        /// and to PurchasePoDetails.razor:227's own <c>ScreenName</c> override.</summary>
        private const string ScreenName = "Purchase Order";

        private const string PdfContentType = "application/pdf";

        private readonly ReportService _reports;

        public PurchaseOrdersController(ReportService reports)
        {
            _reports = reports;
        }

        [HttpGet("{id:int}/print", Name = "printPurchaseOrder")]
        [RequireRight(Right.View)]
        [Produces(PdfContentType)]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Print(int id)
        {
            var entry = PrintRegistry.Find("purchase-pos")
                ?? throw new InvalidOperationException("purchase-pos is not registered in PrintRegistry.");

            var pdf = await _reports.Generate_Report(
                id, entry.Template, entry.Parameter, cancel: false, entry.ScreenName, entry.ProcedureName);

            if (pdf is null || pdf.Length == 0)
                return this.NotFoundProblem("Purchase Order not found.");

            return File(pdf, PdfContentType, $"purchase-po-{id}.pdf");
        }
    }
}
