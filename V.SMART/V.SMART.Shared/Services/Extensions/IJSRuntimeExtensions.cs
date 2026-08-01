using Microsoft.JSInterop;

namespace V.SMART.Shared.Services.Extensions
{
    public static class IJSRuntimeExtensions
    {

        public static async Task ToastrSuccess(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("ShowToastr", "success", message);
        }

        public static async Task ToastrError(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("ShowToastr", "error", message);
        }

        public static async Task ToastrInfo(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("ShowToastr", "info", message);
        }


        public static async Task ToastrWarning(this IJSRuntime js, string message)
        {
            await js.InvokeVoidAsync("ShowToastr", "warning", message);
        }

        //Related to Export data To the Excel
        public static async ValueTask DownloadExcelAsync(this IJSRuntime jsRuntime, string base64String, string fileName)
        {
            await jsRuntime.InvokeVoidAsync("downloadExcelFile", base64String, fileName);
        }

    }
}
