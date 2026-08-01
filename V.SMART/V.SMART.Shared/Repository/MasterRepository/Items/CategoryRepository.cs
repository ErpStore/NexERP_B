using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;

        public CategoryRepository(
            ApplicationDbContext db,
            ILoggingService loggingService,
            CurrentUserService currentUserService) : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
        }

        public async Task<List<Category>> GetByNameFilterAsync(string name)
        {
            return await _db.Category
                .Where(c => c.CategoryName.Contains(name))
                .ToListAsync();
        }

        public async Task<List<Category>> GetUsedCategoriesAsync()
        {
            try
            {
                var result = await (from i in _db.Item
                                    join c in _db.Category on i.CategoryCode equals c.CategoryCode
                                    select new Category
                                    {
                                        CategoryCode = c.CategoryCode,
                                        CategoryName = c.CategoryName
                                    }).Distinct().ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ItemRepository GetUsedCategoriesAsync()");
                return new List<Category>();
            }
        }

    }
}
