namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The per-request rights-resolution seam (KB-105 §2.6).
    /// <para>
    /// M2-A01-02 implemented this by calling
    /// <c>IUnitOfWork.UserRights.GetUserRightsWithScreensAsync(userId)</c> directly, with no cache.
    /// M2-A01-03 added caching behind this interface only; the filter did not change between the
    /// two tasks, and the decision it reaches is identical with the cache cold, warm or disabled.
    /// </para>
    /// <para>
    /// <c>tenantId</c> is an explicit parameter rather than an ambient value so that the cache key
    /// cannot accidentally omit it. The deployment is database-per-tenant, so <c>UserId = 7</c> in
    /// tenant A and <c>UserId = 7</c> in tenant B are different people with different rights
    /// (KB-105 §8.1).
    /// </para>
    /// </summary>
    public interface IUserRightsProvider
    {
        Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct);

        /// <summary>
        /// Evicts one user's cached rights in this process (KB-105 §8.4, consequence 1).
        /// <para>
        /// <b>It bounds nothing today.</b> All five <c>UserRight</c> write sites run in the Blazor
        /// host — a different process with different memory — so no write that exists can reach
        /// this method. It is infrastructure for a future API-side writer, and the escape hatch
        /// M2-A03's permission-matrix harness can use when it flips rights between cases. The real
        /// staleness bound today is the TTL (<see cref="UserRightsCacheOptions"/>).
        /// </para>
        /// <para>Safe to call when the entry is absent or when caching is disabled: it is a no-op.</para>
        /// </summary>
        void Invalidate(int tenantId, int userId);
    }
}
