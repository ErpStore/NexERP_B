using V.SMART.Shared.Data.Master.MasterScreeenManagement;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IMasterSettings
{
    public interface IScreenManagementRepository : IRepository<ScreenManagement>
    {
        Task<bool> IsFeatureEnabledAsync(string screenName, string displayName);
    }
}
