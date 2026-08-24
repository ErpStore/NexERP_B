using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using V.SMART.Api.Authorization;
using Xunit;

namespace V.SMART.Api.Tests.PermissionMatrix
{
    /// <summary>
    /// M2-A03 — <b>the matrix</b>. Every gated <c>(action, right)</c> pair the API declares is
    /// driven through all six fixtures, generated from the assembly rather than written out. The
    /// case count grows by six with every endpoint added, with no edit here.
    ///
    /// <para>Positive rows assert "<b>not</b> 403", never "200": the harness proves authorization
    /// and must not become coupled to each endpoint's success contract. Endpoint behaviour is
    /// asserted by that endpoint's own tests — <c>CurrencyAuthorizationTests</c>,
    /// <c>FileEndpointSecurityTests</c>, <c>MeEndpointTests</c>, <c>ReferenceControllerTests</c>.</para>
    /// </summary>
    public class PermissionMatrixTests
    {
        /// <summary>
        /// The generated matrix: one case per <c>(gated endpoint × fixture)</c>. Data is
        /// <c>(string key, RightsFixture)</c> rather than the endpoint object so that xUnit can
        /// serialise each case and name it in the runner output.
        /// </summary>
        public static TheoryData<string, RightsFixture> Matrix()
        {
            var data = new TheoryData<string, RightsFixture>();

            foreach (var endpoint in ApiEndpointDiscovery.Guarded)
            {
                foreach (var fixture in PermissionMatrixHarness.Fixtures)
                {
                    data.Add(endpoint.Key, fixture);
                }
            }

            return data;
        }

        public static TheoryData<string> GatedEndpoints()
        {
            var data = new TheoryData<string>();
            foreach (var endpoint in ApiEndpointDiscovery.Guarded)
            {
                data.Add(endpoint.Key);
            }

            return data;
        }

        // =====================================================================================
        // The matrix
        // =====================================================================================

        /// <summary>
        /// One case of the matrix. Which of the three outcomes is expected comes from the fixture
        /// alone, so a new endpoint needs no expectation written for it.
        /// </summary>
        [Theory]
        [MemberData(nameof(Matrix))]
        public void Every_gated_endpoint_answers_the_whole_matrix(string key, RightsFixture fixture)
        {
            var endpoint = PermissionMatrixHarness.Endpoint(key);
            var result = PermissionMatrixHarness.Run(endpoint, fixture);

            if (fixture == RightsFixture.NoToken)
            {
                AssertUnauthenticated(result);
                return;
            }

            if (PermissionMatrixHarness.ExpectsForbidden(fixture))
            {
                AssertForbidden(result);
            }
            else
            {
                Assert.True(
                    result.Context.Result is null,
                    $"{result.Describe()} was refused although the required right is granted. " +
                    "The filter produced: " + Describe(result.Context.Result));
            }

            // Isolation, observed rather than assumed: the rights came from this case's own
            // repository call, and the zero-TTL bypass wrote nothing that a later case could read.
            Assert.True(
                result.RightsQueries == 1,
                $"{result.Describe()} resolved rights {result.RightsQueries} time(s); exactly one repository " +
                "call is expected. Zero means a cached value leaked into this case.");

            Assert.False(
                result.CacheWasWritten,
                $"{result.Describe()} wrote a cache entry despite the zero-TTL bypass; the matrix would then " +
                "depend on execution order.");
        }

        /// <summary>
        /// <c>IsHide</c>, named separately because it is the regression ADR-004 §1 calls out:
        /// hiding a screen in navigation must not revoke an operation the user holds
        /// (<c>RightsHelper.cs:19-20</c> is read only by <c>BaseUserRightsComponent.cs:27</c>).
        /// </summary>
        [Theory]
        [MemberData(nameof(GatedEndpoints))]
        public void IsHide_with_the_required_right_granted_is_never_403(string key)
        {
            var endpoint = PermissionMatrixHarness.Endpoint(key);
            var result = PermissionMatrixHarness.Run(endpoint, RightsFixture.RequiredRightAndIsHide);

            Assert.True(
                result.Context.Result is null,
                $"{result.Describe()} was refused with IsHide = true although the required right is granted. " +
                "IsHide is a navigation affordance, never an operation gate (ADR-004 §1, KB-105 B-5/T-4).");
        }

        /// <summary>
        /// 401 and 403 stay distinguishable, and each is asserted on its own. The filter-level
        /// half is the <see cref="RightsFixture.NoToken"/> row; this is the declaration-level
        /// half — an anonymous request never reaches the action at all, because the controller
        /// requires authentication and the action does not opt out. Stated as a declaration
        /// assertion because there is no test host to make the request (R-43, KB-060).
        /// </summary>
        [Theory]
        [MemberData(nameof(GatedEndpoints))]
        public void Every_gated_endpoint_requires_authentication_before_any_right_is_considered(string key)
        {
            var endpoint = PermissionMatrixHarness.Endpoint(key);

            Assert.True(
                endpoint.EndpointMetadata.OfType<IAuthorizeData>().Any(),
                $"{endpoint.Describe()} carries no [Authorize]; an anonymous caller would reach the screen-right " +
                "filter instead of being refused 401 by authentication.");

            Assert.False(
                endpoint.IsAnonymous,
                $"{endpoint.Describe()} is [AllowAnonymous] and gated at the same time.");
        }

        // =====================================================================================
        // Assertions — every message names controller, action, screen and right
        // =====================================================================================

        /// <summary>
        /// The 403 <b>body</b>, not merely the status code: media type, problem type, title,
        /// detail, and the <c>screen</c>/<c>right</c> extensions (KB-105 §7.1;
        /// <c>Middleware/ApiProblems.cs</c> is the only place that builds it).
        /// </summary>
        private static void AssertForbidden(CaseResult result)
        {
            var objectResult = Assert.IsType<ObjectResult>(result.Context.Result);

            Assert.True(
                objectResult.StatusCode == StatusCodes.Status403Forbidden,
                $"{result.Describe()} answered {objectResult.StatusCode}, expected 403.");

            Assert.Contains("application/problem+json", objectResult.ContentTypes);

            var problem = Assert.IsType<ProblemDetails>(objectResult.Value);

            Assert.Equal("https://api.v-smart.local/problems/screen-right-denied", problem.Type);
            Assert.Equal("Screen right denied.", problem.Title);
            Assert.Equal(StatusCodes.Status403Forbidden, problem.Status);

            var screen = result.Endpoint.ScreenName;
            var right = result.Endpoint.RightName;

            Assert.Equal($"You do not have the '{right}' right for the '{screen}' screen.", problem.Detail);
            Assert.Equal(screen, problem.Extensions["screen"]);
            Assert.Equal(right, problem.Extensions["right"]);
        }

        /// <summary>
        /// 401, and it must name neither the screen nor the right: a caller who cannot be
        /// identified learns nothing about what they were asking for (KB-105 D-3, §7.2).
        /// </summary>
        private static void AssertUnauthenticated(CaseResult result)
        {
            var objectResult = Assert.IsType<ObjectResult>(result.Context.Result);

            Assert.True(
                objectResult.StatusCode == StatusCodes.Status401Unauthorized,
                $"{result.Describe()} answered {objectResult.StatusCode}, expected 401 for a caller with no " +
                "usable token. 403 here would mean the API cannot distinguish 'who are you' from 'you may not'.");

            var problem = Assert.IsType<ProblemDetails>(objectResult.Value);
            Assert.Equal("https://api.v-smart.local/problems/invalid-token", problem.Type);

            Assert.False(problem.Extensions.ContainsKey("screen"), $"{result.Describe()} leaked 'screen' in a 401.");
            Assert.False(problem.Extensions.ContainsKey("right"), $"{result.Describe()} leaked 'right' in a 401.");

            // Rights were granted in full for this fixture, and were never consulted: the refusal
            // is about the credential, not about the permission.
            Assert.True(
                result.RightsQueries == 0,
                $"{result.Describe()} queried rights for an unidentifiable caller.");
        }

        private static string Describe(object? actionResult) => actionResult switch
        {
            null => "no result (allowed)",
            ObjectResult o => $"{o.StatusCode} {(o.Value as ProblemDetails)?.Detail}",
            _ => actionResult.GetType().Name
        };

        /// <summary>
        /// A guard on the matrix itself: <see cref="Right"/> has four members, and the fixtures
        /// "only the required flag" and "every flag except the required one" are only exhaustive
        /// while that is true. A fifth member added without revisiting the fixtures would silently
        /// stop testing itself.
        /// </summary>
        [Fact]
        public void The_matrix_covers_every_member_of_the_Right_enum_and_every_fixture()
        {
            Assert.Equal(
                new[] { Right.View, Right.Create, Right.Edit, Right.Delete },
                Enum.GetValues<Right>());

            Assert.Equal(6, PermissionMatrixHarness.Fixtures.Count);

            // Null-guarded rather than dereferenced: an endpoint missing its [RequireRight] must
            // fail EndpointDiscoveryTests with a named message, not this test with a
            // NullReferenceException that says nothing about which endpoint is at fault.
            var declaredRights = ApiEndpointDiscovery.Guarded
                .Where(e => e.Right is not null)
                .Select(e => e.Right!.Right)
                .Distinct()
                .ToArray();

            Assert.NotEmpty(declaredRights);
        }

        /// <summary>
        /// Reports the generated case count, so the number is in the test output rather than in
        /// somebody's memory: <c>gated endpoints × 6</c>.
        /// </summary>
        [Fact]
        public void The_generated_case_count_is_six_per_gated_endpoint()
        {
            Assert.Equal(ApiEndpointDiscovery.Guarded.Count * 6, Matrix().Count);
        }

        /// <summary>
        /// The harness reads attributes the way MVC composes metadata — controller first, action
        /// second, nearest declaration wins. A helper that read <see cref="MethodInfo"/> alone
        /// would find no screen anywhere and pass every endpoint vacuously, so the composition is
        /// asserted rather than trusted.
        /// </summary>
        [Fact]
        public void Screens_are_read_from_controller_level_metadata_not_from_the_method_alone()
        {
            foreach (var endpoint in ApiEndpointDiscovery.Guarded)
            {
                Assert.NotNull(endpoint.Screen);
                Assert.Null(endpoint.Method.GetCustomAttribute<RequireScreenAttribute>());

                Assert.Same(
                    endpoint.Screen,
                    endpoint.EndpointMetadata.OfType<RequireScreenAttribute>().LastOrDefault());
            }
        }
    }
}
