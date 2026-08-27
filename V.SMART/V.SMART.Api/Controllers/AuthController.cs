using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
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

        // M2-A05 — IUnitOfWork, IRefreshTokenService and IUserRightService are deliberately NOT
        // constructor parameters here. All three ultimately depend on the tenant-resolved
        // ApplicationDbContext (AddVSmartDomain(), ServiceCollectionExtensions.cs), which the
        // container's scoped factory (`services.AddScoped<ApplicationDbContext>(sp =>
        // factory.CreateDbContext())`) builds the FIRST time any of them is resolved from this
        // request's scope. ASP.NET Core constructs a controller — resolving every constructor
        // parameter — BEFORE it model-binds [FromBody] parameters, so a constructor-injected
        // IUnitOfWork would already have been built from whatever ITenantProvider.GetCurrentTenant()
        // returned before this request's JSON body (and its `tenant` field, ADR-002 §5) was ever
        // read. That is the exact "genuine chicken-and-egg" this task's own KB describes.
        // Resolving these three lazily, from _serviceProvider, AFTER _tenantProvider.SetTenant(...)
        // has run, is what makes ADR-002 §5's literal `{ tenant, username, password }` body shape
        // actually work — not a header, not a route segment, because those were never the decision
        // ADR-002 §5 recorded.
        private readonly IServiceProvider _serviceProvider;
        private readonly JwtTokenService _jwtTokenService;
        private readonly ITenantProvider _tenantProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IServiceProvider serviceProvider,
            JwtTokenService jwtTokenService,
            ITenantProvider tenantProvider,
            IConfiguration configuration,
            ILogger<AuthController> logger)
        {
            _serviceProvider = serviceProvider;
            _jwtTokenService = jwtTokenService;
            _tenantProvider = tenantProvider;
            _configuration = configuration;
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

        // M2-A05 — Tenant is the field ADR-002 §5 decided on: `{ tenant, username, password }`,
        // resolved against MasterDbContext.Tenants by Name or Hostname (TenantProvider.cs:64-66's
        // own pattern, reused rather than duplicated — see TenantProvider.GetCurrentTenant()'s new
        // step 0). Required, not optional: the Angular SPA is the only caller of this endpoint —
        // Blazor and the MAUI head resolve tenant in-process, through TenantProvider directly, and
        // never call this API's auth routes at all — and a cross-origin SPA has no other reliable
        // signal (its own host will never match a `Tenants.Hostname` row, and the API's
        // wwwroot/config/tenant.json fallback pins the whole API to one tenant, a dev-only shape).
        public record LoginRequest(
            [Required] string Tenant,
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

        // M2-A04. M2-A05 adds Tenant, for the same reason Login's does: RefreshTokenService is
        // constructed from the tenant-resolved ApplicationDbContext, and for a cross-origin SPA an
        // expired access token carries no usable claim to re-derive the tenant from (see Refresh's
        // own doc comment below). The client already knows its tenant — it sent it to log in — and
        // resends the same value here.
        public record RefreshRequest([Required] string Tenant, [Required] string RefreshToken);

        public record RefreshResponse(
            string Token,
            string RefreshToken,
            DateTime TokenExpiresAtUtc);

        // M2-A05 — Tenant added for the identical reason RefreshRequest's was: RevokeAsync needs
        // the tenant-resolved RefreshTokens table bound before it can run.
        public record LogoutRequest([Required] string Tenant, [Required] string RefreshToken);

        /// <summary>
        /// Exchanges a tenant identifier, username and password for a JWT bearer token. Every
        /// other endpoint requires the token this returns, sent as
        /// <c>Authorization: Bearer &lt;token&gt;</c>.
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
            // M2-A05 — bind the tenant BEFORE resolving anything that depends on the tenant-scoped
            // ApplicationDbContext. SetTenant() was a dead setter before this task (it assigned
            // _manualTenant but GetCurrentTenant() never read it) — this is the fix, additive, per
            // ADR-002 §5. Deliberately no more informative on failure than "unable to resolve": a
            // tenant identifier is not a secret (R-01 is about connection strings, not names), but
            // this task's own KB flags it as enumerable, so the response does not echo the value
            // back or distinguish "no such tenant" from any other resolution failure.
            _tenantProvider.SetTenant(request.Tenant);
            var tenant = _tenantProvider.GetCurrentTenant();
            if (tenant == null)
                return this.TenantUnresolvedProblem(
                    StatusCodes.Status400BadRequest,
                    "Unable to resolve tenant.");

            // M2-A05 — only now, with the tenant bound, is it safe to resolve a service that
            // reaches the tenant-scoped ApplicationDbContext. See the constructor's own comment.
            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();

            // M2-A06 — deliberately no more informative than it was before this task: one title
            // for every authentication failure, so the response cannot distinguish an unknown
            // user from a wrong password.
            var user = await unitOfWork.Users.LoginAsync(request.Username, request.Password);
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
            // rights and ADR-004's filter answers 403 to every annotated endpoint.
            var userRightService = _serviceProvider.GetRequiredService<IUserRightService>();
            await SeedAdministratorRightsAsync(userRightService, user.UserId);

            var token = _jwtTokenService.CreateToken(user, tenant.Id);
            var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();
            var refreshToken = await refreshTokenService.IssueAsync(user.UserId);

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
        /// <para><b>Tenant binding (BR-TEN-002), M2-A05.</b> Deliberately not re-derived from a
        /// JWT claim — an expired access token authenticates nobody, so <c>HttpContext.User</c>
        /// here carries no claims to read, and for a cross-origin SPA the host-based fallback
        /// (step 2) can never match either. The client resends the same <c>tenant</c> value it
        /// logged in with, bound the same way <c>Login</c> binds it — which is how
        /// <c>ApplicationDbContext</c>, and therefore which tenant's <c>RefreshTokens</c> table
        /// this call ever sees, gets resolved before this action's tenant-scoped services are
        /// touched. A token issued in tenant A is simply absent from tenant B's database; there
        /// is no cross-tenant row to find, so there is nothing to switch — database-per-tenant
        /// makes "cannot permit a tenant switch" structural, not something this action has to
        /// check for itself.</para>
        /// </summary>
        [HttpPost("refresh", Name = "refresh")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<RefreshResponse>> Refresh([FromBody] RefreshRequest request)
        {
            // M2-A05 — see Login's identical opening and the constructor's comment.
            _tenantProvider.SetTenant(request.Tenant);
            var tenant = _tenantProvider.GetCurrentTenant();
            if (tenant is null)
                return this.TenantUnresolvedProblem(
                    StatusCodes.Status400BadRequest,
                    "Unable to resolve tenant.");

            var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();
            var result = await refreshTokenService.RotateAsync(request.RefreshToken);

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

            var unitOfWork = _serviceProvider.GetRequiredService<IUnitOfWork>();
            var user = await unitOfWork.Users.FirstOrDefaultAsync(u => u.UserId == result.UserId.Value);
            if (user is null || !user.IsActive)
            {
                // Re-check mirrors RotateAsync's own — the row could not have rotated with an
                // inactive user, but IUnitOfWork.Users is a second, independent read and this
                // keeps the controller from ever minting a token for a user it cannot see.
                _logger.LogInformation("[Auth] Refresh refused: user vanished between rotation and lookup.");
                return this.UnauthenticatedProblem("Invalid or expired refresh token.");
            }

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
        ///
        /// <para><b>Tenant binding, M2-A05.</b> Same reason and same mechanism as <c>Refresh</c>:
        /// <c>IRefreshTokenService</c> is tenant-scoped, so the tenant must be bound before it is
        /// resolved. An unresolved tenant still fails loudly here (400) rather than silently —
        /// that is not the same kind of information as "was this token ever valid," which stays
        /// opaque regardless.</para>
        /// </summary>
        [HttpPost("logout", Name = "logout")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
        {
            _tenantProvider.SetTenant(request.Tenant);
            var tenant = _tenantProvider.GetCurrentTenant();
            if (tenant is null)
                return this.TenantUnresolvedProblem(
                    StatusCodes.Status400BadRequest,
                    "Unable to resolve tenant.");

            var refreshTokenService = _serviceProvider.GetRequiredService<IRefreshTokenService>();
            await refreshTokenService.RevokeAsync(request.RefreshToken);
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
        private async Task SeedAdministratorRightsAsync(IUserRightService userRightService, int userId)
        {
            if (userId != AdministratorUserId)
                return;

            try
            {
                _logger.LogInformation(
                    "[UserRights] Administrator login detected. Syncing rights for UserId {UserId}.",
                    userId);

                await userRightService.SyncRightsForUserAsync(userId);
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
