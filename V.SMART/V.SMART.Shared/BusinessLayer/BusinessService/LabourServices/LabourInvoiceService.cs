using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using FastReport;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.InvoiceAutoRunning;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourInvoiceVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.ReportViewModel.LabourStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.LabourServices
{
    public class LabourInvoiceService : ILabourInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;


        public LabourInvoiceService(
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


        //-----------------------------------------------------------------
        public async Task<string> GenerateQrBase64(string signedQrText)
          => await _commonService.GenerateQrBase64(signedQrText);

        public async Task<bool> CheckPrefixValid(string ScreenName)
           => await _commonService.CheckPrefixValid(ScreenName);

        public async Task<bool> CheckEwayRequired(string ScreenName)
            => await _commonService.CheckEwayRequired(ScreenName);
        //----------------------------------------------------------------------




        // 🔹 Labour Invoice operations

        public async Task<bool> GetPriceLockStatusAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(x => x.Display == "Rate Control" && x.ScreenName == "Labour Invoice")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching price lock status for Labour Invoice");
                return false;
            }
        }
        public async Task<int> GetPendingLabourDcOutCountAsync(int custId)
        {
            return await _unitOfWork.LabourDcOutgoings
                .GetQueryable()
                .Where(e => e.CustId == custId && e.DcTally == false && !e.NonReturnDc && !e.ReceivedReturnRM && !e.DcCancel && !e.ShortClose)
                .CountAsync();
        }

        public async Task<LabInvVM?> GetLabourInvByLabInvIdAsync(int? LabInvId)
        {
            try
            {
                //var vm = await _unitOfWork.LabInvs
                //         .GetQueryable()
                //         .AsNoTracking()
                //         .Where(q => q.LabInvId == LabInvId)
                //         .ProjectTo<LabInvVM>(_mapper.ConfigurationProvider)
                //         .FirstOrDefaultAsync();

                //return vm;
                var entity = await _unitOfWork.LabInvs.GetQueryable()
                    .Include(q => q.LabInvSubs)
                    .Include(q => q.LabInvSubs).ThenInclude(s => s.LabourDcOutgoingSub).ThenInclude(h => h.LabourDcOutgoing)
                    .Include(p => p.LabInvSubs).ThenInclude(s => s.LabourDcOutgoingSub).ThenInclude(ps => ps.MfgPoSub).ThenInclude(m=>m.MfgPo)
                    .Include(q => q.LabInvSubs).ThenInclude(s => s.Item)
                    .Include(q => q.LabInvSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                    .Include(q => q.Currency)
                    .Include(j => j.LabInvSubs).ThenInclude(ps => ps.CreditNoteSubs).ThenInclude(p => p.CreditNote)
                    .FirstOrDefaultAsync(q => q.LabInvId == LabInvId);

                return _mapper.Map<LabInvVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetLabourInvByLabInvIdAsync({LabInvId})");
                return null;
            }
        }

        public async Task<IEnumerable<LabInvVM>> GetAllLabInvoiceAsync()
        {
            try
            {
                var entities = await _unitOfWork.LabInvs.GetAllWithIncludeAsync(q => true,
                    q => q.Customer, q => q.LabInvSubs);
                return _mapper.Map<IEnumerable<LabInvVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllLaborOnvoicesync");
                return Enumerable.Empty<LabInvVM>();
            }
        }

        public async Task<LabInv?> GetLastinvoiceNoAsync(int custId)
        {
            try
            {
                return await _unitOfWork.LabInvs.GetLatestAsync(
                    q => q.CustId == custId,
                    q => q.LabInvId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastinvoiceNoAsync for CustId: {custId}");
                return null;
            }
        }
        public async Task<LabInvVM> UpsertLabourInvoiceAsync(LabInvVM labourInvVM)
        {
            if (labourInvVM == null)
                throw new ArgumentNullException(nameof(labourInvVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                LabInv entity;

                if (labourInvVM.LabInvId == 0)
                {
                    entity = _mapper.Map<LabInv>(labourInvVM);


                    // 🔹 Get last number with locking from repository
                    //var NextNumber = await _unitOfWork.LabInvs.GetLastLabInvoiceNoAsync(entity.Suffix);
                    //entity.LabInvNo = NextNumber;

                    //--------AutoDcrunning-------------------------------------------------
                    if (labourInvVM.IsManualInvNo ?? true)
                    {
                        var runningRow = await _unitOfWork.InvoiceAutoRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.InvoiceType == "LABINV" &&
                                x.Suffix == entity.Suffix);

                        if (runningRow == null)
                        {
                            var dcrunn = new InvoiceAutoRunningNumber
                            {
                                LastNumber = Convert.ToInt64(entity.LabInvNo),
                                InvoiceType = "LABINV",
                                Suffix = entity.Suffix
                            };

                            await _unitOfWork.InvoiceAutoRunningNumbers.CreateAsync(dcrunn);
                        }
                        else
                        {
                            runningRow.LastNumber = Convert.ToInt64(entity.LabInvNo);
                            await _unitOfWork.InvoiceAutoRunningNumbers.UpdateAsync(runningRow);
                        }

                        await _unitOfWork.SaveAsync();
                    }
                    else
                    {
                        entity.LabInvNo = await _commonService.GenerateInvoiceAutoRunningNoAsync("LABINV", entity.Suffix);
                    }

                    //-------------------------------------------------------------------------------------------------

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.Balance = labourInvVM.GrandTotal;
                    entity.LabInvSubs = labourInvVM.LabourInvoiceSubVM.Select(s => _mapper.Map<LabInvSub>(s)).ToList();


                    await _unitOfWork.LabInvs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in labourInvVM.LabourInvoiceSubVM)
                    {
                        if (subVM.RefDcSubId > 0)
                            await AdjustLabourDcOutBalanceAsync(subVM.RefDcSubId, 0, subVM.Qty ?? 0, "Labour Invoice Creation");
                        else
                            await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }

                    changes.AppendLine("Labour Invoice Created.");
                }
                else
                {
                    entity = await _unitOfWork.LabInvs.GetQueryable()
                        .Include(q => q.LabInvSubs)
                        .FirstOrDefaultAsync(q => q.LabInvId == labourInvVM.LabInvId)
                        ?? throw new InvalidOperationException("Labour Invoice not found.");

                    var parentChanges = GetPropertyChanges(entity, labourInvVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(labourInvVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.Balance = labourInvVM.GrandTotal;
                    await HandleChildUpdatesAsync(entity, labourInvVM.LabourInvoiceSubVM, changes);

                    changes.AppendLine("Labour Invoice  Updated.");
                }

                var tdsInfo = await _unitOfWork.Customers.GetQueryable()
                  .Where(v => v.CustId == entity.CustId).Select(v => new { v.TdsDeduct, }).FirstOrDefaultAsync();

                if (tdsInfo.TdsDeduct)
                {
                    await AutoDeductTDSForCustomerAsync(entity);
                }
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await LogChangesAsync(changes, labourInvVM.LabInvId == 0 ? "Labour Invoice  Created" : "Labour Invoice  Updated");

                var savedEntity = await _unitOfWork.LabInvs.GetQueryable()
                    .Include(q => q.LabInvSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.Currency)
                    .Include(q => q.LabInvSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.LabInvId == entity.LabInvId);

                return _mapper.Map<LabInvVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Labour Invoice: {labourInvVM.LabInvNo}");
                return null;
            }
        }

        private async Task HandleChildUpdatesAsync(LabInv existingLabInv, List<LabInvSubVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingLabInv.LabInvSubs.Select(s => s.LabInvSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.LabInvSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingLabInv.LabInvSubs.Where(s => !incomingSubIds.Contains(s.LabInvSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - LabinvSubId: {sub.LabInvSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.LabInvSubs.DeleteAsync(sub.LabInvSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefDcSubId > 0)
                    await AdjustLabourDcOutBalanceAsync(sub.RefDcSubId, sub.Qty, 0, "Quote Deletion");
                else
                    await UpdateCostCenterAsync(sub.CostId, false, changes);
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.LabInvSubId == 0)
                {
                    var newSub = _mapper.Map<LabInvSub>(subVM);
                    newSub.LabInvId = existingLabInv.LabInvId;
                    await _unitOfWork.LabInvSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    if (subVM.RefDcSubId > 0)
                        await AdjustLabourDcOutBalanceAsync(subVM.RefDcSubId, 0, subVM.Qty ?? 0, "Labour Invoice Creation");
                    else
                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                }
                else
                {
                    var existingSub = existingLabInv.LabInvSubs.FirstOrDefault(s => s.LabInvSubId == subVM.LabInvSubId);
                    if (existingSub != null)
                    {
                        // Rollback old CostCenter if needed
                        if ((existingSub.CostId != subVM.CostId) && (existingSub.RefDcSubId == null || existingSub.RefDcSubId == 0))
                            await UpdateCostCenterAsync(existingSub.CostId, false, changes);

                        // Adjust Enquiry balance if RefEnqSubId > 0
                        if (subVM.RefDcSubId > 0)
                            await AdjustLabourDcOutBalanceAsync(subVM.RefDcSubId, existingSub.Qty, subVM.Qty ?? 0, "Quote Update");

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
                changes.AppendLine($"CostCenter Updated - ProjectNo {cost.ProjectNo}: AssToQuote set to '{assign}'");
            }
        }

        private async Task AdjustLabourDcOutBalanceAsync(int? RefDcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!RefDcSubId.HasValue || RefDcSubId == 0) return;

                var enqSub = await _unitOfWork.LabourDcOutgoingSubs.GetAsync(RefDcSubId.Value);
                if (enqSub == null) return;

                if (oldQty > 0)
                    enqSub.BalQty += oldQty;

                if (newQty > enqSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Labour DcOutgoing BalQty.");

                if (newQty > 0)
                    enqSub.BalQty -= newQty;

                await _unitOfWork.LabourDcOutgoingSubs.UpdateAsync(enqSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Where(e => e.DcId == enqSub.DcId)
                    .SumAsync(e => e.BalQty);

                var labourDc = await _unitOfWork.LabourDcOutgoings.GetAsync(enqSub.DcId);
                if (labourDc != null)
                {
                    labourDc.DcTally = (totalBalQty == 0);
                    await _unitOfWork.LabourDcOutgoings.UpdateAsync(labourDc);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustLabourDcOutBalanceAsync] Validation failed in {context}");

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustQuoteBalance] Unexpected error in {context}");

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
        public async Task<bool> HasAnyItemOrLabourInvoiceCancelAsync(int labinvid)
        {
            try
            {
                var isGRNCancelled = await _unitOfWork.LabInvs
                    .AnyAsync(q => q.LabInvId == labinvid && q.InvCancel == true);

                var isItemCancelled = await _unitOfWork.LabInvSubs
                    .AnyAsync(i => i.LabInvId == labinvid && i.ItemCancel == true);

                return isGRNCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrLabourInvoiceCancelAsync for InvId: {labinvid}");
                return false;
            }
        }

        public async Task DeleteAndResequenceAsync(LabInvSubVM subitem, LabInvVM LabInv)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.LabInvSubId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.LabInvSubs.GetAsync(subitem.LabInvSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    // Restore balance qty
                    if (entity.RefDcSubId > 0)
                    {
                        await AdjustLabourDcOutBalanceAsync(subitem.RefDcSubId, entity.Qty, 0, "Labour Invoice Deletion");
                    }
                    else
                    {
                        await UpdateCostCenterAsync(subitem.CostId, false, changes);
                    }

                    // Delete from DB
                    await _unitOfWork.LabInvSubs.DeleteAsync(entity.RefDcSubId ?? 0);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "labour Invoice",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"labour Invoice No: {LabInv?.LabInvNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    LabInv.LabourInvoiceSubVM.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.LabInvSubs
                    .GetQueryable()
                    .Where(x => x.LabInvId == LabInv.LabInvId)
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
        public async Task<bool> DeleteLabourInvoiceByLabInvIdAsync(int invid)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var quotation = await _unitOfWork.LabInvs
                    .GetQueryable()
                    .Include(e => e.LabInvSubs)
                    .FirstOrDefaultAsync(e => e.LabInvId == invid);

                if (quotation == null)
                {
                    // Quotation not found
                    return false;
                }

                var changes = new StringBuilder();

                foreach (var sub in quotation.LabInvSubs)
                {
                    if (sub.RefDcSubId > 0)
                    {
                        await AdjustLabourDcOutBalanceAsync(sub.RefDcSubId ?? 0, sub.Qty, 0, "Labour Invoice Deletion");
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
                                x.InvoiceType == "LABINV" &&
                                x.Suffix == quotation.Suffix);
                if (runningRow != null)
                {
                    long oldInvNo = 0;
                    long.TryParse(quotation.LabInvNo.ToString(), out oldInvNo);
                    if (runningRow.LastNumber == oldInvNo)
                    {
                        runningRow.LastNumber = (oldInvNo - 1);
                        await _unitOfWork.InvoiceAutoRunningNumbers.UpdateAsync(runningRow);
                    }
                }

                //----------------------------------------------------------------------------


                // Delete the quotation
                var deleted = await _unitOfWork.LabInvs.DeleteAsync(invid);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Labour Invoice List",
                    action: $"Labour Invoice: {quotation.LabInvNo}",
                    additionalInfo: $"LabInv Id: {quotation.LabInvId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Labour Invoice: {invid}");
                throw;
            }
        }
        public async Task<List<LabInvSubVM>> GetLabourInvSubByInvIdAsync(int InvId)
        {
            try
            {
                var subs = await _unitOfWork.LabInvSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.LabInvId == InvId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<LabInvSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching LabourInvoiceSub items for InvId: {InvId}");
                return null;
            }
        }
        public async Task<List<LabInvSub>> GetLabInvSubDetailsByInvIdAsync(int InvId)
        {
            try
            {
                var subs = await _unitOfWork.LabInvSubs
                                 .GetQueryable()
                                 .Where(s => s.LabInvId == InvId)
                                 .OrderBy(s => s.SlNo)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetLabInvSubDetailsByInvIdAsync items for InvId: {InvId}");
                throw new InvalidOperationException("Failed to retrieve Labour Invoice sub-items. Please try again.");
            }
        }
        public async Task<List<Dictionary<string, object>>> GetDcDetailsByCustId(int custId)
        {
            try
            {
                var result = await (from e in _unitOfWork.LabourDcOutgoings.GetQueryable()
                                    join es in _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                                        on e.DcId equals es.DcId
                                    where e.CustId == custId && !e.DcTally && !e.DcCancel && es.BalQty > 0 && es.TransType == "Out" && !e.ReceivedReturnRM && es.DcRowRem.ToUpper() != "NV" && !e.NonReturnDc 
                                    && !e.ShortClose
                                    select new
                                    {
                                        es.DcSubId,
                                        e.DcNo,
                                        e.Suffix,
                                        e.DcDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.MeasureUnit,
                                        es.Item.SACCode,
                                        es.Qty,
                                        es.BalQty,
                                        es.UnitPrice,
                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo,
                                        es.MfgPoSub.MfgPo.PONo,
                                        es.MfgPoSub.MfgPo.PODate,
                                        es.MfgPoSub.LineNo
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefDcSubId"] = r.DcSubId,
                    ["DcNo"] = $"{r.DcNo}{r.Suffix}",
                    ["DcDate"] = r.DcDate,

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,

                    ["SACCode"] = r.SACCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["Rate"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["RefPoNo"] = r.PONo,
                    ["RefPoDate"] = r.PODate,
                    ["LineNo"] = r.LineNo
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching enquiry details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve enquiry details. Please try again.");
            }
        }
        public async Task<decimal> GetBulkLastUnitPricesItemIdAsync(int itemId, int custId, int refdcsubid)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                decimal rate = 0;
                rate = await (from qs in _unitOfWork.MfgPoSubs.GetQueryable()
                              join q in _unitOfWork.MfgPos.GetQueryable() on qs.PoId equals q.PoId
                              join ds in _unitOfWork.LabourDcOutgoingSubs.GetQueryable() on qs.PoSubId equals ds.RefPoSubId
                              where qs.ItemId == itemId && q.CustId == custId
                              orderby q.PoId descending
                              select qs.UnitPrice)
                              .FirstOrDefaultAsync();

                if (rate == 0)
                {
                    rate = await (from qs in _unitOfWork.LabInvSubs.GetQueryable()
                                  join q in _unitOfWork.LabInvs.GetQueryable() on qs.LabInvId equals q.LabInvId
                                  where qs.ItemId == itemId && q.CustId == custId
                                  orderby qs.LabInvSubId descending
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

                return rate;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for CustId: {custId}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
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
                                  join ds in _unitOfWork.LabourDcOutgoingSubs.GetQueryable() on qs.PoSubId equals ds.RefPoSubId
                                  where qs.ItemId == itemId && q.CustId == custId
                                  orderby q.PoId descending
                                  select qs.UnitPrice)
                                 .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.LabInvSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.LabInvSubId descending
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
                var prefix = await _unitOfWork.LabInvs.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.LabInvId).Where(x => !string.IsNullOrEmpty( x.Prefix))
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
                string nextNumber = await _commonService.GeneratePreviewInvoiceAutoRunningNoAsync("LABINV", suffix);

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Labour Invoice number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate labour invoice number.");
            }
        }

        public async Task<decimal> GetLabourdcOutItemBalQtyFromEnqSubId(int DcSubId)
        {
            try
            {
                return await _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                    .Where(e => e.DcSubId == DcSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();


            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for labour Dc Outgoing: {DcSubId}");
                throw new InvalidOperationException("Failed to retrieve enquiry balance quantity.");
            }
        }
        public async Task<LabInvSubVM?> GetInvoiceSubItemDetailByinvoiceSubIdAsync(int InvSubId)
        {
            try
            {
                return await _unitOfWork.LabInvSubs
                    .GetQueryable()
                    .Where(q => q.LabInvSubId == InvSubId)
                    .Select(q => new LabInvSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching quote sub item detail for LabdcSubId: {InvSubId}");
                return null;
            }
        }


        public async Task<(List<LabInvVM> LabInv, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.LabInvs
                .GetQueryable()

                .Include(j => j.LabInvSubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.LabInvSubs)
                    .ThenInclude(s => s.LabourDcOutgoingSub)
                .Include(j => j.LabInvSubs)
                    .ThenInclude(s => s.CostCenter)
                .Include(j => j.Customer)
                .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = MaterialIssueFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.LabInvId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<LabInvVM>>(list);

            return (vmList, total);
        }

        public static class MaterialIssueFilterBuilder
        {
            public static IQueryable<LabInv> ApplyFilter(
                IQueryable<LabInv> query, string field, object value)
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
                                (string.IsNullOrEmpty(part1) || x.LabInvNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "DCNo":
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
                                x.LabInvSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.LabourDcOutgoingSub.LabourDcOutgoing.DcNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.LabourDcOutgoingSub.LabourDcOutgoing.Suffix.Contains(part2))
                                )
                            );
                        }


                    case "ItemCode":
                        return query.Where(x => x.LabInvSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.LabInvSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(value.ToString()));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.LabInvDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.LabInvDateNow <= toDate);
                        break;
                }

                return query;
            }

            private static IQueryable<LabInv> ApplyStatusFilter(
                IQueryable<LabInv> query, string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.LabInvTally && !x.ShortClose),

                        "Pending" =>
                            query.Where(x =>
                                !x.LabInvTally &&
                                !x.InvCancel &&
                                !x.ShortClose),

                        "Cancelled" =>
                            query.Where(x => x.InvCancel),

                        "Short-Closed" =>
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

        public async Task UpsertlabourInvoiceShortCloseAsync(LabInvVM LabInvVMs)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existinginv = await _unitOfWork.LabInvs.GetAsync(LabInvVMs.LabInvId);
                if (existinginv == null)
                    throw new InvalidOperationException("labour Invoice Enquiry not found.");

                existinginv.ShortClose = LabInvVMs.ShortClose;

                await _unitOfWork.LabInvs.UpdateAsync(existinginv);
                await _unitOfWork.SaveAsync();

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

        public async Task UpdatedCancelStatusAndAddOrRevertQty(LabInvVM LabInvVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingSCN = await _unitOfWork.LabInvs.GetAsync(LabInvVM.LabInvId);
                if (existingSCN == null)
                    throw new InvalidOperationException("Labour SCN not found.");

                var subs = await _unitOfWork.LabInvSubs
                    .GetQueryable()
                    .Where(s => s.LabInvId == LabInvVM.LabInvId)
                    .ToListAsync();

                if (!LabInvVM.InvCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateInvoiceBalanceBeforeRevertAsync(sub);
                    }
                }

                existingSCN.InvCancel = LabInvVM.InvCancel;
                existingSCN.CancelReason = LabInvVM.CancelReason;

                await _unitOfWork.LabInvs.UpdateAsync(existingSCN);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingSCN.InvCancel)
                    {
                        if (sub.RefDcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustLabourDcOutBalanceAsync(sub.RefDcSubId, sub.Qty, 0, "Labour Invoice Creation");
                        }
                    }
                    else
                    {
                        if (sub.RefDcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustLabourDcOutBalanceAsync(sub.RefDcSubId, 0, sub.Qty, "Labour Invoice Creation");
                        }

                    }
                }
                await transaction.CommitAsync();
                LabInvVM = await GetLabourInvByLabInvIdAsync(LabInvVM.LabInvId);
            }
            catch (InvalidOperationException ex)
            {
                LabInvVM = await GetLabourInvByLabInvIdAsync(LabInvVM.LabInvId);
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

        public async Task ValidateInvoiceBalanceBeforeRevertAsync(LabInvSub sub)
        {
            if (sub.RefDcSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.LabourDcOutgoingSubs.GetAsync(sub.RefDcSubId ?? 0);
            if (entity == null)
                throw new InvalidOperationException($"Labour dcOutgoing not found for RefDcSubId: {sub.RefDcSubId}");

            if (entity.BalQty < sub.Qty && entity.TransType == "Out")
            {
                throw new InvalidOperationException(
                    $"Cannot revert because DcOutgoing balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                );
            }
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(LabInvSubVM subItem)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();

                var subEntity = await _unitOfWork.LabInvSubs.GetQueryable().Where
                                (x => x.LabInvSubId == subItem.LabInvSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with DcSubId {subItem.LabInvSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateInvoiceBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.CancelItemReason = subItem.CancelItemReason;
                await _unitOfWork.LabInvSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    //await AdjustSCNSubBalanceAsync(subEntity.RefSCNSubId, subEntity.Qty, 0, $"Labour SCN Item Cancel - {subItem.ItemCode}");
                    if (subItem.RefDcSubId > 0)
                    {
                        await AdjustLabourDcOutBalanceAsync(subItem.RefDcSubId, subItem.Qty ?? 0, 0, "Labour Invoice Creation");
                    }

                }
                else
                {
                    if (subItem.RefDcSubId > 0)
                    {
                        await AdjustLabourDcOutBalanceAsync(subItem.RefDcSubId, 0, subItem.Qty ?? 0, "Labour Invoice Creation");
                    }

                }

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

        public async Task<PoTaxDetailVM?> GetPoTaxDataByRefDcSubIdAsync(int refDcSubId)
        {
            try
            {

                var poTax = await (
                    from lb in _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                    join ms in _unitOfWork.MfgPoSubs.GetQueryable()
                        on lb.RefPoSubId equals ms.PoSubId
                    join m in _unitOfWork.MfgPos.GetQueryable()
                        on ms.PoId equals m.PoId
                    where lb.DcSubId == refDcSubId
                    select new PoTaxDetailVM
                    {

                        LineDiscountAmount = ms.LineDiscountAmount ?? 0,
                        LineDiscountPercent = ms.LineDiscountPercent ?? 0,
                        LineCGSTRate = ms.LineCGSTRate ?? 0,
                        LineIGSTRate = ms.LineIGSTRate ?? 0,
                        LineSGSTRate = ms.LineSGSTRate ?? 0,
                        // HEADER
                        DiscAmtOrPer = m.DiscAmtOrPer,
                        DiscountAmount = m.DiscountAmount,
                        PackingAmtOrPer = m.PackingAmtOrPer,
                        InsuranceAmtOrPer = m.InsuranceAmtOrPer,
                        InsuranceCharges = m.InsuranceCharges,
                        FreightCharges = m.FreightCharges,
                        OtherCharges = m.OtherCharges,
                        TCSAmtOrPer = m.TCSAmtOrPer,
                        TCSAmount = m.TCSAmount,
                        CGstRate = m.CGstRate,
                        IGstRate = m.IGstRate,
                        SGstRate = m.SGstRate

                    }
                ).FirstOrDefaultAsync();

                if (poTax != null)
                    return poTax;

                var fallbackTax = await (
                    from lb in _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                    join dc in _unitOfWork.LabourDcOutgoings.GetQueryable()
                        on lb.DcId equals dc.DcId
                    join m in _unitOfWork.MfgPos.GetQueryable()
                        on dc.CustId equals m.CustId
                    join ms in _unitOfWork.MfgPoSubs.GetQueryable()
                        on m.PoId equals ms.PoId
                    where lb.DcSubId == refDcSubId
                    orderby m.PODate descending, m.PoId descending
                    select new PoTaxDetailVM
                    {

                        LineDiscountAmount = ms.LineDiscountAmount ?? 0,
                        LineDiscountPercent = ms.LineDiscountPercent ?? 0,
                        LineCGSTRate = ms.LineCGSTRate ?? 0,
                        LineIGSTRate = ms.LineIGSTRate ?? 0,
                        LineSGSTRate = ms.LineSGSTRate ?? 0,

                        DiscAmtOrPer = m.DiscAmtOrPer,
                        DiscountAmount = m.DiscountAmount,
                        PackingAmtOrPer = m.PackingAmtOrPer,
                        InsuranceAmtOrPer = m.InsuranceAmtOrPer,
                        InsuranceCharges = m.InsuranceCharges,
                        FreightCharges = m.FreightCharges,
                        OtherCharges = m.OtherCharges,
                        TCSAmtOrPer = m.TCSAmtOrPer,
                        TCSAmount = m.TCSAmount,
                        CGstRate = m.CGstRate,
                        IGstRate = m.IGstRate,
                        SGstRate = m.SGstRate
                    }
                ).FirstOrDefaultAsync();

                return fallbackTax;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"GetPoTaxDataByRefDcSubIdAsync failed. DcSubId={refDcSubId}"
                );
                return null;
            }
        }

        #region Auto TDS Deducts

        //-------TDS----------------
        public async Task<bool> UpdateTDSAmountAsync(LabInvVM LabInvVM)
        {
            try
            {
                var changes = new StringBuilder();

                LabInv entity;

                entity = await _unitOfWork.LabInvs
                                    .GetQueryable()
                                    .FirstOrDefaultAsync(x => x.LabInvId == LabInvVM.LabInvId && x.LabInvNo == LabInvVM.LabInvNo && x.Suffix == LabInvVM.Suffix);

                if (entity == null)
                    return false;

                var parentChanges = GetPropertyChanges(entity, LabInvVM);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                entity.TDSAmount = LabInvVM.TDSAmount;
                entity.Balance = (entity.GrandTotal) - LabInvVM.TDSAmount;

                if (entity.Balance < 0)
                    entity.Balance = 0;

                await _unitOfWork.LabInvs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();

                await LogChangesAsync(changes, LabInvVM.LabInvId == 0 ? "Labour Invoice Created" : "Labour Invoice TDS  Updated");

                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error Updating TDS Amount");
                return false;
            }
        }

        public async Task<bool> AutoDeductTDSForCustomerAsync(LabInv LabInv)
        {
            try
            {
                var customer = await _unitOfWork.Customers.GetQueryable()
                    .FirstOrDefaultAsync(v => v.CustId == LabInv.CustId);

                if (customer == null || !customer.TdsDeduct)
                    return false;

                decimal exemptAmt = customer.TDSExmptdAmt;
                decimal tdsRate = customer.TDSDeductper;
                decimal maxBill = customer.TDSBillval;

                if (tdsRate <= 0)
                    return false;

                // 🔹 Check if any previous invoice already deducted TDS
                bool hasPreviousTDS = await _unitOfWork.LabInvs.GetQueryable()
                    .AnyAsync(x => x.CustId == LabInv.CustId && x.TDSAmount > 0);

                if (hasPreviousTDS)
                {
                    ApplyTDS(LabInv, tdsRate);
                    await _unitOfWork.LabInvs.UpdateAsync(LabInv);
                    await _unitOfWork.SaveAsync();
                    return true;
                }

                // 🔹 Calculate total purchase INCLUDING current invoice
                decimal totalPurchase = await _unitOfWork.LabInvs.GetQueryable()
                    .Where(x => x.CustId == LabInv.CustId)
                    .SumAsync(x => x.TotalTaxable);

                bool thresholdCrossed = totalPurchase >= exemptAmt || (maxBill > 0 && LabInv.TotalTaxable >= maxBill);

                if (!thresholdCrossed)
                    return true;

                // 🔹 Apply TDS to ALL vendor invoices
                var invoices = await _unitOfWork.LabInvs.GetQueryable()
                    .Where(x => x.CustId == LabInv.CustId)
                    .ToListAsync();

                foreach (var inv in invoices)
                {
                    ApplyTDS(inv, tdsRate);
                    await _unitOfWork.LabInvs.UpdateAsync(inv);
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

        private void ApplyTDS(LabInv invoice, decimal tdsRate)
        {
            if (invoice == null || tdsRate <= 0)
                return;

            invoice.TDSAmount = Math.Round(invoice.TotalTaxable * tdsRate / 100, 2);
            invoice.Balance = invoice.GrandTotal - invoice.TDSAmount;

            if (invoice.Balance < 0)
                invoice.Balance = 0;
        }
        #endregion
        public async Task<bool> GetLabourInvoiceByIdIsCancelAsync(int InvId)
        {
            return await _unitOfWork.LabInvs
                .GetQueryable()
                .Where(e => e.LabInvId == InvId)
                .AnyAsync(e =>
                    e.InvCancel == true ||
                    e.LabInvSubs.Any(s => s.ItemCancel == true)
                );
        }


        public async Task<List<LabInvSubVM>> GetDistinctRefDcNosByInvIdIdAsync(int LabinvId)
        {
            return await _unitOfWork.LabInvSubs
                .GetQueryable()
                .Include(x => x.LabourDcOutgoingSub)
                .ThenInclude(x => x.LabourDcOutgoing)
                .Where(s => s.LabInvId == LabinvId)
                .GroupBy(s => new { s.LabourDcOutgoingSub.LabourDcOutgoing.DcNo, s.LabourDcOutgoingSub.LabourDcOutgoing.Suffix, s.LabourDcOutgoingSub.LabourDcOutgoing.DcDate })
                .Select(g => new LabInvSubVM
                {
                    RefDcNo = $"{g.Key.DcNo}{g.Key.Suffix}",
                    RefDcDate = g.Key.DcDate
                })
                .ToListAsync();
        }


        public async Task<bool> HasAnyItemOrInvoiceCreditNoteAsync(int RefSubInvId)
        {
            try
            {
                var isExist = await (from inv in _unitOfWork.CreditNotes.GetQueryable()
                                     join sub in _unitOfWork.CreditNoteSubs.GetQueryable()
                                     on inv.CrId equals sub.CrId
                                     where sub.LabInvSubId == RefSubInvId && inv.Labour == true
                                     select inv.CrId).AnyAsync();

                return isExist;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrInvoiceCreditNoteAsync for RefSubInvId: {RefSubInvId}");
                throw;
            }
        }


        public async Task<List<LabourInvoiceStatusVM>> GetLabourInvoiceStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<LabourInvoiceStatusVM>("Sp_GetLabourInvStatusList", status);
                return result.ToList();

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        public async Task<IEnumerable<Process>> SearchProcessAsync(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                return Enumerable.Empty<Process>();
            try
            {
                searchText = searchText.Trim().ToLower();
                var processes = await _unitOfWork.Processes.GetQueryable()
                    .Where(p => (EF.Functions.Like(p.ProcessName.ToLower(), $"%{searchText}%")))
                    .Select(p => new Process
                    {
                        ProcessId = p.ProcessId,
                        ProcessName = p.ProcessName
                    })
                    .ToListAsync();
                return processes;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error searching processes with text='{searchText}'.");
                return Enumerable.Empty<Process>();
            }
        }

        public async Task<bool> EinvoiceCancelOption()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(x => x.Display == "Invoice Cancel" && x.ScreenName == "Labour Invoice")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in EinvoiceCancelOption()");
                return false;
            }
        }

        public async Task<bool> ProcessSelectOption()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(x => x.Display == "Process Selection" && x.ScreenName == "Labour Invoice")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in ProcessSelectOption()");
                return false;
            }
        }

        public async Task<Process> GetProcessByIdAsync(int processId)
        {
            try
            {
                var process = await _unitOfWork.Processes.GetAsync(processId);
                if (process == null)
                    throw new KeyNotFoundException($"Process with ID {processId} not found.");
                return process;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching process with ID {processId}");
                return new Process();
            }
        }

        public async Task<bool> IsDocumentUploaded(int labourInvoiceId)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Labour-Invoice" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == labourInvoiceId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }

        public async Task<List<BillsVM>> GetBillsByCustomerAsync(int CustId, string billType)
        {
            try
            {
                IQueryable<BillsVM> query;

                switch (billType.ToUpper())
                {
                    case "SALES":
                        query = _unitOfWork.MfgInvs.GetQueryable()
                            .Include(e => e.Customer)
                            .Where(e => e.Balance > 0 && e.CustId == CustId)
                            .OrderByDescending(e => e.InvId)
                            .Select(e => new BillsVM
                            {
                                Id = e.InvId,
                                PartyCode = e.Customer.CustId,
                                BillNo = e.InvNo,
                                BillDate = e.InvDate,
                                Balance = e.Balance
                            });
                        break;

                    case "LABOUR WORK":
                        query = _unitOfWork.LabInvs.GetQueryable()
                            .Include(e => e.Customer)
                            .Where(e => e.Balance > 0 && e.CustId == CustId && e.TDSAmount <= 0)
                            .OrderByDescending(e => e.LabInvId)
                            .Select(e => new BillsVM
                            {
                                Id = e.LabInvId,
                                PartyCode = e.Customer.CustId,
                                BillNo = e.LabInvNo,
                                BillDate = e.LabInvDate,
                                Balance = e.Balance,
                                Suffix = e.Suffix
                            });
                        break;

                    case "SALES + LABOUR":

                        var salesQuery = _unitOfWork.MfgInvs.GetQueryable()
                            .Include(e => e.Customer)
                            .Where(e => e.Balance > 0 && e.CustId == CustId)
                            .Select(e => new BillsVM
                            {
                                Id = e.InvId,
                                PartyCode = e.Customer.CustId,
                                BillNo = e.InvNo,
                                BillDate = e.InvDate,
                                Balance = e.Balance,
                                BillType = "SALES"
                            });

                        var labourQuery = _unitOfWork.LabInvs.GetQueryable()
                            .Include(e => e.Customer)
                            .Where(e => e.Balance > 0 && e.CustId == CustId)
                            .Select(e => new BillsVM
                            {
                                Id = e.LabInvId,
                                PartyCode = e.Customer.CustId,
                                BillNo = e.LabInvNo,
                                BillDate = e.LabInvDate,
                                Balance = e.Balance,
                                BillType = "LABOUR"
                            });

                        query = salesQuery
                            .Concat(labourQuery)
                            .OrderByDescending(e => e.BillDate);

                        break;




                    case "PURCHASE ORDER ADVANCE":
                        query = _unitOfWork.PurchPos.GetQueryable()
                            .Include(e => e.Vendor)
                            .Where(e => e.AdvanceAmountBal > 0)
                            .OrderByDescending(e => e.PoId)
                            .Select(e => new BillsVM
                            {
                                Id = e.PoId,
                                PartyCode = e.Vendor.VendorCode,
                                BillNo = e.PONo,
                                BillDate = e.PODate,
                                Balance = e.AdvanceAmountBal
                            });
                        break;

                    default:
                        return new List<BillsVM>();
                }

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to load pending bills: {billType}");
                throw;
            }
        }

        public async Task<bool> UpdateMultipleTDSAmountAsync(List<BillsVM> labInvVMs)
        {
            try
            {
                if (labInvVMs == null || !labInvVMs.Any())
                    return false;

                var changes = new StringBuilder();

                foreach (var LabInvVM in labInvVMs)
                {
                    var entity = await _unitOfWork.LabInvs.GetQueryable()
                                .FirstOrDefaultAsync(x => x.LabInvId == LabInvVM.Id &&
                                x.Suffix == LabInvVM.Suffix && x.LabInvNo == LabInvVM.BillNo);

                    if (entity == null)
                        continue;

                    var parentChanges = GetPropertyChanges(entity, LabInvVM);

                    if (!string.IsNullOrWhiteSpace(parentChanges))
                    {
                        changes.AppendLine($"Invoice : {LabInvVM.BillNo}\n{parentChanges}");
                    }

                    // Update values
                    entity.TDSAmount = LabInvVM.TDSAmount;

                    entity.Balance = (entity.Balance) - (LabInvVM.TDSAmount);

                    if (entity.Balance < 0)
                        entity.Balance = 0;

                    await _unitOfWork.LabInvs.UpdateAsync(entity);
                }

                // Single save for performance
                await _unitOfWork.SaveAsync();

                await LogChangesAsync(changes, "Multiple Labour Invoice TDS Updated");

                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error Updating Multiple TDS Amount");
                return false;
            }
        }
    }

}