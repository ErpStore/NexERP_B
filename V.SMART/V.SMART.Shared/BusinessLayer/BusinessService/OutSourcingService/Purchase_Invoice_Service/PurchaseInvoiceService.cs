using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchase_Invoice_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseSCNService;
using V.SMART.Shared.BusinessLayer.BusinessService.InventoryService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote;
using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.PerformaInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.Purchase_Invoice_Service
{
    public class PurchaseInvoiceService : IPurchaseInvoiceService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public PurchaseInvoiceService(
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

        public async Task<bool> IsInvTransactionsMatchedAsync(int invId, PurchaseInvoiceVM invoiceVMs)
        {
            try
            {
                //var quoteSubIds = await _unitOfWork.MfgQuoteSubs
                //    .GetQueryable()
                //    .Where(x => x.QuoteId == quoteId)
                //    .Select(x => x.QuoteSubId)
                //    .ToListAsync();

                //bool hasQuote = quoteSubIds.Any();

                //bool hasTransactions = false;

                //if (hasQuote)
                //{
                //    // Check Manufacturing Quotation references
                //    hasTransactions = await _unitOfWork.MfgPoSubs
                //        .GetQueryable()
                //        .AnyAsync(pqs =>
                //            pqs.RefQuoteSubId.HasValue &&
                //            quoteSubIds.Contains(pqs.RefQuoteSubId.Value));
                //}

                // Quantity mismatch check

                bool rateMismatch = false;

                var list = invoiceVMs;

                if (list != null)
                {
                    decimal totalAmount = list.GrandTotal;
                    decimal totalBalance = list.Balance;

                    rateMismatch = totalAmount != totalBalance;
                }

                return rateMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for InvoiceId: {invId}");
                throw new InvalidOperationException("Failed to verify Purchase Invoice transactions.", ex);
            }
        }

        public async Task<(List<PurchaseInvoiceVM> invs, int totalCount)> GetPagedInvoiceAsync(int pageNumber, int pageSize, string search)
        {
            try
            {
                var query = _unitOfWork.PurchaseInvoices
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


                var data = _mapper.Map<List<PurchaseInvoiceVM>>(entities);

                return (data, totalCount);

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetSCNDetailsByVendorCode(int VendorCode)
        {
            try
            {
                var query = _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .Where(ps =>
                        ps.PurchaseSCN.VendorCode == VendorCode &&
                        !ps.PurchaseSCN.SCNTally &&
                        ps.PurchaseSCN.Authorized &&
                        !ps.PurchaseSCN.SCNCancel &&
                        !ps.ItemCancel &&
                        ps.BalQty > 0 && !ps.PurchaseSCN.ShortClose
                    )
                    .Select(ps => new
                    {
                        ps.RefPoSubId,
                        ps.SCNSubId,

                        ps.PurchaseSCN.SCNNo,
                        ps.PurchaseSCN.Suffix,
                        ps.PurchaseSCN.SCNDate,
                        ps.PurchaseSCN.MainRemarks,

                        ps.ItemId,
                        ps.Item.ItemCode,
                        ps.Item.ItemName,
                        ps.Item.Specification,
                        ps.Item.MeasureUnit,
                        ps.Item.HSNCode,
                        CategoryName = ps.Item.Category.CategoryName,

                        ps.AcceptQty,
                        ps.BalQty,
                        ps.RejectQty,
                        ps.ReworkQty,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,

                        PoUnitPrice = ps.RefPoSubId > 0 && ps.PurchPoSub != null
                                    ? ps.PurchPoSub.UnitPrice
                                    : ps.UnitPrice,

                        PoLineDiscountPercent = ps.RefPoSubId > 0 && ps.PurchPoSub != null
                                                ? ps.PurchPoSub.LineDiscountPercent
                                                : 0m,

                        PoLineDiscountAmount = ps.RefPoSubId > 0 && ps.PurchPoSub != null
                                                ? ps.PurchPoSub.LineDiscountAmount
                                                : 0m,

                        PoLineCGSTRate = ps.RefPoSubId > 0 && ps.PurchPoSub != null
                                        ? ps.PurchPoSub.LineCGSTRate
                                        : 0m,

                        PoLineSGSTRate = ps.RefPoSubId > 0 && ps.PurchPoSub != null
                                        ? ps.PurchPoSub.LineSGSTRate
                                        : 0m,

                        PoLineIGSTRate = ps.RefPoSubId > 0 && ps.PurchPoSub != null
                                        ? ps.PurchPoSub.LineIGSTRate
                                        : 0m,
                        ps.PurchaseSCN.Vendor
                    });

                var result = await query.ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,

                    ["PoSubId"] = r.RefPoSubId,
                    ["SCNSubId"] = r.SCNSubId,

                    ["SCNNo"] = $"{r.SCNNo}{r.Suffix}",
                    ["SCNDate"] = r.SCNDate,

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? "",
                    ["ItemName"] = r.ItemName ?? "",
                    ["Specification"] = r.Specification ?? "",
                    ["UOM"] = r.MeasureUnit ?? "",
                    ["HSNCode"] = r.HSNCode ?? "",
                    ["CategoryName"] = r.CategoryName ?? "",

                    ["Qty"] = r.AcceptQty + r.RejectQty + r.ReworkQty,


                    ["BalQty"] = r.BalQty,

                    ["RejectQty"] = r.RejectQty,
                    ["RewQty"] = r.ReworkQty,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? "",
                    ["Remark"] = r.MainRemarks ?? "",

                    // PO Line Values
                    ["PoUnitPrice"] = r.PoUnitPrice,
                    ["PoLineDiscountPercent"] = r.PoLineDiscountPercent,
                    ["PoLineDiscountAmount"] = r.PoLineDiscountAmount,
                    ["PoLineCGSTRate"] = r.PoLineCGSTRate,
                    ["PoLineSGSTRate"] = r.PoLineSGSTRate,
                    ["PoLineIGSTRate"] = r.PoLineIGSTRate,
                    ["Vendor"] = r.Vendor.VendorName,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching SCN details for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve SCN details. Please try again.");
            }
        }


        public async Task<bool> IsInvoiceExistsForVendorAsync(int vendorCode)
        {
            try
            {
                if (vendorCode <= 0)
                    return false;

                return await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .AnyAsync(x => x.VendorCode == vendorCode);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"IsInvoiceExistsForVendorAsync(VendorCode: {vendorCode})");
                return false;
            }
        }

        public async Task LoadPreviousInvoiceHeaderAsync(PurchaseInvoiceVM vm)
        {
            try
            {
                if (vm == null || !vm.VendorCode.HasValue || vm.VendorCode.Value <= 0)
                    return;

                var prevInv = await _unitOfWork.PurchaseInvoices
                    .GetQueryable()
                    .Where(x =>
                        x.VendorCode == vm.VendorCode &&
                        x.InvId != vm.InvId)
                    .OrderByDescending(x => x.InvDate)
                    .ThenByDescending(x => x.InvId)
                    .Select(x => new
                    {
                        x.DiscAmtOrPer,
                        x.DiscountPercent,
                        x.DiscountAmount,
                        x.FreightCharges,

                        x.PackingAmtOrPer,
                        x.PackingPercent,
                        x.PackingCharges,

                        x.InsuranceAmtOrPer,
                        x.InsurancePercent,
                        x.InsuranceCharges,

                        x.CGstRate,
                        x.SGstRate,
                        x.IGstRate,

                        x.OtherCharges,

                        x.TCSAmtOrPer,
                        x.TCSPercent,
                        x.TCSAmount
                    })
                    .FirstOrDefaultAsync();

                if (prevInv == null)
                    return;

                // 🔥 Apply header values
                vm.DiscAmtOrPer = prevInv.DiscAmtOrPer;
                vm.DiscountPercent = prevInv.DiscountPercent;
                vm.DiscountAmount = prevInv.DiscountAmount;
                vm.FreightCharges = prevInv.FreightCharges;

                vm.PackingAmtOrPer = prevInv.PackingAmtOrPer;
                vm.PackingPercent = prevInv.PackingPercent;
                vm.PackingCharges = prevInv.PackingCharges;

                vm.InsuranceAmtOrPer = prevInv.InsuranceAmtOrPer;
                vm.InsurancePercent = prevInv.InsurancePercent;
                vm.InsuranceCharges = prevInv.InsuranceCharges;

                vm.CGstRate = prevInv.CGstRate;
                vm.SGstRate = prevInv.SGstRate;
                vm.IGstRate = prevInv.IGstRate;

                vm.OtherCharges = prevInv.OtherCharges;

                vm.TCSAmtOrPer = prevInv.TCSAmtOrPer;
                vm.TCSPercent = prevInv.TCSPercent;
                vm.TCSAmount = prevInv.TCSAmount;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"LoadPreviousInvoiceHeaderAsync(VendorCode: {vm?.VendorCode}, InvId: {vm?.InvId})");
            }
        }

        public async Task<List<int>> GetPOIdsByPOSubIdsAsync(List<int> poSubIds)
        {
            try
            {
                if (poSubIds == null || !poSubIds.Any())
                    return new List<int>();

                return await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(x =>
                        poSubIds.Contains(x.PoSubId)
                    )
                    .Select(x => x.PoId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetPOIdsByPOSubIdsAsync(POSubIds: {string.Join(",", poSubIds ?? new List<int>())})");
                return new List<int>();
            }
        }


        public async Task LoadPOHeaderToInvoiceAsync(PurchaseInvoiceVM vm, int poId)
        {
            try
            {
                if (vm == null || poId <= 0)
                    return;

                var po = await _unitOfWork.PurchPos
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(x => x.PoId == poId)
                    .Select(x => new
                    {
                        x.DiscAmtOrPer,
                        x.DiscountPercent,
                        x.DiscountAmount,
                        x.FreightCharges,
                        x.PackingAmtOrPer,
                        x.PackingPercent,
                        x.PackingCharges,
                        x.InsuranceAmtOrPer,
                        x.InsurancePercent,
                        x.InsuranceCharges,
                        x.CGstRate,
                        x.SGstRate,
                        x.IGstRate,
                        x.TCSAmtOrPer,
                        x.TCSPercent,
                        x.TCSAmount,
                        x.OtherCharges
                    })
                    .FirstOrDefaultAsync();

                if (po == null)
                    return;

                // 🔥 Apply PO header values to Invoice
                vm.DiscAmtOrPer = po.DiscAmtOrPer;
                vm.DiscountPercent = po.DiscountPercent;
                vm.DiscountAmount = po.DiscountAmount;

                vm.FreightCharges = po.FreightCharges;

                vm.PackingAmtOrPer = po.PackingAmtOrPer;
                vm.PackingPercent = po.PackingPercent;
                vm.PackingCharges = po.PackingCharges;

                vm.InsuranceAmtOrPer = po.InsuranceAmtOrPer;
                vm.InsurancePercent = po.InsurancePercent;
                vm.InsuranceCharges = po.InsuranceCharges;

                vm.CGstRate = po.CGstRate;
                vm.SGstRate = po.SGstRate;
                vm.IGstRate = po.IGstRate;

                vm.TCSAmtOrPer = po.TCSAmtOrPer;
                vm.TCSPercent = po.TCSPercent;
                vm.TCSAmount = po.TCSAmount;

                vm.OtherCharges = po.OtherCharges;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"LoadPOHeaderToInvoiceAsync(POId: {poId})");
            }
        }

        public async Task<PurchaseInvoiceVM?> GetPurchaseInvoiceByIdAsync(int invId)
        {
            try
            {
                var entity = await _unitOfWork.PurchaseInvoices
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()

                    .Include(p => p.Vendor)
                    .Include(p => p.Currency)

                    .Include(p => p.PurchaseInvoiceSubs)
                        .ThenInclude(s => s.Item)
                            .ThenInclude(i => i.Category)

                    .Include(p => p.PurchaseInvoiceSubs)
                        .ThenInclude(s => s.PurchaseSCNSub)
                            .ThenInclude(sc => sc.PurchaseSCN)
                    .Include(j => j.PurchaseInvoiceSubs).ThenInclude(ps => ps.DebitNoteSubs).ThenInclude(p => p.DebitNote)
                    .FirstOrDefaultAsync(p => p.InvId == invId);

                if (entity == null)
                    return null;

                return _mapper.Map<PurchaseInvoiceVM>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetPurchaseInvoiceByIdAsync({invId})");
                throw;
            }
        }


        public async Task<List<PurchaseInvoiceSub>> GetInvSubByInvIdAsync(int InvId)
        {
            try
            {
                var subs = await _unitOfWork.PurchaseInvoiceSubs
                                .GetQueryable()
                                .Where(s => s.InvId == InvId)
                                .OrderBy(s => s.SlNo)
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

        public async Task<PurchaseInvoice?> GetLastInvAsync(int VendorCode)
        {
            try
            {
                return await _unitOfWork.PurchaseInvoices.GetLatestAsync(
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
            return await _unitOfWork.PurchaseSCNs
                .GetQueryable()
                .Where(e => e.VendorCode == VendorCode && e.SCNTally == false)
                .CountAsync();

        }

        public async Task<bool> DeletePurchaseInvoiceByIdAsync(int InvId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var invoice = await _unitOfWork.PurchaseInvoices
                    .GetQueryable()
                    .Include(e => e.PurchaseInvoiceSubs)
                    .FirstOrDefaultAsync(e => e.InvId == InvId);

                if (invoice == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in invoice.PurchaseInvoiceSubs)
                {
                    if (sub.RefSCNSubId > 0)
                    {
                        await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, sub.Qty, 0, invoice.IsAcptRejRewQtyRequired, "Purchase Invoice Update");
                    }
                }

                await _unitOfWork.PurchaseInvoices.DeleteAsync(invoice);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase Invoice",
                    action: $"Deleted Purchase Invoice: {invoice.InvNo}",
                    additionalInfo: $"Invoice Id: {invoice.InvId}\n{changes}"
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

        private async Task AdjustSCNSubBalanceAsync(int? refSCNSubId, decimal oldQty, decimal newQty, bool allowRejectRework, string context)
        {
            try
            {
                if (!refSCNSubId.HasValue || refSCNSubId == 0)
                    return;

                var scnSub = await _unitOfWork.PurchaseSCNSubs.GetAsync(refSCNSubId.Value);
                if (scnSub == null)
                    return;

                decimal BalQty = scnSub.BalQty;
                decimal RejectQty = scnSub.RejectQty;
                decimal ReworkQty = scnSub.ReworkQty;

                if (oldQty > 0)
                {
                    decimal restoreAccepted = Math.Min(oldQty, scnSub.AcceptQty);
                    BalQty += restoreAccepted;
                }

                decimal maxAllowedQty = allowRejectRework
                                        ? BalQty + RejectQty + ReworkQty
                                        : BalQty;

                if (newQty > maxAllowedQty)
                    throw new InvalidOperationException($"{context}: Qty exceeds SCN allowed quantity ({maxAllowedQty}).");

                decimal acceptedConsumed = Math.Min(newQty, BalQty);
                BalQty -= acceptedConsumed;

                scnSub.BalQty = BalQty;
                await _unitOfWork.PurchaseSCNSubs.UpdateAsync(scnSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == scnSub.SCNId && !x.ItemCancel)
                    .SumAsync(x => x.BalQty);

                var scn = await _unitOfWork.PurchaseSCNs.GetAsync(scnSub.SCNId);
                if (scn != null)
                {
                    scn.SCNTally = (totalBalQty == 0);
                    await _unitOfWork.PurchaseSCNs.UpdateAsync(scn);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustSCNSubBalanceAsync] Error in {context}");
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
                var isInvoiceCancelled = await _unitOfWork.PurchaseInvoices
                    .AnyAsync(q => q.InvId == InvId && q.InvCancel == true);

                var isItemCancelled = await _unitOfWork.PurchaseInvoiceSubs
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
            return await _unitOfWork.PurchaseInvoices
                .GetQueryable()
                .Where(e => e.InvId == InvId)
                .AnyAsync(e =>
                    e.InvCancel == true ||
                    e.PurchaseInvoiceSubs.Any(s => s.ItemCancel == true)
                );
        }
        public async Task<PurchaseSCNSubVM> GetSCNQtyDetailsFromSCNSubId(int SCNSubId)
        {
            try
            {
                return await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .Where(e => e.SCNSubId == SCNSubId)
                    .Select(e => new PurchaseSCNSubVM
                    {
                        AcceptQty = e.AcceptQty,
                        BalQty = e.BalQty,
                        RejectQty = e.RejectQty,
                        ReworkQty = e.ReworkQty
                    })
                    .FirstOrDefaultAsync() ?? new PurchaseSCNSubVM();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error fetching SCN Qty Details for SCNSubId: {SCNSubId}");
                throw new InvalidOperationException("Failed to retrieve SCN quantity details.");
            }
        }


        public async Task<decimal> GetExistingPurchInvoiceQtyByInvSubId(int InvSubId)
        {
            try
            {
                return await _unitOfWork.PurchaseInvoiceSubs.GetQueryable()
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

        public async Task<PurchaseInvoiceSubVM?> GetInvSubItemDetailByInvSubIdAsync(int InvSubId)
        {
            try
            {
                return await _unitOfWork.PurchaseInvoiceSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(q => q.InvSubId == InvSubId)
                    .Select(q => new PurchaseInvoiceSubVM
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
                return await _unitOfWork.PurchaseSCNSubs.GetQueryable()
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

       
        public async Task<List<PurchaseInvoiceSubVM>> GetInvoiceSubByInvIdAsync(int InvId)
        {
            try
            {
                var subs = await _unitOfWork.PurchaseInvoiceSubs
                                .GetQueryable()
                                .Where(s => s.InvId == InvId)
                                .OrderBy(s => s.SlNo)
                                .ProjectTo<PurchaseInvoiceSubVM>(_mapper.ConfigurationProvider)
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

        public async Task UpsertPurchaseInvShortCloseAsync(PurchaseInvoiceVM purchaseInvoiceVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingInv = await _unitOfWork.PurchaseInvoices.GetAsync(purchaseInvoiceVM.InvId);
                if (existingInv == null)
                    throw new InvalidOperationException("Purchase Invoice not found.");

                existingInv.ShortClose = purchaseInvoiceVM.ShortClose;

                await _unitOfWork.PurchaseInvoices.UpdateAsync(existingInv);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertPurchaseInvShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertPurchaseInvShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(PurchaseInvoiceVM InvVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingInv = await _unitOfWork.PurchaseInvoices.GetAsync(InvVM.InvId);
                if (existingInv == null)
                    throw new InvalidOperationException("Purchase Invoice not found.");

                var subs = await _unitOfWork.PurchaseInvoiceSubs
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
                await _unitOfWork.PurchaseInvoices.UpdateAsync(existingInv);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingInv.InvCancel)
                    {
                        if (sub.RefSCNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, sub.Qty, 0, existingInv.IsAcptRejRewQtyRequired, "Purchase Invoice Update");
                        }
                    }
                    else
                    {
                        if (sub.RefSCNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustSCNSubBalanceAsync(sub.RefSCNSubId,  0, sub.Qty, existingInv.IsAcptRejRewQtyRequired, "Purchase Invoice Update");
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

        public async Task ValidatePurchaseSCNBalanceBeforeRevertAsync(PurchaseInvoiceSub sub)
        {
            if (sub.RefSCNSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.PurchaseSCNSubs.GetAsync(sub.RefSCNSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"PurchaseInvoice not found for RefSCNSubId: {sub.RefSCNSubId}");

            if (entity.BalQty < sub.Qty)
            {
                throw new InvalidOperationException(
                    $"Cannot revert because PurchaseInvoice balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
            }
        }

        public async Task DeleteAndResequenceAsync(PurchaseInvoiceSubVM subitem, PurchaseInvoiceVM InvVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.InvSubId > 0)
                {
                    var entity = await _unitOfWork.PurchaseInvoiceSubs.GetAsync(subitem.InvSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.RefSCNSubId > 0)
                    {
                        await AdjustSCNSubBalanceAsync(subitem.RefSCNSubId, entity.Qty, 0, InvVM.IsAcptRejRewQtyRequired, "Purchase Invoice Delete");
                    }

                    await _unitOfWork.PurchaseInvoiceSubs.DeleteAsync(entity.InvSubId);
                    await _unitOfWork.SaveAsync();

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
                    InvVM.PurchaseInvoiceSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.PurchaseInvoiceSubs
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
                return await _unitOfWork.PurchaseInvoices
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
        public async Task AdjustAcceptrejRewQty(PurchaseInvoice entity)
        {
            try
            {
                if (entity != null)
                {
                    foreach (var invsub in entity.PurchaseInvoiceSubs)
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

        public async Task<PurchaseInvoiceVM> UpsertPurchaseInvoice(PurchaseInvoiceVM purchgInvoiceVM, int screenCode)
        {
            if (purchgInvoiceVM == null)
                throw new ArgumentNullException(nameof(purchgInvoiceVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                PurchaseInvoice entity;

                if (purchgInvoiceVM.InvId == 0)
                {
                    entity = _mapper.Map<PurchaseInvoice>(purchgInvoiceVM);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.Balance = purchgInvoiceVM.GrandTotal;

                    entity.PurchaseInvoiceSubs = purchgInvoiceVM.PurchaseInvoiceSubVMs
                   .Select(s => _mapper.Map<PurchaseInvoiceSub>(s)).ToList();

                    await _unitOfWork.PurchaseInvoices.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.PurchaseInvoiceSubs)
                    {
                        if (sub.RefSCNSubId > 0)
                        {
                            await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, 0, sub.Qty,entity.IsAcptRejRewQtyRequired,"Purchase Invoice Creation");
                        }
                    }

                    changes.AppendLine("Purchase Invoice Created.");
                }
                else
                {
                    entity = await _unitOfWork.PurchaseInvoices.GetQueryable()
                        .Include(q => q.PurchaseInvoiceSubs)
                        .FirstOrDefaultAsync(q => q.InvId == purchgInvoiceVM.InvId)
                        ?? throw new InvalidOperationException("Purchase Invoice found.");

                    var parentChanges = GetPropertyChanges(entity, purchgInvoiceVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(purchgInvoiceVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, purchgInvoiceVM.PurchaseInvoiceSubVMs, changes, screenCode);

                    changes.AppendLine("Purchase Invoice Updated.");
                }

                await _unitOfWork.SaveAsync();

                // ===== TDS Deduction =====
                var tdsInfo = await _unitOfWork.Vendors.GetQueryable()
                    .Where(v => v.VendorCode == entity.VendorCode)
                    .Select(v => new { v.TdsDeduct })
                    .FirstOrDefaultAsync();

                if (tdsInfo?.TdsDeduct == true)
                {
                    await AutoDeductTDSForVendorAsync(entity);
                }

                await transaction.CommitAsync();

                await LogChangesAsync(changes, purchgInvoiceVM.InvId == 0 ? "Purchase Invoice Created" : "Purchase Invoice  Updated");

                var savedEntity = await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.PurchaseInvoiceSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                    .Include(q => q.Vendor)
                    .FirstOrDefaultAsync(q => q.InvId == entity.InvId);

                return _mapper.Map<PurchaseInvoiceVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Purchase Invoice: {purchgInvoiceVM.InvNo}");
                throw new InvalidOperationException("Failed to save Purchase Invoice. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(PurchaseInvoice existingInvoice, List<PurchaseInvoiceSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingInvoice.PurchaseInvoiceSubs.Select(s => s.InvSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.InvSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingInvoice.PurchaseInvoiceSubs.Where(s => !incomingSubIds.Contains(s.InvSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - InvSubId: {sub.InvSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.PurchaseInvoiceSubs.DeleteAsync(sub.InvSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefSCNSubId > 0)
                {
                    await AdjustSCNSubBalanceAsync(sub.RefSCNSubId, sub.Qty, 0, existingInvoice.IsAcptRejRewQtyRequired, "Purchase Invoice delete");
                }
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                decimal.TryParse((subVM.Qty.Value).ToString(), out decimal qty);
                if (subVM.InvSubId == 0)
                {
                    var newSub = _mapper.Map<PurchaseInvoiceSub>(subVM);
                    newSub.InvId = existingInvoice.InvId;
                    await _unitOfWork.PurchaseInvoiceSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");
                    if (subVM.RefSCNSubId > 0)
                    {
                        await AdjustSCNSubBalanceAsync(subVM.RefSCNSubId,  0, subVM.Qty.GetValueOrDefault(), existingInvoice.IsAcptRejRewQtyRequired, "Purchase Invoice Creation");
                    }
                }
                else
                {
                    var existingSub = existingInvoice.PurchaseInvoiceSubs.FirstOrDefault(s => s.InvSubId == subVM.InvSubId);
                    if (existingSub != null)
                    {

                        if (subVM.RefSCNSubId > 0)
                        {
                            await AdjustSCNSubBalanceAsync(subVM.RefSCNSubId, existingSub.Qty, subVM.Qty.GetValueOrDefault(), existingInvoice.IsAcptRejRewQtyRequired, "Purchase Invoice Update");
                        }

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        public async Task<List<PurchaseInvoiceSubVM>> GetDistinctRefSCNByPurchaseInvIdAsync(int invId)
        {
            return await _unitOfWork.PurchaseInvoiceSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(s => s.InvId == invId)
                        .Include(s => s.PurchaseSCNSub)
                            .ThenInclude(g => g.PurchaseSCN)
                        .Where(s => s.PurchaseSCNSub != null &&
                                    s.PurchaseSCNSub.PurchaseSCN != null)
                        .GroupBy(s => new
                        {
                            s.PurchaseSCNSub.PurchaseSCN.Suffix,
                            s.PurchaseSCNSub.PurchaseSCN.SCNNo,
                            s.PurchaseSCNSub.PurchaseSCN.SCNDate
                        })
                        .Select(g => new PurchaseInvoiceSubVM
                        {
                            RefSCNNo = $"{g.Key.SCNNo}{g.Key.Suffix}",
                            RefSCNDate = g.Key.SCNDate
                        })
                        .ToListAsync();
        }

        public async Task<(bool CanDelete, string Message)> CanDeletePurchaseInvAsync(int invId)
        {
            try
            {
                var invoice = await _unitOfWork.PurchaseInvoices
                                .GetQueryable()
                                .Include(e => e.PurchaseInvoiceSubs)
                                .Where(e => e.InvId == invId).FirstOrDefaultAsync();

                if (invoice.Balance != invoice.GrandTotal)
                    return (false, "Cannot delete this Invoice as it is some transaction made.");

                if (invoice.InvCancel || invoice.ShortClose)
                    return (false, "Cannot delete this Purchase Invoice as it is Cancelled or Short Closed.");

                return (true, "purchase Invoice can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeletePurchaseInvAsync for InvId: {invId}");
                throw new Exception("Error checking Purchase invoice delete eligibility", ex);
            }
        }

        public async Task<(List<PurchaseInvoiceVM> invVms, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.PurchaseInvoices
                .GetQueryable()
                .AsSplitQuery()
                .Include(e => e.Vendor)
                .Include(e => e.PurchaseInvoiceSubs)
                    .ThenInclude(s => s.Item)
                .Include(e => e.PurchaseInvoiceSubs)
                    .ThenInclude(s => s.PurchaseSCNSub)
                        .ThenInclude(g => g.PurchaseSCN)
                .Include(e => e.PurchaseInvoiceSubs)
                    .ThenInclude(s => s.CostCenter)
                .AsQueryable();

            if (filters != null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    query = PurchaseInvFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.InvId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vmList = _mapper.Map<List<PurchaseInvoiceVM>>(list);

            return (vmList, total);
        }


        public static class PurchaseInvFilterBuilder
        {
            public static IQueryable<PurchaseInvoice> ApplyFilter(
                IQueryable<PurchaseInvoice> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                //new("InvNo", "Invoice No", "text"),
                // new("Vendor", "Vendor", "text"),
                // new("ItemCode", "Item Code", "text"),
                // new("ItemName", "Item Name", "text"),
                // new("SCNNo", "SCN No.", "text"),
                // new("Status", "Status", "dropdown", new() { "Completed", "Pending", "Cancelled", "Short Closed" }),
                // new("CreatedBy", "Created By", "text"),
                // new("ModifieBy", "Modified By", "text"),
                // new("FromDate", "From Date", "date"),
                // new("ToDate", "To Date", "date")

                switch (field)
                {
                    case "InvNo":
                        {
                            var input = val.ToString()?.Trim();
                            if (string.IsNullOrEmpty(input))
                                return query;

                            string part1 = input;
                            string part2 = "";

                            int slashIndex = input.IndexOf('/');

                            if (slashIndex > -1)
                            {
                                part1 = input.Substring(0, slashIndex).Trim();
                                part2 = input.Substring(slashIndex).Trim();
                            }

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.InvNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "Vendor":
                        return query.Where(x => x.Vendor.VendorName.Contains(val));

                    case "ItemName":
                        return query.Where(x => x.PurchaseInvoiceSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "ItemCode":
                        return query.Where(x => x.PurchaseInvoiceSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "SCNNo":
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
                                x.PurchaseInvoiceSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.PurchaseSCNSub.PurchaseSCN.SCNNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.PurchaseSCNSub.PurchaseSCN.Suffix.Contains(part2))
                                ));
                        }

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "ModifiedBy":
                        return query.Where(x => x.ModifiedBy.Contains(val));

                    case "FromDate":
                        return query.Where(x => x.InvDateNow >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.InvDateNow <= DateTime.Parse(value.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<PurchaseInvoice> ApplyStatusFilter(
                IQueryable<PurchaseInvoice> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.InvTally == true),
                    "Short Closed" => query.Where(x => x.ShortClose == true),
                    "Cancelled" => query.Where(x => x.InvCancel == true),
                    "Pending" => query.Where(x => x.InvTally == false && x.InvCancel == false && x.ShortClose == false),
                    _ => query
                };
            }
        }

        #endregion


        #region Auto TDS Deducts
        //-------TDS----------------
        public async Task<bool> UpdateTDSAmountAsync(PurchaseInvoiceVM purchgInvoiceVM)
        {
            try
            {
                var changes = new StringBuilder();

                PurchaseInvoice entity;

                entity = await _unitOfWork.PurchaseInvoices.GetAsync(purchgInvoiceVM.InvId);

                if (entity == null)
                    return false;

                var parentChanges = GetPropertyChanges(entity, purchgInvoiceVM);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                entity.TDSAmount = purchgInvoiceVM.TDSAmount;
                entity.Balance = (entity.GrandTotal) - purchgInvoiceVM.TDSAmount;

                if (entity.Balance < 0)
                    entity.Balance = 0;

                await _unitOfWork.PurchaseInvoices.UpdateAsync(entity);
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

        public async Task<bool> AutoDeductTDSForVendorAsync(PurchaseInvoice invoice)
        {
            try
            {
                var vendor = await _unitOfWork.Vendors.GetQueryable()
                    .FirstOrDefaultAsync(v => v.VendorCode == invoice.VendorCode);

                if (vendor == null || !vendor.TdsDeduct)
                    return false;

                decimal exemptAmt = vendor.TDSExmptdAmt;
                decimal tdsRate = vendor.TDSDeductper;
                decimal maxBill = vendor.TDSBillval;

                if (tdsRate <= 0)
                    return false;

                // 🔹 Check if any previous invoice already deducted TDS
                bool hasPreviousTDS = await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .AnyAsync(x => x.VendorCode == invoice.VendorCode && x.TDSAmount > 0);

                if (hasPreviousTDS)
                {
                    ApplyTDS(invoice, tdsRate);
                    await _unitOfWork.PurchaseInvoices.UpdateAsync(invoice);
                    await _unitOfWork.SaveAsync();
                    return true;
                }

                // 🔹 Calculate total purchase INCLUDING current invoice
                decimal totalPurchase = await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .Where(x => x.VendorCode == invoice.VendorCode)
                    .SumAsync(x => x.TotalTaxable);

                bool thresholdCrossed = totalPurchase >= exemptAmt || (maxBill > 0 && invoice.TotalTaxable >= maxBill);

                if (!thresholdCrossed)
                    return true;

                // 🔹 Apply TDS to ALL vendor invoices
                var invoices = await _unitOfWork.PurchaseInvoices.GetQueryable()
                    .Where(x => x.VendorCode == invoice.VendorCode)
                    .ToListAsync();

                foreach (var inv in invoices)
                {
                    ApplyTDS(inv, tdsRate);
                    await _unitOfWork.PurchaseInvoices.UpdateAsync(inv);
                }

                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in AutoDeductTDSForVendorAsync");
                return false;
            }
        }

        private void ApplyTDS(PurchaseInvoice invoice, decimal tdsRate)
        {
            if (invoice == null || tdsRate <= 0)
                return;

            invoice.TDSAmount = Math.Round(invoice.TotalTaxable * tdsRate / 100, 2);
            invoice.Balance = invoice.GrandTotal - invoice.TDSAmount;

            if (invoice.Balance < 0)
                invoice.Balance = 0;
        }


        #endregion


        public async Task<bool> HasAnyItemOrInvoiceDebitNoteAsync(int RefSubInvId)
        {
            try
            {
                var isExist = await (from inv in _unitOfWork.DebitNotes.GetQueryable()
                                     join sub in _unitOfWork.DebitNoteSubs.GetQueryable()
                                     on inv.DbId equals sub.DbId
                                     where sub.RefPurchInvSubId == RefSubInvId && inv.Purchase == true
                                     select inv.DbId).AnyAsync();

                return isExist;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrInvoiceDebitNoteAsync for RefSubInvId: {RefSubInvId}");
                throw;
            }
        }

        public async Task<List<PurchaseInvoiceStatusVM>> GetPurchaseInvoicePendingListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<PurchaseInvoiceStatusVM>("Sp_GetPurchaseInvoiceStatusList", status);
                return result.ToList();

            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message);
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
                        c.ReferenceType == "Purchase Invoice" &&
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
