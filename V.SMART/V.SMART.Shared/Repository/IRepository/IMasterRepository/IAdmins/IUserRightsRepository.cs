using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.Admin_Module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository
{
    public interface IUserRightsRepository : IRepository<UserRight>
    {
        Task<List<UserRight>> GetUserRightsWithScreensAsync(int userId);
    }
    public interface IUserColumnPreferenceRepository : IRepository<UserColumnPreference>
    {

    }
}
