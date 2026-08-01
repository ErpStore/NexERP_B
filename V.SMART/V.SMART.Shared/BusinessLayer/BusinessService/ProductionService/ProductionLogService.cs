using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using OfficeOpenXml.Drawing.Slicer.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;




namespace V.SMART.Shared.BusinessLayer.BusinessService.ProductionService
{
    public class ProductionLogService : IProductionLogService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public ProductionLogService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            IStockManagerService stockManagerService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;
        }

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        //Users
        public async Task<List<string>> GetAllUsersAsync()
            => await _commonService.GetAllActiveUserNamesAsync();

        //Stock
        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId)
        {
            return await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);
        }

        //Stores
        public async Task<List<Store>> GetAllIssueStoresAsync()
        {
            var result = await _commonService.GetAllIssueStoresAsync();
            return result.ToList();
        }

        public async Task<List<Store>> GetAllAddStoresAsync()
        {
            var result = await _commonService.GetAllAddStoresAsync();
            return result.ToList();
        }

        public async Task<List<ShiftAllocation>> GetAllShiftAsync()
        {
            var result = await _commonService.GetAllShiftsAsync();
            return result.ToList();
        }

        //Machines
        public Task<List<Machine>> GetAllActiveMachinesAsync()
            => _commonService.GetAllMachineAsync();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        public async Task<decimal> GetAvailableStockByItemIdAsync(int itemId, int? storeId)
            => await _stockManagerService.GetStockForItemAsync(itemId, storeId);

        public async Task<decimal> GetAvailableStockByItemIdAndRcAndScreenAsync(int itemId,int? storeId,int rcSubId)
        {
            try
            {
                decimal availableStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(x =>
                        x.ItemId == itemId &&
                        x.StoreId == storeId &&
                        x.RcSubID == rcSubId)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                return availableStock;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error in GetAvailableStockByItemIdAsync | ItemId={itemId}, StoreId={storeId}, rcSubId={rcSubId}");
                throw;
            }
        }




        // 🔹 Production Log operations


        public async Task<int?> GetPendingProductionLogIdAsync(string operatorName)
        {
            try
            {
                return await _unitOfWork
                    .ProductionLogs
                    .GetQueryable()
                    .Where(x => x.Operator == operatorName)
                    .OrderByDescending(x => x.LogId)
                    .Where(x => (x.AccQty + x.RejQty + x.RewQty + x.ReturnQty) == 0)
                    .Select(x => (int?)x.LogId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,"Error getting pending production log.");
                return null;
            }
        }

        public async Task<ProductionLogVM?> GetActiveProductionLogAsync(int rcId, int processId, int screenCode)
        {
            try
            {
                var logId = await _unitOfWork.ProductionLogs.GetQueryable()
                    .Where(x =>
                        x.RCId == rcId &&
                        x.ProcessId == processId && 
                        (x.AccQty + x.RejQty + x.RewQty+ x.ReturnQty) == 0)
                    .OrderByDescending(x => x.LogId)
                    .Select(x => (long?)x.LogId)
                    .FirstOrDefaultAsync();

                if (!logId.HasValue)
                    return null;

                var logDetail = await GetProductionLogByLogIdAsync((int)logId, screenCode);

                return logDetail == null ? null : _mapper.Map<ProductionLogVM>(logDetail);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error in GetActiveProductionLogAsync | RCId: {rcId}, ProcessId: {processId}");
                throw new Exception("Error retrieving active Production Log. Please try again later.", ex);
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteProductionLog(long logId, int screenCode)
        {
            try
            {
                var prodLog = await _unitOfWork.ProductionLogs
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.LogId == logId);

                var inspection = await _unitOfWork.IncomingInspections.GetQueryable().Where(x => x.ProductionLogId == logId).Select(x => new
                {
                    x.Id,
                    InspectNo = x.InspectNo + x.Suffix
                }).FirstOrDefaultAsync();

                if (inspection?.Id > 0)
                {
                    return (false, $"Sorry! Unable to delete this Production Log because an Inspection {inspection.InspectNo}  exists for this log.");
                }


                if (prodLog == null)
                    return (true, "Production Log can be safely deleted.");

                var currentProcess = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.RCSubId == prodLog.RCProcessId);

                if (currentProcess == null)
                    return (true, "Route Card process not found. Allow delete.");

                var ProdIds = await _unitOfWork.ProductionLogs
                          .GetQueryable()
                          .Where(s => s.LogId == logId)
                          .Select(s => s.LogId)
                          .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                                .Where(sa =>
                                    ProdIds.Contains(sa.SubItemRefID) &&
                                    sa.ScreenCode == screenCode &&
                                    sa.BalQty < sa.AddQty)
                                .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Production Log. Some sub-items have already been transacted/issued.");


                var nextProcess = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Where(x => x.RCId == currentProcess.RCId &&
                    x.SeqNo > currentProcess.SeqNo &&
                    !x.IsProcessSkip)
                    .OrderBy(x => x.SeqNo)
                    .FirstOrDefaultAsync();

                if (nextProcess == null)
                    return (true, "Last process. Safe to delete.");



                var previousRejectedQty = await _unitOfWork.RouteCardSubs
                                      .GetQueryable()
                                      .Where(x => x.RCId == currentProcess.RCId &&
                                                  x.SeqNo <= currentProcess.SeqNo &&
                                                  !x.IsProcessSkip)
                                      .SumAsync(x => (decimal?)x.RejQty) ?? 0;

                if ((nextProcess.BalQty + previousRejectedQty + nextProcess.AccQty + nextProcess.RejQty) == nextProcess.TotalQty)
                {
                    return (true, "Next process not started. Safe to delete.");
                }



                return (false, "Qty already issued to next process. You cannot delete this log.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error validating Production Log delete");
                return (false, "Validation failed while deleting Production Log.");
            }
        }

        public async Task<bool> DeleteProductionLogByLogId(long logId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var prodLog = await _unitOfWork.ProductionLogs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.LogId == logId);

                if (prodLog == null)
                    return false;

                var (canDelete, message) = await CanDeleteProductionLog(logId, screenCode);
                if (!canDelete)
                    throw new Exception(message);

                RouteCardSub rcProcess = null;

                if (prodLog.RCProcessId.HasValue)
                {
                    rcProcess = await _unitOfWork.RouteCardSubs
                        .GetQueryable()
                        .Include(x => x.RouteCard)
                        .FirstOrDefaultAsync(x => x.RCSubId == prodLog.RCProcessId.Value);
                }

                bool hasOutput = (prodLog.AccQty + prodLog.RejQty + prodLog.RewQty + prodLog.ReturnQty) > 0;

                // ----------------------------
                // REVERSE PROCESS QTY
                // ----------------------------
                if (rcProcess != null)
                {
                    if (hasOutput)
                    {
                        if (prodLog.AccQty + prodLog.RejQty + prodLog.RewQty > 0)
                        {
                            // ----------------------------
                            // STOCK REVERSAL
                            // ----------------------------
                            var stockAdds = await _unitOfWork.StockAdds
                                .GetQueryable()
                                .Where(x => x.SubItemRefID == prodLog.LogId && x.ScreenCode == screenCode)
                                .ToListAsync();


                            foreach (var add in stockAdds)
                                await _stockManagerService.DeleteStockAddAsync(add.AddId);
                        }

                        if (prodLog.ReturnQty > 0)
                        {
                            var stockIssues = await _unitOfWork.StockIssues
                                .GetQueryable()
                                .Where(x => x.SubItemRefID == prodLog.LogId)
                                .ToListAsync();

                            foreach (var issue in stockIssues)
                                await _stockManagerService.DeleteStockIssueAsync(issue.IssueId);
                        }

                        rcProcess.AccQty = Math.Max(0, rcProcess.AccQty - prodLog.AccQty);
                        rcProcess.RejQty = Math.Max(0, rcProcess.RejQty - prodLog.RejQty);
                        rcProcess.RewQty = Math.Max(0, rcProcess.RewQty - prodLog.RewQty);

                        rcProcess.NextProcessQty = Math.Max(0, rcProcess.NextProcessQty - prodLog.AccQty);

                        if (prodLog.ReturnQty > 0)
                        {
                            rcProcess.IssuedQty += prodLog.ReturnQty;

                            if (rcProcess.IssuedQty > rcProcess.TotalQty)
                            {
                                rcProcess.IssuedQty = rcProcess.TotalQty;
                            }

                            //rcProcess.BalQty = Math.Max(0, rcProcess.BalQty - prodLog.ReturnQty);
                            //if (rcProcess.BalQty > rcProcess.TotalQty)
                            //{
                            //    rcProcess.BalQty = rcProcess.TotalQty;
                            //}
                        }
                        if (prodLog.AccQty > 0)
                        {
                            rcProcess.IssuedQty += prodLog.AccQty;

                            if (rcProcess.IssuedQty > rcProcess.TotalQty)
                            {
                                rcProcess.IssuedQty = rcProcess.TotalQty;
                            }
                        }
                        if (prodLog.RewQty > 0)
                        {
                            rcProcess.IssuedQty += prodLog.RewQty;

                            if (rcProcess.IssuedQty > rcProcess.TotalQty)
                            {
                                rcProcess.IssuedQty = rcProcess.TotalQty;
                            }
                            await UpdateRouteCardSubAsync(prodLog.RCPSubId!.Value, prodLog.ItemIdIn.GetValueOrDefault(), prodLog.RewProcessId.GetValueOrDefault(), 0, prodLog.RewQty, prodLog.RCProcessId.Value);



                        }

                        if (prodLog.RejQty > 0)
                        {
                            rcProcess.IssuedQty += prodLog.RejQty;

                            var current = await _unitOfWork.RouteCardSubs.GetQueryable()
                               .FirstOrDefaultAsync(x => x.RCSubId == prodLog.RCProcessId.Value);

                            // Current process + all next processes
                            var routeSubs = await _unitOfWork.RouteCardSubs.GetQueryable()
                                .Where(x =>
                                    x.RCId == current.RCId &&
                                    x.ItemIdIn == prodLog.ItemIdIn &&
                                    !x.IsProcessSkip &&
                                    x.SlNo >= current.SlNo)
                                .OrderBy(x => x.SlNo)
                                .ToListAsync();

                            foreach (var sub in routeSubs)
                            {
                                if (sub.RCSubId == current.RCSubId)
                                    continue;

                                sub.BalQty += prodLog.RejQty;

                                await _unitOfWork.RouteCardSubs.UpdateAsync(sub);

                            }

                            if (rcProcess.IssuedQty > rcProcess.TotalQty)
                            {
                                rcProcess.IssuedQty = rcProcess.TotalQty;
                            }

                        }

                        // Reset log qty
                        prodLog.AccQty = 0;
                        prodLog.RejQty = 0;
                        prodLog.RewQty = 0;
                        prodLog.ReturnQty = 0;

                        await _unitOfWork.ProductionLogs.UpdateAsync(prodLog);

                    }
                    else
                    {
                        rcProcess.IssuedQty = Math.Max(0, rcProcess.IssuedQty - prodLog.InputQty);
                        rcProcess.BalQty += prodLog.InputQty;

                        if (rcProcess.BalQty > rcProcess.TotalQty)
                        {
                            rcProcess.BalQty = rcProcess.TotalQty;
                        }

                        if (prodLog.QtyOut > 0)
                        {
                            var stockIssues = await _unitOfWork.StockIssues
                                .GetQueryable()
                                .Where(x => x.SubItemRefID == prodLog.LogId)
                                .ToListAsync();

                            foreach (var issue in stockIssues)
                                await _stockManagerService.DeleteStockIssueAsync(issue.IssueId);
                        }

                        await _unitOfWork.ProductionLogs.DeleteAsync(prodLog);
                    }

                    // ----------------------------
                    // PROCESS STATUS RECALC
                    // ----------------------------
                    rcProcess.ProcessStatus = await GetProcessStatusAsync(rcProcess);
                    await _unitOfWork.RouteCardSubs.UpdateAsync(rcProcess);

                    // ----------------------------
                    // ROUTE CARD STATUS RECALC
                    // ----------------------------
                    if (prodLog.RCId.HasValue && prodLog.RCId > 0)
                    {
                        var rc = await _unitOfWork.RouteCards.GetAsync(prodLog.RCId.Value);

                        if (rc != null)
                        {

                            bool isCompleted =
                                rcProcess.IsFinalProcess &&
                                rcProcess.IssuedQty ==
                                (rcProcess.AccQty + rcProcess.RejQty) &&
                                rcProcess.BalQty == 0;

                            bool isStarted =
                                rcProcess.IssuedQty > 0 ||
                                rcProcess.AccQty > 0 ||
                                rcProcess.RejQty > 0 ||
                                rcProcess.RewQty > 0;

                            rc.RcStatus = isCompleted
                                ? (byte)2
                                : isStarted
                                    ? (byte)1
                                    : (byte)0;

                            await _unitOfWork.RouteCards.UpdateAsync(rc);
                        }
                    }
                }


                // SINGLE SAVE
                await _unitOfWork.SaveAsync();

                // COMMIT
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                // ROLLBACK EVERYTHING
                await transaction.RollbackAsync();

                await _logs.LogDeveloperError(ex, $"Error deleting Production Log {logId}");
                throw;
            }
        }

        public async Task<(List<ProductionLogVM> logVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber,int pageSize,Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.ProductionLogs.GetQueryable()
                    .Include(j => j.IssueStore)
                    .Include(j => j.AddStore)
                    .Include(j => j.Process)
                    .Include(j => j.RouteCard)
                    .Include(j => j.RouteCardProcess)
                    .Include(j => j.ItemIn)
                    .Include(j => j.ItemOut)
                    .Include(j => j.Machine)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var f in filters)
                    {
                        query = SCNFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                    }
                }

                var total = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.LogId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<ProductionLogVM>>(list);

                return (vmList, total);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error in SearchWithDynamicFilterAsync | Page: {pageNumber}, Size: {pageSize}");
                throw new Exception("Error retrieving Production Logs. Please try again later.",ex);
            }
        }


        public static class SCNFilterBuilder
        {
            public static IQueryable<ProductionLog> ApplyFilter(
                IQueryable<ProductionLog> query,
                string field,
                object value)
            {

                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString()!.Trim();

                switch (field)
                {
                    case "LogNo":
                        {
                            string part1 = val;
                            string part2 = "";

                            int slashIndex = val.IndexOf('/');
                            if (slashIndex > -1)
                            {
                                part1 = val[..slashIndex].Trim();
                                part2 = val[(slashIndex + 1)..].Trim();
                            }

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.LogNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2)));
                        }
                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(value.ToString()));
                    case "ItemCodeIn":
                        return query.Where(x => x.ItemIn.ItemCode.Contains(value.ToString()));
                    case "ItemNameIn":
                        return query.Where(x => x.ItemIn.ItemName.Contains(value.ToString()));

                    case "ItemCodeOut":
                        return query.Where(x => x.ItemOut.ItemCode.Contains(value.ToString()));
                    case "ItemNameOut":
                        return query.Where(x => x.ItemOut.ItemName.Contains(value.ToString()));

                    case "RcNo":
                        {
                            string part1 = val;
                            string part2 = "";

                            int slashIndex = val.IndexOf('/');
                            if (slashIndex > -1)
                            {
                                part1 = val[..slashIndex].Trim();
                                part2 = val[(slashIndex + 1)..].Trim();
                            }

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.RouteCard.RCNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.RouteCard.Suffix.Contains(part2)));
                        }

                    case "Process":
                        return query.Where(x => x.Process.ProcessName.Contains(val));

                    case "Machine":
                        return query.Where(x => x.Machine.MachineName.Contains(val));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.LogDate >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.LogDate <= toDate);
                        break;
                }

                return query;
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteProductionSCNAssyAsync(int scnId, int screenCode)
        {
            try
            {
                var scnSubIds = await _unitOfWork.ProductionSCNAssySubs
                                   .GetQueryable()
                                   .Where(s => s.SCNId == scnId)
                                   .Select(s => s.SCNSubId)
                                   .ToListAsync();

                if (!scnSubIds.Any())
                    return (true, "Production SCN can be safely deleted (no sub-items found).");

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa =>
                        scnSubIds.Contains(sa.SubItemRefID) &&
                        sa.ScreenCode == screenCode &&
                        sa.BalQty < sa.AddQty)
                    .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Prouction SCN Assembly. Some sub-items have already been transacted/issued.");

                return (true, "Production SCN Assembly can be safely deleted.");

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteProductionSCNAssyAsync for SCNId: {scnId}");
                throw new Exception("Error checking Production SCN Assy delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteProdAssySCNBySCNIdAsync(int scnId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var productionSCNAssy = await _unitOfWork.ProductionSCNAssys
                    .GetQueryable()
                    .Include(e => e.ProductionSCNAssySubs)
                    .FirstOrDefaultAsync(e => e.SCNId == scnId);

                if (productionSCNAssy == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in productionSCNAssy.ProductionSCNAssySubs)
                {

                    await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                    await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                    if (sub.RefReturnSubId > 0)
                    {
                        var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                        await AdjustReturnAssyBalanceAsync(sub.RefReturnSubId, totQty, 0, "Production SCN Assembly delete");
                    }
                }


                var ProductionScnAssy = await _unitOfWork.ProductionSCNAssys.GetAsync(scnId);
                await _unitOfWork.ProductionSCNAssys.DeleteAsync(ProductionScnAssy);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Production SCN Assembly",
                    action: $"Deleted SCN No: {ProductionScnAssy.SCNNo}",
                    additionalInfo: $"SCN Id: {ProductionScnAssy.SCNId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Production SCN Assy: {scnId}");
                throw;
            }
        }

        public async Task<ProductionSCNAssySubVM?> GetSCNAssyItemDetailByScnSubIdAsync(int scnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionSCNAssySubs
                    .GetQueryable()
                    .Where(q => q.SCNSubId == scnSubId)
                    .Select(q => new ProductionSCNAssySubVM
                    {
                        AccQty = q.AccQty,
                        RejQty = q.RejQty,
                        RewQty = q.RewQty,
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production SCN Assy sub item detail for SCNSubId: {scnSubId}");
                throw new InvalidOperationException("Failed to retrieve Production SCN sub-item details.");
            }
        }

        public async Task<List<ProductionSCNAssySubVM>> GetDistinctRefGRNNOsbySCNIdAsync(int SCNId)
        {
            return await _unitOfWork.ProductionSCNAssySubs
                .GetQueryable()
                .Where(s => s.SCNId == SCNId)
                .GroupBy(s => new { s.ProductionReturnAssySubs.ProductionReturnAssy.ReturnNo, s.ProductionReturnAssySubs.ProductionReturnAssy.Suffix, s.ProductionReturnAssySubs.ProductionReturnAssy.ReturnDate })
                .Select(g => new ProductionSCNAssySubVM
                {
                    RefReturnNo = $"{g.Key.ReturnNo}{g.Key.Suffix}",
                    RefReturnDate = g.Key.ReturnDate
                })
                .ToListAsync();
        }


        public async Task<ProductionLogVM?> GetProductionLogByLogIdAsync(long logId, int screenCode)
        {
            try
            {
                // =========================
                // LOAD PRODUCTION LOG
                // =========================
                var entity = await _unitOfWork.ProductionLogs.GetQueryable()
                    .Include(q => q.RouteCardProcess)
                    .Include(q => q.Process)
                    .Include(q => q.Shift)
                    .Include(q => q.Machine)
                    .Include(q => q.Customer)
                    .Include(q => q.ItemIn)
                    .Include(q => q.ItemOut)
                    .Include(q => q.IssueStore)
                    .Include(q => q.AddStore)
                    .Include(q => q.CostCenter)
                    .Include(r=>r.RouteCard)
                    .FirstOrDefaultAsync(q => q.LogId == logId);

                if (entity == null)
                    return null;

                // =========================
                // MAP TO VM
                // =========================
                var logVM = _mapper.Map<ProductionLogVM>(entity);

                // =========================
                // LOAD RM STOCK
                // =========================
                if (logVM.ItemIdOut.HasValue &&
                    logVM.IssueStoreId.HasValue &&
                    entity.RouteCardProcess != null)
                {
                    decimal stock = 0;

                    // =========================
                    // SEQ NO = 1 → DIRECT STORE
                    // =========================
                    if (entity.RouteCardProcess.SeqNo == 1)
                    {
                        stock = await GetAvailableStockByItemIdAsync(
                            logVM.ItemIdOut.Value,
                            logVM.IssueStoreId.Value);
                    }
                    else
                    {
                        var sourceRcSubIds = await GetIssueSourceRCSubIdsAsync(
                            logVM.RCId!.Value,
                            entity.RouteCardProcess.SeqNo.Value,
                            logVM.RCProcessId!.Value);

                        foreach (var rcSubId in sourceRcSubIds)
                        {
                            var availableStock =
                                await GetAvailableStockByItemIdAndRcAndScreenAsync(
                                    logVM.ItemIdOut.Value,
                                    logVM.IssueStoreId.Value,
                                    rcSubId);

                            stock += availableStock;
                        }
                    }

                    logVM.RMStock = stock;
                }

                // =========================
                // MAX ALLOWED QTY (HIERARCHY AWARE)
                // =========================
                if (logVM.RCProcessId.HasValue && logVM.RCProcessId.Value > 0)
                {
                    var currentRouteCardSub = await _unitOfWork.RouteCardSubs
                        .GetQueryable()
                        .FirstOrDefaultAsync(r => r.RCSubId == logVM.RCProcessId.Value);

                    if (currentRouteCardSub != null)
                    {
                        logVM.MaxAllowedQty =
                            await CalculateQtyInAsync(currentRouteCardSub);
                    }
                }

                return logVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"GetProductionLogByLogIdAsync({logId})");

                return null;
            }
        }




        public async Task<bool> CheckIfTransactionsMadeAsync(
                List<int> itemIds,
                int storeId,
                int screenCode,
                List<int> refSubItemIds)
        {
            if (itemIds == null || !itemIds.Any() ||
                storeId <= 0 || screenCode <= 0 ||
                refSubItemIds == null || !refSubItemIds.Any())
                return false;

            try
            {
                // 🔹 Single query to check if any stock AddQty != BalQty
                var anyTransaction = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(s =>
                        itemIds.Contains(s.ItemId) &&
                        s.StoreId == storeId &&
                        s.ScreenCode == screenCode &&
                        refSubItemIds.Contains(s.SubItemRefID) &&
                        s.AddQty != s.BalQty)
                    .AnyAsync();

                return anyTransaction;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error checking transactions for StoreId: {storeId}, ScreenCode: {screenCode}");
                throw new InvalidOperationException("Failed to verify transaction status. Please try again.");
            }
        }

        public async Task<List<ProductionSCNAssySubVM>> GetProdSCNSubBySCNIdAsync(int scnId)
        {
            try
            {
                var subs = await _unitOfWork.ProductionSCNAssySubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.SCNId == scnId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ProductionSCNAssySubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ProductionSCNSub items for SCNId: {scnId}");
                throw new InvalidOperationException("Failed to retrieve Production SCN sub-items. Please try again.");
            }
        }

        public async Task<string> GetLogNumberAsync(string suffix)
        {
            try
            {
                var LastLog = await _unitOfWork.ProductionLogs
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.LogNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (LastLog != null)
                {
                    var parts = LastLog.LogNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating prouction log number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Production Log number.");
            }
        }

        public async Task<List<RouteCardVM>> GetPendingRouteCardsAsync()
        {
            try
            {
                return await _unitOfWork.RouteCards.GetQueryable()
                    .Where(x => x.RcStatus <= 1)
                    .OrderByDescending(x => x.RCId)
                    .Select(x => new RouteCardVM
                    {
                        RCId = x.RCId,
                        RCNo = $"{x.RCNo}{x.Suffix}"
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error loading pending Route Cards.");
                throw new InvalidOperationException("Failed to load pending Route Cards.", ex);
            }
        }



        public async Task<List<Process>> LoadNextHierarchyAsync(int rcId)
        {
            var list = await _unitOfWork.RouteCardSubs.GetQueryable()
                .Where(x => x.RCId == rcId && !x.IsProcessSkip)
                .OrderBy(x => x.SeqNo)
                .ToListAsync();

            if (!list.Any())
                return new List<Process>();

            List<RouteCardSub> hierarchy = new();

            int? currentSeq =
                list.Where(x => x.ProcessStatus != 3)
                    .OrderBy(x => x.SeqNo)
                    .Select(x => x.SeqNo)
                    .FirstOrDefault()
                ??
                list.Where(x => x.BalQty > 0 && x.ProcessStatus != 3)
                    .OrderBy(x => x.SeqNo)
                    .Select(x => x.SeqNo)
                    .FirstOrDefault();

            if (currentSeq == null)
                return new List<Process>();

            while (currentSeq != null)
            {
                var currentSeqProcesses = list
                    .Where(x => x.SeqNo == currentSeq)
                    .ToList();

                if (!currentSeqProcesses.Any())
                    break;

                hierarchy.AddRange(currentSeqProcesses);


                bool canMoveNext = false;

                if (currentSeqProcesses.Count == 1)
                {
                    canMoveNext = currentSeqProcesses[0].NextProcessQty > 0;
                }
                else
                {
                    var minNextQty = currentSeqProcesses.Min(x => x.NextProcessQty);

                    canMoveNext = minNextQty > 0;

                    if (canMoveNext)
                    {
                        hierarchy.RemoveAll(x => x.SeqNo == currentSeq);

                        hierarchy.AddRange(
                            currentSeqProcesses
                                .Where(x => x.NextProcessQty == minNextQty)
                        );
                    }
                }

                if (!canMoveNext)
                    break;

                var nextSeq = list
                    .Where(x => x.SeqNo > currentSeq)
                    .OrderBy(x => x.SeqNo)
                    .Select(x => x.SeqNo)
                    .FirstOrDefault();

                if (nextSeq == null)
                    break;

                // Stop if all next seq processes completed
                bool allCompleted = list
                    .Where(x => x.SeqNo == nextSeq)
                    .All(x => x.ProcessStatus == 3);

                if (allCompleted)
                    break;

                currentSeq = nextSeq;
            }

            var processIds = hierarchy
                .Select(x => x.ProcessId)
                .Distinct()
                .ToList();

            return await _unitOfWork.Processes.GetQueryable()
                .Where(p => processIds.Contains(p.ProcessId))
                .Select(p => new Process
                {
                    ProcessId = p.ProcessId,
                    ProcessName = p.ProcessName
                })
                .ToListAsync();
        }


        public async Task<ProductionLogVM> GetRCProcessDetailsByRcIdAndProcessId(int rcId, int processId, int issueStoreid, int screenCode)
        {

            var query = _unitOfWork.RouteCardSubs.GetQueryable()
                    .Include(x => x.RouteCard)
                        .ThenInclude(x => x.Customer)
                    .Include(x => x.RouteCard)
                        .ThenInclude(x => x.CostCenter)
                    .Include(x => x.OutgoingItem)
                    .Include(x => x.IncomingItem)
                    .Include(x => x.Machine)
                    .Include(x => x.Process)
                    .Where(x => x.RCId == rcId && x.ProcessId == processId);

            var sql = query.ToQueryString();

            var data = await _unitOfWork.RouteCardSubs.GetQueryable()
                            .Include(x => x.RouteCard)
                                .ThenInclude(x => x.Customer)
                            .Include(x => x.RouteCard)
                                .ThenInclude(x => x.CostCenter)
                            .Include(x => x.OutgoingItem)
                            .Include(x => x.IncomingItem)
                            .Include(x => x.Machine)
                            .Include(x => x.Process)
                            .Where(x => x.RCId == rcId && x.ProcessId == processId)
                            .FirstOrDefaultAsync();


            if (data == null)
                return null;

            //Important: QtyIn calculation logic
            decimal qtyIn = await CalculateQtyInAsync(data);


            decimal qtyOut = data.SeqNo == 1
                ? data.RouteCard?.RMReqQty ?? 0
                : data.RouteCard?.RcQty ?? 0;

            decimal qtyOutPerUnit = data.RouteCard != null && data.RouteCard.RcQty > 0 ? (data.SeqNo == 1 ? (data.RouteCard.RMWeight) : 1) : 0;


            decimal stock = 0;

            // ===================================================
            // SEQ NO = 1 → DIRECT STORE STOCK
            // ===================================================
            if (data.SeqNo == 1)
            {
                stock = await GetAvailableStockByItemIdAsync(data.ItemIdOut.Value, issueStoreid);
            }
            else
            {
                var sourceRcSubIds = await GetIssueSourceRCSubIdsAsync(data.RCId, data.SeqNo.Value, data.RCSubId);

                if (sourceRcSubIds.Any())
                {
                    foreach (var rcSubId in sourceRcSubIds)
                    {
                        var availableStock =
                            await GetAvailableStockByItemIdAndRcAndScreenAsync(data.ItemIdOut.Value, issueStoreid, rcSubId);

                        stock += availableStock;
                    }
                }
            }


            return new ProductionLogVM
            {
                IsBOM = data.IsBOM,

                //Inspection
                IsQcRequired = data.IsInspection,

                RCId = data.RCId,
                RCQty = data.RouteCard?.RcQty ?? 0,

                RCProcessId = data.RCSubId,
                ProcessId = data.ProcessId,
                ProcessName = data.Process?.ProcessName ?? string.Empty,
                ProcessCost = data.ProcessCost,

                MachineId = data.MachineId,
                MachineName = data.Machine?.MachineName ?? string.Empty,

                Operator = await _currentUserService.GetUsernameAsync(),

                CustId = data.RouteCard?.CustId,
                CustomerName = data.RouteCard?.Customer?.CustName ?? string.Empty,

                ItemIdOut = data.ItemIdOut,
                ItemOutCode = data.OutgoingItem?.ItemCode ?? string.Empty,
                QtyOut = qtyOut,
                QtyOutPerUnit = qtyOutPerUnit,

                // Stock
                RMStock = stock,

                ItemIdIn = data.ItemIdIn,
                ItemInCode = data.IncomingItem?.ItemCode ?? string.Empty,

                InputQty = qtyIn,

                CycleTime = data.CycleTime,
                SettingTime = data.SettingTime,

                AccQty = 0,
                RejQty = 0,
                RejReason = null,
                RewQty = 0,
                RewReason = null,

                ReturnQty = 0,

                CostId = data.RouteCard?.CostId,
                CostCenterName = data.RouteCard?.CostCenter?.ProjectNo ?? string.Empty

            };
        }

        private async Task<List<int>> GetRCSubIdsBySeqNoAsync(int seqNo,int rcId,int currentRcSubId)
        {
            return await _unitOfWork.RouteCardSubs
                .GetQueryable()
                .Where(x =>x.RCId == rcId &&x.SeqNo == seqNo &&x.RCSubId != currentRcSubId)
                .Select(x => x.RCSubId)
                .ToListAsync();
        }


        public async Task<decimal> CalculateQtyInAsync(RouteCardSub data)
        {
            try
            {
                if (data == null)
                    return 0;
                
                decimal usedQty = (data.AccQty) + (data.RejQty);

                if (!data.SeqNo.HasValue)
                    return 0;

                if (data.SeqNo.Value == 1)
                {
                   
                    return (data.RouteCard?.RcQty ?? 0) - usedQty;
                   
                }

                int? prevSeqNo =
                    await GetEffectivePreviousSeqNoAsync(
                        data.RCId,
                        data.SeqNo.Value);

                if (!prevSeqNo.HasValue)
                    return 0;

                decimal prevMinNextQty =
                    await GetPrevSeqMinNextQtyAsync(
                        data.RCId,
                        prevSeqNo.Value);

                decimal qtyIn = 0;

                if (data.RewQty > 0)
                {
                    qtyIn = prevMinNextQty - data.AccQty;
                }
                else
                {
                    qtyIn = prevMinNextQty - usedQty;
                }

            

                return qtyIn < 0 ? 0 : qtyIn;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"RCSubId={data?.RCSubId}, " +
                    $"RCId={data?.RCId}, " +
                    $"SeqNo={data?.SeqNo}, " +
                    $"AccQty={data?.AccQty}, " +
                    $"RejQty={data?.RejQty}, " +
                    $"RewQty={data?.RewQty}");

                throw;
            }
        }

        public async Task<int?> GetEffectivePreviousSeqNoAsync(int rcId, int currentSeqNo)
        {
            try
            {
                var prevSeqNos = await _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(x => x.RCId == rcId && x.SeqNo < currentSeqNo)
                    .Select(x => x.SeqNo)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToListAsync();

                foreach (var seq in prevSeqNos)
                {
                    bool allSkipped = await _unitOfWork.RouteCardSubs.GetQueryable()
                        .Where(x => x.RCId == rcId && x.SeqNo == seq)
                        .AllAsync(x => x.IsProcessSkip);

                    if (!allSkipped)
                        return seq;
                }

                return null;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error finding previous SeqNo for RCId={rcId}, currentSeqNo={currentSeqNo}");
                throw;
            }
        }


        public async Task<decimal> GetPrevSeqMinNextQtyAsync(int rcId, int prevSeqNo)
        {
            try
            {
                var query = _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(x => x.RCId == rcId && x.SeqNo == prevSeqNo && !x.IsProcessSkip);

                if (!await query.AnyAsync())
                    return 0;

                return await query.MinAsync(x => x.NextProcessQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error calculating Min NextProcessQty for RCId={rcId}, prevSeqNo={prevSeqNo}");
                throw;
            }
        }


        public async Task<List<ProductionLogSetting>> GetAllActiveProdLogSettingsAsync()
        {
            try
            {
                return await _unitOfWork.ProductionLogSettings.GetQueryable()
                                .Where(x => x.IsActive)
                                .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error loading Production Log Settings");
                throw;
            }
        }


        public async Task<List<ProductionLogSubVM>> GetProductionLogSubByLogId(long logId, int ScreenCode)
        {
            var list = await _unitOfWork.ProductionLogSubs.GetQueryable ()
                .Include (q => q.ProductionStopageSettings)
                .Where (q => q.LogId == logId).ToListAsync ();
            
            return _mapper.Map<List<ProductionLogSubVM>>(list);
        }


        public async Task<ProductionLogVM> UpsertproductionLogAsync(ProductionLogVM prodLogVM, int screenCode)
        {
            if (prodLogVM == null)
                throw new ArgumentNullException(nameof(prodLogVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ProductionLog entity;

                if (prodLogVM.LogId == 0)
                {
                    entity = _mapper.Map<ProductionLog>(prodLogVM);

                    entity.LogNo = await _unitOfWork.ProductionLogs.GetLastLogNoAsync(entity.Suffix);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    await _unitOfWork.ProductionLogs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    decimal remainingQty = entity.QtyOut;

                    var RcsubDetails = await _unitOfWork.RouteCardSubs.GetQueryable()
                           .FirstOrDefaultAsync(x =>
                               x.RCSubId == entity.RCProcessId &&
                               x.ItemIdOut == entity.ItemIdOut &&
                               x.ProcessId == entity.ProcessId);

                    if (entity.RouteCardProcess.SeqNo == 1)
                    {

                        await _stockManagerService.IssueOrUpdateStockAsync(
                            entity.ItemIdOut.Value,
                            entity.IssueStoreId!.Value,
                            remainingQty,
                            entity.ProcessCost,
                            null,
                            screenCode,
                            (int)entity.LogId,
                             entity.LogNo,
                            entity.LogDate,
                            allowMultipleIssue: true);



                    }
                    else
                    {



                        var issueSubIds = await GetIssueSourceRCSubIdsAsync(
                            entity.RCId.Value,
                            entity.RouteCardProcess.SeqNo.Value,
                            entity.RCProcessId.Value);

                        foreach (var subId in issueSubIds)
                        {
                            if (remainingQty <= 0)
                                break;

                            var available = await GetAvailableStockByItemIdAndRcAndScreenAsync(
                                    entity.ItemIdOut.Value, entity.IssueStoreId!.Value, subId);

                            if (available <= 0)
                                continue;

                            var issueQty = Math.Min(available, remainingQty);

                            await _stockManagerService.IssueOrUpdateStockAsync(
                                   entity.ItemIdOut.Value,
                                   entity.IssueStoreId!.Value,
                                   remainingQty,
                                   entity.ProcessCost,
                                   null,
                                   screenCode,
                                   (int)entity.LogId,
                                   entity.LogNo,
                                   entity.LogDate,
                                   subId,
                                   allowMultipleIssue: true);

                            remainingQty -= issueQty;
                        }
                        if (remainingQty > 0)
                            throw new InvalidOperationException($"Insufficient RC stock. Remaining Qty: {remainingQty}");


                    }

                    var routeCardProcess = await _unitOfWork.RouteCardSubs
                        .GetQueryable()
                        .Include(x => x.RouteCard)
                        .FirstOrDefaultAsync(x => x.RCSubId == entity.RCProcessId!.Value)
                        ?? throw new Exception("Route Card Process not found");

                    routeCardProcess.IssuedQty += entity.InputQty;
                    routeCardProcess.BalQty -= entity.InputQty;

                    routeCardProcess.ProcessStatus = await GetProcessStatusAsync(routeCardProcess);

                    await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardProcess);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Production Log Created.");
                }
                else
                {
                    entity = await _unitOfWork.ProductionLogs.GetQueryable()
                        .FirstOrDefaultAsync(q => q.LogId == prodLogVM.LogId)
                        ?? throw new InvalidOperationException("Production Log not found");

                    _mapper.Map(prodLogVM, entity);
                    entity.UpdatedBy = currentUser;
                    entity.UpdatedDate = now;

                    entity.RCPSubId = prodLogVM.RCPSubId;
                    entity.RewProcessId = prodLogVM.RewProcessId;

                    await _unitOfWork.ProductionLogs.UpdateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    // RC-Process -Status Update
                    await UpdateRouteCardProcessQtyAsync(entity);


                    if (entity.AccQty > 0)
                        await _stockManagerService.AddOrUpdateStockAsync(
                            entity.ItemIdIn.Value,
                            entity.AddStoreId.Value,
                            entity.AccQty,
                            entity.ProcessCost,
                            entity.BatchNo,
                            screenCode,
                            (int)entity.LogId,
                            entity.LogNo,
                            entity.LogDate,
                            entity.Remark,
                            entity.RCProcessId,
                            allowMultipleAdd: true);

                    if (entity.RejQty > 0)
                    {
                        await UpdateRouteCardSubRejQtyAsync(entity.ItemIdIn.GetValueOrDefault(), entity.ProcessId.GetValueOrDefault(), entity.RejQty, entity.RCProcessId.Value);
                        await _stockManagerService.AddOrUpdateStockAsync(
                            entity.ItemIdIn.Value,
                            6,
                            entity.RejQty,
                            entity.ProcessCost,
                            entity.BatchNo,
                            screenCode,
                            (int)entity.LogId,
                            entity.LogNo,
                            entity.LogDate,
                            entity.Remark,
                            entity.RCProcessId,
                            allowMultipleAdd: true);
                    }

                    if (entity.RewQty > 0)
                    {
                        await UpdateRouteCardSubAsync(entity.RCPSubId!.Value, entity.ItemIdIn.GetValueOrDefault(), entity.RewProcessId.GetValueOrDefault(), entity.RewQty, 0, entity.RCProcessId.Value);

                        var RcsubDetails = await _unitOfWork.RouteCardSubs.GetQueryable()
                               .FirstOrDefaultAsync(x =>
                                   x.RCSubId == entity.RCPSubId &&
                                   x.ItemIdIn == entity.ItemIdIn &&
                                   x.ProcessId == entity.RewProcessId);

                        if (RcsubDetails.SeqNo == 1)
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(
                           RcsubDetails.ItemIdOut.Value,
                           7,
                           entity.RewQty,
                           entity.ProcessCost,
                           entity.BatchNo,
                           screenCode,
                           (int)entity.LogId,
                           entity.LogNo,
                           entity.LogDate,
                           entity.Remark,
                           allowMultipleAdd: true);
                        }

                        else
                        {

                            var sourceRcSubIds = await GetIssueSourceRCSubIdsAsync(entity.RCId.Value, RcsubDetails.SeqNo.Value, entity.RCPSubId.Value);

                            foreach (var subs in sourceRcSubIds)
                            {
                                await _stockManagerService.AddOrUpdateStockAsync(
                              RcsubDetails.ItemIdOut.Value,
                              7,
                              entity.RewQty,
                              entity.ProcessCost,
                              entity.BatchNo,
                              screenCode,
                              (int)entity.LogId,
                              entity.LogNo,
                              entity.LogDate,
                              entity.Remark,
                              subs,
                              allowMultipleAdd: true);

                            }

                        }



                    }

                    if (entity.ReturnQty > 0)
                    {
                        var returnIssueQty = entity.ReturnQty * entity.QtyOutPerUnit;
                        var exactIssueQty = entity.QtyOut - returnIssueQty;

                        decimal remainingIssueQty = exactIssueQty;

                        if (entity.RouteCardProcess.SeqNo == 1)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(
                                entity.ItemIdOut.Value,
                                entity.IssueStoreId!.Value,
                                remainingIssueQty,
                                entity.ProcessCost,
                                null,
                                screenCode,
                                (int)entity.LogId,
                                entity.LogNo,
                                entity.LogDate);
                        }
                        else
                        {
                            var stockIssues = await _unitOfWork.StockIssues
                                .GetQueryable()
                                .Where(x => x.SubItemRefID == entity.LogId)
                                .ToListAsync();

                            foreach (var issue in stockIssues)
                                await _stockManagerService.DeleteStockIssueAsync(issue.IssueId);

                            var issueSubIds = await GetIssueSourceRCSubIdsAsync(
                                entity.RCId.Value,
                                entity.RouteCardProcess.SeqNo.Value,
                                entity.RCProcessId.Value);

                            foreach (var subId in issueSubIds)
                            {
                                if (remainingIssueQty <= 0)
                                    break;

                                var available =
                                    await GetAvailableStockByItemIdAndRcAndScreenAsync(
                                        entity.ItemIdOut.Value,
                                        entity.IssueStoreId!.Value,
                                        subId);

                                if (available <= 0)
                                    continue;

                                var issueQty = Math.Min(available, remainingIssueQty);

                                await _stockManagerService.IssueOrUpdateStockAsync(
                                        entity.ItemIdOut.Value,
                                        entity.IssueStoreId!.Value,
                                        remainingIssueQty,
                                        entity.ProcessCost,
                                        null,
                                        screenCode,
                                        (int)entity.LogId,
                                        entity.LogNo,
                                        entity.LogDate,
                                        subId);

                                remainingIssueQty -= issueQty;

                                if (remainingIssueQty > 0)
                                    throw new InvalidOperationException($"Insufficient RC stock after return adjustment. Remaining Qty: {remainingIssueQty}");
                            }


                        }


                    }

                    changes.AppendLine("Production Log Updated.");
                }

                await transaction.CommitAsync();

                await LogChangesAsync(
                    changes,
                    prodLogVM.LogId == 0
                        ? "Production Log Created"
                        : "Production Log Updated");

                var savedEntity = await _unitOfWork.ProductionLogs.GetQueryable()
                    .Include(q => q.AddStore)
                    .Include(q => q.IssueStore)
                    .Include(q => q.ItemIn)
                    .Include(q => q.ItemOut)
                    .Include(q => q.CostCenter)
                    .FirstOrDefaultAsync(q => q.LogId == entity.LogId);

                return _mapper.Map<ProductionLogVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Production Log: {prodLogVM.LogNo}");
                throw new InvalidOperationException("Failed to save Production Log. Please try again.");
            }
        }


        public async Task UpdateRouteCardProcessQtyAsync(ProductionLog entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (!entity.RCProcessId.HasValue)
                throw new Exception("RC Process Id is missing.");

            var routeCardProcess = await _unitOfWork.RouteCardSubs
                .GetQueryable()
                .Include(x => x.RouteCard)
                .FirstOrDefaultAsync(x => x.RCSubId == entity.RCProcessId.Value)
                ?? throw new Exception("Route Card Process not found");

            // -------------------------------
            // Accumulate Production Quantities
            // -------------------------------

            routeCardProcess.AccQty += entity.AccQty;
            routeCardProcess.RejQty += entity.RejQty;
            routeCardProcess.RewQty += entity.RewQty;


            // Next process flow
            routeCardProcess.NextProcessQty += entity.AccQty;

            if (entity.ReturnQty > 0)
            {
                // routeCardProcess.IssuedQty = Math.Max(0, routeCardProcess.IssuedQty - entity.ReturnQty);
                routeCardProcess.IssuedQty = Math.Max(0, routeCardProcess.IssuedQty - (entity.AccQty + entity.RejQty + entity.RewQty));
                routeCardProcess.BalQty += entity.ReturnQty;
            }

            if (entity.RejQty > 0)
            {
                // routeCardProcess.IssuedQty = Math.Max(0, routeCardProcess.IssuedQty - entity.RejQty);
                routeCardProcess.IssuedQty = Math.Max(0, routeCardProcess.IssuedQty - (entity.AccQty + entity.RejQty + entity.RewQty));
                //routeCardProcess.BalQty += entity.RejQty;
            }

            if (entity.RewQty > 0)
            {
                routeCardProcess.IssuedQty = Math.Max(0, routeCardProcess.IssuedQty - (entity.AccQty + entity.RejQty + entity.RewQty));
                routeCardProcess.BalQty += entity.RewQty;
            }

            if (entity.AccQty > 0)
            {
                routeCardProcess.IssuedQty = Math.Max(0, routeCardProcess.IssuedQty - (entity.AccQty + entity.RejQty + entity.RewQty));

            }

            // -------------------------------
            // Process Status Update
            // -------------------------------

            routeCardProcess.ProcessStatus = await GetProcessStatusAsync(routeCardProcess);

            await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardProcess);
            await _unitOfWork.SaveAsync();

            // -------------------------------
            // Route Card Status Update
            // -------------------------------

            if (entity.RCId.HasValue && entity.RCId > 0)
            {
                var rc = await _unitOfWork.RouteCards.GetAsync(entity.RCId.Value);

                if (rc != null)
                {
                    bool isProcessCompleted =
                        routeCardProcess.IsFinalProcess &&
                        routeCardProcess.IssuedQty <=
                        (routeCardProcess.AccQty + routeCardProcess.RejQty + routeCardProcess.RewQty) &&
                        routeCardProcess.BalQty == 0;

                    rc.RcStatus = isProcessCompleted ? (byte)2 : (byte)1; // 2 completed  // 1 inProgress

                    await _unitOfWork.RouteCards.UpdateAsync(rc);
                    await _unitOfWork.SaveAsync();
                }
            }
        }




        public async Task<List<int>> GetIssueSourceRCSubIdsAsync(int rcId,int seqNo,int currentRcSubId)
        {
            var rcSubIds = new List<int>();

            // SAME SEQ (PARALLEL)
            var sameSeqIds = await GetRCSubIdsBySeqNoAsync(seqNo, rcId, currentRcSubId);
            if (sameSeqIds.Any())
                rcSubIds.AddRange(sameSeqIds);

            // PREVIOUS EFFECTIVE SEQ
            var prevSeqNo = await GetEffectivePreviousSeqNoAsync(rcId, seqNo);
            if (prevSeqNo.HasValue)
            {
                var prevIds = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Where(x => x.RCId == rcId &&
                        x.SeqNo == prevSeqNo.Value &&
                        !x.IsProcessSkip)
                    .Select(x => x.RCSubId)
                    .ToListAsync();

                rcSubIds.AddRange(prevIds);
            }

            return rcSubIds.Distinct().ToList();
        }



        public async Task<byte> GetProcessStatusAsync(RouteCardSub routeCardProcess)
        {
            try
            {
                if (routeCardProcess == null)
                    throw new ArgumentNullException(nameof(routeCardProcess));

                decimal balQty = routeCardProcess.BalQty;

                decimal accQty = routeCardProcess.AccQty;
                decimal rejQty = routeCardProcess.RejQty;
                decimal rewQty = routeCardProcess.RewQty;
                decimal WipQty = routeCardProcess.WipQty;

                decimal issuedQty = routeCardProcess.IssuedQty;

                //  decimal usedQty = accQty + rejQty + rewQty;
                decimal usedQty = accQty + rejQty;
                decimal compareQty = 0;

                // =========================
                // DETERMINE COMPARISON QTY
                // =========================
                if (routeCardProcess.SeqNo == 1)
                {
                    compareQty = routeCardProcess.RouteCard.RcQty;
                }
                else
                {
                    int? prevSeqNo = await GetEffectivePreviousSeqNoAsync(routeCardProcess.RCId, routeCardProcess.SeqNo.Value);

                    if (prevSeqNo == null)
                        return 0;

                    compareQty = await GetPrevSeqMinNextQtyAsync(routeCardProcess.RCId, prevSeqNo.Value);
                }

                // =========================
                // STATUS DECISION
                // =========================

                // In Progress
                if ((usedQty > 0 || issuedQty > 0) && balQty > 0)
                    return 1;

                // Partially Completed
                if ((rejQty > 0 || rewQty > 0) && balQty > 0)
                    return 2;

                // Completed
                if (usedQty == compareQty && compareQty > 0 && balQty == 0 && issuedQty == 0 && WipQty == 0)
                    return 3;



                // Not Started
                return 0;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Failed to calculate Process Status | RCId: {routeCardProcess?.RCId}, " +
                    $"SeqNo: {routeCardProcess?.SeqNo}, " +
                    $"IssuedQty: {routeCardProcess?.IssuedQty}, " +
                    $"AccQty: {routeCardProcess?.AccQty}, " +
                    $"RejQty: {routeCardProcess?.RejQty}, " +
                    $"RewQty: {routeCardProcess?.RewQty}"
                );

                throw new InvalidOperationException("Failed to calculate process status. Please try again.", ex);
            }
        }



        private async Task DeleteStockIssueAndTrackAsync(int scnSubId, int itemId, int screenCode)
        {
            var issueIds = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(s => s.SubItemRefID == scnSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.IssueId)
                .ToListAsync();

            foreach (var issueId in issueIds)
            {
                if (issueId > 0)
                    await _stockManagerService.DeleteStockIssueAsync(issueId);
            }
        }


        private async Task DeleteStockAddAsync(int scnSubId, int itemId, int screenCode)
        {
            var addIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == scnSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.AddId)
                .ToListAsync();

            foreach (var addId in addIds)
            {
                if (addId > 0)
                    await _stockManagerService.DeleteStockAddAsync(addId);
            }
        }

        private async Task AdjustReturnAssyBalanceAsync(int? refReturnSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refReturnSubId.HasValue || refReturnSubId == 0) return;

                var AssyReturnSub = await _unitOfWork.ProductionReturnAssySubs.GetAsync(refReturnSubId.Value);
                if (AssyReturnSub == null) return;

                if (oldQty > 0)
                    AssyReturnSub.BalQty += oldQty;

                if (newQty > AssyReturnSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Production Return BalQty.");

                if (newQty > 0)
                    AssyReturnSub.BalQty -= newQty;

                await _unitOfWork.ProductionReturnAssySubs.UpdateAsync(AssyReturnSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.ProductionReturnAssySubs
                    .GetQueryable()
                    .Where(e => e.ReturnId == AssyReturnSub.ReturnId)
                    .SumAsync(e => e.BalQty);

                var productionReturnAssy = await _unitOfWork.ProductionReturnAssys.GetAsync(AssyReturnSub.ReturnId);
                if (productionReturnAssy != null)
                {
                    productionReturnAssy.ReturnTally = (totalBalQty == 0);
                    await _unitOfWork.ProductionReturnAssys.UpdateAsync(productionReturnAssy);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustReturnAssyBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustReturnAssyBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Return Balance. Please contact support.");
            }
        }

        // Get property changes for logging
        private string GetPropertyChanges<TSource, TTarget>(TSource entity, TTarget vm)
        {
            var sb = new StringBuilder();
            foreach (var prop in typeof(TSource).GetProperties())
            {
                var vmProp = typeof(TTarget).GetProperty(prop.Name);
                if (vmProp == null) continue;

                var oldVal = prop.GetValue(entity)?.ToString() ?? "null";
                var newVal = vmProp.GetValue(vm)?.ToString() ?? "null";

                if (oldVal != newVal)
                    sb.AppendLine($"{prop.Name}: '{oldVal}' → '{newVal}'");
            }
            return sb.ToString();
        }

        // Logging
        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _logs.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Production Assembly SCN",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<int> GetReturnPendingCountAsync()
        {
            try
            {
                var count = await _unitOfWork.ProductionReturnAssys.GetQueryable()
                    .Where(x => x.ReturnTally == false)
                    .CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                // optional: log or handle
                throw new Exception("Error fetching pending return count", ex);
            }
        }

        public async Task DeleteAndResequenceAsync(ProductionSCNAssySubVM subitem, ProductionSCNAssyVM prodScn, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.SCNSubId > 0)
                {
                    var entity = await _unitOfWork.ProductionSCNAssySubs.GetAsync(subitem.SCNSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    await DeleteStockIssueAndTrackAsync(subitem.SCNSubId, subitem.ItemId.Value, screenCode);
                    await DeleteStockAddAsync(subitem.SCNSubId, subitem.ItemId.Value, screenCode);

                    if (subitem.RefReturnSubId > 0)
                    {
                        var totQty = subitem.AccQty + subitem.RejQty + subitem.RewQty;
                        await AdjustReturnAssyBalanceAsync(subitem.RefReturnSubId, totQty.GetValueOrDefault(), 0, "Production SCN Assembly delete");
                    }

                    await _unitOfWork.ProductionSCNAssySubs.DeleteAsync(entity.SCNSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Production SCN",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"SCN No: {prodScn?.SCNNo}"
                    );
                }
                else
                {
                    prodScn.ProductionSCNAssySubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.ProductionSCNAssySubs
                    .GetQueryable()
                    .Where(x => x.SCNId == prodScn.SCNId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetProductionAssyReturnDetails()
        {
            try
            {
                var result = await (from e in _unitOfWork.ProductionReturnAssys.GetQueryable()
                                    join es in _unitOfWork.ProductionReturnAssySubs.GetQueryable()
                                        on e.ReturnId equals es.ReturnId
                                    where !e.ReturnTally && es.BalQty > 0 && !e.Rejection && !e.Return
                                    select new
                                    {
                                        es.ReturnSubId,
                                        e.ReturnNo,
                                        e.Suffix,
                                        e.ReturnDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.MeasureUnit,
                                        es.QtyReturned,
                                        es.UnitPrice,
                                        es.BalQty,
                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefReturnSubId"] = r.ReturnSubId,
                    ["ReturnNo"] = $"{r.ReturnNo}{r.Suffix}",
                    ["ReturnDate"] = r.ReturnDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,
                    ["Qty"] = r.QtyReturned,
                    ["UnitPrice"] = r.UnitPrice,
                    ["BalQty"] = r.BalQty,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production Assy Return details");
                throw new InvalidOperationException("Failed to retrieve Production return details. Please try again.");
            }
        }

        public async Task<decimal> GetProdReturnItemBalQtyFromReturnSubId(int returnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionReturnAssySubs.GetQueryable()
                    .Where(e => e.ReturnSubId == returnSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for Production ReturnSubId: {returnSubId}");
                throw new InvalidOperationException("Failed to retrieve Production Return balance quantity.");
            }
        }
        public async Task<ProductionSCNAssySubVM?> GetProdScnSubItemDetailByScnSubIdAsync(int scnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionSCNAssySubs
                    .GetQueryable()
                    .Where(q => q.SCNSubId == scnSubId)
                    .Select(q => new ProductionSCNAssySubVM
                    {
                        AccQty = q.AccQty,
                        RejQty = q.RejQty,
                        RewQty = q.RewQty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production SCN sub item detail for SCNSubId: {scnSubId}");
                throw new InvalidOperationException("Failed to retrieve Production SCN sub-item details.");
            }
        }
        public async Task UpdateRouteCardSubRejQtyAsync(int itemIdIn, int processId, decimal rejectQty, int currentRcSubId)
        {
            if (rejectQty <= 0)
                return;

            // Current process
            var current = await _unitOfWork.RouteCardSubs.GetQueryable()
                .FirstOrDefaultAsync(x => x.RCSubId == currentRcSubId);

            if (current == null)
                return;

            // Current process + all next processes
            var routeSubs = await _unitOfWork.RouteCardSubs.GetQueryable()
                .Where(x =>
                    x.RCId == current.RCId &&
                    x.ItemIdIn == itemIdIn &&
                    !x.IsProcessSkip &&
                    x.SlNo >= current.SlNo)
                .OrderBy(x => x.SlNo)
                .ToListAsync();

            foreach (var sub in routeSubs)
            {
                if (sub.RCSubId != currentRcSubId)
                    sub.BalQty = Math.Max(0, sub.BalQty - rejectQty);

                await _unitOfWork.RouteCardSubs.UpdateAsync(sub);
            }

            await _unitOfWork.SaveAsync();
        }
        public async Task UpdateRouteCardSubAsync(int rcSubId, int itemIdIn, int reworkProcessId, decimal reworkNewQty, decimal reworkOldQty, int CurrentRcsubid)
        {
            if (reworkNewQty <= 0 && reworkOldQty <= 0)
                return;

            // Current Process
            var current = await _unitOfWork.RouteCardSubs.GetQueryable()
                .FirstOrDefaultAsync(x => x.RCSubId == CurrentRcsubid);

            if (current == null)
                return;

            // Selected Rework Process
            var reworkProcess = await _unitOfWork.RouteCardSubs.GetQueryable()
                .FirstOrDefaultAsync(x =>
                    x.RCId == current.RCId &&
                    x.ItemIdIn == itemIdIn &&
                    x.RCSubId == rcSubId);

            if (reworkProcess == null)
                return;

         

            var routeSubs = await _unitOfWork.RouteCardSubs.GetQueryable()
                .Where(x =>
                    x.RCId == current.RCId &&
                    x.ItemIdIn == itemIdIn &&
                    !x.IsProcessSkip &&
                    (
                        // Selected process only
                        x.RCSubId == reworkProcess.RCSubId ||

                        // All later sequences
                        x.SeqNo >= reworkProcess.SeqNo
                    ) &&
                    x.SeqNo <= current.SeqNo)
                .OrderBy(x => x.SeqNo)
                .ThenBy(x => x.SlNo)
                .ToListAsync();

            foreach (var sub in routeSubs)
            {
                if (CurrentRcsubid != sub.RCSubId)
                {
                    if (reworkNewQty > 0)
                    {
                        sub.BalQty += reworkNewQty;
                        sub.AccQty -= reworkNewQty;
                        sub.NextProcessQty -= reworkNewQty;

                        if (sub.AccQty < 0)
                            sub.AccQty = 0;

                        sub.ProcessStatus = 1;

                    }
                    else
                    {
                        sub.BalQty -= reworkOldQty;
                        sub.AccQty += reworkOldQty;
                        sub.NextProcessQty += reworkOldQty;

                        sub.ProcessStatus = await GetProcessStatusAsync(sub);
                    }

                }



                await _unitOfWork.RouteCardSubs.UpdateAsync(sub);
            }

            await _unitOfWork.SaveAsync();
        }
        public async Task<bool> GetIsReworkProcessAsync(int RcId, int ProcessId)
        {
            try
            {
                return await _unitOfWork.RouteCardSubs.GetQueryable().AnyAsync(x => x.RCId == RcId
                      && x.ProcessId == ProcessId
                      && x.IsReworkProcess);

            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<RouteCardSub>> GetReworkProcessesAsync(int itemId, int RcId, int processId)
        {
            try
            {
                var currentSlNo = await _unitOfWork.RouteCardSubs.GetQueryable()
                   .Where(x => x.RCId == RcId && x.ProcessId == processId && x.ItemIdIn == itemId)
                   .Select(x => x.SlNo)
                   .FirstOrDefaultAsync();

                return await _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(x => x.RCId == RcId && x.SlNo < currentSlNo && !x.IsProcessSkip)
                    .OrderBy(x => x.SlNo)
                    .Include(x => x.Process)   // If RouteCardSub has Process navigation property
                    .ToListAsync();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
       

    }
}
