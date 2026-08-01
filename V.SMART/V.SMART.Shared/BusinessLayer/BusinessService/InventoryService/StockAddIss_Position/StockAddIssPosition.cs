using AutoMapper;
using FastReport.Data;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IStockAddIss_Position;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Pages.Master_Module_pages.Store_Pages;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.StockAddVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService.StockAddIss_Position
{
    public class StockAddIssPosition : IStockAddIssPosition
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManager;

        private readonly IStockManagerService _stockManagerService;
        private readonly IReportExecutor _reportExecutor;

        public StockAddIssPosition(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            IStockManagerService stockManagerService,
            ILoggingService logs,
            IMapper mapper,
            IStockManagerService stockManager, IReportExecutor reportExecutor)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;
            _stockManager = stockManager;
            _reportExecutor = reportExecutor;
        }


        // 🔹 Sores
        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
            => await _commonService.GetAllActiveStoresAsync();

        //Attachments
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
          => await _commonService.GetItemByItemIdAsync(itemId);

        //Correspond
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
          => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        //Category
        public async Task<List<Category>> GetAllActiveCategoriesAsync()
            => await _commonService.GetAllActiveCategoriesAsync();

        public async Task<List<Category>> GetAllActiveCategoriesAsync(int? storeId)
        {
            try
            {
                var query = _unitOfWork.StockAdds
                    .GetQueryable()
                    .AsNoTracking();

                if (storeId.HasValue && storeId.Value > 0)
                    query = query.Where(s => s.StoreId == storeId.Value);

                return await query
                    .GroupBy(s => new
                    {
                        s.Item.Category.CategoryCode,
                        s.Item.Category.CategoryName
                    })
                    .Select(g => new Category
                    {
                        CategoryCode = g.Key.CategoryCode,
                        CategoryName = g.Key.CategoryName
                    })
                    .OrderBy(x => x.CategoryName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllActiveCategoriesAsync");
                return new List<Category>();
            }
        }


        public async Task<List<ItemVM>> GetStockItemsAsync(int? storeid, int? categorycode)
        {
            try
            {
                var query = _unitOfWork.StockAdds
                    .GetQueryable()
                    .AsNoTracking();

                if (storeid.HasValue && storeid.Value > 0)
                    query = query.Where(s => s.StoreId == storeid.Value);

                if (categorycode.HasValue && categorycode.Value > 0)
                    query = query.Where(s => s.Item.CategoryCode == categorycode.Value);

                return await query
                    .Select(s => new ItemVM
                    {
                        ItemId = s.ItemId,
                        ItemCode = s.Item.ItemCode,
                        ItemName = s.Item.ItemName
                    })
                    .Distinct()
                    .OrderBy(i => i.ItemCode)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"GetStockItemsAsync Store({storeid}) Category({categorycode})");

                return new List<ItemVM>();
            }
        }

        public async Task<List<StockAddVM>> GetStockDynamicAsync(int? storeId, int? categoryCode, int? itemId)
        {
            IQueryable<StockAdd> query = _unitOfWork.StockAdds
                                                    .GetQueryable()
                                                    .AsNoTracking();

            query = query
                .Include(s => s.Item)
                    .ThenInclude(i => i.Category)
                 .Include(i => i.Item).ThenInclude(a=>a.ItemCustomerAssigns).ThenInclude(c=>c.Customer)
                .Include(s => s.Store)
                .Include(s => s.Screen);

            if (itemId > 0)
                query = query.Where(s => s.ItemId == itemId);

            if (storeId > 0)
                query = query.Where(s => s.StoreId == storeId);

            if (categoryCode > 0)
                query = query.Where(s => s.Item.CategoryCode == categoryCode);

            var list = await query.ToListAsync();

            var vmList = _mapper.Map<List<StockAddVM>>(list);

            int i = 1;
            foreach (var item in vmList)
                item.SlNo = i++;

            return vmList;



        }

        public async Task<List<StockPositionSummaryVM>> GetStockSummaryAsync( int storeId,int categoryCode,int itemId, bool IsDetailsView)
        {
            try
            {
                var result = await _reportExecutor.ExecuteAsync<StockPositionSummaryVM>(
                            "SP_StockPositionSummary",
                            new SqlParameter("@StoreId", storeId),
                            new SqlParameter("@CategoryCode", categoryCode),
                            new SqlParameter("@ItemId", itemId),
                            new SqlParameter("@IsDetailsView", IsDetailsView)
                        );

                return result?.ToList() ?? new List<StockPositionSummaryVM>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetStockFromSelectedCategoryAsync)");
                return null;
            }
        }




        #region For Stock Position(Internal and external)
        public async Task<List<StockAddVM>> GetStockDynamicAsync(int? categoryCode, int? itemId)
        {
            IQueryable<StockAdd> query = _unitOfWork.StockAdds
                                                    .GetQueryable()
                                                    .AsNoTracking();

            query = query
                .Include(s => s.Item)
                    .ThenInclude(i => i.Category)
                .Include(s => s.Screen);

            if (itemId > 0)
                query = query.Where(s => s.ItemId == itemId);

            if (categoryCode > 0)
                query = query.Where(s => s.Item.CategoryCode == categoryCode);

            var list = await query.ToListAsync();

            var vmList = _mapper.Map<List<StockAddVM>>(list);

            return vmList;



        }

        public async Task<List<ItemVM>> GetStockItemsAsync(int? categorycode)
        {
            try
            {
                IQueryable<Item> query = _unitOfWork.ItemRepositories
                                                .GetQueryable()
                                                .AsNoTracking()
                                                .AsSplitQuery();

                if (categorycode.HasValue && categorycode.Value > 0)
                {
                    query = query.Where(s => s.CategoryCode == categorycode.Value);
                }

                var items = await query
                    .Select(s => new
                    {
                        s.ItemId,
                        s.ItemCode,
                        s.ItemName
                    })
                    .GroupBy(x => new { x.ItemId, x.ItemCode, x.ItemName })
                    .Select(g => new ItemVM
                    {
                        ItemId = g.Key.ItemId,
                        ItemCode = g.Key.ItemCode,
                        ItemName = g.Key.ItemName
                    })
                    .OrderBy(i => i.ItemCode)
                    .ToListAsync();

                return items;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetStockItemsAsync Category({categorycode})");
                return null;
            }
        }

        public async Task<(List<StockAddVM>, List<StockAddVM>)> GetStockAsync(List<int> itemIds, bool StockBaseRate)
        {

            try
            {
                var items = await _unitOfWork.ItemRepositories
                .GetQueryable()
                .AsNoTracking()
                .Where(i => itemIds.Contains(i.ItemId))
                .Select(i => new
                {
                    i.ItemId,
                    ItemDescription = i.ItemCode + " : " + i.ItemName,
                    i.Rate
                })
                .ToListAsync();

                // 2️⃣ Stores
                var stores = await _unitOfWork.Stores
                    .GetQueryable()
                    .AsNoTracking()
                    .Select(s => new StockAddVM
                    {
                        StoreId = s.StoreId,
                        AddStoreName = s.StoreName
                    })
                    .ToListAsync();


                var stockLedger = await _unitOfWork.StockAdds
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(s => itemIds.Contains(s.ItemId))
                    .Select(s => new
                    {
                        s.ItemId,
                        s.StoreId,
                        s.BalQty,
                        s.UnitPrice
                    })
                    .ToListAsync();
                var stockQty = stockLedger
                    .GroupBy(x => new { x.ItemId, x.StoreId })
                    .Select(g => new
                    {
                        g.Key.ItemId,
                        g.Key.StoreId,
                        BalQty = Math.Round(g.Sum(x => x.BalQty), 2)
                    })
                    .ToList();

                var rows = new List<StockAddVM>();

                foreach (var item in items)
                {
                    var row = new StockAddVM
                    {
                        ItemId = item.ItemId,
                        ItemDescription = item.ItemDescription
                    };

                    double totalQty = 0;
                    double totalAmount = 0;

                    foreach (var store in stores)
                    {
                        decimal qty = stockQty
                            .FirstOrDefault(x => x.ItemId == item.ItemId && x.StoreId == store.StoreId)
                            ?.BalQty ?? 0;

                        row.StoreQty[store.StoreId] = (double)qty;
                        totalQty += (double)qty;

                        decimal rate;

                        if (StockBaseRate)
                        {
                            var storeLedgerRows = stockLedger
                                .Where(x => x.ItemId == item.ItemId && x.StoreId == store.StoreId);

                            decimal storeQty = storeLedgerRows.Sum(x => x.BalQty);

                            rate = storeQty > 0
                                ? storeLedgerRows.Sum(x => x.UnitPrice * x.BalQty) / storeQty
                                : 0;
                        }
                        else
                        {
                            rate = item.Rate;
                        }

                        double storeValue = (double)(qty * rate);
                        row.StoreValue[store.StoreId] = storeValue;
                        totalAmount += storeValue;
                    }

                    row.TotalQty = totalQty;
                    row.TotalAmount = totalAmount;

                    rows.Add(row);
                }


                return (stores, rows);

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetStockAsync");
                return (new List<StockAddVM>(), new List<StockAddVM>());
            }
        }
        #endregion
    }

}
