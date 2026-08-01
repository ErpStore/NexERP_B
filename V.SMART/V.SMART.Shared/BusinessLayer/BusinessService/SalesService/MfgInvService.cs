using AutoMapper;
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
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.InvoiceAutoRunning;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.SalesAndLabour.Export;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.CreditNote_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ReportViewModel.SalesStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class MfgInvService : IMfgInvService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        private readonly IStockManagerService _stockManagerService;
        

        public MfgInvService(
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

        //screens

        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
                 => await _commonService.GetScreenCodeByScreenNameAsync(screenName);


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

        //Stock Manager
        public async Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId)
            => await _stockManagerService.GetStockForItemAsync(ItemId, StoreId);

        //Mapped Store
        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
           => await _commonService.GetAllActiveStoresAsync();

        //GenerateQrCode
        public async Task<string> GenerateQrBase64(string signedQrText)
          => await _commonService.GenerateQrBase64(signedQrText);

        public async Task<bool> CheckPrefixValid(string ScreenName)
            => await _commonService.CheckPrefixValid(ScreenName);

        public async Task<bool> CheckEwayRequired(string ScreenName)
            => await _commonService.CheckEwayRequired(ScreenName);
        public async Task<string> GetBasicDirectory()
            => await _commonService.GetBasicDirectory();


        // 🔹 Mfg Invoice operations

        public async Task<(List<MfgInvVM> mfgInvVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.MfgInvs.GetQueryable()
                .Include(j => j.Customer)
                .Include(j => j.MfgInvSubs)
                    .ThenInclude(s => s.Item)
                .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = MfgInvFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.InvId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<MfgInvVM>>(list);

            return (vmList, total);
        }

        public static class MfgInvFilterBuilder
        {
            public static IQueryable<MfgInv> ApplyFilter(
                IQueryable<MfgInv> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "InvNo":
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
                                (string.IsNullOrEmpty(part1) || x.InvNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }
                    case "RefDcNo":
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
                                     x.MfgInvSubs.Any(s =>
                                         (string.IsNullOrEmpty(part1) || s.MfgDcSub.MfgDc.DcNo.StartsWith(part1)) &&
                                         (string.IsNullOrEmpty(part2) || s.MfgDcSub.MfgDc.Suffix.Contains(part2))
                                     )
                                 );
                        }
                    case "RefPoNo":
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
                                     x.MfgInvSubs.Any(s =>
                                         (string.IsNullOrEmpty(part1) || s.MfgPoSub.MfgPo.PONo.StartsWith(part1)) &&
                                         (string.IsNullOrEmpty(part2) || s.MfgPoSub.MfgPo.Suffix.Contains(part2))
                                     )
                                 );
                            
                        }


                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(value.ToString()));
                    case "ItemCode":
                        return query.Where(x => x.MfgInvSubs.Any(s => s.Item.ItemCode.Contains(value.ToString())));
                    case "ItemName":
                        return query.Where(x => x.MfgInvSubs.Any(s => s.Item.ItemName.Contains(value.ToString())));
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
            private static IQueryable<MfgInv> ApplyStatusFilter(
                IQueryable<MfgInv> query,
                string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.InvTally),

                        "Pending" =>
                            query.Where(x =>
                                !x.InvTally &&
                                !x.IsCancel &&
                                !x.PoShortClose),

                        "Cancelled" =>
                            query.Where(x => x.IsCancel),

                        "Short-Closed" =>
                            query.Where(x => x.PoShortClose),

                        _ => query
                    };
                }
                catch
                {
                    return query;
                }
            }

        }

        public async Task<int> GetPendingDcCountAsync(int custId)
        {
            return await _unitOfWork.MfgDcs
                .GetQueryable()
                .Where(e => e.CustId == custId && e.DcTally == false)
                .CountAsync();
        }

        public async Task<int> GetPendingPoCountAsync(int custId)
        {
            return await _unitOfWork.MfgPos
                .GetQueryable()
                .Where(e => e.CustId == custId && e.PoTally == false)
                .CountAsync();
        }

        public async Task<MfgInvVM?> GetInvoiceByInvIdAsync(int InvId)
        {
            try
            {
                var entity = await _unitOfWork.MfgInvs.GetQueryable().AsNoTracking().AsSplitQuery()
                    .Include(q => q.MfgInvSubs)
                    .Include(q => q.MfgInvSubs).ThenInclude(s => s.Item)
                    .Include(q => q.MfgInvSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.MfgInvSubs).ThenInclude(s => s.MfgDcSub).ThenInclude(m => m.MfgDc)
                    .Include(q => q.MfgInvSubs).ThenInclude(s => s.MfgPoSub).ThenInclude(m => m.MfgPo)
                    .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                    .Include(q => q.Currency)
                    .Include(s => s.StoreIssue)
                    .Include(j => j.MfgInvSubs).ThenInclude(ps => ps.CreditNoteSubs).ThenInclude(p => p.CreditNote)
                    .FirstOrDefaultAsync(q => q.InvId == InvId);

                return _mapper.Map<MfgInvVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetInvoiceByInvIdAsync({InvId})");
                return null;
            }
        }

        public async Task<MfgInv?> GetLastInvoiceAsync(int custId)
        {
            try
            {
                return await _unitOfWork.MfgInvs.GetLatestAsync(
                    q => q.CustId == custId,
                    q => q.InvId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastInvoiceAsync for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve last Invoice. Please try again.");
            }
        }

        public async Task<bool> IsDuplicateMfgInvoiceNoAsync(string InvNo, string suffix, int? currentInvId = null, int? CustId = null)
        {
            if (string.IsNullOrWhiteSpace(InvNo))
                return false;
            try
            {
                return await _unitOfWork.MfgInvs
                    .GetQueryable()
                    .AnyAsync(x => x.InvNo == InvNo
                                && x.Suffix == suffix
                                && x.CustId == CustId
                                && (currentInvId == null || x.InvId != currentInvId));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in IsDuplicateMfgInvoiceNoAsync forInv No: {InvNo}");
                throw new InvalidOperationException("Failed to check duplicate MfgInvoice Invoice.");
            }
        }

        public async Task<MfgInvVM> UpsertInvoiceAsync(MfgInvVM invoiceVM, int screenCode)
        {
            if (invoiceVM == null)
                throw new ArgumentNullException(nameof(invoiceVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MfgInv entity;

                if (invoiceVM.InvId == 0)
                {
                    entity = _mapper.Map<MfgInv>(invoiceVM);

                    // 🔹 Get last number with locking from repository
                    //var NextNumber = await _unitOfWork.MfgInvs.GetLastInvNoAsync(entity.Suffix);
                    //entity.InvNo = NextNumber;

                    //--------AutoDcrunning-------------------------------------------------
                    if (invoiceVM.IsManualInvNo ?? true)
                    {
                        var runningRow = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.InvoiceType == "MFGINV" &&
                                x.Suffix == entity.Suffix);

                        if (runningRow == null)
                        {
                            var dcrunn = new InvoiceAutoRunningNumber
                            {
                                LastNumber = Convert.ToInt64(entity.InvNo),
                                InvoiceType = "MFGINV",
                                Suffix = entity.Suffix
                            };

                            await _unitOfWork.InvoiceAutoRunningNumbers.CreateAsync(dcrunn);
                        }
                        else
                        {
                            runningRow.LastNumber = Convert.ToInt64(entity.InvNo);
                            await _unitOfWork.InvoiceAutoRunningNumbers.UpdateAsync(runningRow);
                        }

                        await _unitOfWork.SaveAsync();
                    }
                    else
                    {
                        entity.InvNo = await _commonService.GenerateInvoiceAutoRunningNoAsync("MFGINV", entity.Suffix);
                    }
                    //-------------------------------------------------------------------------------------------------

                    bool check = await IsDuplicateMfgInvoiceNoAsync(entity.InvNo, entity.Suffix, entity.InvId, entity.CustId);
                    if (check)
                    {
                        throw new Exception("Duplicate Invoice Number found for this customer.");
                    }


                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.Balance = invoiceVM.GrandTotal;
                    entity.MfgInvSubs = invoiceVM.MfgSubInvVMs.Select(s => _mapper.Map<MfgInvSub>(s)).ToList();

                    await _unitOfWork.MfgInvs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    int RefDcSubId = 0, RefPoSubId = 0;
                    foreach (var subVM in entity.MfgInvSubs)
                    {
                        int.TryParse(subVM.RefDcSubId.ToString(), out RefDcSubId);
                        int.TryParse(subVM.RefPoSubId.ToString(), out RefPoSubId);
                        if (RefDcSubId > 0)
                        {
                            await AdjustMfgDcBalanceAsync(subVM.RefDcSubId, 0, subVM.Qty, "Invoice Creation");
                        }
                        else if (RefPoSubId > 0)
                        {
                            await AdjustMfgPoBalanceAsync(subVM.RefPoSubId, 0, subVM.Qty, "Invoice Creation");
                            if (subVM.RcSubIds != "" && subVM.RcSubIds != null)
                            {
                                await IssueStockBySeqLogicAsync(subVM, entity, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, entity.StoreIssId.Value, (subVM.Qty), subVM.UnitPrice,
                                       "", screenCode, subVM.InvSubId, entity.InvNo, entity.InvDate, null, false);
                            }

                        }
                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }

                    changes.AppendLine("Invoice Created.");
                }
                else
                {
                    entity = await _unitOfWork.MfgInvs.GetQueryable()
                        .Include(q => q.MfgInvSubs)
                        .FirstOrDefaultAsync(q => q.InvId == invoiceVM.InvId)
                        ?? throw new InvalidOperationException("Invoice not found.");

                    var parentChanges = GetPropertyChanges(entity, invoiceVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(invoiceVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.Balance = invoiceVM.GrandTotal;
                    await HandleChildUpdatesAsync(entity, invoiceVM.MfgSubInvVMs, changes, screenCode);

                    changes.AppendLine("Quotation Updated.");
                }


                await _unitOfWork.SaveAsync();

                await UpdateMfgInvoiceTallyStatusAsync(invoiceVM.InvId);

                // ===== TDS Deduction =====
                var tdsInfo = await _unitOfWork.Customers.GetQueryable()
                    .Where(v => v.CustId == entity.CustId)
                    .Select(v => new { v.TdsDeduct })
                    .FirstOrDefaultAsync();

                if (tdsInfo?.TdsDeduct == true)
                {
                    await AutoDeductTDSForCustomerAsync(entity);
                }

                await transaction.CommitAsync();

                await LogChangesAsync(changes, invoiceVM.InvId == 0 ? "Invoice Created" : "Invoice Updated");

                var savedEntity = await _unitOfWork.MfgInvs.GetQueryable()
                    .Include(q => q.MfgInvSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.Currency)
                    .Include(q => q.MfgInvSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.InvId == entity.InvId);

                return _mapper.Map<MfgInvVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Invoice: {invoiceVM.InvNo}");
                throw new InvalidOperationException("Failed to save Invoice. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(MfgInv existingInv, List<MfgInvSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingInv.MfgInvSubs.Select(s => s.InvSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.InvSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingInv.MfgInvSubs.Where(s => !incomingSubIds.Contains(s.InvSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - InvSubId: {sub.InvSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.MfgInvSubs.DeleteAsync(sub.InvSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefDcSubId > 0)
                {
                    await AdjustMfgDcBalanceAsync(sub.RefDcSubId, sub.Qty, 0, "Invoice Deletion");
                }
                else if (sub.RefPoSubId > 0)
                {
                    await AdjustMfgPoBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Invoice Deletion");

                    await DeleteStockIssueAndTrackAsync(sub.InvSubId, sub.ItemId.Value, screenCode);
                }
                else
                    await UpdateCostCenterAsync(sub.CostId, false, changes);
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.InvSubId == 0)
                {
                    var newSub = _mapper.Map<MfgInvSub>(subVM);
                    newSub.InvId = existingInv.InvId;
                    await _unitOfWork.MfgInvSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    if (subVM.RefDcSubId > 0)
                    {
                        await AdjustMfgDcBalanceAsync(subVM.RefDcSubId, 0, subVM.Qty ?? 0, "Invoice Creation");
                    }
                    else if (subVM.RefPoSubId > 0)
                    {
                        await AdjustMfgPoBalanceAsync(subVM.RefPoSubId, 0, subVM.Qty ?? 0, "Invoice Creation");

                        if (subVM.RcSubIds != "" && subVM.RcSubIds != null)
                        {
                            await IssueStockBySeqLogicAsync(newSub, existingInv, screenCode);
                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId, existingInv.StoreIssId.Value, (subVM.Qty ?? 0), subVM.UnitPrice.Value,
                                      "", screenCode, newSub.InvSubId, existingInv.InvNo, existingInv.InvDate, null, false);
                        }

                    }

                    else
                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                }
                else
                {
                    var existingSub = existingInv.MfgInvSubs.FirstOrDefault(s => s.InvSubId == subVM.InvSubId);
                    if (existingSub != null)
                    {
                        // Rollback old CostCenter if needed
                        if ((existingSub.CostId != subVM.CostId) && (existingSub.RefDcSubId == null || existingSub.RefDcSubId == 0))
                            await UpdateCostCenterAsync(existingSub.CostId, false, changes);


                        if (subVM.RefDcSubId > 0)
                        {
                            await AdjustMfgDcBalanceAsync(subVM.RefDcSubId, existingSub.Qty, subVM.Qty ?? 0, "Invoice Update");
                        }
                        else if (subVM.RefPoSubId > 0)
                        {
                            await AdjustMfgPoBalanceAsync(subVM.RefPoSubId, existingSub.Qty, subVM.Qty ?? 0, "Invoice Update");

                            if (subVM.RcSubIds != "" && subVM.RcSubIds != null)
                            {
                                existingSub.Qty = subVM.Qty ?? existingSub.Qty;
                                await IssueStockBySeqLogicAsync(existingSub, existingInv, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId, existingInv.StoreIssId.Value, (subVM.Qty ?? 0), subVM.UnitPrice.Value,
                                      "", screenCode, subVM.InvSubId, existingInv.InvNo, existingInv.InvDate, null, false);
                            }

                        }

                        // Assign new CostCenter if needed
                        if ((subVM.CostId > 0) && (subVM.RefDcSubId == null || subVM.RefDcSubId == 0))
                            await UpdateCostCenterAsync(subVM.CostId, true, changes);


                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
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
                changes.AppendLine($"CostCenter Updated - ProjectNo {cost.ProjectNo}: AssToInvoice set to '{assign}'");
            }
        }

        private async Task AdjustMfgDcBalanceAsync(int? refDcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refDcSubId.HasValue || refDcSubId == 0) return;

                var DcSub = await _unitOfWork.MfgDcSubs.GetAsync(refDcSubId.Value);
                if (DcSub == null) return;

                if (oldQty > 0)
                    DcSub.BalQty += oldQty;

                if (newQty > DcSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Dc BalQty.");

                if (newQty > 0)
                    DcSub.BalQty -= newQty;

                await _unitOfWork.MfgDcSubs.UpdateAsync(DcSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.MfgDcSubs
                    .GetQueryable()
                    .Where(e => e.DcId == DcSub.DcId && !e.ItemCancel)
                    .SumAsync(e => e.BalQty);

                var dc = await _unitOfWork.MfgDcs.GetAsync(DcSub.DcId);
                if (dc != null)
                {
                    dc.DcTally = (totalBalQty == 0);
                    await _unitOfWork.MfgDcs.UpdateAsync(dc);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustDcBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustDcBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Dc Balance. Please contact support.");
            }
        }

        private async Task AdjustMfgPoBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refPoSubId.HasValue || refPoSubId == 0) return;

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
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPOBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPOBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust PO Balance. Please contact support.");
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
                screen: "Invoice Sales",
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

        public async Task<bool> HasAnyItemOrInvoiceCancelAsync(int InvId)
        {
            try
            {
                var isInvCancelled = await _unitOfWork.MfgInvs
                    .AnyAsync(q => q.InvId == InvId && q.IsCancel == true);

                var isItemCancelled = await _unitOfWork.MfgInvSubs
                    .AnyAsync(i => i.InvId == InvId && i.ItemCancel == true);

                return isInvCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrInvoiceCancelAsync for InvId: {InvId}");
                throw;
            }
        }

        public async Task DeleteAndResequenceAsync(MfgInvSubVM subitem, MfgInvVM quote, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.InvSubId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.MfgInvSubs.GetAsync(subitem.InvSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    // Restore balance qty
                    if (entity.RefDcSubId > 0)
                    {
                        await AdjustMfgDcBalanceAsync(subitem.RefDcSubId, entity.Qty, 0, "Invoice Deletion");
                    }
                    else if (entity.RefPoSubId > 0)
                    {
                        await AdjustMfgPoBalanceAsync(subitem.RefPoSubId, entity.Qty, 0, "Invoice Deletion");

                        await DeleteStockIssueAndTrackAsync(subitem.InvSubId, subitem.ItemId, screenCode);

                    }
                    else
                    {
                        await UpdateCostCenterAsync(subitem.CostId, false, changes);
                    }

                    // Delete from DB
                    await _unitOfWork.MfgInvSubs.DeleteAsync(entity.InvSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Mfg Invoice",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Invoice No: {quote?.InvNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    quote.MfgSubInvVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.MfgInvSubs
                    .GetQueryable()
                    .Where(x => x.InvId == quote.InvId)
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

        public async Task<bool> DeleteInvoiceByInvIdAsync(int InvId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var quotation = await _unitOfWork.MfgInvs
                    .GetQueryable()
                    .Include(e => e.MfgInvSubs)
                    .FirstOrDefaultAsync(e => e.InvId == InvId);

                if (quotation == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                foreach (var sub in quotation.MfgInvSubs)
                {
                    if (sub.RefDcSubId > 0)
                    {
                        await AdjustMfgDcBalanceAsync(sub.RefDcSubId, sub.Qty, 0, "Invoice Deletion");
                    }
                    else if (sub.RefPoSubId > 0)
                    {
                        await AdjustMfgPoBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Invoice Deletion");

                        await DeleteStockIssueAndTrackAsync(sub.InvSubId, sub.ItemId.Value, screenCode);
                    }
                    else
                    {
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                    }
                }

                //----------------AutoInvRunning-------------------------------------
                var runningRow = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.InvoiceType == "MFGINV" &&
                                x.Suffix == quotation.Suffix);
                if (runningRow != null)
                {
                    long oldInvNo = 0;
                    long.TryParse(quotation.InvNo.ToString(), out oldInvNo);
                    if (runningRow.LastNumber == oldInvNo )
                    {
                        runningRow.LastNumber = (oldInvNo - 1);
                        await _unitOfWork.InvoiceAutoRunningNumbers.UpdateAsync(runningRow);
                    }
                }

                // Delete the quotation
                var deleted = await _unitOfWork.MfgInvs.DeleteAsync(InvId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "MfgInvoice List",
                    action: $"Deleted Invoice: {quotation.InvNo}",
                    additionalInfo: $"Invoice Id: {quotation.InvId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Invoice Id: {InvId}");
                throw;
            }
        }

        public async Task<List<MfgInvSubVM>> GetInvoiceSubByInvIdAsync(int InvId)
        {
            try
            {
                var subs = await _unitOfWork.MfgInvSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.InvId == InvId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<MfgInvSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching MfgInvSub items for QuoteId: {InvId}");
                throw new InvalidOperationException("Failed to retrieve Invoice sub-items. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetDcDetailsByCustId(int custId)
        {
            try
            {
                var result = await (from e in _unitOfWork.MfgDcs.GetQueryable()
                                    join es in _unitOfWork.MfgDcSubs.GetQueryable()
                                        on e.DcId equals es.DcId
                                    where e.CustId == custId && !e.DcTally && !e.DcCancel && es.BalQty > 0 && !es.ItemCancel && es.Remark.ToUpper() != "NV" && !e.DcShortClose
                                    select new
                                    {
                                        es.DcSubId,
                                        e.DcNo,
                                        e.DcDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.Specification,
                                        es.Item.MeasureUnit,
                                        es.Item.HSNCode,
                                        es.Item.Category.CategoryName,
                                        es.Qty,
                                        es.BalQty,
                                        UnitPrice = (es.RefPoSubId > 0 && es.MfgPoSub != null)
                                                    ? es.MfgPoSub.UnitPrice
                                                    : es.UnitPrice,

                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo,

                                        PoLineDiscountPercent = es.RefPoSubId > 0 && es.MfgPoSub != null
                                                                ? es.MfgPoSub.LineDiscountPercent
                                                                : 0m,
                                        PoLineDiscountAmount = es.RefPoSubId > 0 && es.MfgPoSub != null
                                                                ? es.MfgPoSub.LineDiscountAmount
                                                                : 0m,

                                        PoLineCGSTRate = es.RefPoSubId > 0 && es.MfgPoSub != null
                                                        ? es.MfgPoSub.LineCGSTRate
                                                        : 0m,

                                        PoLineSGSTRate = es.RefPoSubId > 0 && es.MfgPoSub != null
                                                        ? es.MfgPoSub.LineSGSTRate
                                                        : 0m,

                                        PoLineIGSTRate = es.RefPoSubId > 0 && es.MfgPoSub != null
                                                        ? es.MfgPoSub.LineIGSTRate
                                                        : 0m,
                                        e.Customer,

                                    }).ToListAsync();


                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["DcSubId"] = r.DcSubId,
                    ["DcNo"] = r.DcNo,
                    ["DcDate"] = r.DcDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["Specification"] = r.Specification ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Category"] = r.CategoryName ?? string.Empty,
                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["PoLineDiscountPercent"] = r.PoLineDiscountPercent,
                    ["PoLineDiscountAmount"] = r.PoLineDiscountAmount,
                    ["PoLineCGSTRate"] = r.PoLineCGSTRate,
                    ["PoLineSGSTRate"] = r.PoLineSGSTRate,
                    ["PoLineIGSTRate"] = r.PoLineIGSTRate,
                    ["Customer"] = r.Customer.CustName ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching enquiry details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve enquiry details. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId, int storeIssId)
        {
            try
            {
                var poLines = await (from e in _unitOfWork.MfgPos.GetQueryable()
                                     join es in _unitOfWork.MfgPoSubs.GetQueryable()
                                         on e.PoId equals es.PoId
                                     where e.CustId == custId && !e.PoTally && !e.PoCancl && es.BalQty > 0 && !es.ItemCancel && !e.ShortClose   
                                     select new
                                     {
                                         es.PoSubId,
                                         es.PoId,
                                         e.PONo,
                                         e.PODate,
                                         es.ItemId,
                                         es.Item.ItemCode,
                                         es.Item.ItemName,
                                         es.Item.Specification,
                                         es.Item.MeasureUnit,
                                         es.Item.HSNCode,
                                         es.Item.Category.CategoryName,
                                         es.Qty,
                                         es.BalQty,
                                         es.UnitPrice,
                                         CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                         es.CostCenter.ProjectNo,

                                         PoLineDiscountPercent = es.LineDiscountPercent,

                                         PoLineDiscountAmount = es.LineDiscountAmount,

                                         PoLineCGSTRate = es.LineCGSTRate,

                                         PoLineSGSTRate = es.LineSGSTRate,

                                         PoLineIGSTRate = es.LineIGSTRate,
                                         e.Customer,
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


                //var itemStock = await _stockManagerService.GetStockForItemsAsync(itemIds, storeIssId);
                var itemStock = await _unitOfWork.StockAdds.GetQueryable()
                                .Where(s =>
                                    s.StoreId == storeIssId && (s.RcSubID == null || s.RcSubID == 0) &&
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
                        ["PoId"] = po.PoId,
                        ["PONo"] = po.PONo,
                        ["PODate"] = po.PODate,
                        ["ItemId"] = po.ItemId,
                        ["ItemCode"] = po.ItemCode ?? string.Empty,
                        ["ItemName"] = po.ItemName ?? string.Empty,
                        ["Specification"] = po.Specification ?? string.Empty,
                        ["UOM"] = po.MeasureUnit ?? string.Empty,
                        ["HSNCode"] = po.HSNCode ?? string.Empty,
                        ["Category"] = po.CategoryName ?? string.Empty,
                        ["Qty"] = po.Qty,
                        ["BalQty"] = po.BalQty,
                        ["UnitPrice"] = po.UnitPrice,
                        ["Amount"] = (po.UnitPrice * po.BalQty),
                        ["CostCenterId"] = po.CostCenterId ?? (int?)null,
                        ["ProjectNo"] = po.ProjectNo ?? string.Empty,
                        ["StockQty"] = stockQty,
                        ["RcSubId"] = hasRouteCard ? string.Join(",", rcSubIds) : "",

                        ["PoLineDiscountPercent"] = po.PoLineDiscountPercent,
                        ["PoLineDiscountAmount"] = po.PoLineDiscountAmount,
                        ["PoLineCGSTRate"] = po.PoLineCGSTRate,
                        ["PoLineSGSTRate"] = po.PoLineSGSTRate,
                        ["PoLineIGSTRate"] = po.PoLineIGSTRate,
                        ["Customer"] = po.Customer.CustName ?? string.Empty,
                    });
                }

                return data;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching PO details for CustId: {custId}"
                );

                throw new InvalidOperationException(
                    "Failed to retrieve PO details. Please try again."
                );
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

                    rate = await (from qs in _unitOfWork.MfgInvSubs.GetQueryable()
                                  join q in _unitOfWork.MfgInvs.GetQueryable() on qs.InvId equals q.InvId
                                  where qs.ItemId == itemId && q.CustId == custId
                                  orderby q.InvId descending
                                  select qs.UnitPrice)
                                 .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.MfgInvSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.InvSubId descending
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
                var prefix = await _unitOfWork.MfgInvs.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.InvId)
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

        public async Task<string> GetInvoiceNumberAsync(string suffix)
        {
            try
            {
                string nextNumber = await _commonService.GeneratePreviewInvoiceAutoRunningNoAsync("MFGINV", suffix);

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Invoice number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate invoice number.");
            }
        }

        public async Task<decimal> GetDcItemBalQtyFromDcSubId(int DcSubId)
        {
            try
            {
                return await _unitOfWork.MfgDcSubs.GetQueryable()
                    .Where(e => e.DcSubId == DcSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for DcSubId: {DcSubId}");
                throw new InvalidOperationException("Failed to retrieve MfgDc balance quantity.");
            }
        }

        public async Task<decimal> GetPoItemBalQtyFromDcSubId(int PoSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(e => e.PoSubId == PoSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for PoSubId: {PoSubId}");
                throw new InvalidOperationException("Failed to retrieve MfgPo balance quantity.");
            }
        }


        public async Task<MfgInvSubVM?> GetInvSubItemDetailByInvSubIdAsync(int InvSubId)
        {
            try
            {
                return await _unitOfWork.MfgInvSubs
                    .GetQueryable()
                    .Where(q => q.InvSubId == InvSubId)
                    .Select(q => new MfgInvSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Invoice sub item detail for InvSubId: {InvSubId}");
                throw new InvalidOperationException("Failed to retrieve Mfg Invoice sub-item details.");
            }
        }

        public async Task<List<MfgInvSubVM>> GetDistinctRefEnquiriesByQuoteIdAsync(int InvId)
        {
            return await _unitOfWork.MfgInvSubs
                .GetQueryable()
                .Include(x => x.MfgDcSub)
                .ThenInclude(x => x.MfgDc)
                .Where(s => s.InvId == InvId)
                .GroupBy(s => new { s.MfgDcSub.MfgDc.DcNo, s.MfgDcSub.MfgDc.Suffix, s.MfgDcSub.MfgDc.DcDate })
                .Select(g => new MfgInvSubVM
                {
                    RefDcNo = $"{g.Key.DcNo}{g.Key.Suffix}",
                    RefDcDate = g.Key.DcDate
                })
                .ToListAsync();
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(MfgInvSubVM subItem, int screenCode)
        {

            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.MfgInvSubs.GetQueryable().Where
                                (x => x.InvSubId == subItem.InvSubId).FirstOrDefaultAsync();

                var existingInv = await _unitOfWork.MfgInvs.GetQueryable().Where
                                (x => x.InvId == subItem.InvId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with InvSubId {subItem.InvSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateInvoiceBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.MfgInvSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();


                if (subItem.ItemCancel && subItem.RefDcSubId > 0)
                {
                    await AdjustMfgDcBalanceAsync(subEntity.RefDcSubId, subEntity.Qty, 0, $"Invoice Item Cancel - {subItem.ItemCode}");
                }
                else
                {
                    await AdjustMfgDcBalanceAsync(subEntity.RefDcSubId, 0, subEntity.Qty, $"Invoice Item Revert Cancel - {subItem.ItemCode}");
                }


                if (subItem.ItemCancel && subItem.RefPoSubId > 0)
                {
                    await AdjustMfgPoBalanceAsync(subEntity.RefPoSubId, subEntity.Qty, 0, $"Invoice Item Cancel - {subItem.ItemCode}");

                    await DeleteStockIssueAndTrackAsync(subEntity.InvSubId, subEntity.ItemId.Value, screenCode);
                }
                else
                {
                    await AdjustMfgPoBalanceAsync(subEntity.RefPoSubId, 0, subEntity.Qty, $"Invoice Item Revert Cancel - {subItem.ItemCode}");

                    if (subEntity.RcSubIds != "" && subEntity.RcSubIds != null)
                    {
                        await IssueStockBySeqLogicAsync(subEntity, existingInv, screenCode);
                    }

                    await _stockManagerService.IssueOrUpdateStockAsync(subEntity.ItemId.Value, existingInv.StoreIssId.Value, (subEntity.Qty), subEntity.UnitPrice,
                                  "", screenCode, subEntity.InvSubId, existingInv.InvNo, existingInv.InvDate, null, false);
                }


                await UpdateMfgInvoiceTallyStatusAsync(subEntity.InvId);

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

        public async Task UpdateMfgInvoiceTallyStatusAsync(int InvId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.MfgInvSubs
                    .GetQueryable()
                    .Where(x => x.InvId == InvId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var invoice = await _unitOfWork.MfgInvs.GetAsync(InvId);
                if (invoice == null)
                    return;

                invoice.InvTally = (totalBalQty == 0);

                await _unitOfWork.MfgInvs.UpdateAsync(invoice);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateInvoiceTallyStatusAsync] Error updating InvId {InvId}");
                throw new InvalidOperationException("Failed to update Invoice Tally status. Please contact support.");
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(MfgInvVM invoiceVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingInvoice = await _unitOfWork.MfgInvs.GetAsync(invoiceVM.InvId);
                if (existingInvoice == null)
                    throw new InvalidOperationException("Manufacturing Invoice not found.");

                var subs = await _unitOfWork.MfgInvSubs
                    .GetQueryable()
                    .Where(s => s.InvId == invoiceVM.InvId)
                    .ToListAsync();

                if (!invoiceVM.IsCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateInvoiceBalanceBeforeRevertAsync(sub);
                    }
                }

                existingInvoice.IsCancel = invoiceVM.IsCancel;
                existingInvoice.CancelReason = invoiceVM.CancelReason;
                await _unitOfWork.MfgInvs.UpdateAsync(existingInvoice);
                await _unitOfWork.SaveAsync();
                int RefDcSubId = 0, RefPoSubId = 0;
                foreach (var sub in subs)
                {
                    int.TryParse(sub.RefDcSubId.ToString(), out RefDcSubId);
                    int.TryParse(sub.RefPoSubId.ToString(), out RefPoSubId);
                    if (existingInvoice.IsCancel)
                    {
                        if (RefDcSubId > 0)
                        {
                            await AdjustMfgDcBalanceAsync(sub.RefDcSubId.Value, sub.Qty, 0, $"Manufacturing Invoice Cancelled - {existingInvoice.InvNo}");
                        }

                        if (RefPoSubId > 0)
                        {
                            await AdjustMfgPoBalanceAsync(sub.RefPoSubId.Value, sub.Qty, 0, $"Manufacturing Invoice Cancelled - {existingInvoice.InvNo}");

                            await DeleteStockIssueAndTrackAsync(sub.InvSubId, sub.ItemId.Value, screenCode);
                        }
                    }
                    else
                    {
                        if (RefDcSubId > 0)
                        {
                            await AdjustMfgDcBalanceAsync(sub.RefDcSubId.Value, 0, sub.Qty, $"Manufacturing Invoice Reverted - {existingInvoice.InvNo}");
                        }

                        if (RefPoSubId > 0)
                        {
                            await AdjustMfgPoBalanceAsync(sub.RefPoSubId.Value, 0, sub.Qty, $"Manufacturing Invoice Reverted - {existingInvoice.InvNo}");

                            if (sub.RcSubIds != "" && sub.RcSubIds != null)
                            {
                                await IssueStockBySeqLogicAsync(sub, existingInvoice, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId.Value, existingInvoice.StoreIssId.Value, (sub.Qty), sub.UnitPrice,
                                "", screenCode, sub.InvSubId, existingInvoice.InvNo, existingInvoice.InvDate, null, false);
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

        public async Task ValidateInvoiceBalanceBeforeRevertAsync(MfgInvSub sub)
        {
            int RefDcSubId = 0, RefPoSubId = 0;

            int.TryParse(sub.RefDcSubId.ToString(), out RefDcSubId);
            int.TryParse(sub.RefPoSubId.ToString(), out RefPoSubId);


            if (RefDcSubId > 0 || RefPoSubId > 0)
            {

                if (RefDcSubId > 0)
                {
                    var entity = await _unitOfWork.MfgDcSubs.GetAsync(sub.RefDcSubId.Value);
                    if (entity == null)
                        throw new InvalidOperationException($"Dc not found for RefDcSubId: {sub.RefDcSubId}");

                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Dc balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
                if (RefPoSubId > 0)
                {
                    var entity = await _unitOfWork.MfgPoSubs.GetAsync(sub.RefPoSubId.Value);
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

        public async Task<List<MfgInvSub>> GetInvoiceSubDetailsByInvIdAsync(int InvId)
        {
            try
            {
                var subs = await _unitOfWork.MfgInvSubs
                                 .GetQueryable()
                                 .Where(s => s.InvId == InvId)
                                 .OrderBy(s => s.SlNo)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Mfg Invoice  items for InvId: {InvId}");
                throw new InvalidOperationException("Failed to retrieve Mfg Invoice sub-items. Please try again.");
            }
        }

        private async Task IssueStockBySeqLogicAsync(MfgInvSub sub, MfgInv parent, int screenCode)
        {
            decimal remainingQty = sub.Qty;
            if (remainingQty <= 0)
                return;


            var existingIssues = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(x =>
                    x.SubItemRefID == sub.InvSubId &&
                    x.ScreenCode == screenCode)
                .ToListAsync();

            foreach (var issue in existingIssues)
                await _stockManagerService.DeleteStockIssueAsync(issue.IssueId);

            // =====================================================
            // RE-ISSUE
            // =====================================================
            //var issueSubIds = await GetIssueSourceRCSubIdsAsync(
            //    sub.ComponentRouteCardSub.RCId,
            //    sub.ComponentRouteCardSub.SeqNo.Value,
            //    sub.RcSubId.Value);

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
                    parent.StoreIssId,
                    rsubid);

                if (available <= 0)
                    continue;

                var issueQty = Math.Min(available, remainingQty);

                await _stockManagerService.IssueOrUpdateStockAsync(
                    sub.ItemId.Value,
                    parent.StoreIssId.Value,
                    issueQty,
                    sub.UnitPrice,
                    null,
                    screenCode,
                    sub.InvSubId,
                    parent.InvNo,
                    parent.InvDate,
                    rsubid,
                    allowMultipleIssue: true);

                remainingQty -= issueQty;
            }

            if (remainingQty > 0)
                throw new InvalidOperationException(
                    $"Insufficient RC stock after adjustment. Remaining Qty: {remainingQty}");
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

        #region Auto TDS Deducts

        //-------TDS----------------
        public async Task<bool> UpdateTDSAmountAsync(MfgInvVM mfgInvoiceVM)
        {
            try
            {
                var changes = new StringBuilder();

                MfgInv entity;

                entity = await _unitOfWork.MfgInvs
                                    .GetQueryable()
                                    .FirstOrDefaultAsync(x => x.InvId == mfgInvoiceVM.InvId && x.InvNo == mfgInvoiceVM.InvNo && x.Suffix == mfgInvoiceVM.Suffix);

                if (entity == null)
                    return false;


                var parentChanges = GetPropertyChanges(entity, mfgInvoiceVM);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                entity.TDSAmount = mfgInvoiceVM.TDSAmount;
                entity.Balance = (entity.GrandTotal) - mfgInvoiceVM.TDSAmount;

                if (entity.Balance < 0)
                    entity.Balance = 0;

                await _unitOfWork.MfgInvs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();

                await LogChangesAsync(changes, mfgInvoiceVM.InvId == 0 ? "Mfg Invoice Created" : "Mfg Invoice TDS  Updated");

                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error Updating TDS Amount");
                return false;
            }
        }

        public async Task<bool> AutoDeductTDSForCustomerAsync(MfgInv mfgInvoice)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetQueryable()
                    .FirstOrDefaultAsync(v => v.CustId == mfgInvoice.CustId);

                if (customer == null || !customer.TdsDeduct)
                    return false;

                decimal exemptAmt = customer.TDSExmptdAmt;
                decimal tdsRate = customer.TDSDeductper;
                decimal maxBill = customer.TDSBillval;

                if (tdsRate <= 0)
                    return false;

                // 🔹 Check if any previous invoice already deducted TDS
                bool hasPreviousTDS = await _unitOfWork.MfgInvs.GetQueryable()
                    .AnyAsync(x => x.CustId == mfgInvoice.CustId && x.TDSAmount > 0);

                if (hasPreviousTDS)
                {
                    ApplyTDS(mfgInvoice, tdsRate);
                    await _unitOfWork.MfgInvs.UpdateAsync(mfgInvoice);
                    await _unitOfWork.SaveAsync();
                    return true;
                }

                // 🔹 Calculate total purchase INCLUDING current invoice
                decimal totalPurchase = await _unitOfWork.MfgInvs.GetQueryable()
                    .Where(x => x.CustId == mfgInvoice.CustId)
                    .SumAsync(x => x.TotalTaxable);

                bool thresholdCrossed = totalPurchase >= exemptAmt || (maxBill > 0 && mfgInvoice.TotalTaxable >= maxBill);

                if (!thresholdCrossed)
                    return true;

                // 🔹 Apply TDS to ALL vendor invoices
                var invoices = await _unitOfWork.MfgInvs.GetQueryable()
                    .Where(x => x.CustId == mfgInvoice.CustId)
                    .ToListAsync();

                foreach (var inv in invoices)
                {
                    ApplyTDS(inv, tdsRate);
                    await _unitOfWork.MfgInvs.UpdateAsync(inv);
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

        private void ApplyTDS(MfgInv invoice, decimal tdsRate)
        {
            if (invoice == null || tdsRate <= 0)
                return;

            invoice.TDSAmount = Math.Round(invoice.TotalTaxable * tdsRate / 100, 2);
            invoice.Balance = invoice.GrandTotal - invoice.TDSAmount;

            if (invoice.Balance < 0)
                invoice.Balance = 0;
        }
        #endregion

        //LC Details

        public async Task<bool> UpdateLcDetailsAsync(MfgInvVM mfgInvoiceVM)
        {
            try
            {
                var changes = new StringBuilder();

                MfgInv entity;

                entity = await _unitOfWork.MfgInvs
                                    .GetQueryable()
                                    .FirstOrDefaultAsync(x => x.InvId == mfgInvoiceVM.InvId && x.InvNo == mfgInvoiceVM.InvNo && x.Suffix == mfgInvoiceVM.Suffix);

                if (entity == null)
                    return false;


                var parentChanges = GetPropertyChanges(entity, mfgInvoiceVM);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                entity.LcNo = mfgInvoiceVM.LcNo;
                entity.LcDate = mfgInvoiceVM.LcDate;
                entity.LcExpiryDate = mfgInvoiceVM.LcExpiryDate;

                await _unitOfWork.MfgInvs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();

                await LogChangesAsync(changes, mfgInvoiceVM.InvId == 0 ? "Mfg Lc Updated" : "Mfg Lc Updated");

                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error Updating TDS Amount");
                return false;
            }
        }


        //------Short Close----------
        public async Task<MfgInvVM> UpdateMfgInvShortCloseAsync(MfgInvVM mfgInvVM)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MfgInv entity;

                entity = await _unitOfWork.MfgInvs
                             .GetQueryable()
                             .Include(e => e.MfgInvSubs)
                             .FirstOrDefaultAsync(e => e.InvId == mfgInvVM.InvId)
                             ?? throw new InvalidOperationException("Manufacturing Invoice not found.");

                _mapper.Map(mfgInvVM, entity);

                var parentChanges = GetPropertyChanges(entity, mfgInvVM);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                _mapper.Map(mfgInvVM, entity);
                entity.ModifiedBy = currentUser;
                entity.ModifiedDate = now;

                await _unitOfWork.MfgInvs.UpdateAsync(entity);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
                await LogChangesAsync(changes, mfgInvVM.InvId == 0 ? "Short Close Manufacturing Invoice Created" : "ReOpen the Manufacturing Invoice");

                // Return updated entity
                var savedEntity = await _unitOfWork.MfgInvs
                    .GetQueryable()
                    .Include(e => e.MfgInvSubs).ThenInclude(s => s.Item)
                    .Include(e => e.MfgInvSubs).ThenInclude(s => s.CostCenter)
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.InvId == entity.InvId);

                return _mapper.Map<MfgInvVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpdateMfgInvShortCloseAsync for InvId: {mfgInvVM.InvId}");
                throw;
            }

        }

        public async Task<bool> HasAnyItemOrInvoiceCreditNoteAsync(int RefSubInvId)
        {
            try
            {
                var isExist = await (from inv in _unitOfWork.CreditNotes.GetQueryable()
                                     join sub in _unitOfWork.CreditNoteSubs.GetQueryable()
                                     on inv.CrId equals sub.CrId
                                     where sub.RefInvSubId == RefSubInvId && inv.Sales==true
                                     select inv.CrId).AnyAsync();

                return isExist;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrInvoiceCreditNoteAsync for RefSubInvId: {RefSubInvId}");
                throw;
            }
        }

        public async Task<List<ManufacturingInvoiceStatusListVM>> GetMfgInvoiceStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<ManufacturingInvoiceStatusListVM>("Sp_GetManufacturingInvoiceStatusList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        //------------**********Invoice Details for EWAY *****-----------------------\\

        public async Task<List<EWayDocument>> GetMfgInvByCustidAsync(int custId)
        {
            try
            {
                var data = await _unitOfWork.MfgInvs.GetQueryable()
                    .AsNoTracking()
                    .Where(dc =>
                        dc.CustId == custId &&
                        !string.IsNullOrEmpty(dc.InvNo) && dc.DcCumInv &&
                        (dc.EWayNo == null || dc.EWayNo == "0" || dc.EWayNo == "")
                    )
                    .OrderBy(dc => dc.InvId)
                    .Select(dc => new EWayDocument
                    {
                        Id = dc.InvId,
                        DocNo = dc.InvNo + dc.Suffix,
                        Suffix = dc.Suffix,
                        DocDate = dc.InvDate,
                        CustName = dc.Customer.CustName
                    })
                    .ToListAsync();


                return data;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetMfgInvByCustidAsync({custId})");
                return new List<EWayDocument>();
            }
        }
    }
}
