using V.SMART.Shared.Services;
using V.SMART.Shared.Services.Extensions;
using V.SMART.Shared.Services.MultiCompanyService;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace V.SMART.Web.Services
{
    public class WebFileUploadService : IFileUploadService
    {
        private readonly IWebHostEnvironment _env;
        private readonly ILoggingService _loggingService;
        private readonly IJSRuntime _jsRuntime;
        private readonly ITenantProvider _tenantProvider;

        public WebFileUploadService(
            IWebHostEnvironment env,
            ILoggingService loggingService,
            IJSRuntime jsRuntime,
           ITenantProvider tenantProvider)
        {
            _env = env;
            _loggingService = loggingService;
            _jsRuntime = jsRuntime;
            _tenantProvider = tenantProvider ?? throw new ArgumentNullException(nameof(tenantProvider));
        }

        /// <summary>
        /// Save a file using a stream (generic uploads).
        /// </summary>
        public async Task<string> SaveFileAsync(string fileName, Stream fileStream, string target)
        {
            try
            {
                var tenant = _tenantProvider.GetCurrentTenant();
                string companyName = tenant.Hostname;
                var safeCompany = string.Concat(companyName.Where(c => !Path.GetInvalidFileNameChars().Contains(c))).ToLower();
                companyName = string.IsNullOrWhiteSpace(companyName) ? "DefaultCompany" : companyName.Trim();
                var rootPath = _env.WebRootPath ?? "wwwroot";
                string baseFolder = target?.Trim().ToLower() switch
                {
                    "logo" => "uploads/Logos",
                    _ => "uploads/IsoLogos"
                };
                var uploadsFolder = Path.Combine(rootPath, baseFolder, safeCompany);

                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueName = Guid.NewGuid() + Path.GetExtension(fileName);
                var filePath = Path.Combine(uploadsFolder, uniqueName);

                await using var fs = new FileStream(filePath, FileMode.Create);
                await fileStream.CopyToAsync(fs);

                var webPath = $"/{baseFolder}/{safeCompany}/{uniqueName}";
                return $"{webPath}|{filePath}";
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "WebFileUploadService.SaveFileAsync(Stream)");
                throw;
            }
        }

        /// <summary>
        /// Save a browser file under /uploads/Correspondences/{refType}/
        /// </summary>
        public async Task<string> SaveCorresFileAsync(IBrowserFile file, string refType, string docType)
        {
            try
            {
                var tenant = _tenantProvider.GetCurrentTenant();
                string companyName = tenant.Hostname;
                // Use default values if null or empty

                companyName = string.IsNullOrWhiteSpace(companyName) ? "DefaultCompany" : companyName.Trim();
                refType = string.IsNullOrWhiteSpace(refType) ? "ManualUpload" : refType.Trim();

                var safeCompany = string.Concat(companyName.Where(c => !Path.GetInvalidFileNameChars().Contains(c))).ToLower();
                var safeRefType = string.Concat(refType.Where(c => !Path.GetInvalidFileNameChars().Contains(c))).ToLower();

                var rootPath = _env.WebRootPath ?? "wwwroot";
                var fileName = $"{Guid.NewGuid()}_{file.Name}";

                string baseFolder = docType?.Trim().ToLower() switch
                {
                    "drawing" => "uploads/drawings",
                    _ => "uploads/correspondences"
                };

                var relativePath = Path.Combine(baseFolder, safeCompany, safeRefType, fileName);
                var fullPath = Path.Combine(rootPath, relativePath);

                var directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory!))
                    Directory.CreateDirectory(directory!);

                // Use OpenReadStream and FileStream properly for binary content
                await using var fileStream = File.Create(fullPath);
                await using var inputStream = file.OpenReadStream(20 * 1024 * 1024); // 20 MB max
               // await inputStream.CopyToAsync(fileStream);

                return "/" + relativePath.Replace("\\", "/");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "WebFileUploadService.SaveFileAsync(BrowserFile)");
                await _jsRuntime.ToastrError("File upload failed.");
                return string.Empty;
            }
        }

        /// <summary>
        /// Delete file from web path or absolute path.
        /// </summary>
        public Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return Task.CompletedTask;

            try
            {
                string path;

                if (fileUrl.Contains("uploads/") && !fileUrl.Contains(":\\") && !fileUrl.Contains(":/"))
                {
                    var rootPath = _env.WebRootPath ?? "wwwroot";
                    var relativePath = fileUrl.Replace("/", Path.DirectorySeparatorChar.ToString()).TrimStart(Path.DirectorySeparatorChar);
                    path = Path.Combine(rootPath, relativePath);
                }
                else
                {
                    path = fileUrl;
                }

                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                _loggingService.LogDeveloperError(ex, "WebFileUploadService.DeleteFileAsync").ConfigureAwait(false);
                Console.Error.WriteLine($"Error deleting file: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public async Task<(string RelativePath, string FullPath)> SaveFileAsync(byte[] fileBytes, string fileName, string refType, string docType)
        {
            try
            {
                if (fileBytes == null || fileBytes.Length == 0 || string.IsNullOrWhiteSpace(fileName))
                    return (string.Empty, string.Empty);

                var tenant = _tenantProvider.GetCurrentTenant();
                string companyName = tenant?.Hostname;

                companyName = string.IsNullOrWhiteSpace(companyName) ? "DefaultCompany" : companyName.Trim();
                refType = string.IsNullOrWhiteSpace(refType) ? "ManualUpload" : refType.Trim();

                // ✅ Sanitize
                var safeCompany = string.Concat(companyName.Where(c => !Path.GetInvalidFileNameChars().Contains(c))).ToLower();
                var safeRefType = string.Concat(refType.Where(c => !Path.GetInvalidFileNameChars().Contains(c))).ToLower();

                var rootPath = _env.WebRootPath ?? "wwwroot";

                // ✅ Detect extension
                var extension = Path.GetExtension(fileName)?.ToLower();
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                bool isImage = imageExtensions.Contains(extension);

                // ✅ Unique filename (same as first method)
                var originalFileName = Path.GetFileName(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";

                // ✅ Folder logic (aligned)
                string baseFolder = docType?.Trim().ToLower() switch
                {
                    "drawing" => "uploads/drawings",
                    _ => "uploads/correspondences"
                };

                var relativePath = Path.Combine(baseFolder, safeCompany, safeRefType, uniqueFileName);
                var fullPath = Path.Combine(rootPath, relativePath);

                var directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory!))
                    Directory.CreateDirectory(directory!);

                // ✅ Save byte[]
                await File.WriteAllBytesAsync(fullPath, fileBytes);

                return ("/" + relativePath.Replace("\\", "/"), fullPath);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "SaveFileAsync(byte[]) failed");
                await _jsRuntime.ToastrError("File upload failed.");
                return (string.Empty, string.Empty);
            }
        }


    }
}
