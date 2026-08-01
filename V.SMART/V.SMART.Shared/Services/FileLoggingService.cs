namespace V.SMART.Shared.Services
{
    public class FileLoggingService : ILoggingService
    {
        private readonly string _basePath;

        public FileLoggingService()
        {
            try
            {
#if ANDROID || WINDOWS || MACCATALYST
           // _basePath = FileSystem.AppDataDirectory;
#else
                _basePath = Path.Combine(AppContext.BaseDirectory, "App_Data"); // Blazor Server
                Directory.CreateDirectory(_basePath);
#endif
            }
            catch
            {
                _basePath = Path.GetTempPath();
            }
        }

        public async Task LogUserAction(string UserName, string Machine, string IP_Address, string screen, string action, string additionalInfo = null)
        {
            try
            {
                string logFolder = Path.Combine(_basePath, "Logs", "UserLogs");
                Directory.CreateDirectory(logFolder);

                string logFile = Path.Combine(logFolder, $"{DateTime.Today:yyyy-MM-dd}_User_{UserName}.txt");

                string logEntry =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} |UserName:{UserName} | Machine:{Machine} | IP_Address:{IP_Address} | Screen: {screen} | Action: {action} | Info: {additionalInfo}";

                await File.AppendAllTextAsync(logFile, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                await LogInternalFailure("LogUserAction", ex);
            }
        }

        public async Task LogDeveloperInfo(string message, string source = null)
        {
            try
            {
                await WriteDeveloperLog("INFO", message, source);
            }
            catch (Exception ex)
            {
                await LogInternalFailure("LogDeveloperInfo", ex);
            }
        }

        public async Task LogDeveloperError(Exception ex, string source = null)
        {
            try
            {
                string msg = $"EXCEPTION: {ex.Message}\nSTACKTRACE: {ex.StackTrace}";
                await WriteDeveloperLog("ERROR", msg, source);
            }
            catch (Exception innerEx)
            {
                await LogInternalFailure("LogDeveloperError", innerEx);
            }
        }

        private async Task WriteDeveloperLog(string level, string message, string source)
        {
            try
            {
                string logFolder = Path.Combine(_basePath, "Logs", "DeveloperLogs");
                Directory.CreateDirectory(logFolder);

                string logFile = Path.Combine(logFolder, $"{DateTime.Today:yyyy-MM-dd}_ErrorLog.txt");

                string logEntry =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | Level: {level} | Source: {source} | Message: {message}";

                await File.AppendAllTextAsync(logFile, logEntry + Environment.NewLine);
            }
            catch (Exception ex)
            {
                await LogInternalFailure("WriteDeveloperLog", ex);
            }
        }

        private async Task LogInternalFailure(string method, Exception ex)
        {
            try
            {
                string tempFile = Path.Combine(Path.GetTempPath(), "LoggingFailure.txt");

                string entry =
                    $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | METHOD: {method} | ERROR: {ex.Message}";

                await File.AppendAllTextAsync(tempFile, entry + Environment.NewLine);
            }
            catch
            {

            }
        }
    }

}
