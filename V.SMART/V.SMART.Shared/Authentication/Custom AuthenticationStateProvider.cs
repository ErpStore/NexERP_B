using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace V.SMART.Shared.Authentication
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly ClaimsPrincipal _anonymous =
            new ClaimsPrincipal(new ClaimsIdentity());

        private ClaimsPrincipal _currentUser;

        public bool IsQrLogin { get; private set; } = false;

        public bool IsProductionUser { get; private set; } = false;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(
                new AuthenticationState(
                    _currentUser ?? _anonymous));
        }

        public void MarkUserAsAuthenticated(
            string userName,
            int userId,
            string role,
            bool isQrLogin,
            bool isProductionUser)
        {
            IsQrLogin = isQrLogin;
            IsProductionUser = isProductionUser;

            var identity = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim("UserId", userId.ToString()),
                new Claim(ClaimTypes.Role, role),
                new Claim("IsQrLogin", isQrLogin.ToString()),
                new Claim("IsProductionUser", isProductionUser.ToString())
            }, "apiauth_type");

            _currentUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(
                GetAuthenticationStateAsync());
        }

        public void MarkUserAsLoggedOut()
        {
            IsQrLogin = false;
            IsProductionUser = false;

            _currentUser = _anonymous;

            NotifyAuthenticationStateChanged(
                GetAuthenticationStateAsync());
        }
    }
}