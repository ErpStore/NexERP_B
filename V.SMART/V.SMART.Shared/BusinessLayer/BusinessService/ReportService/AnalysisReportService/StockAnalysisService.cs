using AutoMapper;
using FastReport;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel;
using Microsoft.Data.SqlClient;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.AnalysisReportService
{
    public class StockAnalysisService : IStockAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IReportExecutor _report;


        public StockAnalysisService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _report = report;
        }


        // 🔹 Stores
        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
            => await _commonService.GetAllActiveStoresAsync();

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
          => await _commonService.GetItemByItemIdAsync(itemId);

        public async Task<List<Category>> GetAllActiveCategoriesAsync()
                => await _commonService.GetAllActiveCategoriesAsync();

            public async Task<List<Category>> GetAllActiveCategoriesAsync(int? storeId)
            {
                try
                {
                    IQueryable<StockAdd> query = _unitOfWork.StockAdds
                                                .GetQueryable()
                                                .AsNoTracking()
                                                .AsSplitQuery()
                                                .Include(s => s.Item)
                                                .ThenInclude(i => i.Category);

                    if (storeId.HasValue && storeId.Value > 0)
                        query = query.Where(s => s.StoreId == storeId.Value);

                    var categories = await query
                        .Select(s => new Category
                        {
                            CategoryCode = s.Item.Category.CategoryCode,
                            CategoryName = s.Item.Category.CategoryName
                        })
                        .Distinct()
                        .OrderBy(c => c.CategoryName)
                        .ToListAsync();

                    return categories;

                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"GetAllActiveCategoriesAsync ");
                    return null;
                }
            }


            public async Task<List<ItemVM>> GetStockItemsAsync(int? storeid, int? categorycode)
            {
                try
                {
                    IQueryable<StockAdd> query = _unitOfWork.StockAdds
                                                .GetQueryable()
                                                .AsNoTracking()
                                                .AsSplitQuery()
                                                .Include(s => s.Item)
                                                .ThenInclude(i => i.Category);

                    if (storeid.HasValue && storeid > 0)
                        query = query.Where(s => s.StoreId == storeid.Value);


                    if (categorycode.HasValue && categorycode > 0)
                        query = query.Where(s => s.Item.CategoryCode == categorycode.Value);

                    var items = await query
                        .Select(s => new ItemVM
                        {
                            ItemId = s.Item.ItemId,
                            ItemCode = s.Item.ItemCode,
                            ItemName = s.Item.ItemName
                        })
                        .Distinct()
                        .OrderBy(i => i.ItemCode)
                        .ToListAsync();

                    return items;

                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"GetStockFromSelectedCategoryAsync Category({categorycode})");
                    return null;
                }
            }

            public async Task<List<StockAnalysisVM>> GetStockAnalysisAsync(int Storeid, int? category, DateTime fromDate,DateTime toDate)
            {
            try
            {
                return await _report.ExecuteAsync<StockAnalysisVM>(
                "Sp_StockAnalysis",
                new SqlParameter("@StoreId", Storeid),
                new SqlParameter("@CategoryCode", category),
                new SqlParameter("@FromDate", fromDate),
                new SqlParameter("@ToDate", toDate));
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
