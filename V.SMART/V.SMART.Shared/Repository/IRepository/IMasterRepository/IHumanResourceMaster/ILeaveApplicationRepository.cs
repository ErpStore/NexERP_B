using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IHumanResourceMaster
{
    public interface ILeaveApplicationRepository : IRepository<LeaveApplication>
    {
        Task<string> GenerateLeaveApplicationNoAsync();
        Task<List<LeaveApplication>> GetPendingByLevelAsync(string Level);
    }
}
