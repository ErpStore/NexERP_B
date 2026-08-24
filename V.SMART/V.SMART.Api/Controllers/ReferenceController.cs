using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using V.SMART.Api.Authorization;
using V.SMART.Api.Caching;
using V.SMART.Api.Contracts;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.Utility_Constants;

namespace V.SMART.Api.Controllers
{
    /// <summary>
    /// M2-B09 — the small, slow-changing lookup lists every screen needs before it can render a
    /// dropdown, behind one cached route group. Closes KB-041 item <b>B6</b> and the
    /// reference-data half of <b>C1</b>.
    ///
    /// <para><b>Every action is a single <c>ICommonService</c> call plus a projection.</b> No
    /// query is written against <c>ApplicationDbContext</c> here and no business logic lives
    /// here (ADR-002 §2). The one action that is not a service call — <c>gst-rates</c> — reads
    /// the domain's own constants rather than restating them.</para>
    ///
    /// <para><b>Why <c>[NoScreenRight]</c> and not <c>[RequireScreen]</c>.</b> Reference data is
    /// what a screen needs <i>in order to render at all</i>; there is no single screen that owns
    /// "the list of states". Gating it on a screen right would deadlock the UI for exactly the
    /// reason KB-105 §2.4 gives for <c>GET /api/v1/me</c> — a user must be able to fetch the
    /// vocabulary before the app can decide what to show them. The endpoints still require
    /// authentication and are still tenant-scoped; this is an explicit, greppable opt-out from
    /// the screen-right axis, not from authorization.</para>
    /// </summary>
    [ApiController]
    [Route($"{ApiRoutes.V1}/reference")]
    [Authorize]
    [NoScreenRight("Reference data is a precondition for rendering any screen, so no single screen owns it; gating it on a screen right would deadlock the UI exactly as it would for GET /api/v1/me (KB-105 §2.4). Authentication and tenant scoping still apply.")]
    [OutputCache(PolicyName = ReferenceCachePolicy.PolicyName)]
    [Tags("Reference")]
    public class ReferenceController : ControllerBase
    {
        private readonly ICommonService _commonService;

        public ReferenceController(ICommonService commonService)
        {
            _commonService = commonService;
        }

        /// <summary>
        /// The two GST ladders, paired by index. Read from <c>CommonConstants</c>, never retyped.
        /// </summary>
        /// <remarks>
        /// This action is not tenant-dependent — the ladders are compile-time constants — but it
        /// shares the group's tenant-keyed policy anyway. Carving out a per-endpoint exemption
        /// would buy a marginal cache-entry saving in exchange for two policies to reason about,
        /// and the failure mode of getting that wrong is a cross-tenant leak. Uniform is safer.
        /// </remarks>
        [HttpGet("gst-rates", Name = "getGstRates")]
        [ProducesResponseType(typeof(GstRatesResponse), StatusCodes.Status200OK)]
        // M2-B10 - 401 only. The controller is [NoScreenRight], so the screen-right filter never
        // produces a 403 here (KB-114 s11: never declare a status the action cannot return).
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public ActionResult<GstRatesResponse> GetGstRates()
            => Ok(new GstRatesResponse(CommonConstants.IGSTRates, CommonConstants.GSTRates));

        /// <summary>Units of measure. <c>UnitCode</c> is the key; this table has no integer id.</summary>
        [HttpGet("uoms", Name = "getUoms")]
        [ProducesResponseType(typeof(IEnumerable<UomDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<UomDto>>> GetUoms()
        {
            var uoms = await _commonService.GetUOMsAsync();

            return Ok(uoms.Select(u => new UomDto(u.UnitCode, u.UnitDescription, u.IsSystemDefined)));
        }

        /// <summary>States.</summary>
        [HttpGet("states", Name = "getStates")]
        [ProducesResponseType(typeof(IEnumerable<StateDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<StateDto>>> GetStates()
        {
            var states = await _commonService.GetStatesAsync();

            return Ok(states.Select(s => new StateDto(s.StateCode, s.StateName, s.IsSystemDefined)));
        }

        /// <summary>
        /// Active terms and conditions. The active filter is the domain's — this action calls
        /// <c>GetAllActiveTermsAsync</c> and does not re-filter, so the API cannot disagree with
        /// the Blazor screens about what "active" means.
        /// </summary>
        [HttpGet("terms", Name = "getTerms")]
        [ProducesResponseType(typeof(IEnumerable<TermsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<TermsDto>>> GetTerms()
        {
            var terms = await _commonService.GetAllActiveTermsAsync();

            return Ok(terms.Select(t => new TermsDto(t.Id, t.Title, t.Details)));
        }

        /// <summary>
        /// The screen catalogue — the permission <b>vocabulary</b>, not the caller's rights.
        /// </summary>
        /// <remarks>
        /// <para><b>The protection decision, recorded here because it is a security decision.</b>
        /// This returns every screen name in the tenant, which is a map of the application's
        /// surface. It is <i>not</i> sensitive per-user data: the same list is identical for
        /// every user in the tenant, and it discloses nothing about who may do what. The
        /// caller's own rights are a different endpoint (<c>GET /api/v1/me</c>, M2-A07) and are
        /// deliberately not merged into this one — merging them would make this response
        /// caller-dependent and force the cache key to include the user, multiplying the entry
        /// count by the user base for no benefit.</para>
        /// <para>It therefore requires authentication, is tenant-keyed, and is cached like the
        /// rest of the group. It projects to <see cref="ScreenDto"/> specifically to drop the
        /// <c>UserRights</c> navigation — returning the entity would put the tenant's entire
        /// permission matrix on the wire behind a dropdown feed.</para>
        /// </remarks>
        [HttpGet("screens", Name = "getScreens")]
        [ProducesResponseType(typeof(IEnumerable<ScreenDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<ScreenDto>>> GetScreens()
        {
            var screens = await _commonService.GetAllScreenAsync();

            return Ok(screens.Select(s => new ScreenDto(s.Id, s.ScreenCode, s.ScreenName, s.IsPrintRequired)));
        }

        /// <summary>
        /// Currencies, as reference data for dropdowns.
        /// </summary>
        /// <remarks>
        /// <b>Distinct from <c>CurrencyController</c>, deliberately.</b> That controller is the
        /// CRUD surface for the Currency master — paged, sorted, filtered, writable
        /// (<c>/api/v1/currencies</c>). This is a flat, cached, read-only list for populating a
        /// selector (<c>/api/v1/reference/currencies</c>). Different lifetime, different shape,
        /// different cacheability; the route group and these summaries are what keep them
        /// distinguishable in Swagger. It excludes the <c>CurrencyRates</c> navigation — the
        /// daily rate feed changes on a different clock and is what makes the entity unsafe to
        /// cache.
        /// </remarks>
        [HttpGet("currencies", Name = "getReferenceCurrencies")]
        [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<CurrencyDto>>> GetCurrencies()
        {
            var currencies = await _commonService.GetCurrenciesAsync();

            return Ok(currencies.Select(c => new CurrencyDto(c.CurrId, c.CurrName, c.CurrSub, c.Symbol, c.IsSystemDefined)));
        }
    }
}
