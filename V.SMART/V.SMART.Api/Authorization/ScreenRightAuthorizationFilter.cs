using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using V.SMART.Api.Middleware;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// Enforces BR-AUTH-002 on the server (ADR-004, KB-105 §2.8). Registered globally, it decides
    /// only for endpoints that declare <see cref="RequireScreenAttribute"/> and
    /// <see cref="RequireRightAttribute"/>; every other endpoint is passed through untouched.
    /// <para>
    /// It is an MVC filter, not middleware, so it necessarily runs after
    /// <c>UseAuthentication()</c> has populated <c>HttpContext.User</c>
    /// (<c>Program.cs:181-183</c>). No change to the middleware order is required or made.
    /// </para>
    /// </summary>
    public sealed class ScreenRightAuthorizationFilter : IAsyncAuthorizationFilter
    {
        /// <summary>Used in the body when the endpoint failed to declare a screen (KB-105 D-4).</summary>
        private const string UndeclaredScreen = "(undeclared)";

        /// <summary>Used in the body when the endpoint failed to declare a right (KB-105 D-4).</summary>
        private const string UndeclaredRight = "(undeclared)";

        private readonly IUserRightsProvider _provider;
        private readonly ILogger<ScreenRightAuthorizationFilter> _logger;

        public ScreenRightAuthorizationFilter(
            IUserRightsProvider provider,
            ILogger<ScreenRightAuthorizationFilter> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var metadata = context.ActionDescriptor.EndpointMetadata;

            // (1) Anonymous endpoints are none of this filter's business - authentication is
            // [Authorize]'s job. AuthController.Login stays reachable.
            if (metadata.OfType<IAllowAnonymous>().Any())
            {
                return;
            }

            // (2) The explicit, auditable opt-out (KB-105 section 2.4).
            if (metadata.OfType<NoScreenRightAttribute>().Any())
            {
                return;
            }

            // Attribute discovery. EndpointMetadata carries controller-level attributes before
            // action-level ones, so Last() is the nearest declaration if both ever appear.
            var screenAttribute = metadata.OfType<RequireScreenAttribute>().LastOrDefault();
            var rightAttribute = metadata.OfType<RequireRightAttribute>().LastOrDefault();

            // An endpoint that declares neither attribute is not yet part of this mechanism, and
            // the filter must not decide for it.
            //
            // KB-105 D-4 would deny here as well, and ScreenRightStartupValidator would refuse to
            // start. M2-A01-02 deliberately does NOT enable that direction: no controller is
            // annotated in this task (M2-A02 does that), so switching it on now would 403 - and,
            // in the startup form, refuse to start at all - for all six of the API's existing
            // endpoints, which this task requires to behave exactly as before. The direction is
            // enabled by M2-A02, when the first controller carries the attributes. Until then the
            // R-03 hole this filter closes stays open for unannotated controllers, which is
            // precisely why R-03 remains open until M2-A02 and M2-A03.
            if (screenAttribute is null && rightAttribute is null)
            {
                return;
            }

            // (3)(4) Identity. Read the claims directly: CurrentUserService.GetUserIdAsync()
            // silently returns 0 for a missing or unparseable claim (CurrentUserService.cs:59-65,
            // KB-105 F-4) and must not be used here. ClaimTypes.Role is deliberately never read -
            // there is no Administrator bypass (KB-105 D-5, T-13) - and neither is a claim typed
            // "role" (R-18).
            if (!TryGetPositiveIntClaim(context, "UserId", out var userId))
            {
                DenyUnusableClaim(context, "UserId");
                return;
            }

            if (!TryGetPositiveIntClaim(context, "TenantId", out var tenantId))
            {
                DenyUnusableClaim(context, "TenantId");
                return;
            }

            // (5) Half-annotated endpoints fail closed at request time (KB-105 D-4, T-11/T-12).
            // ScreenRightStartupValidator refuses to start for the same two conditions; both paths
            // exist because a dynamically registered application part would bypass the startup
            // sweep.
            if (screenAttribute is null || rightAttribute is null)
            {
                Deny(
                    context,
                    screenAttribute?.ScreenName ?? UndeclaredScreen,
                    rightAttribute?.Right.ToString() ?? UndeclaredRight);
                return;
            }

            var screen = screenAttribute.ScreenName;
            var right = rightAttribute.Right;

            // (6) An exception here propagates to the M2-A06 handler. A database fault must never
            // read as "no rights" (KB-105 section 7.3).
            var rights = await _provider.GetAsync(tenantId, userId, context.HttpContext.RequestAborted);

            // (7) Ambiguity is logged, never acted on: first-match-wins is the semantic being
            // preserved (KB-105 D-2).
            var matchCount = rights.MatchCount(screen);
            if (matchCount > 1)
            {
                _logger.LogWarning(
                    "Ambiguous screen rights: tenant {TenantId}, user {UserId}, screen {Screen} matched {MatchCount} UserRight rows. " +
                    "The first row returned by the query decided the outcome, exactly as RightsHelper does (KB-105 D-2, Q-27).",
                    tenantId, userId, screen, matchCount);
            }

            // (8) The decision. rights.Has mirrors
            // rights.FirstOrDefault(r => r.Screens.ScreenName == screen)?.CanX ?? false
            // (RightsHelper.cs:7-20), including the FirstOrDefault and the "?? false".
            // IsHide is not consulted: it is navigation, not a gate (T-4).
            if (!rights.Has(screen, right))
            {
                Deny(context, screen, right.ToString());
                return;
            }

            // (9) Allow: return without setting context.Result.
        }

        /// <summary>
        /// The <b>only</b> place in this filter that produces a <c>403</c>, and it delegates to the
        /// only place in the API that constructs the body - <c>ApiProblems.ScreenRightDenied</c>
        /// (M2-A06, <c>Middleware/ApiProblems.cs:73-88</c>, shape frozen by KB-105 section 7.1).
        /// <c>ProblemResults.ScreenRightDeniedProblem</c> extends <c>ControllerBase</c> and so
        /// cannot be called from a filter; <c>ApiProblems.ToResult</c> is the equivalent that can.
        /// </summary>
        private static void Deny(AuthorizationFilterContext context, string screen, string right)
        {
            context.Result = ApiProblems.ToResult(
                ApiProblems.ScreenRightDenied(context.HttpContext, screen, right));
        }

        /// <summary>
        /// 401, never 403 (KB-105 D-3, T-10): a token carrying no usable subject is a malformed
        /// credential, not a permission outcome. Body per KB-105 section 7.2 - no <c>screen</c> or
        /// <c>right</c> member.
        /// </summary>
        private static void DenyUnusableClaim(AuthorizationFilterContext context, string claimName)
        {
            context.Result = ApiProblems.ToResult(ApiProblems.Create(
                context.HttpContext,
                StatusCodes.Status401Unauthorized,
                ProblemTypes.InvalidToken,
                "The access token is missing a required claim.",
                $"The token does not carry a usable '{claimName}' claim."));
        }

        /// <summary>
        /// Missing, unparseable or <c>&lt;= 0</c> all fail. The <c>&lt;= 0</c> guard is what makes
        /// the divergence from <c>CurrentUserService</c>'s silent <c>0</c> total (KB-105 D-3).
        /// </summary>
        private static bool TryGetPositiveIntClaim(AuthorizationFilterContext context, string claimName, out int value)
        {
            value = 0;

            var raw = context.HttpContext.User?.FindFirst(claimName)?.Value;
            if (string.IsNullOrWhiteSpace(raw))
            {
                return false;
            }

            if (!int.TryParse(
                    raw,
                    System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var parsed))
            {
                return false;
            }

            if (parsed <= 0)
            {
                return false;
            }

            value = parsed;
            return true;
        }
    }
}
