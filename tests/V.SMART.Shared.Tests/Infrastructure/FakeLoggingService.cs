using V.SMART.Shared.Services;

namespace V.SMART.Shared.Tests.Infrastructure;

/// <summary>
/// No-op recorder for <see cref="ILoggingService"/>, whose three members are declared
/// at V.SMART/V.SMART.Shared/Services/ILoggingService.cs:3-8. Writes nothing; keeps
/// every call so a test can assert a service logged, without touching a database.
/// </summary>
public sealed class FakeLoggingService : ILoggingService
{
    public List<(string UserName, string Machine, string IpAddress, string Screen, string Action, string? AdditionalInfo)> UserActions { get; } = new();
    public List<(string Message, string? Source)> DeveloperInfo { get; } = new();
    public List<(Exception Exception, string? Source)> DeveloperErrors { get; } = new();

    public Task LogUserAction(string UserName, string Machine, string IP_Address, string screen, string action, string additionalInfo = null!)
    {
        UserActions.Add((UserName, Machine, IP_Address, screen, action, additionalInfo));
        return Task.CompletedTask;
    }

    public Task LogDeveloperInfo(string message, string source = null!)
    {
        DeveloperInfo.Add((message, source));
        return Task.CompletedTask;
    }

    public Task LogDeveloperError(Exception ex, string source = null!)
    {
        DeveloperErrors.Add((ex, source));
        return Task.CompletedTask;
    }
}
