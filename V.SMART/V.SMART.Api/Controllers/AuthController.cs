using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Auth;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services.MultiCompanyService;

namespace V.SMART.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ITenantProvider _tenantProvider;

        public AuthController(
            IUnitOfWork unitOfWork,
            JwtTokenService jwtTokenService,
            ITenantProvider tenantProvider)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _tenantProvider = tenantProvider;
        }

        public record LoginRequest(
            [Required] string Username,
            [Required] string Password);

        public record LoginResponse(
            string Token,
            string Username,
            int UserId,
            int TenantId,
            string Role);

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            var tenant = _tenantProvider.GetCurrentTenant();
            if (tenant == null)
                return BadRequest(new { message = "Unable to resolve tenant. Check host or wwwroot/config/tenant.json." });

            var user = await _unitOfWork.Users.LoginAsync(request.Username, request.Password);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            var token = _jwtTokenService.CreateToken(user, tenant.Id);

            return Ok(new LoginResponse(
                token,
                user.UserName,
                user.UserId,
                tenant.Id,
                user.Role?.ToString() ?? string.Empty));
        }
    }
}
