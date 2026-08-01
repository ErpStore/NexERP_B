namespace V.SMART.Shared.Services
{
    public interface IFileOpener
    {
        Task<bool> OpenAsync(string path);
        Task OpenAsync(byte[] fileData, string fileName, string contentType);
    }
}
