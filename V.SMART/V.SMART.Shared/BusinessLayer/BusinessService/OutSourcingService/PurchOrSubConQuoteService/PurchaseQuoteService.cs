using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using FastReport;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPurchaseService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.PurchAndSubConViewModel.Purch_QuotationVM;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.PurchaseService
{
    public class PurchaseQuoteService : IPurchaseQuoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public PurchaseQuoteService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }


        // 🔹 Vendors
        public async Task<List<VendorVM>> GetVendorAsync(int? VendorCode = null)
        {
            if (VendorCode.HasValue && VendorCode.Value > 0)
            {
                var vendor = await _commonService.GetVendorByVenerCodeAsync(VendorCode.Value);
                return vendor != null ? new List<VendorVM> { vendor } : new List<VendorVM>();
            }
            return await _commonService.GetAllActiveVendorsAsync();
        }

        public async Task<IEnumerable<VendorVM>> SearchVendorAsync(string searchText)
        {
            return await _commonService.SearchVendorsAsync(searchText);
        }

        public async Task<VendorVM?> GetVendorByIdAsync(int vendorCode)
            => await _commonService.GetVendorByVenerCodeAsync(vendorCode);

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        // 🔹 Terms
        public async Task<List<TermsAndConditions>> GetTermsAsync()
          => await _commonService.GetAllActiveTermsAsync();

        // 🔹 Currency
        public async Task<List<Currency>> GetCurrenciesAsync()
            => (await _commonService.GetCurrenciesAsync()).ToList();

        public async Task<Currency?> GetCurrencyByIdAsync(int currId)
           => (await _commonService.GetCurrencyByIdAsync(currId));

        // 🔹 Get latest currency rate (from CurrencyToday Service)
        public async Task<decimal?> GetLatestCurrencyValueAsync(int currId)
            => await _commonService.GetLatestCurrencyValueAsync(currId);

        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        // 🔹 Contacts
        public async Task<List<VendorContact>> GetVenorContactPersonsAsync(int VenoderCode)
           => await _commonService.GetContactPersonsVendorAsync(VenoderCode);

        // 🔹 Consignee addresses
        public async Task<List<VendorInDirect>> GetConsigneeAddressesAsync(int VenoderCode)
            => await _commonService.GetConsigneeAddressesVendorAsync(VenoderCode);


        // CostCeneter
        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        // 🔹 Item Attachment details
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
           => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        // 🔹 MyCompanyDetails
        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        // 🔹 Quotation operations


        public async Task<bool> IsQuoteTransactionsMatchedAsync(int quoteId, PurchaseQuoteVM purchQuoteVms)
        {
            try
            {
                var quoteSubIds = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(x => x.QuoteId == quoteId)
                    .Select(x => x.QuoteSubId)
                    .ToListAsync();

                bool hasQuote = quoteSubIds.Any();

                bool hasTransactions = false;

                if (hasQuote)
                {
                    hasTransactions = await _unitOfWork.PurchPoSubs
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.RefQuoteSubId.HasValue &&
                            quoteSubIds.Contains(pqs.RefQuoteSubId.Value));
                }

                bool qtyMismatch = false;

                var list = purchQuoteVms?.PurchaseQuoteSubVM;
                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.Qty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty ?? 0);

                    qtyMismatch = totalQty != totalBalQty;
                }

                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for QuoteId: {quoteId}");
                throw new InvalidOperationException("Failed to verify enquiry transactions.", ex);
            }
        }


        public async Task<int> GetPendingEnquiryCountAsync(int vendorCode)
        {
            return await (from ev in _unitOfWork.EnquiryPurchaseVendorAssigns.GetQueryable()
                          join e in _unitOfWork.EnquiryPurchases.GetQueryable()
                              on ev.EnquiryId equals e.EnquiryId
                          where ev.VendorCode == vendorCode
                                && e.EnquiryTally == false
                          select ev
                        ).CountAsync();
        }

        public async Task<PurchaseQuoteVM> GetQuotationByQuoteIdAsync(int quoteId)
        {
            try
            {
                var entity = await _unitOfWork.PurchaseQuotes.GetQueryable()
                    .Include(q => q.PurchaseQuoteSub)
                    .Include(q => q.PurchaseQuoteSub).ThenInclude(s => s.Item)
                    .Include(q => q.PurchaseQuoteSub).ThenInclude(s => s.CostCenter)
                    .Include(q => q.PurchaseQuoteSub).ThenInclude(s => s.EnquiryPurchaseVendorAssign).ThenInclude(s=> s.Enquiry)
                    .Include(q => q.Vendor).ThenInclude(c => c.VendorInDirects)
                    .Include(q => q.Currency)
                    .FirstOrDefaultAsync(q => q.QuoteId == quoteId);

                return _mapper.Map<PurchaseQuoteVM>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetQuotationAsync({quoteId})");
                return null;
            }
        }

        public async Task<PurchaseQuote?> GetLastQuoteAsync(int VendorCode)
        {
            try
            {
                return await _unitOfWork.PurchaseQuotes.GetLatestAsync(
                    q => q.VendorCode == VendorCode,
                    q => q.QuoteId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastQuoteAsync for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve last purchase quotation. Please try again.");
            }
        }

        public async Task<IEnumerable<PurchaseQuoteVM>> GetAllQuoteAsync()
        {
            try
            {
                var entities = await _unitOfWork.PurchaseQuotes.GetAllWithIncludeAsync(q => true,
                    q => q.Vendor, q => q.PurchaseQuoteSub);

                var vmList = _mapper.Map<IEnumerable<PurchaseQuoteVM>>(entities);

                foreach (var vm in vmList)
                {
                    var pendingExists = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .AnyAsync(s => s.QuoteId == vm.QuoteId && !s.ItemShortClose);

                    //vm.ItemStatus = pendingExists ? "Completed" : "Pending";

                }

                return vmList;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllQuoteAsync");
                return Enumerable.Empty<PurchaseQuoteVM>();
            }
        }

        public async Task<bool> IsDuplicateQuoteAsync(string quoteNo, string suffix, int VendorCode, int? quotId = null)
        {
            try
            {
                return await _unitOfWork.PurchaseQuotes
                    .AnyAsync(e =>
                        e.QuoteNo == quoteNo && e.Suffix == suffix &&
                        (!quotId.HasValue || e.QuoteId != quotId) &&
                        (e.VendorCode == VendorCode));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to check duplicate Quotation No '{quoteNo}'");
                throw;
            }
        }
        public async Task<PurchaseQuoteVM> UpsertQuoteAsync(PurchaseQuoteVM quoteVM)
        {
            if (quoteVM == null)
                throw new ArgumentNullException(nameof(quoteVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                PurchaseQuote entity;

                if (quoteVM.QuoteId == 0)
                {
                    entity = _mapper.Map<PurchaseQuote>(quoteVM);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.PurchaseQuoteSub = quoteVM.PurchaseQuoteSubVM.Select(s => _mapper.Map<PurchaseQuoteSub>(s)).ToList();

                    await _unitOfWork.PurchaseQuotes.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in quoteVM.PurchaseQuoteSubVM)
                    {
                        if (subVM.RefEnqVendorAssignId > 0)
                        {
                            await AdjustEnquiryBalanceAsync(subVM.RefEnqVendorAssignId, 0, subVM.Qty ?? 0, "Quote Creation");
                        }
                    }

                    changes.AppendLine("Quotation Created.");
                }
                else
                {
                    entity = await _unitOfWork.PurchaseQuotes.GetQueryable()
                        .Include(q => q.PurchaseQuoteSub)
                        .FirstOrDefaultAsync(q => q.QuoteId == quoteVM.QuoteId)
                        ?? throw new InvalidOperationException("Quotation not found.");

                    var parentChanges = GetPropertyChanges(entity, quoteVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(quoteVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, quoteVM.PurchaseQuoteSubVM, changes);

                    var sums = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(x => x.QuoteId == entity.QuoteId)
                    .GroupBy(x => 1)
                    .Select(g => new
                    {
                        TotalBalQty = g.Sum(s => s.BalQty)
                    })
                    .FirstOrDefaultAsync();

                    entity.QuotationTally = sums == null && sums.TotalBalQty == 0;


                    changes.AppendLine("Quotation Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateQuoteTallyStatusAsync(quoteVM.QuoteId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, quoteVM.QuoteId == 0 ? "Quotation Created" : "Quotation Updated");

                var savedEntity = await _unitOfWork.PurchaseQuotes.GetQueryable()
                    .Include(q => q.PurchaseQuoteSub).ThenInclude(s => s.Item)
                    .Include(q => q.Vendor)
                    .Include(q => q.Currency)
                    .Include(q => q.PurchaseQuoteSub).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.QuoteId == entity.QuoteId);

                return _mapper.Map<PurchaseQuoteVM>(savedEntity!);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert quotation: {quoteVM.QuoteNo}");
                throw new InvalidOperationException("Failed to save quotation. Please try again.");
            }
        }
        private async Task HandleChildUpdatesAsync(PurchaseQuote existingQuote, List<PurchaseQuoteSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {
                var existingSubIds = existingQuote.PurchaseQuoteSub.Select(s => s.QuoteSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.QuoteSubId).ToHashSet();

                // DELETE removed children
                foreach (var sub in existingQuote.PurchaseQuoteSub.Where(s => !incomingSubIds.Contains(s.QuoteSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - QuoteSubId: {sub.QuoteSubId}, Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.PurchaseQuotesSubs.DeleteAsync(sub.QuoteSubId);
                    await _unitOfWork.SaveAsync();

                    if (sub.RefEnqVendorAssignId > 0)
                        await AdjustEnquiryBalanceAsync(sub.RefEnqVendorAssignId, sub.Qty, 0, "Quote Deletion");

                }

                // ADD or UPDATE children
                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.QuoteSubId == 0)
                    {
                        var newSub = _mapper.Map<PurchaseQuoteSub>(subVM);
                        newSub.QuoteId = existingQuote.QuoteId;
                        await _unitOfWork.PurchaseQuotesSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                        if (subVM.RefEnqVendorAssignId > 0)
                        {
                            await AdjustEnquiryBalanceAsync(subVM.RefEnqVendorAssignId, 0, subVM.Qty ?? 0, "Quote Creation");
                        }

                    }
                    else
                    {
                        var existingSub = existingQuote.PurchaseQuoteSub.FirstOrDefault(s => s.QuoteSubId == subVM.QuoteSubId);
                        if (existingSub != null)
                        {
                            if (subVM.RefEnqVendorAssignId > 0)
                                await AdjustEnquiryBalanceAsync(subVM.RefEnqVendorAssignId, existingSub.Qty, subVM.Qty ?? 0, "Quote Update");

                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to upsert quotation:HandleChildUpdatesAsync ");
                throw new InvalidOperationException("Failed to save quotation. Please try again.");
            }
        }

        private async Task AdjustEnquiryBalanceAsync(int? refEnqAssignId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refEnqAssignId.HasValue || refEnqAssignId == 0) return;

                var enquiryAssign = await _unitOfWork.EnquiryPurchaseVendorAssigns.GetAsync(refEnqAssignId.Value);
                if (enquiryAssign == null) return;

                if (oldQty > 0)
                    enquiryAssign.BalQty += oldQty;

                if (newQty > enquiryAssign.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed  Enquiry Assign. BalQty.");

                if (newQty > 0)
                    enquiryAssign.BalQty -= newQty;

                if (enquiryAssign.BalQty <= 0)
                {
                    enquiryAssign.ItemStatus = 2;
                }
                else
                {
                    enquiryAssign.ItemStatus = 1;
                }


                await _unitOfWork.EnquiryPurchaseVendorAssigns.UpdateAsync(enquiryAssign);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetQueryable()
                    .Where(e => e.EnquiryId == enquiryAssign.EnquiryId && e.ItemStatus <= 2)
                    .SumAsync(e => e.BalQty);

                var enquiry = await _unitOfWork.EnquiryPurchases.GetAsync(enquiryAssign.EnquiryId.Value);
                if (enquiry != null)
                {
                    enquiry.EnquiryTally = (totalBalQty == 0);
                    await _unitOfWork.EnquiryPurchases.UpdateAsync(enquiry);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustEnquiryBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustEnquiryBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Enquiry Assign Balance. Please contact support.");
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

        public async Task DeleteAndResequenceAsync(PurchaseQuoteSubVM subitem, PurchaseQuoteVM quote)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.QuoteSubId > 0)
                {
                    var entity = await _unitOfWork.PurchaseQuotesSubs.GetAsync(subitem.QuoteSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.RefEnqVendorAssignId > 0)
                    {
                        await AdjustEnquiryBalanceAsync(subitem.RefEnqVendorAssignId.Value, entity.Qty, 0, "Quote Deletion");
                    }

                    await _unitOfWork.PurchaseQuotesSubs.DeleteAsync(entity);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Purchase Quote",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Quotation No: {quote?.QuoteNo}"
                    );
                }
                else
                {
                    quote.PurchaseQuoteSubVM.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(x => x.QuoteId == quote.QuoteId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                // Update Quotation Tally Status
                await UpdateQuoteTallyStatusAsync(quote.QuoteId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<bool> DeleteQuotationByQuoteIdAsync(int QuoteId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var quotation = await _unitOfWork.PurchaseQuotes
                    .GetQueryable()
                    .Include(e => e.PurchaseQuoteSub)
                    .FirstOrDefaultAsync(e => e.QuoteId == QuoteId);

                if (quotation == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                foreach (var sub in quotation.PurchaseQuoteSub)
                {
                    if (sub.RefEnqVendorAssignId > 0)
                    {
                        await AdjustEnquiryBalanceAsync(sub.RefEnqVendorAssignId, sub.Qty, 0, "Quote Deletion");
                    }
                }

                var deleted = await _unitOfWork.PurchaseQuotes.DeleteAsync(QuoteId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase Quotation List",
                    action: $"Deleted purchase Quotation: {quotation.QuoteNo}",
                    additionalInfo: $"Quotation Id: {quotation.QuoteId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Purchase Quotation: {QuoteId}");
                throw;
            }
        }

        public async Task ValidateBeforeRevertAsync(int quoteSubId)
        {
            try
            {
                var sub = await _unitOfWork.PurchaseQuotesSubs.GetAsync(quoteSubId);

                if (sub == null)
                    throw new InvalidOperationException("Purchase Enquiry Item not found.");

                if (sub.RefEnqVendorAssignId > 0)
                    await ValidatePurchaseEnquiryBalanceBeforeRevertAsync(sub);

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

        public async Task<List<PurchaseQuoteSub>> GetQuoteSubByQuoteIdAsync(int quoteId)
        {
            try
            {
                var subs = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(s => s.QuoteId == quoteId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching QuoteSub items for QuoteId: {quoteId}");
                throw new InvalidOperationException("Failed to retrieve quote sub-items. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetEnquiryDetailsByVendorCode(int VendorCode, bool purchOrSub)
        {
            try
            {
                var query = _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetQueryable()
                    .Include(evs => evs.EnquiryPurchaseSub)
                        .ThenInclude(es => es.Item)
                    .Include(evs => evs.EnquiryPurchaseSub)
                        .ThenInclude(es => es.CostCenter)
                    .Include(evs => evs.Enquiry)
                    .Include(evs => evs.Vendor)
                    .Where(evs => evs.VendorCode == VendorCode &&
                        !evs.Enquiry.EnquiryTally &&
                        !evs.Enquiry.Cancel &&
                        !evs.Enquiry.EnquiryShortClose &&
                        evs.ItemStatus == 1 &&
                        evs.BalQty > 0 &&
                        evs.Enquiry.PurchORSubCon == purchOrSub
                    )
                    .Select(evs => new
                    {
                        evs.EnqPurchVendorId,
                        es = evs.EnquiryPurchaseSub,
                        e = evs.Enquiry,
                        evs.BalQty
                    });

                var result = await query
                    .Select(x => new
                    {
                        x.EnqPurchVendorId,
                        x.e.EnquiryNo,
                        x.e.Suffix,
                        x.e.EnquiryDate,
                        x.es.ItemId,
                        ItemCode = x.es.Item.ItemCode,
                        ItemName = x.es.Item.ItemName,
                        MeasureUnit = x.es.Item.MeasureUnit,
                        x.es.Qty,
                        x.BalQty,
                        x.es.UnitPrice,
                        x.es.CostCenterId,
                        ProjectNo = x.es.CostCenter.ProjectNo,
                         
                    })
                    .ToListAsync();

                // Convert to dictionary output
                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefEnqPurchVendorId"] = r.EnqPurchVendorId,
                    ["EnquiryNo"] = $"{r.EnquiryNo}{r.Suffix}",
                    ["EnquiryDate"] = r.EnquiryDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["Rate"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId == 0 ? (int?)null : r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                   
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error fetching purchase enquiry details in Purchase Quotation for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve purchase enquiry details. Please try again.");
            }
        }


        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int VendorCode)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                                  join q in _unitOfWork.PurchaseQuotes.GetQueryable() on qs.QuoteId equals q.QuoteId
                                  where qs.ItemId == itemId && q.VendorCode == VendorCode
                                  orderby q.QuoteId descending
                                  select qs.UnitPrice)
                                 .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.QuoteSubId descending
                                      select qs.UnitPrice)
                                     .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId && isub.CustomerId == VendorCode
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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for Vendor: {VendorCode}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }

        public async Task<string> GetQuotationNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.PurchaseQuotes
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.QuoteNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastQuote != null)
                {
                    var parts = lastQuote.QuoteNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating purchase quotation number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate purchase quotation number.");
            }
        }

        public async Task<decimal> GetEnquiryItemBalQtyFromVendorAssignId(int enqAssignId)
        {
            try
            {

                return await _unitOfWork.EnquiryPurchaseVendorAssigns.GetQueryable()
               .Where(e => e.EnqPurchVendorId == enqAssignId)
               .Select(e => e.BalQty)
               .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for EnqVendorAssignId: {enqAssignId}");
                throw new InvalidOperationException("Failed to retrieve enquiry Assign balance quantity.");
            }
        }

        public async Task<PurchaseQuoteSubVM?> GetQuoteSubItemDetailByQuoteSubIdAsync(int quoteSubId)
        {
            try
            {
                return await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(q => q.QuoteSubId == quoteSubId)
                    .Select(q => new PurchaseQuoteSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching quote sub item detail for QuoteSubId: {quoteSubId}");
                throw new InvalidOperationException("Failed to retrieve quote sub-item details.");
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeletePurchaseQuote(int QuoteId)
        {
            try
            {
                var quote = await _unitOfWork.PurchaseQuotes
                                .GetQueryable()
                                .Include(e => e.PurchaseQuoteSub)
                                .Where(e => e.QuoteId == QuoteId).FirstOrDefaultAsync();

                if (quote == null)
                    return (true, "Purchase Quotation can be safely deleted.");

                var quoteSubIds = quote.PurchaseQuoteSub
                    .Select(es => es.QuoteSubId)
                    .ToList();

                bool hasPo = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefQuoteSubId.HasValue &&
                        quoteSubIds.Contains(qs.RefQuoteSubId.Value));

                if (hasPo)
                    return (false, "Cannot delete this Purchase Quotation as a Purchase order transaction exists.");

                if (quote.IsCancel || quote.QuoteShortClose)
                    return (false, "Cannot delete this Purchase Quotation as it is Cancelled or Short Closed.");


                if (quote.PurchaseQuoteSub.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Purchase Quotation as one or more Quotation items are cancelled.");


                return (true, "Purchase Quotation can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeletePurchaseQuote for QuoteId: {QuoteId}");
                throw new Exception("Error checking Purchase Quotation delete eligibility", ex);
            }
        }


        public async Task<(bool CanDelete, string Message)> CanRemoveQuoteAsync(int QuoteId, int QuoteSubId)
        {
            try
            {
                var QuoteSubIds = await _unitOfWork.PurchaseQuotes
                                     .GetQueryable()
                                     .Where(s => s.QuoteId == QuoteId)
                                     .SelectMany(s => s.PurchaseQuoteSub.Select(sub => sub.QuoteSubId))
                                     .ToListAsync();

                if (!QuoteSubIds.Any())
                    return (true, "Purchase Quotation can be safely deleted.");


                bool Quotation = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .AnyAsync(qs => QuoteSubIds.Contains(qs.RefQuoteSubId.Value));

                if (Quotation)
                    return (false, "Cannot delete this Quotation  as a some transaction Made.");

                var sums = await (
                                 from sub in _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                                 where sub.QuoteSubId == QuoteSubId
                                 group sub by 1 into g
                                 select new
                                 {
                                     TotalQty = g.Sum(s => (decimal?)s.Qty) ?? 0,
                                     TotalBalQty = g.Sum(s => (decimal?)s.BalQty) ?? 0
                                 }
                             ).FirstOrDefaultAsync();

                bool hasPurchPo = sums != null && sums.TotalQty == sums.TotalBalQty;

                if (!hasPurchPo)
                    return (false, "Cannot delete this Quotation as some transactions have been made.");

                var Quote = await _unitOfWork.PurchaseQuotes
                               .GetQueryable()
                               .Where(e => e.QuoteId == QuoteId)
                               .Select(e => new
                               {
                                   e.QuoteId,
                                   e.IsCancel,
                                   e.QuoteShortClose,
                                   SubItems = e.PurchaseQuoteSub.Select(s => new
                                   {
                                       s.QuoteSubId,
                                       s.ItemCancel
                                   }).ToList()
                               })
                               .FirstOrDefaultAsync();


                if (Quote == null)
                    return (false, "Quotation not found.");


                if (Quote.IsCancel || Quote.QuoteShortClose)
                    return (false, "Main Quotation is already cancelled Or Short Closed and cannot be deleted.");

                if (Quote.SubItems.Any())
                    return (true, "Purchase Quotation can be safely deleted (no sub-items).");

                return (true, "Quotation can be safely deleted.");

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteEnquiryAsync for Quoteid: {QuoteId}");
                throw new Exception("Error checking Purchase Quotation delete eligibility", ex);
            }
        }


        public async Task UpdateItemCancelAndAddorRevertAsync(PurchaseQuoteSubVM subItem)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {

                var subEntity = await _unitOfWork.PurchaseQuotesSubs.GetQueryable().Where
                                (x => x.QuoteSubId == subItem.QuoteSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with QuoteSubId {subItem.QuoteSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidatePurchaseEnquiryBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.PurchaseQuotesSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    await AdjustEnquiryBalanceAsync(subEntity.RefEnqVendorAssignId, subEntity.Qty, 0, $"Purchase Quote Item Cancel - {subItem.ItemCode}");
                }
                else
                {
                    await AdjustEnquiryBalanceAsync(subEntity.RefEnqVendorAssignId, 0, subEntity.Qty, $"Purchase Quote Revert Cancel - {subItem.ItemCode}");
                }

                await UpdateQuoteTallyStatusAsync(subItem.QuoteId);

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
        public async Task ValidatePurchaseEnquiryBalanceBeforeRevertAsync(PurchaseQuoteSub sub)
        {
            if (sub.RefEnqVendorAssignId <= 0)
                return;

            var entity = await _unitOfWork.EnquiryPurchaseVendorAssigns.GetAsync(sub.RefEnqVendorAssignId.Value);
            if (entity == null)
                throw new InvalidOperationException($"Enquiry Assign not found for RefEnqSubId: {sub.RefEnqVendorAssignId}");

            if (entity.BalQty < sub.Qty)
            {
                throw new InvalidOperationException($"Cannot revert because Enquiry balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(PurchaseQuoteVM quoteVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingQuote = await _unitOfWork.PurchaseQuotes.GetAsync(quoteVM.QuoteId);
                if (existingQuote == null)
                    throw new InvalidOperationException("Purchase Quote not found.");

                var subs = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(s => s.QuoteId == quoteVM.QuoteId)
                    .ToListAsync();

                if (!quoteVM.IsCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidatePurchaseEnquiryBalanceBeforeRevertAsync(sub);
                    }
                }

                existingQuote.IsCancel = quoteVM.IsCancel;
                existingQuote.CancelReason = quoteVM.CancelReason;
                existingQuote.CancelDate = quoteVM.CancelDate;
                existingQuote.CancelledBy = quoteVM.CancelledBy;

                await _unitOfWork.PurchaseQuotes.UpdateAsync(existingQuote);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingQuote.IsCancel)
                    {
                        if (sub.RefEnqVendorAssignId > 0)
                        {
                            await AdjustEnquiryBalanceAsync(sub.RefEnqVendorAssignId, sub.Qty, 0, $"Purchase Quote Cancelled - {existingQuote.QuoteId}");
                        }
                    }
                    else
                    {
                        if (sub.RefEnqVendorAssignId > 0)
                        {
                            await AdjustEnquiryBalanceAsync(sub.RefEnqVendorAssignId, 0, sub.Qty, $"Purchase Quote Reverted - {existingQuote.QuoteNo}");
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
                throw new InvalidOperationException("Failed to update cancel/revert status. Please contact support.", ex);
            }

        }
        public async Task UpdateQuoteTallyStatusAsync(int QuoteId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(x => x.QuoteId == QuoteId && !x.ItemCancel  && !x.ItemShortClose)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var quotation = await _unitOfWork.PurchaseQuotes.GetAsync(QuoteId);
                if (quotation == null)
                    return;

                quotation.QuotationTally = (totalBalQty == 0);

                await _unitOfWork.PurchaseQuotes.UpdateAsync(quotation);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateGRNTallyStatusAsync] Error updating QuoteId:- {QuoteId}");
                throw new InvalidOperationException("Failed to update Purchase Quote Tally status. Please contact support.");
            }
        }

        public async Task<(bool CanItemCancel, string Message)> CanQuoteItemCancelCheckAsync(PurchaseQuoteSubVM subItem)
        {
            try
            {
                bool hasPo = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefQuoteSubId.HasValue && qs.RefQuoteSubId == subItem.QuoteSubId && !qs.ItemCancel);
                if (hasPo)
                    return (false, "Cannot cancel this Item as a Purchase Order transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanQuoteItemCancelCheckAsync for QuoteSubId: {subItem.QuoteSubId}");
                throw new Exception("Error checking Purchase Quotation Item cancel eligibility", ex);
            }
        }

        public async Task UpsertPurchaseQuoteShortCloseAsync(PurchaseQuoteVM PurchQuoteVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingQuote = await _unitOfWork.PurchaseQuotes.GetAsync(PurchQuoteVM.QuoteId);
                if (existingQuote == null)
                    throw new InvalidOperationException("Quotation not found.");

                existingQuote.QuoteShortClose = PurchQuoteVM.QuoteShortClose;

                await _unitOfWork.PurchaseQuotes.UpdateAsync(existingQuote);
                await _unitOfWork.SaveAsync();

                await UpdateQuoteTallyStatusAsync(PurchQuoteVM.QuoteId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertPurchaseQuoteShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertPurchaseQuoteShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }

        public async Task<(List<PurchaseQuoteVM> purchaseQuoteVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.PurchaseQuotes.GetQueryable()
                .Include(j => j.Vendor)
                .Include(j => j.PurchaseQuoteSub)
                    .ThenInclude(s => s.Item)
                .Include(j => j.PurchaseQuoteSub)
                    .ThenInclude(s => s.CostCenter)
                .AsQueryable();


            // Apply dynamic filters
            if (filters != null)
            {
                foreach (var f in filters)
                    query = PurchpoFilterBuilder.ApplyFilter(query, f.Key, f.Value);
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.QuoteId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (_mapper.Map<List<PurchaseQuoteVM>>(list), total);
        }


        public static class PurchpoFilterBuilder
        {
            public static IQueryable<PurchaseQuote> ApplyFilter(
                IQueryable<PurchaseQuote> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                var val = value.ToString().Trim();

                switch (field)
                {
                    case "QuoteNo":
                        {
                            string input = val;
                            string part1 = input;
                            string part2 = "";

                            int slashIndex = input.IndexOf('/');

                            if (slashIndex > -1)
                            {
                                part1 = input.Substring(0, slashIndex).Trim();
                                part2 = input.Substring(slashIndex + 1).Trim();
                            }

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.QuoteNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "VendorName":
                        return query.Where(x =>x.Vendor.VendorName.Contains(val));

                    case "ItemCode":
                        return query.Where(x =>
                            x.PurchaseQuoteSub.Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x =>
                            x.PurchaseQuoteSub.Any(s => s.Item.ItemName.Contains(val)));

                    case "EnqNo":
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
                                x.PurchaseQuoteSub.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.EnquiryPurchaseVendorAssign.Enquiry.EnquiryNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.EnquiryPurchaseVendorAssign.Enquiry.Suffix.Contains(part2))
                                ));
                        }

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "ModifiedBy":
                        return query.Where(x => x.ModifiedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.QuoteDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.QuoteDateNow <= toDate);
                        break;

                    case "Status":
                        return ApplyStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<PurchaseQuote> ApplyStatusFilter(
                IQueryable<PurchaseQuote> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.QuotationTally == true),
                    "Pending" => query.Where(x => x.QuotationTally == false && x.IsCancel == false && x.QuoteShortClose == false),
                    "Cancelled" => query.Where(x => x.IsCancel == true),
                    "Short Closed" => query.Where(x => x.QuoteShortClose == true),
                    _ => query
                };
            }
        }

        public async Task<List<PurchaseQuotationPendingList>> GetPurchSubQuotePendingList(string status)//Shankar
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<PurchaseQuotationPendingList>("Sp_GetPurchandSubQuotePendingList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }
}
