using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService
{
    public class SCNGenService : ISCNGenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public SCNGenService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            IStockManagerService stockManagerService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _stockManagerService = stockManagerService;
            _commonService = commonService;
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
           => await _commonService.GetAllActiveStoresAsync();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        // 🔹 Assembly/ Sub assembly Related
        public async Task<List<ItemVM>> GetAllAssembliesAsync()
            => await _commonService.GetAllAssembliesAsync();
        public async Task<List<ItemVM>> GetAllSubAssembliesByAssyIdAsync(int assyId)
            => await _commonService.GetAllSubAssembliesByAssyIdAsync(assyId);

        public async Task<List<ItemVM>> GetAllSubAssembliesAsync()
            => await _commonService.GetAllSubAssembliesAsync();

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        //Stock
        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId)
        {
            return await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);
        }

        //Cost-Center
        public async Task<List<CostCenterVM>> GetAllCostCenterDetails()
           => await _commonService.GetAllCostCenterDetails();

        //HSN Code
        public async Task GetOrCreateHSNAsync(string hsnCode)
        {
            if (string.IsNullOrWhiteSpace(hsnCode))
                throw new ArgumentException("HSN code is required", nameof(hsnCode));

            var existingHSN = await _unitOfWork.HSNMasters
                .GetQueryable()
                .FirstOrDefaultAsync(h => h.HSNcode == hsnCode);

            if (existingHSN != null)
            {
                if (!existingHSN.Required)
                {
                    existingHSN.Required = true;
                    await _unitOfWork.HSNMasters.UpdateAsync(existingHSN);
                    await _unitOfWork.SaveAsync();
                }
            }
            else
            {
                var newHSN = new HSNMaster
                {
                    HSNCategoryCode = hsnCode,
                    HSNcode = hsnCode,
                    Required = true
                };

                var hsnId = await _unitOfWork.HSNMasters.CreateAsync(newHSN);
                await _unitOfWork.SaveAsync();
            }
        }

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();






        // 🔹 Store Transfer Note Operations operations

        public async Task<(List<SCNGenVM> sCNGenVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.SCNGens
                    .GetQueryable()
                    .Include(x=>x.AssyItem)
                    .Include(x => x.SCNGenSubs)
                        .ThenInclude(s => s.Item)
                    .Include(x => x.SCNGenSubs)
                        .ThenInclude(s => s.CostCenter)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = SCNFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.SCNGenId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<SCNGenVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (SCN GEN)");
                throw new InvalidOperationException("Failed to load Stock Addition Note list.", ex);
            }
        }

        public static class SCNFilterBuilder
        {
            public static IQueryable<SCNGen> ApplyFilter(
                IQueryable<SCNGen> query,
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
                        case "SCNNo":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(x => (string.IsNullOrEmpty(part1) || x.SCNGenNo.StartsWith(part1)) &&
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
                                x.SCNGenSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemName.Contains(val)));

                        case "ItemCode":
                            return query.Where(x =>
                                x.SCNGenSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemCode.Contains(val)));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.SCNGenDate >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.SCNGenDate <= toDate.Date.AddDays(1).AddTicks(-1));
                            return query;

                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }
        }

        public async Task<decimal> GetStockForItemAsync(int itemId, int storeId)
        {
            var stockQty = await _stockManagerService.GetStockForItemAsync(itemId, storeId);
            return stockQty;
        }

        public async Task<int> CreateNewItemAsync(Item newItem)
        {
            await _unitOfWork.ItemRepositories.CreateAsync(newItem);
            await _unitOfWork.SaveAsync();
            return newItem.ItemId;
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

        public async Task<SCNGenVM?> GetSCNBySCNGenIdAsync(int scnGenId)
        {
            try
            {
                var entity = await _unitOfWork.SCNGens.GetQueryable()
                    .Include(q => q.SCNGenSubs)
                    .Include(q => q.SCNGenSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Store)
                    .Include(q => q.AssyItem)
                    .Include(q => q.SubAssyItem)
                    .Include(q => q.SubAssyItem2)
                    .Include(q => q.SubAssyItem3)
                    .Include(q => q.SCNGenSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.SCNGenId == scnGenId);

                if (entity == null)
                    return null;

                // Map entity → ViewModel
                var scnVM = _mapper.Map<SCNGenVM>(entity);

                // Fetch stock quantities for all BOM item IDs
                var itemIds = scnVM.SCNGenSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && scnVM.StoreId.HasValue)
                {
                    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, scnVM.StoreId.Value);

                    foreach (var sub in scnVM.SCNGenSubVMs)
                    {
                        if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                            sub.StockQty = qty;
                        else
                            sub.StockQty = 0m;
                    }
                }

                return scnVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetSCNBySCNGenIdAsync({scnGenId})");
                return null;
            }
        }


        public async Task<string> GetSCNGenNumberAsync(string suffix)
        {
            try
            {
                var lastSCN = await _unitOfWork.SCNGens
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.SCNGenNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastSCN != null)
                {
                    var parts = lastSCN.SCNGenNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating SCN number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate SCN number.");
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

                    rate = await (from ss in _unitOfWork.SCNGenSubs.GetQueryable()
                                  where ss.ItemId == itemId
                                  orderby ss.SCNGenId descending
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

        public async Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(
                        int screenCode,
                        int storeId,
                        int itemId,
                        int subItemRefId)
        {
            try
            {
                return await _stockManagerService.GetQtyBalQtyByStockAddAsync(
                    screenCode,
                    storeId,
                    itemId,
                    subItemRefId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching stock details for Screen Code: {screenCode}, SubItemRefId: {subItemRefId}");
                throw new InvalidOperationException("Failed to retrieve SCNGen sub-item stock details.");
            }
        }

        public async Task DeleteAndResequenceAsync(SCNGenSubVM subitem, SCNGenVM scnGenVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.SCNGenSubId > 0)
                {
                    var entity = await _unitOfWork.SCNGenSubs.GetAsync(subitem.SCNGenSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.RefBomId.GetValueOrDefault() > 0)
                    {
                        await AdjustBOMBalanceAsync(entity.RefBomId.GetValueOrDefault(), entity.Qty, 0, "SCNGen Item Row Deletion");
                    }

                    await DeleteStockAddAsync(entity.SCNGenSubId, entity.ItemId.Value, screenCode);

                    // Delete from DB
                    await _unitOfWork.SCNGenSubs.DeleteAsync(entity.SCNGenSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "SCN Gen",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"SCN No: {scnGenVM?.SCNGenNo}"
                    );
                }
                else
                {
                    scnGenVM.SCNGenSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.SCNGenSubs
                    .GetQueryable()
                    .Where(x => x.SCNGenId == scnGenVM.SCNGenId)
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

        public async Task<List<SCNGenSubVM>> GetSCNSubBySCNGenIdAsync(int scnGenId, int storeId)
        {
            try
            {
                var subs = await _unitOfWork.SCNGenSubs.GetQueryable()
                    .Where(s => s.SCNGenId == scnGenId)
                    .Select(s => new SCNGenSubVM
                    {
                        SCNGenSubId = s.SCNGenSubId,
                        SCNGenId = s.SCNGenId,
                        SlNo = s.SlNo,
                        ItemId = s.ItemId,
                        ItemCode = s.Item != null ? s.Item.ItemCode : null,
                        ItemName = s.Item != null ? s.Item.ItemName : null,
                        UOM = s.Item != null ? s.Item.MeasureUnit : null,
                        UtlQty = s.UtlQty,
                        Qty = s.Qty,
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
                    return new List<SCNGenSubVM>();

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
                await _logs.LogDeveloperError(ex, $"Error fetching SCNGen sub-items for SCNGenId: {scnGenId}, StoreId: {storeId}");
                throw new InvalidOperationException("Failed to retrieve SCNGen sub-items. Please try again later.");
            }
        }

        public async Task<SCNGenVM> UpsertSCNAsync(SCNGenVM scnGenVM , int screenCode)
        {
            if (scnGenVM == null)
                throw new ArgumentNullException(nameof(scnGenVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            try
            {
                SCNGen entity;

                if (scnGenVM.SCNGenId == 0)
                {
                    entity = _mapper.Map<SCNGen>(scnGenVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.SCNGens.GetLastSCNGenNoAsync(entity.Suffix);
                    entity.SCNGenNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.SCNGenSubs = scnGenVM.SCNGenSubVMs.Select(s => _mapper.Map<SCNGenSub>(s)).ToList();

                    await _unitOfWork.SCNGens.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in entity.SCNGenSubs)
                    {
                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, entity.StoreId, subVM.Qty,
                            subVM.UnitPrice, subVM.BatchNo,screenCode, subVM.SCNGenSubId, entity.SCNGenNo,entity.SCNGenDate,subVM.Remark);
                    }

                    changes.AppendLine("SCN General Created.");
                }
                else
                {
                    entity = await _unitOfWork.SCNGens.GetQueryable()
                        .Include(q => q.SCNGenSubs)
                        .FirstOrDefaultAsync(q => q.SCNGenId == scnGenVM.SCNGenId)
                        ?? throw new InvalidOperationException("SCN Gen not found.");

                    var parentChanges = GetPropertyChanges(entity, scnGenVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(scnGenVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, scnGenVM.SCNGenSubVMs, changes , screenCode);

                    changes.AppendLine("SCN General Updated.");
                }

                await _unitOfWork.SaveAsync();

                await LogChangesAsync(changes, scnGenVM.SCNGenId == 0 ? "SCN Gen Created" : "SCN Gen Updated");

                var savedEntity = await _unitOfWork.SCNGens.GetQueryable()
                    .Include(q => q.SCNGenSubs).ThenInclude(s => s.Item)
                    .Include(q => q.SCNGenSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.SCNGenId == entity.SCNGenId);

                return _mapper.Map<SCNGenVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to upsert SCN Gen: {scnGenVM.SCNGenNo}");
                throw new InvalidOperationException("Failed to save SCN Gen. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(SCNGen existingScn, List<SCNGenSubVM> incomingSubVMs, StringBuilder changes ,int screenCode)
        {
            var existingSubIds = existingScn.SCNGenSubs.Select(s => s.SCNGenSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.SCNGenSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingScn.SCNGenSubs.Where(s => !incomingSubIds.Contains(s.SCNGenSubId)).ToList())
            {
                if (sub.RefBomId.GetValueOrDefault() > 0)
                {
                    await AdjustBOMBalanceAsync(sub.RefBomId.GetValueOrDefault(), sub.Qty, 0, "SCNGen Item Row Deletion");
                }

                await DeleteStockAddAsync(sub.SCNGenSubId, sub.ItemId.Value, screenCode);

                changes.AppendLine($"Child Deleted - SCNGenSubId: {sub.SCNGenSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.SCNGenSubs.DeleteAsync(sub.SCNGenSubId);
                await _unitOfWork.SaveAsync();
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.SCNGenSubId == 0)
                {
                    var newSub = _mapper.Map<SCNGenSub>(subVM);
                    newSub.SCNGenId = existingScn.SCNGenId;
                    await _unitOfWork.SCNGenSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingScn.StoreId, newSub.Qty,
                            subVM.UnitPrice.GetValueOrDefault(), newSub.BatchNo, screenCode, newSub.SCNGenSubId, existingScn.SCNGenNo, existingScn.SCNGenDate,newSub.Remark);

                }
                else
                {
                    var existingSub = existingScn.SCNGenSubs.FirstOrDefault(s => s.SCNGenSubId == subVM.SCNGenSubId);
                    if (existingSub != null)
                    {

                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingScn.StoreId, subVM.Qty.GetValueOrDefault(),
                           subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.SCNGenSubId, existingScn.SCNGenNo, existingScn.SCNGenDate,subVM.Remark);

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        public async Task<List<SCNGenSubVM>> GetAssemblyRelatedItemsAsync(int assyId, decimal ReqQty, int storeId)
        {
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
                .Select(ad => new SCNGenSubVM
                {
                    ItemId = ad.PartItem?.ItemId,
                    ItemCode = ad.PartItem?.ItemCode,
                    ItemName = ad.PartItem?.ItemName,
                    UOM = ad.PartItem?.MeasureUnit,
                    UtlQty = ad.UtilQty,
                    Qty = ad.UtilQty * ReqQty,
                    UnitPrice = ad.PartItem?.Rate,
                    BatchNo = null,
                    RackNo = ad.PartItem?.RackNo,
                    Make = ad.PartItem?.Make,
                    Remark = null,
                    StockQty = stockDict.TryGetValue(ad.PartItem.ItemId, out var stockQty) ? stockQty : 0m
                })
                .ToList();
        }

        private async Task DeleteStockAddAsync(int scnGenSubId, int itemId, int screenCode)
        {
            var addId = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == scnGenSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.AddId)
                .FirstOrDefaultAsync();

            if (addId > 0)
                await _stockManagerService.DeleteStockAddAsync(addId);

            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<SCNGenVM>> GetAllSCNGenDetailsAsync()
        {
            try
            {
                var entities = await _unitOfWork.SCNGens.GetAllWithIncludeAsync(q => true,
                    q => q.Store,
                    q => q.AssyItem,
                    q => q.SubAssyItem,
                    q => q.SCNGenSubs );
                return _mapper.Map<IEnumerable<SCNGenVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllSCNGenDetailsAsync");
                return Enumerable.Empty<SCNGenVM>();
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteSCNGenAsync(int scnGenId, int screenCode)
        {
            try
            {
                var scnGenSubIds = await _unitOfWork.SCNGenSubs
                    .GetQueryable()
                    .Where(s => s.SCNGenId == scnGenId)
                    .Select(s => s.SCNGenSubId)
                    .ToListAsync();

                if (!scnGenSubIds.Any())
                    return (true, "SCNGen can be safely deleted (no sub-items found).");

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa =>
                        scnGenSubIds.Contains(sa.SubItemRefID) &&
                        sa.ScreenCode == screenCode &&
                        sa.BalQty < sa.AddQty)
                    .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete SCNGen. Some sub-items have already been transacted/issued.");

                return (true, "SCNGen can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteSCNGenAsync for SCNGenId: {scnGenId}");
                throw new Exception("Error checking SCNGen delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteSCNGenBySCNGenIdAsync(int scnGenId ,int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var scnGen = await _unitOfWork.SCNGens
                    .GetQueryable()
                    .Include(e => e.SCNGenSubs)
                    .FirstOrDefaultAsync(e => e.SCNGenId == scnGenId);

                if (scnGen == null)
                    return false;

                var changes = new StringBuilder();

                // Delete associated StockAdds
                foreach (var sub in scnGen.SCNGenSubs)
                {
                    if(sub.RefBomId.GetValueOrDefault() > 0)
                    {
                        await AdjustBOMBalanceAsync(sub.RefBomId.GetValueOrDefault(), sub.Qty, 0, "SCNGen Deletion");
                    }

                    await DeleteStockAddAsync(sub.SCNGenSubId, sub.ItemId.Value, screenCode);
                }

                var deleted = await _unitOfWork.SCNGens.DeleteAsync(scnGenId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Store Transfer Note (Addition)",
                    action: $"Deleted SCNGen no: {scnGen.SCNGenNo}",
                    additionalInfo: $"SCNGen Id: {scnGen.SCNGenId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete SCNGen: {scnGenId}");
                throw;
            }
        }

        private async Task AdjustBOMBalanceAsync(int? refBOMId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refBOMId.HasValue || refBOMId == 0) return;

                var bomEntity = await _unitOfWork.AssmblyDefs.GetAsync(refBOMId.Value);
                if (bomEntity == null) return;

                if (oldQty > 0)
                {
                    bomEntity.BalQty += oldQty;
                    bomEntity.StockConvertedQty -= oldQty;

                    if (bomEntity.StockConvertedQty < 0)
                        bomEntity.StockConvertedQty = 0;
                }

                if (newQty > 0)
                {
                    bomEntity.BalQty -= newQty;
                    bomEntity.StockConvertedQty += newQty;

                    if (bomEntity.BalQty < 0)
                        bomEntity.BalQty = 0;
                }

                await _unitOfWork.AssmblyDefs.UpdateAsync(bomEntity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustBOMBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust BOM balance. Please contact support.");
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
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
        }
    }

}
