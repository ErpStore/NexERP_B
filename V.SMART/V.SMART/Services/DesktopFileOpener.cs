using V.SMART.Shared.Services;

namespace V.SMART.Services
{
    public class DesktopFileOpener : IFileOpener
    {
        private readonly ILoggingService _logs;

        public DesktopFileOpener(ILoggingService logs)
        {
            _logs = logs;
        }

        public async Task<bool> OpenAsync(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                var formattedPath = path.Replace("\\", "/");
                var uri = new Uri(formattedPath.StartsWith("file://") ? formattedPath : $"file:///{formattedPath}");

                await Launcher.Default.OpenAsync(uri);
                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "DesktopFile.OpenAsync");
                return false;
            }
        }
        public async Task OpenAsync(byte[] fileData, string fileName, string contentType)
        {
            var path = Path.Combine(FileSystem.CacheDirectory, fileName);

            await File.WriteAllBytesAsync(path, fileData);

            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(path)
            });
        }
    }

}
