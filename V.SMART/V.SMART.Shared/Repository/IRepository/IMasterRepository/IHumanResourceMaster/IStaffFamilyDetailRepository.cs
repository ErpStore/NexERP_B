using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster
{
    public interface IStaffFamilyDetailRepository : IRepository<StaffFamilyDetails>
    {
        Task<bool> ExistsByNameAsync(string Name, int? excludeId = null);
        Task DeleteByStaffIdAsync(int staffId);
    }
}
