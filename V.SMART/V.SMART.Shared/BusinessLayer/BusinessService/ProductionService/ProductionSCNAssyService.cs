using AutoMapper;
using V.SMART.Shared.Utility_Constants;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ProductionService
{
    public class ProductionSCNAssyService :IProductionSCNAssyservice
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public ProductionSCNAssyService(
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

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);
        public async Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync()
            => await _commonService.GetAllRejectionReasonAsync();

        public async Task<bool> GetRejectionSelectionEnableAsync()
            => await _commonService.GetRejectionSelectionEnableAsync();



        // 🔹 Production Assembly SCN operations

        public async Task<(List<ProductionSCNAssyVM> scnAssyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ProductionSCNAssys.GetQueryable()
                .Include(j => j.IssueStore)
                .Include(j => j.AddStore)
                .Include(j => j.ProductionSCNAssySubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.ProductionSCNAssySubs)
                    .ThenInclude(s => s.ProductionReturnAssySubs)
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
                .OrderByDescending(x => x.SCNId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<ProductionSCNAssyVM>>(list);

            return (vmList, total);
        }

        public static class SCNFilterBuilder
        {
            public static IQueryable<ProductionSCNAssy> ApplyFilter(
                IQueryable<ProductionSCNAssy> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "SCNNo":
                        {
                            string input = val;
                            if (string.IsNullOrEmpty(input))
                                return query;

                            string part1 = input;
                            string part2 = "";

                            int slashIndex = input.IndexOf('/');

                            if (slashIndex > -1)
                            {
                                part1 = input.Substring(0, slashIndex).Trim();
                                part2 = input.Substring(slashIndex + 1).Trim(); // FIXED
                            }

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.SCNNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "ItemCode":
                        return query.Where(x => x.ProductionSCNAssySubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.ProductionSCNAssySubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.SCNDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.SCNDateNow <= toDate);
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
                    return (false, "Cannot delete Prouction SCN. Some sub-items have already been transacted/issued.");

                return (true, "Production SCN  can be safely deleted.");

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
                        await AdjustReturnAssyBalanceAsync(sub.RefReturnSubId, totQty,0, "Production SCN Assembly delete");
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
                .GroupBy(s => new { s.ProductionReturnAssySubs.ProductionReturnAssy.ReturnNo, s.ProductionReturnAssySubs.ProductionReturnAssy.Suffix, s.ProductionReturnAssySubs.ProductionReturnAssy.ReturnDate})
                .Select(g => new ProductionSCNAssySubVM
                {
                    RefReturnNo = $"{g.Key.ReturnNo}{g.Key.Suffix}",
                    RefReturnDate = g.Key.ReturnDate
                })
                .ToListAsync();
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

        public async Task<ProductionSCNAssyVM?> GetProductionSCNByScnIdAsync(int scnId)
        {
            try
            {
                var entity = await _unitOfWork.ProductionSCNAssys.GetQueryable()
                    .Include(q => q.ProductionSCNAssySubs)
                    .Include(q => q.ProductionSCNAssySubs).ThenInclude(s => s.Item)
                    .Include(q => q.ProductionSCNAssySubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.AddStore)
                    .Include(q => q.IssueStore)
                    .FirstOrDefaultAsync(q => q.SCNId == scnId);

                if (entity == null)
                    return null;

                var scnVM = _mapper.Map<ProductionSCNAssyVM>(entity);

                int? storeId = scnVM.IssueStoreId;

                var itemIds = scnVM.ProductionSCNAssySubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (storeId.HasValue && itemIds.Count > 0)
                {
                    var stockDict = await _stockManagerService
                        .GetStockForItemsAsync(itemIds, storeId.Value);

                    foreach (var sub in scnVM.ProductionSCNAssySubVMs)
                    {
                        if (sub.ItemId.HasValue &&
                            stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                        {
                            sub.StockQty = qty;
                        }
                        else
                        {
                            sub.StockQty = 0m;
                        }
                    }
                }

                return scnVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetProductionSCNByScnIdAsync({scnId})");
                return null;
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

        public async Task<string> GetSCNNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.ProductionSCNAssys
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => Convert.ToInt32(q.SCNNo))
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastQuote != null)
                {
                    var parts = lastQuote.SCNNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Production Assy SCN number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Production SCN number.");
            }
        }

        public async Task<ProductionSCNAssyVM> UpsertproductionSCNAsync(ProductionSCNAssyVM prodAssyVM, int screenCode)
        {
            if (prodAssyVM == null)
                throw new ArgumentNullException(nameof(prodAssyVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ProductionSCNAssy entity;

                if (prodAssyVM.SCNId == 0)
                {
                    entity = _mapper.Map<ProductionSCNAssy>(prodAssyVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.ProductionSCNAssys.GetLastSCNNoAsync(entity.Suffix);
                    entity.SCNNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.ProductionSCNAssySubs = prodAssyVM.ProductionSCNAssySubVMs.Select(s => _mapper.Map<ProductionSCNAssySub>(s)).ToList();

                    await _unitOfWork.ProductionSCNAssys.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.ProductionSCNAssySubs)
                    {
                        if (sub.RefReturnSubId > 0)
                        {
                            var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                            await AdjustReturnAssyBalanceAsync(sub.RefReturnSubId, 0, totQty, "Production SCN Assembly Creation");

                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.IssueStoreId.Value, sub.AccQty, 
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, entity.AddStoreId.Value, sub.AccQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.Remark, allowMultipleAdd: true);

                            if (sub.RejQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.IssueStoreId.Value, sub.RejQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.RejectionStore, sub.RejQty,
                                    sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RejReason: {sub.RejReason}", allowMultipleAdd: true);
                            }
                            if (sub.RewQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.IssueStoreId.Value, sub.RewQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.ReworkStore, sub.RewQty,
                                    sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RewReason: {sub.RewReason}", allowMultipleAdd: true);
                            }
                        }
                    }

                    changes.AppendLine("Production SCN Created.");
                }
                else
                {
                    entity = await _unitOfWork.ProductionSCNAssys.GetQueryable()
                        .Include(q => q.ProductionSCNAssySubs)
                        .FirstOrDefaultAsync(q => q.SCNId == prodAssyVM.SCNId)
                        ?? throw new InvalidOperationException("production SCN not found.");

                    var parentChanges = GetPropertyChanges(entity, prodAssyVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(prodAssyVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, prodAssyVM.ProductionSCNAssySubVMs,screenCode, changes);

                    changes.AppendLine("Quotation Updated.");
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, prodAssyVM.SCNId == 0 ? "Production SCN Created" : "Production SCN Updated");

                var savedEntity = await _unitOfWork.ProductionSCNAssys.GetQueryable()
                    .Include(q => q.ProductionSCNAssySubs).ThenInclude(s => s.Item)
                    .Include(q => q.AddStore)
                    .Include(q => q.IssueStore)
                    .Include(q => q.ProductionSCNAssySubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.SCNId == entity.SCNId);

                return _mapper.Map<ProductionSCNAssyVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Production Assy SCN: {prodAssyVM.SCNNo}");
                throw new InvalidOperationException("Failed to save Production Assy SCN. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(ProductionSCNAssy existingProdScn, List<ProductionSCNAssySubVM> incomingSubVMs,int screenCode, StringBuilder changes)
        {
            var existingSubIds = existingProdScn.ProductionSCNAssySubs.Select(s => s.SCNSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.SCNSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingProdScn.ProductionSCNAssySubs.Where(s => !incomingSubIds.Contains(s.SCNSubId)).ToList())
            {
                await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                changes.AppendLine($"Child Deleted - ScnScubId: {sub.SCNSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ProductionSCNAssySubs.DeleteAsync(sub.SCNSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefReturnSubId > 0)
                {
                    var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                    await AdjustReturnAssyBalanceAsync(sub.RefReturnSubId, totQty, 0, "Production SCN Assembly delete");

                }
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.SCNSubId == 0)
                {
                    var newSub = _mapper.Map<ProductionSCNAssySub>(subVM);
                    newSub.SCNId = existingProdScn.SCNId;
                    await _unitOfWork.ProductionSCNAssySubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.AccQty}");

                    if (newSub.RefReturnSubId > 0)
                    {

                        var totQty = newSub.AccQty + newSub.RejQty + newSub.RewQty;
                        await AdjustReturnAssyBalanceAsync(newSub.RefReturnSubId, 0, totQty, "Production SCN Assembly Creation");

                        await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingProdScn.IssueStoreId.Value, newSub.AccQty, newSub.UnitPrice, null, 
                            screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, allowMultipleIssue: true);

                        await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, existingProdScn.AddStoreId.Value, newSub.AccQty,
                            newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, newSub.Remark, allowMultipleAdd: true);

                        if (newSub.RejQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingProdScn.IssueStoreId.Value, newSub.RejQty, newSub.UnitPrice, null,
                                screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, StoreIds.RejectionStore, newSub.RejQty,
                                newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RejReason: {newSub.RejReason}", allowMultipleAdd: true);
                        }
                        if (newSub.RewQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingProdScn.IssueStoreId.Value, newSub.RewQty, newSub.UnitPrice, null,
                                screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, StoreIds.ReworkStore, newSub.RewQty,
                                newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {newSub.RewReason}", allowMultipleAdd: true);
                        }
                    }
                }
                else
                {
                    var existingSub = existingProdScn.ProductionSCNAssySubs.FirstOrDefault(s => s.SCNSubId == subVM.SCNSubId);
                    if (existingSub != null)
                    {

                        await DeleteStockIssueAndTrackAsync(subVM.SCNSubId, subVM.ItemId.Value, screenCode);
                        await DeleteStockAddAsync(subVM.SCNSubId, subVM.ItemId.Value, screenCode);

                        if (subVM.RefReturnSubId > 0)
                        {
                            var existingQty = existingSub.AccQty + existingSub.RejQty + existingSub.RewQty;
                            var newQty = (subVM.AccQty ?? 0) + (subVM.RejQty ?? 0) + (subVM.RewQty ?? 0);
                            await AdjustReturnAssyBalanceAsync(subVM.RefReturnSubId, existingQty, newQty, "Production SCN Assembly Update");

                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value , existingProdScn.IssueStoreId.Value, subVM.AccQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(), null,
                                screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate,allowMultipleIssue : true);

                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.AddStoreId.Value, subVM.AccQty.GetValueOrDefault(),
                                subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, subVM.Remark, allowMultipleAdd: true);

                            if (subVM.RejQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.IssueStoreId.Value, 
                                    subVM.RejQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(), null,
                                    screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.RejectionStore, subVM.RejQty.GetValueOrDefault(),
                                    subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RejReason: {subVM.RejReason}", allowMultipleAdd: true);

                            }
                            if (subVM.RewQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.IssueStoreId.Value, subVM.RewQty.GetValueOrDefault(), 
                                    subVM.UnitPrice.GetValueOrDefault(), null,
                                    screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.ReworkStore, subVM.RewQty.GetValueOrDefault(),
                                    subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {subVM.RewReason}",allowMultipleAdd: true);
                            }

                        }
                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
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
                await _logs.LogDeveloperError(ex, $"Error fetching Production Assembly Return details");
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


        public async Task<List<ProductionSCNAssyStatusVM>> GetProductionSCNAssyStatusListAsync(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<ProductionSCNAssyStatusVM>("Sp_GetProductionSCNAssyStatusList", status);
                return result.ToList();


            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
