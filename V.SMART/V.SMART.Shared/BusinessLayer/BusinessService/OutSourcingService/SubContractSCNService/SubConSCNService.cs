using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractSCNService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.SubContractSCN;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;
using static V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractSCNService.SubConSCNService;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractSCNService
{
    public class SubConSCNService : ISubConSCNService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IProductionLogService _ProductionLogService;

        public SubConSCNService(
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
        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
        {
            return await _commonService.SearchVendorsAsync(searchText);
        }
        public async Task<VendorVM?> GetVendorByVenerCodeAsync(int vendorCode)
             => await _commonService.GetVendorByVenerCodeAsync(vendorCode);
        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();
        public async Task<bool> GetRejectionSelectionEnableAsync()
              => await _commonService.GetRejectionSelectionEnableAsync();
        public async Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync()
             => await _commonService.GetAllRejectionReasonAsync();

        public async Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId)
           => await _stockManagerService.GetStockForItemAsync(ItemId, StoreId);

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


        public async Task<IEnumerable<VendorVM>> SearchVendors(string value, CancellationToken token)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 3)
                return Enumerable.Empty<VendorVM>();

            try
            {
                token.ThrowIfCancellationRequested();

                var query = from e in _unitOfWork.SubConGRNs.GetQueryable()
                            join es in _unitOfWork.SubConGRNSubs.GetQueryable()
                                on e.GRNId equals es.GRNId
                            join v in _unitOfWork.Vendors.GetQueryable()
                                on e.VendorCode equals v.VendorCode
                            where !e.GRNTally && es.BalQty > 0 && !e.Rejection
                                  && v.VendorName.Contains(value)
                            select new VendorVM
                            {
                                VendorCode = v.VendorCode, // ensure this is int in your VM model
                                VendorName = v.VendorName,
                            };

                // Execute DB query first, then apply Distinct on client
                var list = query.ToList();

                var distinctVendors = list.DistinctBy(x => x.VendorCode);
                return distinctVendors;
            }
            catch (OperationCanceledException)
            {
                return Enumerable.Empty<VendorVM>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"SearchVendors failed for value: {value}");
                return Enumerable.Empty<VendorVM>();
            }
        }


        // 🔹 Production Assembly SCN operations

        public async Task<(List<SubConSCNVM> scnCompVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.SubConSCNs.GetQueryable()
                .Include(j => j.IssueStore)
                .Include(j => j.AddStore)
                .Include(j => j.SubConSCNSubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.SubConSCNSubs)
                    .ThenInclude(s => s.SubConGRNSub)
                .Include(j => j.vendor)
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
            var vmList = _mapper.Map<List<SubConSCNVM>>(list);

            return (vmList, total);
        }

        public static class SCNFilterBuilder
        {
            public static IQueryable<SubConSCN> ApplyFilter(
                IQueryable<SubConSCN> query, string field, object value)
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
                        return query.Where(x => x.SubConSCNSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.SubConSCNSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "Vendor":
                        return query.Where(x => x.vendor.VendorName.Contains(value.ToString()));

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
                    case "Status":
                        return ApplyStatusFilter(query, val.ToString());
                }

                return query;
            }
            private static IQueryable<SubConSCN> ApplyStatusFilter(
            IQueryable<SubConSCN> query, string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.SCNTally || x.SubConSCNSubs.Any(s => s.BalQty == 0)),

                        "Pending" =>
                            query.Where(x =>
                                !x.SCNTally &&
                                !x.SCNCancel &&
                                !x.ShortClose),

                        "Cancelled" =>
                            query.Where(x => x.SCNCancel),

                        "Short Closed" =>
                            query.Where(x => x.ShortClose),

                        _ => query
                    };
                }
                catch
                {
                    return query;
                }
            }
        }
        public async Task<(bool CanDelete, string Message)> CanDeleteSubConSCNAsync(int SCNId, int screenCode, string refNo)
        {
            try
            {
                var SubConSCN = await _unitOfWork.SubConSCNs
                              .GetQueryable()
                              .Include(e => e.SubConSCNSubs)
                              .Where(e => e.SCNId == SCNId).FirstOrDefaultAsync();

                if (SubConSCN == null)
                    return (true, "SubCon SCN can be safely deleted.");

                var ScnSubIds = SubConSCN.SubConSCNSubs
                    .Select(es => es.SCNSubId)
                    .ToList();

                bool hasPo = await _unitOfWork.SubConInvSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefSCNSubId.HasValue &&
                        ScnSubIds.Contains(qs.RefSCNSubId.Value));

                if (hasPo)
                    return (false, "Cannot delete this subcon SCN as a SubCon Invoice exists.");

                if (SubConSCN.SCNCancel || SubConSCN.ShortClose)
                    return (false, "Cannot delete this subcon SCN as it is Cancelled or Short-Closed.");




                var SCNSubIds = await _unitOfWork.SubConSCNSubs
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
                var productionSCNAssy = await _unitOfWork.SubConSCNs
                    .GetQueryable()
                    .Include(e => e.SubConSCNSubs)
                    .FirstOrDefaultAsync(e => e.SCNId == scnId);

                if (productionSCNAssy == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in productionSCNAssy.SubConSCNSubs)
                {

                    await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                    await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                    if (sub.RefGRNSubId > 0)
                    {
                        var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                        await AdjustReturnCompBalanceAsync(sub.RefGRNSubId, totQty, 0, "Production SCN Assembly delete");
                    }
                    if (sub.RcSubId.GetValueOrDefault() > 0)
                    {
                        await SyncRCSubProcessQtyAsync(sub.RcSubId,
                                                        sub.AccQty, sub.RejQty, sub.RewQty, 0, 0, 0,
                                                        "QC Deleted");

                    }
                }


                var ProductionScnAssy = await _unitOfWork.SubConSCNs.GetAsync(scnId);
                await _unitOfWork.SubConSCNs.DeleteAsync(ProductionScnAssy);

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

        public async Task<SubConSCNSubVM?> GetSCNCompItemDetailByScnSubIdAsync(int scnSubId)
        {
            try
            {
                return await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .Where(q => q.SCNSubId == scnSubId)
                    .Select(q => new SubConSCNSubVM
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

        public async Task<List<SubConSCNSubVM>> GetDistinctRefGRNNOsbySCNIdAsync(int SCNId)
        {
            return await _unitOfWork.SubConSCNSubs
                .GetQueryable()
                .Where(s => s.SCNId == SCNId)
                .GroupBy(s => new { s.SubConGRNSub.SubConGRNs.GRNNo, s.SubConGRNSub.SubConGRNs.Suffix, s.SubConGRNSub.SubConGRNs.GRNDate })
                .Select(g => new SubConSCNSubVM
                {
                    RefGRNNo = $"{g.Key.GRNNo}{g.Key.Suffix}",
                    RefGRNDate = g.Key.GRNDate
                })
                .ToListAsync();
        }




        public async Task<bool> IsSCNTransactionsMatchedAsync(
     int scnId,
     SubConSCNVM scnVms,
     int storeId,
     int screenCode)
        {
            try
            {
                // Get all SCN SubIds
                var scnSubIds = await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == scnId)
                    .Select(x => x.SCNSubId)
                    .ToListAsync();

                if (!scnSubIds.Any())
                    return false;

                // -------------------------------
                // 1️⃣ Check SubCon Invoice references
                // -------------------------------
                var hasInvTransactions = await _unitOfWork.SubConInvSubs
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.RefSCNSubId.HasValue &&
                        scnSubIds.Contains(x.RefSCNSubId.Value));

                // -------------------------------
                // 2️⃣ Check Stock Transactions
                // -------------------------------
                var itemIds = scnVms?.SubConSCNSubVMs?
                    .Select(x => x.ItemId)
                    .Distinct()
                    .ToList();

                var hasStockTransactions = false;

                if (itemIds != null && itemIds.Any())
                {
                    hasStockTransactions = await _unitOfWork.StockAdds
                        .GetQueryable()
                        .AnyAsync(s =>
                            itemIds.Contains(s.ItemId) &&
                            s.StoreId == storeId &&
                            s.ScreenCode == screenCode &&
                            scnSubIds.Contains(s.SubItemRefID) &&
                            s.AddQty != s.BalQty);
                }

                // -------------------------------
                // 3️⃣ Quantity mismatch check
                // -------------------------------
                bool qtyMismatch = false;

                var list = scnVms?.SubConSCNSubVMs;

                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.AccQty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty);

                    qtyMismatch = totalQty != totalBalQty;
                }

                // -------------------------------
                // Final Result
                // -------------------------------
                return hasInvTransactions ||
                       hasStockTransactions ||
                       qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error while checking SCN transactions for SCNId: {scnId}");

                throw new InvalidOperationException(
                    "Failed to verify SCN transaction status.",
                    ex);
            }
        }

        public async Task<SubConSCNVM?> GetProductionSCNByScnIdAsync(int scnId)
        {
            try
            {
                var entity = await _unitOfWork.SubConSCNs.GetQueryable()
                    .Include(q => q.SubConSCNSubs)
                    .Include(q => q.SubConSCNSubs).ThenInclude(s => s.SubConGRNSub).ThenInclude(g=>g.SubConGRNs)
                    .Include(q => q.SubConSCNSubs).ThenInclude(s => s.Item)
                    .Include(q => q.SubConSCNSubs).ThenInclude(s => s.CostCenter)
                     .Include(q => q.SubConSCNSubs).ThenInclude(s => s.PurchPoSub).ThenInclude(p=>p.PurchPo)
                    .Include(q => q.AddStore)
                    .Include(q => q.IssueStore)
                    .Include(q => q.SubConSCNSubs)
                    .ThenInclude(s => s.RouteCardSub)
                        .ThenInclude(s => s.RouteCard)
                    .Include(q => q.SubConSCNSubs).ThenInclude(s => s.RouteCardSub).ThenInclude(s => s.Process)
                    .FirstOrDefaultAsync(q => q.SCNId == scnId);

                if (entity == null)
                    return null;

                var scnVM = _mapper.Map<SubConSCNVM>(entity);

                int? storeId = scnVM.IssueStoreId;

                var itemIds = scnVM.SubConSCNSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (storeId.HasValue && itemIds.Count > 0)
                {
                    var stockDict = await _stockManagerService
                        .GetStockForItemsAsync(itemIds, storeId.Value);

                    foreach (var sub in scnVM.SubConSCNSubVMs)
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

        public async Task<List<SubConSCNSubVM>> GetProdSCNSubBySCNIdAsync(int scnId)
        {
            try
            {
                var subs = await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.SCNId == scnId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<SubConSCNSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ProductionSCNSub items for SCNId: {scnId}");
                throw new InvalidOperationException("Failed to retrieve Production SCN sub-items. Please try again.");
            }
        }
        public async Task<List<SubConSCNSub>> GetSCNSubDetailsBySCNIdAsync(int scnId)
        {
            try
            {
                var subs = await _unitOfWork.SubConSCNSubs
                                 .GetQueryable()
                                 .Where(s => s.SCNId == scnId)
                                 .OrderBy(s => s.SlNo)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching SubCon SCN items for SCNId: {scnId}");
                throw new InvalidOperationException("Failed to retrieve Suncon SCN sub-items. Please try again.");
            }
        }


        public async Task<string> GetSCNNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.SubConSCNs
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.SCNNo)
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

        public async Task<SubConSCNVM> UpsertproductionSCNAsync(SubConSCNVM prodCompVM, int screenCode)
        {
            if (prodCompVM == null)
                throw new ArgumentNullException(nameof(prodCompVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                SubConSCN entity;

                if (prodCompVM.SCNId == 0)
                {
                    entity = _mapper.Map<SubConSCN>(prodCompVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.SubConSCNs.GetLastSCNNoAsync(entity.Suffix);
                    entity.SCNNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.SubConSCNSubs = prodCompVM.SubConSCNSubVMs.Select(s => _mapper.Map<SubConSCNSub>(s)).ToList();

                    await _unitOfWork.SubConSCNs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.SubConSCNSubs)
                    {
                        if (sub.RefGRNSubId > 0)
                        {
                            var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                            await AdjustReturnCompBalanceAsync(sub.RefGRNSubId, 0, totQty, "Production SCN Component Creation");

                            //Issue From DifferStore
                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.IssueStoreId.Value, sub.AccQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.RcSubId, allowMultipleIssue: true);

                            //Add To Differ Store
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, entity.AddStoreId.Value, sub.AccQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.Remark, sub.RcSubId, allowMultipleAdd: true);

                            if (sub.RejQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.IssueStoreId.Value, sub.RejQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.RcSubId, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, 6, sub.RejQty,
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

                                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, 7, sub.RewQty,
                                            sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RewReason: {sub.RewReason}", subs, allowMultipleAdd: true);
                                    }
                                }
                                else
                                {
                                    await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, 7, sub.RewQty,
                                            sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RewReason: {sub.RewReason}", sub.RcSubId, allowMultipleAdd: true);
                                }

                                //await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, 7, sub.RewQty,
                                //    sub.UnitPrice, null, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, $"RewReason: {sub.RewReason}", sub.RcSubId, allowMultipleAdd: true);
                            }

                        }

                        if (sub.RcSubId.GetValueOrDefault() > 0)
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
                    entity = await _unitOfWork.SubConSCNs.GetQueryable()
                        .Include(q => q.SubConSCNSubs)
                        .FirstOrDefaultAsync(q => q.SCNId == prodCompVM.SCNId)
                        ?? throw new InvalidOperationException("production SCN not found.");

                    var parentChanges = GetPropertyChanges(entity, prodCompVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(prodCompVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, prodCompVM.SubConSCNSubVMs, screenCode, changes);

                    changes.AppendLine("Production Component SCN Updated.");
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, prodCompVM.SCNId == 0 ? "Production SCN Created" : "Production SCN Updated");

                var savedEntity = await _unitOfWork.SubConSCNs.GetQueryable()
                    .Include(q => q.SubConSCNSubs).ThenInclude(s => s.Item)
                    .Include(q => q.AddStore)
                    .Include(q => q.IssueStore)
                    .Include(q => q.SubConSCNSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.SCNId == entity.SCNId);

                return _mapper.Map<SubConSCNVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Production Component SCN: {prodCompVM.SCNNo}");
                throw new InvalidOperationException("Failed to save Production Component SCN. Please try again.");
            }
        }

        private async Task SyncRCSubProcessQtyAsync(int? rcSubId,
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
                            routeCardProcess.IsFinalProcess && routeCardProcess.ProcessStatus == 3 &&
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

        private async Task HandleChildUpdatesAsync(SubConSCN existingProdScn, List<SubConSCNSubVM> incomingSubVMs, int screenCode, StringBuilder changes)
        {
            var existingSubIds = existingProdScn.SubConSCNSubs.Select(s => s.SCNSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.SCNSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingProdScn.SubConSCNSubs.Where(s => !incomingSubIds.Contains(s.SCNSubId)).ToList())
            {
                await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                changes.AppendLine($"Child Deleted - ScnScubId: {sub.SCNSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.SubConSCNSubs.DeleteAsync(sub.SCNSubId);
                await _unitOfWork.SaveAsync();

                var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                await AdjustReturnCompBalanceAsync(sub.RefGRNSubId, totQty, 0, "Production Component SCN delete");

                if (sub.RefGRNSubId > 0)
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
                    var newSub = _mapper.Map<SubConSCNSub>(subVM);
                    newSub.SCNId = existingProdScn.SCNId;
                    await _unitOfWork.SubConSCNSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.AccQty}");

                    if (newSub.RefGRNSubId > 0)
                    {
                        var totQty = newSub.AccQty + newSub.RejQty + newSub.RewQty;

                        await AdjustReturnCompBalanceAsync(newSub.RefGRNSubId, 0, totQty, "Production SCN Component Creation");

                        await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingProdScn.IssueStoreId.Value, newSub.AccQty, newSub.UnitPrice, null,
                            screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, newSub.RcSubId, allowMultipleIssue: true);

                        await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, existingProdScn.AddStoreId.Value, newSub.AccQty,
                            newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, newSub.Remark, newSub.RcSubId, allowMultipleAdd: true);

                        if (newSub.RejQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingProdScn.IssueStoreId.Value, newSub.RejQty, newSub.UnitPrice, null,
                                screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, newSub.RcSubId, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, 6, newSub.RejQty,
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

                                    await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, 7, newSub.RewQty,
                                        newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {newSub.RewReason}", subs, allowMultipleAdd: true);
                                }
                            }
                            else
                            {
                                await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId, 7, newSub.RewQty,
                                 newSub.UnitPrice, null, screenCode, newSub.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {newSub.RewReason}", newSub.RcSubId, allowMultipleAdd: true);
                            }





                            
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
                    var existingSub = existingProdScn.SubConSCNSubs.FirstOrDefault(s => s.SCNSubId == subVM.SCNSubId);
                    if (existingSub != null)
                    {

                        await DeleteStockIssueAndTrackAsync(subVM.SCNSubId, subVM.ItemId.Value, screenCode);
                        await DeleteStockAddAsync(subVM.SCNSubId, subVM.ItemId.Value, screenCode);

                        if (subVM.RefGRNSubId > 0)
                        {
                            var existingQty = existingSub.AccQty + existingSub.RejQty + existingSub.RewQty;
                            var newQty = (subVM.AccQty ?? 0) + (subVM.RejQty ?? 0) + (subVM.RewQty ?? 0);
                            await AdjustReturnCompBalanceAsync(subVM.RefGRNSubId, existingQty, newQty, "Production SCN Com Update");

                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.IssueStoreId.Value, subVM.AccQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(), null,
                                screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, subVM.RcSubId, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.AddStoreId.Value, subVM.AccQty.GetValueOrDefault(),
                                subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, subVM.Remark, subVM.RcSubId, allowMultipleAdd: true);

                            if (subVM.RejQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingProdScn.IssueStoreId.Value,
                                    subVM.RejQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(), null,
                                    screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, subVM.RcSubId, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, 6, subVM.RejQty.GetValueOrDefault(),
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

                                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, 7, subVM.RewQty.GetValueOrDefault(),
                                            subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.SCNSubId, existingProdScn.SCNNo, existingProdScn.SCNDate, $"RewReason: {subVM.RewReason}", subs, allowMultipleAdd: true);
                                    }
                                }
                                else
                                {
                                    await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, 7, subVM.RewQty.GetValueOrDefault(),
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

        private async Task AdjustReturnCompBalanceAsync(int? refReturnSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refReturnSubId.HasValue || refReturnSubId == 0) return;

                var CompReturnSub = await _unitOfWork.SubConGRNSubs.GetAsync(refReturnSubId.Value);
                if (CompReturnSub == null) return;

                if (oldQty > 0)
                    CompReturnSub.BalQty += oldQty;

                if (newQty > CompReturnSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Production Return BalQty.");

                if (newQty > 0)
                    CompReturnSub.BalQty -= newQty;

                await _unitOfWork.SubConGRNSubs.UpdateAsync(CompReturnSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .Where(e => e.GRNId == CompReturnSub.GRNId && e.TransType == "In")
                    .SumAsync(e => e.BalQty);

                var productionReturnComp = await _unitOfWork.SubConGRNs.GetAsync(CompReturnSub.GRNId);
                if (productionReturnComp != null)
                {
                    productionReturnComp.GRNTally = (totalBalQty == 0);
                    await _unitOfWork.SubConGRNs.UpdateAsync(productionReturnComp);
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
                var count = await _unitOfWork.SubConGRNs.GetQueryable()
                    .Where(x => x.GRNTally == false)
                    .CountAsync();

                return count;
            }
            catch (Exception ex)
            {
                // optional: log or handle
                throw new Exception("Error fetching pending return count", ex);
            }
        }

        public async Task DeleteAndResequenceAsync(SubConSCNSubVM subitem, SubConSCNVM prodScn, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.SCNSubId > 0)
                {
                    var entity = await _unitOfWork.SubConSCNSubs.GetAsync(subitem.SCNSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    await DeleteStockIssueAndTrackAsync(subitem.SCNSubId, subitem.ItemId.Value, screenCode);
                    await DeleteStockAddAsync(subitem.SCNSubId, subitem.ItemId.Value, screenCode);

                    if (subitem.RefGRNSubId > 0)
                    {
                        var totQty = subitem.AccQty + subitem.RejQty + subitem.RewQty;
                        await AdjustReturnCompBalanceAsync(subitem.RefGRNSubId, totQty.GetValueOrDefault(), 0, "Production SCN Assembly delete");
                    }

                    await _unitOfWork.SubConSCNSubs.DeleteAsync(entity.SCNSubId);
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
                    prodScn.SubConSCNSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.SubConSCNSubs
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
        public async Task<List<Dictionary<string, object>>> GetProductionCompReturnDetails(int vendorCode)
        {
            try
            {
                var data = await (from e in _unitOfWork.SubConGRNs.GetQueryable()
                                  join es in _unitOfWork.SubConGRNSubs.GetQueryable()
                                      on e.GRNId equals es.GRNId
                                  where !e.GRNTally
                                        && es.BalQty > 0
                                        && !e.Rejection && !e.Cancel && !e.ShortClose
                                        && !e.Return && e.VendorCode == vendorCode && es.TransType == "In"

                                  select new
                                  {
                                      es.GRNSubId,
                                      e.GRNNo,
                                      e.Suffix,
                                      e.GRNDate,

                                      es.RefRcSubId,
                                      RCNo = es.RouteCardSubs.RouteCard != null ? es.RouteCardSubs.RouteCard.RCNo : "",
                                      RCDate = es.RouteCardSubs.RouteCard != null ? es.RouteCardSubs.RouteCard.RCDate : (DateTime?)null,

                                      es.RefPoSubId,
                                      PONo = es.PurchPoSub.PurchPo != null ? es.PurchPoSub.PurchPo.PONo : "",
                                      PODate = es.PurchPoSub.PurchPo != null ? es.PurchPoSub.PurchPo.PODate : (DateTime?)null,

                                      es.ItemId,
                                      ItemCode = es.Item != null ? es.Item.ItemCode : "",
                                      ItemName = es.Item != null ? es.Item.ItemName : "",
                                      UOM = es.Item != null ? es.Item.MeasureUnit : "",

                                      es.Qty,
                                      es.UnitPrice,
                                      es.BalQty,

                                      CostCenterId = es.CostId == 0 ? null : (int?)es.CostId,
                                      ProjectNo = es.CostCenter != null ? es.CostCenter.ProjectNo : ""
                                  }).ToListAsync();

                return data.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefReturnSubId"] = r.GRNSubId,
                    ["ReturnNo"] = $"{r.GRNNo}{r.Suffix}",
                    ["ReturnDate"] = r.GRNDate,

                    ["RCSubId"] = r.RefRcSubId,
                    ["RCNo"] = r.RCNo != null ? $"{r.RCNo}{r.Suffix}" : string.Empty,
                    ["RCDt"] = r.RCDate,

                    ["POSubId"] = r.RefPoSubId,
                    ["PONo"] = r.PONo,
                    ["PODt"] = r.PODate,

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode,
                    ["ItemName"] = r.ItemName,
                    ["UOM"] = r.UOM,

                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,

                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production Component Return details");
                throw new InvalidOperationException("Failed to retrieve Production return details. Please try again.", ex);
            }
        }


        //public async Task<List<Dictionary<string, object>>> GetProductionCompReturnDetails()
        //{
        //    try
        //    {
        //        var result = await (from e in _unitOfWork.SubConGRNs.GetQueryable()
        //                            join es in _unitOfWork.SubConGRNSubs.GetQueryable()
        //                                on e.ReturnId equals es.ReturnId
        //                            where !e.ReturnTally && es.BalQty > 0 && !e.Rejection && !e.Return
        //                            select new
        //                            {
        //                                es.ReturnSubId,
        //                                e.ReturnNo,
        //                                e.Suffix,
        //                                e.ReturnDate,
        //                                es.RefRcSubId,
        //                                es.RouteCardSubs.RouteCard.RCNo,
        //                                es.RouteCardSubs.RouteCard.RCDate,

        //                                es.RefPoSubId,
        //                                es.PurchPoSub.PurchPo.PONo,
        //                                es.PurchPoSub.PurchPo.PODate,

        //                                es.ItemId,
        //                                es.Item.ItemCode,
        //                                es.Item.ItemName,
        //                                es.Item.MeasureUnit,
        //                                es.Qty,
        //                                es.UnitPrice,
        //                                es.BalQty,
        //                                CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
        //                                es.CostCenter.ProjectNo
        //                            }).ToListAsync();

        //        return result.Select(r => new Dictionary<string, object>
        //        {

        //            ["Selected"] = false,
        //            ["RefReturnSubId"] = r.ReturnSubId,
        //            ["ReturnNo"] = $"{r.ReturnNo}{r.Suffix}",
        //            ["ReturnDate"] = r.ReturnDate,

        //            ["RCSubId"] = r.RefRcSubId,
        //            ["RCNo"] = r.RCNo,
        //            ["RCDt"] = r.RCDate,

        //            ["POSubId"] = r.RefPoSubId,
        //            ["PONo"]= r.PONo,
        //            ["PODt"] = r.PODate,

        //            ["ItemId"] = r.ItemId,
        //            ["ItemCode"] = r.ItemCode ?? string.Empty,
        //            ["ItemName"] = r.ItemName ?? string.Empty,
        //            ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,
        //            ["Qty"] = r.Qty,
        //            ["UnitPrice"] = r.UnitPrice,
        //            ["BalQty"] = r.BalQty,
        //            ["CostCenterId"] = r.CostCenterId ?? (int?)null,
        //            ["ProjectNo"] = r.ProjectNo ?? string.Empty,
        //        }).ToList();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"Error fetching Production Component Return details");
        //        throw new InvalidOperationException("Failed to retrieve Production return details. Please try again.");
        //    }
        //}

        public async Task<decimal> GetProdReturnItemBalQtyFromReturnSubId(int returnSubId)
        {
            try
            {
                return await _unitOfWork.SubConGRNSubs.GetQueryable()
                    .Where(e => e.GRNSubId == returnSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for Production ReturnSubId: {returnSubId}");
                throw new InvalidOperationException("Failed to retrieve Production Return balance quantity.");
            }
        }
        public async Task<SubConSCNSubVM?> GetProdScnSubItemDetailByScnSubIdAsync(int scnSubId)
        {
            try
            {
                return await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .Where(q => q.SCNSubId == scnSubId)
                    .Select(q => new SubConSCNSubVM
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
        public async Task<SubConSCNVM> GetSubconSCNByIdAsync(int SCNId)
        {
            try
            {
                var entity = await _unitOfWork.SubConSCNs.GetQueryable()
                              .AsNoTracking()
                              .AsSplitQuery()
                              .Include(q => q.SubConSCNSubs).ThenInclude(s => s.SubConGRNSub.SubConGRNs)
                              .Include(q => q.SubConSCNSubs)
                              .ThenInclude(s => s.Item)
                              .ThenInclude(c => c.Category)
                              .Include(q => q.vendor)
                              .FirstOrDefaultAsync(q => q.SCNId == SCNId);

                var scnVM = _mapper.Map<SubConSCNVM?>(entity);

                //var itemIds = scnVM.SubConSCNSubs
                //    .Where(s => s.ItemId.HasValue)
                //    .Select(s => s.ItemId!.Value)
                //    .Distinct()
                //    .ToList();

                //if (itemIds.Count > 0 && scnVM.StoreIssId.HasValue)
                //{
                //    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, scnVM.StoreIssId ?? 0);

                //    foreach (var sub in scnVM.LabourSCNSubVMs)
                //    {
                //        if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId ?? 0, out var qty))
                //            sub.StockQty = qty;
                //        else
                //            sub.StockQty = 0m;
                //    }
                //}

                return scnVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetLabourSCNByIdAsync({SCNId})");
                return null;
            }
        }
        public async Task<VendorVM?> GetVendorByIdAsync(int vendorCode)
                => await _commonService.GetVendorByVenerCodeAsync(vendorCode);
        public async Task UpdatedCancelStatusAndAddOrRevertQty(SubConSCNVM scnVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingSCN = await _unitOfWork.SubConSCNs.GetAsync(scnVM.SCNId);
                if (existingSCN == null)
                    throw new InvalidOperationException("SubCon SCN not found.");

                var subs = await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .Where(s => s.SCNId == scnVM.SCNId)
                    .ToListAsync();

                if (!scnVM.SCNCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateGRNBalanceBeforeRevertAsync(sub);
                    }
                }

                existingSCN.SCNCancel = scnVM.SCNCancel;
                existingSCN.CancelReason = scnVM.CancelReason;
                existingSCN.CancelDate = scnVM.CancelDate;
                existingSCN.CanceledBy = scnVM.CanceledBy;

                await _unitOfWork.SubConSCNs.UpdateAsync(existingSCN);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingSCN.SCNCancel)
                    {
                        await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                        await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                        if (sub.RefGRNSubId > 0)
                        {
                            var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                            await AdjustReturnCompBalanceAsync(sub.RefGRNSubId, totQty, 0, "Production SCN Assembly delete");
                        }
                        if (sub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await SyncRCSubProcessQtyAsync(sub.RcSubId,
                                                            sub.AccQty, sub.RejQty, sub.RewQty, 0, 0, 0,
                                                            "QC Deleted");

                        }
                    }
                    else
                    {
                        if (sub.RefGRNSubId > 0)
                        {
                            var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                            await AdjustReturnCompBalanceAsync(sub.RefGRNSubId, 0, totQty, "Production SCN Component Creation");

                            //Issue From DifferStore
                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingSCN.IssueStoreId.Value, sub.AccQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, sub.RcSubId, allowMultipleIssue: true);

                            //Add To Differ Store
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, existingSCN.AddStoreId.Value, sub.AccQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, sub.Remark, sub.RcSubId, allowMultipleAdd: true);

                            if (sub.RejQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingSCN.IssueStoreId.Value, sub.RejQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, sub.RcSubId, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, 6, sub.RejQty,
                                    sub.UnitPrice, null, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, $"RejReason: {sub.RejReason}", sub.RcSubId, allowMultipleAdd: true);
                            }

                            if (sub.RewQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingSCN.IssueStoreId.Value, sub.RewQty,
                                sub.UnitPrice, null, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, sub.RcSubId, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, 7, sub.RewQty,
                                    sub.UnitPrice, null, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, $"RewReason: {sub.RewReason}", sub.RcSubId, allowMultipleAdd: true);
                            }

                        }

                        if (sub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await SyncRCSubProcessQtyAsync(sub.RcSubId,
                                                            0, 0, 0,
                                                            sub.AccQty, sub.RejQty, sub.RewQty,
                                                            "QC Create");

                        }
                    }
                }

                await transaction.CommitAsync();
                scnVM = await GetProductionSCNByScnIdAsync(scnVM.SCNId);
            }
            catch (InvalidOperationException ex)
            {
                scnVM = await GetProductionSCNByScnIdAsync(scnVM.SCNId);
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Unexpected error");
                throw new InvalidOperationException("Failed to update cancel/revert status. Please contact support.");
            }
        }
        public async Task UpsertSubConSCNShortCloseAsync(SubConSCNVM SubConSCNVMs)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingGRN = await _unitOfWork.SubConSCNs.GetAsync(SubConSCNVMs.SCNId);
                if (existingGRN == null)
                    throw new InvalidOperationException("SubConSCN not found.");

                existingGRN.ShortClose = SubConSCNVMs.ShortClose;

                await _unitOfWork.SubConSCNs.UpdateAsync(existingGRN);
                await _unitOfWork.SaveAsync();

                await UpdateSubConSCNTallyStatusAsync(SubConSCNVMs.SCNId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertlabourGRNShortCloseAsync] Validation issue");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertlabourGRNShortCloseAsync] Unexpected error");

            }
        }
        public async Task UpdateSubConSCNTallyStatusAsync(int SCNId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == SCNId)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var SCN = await _unitOfWork.SubConSCNs.GetAsync(SCNId);
                if (SCN == null)
                    return;

                if (SCN.ShortClose || SCN.SCNCancel)
                    return;

                SCN.SCNCancel = (totalBalQty == 0);

                await _unitOfWork.SubConSCNs.UpdateAsync(SCN);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateSubConSCNTallyStatusAsync] Error updating SCNID {SCNId}");

            }
        }
        public async Task ValidateGRNBalanceBeforeRevertAsync(SubConSCNSub sub)
        {
            if (sub.RefGRNSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.SubConGRNSubs.GetAsync(sub.RefGRNSubId ?? 0);
            if (entity == null)
                throw new InvalidOperationException($"SubCon GRN not found for RefGRNSubId: {sub.RefGRNSubId}");

            if (entity.BalQty < sub.AccQty && entity.TransType == "In")
            {
                throw new InvalidOperationException($"Cannot revert because GRN balance ({entity.BalQty}) is less than required quantity ({sub.AccQty}).");
            }

        }


        public async Task<List<SubContractSCNPendingVM>> GetSubContractScnPendingList(string status)//Shankar
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<SubContractSCNPendingVM>("Sp_GetSubContractScnPendingList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> IsDocumentUploaded(int invId)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Sub-Contract SCN" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == invId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }

    }
}
