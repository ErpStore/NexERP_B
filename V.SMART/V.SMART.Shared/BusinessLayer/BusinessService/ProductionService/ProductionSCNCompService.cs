using AutoMapper;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM;
using V.SMART.Shared.Data.AccountsModule;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ProductionService
{
    public class ProductionSCNCompService : IProductionSCNCompService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IProductionLogService _ProductionLogService;
        public ProductionSCNCompService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            IStockManagerService stockManagerService,
            ILoggingService logs,
            IMapper mapper, IProductionLogService productionLogService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;
            _ProductionLogService = productionLogService;
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

        public async Task<(List<ProductionSCNCompVM> scnCompVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ProductionSCNComps.GetQueryable()
                .Include(j => j.IssueStore)
                .Include(j => j.AddStore)
                .Include(j => j.ProductionSCNCompSubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.ProductionSCNCompSubs)
                    .ThenInclude(s => s.ProductionReturnCompSubs)
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
            var vmList = _mapper.Map<List<ProductionSCNCompVM>>(list);

            return (vmList, total);
        }

        public static class SCNFilterBuilder
        {
            public static IQueryable<ProductionSCNComp> ApplyFilter(
                IQueryable<ProductionSCNComp> query, string field, object value)
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
                        return query.Where(x => x.ProductionSCNCompSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.ProductionSCNCompSubs
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

        public async Task<(bool CanDelete, string Message)> CanDeleteProductionSCNAssyAsync(int SCNId, int screenCode, string refNo)
        {
            try
            {
                var SubConSCN = await _unitOfWork.ProductionSCNComps
                              .GetQueryable()
                              .Include(e => e.ProductionSCNCompSubs)
                              .Where(e => e.SCNId == SCNId).FirstOrDefaultAsync();

                if (SubConSCN == null)
                    return (true, "SubCon SCN can be safely deleted.");

                var ScnSubIds = SubConSCN.ProductionSCNCompSubs
                    .Select(es => es.SCNSubId)
                    .ToList();

                bool hasPo = await _unitOfWork.SubConInvSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefSCNSubId.HasValue &&
                        ScnSubIds.Contains(qs.RefSCNSubId.Value));

                if (hasPo)
                    return (false, "Cannot delete this subcon SCN as a SubCon Invoice exists.");

        


                var SCNSubIds = await _unitOfWork.ProductionSCNCompSubs
                    .GetQueryable()
                    .Where(s => s.SCNId == SCNId)
                    .Select(s => s.SCNSubId)
                    .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa =>
                        SCNSubIds.Contains(sa.SubItemRefID) &&
                        sa.ScreenCode == screenCode && sa.RefNo == refNo &&
                        sa.BalQty < sa.AddQty)
                    .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete subcon SCN. Some sub-items have already been transacted/issued.");

                return (true, "subcon SCN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ToCheckStockQtyIssued for SCNId: {SCNId}");
                throw new Exception("Error checking subcon delete eligibility", ex);
            }
        }
        public async Task<bool> DeleteProdAssySCNBySCNIdAsync(int scnId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var productionSCNAssy = await _unitOfWork.ProductionSCNComps
                    .GetQueryable()
                    .Include(e => e.ProductionSCNCompSubs)
                    .FirstOrDefaultAsync(e => e.SCNId == scnId);

                if (productionSCNAssy == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in productionSCNAssy.ProductionSCNCompSubs)
                {

                    await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                    await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                    if (sub.RefReturnSubId > 0)
                    {
                        var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                        await AdjustReturnCompBalanceAsync(sub.RefReturnSubId, totQty, 0, "Production SCN Assembly delete");
                    }
                    if (sub.RcSubId.GetValueOrDefault() > 0)
                    {
                        await SyncRCSubProcessQtyAsync(sub.RcSubId,
                                                        sub.AccQty, sub.RejQty, sub.RewQty, 0, 0, 0,
                                                        "QC Deleted");

                    }
                }


                var ProductionScnAssy = await _unitOfWork.ProductionSCNComps.GetAsync(scnId);
                await _unitOfWork.ProductionSCNComps.DeleteAsync(ProductionScnAssy);

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
            //try
            //{
            //    var productionSCNAssy = await _unitOfWork.ProductionSCNComps
            //        .GetQueryable()
            //        .Include(e => e.ProductionSCNCompSubs)
            //        .FirstOrDefaultAsync(e => e.SCNId == scnId);

            //    if (productionSCNAssy == null)
            //        return false;

            //    var changes = new StringBuilder();

            //    foreach (var sub in productionSCNAssy.ProductionSCNCompSubs)
            //    {

            //        await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
            //        await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

            //        var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
            //        if (sub.RefReturnSubId > 0)
            //        {

            //            await AdjustReturnCompBalanceAsync(sub.RefReturnSubId, totQty, 0, "Production SCN Assembly delete");
            //        }

            //        if (sub.RcSubId.GetValueOrDefault()>0)
            //        {
            //            await AdjustRcBalanceAsync(sub.RcSubId, totQty, 0, "Production SCN Assembly delete");
            //        }
            //    }


            //    var ProductionScnAssy = await _unitOfWork.ProductionSCNComps.GetAsync(scnId);
            //    await _unitOfWork.ProductionSCNComps.DeleteAsync(ProductionScnAssy);

            //    await _unitOfWork.SaveAsync();
            //    await transaction.CommitAsync();

            //    await _logs.LogUserAction(
            //        UserName: await _currentUserService.GetUsernameAsync(),
            //        Machine: _currentUserService.MachineName,
            //        IP_Address: _currentUserService.IpAddress,
            //        screen: "Production SCN Assembly",
            //        action: $"Deleted SCN No: {ProductionScnAssy.SCNNo}",
            //        additionalInfo: $"SCN Id: {ProductionScnAssy.SCNId}\n{changes}"
            //    );

            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    await transaction.RollbackAsync();
            //    await _logs.LogDeveloperError(ex, $"Failed to delete Production SCN Assy: {scnId}");
            //    throw;
            //}
        }

        public async Task<ProductionSCNCompSubVM?> GetSCNCompItemDetailByScnSubIdAsync(int scnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionSCNCompSubs
                    .GetQueryable()
                    .Where(q => q.SCNSubId == scnSubId)
                    .Select(q => new ProductionSCNCompSubVM
                    {
                        BalQty = q.BalQty,
                        AccQty = q.AccQty,
                        RejQty = q.RejQty,
                        RewQty = q.RewQty,
                        SCNBalQty=q.SCNBalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production SCN Assy sub item detail for SCNSubId: {scnSubId}");
                throw new InvalidOperationException("Failed to retrieve Production SCN sub-item details.");
            }
        }

        public async Task<List<ProductionSCNCompSubVM>> GetDistinctRefGRNNOsbySCNIdAsync(int SCNId)
        {
            return await _unitOfWork.ProductionSCNCompSubs
                .GetQueryable()
                .Where(s => s.SCNId == SCNId)
                .GroupBy(s => new { s.ProductionReturnCompSubs.ProductionReturnComp.ReturnNo, s.ProductionReturnCompSubs.ProductionReturnComp.Suffix, s.ProductionReturnCompSubs.ProductionReturnComp.ReturnDate })
                .Select(g => new ProductionSCNCompSubVM
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

        public async Task<ProductionSCNCompVM?> GetProductionSCNByScnIdAsync(int scnId)
        {
            try
            {   
                var entity = await _unitOfWork.ProductionSCNComps.GetQueryable()
                    .Include(q => q.ProductionSCNCompSubs)
                    .Include(q => q.ProductionSCNCompSubs).ThenInclude (s => s.ProductionReturnCompSubs)
                    .Include(q => q.ProductionSCNCompSubs).ThenInclude(s => s.Item)
                    .Include(q => q.ProductionSCNCompSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.AddStore)
                    .Include(q => q.IssueStore)
                    .FirstOrDefaultAsync(q => q.SCNId == scnId);

                if (entity == null)
                    return null;

                var scnVM = _mapper.Map<ProductionSCNCompVM>(entity);

                int? storeId = scnVM.IssueStoreId;

                var itemIds = scnVM.ProductionSCNCompSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (storeId.HasValue && itemIds.Count > 0)
                {
                    var stockDict = await _stockManagerService
                        .GetStockForItemsAsync(itemIds, storeId.Value);

                    foreach (var sub in scnVM.ProductionSCNCompSubVMs)
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

        public async Task<List<ProductionSCNCompSubVM>> GetProdSCNSubBySCNIdAsync(int scnId)
        {
            try
            {
                var subs = await _unitOfWork.ProductionSCNCompSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.SCNId == scnId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ProductionSCNCompSubVM>>(subs);
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
                var lastQuote = await _unitOfWork.ProductionSCNComps
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q =>Convert.ToInt32(q.SCNNo))
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

        public async Task<ProductionSCNCompVM> UpsertproductionSCNAsync(ProductionSCNCompVM prodCompVM, int screenCode)
        {
            if (prodCompVM == null)
                throw new ArgumentNullException(nameof(prodCompVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ProductionSCNComp entity;

                if (prodCompVM.SCNId == 0)
                {
                    entity = _mapper.Map<ProductionSCNComp>(prodCompVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.ProductionSCNComps.GetLastSCNNoAsync(entity.Suffix);
                    entity.SCNNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.ProductionSCNCompSubs = prodCompVM.ProductionSCNCompSubVMs.Select(s => _mapper.Map<ProductionSCNCompSub>(s)).ToList();

                    await _unitOfWork.ProductionSCNComps.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.ProductionSCNCompSubs)
                    {
                        if (sub.RefReturnSubId > 0)
                        {
                            var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                           
                            await AdjustReturnCompBalanceAsync(sub.RefReturnSubId, 0, totQty, "Production SCN Component Creation");

                            //Issue From DifferStore
                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.IssueStoreId.Value, sub.AccQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate,sub.RcSubId, allowMultipleIssue: true);

                            //Add To Differ Store
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, entity.AddStoreId.Value, sub.AccQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.Remark, sub.RcSubId,allowMultipleAdd: true);

                            if (sub.RejQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.IssueStoreId.Value, sub.RejQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.RcSubId, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.RejectionStore, sub.RejQty,
                                    sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RejReason: {sub.RejReason}", sub.RcSubId, allowMultipleAdd: true);
                            }

                            if (sub.RewQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.IssueStoreId.Value, sub.RewQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.RcSubId, allowMultipleIssue: true);
                                if (sub.RcSubId.GetValueOrDefault() > 0)
                                {

                                    var RcsubDetails = await _unitOfWork.RouteCardSubs.GetQueryable()
                                       .FirstOrDefaultAsync(x =>
                                           x.RCSubId == sub.RcSubId &&
                                           x.ItemIdIn == sub.ItemId
                                       );

                                    var sourceRcSubIds = await _ProductionLogService.GetIssueSourceRCSubIdsAsync(RcsubDetails.RCId, RcsubDetails.SeqNo.Value, RcsubDetails.RCSubId);

                                    foreach (var subs in sourceRcSubIds)
                                    {
                                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.ReworkStore, sub.RewQty,
                                        sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RewReason: {sub.RewReason}", subs, allowMultipleAdd: true);
                                    }
                                }
                                else
                                {
                                    await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.ReworkStore, sub.RewQty,
                                       sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RewReason: {sub.RewReason}", sub.RcSubId, allowMultipleAdd: true);

                                }

                                //await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, 7, sub.RewQty,
                                //    sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RewReason: {sub.RewReason}", sub.RcSubId, allowMultipleAdd: true);
                            }

                        }

                        if(sub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await SyncRCSubProcessQtyAsync(sub.RcSubId,
                                                            0, 0, 0,
                                                            sub.AccQty, sub.RejQty, sub.RewQty,
                                                            "QC Create");

                        }
                    }

                    changes.AppendLine("Production Component SCN Created.");
                }
                else
                {
                    entity = await _unitOfWork.ProductionSCNComps.GetQueryable()
                        .Include(q => q.ProductionSCNCompSubs)
                        .FirstOrDefaultAsync(q => q.SCNId == prodCompVM.SCNId)
                        ?? throw new InvalidOperationException("production SCN not found.");

                    var parentChanges = GetPropertyChanges(entity, prodCompVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(prodCompVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, prodCompVM.ProductionSCNCompSubVMs, screenCode, changes);

                    changes.AppendLine("Production Component SCN Updated.");
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, prodCompVM.SCNId == 0 ? "Production SCN Created" : "Production SCN Updated");

                var savedEntity = await _unitOfWork.ProductionSCNComps.GetQueryable()
                    .Include(q => q.ProductionSCNCompSubs).ThenInclude(s => s.Item)
                    .Include(q => q.AddStore)
                    .Include(q => q.IssueStore)
                    .Include(q => q.ProductionSCNCompSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.SCNId == entity.SCNId);

                return _mapper.Map<ProductionSCNCompVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Production Component SCN: {prodCompVM.SCNNo}");
                throw new InvalidOperationException("Failed to save Production Component SCN. Please try again.");
            }
        }



        public async Task SyncRCSubProcessQtyAsync(int? rcSubId,
                       decimal oldAccQty, decimal oldRejQty, decimal oldRewQty,
                       decimal newAccQty, decimal newRejQty, decimal newRewQty,
                       string context)
        {
            try
            {
                if (!rcSubId.HasValue || rcSubId <= 0)
                    return;

                var routeCardProcess = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Include(x => x.RouteCard)
                    .FirstOrDefaultAsync(x => x.RCSubId == rcSubId.Value)
                    ?? throw new InvalidOperationException("Route Card Process not found");

                var oldTotal = oldAccQty + oldRejQty;

                if (oldTotal > 0)
                {
                    routeCardProcess.WipQty += oldTotal + oldRewQty;

                    routeCardProcess.AccQty -= oldAccQty;
                    routeCardProcess.RejQty -= oldRejQty;
                    routeCardProcess.RewQty -= oldRewQty;
                    routeCardProcess.BalQty -= oldRewQty;

                    routeCardProcess.NextProcessQty -= oldAccQty;


                    if (oldRejQty > 0)
                    {

                        var current = await _unitOfWork.RouteCardSubs.GetQueryable()
                           .FirstOrDefaultAsync(x => x.RCSubId == rcSubId.Value);

                        // Current process + all next processes
                        var routeSubs = await _unitOfWork.RouteCardSubs.GetQueryable()
                            .Where(x =>
                                x.RCId == current.RCId &&
                                x.ItemIdIn == routeCardProcess.ItemIdIn &&
                                !x.IsProcessSkip &&
                                x.SlNo >= current.SlNo)
                            .OrderBy(x => x.SlNo)
                            .ToListAsync();

                        foreach (var sub in routeSubs)
                        {
                            if (sub.RCSubId == current.RCSubId)
                                continue;

                            sub.BalQty += oldRejQty;

                            await _unitOfWork.RouteCardSubs.UpdateAsync(sub);

                        }
                    }

                }

                var newTotal = newAccQty + newRejQty;

                if (newTotal > 0)
                {
                    if (newTotal > routeCardProcess.WipQty)
                        throw new InvalidOperationException(
                            $"{context}: Process qty cannot exceed available WIP.");

                    routeCardProcess.WipQty -= newTotal + newRewQty;

                    routeCardProcess.AccQty += newAccQty;
                    routeCardProcess.RejQty += newRejQty;
                    routeCardProcess.RewQty += newRewQty;

                    routeCardProcess.NextProcessQty += newAccQty;
                    routeCardProcess.BalQty += newRewQty;

                    if (newRejQty > 0)
                    {
                        await _ProductionLogService.UpdateRouteCardSubRejQtyAsync(routeCardProcess.ItemIdIn.GetValueOrDefault(), routeCardProcess.ProcessId, newRejQty, routeCardProcess.RCSubId);
                    }


                }




                routeCardProcess.ProcessStatus = await GetProcessStatusAsync(routeCardProcess);

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardProcess);
                await _unitOfWork.SaveAsync();

                if (routeCardProcess.RCId > 0)
                {
                    var rc = await _unitOfWork.RouteCards.GetAsync(routeCardProcess.RCId);

                    if (rc != null)
                    {
                        bool isProcessCompleted =
                            routeCardProcess.IsFinalProcess && routeCardProcess.ProcessStatus == 3

                           &&
                            routeCardProcess.BalQty == 0;

                        rc.RcStatus = isProcessCompleted ? (byte)2 : (byte)1; // 2 completed  // 1 inProgress

                        await _unitOfWork.RouteCards.UpdateAsync(rc);
                        await _unitOfWork.SaveAsync();
                    }
                }

            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[SyncRCSubProcessQtyAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[SyncRCSubProcessQtyAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to sync Route Card process quantities. Please contact support.");
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
                decimal WipQty = routeCardProcess.WipQty;

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
                    int? prevSeqNo = await _ProductionLogService.GetEffectivePreviousSeqNoAsync(routeCardProcess.RCId, routeCardProcess.SeqNo.Value);

                    if (prevSeqNo == null)
                        return 0;

                    compareQty = await _ProductionLogService.GetPrevSeqMinNextQtyAsync(routeCardProcess.RCId, prevSeqNo.Value);
                }

                // =========================
                // STATUS DECISION
                // =========================

                // In Progress
                if ((usedQty > 0 || issuedQty > 0) && balQty > 0)
                    return 1;

                // Partially Completed
                if ((rejQty > 0 || rewQty > 0) && balQty > 0)
                    return 0;

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


        private async Task HandleChildUpdatesAsync(ProductionSCNComp existingProdScn, List<ProductionSCNCompSubVM> incomingSubVMs, int screenCode, StringBuilder changes)
        {
            var existingSubIds = existingProdScn.ProductionSCNCompSubs.Select(s => s.SCNSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.SCNSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingProdScn.ProductionSCNCompSubs.Where(s => !incomingSubIds.Contains(s.SCNSubId)).ToList())
            {
                await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);
                
                changes.AppendLine($"Child Deleted - ScnScubId: {sub.SCNSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ProductionSCNCompSubs.DeleteAsync(sub.SCNSubId);
                await _unitOfWork.SaveAsync();

                var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                await AdjustReturnCompBalanceAsync(sub.RefReturnSubId, totQty, 0, "Production Component SCN delete");

                if (sub.RefReturnSubId > 0)
                {
                    await SyncRCSubProcessQtyAsync(sub.RcSubId,
                                                    sub.AccQty, sub.RejQty, sub.RewQty,
                                                    0, 0, 0,
                                                    "QC Delete");

                }
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.SCNSubId == 0)
                {
                    var newSub = _mapper.Map<ProductionSCNCompSub>(subVM);
                    newSub.SCNId = existingProdScn.SCNId;
                    await _unitOfWork.ProductionSCNCompSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.AccQty}");

                    if (newSub.RefReturnSubId > 0)
                    {
                        var totQty = newSub.AccQty + newSub.RejQty + newSub.RewQty;

                        await AdjustReturnCompBalanceAsync(newSub.RefReturnSubId, 0, totQty, "Production SCN Component Creation");

                        await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingProdScn.IssueStoreId.Value, newSub.AccQty, newSub.UnitPrice, null,
                            screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, newSub.RcSubId, allowMultipleIssue: true);

                        await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, existingProdScn.AddStoreId.Value, newSub.AccQty,
                            newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, newSub.Remark, newSub.RcSubId, allowMultipleAdd: true);

                        if (newSub.RejQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingProdScn.IssueStoreId.Value, newSub.RejQty, newSub.UnitPrice, null,
                                screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, newSub.RcSubId, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, StoreIds.RejectionStore, newSub.RejQty,
                                newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RejReason: {newSub.RejReason}", newSub.RcSubId, allowMultipleAdd: true);
                        }
                        if (newSub.RewQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingProdScn.IssueStoreId.Value, newSub.RewQty, newSub.UnitPrice, null,
                                screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, newSub.RcSubId, allowMultipleIssue: true);
                            
                            if (newSub.RcSubId.GetValueOrDefault() > 0)
                            {
                                var RcsubDetails = await _unitOfWork.RouteCardSubs.GetQueryable()
                                     .FirstOrDefaultAsync(x =>
                                         x.RCSubId == newSub.RcSubId &&
                                         x.ItemIdIn == newSub.ItemId
                                     );

                                var sourceRcSubIds = await _ProductionLogService.GetIssueSourceRCSubIdsAsync(RcsubDetails.RCId, RcsubDetails.SeqNo.Value, RcsubDetails.RCSubId);

                                foreach (var subs in sourceRcSubIds)
                                {
                                    await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, StoreIds.ReworkStore, newSub.RewQty,
                                    newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {newSub.RewReason}", subs, allowMultipleAdd: true);
                                }
                            }
                            else
                            {
                                await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, StoreIds.ReworkStore, newSub.RewQty,
                                   newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {newSub.RewReason}", subVM.RcSubId, allowMultipleAdd: true);
                            }

                            //await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, 7, newSub.RewQty,
                            //    newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {newSub.RewReason}", newSub.RcSubId, allowMultipleAdd: true);
                        }

                        if (newSub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await SyncRCSubProcessQtyAsync(newSub.RcSubId,
                                                            0, 0, 0,
                                                            newSub.AccQty, newSub.RejQty, newSub.RewQty,
                                                            "QC Create");

                        }
                    }
                }
                else
                {
                    var existingSub = existingProdScn.ProductionSCNCompSubs.FirstOrDefault(s => s.SCNSubId == subVM.SCNSubId);
                    if (existingSub != null)
                    {

                        await DeleteStockIssueAndTrackAsync(subVM.SCNSubId, subVM.ItemId.Value, screenCode);
                        await DeleteStockAddAsync(subVM.SCNSubId, subVM.ItemId.Value, screenCode);

                        if (subVM.RefReturnSubId > 0)
                        {
                            var existingQty = existingSub.AccQty + existingSub.RejQty + existingSub.RewQty;
                            var newQty = (subVM.AccQty ?? 0) + (subVM.RejQty ?? 0) + (subVM.RewQty ?? 0);
                            await AdjustReturnCompBalanceAsync(subVM.RefReturnSubId, existingQty, newQty, "Production SCN Com Update");

                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.IssueStoreId.Value, subVM.AccQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(), null,
                                screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, subVM.RcSubId, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.AddStoreId.Value, subVM.AccQty.GetValueOrDefault(),
                                subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, subVM.Remark, subVM.RcSubId, allowMultipleAdd: true);

                            if (subVM.RejQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.IssueStoreId.Value,
                                    subVM.RejQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(), null,
                                    screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, subVM.RcSubId, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.RejectionStore, subVM.RejQty.GetValueOrDefault(),
                                    subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RejReason: {subVM.RejReason}", subVM.RcSubId, allowMultipleAdd: true);

                            }
                            if (subVM.RewQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.IssueStoreId.Value, subVM.RewQty.GetValueOrDefault(),
                                    subVM.UnitPrice.GetValueOrDefault(), null,
                                    screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, subVM.RcSubId, allowMultipleIssue: true);

                                if (subVM.RcSubId.GetValueOrDefault() > 0)
                                {
                                    var RcsubDetails = await _unitOfWork.RouteCardSubs.GetQueryable()
                                   .FirstOrDefaultAsync(x =>
                                       x.RCSubId == subVM.RcSubId &&
                                       x.ItemIdIn == subVM.ItemId
                                   );

                                    var sourceRcSubIds = await _ProductionLogService.GetIssueSourceRCSubIdsAsync(RcsubDetails.RCId, RcsubDetails.SeqNo.Value, RcsubDetails.RCSubId);

                                    foreach (var subs in sourceRcSubIds)
                                    {
                                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.ReworkStore, subVM.RewQty.GetValueOrDefault(),
                                        subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {subVM.RewReason}", subs, allowMultipleAdd: true);
                                    }
                                }
                                else
                                {
                                    await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.ReworkStore, subVM.RewQty.GetValueOrDefault(),
                                       subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {subVM.RewReason}", subVM.RcSubId, allowMultipleAdd: true);
                                }
                            }

                            if (subVM.RcSubId.GetValueOrDefault() > 0)
                            {
                                await SyncRCSubProcessQtyAsync(subVM.RcSubId,
                                        existingSub.AccQty, existingSub.RejQty, existingSub.RewQty,
                                        subVM.AccQty ?? 0, subVM.RejQty ?? 0, subVM.RewQty ?? 0,
                                        "QC Update");
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

        public async Task AdjustReturnCompBalanceAsync(int? refReturnSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refReturnSubId.HasValue || refReturnSubId == 0) return;

                var CompReturnSub = await _unitOfWork.ProductionReturnCompSubs.GetAsync(refReturnSubId.Value);
                if (CompReturnSub == null) return;

                if (oldQty > 0)
                    CompReturnSub.BalQty += oldQty;

                if (newQty > CompReturnSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Production Return BalQty.");

                if (newQty > 0)
                    CompReturnSub.BalQty -= newQty;

                await _unitOfWork.ProductionReturnCompSubs.UpdateAsync(CompReturnSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.ProductionReturnCompSubs
                    .GetQueryable()
                    .Where(e => e.ReturnId == CompReturnSub.ReturnId)
                    .SumAsync(e => e.BalQty);

                var productionReturnComp = await _unitOfWork.ProductionReturnComps.GetAsync(CompReturnSub.ReturnId);
                if (productionReturnComp != null)
                {
                    productionReturnComp.ReturnTally = (totalBalQty == 0);
                    await _unitOfWork.ProductionReturnComps.UpdateAsync(productionReturnComp);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustReturnCompBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustReturnCompBalanceAsync] Unexpected error in {context}");
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
                screen: "Production Component SCN",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<int> GetReturnPendingCountAsync()
        {
            try
            {
                var count = await _unitOfWork.ProductionReturnComps.GetQueryable()
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

        public async Task DeleteAndResequenceAsync(ProductionSCNCompSubVM subitem, ProductionSCNCompVM prodScn, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.SCNSubId > 0)
                {
                    var entity = await _unitOfWork.ProductionSCNCompSubs.GetAsync(subitem.SCNSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    await DeleteStockIssueAndTrackAsync(subitem.SCNSubId, subitem.ItemId.Value, screenCode);
                    await DeleteStockAddAsync(subitem.SCNSubId, subitem.ItemId.Value, screenCode);

                    if (subitem.RefReturnSubId > 0)
                    {
                        var totQty = subitem.AccQty + subitem.RejQty + subitem.RewQty;
                        await AdjustReturnCompBalanceAsync(subitem.RefReturnSubId, totQty.GetValueOrDefault(), 0, "Production SCN Assembly delete");
                    }

                    await _unitOfWork.ProductionSCNCompSubs.DeleteAsync(entity.SCNSubId);
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
                    prodScn.ProductionSCNCompSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.ProductionSCNCompSubs
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

        
        public async Task<List<Dictionary<string, object>>> GetProductionCompReturnDetails()
        {
            try
            {
                var result = await (from e in _unitOfWork.ProductionReturnComps.GetQueryable()
                            join es in _unitOfWork.ProductionReturnCompSubs.GetQueryable()
                                on e.ReturnId equals es.ReturnId
                            where !e.ReturnTally && es.BalQty > 0 && !e.Rejection && !e.Return
                            select new
                            {
                                es.ReturnSubId,
                                e.ReturnNo,
                                e.Suffix,
                                e.ReturnDate,

                                es.RefRcSubId,
                                RCNo = es.RouteCardSubs != null && es.RouteCardSubs.RouteCard != null
                                        ? es.RouteCardSubs.RouteCard.RCNo
                                        : null,
                                RCDate = es.RouteCardSubs != null && es.RouteCardSubs.RouteCard != null
                                        ? es.RouteCardSubs.RouteCard.RCDate
                                        : (DateTime?)null,

                                es.RefPoSubId,
                                PONo = es.MfgPoSub != null && es.MfgPoSub.MfgPo != null
                                        ? es.MfgPoSub.MfgPo.PONo
                                        : null,
                                PODate = es.MfgPoSub != null && es.MfgPoSub.MfgPo != null
                                        ? es.MfgPoSub.MfgPo.PODate
                                        : (DateTime?)null,

                                es.ItemId,
                                ItemCode = es.Item != null ? es.Item.ItemCode : null,
                                ItemName = es.Item != null ? es.Item.ItemName : null,
                                MeasureUnit = es.Item != null ? es.Item.MeasureUnit : null,

                                es.Qty,
                                es.UnitPrice,
                                es.BalQty,

                                CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                ProjectNo = es.CostCenter != null ? es.CostCenter.ProjectNo : null,
                                Process= es.RouteCardSubs.Process.ProcessName,
                            }
                        ).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {

                    ["Selected"] = false,
                    ["RefReturnSubId"] = r.ReturnSubId,
                    ["ReturnNo"] = $"{r.ReturnNo}{r.Suffix}",
                    ["ReturnDate"] = r.ReturnDate,
                    
                    ["RCSubId"] = r.RefRcSubId,
                    ["RCNo"] = r.RCNo == null ? null : $"{r.RCNo}{r.Suffix}",
                    ["RCDt"] = r.RCDate,

                    ["POSubId"] = r.RefPoSubId,
                    ["PONo"]= r.PONo,
                    ["PODt"] = r.PODate,

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["BalQty"] = r.BalQty,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Process"] = r.Process ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production Component Return details");
                throw new InvalidOperationException("Failed to retrieve Production return details. Please try again.");
            }
        }

        public async Task<decimal> GetProdReturnItemBalQtyFromReturnSubId(int returnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionReturnCompSubs.GetQueryable()
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
        public async Task<ProductionSCNCompSubVM?> GetProdScnSubItemDetailByScnSubIdAsync(int scnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionSCNCompSubs
                    .GetQueryable()
                    .Where(q => q.SCNSubId == scnSubId)
                    .Select(q => new ProductionSCNCompSubVM
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

        public async Task<List<ProductionSCNCompStatusVM>> GetProductionSCNComponentStatusListAsync(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<ProductionSCNCompStatusVM>("Sp_GetProductionSCNCompStatusList", status);
                return result.ToList();


            }
            catch (Exception ex)
            {

                throw;
            }
        }


        private async Task AdjustRcBalanceAsync(int? refRcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refRcSubId.HasValue || refRcSubId == 0)
                    return;

                var routeCardSub = await _unitOfWork.RouteCardSubs.GetAsync(refRcSubId.Value);
                if (routeCardSub == null)
                    return;

                if (oldQty > 0)
                {
                    routeCardSub.WipQty += oldQty;
                    routeCardSub.AccQty -= oldQty;
                }

                if (newQty > routeCardSub.WipQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Route Card Item Work in Progress Qty.");

                if (newQty > 0)
                {
                    routeCardSub.WipQty -= newQty;
                    routeCardSub.AccQty += newQty;
                }

               // routeCardSub.ProcessStatus = await GetProcessStatusAsync(routeCardSub);

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardSub);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustRcBalanceAsync] RouteCard validation failed | Context: {context} | RcSubId: {refRcSubId}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustRcBalanceAsync] Unexpected error while adjusting RouteCard balance | Context: {context} | RcSubId: {refRcSubId}");
                throw new InvalidOperationException("Failed to adjust Route Card balance. Please contact support.");
            }
        }

        public async Task<bool> IsProductionLogAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.AnyAsync(i =>
                               i.ScreenName == "Production Return Component" &&
                               i.Display == "Production Log Configuration" &&
                               i.Required);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error checking Production Log configuration");
                throw new InvalidOperationException("Failed to check Production Log configuration. Please try again.", ex);
            }
        }

        public async Task<List<Staff>> GetAllStaffAsync()
              => await _commonService.GetAllStaffAsync();

    }
}
