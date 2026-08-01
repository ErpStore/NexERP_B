using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster
{
    public interface IStaffEducationRepository : IRepository<StaffEducation>
    {
        Task<bool> ExistsByNameAsync(string Degree, int? excludeId = null);
        Task DeleteByStaffIdAsync(int staffId);

    }
}
