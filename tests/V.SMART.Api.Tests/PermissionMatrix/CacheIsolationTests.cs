using Microsoft.Extensions.Caching.Memory;
using Moq;
using V.SMART.Api.Authorization;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository;
using Xunit;

namespace V.SMART.Api.Tests.PermissionMatrix
{
    /// <summary>
    /// M2-A03 — <b>the meta-tests</b>. The matrix flips rights between cases; if the rights cache
    /// leaked, the matrix would still be green and it would be wrong. These are the tests that
    /// fail when the isolation mechanism silently stops working, which is the only reason to
    /// trust the sixty cases next door.
    ///
    /// <para>They exercise the <b>real</b> <see cref="UserRightsProvider"/> over a <b>real</b>
    /// <see cref="MemoryCache"/> — not the matrix's stand-in — because a cache that is never
    /// constructed cannot leak, and a test of a cache that does not exist proves nothing.
    /// M2-A01-03 documented two levers and both are exercised here: the zero-TTL bypass
    /// (<c>UserRightsCacheOptions.IsEnabled</c>, <c>UserRightsProvider.cs:57-62</c>) which the
    /// matrix uses, and <c>Invalidate</c> (<c>:91</c>) which is the escape hatch when a TTL is
    /// configured.</para>
    ///
    /// <para><b>Driven hundreds of times, not a handful.</b> M2-A03's investigation requirement is
    /// that the bypass be confirmed under the load the harness actually applies.
    /// <see cref="The_bypass_does_not_leak_when_driven_hundreds_of_times"/> flips a right 400
    /// times against one provider and one cache instance and asserts every single observation.</para>
    /// </summary>
    public class CacheIsolationTests
    {
        private const string Screen = "Currency";
        private const int Tenant = 1;
        private const int User = 7;

        // =====================================================================================
        // The mechanism the matrix relies on
        // =====================================================================================

        /// <summary>
        /// The meta-test M2-A03 names: flip a right, and the change is observed <b>immediately</b>
        /// — no delay, no eviction call, no second provider.
        /// </summary>
        [Fact]
        public async Task A_flipped_right_is_observed_immediately_under_the_zero_TTL_bypass()
        {
            var granted = true;
            var provider = Provider(ttlSeconds: 0, () => new List<UserRight>
            {
                PermissionMatrixHarness.Row(Screen, canView: granted)
            }, out var cache);

            Assert.True((await provider.GetAsync(Tenant, User, default)).Has(Screen, Right.View));

            granted = false;
            Assert.False(
                (await provider.GetAsync(Tenant, User, default)).Has(Screen, Right.View),
                "A revoked right was still observed as granted: the bypass is not bypassing.");

            granted = true;
            Assert.True((await provider.GetAsync(Tenant, User, default)).Has(Screen, Right.View));

            Assert.False(
                cache.TryGetValue(UserRightsProvider.CacheKey(Tenant, User), out object? _),
                "The zero-TTL bypass wrote a cache entry; nothing may be written when caching is disabled.");
        }

        /// <summary>
        /// The load question, answered rather than assumed. 400 alternating flips against a single
        /// provider and a single cache; every observation is asserted, so one leaked read fails
        /// the test and names the iteration.
        /// </summary>
        [Fact]
        public async Task The_bypass_does_not_leak_when_driven_hundreds_of_times()
        {
            var iteration = 0;
            var provider = Provider(ttlSeconds: 0, () => new List<UserRight>
            {
                PermissionMatrixHarness.Row(Screen, canView: iteration % 2 == 0)
            }, out var cache);

            for (iteration = 0; iteration < 400; iteration++)
            {
                var expected = iteration % 2 == 0;

                Assert.True(
                    (await provider.GetAsync(Tenant, User, default)).Has(Screen, Right.View) == expected,
                    $"Iteration {iteration}: expected CanView = {expected} but the provider returned the " +
                    "opposite. A previous iteration's rights leaked into this one.");
            }

            Assert.False(cache.TryGetValue(UserRightsProvider.CacheKey(Tenant, User), out object? _));
        }

        /// <summary>
        /// The control case, and the reason the bypass is required rather than merely nice: with a
        /// normal TTL the <i>same</i> flip is <b>not</b> observed. A harness built on the default
        /// configuration would pass falsely, which is exactly the failure M2-A03 calls the single
        /// biggest risk in the task.
        /// </summary>
        [Fact]
        public async Task With_the_cache_enabled_the_same_flip_is_not_observed_which_is_why_the_bypass_exists()
        {
            var granted = true;
            var provider = Provider(ttlSeconds: 60, () => new List<UserRight>
            {
                PermissionMatrixHarness.Row(Screen, canView: granted)
            }, out var cache);

            Assert.True((await provider.GetAsync(Tenant, User, default)).Has(Screen, Right.View));

            granted = false;
            Assert.True(
                (await provider.GetAsync(Tenant, User, default)).Has(Screen, Right.View),
                "The enabled cache served a fresh value; this control case documents the stale read the " +
                "bypass avoids, so if it ever stops being stale the harness's rationale must be revisited.");

            Assert.True(cache.TryGetValue(UserRightsProvider.CacheKey(Tenant, User), out object? _));

            // The second documented lever: explicit eviction makes the flip visible again.
            provider.Invalidate(Tenant, User);

            Assert.False((await provider.GetAsync(Tenant, User, default)).Has(Screen, Right.View));
        }

        // =====================================================================================
        // Isolation across matrix cases, asserted end to end
        // =====================================================================================

        /// <summary>
        /// The property the matrix depends on, stated over the matrix itself rather than over the
        /// provider: running the whole matrix for one endpoint in the most adversarial order —
        /// every "granted" case immediately followed by a "denied" case — produces the same
        /// outcomes as running each case alone. If any state survived a case, these two runs would
        /// disagree.
        /// </summary>
        [Fact]
        public void Matrix_outcomes_do_not_depend_on_the_order_cases_run_in()
        {
            var endpoint = ApiEndpointDiscovery.Guarded.FirstOrDefault();
            Assert.NotNull(endpoint);

            // Materialised deliberately, and in the stated order: LINQ's Reverse buffers its
            // source, so a lazy chain would have run the cases forwards either way and proved
            // nothing about ordering.
            var forward = new List<(RightsFixture Fixture, bool Denied)>();
            foreach (var fixture in PermissionMatrixHarness.Fixtures)
            {
                forward.Add((fixture, IsDenied(endpoint!, fixture)));
            }

            var backward = new List<(RightsFixture Fixture, bool Denied)>();
            foreach (var fixture in PermissionMatrixHarness.Fixtures.Reverse().ToList())
            {
                backward.Add((fixture, IsDenied(endpoint!, fixture)));
            }

            backward.Reverse();

            Assert.Equal(forward, backward);

            // Interleaved, twice over, to make a leak from a granting case into a denying case
            // impossible to miss.
            foreach (var _ in Enumerable.Range(0, 2))
            {
                Assert.False(IsDenied(endpoint!, RightsFixture.OnlyRequiredRight));
                Assert.True(IsDenied(endpoint!, RightsFixture.AllFlagsFalse));
                Assert.False(IsDenied(endpoint!, RightsFixture.RequiredRightAndIsHide));
                Assert.True(IsDenied(endpoint!, RightsFixture.NoRowForScreen));
            }
        }

        private static bool IsDenied(ApiEndpoint endpoint, RightsFixture fixture)
            => PermissionMatrixHarness.Run(endpoint, fixture).Context.Result is not null;

        private static UserRightsProvider Provider(
            int ttlSeconds,
            Func<List<UserRight>> rows,
            out IMemoryCache cache)
        {
            var repository = new Mock<IUserRightsRepository>(MockBehavior.Strict);
            repository
                .Setup(r => r.GetUserRightsWithScreensAsync(It.IsAny<int>()))
                .ReturnsAsync(rows);

            var unitOfWork = new Mock<IUnitOfWork>();
            unitOfWork.SetupGet(u => u.UserRights).Returns(repository.Object);

            cache = new MemoryCache(new MemoryCacheOptions());

            return new UserRightsProvider(unitOfWork.Object, cache, new UserRightsCacheOptions(ttlSeconds));
        }
    }
}
