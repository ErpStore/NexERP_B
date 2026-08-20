using V.SMART.Shared.Repository.IRepository;

namespace V.SMART.Api.Authorization
{
    /// <summary>
    /// Thin delegation to the existing rights query. <b>No caching, no filtering, no ordering
    /// change</b> — any of the three would change which row <c>FirstOrDefault</c> returns and so
    /// would change behaviour (KB-105 D-2, B-7). Caching is M2-A01-03's, behind
    /// <see cref="IUserRightsProvider"/>.
    /// </summary>
    /// <remarks>
    /// <paramref name="tenantId"/> is not used here and must not be: the tenant is already bound
    /// to the request's <c>ApplicationDbContext</c> by <c>TenantProvider</c> from the same
    /// <c>"TenantId"</c> claim (BR-TEN-001/002), so a cross-tenant read is structurally
    /// impossible through this repository. The parameter exists for M2-A01-03's cache key.
    /// </remarks>
    public sealed class UserRightsProvider : IUserRightsProvider
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserRightsProvider(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ScreenRightSet> GetAsync(int tenantId, int userId, CancellationToken ct)
        {
            // The one query, unchanged: V.SMART.Shared/Repository/MasterRepository/Admins/
            // UserRightsRepository.cs:22-28. It takes no CancellationToken, so ct cannot be
            // threaded into it without changing a V.SMART.Shared signature the Blazor host also
            // calls — out of scope here (M2-A01-02 changes nothing under V.SMART.Shared).
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
