using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using V.SMART.Api.Authorization;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using Xunit;

namespace V.SMART.Api.Tests
{
    /// <summary>
    /// M2-A01-03 — the screen-rights cache (KB-105 §8), exercised against the real
    /// <see cref="UserRightsProvider"/> and a real <see cref="MemoryCache"/>. No host and no
    /// database: the one repository call the provider makes is a Moq stand-in, so the observable
    /// fact these tests assert is <b>how many times the query runs</b>, which is the whole point of
    /// the cache.
    /// <para>
    /// These tests exist because the M2-A01-03 validation could otherwise only reason about the
    /// cache from the source. They do not re-test the decision logic — that is
    /// <see cref="ScreenRightAuthorizationFilterTests"/> (KB-105 §3, T-1..T-13) and is unchanged
    /// by caching.
    /// </para>
    /// </summary>
    public class UserRightsCacheTests
    {
        private const string Screen = "Currency";

        // ---- warm / cold ------------------------------------------------------------------

        [Fact]
        public async Task A_second_call_inside_the_TTL_does_not_query_again()
        {
            var repo = Repository(Row(Screen, canView: true));
            var provider = Provider(repo, ttlSeconds: 60, out _);

            var first = await provider.GetAsync(1, 7, CancellationToken.None);
            var second = await provider.GetAsync(1, 7, CancellationToken.None);

            repo.Verify(r => r.GetUserRightsWithScreensAsync(7), Times.Once);
            Assert.Same(first, second);
            Assert.True(second.Has(Screen, Right.View));
        }

        [Fact]
        public async Task A_cold_call_returns_exactly_what_the_repository_produced()
        {
            // Same rows, same order, no filtering and no projection beyond M2-A01-02's own
            // (KB-105 D-2/B-7: FirstOrDefault depends on the query's order).
            var repo = Repository(
                Row("Sales Order", canView: true),
                Row(Screen, canEdit: true),
                Row("Sales Order", canDelete: true));

            var provider = Provider(repo, ttlSeconds: 60, out _);

            var rights = await provider.GetAsync(1, 7, CancellationToken.None);

            Assert.Equal(
                new[] { "Sales Order", Screen, "Sales Order" },
                rights.Entries.Select(e => e.ScreenName).ToArray());

            // First-match-wins on the duplicate, exactly as RightsHelper does today.
            Assert.True(rights.Has("Sales Order", Right.View));
            Assert.False(rights.Has("Sales Order", Right.Delete));
        }

        // ---- tenant isolation of the key (BR-TEN-001 sits outside an in-process cache) -----

        [Fact]
        public async Task Two_tenants_with_the_same_user_id_do_not_share_an_entry()
        {
            var repo = new Mock<IUserRightsRepository>(MockBehavior.Strict);
            repo.SetupSequence(r => r.GetUserRightsWithScreensAsync(1))
                .ReturnsAsync(new List<UserRight> { Row("TenantOneScreen", canView: true) })
                .ReturnsAsync(new List<UserRight> { Row("TenantTwoScreen", canView: true) });

            var provider = Provider(repo, ttlSeconds: 60, out _);

            var tenantOne = await provider.GetAsync(1, 1, CancellationToken.None);
            var tenantTwo = await provider.GetAsync(2, 1, CancellationToken.None);

            repo.Verify(r => r.GetUserRightsWithScreensAsync(1), Times.Exactly(2));
            Assert.True(tenantOne.Has("TenantOneScreen", Right.View));
            Assert.False(tenantOne.Has("TenantTwoScreen", Right.View));
            Assert.True(tenantTwo.Has("TenantTwoScreen", Right.View));
            Assert.False(tenantTwo.Has("TenantOneScreen", Right.View));

            Assert.Equal("screenrights:v1:1:1", UserRightsProvider.CacheKey(1, 1));
            Assert.Equal("screenrights:v1:2:1", UserRightsProvider.CacheKey(2, 1));
            Assert.NotEqual(UserRightsProvider.CacheKey(1, 1), UserRightsProvider.CacheKey(2, 1));
        }

        // ---- expiry: absolute, not sliding ------------------------------------------------

        [Fact]
        public async Task The_entry_expires_and_the_window_is_absolute_not_sliding()
        {
            // The only test here that spends wall-clock time. TTL 1 s (a legal configured value);
            // the mid-window read at ~0.4 s would postpone expiry if the entry were sliding, and
            // the read after 1.3 s proves it did not.
            var repo = new Mock<IUserRightsRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetUserRightsWithScreensAsync(7))
                .ReturnsAsync(() => new List<UserRight> { Row(Screen, canView: true) });

            var provider = Provider(repo, ttlSeconds: 1, out _);

            await provider.GetAsync(1, 7, CancellationToken.None);
            await Task.Delay(TimeSpan.FromMilliseconds(400));
            await provider.GetAsync(1, 7, CancellationToken.None);

            repo.Verify(r => r.GetUserRightsWithScreensAsync(7), Times.Once);

            await Task.Delay(TimeSpan.FromMilliseconds(900));
            await provider.GetAsync(1, 7, CancellationToken.None);

            repo.Verify(r => r.GetUserRightsWithScreensAsync(7), Times.Exactly(2));
        }

        // ---- the documented bypass and the explicit eviction path -------------------------

        [Fact]
        public async Task A_zero_TTL_disables_the_cache_entirely()
        {
            // M2-A03's harness flips rights between cases; without this bypass it would read a
            // warm entry and pass falsely (KB-105 §8.6).
            var repo = Repository(Row(Screen, canView: true));
            var provider = Provider(repo, ttlSeconds: 0, out var cache);

            await provider.GetAsync(1, 7, CancellationToken.None);
            await provider.GetAsync(1, 7, CancellationToken.None);

            repo.Verify(r => r.GetUserRightsWithScreensAsync(7), Times.Exactly(2));
            Assert.False(cache.TryGetValue(UserRightsProvider.CacheKey(1, 7), out object? _));
        }

        [Fact]
        public async Task Invalidate_evicts_only_the_named_user_and_is_safe_when_absent()
        {
            var repo = Repository(Row(Screen, canView: true));
            var provider = Provider(repo, ttlSeconds: 60, out _);

            await provider.GetAsync(1, 7, CancellationToken.None);
            await provider.GetAsync(1, 8, CancellationToken.None);

            provider.Invalidate(1, 7);
            provider.Invalidate(1, 999);   // absent entry — must be a no-op, not a throw

            await provider.GetAsync(1, 7, CancellationToken.None);
            await provider.GetAsync(1, 8, CancellationToken.None);

            repo.Verify(r => r.GetUserRightsWithScreensAsync(7), Times.Exactly(2));
            repo.Verify(r => r.GetUserRightsWithScreensAsync(8), Times.Once);
        }

        // ---- no negative caching ----------------------------------------------------------

        [Fact]
        public async Task A_failing_query_is_not_cached_and_never_reads_as_an_allow()
        {
            var repo = new Mock<IUserRightsRepository>(MockBehavior.Strict);
            repo.SetupSequence(r => r.GetUserRightsWithScreensAsync(7))
                .ThrowsAsync(new InvalidOperationException("rights query failed"))
                .ReturnsAsync(new List<UserRight> { Row(Screen, canView: true) });

            var provider = Provider(repo, ttlSeconds: 60, out var cache);

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => provider.GetAsync(1, 7, CancellationToken.None));

            // Nothing written: a database fault must never be recorded as "no rights" either,
            // because a cached empty set would deny for the whole TTL and mask the fault.
            Assert.False(cache.TryGetValue(UserRightsProvider.CacheKey(1, 7), out object? _));

            var afterRecovery = await provider.GetAsync(1, 7, CancellationToken.None);

            Assert.True(afterRecovery.Has(Screen, Right.View));
            repo.Verify(r => r.GetUserRightsWithScreensAsync(7), Times.Exactly(2));
        }

        // ---- configuration ----------------------------------------------------------------

        [Fact]
        public void A_missing_TTL_takes_the_ADR_004_default_of_sixty_seconds()
        {
            var options = UserRightsCacheOptions.FromConfiguration(Configuration(null));

            Assert.Equal(60, options.TtlSeconds);
            Assert.Equal(UserRightsCacheOptions.DefaultTtlSeconds, options.TtlSeconds);
            Assert.True(options.IsEnabled);
            Assert.Equal(TimeSpan.FromSeconds(60), options.Ttl);
        }

        [Fact]
        public void A_zero_TTL_binds_as_disabled_rather_than_as_an_error()
        {
            var options = UserRightsCacheOptions.FromConfiguration(Configuration("0"));

            Assert.Equal(0, options.TtlSeconds);
            Assert.False(options.IsEnabled);
        }

        [Theory]
        [InlineData("301")]     // above MaxTtlSeconds — a security regression, not a tuning knob
        [InlineData("-1")]
        [InlineData("abc")]
        public void An_out_of_range_or_unparseable_TTL_throws_naming_the_configuration_key(string raw)
        {
            var error = Assert.Throws<InvalidOperationException>(
                () => UserRightsCacheOptions.FromConfiguration(Configuration(raw)));

            Assert.Contains(UserRightsCacheOptions.TtlSecondsConfigurationKey, error.Message);
        }

        [Fact]
        public void The_maximum_itself_is_accepted()
        {
            var options = UserRightsCacheOptions.FromConfiguration(Configuration("300"));

            Assert.Equal(UserRightsCacheOptions.MaxTtlSeconds, options.TtlSeconds);
        }

        // ---- helpers ----------------------------------------------------------------------

        private static IConfiguration Configuration(string? rightsCacheSeconds)
            => new ConfigurationBuilder()
                .AddInMemoryCollection(rightsCacheSeconds is null
                    ? new Dictionary<string, string?>()
                    : new Dictionary<string, string?>
                    {
                        [UserRightsCacheOptions.TtlSecondsConfigurationKey] = rightsCacheSeconds
                    })
                .Build();

        private static Mock<IUserRightsRepository> Repository(params UserRight[] rows)
        {
            var repo = new Mock<IUserRightsRepository>(MockBehavior.Strict);
            repo.Setup(r => r.GetUserRightsWithScreensAsync(It.IsAny<int>()))
                .ReturnsAsync(() => rows.ToList());
            return repo;
        }

        private static UserRightsProvider Provider(
            Mock<IUserRightsRepository> repository,
            int ttlSeconds,
            out IMemoryCache cache)
        {
            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.UserRights).Returns(repository.Object);

            cache = new MemoryCache(new MemoryCacheOptions());

            return new UserRightsProvider(
                unitOfWork.Object,
                cache,
                new UserRightsCacheOptions(ttlSeconds));
        }

        private static UserRight Row(
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
    }
}
