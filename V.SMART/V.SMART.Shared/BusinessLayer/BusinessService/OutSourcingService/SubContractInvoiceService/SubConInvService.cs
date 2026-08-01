using AutoMapper;
using AutoMapper.QueryableExtensions;
using FastReport;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseSCNService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractInvoiceService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.OutSourcing.SubContractInvoice;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;
using static V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractInvoiceService.SubConInvService;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractInvoiceService
{
    public class SubConInvService :ISubConInvService
    {
            private readonly IUnitOfWork _unitOfWork;
            private readonly ICommonService _commonService;
            private readonly CurrentUserService _currentUserService;
            private readonly ILoggingService _logs;
            private readonly IMapper _mapper;

            public SubConInvService(
                IUnitOfWork unitOfWork,
                ICommonService commonService,
                CurrentUserService userService,
                IStockManagerService stockManagerService,
                ILoggingService logs,
                IMapper mapper, IPurchaseSCNService scnService)
            {
                _unitOfWork = unitOfWork;
                _commonService = commonService;
                _currentUserService = userService;
                _logs = logs;
                _mapper = mapper;

            }

            #region Common Service

            //Screen
            public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
                => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

            // 🔹 Vendors
            public async Task<VendorVM?> GetVendorByIdAsync(int VendorCode)
               => await _commonService.GetVendorByVenerCodeAsync(VendorCode);

            public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
            {
                return await _commonService.SearchVendorsAsync(searchText);
            }
            // 🔹 Contacts
            public async Task<List<VendorContact>> GetContactPersonsVendorAsync(int Vendorcode)
                => await _commonService.GetContactPersonsVendorAsync(Vendorcode);

            // 🔹 Consignee addresses
            public async Task<List<VendorInDirect>> GetConsigneeAddressesAsync(int VendorCode)
                => await _commonService.GetConsigneeAddressesVendorAsync(VendorCode);

            // 🔹 Items
            public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
                => await _commonService.SearchItemsAsync(searchText);

            public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
               => await _commonService.GetItemByItemIdAsync(itemId);

            public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
               => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

            // 🔹 Currency
            public async Task<List<Currency>> GetCurrenciesAsync()
                => (await _commonService.GetCurrenciesAsync()).ToList();

            // 🔹 Decimal places
            public async Task<int> GetDecimalPlacesAsync()
                => await _commonService.GetDecimalPlacesAsync();

            public async Task<Currency?> GetCurrencyByIdAsync(int currId)
               => (await _commonService.GetCurrencyByIdAsync(currId));

            // 🔹 Get latest currency rate (from CurrencyToday Service)
            public async Task<decimal?> GetLatestCurrencyValueAsync(int currId)
                => await _commonService.GetLatestCurrencyValueAsync(currId);


            public async Task<Companydetails?> GetCompanyDetailsAsync()
                => await _commonService.GetCompanyDetailsAsync();

            #endregion

            #region Invoice operation
            public async Task<(List<SubConInvVM> invs, int totalCount)> GetPagedInvoiceAsync(int pageNumber, int pageSize, string search)
            {
                try
                {
                    var query = _unitOfWork.SubConInvs
                           .GetQueryable()
                           .AsNoTracking()
                           .AsSplitQuery()
                           .Include(q => q.Vendor)
                           .Include(c => c.Currency);


                    int totalCount = await query.CountAsync();


                    var entities = await query
                        .OrderByDescending(i => i.InvId)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();


                    var data = _mapper.Map<List<SubConInvVM>>(entities);

                    return (data, totalCount);

                }
                catch (Exception ex)
                {

                    throw;
                }

            }

            public async Task<List<Dictionary<string, object>>> GetSCNDetailsByVendorCode(int VendorCode, bool AcptRejRewQtRequired)
            {
                try
                {
                    var result = await (from p in _unitOfWork.SubConSCNs.GetQueryable()
                                        join ps in _unitOfWork.SubConSCNSubs.GetQueryable()
                                            on p.SCNId equals ps.SCNId
                                        where p.VendorCode == VendorCode
                                              && !p.SCNTally
                                              && !p.SCNCancel && !p.ShortClose &&
                                                ps.BalQty > 0
                                        select new
                                        {
                                            ps.SCNSubId,
                                            p.SCNNo,
                                            p.Suffix,
                                            p.SCNDate,
                                            ps.ItemId,
                                            ps.Item.ItemCode,
                                            ps.Item.ItemName,
                                            ps.Item.Specification,
                                            ps.Item.MeasureUnit,
                                            ps.Item.HSNCode,
                                            ps.Item.Category.CategoryName,
                                            ps.BalQty,
                                            ps.UnitPrice,
                                            ps.RejQty,
                                            ps.RewQty,
                                            CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                                            ps.CostCenter.ProjectNo,
                                            p.MainRemarks
                                        }).ToListAsync();

                    return result.Select(r => new Dictionary<string, object>
                    {
                        ["Selected"] = false,
                        ["SCNSubId"] = r.SCNSubId,

                        ["SCNNo"] = r.SCNNo,
                        ["Suffix"] = r.Suffix,
                        ["SCNDate"] = r.SCNDate,

                        ["ItemId"] = r.ItemId,
                        ["ItemCode"] = r.ItemCode ?? string.Empty,
                        ["ItemName"] = r.ItemName ?? string.Empty,
                        ["Specification"] = r.Specification ?? string.Empty,
                        ["UOM"] = r.MeasureUnit ?? string.Empty,
                        ["HSNCode"] = r.HSNCode ?? string.Empty,
                        ["Category"] = r.CategoryName ?? string.Empty,
                        ["Qty"] = AcptRejRewQtRequired ? (r.BalQty + r.RejQty + r.RewQty) : r.BalQty,
                        ["BalQty"] = AcptRejRewQtRequired ? (r.BalQty + r.RejQty + r.RewQty) : r.BalQty,
                        ["UnitPrice"] = r.UnitPrice,

                        ["RejectQty"] = r.RejQty,
                        ["RewQty"] = r.RewQty,
                        ["CostCenterId"] = r.CostCenterId,
                        ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                        ["Remark"] = r.MainRemarks ?? string.Empty,
                    }).ToList();
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error fetching SCN details for VendorCode: {VendorCode}");
                    throw new InvalidOperationException("Failed to retrieve SCN details. Please try again.");
                }
            }

            public async Task<SubConInvVM> GetPurchaseInvoiceByIdAsync(int InvId)
            {
                try
                {
                    var entity = await _unitOfWork.SubConInvs.GetQueryable()
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(q => q.SubConInvSubs)
                        .Include(q => q.SubConInvSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                        .Include(q => q.Vendor)
                        .Include(q => q.Currency)
                        .Include(j => j.SubConInvSubs).ThenInclude(ps => ps.DebitNoteSubs).ThenInclude(p => p.DebitNote)
                        .FirstOrDefaultAsync(q => q.InvId == InvId);

                    return _mapper.Map<SubConInvVM?>(entity);
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"GetPurchaseInvoiceByIdAsync({InvId})");
                    return null;
                }
            }

        public async Task<List<SubConInvSubVM>> GetSubConInvSubsByInvIdAsync(int invId)
        {
            try
            {
                return await _unitOfWork.SubConInvSubs
                    .GetQueryable()
                    .Where(s => s.InvId == invId)
                    .OrderBy(s => s.SlNo)
                    .ProjectTo<SubConInvSubVM>(_mapper.ConfigurationProvider)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching Sub-Contract Invoice sub-items for InvId: {invId}"
                );

                throw new InvalidOperationException(
                    "Failed to retrieve Sub-Contract Invoice items. Please try again."
                );
            }
        }


        public async Task<SubConInv?> GetLastInvAsync(int VendorCode)
            {
                try
                {
                    return await _unitOfWork.SubConInvs.GetLatestAsync(
                        q => q.VendorCode == VendorCode,
                        q => q.InvId);
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error in GetLastQuoteAsync for VendorCode: {VendorCode}");
                    throw new InvalidOperationException("Failed to retrieve last purchase quotation. Please try again.");
                }
            }

            public async Task<int> GetPendingSCNCountAsync(int VendorCode)
            {
                return await _unitOfWork.SubConSCNs
                    .GetQueryable()
                    .Where(e => e.VendorCode == VendorCode && e.SCNTally == false && !e.SCNCancel && !e.ShortClose)
                    .CountAsync();

            }

            public async Task<bool> DeletePurchaseInvoiceByIdAsync(int InvId, int screenCode)
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var invoice = await _unitOfWork.SubConInvs
                        .GetQueryable()
                        .Include(e => e.SubConInvSubs)
                        .FirstOrDefaultAsync(e => e.InvId == InvId);

                    if (invoice == null)
                        return false;

                    var changes = new StringBuilder();

                    foreach (var sub in invoice.SubConInvSubs)
                    {
                        if (sub.RefSCNSubId > 0)
                        {
                            await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, sub.Qty, 0, "Invoice Deletion");
                        }
                    }

                    var deleted = await _unitOfWork.SubConInvs.DeleteAsync(InvId);
                    if (!deleted) return false;

                    await _unitOfWork.SaveAsync();
                    await transaction.CommitAsync();

                    await _logs.LogUserAction(
                        UserName: await _currentUserService.GetUsernameAsync(),
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Purchase Invoice",
                        action: $"Deleted Purchase Invoice: {invoice.InvNo}",
                        additionalInfo: $"GRN Id: {invoice.InvId}\n{changes}"
                    );

                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await _logs.LogDeveloperError(ex, $"Failed to delete Invoice: {InvId}");
                    throw;
                }
            }

            private async Task AdjustSCNSubBalanceAsync(int? refSCNSubId, decimal oldQty, decimal newQty, string context)
            {
                try
                {
                    if (!refSCNSubId.HasValue || refSCNSubId == 0) return;


                    var SCNSub = await _unitOfWork.SubConSCNSubs.GetAsync(refSCNSubId.Value);
                    if (SCNSub == null) return;

                    if (oldQty > 0)
                        SCNSub.BalQty += oldQty;

                    if (newQty > SCNSub.BalQty)
                        throw new InvalidOperationException($"{context}: Qty cannot exceed Quote BalQty.");

                    if (newQty > 0)
                        SCNSub.BalQty -= newQty;

                    await _unitOfWork.SubConSCNSubs.UpdateAsync(SCNSub);
                    await _unitOfWork.SaveAsync();

                    // ✅ Calculate total BalQty for the parent PO
                    var totalBalQty = await _unitOfWork.SubConSCNSubs
                        .GetQueryable()
                        .Where(e => e.SCNId == SCNSub.SCNId)
                        .SumAsync(e => e.BalQty);

                    var scn = await _unitOfWork.SubConSCNs.GetAsync(SCNSub.SCNId);
                    if (scn != null)
                    {
                        scn.SCNTally = (totalBalQty == 0); // Tally only if all BalQty consumed
                        await _unitOfWork.SubConSCNs.UpdateAsync(scn);
                        await _unitOfWork.SaveAsync();
                    }


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

            private async Task LogChangesAsync(StringBuilder changes, string action)
            {
                try
                {
                    if (changes.Length == 0) return;

                    await _logs.LogUserAction(
                        UserName: await _currentUserService.GetUsernameAsync(),
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Purchase Invoice",
                        action: action,
                        additionalInfo: changes.ToString()
                    );

                }
                catch (Exception ex)
                {

                    await _logs.LogDeveloperError(ex, $"Failed to LogChangesAsync in Purchase GRN");
                }
            }

            public async Task<bool> HasAnyItemOrPurchaseInvoiceCancelAsync(int InvId)
            {
                try
                {
                    var isInvoiceCancelled = await _unitOfWork.SubConInvs
                        .AnyAsync(q => q.InvId == InvId && q.InvCancel == true);

                    var isItemCancelled = await _unitOfWork.SubConInvSubs
                        .AnyAsync(i => i.InvId == InvId && i.ItemCancel == true);

                    return isInvoiceCancelled || isItemCancelled;
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrPurchaseInvoiceCancelAsync for InvId: {InvId}");
                    throw;
                }
            }

            public async Task<bool> GetPurchaseInvoiceByIdIsCancelAsync(int InvId)
            {
                return await _unitOfWork.SubConInvs
                    .GetQueryable()
                    .Where(e => e.InvId == InvId)
                    .AnyAsync(e =>
                        e.InvCancel == true ||
                        e.SubConInvSubs.Any(s => s.ItemCancel == true)
                    );
            }

            public async Task<decimal> GetSCNItemPerformaBalQtyFromSCNSubId(int SCNSubId)
            {
                try
                {
                    return await _unitOfWork.SubConSCNSubs.GetQueryable()
                        .Where(e => e.SCNSubId == SCNSubId)
                        .Select(e => e.BalQty)
                        .FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error fetching PurchaseSCNBalQty for SCNSubId: {SCNSubId}");
                    throw new InvalidOperationException("Failed to retrieve SCN balance quantity.");
                }
            }

            public async Task<decimal> GetExistingPurchInvoiceQtyByInvSubId(int InvSubId)
            {
                try
                {
                    return await _unitOfWork.SubConInvSubs.GetQueryable()
                        .Where(e => e.InvSubId == InvSubId)
                        .Select(e => e.Qty)
                        .FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error fetching Purchase Invoice Qty for InvSubId: {InvSubId}");
                    throw new InvalidOperationException("Failed to retrieve GetExistingPurchInvoiceQtyByInvSubId.");
                }
            }

            public async Task<SubConInvSubVM?> GetInvSubItemDetailByInvSubIdAsync(int InvSubId)
            {
                try
                {
                    return await _unitOfWork.SubConInvSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Where(q => q.InvSubId == InvSubId)
                        .Select(q => new SubConInvSubVM
                        {
                            Qty = q.Qty,
                            BalQty = q.BalQty
                        })
                        .FirstOrDefaultAsync();
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error fetching Invoice sub item detail for InvSubId: {InvSubId}");
                    throw new InvalidOperationException("Failed to retrieve Invoice sub-item details.");
                }
            }

            public async Task<decimal> GetSCNItemInvoiceBalQtyFromSCNSubId(int SCNSubId)
            {
                try
                {
                    return await _unitOfWork.SubConSCNSubs.GetQueryable()
                        .Where(e => e.SCNSubId == SCNSubId)
                        .Select(e => e.BalQty)
                        .FirstOrDefaultAsync();   // <-- missing line
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error fetching BalQty for SCNSubId: {SCNSubId}");
                    throw new InvalidOperationException("Failed to retrieve SCN balance quantity.");
                }
            }

            public async Task UpdateItemCancelAndAddorRevertAsync(SubConInvSubVM subItem, int screenCode, string InvNo, DateTime InvDate)
            {
                await using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var subEntity = await _unitOfWork.SubConInvSubs.GetQueryable().Where
                                    (x => x.InvSubId == subItem.InvSubId).FirstOrDefaultAsync();

                    if (subEntity == null)
                        throw new KeyNotFoundException($"Subitem with InvSubId {subItem.InvSubId} not found.");

                    if (!subItem.ItemCancel)
                    {
                        await ValidatePurchaseSCNBalanceBeforeRevertAsync(subEntity);
                    }

                    subEntity.ItemCancel = subItem.ItemCancel;
                    subEntity.CancelItemReason = subItem.ItemCancelReason;
                    await _unitOfWork.SubConInvSubs.UpdateAsync(subEntity);
                    await _unitOfWork.SaveAsync();

                    if (subItem.ItemCancel)
                    {
                        await AdjustSCNSubBalanceAsync(subEntity.RefSCNSubId, subEntity.Qty, 0, $"Purchase GRN Item Cancel - {subItem.ItemCode}");

                    }
                    else
                    {
                        await AdjustSCNSubBalanceAsync(subEntity.RefSCNSubId, 0, subEntity.Qty, $"Purchase GRN Revert Cancel - {subItem.ItemCode}");

                    }

                    //  await UpdateSCNTallyStatusAsync(subItem.i);

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

            public async Task<List<SubConInvSubVM>> GetInvoiceSubByInvIdAsync(int InvId)
            {
                try
                {
                    var subs = await _unitOfWork.SubConInvSubs
                                    .GetQueryable()
                                    .Where(s => s.InvId == InvId)
                                    .OrderBy(s => s.SlNo)
                                    .ProjectTo<SubConInvSubVM>(_mapper.ConfigurationProvider)
                                    .AsNoTracking()
                                    .ToListAsync();

                    return subs;
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error fetching Purchase Invoice items for InvId: {InvId}");
                    throw new InvalidOperationException("Failed to retrieve Purchase Invoice sub-items. Please try again.");
                }
            }

            public async Task UpdatedCancelStatusAndAddOrRevertQty(SubConInvVM InvVM, int screenCode)
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var existingInv = await _unitOfWork.SubConInvs.GetAsync(InvVM.InvId);
                    if (existingInv == null)
                        throw new InvalidOperationException("Purchase Invoice not found.");

                    var subs = await _unitOfWork.SubConInvSubs
                        .GetQueryable()
                        .Where(s => s.InvId == InvVM.InvId)
                        .ToListAsync();

                    if (!InvVM.InvCancel)
                    {
                        foreach (var sub in subs)
                        {
                            await ValidatePurchaseSCNBalanceBeforeRevertAsync(sub);
                        }
                    }

                    existingInv.InvCancel = InvVM.InvCancel;
                    existingInv.CancelReason = InvVM.CancelReason;
                    await _unitOfWork.SubConInvs.UpdateAsync(existingInv);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in subs)
                    {
                        if (existingInv.InvCancel)
                        {
                            if (sub.RefSCNSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustSCNSubBalanceAsync(sub.RefSCNSubId.Value, sub.Qty, 0, $"Purchase Invoice Cancelled - {existingInv.InvNo}");
                            }
                        }
                        else
                        {
                            if (sub.RefSCNSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustSCNSubBalanceAsync(sub.RefSCNSubId.Value, 0, sub.Qty, $"Purchase Invoice Reverted - {existingInv.InvNo}"
                                );
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

            private async Task ValidatePurchaseSCNBalanceBeforeRevertAsync(SubConInvSub sub)
            {
                if (sub.RefSCNSubId.GetValueOrDefault() <= 0)
                    return;

                var entity = await _unitOfWork.SubConSCNSubs.GetAsync(sub.RefSCNSubId.Value);
                if (entity == null)
                    throw new InvalidOperationException($"PurchaseInvoice not found for RefSCNSubId: {sub.RefSCNSubId}");

                if (entity.BalQty < sub.Qty)
                {
                    throw new InvalidOperationException(
                        $"Cannot revert because PurchaseInvoice balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
                }
            }

            public async Task DeleteAndResequenceAsync(SubConInvSubVM subitem, SubConInvVM InvVM)
            {
                using var transaction = await _unitOfWork.BeginTransactionAsync();
                var changes = new StringBuilder();

                try
                {
                    if (subitem.InvSubId > 0) // persisted subitem
                    {
                        var entity = await _unitOfWork.SubConInvSubs.GetAsync(subitem.InvSubId);
                        if (entity == null)
                            throw new InvalidOperationException("Sub item not found.");

                        // Restore balance qty
                        if (entity.RefSCNSubId > 0)
                        {
                            await AdjustSCNSubBalanceAsync(subitem.RefSCNSubId, entity.Qty, 0, "Invoice Deletion");
                        }
                        //else
                        //{
                        //    await UpdateCostCenterAsync(subitem.CostId, false, changes);
                        //}

                        // Delete from DB
                        await _unitOfWork.SubConInvSubs.DeleteAsync(entity.InvSubId);
                        await _unitOfWork.SaveAsync();

                        // Log action
                        await _logs.LogUserAction(
                            await _currentUserService.GetUsernameAsync(),
                            _currentUserService.MachineName,
                            _currentUserService.IpAddress,
                            "Purchase Invoice",
                            $"Deleted Item: {subitem.ItemCode}",
                            $"Invoice No: {InvVM?.InvNo}"
                        );
                    }
                    else
                    {
                        // Not yet persisted → just remove from VM
                        InvVM.SubConInvSubVMs.Remove(subitem);
                        return;
                    }

                    // Resequence persisted subitems
                    var remaining = await _unitOfWork.SubConInvSubs
                        .GetQueryable()
                        .Where(x => x.InvId == InvVM.InvId)
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

            public async Task<bool> IsDuplicateInvoiceAsync(string InvNo, string suffix, int? currentInvId = null, int? VendorCode = null)
            {
                if (string.IsNullOrWhiteSpace(InvNo))
                    return false;

                try
                {
                    return await _unitOfWork.SubConInvs
                        .GetQueryable()
                        .AnyAsync(x => x.InvNo == InvNo
                                    && x.Suffix == suffix
                                    && x.VendorCode == VendorCode
                                    && (currentInvId == null || x.InvId != currentInvId));
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, $"Error in IsDuplicateInvoiceAsync for Invoice NO: {InvNo}");
                    throw new InvalidOperationException("Failed to check duplicate Invoice.");
                }
            }

            //AcceptQty+rejectQty+reworkqty
            public async Task AdjustAcceptrejRewQty(SubConInv entity)
            {
                try
                {
                    if (entity != null)
                    {
                        foreach (var invsub in entity.SubConInvSubs)
                        {
                            var scnSub = await _unitOfWork.PurchaseSCNSubs.GetAsync(invsub.RefSCNSubId.Value);
                            if (scnSub == null)
                                continue;

                            decimal accQty = scnSub.BalQty;
                            decimal rejQty = scnSub.RejectQty;
                            decimal rewQty = scnSub.ReworkQty;

                            decimal actualInvQty = invsub.Qty;

                            decimal maxPossibleQty = accQty + rejQty + rewQty;

                            // CASE 4 - Qty exceeds possible total
                            if (actualInvQty > maxPossibleQty)
                            {
                                throw new Exception($"Invoice Qty {actualInvQty} is greater than available Qty {maxPossibleQty} for Item {invsub.ItemId}.");
                            }

                            // CASE 1: <= Accept Qty
                            if (actualInvQty <= accQty)
                            {
                                invsub.Qty = actualInvQty;
                                invsub.BalQty = actualInvQty;
                                invsub.RejectQty = 0;
                                invsub.ReworkQty = 0;
                            }
                            // CASE 2: Between Accept and Accept + Reject
                            else if (actualInvQty <= accQty + rejQty)
                            {
                                invsub.Qty = accQty;
                                invsub.BalQty = accQty;
                                invsub.RejectQty = actualInvQty - accQty;
                                invsub.ReworkQty = 0;
                            }
                            // CASE 3: Between Accept+Reject and Total
                            else if (actualInvQty <= maxPossibleQty)
                            {
                                invsub.Qty = accQty;
                                invsub.BalQty = accQty;
                                invsub.RejectQty = rejQty;
                                invsub.ReworkQty = actualInvQty - (accQty + rejQty);
                            }
                        }
                    }

                }
                catch (Exception ex)
                {

                    await _logs.LogDeveloperError(ex, $"Failed to AdjustAcceptrejRewQty Purchase Invoice: {entity.InvNo}");
                    throw new InvalidOperationException("Failed to AdjustAcceptrejRewQty Purchase Invoice. Please try again.");
                }
            }

            public async Task<SubConInvVM> UpsertPurchaseInvoice(SubConInvVM purchgInvoiceVM, int screenCode)
            {
                if (purchgInvoiceVM == null)
                    throw new ArgumentNullException(nameof(purchgInvoiceVM));

                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();
                var changes = new StringBuilder();

                using var transaction = await _unitOfWork.BeginTransactionAsync();

                try
                {
                    SubConInv entity;

                    if (purchgInvoiceVM.InvId == 0)
                    {
                        entity = _mapper.Map<SubConInv>(purchgInvoiceVM);

                        entity.CreatedBy = currentUser;
                        entity.CreatedDate = now;
                        entity.NoOfItems = purchgInvoiceVM.SubConInvSubVMs.Count();
                        entity.Balance = purchgInvoiceVM.GrandTotal;

                        entity.SubConInvSubs = purchgInvoiceVM.SubConInvSubVMs
                       .Select(s => _mapper.Map<SubConInvSub>(s)).ToList();
                        if (entity.IsAcptRejRewQtyRequired)
                        {
                            await AdjustAcceptrejRewQty(entity);
                        }


                        await _unitOfWork.SubConInvs.CreateAsync(entity);
                        await _unitOfWork.SaveAsync();

                        foreach (var sub in entity.SubConInvSubs)
                        {
                            if (sub.RefSCNSubId > 0)
                            {
                                await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, 0, sub.Qty, "Purchase Invoice Creation");
                            }

                        }

                        changes.AppendLine("Purchase Invoice Created.");
                    }
                    else
                    {
                        entity = await _unitOfWork.SubConInvs.GetQueryable()
                            .Include(q => q.SubConInvSubs)
                            .FirstOrDefaultAsync(q => q.InvId == purchgInvoiceVM.InvId)
                            ?? throw new InvalidOperationException("Purchase Invoice found.");

                        var parentChanges = GetPropertyChanges(entity, purchgInvoiceVM);
                        if (!string.IsNullOrEmpty(parentChanges))
                            changes.AppendLine("Parent Changes:\n" + parentChanges);

                        _mapper.Map(purchgInvoiceVM, entity);
                        entity.ModifiedBy = currentUser;
                        entity.ModifiedDate = now;
                        entity.NoOfItems = entity.SubConInvSubs.Count();
                        entity.Balance = purchgInvoiceVM.GrandTotal;

                        if (entity.IsAcptRejRewQtyRequired)
                        {
                            await AdjustAcceptrejRewQty(entity);
                        }

                        await HandleChildUpdatesAsync(entity, purchgInvoiceVM.SubConInvSubVMs, changes, screenCode);

                        changes.AppendLine("Purchase Invoice Updated.");
                    }

                    var tdsInfo = await _unitOfWork.Vendors.GetQueryable()
                    .Where(v => v.VendorCode == entity.VendorCode).Select(v => new { v.TdsDeduct, }).FirstOrDefaultAsync();
                    if (tdsInfo.TdsDeduct)
                    {
                        await AutoDeductTDSForVendorAsync(entity);
                    }

                    await _unitOfWork.SaveAsync();
                    // await UpdateGRNTallyStatusAsync(purchgInvoiceVM.GRNId);
                    await transaction.CommitAsync();

                    await LogChangesAsync(changes, purchgInvoiceVM.InvId == 0 ? "Purchase Invoice Created" : "Purchase Invoice  Updated");

                    var savedEntity = await _unitOfWork.SubConInvs.GetQueryable()
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(q => q.SubConInvSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                        .Include(q => q.Vendor)
                        .FirstOrDefaultAsync(q => q.InvId == entity.InvId);

                    return _mapper.Map<SubConInvVM>(savedEntity!);
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    await _logs.LogDeveloperError(ex, $"Failed to upsert Purchase Invoice: {purchgInvoiceVM.InvNo}");
                    throw new InvalidOperationException("Failed to save Purchase Invoice. Please try again.");
                }
            }

            private async Task HandleChildUpdatesAsync(SubConInv existingInvoice, List<SubConInvSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
            {
                var existingSubIds = existingInvoice.SubConInvSubs.Select(s => s.InvSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.InvSubId).ToHashSet();

                // DELETE removed children
                foreach (var sub in existingInvoice.SubConInvSubs.Where(s => !incomingSubIds.Contains(s.InvSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - InvSubId: {sub.InvSubId}, Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.SubConInvSubs.DeleteAsync(sub.InvSubId);
                    await _unitOfWork.SaveAsync();

                    if (sub.RefSCNSubId > 0)
                    {
                        await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, sub.Qty, 0, "Invoice Deletion");
                    }

                }

                // ADD or UPDATE children
                foreach (var subVM in incomingSubVMs)
                {
                    decimal.TryParse((subVM.Qty.Value).ToString(), out decimal qty);
                    if (subVM.InvSubId == 0)
                    {
                        var newSub = _mapper.Map<SubConInvSub>(subVM);
                        newSub.InvId = existingInvoice.InvId;
                        await _unitOfWork.SubConInvSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");
                        if (subVM.RefSCNSubId > 0)
                        {
                            await AdjustSCNSubBalanceAsync(subVM.RefSCNSubId, 0, subVM.Qty ?? 0, "GRN Creation");
                        }

                    }
                    else
                    {
                        var existingSub = existingInvoice.SubConInvSubs.FirstOrDefault(s => s.InvSubId == subVM.InvSubId);
                        if (existingSub != null)
                        {
                            //if ((existingSub.CostId != subVM.CostId) && (existingSub.RefPoSubId == null || existingSub.RefPoSubId == 0))
                            //    await UpdateCostCenterAsync(existingSub.CostId, false, changes);


                            if (subVM.RefSCNSubId > 0)
                                await AdjustSCNSubBalanceAsync(subVM.RefSCNSubId, existingSub.Qty, subVM.Qty ?? 0, "Invoice Update");

                            //if ((subVM.CostId > 0) && (subVM.RefPoSubId == null || subVM.RefPoSubId == 0))
                            //    await UpdateCostCenterAsync(subVM.CostId, true, changes);

                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }
            }
            public async Task<List<SubConInvSubVM>> GetDistinctRefSCNByPurchaseInvIdAsync(int invId)
            {
                return await _unitOfWork.SubConInvSubs
                            .GetQueryable()
                            .AsNoTracking()
                            .Where(s => s.InvId == invId)
                            .Include(s => s.SubConSCNSub)
                                .ThenInclude(g => g.SubConSCN)
                            .Where(s => s.SubConSCNSub != null &&
                                        s.SubConSCNSub.SubConSCN != null)
                            .GroupBy(s => new
                            {
                                s.SubConSCNSub.SubConSCN.Suffix,
                                s.SubConSCNSub.SubConSCN.SCNNo,
                                s.SubConSCNSub.SubConSCN.SCNDate
                            })
                            .Select(g => new SubConInvSubVM
                            {
                                RefSCNNo = $"{g.Key.SCNNo}{g.Key.Suffix}",
                                RefSCNDate = g.Key.SCNDate
                            })
                            .ToListAsync();
            }

            public async Task<(List<SubConInvVM> Invies, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
            {
                var query = _unitOfWork.SubConInvs.GetQueryable()
                    .Include(j => j.SubConInvSubs).ThenInclude(s => s.Item)
                    .Include(j => j.Vendor)
                    .Include(j => j.Currency)
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
                    .OrderByDescending(x => x.InvId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Use AutoMapper
                var vmList = _mapper.Map<List<SubConInvVM>>(list);

                return (vmList, total);
            }

            #endregion

            #region Auto TDS Deducts

            //-------TDS----------------
            public async Task<bool> UpdateTDSAmountAsync(SubConInvVM purchgInvoiceVM)
            {
                try
                {
                    var changes = new StringBuilder();

                    SubConInv entity;

                    entity = await _unitOfWork.SubConInvs
                                        .GetQueryable()
                                        .FirstOrDefaultAsync(x => x.InvId == purchgInvoiceVM.InvId && x.InvNo == purchgInvoiceVM.InvNo && x.Suffix == purchgInvoiceVM.Suffix);

                    if (entity == null)
                        return false;


                    var parentChanges = GetPropertyChanges(entity, purchgInvoiceVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    entity.TDSAmount = purchgInvoiceVM.TDSAmount;
                    entity.Balance = (entity.GrandTotal) - purchgInvoiceVM.TDSAmount;

                    if (entity.Balance < 0)
                        entity.Balance = 0;

                    await _unitOfWork.SubConInvs.UpdateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    await LogChangesAsync(changes, purchgInvoiceVM.InvId == 0 ? "Purchase Invoice Created" : "Purchase Invoice TDS  Updated");

                    return true;
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, "Error Updating TDS Amount");
                    return false;
                }
            }

            public async Task<bool> AutoDeductTDSForVendorAsync(SubConInv purchgInvoiceVM)
            {
                try
                {

                    var vendor = await _unitOfWork.Vendors.GetQueryable()
                                    .FirstOrDefaultAsync(v => v.VendorCode == purchgInvoiceVM.VendorCode);

                    if (vendor == null || vendor.TdsDeduct == false)
                        return false;

                    decimal exemptAmt = (decimal)vendor.TDSExmptdAmt;
                    decimal tdsRate = (decimal)vendor.TDSDeductper;
                    decimal maxBill = (decimal)vendor.TDSBillval;


                    var invSubs = await _unitOfWork.SubConInvSubs.GetQueryable()
                                    .Where(s => s.InvId == purchgInvoiceVM.InvId)
                                    .ToListAsync();


                    decimal totalAmount = invSubs.Sum(r =>
                    {
                        decimal qty = r.Qty;
                        decimal rate = r.UnitPrice;
                        decimal disc = r.LineDiscountPercent ?? 0;

                        return (qty * rate) - (qty * rate * disc / 100);
                    });


                    decimal tdsAmount = Math.Round((totalAmount * tdsRate / 100), 3);

                    decimal vendorTotalPurchase = await _unitOfWork.SubConInvs
                        .GetQueryable()
                        .Where(x => x.VendorCode == purchgInvoiceVM.VendorCode)
                        .SumAsync(x => x.GrandTotal);


                    bool hasPreviousTDS = await _unitOfWork.SubConInvs
                        .GetQueryable()
                        .AnyAsync(x => x.VendorCode == purchgInvoiceVM.VendorCode && (x.TDSAmount) > 0);

                    if (hasPreviousTDS)
                    {
                        await UpdateInvoiceTDS(purchgInvoiceVM.InvId, tdsAmount);
                        return true;
                    }


                    if (vendorTotalPurchase >= exemptAmt || (maxBill > 0 && totalAmount >= maxBill))
                    {
                        var invoices = await _unitOfWork.SubConInvs
                            .GetQueryable()
                            .Where(x => x.VendorCode == purchgInvoiceVM.VendorCode)
                            .ToListAsync();

                        foreach (var inv in invoices)
                        {

                            decimal invTotal = (inv.GrandTotal);
                            decimal invTds = invTotal * tdsRate / 100;

                            inv.TDSAmount = invTds;
                            inv.Balance = invTotal - invTds;

                            if (inv.Balance < 0)
                                inv.Balance = 0;

                            await _unitOfWork.SubConInvs.UpdateAsync(inv);
                        }

                        await _unitOfWork.SaveAsync();
                        return true;
                    }


                    await UpdateInvoiceTDS(purchgInvoiceVM.InvId, tdsAmount);

                    return true;
                }
                catch (Exception ex)
                {
                    await _logs.LogDeveloperError(ex, "Error in AutoDeductTDSForVendorAsync");
                    return false;
                }
            }

            private async Task UpdateInvoiceTDS(int invId, decimal tdsAmount)
            {
                var invoice = await _unitOfWork.SubConInvs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.InvId == invId);

                if (invoice == null) return;

                decimal total = invoice.GrandTotal;

                invoice.TDSAmount = tdsAmount;
                invoice.Balance = total - tdsAmount;

                if (invoice.Balance < 0)
                    invoice.Balance = 0;

                await _unitOfWork.SubConInvs.UpdateAsync(invoice);
                await _unitOfWork.SaveAsync();
            }

        #endregion

        public async Task<bool> HasAnyItemOrInvoiceDebitNoteAsync(int RefSubInvId)
        {
            try
            {
                var isExist = await (from inv in _unitOfWork.DebitNotes.GetQueryable()
                                     join sub in _unitOfWork.DebitNoteSubs.GetQueryable()
                                     on inv.DbId equals sub.DbId
                                     where sub.RefSubConInvSubId == RefSubInvId && inv.SubContract == true
                                     select inv.DbId).AnyAsync();

                return isExist;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrInvoiceDebitNoteAsync for RefSubInvId: {RefSubInvId}");
                throw;
            }
        }


        public static class DynamicWhereBuilder
            {
                public static IQueryable<SubConInv> ApplyFilter(IQueryable<SubConInv> query, string field, object value)
                {
                    if (value == null) return query;

                    switch (field)
                    {
                        case "InvNo":
                            return query.Where(x => x.InvNo.Contains(value.ToString()));
                        case "Vendor":
                            return query.Where(x => x.Vendor.VendorName.Contains(value.ToString()));
                        case "ItemCode":
                            return query.Where(x => x.SubConInvSubs.Any(s => s.Item.ItemCode.Contains(value.ToString())));
                        case "ItemName":
                            return query.Where(x => x.SubConInvSubs.Any(s => s.Item.ItemName.Contains(value.ToString())));
                        case "CreatedBy":
                            return query.Where(x => x.CreatedBy.Contains(value.ToString()));

                        case "FromDate":
                            return query.Where(x => x.CreatedDate >= DateTime.Parse(value.ToString()));

                        case "ToDate":
                            return query.Where(x => x.CreatedDate <= DateTime.Parse(value.ToString()));
                    }

                    return query;
                }
            }

        public async Task<bool> IsDocumentUploaded(int Dcid)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Sub-Contract Invoice" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == Dcid);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }


        public async Task<List<SubConInvoicePendingVM>> GetSubContractInvoicePendingList(string status)//Shankar
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<SubConInvoicePendingVM>("Sp_GetSubContractInvoicePendingList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
