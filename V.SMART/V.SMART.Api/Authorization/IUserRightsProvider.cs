namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// The per-request rights-resolution seam (KB-105 §2.6).
    /// <para>
    /// M2-A01-02 implements this by calling
    /// <c>IUnitOfWork.UserRights.GetUserRightsWithScreensAsync(userId)</c> directly, with
    /// <b>no cache</b>. M2-A01-03 adds caching behind this interface only; the filter does not
    /// change between the two tasks.
    /// </para>
    /// <para>
    /// <paramref name="tenantId"/> is an explicit parameter rather than an ambient value so that
    /// M2-A01-03's cache key cannot accidentally omit it. The deployment is database-per-tenant,
    /// so <c>UserId = 7</c> in tenant A and <c>UserId = 7</c> in tenant B are different people
    /// with different rights (KB-105 §8.1).
    /// </para>
    /// </summary>
    public interface IUserRightsProvider
    {
        Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct);
    }
}
