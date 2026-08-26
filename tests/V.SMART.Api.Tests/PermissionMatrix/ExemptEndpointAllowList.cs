namespace V.SMART.Api.Tests.PermissionMatrix
{
    /// <summary>
    /// M2-A03 — <b>the allow-list</b>. Every endpoint of the API that is <i>not</i> gated by
    /// <c>[RequireScreen]</c> + <c>[RequireRight]</c> is named here, by hand, with a written
    /// reason. This is the file a reviewer reads to answer "what is currently ungated?" without
    /// grepping the assembly.
    ///
    /// <para><b>The rule this file exists to enforce: an endpoint must never become exempt by
    /// omission.</b> <see cref="EndpointDiscoveryTests"/> compares the two sets below against
    /// what the assembly actually declares, <i>in both directions</i>. Adding
    /// <c>[AllowAnonymous]</c> or <c>[NoScreenRight]</c> to a new action therefore fails the
    /// suite until someone edits this file and states why — and deleting an entry here fails it
    /// too, so the list cannot rot into a stale superset.</para>
    ///
    /// <para><b>Two lists, because they are two different exemptions.</b>
    /// <list type="bullet">
    /// <item><see cref="AnonymousActions"/> — no authentication at all. The task's "the allow-list
    /// of anonymous/exempt endpoints; <c>POST /api/v1/auth/login</c> is its only entry today"
    /// is this list, and it does still have exactly one entry.</item>
    /// <item><see cref="ScreenRightExemptActions"/> — authenticated, but carrying no screen right.
    /// The API grew this second category after M2-A03 was written (M2-A07's <c>/api/v1/me</c> and
    /// M2-B03's <c>/api/v1/reference/*</c>), expressed in production as
    /// <c>[NoScreenRight(justification)]</c>. The attribute already forces a justification at the
    /// declaration site; this list is the second signature, so that no single edit in a
    /// controller can widen the ungated surface.</item>
    /// </list></para>
    ///
    /// <para>Keys are <c>TypeName.MethodName</c> — deliberately not routes. Routes moved once
    /// already (<c>api/auth</c> to <c>api/v1/auth</c>, M2-B01) and this list must not need editing
    /// when they move again. The route is carried alongside for the reader, and is not asserted
    /// on.</para>
    /// </summary>
    internal static class ExemptEndpointAllowList
    {
        /// <summary>
        /// Endpoints reachable with no token at all. <b>One entry, and adding a second is a
        /// security decision that belongs at review, not in a controller diff.</b>
        /// </summary>
        public static IReadOnlyDictionary<string, ExemptEndpoint> AnonymousActions { get; } =
            new Dictionary<string, ExemptEndpoint>(StringComparer.Ordinal)
            {
                ["AuthController.Login"] = new(
                    "POST /api/v1/auth/login",
                    "Login is how a caller obtains a token; requiring one to call it would be circular. " +
                    "It is the only [AllowAnonymous] action in the API (V.SMART/V.SMART.Api/Controllers/AuthController.cs:76-78)."),
            };

        /// <summary>
        /// Endpoints that require authentication but deliberately carry no screen right, via
        /// <c>[NoScreenRight]</c>. Each justification here must agree with the one written at the
        /// declaration site.
        /// </summary>
        public static IReadOnlyDictionary<string, ExemptEndpoint> ScreenRightExemptActions { get; } =
            new Dictionary<string, ExemptEndpoint>(StringComparer.Ordinal)
            {
                ["MeController.Get"] = new(
                    "GET /api/v1/me",
                    "Every authenticated user must be able to read their own identity and rights; gating this on a " +
                    "screen right would deadlock login, because the SPA needs the response to know which screens it " +
                    "may render (M2-A07; V.SMART/V.SMART.Api/Controllers/MeController.cs:37-41)."),

                ["ReferenceController.GetGstRates"] = new(
                    "GET /api/v1/reference/gst-rates",
                    "Reference lookup: no single screen owns it (M2-B03; ReferenceController.cs:33)."),

                ["ReferenceController.GetUoms"] = new(
                    "GET /api/v1/reference/uoms",
                    "Reference lookup: no single screen owns it (M2-B03; ReferenceController.cs:33)."),

                ["ReferenceController.GetStates"] = new(
                    "GET /api/v1/reference/states",
                    "Reference lookup: no single screen owns it (M2-B03; ReferenceController.cs:33)."),

                ["ReferenceController.GetTerms"] = new(
                    "GET /api/v1/reference/terms",
                    "Reference lookup: no single screen owns it (M2-B03; ReferenceController.cs:33)."),

                ["ReferenceController.GetScreens"] = new(
                    "GET /api/v1/reference/screens",
                    "Reference lookup: the screen catalogue itself, needed before any screen can be gated " +
                    "(M2-B03; ReferenceController.cs:33)."),

                ["ReferenceController.GetCurrencies"] = new(
                    "GET /api/v1/reference/currencies",
                    "Reference lookup: no single screen owns it (M2-B03; ReferenceController.cs:33)."),

                ["ReportsController.GetCatalogue"] = new(
                    "GET /api/v1/reports",
                    "Metadata only - lists registered report slugs, display names, parameter shapes and which " +
                    "screen gates each one. Executes no stored procedure and returns no report row, so nothing " +
                    "here can leak report data; each report's own endpoint (e.g. HsnSummaryReportController.Get) " +
                    "still independently enforces its own [RequireScreen] when actually called (M2-B08; " +
                    "ReportsController.cs)."),
            };

        /// <summary>Both lists together — every key that is allowed to be ungated.</summary>
        public static IReadOnlySet<string> AllExemptKeys { get; } =
            new HashSet<string>(
                AnonymousActions.Keys.Concat(ScreenRightExemptActions.Keys),
                StringComparer.Ordinal);
    }

    /// <summary>One allow-list entry: the route a reviewer recognises, and why it is ungated.</summary>
    /// <param name="Route">Indicative only. Never asserted on — routes change, keys do not.</param>
    /// <param name="Justification">Why this endpoint is not gated by a screen right.</param>
    internal sealed record ExemptEndpoint(string Route, string Justification);
}
