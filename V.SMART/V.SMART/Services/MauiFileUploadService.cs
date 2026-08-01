using V.SMART.Shared.Services;
using V.SMART.Shared.Services.Extensions;
using V.SMART.Shared.Services.MultiCompanyService;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace V.SMART.Services
{
    public class MauiFileUploadService : IFileUploadService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILoggingService _loggingService;
        private readonly ITenantProvider _tenantProvider;

        public MauiFileUploadService(IJSRuntime jsRuntime, ILoggingService loggingService, ITenantProvider tenantProvider)
        {
            _jsRuntime = jsRuntime;
            _loggingService = loggingService;
            _tenantProvider = tenantProvider;
        }

        // For image (logo) upload
        public async Task<string> SaveFileAsync(string fileName, Stream fileStream, string target)
        {
            try
            {
                var tenant = _tenantProvider.GetCurrentTenant();
                string companyName = tenant?.Hostname;

                var safeCompany = string.Concat(companyName?
                    .Where(c => !Path.GetInvalidFileNameChars().Contains(c)))
                    .ToLower();

                companyName = string.IsNullOrWhiteSpace(companyName)
                    ? "defaultcompany"
                    : companyName.Trim();

                // ✅ MAUI base path (NOT wwwroot)
                var rootPath = FileSystem.AppDataDirectory;

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

                // ✅ Keep same format as Web
                var relativePath = $"/{baseFolder}/{safeCompany}/{uniqueName}";

                return $"{relativePath}|{filePath}";
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "MauiFileUploadService.SaveFileAsync");
                return string.Empty;
            }
        }

        // For general file uploads (correspondence, attachments)
        public async Task<string> SaveCorresFileAsync(IBrowserFile file, string refType, string docType)
        {
            try
            {
                var basePath = _tenantProvider.GetTenantCorrespondenceUploadPath(); // e.g., C:\TenantData\Uploads
                if (string.IsNullOrWhiteSpace(basePath))
                {
                    await _jsRuntime.ToastrError("Storage path not configured.");
                    return string.Empty;
                }

                refType = string.IsNullOrWhiteSpace(refType) ? "ManualUpload" : refType.Trim();
                var safeRefType = refType.Replace(" ", "").ToLower();

                string subfolder = docType?.Trim().ToLower() switch
                {
                    "drawing" => "drawings",
                    _ => "correspondences" // includes null, empty, and "correspondence"
                };

                // Final save path: basePath\subfolder\refType\
                var savePath = Path.Combine(basePath, subfolder, safeRefType);

                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                var fileName = $"{Guid.NewGuid()}_{file.Name}";
                var fullPath = Path.Combine(savePath, fileName);

                await using var stream = File.Create(fullPath);
                await file.OpenReadStream(maxAllowedSize: 20 * 1024 * 1024).CopyToAsync(stream); // 20 MB max

                // If you want to return the relative path to store in DB, do that here
                return new Uri(fullPath).AbsoluteUri; // Or return relative if needed
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "MauiFileUploadService.SaveFileAsync");
                await _jsRuntime.ToastrError("File upload failed.");
                return string.Empty;
            }
        }

        public async Task DeleteFileAsync(string filePath)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    var localPath = new Uri(filePath).LocalPath;
                    if (File.Exists(localPath))
                        File.Delete(localPath);
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "MauiFileUploadService.DeleteFileAsync");
            }
        }

        public async Task<(string RelativePath, string FullPath)> SaveFileAsync(byte[] fileBytes, string fileName, string refType, string docType)
        {
            try
            {
                if (fileBytes == null || fileBytes.Length == 0 || string.IsNullOrWhiteSpace(fileName))
                    return (string.Empty, string.Empty);

                // ✅ Tenant (same as web)
                var tenant = _tenantProvider?.GetCurrentTenant();
                string companyName = tenant?.Hostname;

                companyName = string.IsNullOrWhiteSpace(companyName)
                    ? "DefaultCompany"
                    : companyName.Trim();

                refType = string.IsNullOrWhiteSpace(refType)
                    ? "ManualUpload"
                    : refType.Trim();

                // ✅ Sanitize
                var safeCompany = string.Concat(companyName
                    .Where(c => !Path.GetInvalidFileNameChars().Contains(c)))
                    .ToLower();

                var safeRefType = string.Concat(refType
                    .Where(c => !Path.GetInvalidFileNameChars().Contains(c)))
                    .ToLower();

                // ✅ Base path (MAUI)
                var rootPath = FileSystem.AppDataDirectory;

                // ✅ Detect extension
                var extension = Path.GetExtension(fileName)?.ToLower();
                var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                bool isImage = imageExtensions.Contains(extension);

                // ✅ Unique filename
                var originalFileName = Path.GetFileName(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";

                // ✅ Folder logic (same as web)
                string baseFolder = docType?.Trim().ToLower() switch
                {
                    "drawing" when !isImage => "drawings",
                    _ => "correspondences"
                };

                // ✅ Relative path (logical, not URL in MAUI)
                var relativePath = Path.Combine(baseFolder, safeCompany, safeRefType, uniqueFileName);

                // ✅ Full path (device storage)
                var fullPath = Path.Combine(rootPath, relativePath);

                var directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory!))
                    Directory.CreateDirectory(directory!);

                // ✅ Save file
                await File.WriteAllBytesAsync(fullPath, fileBytes);

                // Normalize relative path (important for cross-platform consistency)
                var normalizedRelativePath = relativePath.Replace("\\", "/");

                return (normalizedRelativePath, fullPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving file: {ex.Message}");
                return (string.Empty, string.Empty);
            }
        }

    }
}
