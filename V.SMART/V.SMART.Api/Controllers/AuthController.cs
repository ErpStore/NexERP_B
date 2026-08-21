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
    [Route($"{ApiRoutes.V1}/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ITenantProvider _tenantProvider;
        private readonly IConfiguration _configuration;

        public AuthController(
            IUnitOfWork unitOfWork,
            JwtTokenService jwtTokenService,
            ITenantProvider tenantProvider,
            IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _tenantProvider = tenantProvider;
            _configuration = configuration;
        }

        /// <summary>
        /// <c>Login.razor:224</c> — <c>private bool IsDesktop =&gt; Configuration["AppEnvironment"] == "Desktop";</c>
        /// read from this host's configuration, which is how the trial gate's first carve-out is
        /// evaluated. The API is not the desktop build, so this is normally <c>false</c> and the gate
        /// applies; the read exists so that a desktop-hosted API would behave as the desktop Blazor
        /// host does rather than silently diverge (M2-A08).
        /// </summary>
        private bool HostIsDesktop =>
            string.Equals(
                _configuration[TrialGate.HostEnvironmentKey],
                TrialGate.DesktopHostValue,
                StringComparison.Ordinal);

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

            // M2-A08 — the trial gate, in the position Login.razor:271 puts it: after the credential
            // check, before anything is issued. Before this task the API had no trial check at all,
            // so an expired-trial user authenticated cleanly here while being refused in Blazor.
            //
            // 403 with a distinguishable ProblemDetails.type, never a 401: the password was right,
            // and re-prompting for it cannot help. The message is Login.razor:273 byte for byte.
            //
            // The DEVICE gate (Login.razor:277-322) is deliberately NOT here — decision P4 is
            // deferred and unanswered, and the trust-on-first-use half cannot run server-side at all
            // (UserService.cs:722-725 calls IJSRuntime). DeviceGate in Auth/AccountGates.cs holds the
            // ported behaviour, tested, for whoever owns that decision. KB-108 §4.
            var trialRefusal = TrialGate.Evaluate(user, HostIsDesktop, DateTime.Today);
            if (trialRefusal is not null)
                return this.AccountGateProblem(trialRefusal);

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
