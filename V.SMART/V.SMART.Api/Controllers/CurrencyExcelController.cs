using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using V.SMART.Api.Contracts;
using V.SMART.Api.Middleware;
using V.SMART.Api.Services;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAccountsService;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;

namespace V.SMART.Api.Controllers
{
    /// <summary>
    /// The <b>reference implementation</b> of the Excel export/import endpoints (M2-B06, ADR-005:
    /// "Excel export/import likewise stays server-side … behind endpoints"). Rolling the pattern
    /// out to the other resources is per-module work (KB-080 §10), not this task.
    ///
    /// <para><b>Why Currency.</b> It is the only resource with a complete API surface today
    /// (<c>CurrencyController</c>), and the only one with an M2-B02 typed filter DTO
    /// (<see cref="CurrencyQuery"/>). M2-B06 requires the export to apply the resource's filters
    /// so that "the export matches what the grid shows" — for any other resource that filter DTO
    /// would have had to be invented first, which is M2-B02's work, not this task's.</para>
    ///
    /// <para><b>Why a second controller rather than actions on <c>CurrencyController</c>.</b>
    /// These three endpoints are gated by <c>[RequireScreen]</c> + <c>[RequireRight]</c>.
    /// <c>CurrencyController</c> is deliberately unannotated — M2-A01-02 records that no
    /// controller declares them yet and that its five existing endpoints must behave exactly as
    /// before — and <c>[RequireScreen]</c> is a class-level attribute, so annotating it would
    /// change the authorization of endpoints outside this task's scope. Both controllers share the
    /// <c>api/v1/currencies</c> prefix; the action templates (<c>export</c>, <c>import</c>,
    /// <c>import-template</c>) cannot collide with its <c>""</c> and <c>{id:int}</c> routes.</para>
    ///
    /// <para><c>ExcelExportService</c> and <c>ExcelTemplateService</c> are <b>wrapped, never
    /// modified</b>: both already produce <c>byte[]</c>.</para>
    /// </summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/currencies")]
    [Authorize]
    [RequireScreen("Currency")]
    public class CurrencyExcelController : ControllerBase
    {
        /// <summary>The only value <c>format</c> accepts. ADR-005 puts PDF behind M2-B08, not here.</summary>
        public const string XlsxFormat = "xlsx";

        /// <summary>The <c>Content-Type</c> of an <c>.xlsx</c> workbook.</summary>
        public const string XlsxContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        /// <summary>
        /// The bound M2-B06 requires an export to state ("an export is the whole filtered set,
        /// which must be stated and bounded"). Beyond this the request is refused rather than
        /// silently truncated — a truncated export is a wrong answer that looks like a right one.
        /// </summary>
        public const int MaxExportRows = 10_000;

        private readonly ICurrencyService _currencyService;
        private readonly ExcelExportService _excelExportService;
        private readonly IExcelTemplateService _excelTemplateService;

        /// <summary>The header row of the import template, and the columns the importer reads.</summary>
        private static readonly string[] ImportHeaders = { "Currency Name", "Sub Currency", "Symbol" };

        public CurrencyExcelController(
            ICurrencyService currencyService,
            ExcelExportService excelExportService,
            IExcelTemplateService excelTemplateService)
        {
            _currencyService = currencyService;
            _excelExportService = excelExportService;
            _excelTemplateService = excelTemplateService;
        }

        /// <summary>
        /// <b>GET api/v1/currencies/export?format=xlsx</b> — the filtered set as a workbook.
        ///
        /// <para>It takes the same <see cref="CurrencyQuery"/> the list endpoint does, so the same
        /// query string produces the same rows; only <c>pageNumber</c>/<c>pageSize</c> are ignored,
        /// because an export is the whole filtered set rather than a page of it.</para>
        /// </summary>
        [HttpGet("export")]
        [RequireRight(Right.View)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Export([FromQuery] CurrencyQuery query, [FromQuery] string? format)
        {
            if (!ModelState.IsValid)
                return this.ValidationProblemResult();

            if (!string.IsNullOrWhiteSpace(format) && !string.Equals(format, XlsxFormat, StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("format", $"Unsupported export format '{format}'. Only '{XlsxFormat}' is supported.");
                return this.ValidationProblemResult();
            }

            var (items, totalCount) = await _currencyService.SearchWithDynamicFilterAsync(
                1,
                MaxExportRows,
                FilterDictionaryAdapter.ForCurrency(query),
                query.ToServiceSort());

            if (totalCount > MaxExportRows)
            {
                return this.BusinessRuleProblem(
                    $"The filtered set contains {totalCount} rows, which exceeds the export limit of {MaxExportRows}. Narrow the filter and export again.");
            }

            var bytes = _excelExportService.ExportListToExcel(items, "Currencies");
            if (bytes is null || bytes.Length == 0)
            {
                // ExcelExportService logs and returns null on failure (ExcelExportService.cs:108).
                // A null here is a server fault, not a caller error.
                throw new InvalidOperationException("Excel export produced no content.");
            }

            return File(bytes, XlsxContentType, $"currencies-{DateTime.Now:yyyyMMdd-HHmmss}.xlsx");
        }

        /// <summary>
        /// <b>GET api/v1/currencies/import-template</b> — the blank workbook a user fills in,
        /// produced by the unmodified <c>IExcelTemplateService.CreateTemplateAsync</c>.
        /// </summary>
        /// <remarks>
        /// It does <b>not</b> call <c>DownloadTemplate(uploadType)</c>: that method's
        /// <c>LoadHeadings</c> switch knows four item-oriented upload types only
        /// (<c>ExcelTemplateService.cs:46-76</c>) and returns an empty header array for anything
        /// else, so a "Currency" upload type would silently produce an empty template. Passing the
        /// headers explicitly wraps the same service without modifying it.
        /// </remarks>
        [HttpGet("import-template")]
        [RequireRight(Right.View)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ImportTemplate()
        {
            var bytes = await _excelTemplateService.CreateTemplateAsync("Currency", ImportHeaders);
            return File(bytes, XlsxContentType, "currency-import-template.xlsx");
        }

        /// <summary>
        /// <b>POST api/v1/currencies/import</b> — <c>multipart/form-data</c> with <c>file</c>.
        ///
        /// <para>Every row goes through <c>ICurrencyService.CreateAsync</c>. The controller parses
        /// the spreadsheet and does nothing else: duplicate detection, validation and persistence
        /// stay in the business service, and its refusal message is reported verbatim
        /// (BR-SO-001). A bad row is reported, not thrown; the remaining rows are still
        /// processed.</para>
        /// </summary>
        [HttpPost("import")]
        [RequireRight(Right.Create)]
        [RequestSizeLimit(FileStorageOptions.DefaultMaxUploadBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = FileStorageOptions.DefaultMaxUploadBytes)]
        [ProducesResponseType(typeof(ImportResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ImportResult>> Import(IFormFile? file)
        {
            if (file is null || file.Length == 0)
            {
                ModelState.AddModelError("file", "A non-empty file is required.");
                return this.ValidationProblemResult();
            }

            var extension = Path.GetExtension(file.FileName);
            if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("file", $"File type '{extension}' is not allowed. Upload an .xlsx workbook.");
                return this.ValidationProblemResult();
            }

            await using var upload = file.OpenReadStream();
            using var buffer = new MemoryStream();
            await upload.CopyToAsync(buffer, HttpContext.RequestAborted);
            buffer.Position = 0;

            XLWorkbook workbook;
            try
            {
                workbook = new XLWorkbook(buffer);
            }
            catch (Exception)
            {
                ModelState.AddModelError("file", "The file could not be read as an Excel workbook.");
                return this.ValidationProblemResult();
            }

            using (workbook)
            {
                var sheet = workbook.Worksheets.FirstOrDefault();
                if (sheet is null)
                {
                    ModelState.AddModelError("file", "The workbook contains no worksheet.");
                    return this.ValidationProblemResult();
                }

                var errors = new List<ImportRowError>();
                var accepted = 0;
                var total = 0;

                var rows = sheet.RangeUsed()?.RowsUsed().Skip(1).ToList() ?? new List<IXLRangeRow>();

                foreach (var row in rows)
                {
                    total++;
                    var rowNumber = row.RowNumber();

                    var vm = new CurrencyVM
                    {
                        CurrName = row.Cell(1).GetString().Trim(),
                        CurrSub = row.Cell(2).GetString().Trim(),
                        Symbol = row.Cell(3).GetString().Trim()
                    };

                    var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                    var valid = System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                        vm,
                        new System.ComponentModel.DataAnnotations.ValidationContext(vm),
                        validationResults,
                        validateAllProperties: true);

                    if (!valid)
                    {
                        // The ViewModel's own DataAnnotations messages (CurrencyVM.cs:14-25),
                        // verbatim — the same strings the Blazor form shows.
                        errors.Add(new ImportRowError(rowNumber, string.Join(" ", validationResults.Select(r => r.ErrorMessage))));
                        continue;
                    }

                    var (success, message, _) = await _currencyService.CreateAsync(vm);
                    if (success)
                        accepted++;
                    else
                        errors.Add(new ImportRowError(rowNumber, message));
                }

                return Ok(new ImportResult(total, accepted, errors.Count, errors));
            }
        }
    }
}
