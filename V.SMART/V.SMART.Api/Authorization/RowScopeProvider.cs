using Microsoft.Extensions.Caching.Memory;
using V.SMART.Shared.Repository.IRepository;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// Resolves the caller's <see cref="RowScope"/> from <c>User.StateCodesCsv</c>, behind the same
    /// short, tenant-scoped memory cache the screen rights use (M2-A08, KB-108 §5; cache shape from
    /// KB-105 §8).
    ///
    /// <para><b>One row, one column.</b> The Blazor original loads <i>every</i> user row and finds
    /// the caller by username string equality in memory (<c>LeadService.cs:132-134</c>). This reads
    /// the single <c>StateCodesCsv</c> value by primary key. Two deliberate, recorded changes
    /// (KB-108 §6): the lookup is by the <c>UserId</c> claim rather than by username — the API has a
    /// user id and a name collision is then not a security question — and the query is projected
    /// rather than materialised.</para>
    /// </summary>
    public sealed class RowScopeProvider : IRowScopeProvider
    {
        /// <summary>
        /// <c>rowscope:v1:{tenantId}:{userId}</c>. Same convention, same versioned prefix and same
        /// tenant-first key as <c>screenrights:v1:</c> (<c>UserRightsProvider.cs:46-53</c>); a
        /// separate prefix because the two caches hold different types with independent lifetimes.
        /// </summary>
        private const string KeyPrefix = "rowscope:v1:";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserRightsCacheOptions _options;

        /// <summary>
        /// The TTL is <see cref="UserRightsCacheOptions"/>'s, shared on purpose rather than given a
        /// second configuration knob: both caches hold per-request authorization state read from the
        /// tenant database, and a scope revocation must not be able to outlive a rights revocation.
        /// One knob cannot be widened for one axis and forgotten for the other.
        /// </summary>
        public RowScopeProvider(IUnitOfWork unitOfWork, IMemoryCache cache, UserRightsCacheOptions options)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _options = options;
        }

        public static string CacheKey(int tenantId, int userId)
            => string.Create(
                System.Globalization.CultureInfo.InvariantCulture,
                $"{KeyPrefix}{tenantId}:{userId}");

        public async Task<RowScope> GetAsync(int tenantId, int userId, CancellationToken ct)
        {
            // The UserId == 1 carve-out, ported from LeadsList.razor:470-484. Resolved here, from
            // the claim, so it is one decision in one place rather than a branch at every call site.
            // No query is needed for it, and none is made.
            if (userId == RowScope.UnscopedUserId)
            {
                return RowScope.Unrestricted;
            }

            if (!_options.IsEnabled)
            {
                return await LoadAsync(userId);
            }

            var key = CacheKey(tenantId, userId);

            if (_cache.TryGetValue(key, out RowScope? cached) && cached is not null)
            {
                return cached;
            }

            // NO NEGATIVE CACHING, for the same reason as the rights cache (KB-105 §7.3): if the
            // query throws, this await throws with it, nothing is written, and the M2-A06 handler
            // turns it into a 500. A database fault must never be recorded as "scope: nothing" —
            // which here would silently show the user an empty list for the whole TTL.
            var scope = await LoadAsync(userId);

            _cache.Set(key, scope, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _options.Ttl,
                Size = 1
            });

            return scope;
        }

        public void Invalidate(int tenantId, int userId) => _cache.Remove(CacheKey(tenantId, userId));

        /// <summary>
        /// <c>SELECT TOP 1 StateCodesCsv FROM Users WHERE UserId = @userId</c>.
        /// <para>
        /// A missing user and a null <c>StateCodesCsv</c> both produce <c>null</c> here and both
        /// become <see cref="RowScope.Empty"/>. They are not distinguished, and they must not be:
        /// the fail-closed answer is identical for either, and telling them apart would be an
        /// enumeration oracle for user ids.
        /// </para>
        /// </summary>
        private async Task<RowScope> LoadAsync(int userId)
        {
            var csv = await _unitOfWork.Users.GetPropertyByIdAsync(
                u => u.UserId == userId,
                u => u.StateCodesCsv);

            return RowScope.FromStateCodesCsv(csv);
        }
    }
}
