using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository
{
    public interface IGroupingSubRepository : IRepository<GroupingSub>
    {
        Task<List<ProcessFlowChart>> GetProcessFlowChartsByGroupingId(int groupingId);
    }
}
