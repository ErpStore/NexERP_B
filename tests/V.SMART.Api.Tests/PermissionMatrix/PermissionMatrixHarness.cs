using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using V.SMART.Api.Authorization;
using V.SMART.Api.Tests.Infrastructure;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;

namespace V.SMART.Api.Tests.PermissionMatrix
{
    /// <summary>
    /// The six rights fixtures M2-A03 requires, one per row of its matrix.
    /// </summary>
    public enum RightsFixture
    {
        /// <summary>No <c>UserRight</c> row for the endpoint's screen at all. BR-AUTH-002's
        /// deny-by-default — the <c>?? false</c> at <c>RightsHelper.cs:7-20</c>. Expect 403.</summary>
        NoRowForScreen,

        /// <summary>A row exists for the screen with every flag false. Expect 403.</summary>
        AllFlagsFalse,

        /// <summary>Only the required flag true. Expect <b>not</b> 403.</summary>
        OnlyRequiredRight,

        /// <summary>Every flag true except the required one. Expect 403 — this is the fixture that
        /// catches an endpoint annotated with the wrong right, which no other row can.</summary>
        EveryFlagExceptRequired,

        /// <summary>Required flag true <i>and</i> <c>IsHide</c> true. Expect <b>not</b> 403:
        /// <c>IsHide</c> is navigation, never an operation gate (ADR-004 §1, KB-105 B-5/T-4).</summary>
        RequiredRightAndIsHide,

        /// <summary>No usable token, with every right granted. Expect 401, not 403 — and no
        /// screen or right named in the body (KB-105 D-3, §7.2).</summary>
        NoToken
    }

    /// <summary>
    /// M2-A03 — the generic runner. One call drives one <c>(endpoint × fixture)</c> case through
    /// the <b>real</b> <see cref="ScreenRightAuthorizationFilter"/>, over the <b>real</b>
    /// endpoint's real attribute metadata, backed by the <b>real</b>
    /// <see cref="UserRightsProvider"/>.
    ///
    /// <para><b>Rights and cache isolation — the single highest-risk part of this task.</b> A
    /// leaking cache produces green results that are wrong, which is worse than no harness at
    /// all. Three things make a leak structurally impossible here rather than merely unlikely:
    /// <list type="number">
    /// <item>Every case constructs its own <see cref="MemoryCache"/>. Nothing is static and
    /// nothing is shared, so no entry can outlive the case that wrote it.</item>
    /// <item>Every case constructs its <see cref="UserRightsProvider"/> with
    /// <c>ttlSeconds: 0</c> — M2-A01-03's <b>documented bypass</b>
    /// (<c>UserRightsCacheOptions.IsEnabled</c>, <c>UserRightsProvider.cs:57-62</c>), proved by
    /// <c>UserRightsCacheTests.A_zero_TTL_disables_the_cache_entirely</c>. Every resolution goes
    /// to the repository; nothing is ever written to the cache.</item>
    /// <item><see cref="CaseResult.RightsQueries"/> counts repository calls, and every matrix
    /// case asserts it, so isolation is <i>observed</i> rather than assumed. A stale read would
    /// show as zero queries.</item>
    /// </list>
    /// <see cref="CacheIsolationTests"/> then proves the same mechanism holds when driven
    /// hundreds of times in one run, and that a flipped right is observed immediately.</para>
    ///
    /// <para><b>What this harness does not prove.</b> There is no host: this project references
    /// neither <c>Microsoft.AspNetCore.Mvc.Testing</c> nor a <c>WebApplicationFactory</c> (R-43,
    /// KB-060). Results are asserted on the filter's <c>IActionResult</c> and its
    /// <c>ProblemDetails</c> body, not on bytes leaving a socket, and the "no token" row is a
    /// filter-level 401 plus a declaration-level <c>[Authorize]</c> assertion rather than a
    /// round trip. Stated so nothing here over-claims.</para>
    /// </summary>
    internal static class PermissionMatrixHarness
    {
        /// <summary>A screen name no controller declares, used for the "no row" fixture.</summary>
        private const string UnrelatedScreen = "__no_such_screen__";

        /// <summary>Every fixture, in matrix order.</summary>
        public static IReadOnlyList<RightsFixture> Fixtures { get; } =
            Enum.GetValues<RightsFixture>().ToList();

        public static ApiEndpoint Endpoint(string key)
            => ApiEndpointDiscovery.All.SingleOrDefault(e => string.Equals(e.Key, key, StringComparison.Ordinal))
               ?? throw new InvalidOperationException(
                   $"No discovered endpoint with key '{key}'. Discovery found: " +
                   string.Join(", ", ApiEndpointDiscovery.All.Select(e => e.Key)));

        /// <summary>
        /// Runs one case. The endpoint's own <c>[RequireRight]</c> decides which flag the fixture
        /// grants or withholds, so nothing about any individual controller is hard-coded.
        /// </summary>
        public static CaseResult Run(ApiEndpoint endpoint, RightsFixture fixture)
        {
            ArgumentNullException.ThrowIfNull(endpoint);

            if (endpoint.Screen is null || endpoint.Right is null)
            {
                // Reaching here means the discovery tests should already have failed; say so
                // rather than throwing a NullReferenceException three frames deeper.
                throw new InvalidOperationException(
                    $"{endpoint.Describe()} cannot be driven through the permission matrix because it " +
                    "declares no screen and/or no right. EndpointDiscoveryTests is the test that must fail here.");
            }

            var screen = endpoint.Screen.ScreenName;
            var right = endpoint.Right.Right;

            var rows = Rows(fixture, screen, right);

            // --- isolation: a cache and a provider that exist only for this case ---------------
            var repository = new Mock<IUserRightsRepository>(MockBehavior.Strict);
            var queries = 0;
            repository
                .Setup(r => r.GetUserRightsWithScreensAsync(It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    queries++;
                    return rows.ToList();
                });

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.UserRights).Returns(repository.Object);

            using var cache = new MemoryCache(new MemoryCacheOptions());

            var provider = new UserRightsProvider(
                unitOfWork.Object,
                cache,
                new UserRightsCacheOptions(ttlSeconds: 0));   // the documented bypass

            var context = FilterContext(endpoint, authenticated: fixture != RightsFixture.NoToken);

            new ScreenRightAuthorizationFilter(provider, NullLogger<ScreenRightAuthorizationFilter>.Instance)
                .OnAuthorizationAsync(context)
                .GetAwaiter()
                .GetResult();

            var cacheKey = UserRightsProvider.CacheKey(TenantId, UserId);

            return new CaseResult(
                endpoint,
                fixture,
                context,
                queries,
                CacheWasWritten: cache.TryGetValue(cacheKey, out object? _));
        }

        /// <summary>
        /// The rows one fixture presents, derived from the endpoint's own required right. The
        /// four operation flags come from <see cref="Right"/> itself, so adding a fifth member to
        /// that enum would surface here rather than silently narrowing the matrix.
        /// </summary>
        public static IReadOnlyList<UserRight> Rows(RightsFixture fixture, string screen, Right right)
            => fixture switch
            {
                // A row for some other screen, so the repository is exercised and the set is
                // non-empty; the endpoint's own screen still has no row.
                RightsFixture.NoRowForScreen => new[]
                {
                    Row(UnrelatedScreen, canView: true, canCreate: true, canEdit: true, canDelete: true)
                },

                RightsFixture.AllFlagsFalse => new[] { Row(screen) },

                RightsFixture.OnlyRequiredRight => new[] { Granting(screen, right) },

                RightsFixture.EveryFlagExceptRequired => new[]
                {
                    Row(screen,
                        canView: right != Right.View,
                        canCreate: right != Right.Create,
                        canEdit: right != Right.Edit,
                        canDelete: right != Right.Delete,
                        isHide: true)
                },

                RightsFixture.RequiredRightAndIsHide => new[] { Granting(screen, right, isHide: true) },

                // Every right granted, deliberately: the 401 must come from the credential, never
                // from an absent right, or the two outcomes are not actually distinguished.
                RightsFixture.NoToken => new[]
                {
                    Row(screen, canView: true, canCreate: true, canEdit: true, canDelete: true)
                },

                _ => throw new ArgumentOutOfRangeException(nameof(fixture), fixture, "Unhandled rights fixture.")
            };

        /// <summary>403 for these fixtures, and only for these.</summary>
        public static bool ExpectsForbidden(RightsFixture fixture)
            => fixture is RightsFixture.NoRowForScreen
                or RightsFixture.AllFlagsFalse
                or RightsFixture.EveryFlagExceptRequired;

        public const int TenantId = 1;
        public const int UserId = 7;

        public static UserRight Row(
            string screenName,
            bool canView = false,
            bool canCreate = false,
            bool canEdit = false,
            bool canDelete = false,
            bool isHide = false)
            => new()
            {
                CanView = canView,
                CanCreate = canCreate,
                CanEdit = canEdit,
                CanDelete = canDelete,
                IsHide = isHide,
                Screens = new Screens { ScreenName = screenName }
            };

        public static UserRight Granting(string screen, Right right, bool isHide = false) => right switch
        {
            Right.View => Row(screen, canView: true, isHide: isHide),
            Right.Create => Row(screen, canCreate: true, isHide: isHide),
            Right.Edit => Row(screen, canEdit: true, isHide: isHide),
            Right.Delete => Row(screen, canDelete: true, isHide: isHide),
            _ => throw new ArgumentOutOfRangeException(nameof(right), right, "Unhandled right.")
        };

        /// <summary>
        /// An <see cref="AuthorizationFilterContext"/> whose <c>EndpointMetadata</c> is the real
        /// metadata discovery read from the real controller and action, in MVC's own order.
        /// Nothing is hand-written, so deleting an attribute in a controller changes what the
        /// filter sees here and fails the matrix instead of quietly passing it.
        /// </summary>
        public static AuthorizationFilterContext FilterContext(ApiEndpoint endpoint, bool authenticated)
        {
            var http = ErrorContractTestContext.Create(endpoint.Route.Split(' ').Last(), "GET");

            http.User = authenticated
                ? new ClaimsPrincipal(new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.Name, "tester"),
                        new Claim("UserId", UserId.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                        new Claim("TenantId", TenantId.ToString(System.Globalization.CultureInfo.InvariantCulture))
                    },
                    "Test"))
                : new ClaimsPrincipal(new ClaimsIdentity());   // no token: no identity, no claims

            var descriptor = new ControllerActionDescriptor
            {
                ControllerName = endpoint.ControllerName,
                ActionName = endpoint.ActionName,
                ControllerTypeInfo = endpoint.ControllerType.GetTypeInfo(),
                MethodInfo = endpoint.Method,
                EndpointMetadata = endpoint.EndpointMetadata.ToList()
            };

            return new AuthorizationFilterContext(
                new ActionContext(http, new RouteData(), descriptor),
                new List<IFilterMetadata>());
        }
    }

    /// <summary>The outcome of one matrix case, with everything an assertion needs to name it.</summary>
    internal sealed record CaseResult(
        ApiEndpoint Endpoint,
        RightsFixture Fixture,
        AuthorizationFilterContext Context,
        int RightsQueries,
        bool CacheWasWritten)
    {
        /// <summary>The message every assertion in the matrix carries (M2-A03: a bare failure
        /// across hundreds of cases is unusable).</summary>
        public string Describe() => $"[{Fixture}] {Endpoint.Describe()}";
    }
}
