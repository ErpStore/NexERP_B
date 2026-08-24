using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using Xunit;

namespace V.SMART.Api.Tests.PermissionMatrix
{
    /// <summary>
    /// M2-A03 — <b>the harness proving it can fail</b>. A gate that has only ever been seen to
    /// pass is not known to work, and the task requires the two deliberate breakages be
    /// demonstrated: an action with its <c>[RequireRight]</c> removed, and a misspelled screen
    /// name.
    ///
    /// <para>They are demonstrated here <b>permanently and automatically</b>, against stand-in
    /// controllers declared in this file, rather than by temporarily editing a real controller and
    /// writing the outcome down in prose. Two reasons. First, M2-A03 forbids the final diff from
    /// containing any controller change, and a manual edit-and-revert leaves the proof in a commit
    /// message that no future run re-checks. Second, and more important: this way the proof runs
    /// on every push. If someone weakens
    /// <see cref="AnnotationAudit"/> next year so it can no longer detect a missing right, this
    /// file goes red — a prose record would not.</para>
    ///
    /// <para>The stand-ins live in the test assembly, so they are invisible to
    /// <see cref="ApiEndpointDiscovery.All"/>, which sweeps <c>V.SMART.Api</c> only. They are fed
    /// through the same <see cref="ApiEndpointDiscovery.DiscoverFrom"/> and the same
    /// <see cref="AnnotationAudit.Problems"/> the real assembly goes through — not a copy of the
    /// rules.</para>
    /// </summary>
    public class HarnessSelfTests
    {
        // =====================================================================================
        // The control: a correctly annotated controller must pass
        // =====================================================================================

        /// <summary>
        /// Without this, every test below could pass because the audit rejects everything.
        /// </summary>
        [Fact]
        public void A_correctly_annotated_controller_produces_no_problems()
        {
            Assert.Empty(Audit<WellFormedController>());
        }

        // =====================================================================================
        // Deliberate breakage 1 — [RequireRight] removed
        // =====================================================================================

        /// <summary>
        /// M2-A03 implementation step 12(a). The action below is identical to
        /// <see cref="WellFormedController.Get"/> except that its <c>[RequireRight]</c> is absent.
        /// </summary>
        [Fact]
        public void Removing_RequireRight_from_an_action_fails_the_harness()
        {
            var problem = Assert.Single(Audit<MissingRightController>());

            Assert.Contains("MissingRightController.Get", problem, StringComparison.Ordinal);
            Assert.Contains("0 [RequireRight]", problem, StringComparison.Ordinal);
            Assert.Contains("exactly one is required", problem, StringComparison.Ordinal);
        }

        /// <summary>
        /// And the production startup validator refuses to start the host for the same condition,
        /// so the two guards agree. If they ever diverge, one of them has been weakened.
        /// </summary>
        [Fact]
        public void Removing_RequireRight_also_refuses_to_start_the_host()
        {
            var error = Assert.Throws<InvalidOperationException>(() =>
                ScreenRightStartupValidator.Validate(
                    EndpointDiscoveryTests.ServicesFor(
                        ApiEndpointDiscovery.DiscoverFrom(new[] { typeof(MissingRightController) }))));

            Assert.Contains("MissingRightController.Get", error.Message, StringComparison.Ordinal);
        }

        // =====================================================================================
        // Deliberate breakage 2 — a misspelled screen name
        // =====================================================================================

        /// <summary>
        /// M2-A03 implementation step 12(b): <c>"Currencyy"</c>. This is the failure mode that
        /// cannot be caught by a compiler and does not error at runtime — it silently denies every
        /// call, in every tenant, forever.
        /// </summary>
        [Fact]
        public void A_misspelled_screen_name_fails_the_harness()
        {
            var problem = Assert.Single(Audit<MisspelledScreenController>());

            Assert.Contains("MisspelledScreenController.Get", problem, StringComparison.Ordinal);
            Assert.Contains("Currencyy", problem, StringComparison.Ordinal);
            Assert.Contains("not one of the seeded", problem, StringComparison.Ordinal);

            // The correctly spelled name is seeded, so the check is a set-membership test and not
            // an accident of the string being unusual.
            Assert.Contains("Currency", ScreenCatalogue.SeededScreenNames);
            Assert.DoesNotContain("Currencyy", ScreenCatalogue.SeededScreenNames);
        }

        /// <summary>
        /// A <i>real but wrong</i> name is the harder case: <c>"Currency Today"</c> is seeded, so
        /// no catalogue check can reject it. Recorded here as a known limit of this gate rather
        /// than left as an unstated assumption — only the endpoint's own tests can catch it.
        /// </summary>
        [Fact]
        public void A_real_but_wrong_screen_name_is_not_caught_and_that_limit_is_deliberate()
        {
            Assert.Empty(Audit<WrongButSeededScreenController>());
            Assert.Contains("Currency Today", ScreenCatalogue.SeededScreenNames);
        }

        /// <summary>
        /// The misspelling also refuses to start the host — the production check (KB-105 D-6) this
        /// harness mirrors.
        /// </summary>
        [Fact]
        public void A_misspelled_screen_name_also_refuses_to_start_the_host()
        {
            var error = Assert.Throws<InvalidOperationException>(() =>
                ScreenRightStartupValidator.Validate(
                    EndpointDiscoveryTests.ServicesFor(
                        ApiEndpointDiscovery.DiscoverFrom(new[] { typeof(MisspelledScreenController) }))));

            Assert.Contains("Currencyy", error.Message, StringComparison.Ordinal);
        }

        // =====================================================================================
        // Breakage 3 — the fail-open hole: a controller with no annotation at all
        // =====================================================================================

        /// <summary>
        /// The condition R-03 is actually about: a new controller that carries no screen-right
        /// annotation at all is served with <b>no permission check</b>. The harness fails on it,
        /// which is what makes "adding a controller requires no harness edit" a gate rather than a
        /// convenience.
        /// </summary>
        [Fact]
        public void An_entirely_unannotated_controller_fails_the_harness()
        {
            var problem = Assert.Single(Audit<UnannotatedController>());

            Assert.Contains("UnannotatedController.Get", problem, StringComparison.Ordinal);
            Assert.Contains("declares no [RequireScreen]", problem, StringComparison.Ordinal);
            Assert.Contains("R-03", problem, StringComparison.Ordinal);
        }

        /// <summary>
        /// <b>And the production code still lets it through</b> — asserted, not glossed over.
        /// <c>ScreenRightStartupValidator.cs:83-88</c> continues past an unannotated action and
        /// <c>ScreenRightAuthorizationFilter.cs:69-72</c> passes the request through, so the host
        /// starts and the endpoint serves unchecked. This harness is therefore the only thing
        /// standing between an unannotated controller and production today. Switching that
        /// direction on is open question <b>Q-71</b>, owned by the repository owner; M2-A03's
        /// scope explicitly forbids editing <c>V.SMART.Api/Authorization/**</c>, so this test
        /// records the gap in executable form and will fail — deliberately, telling the next
        /// session to delete it — on the day Q-71 is resolved.
        /// </summary>
        [Fact]
        public void The_production_validator_still_allows_an_unannotated_controller_which_is_Q_71()
        {
            var error = Record.Exception(() =>
                ScreenRightStartupValidator.Validate(
                    EndpointDiscoveryTests.ServicesFor(
                        ApiEndpointDiscovery.DiscoverFrom(new[] { typeof(UnannotatedController) }))));

            Assert.True(
                error is null,
                "The production startup validator now rejects an entirely unannotated controller. That is the " +
                "Q-71 direction being switched on — good news. Delete this test and tighten " +
                "AnnotationAudit's comment accordingly.");
        }

        // =====================================================================================
        // Breakage 4 — exemption by omission
        // =====================================================================================

        /// <summary>
        /// <c>[AllowAnonymous]</c> added without an allow-list entry: the exact "exempt by
        /// omission" the task forbids.
        /// </summary>
        [Fact]
        public void An_anonymous_endpoint_that_is_not_on_the_allow_list_fails_the_harness()
        {
            var problem = Assert.Single(Audit<UnlistedAnonymousController>());

            Assert.Contains("UnlistedAnonymousController.Get", problem, StringComparison.Ordinal);
            Assert.Contains("anonymous allow-list", problem, StringComparison.Ordinal);
        }

        /// <summary>The same for the authenticated-but-ungated exemption.</summary>
        [Fact]
        public void A_NoScreenRight_endpoint_that_is_not_on_the_allow_list_fails_the_harness()
        {
            var problem = Assert.Single(Audit<UnlistedExemptController>());

            Assert.Contains("UnlistedExemptController.Get", problem, StringComparison.Ordinal);
            Assert.Contains("screen-right-exempt", problem, StringComparison.Ordinal);
        }

        // =====================================================================================
        // Failure messages
        // =====================================================================================

        /// <summary>
        /// M2-A03: "a bare assertion failure across hundreds of cases is unusable". Every problem
        /// string names the controller, the action, the screen and the right.
        /// </summary>
        [Fact]
        public void Every_failure_message_names_controller_action_screen_and_right()
        {
            var problems = AnnotationAudit.Problems(ApiEndpointDiscovery.DiscoverFrom(new[]
            {
                typeof(MissingRightController),
                typeof(MisspelledScreenController),
                typeof(UnannotatedController)
            }));

            Assert.Equal(3, problems.Count);

            foreach (var problem in problems)
            {
                Assert.Contains("Controller.", problem, StringComparison.Ordinal);   // controller + action
                Assert.Contains("screen='", problem, StringComparison.Ordinal);
                Assert.Contains("right='", problem, StringComparison.Ordinal);
            }
        }

        private static IReadOnlyList<string> Audit<TController>()
            => AnnotationAudit.Problems(ApiEndpointDiscovery.DiscoverFrom(new[] { typeof(TController) }));

        // =====================================================================================
        // Stand-ins. Never routed: they live in the test assembly, which no host loads.
        // =====================================================================================

        [ApiController]
        [Route("api/v1/self-test")]
        [Authorize]
        [RequireScreen("Currency")]
        private sealed class WellFormedController : ControllerBase
        {
            [HttpGet]
            [RequireRight(Right.View)]
            public IActionResult Get() => Ok();
        }

        [ApiController]
        [Route("api/v1/self-test")]
        [Authorize]
        [RequireScreen("Currency")]
        private sealed class MissingRightController : ControllerBase
        {
            [HttpGet]
            public IActionResult Get() => Ok();
        }

        [ApiController]
        [Route("api/v1/self-test")]
        [Authorize]
        [RequireScreen("Currencyy")]
        private sealed class MisspelledScreenController : ControllerBase
        {
            [HttpGet]
            [RequireRight(Right.View)]
            public IActionResult Get() => Ok();
        }

        [ApiController]
        [Route("api/v1/self-test")]
        [Authorize]
        [RequireScreen("Currency Today")]
        private sealed class WrongButSeededScreenController : ControllerBase
        {
            [HttpGet]
            [RequireRight(Right.View)]
            public IActionResult Get() => Ok();
        }

        [ApiController]
        [Route("api/v1/self-test")]
        [Authorize]
        private sealed class UnannotatedController : ControllerBase
        {
            [HttpGet]
            public IActionResult Get() => Ok();
        }

        [ApiController]
        [Route("api/v1/self-test")]
        private sealed class UnlistedAnonymousController : ControllerBase
        {
            [HttpGet]
            [AllowAnonymous]
            public IActionResult Get() => Ok();
        }

        [ApiController]
        [Route("api/v1/self-test")]
        [Authorize]
        [NoScreenRight("A stand-in for the self-tests; never routed.")]
        private sealed class UnlistedExemptController : ControllerBase
        {
            [HttpGet]
            public IActionResult Get() => Ok();
        }
    }
}
