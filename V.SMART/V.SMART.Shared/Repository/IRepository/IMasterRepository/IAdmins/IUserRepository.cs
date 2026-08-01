using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IAdminRepository
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> LoginAsync(string username, string password);

        Task<User?> GetUserByQrToken(Guid token);
        
        Task<UserVM?> GetUserTrialAsync(int userId);
    }

}
