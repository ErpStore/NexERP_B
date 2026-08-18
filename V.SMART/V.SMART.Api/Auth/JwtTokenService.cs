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

        public string CreateToken(User user, int tenantId)
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

            var expiresMinutes = int.TryParse(_configuration["Jwt:ExpiresMinutes"], out var m) ? m : 480;

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
