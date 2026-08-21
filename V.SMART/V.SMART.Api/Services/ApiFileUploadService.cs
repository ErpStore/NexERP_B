using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using V.SMART.Shared.Services;
using V.SMART.Shared.Services.MultiCompanyService;

namespace V.SMART.Api.Services
{
    /// <summary>
    /// The API host's <see cref="IFileUploadService"/> (M2-B06). M2-B07 deliberately leaves this
    /// interface out of <c>AddVSmartDomain()</c> because it is host-specific; this type is what
    /// fills the gap, and it is registered in <c>Program.cs</c> beside that call, never inside it.
    ///
    /// <para><b>Same bytes, same folders as Blazor.</b> Every folder, sanitisation and naming rule
    /// is transcribed from <c>V.SMART/V.SMART.Web/Services/WebFileUploadService.cs</c> through
    /// <see cref="UploadPaths"/>, so a file written here is indistinguishable from one written
    /// there — provided both hosts are pointed at the same <see cref="FileStorageOptions.Root"/>,
    /// which they are not by default (see that type).</para>
    ///
    /// <para><b>The one deliberate behavioural difference.</b>
    /// <c>WebFileUploadService.SaveCorresFileAsync</c> has the stream copy commented out
    /// (<c>WebFileUploadService.cs:102</c>) and therefore writes a <b>zero-byte</b> file while
    /// returning a path as though it succeeded. This implementation copies the stream. Shipping a
    /// knowingly-broken new code path is not an option; equally, M2-B06 forbids editing the Blazor
    /// implementation, so the defect is recorded in
    /// <c>docs/kb/risks/technical-debt-register.md</c> rather than fixed here.</para>
    ///
    /// <para><b>No <c>IJSRuntime</c>.</b> The Blazor implementation reports failures with a toast
    /// (<c>:109</c>, <c>:199</c>). An API cannot; failures propagate and become
    /// <c>application/problem+json</c> through M2-A06's middleware.</para>
    /// </summary>
    public sealed class ApiFileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILoggingService _loggingService;
        private readonly ITenantProvider _tenantProvider;
        private readonly FileStorageOptions _options;

        public ApiFileUploadService(
            IWebHostEnvironment env,
            ILoggingService loggingService,
            ITenantProvider tenantProvider,
            IOptions<FileStorageOptions> options)
        {
            _env = env ?? throw new ArgumentNullException(nameof(env));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
            _options = (options ?? throw new ArgumentNullException(nameof(options))).Value;
        }

        /// <summary>
        /// The uploads root for this host: the configured <see cref="FileStorageOptions.Root"/>
        /// when set, otherwise <c>WebRootPath ?? "wwwroot"</c> — the same expression
        /// <c>WebFileUploadService.cs:39</c> uses.
        /// </summary>
        public string RootPath => string.IsNullOrWhiteSpace(_options.Root)
            ? (_env.WebRootPath ?? "wwwroot")
            : _options.Root!;

        /// <summary>
        /// The tenant folder segment: <c>tenant.Hostname</c>, sanitised
        /// (<c>WebFileUploadService.cs:36-37</c>). <c>TenantProvider.GetCurrentTenant()</c> returns
        /// <c>null</c> when it cannot resolve a tenant (<c>TenantProvider.cs:82</c>); the Blazor
        /// implementation dereferences it unguarded at <c>:36</c> and throws a
        /// NullReferenceException. This host fails closed with a named exception instead, because
        /// writing an unresolved tenant's file into the <c>defaultcompany</c> folder would silently
        /// mix tenants' data.
        /// </summary>
        private string TenantFolderSegment()
        {
            var tenant = _tenantProvider.GetCurrentTenant();
            if (tenant is null)
                throw new InvalidOperationException("The tenant for this request could not be resolved; refusing to store a file.");

            return UploadPaths.Sanitise(tenant.Hostname);
        }

        /// <summary>
        /// Logo / ISO-logo upload. Transcribed from <c>WebFileUploadService.cs:31-64</c>, including
        /// the pipe-delimited <c>"{webPath}|{filePath}"</c> return at <c>:56-57</c> that
        /// <c>CompanyService.cs:145</c> splits — the composite is preserved deliberately, because
        /// changing it here would change <c>CompanyService</c> for the Blazor host too.
        /// </summary>
        public async Task<string> SaveFileAsync(string fileName, Stream fileStream, string target)
        {
            try
            {
                var safeCompany = TenantFolderSegment();
                var rootPath = RootPath;
                var baseFolder = UploadPaths.LogoBaseFolder(target);
                var uploadsFolder = Path.Combine(rootPath, baseFolder, safeCompany);

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // :50 — GUID + the original extension only. The caller's file name never reaches
                // the file system.
                var uniqueName = Guid.NewGuid() + Path.GetExtension(fileName);
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                await using var fs = new FileStream(filePath, FileMode.Create);
                await fileStream.CopyToAsync(fs);

                var webPath = $"/{baseFolder}/{safeCompany}/{uniqueName}";
                return $"{webPath}|{filePath}";
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "ApiFileUploadService.SaveFileAsync(Stream)");
                throw;
            }
        }

        /// <summary>
        /// <b>Not supported in this host, by design.</b> <see cref="IBrowserFile"/> is produced by
        /// Blazor's <c>InputFile</c> component and never exists in an HTTP request, which produces
        /// <c>IFormFile</c>. The member stays on the interface because <c>V.SMART.Web</c> and the
        /// MAUI head implement and use it and M2-B06 must not change them; the HTTP equivalent is
        /// <c>SaveCorrespondenceFileAsync(Stream, fileName, refType, docType)</c>.
        /// </summary>
        public Task<string> SaveCorresFileAsync(IBrowserFile file, string refType, string docType)
            => throw new NotSupportedException(
                "IBrowserFile is a Blazor type and never occurs in an HTTP request. " +
                "Use ApiFileUploadService.SaveCorrespondenceFileAsync(Stream, fileName, refType, docType) instead.");

        /// <summary>
        /// The HTTP-side replacement for <c>SaveCorresFileAsync</c>: the folder layout of
        /// <c>WebFileUploadService.cs:69-104</c> — <c>uploads/drawings</c> or
        /// <c>uploads/correspondences</c>, then <c>{safeCompany}/{safeRefType}/{guid}_{name}</c> —
        /// <b>with the stream copy that <c>:102</c> has commented out</b>.
        /// </summary>
        /// <returns>The web-relative path (leading slash, forward slashes) and the absolute path,
        /// matching the byte[] overload's tuple at <c>:194</c>.</returns>
        public async Task<(string RelativePath, string FullPath)> SaveCorrespondenceFileAsync(
            Stream content, string fileName, string refType, string docType)
        {
            ArgumentNullException.ThrowIfNull(content);

            try
            {
                var safeCompany = TenantFolderSegment();
                var safeRefType = UploadPaths.Sanitise(UploadPaths.RefTypeOrDefault(refType));
                var rootPath = RootPath;
                var baseFolder = UploadPaths.DocumentBaseFolder(docType);

                var relativePath = Path.Combine(baseFolder, safeCompany, safeRefType, UploadPaths.UniqueFileName(fileName));
                var fullPath = Path.Combine(rootPath, relativePath);

                var directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory!))
                    Directory.CreateDirectory(directory!);

                await using (var fileStream = File.Create(fullPath))
                {
                    // The line WebFileUploadService.cs:102 has commented out. Present here.
                    await content.CopyToAsync(fileStream);
                }

                return ("/" + relativePath.Replace("\\", "/"), fullPath);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "ApiFileUploadService.SaveCorrespondenceFileAsync(Stream)");
                throw;
            }
        }

        /// <summary>
        /// Transcribed from <c>WebFileUploadService.cs:149-202</c>. Same guard on empty input
        /// (<c>:153</c>), same folders, same <c>{guid}_{name}</c>. The Blazor version's toast on
        /// failure is replaced by a rethrow: an API reports failures through the error contract.
        /// </summary>
        public async Task<(string RelativePath, string FullPath)> SaveFileAsync(
            byte[] fileBytes, string fileName, string refType, string docType)
        {
            if (fileBytes == null || fileBytes.Length == 0 || string.IsNullOrWhiteSpace(fileName))
                return (string.Empty, string.Empty);

            using var stream = new MemoryStream(fileBytes, writable: false);
            return await SaveCorrespondenceFileAsync(stream, fileName, refType, docType);
        }

        /// <summary>
        /// Transcribed from <c>WebFileUploadService.cs:117-147</c>, with one addition: the resolved
        /// path must be inside <see cref="RootPath"/> (<c>UploadPaths.ResolveStoredPath</c>). The
        /// Blazor version deletes any absolute path it is handed; this host will not, because the
        /// value can now reach it over HTTP.
        /// </summary>
        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.CompletedTask;

            try
            {
                var path = UploadPaths.ResolveStoredPath(RootPath, fileUrl);
                if (path is not null && File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _loggingService.LogDeveloperError(ex, "ApiFileUploadService.DeleteFileAsync").ConfigureAwait(false);
            }

            return Task.CompletedTask;
        }
    }
}
