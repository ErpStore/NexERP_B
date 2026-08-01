using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Inventory_Stock_.MaterialIssueNote;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.InventoryStockRepository;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.MaterialIssueNoteVM;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.InventoryViewModel.StockIssueRequestVM;
using V.SMART.Shared.Data.Inventory_Stock_.StockIssueRequest;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService
{
    public class StockIssueRequestService : IStockIssueRequestService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public StockIssueRequestService(
            IUnitOfWork unitOfWork,
            IStockManagerService stockManagerService,
            ICommonService commonService,
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

        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        // 🔹 Correspondence Attachments Count
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        // 🔹 Items
        public async Task<List<ItemVM>> GetItemVMsByItemIdsAsync(List<int> itemIds)
        {
            return await _commonService.GetItemVMsByItemIdsAsync(itemIds);
        }

        public async Task<Dictionary<string, int>> GetItemIdsByItemCodesAsync(List<string> itemCodes)
        {
            return await _commonService.GetItemIdsByItemCodesAsync(itemCodes);
        }
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        // 🔹 Stores
        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
           => await _commonService.GetAllIssueStoresAsync();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        // 🔹 Assembly/ Sub assembly Related
        public async Task<List<ItemVM>> GetAllAssembliesAsync()
            => await _commonService.GetAllAssembliesAsync();

        public async Task<List<ItemVM>> GetAllSubAssembliesAsync()
            => await _commonService.GetAllSubAssembliesAsync();

        public async Task<List<ItemVM>> GetAllSubAssembliesByAssyIdAsync(int assyId)
            => await _commonService.GetAllSubAssembliesByAssyIdAsync(assyId);

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        //Stock
        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId)
        {
            return await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);
        }

        // 🔹 CostCenter
        public async Task<List<CostCenterVM>> GetAllCostCenterDetails()
           => await _commonService.GetAllCostCenterDetails();

        //Uom
        public async Task<List<UOM>> GetUOMsAsync()
        {
            return await _commonService.GetUOMsAsync();
        }

        public async Task<Companydetails?> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();




        // 🔹 Material Issue Note Operations operations

        public async Task<(List<StockIssueRequestVM> minVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.StockRequestIssues
                    .GetQueryable()
                    .Include(x => x.AssyItem)
                    .Include(x=>x.Store)
                    .Include(x => x.StockIssueRequestSubs)
                        .ThenInclude(s => s.Item)
                    .Include(x => x.StockIssueRequestSubs)
                        .ThenInclude(s => s.CostCenter)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = MinFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.RequestId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<StockIssueRequestVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Material Issue Note)");
                throw new InvalidOperationException("Failed to load Material Issue Note list.", ex);
            }
        }

        public static class MinFilterBuilder
        {
            public static IQueryable<StockIssueRequest> ApplyFilter(
                IQueryable<StockIssueRequest> query,
                string field,
                object value)
            {
                try
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return query;

                    string val = value.ToString()!.Trim();

            

                    switch (field)
                    {
                        case "RequestNo":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(x => (string.IsNullOrEmpty(part1) || x.RequestNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2)))
                                );
                            }

                        case "AssemblyCode":
                            return query.Where(x =>
                                x.AssyItem != null &&
                                x.AssyItem.ItemCode.Contains(val));

                        case "AssemblyName":
                            return query.Where(x =>
                                x.AssyItem != null &&
                                x.AssyItem.ItemName.Contains(val));

                        case "ItemName":
                            return query.Where(x =>
                                x.StockIssueRequestSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemName.Contains(val)));

                        case "ItemCode":
                            return query.Where(x =>
                                x.StockIssueRequestSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemCode.Contains(val)));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.RequestDateNow >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.RequestDateNow <= toDate.Date.AddDays(1).AddTicks(-1));

                            return query;
                        case "Status":
                            return ApplyStatusFilter(query, val.ToString());
                        case "ApprovalStatus":
                            return ApplyApprovalStatusFilter(query, val);


                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }
        }
        private static IQueryable<StockIssueRequest> ApplyStatusFilter(
              IQueryable<StockIssueRequest> query, string status)
        {
            try
            {
                return status switch
                {
                    "Completed" =>
                        query.Where(x => x.ReqTally),

                    "Pending" =>
                        query.Where(x =>
                            !x.ReqTally  ),

                    _ => query
                };
            }
            catch
            {
                return query;
            }
        }
        private static IQueryable<StockIssueRequest> ApplyApprovalStatusFilter(
               IQueryable<StockIssueRequest> query, string status)
        {
            return status switch
            {
                "Approved" => query.Where(x => x.IsAuthorized == true),
                "Rejected" => query.Where(x => x.IsRejected == true),
                "Pending" => query.Where(x => x.IsAuthorized == false && x.IsRejected == false),
                _ => query
            };
        }

        public async Task<List<string>> GetAllExistingUserNamesinMINAsync()
        {
            var result = await _unitOfWork.StockRequestIssues.GetQueryable()
                         .Select(m => m.ToWhom)
                         .Distinct()
                         .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<StockIssueRequestVM>> GetAllMINDetailsAsync()
        {
            try
            {
                var entities = await _unitOfWork.StockRequestIssues.GetAllWithIncludeAsync(q => true,
                    q => q.Store,
                    q => q.AssyItem,
                    q => q.SubAssyItem,
                    q => q.StockIssueRequestSubs);
                return _mapper.Map<IEnumerable<StockIssueRequestVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllMINDetailsAsync");
                return Enumerable.Empty<StockIssueRequestVM>();
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteMINAsync(int minId)
        {
            try
            {
                
                var requestSubIds = await _unitOfWork.StockRequestIssueSubs
                    .GetQueryable()
                    .Where(s => s.RequestId == minId)
                    .Select(s => s.RequestSubId)
                    .ToListAsync();

                if (!requestSubIds.Any())
                    return (true, "Stock Issue Note can be safely deleted.");

                
                bool isTransactionMade = await _unitOfWork.MINSubs
                    .GetQueryable()
                    .AnyAsync(x => x.RefRequestSubId.HasValue &&
                                   requestSubIds.Contains(x.RefRequestSubId.Value));

                if (isTransactionMade)
                {
                    return (false, "Cannot delete. Material Issue Note transaction has already been created for this Stock Request Issue.");
                }

                return (true, "Stock Issue Note can be safely deleted.");

               
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteMINAsync for MINId: {minId}");
                throw new Exception("Error checking Material Issue Note delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteMINByMINIdAsync(int minId, int screenCode)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var min = await _unitOfWork.StockRequestIssues
                    .GetQueryable()
                    .Include(e => e.StockIssueRequestSubs)
                    .FirstOrDefaultAsync(e => e.RequestId == minId);

                if (min == null)
                    return false;

                var changes = new StringBuilder();

                if (min.StockIssueRequestSubs != null && min.StockIssueRequestSubs.Any())
                {
                    foreach (var sub in min.StockIssueRequestSubs.ToList())
                    {
                        await DeleteStockIssueAndTrackAsync(sub.RequestSubId, sub.ItemId.Value, screenCode);

                        await _unitOfWork.StockRequestIssueSubs.DeleteAsync(sub);
                        changes.AppendLine($"Deleted Sub Item: {sub.ItemId}, Qty: {sub.ReqQty}");
                    }
                }

                await _unitOfWork.StockRequestIssues.DeleteAsync(min);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Material Issue Note (Reduction)",
                    action: $"Deleted MIN No: {min.RequestNo}",
                    additionalInfo: $"MIN Id: {min.RequestId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete MIN: {minId}");
                throw;
            }
        }

        public async Task<StockIssueRequestVM?> GetStockIssueRequestById(int minId)
        {
            try
            {
                // 1️⃣ Load entity with all necessary relationships
                var entity = await _unitOfWork.StockRequestIssues.GetQueryable()
                    .Include(m => m.StockIssueRequestSubs)
                    .Include(m => m.StockIssueRequestSubs).ThenInclude(s => s.Item)
                    .Include(m => m.StockIssueRequestSubs).ThenInclude(s => s.CostCenter)
                    .Include(m => m.Store)
                    .Include(m => m.AssyItem)
                    .Include(m => m.SubAssyItem)
                    .Include(m => m.SubAssyItem2)
                    .Include(m => m.SubAssyItem3)
                    .FirstOrDefaultAsync(m => m.RequestId == minId);

                if (entity == null)
                    return null;

                // 2️⃣ Map entity → VM using AutoMapper
                var minVM = _mapper.Map<StockIssueRequestVM>(entity);

                // 3️⃣ Fetch live stock for all ItemIds
                var itemIds = minVM.StockIssueRequestSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && minVM.StoreIssId.HasValue)
                {
                    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, minVM.StoreIssId.Value);

                    foreach (var sub in minVM.StockIssueRequestSubVMs)
                    {
                        if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                            sub.StockQty = qty;
                        else
                            sub.StockQty = 0m;
                    }
                }

                return minVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetMINByMINIdAsync({minId}) failed.");
                return null;
            }
        }


        public async Task<string> GetMINIssueNumberAsync(string suffix)
        {
            try
            {
                var lastSCN = await _unitOfWork.StockRequestIssues
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.RequestNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastSCN != null)
                {
                    var parts = lastSCN.RequestNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating MIN number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate MIN number.");
            }
        }

        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from ss in _unitOfWork.StockRequestIssueSubs.GetQueryable()
                                  where ss.ItemId == itemId
                                  orderby ss.RequestSubId descending
                                  select ss.UnitPrice)
                                    .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId
                                      select isub.Rate)
                                        .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from i in _unitOfWork.ItemRepositories.GetQueryable()
                                      where i.ItemId == itemId
                                      select i.Rate)
                                        .FirstOrDefaultAsync();
                    }

                    result[itemId] = rate;
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for ItemId: {itemIds}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }

        public async Task<List<StockIssueRequestSubVM>> GetAssemblyRelatedItemsAsync(int assyId, decimal ReqQty, int storeId)
        {
            // Get all assembly items with related PartItem
            var assemblyItems = await _unitOfWork.AssmblyDefs.GetQueryable()
                .Include(ad => ad.PartItem)
                .Where(ad => ad.AssmblyID == assyId)
                .ToListAsync();

            var itemIds = assemblyItems
                          .Where(ad => ad.PartItem != null)
                          .Select(ad => ad.PartItem.ItemId)
                          .Distinct()
                          .ToList();

            var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);

            return assemblyItems
               .Where(ad => ad.PartItem != null)
               .Select(ad => new StockIssueRequestSubVM
               {
                   ItemId = ad.PartItem?.ItemId,
                   ItemCode = ad.PartItem?.ItemCode,
                   ItemName = ad.PartItem?.ItemName,
                   UOM = ad.PartItem?.MeasureUnit,
                   UtlQty = ad.UtilQty,
                   ReqQty = ad.UtilQty * ReqQty,
                   UnitPrice = ad.PartItem?.Rate,
                   BatchNo = null,
                   RackNo = ad.PartItem?.RackNo,
                   Make = ad.PartItem?.Make,
                   Remark = null,
                   StockQty = stockDict.TryGetValue(ad.PartItem.ItemId, out var stockQty) ? stockQty : 0m
               })
               .ToList();
        }

        public async Task<StockIssueRequestSubVM?> GetMINSubItemDetailByMINSubIdAsync(int minSubId)
        {
            try
            {
                return await _unitOfWork.StockRequestIssueSubs
                    .GetQueryable()
                    .Where(q => q.RequestSubId == minSubId)
                    .Select(q => new StockIssueRequestSubVM
                    {
                        ReqQty = q.ReqQty,
                        ReqBalQty=q.ReqBalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching scn sub item detail for MinSubid: {minSubId}");
                throw new InvalidOperationException("Failed to retrieve Min sub-item details.");
            }
        }

        public async Task DeleteAndResequenceAsync(StockIssueRequestSubVM subitem, StockIssueRequestVM minVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.RequestSubId > 0)
                {
                    var existingMINSubItem = await _unitOfWork.StockRequestIssueSubs.GetAsync(subitem.RequestSubId);
                    if (existingMINSubItem == null)
                        throw new InvalidOperationException("Sub item not found.");

                    //Delete StockIssue AndTrack And BalUpdate
                    await DeleteStockIssueAndTrackAsync(existingMINSubItem.RequestSubId, existingMINSubItem.ItemId.Value, screenCode);

                    // Delete from DB
                    await _unitOfWork.StockRequestIssueSubs.DeleteAsync(existingMINSubItem);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Material Issue Note",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"MIN No: {minVM?.RequestNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    minVM.StockIssueRequestSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.StockRequestIssueSubs
                    .GetQueryable()
                    .Where(x => x.RequestId == minVM.RequestId)
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

        private async Task DeleteStockIssueAndTrackAsync(int minSubId, int itemId, int screenCode)
        {
            var issueId = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(s => s.SubItemRefID == minSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.IssueId)
                .FirstOrDefaultAsync();

            if (issueId > 0)
                await _stockManagerService.DeleteStockIssueAsync(issueId);

            await _unitOfWork.SaveAsync();
        }

        public async Task<List<StockIssueRequestSubVM>> GetMINSubByMINIdAsync(int minId, int storeId)
        {
            try
            {
                var subs = await _unitOfWork.StockRequestIssueSubs.GetQueryable()
                    .Where(s => s.RequestId == minId)
                    .Select(s => new StockIssueRequestSubVM
                    {
                        RequestSubId = s.RequestSubId,
                        RequestId = s.RequestId,
                        SlNo = s.SlNo,
                        ItemId = s.ItemId,
                        ItemCode = s.Item != null ? s.Item.ItemCode : null,
                        ItemName = s.Item != null ? s.Item.ItemName : null,
                        UOM = s.Item != null ? s.Item.MeasureUnit : null,
                        UtlQty = s.UtlQty,
                        ReqQty = s.ReqQty,
                        UnitPrice = s.UnitPrice,
                        Remark = s.Remark,
                        BatchNo = s.BatchNo,
                        Make = s.Make,
                        RackNo = s.RackNo,
                        CostId = s.CostId,
                        ProjectNo = s.CostCenter != null ? s.CostCenter.ProjectNo : null,
                        IsEditable = false,
                        StockQty = 0m
                    })
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                if (!subs.Any())
                    return new List<StockIssueRequestSubVM>();

                var itemIds = subs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);

                foreach (var sub in subs)
                {
                    if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                        sub.StockQty = qty;
                    else
                        sub.StockQty = 0m;
                }

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching MIN items for MinId: {minId}");
                throw new InvalidOperationException("Failed to retrieve MIN sub-items. Please try again.");
            }
        }

        public async Task<StockIssueRequestVM> UpsertMINAsync(StockIssueRequestVM minVM, int screenCode)
        {
            if (minVM == null)
                throw new ArgumentNullException(nameof(minVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                StockIssueRequest entity;

                if (minVM.RequestId == 0)
                {
                    entity = _mapper.Map<StockIssueRequest>(minVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.MINs.GetLastMINNoAsync(entity.Suffix);
                    entity.RequestNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.StockIssueRequestSubs = minVM.StockIssueRequestSubVMs.Select(s => _mapper.Map<StockIssueRequestSub>(s)).ToList();

                    await _unitOfWork.StockRequestIssues.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    //foreach (var subVM in entity.StockRequestIssueSubs)
                    //{
                    //    await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, entity.StoreIssId.Value, subVM.ReqQty,
                    //        subVM.UnitPrice, subVM.BatchNo, screenCode, subVM.RequestSubId, entity.RequestNo, entity.RequestDate);
                    //}
                    await SetPoAuthorizationStatusAsync(entity, currentUser);
                    changes.AppendLine("MIN Created.");
                }
                else
                {
                    entity = await _unitOfWork.StockRequestIssues.GetQueryable()
                        .Include(q => q.StockIssueRequestSubs)
                        .FirstOrDefaultAsync(q => q.RequestId == minVM.RequestId)
                        ?? throw new InvalidOperationException("MIN not found.");

                    var parentChanges = GetPropertyChanges(entity, minVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(minVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    await SetPoAuthorizationStatusAsync(entity, currentUser);
                    await HandleChildUpdatesAsync(entity, minVM.StockIssueRequestSubVMs, changes, screenCode);

                    changes.AppendLine("MIN Updated.");
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, minVM.RequestId == 0 ? "RequestNo Created" : "RequestNo Updated");

                var savedEntity = await _unitOfWork.StockRequestIssues.GetQueryable()
                    .Include(q => q.StockIssueRequestSubs).ThenInclude(s => s.Item)
                    .Include(q => q.StockIssueRequestSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.RequestId == entity.RequestId);

                return _mapper.Map<StockIssueRequestVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert RequestNo: {minVM.RequestNo}");
                throw new InvalidOperationException("Failed to save RequestNo. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(StockIssueRequest existingMin, List<StockIssueRequestSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingMin.StockIssueRequestSubs.Select(s => s.RequestSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.RequestSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingMin.StockIssueRequestSubs.Where(s => !incomingSubIds.Contains(s.RequestSubId)).ToList())
            {
                //await DeleteStockIssueAndTrackAsync(sub.RequestSubId, sub.ItemId.Value, screenCode);

                changes.AppendLine($"Child Deleted - RequestSubId: {sub.RequestSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.MINSubs.DeleteAsync(sub.RequestSubId);
                await _unitOfWork.SaveAsync();
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.RequestSubId == 0)
                {
                    var newSub = _mapper.Map<StockIssueRequestSub>(subVM);
                    newSub.RequestId = existingMin.RequestId;
                    await _unitOfWork.StockRequestIssueSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode},Issue Qty: {subVM.ReqQty}");

                    //await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingMin.StoreIssId.Value, subVM.ReqQty.GetValueOrDefault(),
                    //    subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.RequestSubId, existingMin.RequestNo, existingMin.RequestDate);

                }
                else
                {
                    var existingSub = existingMin.StockIssueRequestSubs.FirstOrDefault(s => s.RequestSubId == subVM.RequestSubId);
                    if (existingSub != null)
                    {
                        //await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingMin.StoreIssId.Value, subVM.ReqQty.GetValueOrDefault(),
                        //subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.RequestSubId, existingMin.RequestNo, existingMin.RequestDate);

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }
        private async Task SetPoAuthorizationStatusAsync(StockIssueRequest entity, string currentUser)
        {
            var PoAuthorityExists = await _unitOfWork.UserAuthorities
                .AnyAsync(x => x.IsStockReq == true);

            if (!PoAuthorityExists)
            {
                entity.IsAuthorized = true;
                entity.ApprovedBy = currentUser;
                entity.ApprovalDate = DateTime.Now;
            }
            else
            {
                entity.IsAuthorized = false;
                entity.ApprovedBy = null;
                entity.ApprovalDate = null;

                entity.IsLevel1 = false;
                entity.IsLevel2 = false;
                entity.IsLevel3 = false;

                entity.Level1Sign = null;
                entity.Level2Sign = null;
                entity.Level3Sign = null;
            }

            entity.IsRejected = false;
            entity.RejectReason = string.Empty;
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
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

    }
}
