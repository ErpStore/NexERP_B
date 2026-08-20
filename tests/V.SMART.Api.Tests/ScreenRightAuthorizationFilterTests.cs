using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using V.SMART.Api.Authorization;
using V.SMART.Api.Tests.Infrastructure;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A01-02 — the truth table of KB-105 §3, T-1 through T-13, exercised against
    /// <see cref="ScreenRightAuthorizationFilter"/> directly. No host and no database: the filter
    /// takes its rights from a stand-in <see cref="IUserRightsProvider"/>, which is the whole
    /// point of that seam.
    /// <para>
    /// These are decision-logic tests only. The end-to-end proof that an annotated controller
    /// denies a real user belongs to M2-A02 and the permission matrix to M2-A03.
    /// </para>
    /// </summary>
    public class ScreenRightAuthorizationFilterTests
    {
        private const string Screen = "Currency";

        // ---- T-1 --------------------------------------------------------------------------

        [Fact]
        public async Task T1_no_UserRight_row_for_the_screen_denies()
        {
            var context = Annotated(Right.View);

            await Filter(Set(Entry("Sales Order", canView: true))).OnAuthorizationAsync(context);

            AssertDenied(context, Screen, "View");
        }

        // ---- T-2 / T-3 --------------------------------------------------------------------

        [Theory]
        [InlineData(Right.View)]
        [InlineData(Right.Create)]
        [InlineData(Right.Edit)]
        [InlineData(Right.Delete)]
        public async Task T2_row_present_with_the_required_flag_false_denies(Right right)
        {
            var context = Annotated(right);

            await Filter(Set(Entry(Screen))).OnAuthorizationAsync(context);

            AssertDenied(context, Screen, right.ToString());
        }

        [Theory]
        [InlineData(Right.View)]
        [InlineData(Right.Create)]
        [InlineData(Right.Edit)]
        [InlineData(Right.Delete)]
        public async Task T3_row_present_with_the_required_flag_true_allows(Right right)
        {
            var context = Annotated(right);

            await Filter(Set(Granting(Screen, right))).OnAuthorizationAsync(context);

            Assert.Null(context.Result);
        }

        [Theory]
        [InlineData(Right.View)]
        [InlineData(Right.Create)]
        [InlineData(Right.Edit)]
        [InlineData(Right.Delete)]
        public async Task Only_the_required_flag_grants_the_operation(Right required)
        {
            foreach (var granted in new[] { Right.View, Right.Create, Right.Edit, Right.Delete })
            {
                var context = Annotated(required);
                await Filter(Set(Granting(Screen, granted))).OnAuthorizationAsync(context);

                if (granted == required)
                {
                    Assert.Null(context.Result);
                }
                else
                {
                    AssertDenied(context, Screen, required.ToString());
                }
            }
        }

        // ---- T-4 --------------------------------------------------------------------------

        [Fact]
        public async Task T4_IsHide_true_with_the_required_flag_true_allows()
        {
            var context = Annotated(Right.Edit);

            await Filter(Set(Entry(Screen, canEdit: true, isHide: true))).OnAuthorizationAsync(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public async Task IsHide_does_not_grant_anything_on_its_own()
        {
            var context = Annotated(Right.Edit);

            await Filter(Set(Entry(Screen, isHide: true))).OnAuthorizationAsync(context);

            AssertDenied(context, Screen, "Edit");
        }

        // ---- T-5 / T-6 --------------------------------------------------------------------

        [Fact]
        public async Task T5_screen_absent_from_the_catalogue_denies_indistinguishably()
        {
            var context = Annotated(Right.View, screen: "No Such Screen");

            await Filter(Set(Granting(Screen, Right.View))).OnAuthorizationAsync(context);

            AssertDenied(context, "No Such Screen", "View");
        }

        [Fact]
        public async Task T6_user_with_zero_rows_is_denied()
        {
            var context = Annotated(Right.View);

            await Filter(ScreenRightSet.Empty).OnAuthorizationAsync(context);

            AssertDenied(context, Screen, "View");
        }

        // ---- T-7 / T-8 — duplicates resolve first-match-wins (KB-105 D-2) -----------------

        [Fact]
        public async Task T7_first_of_two_matching_rows_grants()
        {
            var context = Annotated(Right.Edit);

            await Filter(Set(Entry(Screen, canEdit: true), Entry(Screen)))
                .OnAuthorizationAsync(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public async Task T8_first_of_two_matching_rows_denies()
        {
            var context = Annotated(Right.Edit);

            await Filter(Set(Entry(Screen), Entry(Screen, canEdit: true)))
                .OnAuthorizationAsync(context);

            AssertDenied(context, Screen, "Edit");
        }

        // ---- T-9 — ordinal, case-sensitive (KB-105 D-1) -----------------------------------

        [Fact]
        public async Task T9_a_case_difference_does_not_match()
        {
            var context = Annotated(Right.View, screen: "currency");

            await Filter(Set(Granting("Currency", Right.View))).OnAuthorizationAsync(context);

            AssertDenied(context, "currency", "View");
        }

        // ---- T-10 — unusable identity claims are 401, never 403 (KB-105 D-3) --------------

        [Theory]
        [InlineData(null, "1")]
        [InlineData("", "1")]
        [InlineData("not-a-number", "1")]
        [InlineData("0", "1")]
        [InlineData("-7", "1")]
        public async Task T10_unusable_UserId_claim_is_401(string? userId, string tenantId)
        {
            var context = Annotated(Right.View, userId: userId, tenantId: tenantId);

            await Filter(Set(Granting(Screen, Right.View))).OnAuthorizationAsync(context);

            AssertInvalidToken(context, "UserId");
        }

        [Theory]
        [InlineData("1", null)]
        [InlineData("1", "")]
        [InlineData("1", "not-a-number")]
        [InlineData("1", "0")]
        [InlineData("1", "-7")]
        public async Task T10_unusable_TenantId_claim_is_401(string userId, string? tenantId)
        {
            var context = Annotated(Right.View, userId: userId, tenantId: tenantId);

            await Filter(Set(Granting(Screen, Right.View))).OnAuthorizationAsync(context);

            AssertInvalidToken(context, "TenantId");
        }

        [Fact]
        public async Task An_unusable_claim_never_reaches_the_rights_query()
        {
            var provider = new StubUserRightsProvider(Set(Granting(Screen, Right.View)));
            var context = Annotated(Right.View, userId: null);

            await new ScreenRightAuthorizationFilter(provider, NullLogger<ScreenRightAuthorizationFilter>.Instance)
                .OnAuthorizationAsync(context);

            Assert.Equal(0, provider.Calls);
        }

        // ---- T-11 / T-12 — half-annotated endpoints fail closed (KB-105 D-4) --------------

        [Fact]
        public async Task T11_RequireRight_without_RequireScreen_fails_closed()
        {
            var context = FilterContext(new object[] { new AuthorizeAttribute(), new RequireRightAttribute(Right.Delete) });

            await Filter(Set(Granting(Screen, Right.Delete))).OnAuthorizationAsync(context);

            AssertDenied(context, "(undeclared)", "Delete");
        }

        [Fact]
        public async Task T12_RequireScreen_without_RequireRight_fails_closed()
        {
            var context = FilterContext(new object[] { new AuthorizeAttribute(), new RequireScreenAttribute(Screen) });

            await Filter(Set(Granting(Screen, Right.View))).OnAuthorizationAsync(context);

            AssertDenied(context, Screen, "(undeclared)");
        }

        // ---- T-13 — no Administrator bypass (KB-105 D-5) ----------------------------------

        [Fact]
        public async Task T13_an_Administrator_with_no_row_is_denied()
        {
            var context = Annotated(Right.View);
            var identity = (ClaimsIdentity)context.HttpContext.User.Identity!;
            identity.AddClaim(new Claim(ClaimTypes.Role, "Administrator"));
            identity.AddClaim(new Claim("role", "Administrator"));

            await Filter(ScreenRightSet.Empty).OnAuthorizationAsync(context);

            AssertDenied(context, Screen, "View");
        }

        // ---- Pass-through cases -----------------------------------------------------------

        [Fact]
        public async Task An_anonymous_endpoint_is_not_decided()
        {
            var context = FilterContext(new object[]
            {
                new AllowAnonymousAttribute(),
                new RequireScreenAttribute(Screen),
                new RequireRightAttribute(Right.View)
            });

            await Filter(ScreenRightSet.Empty).OnAuthorizationAsync(context);

            Assert.Null(context.Result);
        }

        [Fact]
        public async Task A_NoScreenRight_endpoint_is_reachable_with_zero_rights()
        {
            var context = FilterContext(new object[]
            {
                new AuthorizeAttribute(),
                new NoScreenRightAttribute("GET /api/v1/me must be reachable by every authenticated user (KB-105 §2.4).")
            });

            await Filter(ScreenRightSet.Empty).OnAuthorizationAsync(context);

            Assert.Null(context.Result);
        }

        /// <summary>
        /// The six endpoints the API has today declare neither attribute, and this task does not
        /// annotate them. The filter must stay dormant for them, or M2-A01-02 would change the
        /// behaviour of every existing endpoint instead of none.
        /// </summary>
        [Fact]
        public async Task An_unannotated_endpoint_is_not_decided_and_is_not_queried()
        {
            var provider = new StubUserRightsProvider(ScreenRightSet.Empty);
            var context = FilterContext(new object[] { new AuthorizeAttribute() });

            await new ScreenRightAuthorizationFilter(provider, NullLogger<ScreenRightAuthorizationFilter>.Instance)
                .OnAuthorizationAsync(context);

            Assert.Null(context.Result);
            Assert.Equal(0, provider.Calls);
        }

        // ---- The provider seam carries the tenant (KB-105 §8.1) ---------------------------

        [Fact]
        public async Task The_provider_receives_both_the_tenant_and_the_user()
        {
            var provider = new StubUserRightsProvider(Set(Granting(Screen, Right.View)));
            var context = Annotated(Right.View, userId: "7", tenantId: "42");

            await new ScreenRightAuthorizationFilter(provider, NullLogger<ScreenRightAuthorizationFilter>.Instance)
                .OnAuthorizationAsync(context);

            Assert.Equal(42, provider.LastTenantId);
            Assert.Equal(7, provider.LastUserId);
        }

        /// <summary>A database fault must never read as "no rights" (KB-105 §7.3).</summary>
        [Fact]
        public async Task A_provider_failure_propagates_rather_than_denying()
        {
            var context = Annotated(Right.View);
            var filter = new ScreenRightAuthorizationFilter(
                new ThrowingUserRightsProvider(),
                NullLogger<ScreenRightAuthorizationFilter>.Instance);

            await Assert.ThrowsAsync<InvalidOperationException>(() => filter.OnAuthorizationAsync(context));
            Assert.Null(context.Result);
        }

        // ---- Helpers ----------------------------------------------------------------------

        private static ScreenRightAuthorizationFilter Filter(ScreenRightSet rights)
            => new(new StubUserRightsProvider(rights), NullLogger<ScreenRightAuthorizationFilter>.Instance);

        private static ScreenRightSet Set(params ScreenRightEntry[] entries) => new(entries);

        private static ScreenRightEntry Entry(
            string screen,
            bool canView = false,
            bool canCreate = false,
            bool canEdit = false,
            bool canDelete = false,
            bool isHide = false)
            => new(screen, canView, canCreate, canEdit, canDelete, isHide);

        private static ScreenRightEntry Granting(string screen, Right right) => right switch
        {
            Right.View => Entry(screen, canView: true),
            Right.Create => Entry(screen, canCreate: true),
            Right.Edit => Entry(screen, canEdit: true),
            _ => Entry(screen, canDelete: true)
        };

        private static AuthorizationFilterContext Annotated(
            Right right,
            string screen = Screen,
            string? userId = "1",
            string? tenantId = "1")
            => FilterContext(
                new object[] { new AuthorizeAttribute(), new RequireScreenAttribute(screen), new RequireRightAttribute(right) },
                userId,
                tenantId);

        private static AuthorizationFilterContext FilterContext(
            IList<object> metadata,
            string? userId = "1",
            string? tenantId = "1")
        {
            var http = ErrorContractTestContext.Create("/api/currencies/7", "PUT");

            var claims = new List<Claim> { new(ClaimTypes.Name, "tester") };
            if (userId is not null)
            {
                claims.Add(new Claim("UserId", userId));
            }

            if (tenantId is not null)
            {
                claims.Add(new Claim("TenantId", tenantId));
            }

            http.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

            var descriptor = new ControllerActionDescriptor
            {
                ControllerName = "Currency",
                ActionName = "Update",
                EndpointMetadata = metadata
            };

            return new AuthorizationFilterContext(
                new ActionContext(http, new RouteData(), descriptor),
                new List<IFilterMetadata>());
        }

        private static void AssertDenied(AuthorizationFilterContext context, string screen, string right)
        {
            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
            Assert.Contains("application/problem+json", result.ContentTypes);

            var problem = Assert.IsType<ProblemDetails>(result.Value);
            Assert.Equal("https://api.v-smart.local/problems/screen-right-denied", problem.Type);
            Assert.Equal("Screen right denied.", problem.Title);
            Assert.Equal($"You do not have the '{right}' right for the '{screen}' screen.", problem.Detail);
            Assert.Equal(screen, problem.Extensions["screen"]);
            Assert.Equal(right, problem.Extensions["right"]);
        }

        private static void AssertInvalidToken(AuthorizationFilterContext context, string claimName)
        {
            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(401, result.StatusCode);

            var problem = Assert.IsType<ProblemDetails>(result.Value);
            Assert.Equal("https://api.v-smart.local/problems/invalid-token", problem.Type);
            Assert.Equal("The access token is missing a required claim.", problem.Title);
            Assert.Equal($"The token does not carry a usable '{claimName}' claim.", problem.Detail);
            Assert.False(problem.Extensions.ContainsKey("screen"));
            Assert.False(problem.Extensions.ContainsKey("right"));
        }

        private sealed class StubUserRightsProvider : IUserRightsProvider
        {
            private readonly ScreenRightSet _rights;

            public StubUserRightsProvider(ScreenRightSet rights) => _rights = rights;

            public int Calls { get; private set; }

            public int LastTenantId { get; private set; }

            public int LastUserId { get; private set; }

            public Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct)
            {
                Calls++;
                LastTenantId = tenantId;
                LastUserId = userId;
                return Task.FromResult(_rights);
            }

            /// <summary>
            /// Present because <c>IUserRightsProvider</c> declares it (M2-A01-03). The filter never
            /// evicts — it only reads — so a no-op here is the faithful stand-in, and
            /// <see cref="InvalidateCalls"/> is asserted nowhere except to prove that.
            /// </summary>
            public void Invalidate(int tenantId, int userId) => InvalidateCalls++;

            public int InvalidateCalls { get; private set; }
        }

        private sealed class ThrowingUserRightsProvider : IUserRightsProvider
        {
            public Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct)
                => throw new InvalidOperationException("rights query failed");

            public void Invalidate(int tenantId, int userId)
                => throw new InvalidOperationException("Invalidate must not be called by the filter.");
        }
    }
}
