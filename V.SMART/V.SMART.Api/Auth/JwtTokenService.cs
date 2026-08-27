using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Services;

namespace V.SMART.Api.Auth
{
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;

        public JwtTokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public IssuedAccessToken CreateToken(User user, int tenantId)
        {
            // M0-03-03: defer to the one code path that decides whether Jwt:Secret is
            // acceptable, so this guard cannot drift from the startup one. Startup
            // validation means this should never throw in a host that is running.
            StartupConfigurationValidator.ValidateJwtSecret(_configuration);
            var secret = _configuration["Jwt:Secret"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new("UserId", user.UserId.ToString()),
                new("TenantId", tenantId.ToString()),
                new(ClaimTypes.Role, user.Role?.ToString() ?? string.Empty)
            };

            // M2-A04 — shortened from the previous 480-minute (8-hour) default. 15 minutes is
            // roughly 1/32 of that window: comfortably short enough that IsActive is re-checked
            // often (RefreshTokenService.RotateAsync), while the 1-minute ClockSkew configured at
            // Program.cs:~192 stays under 7% of the lifetime it is skewing — clock drift cannot
            // meaningfully extend a stolen access token's usable life. Configurable, not
            // hard-coded; this is only the fallback if Jwt:ExpiresMinutes is unset or unparsable.
            var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var m) ? m : 15;

            var expiresAtUtc = DateTime.UtcNow.AddMinutes(expiresMinutes);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return new IssuedAccessToken(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }
    }

    /// <summary>M2-A04 — the access-token expiry travels with the token itself, rather than the
    /// caller re-deriving it from <c>Jwt:ExpiresMinutes</c> (which would drift the moment the two
    /// reads happen against different config, or someone changes the default in only one place).</summary>
    public sealed record IssuedAccessToken(string Token, DateTime ExpiresAtUtc);
}
