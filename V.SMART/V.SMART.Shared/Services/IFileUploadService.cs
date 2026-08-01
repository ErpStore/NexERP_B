using Microsoft.AspNetCore.Components.Forms;

namespace V.SMART.Shared.Services
{
    public interface IFileUploadService
    {
        Task<string> SaveFileAsync(string fileName, Stream fileStream, string target);
        Task DeleteFileAsync(string fileUrl);
        Task<string> SaveCorresFileAsync(IBrowserFile file, string refType, string docType);
        Task<(string RelativePath, string FullPath)> SaveFileAsync(byte[] fileBytes, string fileName, string refType, string docType);
    }
}
