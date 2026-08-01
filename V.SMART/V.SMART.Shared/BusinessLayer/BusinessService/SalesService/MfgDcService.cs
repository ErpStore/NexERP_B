using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Vml.Office;
using FastReport.Data;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.DcAutoRunning;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.SalesAndLabour.Export;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class MfgDcService : IMfgDcService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManagerService;

        public MfgDcService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper, IStockManagerService stock)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _stockManagerService = stock;
        }


        // 🔹 Customers
        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetCustomerByIdAsync(custId);
        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        // 🔹 Contacts
        public async Task<List<ContactPerson>> GetContactPersonsAsync(int custId)
            => await _commonService.GetContactPersonsAsync(custId);

        // 🔹 Consignee addresses
        public async Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId)
            => await _commonService.GetConsigneeAddressesAsync(custId);

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? custId = null)
            => await _commonService.SearchItemsAsync(searchText, custId);
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);
        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        // 🔹 Sores
        public async Task<List<Store>> GetAllIssueStoresAsync()
            => (await _commonService.GetAllIssueStoresAsync()).ToList();

        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
            => await _commonService.GetAllActiveStoresAsync();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
           => await _commonService.GetMappedStoreForFormAsync(formName);

        //Stock Manager
        public async Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId)
            => await _stockManagerService.GetStockForItemAsync(ItemId, StoreId);

        //screens

        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
                 => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        // 🔹 Dc operations

        public async Task<(List<MfgDcVM> mfgInvVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                   Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.MfgDcs.GetQueryable()
                .Include(j => j.Customer)
                .Include(j => j.MfgDcSubs).ThenInclude(ps => ps.MfgPoSub).ThenInclude(p=>p.MfgPo)
                .Include(j => j.MfgDcSubs).ThenInclude(s => s.Item)
                .AsQueryable().AsNoTracking().AsSplitQuery();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = MfgDcFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.DcId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<MfgDcVM>>(list);

            return (vmList, total);
        }

        public static class MfgDcFilterBuilder
        {
            public static IQueryable<MfgDc> ApplyFilter(
                IQueryable<MfgDc> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "DcNo":
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
                                (string.IsNullOrEmpty(part1) || x.DcNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }
                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(value.ToString()));
                    case "ItemCode":
                        return query.Where(x => x.MfgDcSubs.Any(s => s.Item.ItemCode.Contains(value.ToString())));
                    case "ItemName":
                        return query.Where(x => x.MfgDcSubs.Any(s => s.Item.ItemName.Contains(value.ToString())));
                    case "PoNo":
                        {
                            var input = val;
                            string part1 = input;
                            string part2 = "";

                            int slashIndex = input.IndexOf('/');

                            if (slashIndex > -1)
                            {
                                part1 = input.Substring(0, slashIndex).Trim();
                                part2 = input.Substring(slashIndex + 1).Trim();
                            }

                            return query.Where(x =>
                                x.MfgDcSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.MfgPoSub.MfgPo.PONo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.MfgPoSub.MfgPo.Suffix.Contains(part2))
                                ));
                        }
                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(value.ToString()));

                    case "FromDate":
                        return query.Where(x => x.CreatedDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.CreatedDate <= DateTime.Parse(value.ToString()));
                    case "Status":
                        return ApplyStatusFilter(query, val);

                }

                return query;
            }
            private static IQueryable<MfgDc> ApplyStatusFilter(
                IQueryable<MfgDc> query,
                string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.DcTally),

                        "Pending" =>
                            query.Where(x =>
                                !x.DcTally &&
                                !x.DcCancel &&
                                !x.DcShortClose),

                        "Cancelled" =>
                            query.Where(x => x.DcCancel),

                        "Short-Closed" =>
                            query.Where(x => x.DcShortClose),

                        _ => query
                    };
                }
                catch
                {
                    return query;
                }
            }

        }

        public async Task<string> GetPrefixFromDb()
        {
            try
            {
                var prefix = await _unitOfWork.MfgDcs.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.DcId)
                            .Select(q => q.Prefix)
                            .FirstOrDefaultAsync();

                return prefix ?? string.Empty;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetPrefixFromDb()");
                return string.Empty;
            }
        }

        public async Task<string> GetDcOutgoingNoAsync(string suffix)
        {
            try
            {
                string nextNumber = await _commonService.GeneratePreviewAutoDcRunningNoAsync("MFGDC", suffix);
                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating MfgDc number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate GetDcOutgoingNoAsync DcNumber.");
            }
        }

        public async Task<List<MfgDcSubVM>> GetDistinctRefPoByDcIdAsync(int dcId)
        {
            return await _unitOfWork.MfgDcSubs
                .GetQueryable()
                .Where(s => s.DcId == dcId)
                .GroupBy(s => new { s.RefPoNo, s.RefPoDate })
                .Select(g => new MfgDcSubVM
                {
                    RefPoNo = g.Key.RefPoNo,
                    RefPoDate = g.Key.RefPoDate
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<MfgDcVM>> GetAllDcDetailsAsync()
        {
            try
            {
                var entities = await _unitOfWork.MfgDcs.GetAllWithIncludeAsync(q => true,
                    q => q.Customer,
                    q => q.Consignee,
                    q => q.Store,
                    q => q.MfgDcSubs);
                return _mapper.Map<IEnumerable<MfgDcVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllDcDetailsAsync");
                return Enumerable.Empty<MfgDcVM>();
            }
        }

        public async Task<bool> DeleteMfgDcByDcIdAsync(int dcId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var dc = await _unitOfWork.MfgDcs
                    .GetQueryable()
                    .Include(e => e.MfgDcSubs)
                    .FirstOrDefaultAsync(e => e.DcId == dcId);

                if (dc == null)
                    return false;

                var changes = new StringBuilder();
                
                foreach (var sub in dc.MfgDcSubs)
                {
                    decimal newAcceptedQty, oldAcceptedQty = 0;
                    bool rejRetrack = false;

                    if (sub.RefPoSubId > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            oldAcceptedQty = sub.Qty - sub.RejectQty;

                        }
                        else
                        {
                            oldAcceptedQty = sub.Qty;
                        }
                        await AdjustPoSubBalanceAsync(sub.RefPoSubId, oldAcceptedQty, 0, "DC Deletion");


                        await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId.Value, screenCode);
                    }
                    if (dc.IsPurchaseReturn)
                    {
                        await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId.Value, screenCode);
                        await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, sub.Qty, 0, "Dc Creation");
                    }
                }

                //----------------AutoDcRunning-------------------------------------
                var runningRow = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.DcType == "MFGDC" &&
                                x.Suffix == dc.Suffix);
                if (runningRow != null)
                {
                    long oldDcNo = 0;
                    long.TryParse(dc.DcNo.ToString(), out oldDcNo);
                    if (runningRow.LastNumber == oldDcNo )
                    {
                        runningRow.LastNumber = (oldDcNo - 1);
                        await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                    }
                }
                //----------------------------------------------------------------------------

                var deleted = await _unitOfWork.MfgDcs.DeleteAsync(dcId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Delivery challan-Outgoing",
                    action: $"Deleted Dc: {dc.DcNo}",
                    additionalInfo: $"Po Id: {dc.DcId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Dc: {dcId}");
                throw;
            }
        }

        public async Task<MfgDcVM?> GetMfgDcByDcIdAsync(int dcId)
        {
            try
            {
                var entity = await _unitOfWork.MfgDcs.GetQueryable().AsNoTracking().AsSplitQuery()
                    .Include(q => q.MfgDcSubs)
                    .Include(q => q.MfgDcSubs).ThenInclude(s => s.Item)
                    .Include(q => q.MfgDcSubs).ThenInclude(s => s.CostCenter)
                    .Include(p => p.MfgDcSubs).ThenInclude(ps=>ps.MfgPoSub).ThenInclude(p=>p.MfgPo)
                    .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                    .Include(q => q.Vendor)
                    .Include(q => q.Store)
                    .FirstOrDefaultAsync(q => q.DcId == dcId);

                return _mapper.Map<MfgDcVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetMfgDcByDcIdAsync({dcId})");
                return null;
            }
        }

        public async Task<int> GetPendingPoCountAsync(int custId)
        {
            return await _unitOfWork.MfgPos
                .GetQueryable()
                .Where(e => e.CustId == custId && e.PoTally == false && e.MfgORLab && e.IsAuthorized == true)
                .CountAsync();
        }

        public async Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(e => e.PoSubId == poSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for MfgPoSubId: {poSubId}");
                throw new InvalidOperationException("Failed to retrieve PO balance quantity.");
            }
        }

        public async Task<MfgDcSubVM?> GetDcSubItemDetailByDcSubIdAsync(int dcSubId)
        {
            try
            {
                return await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .Where(q => q.DcSubId == dcSubId)
                    .Select(q => new MfgDcSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty,
                        RejectQty=q.RejectQty,
                        ReworkQty=q.ReworkQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Dc sub item detail for DcSubId: {dcSubId}");
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(MfgDcSubVM subItem, int screenCode)
        {

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            if (subItem == null)
                throw new ArgumentNullException(nameof(subItem), "Subitem cannot be null.");

            if (subItem.DcSubId == 0)
                throw new ArgumentException("Cannot update unsaved subitem.");

            try
            {
                var subEntity = await _unitOfWork.MfgDcSubs.GetQueryable().Where
                   (x => x.DcSubId == subItem.DcSubId).FirstOrDefaultAsync();

                var existingInv = await _unitOfWork.MfgDcs.GetQueryable().Where
                                (x => x.DcId == subItem.DcId).FirstOrDefaultAsync();
                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with DcSubId {subItem.DcSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateDcBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.MfgDcSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                int RefPoSubId = 0;
                decimal newAcceptedQty, oldAcceptedQty = 0;
                bool rejRetrack = false;

                int.TryParse(subItem.RefPoSubId.ToString(),out RefPoSubId);

                if (RefPoSubId>0)
                {
                    rejRetrack = await HasRejectReworkPoByPoSubidAsync(subEntity.RefPoSubId ?? 0);

                    if (subEntity.ItemCancel)
                    {
                        if (rejRetrack)
                        {
                            oldAcceptedQty = subEntity.Qty - subEntity.RejectQty;

                        }
                        else
                        {
                            oldAcceptedQty = subEntity.Qty;
                        }
                        await AdjustPoSubBalanceAsync(subEntity.RefPoSubId, oldAcceptedQty, 0, $"Dc Item Cancel - {subItem.ItemCode}");

                        await DeleteStockIssueAndTrackAsync(subEntity.DcSubId, subEntity.ItemId.Value, screenCode);
                    }
                    else
                    {
                        if (rejRetrack)
                        {

                            oldAcceptedQty = subEntity.Qty - subEntity.RejectQty;

                        }
                        else
                        {
                            oldAcceptedQty = subEntity.Qty;
                        }

                        await AdjustPoSubBalanceAsync(subEntity.RefPoSubId, 0, oldAcceptedQty, $"Dc Item Revert Cancel - {subItem.ItemCode}");

                        if (subEntity.RcSubIds != "" && subEntity.RcSubIds != null)
                        {
                            await IssueStockBySeqLogicAsync(subEntity, existingInv, screenCode);
                        }

                        await _stockManagerService.IssueOrUpdateStockAsync(subEntity.ItemId.Value, existingInv.IssueStoreId.Value, (subEntity.Qty), subEntity.UnitPrice,
                                      "", screenCode, subEntity.DcSubId, existingInv.DcNo, existingInv.DcDate, null, false);
                    }

                }
                else 
                {
                    if (subItem.ItemCancel)
                    {
                        await DeleteStockIssueAndTrackAsync(subEntity.DcSubId, subEntity.ItemId.Value, screenCode);


                        if (existingInv.IsPurchaseReturn)
                        {
                            await AdjustSCNSubBalanceAsync(subEntity.RefSCNSubId, subEntity.Qty, 0, "Dc Creation");
                        }
                    }
                    else
                    {
                        await _stockManagerService.IssueOrUpdateStockAsync(subEntity.ItemId.Value, existingInv.IssueStoreId.Value, (subEntity.Qty), subEntity.UnitPrice,
                                      "", screenCode, subEntity.DcSubId, existingInv.DcNo, existingInv.DcDate, null, false);

                        if (existingInv.IsPurchaseReturn)
                        {
                            await AdjustSCNSubBalanceAsync(subEntity.RefSCNSubId,0, subEntity.Qty, "Dc Creation");
                        }
                    }

                }

                await UpdateMfgDcTallyStatusAsync(subItem.DcId);

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
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAsync for ItemCode {subItem.ItemCode}");
                throw;
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(MfgDcVM dcVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingDc = await _unitOfWork.MfgDcs.GetAsync(dcVM.DcId);
                if (existingDc == null)
                    throw new InvalidOperationException("Manufacturing Dc not found.");

                var subs = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .Where(s => s.DcId == dcVM.DcId)
                    .ToListAsync();

                if (!dcVM.DcCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateDcBalanceBeforeRevertAsync(sub);
                    }
                }

                existingDc.DcCancel = dcVM.DcCancel;
                existingDc.CancelReason = dcVM.CancelReason;
                await _unitOfWork.MfgDcs.UpdateAsync(existingDc);
                await _unitOfWork.SaveAsync();

                int RefPoSubId = 0;

                foreach (var sub in subs)
                {
                    decimal newAcceptedQty, oldAcceptedQty = 0;
                    bool rejRetrack = false;

                    int.TryParse(sub.RefPoSubId.ToString(), out RefPoSubId);

                    if (existingDc.DcCancel)
                    {
                        if (RefPoSubId > 0)
                        {
                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);
                            if (rejRetrack)
                            {
                                oldAcceptedQty = sub.Qty - sub.RejectQty;

                            }
                            else
                            {
                                oldAcceptedQty = sub.Qty;
                            }

                            await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, oldAcceptedQty, 0, $"Manufacturing Dc Cancelled - {existingDc.DcNo}");

                            await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId.Value, screenCode);
                        }
                        else
                        {
                            if (existingDc.IsPurchaseReturn)
                            {
                                await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, sub.Qty, 0, "Dc Creation");
                            }
                            await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId.Value, screenCode);
                        }
                    }
                    else
                    {
                        if (RefPoSubId > 0)
                        {
                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                            if (rejRetrack)
                            {
                                oldAcceptedQty = sub.Qty - sub.RejectQty;

                            }
                            else
                            {
                                oldAcceptedQty = sub.Qty;
                            }

                            await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, 0, oldAcceptedQty, $"Manufacturing Dc Reverted - {existingDc.DcNo}");

                            if (sub.RcSubIds != "" && sub.RcSubIds != null)
                            {
                                await IssueStockBySeqLogicAsync(sub, existingDc, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId.Value, existingDc.IssueStoreId.Value, (sub.Qty), sub.UnitPrice,
                                "", screenCode, sub.DcSubId, existingDc.DcNo, existingDc.DcDate, null, false);
                            }

                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId.Value, existingDc.IssueStoreId.Value, (sub.Qty), sub.UnitPrice,
                               "", screenCode, sub.DcSubId, existingDc.DcNo, existingDc.DcDate, null, false);

                            if (existingDc.IsPurchaseReturn)
                            {
                                await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, 0, sub.Qty, "Dc Creation");
                            }
                        }
                    }
                }

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
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

        public async Task ValidateDcBalanceBeforeRevertAsync(MfgDcSub sub)
        {
            int RefPoSubId = 0;
            int.TryParse(sub.RefPoSubId.ToString(), out RefPoSubId);

            if (RefPoSubId > 0)
            {
                var entity = await _unitOfWork.MfgPoSubs.GetAsync(RefPoSubId);
                if (entity == null)
                    throw new InvalidOperationException($"Po not found for RefPoSubId: {sub.RefPoSubId}");

                if (entity.BalQty < sub.Qty)
                {
                    throw new InvalidOperationException(
                        $"Cannot revert because PO balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                    );
                }

            }


        }

        public async Task<List<MfgDcSubVM>> GetDcSubByDcIdAsync(int dcId)
        {
            try
            {
                var subs = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.DcId == dcId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<MfgDcSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Manufacturing DC items for DcId: {dcId}");
                throw new InvalidOperationException("Failed to retrieve Manufacturing DC sub-items. Please try again.");
            }
        }

        public async Task<bool> IsDuplicateDcNoAsync(string DcNo, string suffix, int? currentDcId = null, int? CustId = null)
        {
            if (string.IsNullOrWhiteSpace(DcNo))
                return false;

            try
            {
                return await _unitOfWork.MfgDcs
                    .GetQueryable()
                    .AnyAsync(x => x.DcNo == DcNo
                                && x.Suffix == suffix
                                && x.CustId == CustId
                                && (currentDcId == null || x.DcId != currentDcId));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in IsDuplicateDcNoAsync for Dc NO: {DcNo}");
                throw new InvalidOperationException("Failed to check duplicate mfgDc.");
            }
        }

        
        public async Task<MfgDcVM> UpsertDCAsync(MfgDcVM mfgDcVM, int screenCode)
        {
            if (mfgDcVM == null)
                throw new ArgumentNullException(nameof(mfgDcVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MfgDc entity;

                if (mfgDcVM.DcId == 0)
                {
                    entity = _mapper.Map<MfgDc>(mfgDcVM);

                    // entity.DcNo = await _unitOfWork.MfgDcs.GetLastDcNoAsync(entity.Suffix);

                    //--------AutoDcrunning
                    if (mfgDcVM.IsManualDcNo ?? true)
                    {
                        var runningRow = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.DcType == "MFGDC" &&
                                x.Suffix == entity.Suffix);

                        if (runningRow == null)
                        {
                            var dcrunn = new DcRunningNumber
                            {
                                LastNumber = Convert.ToInt64(entity.DcNo),
                                DcType = "MFGDC",
                                Suffix = entity.Suffix
                            };

                            await _unitOfWork.DcRunningNumbers.CreateAsync(dcrunn);
                        }
                        else
                        {
                            runningRow.LastNumber = Convert.ToInt64(entity.DcNo);
                            await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                        }

                        await _unitOfWork.SaveAsync();
                    }
                    else
                    {
                        entity.DcNo = await _commonService.GenerateAutoRunningNoAsync("MFGDC", entity.Suffix);
                    }
                    //-----------------------------------------------------------------------------------------

                    bool check = await IsDuplicateDcNoAsync(entity.DcNo, entity.Suffix, entity.DcId, entity.CustId);
                    if (check)
                    {
                        throw new Exception("Duplicate DC Number found.");
                    }


                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.MfgDcSubs = mfgDcVM.MfgDcSubVMs.Select(s => _mapper.Map<MfgDcSub>(s)).ToList();

                    await _unitOfWork.MfgDcs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    int RefPoSubId = 0;

                    foreach (var subVM in entity.MfgDcSubs)
                    {
                        int.TryParse(subVM.RefPoSubId.ToString(), out RefPoSubId);

                        if (RefPoSubId > 0)
                        {
                            await AdjustPoSubBalanceAsync(subVM.RefPoSubId, 0, subVM.Qty, "Dc Creation");

                            if (subVM.RcSubIds != "" && subVM.RcSubIds != null)
                            {
                                await IssueStockBySeqLogicAsync(subVM, entity, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, entity.IssueStoreId.Value, (subVM.Qty), subVM.UnitPrice,
                                       "", screenCode, subVM.DcSubId, entity.DcNo, entity.DcDate, null, false);
                            }
                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, entity.IssueStoreId.Value, (subVM.Qty), subVM.UnitPrice,
                                       "", screenCode, subVM.DcSubId, entity.DcNo, entity.DcDate, null, false);
                            if (mfgDcVM.IsPurchaseReturn)
                            {
                                await AdjustSCNSubBalanceAsync(subVM.RefSCNSubId, 0, subVM.Qty, "Dc Creation");
                            }
                        }

                    }

                    changes.AppendLine("Manufacturing DC Created.");
                }
                else
                {
                    entity = await _unitOfWork.MfgDcs.GetQueryable()
                        .Include(q => q.MfgDcSubs)
                        .FirstOrDefaultAsync(q => q.DcId == mfgDcVM.DcId)
                        ?? throw new InvalidOperationException("DC not found.");

                    var parentChanges = GetPropertyChanges(entity, mfgDcVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(mfgDcVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, mfgDcVM.MfgDcSubVMs, changes, screenCode);

                    changes.AppendLine("Manufacturing DC Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateMfgDcTallyStatusAsync(mfgDcVM.DcId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, mfgDcVM.DcId == 0 ? "Manufacturing Dc Created" : "Manufacturing DC  Updated");

                var savedEntity = await _unitOfWork.MfgDcs.GetQueryable()
                    .Include(q => q.MfgDcSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.Store)
                    .FirstOrDefaultAsync(q => q.DcId == entity.DcId);

                return _mapper.Map<MfgDcVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex,$"Failed to upsert Manufacturing DC: {mfgDcVM.DcNo}");
                throw new InvalidOperationException(" Failed to save Manufacturing DC. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(MfgDc existingDc, List<MfgDcSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingDc.MfgDcSubs.Select(s => s.DcSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.DcSubId).ToHashSet();

            decimal newAcceptedQty, oldAcceptedQty = 0;
            bool rejRetrack = false;

            // DELETE removed children
            foreach (var sub in existingDc.MfgDcSubs.Where(s => !incomingSubIds.Contains(s.DcSubId)).ToList())
            {
                newAcceptedQty = 0; 
                oldAcceptedQty = 0;
                rejRetrack = false;


                changes.AppendLine($"Child Deleted - DcSubId: {sub.DcSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.MfgDcSubs.DeleteAsync(sub.DcSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefPoSubId > 0)
                {
                    rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                    if (rejRetrack)
                    {
                        oldAcceptedQty = sub.Qty - sub.RejectQty;

                    }
                    else
                    {
                        oldAcceptedQty = sub.Qty;
                    }

                    await AdjustPoSubBalanceAsync(sub.RefPoSubId, oldAcceptedQty, 0, "Dc Deletion");

                    await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId.Value, screenCode);

                }
                else
                {
                    if (existingDc.IsPurchaseReturn)
                    {
                        await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, sub.Qty, 0,  "Dc Creation");
                    }
                    await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId.Value, screenCode);
                }

            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                newAcceptedQty = 0;
                oldAcceptedQty = 0;
                rejRetrack = false;

                if (subVM.DcSubId == 0)
                {
                    var newSub = _mapper.Map<MfgDcSub>(subVM);
                    newSub.DcId = existingDc.DcId;
                    await _unitOfWork.MfgDcSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    if (subVM.RefPoSubId > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            newAcceptedQty = subVM.Qty - subVM.RejectQty??0;

                        }
                        else
                        {
                            newAcceptedQty = subVM.Qty??0;
                        }

                        await AdjustPoSubBalanceAsync(subVM.RefPoSubId, 0, newAcceptedQty, "Dc Creation");

                        if (subVM.RcSubIds != "" && subVM.RcSubIds != null)
                        {
                            await IssueStockBySeqLogicAsync(newSub, existingDc, screenCode);
                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingDc.IssueStoreId.Value, (subVM.Qty ?? 0), subVM.UnitPrice.Value,
                                "", screenCode, newSub.DcSubId, existingDc.DcNo, existingDc.DcDate, null, false);
                        }
                    }
                    else
                    {
                        await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingDc.IssueStoreId.Value, (subVM.Qty ?? 0), subVM.UnitPrice.Value,
                            "", screenCode, subVM.DcSubId, existingDc.DcNo, existingDc.DcDate, null, false);

                        if (existingDc.IsPurchaseReturn)
                        {
                            await AdjustSCNSubBalanceAsync(subVM.RefSCNSubId, 0, subVM.Qty.Value, "Dc Creation");
                        }
                    }

                }
                else
                {
                    var existingSub = existingDc.MfgDcSubs.FirstOrDefault(s => s.DcSubId == subVM.DcSubId && !s.ItemCancel);
                    if (existingSub != null)
                    {
                        if (subVM.RefPoSubId > 0)
                        {
                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId ?? 0);

                            if (rejRetrack)
                            {
                                newAcceptedQty = subVM.Qty - subVM.RejectQty ?? 0;
                                oldAcceptedQty = existingSub.Qty - existingSub.RejectQty;

                            }
                            else
                            {
                                newAcceptedQty = subVM.Qty ?? 0;
                                oldAcceptedQty = existingSub.Qty;
                            }

                            await AdjustPoSubBalanceAsync(subVM.RefPoSubId, oldAcceptedQty, newAcceptedQty, "Dc Update");

                            if (subVM.RcSubIds != "" && subVM.RcSubIds != null)
                            {
                                existingSub.Qty = subVM.Qty ?? existingSub.Qty;
                                await IssueStockBySeqLogicAsync(existingSub, existingDc, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingDc.IssueStoreId.Value, (subVM.Qty ?? 0), subVM.UnitPrice.Value,
                                      "", screenCode, subVM.DcSubId, existingDc.DcNo, existingDc.DcDate, null, false);
                            }
                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingDc.IssueStoreId.Value, (subVM.Qty ?? 0), subVM.UnitPrice.Value,
                                      "", screenCode, subVM.DcSubId, existingDc.DcNo, existingDc.DcDate, null, false);

                            if (existingDc.IsPurchaseReturn)
                            {
                                await AdjustSCNSubBalanceAsync(subVM.RefSCNSubId, existingSub.Qty, subVM.Qty.Value, "Dc Creation");
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

        private async Task AdjustPoSubBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refPoSubId.HasValue || refPoSubId == 0) return;

                var isOpenPo = await _unitOfWork.MfgPoSubs
                                .GetQueryable()
                                .Where(s => s.PoSubId == refPoSubId)
                                .Join(_unitOfWork.MfgPos.GetQueryable(),
                                        sub => sub.PoId,
                                        po => po.PoId,
                                        (sub, po) => po.IsOpenPO)
                                .FirstOrDefaultAsync();

                if (!isOpenPo)
                {
                    var PoSub = await _unitOfWork.MfgPoSubs.GetAsync(refPoSubId.Value);
                    if (PoSub == null) return;

                    if (oldQty > 0)
                        PoSub.BalQty += oldQty;

                    if (newQty > PoSub.BalQty)
                        throw new InvalidOperationException($"{context}: Qty cannot exceed PO BalQty.");

                    if (newQty > 0)
                        PoSub.BalQty -= newQty;

                    await _unitOfWork.MfgPoSubs.UpdateAsync(PoSub);
                    await _unitOfWork.SaveAsync();

                    var totalBalQty = await _unitOfWork.MfgPoSubs
                        .GetQueryable()
                        .Where(e => e.PoId == PoSub.PoId && !e.ItemCancel)
                        .SumAsync(e => e.BalQty);

                    var po = await _unitOfWork.MfgPos.GetAsync(PoSub.PoId);
                    if (po != null)
                    {
                        po.PoTally = (totalBalQty == 0);
                        await _unitOfWork.MfgPos.UpdateAsync(po);
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
                throw new InvalidOperationException("Failed to adjust Po balance. Please contact support.");
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

        public async Task UpdateMfgDcTallyStatusAsync(int DcId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .Where(x => x.DcId == DcId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var dcs = await _unitOfWork.MfgDcs.GetAsync(DcId);
                if (dcs == null)
                    return;

                dcs.DcTally = (totalBalQty == 0);

                await _unitOfWork.MfgDcs.UpdateAsync(dcs);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateMfgDcTallyStatusAsync] Error updating DcId {DcId}");
                throw new InvalidOperationException("Failed to update Mfg Dc Tally status. Please contact support.");
            }
        }

        public async Task DeleteAndResequenceAsync(MfgDcSubVM subitem, MfgDcVM dcVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.DcSubId > 0)
                {
                    var entity = await _unitOfWork.MfgDcSubs.GetAsync(subitem.DcSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    decimal newAcceptedQty, oldAcceptedQty = 0;
                    bool rejRetrack = false;

                    // Restore balance qty
                    if (entity.RefPoSubId > 0)
                    {
                        if (rejRetrack)
                        {
                            oldAcceptedQty = subitem.Qty - subitem.RejectQty??0;

                        }
                        else
                        {
                            oldAcceptedQty = subitem.Qty??0;
                        }

                        await AdjustPoSubBalanceAsync(subitem.RefPoSubId, oldAcceptedQty, 0, "Dc Deletion");
                    }

                    await DeleteStockIssueAndTrackAsync(subitem.DcSubId, subitem.ItemId.Value, screenCode);

                    await _unitOfWork.MfgDcSubs.DeleteAsync(entity.DcSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Mfg Dc",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Dc No: {dcVM?.DcNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    dcVM.MfgDcSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .Where(x => x.DcId == dcVM.DcId)
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

        public async Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId, int storeIssId)
        {
            try
            {
                var poLines = await (from p in _unitOfWork.MfgPos.GetQueryable()
                                     join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                                         on p.PoId equals ps.PoId
                                     where p.CustId == custId
                                           && !p.PoTally && p.MfgORLab
                                           && !p.PoCancl && ps.BalQty > 0 && !ps.ItemCancel && p.IsAuthorized && !p.ShortClose
                                     select new
                                     {
                                         ps.PoId,
                                         ps.PoSubId,
                                         p.IsOpenPO,
                                         p.PONo,
                                         p.Suffix,
                                         p.PODate,
                                         ps.LineNo,
                                         ps.ItemId,
                                         ps.Item.ItemCode,
                                         ps.Item.ItemName,
                                         ps.Item.MeasureUnit,
                                         ps.Item.HSNCode,
                                         ps.Qty,
                                         ps.BalQty,
                                         ps.UnitPrice,
                                         ps.DueDate,
                                         CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                                         ps.CostCenter.ProjectNo,
                                         p.MainRemark,
                                         p.Customer
                                     }).ToListAsync();
                if (!poLines.Any())
                    return new();

                var poIds = poLines.Select(x => x.PoId).Distinct().ToList();
                var itemIds = poLines.Select(x => x.ItemId).Distinct().ToList();

                var rcSubs = await _unitOfWork.RouteCardSubs.GetQueryable()
                            .Where(rcs =>
                                rcs.RouteCard != null && rcs.IsFinalProcess &&
                                poIds.Contains(rcs.RouteCard.RefPoId.Value))
                            .Select(rcs => new
                            {
                                rcs.RCSubId,
                                rcs.ItemIdIn,
                                rcs.RouteCard.RefPoId
                            })
                            .ToListAsync();

                var rcSubIds = rcSubs.Select(x => x.RCSubId).ToList();

                var rcStockDict = await _unitOfWork.StockAdds.GetQueryable()
                                .Where(s =>
                                    s.StoreId == storeIssId &&
                                    s.RcSubID.HasValue &&
                                    rcSubIds.Contains(s.RcSubID.Value))
                                .GroupBy(s => s.ItemId)
                                .Select(g => new
                                {
                                    ItemId = g.Key,
                                    BalQty = g.Sum(x => x.BalQty),
                                })
                                .Where(x => x.BalQty > 0)
                                .ToDictionaryAsync(x => x.ItemId, x => x.BalQty);



                // var itemStock = await _stockManagerService.GetStockForItemsAsync(itemIds, storeIssId);
                var itemStock= await _unitOfWork.StockAdds.GetQueryable()
                                .Where(s =>
                                    s.StoreId == storeIssId && (s.RcSubID==null|| s.RcSubID ==0) &&
                                    itemIds.Contains(s.ItemId)) 
                                .GroupBy(s => s.ItemId)
                                .Select(g => new
                                {
                                    ItemId = g.Key,
                                    BalQty = g.Sum(x => x.BalQty),
                                })
                                .Where(x => x.BalQty > 0)
                                .ToDictionaryAsync(x => x.ItemId, x => x.BalQty);



                var data = new List<Dictionary<string, object>>();
                decimal stockQty = 0;
                string rcsubId = "";

                foreach (var po in poLines)
                {
                    stockQty = 0;
                    bool hasRouteCard = rcSubs.Any(x => x.RefPoId == po.PoId);
                    if (hasRouteCard)
                    {
                        rcStockDict.TryGetValue(po.ItemId, out stockQty);

                    }
                    else
                    {
                        itemStock.TryGetValue(po.ItemId, out stockQty);
                    }
                    data.Add(new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["PoSubId"] = po.PoSubId,
                        ["IsOpenPo"] = po.IsOpenPO ? "Yes" : "No",
                        ["PoNo"] = po.PONo,
                        ["Suffix"] = po.Suffix,
                        ["PoDate"] = po.PODate,
                        ["LineId"] = po.LineNo,
                        ["ItemId"] = po.ItemId,
                        ["ItemCode"] = po.ItemCode ?? string.Empty,
                        ["ItemName"] = po.ItemName ?? string.Empty,
                        ["UOM"] = po.MeasureUnit ?? string.Empty,
                        ["HSNCode"] = po.HSNCode ?? string.Empty,
                        ["Qty"] = po.BalQty,
                        ["BalQty"] = po.BalQty,
                        ["UnitPrice"] = po.UnitPrice,
                        ["PoDuedate"] = po.DueDate,
                        ["CostCenterId"] = po.CostCenterId,
                        ["ProjectNo"] = po.ProjectNo ?? string.Empty,
                        ["Remark"] = po.MainRemark ?? string.Empty,
                        ["StockQty"] = stockQty,
                        ["RcSubId"] = hasRouteCard ? string.Join(",", rcSubIds) : "",
                        ["Customer"] = po.Customer.CustName ?? string.Empty,
                    });
                }
                return data;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PO details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.");
            }
        }

        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int custId)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.MfgPoSubs.GetQueryable()
                                  join q in _unitOfWork.MfgPos.GetQueryable() on qs.PoId equals q.PoId
                                  where qs.ItemId == itemId && q.CustId == custId
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
                                      where isub.ItemId == itemId && isub.CustomerId == custId
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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for CustId: {custId}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }


        public async Task<bool> IsOpenPoAsync(int poSubId)
        {
            return await _unitOfWork.MfgPoSubs
                .GetQueryable()
                .Where(s => s.PoSubId == poSubId)
                .Join(_unitOfWork.MfgPos.GetQueryable(),
                      sub => sub.PoId,
                      po => po.PoId,
                      (sub, po) => po.IsOpenPO)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetAvailableStockByItemIdAndRcAndScreenAsync(int itemId, int? storeId, int rcSubId)
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
                await _logs.LogDeveloperError(ex, $"Error in GetAvailableStockByItemIdAsync | ItemId={itemId}, StoreId={storeId}, rcSubId={rcSubId}");
                throw;
            }
        }

        private async Task IssueStockBySeqLogicAsync(MfgDcSub sub, MfgDc parent, int screenCode)
        {
            try
            {
                decimal remainingQty = sub.Qty;
                if (remainingQty <= 0)
                    return;


                var existingIssues = await _unitOfWork.StockIssues
                    .GetQueryable()
                    .Where(x =>
                        x.SubItemRefID == sub.DcSubId &&
                        x.ScreenCode == screenCode)
                    .ToListAsync();

                foreach (var issue in existingIssues)
                    await _stockManagerService.DeleteStockIssueAsync(issue.IssueId);



                var ids = sub.RcSubIds
                        .Replace("'", "")
                        .Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var rcSubId in ids)
                {
                    if (remainingQty <= 0)
                        break;

                    int.TryParse(rcSubId.ToString(), out int rsubid);

                    var available = await GetAvailableStockByItemIdAndRcAndScreenAsync(
                        sub.ItemId.Value,
                        parent.IssueStoreId,
                        rsubid);

                    if (available <= 0)
                        continue;

                    var issueQty = Math.Min(available, remainingQty);

                    await _stockManagerService.IssueOrUpdateStockAsync(
                        sub.ItemId.Value,
                        parent.IssueStoreId.Value,
                        issueQty,
                        sub.UnitPrice,
                        null,
                        screenCode,
                        sub.DcSubId,
                        parent.DcNo,
                        parent.DcDate,
                        rsubid,
                        allowMultipleIssue: true);

                    remainingQty -= issueQty;
                }

                if (remainingQty > 0)
                    throw new InvalidOperationException(
                        $"Insufficient RC stock after adjustment. Remaining Qty: {remainingQty}");

            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Error in IssueStockBySeqLogicAsync");
            }
        }

        private async Task DeleteStockIssueAndTrackAsync(int InvSubId, int itemId, int screenCode)
        {
            try
            {
                var issueIds = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(s => s.SubItemRefID == InvSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
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

        public async Task<bool> HasAnyItemOrDcCancelAsync(int DcId)
        {
            try
            {
                var isInvCancelled = await _unitOfWork.MfgDcs
                    .AnyAsync(q => q.DcId == DcId && q.DcCancel == true);

                var isItemCancelled = await _unitOfWork.MfgDcSubs
                    .AnyAsync(i => i.DcId == DcId && i.ItemCancel == true);

                return isInvCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrInvoiceCancelAsync for DcId: {DcId}");
                throw;
            }
        }

        public async Task<List<MfgDcSub>> GetDcDetailsSubByDcIdAsync(int dcId)
        {
            try
            {
                var subs = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.DcId == dcId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<MfgDcSub>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Manufacturing DC items for DcId: {dcId}");
                throw new InvalidOperationException("Failed to retrieve Manufacturing DC sub-items. Please try again.");
            }
        }


        //------Short Close----------
        public async Task<MfgDcVM> UpdateMfgDcShortCloseAsync(MfgDcVM mfgDcVM)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MfgDc entity;

                entity = await _unitOfWork.MfgDcs
                             .GetQueryable()
                             .Include(e => e.MfgDcSubs)
                             .FirstOrDefaultAsync(e => e.DcId == mfgDcVM.DcId)
                             ?? throw new InvalidOperationException("Manufacturing Dc not found.");

                _mapper.Map(mfgDcVM, entity);

                var parentChanges = GetPropertyChanges(entity, mfgDcVM);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                _mapper.Map(mfgDcVM, entity);
                entity.ModifiedBy = currentUser;
                entity.ModifiedDate = now;

                await _unitOfWork.MfgDcs.UpdateAsync(entity);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
                await LogChangesAsync(changes, mfgDcVM.DcId == 0 ? "Short Close Manufacturing Dc Created" : "ReOpen the Manufacturing Dc");

                // Return updated entity
                var savedEntity = await _unitOfWork.MfgDcs
                    .GetQueryable()
                    .Include(e => e.MfgDcSubs).ThenInclude(s => s.Item)
                    .Include(e => e.MfgDcSubs).ThenInclude(s => s.CostCenter)
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.DcId == entity.DcId);

                return _mapper.Map<MfgDcVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpdateMfgInvShortCloseAsync for DcId: {mfgDcVM.DcId}");
                throw;
            }

        }



        //-----------Update rejection-------------\\

        public async Task<MfgDcVM> UpdateMfgDCRejectRework(MfgDcVM mfgDcVM)
        {
            try
            {
                if (mfgDcVM == null)
                    throw new ArgumentNullException(nameof(mfgDcVM));

               
                var dcEntity = await _unitOfWork.MfgDcs
                    .GetQueryable()
                    .Include(d => d.MfgDcSubs)
                    .FirstOrDefaultAsync(d => d.DcId == mfgDcVM.DcId);
                
                if (dcEntity == null)
                    throw new Exception("DC not found");

                dcEntity.RejRemark = mfgDcVM.RejRemark;
                dcEntity.ReduceInvAsPerRej=mfgDcVM.ReduceInvAsPerRej;
                
                foreach (var vmSub in mfgDcVM.MfgDcSubVMs)
                {
                    var dcSubEntity = dcEntity.MfgDcSubs
                        .FirstOrDefault(s => s.DcSubId == vmSub.DcSubId);

                    if (dcSubEntity == null)
                        continue;

               
                    // 4️⃣ PO Balance adjustment
                    if (dcSubEntity.RefPoSubId > 0)
                    {
                        bool hasReject = await HasRejectReworkPoByPoSubidAsync(dcSubEntity.RefPoSubId.Value);

                        if (hasReject)
                        {
                            await AdjustPoSubBalanceAsync(dcSubEntity.RefPoSubId.Value, vmSub.RejectQty, dcSubEntity.RejectQty,"MfgDc Reject/Rework");
                        }
                    }

                    dcSubEntity.BalQty = vmSub.BalQty;
                    dcSubEntity.RejectQty = vmSub.RejectQty;
                    dcSubEntity.ReworkQty = vmSub.ReworkQty;
                    dcSubEntity.RejRowRemark = vmSub.RejRowRemark;
                    await _unitOfWork.MfgDcSubs.UpdateAsync(dcSubEntity);
                }


                await _unitOfWork.SaveAsync();

                return mfgDcVM;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> HasRejectReworkPoByPoSubidAsync(int PosubId)
        {
            try
            {
                return await (from poSub in _unitOfWork.MfgPoSubs.GetQueryable()
                              join po in _unitOfWork.MfgPos.GetQueryable()
                                  on poSub.PoId equals po.PoId
                              where poSub.PoSubId == PosubId
                                    && po.isRejTrackReq == true
                              select poSub.PoSubId
                        ).AnyAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasRejectReworkPoByPoSubidAsync");
                return false;
            }
        }

        public async Task<bool> CanMfgDcOutgoingTrasByDcsubIdAsync(List<int> dcSubIds)
        {
            try
            {
                if (dcSubIds == null || dcSubIds.Count == 0)
                    return false;

                return await (
                        from sub in _unitOfWork.MfgInvSubs.GetQueryable()
                        join inv in _unitOfWork.MfgInvs.GetQueryable()
                            on sub.InvId equals inv.InvId
                        join dcSub in _unitOfWork.MfgDcSubs.GetQueryable()
                            on sub.RefDcSubId equals dcSub.DcSubId
                        join dc in _unitOfWork.MfgDcs.GetQueryable()
                            on dcSub.DcId equals dc.DcId
                        where
                            sub.RefDcSubId.HasValue &&
                            dcSubIds.Contains(sub.RefDcSubId.Value) &&
                            !sub.ItemCancel &&
                            !inv.IsCancel &&
                            dc.ReduceInvAsPerRej == true   // 🔥 IMPORTANT CONDITION
                        select sub.InvSubId
                    ).AnyAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanlabourDcOutgoingTrasByDcsubIdAsync");
                return false;
            }


        }

        public async Task<MfgDcSubVM?> GetRejReworkDcSubItemDetailByDcSubIdAsync(int dcSubId)
        {
            try
            {
                var dc = await _unitOfWork.MfgDcSubs.GetQueryable()
                    .FirstOrDefaultAsync(x => x.DcSubId == dcSubId);

                if (dc == null)
                    return null;


                bool invExists = await (
                    from sub in _unitOfWork.MfgInvSubs.GetQueryable()
                    join inv in _unitOfWork.MfgInvs.GetQueryable()
                        on sub.InvId equals inv.InvId
                    where sub.RefDcSubId == dcSubId && !sub.ItemCancel && !inv.IsCancel
                    select sub
                ).AnyAsync();

                decimal realTimeBalQty;

                if (invExists)
                {

                    decimal invoicedQty = await (
                        from sub in _unitOfWork.MfgInvSubs.GetQueryable()
                        join inv in _unitOfWork.MfgInvs.GetQueryable()
                            on sub.InvId equals inv.InvId
                        where sub.RefDcSubId == dcSubId && !sub.ItemCancel && !inv.IsCancel
                        select (decimal?)sub.Qty
                    ).SumAsync() ?? 0m;

                    realTimeBalQty = dc.Qty - invoicedQty;
                }
                else
                {
                    realTimeBalQty = dc.Qty;
                }

                return new MfgDcSubVM
                {
                    Qty = dc.Qty,
                    RejectQty = dc.RejectQty,
                    ReworkQty = dc.ReworkQty,
                    BalQty = realTimeBalQty
                };
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching Dc sub item detail for DcSubId: {dcSubId}"
                );
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }

        //-------------------------------------------


        //---------------Load PurchaseGrn--------------
        

        public async Task<int> GetRejectionCountAsync(int vendorCode)
        {
            var count = await
                (from g in _unitOfWork.PurchaseGRNs.GetQueryable()
                 join gs in _unitOfWork.PurchaseGRNSubs.GetQueryable()
                     on g.GRNId equals gs.GRNId
                 join ss in _unitOfWork.PurchaseSCNSubs.GetQueryable()
                     on gs.GRNSubId equals ss.RefGRNSubId
                 where ss.RejectBalQty > 0 && !ss.ItemCancel
                       && g.VendorCode == vendorCode
                       && !_unitOfWork.MfgDcs.GetQueryable()
                            .Any(dc => dc.RefGRNId == g.GRNId)
                 select g.GRNId)
                .Distinct()
                .CountAsync();

            return count;
        }

        public async Task<List<Dictionary<string, object>>> GetGRNDetailsByVendorCode(int VendorCode, int storeIssId)
        {
            try
            {

                var grnLines = await
                (
                    from g in _unitOfWork.PurchaseGRNs.GetQueryable()
                    join gs in _unitOfWork.PurchaseGRNSubs.GetQueryable()
                        on g.GRNId equals gs.GRNId
                    join ss in _unitOfWork.PurchaseSCNSubs.GetQueryable()
                        on gs.GRNSubId equals ss.RefGRNSubId
                    where g.VendorCode == VendorCode
                            && ss.RejectBalQty > 0 && !ss.ItemCancel
                            && !g.GRNCancel
                            && !_unitOfWork.MfgDcs.GetQueryable()
                                .Any(dc => dc.RefGRNId == g.GRNId)
                    select new
                    {
                        g.GRNId,
                        g.GRNNo,
                        g.Suffix,
                        g.GRNDate,
                        g.VendorCode,
                        gs.GRNSubId,
                        gs.ItemId,
                        gs.Item.ItemCode,
                        gs.Item.ItemName,
                        gs.Item.MeasureUnit,
                        gs.Item.HSNCode,
                        gs.Item.Specification,
                        gs.Item.Category.CategoryName,
                        gs.Item.Category.CategoryCode,
                        ss.RejectBalQty,
                        ss.UnitPrice,
                        ss.Remark,
                        CostCenterId = gs.CostId == 0 ? (int?)null : gs.CostId,
                        gs.CostCenter.ProjectNo,
                    }
                ).ToListAsync();

                if (!grnLines.Any())
                    return new();

                var itemIds = grnLines.Select(x => x.ItemId).Distinct().ToList();
                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeIssId);

                var result = grnLines.Select(x =>
                {
                    stockDict.TryGetValue(x.ItemId, out decimal stockQty);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,
                        ["GRNId"] = x.GRNId,
                        ["GRNSubId"] = x.GRNSubId,
                        ["GRNNo"] = x.GRNNo,
                        ["Suffix"] = x.Suffix,
                        ["GRNDate"] = x.GRNDate,

                        ["ItemId"] = x.ItemId,
                        ["ItemCode"] = x.ItemCode ?? "",
                        ["ItemName"] = x.ItemName ?? "",
                        ["Specification"] = x.Specification ?? string.Empty,
                        ["UOM"] = x.MeasureUnit ?? "",
                        ["HSNCode"] = x.HSNCode ?? "",
                        ["Category"] = x.CategoryName ?? "",

                        ["Qty"] = x.RejectBalQty,
                        ["BalQty"] = x.RejectBalQty,
                        ["UnitPrice"] = x.UnitPrice,
                        
                        ["CategoryCode"] = x.CategoryCode,
                        ["Stock Qty"] = stockQty,
                        ["CostCenterId"] = x.CostCenterId,
                        ["ProjectNo"] = x.ProjectNo ?? string.Empty,
                        ["Remark"] = "NV",

                    };

                }).ToList();


                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GRN details for CustId: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve GRN details. Please try again.");
            }
        }

        private async Task AdjustSCNSubBalanceAsync(int? refSCNSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refSCNSubId.HasValue || refSCNSubId == 0) return;


                var SCNSub = await _unitOfWork.PurchaseSCNSubs.GetAsync(refSCNSubId.Value);
                if (SCNSub == null) return;

                if (oldQty > 0)
                    SCNSub.RejectBalQty += oldQty;

                if (newQty > SCNSub.RejectBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Quote BalQty.");

                if (newQty > 0)
                    SCNSub.RejectBalQty -= newQty;

                await _unitOfWork.PurchaseSCNSubs.UpdateAsync(SCNSub);
                await _unitOfWork.SaveAsync();

                //// ✅ Calculate total BalQty for the parent PO
                //var totalBalQty = await _unitOfWork.PurchaseSCNSubs
                //    .GetQueryable()
                //    .Where(e => e.SCNId == SCNSub.SCNId)
                //    .SumAsync(e => e.BalQty);

                //var scn = await _unitOfWork.PurchaseSCNs.GetAsync(SCNSub.SCNId);
                //if (scn != null)
                //{
                //    scn.SCNTally = (totalBalQty == 0); // Tally only if all BalQty consumed
                //    await _unitOfWork.PurchaseSCNs.UpdateAsync(scn);
                //    await _unitOfWork.SaveAsync();
                //}


            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustSCNBalance] Validation failed in {context}");
                throw; // rethrow so UI/business logic can show proper error
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust SCN balance. Please contact support.");
            }
        }


        public async Task<decimal> GetSCNItemBalQtyFromSCNSubId(int ScnSubId)
        {
            try
            {
                return await _unitOfWork.PurchaseSCNSubs.GetQueryable()
                    .Where(e => e.SCNSubId == ScnSubId)
                    .Select(e => e.RejectBalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for SCNSubId: {ScnSubId}");
                throw new InvalidOperationException("Failed to retrieve PO balance quantity.");
            }
        }



        //------------**********DC Details for EWAY *****-----------------------\\

        public async Task<List<EWayDocument>> GetMfgDcByCustidAsync(int custId)
        {
            try
            {
                var data = await _unitOfWork.MfgDcs.GetQueryable()
                    .AsNoTracking()
                    .Where(dc =>
                        dc.CustId == custId &&
                        !string.IsNullOrEmpty(dc.DcNo) &&
                        (dc.EwayBillNo == null || dc.EwayBillNo == "0" || dc.EwayBillNo == "")

                    )
                    .OrderBy(dc => dc.DcId)
                    .Select(dc => new EWayDocument
                    {
                        DcId = dc.DcId,
                        DocNo = dc.DcNo + dc.Suffix,
                        Suffix = dc.Suffix,
                        DocDate = dc.DcDate,
                        CustName = dc.Customer.CustName
                    })
                    .ToListAsync();


                return data;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetMfgDcByCustidAsync({custId})");
                return new List<EWayDocument>();
            }
        }

    }


}
