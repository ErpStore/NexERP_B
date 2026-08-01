namespace V.SMART.Shared.Services
{
    public interface ILoggingService
    {
        Task LogUserAction(string UserName, string Machine, string IP_Address, string screen, string action, string additionalInfo = null);
        Task LogDeveloperInfo(string message, string source = null);
        Task LogDeveloperError(Exception ex, string source = null);
    }
}
