using V.SMART.Shared.Services;
using Microsoft.JSInterop;

namespace V.SMART.Web.Services
{
    public class WebFileOpener : IFileOpener
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly ILoggingService _logs;

        public WebFileOpener(IJSRuntime jsRuntime, ILoggingService logs)
        {
            _jsRuntime = jsRuntime;
            _logs = logs;
        }

        public async Task<bool> OpenAsync(string filePath)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(filePath))
                {
                    string extension = Path.GetExtension(filePath).ToLowerInvariant();

                    // List of image extensions
                    var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".txt", ".dwg", ".dxf", ".svg", ".eml", ".msg", ".Pdf" };

                    //if (imageExtensions.Contains(extension))
                    //{
                    // // Open in new tab for images text files
                        await _jsRuntime.InvokeVoidAsync("open", filePath, "_blank");
                    //}
                    //else
                    //{
                    //    // Open in same tab for others downloadable files
                    //    await _jsRuntime.InvokeVoidAsync("open", filePath, "_self");
                    //}

                    return true;
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "WebFileOpener.OpenAsync");
            }

            return false;
        }
        public async Task OpenAsync(byte[] fileData, string fileName, string contentType)
        {
            var base64 = Convert.ToBase64String(fileData);

            await _jsRuntime.InvokeVoidAsync("openFile", base64, contentType);
        }

    }
}
