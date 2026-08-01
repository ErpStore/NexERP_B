using V.SMART.Shared.Data.Master.Inventory_module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems
{
    public interface IAssemblyDefLabourRepository : IRepository<AssemblyDefLabour>
    {
        Task<AssemblyDefLabour?> GetByAssmblyIdAsync(int assmblyId);

        Task<bool> ExistsAsync(int assemblyId, int itemId);
        Task<int> GetNextSlNoAsync(int assemblyId);
    }
}
