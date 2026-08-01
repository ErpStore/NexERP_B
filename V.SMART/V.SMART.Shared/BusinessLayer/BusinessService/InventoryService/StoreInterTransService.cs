using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.StoreInterTransferViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService
{
    public class StoreInterTransService : IStoreInterTransService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public StoreInterTransService(
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
           => await _commonService.GetAllActiveStoresAsync();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

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
        public async Task<List<string>> GetUOMsAsync()
        {
            //return await _commonService.GetUOMsAsync();
            return null;
        }

        public async Task<Companydetails?> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();

        public async Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId)
           => await _stockManagerService.GetStockForItemAsync(ItemId, StoreId);
        // 🔹 Inter Store Transfer Operations operations
        //public async Task<IEnumerable<StoreInterTransVM>> GetAllISTDetailsAsync()
        //{
        //    try
        //    {
        //        var entities = await _unitOfWork.StoreInterTranses.GetAllWithIncludeAsync(
        //           q => true,              // load all IST
        //           q => q.StoreAdd,        // include Add-To store
        //           q => q.StoreIssue,      // include Issue-From store
        //           q => q.StoreInterTransSubs  // include child rows
        //        );

        //        return _mapper.Map<IEnumerable<StoreInterTransVM>>(entities);

        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, "GetAllISTDetailsAsync");
        //        return Enumerable.Empty<StoreInterTransVM>();
        //    }
        //}

        public async Task<bool> IsRouteCardStockExistsAsync(int itemId)
        {
            try
            {
                return await _unitOfWork.StockAdds.GetQueryable()
                    .AnyAsync(x => x.ItemId == itemId
                                && x.RcSubID != null
                                && x.RcSubID > 0 && x.BalQty>0);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to validate RouteCard stock. ItemId: {itemId}");
                throw;
            }
        }


        public async Task<IEnumerable<StoreInterTransVM>> GetAllISTDetailsAsync()
        {
            try
            {
                // 1) Load IST + children
                var entities = await _unitOfWork.StoreInterTrans.GetAllWithIncludeAsync(
                    q => true,
                    q => q.StoreAdd,
                    q => q.StoreIssue,
                    q => q.StoreInterTransSubs
                );

                var vmList = _mapper.Map<List<StoreInterTransVM>>(entities.ToList());

                if (!vmList.Any())
                    return vmList;

                // Get all IST numbers
                var istNos = vmList
                    .Where(v => !string.IsNullOrWhiteSpace(v.ISTNo))
                    .Select(v => v.ISTNo)
                    .Distinct()
                    .ToList();

                // Get all StockAdd rows for these ISTs
                var stockAdds = await _unitOfWork.StockAdds
                    .GetQueryable()
                    .Where(sa => istNos.Contains(sa.RefNo))
                    .Select(sa => new
                    {
                        sa.RefNo,
                        sa.SubItemRefID,   // Contains ItemId
                        sa.BalQty
                    })
                    .ToListAsync();

                var stockByRef = stockAdds
                    .GroupBy(s => s.RefNo)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var vm in vmList)
                {
                    // -------- STATUS LOGIC --------
                    // Completed if ANY sub row has BalQty = 0
                    if (vm.StoreInterTransSubVMs != null &&
                        vm.StoreInterTransSubVMs.Any(s => s.BalQty == 0))
                    {
                        vm.Status = "Completed";
                    }
                    else
                    {
                        vm.Status = "Pending";
                    }

                    // -------- BALQTY IS ALREADY IN StoreInterTransSub TABLE --------
                    // No StockAdd reference required.
                }

                return vmList;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllISTDetailsAsync");
                return Enumerable.Empty<StoreInterTransVM>();
            }
        }



        public async Task<(bool CanDelete, string Message)> CanDeleteISTAsync(int istId)
        {
            try
            {
                return (true, "Inter Store Transfer can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteISTAsync for ISTId: {istId}");
                throw new Exception("Error checking Inter Store Transfer delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteISTByISTIdAsync(int istId, int screenCode)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var ist = await _unitOfWork.StoreInterTrans
                    .GetQueryable()
                    .Include(e => e.StoreInterTransSubs)
                    .FirstOrDefaultAsync(e => e.ISTId == istId);

                if (ist == null)
                    return false;

                var changes = new StringBuilder();

                if (ist.StoreInterTransSubs != null && ist.StoreInterTransSubs.Any())
                {
                    foreach (var sub in ist.StoreInterTransSubs.ToList())
                    {
                        await DeleteStockIssueAndTrackAsync(sub.ISTSubId, sub.ItemId.Value, screenCode);

                        await DeleteStockAddAndTrackAsync(sub.ISTSubId, sub.ItemId.Value, screenCode);

                        await _unitOfWork.StoreInterTransSubs.DeleteAsync(sub);
                        changes.AppendLine($"Deleted Sub Item: {sub.ItemId}, Qty: {sub.Qty}");
                    }
                }

                await _unitOfWork.StoreInterTrans.DeleteAsync(ist);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Inter Store Transfer",
                    action: $"Deleted IST No: {ist.ISTNo}",
                    additionalInfo: $"IST Id: {ist.ISTId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete IST: {istId}");
                //throw;
                return false;
            }
        }

        public async Task<List<StoreInterTrans>> InstantSearchInterStoreTransAsync(StoreInterTransVM search)
        {
            return await _unitOfWork.StoreInterTrans.InstantSearchInterStoreTransAsync(search);
        }

        public async Task<StoreInterTransVM?> GetISTByISTIdAsync(int istId)
        {
            try
            {
                //// 1️⃣ Load entity with all necessary relationships
                var entity = await _unitOfWork.StoreInterTrans.GetQueryable()
                    .Include(m => m.StoreInterTransSubs)
                    .Include(m => m.StoreInterTransSubs).ThenInclude(s => s.Item)
                    .Include(m => m.StoreAdd)
                    .Include(m => m.StoreIssue)
                    .FirstOrDefaultAsync(m => m.ISTId == istId);

                if (entity == null)
                    return null;

                // 2️⃣ Map entity → VM using AutoMapper
                var istVM = _mapper.Map<StoreInterTransVM>(entity);

                istVM.StoreIdFrom = entity.StoreIssueId; // Issue From
                istVM.StoreIdTo = entity.StoreAddId;     // Add To

                // 3️⃣ Fetch StockAdd rows for this IST (RefNo == ISTNo)
                if (!string.IsNullOrWhiteSpace(istVM.ISTNo))
                {
                    var stockForIst = await _unitOfWork.StockAdds.GetQueryable()
                        .Where(a => a.RefNo == istVM.ISTNo)
                        .Select(a => new { a.SubItemRefID, a.BalQty })
                        .ToListAsync();

                    if (stockForIst.Any())
                    {
                        // Compute status
                        istVM.Status = stockForIst.Any(s => s.BalQty == 0)
                            ? "Completed"
                            : "Pending";

                        // Fill each sub's BalQty by ItemId match
                        //foreach (var sub in istVM.StoreInterTransSubVMs)
                        //{
                        //    var match = stockForIst.FirstOrDefault(s => s.SubItemRefID == sub.ItemId);
                        //    if (match != null)
                        //        sub.BalQty = match.BalQty;
                        //    else
                        //        sub.BalQty = 0;
                        //}
                        foreach (var sub in istVM.StoreInterTransSubVMs)
                        {
                            var match = stockForIst.FirstOrDefault(s => s.SubItemRefID == sub.ItemId);

                            sub.BalQty = match != null ? match.BalQty : 0;
                        }

                    }
                    else
                    {
                        // No stock rows found for this ISTNo
                        istVM.Status = "Pending";
                        foreach (var sub in istVM.StoreInterTransSubVMs ?? Enumerable.Empty<StoreInterTransSubVM>())
                            sub.BalQty = sub.BalQty; // keep existing or default
                    }
                }

                // 4️⃣ Fetch live stock for all ItemIds at source store as before (existing logic)
                var itemIds = istVM.StoreInterTransSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && istVM.StoreIdFrom > 0)
                {
                    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, istVM.StoreIdFrom);

                    foreach (var sub in istVM.StoreInterTransSubVMs)
                    {
                        if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                            sub.StockQty = qty;
                        else
                            sub.StockQty = 0m;
                    }
                }

                return istVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetISTByISTIdAsync({istId}) failed.");
                return null;
            }
        }


        //public async Task<StoreInterTransVM?> GetISTByISTIdAsync(int istId)
        //{
        //    try
        //    {
        //        //// 1️⃣ Load entity with all necessary relationships
        //        var entity = await _unitOfWork.StoreInterTranses.GetQueryable()
        //            .Include(m => m.StoreInterTransSubs)
        //            .Include(m => m.StoreInterTransSubs).ThenInclude(s => s.Item)
        //            //.Include(m => m.StoreInterTransSubs).ThenInclude(s => s.CostCenter)
        //            .Include(m => m.StoreAdd)
        //            .Include(m => m.StoreIssue)
        //            .FirstOrDefaultAsync(m => m.ISTId == istId);

        //        if (entity == null)
        //            return null;

        //        // 2️⃣ Map entity → VM using AutoMapper
        //        var istVM = _mapper.Map<StoreInterTransVM>(entity);

        //        istVM.StoreIdFrom = entity.StoreIssueId; // Issue From
        //        istVM.StoreIdTo = entity.StoreAddId;     // Add To

        //        // 3️⃣ Fetch live stock for all ItemIds
        //        var itemIds = istVM.StoreInterTransSubVMs
        //            .Where(s => s.ItemId.HasValue)
        //            .Select(s => s.ItemId!.Value)
        //            .Distinct()
        //            .ToList();

        //        if (itemIds.Count > 0 && istVM.StoreIdFrom > 0)
        //        {
        //            var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, istVM.StoreIdFrom);

        //            foreach (var sub in istVM.StoreInterTransSubVMs)
        //            {
        //                if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId.Value, out var qty))
        //                    sub.StockQty = qty;
        //                else
        //                    sub.StockQty = 0m;
        //            }
        //        }
        //        return istVM;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"GetISTByISTIdAsync({istId}) failed.");
        //        return null;
        //    }
        //}

        public async Task<string> GetISTInterStoreNumberAsync(string suffix)
        {
            try
            {
                var lastIST = await _unitOfWork.StoreInterTrans
                 .GetQueryable()
                 .Where(q => q.Suffix == suffix)
                 .OrderByDescending(q => q.ISTNo.Length)
                 .ThenByDescending(q => q.ISTNo)
                 .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastIST != null)
                {
                    var parts = lastIST.ISTNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating IST number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate IST number.");
            }
        }

        public async Task<StoreInterTransSubVM?> GetISTSubItemDetailByISTSubIdAsync(int istSubId)
        {
            try
            {
                return await _unitOfWork.StoreInterTransSubs
                    .GetQueryable()
                    .Where(q => q.ISTSubId == istSubId)
                    .Select(q => new StoreInterTransSubVM
                    {
                        Qty = q.Qty,
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ist sub item detail for ISTSubid: {istSubId}");
                throw new InvalidOperationException("Failed to retrieve Ist sub-item details.");
            }
        }

        public async Task DeleteAndResequenceAsync(StoreInterTransSubVM subitem, StoreInterTransVM istVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.ISTSubId > 0) // persisted subitem
                {
                    var existingISTSubItem = await _unitOfWork.StoreInterTransSubs.GetAsync(subitem.ISTSubId);
                    if (existingISTSubItem == null)
                        throw new InvalidOperationException("Sub item not found.");

                    //Delete StockIssue AndTrack And BalUpdate
                    await DeleteStockIssueAndTrackAsync(existingISTSubItem.ISTSubId, existingISTSubItem.ItemId.Value, screenCode);

                    //Delete StockAdd AndTrack And BalUpdate
                    await DeleteStockAddAndTrackAsync(existingISTSubItem.ISTSubId, existingISTSubItem.ItemId.Value, screenCode);

                    // Delete from DB
                    await _unitOfWork.StoreInterTransSubs.DeleteAsync(existingISTSubItem);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Inter Store Transfer",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"SIT No: {istVM?.ISTNo}"
                    );
                }
                else
                {
                    istVM.StoreInterTransSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.StoreInterTransSubs
                    .GetQueryable()
                    .Where(x => x.ISTId == istVM.ISTId)
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

        private async Task DeleteStockIssueAndTrackAsync(int istSubId, int itemId, int screenCode)
        {
            var issueId = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(s => s.SubItemRefID == istSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.IssueId)
                .FirstOrDefaultAsync();

            if (issueId > 0)
                await _stockManagerService.DeleteStockIssueAsync(issueId);

            await _unitOfWork.SaveAsync();
        }

        private async Task DeleteStockAddAndTrackAsync(int istSubId, int itemId, int screenCode)
        {
            var addId = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == istSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.AddId)
                .FirstOrDefaultAsync();

            if (addId > 0)
                await _stockManagerService.DeleteStockAddAsync(addId);

            await _unitOfWork.SaveAsync();
        }

        public async Task<List<StoreInterTransSubVM>> GetISTSubByISTIdAsync(int istId, int storeId)
        {
            try
            {
                var subs = await _unitOfWork.StoreInterTransSubs.GetQueryable()
                    .Where(s => s.ISTId == istId)
                    .Select(s => new StoreInterTransSubVM
                    {
                        ISTSubId = s.ISTSubId,
                        ISTId = s.ISTId,
                        SlNo = s.SlNo,
                        ItemId = s.ItemId,
                        ItemCode = s.Item != null ? s.Item.ItemCode : null,
                        ItemName = s.Item != null ? s.Item.ItemName : null,
                        UOM = s.Item != null ? s.Item.MeasureUnit : null,
                        Qty = s.Qty,
                        UnitPrice = s.UnitPrice,
                        Remark = s.Remark,
                        IsEditable = false,
                        StockQty = 0m
                    })
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                if (!subs.Any())
                    return new List<StoreInterTransSubVM>();

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
                await _logs.LogDeveloperError(ex, $"Error fetching IST items for ISTId: {istId}");
                throw new InvalidOperationException("Failed to retrieve IST sub-items. Please try again.");
            }
        }

        public async Task<StoreInterTransVM> UpsertISTAsync(StoreInterTransVM istVM, int screenCode)
        {
            if (istVM == null)
                throw new ArgumentNullException(nameof(istVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                StoreInterTrans entity;

                if (istVM.ISTId == 0)
                {
                    entity = _mapper.Map<StoreInterTrans>(istVM);

                    entity.StoreIssueId = istVM.StoreIdFrom ?? 0; // Source store (issue)
                    entity.StoreAddId = istVM.StoreIdTo ?? 0;   // Destination store (add)

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.StoreInterTrans.GetLastISTNoAsync(entity.Suffix);
                    entity.ISTNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.StoreInterTransSubs = istVM.StoreInterTransSubVMs.Select(s => _mapper.Map<StoreInterTransSub>(s)).ToList();

                    await _unitOfWork.StoreInterTrans.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    // ISSUE + ADD STOCK
                    foreach (var sub in entity.StoreInterTransSubs)
                    {
                        // ISSUE FROM SOURCE STORE
                        await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId!.Value, entity.StoreIssueId, sub.Qty,
                            sub.UnitPrice, sub.BatchNo, screenCode, sub.ISTSubId, entity.ISTNo, entity.ISTDate);

                        // ADD TO DESTINATION STORE
                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId!.Value, entity.StoreAddId, sub.Qty,
                            sub.UnitPrice, sub.BatchNo, screenCode, sub.ISTSubId, entity.ISTNo, entity.ISTDate, entity.MainRemark);
                    }
                    changes.AppendLine("IST Created.");
                }
                else
                {
                    entity = await _unitOfWork.StoreInterTrans.GetQueryable()
                        .Include(q => q.StoreInterTransSubs)
                        .FirstOrDefaultAsync(q => q.ISTId == istVM.ISTId)
                        ?? throw new InvalidOperationException("IST not found.");

                    var parentChanges = GetPropertyChanges(entity, istVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(istVM, entity);

                    entity.StoreIssueId = istVM.StoreIdFrom ?? 0;
                    entity.StoreAddId = istVM.StoreIdTo ?? 0;

                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, istVM.StoreInterTransSubVMs, changes, screenCode);

                    changes.AppendLine("IST Updated.");
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, istVM.ISTId == 0 ? "IST Created" : "IST Updated");

                var savedEntity = await _unitOfWork.StoreInterTrans.GetQueryable()
                    .Include(q => q.StoreInterTransSubs).ThenInclude(s => s.Item)
                    //.Include(q => q.StoreInterTransSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.ISTId == entity.ISTId);

                return _mapper.Map<StoreInterTransVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert IST: {istVM.ISTNo}");
                throw new InvalidOperationException("Failed to save IST. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(StoreInterTrans existingSit, List<StoreInterTransSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingSit.StoreInterTransSubs.Select(s => s.ISTSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.ISTSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingSit.StoreInterTransSubs.Where(s => !incomingSubIds.Contains(s.ISTSubId)).ToList())
            {
                await DeleteStockIssueAndTrackAsync(sub.ISTSubId, sub.ItemId.Value, screenCode);

                await DeleteStockAddAndTrackAsync(sub.ISTSubId, sub.ItemId.Value, screenCode);

                changes.AppendLine($"Child Deleted - ISTSubId: {sub.ISTSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.StoreInterTransSubs.DeleteAsync(sub.ISTSubId);
                await _unitOfWork.SaveAsync();
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.ISTSubId == 0)
                {
                    var newSub = _mapper.Map<StoreInterTransSub>(subVM);
                    newSub.ISTId = existingSit.ISTId;
                    await _unitOfWork.StoreInterTransSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    // Update stock for new child: issue from Source and add to Destination
                    await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId!.Value, existingSit.StoreIssueId, newSub.Qty, newSub.UnitPrice, newSub.BatchNo,
                        screenCode, newSub.ISTSubId, existingSit.ISTNo, existingSit.ISTDate);

                    await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId!.Value, existingSit.StoreAddId, newSub.Qty, newSub.UnitPrice, newSub.BatchNo,
                       screenCode, newSub.ISTSubId, existingSit.ISTNo, existingSit.ISTDate, existingSit.MainRemark);
                }
                else
                {
                    var existingSub = existingSit.StoreInterTransSubs.FirstOrDefault(s => s.ISTSubId == subVM.ISTSubId);
                    if (existingSub != null)
                    {
                        await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSit.StoreIssueId, subVM.Qty.GetValueOrDefault(),
                        subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.ISTSubId, existingSit.ISTNo, existingSit.ISTDate);

                        await _stockManagerService.AddOrUpdateStockAsync(existingSub.ItemId!.Value, existingSit.StoreAddId, subVM.Qty.GetValueOrDefault(),
                             subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.ISTSubId, existingSit.ISTNo, existingSit.ISTDate, existingSit.MainRemark);

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        public async Task<(bool CanDelete, string Message)> ToCheck_Stock_Qty_Issued(int ISTId, int screenCode, string refNo)
        {
            try
            {
                var ISTSubIds = await _unitOfWork.StoreInterTransSubs
                    .GetQueryable()
                    .Where(s => s.ISTId == ISTId)
                    .Select(s => s.ISTSubId)
                    .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa =>
                        ISTSubIds.Contains(sa.SubItemRefID) &&
                        sa.ScreenCode == screenCode && sa.RefNo == refNo &&
                        sa.BalQty < sa.AddQty)
                    .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Inter Store Transfer. Some sub-items have already been transacted/issued.");

                return (true, "Inter Store Transfer can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ToCheck_Stock_Qty_Issued for ISTId: {ISTId}");
                throw new Exception("Error checking Inter Store Transfer delete eligibility", ex);
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
                screen: "Inter Store Transfer",
                action: action,
                additionalInfo: changes.ToString()
            );
        }
    }
}
