using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Auth;
using V.SMART.Api.Middleware;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services.MultiCompanyService;

namespace V.SMART.Api.Controllers
{
    [ApiController]
    [Route($"{ApiRoutes.V1}/auth")]
    // M2-B10 - the OpenAPI tag is declared rather than inherited from the class name, so renaming
    // the controller cannot silently regroup the generated client.
    [Tags("Auth")]
    public class AuthController : ControllerBase
    {
        /// <summary>
        /// M2-A10 — the only user for whom the API seeds rights on login, mirroring
        /// <c>Login.razor:345</c>'s <c>if (user.UserId == 1)</c>.
        ///
        /// This gate IS the safety property, not an incidental detail:
        /// <c>SyncRightsForUserAsync</c> writes <c>CanView</c>, <c>CanCreate</c>, <c>CanEdit</c> and
        /// <c>CanDelete</c> all <c>true</c> (<c>UserRightService.cs:67-70</c>) for every screen the
        /// user has no row for, so widening it by even one user silently grants delete on all
        /// screens. That was option B in KB-109 and the owner rejected it on 2026-08-24.
        /// Do not generalise, do not make configurable.
        /// </summary>
        private const int AdministratorUserId = 1;

        private readonly IUnitOfWork _unitOfWork;
        private readonly JwtTokenService _jwtTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly ITenantProvider _tenantProvider;
        private readonly IConfiguration _configuration;
        private readonly IUserRightService _userRightService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IUnitOfWork unitOfWork,
            JwtTokenService jwtTokenService,
            IRefreshTokenService refreshTokenService,
            ITenantProvider tenantProvider,
            IConfiguration configuration,
            IUserRightService userRightService,
            ILogger<AuthController> logger)
        {
            _unitOfWork = unitOfWork;
            _jwtTokenService = jwtTokenService;
            _refreshTokenService = refreshTokenService;
            _tenantProvider = tenantProvider;
            _configuration = configuration;
            _userRightService = userRightService;
            _logger = logger;
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

        // M2-A04 — RefreshToken and TokenExpiresAtUtc are the only additions; the four original
        // fields keep their names, types and order. Breaking for the Angular pilot, which M2-C11
        // archives — recorded, not a live concern.
        public record LoginResponse(
            string Token,
            string RefreshToken,
            DateTime TokenExpiresAtUtc,
            string Username,
            int UserId,
            int TenantId,
            string Role);

        // M2-A04
        public record RefreshRequest([Required] string RefreshToken);

        public record RefreshResponse(
            string Token,
            string RefreshToken,
            DateTime TokenExpiresAtUtc);

        public record LogoutRequest([Required] string RefreshToken);

        /// <summary>
        /// Exchanges a username and password for a JWT bearer token. Every other endpoint requires
        /// the token this returns, sent as <c>Authorization: Bearer &lt;token&gt;</c>.
        /// </summary>
        [HttpPost("login", Name = "login")]
        [AllowAnonymous]
        // M2-B10 - KB-114 s11 / divergence 13.2: before this task Login declared nothing, so its
        // 400, 401 and 403 were invisible to the generated client. All three are real paths in the
        // body below; none is over-declared. There is no 401 from the authentication middleware
        // here because the action is [AllowAnonymous] - the 401 is the credential refusal.
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
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

            // M2-A10 — administrator rights seeding, in the position Login.razor:345-349 puts it:
            // after the credential and account gates, before anything is issued. Without it an
            // administrator who has only ever authenticated through the API holds zero UserRight
            // rows and ADR-004's filter answers 403 to every annotated endpoint.
            await SeedAdministratorRightsAsync(user.UserId);

            var token = _jwtTokenService.CreateToken(user, tenant.Id);
            var refreshToken = await _refreshTokenService.IssueAsync(user.UserId);

            return Ok(new LoginResponse(
                token.Token,
                refreshToken.RawValue,
                token.ExpiresAtUtc,
                user.UserName,
                user.UserId,
                tenant.Id,
                user.Role?.ToString() ?? string.Empty));
        }

        /// <summary>
        /// M2-A04 — exchanges a live refresh token for a new access/refresh pair. One-time use
        /// with rotation: the presented token is revoked in the same call that issues its
        /// replacement (<c>RefreshTokenService.RotateAsync</c>), so replaying it always fails.
        ///
        /// <para><b>Why <c>[AllowAnonymous]</c>.</b> The access token that would normally
        /// authenticate this caller may already be expired — that is the entire reason this
        /// endpoint exists. The refresh token itself is the credential.</para>
        ///
        /// <para><b>Tenant binding (BR-TEN-002).</b> Deliberately not re-derived from a JWT
        /// claim — an expired access token authenticates nobody, so <c>HttpContext.User</c> here
        /// carries no claims to read. Tenant context instead comes from the same
        /// <c>ITenantProvider</c> host-resolution path <c>Login</c> already uses (Host header /
        /// tenant.json), which is how <c>ApplicationDbContext</c> — and therefore which tenant's
        /// <c>RefreshTokens</c> table this call ever sees — was already resolved before this
        /// action runs. A token issued in tenant A is simply absent from tenant B's database;
        /// there is no cross-tenant row to find, so there is nothing to switch.</para>
        /// </summary>
        [HttpPost("refresh", Name = "refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshRequest request)
        {
            var result = await _refreshTokenService.RotateAsync(request.RefreshToken);

            // Testing item 10 / Target Result 5 — one body for every failure reason. The
            // *reason* (unknown, expired, revoked, deactivated user) is real information and is
            // logged server-side; the response tells an attacker nothing more than "no".
            if (result.Outcome != RefreshOutcome.Success || result.Issued is null || result.UserId is null)
            {
                _logger.LogInformation(
                    "[Auth] Refresh refused: {Outcome}.",
                    result.Outcome);
                return this.UnauthenticatedProblem("Invalid or expired refresh token.");
            }

            var user = await _unitOfWork.Users.FirstOrDefaultAsync(u => u.UserId == result.UserId.Value);
            if (user is null || !user.IsActive)
            {
                // Re-check mirrors RotateAsync's own — the row could not have rotated with an
                // inactive user, but IUnitOfWork.Users is a second, independent read and this
                // keeps the controller from ever minting a token for a user it cannot see.
                _logger.LogInformation("[Auth] Refresh refused: user vanished between rotation and lookup.");
                return this.UnauthenticatedProblem("Invalid or expired refresh token.");
            }

            var tenant = _tenantProvider.GetCurrentTenant();
            if (tenant is null)
                return this.TenantUnresolvedProblem(
                    StatusCodes.Status400BadRequest,
                    "Unable to resolve tenant. Check host or wwwroot/config/tenant.json.");

            var token = _jwtTokenService.CreateToken(user, tenant.Id);

            return Ok(new RefreshResponse(token.Token, result.Issued.RawValue, token.ExpiresAtUtc));
        }

        /// <summary>
        /// M2-A04 — revokes the presented refresh token. Revokes exactly that one token, not
        /// every token belonging to the user: the request contract is "the refresh token to
        /// revoke" (singular), matching one-session-per-device logout. A revoke-all/"sign out
        /// everywhere" capability is a natural extension, left to whichever future task needs it
        /// rather than assumed here.
        ///
        /// <para>Idempotent by design (Target Result 4 / the logout error-model row): revoking an
        /// unknown or already-revoked token still returns <c>204</c> — the response must never
        /// leak whether a token was ever valid.</para>
        /// </summary>
        [HttpPost("logout", Name = "logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            await _refreshTokenService.RevokeAsync(request.RefreshToken);
            return NoContent();
        }

        /// <summary>
        /// M2-A10 — mirrors <c>Login.razor:345-349</c>: sync rights for the administrator, and for
        /// nobody else.
        ///
        /// <para><b>Failure behaviour — the deliberate choice.</b> A seeding failure is logged and
        /// swallowed; the login still succeeds. Justification: the credential check and the account
        /// gates have already passed, so the caller IS authenticated, and rights seeding is a repair
        /// of a missing-rows condition rather than part of authentication. Letting it fail the login
        /// would convert any transient database fault during the repair into a total lockout of the
        /// one account that could fix it, while gaining nothing — an administrator whose rows failed
        /// to seed is in exactly the state they were already in, and ADR-004's filter still refuses
        /// the endpoints they lack rows for. Nothing is granted by continuing.</para>
        ///
        /// <para>This diverges from the Blazor page as written, but less than it first appears.
        /// <c>Login.razor:337</c> calls <c>MarkUserAsAuthenticated</c> <i>before</i> the seeding call
        /// at <c>:345-349</c>, so the Blazor user is already signed in when seeding runs; the page's
        /// catch (<c>:357-362</c>) only toasts an error and skips <c>NavigateTo("/dashboard")</c>,
        /// leaving them authenticated but stranded on the login page. The real divergence is
        /// therefore that Blazor loses the navigation while the API returns its normal 200. The
        /// scope of this task (<c>docs/kb/execution/tasks/M2-A10.md</c> §Scope 2, §Acceptance 3)
        /// requires the API to continue. Blazor is left byte-unchanged.</para>
        /// </summary>
        private async Task SeedAdministratorRightsAsync(int userId)
        {
            if (userId != AdministratorUserId)
                return;

            try
            {
                _logger.LogInformation(
                    "[UserRights] Administrator login detected. Syncing rights for UserId {UserId}.",
                    userId);

                await _userRightService.SyncRightsForUserAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "[UserRights] Error while syncing rights for UserId {UserId}. Login continues.",
                    userId);
            }
        }
    }
}
