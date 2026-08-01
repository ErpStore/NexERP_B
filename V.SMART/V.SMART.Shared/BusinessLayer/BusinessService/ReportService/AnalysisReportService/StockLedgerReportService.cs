using DocumentFormat.OpenXml.Spreadsheet;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ReportService.AnalysisReportService
{
    public class StockLedgerReportService : IStockLedgerReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IReportExecutor _report;

        public StockLedgerReportService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs, IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _report = report;
        }

        public async Task<List<ItemVM>> GetAllActiveItemsAsync()
            =>await _commonService.GetAllActiveItemsAsync();
        public async Task<List<StockLedgerReportVM>> GetStockLedgerList(int itemid, int storeid, DateTime fromDate,DateTime toDate)
        {
            try
            {
                return await _report.ExecuteAsync<StockLedgerReportVM>(
                "Sp_StockLedger",
                new SqlParameter("@ItemId", itemid),
                new SqlParameter("@StoreId", storeid),
                new SqlParameter("@fromDate", fromDate),
                new SqlParameter("@toDate", toDate));
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
            => await _commonService.GetAllActiveStoresAsync();

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
          => await _commonService.GetItemByItemIdAsync(itemId);

        public async Task<List<ItemVM>> GetStockItemsAsync(int? storeid,DateTime fromDate,DateTime toDate)
        {
            try
            {
                //Stock Add Query
                IQueryable<StockAdd> stockAddQuery = _unitOfWork.StockAdds
                                            .GetQueryable()
                                            .AsNoTracking()
                                            .AsSplitQuery()
                                            .Include(s => s.Item)
                                            .Where(s => s.AddDate >= fromDate && s.AddDate <= toDate);

                // Stock Issue Query
                IQueryable<StockIssue> stockIssueQuery = _unitOfWork.StockIssues
                    .GetQueryable()
                    .AsNoTracking()
                    .Include(s => s.Item)
                    .Where(s => s.IssueDate >= fromDate && s.IssueDate <= toDate);


                if (storeid.HasValue && storeid > 0)
                    //query = query.Where(s => s.StoreId == storeid.Value);
                    stockAddQuery = stockAddQuery.Where(s => s.StoreId == storeid.Value);
                    stockIssueQuery = stockIssueQuery.Where(s => s.StoreId == storeid.Value);

                // Items from Stock Add
                var stockAddItems = stockAddQuery
                    .Select(s => new ItemVM
                    {
                        ItemId = s.Item.ItemId,
                        ItemCode = s.Item.ItemCode,
                        ItemName = s.Item.ItemName,
                        MeasureUnit = s.Item.MeasureUnit
                    });

                        // Items from Stock Issue
                        var stockIssueItems = stockIssueQuery
                            .Select(s => new ItemVM
                            {
                                ItemId = s.Item.ItemId,
                                ItemCode = s.Item.ItemCode,
                                ItemName = s.Item.ItemName,
                                MeasureUnit = s.Item.MeasureUnit
                            });

                        // Combine both
                        var items = await stockAddItems
                            .Union(stockIssueItems)
                            .Distinct()
                            .OrderBy(i => i.ItemCode)
                            .ToListAsync();

                        return items;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetStockFromSelectedStoreAsync Store({storeid}) from ({fromDate}) to ({toDate})");
                return null;
            }
        }

    }
 }
