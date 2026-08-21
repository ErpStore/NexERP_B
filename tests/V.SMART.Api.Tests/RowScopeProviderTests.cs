using System.Linq.Expressions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using V.SMART.Api.Authorization;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A08 — resolving one caller's <see cref="RowScope"/> from <c>User.StateCodesCsv</c>, and
    /// the tenant-scoped cache around it. Same shape as <see cref="UserRightsCacheTests"/>: no host
    /// and no database, so what these assert is the scope produced and <b>how many times the query
    /// runs</b>.
    /// </summary>
    public class RowScopeProviderTests
    {
        // ---- parsing: the fail-closed rule and the LeadService port -------------------------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(",,")]
        [InlineData("abc")]
        public void A_blank_or_unusable_StateCodesCsv_is_an_empty_scope_never_an_unrestricted_one(string? csv)
        {
            // LeadService.cs:136-142 — the single most important behaviour in this task. An
            // unconfigured user sees ZERO leads, not all of them (KB-108 P2). "abc" is here because
            // User.StateCodes (User.cs:53-64) would throw on it via int.Parse; an unparseable column
            // must not turn a login into a 500, and it must not fail open either.
            var scope = RowScope.FromStateCodesCsv(csv);

            Assert.False(scope.IsUnrestricted);
            Assert.Empty(scope.StateCodes);
            Assert.False(scope.Allows(1));
        }

        [Theory]
        [InlineData("7", new[] { 7 })]
        [InlineData("7,9", new[] { 7, 9 })]
        [InlineData("7, 9", new[] { 7, 9 })]          // LeadService.cs:138 trims each token.
        [InlineData("7,,9", new[] { 7, 9 })]          // RemoveEmptyEntries, as LeadService.cs:136.
        [InlineData("7,9,7", new[] { 7, 9 })]         // A duplicated code is still one code.
        [InlineData("7,x,9", new[] { 7, 9 })]         // A junk token is dropped, not thrown on.
        public void A_populated_StateCodesCsv_parses_to_its_codes(string csv, int[] expected)
        {
            var scope = RowScope.FromStateCodesCsv(csv);

            Assert.Equal(expected, scope.StateCodes);
            Assert.False(scope.IsUnrestricted);
        }

        [Fact]
        public void Empty_and_Unrestricted_are_told_apart_by_IsUnrestricted_not_by_the_code_count()
        {
            // Both carry zero codes and they mean opposite things. Anything reading Count == 0 as
            // "no restriction" fails open for every unconfigured user.
            Assert.Empty(RowScope.Empty.StateCodes);
            Assert.Empty(RowScope.Unrestricted.StateCodes);
            Assert.False(RowScope.Empty.IsUnrestricted);
            Assert.True(RowScope.Unrestricted.IsUnrestricted);

            Assert.False(RowScope.Empty.Allows(7));
            Assert.True(RowScope.Unrestricted.Allows(7));
        }

        [Fact]
        public void Allows_answers_the_single_row_question()
        {
            // The by-id path (KB-108 P8): the row is already materialised, so there is no query
            // left to filter and the caller must ask.
            var scope = RowScope.ForStateCodes(7, 9);

            Assert.True(scope.Allows(7));
            Assert.False(scope.Allows(8));
        }

        // ---- resolution -------------------------------------------------------------------

        [Fact]
        public async Task The_scope_comes_from_the_callers_own_StateCodesCsv()
        {
            var users = Users("7,9");
            var provider = Provider(users, ttlSeconds: 60);

            var scope = await provider.GetAsync(1, 7, CancellationToken.None);

            Assert.Equal(new[] { 7, 9 }, scope.StateCodes);
        }

        [Fact]
        public async Task User_1_is_unrestricted_and_costs_no_query_at_all()
        {
            // LeadsList.razor:470-484, ported. The carve-out is resolved from the claim, once, and
            // never from a call site.
            var users = Users("this value must not be read");
            var provider = Provider(users, ttlSeconds: 60);

            var scope = await provider.GetAsync(1, RowScope.UnscopedUserId, CancellationToken.None);

            Assert.True(scope.IsUnrestricted);
            users.Verify(
                r => r.GetPropertyByIdAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Expression<Func<User, string?>>>()),
                Times.Never);
        }

        [Fact]
        public async Task A_missing_user_row_resolves_to_the_empty_scope()
        {
            // A missing user and a null StateCodesCsv are deliberately indistinguishable: the
            // fail-closed answer is the same for both, and telling them apart would enumerate ids.
            var provider = Provider(Users(null), ttlSeconds: 60);

            var scope = await provider.GetAsync(1, 7, CancellationToken.None);

            Assert.False(scope.IsUnrestricted);
            Assert.Empty(scope.StateCodes);
        }

        // ---- the cache --------------------------------------------------------------------

        [Fact]
        public async Task A_second_call_inside_the_TTL_does_not_query_again()
        {
            var users = Users("7");
            var provider = Provider(users, ttlSeconds: 60);

            var first = await provider.GetAsync(1, 7, CancellationToken.None);
            var second = await provider.GetAsync(1, 7, CancellationToken.None);

            Assert.Same(first, second);
            users.Verify(
                r => r.GetPropertyByIdAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Expression<Func<User, string?>>>()),
                Times.Once);
        }

        [Fact]
        public async Task A_zero_TTL_disables_the_cache_and_every_call_resolves_again()
        {
            var users = Users("7");
            var provider = Provider(users, ttlSeconds: 0);

            await provider.GetAsync(1, 7, CancellationToken.None);
            await provider.GetAsync(1, 7, CancellationToken.None);

            users.Verify(
                r => r.GetPropertyByIdAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Expression<Func<User, string?>>>()),
                Times.Exactly(2));
        }

        [Fact]
        public void The_cache_key_carries_the_tenant_so_two_tenants_cannot_share_a_scope()
        {
            // Test 6 — ADR-004 §5. UserId = 7 in tenant 1 and UserId = 7 in tenant 2 are different
            // people in different databases who may hold identical StateCodesCsv values. The cache
            // sits outside the per-request tenant binding, so the tenant must be in the key.
            Assert.NotEqual(RowScopeProvider.CacheKey(1, 7), RowScopeProvider.CacheKey(2, 7));
            Assert.Equal("rowscope:v1:1:7", RowScopeProvider.CacheKey(1, 7));

            // And it must not collide with the screen-rights entries in the same shared cache.
            Assert.NotEqual(UserRightsProvider.CacheKey(1, 7), RowScopeProvider.CacheKey(1, 7));
        }

        [Fact]
        public async Task Two_tenants_with_the_same_user_id_get_their_own_scope()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());

            var tenantOne = Provider(Users("7"), ttlSeconds: 60, cache);
            var tenantTwo = Provider(Users("9"), ttlSeconds: 60, cache);

            var one = await tenantOne.GetAsync(1, 7, CancellationToken.None);
            var two = await tenantTwo.GetAsync(2, 7, CancellationToken.None);

            Assert.Equal(new[] { 7 }, one.StateCodes);
            Assert.Equal(new[] { 9 }, two.StateCodes);
        }

        [Fact]
        public async Task Invalidate_evicts_only_the_named_tenant_and_user()
        {
            var cache = new MemoryCache(new MemoryCacheOptions());
            var users = Users("7");
            var provider = Provider(users, ttlSeconds: 60, cache);

            await provider.GetAsync(1, 7, CancellationToken.None);
            provider.Invalidate(1, 8);
            await provider.GetAsync(1, 7, CancellationToken.None);

            users.Verify(
                r => r.GetPropertyByIdAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Expression<Func<User, string?>>>()),
                Times.Once);

            provider.Invalidate(1, 7);
            await provider.GetAsync(1, 7, CancellationToken.None);

            users.Verify(
                r => r.GetPropertyByIdAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Expression<Func<User, string?>>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task A_failing_query_propagates_and_is_never_cached_as_an_empty_scope()
        {
            // No negative caching (KB-105 §7.3, mirrored). A database fault recorded as "scope:
            // nothing" would show the user an empty list for the whole TTL and look like data loss.
            var users = new Mock<IUserRepository>();
            users
                .Setup(r => r.GetPropertyByIdAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Expression<Func<User, string?>>>()))
                .ThrowsAsync(new InvalidOperationException("database is down"));

            var cache = new MemoryCache(new MemoryCacheOptions());
            var provider = Provider(users, ttlSeconds: 60, cache);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => provider.GetAsync(1, 7, CancellationToken.None));

            Assert.False(cache.TryGetValue(RowScopeProvider.CacheKey(1, 7), out _));
        }

        // ---- fixtures ----------------------------------------------------------------------

        private static Mock<IUserRepository> Users(string? stateCodesCsv)
        {
            var users = new Mock<IUserRepository>();
            users
                .Setup(r => r.GetPropertyByIdAsync(
                    It.IsAny<Expression<Func<User, bool>>>(),
                    It.IsAny<Expression<Func<User, string?>>>()))
                .ReturnsAsync(stateCodesCsv);
            return users;
        }

        private static RowScopeProvider Provider(Mock<IUserRepository> users, int ttlSeconds, IMemoryCache? cache = null)
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.Users).Returns(users.Object);

            return new RowScopeProvider(
                unitOfWork.Object,
                cache ?? new MemoryCache(new MemoryCacheOptions()),
                new UserRightsCacheOptions(ttlSeconds));
        }
    }
}
