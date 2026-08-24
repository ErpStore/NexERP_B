using V.SMART.Api.Authorization;

namespace V.SMART.Api.Tests.PermissionMatrix
{
    /// <summary>
    /// M2-A03 — the annotation-completeness rules, in one place so that the real API assembly and
    /// the deliberately-broken stand-ins in <see cref="HarnessSelfTests"/> are judged by exactly
    /// the same code. A rule that is only ever run against passing input is not known to work.
    ///
    /// <para><b>Relationship to the production validator.</b>
    /// <see cref="ScreenRightStartupValidator"/> already refuses to start the host for two of
    /// these conditions and is asserted directly by
    /// <see cref="EndpointDiscoveryTests.The_sweep_agrees_with_the_production_startup_validator"/>.
    /// The rules here are deliberately <b>stricter</b> in one respect, which is the reason this
    /// class exists at all: the production validator still <i>passes through</i> an action whose
    /// controller declares no <c>[RequireScreen]</c> at all
    /// (<c>ScreenRightStartupValidator.cs:83-88</c>, matching
    /// <c>ScreenRightAuthorizationFilter.cs:69-72</c>) — the remaining fail-open half of R-03,
    /// recorded as open question Q-71. Until that direction is switched on in production, this
    /// harness is what makes an unannotated controller a build failure, and the allow-list is
    /// what makes the exception explicit.</para>
    /// </summary>
    internal static class AnnotationAudit
    {
        /// <summary>
        /// Every problem found, one string per offending action, each naming controller, action,
        /// screen and right. Empty means the surface is fully annotated.
        /// </summary>
        public static IReadOnlyList<string> Problems(IEnumerable<ApiEndpoint> endpoints)
        {
            var problems = new List<string>();

            foreach (var endpoint in endpoints)
            {
                if (endpoint.IsAnonymous)
                {
                    if (!ExemptEndpointAllowList.AnonymousActions.ContainsKey(endpoint.Key))
                    {
                        problems.Add(
                            $"{endpoint.Describe()} is [AllowAnonymous] but is not on the anonymous allow-list. " +
                            "An endpoint must never become exempt by omission: add it to " +
                            "ExemptEndpointAllowList.AnonymousActions with a written justification, or remove " +
                            "[AllowAnonymous].");
                    }

                    continue;
                }

                if (endpoint.IsScreenRightExempt)
                {
                    if (!ExemptEndpointAllowList.ScreenRightExemptActions.ContainsKey(endpoint.Key))
                    {
                        problems.Add(
                            $"{endpoint.Describe()} carries [NoScreenRight] but is not on the screen-right-exempt " +
                            "allow-list. Add it to ExemptEndpointAllowList.ScreenRightExemptActions with a written " +
                            "justification, or gate it with [RequireScreen] + [RequireRight].");
                    }
                    else if (string.IsNullOrWhiteSpace(endpoint.ScreenRightExemption!.Justification))
                    {
                        problems.Add(
                            $"{endpoint.Describe()} carries [NoScreenRight] with a blank justification.");
                    }

                    continue;
                }

                // --- from here on the endpoint must be fully gated ------------------------------

                if (endpoint.Screen is null)
                {
                    problems.Add(
                        $"{endpoint.Describe()} is on a controller that declares no [RequireScreen], and the action " +
                        "is neither [AllowAnonymous] nor [NoScreenRight]. It would be served with NO permission " +
                        "check at all (R-03). Add [RequireScreen(\"<seeded screen name>\")] to the controller, or " +
                        "declare the exemption explicitly.");

                    // One problem per action: a controller with no screen will also have no
                    // right, and reporting both would bury the headline in noise.
                    continue;
                }

                if (endpoint.RightAttributeCount != 1)
                {
                    problems.Add(
                        $"{endpoint.Describe()} declares {endpoint.RightAttributeCount} [RequireRight] attributes; " +
                        "exactly one is required (ADR-004; RequireRightAttribute is AllowMultiple = false).");
                }

                if (endpoint.Screen!.Seeded &&
                    !ScreenCatalogue.SeededScreenNames.Contains(endpoint.Screen.ScreenName))
                {
                    problems.Add(
                        $"{endpoint.Describe()} declares [RequireScreen(\"{endpoint.Screen.ScreenName}\")], which is " +
                        "not one of the seeded Screens.ScreenName values (ordinal comparison, ScreenCatalogue / " +
                        "KB-105 Appendix A). A screen name that does not exist DENIES every call silently instead " +
                        "of erroring. Fix the string, or set Seeded = false with written justification at review.");
                }
            }

            return problems;
        }

        /// <summary>A single message listing every problem — what a failing assertion prints.</summary>
        public static string Report(IReadOnlyList<string> problems)
            => string.Join(Environment.NewLine, problems.Select(p => "  - " + p));
    }
}
