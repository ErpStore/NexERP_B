using AutoMapper;
using DocumentFormat.OpenXml.InkML;
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
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.SalesAndLabour.Export;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ExpInv_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using V.SMART.Shared.ViewModels.ReportViewModel.SalesStatusVM;
using static V.SMART.Shared.BusinessLayer.BusinessService.SalesService.MfgInvService;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class ExpInvService : IExpInvService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        private readonly IStockManagerService _stockManagerService;


        public ExpInvService(
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
                var customer = await _commonService.GetExportCustomerByIdAsync(custId.Value);
                return customer != null ? new List<CustomerVM> { customer } : new List<CustomerVM>();
            }
            return await _commonService.GetExportAllActiveCustomersAsync();
        }

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchExportCustomersAsync(searchText);
        }

        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetExportCustomerByIdAsync(custId);

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


        //--------------------------------------------------------
        public async Task<string> GenerateQrBase64(string signedQrText)
           => await _commonService.GenerateQrBase64(signedQrText);

        public async Task<bool> CheckPrefixValid(string ScreenName)
           => await _commonService.CheckPrefixValid(ScreenName);

        public async Task<bool> CheckEwayRequired(string ScreenName)
            => await _commonService.CheckEwayRequired(ScreenName);
        //-------------------------------------------------------


        // 🔹 Export Invoice operations

        public async Task<(List<ExpInvVM> expInvVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ExpInvs.GetQueryable()
                .Include(j => j.Customer)
                .Include(j => j.ExpInvSubs)
                    .ThenInclude(s => s.Item)
                .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = ExpInvFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.ExpInvId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<ExpInvVM>>(list);

            return (vmList, total);
        }

        public async Task<bool> HasAnyItemOrExpInvoiceCancelAsync(int ExpInvId)
        {
            try
            {
                var isInvCancelled = await _unitOfWork.ExpInvs
                    .AnyAsync(q => q.ExpInvId == ExpInvId && q.IsCancel == true);

                var isItemCancelled = await _unitOfWork.ExpInvSubs
                    .AnyAsync(i => i.ExpInvId == ExpInvId && i.ItemCancel == true);

                return isInvCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrExpInvoiceCancelAsync for ExpInvId: {ExpInvId}");
                throw;
            }
        }

        public async Task<bool> DeleteExpInvoiceByInvIdAsync(int ExpInvId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var expinv = await _unitOfWork.ExpInvs
                    .GetQueryable()
                    .Include(e => e.ExpInvSubs)
                    .FirstOrDefaultAsync(e => e.ExpInvId == ExpInvId);

                if (expinv == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                foreach (var sub in expinv.ExpInvSubs)
                {
                    if (sub.RefDcSubId > 0)
                    {
                        await AdjustMfgDcBalanceAsync(sub.RefDcSubId, sub.Qty, 0, "Invoice Deletion");
                    }
                    else if (sub.RefPoSubId > 0)
                    {
                        await AdjustMfgPoBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Invoice Deletion");

                        await DeleteStockIssueAndTrackAsync(sub.ExpInvSubId, sub.ItemId.Value, screenCode);
                    }
                    else
                    {
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                    }
                }

                //----------------AutoExpInvRunning-------------------------------------
                var runningRow = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.InvoiceType == "EXPINV" &&
                                x.Suffix == expinv.Suffix);
                if (runningRow != null)
                {
                    long oldExpInvNo = 0;
                    long.TryParse(expinv.ExpInvNo.ToString(), out oldExpInvNo);
                    if (runningRow.LastNumber == oldExpInvNo)
                    {
                        runningRow.LastNumber = (oldExpInvNo - 1);
                        await _unitOfWork.InvoiceAutoRunningNumbers.UpdateAsync(runningRow);
                    }
                }

                //----------------------------------------------------------------------------

                // Delete the quotation
                var deleted = await _unitOfWork.ExpInvs.DeleteAsync(ExpInvId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Export Invoice List",
                    action: $"Deleted Invoice: {expinv.ExpInvNo}",
                    additionalInfo: $"Invoice Id: {expinv.ExpInvId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete export Invoice Id: {ExpInvId}");
                throw;
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

        public async Task<ExpInvVM?> GetExpInvoiceByExpInvIdAsync(int ExpInvId)
        {
            try
            {
                var entity = await _unitOfWork.ExpInvs.GetQueryable()
                    .Include(q => q.ExpInvSubs)
                    .Include(q => q.ExpInvSubs).ThenInclude(s => s.Item)
                    .Include(q => q.ExpInvSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.ExpInvSubs).ThenInclude(s => s.MfgDcSub).ThenInclude(m => m.MfgDc)
                    .Include(q => q.ExpInvSubs).ThenInclude(s => s.MfgPoSub).ThenInclude(m => m.MfgPo)
                    .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                    .Include(q => q.Currency)
                    .Include(s => s.StoreIssue)
                    .Include(j => j.ExpInvSubs).ThenInclude(ps => ps.CreditNoteSubs).ThenInclude(p => p.CreditNote)
                    .FirstOrDefaultAsync(q => q.ExpInvId == ExpInvId);

                return _mapper.Map<ExpInvVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetInvoiceByInvIdAsync({ExpInvId})");
                return null;
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

                    rate = await (from qs in _unitOfWork.ExpInvSubs.GetQueryable()
                                  join q in _unitOfWork.ExpInvs.GetQueryable() on qs.ExpInvId equals q.ExpInvId
                                  where qs.ItemId == itemId && q.CustId == custId
                                  orderby q.ExpInvId descending
                                  select qs.UnitPrice)
                                 .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.ExpInvSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.ExpInvSubId descending
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
                var prefix = await _unitOfWork.ExpInvs.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.ExpInvId)
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

        public async Task<string> GetExpInvoiceNumberAsync(string suffix)
        {
            try
            {
                string nextNumber = await _commonService.GeneratePreviewInvoiceAutoRunningNoAsync("EXPINV", suffix);

                return $"{nextNumber}";


            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating ExpInvoice number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate ExpInvoice number.");
            }
        }

        public async Task<ExpInv?> GetLastExpInvoiceAsync(int custId)
        {
            try
            {
                return await _unitOfWork.ExpInvs.GetLatestAsync(
                    q => q.CustId == custId,
                    q => q.ExpInvId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastExpInvoiceAsync for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve last ExpInvoice. Please try again.");
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

        public async Task<List<ExpInvSub>> GetExpInvoiceSubByInvIdAsync(int ExpInvId)
        {
            try
            {
                var subs = await _unitOfWork.ExpInvSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.ExpInvId == ExpInvId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ExpInvSub>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ExpInv items for ExpInvId: {ExpInvId}");
                throw new InvalidOperationException("Failed to retrieve ExpInv sub-items. Please try again.");
            }
        }

        public async Task<ExpInvSubVM?> GetInvSubItemDetailByInvSubIdAsync(int ExpInvSubId)
        {
            try
            {
                return await _unitOfWork.ExpInvSubs
                    .GetQueryable()
                    .Where(q => q.ExpInvSubId == ExpInvSubId)
                    .Select(q => new ExpInvSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Invoice sub item detail for ExpInvSubId: {ExpInvSubId}");
                throw new InvalidOperationException("Failed to retrieve ExpInv sub-item details.");
            }
        }

        public async Task DeleteAndResequenceAsync(ExpInvSubVM subitem, ExpInvVM expinv, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.ExpInvSubId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.ExpInvSubs.GetAsync(subitem.ExpInvSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    // Restore balance qty
                    if (entity.RefDcSubId > 0)
                    {
                        await AdjustMfgDcBalanceAsync(subitem.RefDcSubId, entity.Qty, 0, "Export Invoice Deletion");
                    }
                    else if (entity.RefPoSubId > 0)
                    {
                        await AdjustMfgPoBalanceAsync(subitem.RefPoSubId, entity.Qty, 0, "Export Invoice Deletion");

                        await DeleteStockIssueAndTrackAsync(subitem.ExpInvSubId, subitem.ItemId, screenCode);

                    }
                    else
                    {
                        await UpdateCostCenterAsync(subitem.CostId, false, changes);
                    }

                    // Delete from DB
                    await _unitOfWork.ExpInvSubs.DeleteAsync(entity.ExpInvSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Export Invoice",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"ExpInvoice No: {expinv?.ExpInvNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    expinv.ExpSubInvVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.ExpInvSubs
                    .GetQueryable()
                    .Where(x => x.ExpInvId == expinv.ExpInvId)
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

        public async Task ValidateExpInvoiceBalanceBeforeRevertAsync(ExpInvSub sub)
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

        public async Task UpdatedCancelStatusAndAddOrRevertQty(ExpInvVM expinvoiceVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingInvoice = await _unitOfWork.ExpInvs.GetAsync(expinvoiceVM.ExpInvId);
                if (existingInvoice == null)
                    throw new InvalidOperationException("Manufacturing Invoice not found.");

                var subs = await _unitOfWork.ExpInvSubs
                    .GetQueryable()
                    .Where(s => s.ExpInvId == expinvoiceVM.ExpInvId)
                    .ToListAsync();

                if (!expinvoiceVM.IsCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateExpInvoiceBalanceBeforeRevertAsync(sub);
                    }
                }

                existingInvoice.IsCancel = expinvoiceVM.IsCancel;
                existingInvoice.CancelReason = expinvoiceVM.CancelReason;
                await _unitOfWork.ExpInvs.UpdateAsync(existingInvoice);
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
                            await AdjustMfgDcBalanceAsync(sub.RefDcSubId.Value, sub.Qty, 0, $"Export Invoice Cancelled - {existingInvoice.ExpInvNo}");
                        }

                        if (RefPoSubId > 0)
                        {
                            await AdjustMfgPoBalanceAsync(sub.RefPoSubId.Value, sub.Qty, 0, $"Export Invoice Cancelled - {existingInvoice.ExpInvNo}");

                            await DeleteStockIssueAndTrackAsync(sub.ExpInvSubId, sub.ItemId.Value, screenCode);
                        }
                    }
                    else
                    {
                        if (RefDcSubId > 0)
                        {
                            await AdjustMfgDcBalanceAsync(sub.RefDcSubId.Value, 0, sub.Qty, $"Export Invoice Reverted - {existingInvoice.ExpInvNo}");
                        }

                        if (RefPoSubId > 0)
                        {
                            await AdjustMfgPoBalanceAsync(sub.RefPoSubId.Value, 0, sub.Qty, $"Export Invoice Reverted - {existingInvoice.ExpInvNo}");

                            if (sub.RcSubIds != "" && sub.RcSubIds != null)
                            {
                                await IssueStockBySeqLogicAsync(sub, existingInvoice, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId.Value, existingInvoice.StoreIssId.Value, (sub.Qty), sub.UnitPrice,
                                "", screenCode, sub.ExpInvSubId, existingInvoice.ExpInvNo, existingInvoice.ExpInvDate, null, false);
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

        private async Task IssueStockBySeqLogicAsync(ExpInvSub sub, ExpInv parent, int screenCode)
        {
            try
            {
                decimal remainingQty = sub.Qty;
                if (remainingQty <= 0)
                    return;


                var existingIssues = await _unitOfWork.StockIssues
                    .GetQueryable()
                    .Where(x =>
                        x.SubItemRefID == sub.ExpInvSubId &&
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
                        sub.ExpInvSubId,
                        parent.ExpInvNo,
                        parent.ExpInvDate,
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

        public async Task<List<ExpInvSubVM>> GetExpInvoiceDetailsSubByInvIdAsync(int ExpInvId)
        {
            try
            {
                var subs = await _unitOfWork.ExpInvSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.ExpInvId == ExpInvId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ExpInvSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ExpInvSub items for ExpInvId: {ExpInvId}");
                throw new InvalidOperationException("Failed to retrieve Export Invoice sub-items. Please try again.");
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
                                        es.Item.PartWeight,
                                        es.Qty,
                                        es.BalQty,
                                        es.UnitPrice,
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
                                        e.Customer


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
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["PartWeight"] = r.PartWeight,

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
                                         es.Item.PartWeight,
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
                        ["PartWeight"] = po.PartWeight,

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
                await _logs.LogDeveloperError(ex,$"Error fetching PO details for CustId: {custId}"
                );

                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.");
            }
        }

        public async Task<bool> IsDuplicateExportNoAsync(string ExpInvNo, string suffix, int? currentExpInvId = null, int? CustId = null)
        {
            if (string.IsNullOrWhiteSpace(ExpInvNo))
                return false;

            try
            {
                return await _unitOfWork.ExpInvs
                    .GetQueryable()
                    .AnyAsync(x => x.ExpInvNo == ExpInvNo
                                && x.Suffix == suffix
                                && x.CustId == CustId
                                && (currentExpInvId == null || x.ExpInvId != currentExpInvId));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in IsDuplicateExportNoAsync for ExpInv No: {ExpInvNo}");
                throw new InvalidOperationException("Failed to check duplicate Export Invoice.");
            }
        }

        public async Task<ExpInvVM> UpsertInvoiceAsync(ExpInvVM invoiceVM, int screenCode)
        {
            if (invoiceVM == null)
                throw new ArgumentNullException(nameof(invoiceVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ExpInv entity;

                if (invoiceVM.ExpInvId == 0)
                {
                    entity = _mapper.Map<ExpInv>(invoiceVM);

                    // 🔹 Get last number with locking from repository
                    //var NextNumber = await _unitOfWork.ExpInvs.GetLastExpInvNoAsync(entity.Suffix);
                    //entity.ExpInvNo = NextNumber;

                    //--------AutoDcrunning-------------------------------------------------
                    if (invoiceVM.IsManualInvNo ?? true)
                    {
                        var runningRow = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.InvoiceType == "EXPINV" &&
                                x.Suffix == entity.Suffix);

                        if (runningRow == null)
                        {
                            var dcrunn = new InvoiceAutoRunningNumber
                            {
                                LastNumber = Convert.ToInt64(entity.ExpInvNo),
                                InvoiceType = "EXPINV",
                                Suffix = entity.Suffix
                            };

                            await _unitOfWork.InvoiceAutoRunningNumbers.CreateAsync(dcrunn);
                        }
                        else
                        {
                            runningRow.LastNumber = Convert.ToInt64(entity.ExpInvNo);
                            await _unitOfWork.InvoiceAutoRunningNumbers.UpdateAsync(runningRow);
                        }

                        await _unitOfWork.SaveAsync();
                    }
                    else
                    {
                        entity.ExpInvNo = await _commonService.GenerateInvoiceAutoRunningNoAsync("EXPINV", entity.Suffix);
                    }
                    //-------------------------------------------------------------------------------------------------

                    bool check = await IsDuplicateExportNoAsync(entity.ExpInvNo, entity.Suffix, entity.ExpInvId, entity.CustId);
                    if (check)
                    {
                        throw new Exception("Duplicate Export Number found for this customer.");
                    }

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.Balance = invoiceVM.GrandTotal;
                    entity.ExpInvSubs = invoiceVM.ExpSubInvVMs.Select(s => _mapper.Map<ExpInvSub>(s)).ToList();

                    await _unitOfWork.ExpInvs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    int RefDcSubId = 0, RefPoSubId = 0;
                    foreach (var subVM in entity.ExpInvSubs)
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
                                       "", screenCode, subVM.ExpInvSubId, entity.ExpInvNo, entity.ExpInvDate, null, false);
                            }

                        }
                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }

                    changes.AppendLine("Invoice Created.");
                }
                else
                {
                    entity = await _unitOfWork.ExpInvs.GetQueryable()
                        .Include(q => q.ExpInvSubs)
                        .FirstOrDefaultAsync(q => q.ExpInvId == invoiceVM.ExpInvId)
                        ?? throw new InvalidOperationException("Export Invoice not found.");

                    var parentChanges = GetPropertyChanges(entity, invoiceVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(invoiceVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.Balance = invoiceVM.GrandTotal;
                    await HandleChildUpdatesAsync(entity, invoiceVM.ExpSubInvVMs, changes, screenCode);

                    changes.AppendLine("Quotation Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateExportInvoiceTallyStatusAsync(invoiceVM.ExpInvId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, invoiceVM.ExpInvId == 0 ? "Export Invoice Created" : "Export Invoice Updated");

                var savedEntity = await _unitOfWork.ExpInvs.GetQueryable()
                    .Include(q => q.ExpInvSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.Currency)
                    .Include(q => q.ExpInvSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.ExpInvId == entity.ExpInvId);

                return _mapper.Map<ExpInvVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Export Invoice: {invoiceVM.ExpInvNo}");
                throw new InvalidOperationException("Failed to save Export Invoice. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(ExpInv existingInv, List<ExpInvSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingInv.ExpInvSubs.Select(s => s.ExpInvSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.ExpInvSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingInv.ExpInvSubs.Where(s => !incomingSubIds.Contains(s.ExpInvSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - ExpInvSubId: {sub.ExpInvSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ExpInvSubs.DeleteAsync(sub.ExpInvSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefDcSubId > 0)
                {
                    await AdjustMfgDcBalanceAsync(sub.RefDcSubId, sub.Qty, 0, "Export Invoice Deletion");
                }
                else if (sub.RefPoSubId > 0)
                {
                    await AdjustMfgPoBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Export Invoice Deletion");

                    await DeleteStockIssueAndTrackAsync(sub.ExpInvSubId, sub.ItemId.Value, screenCode);
                }
                else
                    await UpdateCostCenterAsync(sub.CostId, false, changes);
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.ExpInvSubId == 0)
                {
                    var newSub = _mapper.Map<ExpInvSub>(subVM);
                    newSub.ExpInvId = existingInv.ExpInvId;
                    await _unitOfWork.ExpInvSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    if (subVM.RefDcSubId > 0)
                    {
                        await AdjustMfgDcBalanceAsync(subVM.RefDcSubId, 0, subVM.Qty ?? 0, "Export Invoice Creation");
                    }
                    else if (subVM.RefPoSubId > 0)
                    {
                        await AdjustMfgPoBalanceAsync(subVM.RefPoSubId, 0, subVM.Qty ?? 0, "Export Invoice Creation");

                        if (subVM.RcSubIds != "" && subVM.RcSubIds != null)
                        {
                            await IssueStockBySeqLogicAsync(newSub, existingInv, screenCode);
                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId, existingInv.StoreIssId.Value, (subVM.Qty ?? 0), subVM.UnitPrice.Value,
                                      "", screenCode, newSub.ExpInvSubId, existingInv.ExpInvNo, existingInv.ExpInvDate, null, false);
                        }

                    }

                    else
                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                }
                else
                {
                    var existingSub = existingInv.ExpInvSubs.FirstOrDefault(s => s.ExpInvSubId == subVM.ExpInvSubId);
                    if (existingSub != null)
                    {
                        // Rollback old CostCenter if needed
                        if ((existingSub.CostId != subVM.CostId) && (existingSub.RefDcSubId == null || existingSub.RefDcSubId == 0))
                            await UpdateCostCenterAsync(existingSub.CostId, false, changes);


                        if (subVM.RefDcSubId > 0)
                        {
                            await AdjustMfgDcBalanceAsync(subVM.RefDcSubId, existingSub.Qty, subVM.Qty ?? 0, "Export Invoice Update");
                        }
                        else if (subVM.RefPoSubId > 0)
                        {
                            await AdjustMfgPoBalanceAsync(subVM.RefPoSubId, existingSub.Qty, subVM.Qty ?? 0, "Export Invoice Update");

                            if (subVM.RcSubIds != "" && subVM.RcSubIds != null)
                            {
                                existingSub.Qty = subVM.Qty ?? existingSub.Qty;
                                await IssueStockBySeqLogicAsync(existingSub, existingInv, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId, existingInv.StoreIssId.Value, (subVM.Qty ?? 0), subVM.UnitPrice.Value,
                                      "", screenCode, subVM.ExpInvSubId, existingInv.ExpInvNo, existingInv.ExpInvDate, null, false);
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

        public async Task UpdateExportInvoiceTallyStatusAsync(int ExpInvId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.ExpInvSubs
                    .GetQueryable()
                    .Where(x => x.ExpInvId == ExpInvId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var invoice = await _unitOfWork.ExpInvs.GetAsync(ExpInvId);
                if (invoice == null)
                    return;

                invoice.InvTally = (totalBalQty == 0);

                await _unitOfWork.ExpInvs.UpdateAsync(invoice);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateInvoiceTallyStatusAsync] Error updating InvId {ExpInvId}");
                throw new InvalidOperationException("Failed to update Export Invoice Tally status. Please contact support.");
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

        //------Short Close----------
        public async Task<ExpInvVM> UpdateExportInvShortCloseAsync(ExpInvVM expInvVM)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ExpInv entity;

                entity = await _unitOfWork.ExpInvs
                             .GetQueryable()
                             .Include(e => e.ExpInvSubs)
                             .FirstOrDefaultAsync(e => e.ExpInvId == expInvVM.ExpInvId)
                             ?? throw new InvalidOperationException("Manufacturing Invoice not found.");

                _mapper.Map(expInvVM, entity);

                var parentChanges = GetPropertyChanges(entity, expInvVM);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                _mapper.Map(expInvVM, entity);
                entity.ModifiedBy = currentUser;
                entity.ModifiedDate = now;

                await _unitOfWork.ExpInvs.UpdateAsync(entity);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
                await LogChangesAsync(changes, expInvVM.ExpInvId == 0 ? "Short Close Export Invoice Created" : "ReOpen the Export Invoice");

                // Return updated entity
                var savedEntity = await _unitOfWork.ExpInvs
                    .GetQueryable()
                    .Include(e => e.ExpInvSubs).ThenInclude(s => s.Item)
                    .Include(e => e.ExpInvSubs).ThenInclude(s => s.CostCenter)
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.ExpInvId == entity.ExpInvId);

                return _mapper.Map<ExpInvVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpdateExportInvShortCloseAsync for PoId: {expInvVM.ExpInvId}");
                throw;
            }

        }

        public async Task<List<ExpInvVM>> GetPackingInvoiceDetailsAsync(int ExpInvId)
        {
            try
            {
                var subs = await _unitOfWork.ExpInvs
                    .GetQueryable()
                    .Where(s => s.ExpInvId == ExpInvId)
                    .ToListAsync();

                return _mapper.Map<List<ExpInvVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetPackingInvoiceDetailsAsync items for ExpInvId: {ExpInvId}");
                throw new InvalidOperationException("Failed to retrieve GetPackingInvoiceDetailsAsync Export Invoice . Please try again.");
            }
        }

        public async Task<ExpInvVM> UpdatePackingInvoiceDetailsAsync(ExpInvVM expinv)
        {
            try
            {
                var entity = await _unitOfWork.ExpInvs
                    .FirstOrDefaultAsync(x => x.ExpInvId == expinv.ExpInvId);

                if (entity == null)
                    throw new Exception("Packing Invoice not found");

                // Update fields
                entity.CountryOfOrigin = expinv.CountryOfOrigin;
                entity.CountryOfDestination = expinv.CountryOfDestination;
                entity.PreCarriage = expinv.PreCarriage;
                entity.PlaceOfReceipt = expinv.PlaceOfReceipt;
                entity.FlightOrVesselNo = expinv.FlightOrVesselNo;
                entity.PortOfLoading = expinv.PortOfLoading;
                entity.PortOfDischarge = expinv.PortOfDischarge;
                entity.FinalDestination = expinv.FinalDestination;
                entity.NetWeight = expinv.NetWeight;
                entity.GrossWeight = expinv.GrossWeight;
                entity.PackageDetails = expinv.PackageDetails;
                entity.DocketNumber = expinv.DocketNumber;
                entity.TransporterName = expinv.TransporterName;
                entity.TransporterAddress = expinv.TransporterAddress;
                entity.CompanyType = expinv.CompanyType;
                entity.Incoterm = expinv.Incoterm;

                await _unitOfWork.SaveAsync();

                // Optional: return updated VM
                return expinv;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "UpdatePackingInvoiceDetailsAsync failed");
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
                                     where sub.ExpInvSubId == RefSubInvId && inv.Export==true
                                     select inv.CrId).AnyAsync();

                return isExist;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrInvoiceCreditNoteAsync for RefSubInvId: {RefSubInvId}");
                throw;
            }
        }

        public async Task<List<ExportInvoiceStatusVM>> GetExportInvoiceStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<ExportInvoiceStatusVM>("Sp_GetExportInvoiceStatusList", status);
                return result.ToList();

            }
            catch (Exception ex)
            {

                throw;
            }
        }




        public static class ExpInvFilterBuilder
        {
            public static IQueryable<ExpInv> ApplyFilter(
                IQueryable<ExpInv> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "ExpInvNo":
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
                                (string.IsNullOrEmpty(part1) || x.ExpInvNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }
                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(value.ToString()));
                    case "ItemCode":
                        return query.Where(x => x.ExpInvSubs.Any(s => s.Item.ItemCode.Contains(value.ToString())));
                    case "ItemName":
                        return query.Where(x => x.ExpInvSubs.Any(s => s.Item.ItemName.Contains(value.ToString())));
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
            private static IQueryable<ExpInv> ApplyStatusFilter(IQueryable<ExpInv> query, string status)
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
                                !x.InvShortClose),

                        "Cancelled" =>
                            query.Where(x => x.IsCancel),

                        "Short-Closed" =>
                            query.Where(x => x.InvShortClose),

                        _ => query
                    };
                }
                catch
                {
                    return query;
                }
            }

        }
    }
}
