using AutoMapper;
using V.SMART.Shared.Utility_Constants;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml.Office;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.InventoryService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.LabourServices
{
    public class LabourSCNService:ILabourSCNService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManagerService;

        

        public LabourSCNService(
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

        //Companydetails
        public async Task<Companydetails> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();
        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        // 🔹 Customer
        public async Task<CustomerVM?> GetCustomerByIdAsync(int CustId)
           => await _commonService.GetCustomerByIdAsync(CustId);

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }
        // 🔹 ContactPerson
        public async Task<List<ContactPerson>> GetContactPersonsCustomerAsync(int CustId)
            => await _commonService.GetContactPersonsAsync(CustId);

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
          => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        // 🔹 Sores
        public async Task<IEnumerable<Store>> GetAllAddStoresAsync()
            => await _commonService.GetAllAddStoresAsync();

        // 🔹 Sores
        public async Task<List<Store>> GetAllIssueStoresAsync()
            => (await _commonService.GetAllIssueStoresAsync()).ToList();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
          => await _commonService.GetMappedStoreForFormAsync(formName);

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
           => await _commonService.GetItemByItemIdAsync(itemId);

        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        //Stock Manager
        public async Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId)
            => await _stockManagerService.GetStockForItemAsync(ItemId, StoreId);
        public async Task<bool> GetRejectionSelectionEnableAsync()
           => await _commonService.GetRejectionSelectionEnableAsync();
        public async Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync()
             => await _commonService.GetAllRejectionReasonAsync();



        //--------------------LabourSCNList Operation-------------------------------------
        public async Task<bool> IsSCNTransactionsMatchedAsync(int scnId, LabourSCNVM scnVms)
        {
            try
            {
                var scnSubIds = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == scnId)
                    .Select(x => x.SCNSubId)
                    .ToListAsync();

                bool hasScn = scnSubIds.Any();

                bool hasTransactions = false;

                if (hasScn)
                {
                    // Check Manufacturing Quotation references
                    hasTransactions = await _unitOfWork.LabourDcOutgoingSubs
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.RefSCNSubId.HasValue &&
                            scnSubIds.Contains(pqs.RefSCNSubId.Value));
                }

                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = scnVms?.LabourSCNSubVMs;
                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.AcceptQty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty ?? 0);

                    qtyMismatch = totalQty != totalBalQty;
                }

                // If either transactions exist OR quantity mismatch → return true
                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for ScnId: {scnId}");
                throw new InvalidOperationException("Failed to verify enquiry transactions.", ex);
            }
        }



        public async Task<(List<LabourSCNVM> Scns, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.LabourSCNs.GetQueryable()
                .AsNoTracking()
                .Include(j => j.LabourSCNSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                .Include(j => j.Customer)
                .Include(j => j.StoreAdd)
                .Include(j => j.StoreIssue)
                .AsQueryable();

                if (filters != null)
                {
                    foreach (var f in filters)
                    {
                        query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                    }
                }

                var total = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.SCNId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Use AutoMapper
                var vmList = _mapper.Map<List<LabourSCNVM>>(list);

                return (vmList, total);

            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, "[SearchWithDynamicFilterAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update SearchWithDynamicFilterAsync status. Please contact support.");
            }
        }

        public async Task<bool> HasAnyItemOrLabourSCNCancelAsync(int SCNId)
        {
            try
            {
                var isGRNCancelled = await _unitOfWork.LabourSCNs
                    .AnyAsync(q => q.SCNId == SCNId && q.SCNCancel == true);

                var isItemCancelled = await _unitOfWork.LabourSCNSubs
                    .AnyAsync(i => i.SCNId == SCNId && i.ItemCancel == true);

                return isGRNCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrLabourSCNCancelAsync for SCNId: {SCNId}");
                throw;
            }
        }

        public async Task<(bool CanDelete, string Message)> NeedTocheckRejection(int refGRNSubId, decimal rejQty)
        {
            var grnSub = await _unitOfWork.LabourGRNSubs.GetQueryable()
                                .Where(g => g.GRNSubId == refGRNSubId)
                                .FirstOrDefaultAsync();

            if (grnSub != null)
            {
                var poSub = await _unitOfWork.MfgPoSubs.GetQueryable()
                        .Where(p => p.PoSubId == grnSub.RefPoSubId)
                        .FirstOrDefaultAsync();

                if (poSub != null)
                {
                    var po = await _unitOfWork.MfgPos.GetQueryable()
                            .Where(p => p.PoId == poSub.PoId)
                            .FirstOrDefaultAsync();

                    if (po != null && po.isRejTrackReq)
                    {
                        
                        if (poSub.BalQty < rejQty)
                        {
                            return (false, "SCN deletion is not allowed because the rejected quantity is already linked to a GRN created against this Manufacturing Po Order.");
                        }
                    }
                }
            }

            // ✔ Allowed to delete
            return (true, string.Empty);
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteLabourSCNAsync(int SCNId, int screenCode, string refNo)
        {
            try
            {
                var LabSCN = await _unitOfWork.LabourSCNs
                              .GetQueryable()
                              .Include(e => e.LabourSCNSubs)
                              .Where(e => e.SCNId == SCNId).FirstOrDefaultAsync();

                if (LabSCN == null)
                    return (true, "Labour SCN can be safely deleted.");

                var grnSubIds = LabSCN.LabourSCNSubs
                    .Select(es => es.RefGRNSubId)
                    .ToList();

                bool hasPo = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefGRNSubId.HasValue &&
                        grnSubIds.Contains(qs.RefGRNSubId.Value));

                if (hasPo)
                    return (false, "Cannot delete this Labour SCN as a Labour Invoice exists.");

                if (LabSCN.SCNCancel || LabSCN.ShortClose)
                    return (false, "Cannot delete this Labour SCN as it is Cancelled or Short-Closed.");


                if (LabSCN.LabourSCNSubs.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Labour SCN as one or more SCN items are cancelled.");


                var SCNSubIds = await _unitOfWork.LabourSCNSubs
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
                    return (false, "Cannot delete Labour SCN. Some sub-items have already been transacted/issued.");

                return (true, "Labour SCN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ToCheckStockQtyIssued for SCNId: {SCNId}");
                throw new Exception("Error checking LabourSCN delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteLabourSCNByIdAsync(int SCNId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var scn = await _unitOfWork.LabourSCNs
                    .GetQueryable()
                    .Include(e => e.LabourSCNSubs)
                    .FirstOrDefaultAsync(e => e.SCNId == SCNId);

                if (scn == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in scn.LabourSCNSubs)
                {
                    if (sub.RefGRNSubId > 0)
                    {
                        await AdjustGRNSubBalanceAsync(sub.RefGRNSubId, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), 0, "SCN Deletion");
                    }

                    await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                    await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);
                }

                var deleted = await _unitOfWork.LabourSCNs.DeleteAsync(SCNId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Labour SCN",
                    action: $"Deleted SCN: {scn.SCNNo}",
                    additionalInfo: $"SCN Id: {scn.SCNId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete SCN: {SCNId}");
                throw;
            }
        }

        private async Task DeleteStockIssueAndTrackAsync(int ScnSubId, int itemId, int screenCode)
        {
            try
            {
                var issueIds = await _unitOfWork.StockIssues
                            .GetQueryable()
                            .Where(s => s.SubItemRefID == ScnSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                            .Select(s => s.IssueId)
                            .ToListAsync();

                foreach (var issueid in issueIds)
                {
                    if (issueid > 0)
                        await _stockManagerService.DeleteStockIssueAsync(issueid);

                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to DeleteStockIssueAndTrackAsync in Purchase SCN");
            }
        }

        private async Task DeleteStockAddAsync(int ScnSubId, int itemId, int screenCode)
        {
            try
            {
                var AddIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == ScnSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.AddId)
                .ToListAsync();

                foreach (var addId in AddIds)
                {
                    if (addId > 0)
                        await _stockManagerService.DeleteStockAddAsync(addId);

                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to DeleteStockIssueAndTrackAsync in Purchase SCN");
            }
        }

        private async Task AdjustGRNSubBalanceAsync(int? refGRNSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refGRNSubId.HasValue || refGRNSubId == 0) return;


                if (refGRNSubId > 0)
                {
                    var grnSub = await _unitOfWork.LabourGRNSubs.GetAsync(refGRNSubId.Value);
                    if (grnSub == null || grnSub.TransType=="Out") return;

                    if (oldQty > 0 && grnSub.TransType == "In")
                        grnSub.BalQty += oldQty;

                    if (newQty > grnSub.BalQty && grnSub.TransType == "In")
                        throw new InvalidOperationException($"{context}: Qty cannot exceed GRN BalQty.");

                    if (newQty > 0 && grnSub.TransType == "In")
                        grnSub.BalQty -= newQty;

                    await _unitOfWork.LabourGRNSubs.UpdateAsync(grnSub);
                    await _unitOfWork.SaveAsync();

                    // ✅ Calculate total BalQty for the parent PO
                    var totalBalQty = await _unitOfWork.LabourGRNSubs
                        .GetQueryable()
                        .Where(e => e.GRNId == grnSub.GRNId && e.TransType=="In")
                        .SumAsync(e => e.BalQty);

                    var grn = await _unitOfWork.LabourGRNs.GetAsync(grnSub.GRNId);
                    if (grn != null)
                    {
                        grn.DcTally = (totalBalQty == 0); // Tally only if all BalQty consumed
                        await _unitOfWork.LabourGRNs.UpdateAsync(grn);
                        await _unitOfWork.SaveAsync();
                    }
                }

            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPOBalance] Validation failed in {context}");
                throw; // rethrow so UI/business logic can show proper error
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust GRN balance. Please contact support.");
            }
        }

        //-------------------------------------------------------------------------------------------

        //-------------------------------SCN Upsert--------------------------------------------------------------------
        public async Task<string> GetLastSCNNumberAsync(string suffix)
        {
            try
            {
                var lastGrn = await _unitOfWork.LabourSCNs
                            .GetQueryable()
                            .Where(q => q.Suffix == suffix)
                            .OrderByDescending(q => q.SCNNo)
                            .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastGrn != null)
                {
                    var parts = lastGrn.SCNNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Labour SCN number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Labour number.");
            }
        }

        public async Task<LabourSCNVM> GetLabourSCNByIdAsync(int SCNId)
        {
            try
            {
                var entity = await _unitOfWork.LabourSCNs.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.LabourSCNSubs)
                    .Include(q => q.LabourSCNSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                    .Include(q => q.LabourSCNSubs).ThenInclude(s => s.LabourGRNSub.LabourGRN)
                    .Include(q => q.Customer)
                    .Include(q => q.StoreAdd)
                    .Include(q => q.StoreIssue)
                    .FirstOrDefaultAsync(q => q.SCNId == SCNId);

                var scnVM = _mapper.Map<LabourSCNVM?>(entity);

                var itemIds = scnVM.LabourSCNSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && scnVM.StoreIssId.HasValue)
                {
                    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, scnVM.StoreIssId ?? 0);

                    foreach (var sub in scnVM.LabourSCNSubVMs)
                    { 
                        if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId ?? 0, out var qty))
                            sub.StockQty = qty;
                        else
                            sub.StockQty = 0m;
                    }
                }

                return scnVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetLabourSCNByIdAsync({SCNId})");
                return null;
            }
        }

        public async Task<bool> GetLabourSCNByIdIsCancelAsync(int SCNId)
        {
            return await _unitOfWork.LabourSCNs
                .GetQueryable()
                .Where(e => e.SCNId == SCNId)
                .AnyAsync(e =>
                    e.SCNCancel == true ||
                    e.LabourSCNSubs.Any(s => s.ItemCancel == true)
                );
        }

        public async Task<int> GetPendingGRNCountAsync(int CustId)
        {
            return await _unitOfWork.LabourGRNs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(h =>
                            h.CustId == CustId &&
                            !h.DcTally &&
                            !h.DcCancel &&
                            h.LabourGRNSubs.Any(s =>
                                s.BalQty > 0 &&
                                !s.ItemCancel && s.TransType=="In"
                            )
                        )
                        .CountAsync();

        }

        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int CustId)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.MfgPoSubs.GetQueryable()
                                  join q in _unitOfWork.MfgPos.GetQueryable() on qs.PoId equals q.PoId
                                  where qs.ItemId == itemId && q.CustId == CustId
                                  orderby q.PoId descending
                                  select qs.UnitPrice)
                                    .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.MfgPoSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.PoSubId descending
                                      select qs.UnitPrice)
                                        .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId && isub.VendorId == CustId
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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for CustId: {CustId}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }

        public async Task<decimal> GetGRNItemBalQtyFromGRNSubId(int GRNSubId)
        {
            try
            {
               
                var sum = await _unitOfWork.LabourGRNSubs.GetQueryable()
                                .Where(e => e.GRNSubId == GRNSubId && e.TransType=="In")
                                .Select(e => (e.BalQty))
                                .SumAsync();

                return sum;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty in Labour SCN for LabourGRNSubId: {GRNSubId}");
                throw new InvalidOperationException("Failed to retrieve LabourSCN balance quantity.");
            }
        }

        public async Task<LabourSCNSubVM?> GetSCNSubItemDetailBySCNSubIdAsync(int SCNSubId)
        {
            try
            {
                return await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(q => q.SCNSubId == SCNSubId)
                    .Select(q => new LabourSCNSubVM
                    {
                        BalQty = q.BalQty,
                        AcceptQty = q.AcceptQty,
                        RejectQty = q.RejectQty,
                        ReworkQty = q.ReworkQty,
                        SCNBalQty = q.SCNBalQty,

                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Dc sub item detail for SCNSubId: {SCNSubId}");
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }

        //Item Cancel
        public async Task UpdateItemCancelAndAddorRevertAsync(LabourSCNSubVM subItem, int screenCode, string SCNNo, int AddStoreId, DateTime SCNDate)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.LabourSCNSubs.GetQueryable().Where
                                (x => x.SCNSubId == subItem.SCNSubId).FirstOrDefaultAsync();
                var existingSCN = await _unitOfWork.LabourSCNs.GetAsync(subItem.SCNId);

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with SCNSubId {subItem.SCNSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateGRNBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.CancelItemReason = subItem.CancelItemReason;

                await _unitOfWork.LabourSCNSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    await AdjustGRNSubBalanceAsync(subEntity.RefGRNSubId, (subEntity.AcceptQty + subEntity.RejectQty + subEntity.ReworkQty), 0, $"Labour SCN Item Cancel - {subItem.ItemCode}");

                    await DeleteStockIssueAndTrackAsync(subItem.SCNSubId, subItem.ItemId ?? 0, screenCode);
                    await DeleteStockAddAsync(subItem.SCNSubId, subItem.ItemId.Value, screenCode);
                }
                else
                {
                    await AdjustGRNSubBalanceAsync(subEntity.RefGRNSubId, 0, (subEntity.AcceptQty + subEntity.RejectQty + subEntity.ReworkQty), $"Labour SCN Revert Cancel - {subItem.ItemCode}");

                    await _stockManagerService.IssueOrUpdateStockAsync(subItem.ItemId??0, existingSCN.StoreIssId ?? 0, subItem.AcceptQty ?? 0,
                        subItem.UnitPrice ?? 0, subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate);

                    await _stockManagerService.AddOrUpdateStockAsync(subItem.ItemId ?? 0, AddStoreId, subItem.AcceptQty ?? 0,      subItem.UnitPrice ?? 0,
                        subItem.BatchNo, screenCode, subItem.SCNSubId, SCNNo, SCNDate, subItem.Remark);

                    if (subItem.RejectQty > 0)
                    {
                        await _stockManagerService.IssueOrUpdateStockAsync(subItem.ItemId ?? 0, existingSCN.StoreIssId ?? 0, subItem.RejectQty.Value,
                            subItem.UnitPrice ?? 0, subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                        await _stockManagerService.AddOrUpdateStockAsync(subItem.ItemId ?? 0, StoreIds.RejectionStore, (subItem.RejectQty.Value), subItem.UnitPrice ?? 0,
                            subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                    }

                    if (subItem.ReworkQty > 0)
                    {
                        await _stockManagerService.IssueOrUpdateStockAsync(subItem.ItemId ?? 0, existingSCN.StoreIssId ?? 0, subItem.ReworkQty.Value,
                            subItem.UnitPrice ?? 0, subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                        await _stockManagerService.AddOrUpdateStockAsync(subItem.ItemId ?? 0, StoreIds.ReworkStore, (subItem.ReworkQty.Value), subItem.UnitPrice ?? 0,
                            subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                    }
                }

                await UpdateSCNTallyStatusAsync(subItem.SCNId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {

                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdateItemCancelAndAddorRevertAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAndAddorRevertAsync for ItemCode {subItem.ItemCode}");
                throw new InvalidOperationException("Failed to update Item cancel/revert status. Please contact support.");
            }
        }

        public async Task ValidateBeforeRevertAsync(int scnSubId)
        {
            try
            {
                var sub = await _unitOfWork.LabourSCNSubs.GetAsync(scnSubId);

                if (sub == null)
                    throw new InvalidOperationException("Sales PO Item not found.");

                if (sub.RefGRNSubId > 0)
                    await ValidateGRNBalanceBeforeRevertAsync(sub);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "[ValidateBeforeRevertAsync]");
                throw new InvalidOperationException("Failed to validate item cancel/revert. Please contact support.");
            }
        }



        public async Task ValidateGRNBalanceBeforeRevertAsync(LabourSCNSub sub)
        {
            if (sub.RefGRNSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.LabourGRNSubs.GetAsync(sub.RefGRNSubId ?? 0);
            if (entity == null)
                throw new InvalidOperationException($"Labour GRN not found for RefGRNSubId: {sub.RefGRNSubId}");

            if (entity.BalQty < sub.AcceptQty && entity.TransType=="In")
            {
                throw new InvalidOperationException($"Cannot revert because GRN balance ({entity.BalQty}) is less than required quantity ({sub.AcceptQty}).");
            }

        }

        public async Task UpdateSCNTallyStatusAsync(int scnId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == scnId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var labourSCN = await _unitOfWork.LabourSCNs.GetAsync(scnId);

                if (labourSCN == null)
                    return;

                if (labourSCN.ShortClose || labourSCN.SCNCancel)
                    return;

                labourSCN.SCNTally = (totalBalQty == 0);

                await _unitOfWork.LabourSCNs.UpdateAsync(labourSCN);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateSCNTallyStatusAsync] Error updating SCN:- {scnId}");
                throw new InvalidOperationException("Failed to update Labour SCN Tally status. Please contact support.");
            }
        }

        public async Task<List<LabourSCNSubVM>> GetSCNSubBySCNIdAsync(int scnId)
        {
            try
            {
                var subs = await _unitOfWork.LabourSCNSubs
                                 .GetQueryable()
                                 .Where(s => s.SCNId == scnId)
                                 .OrderBy(s => s.SlNo)
                                 .ProjectTo<LabourSCNSubVM>(_mapper.ConfigurationProvider)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Purchase SCN items for SCNId: {scnId}");
                throw new InvalidOperationException("Failed to retrieve Labour SCN sub-items. Please try again.");
            }
        }

        public async Task<List<LabourSCNSub>> GetSCNSubDetailsBySCNIdAsync(int scnId)
        {
            try
            {
                var subs = await _unitOfWork.LabourSCNSubs
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
                await _logs.LogDeveloperError(ex, $"Error fetching Purchase SCN items for SCNId: {scnId}");
                throw new InvalidOperationException("Failed to retrieve Labour SCN sub-items. Please try again.");
            }
        }


        //SCN Cancel
        public async Task UpdatedCancelStatusAndAddOrRevertQty(LabourSCNVM scnVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingSCN = await _unitOfWork.LabourSCNs.GetAsync(scnVM.SCNId);
                if (existingSCN == null)
                    throw new InvalidOperationException("Labour SCN not found.");

                var subs = await _unitOfWork.LabourSCNSubs
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

                await _unitOfWork.LabourSCNs.UpdateAsync(existingSCN);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingSCN.SCNCancel)
                    {
                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGRNSubBalanceAsync(sub.RefGRNSubId ?? 0, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), 0, $"Labour SCN Cancelled - {existingSCN.SCNNo}");
                        }
                        await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);

                        await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);
                    }
                    else
                    {
                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGRNSubBalanceAsync(sub.RefGRNSubId ?? 0, 0, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), $"Labour SCN Reverted - {existingSCN.SCNNo}");
                        }
                        await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId,  existingSCN.StoreIssId ?? 0, sub.AcceptQty,
                            sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate);

                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, existingSCN.AddStoreId ?? 0, sub.AcceptQty, sub.UnitPrice,
                            sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, sub.Remark);
                       
                        
                        if (sub.RejectQty > 0)
                        {

                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingSCN.StoreIssId ?? 0, sub.RejectQty,
                                sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.RejectionStore, (sub.RejectQty), sub.UnitPrice,
                                sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }

                        if (sub.ReworkQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingSCN.StoreIssId ?? 0, sub.ReworkQty,
                                sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.ReworkStore, (sub.ReworkQty), sub.UnitPrice,
                                sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }
                    }
                }

                await transaction.CommitAsync();
                scnVM = await GetLabourSCNByIdAsync(scnVM.SCNId);
            }
            catch (InvalidOperationException ex)
            {
                scnVM = await GetLabourSCNByIdAsync(scnVM.SCNId);
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

        private async Task DeleteStockAddAsync(int SCNSubId, int itemId, int screenCode, string refNo)
        {
            var addId = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == SCNSubId && s.ItemId == itemId && s.ScreenCode == screenCode && s.RefNo == refNo)
                .Select(s => s.AddId)
                .FirstOrDefaultAsync();

            if (addId > 0)
                await _stockManagerService.DeleteStockAddAsync(addId);

            await _unitOfWork.SaveAsync();
        }


        public async Task DeleteAndResequenceAsync(LabourSCNSubVM subitem, LabourSCNVM scnVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.SCNSubId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.LabourSCNSubs.GetAsync(subitem.SCNSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    // Restore balance qty
                    if (entity.RefGRNSubId > 0)
                    {
                        await AdjustGRNSubBalanceAsync(subitem.RefGRNSubId, entity.AcceptQty, 0, "SCN Deletion");

                        await DeleteStockIssueAndTrackAsync(subitem.SCNSubId, subitem.ItemId ?? 0, screenCode);
                        await DeleteStockAddAsync(subitem.SCNSubId, subitem.ItemId ?? 0, screenCode);

                    }
                    
                    // Delete from DB
                    await _unitOfWork.LabourSCNSubs.DeleteAsync(entity);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Labour SCN",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"SCN No: {scnVM?.SCNNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    scnVM.LabourSCNSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == scnVM.SCNId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                await UpdateSCNTallyStatusAsync(scnVM.SCNId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetGRNDetailsByCustId(int CustId, int ScreenCode, int StoreId)
        {
            try
            {

                var grnData = await (
                            from g in _unitOfWork.LabourGRNs.GetQueryable().AsNoTracking()
                            join gs in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                                on g.GRNId equals gs.GRNId
                            join s in _unitOfWork.Stores.GetQueryable().AsNoTracking()
                                on g.AddStoreId equals s.StoreId
                            join i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                                on gs.ItemId equals i.ItemId
                            where g.CustId == CustId
                                    && !g.DcTally
                                    && !g.DcCancel
                                    && !gs.ItemCancel
                                    && gs.ItemId.HasValue &&gs.TransType=="In" && gs.BalQty>0 && !g.ShortClose
                            select new
                            {
                                gs.GRNSubId,
                                gs.GRNId,
                                g.GRNNo,
                                g.Suffix,
                                g.GRNDate,

                                ItemIdIN = gs.ItemId,

                                ItemCode = i.ItemCode,
                                ItemName = i.ItemName,
                                Specification = gs.ItemSpecification,
                                UOM = i.MeasureUnit,
                                HSN = i.HSNCode,
                                CategoryName = i.Category != null ? i.Category.CategoryName : null,

                                UnitConvert = i.UnitConvert,
                                AltRate = i.AltRate,

                                gs.Qty,
                                gs.BalQty,
                                gs.UnitPrice,

                                g.RefDcNo,
                                g.RefDcDate,

                                CostCenterId = gs.CostId == 0 ? (int?)null : gs.CostId,
                                ProjectNo = gs.CostCenter != null ? gs.CostCenter.ProjectNo : null,

                                Remarks = g.Remarks,
                                BatchNo = gs.BatchNo,
                                HeatNo = gs.HeatNo,
                                StoreName = s.StoreName,
                                Customer=g.Customer.CustName
                            }
                        ).ToListAsync();

                var itemIds = grnData.Select(x => x.ItemIdIN).Where(x => x.HasValue).Select(x => x.Value).Distinct().ToList();

                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, StoreId);


                var result = grnData.Select(r =>
                {
                    decimal stockQty = 0;

                    if (r.ItemIdIN.HasValue)
                    {
                        stockDict.TryGetValue(r.ItemIdIN ?? 0, out stockQty);
                    }

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["GRNSubId"] = r.GRNSubId,
                        ["GRNNo"] = $"{r.GRNNo}{r.Suffix}",
                        ["GRNDate"] = r.GRNDate.ToString("dd/MM/yyyy"),

                        ["ItemId"] = r.ItemIdIN,
                        ["ItemCode"] = r.ItemCode ?? "",
                        ["ItemName"] = r.ItemName ?? "",
                        ["Specification"] = r.Specification ?? "",
                        ["UOM"] = r.UOM ?? "",
                        ["HSNCode"] = r.HSN ?? "",
                        ["Category"] = r.CategoryName ?? "",

                        ["Qty"] = r.BalQty,
                        ["BalQty"] = r.BalQty,
                        ["UnitPrice"] = r.UnitPrice.GetValueOrDefault(),

                        ["Stock Qty"] = stockQty,  

                        ["RefDcNo"] = r.RefDcNo,
                        ["RefDcDate"] = r.RefDcDate?.ToString("dd/MM/yyyy"),

                        ["CostId"] = (int?)r.CostCenterId,
                        ["ProjectNo"] = r.ProjectNo ?? "",

                        ["Remarks"] = r.Remarks ?? "",

                        ["UnitConvert"] = (r.UnitConvert == null || r.UnitConvert == 0) ? 1 : r.UnitConvert,
                        ["AltRate"] = r.AltRate,
                        ["Store Name"] = r.StoreName,

                        ["BatchNo"] = r.BatchNo,
                        ["HeatNo"] = r.HeatNo,
                        ["Customer"] = r.Customer ?? string.Empty,
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching SCN details for CustId: {CustId}");
                throw new InvalidOperationException("Failed to retrieve SCN details. Please try again.");
            }
        }

        public async Task<LabourSCNVM> UpsertSCNAsync(LabourSCNVM labourscnVM, int screenCode)
        {
            if (labourscnVM == null)
                throw new ArgumentNullException(nameof(labourscnVM));

            var now = DateTime.Now;

            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                LabourSCN entity;

                if (labourscnVM.SCNId == 0)
                {
                    entity = _mapper.Map<LabourSCN>(labourscnVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.LabourSCNs.GetLastSCNNoAsync(entity.Suffix);

                    entity.SCNNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.NoOfItems = labourscnVM.LabourSCNSubVMs.Count();
                    entity.LabourSCNSubs = labourscnVM.LabourSCNSubVMs.Select(s => _mapper.Map<LabourSCNSub>(s)).ToList();

                    await SetSCNAuthorizationStatusAsync(entity, currentUser);

                    await _unitOfWork.LabourSCNs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    decimal altrate = 0, qtyConvert = 0;

                    foreach (var sub in entity.LabourSCNSubs)
                    {
                        if (sub.RefGRNSubId > 0)
                        {
                            await AdjustGRNSubBalanceAsync(sub.RefGRNSubId, 0, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), "Labour SCN Creation");
                        }

                        //======= Accecpt=============
                        if (sub.AcceptQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.StoreIssId ?? 0, sub.AcceptQty,
                            sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, entity.AddStoreId.GetValueOrDefault(), sub.QtyConvert.GetValueOrDefault(), sub.UnitPrice,
                                  sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.Remark, allowMultipleAdd: true);
                        }

                        // ========= Reject ===============
                        if (sub.RejectQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.StoreIssId ?? 0, sub.RejectQty,
                                sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.RejectionStore, (sub.RejectQty), sub.UnitPrice,
                                sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, null, allowMultipleAdd: true);
                        }

                        if (sub.ReworkQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.StoreIssId ?? 0, sub.ReworkQty,
                            sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, StoreIds.ReworkStore, (sub.ReworkQty), sub.UnitPrice,
                                                                        sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, null, allowMultipleAdd: true);
                        }
                    }

                    changes.AppendLine("Labour SCN Created.");
                }
                else
                {
                    entity = await _unitOfWork.LabourSCNs.GetQueryable()
                        .Include(q => q.LabourSCNSubs)
                        .FirstOrDefaultAsync(q => q.SCNId == labourscnVM.SCNId)
                        ?? throw new InvalidOperationException("Labour SCN found.");

                    var parentChanges = GetPropertyChanges(entity, labourscnVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(labourscnVM, entity);

                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.NoOfItems = entity.LabourSCNSubs.Count();

                    await SetSCNAuthorizationStatusAsync(entity, currentUser);

                    await HandleChildUpdatesAsync(entity, labourscnVM.LabourSCNSubVMs, changes, screenCode);

                    changes.AppendLine("Purchase SCN Updated.");
                }


                await _unitOfWork.SaveAsync();
                await UpdateSCNTallyStatusAsync(labourscnVM.SCNId);
                await transaction.CommitAsync();

                await LogChangesAsync(changes, labourscnVM.SCNId == 0 ? "Purchase SCN Created" : "Purchase SCN  Updated");

                var savedEntity = await _unitOfWork.LabourSCNs.GetQueryable()
                    .Include(q => q.LabourSCNSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.StoreAdd)
                    .Include(q => q.StoreIssue)
                    .FirstOrDefaultAsync(q => q.SCNId == entity.SCNId);

                return _mapper.Map<LabourSCNVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Labour SCN: {labourscnVM.SCNNo}");
                throw new InvalidOperationException("Failed to save Labour SCN. Please try again.");
            }
        }

        private async Task SetSCNAuthorizationStatusAsync(LabourSCN entity, string currentUser)
        {
            var SCNAuthorityExists = await _unitOfWork.UserAuthorities
                .AnyAsync(x => x.IsLabourSCN == true);

            if (!SCNAuthorityExists)
            {
                entity.Authorized = true;
                entity.ApprovedBy = currentUser;
                entity.ApprovalDate = DateTime.Now;
            }
            else
            {

                entity.Authorized = false;
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
            try
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
            catch (Exception ex)
            {

                _logs.LogDeveloperError(ex, $"Failed to GetPropertyChanges in Purchase GRN");
                return null;
            }
        }

        private async Task HandleChildUpdatesAsync(LabourSCN existingSCN, List<LabourSCNSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            try
            {
                var existingSubIds = existingSCN.LabourSCNSubs.Select(s => s.SCNSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.SCNSubId).ToHashSet();

                // DELETE removed children
                foreach (var sub in existingSCN.LabourSCNSubs.Where(s => !incomingSubIds.Contains(s.SCNSubId)).ToList())
                {

                    await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);

                    await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                    changes.AppendLine($"Child Deleted - SCNSubId: {sub.SCNSubId}, Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.LabourSCNSubs.DeleteAsync(sub.SCNSubId);
                    await _unitOfWork.SaveAsync();

                    if (sub.RefGRNSubId > 0)
                    {
                        await AdjustGRNSubBalanceAsync(sub.RefGRNSubId, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), 0, "SCN Deletion");
                    }

                }

                foreach (var subVM in incomingSubVMs)
                {
                    decimal.TryParse((subVM.AcceptQty.Value).ToString(), out decimal qty);

                    if (subVM.SCNSubId == 0)
                    {
                        var newSub = _mapper.Map<LabourSCNSub>(subVM);
                        newSub.SCNId = existingSCN.SCNId;
                        await _unitOfWork.LabourSCNSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.AcceptQty}");

                        if (subVM.RefGRNSubId > 0)
                        {
                            await AdjustGRNSubBalanceAsync(subVM.RefGRNSubId, 0, (subVM.AcceptQty.Value + subVM.RejectQty.Value + subVM.ReworkQty.Value), "SCN Creation");
                        }
                        //---------------Stock Add and Issue---------------------------------------------

                        if (subVM.AcceptQty.GetValueOrDefault() > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId ?? 0, subVM.AcceptQty ?? 0,
                                subVM.UnitPrice ?? 0, subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId ?? 0, existingSCN.AddStoreId.GetValueOrDefault(), subVM.QtyConvert.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(),
                                subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }

                        if (subVM.RejectQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId ?? 0, subVM.RejectQty ?? 0,
                                subVM.UnitPrice ?? 0, subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId ?? 0, StoreIds.RejectionStore, (subVM.RejectQty.Value), subVM.UnitPrice ?? 0,
                                                                        subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }

                        if (subVM.ReworkQty > 0)
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId ?? 0, existingSCN.StoreIssId.Value, subVM.ReworkQty.Value,
                                subVM.UnitPrice ?? 0, subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.ReworkStore, (subVM.ReworkQty.Value), subVM.UnitPrice ?? 0,
                                                                        subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }
                        //-----------------------------------------------------------------------------------
                    }
                    else
                    {
                        var existingSub = existingSCN.LabourSCNSubs.FirstOrDefault(s => s.SCNSubId == subVM.SCNSubId);
                        if (existingSub != null)
                        {
                            if (subVM.RefGRNSubId > 0)
                                await AdjustGRNSubBalanceAsync(subVM.RefGRNSubId, (existingSub.AcceptQty + existingSub.RejectQty + existingSub.ReworkQty),
                                    (subVM.AcceptQty.Value + subVM.RejectQty.Value + subVM.ReworkQty.Value), "SCN Update");

                            //---------------------------------------------Stock Add and Issue------------------

                            await DeleteStockIssueAndTrackAsync(subVM.SCNSubId, subVM.ItemId ?? 0, screenCode);

                            await DeleteStockAddAsync(subVM.SCNSubId, subVM.ItemId ?? 0, screenCode);

                            if (subVM.AcceptQty.GetValueOrDefault() > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId ?? 0, existingSCN.StoreIssId.Value, subVM.AcceptQty.Value,
                                    subVM.UnitPrice ?? 0, subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId ?? 0, existingSCN.AddStoreId.GetValueOrDefault(), subVM.QtyConvert.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(),
                                    subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                            }

                            if (subVM.RejectQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId ?? 0, subVM.RejectQty.Value,
                                    subVM.UnitPrice ?? 0, subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId ?? 0, StoreIds.RejectionStore, (subVM.RejectQty.Value), subVM.UnitPrice.Value,
                                    subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                            }

                            if (subVM.ReworkQty > 0)
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId ?? 0, existingSCN.StoreIssId ?? 0, subVM.ReworkQty.Value,
                                    subVM.UnitPrice??0, subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                                await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId ?? 0, StoreIds.ReworkStore, (subVM.ReworkQty.Value), subVM.UnitPrice.Value,
                                    subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                            }
                            //--------------------------------------------------------------------------------------------------------------------------------------------

                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                            await _unitOfWork.LabourSCNSubs.UpdateAsync(existingSub);
                            await _unitOfWork.SaveAsync();

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to upsert HandleChildUpdatesAsync SCN");
                throw new InvalidOperationException("Failed to save Labour SCN. Please try again.");
            }
        }

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            try
            {
                if (changes.Length == 0) return;

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase SCN",
                    action: action,
                    additionalInfo: changes.ToString()
                );
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to LogChangesAsync in Purchase SCN");
            }
        }


        public static class DynamicWhereBuilder
        {
            public static IQueryable<LabourSCN> ApplyFilter(IQueryable<LabourSCN> query, string field, object value)
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

                            return query.Where(x => (string.IsNullOrEmpty(part1) || x.SCNNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2)))
                            );
                        }

                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(val.ToString()));

                    case "GRNNo":
                        {
                            string part1 = val;
                            string part2 = string.Empty;

                            int slashIndex = val.IndexOf('/');
                            if (slashIndex > -1)
                            {
                                part1 = val[..slashIndex].Trim();
                                part2 = val[(slashIndex + 1)..].Trim();
                            }

                            return query.Where(x =>
                                x.LabourSCNSubs.Any(s =>
                                    s.LabourGRNSub != null &&
                                    s.LabourGRNSub.LabourGRN != null &&
                                    (string.IsNullOrEmpty(part1) ||
                                        s.LabourGRNSub.LabourGRN.GRNNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) ||
                                        (x.Suffix != null && x.Suffix.Contains(part2)))
                                )
                            );
                        }
                    case "RefDCNo":
                        {

                            string searchText = val?.ToString() ?? "";

                            return query.Where(x =>
                                x.LabourSCNSubs.Any(s =>
                                    s.LabourGRNSub != null &&
                                    s.LabourGRNSub.LabourGRN != null &&
                                    !string.IsNullOrEmpty(s.LabourGRNSub.LabourGRN.RefDcNo) &&
                                    (string.IsNullOrEmpty(searchText) ||
                                     s.LabourGRNSub.LabourGRN.RefDcNo.Contains(searchText))
                                )
                            );
                        }

                    case "ItemCode":
                        return query.Where(x => x.LabourSCNSubs.Any(s => s.Item.ItemCode.Contains(val.ToString())));


                    case "ItemName":
                        return query.Where(x => x.LabourSCNSubs.Any(s => s.Item.ItemName.Contains(val.ToString())));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val.ToString()));

                    case "FromDate":
                        return query.Where(x => x.CreatedDate >= DateTime.Parse(val.ToString()));

                    case "ToDate":
                        return query.Where(x => x.CreatedDate <= DateTime.Parse(val.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val.ToString());
                    
                }

                return query;
            }

            private static IQueryable<LabourSCN> ApplyStatusFilter(
              IQueryable<LabourSCN> query, string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.SCNTally && !x.ShortClose),

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

        public async Task<(bool CanDelete, string Message)> CanCancelAndRevokelabourSCNAsync(int scnId, int scnSubId)
        {
            try
            {
                bool hasTransaction = await _unitOfWork.LabourDcReturnCompTracks
                    .GetQueryable()
                    .AnyAsync(x => x.RefSCNSubId == scnSubId);

                return hasTransaction
                    ? (false, "Cannot cancel Labour SCN because transactions are already made.")
                    : (true, "Labour SCN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanCancelAndRevokelabourSCNAsync for SCNId: {scnId}");
                return (false, "Error while validating Labour SCN.");
            }
        }

        public async Task UpsertlabourSCNShortCloseAsync(LabourSCNVM LabourSCNVms)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingGRN = await _unitOfWork.LabourSCNs.GetAsync(LabourSCNVms.SCNId);
                if (existingGRN == null)
                    throw new InvalidOperationException("Sales Enquiry not found.");

                existingGRN.ShortClose = LabourSCNVms.ShortClose;

                await _unitOfWork.LabourSCNs.UpdateAsync(existingGRN);
                await _unitOfWork.SaveAsync();

                await UpdateLabourSCNTallyStatusAsync(LabourSCNVms.SCNId);

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
        public async Task UpdateLabourSCNTallyStatusAsync(int SCNId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == SCNId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var SCN = await _unitOfWork.LabourSCNs.GetAsync(SCNId);
                if (SCN == null)
                    return;

                if (SCN.ShortClose || SCN.SCNCancel)
                    return;

                SCN.SCNCancel = (totalBalQty == 0);

                await _unitOfWork.LabourSCNs.UpdateAsync(SCN);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateLabourGrnTallyStatusAsync] Error updating SCNID {SCNId}");

            }
        }

        public async Task<bool> GetParamemterLisAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(x => x.Display == "Rejection Parameters" && x.ScreenName == "Labour SCN")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetParamemterLisAsync()");
                return false;
            }
        }

        public async Task<List<LabourSCNStatusListVM>> GetLabourSCNStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<LabourSCNStatusListVM>("Sp_GetLabourSCNStatusList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> IsDocumentUploaded(int scnId)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Labour-SCN" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == scnId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }
    }
}
