using V.SMART.Shared.Data.Master.Inventory_module;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItems
{
    public interface IAssemblyRepository : IRepository<AssmblyDef>
    {
        Task<AssmblyDef?> GetByAssmblyIdAsync(int assmblyId);

        Task<bool> ExistsAsync(int assemblyId, int itemId);
        Task<int> GetNextSlNoAsync(int assemblyId);

    }
}
