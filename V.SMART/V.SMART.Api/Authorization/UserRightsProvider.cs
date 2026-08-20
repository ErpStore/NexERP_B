using Microsoft.Extensions.Caching.Memory;
using V.SMART.Shared.Repository.IRepository;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The rights query (M2-A01-02) behind a short, tenant-scoped memory cache (M2-A01-03,
    /// KB-105 §8).
    /// <para>
    /// <b>No filtering and no ordering change</b> — either would change which row
    /// <c>FirstOrDefault</c> returns and so would change behaviour (KB-105 D-2, B-7). The cache
    /// stores the <see cref="ScreenRightSet"/> the uncached path produced, by reference, exactly as
    /// it was produced.
    /// </para>
    /// </summary>
    /// <remarks>
    /// <c>tenantId</c> is not used for the query and must not be: the tenant is already bound to the
    /// request's <c>ApplicationDbContext</c> by <c>TenantProvider</c> from the same
    /// <c>"TenantId"</c> claim (BR-TEN-001/002), so a cross-tenant read is structurally impossible
    /// through this repository. The cache, however, sits <b>outside</b> that structural guarantee —
    /// it is one process-wide dictionary shared by every tenant — which is why <c>tenantId</c> is
    /// the first component of the key (KB-105 §8.1).
    /// </remarks>
    public sealed class UserRightsProvider : IUserRightsProvider
    {
        /// <summary>
        /// Prefix and schema version of the cache key (KB-105 §8.1). The <c>v1</c> segment lets
        /// <see cref="ScreenRightSet"/>'s shape change without a stale-entry hazard across a rolling
        /// deploy; the prefix keeps these entries from colliding with any other consumer of the
        /// shared <see cref="IMemoryCache"/>.
        /// </summary>
        private const string KeyPrefix = "screenrights:v1:";

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMemoryCache _cache;
        private readonly UserRightsCacheOptions _options;

        public UserRightsProvider(IUnitOfWork unitOfWork, IMemoryCache cache, UserRightsCacheOptions options)
        {
            _unitOfWork = unitOfWork;
            _cache = cache;
            _options = options;
        }

        /// <summary>
        /// <c>screenrights:v1:{tenantId}:{userId}</c> — KB-105 §8.1. Both identities are present:
        /// <c>UserId</c> is a primary key inside one tenant database, so two tenants can both have
        /// <c>UserId = 1</c> and a tenant-blind key would serve one tenant's rights to another.
        /// </summary>
        public static string CacheKey(int tenantId, int userId)
            => string.Create(
                System.Globalization.CultureInfo.InvariantCulture,
                $"{KeyPrefix}{tenantId}:{userId}");

        public async Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct)
        {
            if (!_options.IsEnabled)
            {
                // TTL 0 — the documented bypass. Every call resolves from the database, which is
                // exactly the uncached M2-A01-02 behaviour.
                return await LoadAsync(userId);
            }

            var key = CacheKey(tenantId, userId);

            if (_cache.TryGetValue(key, out ScreenRightSet? cached) && cached is not null)
            {
                return cached;
            }

            // NO NEGATIVE CACHING. If the query throws, this await throws with it: nothing is
            // written, and the exception propagates to the M2-A06 handler. A database fault must
            // never be recorded as "no rights", and must never read as an allow (KB-105 §7.3).
            var rights = await LoadAsync(userId);

            // ABSOLUTE, never sliding. A sliding window on a busy user never expires, which is the
            // failure ADR-004 §2 rejected when it refused to put rights in the JWT (KB-105 §8.2).
            //
            // Size is set even though no SizeLimit is configured on the shared cache: it is
            // harmless without one, and it means a future cap needs no change here. KB-105 §8.2
            // records why the cap is not imposed on the shared singleton.
            _cache.Set(key, rights, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _options.Ttl,
                Size = 1
            });

            return rights;
        }

        public void Invalidate(int tenantId, int userId) => _cache.Remove(CacheKey(tenantId, userId));

        /// <summary>
        /// The one query, unchanged from M2-A01-02:
        /// <c>V.SMART.Shared/Repository/MasterRepository/Admins/UserRightsRepository.cs:22-28</c>.
        /// It takes no <c>CancellationToken</c>, so <c>ct</c> cannot be threaded into it without
        /// changing a <c>V.SMART.Shared</c> signature the Blazor host also calls — out of scope.
        /// </summary>
        private async Task<ScreenRightSet> LoadAsync(int userId)
        {
            var rows = await _unitOfWork.UserRights.GetUserRightsWithScreensAsync(userId);

            var entries = new List<ScreenRightEntry>(rows.Count);
            foreach (var row in rows)
            {
                // Rows whose Screens navigation did not load carry no name to match on, so they
                // can never satisfy an ordinal comparison; they are dropped rather than projected
                // with a null name. This is the same outcome RightsHelper reaches — a null
                // ScreenName never equals a declared screen — reached without a null dereference.
                var screenName = row.Screens?.ScreenName;
                if (screenName is null)
                {
                    continue;
                }

                entries.Add(new ScreenRightEntry(
                    screenName,
                    row.CanView,
                    row.CanCreate,
                    row.CanEdit,
                    row.CanDelete,
                    row.IsHide));
            }

            return new ScreenRightSet(entries);
        }
    }
}
