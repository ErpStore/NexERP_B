using DocumentFormat.OpenXml.InkML;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository.IMasterRepository.IItemRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace V.SMART.Shared.Repository.MasterRepository.ItemsRepository
{
    public class ItemRepository : Repository<Item>, IItemRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        //private readonly TenantDbContextFactory _dbFactory;

        public ItemRepository(
               ApplicationDbContext db,
               ILoggingService loggingService,
               CurrentUserService currentUserService)
               //TenantDbContextFactory dbFactory)
       : base(db, loggingService)
        {
            _db = db;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            //_dbFactory = dbFactory;
        }

        public async Task<List<Item>> GetItemsByCategoryCodeAsync(int categoryCode)
        {
            try
            {
                return await _db.Item
                    .AsNoTracking()
                    .Where(i => i.CategoryCode == categoryCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ItemRepository.GetItemsByCategoryCodeAsync method");
                return new List<Item>(); // Or return null based on your use case
            }
        }

        public async Task UpdateRateByItemIdAsync(int itemId, float rate)
        {
            try
            {
                await _db.Database.ExecuteSqlRawAsync(
                    "UPDATE Item SET Rate = {0} WHERE ItemId = {1}", rate, itemId);

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Item Rate Updation",
                    action: "Item Rate modified",
                    additionalInfo: $"ItemId:{itemId} and Rate:{rate}"
                );
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in ItemRepository.UpdateRateByItemIdAsync method");
            }
        }


        public async Task<int> GetItemIdByCodeAsync(string itemCode)
        {
            return await _db.Item
                .Where(x => x.ItemCode == itemCode)
                .Select(x => x.ItemId)
                .FirstOrDefaultAsync();
        }

        public override async Task<Item?> GetAsync(int id)
        {
            return await _db.Item
                .Include(i => i.Category)
                .Include(i => i.RawMaterial)
                .Include(i => i.UOM)
                .FirstOrDefaultAsync(i => i.ItemId == id);
        }

        public async Task<List<ItemVM>> SearchItemsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return new List<ItemVM>();

            keyword = keyword.Trim();

            return await _db.Item
                .AsNoTracking()
                .Where(i => i.ItemCode.Contains(keyword) || i.ItemName.Contains(keyword))
                .OrderBy(i => i.ItemName)
                .Select(i => new ItemVM
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    MeasureUnit = i.MeasureUnit,
                    HSNCode = i.HSNCode,
                    Rate = i.Rate,
                    SACCode = i.SACCode,
                })
                .Take(50) 
                .ToListAsync();
        }



        public async Task<List<ItemVM>> SearchItemsByCategoryAsync(int? categoryCode, string keyword)
        {
            if (categoryCode == null || categoryCode == 0)
                return new List<ItemVM>();

            //return await _db.Item
            //    .Where(i => i.CategoryCode == categoryCode &&
            //                (i.ItemCode.Contains(keyword) || i.ItemName.Contains(keyword)))
            //    .Select(i => new ItemVM
            //    {
            //        ItemId = i.ItemId,
            //        ItemCode = i.ItemCode,
            //        ItemName = i.ItemName,
            //        HSNCode = i.HSNCode,
            //        MeasureUnit = i.MeasureUnit
            //    })
            //    .Take(50)
            //    .ToListAsync();
            return await (
                from i in _db.Item
                join s in _db.StockAdd
                    on i.ItemId equals s.ItemId
                where i.CategoryCode == categoryCode
                      && (i.ItemCode.Contains(keyword) || i.ItemName.Contains(keyword))
                      && s.BalQty > 0 
                select new ItemVM
                {
                    ItemId = i.ItemId,
                    ItemCode = i.ItemCode,
                    ItemName = i.ItemName,
                    HSNCode = i.HSNCode,
                    MeasureUnit = i.MeasureUnit
                })
                .Distinct()
                .Take(50)
                .ToListAsync();
        }


    }
}
