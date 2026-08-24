using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using V.SMART.Api.Middleware;

namespace V.SMART.Api.Controllers
{
    /// <summary>
    /// M2-A07 — the SPA bootstrap endpoint: who the caller is, which tenant they are in, their
    /// role, and their complete screen-rights map, in one call.
    /// <para>
    /// <b>PRESENTATION ONLY (ADR-004 §3).</b> The map this endpoint returns exists so the client
    /// can render a permission-correct navigation and disable controls the caller cannot use. It
    /// is <b>never</b> the enforcement point. The server re-checks every request independently
    /// through <see cref="ScreenRightAuthorizationFilter"/>, and nothing here relaxes that filter.
    /// A client must not treat this response as authoritative.
    /// </para>
    /// <para>
    /// <b>Route (M2-A07 decision, for M2-B01).</b> This ships at <c>/api/v1/me</c> — the name
    /// ADR-004 §3, KB-105 and KB-080 §9 all use, and the name M2-C02 will code against — making it
    /// the first versioned route in an API whose other routes are still <c>api/auth</c> and
    /// <c>api/currencies</c>. The version segment is a literal here because the versioning
    /// mechanism does not exist yet. When M2-B01 lands it must re-express this route through the
    /// versioning convention <b>without changing the URL</b>; the URL is the contract, the literal
    /// is not.
    /// </para>
    /// <para>
    /// <b>Thin controller (ADR-002 §2).</b> Read claims, one provider call, map, return. No
    /// business logic, no database access of its own: every value in the response comes either
    /// from the caller's own token or from <see cref="IUserRightsProvider"/>.
    /// </para>
    /// </summary>
    [ApiController]
    [Route("api/v1/me")]
    [Tags("Me")]
    [Authorize]
    [NoScreenRight(
        "M2-A07 — a caller's own identity is not a screen. Gating /api/v1/me on a screen right " +
        "would deadlock the SPA: the client cannot know which rights it holds until it has read " +
        "this endpoint. Authentication is still required ([Authorize] above); only the " +
        "screen-right check is waived, explicitly and auditably (KB-105 §2.4).")]
    public class MeController : ControllerBase
    {
        private readonly IUserRightsProvider _rightsProvider;

        public MeController(IUserRightsProvider rightsProvider)
        {
            _rightsProvider = rightsProvider;
        }

        /// <summary>
        /// One screen's five booleans, named exactly as the client's <c>ScreenRight</c> interface
        /// names them (KB-050 § Permission-based rendering). <c>Hidden</c> carries <c>IsHide</c>:
        /// navigation is filtered on <c>view &amp;&amp; !hidden</c>, which is the flag's actual
        /// purpose — it is not, and must not become, a second gate.
        /// </summary>
        public record ScreenRight(
            bool View,
            bool Create,
            bool Edit,
            bool Delete,
            bool Hidden);

        /// <summary>
        /// <para>
        /// <c>Rights</c> is keyed by <c>Screens.ScreenName</c> verbatim, so a client lookup is the
        /// one-step equivalent of
        /// <c>rights.FirstOrDefault(r =&gt; r.Screens.ScreenName == screenName)?.CanX ?? false</c>
        /// (<c>V.SMART/V.SMART.Shared/Shared/RightsHelper.cs:7-20</c>).
        /// </para>
        /// <para>
        /// <b>Absent rows are omitted, not materialised</b> (BR-AUTH-002). A screen the caller
        /// holds no <c>UserRight</c> row for simply has no key — the response never carries 152
        /// all-<c>false</c> entries. The client's default for a missing key is therefore
        /// <b>deny</b>, matching the database and matching the <c>?? false</c> the Blazor UI has
        /// always applied. A client that defaults a missing key to "allow" is wrong, and this
        /// contract says so.
        /// </para>
        /// <para>
        /// No <c>User</c> flag beyond the identity is included, and no connection string, password
        /// or token ever is (R-01). See the endpoint's remarks for the per-field justification.
        /// </para>
        /// </summary>
        public record MeResponse(
            int UserId,
            string UserName,
            int TenantId,
            string Role,
            IReadOnlyDictionary<string, ScreenRight> Rights);

        /// <summary>
        /// <b>What this response deliberately does not contain, and why</b> (M2-A07 step 3 — an
        /// over-broad <c>/me</c> becomes a personal-data endpoint by accident):
        /// <list type="bullet">
        /// <item><description><c>User.IsViewOnly</c> — not a runtime gate. At user creation it
        /// materialises a <c>UserRight</c> row per screen with <c>CanView</c> true and everything
        /// else false (<c>V.SMART/V.SMART.Shared/BusinessLayer/.../UserService.cs</c>), so its
        /// effect is already fully represented in <c>Rights</c>. Sending it too would invite a
        /// client to gate on it a second time and diverge.</description></item>
        /// <item><description><c>User.HideManualJobOrderButton</c> — read only by
        /// <c>JobOrderService.IsManualyJobOrderHiden</c> for one button on one screen. It belongs
        /// to the M3 job-order endpoint, not to the bootstrap payload.</description></item>
        /// <item><description><c>User.LevelAuthorization</c> — an approval-authority input, used
        /// when selecting approvers. ADR-004 §4 enforces approval authority inside the approval
        /// endpoints (module M3-4); it is not a UI gate.</description></item>
        /// <item><description><c>User.IsProductionBased</c> — reaches the Blazor menu only through
        /// the QR-login path (<c>ShowProductionMenu = IsQrLogin &amp;&amp; IsProductionUser</c>,
        /// <c>V.SMART/V.SMART.Shared/Layout/NavMenu.razor</c>). The API has no QR-login path, so
        /// on its own the flag changes nothing. Deferred until an SPA screen needs it.</description></item>
        /// <item><description><c>UserAuthority</c> (12 document-type/level pairs) — <b>deferred to
        /// M3-4</b>. ADR-004 §4 enforces it server-side inside the approval endpoints, no M2
        /// consumer references it, and KB-050's permission contract has no slot for
        /// it.</description></item>
        /// <item><description>The tenant's display name and every other <c>TenantInfo</c> member —
        /// <c>TenantInfo</c> carries a plaintext credentialed <c>ConnectionString</c> (R-01), so
        /// this endpoint projects the tenant identity from the token claim and never touches the
        /// tenant record at all.</description></item>
        /// </list>
        /// <para>
        /// <b>Role source (M2-A07 decision).</b> The <c>ClaimTypes.Role</c> claim minted by
        /// <c>JwtTokenService</c>, not a fresh <c>User</c> row read: the login response
        /// (<c>AuthController.LoginResponse.Role</c>) is that same expression, so claim-sourcing
        /// keeps <c>/me</c> and <c>/auth/login</c> from disagreeing inside one token's lifetime,
        /// and it costs no query. <c>UserRole</c> has exactly two members, <c>Administrator</c> and
        /// <c>User</c>; the role the Blazor <c>AuthorizeView</c>s also name, <c>ERPAdmin</c>, does
        /// not exist in that enum (R-31) and is deliberately <b>not</b> propagated here.
        /// <c>CurrentUserService.GetUserRoleAsync()</c> is <b>not</b> called: it reads claim type
        /// <c>"role"</c> while both providers write <c>ClaimTypes.Role</c> (R-18).
        /// </para>
        /// <para>
        /// <b>Failure is loud.</b> A rights-load fault propagates out of
        /// <see cref="IUserRightsProvider"/> to M2-A06's handler as a <c>500</c>. It must never
        /// degrade to an empty map: an empty map renders as "no permissions" and reads to a user
        /// as a permission problem rather than an outage. The provider never negative-caches, so
        /// nothing is written on failure either.
        /// </para>
        /// </summary>
        [HttpGet(Name = "getMe")]
        [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
        // M2-B10 - 401 is reachable two ways: the authentication middleware, and InvalidTokenProblem
        // below. No 403: the controller is [NoScreenRight], so the screen-right filter never denies
        // it, and declaring one would put a dead branch in the generated client (KB-114 s11).
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<MeResponse>> Get(CancellationToken cancellationToken)
        {
            // Claims are read directly, exactly as ScreenRightAuthorizationFilter reads them, and
            // for the same reason: CurrentUserService.GetUserIdAsync() silently returns 0 for a
            // missing or unparseable claim. A token with no usable subject is a malformed
            // credential — 401, never 403 (KB-105 D-3), and the body shape is M2-A06's.
            if (!TryGetPositiveIntClaim("UserId", out var userId))
            {
                return InvalidTokenProblem("UserId");
            }

            if (!TryGetPositiveIntClaim("TenantId", out var tenantId))
            {
                return InvalidTokenProblem("TenantId");
            }

            // BR-TEN-002 — the tenant is the JWT's, and the rights are read from that tenant's
            // database. No parameter on this endpoint can name another user or another tenant:
            // the caller is identified entirely by the bearer token.
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

            // THE SAME SEAM THE FILTER USES (M2-A01-02/03). Not the repository: a second read path
            // would eventually disagree with the filter, and a UI that offers a button the API
            // refuses is worse than no button. Also the same cache, so the map rendered and the
            // map enforced are the same object within the TTL.
            var rights = await _rightsProvider.GetAsync(tenantId, userId, cancellationToken);

            return Ok(new MeResponse(
                userId,
                userName,
                tenantId,
                ReadRole(),
                MapRights(rights)));
        }

        /// <summary>
        /// First-match-wins, mirroring <see cref="ScreenRightSet.Has"/> and through it
        /// <c>RightsHelper</c>. If duplicate <c>(UserId, ScreenId)</c> rows exist for one screen
        /// (Q-27, unresolved), the map must keep the <b>first</b> entry — the same one the filter's
        /// <c>FirstOrDefault</c> decides on — or the rendered map and the enforced map would
        /// disagree on exactly those rows. <c>TryAdd</c> is what makes that true, and the ordinal
        /// comparer is <c>string ==</c> spelt out, not a change of semantics.
        /// </summary>
        private static IReadOnlyDictionary<string, ScreenRight> MapRights(ScreenRightSet rights)
        {
            var map = new Dictionary<string, ScreenRight>(rights.Entries.Count, StringComparer.Ordinal);

            foreach (var entry in rights.Entries)
            {
                map.TryAdd(
                    entry.ScreenName,
                    new ScreenRight(entry.CanView, entry.CanCreate, entry.CanEdit, entry.CanDelete, entry.IsHide));
            }

            return map;
        }

        /// <summary>
        /// <c>ClaimTypes.Role</c> is the claim <c>JwtTokenService</c> writes. It travels on the
        /// wire as the short name <c>"role"</c> and the JWT handler's inbound map normally restores
        /// it, but that map can be switched off by configuration; the short-name fallback below
        /// makes this read correct either way. That is the opposite of R-18, which reads
        /// <c>"role"</c> <b>only</b> and therefore always returns <c>""</c>.
        /// <para>
        /// Empty string, never null: <c>User.Role</c> is nullable despite its <c>[Required]</c>
        /// attribute, and <c>JwtTokenService</c> already collapses a null role to <c>""</c>, which
        /// <c>AuthController.LoginResponse</c> also returns. This keeps the two agreeing.
        /// </para>
        /// </summary>
        private string ReadRole()
            => User.FindFirst(ClaimTypes.Role)?.Value
               ?? User.FindFirst("role")?.Value
               ?? string.Empty;

        private bool TryGetPositiveIntClaim(string claimName, out int value)
        {
            value = 0;

            var raw = User.FindFirst(claimName)?.Value;
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

        /// <summary>
        /// The same body the filter produces for the same condition (KB-105 §7.2), so a client sees
        /// one shape for "your token is unusable" wherever it is detected.
        /// </summary>
        private ActionResult InvalidTokenProblem(string claimName)
            => ApiProblems.ToResult(ApiProblems.Create(
                HttpContext,
                StatusCodes.Status401Unauthorized,
                ProblemTypes.InvalidToken,
                "The access token is missing a required claim.",
                $"The token does not carry a usable '{claimName}' claim."));
    }
}
