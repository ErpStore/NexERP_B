using V.SMART.Shared.Data.Master.Inventory;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository
{
    public interface IHSNMasterRepository : IRepository<HSNMaster>
    {
        void AddRange(IEnumerable<HSNMaster> entities);

        Task<List<string>> GetExistingHSNCodesAsync(List<string> hsnCodes);
    }
}
