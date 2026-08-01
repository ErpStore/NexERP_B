using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository
{
    public interface IItemRepository : IRepository<Item>
    {
        Task<List<Item>> GetItemsByCategoryCodeAsync(int categoryCode);
        Task<int> GetItemIdByCodeAsync(string itemCode);
        Task UpdateRateByItemIdAsync(int itemId, float rate);
        Task<List<ItemVM>> SearchItemsAsync(string keyword);

        Task<List<ItemVM>> SearchItemsByCategoryAsync(int? categoryCode, string keyword);
    }
}
