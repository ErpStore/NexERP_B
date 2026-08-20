namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The per-request row-scope resolution seam (M2-A08, KB-108 §5) — the second authorization
    /// axis beside <see cref="IUserRightsProvider"/>, and deliberately the same shape as it.
    ///
    /// <para><c>tenantId</c> is an explicit parameter for the same reason it is on
    /// <see cref="IUserRightsProvider"/>: the deployment is database-per-tenant, so
    /// <c>UserId = 7</c> in tenant A and <c>UserId = 7</c> in tenant B are different people with
    /// different <c>StateCodesCsv</c> values, and a tenant-blind cache key would serve one tenant's
    /// scope to another (KB-105 §8.1, ADR-004 §5).</para>
    ///
    /// <para><b>Scope is resolved per request, never carried in the JWT.</b> ADR-004 §2 refused to
    /// put rights in the token; scope is the same kind of state and gets the same treatment. A token
    /// outlives a scope change exactly as it outlives a rights change, and
    /// <c>V.SMART/V.SMART.Api/Auth/JwtTokenService.cs</c> is unchanged by M2-A08.</para>
    /// </summary>
    public interface IRowScopeProvider
    {
        Task<RowScope> GetAsync(int tenantId, int userId, CancellationToken ct);

        /// <summary>
        /// Evicts one user's cached scope in this process. Same caveat as
        /// <see cref="IUserRightsProvider.Invalidate"/>: every writer of <c>StateCodesCsv</c> today
        /// runs in the Blazor host (<c>UserService.cs:373,437,491,532-535</c>), a different process,
        /// so nothing that exists can call this. The real staleness bound is the TTL.
        /// </summary>
        void Invalidate(int tenantId, int userId);
    }
}
