using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Auth;
using V.SMART.Api.Middleware;
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
            // M2-A06 — same status and same message as before this task; only the body shape
            // changes, to application/problem+json. The message is reproduced verbatim and
            // carries no connection string (R-01).
            var tenant = _tenantProvider.GetCurrentTenant();
            if (tenant == null)
                return this.TenantUnresolvedProblem(
                    StatusCodes.Status400BadRequest,
                    "Unable to resolve tenant. Check host or wwwroot/config/tenant.json.");

            // M2-A06 — deliberately no more informative than it was before this task: one title
            // for every authentication failure, so the response cannot distinguish an unknown
            // user from a wrong password.
            var user = await _unitOfWork.Users.LoginAsync(request.Username, request.Password);
            if (user == null)
                return this.UnauthenticatedProblem("Invalid username or password.");

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
