using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace V.SMART.Api.Auth
{
    /// <summary>
    /// Bridges JWT claims (from HttpContext.User) into the Blazor-style AuthenticationStateProvider
    /// that CurrentUserService already depends on — so Shared services work without Blazor circuits.
    /// </summary>
    public class ApiAuthStateProvider : AuthenticationStateProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiAuthStateProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = _httpContextAccessor.HttpContext?.User
                       ?? new ClaimsPrincipal(new ClaimsIdentity());
            return Task.FromResult(new AuthenticationState(user));
        }
    }
}
