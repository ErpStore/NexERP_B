using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.PoPendingModel;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using Microsoft.Data.SqlClient;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class MfgPoService : IMfgPoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IExcelTemplateService _excelTemplateService;
       
        public MfgPoService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            IExcelTemplateService excelTemplateService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _excelTemplateService = excelTemplateService;
           
        }



        // 🔹 Customers

        public async Task<bool> GetLineIdByCustomerAsync(int custId)
        {
            return await _commonService.GetLineIdByCustomerAsync(custId);
        }
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
        public async Task<bool> CheckAssemblyExistsAsync(int itemId)
        {
            try
            {
                if (itemId <= 0)
                    throw new ArgumentException("Invalid ItemId. Must be greater than zero.", nameof(itemId));

                return await _unitOfWork.AssmblyDefs
                    .AnyAsync(a => a.AssmblyID == itemId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error checking Assembly existence for ItemId: {itemId}");
                throw;
            }
        }


        // 🔹 Po operations

        public async Task<bool> IsPoTransactionsMatchedAsync(int poId, MfgPoVM mfgPoVMs)
        {
            try
            {
                var poSubIds = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Where(x => x.PoId == poId)
                    .Select(x => x.PoSubId)
                    .ToListAsync();

                bool hasPo = poSubIds.Any();

                bool hasTransactions = false;

                if (hasPo)
                {
                    hasTransactions = await _unitOfWork.MfgDcSubs
                        .GetQueryable()
                        .AnyAsync(pqs => pqs.RefPoSubId.HasValue && poSubIds.Contains(pqs.RefPoSubId.Value));


                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.PerformaInvSubs
                            .GetQueryable()
                            .AnyAsync(pqs => pqs.RefPoSubId.HasValue && poSubIds.Contains(pqs.RefPoSubId.Value));
                    }

                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.RouteCards
                            .GetQueryable()
                            .AnyAsync(rc => rc.RefPoId == poId);
                    }

                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.MaterialReqSubs
                            .GetQueryable()
                            .AnyAsync(mrs => mrs.RefPoSubId.HasValue && poSubIds.Contains(mrs.RefPoSubId.Value));
                    }

                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.JobOrders
                            .GetQueryable()
                            .AnyAsync(jo => jo.RefPoSubId.HasValue && poSubIds.Contains(jo.RefPoSubId.Value));
                    }

                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.ContractReviews
                            .GetQueryable()
                            .AnyAsync(cr => cr.PoId.HasValue && cr.PoId == poId);
                    }

                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.ProductionIssueCompSubs
                            .GetQueryable()
                            .AnyAsync(pcs => pcs.RefPoSubId.HasValue && poSubIds.Contains(pcs.RefPoSubId.Value));
                    }

                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.MfgInvSubs
                            .GetQueryable()
                            .AnyAsync(mis => mis.RefPoSubId.HasValue && poSubIds.Contains(mis.RefPoSubId.Value));
                    }

                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.ExpInvSubs
                            .GetQueryable()
                            .AnyAsync(eis => eis.RefPoSubId.HasValue && poSubIds.Contains(eis.RefPoSubId.Value));
                    }

                }

                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = mfgPoVMs?.MfgPOSubVMs;
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
                await _logs.LogDeveloperError(ex, $"Error while checking Sales Order transactions for PoId: {poId}");
                throw new InvalidOperationException("Failed to verify Sales Order transactions.", ex);
            }
        }
        public async Task<bool> IsPoTransactionsMatchedRowRemoveAsync(int? PoSubId, MfgPoVM mfgPoVMs)
        {
            try
            {
                bool hasTransactions = false;
                bool qtyMismatch = false;


                if (PoSubId.HasValue && PoSubId.Value > 0)
                {
                    bool hasPo = await _unitOfWork.MfgPoSubs
                     .GetQueryable()
                     .AnyAsync(x => x.PoSubId == PoSubId.Value);

                    if (hasPo)
                    {
                        hasTransactions =
                            await _unitOfWork.MfgDcSubs.GetQueryable()
                                .AnyAsync(x => x.RefPoSubId == PoSubId)
                        || await _unitOfWork.PerformaInvSubs.GetQueryable()
                                .AnyAsync(x => x.RefPoSubId == PoSubId)
                        || await _unitOfWork.MaterialReqSubs.GetQueryable()
                                .AnyAsync(x => x.RefPoSubId == PoSubId)
                        || await _unitOfWork.JobOrders.GetQueryable()
                                .AnyAsync(x => x.RefPoSubId == PoSubId)
                        || await _unitOfWork.ProductionIssueCompSubs.GetQueryable()
                                .AnyAsync(x => x.RefPoSubId == PoSubId)
                        || await _unitOfWork.MfgInvSubs.GetQueryable()
                                .AnyAsync(x => x.RefPoSubId == PoSubId)
                        || await _unitOfWork.ExpInvSubs.GetQueryable()
                                .AnyAsync(x => x.RefPoSubId == PoSubId);

                    }

                    var row = mfgPoVMs?.MfgPOSubVMs
                        .FirstOrDefault(x => x.PoSubId == PoSubId);

                    if (row != null)
                    {
                        decimal qty = row.Qty ?? 0;
                        decimal balQty = row.BalQty ?? 0;

                        qtyMismatch = qty != balQty;
                    }
                }

                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking PO transactions for PoSubId: {PoSubId}");
                throw new InvalidOperationException("Failed to verify Sales Order transactions.", ex);
            }
        }

        public async Task<(List<MfgPoVM> mfgPoVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.MfgPos
                    .GetQueryable()
                    .Include(x => x.Customer)
                    .Include(x => x.MfgPoSubs)
                        .ThenInclude(s => s.Item)
                    .Include(x => x.MfgPoSubs)
                        .ThenInclude(s => s.MfgQuoteSub)
                            .ThenInclude(es => es.MfgQuote)
                    .AsQueryable();

                if (filters != null)
                {
                    foreach (var filter in filters)
                    {
                        query = MfgPoFilterBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query.AsNoTracking().AsSplitQuery()
                    .OrderByDescending(x => x.PoId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<MfgPoVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (MfgPo)");
                throw new InvalidOperationException("Failed to load manufacturing PO list.", ex);
            }
        }

        public static class MfgPoFilterBuilder
        {
            public static IQueryable<MfgPo> ApplyFilter(
                IQueryable<MfgPo> query,
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
                        case "Po":
                            {
                                string part1 = val;
                                string part2 = string.Empty;

                                int slashIndex = val.IndexOf('/');
                                if (slashIndex > -1)
                                {
                                    part1 = val[..slashIndex].Trim();
                                    part2 = val[(slashIndex + 1)..].Trim();
                                }

                                return query.Where(x => (string.IsNullOrEmpty(part1) || x.PONo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2)))
                                );
                            }

                        case "QuoteNo":
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
                                    x.MfgPoSubs.Any(s =>
                                        s.MfgQuoteSub != null &&
                                        s.MfgQuoteSub.MfgQuote != null &&
                                        (string.IsNullOrEmpty(part1) ||
                                            s.MfgQuoteSub.MfgQuote.QuoteNo.StartsWith(part1)) &&
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
                                x.MfgPoSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemName.Contains(val)));

                        case "ItemCode":
                            return query.Where(x =>
                                x.MfgPoSubs.Any(s =>
                                    s.Item != null &&
                                    s.Item.ItemCode.Contains(val)));

                        case "CreatedBy":
                            return query.Where(x =>
                                x.CreatedBy != null &&
                                x.CreatedBy.Contains(val));

                        case "FromDate":
                            if (DateTime.TryParse(val, out var fromDate))
                                return query.Where(x => x.PODateNow >= fromDate.Date);
                            return query;

                        case "ToDate":
                            if (DateTime.TryParse(val, out var toDate))
                                return query.Where(x =>
                                    x.PODateNow <= toDate.Date.AddDays(1).AddTicks(-1));
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

            private static IQueryable<MfgPo> ApplyStatusFilter(
                IQueryable<MfgPo> query,
                string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.PoTally),

                        "Pending" =>
                            query.Where(x =>
                                !x.PoTally &&
                                !x.PoCancl &&
                                !x.ShortClose),

                        "Cancelled" =>
                            query.Where(x => x.PoCancl),

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

        public async Task<(bool CanDelete, string Message)> CanDeleteSalesOrderAsync(int poId)
        {
            try
            {
                var po = await _unitOfWork.MfgPos
                                .GetQueryable()
                                .Include(e => e.MfgPoSubs)
                                .Where(e => e.PoId == poId).FirstOrDefaultAsync();

                if (po == null)
                    return (true, "Sales Order can be safely deleted.");

                var PoSubIds = po.MfgPoSubs
                    .Select(es => es.PoSubId)
                    .ToList();

                // Check Sales DC
                bool hasDc = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefPoSubId.HasValue &&
                        PoSubIds.Contains(qs.RefPoSubId.Value));
                if (hasDc)
                    return (false, "Cannot delete this Sales Order as a Sales DC transaction exists.");


                bool hasInvoice = await _unitOfWork.MfgInvSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefPoSubId.HasValue &&
                        PoSubIds.Contains(qs.RefPoSubId.Value));
                if (hasInvoice)
                    return (false, "Cannot delete this Sales Order as a Invoice transaction exists.");

                bool hasExpInvoice = await _unitOfWork.ExpInvSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefPoSubId.HasValue &&
                        PoSubIds.Contains(qs.RefPoSubId.Value));
                if (hasExpInvoice)
                    return (false, "Cannot delete this Sales Order as a Export - Invoice transaction exists.");


                // Check Sales PerformaInvoice
                bool hasPerforma = await _unitOfWork.PerformaInvSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefPoSubId.HasValue &&
                        PoSubIds.Contains(qs.RefPoSubId.Value));

                if (hasPerforma)
                    return (false, "Cannot delete this Sales Order as a Performa-Invoice transaction exists.");

                // Check Sales RC
                bool hasRc = await _unitOfWork.RouteCards.GetQueryable().AnyAsync(qs => qs.RefPoId == poId);
                if (hasRc)
                    return (false, "Cannot delete this Sales Order as a Route-Card transaction exists.");

                // Check Contract Review
                bool hasCR = await _unitOfWork.ContractReviews.GetQueryable().AnyAsync(qs => qs.PoId == poId);
                if (hasCR)
                    return (false, "Cannot delete this Sales Order as a Contract Review transaction exists.");

                // Check Sales MReq
                bool hasMR = await _unitOfWork.MaterialReqSubs.GetQueryable().AnyAsync(qs =>
                        qs.RefPoSubId.HasValue &&
                        PoSubIds.Contains(qs.RefPoSubId.Value));

                if (hasMR)
                    return (false, "Cannot delete this Sales Order as a Purchase Requisition / Material Requisition transaction exists.");

                // chceck JobOrer
                bool hasJO = await _unitOfWork.JobOrders.GetQueryable().AnyAsync(qs =>
                        qs.RefPoSubId.HasValue &&
                        PoSubIds.Contains(qs.RefPoSubId.Value));

                if (hasJO)
                    return (false, "Cannot delete this Sales Order as a Job Order transaction exists.");

                // check Production Component Issue
                bool hasPCI = await _unitOfWork.ProductionIssueCompSubs.GetQueryable().AnyAsync(qs =>
                        qs.RefPoSubId.HasValue &&
                        PoSubIds.Contains(qs.RefPoSubId.Value));
                if (hasPCI)
                    return (false, "Cannot delete this Sales Order as a Production Component Issue transaction exists.");


                if (po.PoCancl || po.ShortClose)
                    return (false, "Cannot delete this Sales Order as it is Cancelled or Short Closed.");

                if (po.MfgPoSubs.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Sales Order as one or more PO items are cancelled.");

                return (true, "Sales Order can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteSalesOrderAsync for PoId: {poId}");
                throw new Exception("Error checking Sales Order delete eligibility", ex);
            }
        }

        public async Task ValidateBeforeRevertAsync(int poSubID)
        {
            try
            {
                var sub = await _unitOfWork.MfgPoSubs.GetAsync(poSubID);

                if (sub == null)
                    throw new InvalidOperationException("Sales PO Item not found.");

                if (sub.RefQuoteSubId > 0)
                    await ValidateQuotationBalanceBeforeRevertAsync(sub);
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

        public async Task<(bool CanItemCancel, string Message)> CanSalesOrderItemCancelCheckAsync(MfgPoSubVM subItem)
        {
            try
            {
                bool hasDc = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefPoSubId.HasValue && qs.RefPoSubId == subItem.PoSubId && !qs.ItemCancel);
                if (hasDc)
                    return (false, "Cannot cancel this Item as a Sales DC transaction exists.");


                bool hasPerforma = await _unitOfWork.PerformaInvSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefPoSubId.HasValue && qs.RefPoSubId == subItem.PoSubId);
                if (hasPerforma)
                    return (false, "Cannot cancel this Item as a Performa-Invoice transaction exists.");


                bool hasRc = await _unitOfWork.RouteCards.GetQueryable().AnyAsync(qs => qs.RefPoId == subItem.PoId);
                if (hasRc)
                    return (false, "Cannot cancel this Item as a Route-Card transaction exists.");


                bool hasCR = await _unitOfWork.ContractReviews.GetQueryable().AnyAsync(qs => qs.PoId == subItem.PoId);
                if (hasRc)
                    return (false, "Cannot cancel this Item as a Contract Review transaction exists.");


                bool hasMR = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefPoSubId.HasValue && qs.RefPoSubId == subItem.PoSubId && !qs.ItemCancel);
                if (hasMR)
                    return (false, "Cannot cancel this Item as a Purchase Requisition / Material Requisition transaction exists.");


                bool hasJO = await _unitOfWork.JobOrders
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefPoSubId.HasValue && qs.RefPoSubId == subItem.PoSubId);
                if (hasJO)
                    return (false, "Cannot cancel this Item as a Job Order transaction exists.");


                bool hasPCI = await _unitOfWork.ProductionIssueCompSubs
                     .GetQueryable()
                     .AnyAsync(qs => qs.RefPoSubId.HasValue && qs.RefPoSubId == subItem.PoSubId);
                if (hasPCI)
                    return (false, "Cannot cancel this item as a Production Component Issue transaction exists.");

                bool hasInvoice = await _unitOfWork.MfgInvSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefPoSubId.HasValue && qs.RefPoSubId == subItem.PoSubId && !qs.ItemCancel);
                if (hasInvoice)
                    return (false, "Cannot cancel this Item as a Invoice transaction exists.");

                bool hasExpInvoice = await _unitOfWork.ExpInvSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefPoSubId.HasValue && qs.RefPoSubId == subItem.PoSubId && !qs.ItemCancel);
                if (hasExpInvoice)
                    return (false, "Cannot cancel this Item as a Export - Invoice transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanSalesOrderItemCancelCheckAsync for PoSubId: {subItem.PoSubId}");
                throw new Exception("Error checking Sales Order Item cancel eligibility", ex);
            }
        }



        public async Task<int> GetPendingQuoteCountAsync(int custId)
        {
            return await _unitOfWork.MfgQuotes
                .GetQueryable()
                .Where(e => e.CustId == custId && e.QuotationTally == false)
                .CountAsync();
        }

        public async Task<bool> GetAutoGenerateSalesOrderFlagAsync()
        {
            try
            {
                var screenSetting = await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(s => s.ScreenName == "SalesOrder" && s.Display == "Auto Generate SalesOrder No.")
                    .Select(s => s.Required)
                    .FirstOrDefaultAsync();

                return screenSetting;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAutoGenerateSalesOrderFlagAsync");
                return false;
            }
        }

        public async Task<List<PoType>> GetAllPoTypesAsync()
        {
            try
            {
                return await _unitOfWork.PoTypes.GetQueryable()
                    .GroupBy(e => e.TypeName)
                    .Select(g => g.First())
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllPoTypesAsync");
                return new List<PoType>();
            }
        }

        public async Task<string> GenerateNextWoNoAsync(IEnumerable<MfgPoSubVM> existingSubItems)
        {
            var previousItem = existingSubItems.LastOrDefault();
            if (previousItem != null && !string.IsNullOrEmpty(previousItem.WoNO))
            {
                return GenerateNextWoNo(previousItem.WoNO);
            }

            var lastWoNoFromDb = await GetLastWoNoAsync();
            if (!string.IsNullOrEmpty(lastWoNoFromDb))
            {
                return GenerateNextWoNo(lastWoNoFromDb);
            }

            return "";
        }

        public async Task<string?> GetLastWoNoAsync()
        {
            return await _unitOfWork.MfgPoSubs.GetQueryable()
                .Where(s => !string.IsNullOrEmpty(s.WoNO))
                .OrderByDescending(s => s.PoSubId)
                .Select(s => s.WoNO)
                .FirstOrDefaultAsync();
        }

        public string GenerateNextWoNo(string lastWoNo)
        {
            if (!string.IsNullOrEmpty(lastWoNo))
            {
                int i = lastWoNo.Length - 1;
                while (i >= 0 && char.IsDigit(lastWoNo[i]))
                    i--;

                string prefix = lastWoNo.Substring(0, i + 1);
                string numericPart = lastWoNo.Substring(i + 1);

                if (int.TryParse(numericPart, out int number))
                {
                    int length = numericPart.Length;
                    return prefix + (number + 1).ToString("D" + length);
                }
            }

            return "";
        }


        public async Task<IEnumerable<MfgPoVM>> GetAllPODetailsAsync()
        {
            try
            {
                var entities = await _unitOfWork.MfgPos.GetAllWithIncludeAsync(q => true,
                    q => q.Customer,
                    q => q.Currency,
                    q => q.PoType,
                    q => q.Consinee,
                    q => q.MfgPoSubs);
                return _mapper.Map<IEnumerable<MfgPoVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllPODetailsAsync");
                return Enumerable.Empty<MfgPoVM>();
            }
        }


        public async Task<bool> HasAnyItemOrPoCancelAsync(int poId)
        {
            try
            {
                var isPoCancelled = await _unitOfWork.MfgPos
                    .AnyAsync(q => q.PoId == poId && q.PoCancl == true);

                var isItemCancelled = await _unitOfWork.MfgPoSubs
                    .AnyAsync(i => i.PoId == poId && i.ItemCancel == true);

                return isPoCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrPoCancelAsync for PoId: {poId}");
                throw;
            }
        }

        public async Task<bool> DeletePOByPOIdAsync(int poId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var po = await _unitOfWork.MfgPos
                    .GetQueryable()
                    .Include(e => e.MfgPoSubs)
                    .FirstOrDefaultAsync(e => e.PoId == poId);

                if (po == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in po.MfgPoSubs)
                {
                    if (sub.RefQuoteSubId > 0)
                    {
                        await AdjustQuoteBalanceAsync(sub.RefQuoteSubId, sub.Qty, 0, "PO Deletion");
                    }
                    else
                    {
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                    }
                }

                await _unitOfWork.MfgPos.DeleteAsync(po);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Sales-Order List",
                    action: $"Deleted Po: {po.PONo}",
                    additionalInfo: $"Po Id: {po.PoId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete PO: {poId}");
                throw;
            }
        }

        public async Task<MfgPoVM?> GetPoByPoIdAsync(int poId)
        {
            try
            {
                var entity = await _unitOfWork.MfgPos.GetQueryable().AsSplitQuery().AsNoTracking()
                    .Include(q => q.MfgPoSubs)
                    .Include(q => q.MfgPoSubs).ThenInclude(s => s.Item)
                    .Include(q => q.MfgPoSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                    .Include(q => q.Currency)
                    .FirstOrDefaultAsync(q => q.PoId == poId);

                return _mapper.Map<MfgPoVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetPoByPoIdAsync({poId})");
                return null;
            }
        }
        public async Task<MfgPo?> GetLastPoAsync(int custId)
        {
            try
            {
                return await _unitOfWork.MfgPos.GetLatestAsync(
                    q => q.CustId == custId,
                    q => q.PoId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastPoAsync for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve last MfgPo. Please try again.");
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
        public async Task<decimal> GetQuoteItemBalQtyFromQuoteSubId(int quoteSubId)
        {
            try
            {
                return await _unitOfWork.MfgQuoteSubs.GetQueryable()
                    .Where(e => e.QuoteSubId == quoteSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for MfgQuoteSubId: {quoteSubId}");
                throw new InvalidOperationException("Failed to retrieve Quotation balance quantity.");
            }
        }
        public async Task<MfgPoSubVM?> GetPoSubItemDetailByPoSubIdAsync(int PoSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Where(q => q.PoSubId == PoSubId)
                    .Select(q => new MfgPoSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Po sub item detail for PoSubId: {PoSubId}");
                throw new InvalidOperationException("Failed to retrieve Po sub-item details.");
            }
        }

        public async Task<bool> IsDuplicatePoAsync(string poNo, string suffix, int? currentPoId = null, int? custId = null)
        {
            if (string.IsNullOrWhiteSpace(poNo))
                return false;

            try
            {
                return await _unitOfWork.MfgPos
                    .GetQueryable()
                    .AnyAsync(x => x.PONo == poNo
                                && x.Suffix == suffix
                                && x.CustId == custId
                                && (currentPoId == null || x.PoId != currentPoId));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in IsDuplicatePoAsync for PoNO: {poNo}");
                throw new InvalidOperationException("Failed to check duplicate PO.");
            }
        }

        public async Task<MfgPoVM> UpsertPoAsync(MfgPoVM mfgPoVM)
        {
            if (mfgPoVM == null)
                throw new ArgumentNullException(nameof(mfgPoVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MfgPo entity;

                if (mfgPoVM.PoId == 0)
                {
                    entity = _mapper.Map<MfgPo>(mfgPoVM);
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.MfgPoSubs = mfgPoVM.MfgPOSubVMs.Select(s => _mapper.Map<MfgPoSub>(s)).ToList();

                    await SetSalesPoAuthorizationStatusAsync(entity, currentUser);

                    await _unitOfWork.MfgPos.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in mfgPoVM.MfgPOSubVMs)
                    {
                        if (subVM.RefQuoteSubId > 0)
                            await AdjustQuoteBalanceAsync(subVM.RefQuoteSubId, 0, subVM.Qty ?? 0, "PO Creation");
                        else
                            await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }

                    changes.AppendLine("Sales Order Created.");
                }
                else
                {
                    entity = await _unitOfWork.MfgPos.GetQueryable()
                        .Include(q => q.MfgPoSubs)
                        .FirstOrDefaultAsync(q => q.PoId == mfgPoVM.PoId)
                        ?? throw new InvalidOperationException("PO not found.");

                    var parentChanges = GetPropertyChanges(entity, mfgPoVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(mfgPoVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await SetSalesPoAuthorizationStatusAsync(entity, currentUser);

                    await HandleChildUpdatesAsync(entity, mfgPoVM.MfgPOSubVMs, changes);

                    changes.AppendLine("Sals Order Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdatePoTallyStatusAsync(mfgPoVM.PoId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, mfgPoVM.PoId == 0 ? "Manufacturing PO Created" : "Manufacturing PO  Updated");

                var savedEntity = await _unitOfWork.MfgPos.GetQueryable()
                    .Include(q => q.MfgPoSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.Currency)
                    .Include(q => q.MfgPoSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.PoId == entity.PoId);

                return _mapper.Map<MfgPoVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Manufacturing PO: {mfgPoVM.PONo}");
                throw new InvalidOperationException("Failed to save Manufacturing PO. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(MfgPo existingPo, List<MfgPoSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {
                var existingSubIds = existingPo.MfgPoSubs.Select(s => s.PoSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.PoSubId).ToHashSet();

                // DELETE removed children
                foreach (var sub in existingPo.MfgPoSubs.Where(s => !incomingSubIds.Contains(s.PoSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - PoSubId: {sub.PoSubId}, Item: {sub.Item?.ItemCode}");

                    await _unitOfWork.MfgPoSubs.DeleteAsync(sub.PoSubId);
                    await _unitOfWork.SaveAsync();

                    if (sub.RefQuoteSubId > 0)
                        await AdjustQuoteBalanceAsync(sub.RefQuoteSubId, sub.Qty, 0, "PO Deletion");
                    else
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                }

                // ADD / UPDATE children
                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.PoSubId == 0)
                    {
                        var newSub = _mapper.Map<MfgPoSub>(subVM);
                        newSub.PoId = existingPo.PoId;

                        await _unitOfWork.MfgPoSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                        if (subVM.RefQuoteSubId > 0)
                            await AdjustQuoteBalanceAsync(subVM.RefQuoteSubId, 0, subVM.Qty ?? 0, "PO Creation");
                        else
                            await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }
                    else
                    {
                        var existingSub = existingPo.MfgPoSubs.FirstOrDefault(s => s.PoSubId == subVM.PoSubId);
                        if (existingSub != null)
                        {
                            if ((existingSub.CostId != subVM.CostId) &&
                                (existingSub.RefQuoteSubId == null || existingSub.RefQuoteSubId == 0))
                            {
                                await UpdateCostCenterAsync(existingSub.CostId, false, changes);
                            }

                            if (subVM.RefQuoteSubId > 0)
                                await AdjustQuoteBalanceAsync(subVM.RefQuoteSubId, existingSub.Qty, subVM.Qty ?? 0, "PO Update");

                            if ((subVM.CostId > 0) &&
                                (subVM.RefQuoteSubId == null || subVM.RefQuoteSubId == 0))
                            {
                                await UpdateCostCenterAsync(subVM.CostId, true, changes);
                            }

                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                            {
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");
                            }

                            _mapper.Map(subVM, existingSub);
                            await _unitOfWork.MfgPoSubs.UpdateAsync(existingSub);
                        }
                    }
                }
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error occurred while handling PO Sub items update.");
                throw new InvalidOperationException("Failed to update PO Sub items. Check logs for details.");
            }
        }

        private async Task<List<BOMItem>> LoadBomHierarchyAsync(int assemblyId)
        {
            var result = new List<BOMItem>();
            await LoadRecursive(assemblyId, result);
            return result;
        }

        private async Task LoadRecursive(int assemblyId, List<BOMItem> result, decimal parentUtil = 1)
        {
            var children = await _unitOfWork.AssmblyDefs
                .GetQueryable()
                .Where(x => x.AssmblyID == assemblyId)
                .ToListAsync();

            foreach (var child in children)
            {
                decimal util = Math.Round(child.UtilQty, 3);
                decimal cumUtil = Math.Round(parentUtil * util, 3);

                var bomItem = new BOMItem
                {
                    ItemID = child.ItemId,
                    UtilQty = util,
                    CumulativeUtilQty = cumUtil
                };

                result.Add(bomItem);

                bool isSubAssembly = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .AnyAsync(i => i.ItemId == child.ItemId &&
                                   (i.CategoryCode == 3 || i.CategoryCode == 7));

                if (isSubAssembly)
                {
                    await LoadRecursive(child.ItemId, result, cumUtil);
                }
            }
        }



        private async Task UpdateCostCenterAsync(int? costCenterId, bool assign, StringBuilder changes)
        {
            if (!costCenterId.HasValue || costCenterId == 0) return;

            var cost = await _unitOfWork.CostCenters.GetAsync(costCenterId.Value);
            if (cost != null && cost.AssToMfgPO != assign)
            {
                cost.AssToMfgPO = assign;
                await _unitOfWork.CostCenters.UpdateAsync(cost);
                await _unitOfWork.SaveAsync();
                changes.AppendLine($"CostCenter Updated - ProjectNo {cost.ProjectNo}: AssToMfgPo set to '{assign}'");
            }
        }

        private async Task AdjustQuoteBalanceAsync(int? refQuoteSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refQuoteSubId.HasValue || refQuoteSubId == 0) return;

                var quoteSub = await _unitOfWork.MfgQuoteSubs.GetAsync(refQuoteSubId.Value);
                if (quoteSub == null) return;

                if (oldQty > 0)
                    quoteSub.BalQty += oldQty;

                if (newQty > quoteSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Quote BalQty.");

                if (newQty > 0)
                    quoteSub.BalQty -= newQty;

                await _unitOfWork.MfgQuoteSubs.UpdateAsync(quoteSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(e => e.QuoteId == quoteSub.QuoteId && !e.ItemCancel)
                    .SumAsync(e => e.BalQty);

                var quotation = await _unitOfWork.MfgQuotes.GetAsync(quoteSub.QuoteId);
                if (quotation != null)
                {
                    quotation.QuotationTally = (totalBalQty == 0);
                    await _unitOfWork.MfgQuotes.UpdateAsync(quotation);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustQuoteBalance] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustQuoteBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust Quote balance. Please contact support.");
            }
        }

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

        public async Task UpsertSalesOrderShortCloseAsync(MfgPoVM mfgPoVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingPo = await _unitOfWork.MfgPos.GetAsync(mfgPoVM.PoId);
                if (existingPo == null)
                    throw new InvalidOperationException("Sales Order not found.");

                existingPo.ShortClose = mfgPoVM.ShortClose;

                await _unitOfWork.MfgPos.UpdateAsync(existingPo);
                await _unitOfWork.SaveAsync();

                await UpdatePoTallyStatusAsync(mfgPoVM.PoId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSalesOrderShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSalesQuoteShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(MfgPoSubVM subItem, int poId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.MfgPoSubs.GetQueryable().Where
                                (x => x.PoSubId == subItem.PoSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with PoSubId {subItem.PoSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateQuotationBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.MfgPoSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    await AdjustQuoteBalanceAsync(subEntity.RefQuoteSubId, subEntity.Qty, 0, $"Quotation Item Cancel - {subItem.ItemCode}");
                }
                else
                {
                    await AdjustQuoteBalanceAsync(subEntity.RefQuoteSubId, 0, subEntity.Qty, $"Quotation Item Revert Cancel - {subItem.ItemCode}");
                }

                await UpdatePoTallyStatusAsync(poId);

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



        public async Task UpdatePoTallyStatusAsync(int poId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Where(x => x.PoId == poId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var po = await _unitOfWork.MfgPos.GetAsync(poId);
                if (po == null)
                    return;

                if (po.ShortClose || po.PoCancl)
                    return;

                po.PoTally = (totalBalQty == 0);

                await _unitOfWork.MfgPos.UpdateAsync(po);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdatePoTallyStatusAsync] Error updating PoId {poId}");
                throw new InvalidOperationException("Failed to update Po Tally status. Please contact support.");
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(MfgPoVM poVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingPo = await _unitOfWork.MfgPos.GetAsync(poVM.PoId);
                if (existingPo == null)
                    throw new InvalidOperationException("Manufacturing PO not found.");

                var subs = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Where(s => s.PoId == poVM.PoId)
                    .ToListAsync();

                if (!poVM.PoCancl)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateQuotationBalanceBeforeRevertAsync(sub);
                    }
                }

                existingPo.PoCancl = poVM.PoCancl;
                existingPo.CancelReason = poVM.CancelReason;
                existingPo.CancelDate = poVM.CancelDate;
                existingPo.CancelBy = poVM.CancelBy;

                await _unitOfWork.MfgPos.UpdateAsync(existingPo);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingPo.PoCancl)
                    {
                        if (sub.RefQuoteSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustQuoteBalanceAsync(
                                sub.RefQuoteSubId.Value,
                                sub.Qty,
                                0,
                                $"Manufacturing PO Cancelled - {existingPo.PONo}"
                            );
                        }
                    }
                    else
                    {
                        if (sub.RefQuoteSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustQuoteBalanceAsync(
                                sub.RefQuoteSubId.Value,
                                0,
                                sub.Qty,
                                $"Manufacturing PO Reverted - {existingPo.PONo}"
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

        public async Task ValidateQuotationBalanceBeforeRevertAsync(MfgPoSub? sub)
        {
            if (sub.RefQuoteSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.MfgQuoteSubs.GetAsync(sub.RefQuoteSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"Quotation not found for RefQuoteSubId: {sub.RefQuoteSubId}");

            if (entity.BalQty < sub.Qty && !entity.ItemCancel)
            {
                throw new InvalidOperationException(
                    $"Cannot revert because Quotation balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                );
            }

        }

        public async Task DeleteAndResequenceAsync(MfgPoSubVM subitem, MfgPoVM poVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.PoSubId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.MfgPoSubs.GetAsync(subitem.PoSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.RefQuoteSubId > 0)
                    {
                        await AdjustQuoteBalanceAsync(subitem.RefQuoteSubId, entity.Qty, 0, "PO Deletion");
                    }
                    else
                    {
                        await UpdateCostCenterAsync(subitem.CostId, false, changes);
                    }

                    await _unitOfWork.MfgPoSubs.DeleteAsync(entity.PoSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Mfg PO",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"PO No: {poVM?.PONo}"
                    );
                }
                else
                {
                    poVM.MfgPOSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Where(x => x.PoId == poVM.PoId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                await UpdatePoTallyStatusAsync(poVM.PoId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<MfgPoSub>> GetPOSubByPoIdAsync(int poId)
        {
            try
            {
                var subs = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.PoId == poId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Manufacturing PO items for PoId: {poId}");
                throw new InvalidOperationException("Failed to retrieve Sales Order sub-items. Please try again.");
            }
        }
        public async Task<List<Dictionary<string, object>>> GetQuoteDetailsByCustId(int custId, bool mfgOrLab)
        {
            try
            {
                var result = await (from q in _unitOfWork.MfgQuotes.GetQueryable()
                                    join qs in _unitOfWork.MfgQuoteSubs.GetQueryable()
                                        on q.QuoteId equals qs.QuoteId
                                    where q.CustId == custId && !q.QuotationTally && !q.IsCancel && !q.ShortClose && qs.BalQty > 0
                                    && !qs.ItemCancel && q.MfgOrLab == mfgOrLab && q.IsAuthorized == true
                                    select new
                                    {
                                        qs.QuoteSubId,
                                        q.QuoteNo,
                                        q.Suffix,
                                        q.QuoteDate,
                                        qs.ItemId,
                                        qs.Item.ItemCode,
                                        qs.Item.ItemName,
                                        qs.Item.MeasureUnit,
                                        qs.Item.HSNCode,
                                        qs.Qty,
                                        qs.BalQty,
                                        qs.UnitPrice,
                                        CostCenterId = qs.CostId == 0 ? (int?)null : qs.CostId,
                                        qs.CostCenter.ProjectNo,
                                        q.Customer
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["QuoteSubId"] = r.QuoteSubId,
                    ["QuoteNo"] = $"{r.QuoteNo}{r.Suffix}",
                    ["QuoteDate"] = r.QuoteDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Customer"] = r.Customer.CustName ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Quotation details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve Quotation details. Please try again.");
            }
        }

        public async Task<string> GetNextSaleOrderNoAsync(int poTypeId)
        {
            try
            {
                var query = _unitOfWork.MfgPos.GetQueryable().Where(x => x.PoTypeId == poTypeId);

                if (await query.AnyAsync())
                {
                    var lastOrderNo = await query
                        .OrderByDescending(x => x.PoId)
                        .Select(x => x.SaleOrderNo)
                        .FirstOrDefaultAsync();

                    if (!string.IsNullOrEmpty(lastOrderNo))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(lastOrderNo, @"(\d+)$");
                        if (match.Success)
                        {
                            int lastNumber = int.Parse(match.Value);
                            int nextNumber = lastNumber + 1;

                            string prefix = lastOrderNo.Substring(0, match.Index);
                            return $"{prefix}{nextNumber.ToString(new string('0', match.Value.Length))}";
                        }
                    }

                    return await GetSeriesNoAsync(poTypeId) ?? "SO-0001";
                }
                else
                {
                    return await GetSeriesNoAsync(poTypeId) ?? "SO-0001";
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating SaleOrderNo for PoTypeId {poTypeId}");
                return "SO-0001";
            }
        }

        private async Task<string?> GetSeriesNoAsync(int poTypeId)
        {
            return await _unitOfWork.PoTypes
                .GetQueryable()
                .Where(p => p.Id == poTypeId)
                .Select(p => p.SeriesNo)
                .FirstOrDefaultAsync();
        }

        public async Task UpdateItemCancelAsync(MfgPoSubVM subItem)
        {
            if (subItem == null)
                throw new ArgumentNullException(nameof(subItem), "Subitem cannot be null.");

            if (subItem.PoSubId == 0)
                throw new ArgumentException("Cannot update unsaved subitem.");

            try
            {
                var entity = await _unitOfWork.MfgPoSubs
                    .FirstOrDefaultAsync(x => x.PoSubId == subItem.PoSubId);

                if (entity == null)
                    throw new KeyNotFoundException($"Subitem with PoSubId {subItem.PoSubId} not found.");

                entity.ItemCancel = subItem.ItemCancel;

                await _unitOfWork.MfgPoSubs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAsync for ItemCode {subItem.ItemCode}");
                throw;
            }
        }
        public async Task<string> GetNextOANumberAsync(string suffix)
        {
            try
            {
                var lastPo = await _unitOfWork.MfgPos
                    .GetQueryable()
                    .Where(q => !string.IsNullOrEmpty(q.OANo) && q.Suffix == suffix)
                    .OrderByDescending(q => q.PoId)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;

                if (lastPo != null)
                {
                    var parts = lastPo.OANo.Split('/');
                    if (parts.Length > 0 && int.TryParse(parts[0], out int lastNumber))
                    {
                        nextNumber = lastNumber + 1;
                    }
                }

                return $"{nextNumber}{suffix}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating OA number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Order Acceptance number.");
            }
        }
        public async Task<string> GetLastOATermsAsync()
        {
            try
            {
                var lastTerms = await _unitOfWork.MfgPos
                    .GetQueryable()
                    .Where(p => !string.IsNullOrEmpty(p.OATerms))
                    .OrderByDescending(p => p.PoId)
                    .Select(p => p.OATerms)
                    .FirstOrDefaultAsync();

                return lastTerms ?? string.Empty;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching last OA Terms");
                return string.Empty;
            }
        }



        public async Task<List<MfgPoSubVM>> GetDistinctRefQuoteByPoIdAsync(int poId)
        {
            return await _unitOfWork.MfgPoSubs
                .GetQueryable()
                .Where(s => s.PoId == poId)
                .GroupBy(s => new { s.MfgQuoteSub.MfgQuote.QuoteNo, s.MfgQuoteSub.MfgQuote.QuoteDate })
                .Select(g => new MfgPoSubVM
                {
                    RefQuoteNo = g.Key.QuoteNo,
                    RefQuoteDate = g.Key.QuoteDate
                })
                .ToListAsync();
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

        private async Task SetSalesPoAuthorizationStatusAsync(MfgPo entity, string currentUser)
        {
            var prAuthorityExists = await _unitOfWork.UserAuthorities
                .AnyAsync(x => x.IsSalesPo == true);

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

        public async Task<bool> IsLabourPoByDefault()
        {
            try
            {
                var screenSetting = await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(s => s.ScreenName == "Sales-Order" && s.Display == "Enable Labour Button by Default")
                    .Select(s => s.Required)
                    .FirstOrDefaultAsync();
                return screenSetting;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsLabourPoByDefault");
                return false;
            }
        }

        public async Task<List<MfgPoPendingVM>> GetMfgPosPendingList(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<MfgPoPendingVM>("Sp_GetMfgPosPendingList", status);
                return result.ToList();  
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> IsDocumentUploaded(int poid)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Manufacturing-PO" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == poid);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }
    }

}
