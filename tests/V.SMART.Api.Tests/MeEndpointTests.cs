using System.Reflection;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using V.SMART.Api.Authorization;
using V.SMART.Api.Controllers;
using V.SMART.Api.Tests.Infrastructure;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A07 — <c>GET /api/v1/me</c>.
    /// <para>
    /// Controller-level, like every other suite in this project: no host and no database (see
    /// <c>Infrastructure/ErrorContractTestContext.cs</c>). The controller takes its rights from a
    /// stand-in <see cref="IUserRightsProvider"/>, which is the whole point of that seam, and its
    /// identity from a hand-built <see cref="ClaimsPrincipal"/>.
    /// </para>
    /// <para>
    /// <b>What this level cannot prove, stated plainly:</b> the <c>401</c> a request with no token
    /// receives is produced by the JwtBearer challenge, above MVC, so it is asserted here as the
    /// declaration that causes it (<c>[Authorize]</c> present, <c>[AllowAnonymous]</c> absent)
    /// rather than as an observed status code. An over-the-wire assertion needs
    /// <c>Microsoft.AspNetCore.Mvc.Testing</c>, which this project does not reference.
    /// </para>
    /// </summary>
    public class MeEndpointTests
    {
        // ---- Identity ---------------------------------------------------------------------

        [Fact]
        public async Task Returns_the_callers_own_identity_tenant_and_role()
        {
            var response = await Invoke(
                ScreenRightSet.Empty,
                userId: "7",
                tenantId: "42",
                userName: "vivek",
                role: "Administrator");

            Assert.Equal(7, response.UserId);
            Assert.Equal("vivek", response.UserName);
            Assert.Equal(42, response.TenantId);
            Assert.Equal("Administrator", response.Role);
        }

        /// <summary>
        /// The role is the <c>ClaimTypes.Role</c> claim <c>JwtTokenService.cs:34</c> mints, and it
        /// survives the short-name form the JWT handler puts on the wire.
        /// <c>CurrentUserService.GetUserRoleAsync()</c> — which reads <c>"role"</c> only and so
        /// always returns <c>""</c> (R-18) — is not called; it is not even referenced by the
        /// controller, as <see cref="The_controller_does_not_reference_CurrentUserService"/> asserts.
        /// </summary>
        [Theory]
        [InlineData("Administrator")]
        [InlineData("User")]
        public async Task Role_comes_from_the_ClaimTypes_Role_claim(string role)
        {
            var response = await Invoke(ScreenRightSet.Empty, role: role);

            Assert.Equal(role, response.Role);
        }

        [Fact]
        public async Task Role_is_read_when_the_inbound_claim_map_left_it_short_named()
        {
            var response = await Invoke(ScreenRightSet.Empty, role: null, shortRole: "User");

            Assert.Equal("User", response.Role);
        }

        /// <summary>
        /// <c>User.Role</c> is nullable despite its <c>[Required]</c> attribute, and
        /// <c>JwtTokenService</c> collapses a null role to <c>""</c>. The response agrees with the
        /// login response rather than inventing a null.
        /// </summary>
        [Fact]
        public async Task Role_is_empty_never_null_when_the_token_carries_none()
        {
            var response = await Invoke(ScreenRightSet.Empty, role: null);

            Assert.Equal(string.Empty, response.Role);
        }

        /// <summary>
        /// R-31: <c>NavMenu.razor:36,148</c> and <c>Home.razor:240</c> name a role
        /// <c>ERPAdmin</c> that <c>UserRole</c> does not contain. Nothing in the API model
        /// mentions it, and this test is the tripwire that keeps it that way.
        /// </summary>
        [Fact]
        public void ERPAdmin_is_not_propagated_into_the_API_model()
        {
            var names = typeof(MeController)
                .GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Select(m => m.Name)
                .Concat(typeof(MeController.MeResponse).GetProperties().Select(p => p.Name))
                .Concat(typeof(MeController.ScreenRight).GetProperties().Select(p => p.Name))
                .ToList();

            Assert.DoesNotContain(names, n => n.Contains("ERPAdmin", StringComparison.OrdinalIgnoreCase));

            // The response carries the role as an opaque string lifted from the token, so the API
            // model neither defines nor invents a role name at all — which is the strongest form
            // of "not propagated".
            Assert.Equal(typeof(string), typeof(MeController.MeResponse).GetProperty("Role")!.PropertyType);
        }

        // ---- The rights map ---------------------------------------------------------------

        /// <summary>
        /// Exactly the caller's rows, no more and no fewer, keyed by <c>Screens.ScreenName</c>
        /// verbatim so a client lookup mirrors <c>RightsHelper.cs:8</c>.
        /// </summary>
        [Fact]
        public async Task The_map_contains_exactly_the_callers_rows_keyed_by_screen_name()
        {
            var rights = Set(
                new ScreenRightEntry("Currency", true, false, true, false, false),
                new ScreenRightEntry("Sales Order", true, true, true, true, true));

            var response = await Invoke(rights);

            Assert.Equal(2, response.Rights.Count);
            Assert.Equal(new[] { "Currency", "Sales Order" }, response.Rights.Keys.OrderBy(k => k, StringComparer.Ordinal));

            var currency = response.Rights["Currency"];
            Assert.True(currency.View);
            Assert.False(currency.Create);
            Assert.True(currency.Edit);
            Assert.False(currency.Delete);
            Assert.False(currency.Hidden);
        }

        /// <summary>
        /// <c>IsHide</c> is present as <c>hidden</c>. It is the reason this endpoint exists for
        /// M2-C03: navigation is filtered on <c>view &amp;&amp; !hidden</c>.
        /// </summary>
        [Fact]
        public async Task IsHide_is_carried_as_hidden()
        {
            var response = await Invoke(Set(new ScreenRightEntry("Currency", true, false, false, false, true)));

            Assert.True(response.Rights["Currency"].Hidden);
            Assert.True(response.Rights["Currency"].View);
        }

        /// <summary>BR-AUTH-002 — absent rows are omitted, and the client's default is deny.</summary>
        [Fact]
        public async Task Screens_the_caller_holds_no_row_for_are_absent_not_materialised_as_false()
        {
            var response = await Invoke(Set(new ScreenRightEntry("Currency", true, false, false, false, false)));

            Assert.Single(response.Rights);
            Assert.False(response.Rights.ContainsKey("Sales Order"));
        }

        /// <summary>A user with no rights at all gets an empty map — not an error, not a 500.</summary>
        [Fact]
        public async Task A_caller_with_no_rights_rows_receives_an_empty_map_not_an_error()
        {
            var response = await Invoke(ScreenRightSet.Empty);

            Assert.Empty(response.Rights);
        }

        /// <summary>
        /// Q-27 duplicates: the map must keep the <b>first</b> entry, because
        /// <see cref="ScreenRightSet.Has"/> — and therefore the filter — decides on the first.
        /// If the map kept the last, the rendered and the enforced answer would differ on exactly
        /// the rows Q-27 is about.
        /// </summary>
        [Fact]
        public async Task Duplicate_rows_for_one_screen_collapse_first_match_wins()
        {
            var rights = Set(
                new ScreenRightEntry("Currency", true, false, false, false, false),
                new ScreenRightEntry("Currency", false, true, true, true, true));

            var response = await Invoke(rights);

            Assert.Single(response.Rights);
            Assert.True(response.Rights["Currency"].View);
            Assert.False(response.Rights["Currency"].Create);
            Assert.Equal(rights.Has("Currency", Right.View), response.Rights["Currency"].View);
            Assert.Equal(rights.Has("Currency", Right.Create), response.Rights["Currency"].Create);
        }

        /// <summary>Screen names are matched ordinally, exactly as <c>string ==</c> matches them.</summary>
        [Fact]
        public async Task Screen_name_keys_are_ordinal_and_case_sensitive_like_RightsHelper()
        {
            var response = await Invoke(Set(new ScreenRightEntry("Currency", true, false, false, false, false)));

            Assert.True(response.Rights.ContainsKey("Currency"));
            Assert.False(response.Rights.ContainsKey("currency"));
        }

        // ---- The map and the filter share one source ---------------------------------------

        /// <summary>
        /// The consistency proof, at the level this test project can reach: one
        /// <see cref="ScreenRightSet"/> — the single value <see cref="IUserRightsProvider"/>
        /// returns — is fed to <see cref="MeController"/> and to
        /// <see cref="ScreenRightAuthorizationFilter"/>, and where the map says
        /// <c>create = false</c> the filter answers <c>403</c> for the same caller on the same
        /// screen.
        /// <para>
        /// <b>This is not the end-to-end assertion the task asks for.</b> The task names
        /// <c>POST /api/currencies</c> specifically, and <c>CurrencyController</c> carries
        /// <c>[Authorize]</c> but no <c>[RequireScreen]</c>/<c>[RequireRight]</c> — annotating it
        /// is M2-A02's work — so the filter passes it through undecided today
        /// (<c>ScreenRightAuthorizationFilter.cs:69-72</c>). Until M2-A02 lands, no configuration
        /// of this repository can make that endpoint return 403 for a rights reason, and a test
        /// claiming otherwise would be a fiction. Re-point this test at the real endpoint when
        /// M2-A02 annotates it.
        /// </para>
        /// </summary>
        [Fact]
        public async Task Where_the_map_says_create_is_false_the_filter_denies_the_same_caller()
        {
            var rights = Set(new ScreenRightEntry("Currency", true, false, false, false, false));

            var response = await Invoke(rights);
            Assert.False(response.Rights["Currency"].Create);

            var filterContext = FilterContext(
                new object[]
                {
                    new AuthorizeAttribute(),
                    new RequireScreenAttribute("Currency"),
                    new RequireRightAttribute(Right.Create)
                });

            await new ScreenRightAuthorizationFilter(
                    new StubProvider(rights),
                    NullLogger<ScreenRightAuthorizationFilter>.Instance)
                .OnAuthorizationAsync(filterContext);

            var result = Assert.IsType<ObjectResult>(filterContext.Result);
            Assert.Equal(403, result.StatusCode);
        }

        /// <summary>
        /// And the other direction, so the test above is not passing for the trivial reason that
        /// the filter denies everything.
        /// </summary>
        [Fact]
        public async Task Where_the_map_says_create_is_true_the_filter_allows_the_same_caller()
        {
            var rights = Set(new ScreenRightEntry("Currency", true, true, false, false, false));

            var response = await Invoke(rights);
            Assert.True(response.Rights["Currency"].Create);

            var filterContext = FilterContext(
                new object[]
                {
                    new AuthorizeAttribute(),
                    new RequireScreenAttribute("Currency"),
                    new RequireRightAttribute(Right.Create)
                });

            await new ScreenRightAuthorizationFilter(
                    new StubProvider(rights),
                    NullLogger<ScreenRightAuthorizationFilter>.Instance)
                .OnAuthorizationAsync(filterContext);

            Assert.Null(filterContext.Result);
        }

        // ---- The seam, the tenant and the absent parameter ----------------------------------

        /// <summary>
        /// Rights are read through <see cref="IUserRightsProvider"/> — the same seam and the same
        /// cache the filter uses — and the provider is handed <b>both</b> identities from the
        /// caller's own token (BR-TEN-002).
        /// </summary>
        [Fact]
        public async Task Rights_are_read_through_the_provider_with_the_tenant_from_the_token()
        {
            var provider = new StubProvider(ScreenRightSet.Empty);
            var controller = Controller(provider, userId: "7", tenantId: "42");

            await controller.Get(CancellationToken.None);

            Assert.Equal(1, provider.Calls);
            Assert.Equal(42, provider.LastTenantId);
            Assert.Equal(7, provider.LastUserId);
        }

        /// <summary>
        /// M2-A07 tenant isolation: the same <c>UserId</c> in two tenants resolves to two different
        /// provider keys and two different maps. The endpoint contributes no cache key of its own —
        /// it passes the token's tenant straight through, which is why the M2-A01-03 key holds here.
        /// </summary>
        [Fact]
        public async Task Two_tenants_with_the_same_UserId_receive_their_own_tenants_rights()
        {
            var provider = new PerTenantProvider(
                tenantOne: Set(new ScreenRightEntry("Currency", true, false, false, false, false)),
                tenantTwo: Set(new ScreenRightEntry("Sales Order", true, true, false, false, false)));

            var one = await Read(Controller(provider, userId: "7", tenantId: "1"));
            var two = await Read(Controller(provider, userId: "7", tenantId: "2"));

            Assert.Equal(new[] { "Currency" }, one.Rights.Keys);
            Assert.Equal(new[] { "Sales Order" }, two.Rights.Keys);
        }

        /// <summary>
        /// A caller cannot ask for another user's rights because no parameter exists that would let
        /// them: the action takes a <see cref="CancellationToken"/> and nothing else, and it is
        /// bound entirely from the token.
        /// </summary>
        [Fact]
        public void The_action_exposes_no_parameter_that_could_name_another_user()
        {
            var parameters = typeof(MeController)
                .GetMethod(nameof(MeController.Get))!
                .GetParameters();

            Assert.Single(parameters);
            Assert.Equal(typeof(CancellationToken), parameters[0].ParameterType);
        }

        // ---- Failure is loud ----------------------------------------------------------------

        /// <summary>
        /// A rights-load fault propagates to the M2-A06 handler as a <c>500</c>. It must never
        /// degrade to an empty map: an empty map reads to a user as a permission problem rather
        /// than an outage.
        /// </summary>
        [Fact]
        public async Task A_rights_load_failure_propagates_and_never_returns_an_empty_map()
        {
            var controller = Controller(new ThrowingProvider());

            await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Get(CancellationToken.None));
        }

        /// <summary>
        /// A token with no usable subject is a malformed credential — 401, never 403 and never a
        /// map for user 0 (KB-105 D-3, and the <c>&lt;= 0</c> guard that makes the divergence from
        /// <c>CurrentUserService</c>'s silent 0 total).
        /// </summary>
        [Theory]
        [InlineData(null, "1", "UserId")]
        [InlineData("0", "1", "UserId")]
        [InlineData("not-a-number", "1", "UserId")]
        [InlineData("1", null, "TenantId")]
        [InlineData("1", "-3", "TenantId")]
        public async Task An_unusable_subject_claim_is_401_and_the_provider_is_never_called(
            string? userId, string? tenantId, string expectedClaim)
        {
            var provider = new StubProvider(ScreenRightSet.Empty);
            var controller = Controller(provider, userId: userId, tenantId: tenantId);

            var action = await controller.Get(CancellationToken.None);

            var result = Assert.IsType<ObjectResult>(action.Result);
            Assert.Equal(401, result.StatusCode);

            var problem = Assert.IsType<ProblemDetails>(result.Value);
            Assert.Equal("https://api.v-smart.local/problems/invalid-token", problem.Type);
            Assert.Equal($"The token does not carry a usable '{expectedClaim}' claim.", problem.Detail);
            Assert.Equal(0, provider.Calls);
        }

        // ---- Declaration: authenticated, no screen right, no secrets -------------------------

        /// <summary>
        /// The endpoint requires authentication. The <c>401</c> itself is issued by the JwtBearer
        /// challenge above MVC, so what is asserted here is the declaration that causes it.
        /// </summary>
        [Fact]
        public void The_endpoint_requires_authentication_and_is_not_anonymous()
        {
            Assert.NotEmpty(typeof(MeController).GetCustomAttributes<AuthorizeAttribute>(inherit: true));
            Assert.Empty(typeof(MeController).GetCustomAttributes<AllowAnonymousAttribute>(inherit: true));
            Assert.Empty(typeof(MeController).GetMethod(nameof(MeController.Get))!
                .GetCustomAttributes<AllowAnonymousAttribute>(inherit: true));
        }

        /// <summary>
        /// The exempt allow-list entry. The mechanism M2-A01-02 shipped for this is
        /// <see cref="NoScreenRightAttribute"/> — declared, mandatory-justification and greppable,
        /// never exempt by omission. M2-A03's permission-matrix harness has not landed
        /// (Blocked, KB-081); when it does, its allow-list is populated from exactly this
        /// declaration rather than from a second hand-maintained list.
        /// </summary>
        [Fact]
        public void The_endpoint_is_exempt_from_the_screen_right_check_explicitly_and_with_a_reason()
        {
            var exemption = Assert.Single(typeof(MeController).GetCustomAttributes<NoScreenRightAttribute>(inherit: true));

            Assert.False(string.IsNullOrWhiteSpace(exemption.Justification));
            Assert.Contains("/api/v1/me", exemption.Justification, StringComparison.Ordinal);

            Assert.Empty(typeof(MeController).GetCustomAttributes<RequireScreenAttribute>(inherit: true));
            Assert.Empty(typeof(MeController).GetCustomAttributes<RequireRightAttribute>(inherit: true));
        }

        /// <summary>
        /// And the exemption is honoured by the filter itself, with zero rights: gating this
        /// endpoint on a screen right would deadlock the SPA.
        /// </summary>
        [Fact]
        public async Task The_filter_lets_the_endpoint_through_with_zero_rights()
        {
            var provider = new StubProvider(ScreenRightSet.Empty);
            var context = FilterContext(typeof(MeController)
                .GetCustomAttributes(inherit: true)
                .Concat(typeof(MeController).GetMethod(nameof(MeController.Get))!.GetCustomAttributes(inherit: true))
                .ToArray());

            await new ScreenRightAuthorizationFilter(provider, NullLogger<ScreenRightAuthorizationFilter>.Instance)
                .OnAuthorizationAsync(context);

            Assert.Null(context.Result);
            Assert.Equal(0, provider.Calls);
        }

        /// <summary>
        /// And the startup sweep accepts it, so adding this controller cannot stop the host from
        /// booting. Run against the controller's <b>real</b> attributes, not a hand-built stand-in.
        /// </summary>
        [Fact]
        public void The_startup_validator_accepts_the_endpoint_as_declared()
        {
            var descriptor = new ControllerActionDescriptor
            {
                ControllerName = "Me",
                ActionName = nameof(MeController.Get),
                EndpointMetadata = typeof(MeController)
                    .GetCustomAttributes(inherit: true)
                    .Concat(typeof(MeController).GetMethod(nameof(MeController.Get))!.GetCustomAttributes(inherit: true))
                    .ToArray()
            };

            var services = new ServiceCollection()
                .AddSingleton<IActionDescriptorCollectionProvider>(new SingleActionProvider(descriptor))
                .BuildServiceProvider();

            ScreenRightStartupValidator.Validate(services);
        }

        /// <summary>The route decision, pinned so M2-B01 cannot move the URL by accident.</summary>
        [Fact]
        public void The_route_is_api_v1_me()
        {
            var route = Assert.Single(typeof(MeController).GetCustomAttributes<RouteAttribute>(inherit: true));

            Assert.Equal("api/v1/me", route.Template);
        }

        /// <summary>
        /// R-01: no connection string, password, password hash, QR token or bearer token may appear
        /// anywhere in the serialised response. The response model carries five members and a
        /// dictionary of five booleans, so this is asserted on the wire form, not on the CLR type.
        /// </summary>
        [Fact]
        public async Task No_secret_appears_anywhere_in_the_serialised_response()
        {
            var response = await Invoke(
                Set(new ScreenRightEntry("Currency", true, true, true, true, false)),
                userName: "vivek",
                role: "Administrator");

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));

            foreach (var forbidden in new[]
                     {
                         "connectionstring", "password", "server=", "data source", "user id=",
                         "qrtoken", "token", "secret", "hash"
                     })
            {
                Assert.DoesNotContain(forbidden, json, StringComparison.OrdinalIgnoreCase);
            }

            Assert.Equal(
                new[] { "userId", "userName", "tenantId", "role", "rights" },
                JsonDocument.Parse(json).RootElement.EnumerateObject().Select(p => p.Name));
        }

        /// <summary>
        /// The wire names the SPA's <c>ScreenRight</c> interface expects (KB-050 § Permission-based
        /// rendering): <c>view</c>, <c>create</c>, <c>edit</c>, <c>delete</c>, <c>hidden</c>.
        /// </summary>
        [Fact]
        public async Task The_wire_shape_matches_the_client_contract()
        {
            var response = await Invoke(Set(new ScreenRightEntry("Currency", true, false, false, false, true)));

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            var entry = JsonDocument.Parse(json).RootElement.GetProperty("rights").GetProperty("Currency");

            Assert.Equal(
                new[] { "view", "create", "edit", "delete", "hidden" },
                entry.EnumerateObject().Select(p => p.Name));
            Assert.True(entry.GetProperty("view").GetBoolean());
            Assert.True(entry.GetProperty("hidden").GetBoolean());
        }

        /// <summary>R-18 tripwire: the buggy reader is not reachable from this controller.</summary>
        [Fact]
        public void The_controller_does_not_reference_CurrentUserService()
        {
            var constructorParameters = typeof(MeController)
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType.Name);

            Assert.Equal(new[] { nameof(IUserRightsProvider) }, constructorParameters);
        }

        // ---- Helpers -------------------------------------------------------------------------

        private static ScreenRightSet Set(params ScreenRightEntry[] entries) => new(entries);

        private static async Task<MeController.MeResponse> Invoke(
            ScreenRightSet rights,
            string? userId = "1",
            string? tenantId = "1",
            string userName = "tester",
            string? role = "User",
            string? shortRole = null)
            => await Read(Controller(new StubProvider(rights), userId, tenantId, userName, role, shortRole));

        private static async Task<MeController.MeResponse> Read(MeController controller)
        {
            var action = await controller.Get(CancellationToken.None);

            var ok = Assert.IsType<OkObjectResult>(action.Result);
            return Assert.IsType<MeController.MeResponse>(ok.Value);
        }

        private static MeController Controller(
            IUserRightsProvider provider,
            string? userId = "1",
            string? tenantId = "1",
            string userName = "tester",
            string? role = "User",
            string? shortRole = null)
        {
            var http = ErrorContractTestContext.Create("/api/v1/me", "GET");
            http.User = Principal(userId, tenantId, userName, role, shortRole);

            return new MeController(provider)
            {
                ControllerContext = new ControllerContext(new ActionContext(http, new RouteData(), new ControllerActionDescriptor()))
            };
        }

        private static ClaimsPrincipal Principal(
            string? userId, string? tenantId, string userName, string? role, string? shortRole)
        {
            var claims = new List<Claim> { new(ClaimTypes.Name, userName) };

            if (userId is not null)
            {
                claims.Add(new Claim("UserId", userId));
            }

            if (tenantId is not null)
            {
                claims.Add(new Claim("TenantId", tenantId));
            }

            if (role is not null)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            if (shortRole is not null)
            {
                claims.Add(new Claim("role", shortRole));
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"));
        }

        private static AuthorizationFilterContext FilterContext(IList<object> metadata)
        {
            var http = ErrorContractTestContext.Create("/api/v1/me", "GET");
            http.User = Principal("1", "1", "tester", "User", null);

            var descriptor = new ControllerActionDescriptor
            {
                ControllerName = "Me",
                ActionName = "Get",
                EndpointMetadata = metadata
            };

            return new AuthorizationFilterContext(
                new ActionContext(http, new RouteData(), descriptor),
                new List<IFilterMetadata>());
        }

        private sealed class SingleActionProvider : IActionDescriptorCollectionProvider
        {
            public SingleActionProvider(ActionDescriptor action)
                => ActionDescriptors = new ActionDescriptorCollection(new[] { action }, 1);

            public ActionDescriptorCollection ActionDescriptors { get; }
        }

        private sealed class StubProvider : IUserRightsProvider
        {
            private readonly ScreenRightSet _rights;

            public StubProvider(ScreenRightSet rights) => _rights = rights;

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

            public void Invalidate(int tenantId, int userId) { }
        }

        /// <summary>Two tenants, one <c>UserId</c> — the M2-A01-03 cache-key hazard, at the seam.</summary>
        private sealed class PerTenantProvider : IUserRightsProvider
        {
            private readonly ScreenRightSet _tenantOne;
            private readonly ScreenRightSet _tenantTwo;

            public PerTenantProvider(ScreenRightSet tenantOne, ScreenRightSet tenantTwo)
            {
                _tenantOne = tenantOne;
                _tenantTwo = tenantTwo;
            }

            public Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct)
                => Task.FromResult(tenantId == 1 ? _tenantOne : _tenantTwo);

            public void Invalidate(int tenantId, int userId) { }
        }

        private sealed class ThrowingProvider : IUserRightsProvider
        {
            public Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct)
                => throw new InvalidOperationException("The rights query failed.");

            public void Invalidate(int tenantId, int userId) { }
        }
    }
}
