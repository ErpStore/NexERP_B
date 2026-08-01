using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Data.Utilities;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Microsoft.Data.SqlClient;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class MfgQuotationService : IMfgQuotationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IExcelTemplateService _excelTemplateService;
        private readonly IReportExecutor _report;
        public MfgQuotationService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            IExcelTemplateService excelTemplateService, IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _excelTemplateService = excelTemplateService;
            _report = report;
        }



        // 🔹 Customers
        public async Task<List<CustomerVM>> GetCustomersAsync(int? custId = null)
        {
            if (custId.HasValue && custId.Value > 0)
            {
                var customer = await _commonService.GetCustomerByIdAsync(custId.Value);
                return customer != null ? new List<CustomerVM> { customer } : new List<CustomerVM>();
            }
            return await _commonService.GetAllActiveCustomersAsync();
        }

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetCustomerByIdAsync(custId);

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? custId = null)
            => await _commonService.SearchItemsAsync(searchText, custId);

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
        public async Task<List<ContactPerson>> GetContactPersonsAsync(int custId)
            => await _commonService.GetContactPersonsAsync(custId);


        // 🔹 Consignee addresses
        public async Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId)
            => await _commonService.GetConsigneeAddressesAsync(custId);

        // CostCeneter
        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();



        // 🔹 Quotation operations

        public async Task<bool> IsQuoteTransactionsMatchedAsync(int quoteId, MfgQuoteVM quoteVMs)
        {
            try
            {
                var quoteSubIds = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(x => x.QuoteId == quoteId)
                    .Select(x => x.QuoteSubId)
                    .ToListAsync();

                bool hasQuote = quoteSubIds.Any();

                bool hasTransactions = false;

                if (hasQuote)
                {
                    // Check Manufacturing Quotation references
                    hasTransactions = await _unitOfWork.MfgPoSubs
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.RefQuoteSubId.HasValue &&
                            quoteSubIds.Contains(pqs.RefQuoteSubId.Value));
                }

                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = quoteVMs?.MfgQuoteSubVM;
                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.Qty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty ?? 0);

                    qtyMismatch = totalQty != totalBalQty;
                }

                // If either transactions exist OR quantity mismatch → return true
                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for QuoteId: {quoteId}");
                throw new InvalidOperationException("Failed to verify enquiry transactions.", ex);
            }
        }


        public async Task<(List<MfgQuoteVM> mfgQuoteVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber,int pageSize,Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.MfgQuotes
                    .GetQueryable()
                    .Include(x => x.Customer)
                    .Include(x => x.MfgQuoteSub)
                        .ThenInclude(s => s.Item)
                    .Include(x => x.MfgQuoteSub)
                        .ThenInclude(s => s.EnquirySalesSub)
                            .ThenInclude(es => es.EnquirySales)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = MfgQuoteFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.QuoteId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<MfgQuoteVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,"Error in SearchWithDynamicFilterAsync (MfgQuote)");
                throw new InvalidOperationException("Failed to load manufacturing quotation list.",ex);
            }
        }

        public static class MfgQuoteFilterBuilder
        {
            public static IQueryable<MfgQuote> ApplyFilter(
                IQueryable<MfgQuote> query,
                string field,
                object value)
            {
                try
                {
                    if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                        return query;

                    string val = value.ToString()!.Trim();

                    switch (field)
                    {

                        case "QuoteNo":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                if (string.IsNullOrEmpty(part1))
                                    return query;

                                string quoteNo = val.Split('/')[0];

                                int revisionNo = 0;
                                string suffix = "";

                                var parts = val.Split('/', StringSplitOptions.RemoveEmptyEntries);

                                foreach (var part in parts)
                                {
                                    if (part.StartsWith("R", StringComparison.OrdinalIgnoreCase))
                                    {
                                        int.TryParse(part.Substring(1), out revisionNo);
                                    }
                                    else if (part.Contains('-')) // Financial Year
                                    {
                                        suffix = "/" + part;
                                    }
                                }
                                return query.Where(x =>
                                    x.QuoteNo.StartsWith(quoteNo) &&
                                    (revisionNo == 0 || x.RevisionQuoteNo == revisionNo) &&
                                    (string.IsNullOrEmpty(suffix) || x.Suffix == suffix)
                                );

                            }

                        case "EnqNo":
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
                                    x.MfgQuoteSub.Any(s =>
                                        s.EnquirySalesSub != null &&
                                        s.EnquirySalesSub.EnquirySales != null &&
                                        (string.IsNullOrEmpty(part1) ||
                                            s.EnquirySalesSub.EnquirySales.EnquiryNo.StartsWith(part1)) &&
                                        (string.IsNullOrEmpty(part2) ||
                                            (x.Suffix != null && x.Suffix.Contains(part2)))
                                    )
                                );
                            }

                        case "Customer":
                            return query.Where(x =>
                                x.Customer != null &&
                                x.Customer.CustName.Contains(val));

                        case "ItemName":
                            return query.Where(x =>
                                x.MfgQuoteSub.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemName.Contains(val)));

                        case "ItemCode":
                            return query.Where(x =>
                                x.MfgQuoteSub.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemCode.Contains(val)));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.QuoteDateNow >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.QuoteDateNow <= toDate.Date.AddDays(1).AddTicks(-1));
                            return query;

                        case "Status":
                            return ApplyStatusFilter(query, val);
                    }

                    return query;
                }
                catch
                {
                    return query;
                }
            }

            private static IQueryable<MfgQuote> ApplyStatusFilter(
                IQueryable<MfgQuote> query,
                string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.QuotationTally),

                        "Pending" =>
                            query.Where(x =>
                                !x.QuotationTally &&
                                !x.IsCancel &&
                                !x.ShortClose),

                        "Cancelled" =>
                            query.Where(x => x.IsCancel),

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

        public async Task ValidateBeforeRevertAsync(int quoteSubId)
        {
            try
            {
                var sub = await _unitOfWork.MfgQuoteSubs.GetAsync(quoteSubId);

                if (sub == null)
                    throw new InvalidOperationException("Sales Quotation Item not found.");

                if (sub.RefEnqSubId > 0)
                    await ValidateEnquiryBalanceBeforeRevertAsync(sub);
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

        public async Task<(bool CanItemCancel, string Message)> CanQuoteItemCancelCheckAsync(MfgQuoteSubVM subItem)
        {
            try
            {
                bool hasPo = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefQuoteSubId.HasValue && qs.RefQuoteSubId == subItem.QuoteSubId && !qs.ItemCancel);
                if (hasPo)
                    return (false, "Cannot cancel this Item as a Sales Order transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanQuoteItemCancelCheckAsync for QuoteSubId: {subItem.QuoteSubId}");
                throw new Exception("Error checking Sales Quotation Item cancel eligibility", ex);
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteSalesQuotationAsync(int quoteId)
        {
            try
            {
                var quote = await _unitOfWork.MfgQuotes
                                .GetQueryable()
                                .Include(e => e.MfgQuoteSub)
                                .Where(e => e.QuoteId == quoteId).FirstOrDefaultAsync();

                if (quote == null)
                    return (true, "Sales Quotation can be safely deleted.");

                var quoteSubIds = quote.MfgQuoteSub
                    .Select(es => es.QuoteSubId)
                    .ToList();

                bool hasPo = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefQuoteSubId.HasValue &&
                        quoteSubIds.Contains(qs.RefQuoteSubId.Value));

                if (hasPo)
                    return (false, "Cannot delete this Sales Quotation as a Sales order transaction exists.");

                if (quote.IsCancel || quote.ShortClose)
                    return (false, "Cannot delete this Sales Quotation as it is Cancelled or Short Closed.");


                if (quote.MfgQuoteSub.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Sales Quotation as one or more enquiry items are cancelled.");


                return (true, "Sales Quotation can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteSalesQuotationAsync for QuoteId: {quoteId}");
                throw new Exception("Error checking Sales Quotation delete eligibility", ex);
            }
        }



        public async Task<int> GetPendingEnquiryCountAsync(int custId)
        {
            return await _unitOfWork.EnquirySaless
                .GetQueryable()
                .Where(e => e.CustId == custId && e.EnquiryTally == false)
                .CountAsync();
        }

        public async Task<MfgQuoteVM?> GetQuotationByQuoteIdAsync(int quoteId)
        {
            try
            {
                var entity = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Include(q => q.MfgQuoteSub)
                    .Include(q => q.MfgQuoteSub).ThenInclude(s => s.Item)
                    .Include(q => q.MfgQuoteSub).ThenInclude(s => s.CostCenter)
                    .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                    .Include(q => q.Currency)
                    .FirstOrDefaultAsync(q => q.QuoteId == quoteId);

                return _mapper.Map<MfgQuoteVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetQuotationAsync({quoteId})");
                return null;
            }
        }

        public async Task<IEnumerable<MfgQuoteVM>> GetAllQuoteAsync()
        {
            try
            {
                var quotes = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Include(q => q.Customer)
                    .AsNoTracking()
                    .ToListAsync();

                var quoteIds = quotes.Select(q => q.QuoteId).ToList();

                var subItems = await _unitOfWork.MfgQuoteSubs.GetQueryable()
                    .Where(s => quoteIds.Contains(s.QuoteId))
                    .AsNoTracking()
                    .ToListAsync();

                var quoteVMs = _mapper.Map<IEnumerable<MfgQuoteVM>>(quotes);

                foreach (var quoteVM in quoteVMs)
                {
                    quoteVM.MfgQuoteSubVM = subItems
                        .Where(s => s.QuoteId == quoteVM.QuoteId)
                        .Select(s => _mapper.Map<MfgQuoteSubVM>(s))
                        .ToList();
                }

                return quoteVMs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllQuoteAsync");
                return Enumerable.Empty<MfgQuoteVM>();
            }
        }


        public async Task<MfgQuote?> GetLastQuoteAsync(int custId)
        {
            try
            {
                return await _unitOfWork.MfgQuotes.GetLatestAsync(
                    q => q.CustId == custId,
                    q => q.QuoteId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastQuoteAsync for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve last quotation. Please try again.");
            }
        }

        public async Task<MfgQuoteVM> UpsertQuoteAsync(MfgQuoteVM quoteVM)
        {
            if (quoteVM == null)
                throw new ArgumentNullException(nameof(quoteVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MfgQuote entity;

                if (quoteVM.QuoteId == 0)
                {
                    entity = _mapper.Map<MfgQuote>(quoteVM);


                    // 🔹 Get last number with locking from repository
                    if (!entity.IsRevisionNo)
                    {
                        var NextNumber = await _unitOfWork.MfgQuotes.GetLastQuoteNoAsync(entity.Suffix);
                        entity.QuoteNo = NextNumber;
                    }

                    entity.QuoteNo = quoteVM.QuoteNo;
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.MfgQuoteSub = quoteVM.MfgQuoteSubVM.Select(s => _mapper.Map<MfgQuoteSub>(s)).ToList();

                    if (entity.IsRevisionNo && entity.RevisionQuoteNo.HasValue && entity.RevisionQuoteNo > 0 && entity.RefRevisionQuoteId.HasValue)
                    {
                        var previousQuote = await _unitOfWork.MfgQuotes.GetQueryable()
                                        .Include(p => p.MfgQuoteSub)
                                        .Where(x => x.QuoteId == entity.RefRevisionQuoteId.Value).FirstOrDefaultAsync();

                        

                        if (previousQuote != null)
                        {
                            foreach (var sub in previousQuote.MfgQuoteSub)
                            {
                                if (sub.RefEnqSubId.GetValueOrDefault() > 0)
                                {
                                    await AdjustEnquiryBalanceAsync(sub.RefEnqSubId, sub.Qty, 0, $"Quote Revised - {entity.QuoteNo}");
                                   
                                }
                            }

                            previousQuote.IsCancel = true;
                            previousQuote.CancelReason = $"Auto closed due to revision - {entity.QuoteNo}{entity.RevisionQuoteNo}{entity.Suffix}";

                            await _unitOfWork.MfgQuotes.UpdateAsync(previousQuote);
                            await _unitOfWork.SaveAsync();
                        }

                    }


                    await SetSalesQuoteAuthorizationStatusAsync(entity, currentUser);

                    await _unitOfWork.MfgQuotes.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in quoteVM.MfgQuoteSubVM)
                    {
                        if (subVM.RefEnqSubId > 0)
                            await AdjustEnquiryBalanceAsync(subVM.RefEnqSubId, 0, subVM.Qty ?? 0, "Quote Creation");
                        else
                            await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }

                    changes.AppendLine("Quotation Created.");
                }
                else
                {
                    entity = await _unitOfWork.MfgQuotes.GetQueryable()
                        .Include(q => q.MfgQuoteSub)
                        .FirstOrDefaultAsync(q => q.QuoteId == quoteVM.QuoteId)
                        ?? throw new InvalidOperationException("Quotation not found.");

                    var parentChanges = GetPropertyChanges(entity, quoteVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(quoteVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await SetSalesQuoteAuthorizationStatusAsync(entity, currentUser);

                    await HandleChildUpdatesAsync(entity, quoteVM.MfgQuoteSubVM, changes);

                    changes.AppendLine("Quotation Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateQuotationTallyStatusAsync(entity.QuoteId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, quoteVM.QuoteId == 0 ? "Quotation Created" : "Quotation Updated");

                var savedEntity = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Include(q => q.MfgQuoteSub).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.Currency)
                    .Include(q => q.MfgQuoteSub).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.QuoteId == entity.QuoteId);

                return _mapper.Map<MfgQuoteVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert quotation: {quoteVM.QuoteNo}");
                throw new InvalidOperationException("Failed to save quotation. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(MfgQuote existingQuote, List<MfgQuoteSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {

                var existingSubIds = existingQuote.MfgQuoteSub.Select(s => s.QuoteSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.QuoteSubId).ToHashSet();

                // DELETE removed children
                foreach (var sub in existingQuote.MfgQuoteSub.Where(s => !incomingSubIds.Contains(s.QuoteSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - QuoteSubId: {sub.QuoteSubId}, Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.MfgQuoteSubs.DeleteAsync(sub.QuoteSubId);
                    await _unitOfWork.SaveAsync();

                    if (sub.RefEnqSubId > 0)
                        await AdjustEnquiryBalanceAsync(sub.RefEnqSubId, sub.Qty, 0, "Quote Deletion");
                    else
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                }

                // ADD or UPDATE children
                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.QuoteSubId == 0)
                    {
                        var newSub = _mapper.Map<MfgQuoteSub>(subVM);
                        newSub.QuoteId = existingQuote.QuoteId;
                        await _unitOfWork.MfgQuoteSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                        if (subVM.RefEnqSubId > 0)
                            await AdjustEnquiryBalanceAsync(subVM.RefEnqSubId, 0, subVM.Qty ?? 0, "Quote Creation");
                        else
                            await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }
                    else
                    {
                        var existingSub = existingQuote.MfgQuoteSub.FirstOrDefault(s => s.QuoteSubId == subVM.QuoteSubId);
                        if (existingSub != null)
                        {
                            // Rollback old CostCenter if needed
                            if ((existingSub.CostId != subVM.CostId) && (existingSub.RefEnqSubId == null || existingSub.RefEnqSubId == 0))
                                await UpdateCostCenterAsync(existingSub.CostId, false, changes);

                            // Adjust Enquiry balance if RefEnqSubId > 0
                            if (subVM.RefEnqSubId > 0)
                                await AdjustEnquiryBalanceAsync(subVM.RefEnqSubId, existingSub.Qty, subVM.Qty ?? 0, "Quote Update");

                            // Assign new CostCenter if needed
                            if ((subVM.CostId > 0) && (subVM.RefEnqSubId == null || subVM.RefEnqSubId == 0))
                                await UpdateCostCenterAsync(subVM.CostId, true, changes);




                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "[HandleChildUpdatesAsync - Sales Quotation] Failed to update Quotation children");
                throw new InvalidOperationException("Failed to update Quotation details. Please contact support.");
            }
        }

        // CostCenter update helper
        private async Task UpdateCostCenterAsync(int? costCenterId, bool assign, StringBuilder changes)
        {
            if (!costCenterId.HasValue || costCenterId == 0) return;

            var cost = await _unitOfWork.CostCenters.GetAsync(costCenterId.Value);
            if (cost != null && cost.AssToQuote != assign)
            {
                cost.AssToQuote = assign;
                await _unitOfWork.CostCenters.UpdateAsync(cost);
                await _unitOfWork.SaveAsync();
                changes.AppendLine($"CostCenter Updated - ProjectNo {cost.ProjectNo}: AssToQuote set to '{assign}'");
            }
        }

        private async Task AdjustEnquiryBalanceAsync(int? refEnqSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refEnqSubId.HasValue || refEnqSubId == 0) return;

                var enqSub = await _unitOfWork.EnquirySalesSubs.GetAsync(refEnqSubId.Value);
                if (enqSub == null) return;

                if (oldQty > 0)
                    enqSub.BalQty += oldQty;

                if (newQty > enqSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Enquiry BalQty.");

                if (newQty > 0)
                    enqSub.BalQty -= newQty;

                await _unitOfWork.EnquirySalesSubs.UpdateAsync(enqSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.EnquirySalesSubs
                    .GetQueryable()
                    .Where(e => e.EnquiryId == enqSub.EnquiryId && !e.ItemCancel)
                    .SumAsync(e => e.BalQty);


                var Enquiry = await _unitOfWork.EnquirySaless.GetAsync(enqSub.EnquiryId);
                if (Enquiry != null)
                {
                    Enquiry.EnquiryTally = (totalBalQty == 0);
                    await _unitOfWork.EnquirySaless.UpdateAsync(Enquiry);
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
                await _logs.LogDeveloperError(ex, $"[AdjustQuoteBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Enquiry Balance. Please contact support.");
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

        public async Task<bool> HasAnyPOTransactionMadeAsync(int quoteId)
        {
            try
            {
                var quoteSubIds = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(s => s.QuoteId == quoteId)
                    .Select(s => s.QuoteSubId)
                    .ToListAsync();

                if (!quoteSubIds.Any())
                    return false;

                return await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefQuoteSubId.HasValue && quoteSubIds.Contains(qs.RefQuoteSubId.Value));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyPOTransactionMadeAsync for QuoteId: {quoteId}");
                throw;
            }
        }

        public async Task<bool> HasAnyItemOrQuoteCancelAsync(int quoteId)
        {
            try
            {
                var isQuoteCancelled = await _unitOfWork.MfgQuotes
                    .AnyAsync(q => q.QuoteId == quoteId && q.IsCancel == true);

                var isItemCancelled = await _unitOfWork.MfgQuoteSubs
                    .AnyAsync(i => i.QuoteId == quoteId && i.ItemCancel == true);

                return isQuoteCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrQuoteCancelAsync for QuoteId: {quoteId}");
                throw;
            }
        }


        public async Task DeleteAndResequenceAsync(MfgQuoteSubVM subitem, MfgQuoteVM quote)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.QuoteSubId > 0)
                {
                    var entity = await _unitOfWork.MfgQuoteSubs.GetAsync(subitem.QuoteSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.RefEnqSubId > 0)
                    {
                        await AdjustEnquiryBalanceAsync(subitem.RefEnqSubId, entity.Qty, 0, "Quote Deletion");
                    }
                    else
                    {   
                        await UpdateCostCenterAsync(subitem.CostId, false, changes);
                    }

                    await _unitOfWork.MfgQuoteSubs.DeleteAsync(entity);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Mfg Quote",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Quotation No: {quote?.QuoteNo}"
                    );
                }
                else
                {
                    quote.MfgQuoteSubVM.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.MfgQuoteSubs
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
                await UpdateQuotationTallyStatusAsync(quote.QuoteId);

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
                // Get the quotation with its sub-items
                var quotation = await _unitOfWork.MfgQuotes
                    .GetQueryable()
                    .Include(e => e.MfgQuoteSub)
                    .FirstOrDefaultAsync(e => e.QuoteId == QuoteId);

                if (quotation == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                foreach (var sub in quotation.MfgQuoteSub)
                {
                    if (sub.RefEnqSubId > 0)
                    {
                        await AdjustEnquiryBalanceAsync(sub.RefEnqSubId, sub.Qty, 0, "Quote Deletion");
                    }
                    else
                    {
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                    }
                }

                // Delete the quotation
                await _unitOfWork.MfgQuotes.DeleteAsync(quotation);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Quotation List",
                    action: $"Deleted Quotation: {quotation.QuoteNo}",
                    additionalInfo: $"Quotation Id: {quotation.QuoteId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Quotation: {QuoteId}");
                throw;
            }
        }

        public async Task<List<MfgQuoteSub>> GetQuoteSubByQuoteIdAsync(int quoteId)
        {
            try
            {
                var subs = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Include(s => s.Item)
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
        public async Task<List<Dictionary<string, object>>> GetEnquiryDetailsByCustId(int custId, bool mfgOrLab)
        {
            try
            {
                var result = await (from e in _unitOfWork.EnquirySaless.GetQueryable()
                                    join es in _unitOfWork.EnquirySalesSubs.GetQueryable()
                                        on e.EnquiryId equals es.EnquiryId
                                    where e.CustId == custId && !e.EnquiryTally && !e.Cancel  && !e.ShortClose && es.BalQty > 0 && !es.ItemCancel && e.MfgORLab == mfgOrLab
                                    select new
                                    {
                                        es.EnquirySubId,
                                        e.EnquiryNo,
                                        e.Suffix,
                                        e.EnquiryDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.HSNCode,
                                        es.Item.MeasureUnit,
                                        es.Qty,
                                        es.BalQty,
                                        es.UnitPrice,
                                        CostCenterId = es.CostCenterId == 0 ? (int?)null : es.CostCenterId,
                                        es.CostCenter.ProjectNo,
                                        e.Customer
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefEnqSubId"] = r.EnquirySubId,
                    ["EnquiryNo"] = $"{r.EnquiryNo}{r.Suffix}",
                    ["EnquiryDate"] = r.EnquiryDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["Rate"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Customer"] = r.Customer.CustName ?? string.Empty
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching enquiry details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve enquiry details. Please try again.");
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

                    rate = await (from qs in _unitOfWork.MfgQuoteSubs.GetQueryable()
                                  join q in _unitOfWork.MfgQuotes.GetQueryable() on qs.QuoteId equals q.QuoteId
                                  where qs.ItemId == itemId && q.CustId == custId
                                  orderby q.QuoteId descending
                                  select qs.UnitPrice)
                                 .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.MfgQuoteSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.QuoteSubId descending
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
        public async Task<string> GetPrefixFromDb()
        {
            try
            {
                var prefix = await _unitOfWork.MfgQuotes.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.QuoteId)
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
        public async Task<string> GetQuotationNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.MfgQuotes
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q =>Convert.ToInt32(q.QuoteNo))
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
                await _logs.LogDeveloperError(ex, $"Error generating quotation number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate quotation number.");
            }
        }
        public async Task<decimal> GetEnquiryItemBalQtyFromEnqSubId(int enqSubId)
        {
            try
            {
                return await _unitOfWork.EnquirySalesSubs.GetQueryable()
                    .Where(e => e.EnquirySubId == enqSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for EnqSubId: {enqSubId}");
                throw new InvalidOperationException("Failed to retrieve enquiry balance quantity.");
            }
        }
        public async Task<MfgQuoteSubVM?> GetQuoteSubItemDetailByQuoteSubIdAsync(int quoteSubId)
        {
            try
            {
                return await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(q => q.QuoteSubId == quoteSubId)
                    .Select(q => new MfgQuoteSubVM
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

        public async Task<List<MfgQuoteSubVM>> GetDistinctRefEnquiriesByQuoteIdAsync(int quoteId)
        {
            return await _unitOfWork.MfgQuoteSubs
                .GetQueryable()
                .Include(x => x.EnquirySalesSub)
                .ThenInclude(x => x.EnquirySales)
                .Where(s => s.QuoteId == quoteId)
                .GroupBy(s => new { s.EnquirySalesSub.EnquirySales.EnquiryNo, s.EnquirySalesSub.EnquirySales.Suffix, s.EnquirySalesSub.EnquirySales.EnquiryDate })
                .Select(g => new MfgQuoteSubVM
                {
                    RefEnqNo = $"{g.Key.EnquiryNo}{g.Key.Suffix}",
                    RefEnqDate = g.Key.EnquiryDate
                })
                .ToListAsync();
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(MfgQuoteSubVM subItem, int quoteId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.MfgQuoteSubs.GetQueryable().Where
                                (x => x.QuoteSubId == subItem.QuoteSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with QuoteSubId {subItem.QuoteSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateEnquiryBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.MfgQuoteSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    await AdjustEnquiryBalanceAsync(
                        subEntity.RefEnqSubId,
                        subEntity.Qty,
                        0,
                        $"Quotation Item Cancel - {subItem.ItemCode}"
                    );
                }
                else
                {
                    await AdjustEnquiryBalanceAsync(
                        subEntity.RefEnqSubId,
                        0,
                        subEntity.Qty,
                        $"Quotation Item Revert Cancel - {subItem.ItemCode}"
                    );
                }

                await UpdateQuotationTallyStatusAsync(quoteId);

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



        public async Task UpdateQuotationTallyStatusAsync(int quoteId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(x => x.QuoteId == quoteId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var quotation = await _unitOfWork.MfgQuotes.GetAsync(quoteId);
                if (quotation == null)
                    return;

                if (quotation.ShortClose || quotation.IsCancel)
                    return;

                quotation.QuotationTally = (totalBalQty == 0);

                await _unitOfWork.MfgQuotes.UpdateAsync(quotation);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateQuotationTallyStatusAsync] Error updating QuoteId {quoteId}");
                throw new InvalidOperationException("Failed to update Quotation Tally status. Please contact support.");
            }
        }

        public async Task UpsertSalesQuoteShortCloseAsync(MfgQuoteVM mfgQuoteVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingQuote = await _unitOfWork.MfgQuotes.GetAsync(mfgQuoteVM.QuoteId);
                if (existingQuote == null)
                    throw new InvalidOperationException("Sales Enquiry not found.");

                existingQuote.ShortClose = mfgQuoteVM.ShortClose;

                await _unitOfWork.MfgQuotes.UpdateAsync(existingQuote);
                await _unitOfWork.SaveAsync();

                await UpdateQuotationTallyStatusAsync(mfgQuoteVM.QuoteId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSalesQuoteShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSalesQuoteShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }


        public async Task UpdatedCancelStatusAndAddOrRevertQty(MfgQuoteVM quoteVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingQuote = await _unitOfWork.MfgQuotes.GetAsync(quoteVM.QuoteId);
                if (existingQuote == null)
                    throw new InvalidOperationException("Manufacturing Quotation not found.");

                var subs = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(s => s.QuoteId == quoteVM.QuoteId)
                    .ToListAsync();

                if (!quoteVM.IsCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateEnquiryBalanceBeforeRevertAsync(sub);
                    }
                }

                existingQuote.IsCancel = quoteVM.IsCancel;
                existingQuote.CancelDate = quoteVM.CancelDate;
                existingQuote.CancelReason = quoteVM.CancelReason;
                existingQuote.CancelBy = quoteVM.CancelBy;

                await _unitOfWork.MfgQuotes.UpdateAsync(existingQuote);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingQuote.IsCancel)
                    {
                        if (sub.RefEnqSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustEnquiryBalanceAsync(sub.RefEnqSubId.Value, sub.Qty, 0, $"Manufacturing Quotation Cancelled - {existingQuote.QuoteNo}");
                        }
                    }
                    else
                    {
                        if (sub.RefEnqSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustEnquiryBalanceAsync(sub.RefEnqSubId.Value, 0, sub.Qty, $"Manufacturing Quotation Reverted - {existingQuote.QuoteNo}");
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

        public async Task ValidateEnquiryBalanceBeforeRevertAsync(MfgQuoteSub sub)
        {
            if (sub.RefEnqSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.EnquirySalesSubs.GetAsync(sub.RefEnqSubId.Value);

            if (entity == null)
                throw new InvalidOperationException($"enquiry not found for RefEnqSubId: {sub.RefEnqSubId}");

            if (entity.BalQty < sub.Qty && !entity.ItemCancel)
            {
                throw new InvalidOperationException($"Cannot revert because Enquiry balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
            }
        }



        public async Task<byte[]> CreateTemplateAsync(string uploadtype)
        {
            try
            {
                var bytes = await _excelTemplateService.DownloadTemplate(uploadtype);
                return bytes;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error creating Excel template");
                return Array.Empty<byte>();
            }
        }


        private async Task SetSalesQuoteAuthorizationStatusAsync(MfgQuote entity, string currentUser)
        {
            var prAuthorityExists = await _unitOfWork.UserAuthorities
                .AnyAsync(x => x.IsMfgQuote == true);

            if (!prAuthorityExists)
            {
                entity.IsAuthorized = true;
                entity.LastApproved = currentUser;
                entity.ApprovedDate = DateTime.Now;
            }
            else
            {
                entity.IsAuthorized = false;
                entity.LastApproved = null;
                entity.ApprovedDate = null;

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

        #region revise mfg Quotation
        public async Task<MfgQuoteVM> ReviseQuotationDraftAsync(int QuoteId)
        {
            try
            {
                var entity = await _unitOfWork.MfgQuotes.GetQueryable()
                            .AsNoTracking()
                            .Include(q => q.MfgQuoteSub).ThenInclude(s => s.Item)
                            .Include(q => q.MfgQuoteSub).ThenInclude(s => s.CostCenter)
                            .Include(q => q.MfgQuoteSub).ThenInclude(s => s.EnquirySalesSub).ThenInclude(s => s.EnquirySales)
                            .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                            .FirstOrDefaultAsync(q => q.QuoteId == QuoteId);

                if (entity == null)
                    throw new Exception("Mfg Quotation  not found.");

                var lastRev = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Where(x => x.RefRevisionQuoteId == QuoteId || x.QuoteId == QuoteId)
                    .MaxAsync(x => (int?)x.RevisionQuoteNo) ?? 0;

                var newRev = lastRev + 1;

                var newQuote = _mapper.Map<MfgQuote>(entity);

                newQuote.QuoteId = 0;
                newQuote.RevisionQuoteNo = newRev;
                newQuote.IsRevisionNo = true;
                newQuote.QuoteDate = DateTime.Now;
                newQuote.ApprovalDate = null;
                newQuote.RefRevisionQuoteId = QuoteId;

                foreach (var sub in newQuote.MfgQuoteSub)
                {
                    sub.QuoteSubId = 0;
                    sub.QuoteId = 0;
                }

                var vm = _mapper.Map<MfgQuoteVM>(newQuote);
                return vm;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to revise Mfg Quote: {QuoteId}");
                throw new InvalidOperationException("Failed to revise Mfg Quote. Please try again.");
            }
        }
        #endregion


        public async Task<bool> CheckreviseQuoteByQuoteIdAsyn(string quoteNo)
        {
            try
            {
                var isQuoteCancelled = await _unitOfWork.MfgQuotes
                    .AnyAsync(q => q.QuoteNo == quoteNo && q.IsRevisionNo == true);

                return isQuoteCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CheckreviseQuoteByQuoteIdAsyn for QuoteId: {quoteNo}");
                throw;
            }
        }

        public async Task<DataTable> GetMfgLabQuotePendingList(string status)//Shankar
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Status", status ?? (object)DBNull.Value),
                   
                };
                var result = await _report.ExecuteDataTableAsync("Sp_GetSalesandLabQuotePendingList", parameters);
                return result ?? new DataTable();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
       
    }
}
