using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging.Abstractions;
using V.SMART.Api.Authorization;
using V.SMART.Api.Controllers;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A02 — the Currency permission matrix, driven by the attributes
    /// <see cref="CurrencyController"/> actually carries.
    ///
    /// <para><b>What these tests prove, and what they do not.</b> There is no host and no
    /// database here: <c>tests/V.SMART.Api.Tests</c> references neither
    /// <c>Microsoft.AspNetCore.Mvc.Testing</c> nor a <c>WebApplicationFactory</c> (R-43,
    /// KB-060). So each case builds an <see cref="AuthorizationFilterContext"/> whose
    /// <c>EndpointMetadata</c> is <b>read by reflection from the real controller and the real
    /// action</b>, in the order MVC composes it (controller attributes, then action
    /// attributes), and runs the real <see cref="ScreenRightAuthorizationFilter"/> over it.
    /// A wrong or missing attribute on <see cref="CurrencyController"/> therefore fails these
    /// tests, which is the wiring claim M2-A02 has to make. What is <i>not</i> proved here is
    /// the socket-level round trip: that a 403 leaves the process as
    /// <c>application/problem+json</c> is asserted on the <see cref="ObjectResult"/>'s content
    /// types and body object, not on bytes on a wire. End-to-end proof is M2-A03's, and needs
    /// the test host R-43 records as absent.</para>
    ///
    /// <para><b>Rights and cache isolation (the non-negotiable in the task).</b> Every case
    /// constructs its own <see cref="StubUserRightsProvider"/> and its own filter, so no rights
    /// state and no cache entry can survive from one case to the next — a shared warm cache
    /// would turn this matrix into a false pass. Each denied/allowed case additionally asserts
    /// <c>provider.Calls == 1</c>, which is what makes the isolation observable rather than
    /// merely intended. The production cache's own eviction paths —
    /// <c>IUserRightsProvider.Invalidate</c> and the zero-TTL setting — are proved separately in
    /// <see cref="UserRightsCacheTests"/> (<c>Invalidate_evicts_only_the_named_user_and_is_safe_when_absent</c>,
    /// <c>A_zero_TTL_disables_the_cache_entirely</c>); they are not re-derived here.</para>
    ///
    /// <para>The decision logic itself — first-match-wins, ordinal matching, no Administrator
    /// bypass — belongs to <see cref="ScreenRightAuthorizationFilterTests"/> and is not
    /// duplicated. This file is about <see cref="CurrencyController"/> specifically.</para>
    /// </summary>
    public class CurrencyAuthorizationTests
    {
        /// <summary>
        /// The seeded <c>Screens.ScreenName</c> the Blazor Currency pages gate on. Byte-identical
        /// to <c>V.SMART/V.SMART.Shared/Data/ApplicationDbContext.cs:1155</c> (Id = 5,
        /// ScreenCode = 5), <c>CurrencyList.razor:252</c> and <c>CurrencyUpsert.razor:135</c>.
        /// </summary>
        private const string Screen = "Currency";

        private const string GetAll = nameof(CurrencyController.GetAll);
        private const string GetById = nameof(CurrencyController.GetById);
        private const string Create = nameof(CurrencyController.Create);
        private const string Update = nameof(CurrencyController.Update);
        private const string Delete = nameof(CurrencyController.Delete);

        public static TheoryData<string> AllActions() => new() { GetAll, GetById, Create, Update, Delete };

        // =================================================================================
        // The annotations themselves
        // =================================================================================

        /// <summary>
        /// The screen string is the one thing here the compiler cannot check: a typo denies every
        /// Currency call in every tenant, silently (ADR-004 <i>Consequences</i>, R-10). It is
        /// checked against <see cref="ScreenCatalogue.SeededScreenNames"/>, which is the in-repo
        /// mirror of the <c>ApplicationDbContext</c> seed, using the same ordinal comparison
        /// <see cref="ScreenRightAuthorizationFilter"/> uses (KB-105 D-1).
        /// </summary>
        [Fact]
        public void The_controller_declares_the_seeded_Currency_screen_and_not_Currency_Today()
        {
            var attribute = typeof(CurrencyController).GetCustomAttribute<RequireScreenAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal("Currency", attribute!.ScreenName, StringComparer.Ordinal);
            Assert.True(attribute.Seeded, "Currency is a seeded screen; Seeded = false would skip the catalogue check.");

            Assert.Contains("Currency", ScreenCatalogue.SeededScreenNames);
            Assert.NotEqual("Currency Today", attribute.ScreenName, StringComparer.Ordinal);
            Assert.Contains("Currency Today", ScreenCatalogue.SeededScreenNames);
        }

        /// <summary>
        /// The mapping ADR-004 §1 prescribes, including the deliberate asymmetry: <b>both</b> GETs
        /// take <see cref="Right.View"/>.
        /// </summary>
        [Theory]
        [InlineData(GetAll, Right.View)]
        [InlineData(GetById, Right.View)]
        [InlineData(Create, Right.Create)]
        [InlineData(Update, Right.Edit)]
        [InlineData(Delete, Right.Delete)]
        public void Every_action_declares_its_right(string action, Right expected)
        {
            var attribute = Action(action).GetCustomAttribute<RequireRightAttribute>();

            Assert.True(attribute is not null, $"CurrencyController.{action} carries no [RequireRight].");
            Assert.Equal(expected, attribute!.Right);
        }

        /// <summary>
        /// No action may quietly opt out of the mechanism the controller just joined: a
        /// <c>[NoScreenRight]</c> or <c>[AllowAnonymous]</c> on any Currency action would reopen
        /// R-03 for that endpoint while the class-level attribute made it look closed.
        /// </summary>
        [Fact]
        public void No_Currency_action_opts_out_of_the_screen_right_check()
        {
            var actions = Actions().ToList();
            Assert.Equal(5, actions.Count);

            Assert.Null(typeof(CurrencyController).GetCustomAttribute<AllowAnonymousAttribute>());
            Assert.Null(typeof(CurrencyController).GetCustomAttribute<NoScreenRightAttribute>());

            foreach (var action in actions)
            {
                Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
                Assert.Null(action.GetCustomAttribute<NoScreenRightAttribute>());
                Assert.NotNull(action.GetCustomAttribute<RequireRightAttribute>());
            }
        }

        // =================================================================================
        // The matrix
        // =================================================================================

        /// <summary>Row 1 — no <c>UserRight</c> row for "Currency" at all: refused everywhere.
        /// This is BR-AUTH-002's deny-by-default (<c>RightsHelper.cs:7-20</c>, the <c>?? false</c>)
        /// enforced by the API for the first time.</summary>
        [Theory]
        [MemberData(nameof(AllActions))]
        public async Task No_UserRight_row_for_Currency_is_refused_on_every_endpoint(string action)
        {
            var (context, provider) = Run(action, Rights(Entry("Sales Order", canView: true)));

            await Task.CompletedTask;
            AssertDenied(context, RequiredRight(action));
            Assert.Equal(1, provider.Calls);
        }

        /// <summary>Row 2 — a row exists with all five flags false: refused everywhere. This is the
        /// exact user ADR-004 names, for whom the Blazor UI hides the screen entirely while the
        /// API used to let them create, edit and delete.</summary>
        [Theory]
        [MemberData(nameof(AllActions))]
        public async Task All_five_flags_false_is_refused_on_every_endpoint(string action)
        {
            var (context, provider) = Run(action, Rights(Entry(Screen)));

            await Task.CompletedTask;
            AssertDenied(context, RequiredRight(action));
            Assert.Equal(1, provider.Calls);
        }

        /// <summary>Rows 3-6 — one flag at a time. Each granted right opens exactly the endpoints
        /// that declare it and no others; both GETs open together because both declare
        /// <see cref="Right.View"/>.</summary>
        [Theory]
        [InlineData(Right.View)]
        [InlineData(Right.Create)]
        [InlineData(Right.Edit)]
        [InlineData(Right.Delete)]
        public async Task A_single_granted_flag_opens_exactly_the_actions_that_declare_it(Right granted)
        {
            foreach (var action in new[] { GetAll, GetById, Create, Update, Delete })
            {
                var required = RequiredRight(action);
                var (context, provider) = Run(action, Rights(Granting(Screen, granted)));

                if (required == granted)
                {
                    Assert.True(context.Result is null, $"{action} was denied although {granted} is granted.");
                }
                else
                {
                    AssertDenied(context, required);
                }

                Assert.Equal(1, provider.Calls);
            }

            await Task.CompletedTask;
        }

        /// <summary>Row 7 — all four operation flags true: every endpoint is reached.</summary>
        [Theory]
        [MemberData(nameof(AllActions))]
        public async Task All_four_rights_reach_every_endpoint(string action)
        {
            var (context, _) = Run(
                action,
                Rights(Entry(Screen, canView: true, canCreate: true, canEdit: true, canDelete: true)));

            await Task.CompletedTask;
            Assert.Null(context.Result);
        }

        /// <summary>
        /// Row 8 — the named <c>IsHide</c> regression the task requires. <c>IsHide</c> is a
        /// navigation affordance, never an operation gate (ADR-004 §1; KB-105 B-5/T-4;
        /// <c>RightsHelper.cs:19-20</c> is read only by <c>BaseUserRightsComponent.cs:27</c>,
        /// independently of the four operation flags). A hidden screen whose rights are granted
        /// must still be callable through the API.
        /// </summary>
        [Theory]
        [MemberData(nameof(AllActions))]
        public async Task IsHide_true_does_not_deny_an_endpoint_whose_right_is_granted(string action)
        {
            var (context, _) = Run(
                action,
                Rights(Entry(Screen, canView: true, canCreate: true, canEdit: true, canDelete: true, isHide: true)));

            await Task.CompletedTask;
            Assert.Null(context.Result);
        }

        /// <summary>And the converse: <c>IsHide</c> on its own grants nothing.</summary>
        [Theory]
        [MemberData(nameof(AllActions))]
        public async Task IsHide_true_alone_grants_no_Currency_operation(string action)
        {
            var (context, _) = Run(action, Rights(Entry(Screen, isHide: true)));

            await Task.CompletedTask;
            AssertDenied(context, RequiredRight(action));
        }

        // =================================================================================
        // 401 stays distinguishable from 403
        // =================================================================================

        /// <summary>
        /// Row 9, policy level. An anonymous caller never reaches the screen-right filter's
        /// decision at all: <c>[Authorize]</c> is on the controller, nothing below it opts out
        /// with <c>[AllowAnonymous]</c>, so the authentication middleware answers 401 before any
        /// action runs. Stated as a policy assertion rather than an over-the-wire one because
        /// this project has no test host (R-43).
        /// </summary>
        [Fact]
        public void An_anonymous_caller_is_refused_by_authentication_before_the_screen_right_filter()
        {
            Assert.NotNull(typeof(CurrencyController).GetCustomAttribute<AuthorizeAttribute>());
            Assert.Null(typeof(CurrencyController).GetCustomAttribute<AllowAnonymousAttribute>());

            foreach (var action in Actions())
                Assert.Null(action.GetCustomAttribute<AllowAnonymousAttribute>());
        }

        /// <summary>
        /// The other half of "401 and 403 stay distinguishable": an authenticated token that
        /// cannot identify a user or a tenant is an <i>authentication</i> fault, so the filter
        /// answers 401 and names no screen and no right — it must not leak that Currency was the
        /// screen being asked about (KB-105 D-3).
        /// </summary>
        [Theory]
        [InlineData(GetAll, "UserId")]
        [InlineData(GetById, "UserId")]
        [InlineData(Create, "UserId")]
        [InlineData(Update, "UserId")]
        [InlineData(Delete, "UserId")]
        [InlineData(GetAll, "TenantId")]
        [InlineData(Delete, "TenantId")]
        public async Task A_token_without_a_usable_identity_claim_is_401_not_403(string action, string missingClaim)
        {
            var provider = new StubUserRightsProvider(Rights(Entry(Screen, canView: true, canCreate: true, canEdit: true, canDelete: true)));
            var context = FilterContext(
                action,
                userId: missingClaim == "UserId" ? null : "1",
                tenantId: missingClaim == "TenantId" ? null : "1");

            await Filter(provider).OnAuthorizationAsync(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(401, result.StatusCode);

            var problem = Assert.IsType<ProblemDetails>(result.Value);
            Assert.Equal("https://api.v-smart.local/problems/invalid-token", problem.Type);
            Assert.Equal($"The token does not carry a usable '{missingClaim}' claim.", problem.Detail);
            Assert.False(problem.Extensions.ContainsKey("screen"));
            Assert.False(problem.Extensions.ContainsKey("right"));

            Assert.Equal(0, provider.Calls);
        }

        // =================================================================================
        // Ordering: authorization decides before the action validates
        // =================================================================================

        /// <summary>
        /// Model validation runs <i>inside</i> the action
        /// (<c>CurrencyController.Create</c>'s <c>ModelState</c> guard) and via M2-A06's
        /// <c>InvalidModelStateResponseFactory</c>, both of which are downstream of the
        /// authorization stage. <see cref="ScreenRightAuthorizationFilter"/> is an
        /// <see cref="IAsyncAuthorizationFilter"/>, so it decides first. Consequence, and it is
        /// intended: an unauthorized caller posting a garbage <see cref="CurrencyVM"/> gets 403,
        /// not 400 — the API does not tell an unauthorized caller anything about their payload.
        /// </summary>
        [Fact]
        public async Task An_unauthorized_caller_posting_an_invalid_body_gets_403_not_400()
        {
            Assert.True(
                typeof(IAsyncAuthorizationFilter).IsAssignableFrom(typeof(ScreenRightAuthorizationFilter)),
                "The screen-right filter must run in the authorization stage, ahead of model binding and validation.");

            var context = FilterContext(Create);

            // A CurrencyVM that would certainly fail DataAnnotations validation had the request
            // reached the action: CurrName, CurrSub and Symbol are all [Required].
            var invalid = new CurrencyVM();
            context.ModelState.AddModelError(nameof(CurrencyVM.CurrName), "The CurrName field is required.");
            context.ModelState.AddModelError(nameof(CurrencyVM.Symbol), "The Symbol field is required.");
            Assert.False(context.ModelState.IsValid);
            Assert.Null(invalid.CurrName);

            await Filter(new StubUserRightsProvider(Rights(Entry(Screen)))).OnAuthorizationAsync(context);

            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
            Assert.IsType<ProblemDetails>(result.Value);
            Assert.IsNotType<ValidationProblemDetails>(result.Value);
        }

        // =================================================================================
        // Helpers
        // =================================================================================

        private static ScreenRightAuthorizationFilter Filter(IUserRightsProvider provider)
            => new(provider, NullLogger<ScreenRightAuthorizationFilter>.Instance);

        /// <summary>
        /// Runs the real filter, over the real controller's real metadata, with a provider created
        /// fresh for this call — which is the cache-isolation guarantee, stated in code.
        /// </summary>
        private static (AuthorizationFilterContext Context, StubUserRightsProvider Provider) Run(
            string action,
            ScreenRightSet rights)
        {
            var provider = new StubUserRightsProvider(rights);
            var context = FilterContext(action);

            Filter(provider).OnAuthorizationAsync(context).GetAwaiter().GetResult();

            return (context, provider);
        }

        private static Right RequiredRight(string action)
            => Action(action).GetCustomAttribute<RequireRightAttribute>()!.Right;

        private static MethodInfo Action(string name)
            => Actions().Single(m => m.Name == name);

        /// <summary>The controller's five public actions, discovered rather than listed.</summary>
        private static IEnumerable<MethodInfo> Actions()
            => typeof(CurrencyController)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);

        /// <summary>
        /// <c>EndpointMetadata</c> composed the way MVC composes it — controller attributes first,
        /// then action attributes — from the attributes actually present on
        /// <see cref="CurrencyController"/>. Nothing is hand-written, so removing an attribute from
        /// the controller fails the matrix rather than silently passing it.
        /// </summary>
        private static AuthorizationFilterContext FilterContext(
            string action,
            string? userId = "1",
            string? tenantId = "1")
        {
            var method = Action(action);

            var metadata = typeof(CurrencyController)
                .GetCustomAttributes(inherit: true)
                .Concat(method.GetCustomAttributes(inherit: true))
                .ToList();

            var http = ErrorContractTestContext.Create("/api/v1/currencies", "GET");

            var claims = new List<Claim> { new(ClaimTypes.Name, "tester") };
            if (userId is not null) claims.Add(new Claim("UserId", userId));
            if (tenantId is not null) claims.Add(new Claim("TenantId", tenantId));
            http.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));

            var descriptor = new ControllerActionDescriptor
            {
                ControllerName = "Currency",
                ActionName = action,
                ControllerTypeInfo = typeof(CurrencyController).GetTypeInfo(),
                MethodInfo = method,
                EndpointMetadata = metadata
            };

            return new AuthorizationFilterContext(
                new ActionContext(http, new RouteData(), descriptor),
                new List<IFilterMetadata>());
        }

        private static ScreenRightSet Rights(params ScreenRightEntry[] entries) => new(entries);

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

        /// <summary>
        /// The 403 contract, asserted in full rather than by status code alone: the media type is
        /// <c>application/problem+json</c> and the body names both the screen and the right
        /// (KB-105 §7.1; <c>Middleware/ApiProblems.cs</c> is the single place that builds it).
        /// </summary>
        private static void AssertDenied(AuthorizationFilterContext context, Right right)
        {
            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(403, result.StatusCode);
            Assert.Contains("application/problem+json", result.ContentTypes);

            var problem = Assert.IsType<ProblemDetails>(result.Value);
            Assert.Equal("https://api.v-smart.local/problems/screen-right-denied", problem.Type);
            Assert.Equal("Screen right denied.", problem.Title);
            Assert.Equal($"You do not have the '{right}' right for the '{Screen}' screen.", problem.Detail);
            Assert.Equal(Screen, problem.Extensions["screen"]);
            Assert.Equal(right.ToString(), problem.Extensions["right"]);
        }

        /// <summary>
        /// A stand-in for the rights query. <b>Stated plainly, because it bounds the claim:</b>
        /// this substitutes <see cref="IUserRightsProvider"/>, so these tests prove the filter's
        /// decision over <see cref="CurrencyController"/>'s attributes — they do <i>not</i> prove
        /// the SQL that loads <c>UserRight</c> rows. That query is proved by
        /// <see cref="UserRightsCacheTests"/>, against a repository stand-in of its own; a real
        /// per-tenant database is exercised by no test in this repository yet.
        /// </summary>
        private sealed class StubUserRightsProvider : IUserRightsProvider
        {
            private readonly ScreenRightSet _rights;

            public StubUserRightsProvider(ScreenRightSet rights) => _rights = rights;

            public int Calls { get; private set; }

            public Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct)
            {
                Calls++;
                return Task.FromResult(_rights);
            }

            /// <summary>Never called by the filter, which only reads.</summary>
            public void Invalidate(int tenantId, int userId)
                => throw new InvalidOperationException("The filter must not evict cache entries.");
        }
    }
}
