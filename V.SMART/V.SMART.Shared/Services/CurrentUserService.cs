using Microsoft.AspNetCore.Components.Authorization;
using System.Net;

namespace V.SMART.Shared.Services
{
    public class CurrentUserService
    {
        private readonly AuthenticationStateProvider _authProvider;

        private string? _username;
        private int? _userId;

        public string MachineName { get; private set; }
        public string IpAddress { get; private set; }

        public CurrentUserService(AuthenticationStateProvider authProvider)
        {
            _authProvider = authProvider;

            MachineName = Environment.MachineName;

            try
            {
                IpAddress = Dns.GetHostEntry(MachineName)
                    .AddressList
                    .First(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .ToString();
            }
            catch
            {
                IpAddress = "Unavailable";
            }
        }

        public async Task<string> GetUsernameAsync()
        {
            if (!string.IsNullOrEmpty(_username))
                return _username;

            var authState = await _authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            _username = user.Identity?.IsAuthenticated == true
                ? user.Identity.Name ?? string.Empty
                : string.Empty;

            return _username;
        }

        public async Task<int> GetUserIdAsync()
        {
            // Only return cached value if it's greater than 0
            if (_userId.HasValue && _userId.Value > 0)
                return _userId.Value;

            var authState = await _authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            var userIdClaim = user.Claims.FirstOrDefault(c => c.Type == "UserId");

            _userId = userIdClaim != null && int.TryParse(userIdClaim.Value, out int id)
                ? id
                : 0;

            return _userId ?? 0;
        }

        public async Task<string> GetUserRoleAsync()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            var roleClaim = user.Claims.FirstOrDefault(c => c.Type == "role");
            return roleClaim?.Value ?? string.Empty;
        }

    }
}
