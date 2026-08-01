using V.SMART.Shared.Data.Master.Admin_Module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdmins
{
    public interface IUserAuthorityRepository : IRepository<UserAuthority>
    {
        Task<IEnumerable<UserAuthority>> GetAllWithUserAsync();
    }
}
