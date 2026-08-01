using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using Microsoft.EntityFrameworkCore;
using System.Text;
namespace V.SMART.Shared.BusinessLayer.BusinessService.PlanningService
{
    public class RouteCardService : IRouteCardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public RouteCardService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            IStockManagerService stockManagerService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _stockManagerService = stockManagerService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        // 🔹 Customers

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }
        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetCustomerByIdAsync(custId);

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchComponentItemsAsync(searchText);
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        public Task<IEnumerable<Process>> GetAllProcessesAsync()
            => _commonService.GetAllProcessAsync();

        public Task<List<Machine>> GetAllMachinesAsync()
            => _commonService.GetAllMachineAsync();

        public Task<List<VendorVM>> GetAllVendorsAsync()
            => _commonService.GetAllActiveVendorsAsync();

        public async Task<decimal> GetAvailableStockByItemIdAsync(int itemId, int? storeId)
            => await _stockManagerService.GetStockForItemAsync(itemId, storeId);

        public async Task<bool> GetScreenPermissionsAsync()
            => await _commonService.GetScreenPermissionsAsync("Route Card", "Manual RC");

        // 🔹 Route Card operations

        public async Task<(bool CanDelete, string Message)> CanDeleteRoutecardAsync(int rcId)
        {
            try
            {
                var routeCard = await _unitOfWork.RouteCards
                                .GetQueryable()
                                .Include(e => e.RouteCardSubs)
                                .Where(e => e.RCId == rcId).FirstOrDefaultAsync();

                if (routeCard == null)
                    return (true, "Route Card can be safely deleted.");

                var rcSubIds = routeCard.RouteCardSubs
                    .Select(es => es.RCSubId)
                    .ToList();

                bool hasDailyLog = await _unitOfWork.ProductionLogs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RCProcessId.HasValue &&
                        rcSubIds.Contains(qs.RCProcessId.Value));

                if (hasDailyLog)
                    return (false, "Cannot delete this Route Card as a Daily Production Log transaction exists.");


                bool hasProductionComp = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RcSubId.HasValue &&
                        rcSubIds.Contains(qs.RcSubId.Value));

                if (hasProductionComp)
                    return (false, "Cannot delete this Route Card as a Prouction Component transaction exists.");


                bool hasSubcon = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RcSubId.HasValue &&
                        rcSubIds.Contains(qs.RcSubId.Value));

                if (hasSubcon)
                    return (false, "Cannot delete this Route Card as a Sub- Contract transaction exists.");

                if (routeCard.RcStatus > 0)
                    return (false, "Cannot delete this Route Card because transactions already exist or the Route Card is cancelled.");

                return (true, "Route card can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteRoutecardAsync for RcId: {rcId}");
                throw new Exception("Error checking Route card delete eligibility", ex);
            }
        }



        public async Task<bool> HasAnyRcTransactionMadeAsync(int rcId)
        {
            try
            {
                var rcSubIds = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Where(s => s.RCId == rcId)
                    .Select(s => s.RCSubId)
                    .ToListAsync();

                if (!rcSubIds.Any())
                    return false;

                var hasProdLog = await _unitOfWork.ProductionLogs
                    .GetQueryable()
                    .AnyAsync(x => rcSubIds.Contains(x.RCProcessId.Value));

                if (hasProdLog)
                    return true;

                var hasIssueComp = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .AnyAsync(x => rcSubIds.Contains(x.RcSubId.Value));

                if (hasIssueComp)
                    return true;




                return false;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyRcTransactionMadeAsync for RCId: {rcId}");
                throw;
            }
        }

        public async Task<bool> HasAnyRCCancelAsync(int rcId)
        {
            try
            {
                var isRcCanceled = await _unitOfWork.RouteCards
                    .AnyAsync(q => q.RCId == rcId && q.RcStatus == 3);

                return isRcCanceled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyRCCancelAsync for RcId: {rcId}");
                throw;
            }
        }

        public async Task<bool> DeleteRouteCardByRCIdAsync(int rcId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var routeCard = await _unitOfWork.RouteCards
                    .GetQueryable()
                    .Where(e => e.RCId == rcId).FirstOrDefaultAsync();

                if (routeCard == null)
                {
                    return false;
                }

                var changes = new StringBuilder();


                if (routeCard.RefRcReleseId > 0 && routeCard.AssyId > 0)
                {
                    await AdjustRcReleaseComponentTrackerAsync(routeCard.RefRcReleseId, routeCard.AssyId, routeCard.CompItemId, routeCard.RcQty, 0, "Route Card Delete");
                }
                else if (routeCard.RefPoId > 0 && routeCard.CompItemId > 0)
                {
                    await AdjustPoRouteCardBalanceAsync(routeCard.RefPoId, routeCard.CompItemId, routeCard.RcQty, 0, "Route Card Deletion");
                }

                var deleted = await _unitOfWork.RouteCards.DeleteAsync(rcId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Route Card List",
                    action: $"Deleted RouteCard: {routeCard.RCNo}",
                    additionalInfo: $"RouteCard Id: {routeCard.RCId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete RouteCard: {rcId}");
                throw;
            }
        }


        public async Task<List<ItemVM>> GetAssemblyItemsByRcRelease(int? rcReleaseId)
        {

            try
            {
                if (!rcReleaseId.HasValue || rcReleaseId.Value <= 0)
                    return new List<ItemVM>();

                var items = await _unitOfWork.RcReleases.GetQueryable()
                    .AsNoTracking()
                    .Where(m =>
                        m.RouteCardReleaseId == rcReleaseId.Value &&
                        !m.RcReleaseTally &&
                        m.Assembly != null)
                    .Select(m => new ItemVM
                    {
                        ItemId = m.Assembly.ItemId,
                        ItemCode = m.Assembly.ItemCode
                    })
                    .Distinct()
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetAssemblyItemsByRcRelease({rcReleaseId})");
                return new List<ItemVM>();
            }
        }

        public async Task<List<ItemVM>> GetComponentItemsByPO(int? poId)
        {
            try
            {
                if (!poId.HasValue || poId.Value == 0)
                    return new List<ItemVM>();

                return await _unitOfWork.MfgPoSubs.GetQueryable()
                        .AsNoTracking()
                        .Where(m =>
                            m.PoId == poId.Value &&
                            m.RouteCardBalQty > 0 &&
                            !m.ItemCancel &&
                            m.Item.CategoryCode == 2 &&
                            !m.MfgPo.PoTally)
                        .Select(m => new ItemVM
                        {
                            ItemId = m.Item.ItemId,
                            ItemCode = m.Item.ItemCode
                        })
                        .GroupBy(x => x.ItemId)
                        .Select(g => g.First())
                        .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetComponentItemsByPO({poId})");
                return new List<ItemVM>();
            }
        }

        public async Task<List<ItemVM>> GetSubAssyByParentAsync(int parentItemId)
        {
            try
            {
                return await _unitOfWork.AssmblyDefs.GetQueryable()
                    .AsNoTracking()
                    .Include(b => b.PartItem)
                    .Where(b => b.AssmblyID == parentItemId &&
                                b.PartItem.CategoryCode == 7)
                    .Select(b => new ItemVM
                    {
                        ItemId = b.PartItem.ItemId,
                        ItemCode = b.PartItem.ItemCode
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetSubAssyByParentAsync({parentItemId})");
                return new List<ItemVM>();
            }
        }

        public async Task<List<ItemVM>> GetComponentsByParentAsync(int parentItemId, int rcReleaseId)
        {
            try
            {
                if (parentItemId <= 0 || rcReleaseId <= 0)
                    return new List<ItemVM>();

                var items = await _unitOfWork.RcReleases.GetQueryable()
                    .AsNoTracking()
                    .Where(b => b.AssemblyItemId == parentItemId && b.RouteCardReleaseId == rcReleaseId)
                    .SelectMany(b => b.RouteCardReleaseSubs)
                    .Where(s => s.Item != null && s.RemainingQty > 0 && !s.IsCompleted)
                    .Select(s => new ItemVM
                    {
                        ItemId = s.Item.ItemId,
                        ItemCode = s.Item.ItemCode
                    })
                    .Distinct()
                    .ToListAsync();

                return items;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetComponentsByParentAsync({parentItemId},{rcReleaseId})");
                return new List<ItemVM>();
            }
        }



        public async Task<MfgPoSub?> GetPOQtyAndRCBalQtyByPoIdAndItemId(int poId, int compId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs.GetQueryable()
                    .AsNoTracking()
                    .Where(po => po.PoId == poId && po.ItemId == compId)
                    .Select(po => new MfgPoSub
                    {
                        Qty = po.Qty,
                        RouteCardBalQty = po.RouteCardBalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to get PO Qty and RC Bal Qty for PoId {poId} and CompId {compId}");
                return null;
            }
        }


        public async Task<decimal> GetRemainingQtyByAssyPoCompTrack(int rcReleaseId, int assyId, int compItemId)
        {
            try
            {
                var remainingQty = await _unitOfWork.RcReleaseSubs
                    .GetQueryable()
                    .Where(x =>
                        x.RouteCardReleaseId == rcReleaseId &&
                        x.RouteCardRelease.AssemblyItemId == assyId &&
                        x.ComponentItemId == compItemId)
                    .Select(x => x.RemainingQty)
                    .FirstOrDefaultAsync();

                return remainingQty < 0 ? 0 : remainingQty;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to get Remaining Qty for RcReleaseId {rcReleaseId}, AssyId {assyId}, CompId {compItemId}");
                throw;
            }
        }


        public async Task<List<ItemVM>> GetRawMaterialByCompItemId(int compItemId)
        {
            try
            {
                var rmItems = await _unitOfWork.CompMasters.GetQueryable()
                    .Where(x => x.CompItemId == compItemId)
                    .Include(x => x.RawMaterial)
                    .ToListAsync();

                return rmItems.Select(rm => new ItemVM
                {
                    ItemId = rm.RMId ?? 0,
                    ItemCode = rm.RawMaterial.ItemCode
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to load Raw Materials for Component ItemId {compItemId}");
                throw;
            }
        }

        public async Task<RMDetailsVM?> GetRMDetails(int rmId, int compItemId, decimal rcQty)
        {
            try
            {
                var rm = await _unitOfWork.CompMasters.GetQueryable()
                    .Include(x => x.RawMaterial)
                    .Where(x => x.RMId == rmId && x.CompItemId == compItemId).FirstOrDefaultAsync();

                if (rm == null)
                    return null;

                var stock = await _stockManagerService.GetStockForItemAsync(rmId, null);

                return new RMDetailsVM
                {
                    RequiredQty = rm.Weight * rcQty,
                    StockQty = stock,
                    Weight = rm.Weight,
                    BatchNo = rm.RawMaterial?.BatchNo ?? "",
                    Remarks = ""
                };
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to load RM Details for RMId {rmId}");
                throw;
            }
        }

        public async Task<List<ProcessFlowChart>> GetProcessFlowAsync(int compItemId, int rmId)
        {
            try
            {
                return await _unitOfWork.ProcessFlowCharts.GetQueryable()
                .Include(x => x.Process)
                .Include(x => x.Machine)
                .Include(x => x.Vendor)
                .Where(x => x.CompItemId == compItemId && x.RMId == rmId)
                .OrderBy(x => x.SlNo)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to load RC Process for RMId {rmId} And CompItemId {compItemId}");
                throw;
            }
        }

        public async Task<(List<RouteCardVM> routeCardVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.RouteCards.GetQueryable()
                    .Include(j => j.Customer)
                    .Include(j => j.MfgPo)
                    .Include(j => j.ComponentItem)
                    .Include(j => j.AssemblyItem)
                    .Include(j => j.SubAssemblyItem)
                    .Include(j => j.SubAssemblyItem2)
                    .Include(j => j.SubAssemblyItem3)
                    .Include(j => j.RawMaterialItem)
                    .Include(j => j.AddStore)
                    .Include(j => j.IssueStore)
                    .Include(j => j.RouteCardSubs)
                        .ThenInclude(s => s.Process)
                    .Include(j => j.RouteCardSubs)
                        .ThenInclude(s => s.Machine)
                    .Include(j => j.RouteCardSubs)
                        .ThenInclude(s => s.Vendor)
                    .AsQueryable();

                // Apply Dynamic Filters
                if (filters != null)
                {
                    foreach (var f in filters)
                    {
                        query = RouteCardFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                    }
                }

                var total = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.RCId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // AutoMapper mapping
                var vmList = _mapper.Map<List<RouteCardVM>>(list);

                return (vmList, total);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"SearchWithDynamicFilterAsync on RouteCrad Load");
                return (new List<RouteCardVM>(), 0);
            }
        }

        public static class RouteCardFilterBuilder
        {
            public static IQueryable<RouteCard> ApplyFilter(
                IQueryable<RouteCard> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "RCNo":
                        {
                            var input = val.ToString()?.Trim();
                            if (string.IsNullOrEmpty(input))
                                return query;

                            string part1 = input;
                            string part2 = "";

                            int slashIndex = input.IndexOf('/');

                            if (slashIndex > -1)
                            {
                                part1 = input.Substring(0, slashIndex).Trim();
                                part2 = input.Substring(slashIndex).Trim();
                            }

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.RCNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "RefPoNo":
                        {
                            var input = val.ToString()?.Trim();
                            if (string.IsNullOrEmpty(input))
                                return query;

                            string part1 = input;
                            string part2 = "";

                            int slashIndex = input.IndexOf('/');

                            if (slashIndex > -1)
                            {
                                part1 = input.Substring(0, slashIndex).Trim();
                                part2 = input.Substring(slashIndex).Trim();
                            }

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.MfgPo.PONo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.MfgPo.Suffix.Contains(part2))
                            );
                        }


                    case "CustName":
                        return query.Where(x => x.Customer.CustName.Contains(val));

                    case "AssyCode":
                        return query.Where(x => x.AssemblyItem.ItemCode.Contains(val));

                    case "SubAssyCode":
                        return query.Where(x => x.SubAssemblyItem.ItemCode.Contains(val));

                    case "SubAssyCode2":
                        return query.Where(x => x.SubAssemblyItem2.ItemCode.Contains(val));

                    case "SubAssyCode3":
                        return query.Where(x => x.SubAssemblyItem3.ItemCode.Contains(val));

                    case "CompItemCode":
                        return query.Where(x => x.ComponentItem.ItemCode.Contains(val));

                    case "ItemName":
                        return query.Where(x => x.ComponentItem.ItemName.Contains(val));

                    case "RMCode":
                        return query.Where(x => x.ComponentItem.ItemCode.Contains(val));

                    case "RMName":
                        return query.Where(x => x.ComponentItem.ItemName.Contains(val));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.RCDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.RCDateNow <= toDate);
                        break;

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "RCStatus":
                        return ApplyRCStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<RouteCard> ApplyStatusFilter(
                IQueryable<RouteCard> query, string status)
            {
                return status switch
                {
                    "Pending" => query.Where(x => x.RcStatus == 0),
                    "Work-In-Progress" => query.Where(x => x.RcStatus == 1),
                    "Completed" => query.Where(x => x.RcStatus == 2),
                    "Cancelled" => query.Where(x => x.RcStatus == 3),
                    _ => query
                };
            }

            private static IQueryable<RouteCard> ApplyRCStatusFilter(
                IQueryable<RouteCard> query, string status)
            {
                return status switch
                {
                    "Manual" => query.Where(x => x.RcStatus == 1),
                    "Auto" => query.Where(x => x.RcStatus == 0),
                    _ => query
                };
            }
        }


        public async Task UpsertRouteCardSubAsync(RouteCardSubVM subVM)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var subEntity = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.RCSubId == subVM.RCSubId);

                if (subEntity == null)
                {
                    throw new KeyNotFoundException($"RouteCardSub not found for RCSubId: {subVM.RCSubId}");
                }

                subEntity.IsProcessSkip = subVM.IsProcessSkip;
                subEntity.IsBOM = subVM.IsBOM;
                subEntity.IsInspection = subVM.IsInspection;
                subEntity.IsFinalProcess = subVM.IsFinalProcess;

                await _unitOfWork.RouteCardSubs.UpdateAsync(subEntity);

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch (KeyNotFoundException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertRouteCardSubAsync] SubEntity Not Found");
                throw;
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertRouteCardSubAsync] Invalid operation during update");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Unexpected error in UpsertRouteCardSubAsync for RCSubId {subVM.RCSubId}");
                throw new InvalidOperationException("Failed to update route card process. Please contact support.");
            }
        }

        public async Task<List<RouteCardSubVM>> GetRouteCardSubsByRCIdAsync(int rcId)
        {
            try
            {
                var entityList = await _unitOfWork.RouteCardSubs.GetQueryable()
                    .Include(x => x.Process)
                    .Include(x => x.Machine)
                    .Where(x => x.RCId == rcId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                if (entityList == null || entityList.Count == 0)
                    return new List<RouteCardSubVM>();

                var result = _mapper.Map<List<RouteCardSubVM>>(entityList);
                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading RouteCardSubs for RCId: {rcId}");
                throw new InvalidOperationException("Failed to load Route Card processes. Please contact support.");
            }
        }


        public async Task<RouteCardVM?> GetRouteCardByRCIdAsync(int rcId)
        {
            try
            {
                var query = _unitOfWork.RouteCards.GetQueryable()
                    .Include(rc => rc.RouteCardSubs)
                        .ThenInclude(sub => sub.Machine)
                    .Include(rc => rc.RouteCardSubs)
                        .ThenInclude(sub => sub.Vendor)
                    .Include(rc => rc.RcRelease)
                    .Include(rc => rc.Customer)
                    .Include(rc => rc.AssemblyItem)
                    .Include(rc => rc.SubAssemblyItem)
                    .Include(rc => rc.SubAssemblyItem2)
                    .Include(rc => rc.SubAssemblyItem3)
                    .Include(rc => rc.ComponentItem)
                    .Include(rc => rc.RawMaterialItem)
                    .Include(rc => rc.MfgPo);

                var entity = await query.FirstOrDefaultAsync(rc => rc.RCId == rcId);

                if (entity == null)
                    return null;

                var sortedSubs = entity.RouteCardSubs
                    .OrderBy(x => x.SeqNo)
                    .ThenBy(x => x.RCSubId)
                    .ToList();

                entity.RouteCardSubs = sortedSubs;

                var rcVM = _mapper.Map<RouteCardVM>(entity);

                if (rcVM.RMId.HasValue && rcVM.RMId.Value > 0)
                {
                    rcVM.RMStockQty = await _stockManagerService.GetStockForItemAsync(rcVM.RMId.Value, null);
                }

                return rcVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetRouteCardByRCIdAsync({rcId})");
                return null;
            }
        }

        public async Task UpdateItemCancelAsync(JobOrderSubVM subItem)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.JobOrderSubs.GetQueryable().Where
                                (x => x.JobSubId == subItem.JobSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with JobSubId {subItem.JobSubId} not found.");

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.JobOrderSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdateItemCancelAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAsync for ItemCode {subItem.ItemCode}");
                throw new InvalidOperationException("Failed to update Item cancel/revert status. Please contact support.");
            }
        }


        public async Task<List<JobOrderSubVM>> GetJobOrderSubByJobIdAsync(int jobId)
        {
            try
            {
                var subs = await _unitOfWork.JobOrderSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.JobId == jobId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<JobOrderSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Job Order items for JobId: {jobId}");
                throw new InvalidOperationException("Failed to retrieve Job Order sub-items. Please try again.");
            }
        }

        public async Task<bool> UpdatedCancelStatusAsync(JobOrderVM jobVM, string cancelReason)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingJobOrder = await _unitOfWork.JobOrders.GetAsync(jobVM.JobId);
                if (existingJobOrder == null)
                    return false;

                if (!jobVM.Cancel)
                {
                    existingJobOrder.Cancel = true;
                    existingJobOrder.CancelReason = cancelReason;
                }
                else
                {
                    existingJobOrder.Cancel = false;
                    existingJobOrder.CancelReason = string.Empty;
                }
                await _unitOfWork.JobOrders.UpdateAsync(existingJobOrder);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Unexpected error");
                return false;
            }
        }


        public async Task AdjustPoRouteCardBalanceAsync(int? poId, int? compItemId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if ((!poId.HasValue || poId == 0) && (!compItemId.HasValue || compItemId == 0)) return;

                var poSub = await _unitOfWork.MfgPoSubs.GetQueryable().Where(X => X.PoId == poId && X.ItemId == compItemId).FirstOrDefaultAsync();
                if (poSub == null) return;

                if (oldQty > 0)
                    poSub.RouteCardBalQty += oldQty;

                if (newQty > poSub.RouteCardBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed PO RouteCard balance Qty.");

                if (newQty > 0)
                    poSub.RouteCardBalQty -= newQty;

                await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoRouteCardBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoRouteCardBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust Po RouteCard Balance Qty. Please contact support.");
            }
        }

        public async Task AdjustRcReleaseComponentTrackerAsync(int? rcReleaseId, int? assyId, int? compItemId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (rcReleaseId.GetValueOrDefault() <= 0 ||
                    assyId.GetValueOrDefault() <= 0 ||
                    compItemId.GetValueOrDefault() <= 0)
                {
                    return;
                }

                var routeCardReleaseSub = await _unitOfWork.RcReleaseSubs
                    .GetQueryable()
                    .Include(x => x.RouteCardRelease)
                    .FirstOrDefaultAsync(x =>
                        x.RouteCardReleaseId == rcReleaseId.Value &&
                        x.ComponentItemId == compItemId.Value &&
                        x.RouteCardRelease.AssemblyItemId == assyId.Value);

                if (routeCardReleaseSub == null)
                    return;

                decimal diffQty = newQty - oldQty;

                if (diffQty > 0 && diffQty > routeCardReleaseSub.RemainingQty)
                {
                    throw new InvalidOperationException($"{context}: Qty cannot exceed PO Component Remaining balance Qty.");
                }

                if (diffQty != 0)
                {
                    if (diffQty > 0)
                    {
                        routeCardReleaseSub.RemainingQty -= diffQty;
                        routeCardReleaseSub.IssuedQty += diffQty;
                    }
                    else
                    {
                        var absDiff = Math.Abs(diffQty);

                        routeCardReleaseSub.RemainingQty += absDiff;
                        routeCardReleaseSub.IssuedQty -= absDiff;

                        if (routeCardReleaseSub.IssuedQty < 0)
                            routeCardReleaseSub.IssuedQty = 0;
                    }
                }

                routeCardReleaseSub.IsCompleted = routeCardReleaseSub.RemainingQty == 0;

                await _unitOfWork.RcReleaseSubs.UpdateAsync(routeCardReleaseSub);

                var totalBalQty = await _unitOfWork.RcReleaseSubs
                    .GetQueryable()
                    .Where(e => e.RouteCardReleaseId == rcReleaseId.Value)
                    .SumAsync(e => (decimal?)e.RemainingQty) ?? 0;

                var rcRelease = await _unitOfWork.RcReleases.GetAsync(rcReleaseId.Value);

                if (rcRelease != null)
                {
                    rcRelease.RcReleaseTally = totalBalQty == 0;
                    await _unitOfWork.RcReleases.UpdateAsync(rcRelease);
                }

                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustRcReleaseComponentTrackerAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustRcReleaseComponentTrackerAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust Route Card Component Remaining Qty. Please contact support.");
            }
        }


        public async Task<decimal?> GetMfgPoRCBalQtyByRefPoIdAndItemId(int poId, int itemId)
        {
            try
            {
                var query = _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(x => x.PoId == poId && x.ItemId == itemId);

                var result = await query
                    .Select(x => x.RouteCardBalQty)
                    .FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetMfgPoWOBalQtyByRefPoSubId → PoId: {poId}, ItemId: {itemId}");
                return null;
            }
        }

        public async Task<decimal?> GetRcSubProcessBalQtyByRcId(int rcId)
        {
            try
            {
                var balQty = await _unitOfWork.RouteCardSubs.GetQueryable()
                                .Where(x => x.RCId == rcId && x.SeqNo == 1)
                                .Select(x => x.BalQty)
                                .FirstOrDefaultAsync();

                return balQty;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetRcSubProcessBalQtyByRcId for RCId: {rcId}");
                return null;
            }
        }

        public async Task<decimal?> GetExistingRCQtyByRoutecard(int rcId)
        {
            try
            {
                var balQty = await _unitOfWork.RouteCards.GetQueryable()
                                .Where(x => x.RCId == rcId)
                                .Select(x => x.RcQty)
                                .FirstOrDefaultAsync();

                return balQty;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetRcSubProcessBalQtyByRcId for RCId: {rcId}");
                return null;
            }
        }

        public async Task<RouteCardVM> UpsertRouteCardAsync(RouteCardVM routeCardVM)
        {
            if (routeCardVM == null)
                throw new ArgumentNullException(nameof(routeCardVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                RouteCard entity;

                if (routeCardVM.RCId == 0)
                {
                    entity = _mapper.Map<RouteCard>(routeCardVM);

                    var NextNumber = await _unitOfWork.RouteCards.GetLastRcNoAsync(entity.Suffix);
                    entity.RCNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.RouteCardSubs = routeCardVM.RouteCardSubVMs.Select(s => _mapper.Map<RouteCardSub>(s)).ToList();

                    await _unitOfWork.RouteCards.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Route Card Created.");

                    if ((entity.RefPoId > 0 || entity.RefRcReleseId > 0) && entity.CompItemId > 0)
                    {
                        if (entity.SelectedItemType == "COMP")
                        {
                            await AdjustPoRouteCardBalanceAsync(entity.RefPoId, entity.CompItemId, 0, entity.RcQty, "Route Card Created");
                        }
                        else
                        {
                            await AdjustRcReleaseComponentTrackerAsync(entity.RefRcReleseId, entity.AssyId, entity.CompItemId, 0, entity.RcQty, "Route Card Created");
                        }
                    }
                }
                else
                {
                    entity = await _unitOfWork.RouteCards.GetQueryable()
                        .Include(q => q.RouteCardSubs)
                        .FirstOrDefaultAsync(q => q.RCId == routeCardVM.RCId)
                        ?? throw new InvalidOperationException("Route Card not found.");

                    var parentChanges = GetPropertyChanges(entity, routeCardVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);


                    if ((entity.RefPoId.GetValueOrDefault() > 0 || entity.RefRcReleseId.GetValueOrDefault() > 0)
                        && entity.CompItemId.GetValueOrDefault() > 0)
                    {
                        if (entity.SelectedItemType == "COMP")
                        {
                            await AdjustPoRouteCardBalanceAsync(entity.RefPoId, entity.CompItemId, entity.RcQty, routeCardVM.RcQty, "Route Card Updated");
                        }
                        else
                        {
                            await AdjustRcReleaseComponentTrackerAsync(entity.RefRcReleseId, entity.AssyId, entity.CompItemId, entity.RcQty, routeCardVM.RcQty, "Route Card Updated");
                        }
                    }

                    _mapper.Map(routeCardVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, routeCardVM.RouteCardSubVMs, changes);

                    changes.AppendLine("Route card Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateRcProcessTallyStatusAsync(entity.RouteCardSubs);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, routeCardVM.RCId == 0 ? "Route Card Created" : "Route Card Updated");

                var savedEntity = await _unitOfWork.RouteCards.GetQueryable()
                    .Include(q => q.RouteCardSubs).ThenInclude(s => s.Process)
                    .Include(q => q.Customer)
                    .Include(q => q.RouteCardSubs)
                    .FirstOrDefaultAsync(q => q.RCId == entity.RCId);

                return _mapper.Map<RouteCardVM>(savedEntity!);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Route Card : {routeCardVM.RCNo}");
                throw new InvalidOperationException("Failed to save Route Card. Please try again.");
            }
        }

        private async Task UpdateRcProcessTallyStatusAsync(IEnumerable<RouteCardSub> subs)
        {
            try
            {
                if (subs == null || !subs.Any())
                    return;

                foreach (var process in subs)
                {
                    byte status = await GetProcessStatusAsync(process);
                    process.ProcessStatus = status;
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Failed in UpdateRcProcessTallyStatusAsync()");
                throw;
            }
        }

        private async Task<byte> GetProcessStatusAsync(RouteCardSub routeCardProcess)
        {
            try
            {
                if (routeCardProcess == null)
                    throw new ArgumentNullException(nameof(routeCardProcess));

                decimal balQty = routeCardProcess.BalQty;
                decimal accQty = routeCardProcess.AccQty;
                decimal rejQty = routeCardProcess.RejQty;
                decimal rewQty = routeCardProcess.RewQty;
                decimal issuedQty = routeCardProcess.IssuedQty;

                decimal usedQty = accQty + rejQty + rewQty;

                if (issuedQty == 0 && usedQty == 0 && balQty > 0)
                    return 0;

                if (balQty == 0 && usedQty > 0)
                    return 3;

                if ((rejQty > 0 || rewQty > 0) && balQty > 0)
                    return 2;

                if ((usedQty > 0 || issuedQty > 0) && balQty > 0)
                    return 1;

                return 0;
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Error in GetProcessStatusAsync | RCId: {routeCardProcess?.RCId}, SeqNo: {routeCardProcess?.SeqNo}");
                throw new InvalidOperationException("Error calculating process status", ex);

            }
        }



        private async Task HandleChildUpdatesAsync(RouteCard existingRC, List<RouteCardSubVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingRC.RouteCardSubs.Select(s => s.RCSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.RCSubId).ToHashSet();

            foreach (var sub in existingRC.RouteCardSubs.Where(s => !incomingSubIds.Contains(s.RCSubId)).ToList())
            {
                if ((existingRC.RefPoId > 0 || existingRC.RefRcReleseId > 0) && existingRC.CompItemId > 0)
                {
                    if (existingRC.SelectedItemType == "COMP")
                    {
                        await AdjustPoRouteCardBalanceAsync(existingRC.RefPoId, existingRC.CompItemId, existingRC.RcQty, 0, "Route Card Delete");
                    }
                    else
                    {
                        await AdjustRcReleaseComponentTrackerAsync(existingRC.RefRcReleseId, existingRC.AssyId, existingRC.CompItemId, existingRC.RcQty, 0, "Route Card Delete");
                    }
                }

                changes.AppendLine($"Child Deleted - RcSubId: {sub.RCSubId}, Process : {sub.Process?.ProcessName}");
                await _unitOfWork.RouteCardSubs.DeleteAsync(sub.RCSubId);
                await _unitOfWork.SaveAsync();

            }

            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.RCSubId == 0)
                {
                    var newSub = _mapper.Map<RouteCardSub>(subVM);
                    newSub.RCId = existingRC.RCId;
                    await _unitOfWork.RouteCardSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - Process: {subVM.ProcessName}");

                    if ((existingRC.RefPoId > 0 || existingRC.RefRcReleseId > 0) && existingRC.CompItemId > 0)
                    {
                        if (existingRC.SelectedItemType == "COMP")
                        {
                            await AdjustPoRouteCardBalanceAsync(existingRC.RefPoId, existingRC.CompItemId, 0, existingRC.RcQty, "Route Card Created");
                        }
                        else
                        {
                            await AdjustRcReleaseComponentTrackerAsync(existingRC.RefRcReleseId, existingRC.AssyId, existingRC.CompItemId, 0, existingRC.RcQty, "Route Card Created");
                        }
                    }
                }
                else
                {
                    var existingSub = existingRC.RouteCardSubs.FirstOrDefault(s => s.RCSubId == subVM.RCSubId);
                    if (existingSub != null)
                    {
                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ProcessName {subVM.ProcessName}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        public async Task<List<MfgPoVM>> GetPendingPOsByCustomerAsync(int custId,string type)
        {
            //27062026 commented
            //try
            //{
            //    return await _unitOfWork.MfgPos.GetQueryable()
            //       .Include(p => p.MfgPoSubs)
            //       .Where(p => p.CustId == custId && !p.PoTally && !p.PoCancl
            //                && p.MfgPoSubs.Any(ps => ps.RouteCardBalQty > 0 && !ps.ItemCancel))
            //       .Select(p => new MfgPoVM
            //       {
            //           PoId = p.PoId,
            //           PONo = $"{p.PONo}{p.Suffix}",
            //       })
            //       .ToListAsync();
            //}
            //catch (Exception ex)
            //{
            //    await _logs.LogDeveloperError(ex, $"Failed to GetPendingPOsByCustomerAsync");
            //    throw;
            //}
            try
            {
                var query = _unitOfWork.MfgPos.GetQueryable()
                    .Include(p => p.MfgPoSubs)
                    .Where(p => p.CustId == custId
                             && !p.PoTally
                             && !p.PoCancl
                             && p.MfgPoSubs.Any(ps => ps.RouteCardBalQty > 0 && !ps.ItemCancel));

                // Apply filter based on type
                if (!string.IsNullOrWhiteSpace(type))
                {
                    if (type.Equals("Sales", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(p => p.MfgORLab == true);
                    }
                    else if (type.Equals("Labour", StringComparison.OrdinalIgnoreCase))
                    {
                        query = query.Where(p => p.MfgORLab == false);
                    }
                }

                return await query
                    .Select(p => new MfgPoVM
                    {
                        PoId = p.PoId,
                        PONo = $"{p.PONo}{p.Suffix}"
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to GetPendingPOsByCustomerAsync");
                throw;
            }
        }
        public async Task<List<RouteCardReleaseVM>> GetPendingAssyRcReleasesByCustomerAsync(int custId)
        {
            try
            {
                return await _unitOfWork.RcReleases.GetQueryable()
                    .Where(p => p.CustId == custId
                                && !p.RcReleaseTally
                                && p.MfgPo != null)
                    .Select(p => new RouteCardReleaseVM
                    {
                        RouteCardReleaseId = p.RouteCardReleaseId,
                        RcReleaseNo = (p.RcReleaseNo ?? "") + (p.Suffix ?? "")
                    })
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Failed to GetPendingAssyPOsByCustomerAsync");
                throw;
            }
        }

        public async Task<List<JobOrderAssyPoItemVM>> GetAssemblyItemsByPoIdAsync(int poId)
        {
            try
            {
                // 1. Get PO Assembly Items
                var poItems = await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(ps => ps.PoId == poId
                             && ps.WOBalQty > 0
                             && !ps.ItemCancel
                             && !ps.MfgPo.PoTally
                             && !ps.MfgPo.PoCancl)
                    .Join(_unitOfWork.ItemRepositories.GetQueryable(),
                        ps => ps.ItemId,
                        i => i.ItemId,
                        (ps, i) => new { ps, i })
                    .Where(x => x.i.CategoryCode == 3)
                    .Select(x => new JobOrderAssyPoItemVM
                    {
                        RefPoSubId = x.ps.PoSubId,
                        RefPoNo = $"{x.ps.MfgPo.PONo}{x.ps.MfgPo.Suffix}",
                        RefPoDate = x.ps.MfgPo.PODate,
                        AssyItemId = x.ps.ItemId,
                        AssyItemCode = x.i.ItemCode,
                        PoQty = x.ps.Qty,
                        WoBalQty = x.ps.WOBalQty,
                        ReqQty = x.ps.WOBalQty,
                        StockQty = 0m, // to fill after
                        CostId = x.ps.CostId,
                        ProjectNo = x.ps.CostCenter != null ? x.ps.CostCenter.ProjectNo : null
                    })
                    .ToListAsync();

                if (!poItems.Any())
                    return poItems;

                var itemIds = poItems
                    .Select(x => x.AssyItemId.Value)
                    .Distinct()
                    .ToList();

                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, null);

                foreach (var item in poItems)
                {
                    if (item.AssyItemId.HasValue && stockDict.TryGetValue(item.AssyItemId.Value, out var stock))
                        item.StockQty = stock;
                    else
                        item.StockQty = 0;
                }

                return poItems;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed: GetAssemblyItemsByPoIdAsync");
                throw;
            }
        }

        public async Task<List<JobOrderPreviewVM>> GetBOMHierarchyAsync(List<int> assyIds, List<JobOrderAssyPoItemVM> selectedAssys)
        {
            var result = new List<BOMItem>();

            try
            {
                async Task AddChildrenAsync(int assyId, decimal reqParentQty, int refPoSubId, string refPoNo, decimal poQty, int? CostId, string? ProjectNo)
                {
                    var children = await _unitOfWork.AssmblyDefs.GetQueryable()
                        .Include(a => a.PartItem)
                        .Where(a => a.AssmblyID == assyId && a.PartItem != null)
                        .Select(a => new BOMItem
                        {
                            Id = a.Id,
                            AssmblyID = a.AssmblyID,
                            ItemID = a.ItemId,
                            UtilQty = a.UtilQty,
                            ItemCode = a.PartItem.ItemCode,
                            ItemName = a.PartItem.ItemName,
                            categoryCode = a.PartItem.CategoryCode,
                            MeasureUnit = a.PartItem.MeasureUnit,
                            ReqQty = a.UtilQty * reqParentQty,

                            RefPoSubId = refPoSubId,
                            RefPoNo = refPoNo,
                            PoQty = poQty,
                            JobOrderQty = reqParentQty,
                            CostId = CostId,
                            ProjectNo = ProjectNo
                        })
                        .ToListAsync();

                    foreach (var child in children)
                    {
                        result.Add(child);
                        await AddChildrenAsync(child.ItemID, reqParentQty, refPoSubId, refPoNo, poQty, CostId, ProjectNo);
                    }
                }

                foreach (var assy in selectedAssys)
                {
                    await AddChildrenAsync(
                        assy.AssyItemId!.Value,
                        assy.ReqQty,
                        assy.RefPoSubId.GetValueOrDefault(),
                        assy.RefPoNo,
                        assy.PoQty,
                        assy.CostId,
                        assy.ProjectNo
                    );
                }

                var filtered = result.Where(r => r.categoryCode == 3 || r.categoryCode == 7).ToList();

                string fySuffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);
                int nextNo = await GetPreviewRcNoAsync(fySuffix);

                var itemIds = filtered.Select(x => x.ItemID).Distinct().ToList();
                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, null);

                var final = filtered.Select((x, index) => new JobOrderPreviewVM
                {
                    TempJobNo = $"{(nextNo + index).ToString("D3")}{fySuffix}",
                    SubAssyId = x.ItemID,
                    SubAssyCode = $"{x.ItemCode} - {x.ItemName}",
                    MeasureUnit = x.MeasureUnit ?? "",
                    UtilQty = x.UtilQty,
                    ReqQty = x.ReqQty,
                    StockQty = stockDict.ContainsKey(x.ItemID) ? stockDict[x.ItemID] : 0,
                    IsSelected = true,

                    RefPoSubId = x.RefPoSubId,
                    RefPoNo = x.RefPoNo,
                    PoQty = x.PoQty,
                    JobOrderQty = x.JobOrderQty,

                    CostId = x.CostId,
                    ProjectNo = x.ProjectNo

                }).ToList();

                return final;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while loading BOM for Job Order Preview");
                return new List<JobOrderPreviewVM>();
            }
        }


        public async Task<List<JobOrder>> GetReferenceJobListByParentJobId(int parentJobId, int refPoSubId)
        {
            try
            {
                var jobOrders = await _unitOfWork.JobOrders
                    .GetQueryable()
                    .Include(j => j.JobOrderSubs)
                    .Where(j =>
                        j.ParentJobId == parentJobId &&
                        j.RefPoSubId == refPoSubId)
                    .ToListAsync();

                return jobOrders;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error in GetReferenceJobListByParentJobId(parentJobId: {parentJobId}, refPoSubId: {refPoSubId})"
                );

                return new List<JobOrder>();
            }
        }

        public async Task<bool> HasProductionIssueForJobAsync(int itemId, int jobId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .AnyAsync(s => s.AssyId == itemId &&
                                   s.RefJobId == jobId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasProductionIssueForJobAsync (itemId: {itemId}, jobId: {jobId})");
                return false;
            }
        }

        public async Task<List<BOMItem>> GetBOMItemsByAssyIdAsync(int assyId)
        {
            return await _unitOfWork.AssmblyDefs.GetQueryable()
                .Where(x => x.AssmblyID == assyId && !x.Cancel)
                .OrderBy(x => x.SlNo)
                .Select(x => new BOMItem
                {
                    ItemID = x.ItemId,
                    UtilQty = x.UtilQty
                })
                .ToListAsync();
        }

        public async Task<int> GetPreviewRcNoAsync(string suffix)
        {
            var lastNo = await _unitOfWork.RouteCards.GetQueryable()
                .Where(x => x.Suffix == suffix)
                .OrderByDescending(x => x.RCId)
                .Select(x => x.RCNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastNo))
            {
                var parts = lastNo.Split("/");
                nextNumber = int.Parse(parts.Last()) + 1;
            }
            return nextNumber;
        }

        public async Task<List<JobOrderSubVM>> GetItemDetailsByAssySubAssyIdAsync(int assyId, decimal joQty)
        {
            var bomItems = await _unitOfWork.AssmblyDefs.GetQueryable()
                .Where(x => x.AssmblyID == assyId)
                .Include(x => x.PartItem)
                .ToListAsync();

            var itemIds = bomItems.Select(x => x.ItemId).Distinct().ToList();

            var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, null);

            var result = bomItems.Select((x, index) => new JobOrderSubVM
            {
                SlNo = index + 1,
                ItemId = x.ItemId,
                ItemCode = x.PartItem?.ItemCode,
                ItemName = x.PartItem?.ItemName,
                Measureunit = x.PartItem?.MeasureUnit,

                UtilQty = x.UtilQty,
                ReqQty = x.UtilQty * joQty,
                BalQty = x.UtilQty * joQty,

                stockQty = stockDict.TryGetValue(x.ItemId, out var stock) ? stock : 0,

                ItemCancel = false
            }).ToList();

            return result;
        }

        public async Task<bool> CancelJobOrdersRecursiveAsync(List<int> jobIds, JobOrderVM ParentJobOrderVM, string cancelReason)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {

                if (!ParentJobOrderVM.Cancel)
                {

                    foreach (var jobId in jobIds)
                    {
                        var jobOrder = await _unitOfWork.JobOrders
                            .GetQueryable()
                            .Include(e => e.JobOrderSubs)
                            .FirstOrDefaultAsync(e => e.JobId == jobId);

                        if (jobOrder == null)
                            continue;

                        jobOrder.Cancel = true;
                        jobOrder.CancelReason = cancelReason;

                        await _unitOfWork.JobOrders.UpdateAsync(jobOrder);
                        await _unitOfWork.SaveAsync();

                        await _logs.LogUserAction(
                           UserName: await _currentUserService.GetUsernameAsync(),
                           Machine: _currentUserService.MachineName,
                           IP_Address: _currentUserService.IpAddress,
                           screen: "Job Order",
                           action: $"Cancelled Job Order No: {jobOrder.JobNo}",
                           additionalInfo: $"Job Id: {jobOrder.JobId}");
                    }

                    var isPOAssyItem = await _unitOfWork.MfgPoSubs.GetQueryable()
                                        .AnyAsync(p => p.PoId == ParentJobOrderVM.RefPoId && p.ItemId == ParentJobOrderVM.AssyId);
                    //if (isPOAssyItem)
                    //{
                    //    await AdjustPoWorkOrderBalanceAsync(ParentJobOrderVM.RefPoSubId, ParentJobOrderVM.JobOrderQty, 0, "Job order Cancel");
                    //}

                }
                else
                {
                    foreach (var jobId in jobIds)
                    {
                        var jobOrder = await _unitOfWork.JobOrders
                            .GetQueryable()
                            .Include(e => e.JobOrderSubs)
                            .FirstOrDefaultAsync(e => e.JobId == jobId);

                        if (jobOrder == null)
                            continue;

                        jobOrder.Cancel = false;
                        jobOrder.CancelReason = string.Empty;

                        await _unitOfWork.JobOrders.UpdateAsync(jobOrder);
                        await _unitOfWork.SaveAsync();

                        await _logs.LogUserAction(
                        UserName: await _currentUserService.GetUsernameAsync(),
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Job Order",
                        action: $"Reverted Job Order No: {jobOrder.JobNo}",
                        additionalInfo: $"Job Id: {jobOrder.JobId}");

                    }

                    var isPOAssyItem = await _unitOfWork.MfgPoSubs.GetQueryable()
                                        .AnyAsync(p => p.PoId == ParentJobOrderVM.RefPoId && p.ItemId == ParentJobOrderVM.AssyId);
                    //if (isPOAssyItem)
                    //{
                    //    await AdjustPoWorkOrderBalanceAsync(ParentJobOrderVM.RefPoSubId, 0, ParentJobOrderVM.JobOrderQty, "Job order Revert");
                    //}

                }


                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed recursive job order delete");
                return false;
            }
        }

        public async Task<bool> DeleteJobOrdersRecursiveAsync(List<int> jobIds, JobOrderVM? parentJobVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var jobId in jobIds)
                {
                    var jobOrder = await _unitOfWork.JobOrders
                        .GetQueryable()
                        .Include(e => e.JobOrderSubs)
                        .FirstOrDefaultAsync(e => e.JobId == jobId);

                    if (jobOrder == null)
                        continue;

                    await _unitOfWork.JobOrders.DeleteAsync(jobOrder);
                }
                await _unitOfWork.SaveAsync();


                if (parentJobVM != null)
                {
                    var isPOAssyItem = await _unitOfWork.MfgPoSubs.GetQueryable()
                                        .AnyAsync(p => p.PoId == parentJobVM.RefPoId && p.ItemId == parentJobVM.AssyId);
                    //if (isPOAssyItem)
                    //{
                    //    await AdjustPoWorkOrderBalanceAsync(parentJobVM.RefPoSubId, parentJobVM.JobOrderQty, 0, "Job order Delete");
                    //}
                }


                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed recursive job order delete");
                return false;
            }
        }

        public async Task<bool> DeleteJobOrderByJobIdAsync(int jobId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var jobOrder = await _unitOfWork.JobOrders
                    .GetQueryable()
                    .Include(e => e.JobOrderSubs)
                    .FirstOrDefaultAsync(e => e.JobId == jobId);

                if (jobOrder == null)
                    return false;

                var changes = new StringBuilder();

                var isPOAssyItem = await _unitOfWork.MfgPoSubs.GetQueryable()
                                    .AnyAsync(p => p.PoId == jobOrder.RefPoId && p.ItemId == jobOrder.AssyId);

                //if (isPOAssyItem)
                //{
                //    await AdjustPoWorkOrderBalanceAsync(jobOrder.RefPoSubId, jobOrder.JobOrderQty, 0, "Job order Delete");
                //}

                await _unitOfWork.JobOrders.DeleteAsync(jobOrder);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Job Order List",
                    action: $"Deleted Job Order: {jobOrder.JobNo}",
                    additionalInfo: $"Job Id: {jobOrder.JobId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Job Order. JobId: {jobId}");
                throw;
            }
        }

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
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<bool> IsPoExists(int poId)
        {
            try
            {
                var exists = await _unitOfWork.MfgPoSubs.GetQueryable()
                    .AnyAsync(j => j.PoId == poId);

                return exists;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error checking PO existence for RefPoId: {poId}");
                return false;
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteJobOrderAsync(int jobId)
        {
            try
            {
                var jobOrder = await _unitOfWork.JobOrders.GetQueryable().Where(j => j.JobId == jobId).FirstOrDefaultAsync();

                if (jobOrder == null)
                    return (true, "Job Order can be safely deleted.");

                bool hapProductionIssueAssyExists = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefJobId == jobOrder.JobId && qs.AssyId == jobOrder.AssyId);

                if (hapProductionIssueAssyExists)
                    return (false, "Cannot delete this Job Order as a Production Assembly Issue transaction exists.");

                return (true, "Job Order can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteJobOrderAsync for JobId: {jobId}");
                throw new Exception("Error checking Job Order delete eligibility", ex);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetExistingBOMDetailsAsync(int jobId)
        {
            try
            {
                var result = await _unitOfWork.JobOrderSubs.GetQueryable()
                    .Where(j => j.JobId == jobId)
                    .Select(j => new
                    {
                        j.ItemId,
                        j.Item.ItemCode,
                        j.Item.ItemName,
                        j.Item.MeasureUnit,
                        j.UtilQty
                    })
                    .ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode,
                    ["ItemName"] = r.ItemName,
                    ["UOM"] = r.MeasureUnit,
                    ["UtilQty"] = r.UtilQty,

                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching Existing BOM Details");
                throw new InvalidOperationException("Failed to retrieve Existing BOM Details. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetLatestBOMFromAssemblyDefAsync(int AssyId)
        {
            try
            {
                var result = await _unitOfWork.AssmblyDefs.GetQueryable()
                    .Where(j => j.AssmblyID == AssyId)
                    .Select(j => new
                    {
                        j.ItemId,
                        j.PartItem.ItemCode,
                        j.PartItem.ItemName,
                        j.PartItem.MeasureUnit,
                        j.UtilQty
                    })
                    .ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode,
                    ["ItemName"] = r.ItemName,
                    ["UOM"] = r.MeasureUnit,
                    ["UtilQty"] = r.UtilQty,

                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching Existing BOM Details");
                throw new InvalidOperationException("Failed to retrieve Existing BOM Details. Please try again.");
            }
        }

    }
}
