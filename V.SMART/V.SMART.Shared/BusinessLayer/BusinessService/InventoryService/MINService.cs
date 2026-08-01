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

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService
{
    public class MINService : IMINService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public MINService(
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

        public async Task<(List<MaterialIssNoteVM> minVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.MINs
                    .GetQueryable()
                    .Include(x => x.AssyItem)
                    .Include(x => x.MaterialIssNoteSubs)
                        .ThenInclude(s => s.Item)
                    .Include(x => x.MaterialIssNoteSubs)
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
                    .OrderByDescending(x => x.MINId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<MaterialIssNoteVM>>(list);

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
            public static IQueryable<MaterialIssNote> ApplyFilter(
                IQueryable<MaterialIssNote> query,
                string field,
                object value)
            {
                try
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return query;

                    string val = value.ToString()!.Trim();

                //    new("MINNo", "MIN No.", "text"),
                //new("AssemblyCode", "Assembly Code", "text"),
                //new("AssemblyName", "Assembly Name", "text"),
                //new("ItemCode", "Item Code", "text"),
                //new("ItemName", "Item Name", "text"),
                //new("CreatedBy", "Created By", "text"),
                //new("FromDate", "From Date", "date"),
                //new("ToDate", "To Date", "date")

                    switch (field)
                    {
                        case "MINNo":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(x => (string.IsNullOrEmpty(part1) || x.IssueNo.StartsWith(part1)) &&
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
                                x.MaterialIssNoteSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemName.Contains(val)));

                        case "ItemCode":
                            return query.Where(x =>
                                x.MaterialIssNoteSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemCode.Contains(val)));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.IssueDateNow >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.IssueDateNow <= toDate.Date.AddDays(1).AddTicks(-1));
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

        public async Task<List<string>> GetAllExistingUserNamesinMINAsync()
        {
            var result = await _unitOfWork.MINs.GetQueryable()
                         .Select(m => m.ToWhom)
                         .Distinct()
                         .ToListAsync();
            return result;
        }

        public async Task<IEnumerable<MaterialIssNoteVM>> GetAllMINDetailsAsync()
        {
            try
            {
                var entities = await _unitOfWork.MINs.GetAllWithIncludeAsync(q => true,
                    q => q.Store,
                    q => q.AssyItem,
                    q => q.SubAssyItem,
                    q => q.MaterialIssNoteSubs);
                return _mapper.Map<IEnumerable<MaterialIssNoteVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllMINDetailsAsync");
                return Enumerable.Empty<MaterialIssNoteVM>();
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteMINAsync(int minId)
        {
            try
            {
                var minSubIds = await _unitOfWork.MINSubs
                    .GetQueryable()
                    .Where(s => s.MINId == minId)
                    .Select(s => s.MINSubId)
                    .ToListAsync();

                if (!minSubIds.Any())
                    return (true, "Material Issue Note can be safely deleted.");


                //// Check Performa Invoice
                //bool hasPerformaInv = await _unitOfWork.PerformaInvSubs
                //    .GetQueryable()
                //    .AnyAsync(qs => qs.RefPoSubId.HasValue && poSubIds.Contains(qs.RefPoSubId.Value));
                //if (hasPerformaInv)
                //    return (false, "Cannot delete this Purchase Order as a Performa Invoice transaction exists.");


                //// Check Sales DC
                //bool hasDcTrans = await _unitOfWork.MfgDcSubs
                //    .GetQueryable()
                //    .AnyAsync(qs => qs.RefPoSubId.HasValue && poSubIds.Contains(qs.RefPoSubId.Value));
                //if (hasDcTrans)
                //    return (false, "Cannot delete this Purchase Order as a Sales DC transaction exists.");


                return (true, "Material Issue Note can be safely deleted.");
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
                var min = await _unitOfWork.MINs
                    .GetQueryable()
                    .Include(e => e.MaterialIssNoteSubs)
                    .FirstOrDefaultAsync(e => e.MINId == minId);

                if (min == null)
                    return false;

                var changes = new StringBuilder();

                if (min.MaterialIssNoteSubs != null && min.MaterialIssNoteSubs.Any())
                {
                    foreach (var sub in min.MaterialIssNoteSubs.ToList())
                    {
                        await DeleteStockIssueAndTrackAsync(sub.MINSubId, sub.ItemId.Value, screenCode);

                        if (sub.RefRequestSubId.HasValue)
                        {
                            await AdjustMaterailRequesttBalanceAsync(sub.RefRequestSubId, sub.ItemId.Value, sub.IssueQty, 0, "Material Issue Create");
                        }
                        await _unitOfWork.MINSubs.DeleteAsync(sub);
                        changes.AppendLine($"Deleted Sub Item: {sub.ItemId}, Qty: {sub.IssueQty}");
                    }
                }

                await _unitOfWork.MINs.DeleteAsync(min);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Material Issue Note (Reduction)",
                    action: $"Deleted MIN No: {min.IssueNo}",
                    additionalInfo: $"MIN Id: {min.MINId}\n{changes}"
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
        private async Task AdjustMaterailRequesttBalanceAsync(int? refReqIssueSubId, int itemId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (refReqIssueSubId == 0) return;

                var GrnSub = await _unitOfWork.StockRequestIssueSubs.GetQueryable()
                    .Where(j => j.RequestSubId == refReqIssueSubId && j.ItemId == itemId).FirstOrDefaultAsync();

                if (GrnSub == null) return;

                if (oldQty > 0)
                    GrnSub.ReqBalQty += oldQty;

                if (newQty > GrnSub.ReqQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

                if (newQty > 0)
                    GrnSub.ReqBalQty -= newQty;

                await _unitOfWork.StockRequestIssueSubs.UpdateAsync(GrnSub);
                await _unitOfWork.SaveAsync();


                var totalBalQty = await _unitOfWork.StockRequestIssueSubs
                    .GetQueryable()
                    .Where(e => e.RequestId == GrnSub.RequestId)
                    .SumAsync(e => e.ReqBalQty);

                var DC = await _unitOfWork.StockRequestIssues.GetAsync(GrnSub.RequestId);
                if (DC != null)
                {
                    DC.ReqTally = (totalBalQty == 0);
                    await _unitOfWork.StockRequestIssues.UpdateAsync(DC);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustMaterailRequesttBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustMaterailRequesttBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to AdjustMaterailRequesttBalanceAsync. Please contact support.");
            }
        }
        public async Task<MaterialIssNoteVM?> GetMINByMINIdAsync(int minId)
        {
            try
            {
                // 1️⃣ Load entity with all necessary relationships
                var entity = await _unitOfWork.MINs.GetQueryable()
                    .Include(m => m.MaterialIssNoteSubs)
                    .Include(m => m.MaterialIssNoteSubs).ThenInclude(s => s.Item)
                    .Include(m => m.MaterialIssNoteSubs).ThenInclude(s => s.CostCenter)
                    .Include(m => m.Store)
                    .Include(m => m.AssyItem)
                    .Include(m => m.SubAssyItem)
                    .Include(m => m.SubAssyItem2)
                    .Include(m => m.SubAssyItem3)
                    .FirstOrDefaultAsync(m => m.MINId == minId);

                if (entity == null)
                    return null;

                // 2️⃣ Map entity → VM using AutoMapper
                var minVM = _mapper.Map<MaterialIssNoteVM>(entity);

                // 3️⃣ Fetch live stock for all ItemIds
                var itemIds = minVM.MaterialIssNoteSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && minVM.StoreIssId.HasValue)
                {
                    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, minVM.StoreIssId.Value);

                    foreach (var sub in minVM.MaterialIssNoteSubVMs)
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
                var lastSCN = await _unitOfWork.MINs
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.IssueNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastSCN != null)
                {
                    var parts = lastSCN.IssueNo.Split('/');
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

                    rate = await (from ss in _unitOfWork.MINSubs.GetQueryable()
                                  where ss.ItemId == itemId
                                  orderby ss.MINSubId descending
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

        public async Task<List<MaterialIssNoteSubVM>> GetAssemblyRelatedItemsAsync(int assyId, decimal ReqQty, int storeId)
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
               .Select(ad => new MaterialIssNoteSubVM
               {
                   ItemId = ad.PartItem?.ItemId,
                   ItemCode = ad.PartItem?.ItemCode,
                   ItemName = ad.PartItem?.ItemName,
                   UOM = ad.PartItem?.MeasureUnit,
                   UtlQty = ad.UtilQty,
                   IssueQty = ad.UtilQty * ReqQty,
                   UnitPrice = ad.PartItem?.Rate,
                   BatchNo = null,
                   RackNo = ad.PartItem?.RackNo,
                   Make = ad.PartItem?.Make,
                   Remark = null,
                   StockQty = stockDict.TryGetValue(ad.PartItem.ItemId, out var stockQty) ? stockQty : 0m
               })
               .ToList();
        }

        public async Task<MaterialIssNoteSubVM?> GetMINSubItemDetailByMINSubIdAsync(int minSubId)
        {
            try
            {
                return await _unitOfWork.MINSubs
                    .GetQueryable()
                    .Where(q => q.MINSubId == minSubId)
                    .Select(q => new MaterialIssNoteSubVM
                    {
                        IssueQty = q.IssueQty,
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching scn sub item detail for MinSubid: {minSubId}");
                throw new InvalidOperationException("Failed to retrieve Min sub-item details.");
            }
        }

        public async Task DeleteAndResequenceAsync(MaterialIssNoteSubVM subitem, MaterialIssNoteVM minVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.MINSubId > 0)
                {
                    var existingMINSubItem = await _unitOfWork.MINSubs.GetAsync(subitem.MINSubId);
                    if (existingMINSubItem == null)
                        throw new InvalidOperationException("Sub item not found.");

                    //Delete StockIssue AndTrack And BalUpdate
                    await DeleteStockIssueAndTrackAsync(existingMINSubItem.MINSubId, existingMINSubItem.ItemId.Value, screenCode);
                    if(existingMINSubItem.RefRequestSubId>0)
                    {
                        await AdjustMaterailRequesttBalanceAsync(existingMINSubItem.RefRequestSubId, existingMINSubItem.ItemId.Value, existingMINSubItem.IssueQty, 0, "Material Issue Deleted");

                    }
                   
                    // Delete from DB
                    await _unitOfWork.MINSubs.DeleteAsync(existingMINSubItem);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Material Issue Note",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"MIN No: {minVM?.IssueNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    minVM.MaterialIssNoteSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.MINSubs
                    .GetQueryable()
                    .Where(x => x.MINId == minVM.MINId)
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

        public async Task<List<MaterialIssNoteSubVM>> GetMINSubByMINIdAsync(int minId, int storeId)
        {
            try
            {
                var subs = await _unitOfWork.MINSubs.GetQueryable()
                    .Where(s => s.MINId == minId)
                    .Select(s => new MaterialIssNoteSubVM
                    {
                        MINSubId = s.MINSubId,
                        MINId = s.MINId,
                        SlNo = s.SlNo,
                        ItemId = s.ItemId,
                        ItemCode = s.Item != null ? s.Item.ItemCode : null,
                        ItemName = s.Item != null ? s.Item.ItemName : null,
                        UOM = s.Item != null ? s.Item.MeasureUnit : null,
                        UtlQty = s.UtlQty,
                        IssueQty = s.IssueQty,
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
                    return new List<MaterialIssNoteSubVM>();

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

        public async Task<MaterialIssNoteVM> UpsertMINAsync(MaterialIssNoteVM minVM, int screenCode)
        {
            if (minVM == null)
                throw new ArgumentNullException(nameof(minVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MaterialIssNote entity;

                if (minVM.MINId == 0)
                {
                    entity = _mapper.Map<MaterialIssNote>(minVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.MINs.GetLastMINNoAsync(entity.Suffix);
                    entity.IssueNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.MaterialIssNoteSubs = minVM.MaterialIssNoteSubVMs.Select(s => _mapper.Map<MaterialIssNoteSub>(s)).ToList();

                    await _unitOfWork.MINs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in entity.MaterialIssNoteSubs)
                    {
                        if (subVM.RefRequestSubId.HasValue)
                        {
                            await AdjustMaterailRequesttBalanceAsync(subVM.RefRequestSubId, subVM.ItemId.Value, 0, subVM.IssueQty, "Material Issue Create");
                        }

                        await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, entity.StoreIssId.Value, subVM.IssueQty,
                            subVM.UnitPrice, subVM.BatchNo, screenCode, subVM.MINSubId, entity.IssueNo, entity.IssueDate);
                    }

                    changes.AppendLine("MIN Created.");
                }
                else
                {
                    entity = await _unitOfWork.MINs.GetQueryable()
                        .Include(q => q.MaterialIssNoteSubs)
                        .FirstOrDefaultAsync(q => q.MINId == minVM.MINId)
                        ?? throw new InvalidOperationException("MIN not found.");

                    var parentChanges = GetPropertyChanges(entity, minVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(minVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, minVM.MaterialIssNoteSubVMs, changes, screenCode);

                    changes.AppendLine("MIN Updated.");
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, minVM.MINId == 0 ? "MIN Created" : "MIN Updated");

                var savedEntity = await _unitOfWork.MINs.GetQueryable()
                    .Include(q => q.MaterialIssNoteSubs).ThenInclude(s => s.Item)
                    .Include(q => q.MaterialIssNoteSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.MINId == entity.MINId);

                return _mapper.Map<MaterialIssNoteVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert MIN: {minVM.IssueNo}");
                throw new InvalidOperationException("Failed to save MIN. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(MaterialIssNote existingMin, List<MaterialIssNoteSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingMin.MaterialIssNoteSubs.Select(s => s.MINSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.MINSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingMin.MaterialIssNoteSubs.Where(s => !incomingSubIds.Contains(s.MINSubId)).ToList())
            {
                await DeleteStockIssueAndTrackAsync(sub.MINSubId, sub.ItemId.Value, screenCode);
                if (sub.RefRequestSubId.HasValue)
                {
                    await AdjustMaterailRequesttBalanceAsync(sub.RefRequestSubId, sub.ItemId.Value, sub.IssueQty, 0, "Material Issue Deleted");
                }
                changes.AppendLine($"Child Deleted - MINSubId: {sub.MINSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.MINSubs.DeleteAsync(sub.MINSubId);
                await _unitOfWork.SaveAsync();
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.MINSubId == 0)
                {
                    var newSub = _mapper.Map<MaterialIssNoteSub>(subVM);
                    newSub.MINId = existingMin.MINId;
                    await _unitOfWork.MINSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();
                    if (subVM.RefRequestSubId.HasValue)
                    {
                        await AdjustMaterailRequesttBalanceAsync(subVM.RefRequestSubId, subVM.ItemId.Value, 0, subVM.IssueQty.GetValueOrDefault(), "Material Issue Create");
                    }
                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode},Issue Qty: {subVM.IssueQty}");

                    await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingMin.StoreIssId.Value, subVM.IssueQty.GetValueOrDefault(),
                        subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.MINSubId, existingMin.IssueNo, existingMin.IssueDate);

                }
                else
                {
                    var existingSub = existingMin.MaterialIssNoteSubs.FirstOrDefault(s => s.MINSubId == subVM.MINSubId);
                    if (existingSub != null)
                    {
                        await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingMin.StoreIssId.Value, subVM.IssueQty.GetValueOrDefault(),
                        subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.MINSubId, existingMin.IssueNo, existingMin.IssueDate);

                        if (subVM.RefRequestSubId.HasValue)
                        {
                            await AdjustMaterailRequesttBalanceAsync(subVM.RefRequestSubId, subVM.ItemId.Value, existingSub.IssueQty, subVM.IssueQty.GetValueOrDefault(), "Material Issue Create");
                        }

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
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
        public async Task<List<Dictionary<string, object>>> GetStockIssueReqDetails()
        {
            try
            {
                var result = await (from e in _unitOfWork.StockRequestIssues.GetQueryable()
                                    join es in _unitOfWork.StockRequestIssueSubs.GetQueryable()
                                        on e.RequestId equals es.RequestId
                                    where es.ReqBalQty > 0 && !e.ReqTally && e.IsAuthorized
                                    select new
                                    {
                                        es.RequestSubId,
                                        e.RequestNo,
                                        e.Suffix,
                                        e.RequestDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.HSNCode,
                                        es.Item.MeasureUnit,
                                        es.ReqQty,
                                        es.ReqBalQty,
                                        es.UnitPrice,
                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RequestSubId"] = r.RequestSubId,
                    ["RequestNo"] = $"{r.RequestNo}{r.Suffix}",
                    ["RequestDate"] = r.RequestDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["ReqQty"] = r.ReqQty,
                    ["ReqBalQty"] = r.ReqBalQty,
                    ["Rate"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Stock Issue Request details for");
                throw new InvalidOperationException("Failed to retrieve Stock Issue details. Please try again.");
            }
        }

    }
}
