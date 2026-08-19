using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace V.SMART.Shared.Tests.Infrastructure;

/// <summary>
/// Returns a fixed authenticated principal so the concrete
/// <see cref="V.SMART.Shared.Services.CurrentUserService"/> can be constructed.
///
/// CurrentUserService is a class with non-virtual members and a single constructor
/// parameter of type <see cref="AuthenticationStateProvider"/>
/// (V.SMART/V.SMART.Shared/Services/CurrentUserService.cs:6-16), so it cannot be
/// mocked - it must be constructed with a provider like this one.
///
/// The "UserId" claim type is not a constant anywhere: CurrentUserService.GetUserIdAsync
/// looks for exactly <c>c.Type == "UserId"</c> (CurrentUserService.cs:59), and
/// GetUserRoleAsync looks for exactly <c>c.Type == "role"</c> (:72). A principal
/// carrying only a name yields UserId 0.
/// </summary>
public sealed class TestAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly AuthenticationState _state;

    public TestAuthenticationStateProvider(string userName = "Administrator", int userId = 1, string role = "Administrator")
    {
        var identity = new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim("UserId", userId.ToString()),
                new Claim("role", role),
            },
            authenticationType: "TestAuth");

        _state = new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(_state);
}
