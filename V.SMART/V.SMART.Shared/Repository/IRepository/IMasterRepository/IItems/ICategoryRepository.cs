using V.SMART.Shared.Data.Master.Inventory;

namespace V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<List<Category>> GetByNameFilterAsync(string name);
        Task<List<Category>> GetUsedCategoriesAsync();
    }

}
