using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using V.SMART.Api.Authorization;
using V.SMART.Api.Contracts;
using V.SMART.Api.Middleware;
using V.SMART.Api.Services;
using V.SMART.Shared.Data.Utilities.Correspondence;
using V.SMART.Shared.Repository.IRepository;

namespace V.SMART.Api.Controllers
{
    /// <summary>
    /// File upload and download over HTTP (M2-B06) — the replacement for Blazor's
    /// <c>IBrowserFile</c> on the way in and <c>IFileOpener</c>'s JS interop on the way out. Over
    /// HTTP a download <i>is</i> a response; there is no base64-over-SignalR equivalent and no
    /// reason to want one.
    ///
    /// <para><b>Screen and rights.</b> <c>[RequireScreen("Correspondences")]</c> is the seeded
    /// screen that owns these files (<c>ScreenCatalogue.cs:56</c>; the upload UI it mirrors is
    /// <c>CorrespondenceUpload.razor</c>). Upload requires <c>Create</c>, download requires
    /// <c>View</c>, per ADR-004 / KB-105 — the rights are evaluated by the globally registered
    /// <c>ScreenRightAuthorizationFilter</c> against the tenant database, never against a
    /// compile-time list.</para>
    ///
    /// <para><b>Tenant isolation is structural, not a check.</b> The download resolves
    /// <c>{id}</c> through <c>IUnitOfWork.Correspondances</c>, which is scoped to the
    /// tenant-resolved <c>ApplicationDbContext</c> (M2-B07). A tenant-B token therefore queries
    /// tenant B's database, where tenant A's id either does not exist or is a different row; the
    /// answer for "not yours" and "does not exist" is the same 404, so the response never reveals
    /// whether an id exists elsewhere.</para>
    ///
    /// <para><b>The upload is not idempotent.</b> Every POST creates a new file (the
    /// <c>Guid.NewGuid()</c> prefix guarantees a distinct name) and a new row, so a client that
    /// retries a timed-out request creates a duplicate. A general idempotency-key mechanism is
    /// <c>M2-B12-03</c>; until it lands, clients must not retry blind.</para>
    /// </summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/files")]
    [Authorize]
    [RequireScreen("Correspondences")]
    public class FilesController : ControllerBase
    {
        private readonly ApiFileUploadService _fileUploadService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly FileStorageOptions _options;

        public FilesController(
            ApiFileUploadService fileUploadService,
            IUnitOfWork unitOfWork,
            IOptions<FileStorageOptions> options)
        {
            _fileUploadService = fileUploadService;
            _unitOfWork = unitOfWork;
            _options = options.Value;
        }

        /// <summary>
        /// <b>POST api/v1/files</b> — <c>multipart/form-data</c> with <c>file</c>, <c>refType</c>
        /// and <c>docType</c>.
        ///
        /// <para>The order of the checks is the security contract: extension allow-list, then
        /// size, then the duplicate-name business rule, and only then is a byte written. The
        /// stored record mirrors what <c>CorrespondenceUpload.razor:299-312</c> writes — file
        /// name, size string, content type, uploader, and the bytes in
        /// <c>Correspondence.Image</c> (<c>:306-309</c>) — so a file uploaded here is readable by
        /// the existing Blazor list screens.</para>
        ///
        /// <para><b>Two size limits, deliberately.</b> The framework refuses anything over
        /// <see cref="FileStorageOptions.DefaultMaxUploadBytes"/> (20 MB) before the action runs —
        /// that attribute must be a compile-time constant — and the action then applies the
        /// configured <c>FileStorage:MaxUploadBytes</c>, which a deployment may lower. 20 MB is
        /// the same ceiling <c>WebFileUploadService.cs:101</c> passes to
        /// <c>OpenReadStream</c>. Note that the correspondence <i>screen</i> refuses at 5 MB
        /// (<c>CorrespondenceUpload.razor:222</c>) — a page-level rule, not a storage rule, and
        /// not carried here; a deployment wanting parity sets <c>FileStorage:MaxUploadBytes</c>.</para>
        /// </summary>
        [HttpPost]
        [RequireRight(Right.Create)]
        [RequestSizeLimit(FileStorageOptions.DefaultMaxUploadBytes)]
        [RequestFormLimits(MultipartBodyLengthLimit = FileStorageOptions.DefaultMaxUploadBytes)]
        [ProducesResponseType(typeof(FileUploadResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status413PayloadTooLarge)]
        public async Task<ActionResult<FileUploadResponse>> Upload(
            IFormFile? file,
            [FromForm] string? refType,
            [FromForm] string? docType)
        {
            if (file is null || file.Length == 0)
            {
                ModelState.AddModelError("file", "A non-empty file is required.");
                return this.ValidationProblemResult();
            }

            if (!UploadContentTypes.TryResolve(file.FileName, out var contentType))
            {
                ModelState.AddModelError(
                    "file",
                    $"File type '{Path.GetExtension(file.FileName)}' is not allowed.");
                return this.ValidationProblemResult();
            }

            if (file.Length > _options.MaxUploadBytes)
            {
                return this.PayloadTooLargeProblem(
                    $"File size exceeds the {_options.MaxUploadBytes / (1024 * 1024)} MB limit.");
            }

            // CorrespondenceUpload.razor:341-347 — the same duplicate-file-name rule, with the
            // service's own message carried verbatim (ADR-002 §4, BR-SO-001).
            var duplicate = await _unitOfWork.Correspondances.ExistsByNameAsync(
                "FileName", file.FileName?.Trim() ?? string.Empty, "Id", null);
            if (duplicate)
                return this.BusinessRuleProblem("File name already exists.");

            byte[] bytes;
            await using (var upload = file.OpenReadStream())
            using (var buffer = new MemoryStream())
            {
                await upload.CopyToAsync(buffer, HttpContext.RequestAborted);
                bytes = buffer.ToArray();
            }

            using var toDisk = new MemoryStream(bytes, writable: false);
            var (relativePath, _) = await _fileUploadService.SaveCorrespondenceFileAsync(
                toDisk, file.FileName!, refType ?? string.Empty, docType ?? string.Empty);

            var correspondence = new Correspondence
            {
                FileName = file.FileName!,
                FilePath = relativePath,
                FileSize = $"{bytes.Length / 1024.0:F2} KB",
                FileType = contentType,
                Image = bytes,
                UploadedOn = DateTime.Now,
                UploadedBy = User.Identity?.Name ?? string.Empty,
                ReferenceType = string.IsNullOrWhiteSpace(refType) ? null : refType.Trim(),
                DocumentType = string.IsNullOrWhiteSpace(docType) ? "Correspondence" : docType.Trim()
            };

            await _unitOfWork.Correspondances.CreateAsync(correspondence);
            await _unitOfWork.SaveAsync();

            var response = new FileUploadResponse(
                correspondence.Id,
                correspondence.FileName,
                relativePath,
                contentType,
                bytes.LongLength);

            return CreatedAtAction(nameof(Download), new { id = correspondence.Id }, response);
        }

        /// <summary>
        /// <b>GET api/v1/files/{id}</b> — the bytes, with the content type this API resolved from
        /// the file's extension and a <c>Content-Disposition: attachment</c> naming it. The SPA
        /// turns the response into a blob URL, exactly as ADR-005 specifies for PDFs.
        ///
        /// <para><b>Why the database column is preferred over the file on disk.</b> The bytes are
        /// held twice: on disk, and in <c>Correspondence.Image</c>
        /// (<c>Correspondence.cs:14</c>, written by <c>CorrespondenceUpload.razor:306-309</c>).
        /// For files uploaded through Blazor the two disagree — the on-disk copy is <b>zero bytes</b>
        /// because <c>WebFileUploadService.cs:102</c> is commented out — and the two live download
        /// screens disagree with each other about which to use:
        /// <c>CorrespondenceListByReference.razor:357-363</c> abandoned the path and serves the
        /// column, while <c>CorrespondanceList.razor:319-321</c> still opens the (empty) path.
        /// This endpoint follows the screen that works. The disk copy is the fallback, used only
        /// when the column is empty, and only after the resolved absolute path has been proved to
        /// be inside the uploads root.</para>
        ///
        /// <para><c>{id:int}</c> is a route constraint, so <c>../</c>, <c>%2e%2e%2f</c> and every
        /// other traversal string fails to match the route at all and never reaches this method.</para>
        /// </summary>
        [HttpGet("{id:int}")]
        [RequireRight(Right.View)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Download(int id)
        {
            var record = await _unitOfWork.Correspondances.GetAsync(id);

            // One message for "no such id in this tenant", "the row has no bytes" and "the file is
            // missing from disk". A caller must not be able to tell them apart, or the endpoint
            // becomes an existence oracle across tenants.
            if (record is null)
                return this.NotFoundProblem("File not found.");

            // The served content type is this API's own mapping from the extension, never the
            // FileType string a client supplied.
            if (!UploadContentTypes.TryResolve(record.FileName, out var contentType))
                contentType = UploadContentTypes.Fallback;

            var downloadName = Path.GetFileName(record.FileName ?? string.Empty);
            if (string.IsNullOrWhiteSpace(downloadName))
                downloadName = $"file-{id}";

            if (record.Image is { Length: > 0 })
                return File(record.Image, contentType, downloadName);

            var absolute = UploadPaths.ResolveStoredPath(_fileUploadService.RootPath, record.FilePath);
            if (absolute is null || !System.IO.File.Exists(absolute))
                return this.NotFoundProblem("File not found.");

            var bytes = await System.IO.File.ReadAllBytesAsync(absolute, HttpContext.RequestAborted);
            return File(bytes, contentType, downloadName);
        }
    }
}
