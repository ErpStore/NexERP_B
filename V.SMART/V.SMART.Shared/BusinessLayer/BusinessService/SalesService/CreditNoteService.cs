using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.BusinessLayer.BusinessService.InventoryService;
using V.SMART.Shared.Data.InvoiceAutoRunning;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.CreditNote_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.CreditNoteListViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class CreditNoteService: ICreditNoteService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

       

        public CreditNoteService(
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

        //GetScreenCode
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
                => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        //Currency
        public async Task<List<Currency>> GetCurrenciesAsync()
           => (await _commonService.GetCurrenciesAsync()).ToList();

        public async Task<Currency?> GetCurrencyByIdAsync(int currId)
         => (await _commonService.GetCurrencyByIdAsync(currId));

        // 🔹 Get latest currency rate (from CurrencyToday Service)
        public async Task<decimal?> GetLatestCurrencyValueAsync(int currId)
            => await _commonService.GetLatestCurrencyValueAsync(currId);

        //DecimalPlaces
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

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

        // 🔹 Contacts
        public async Task<List<ContactPerson>> GetContactPersonsAsync(int custId)
            => await _commonService.GetContactPersonsAsync(custId);


        // 🔹 Consignee addresses
        public async Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId)
            => await _commonService.GetConsigneeAddressesAsync(custId);

        public async Task<List<CustomerVM>> GetExportCustomerByIdAsync(int? custId = null)
        {
            if (custId.HasValue && custId.Value > 0)
            {
                var customer = await _commonService.GetExportCustomerByIdAsync(custId.Value);
                return customer != null ? new List<CustomerVM> { customer } : new List<CustomerVM>();
            }
            return await _commonService.GetExportAllActiveCustomersAsync();
        }

        public async Task<IEnumerable<CustomerVM>> SearchExportCustomersAsync(string searchText)
        {
            return await _commonService.SearchExportCustomersAsync(searchText);
        }

        public async Task<CustomerVM?> GetExportCustomerByIdAsync(int custId)
            => await _commonService.GetExportCustomerByIdAsync(custId);


        // CostCeneter
        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        //GenerateQrCode
        public async Task<string> GenerateQrBase64(string signedQrText)
          => await _commonService.GenerateQrBase64(signedQrText);

        public async Task<bool> CheckPrefixValid(string ScreenName)
           => await _commonService.CheckPrefixValid(ScreenName);

        // 🔹 Mfg Invoice operations

        public async Task<(List<CreditNoteVM> creditNoteVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.CreditNotes.GetQueryable().AsSplitQuery().AsNoTracking()
                .Include(j => j.Customer)
                .Include(j => j.CreditNoteSubs).ThenInclude(s => s.Item)
                .Include(q => q.Currency)
                .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                .Include(q => q.CreditNoteSubs).ThenInclude(s => s.CostCenter)
                .Include(j => j.CreditNoteSubs).ThenInclude(ps => ps.MfgInvSub).ThenInclude(p => p.MfgInv)
                .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = CreditNoteFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.CrId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<CreditNoteVM>>(list);

            return (vmList, total);
        }

        public static class CreditNoteFilterBuilder
        {
            public static IQueryable<CreditNote> ApplyFilter(
                IQueryable<CreditNote> query, string field, object value)
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
                                (string.IsNullOrEmpty(part1) || x.CreditNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }
                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(value.ToString()));
                    
                    case "ItemCode":
                        return query.Where(x => x.CreditNoteSubs.Any(s => s.Item.ItemCode.Contains(value.ToString())));
                    case "ItemName":
                        return query.Where(x => x.CreditNoteSubs.Any(s => s.Item.ItemName.Contains(value.ToString())));
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

        public async Task<string> GetPrefixFromDb()
        {
            try
            {
                var prefix = await _unitOfWork.CreditNotes.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.CrId)
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

        public async Task<string> GetCreditNoteNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.CreditNotes
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => Convert.ToInt32(q.CreditNo))
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastQuote != null)
                {
                    var parts = lastQuote.CreditNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating GetCreditNoteNumberAsync for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate GetCreditNoteNumberAsync.");
            }
        }

        public async Task<int> GetPendingInvCountAsync(int custId,string type)
        {
            try
            {
                int count = 0;

                if (type== "Sales")
                {
                    count = await _unitOfWork.MfgInvs.GetQueryable()
                            .Where(m => m.CustId == custId && m.Balance > 0
                            && _unitOfWork.MfgInvSubs.GetQueryable()
                            .Any(s => s.InvId == m.InvId && !m.IsCancel && s.CrBalQty > 0)).CountAsync();
                }
                else if(type == "Labour")
                {
                    count = await _unitOfWork.LabInvs.GetQueryable()
                            .Where(m => m.CustId == custId && m.Balance > 0
                            && _unitOfWork.LabInvSubs.GetQueryable()
                            .Any(s => s.LabInvId == m.LabInvId && !m.InvCancel && s.CrBalQty > 0)).CountAsync();

                }
                else if (type== "Export")
                {
                    count = await _unitOfWork.ExpInvs.GetQueryable()
                            .Where(m => m.CustId == custId && m.Balance > 0
                            && _unitOfWork.ExpInvSubs.GetQueryable()
                            .Any(s => s.ExpInvId == m.ExpInvId && !m.IsCancel && s.CrBalQty > 0)).CountAsync();
                }
                return count;
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }

        public async Task<CreditNote?> GetLastcreditNoteAsync(int custId)
        {
            try
            {
                return await _unitOfWork.CreditNotes.GetLatestAsync(
                    q => q.CustId == custId,
                    q => q.CrId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastcreditNoteAsync for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve last Creditnote. Please try again.");
            }
        }

        public async Task<bool> DeleteCreditNoteByCrIdAsync(int CrId, int screenCode,string type)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var credit = await _unitOfWork.CreditNotes
                    .GetQueryable()
                    .Include(e => e.CreditNoteSubs)
                    .FirstOrDefaultAsync(e => e.CrId == CrId);

                if (credit == null)
                {
                    return false;
                }

                int? RefInvSubId = 0;
                 
                var changes = new StringBuilder();
                bool hasReject = false;
                foreach (var sub in credit.CreditNoteSubs)
                {
                    RefInvSubId = sub.RefInvSubId ?? sub.LabInvSubId ?? sub.ExpInvSubId;
                    if (RefInvSubId > 0)
                    {
                        if (credit.Rejection.Value)
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, sub.RejQty, 0, "CreditNote Deletion", type);

                            if (sub.RefPoSubId>0)
                            {
                                hasReject = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId.Value);
                                if (hasReject)
                                {
                                    await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value,0,sub.RejQty, "CreditNote Reject/Rework");
                                }
                            }

                        }
                        else
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, sub.CrDrQty, 0, "CreditNote Deletion", type);
                        }

                    }
                   
                    else
                    {
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                    }
                }

                await UpdatePendingBalanceAsync(type, null, credit);

                //----------------------------------------------------------------------------
                // Delete the quotation
                var deleted = await _unitOfWork.CreditNotes.DeleteAsync(CrId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "MfgInvoice List",
                    action: $"Deleted CreditNote: {credit.CreditNo}",
                    additionalInfo: $"CreditNote Id: {credit.CrId}\n{changes}"
                );

                return true;
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPOBalance] Validation failed in CreditNote Deletion");
                throw; 
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete CreditNote CrId: {CrId}");
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

        private async Task AdjustInvoiceSubBalanceAsync(int? refinvSubId, decimal oldQty, decimal newQty, string context, string type)
        {
            try
            {
                if (!refinvSubId.HasValue || refinvSubId == 0)
                    return;

                // Strongly typed variable
                dynamic invSub = null;

                // Load the correct entity
                if (type == "Sales")
                {
                    invSub = await _unitOfWork.MfgInvSubs.GetAsync(refinvSubId.Value);
                }
                else if (type == "Labour")
                {
                    invSub = await _unitOfWork.LabInvSubs.GetAsync(refinvSubId.Value);
                }
                else if (type == "Export")
                {
                    invSub = await _unitOfWork.ExpInvSubs.GetAsync(refinvSubId.Value);
                }

                if (invSub == null)
                    return;

                
                if (oldQty > 0)
                    invSub.CrBalQty += oldQty;

                
                if (newQty > invSub.CrBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Invoice CrBalQty.");

                
                if (newQty > 0)
                    invSub.CrBalQty -= newQty;

                if (type == "Sales")
                    await _unitOfWork.MfgInvSubs.UpdateAsync(invSub);
                else if (type == "Labour")
                    await _unitOfWork.LabInvSubs.UpdateAsync(invSub);
                else if (type == "Export")
                    await _unitOfWork.ExpInvSubs.UpdateAsync(invSub);

                await _unitOfWork.SaveAsync();

               
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustInvoiceSubBalance] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustInvoiceSubBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust PO balance. Please contact support.");
            }
        }

        public async Task<CreditNoteVM?> GetCreditNoteBycrIdAsync(int CrId)
        {
            try
            {
                var entity = await _unitOfWork.CreditNotes.GetQueryable()
                    .Include(q => q.CreditNoteSubs)
                    .Include(q => q.CreditNoteSubs).ThenInclude(s => s.Item)
                    .Include(q => q.CreditNoteSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.CreditNoteSubs).ThenInclude(s => s.MfgInvSub).ThenInclude(m => m.MfgInv)
                    .Include(q => q.Customer).ThenInclude(c => c.CustomerIndirects)
                    .Include(q => q.Currency)
                    .FirstOrDefaultAsync(q => q.CrId == CrId);

                return _mapper.Map<CreditNoteVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetCreditNoteBycrIdAsync({CrId})");
                return null;
            }
        }

        public async Task<decimal> GetInvItemBalQtyFromDcSubId(int invSubId, string type)
        {
            try
            {
                switch (type)
                {
                    case "Sales":
                        return await _unitOfWork.MfgInvSubs.GetQueryable()
                            .Where(e => e.InvSubId == invSubId)
                            .Select(e => e.CrBalQty ?? 0)
                            .FirstOrDefaultAsync();

                    case "Labour":
                        return await _unitOfWork.LabInvSubs.GetQueryable()
                            .Where(e => e.LabInvSubId == invSubId)
                            .Select(e => e.BalQty)
                            .FirstOrDefaultAsync();

                    case "Export":
                        return await _unitOfWork.ExpInvSubs.GetQueryable()
                            .Where(e => e.ExpInvSubId == invSubId)
                            .Select(e => e.BalQty)
                            .FirstOrDefaultAsync();

                    default:
                        return 0;
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for DcSubId: {invSubId}");
                throw new InvalidOperationException("Failed to retrieve Invoice balance quantity in GetInvItemBalQtyFromDcSubId");
            }
        }

        public async Task<List<CreditNoteSubVM>> GetCreditNoteSubByCrIdAsync(int CrId)
        {
            try
            {
                var subs = await _unitOfWork.CreditNoteSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.CrId == CrId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<CreditNoteSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching MfgInvSub items for QuoteId: {CrId}");
                throw new InvalidOperationException("Failed to retrieve Invoice sub-items. Please try again.");
            }
        }

        public async Task<CreditNoteSubVM?> GetCreditNoteSubItemDetailByCrSubIdAsync(int CrSubId, string reason)
        {
            try
            {
                // Fetch sub-item by CrSubId
                var subItem = await _unitOfWork.CreditNoteSubs
                    .GetQueryable()
                    .Where(q => q.CrSubId == CrSubId)
                    .Select(q => new CreditNoteSubVM
                    {
                        Qty = q.Qty,
                        RejQty = q.RejQty,
                        CrDrQty= q.CrDrQty,
                    })
                    .FirstOrDefaultAsync();

                
                if (subItem != null && reason == "Rejection")
                {
                    
                    subItem.RejQty = subItem.RejQty;
                    subItem.CrDrQty = 0;
                }
                else
                {
                    subItem.CrDrQty = subItem.CrDrQty;
                    subItem.RejQty=0;
                }

                    return subItem;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Credit Note sub item detail for CrSubId: {CrSubId}");
                throw new InvalidOperationException("Failed to retrieve Credit Note sub-item details.");
            }
        }

        public async Task DeleteAndResequenceAsync(CreditNoteSubVM subitem, CreditNoteVM quote, int screenCode,string type)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                int? RefInvSubId = 0;
                if (subitem.CrSubId > 0) // persisted subitem
                {
                    bool hasReject = false;
                    var entity = await _unitOfWork.CreditNoteSubs.GetAsync(subitem.CrSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");


                    RefInvSubId = entity.RefInvSubId ?? entity.LabInvSubId ?? entity.ExpInvSubId;

                    // Restore balance qty
                    if (RefInvSubId > 0)
                    {
                        if (quote.Rejection.Value)
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId,entity.RejQty,0, "CreditNote Deletion", type);

                            if (entity.RefPoSubId > 0)
                            {
                                hasReject = await HasRejectReworkPoByPoSubidAsync(entity.RefPoSubId.Value);
                                if (hasReject)
                                {
                                    await AdjustPoSubBalanceAsync(entity.RefPoSubId.Value, entity.RejQty, 0, "CreditNote Deletion");
                                }
                            }
                        }
                        else
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId,entity.CrDrQty,0, "CreditNote Deletion", type);
                        }
                    }
                    else
                    {
                        await UpdateCostCenterAsync(subitem.CostId, false, changes);
                    }

                    // Delete from DB
                    await _unitOfWork.CreditNoteSubs.DeleteAsync(entity.CrSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Credit Note",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"CreditNote No: {quote?.CreditNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    quote.CreditNoteSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.CreditNoteSubs
                    .GetQueryable()
                    .Where(x => x.CrId == quote.CrId)
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

        public async Task<List<Dictionary<string, object>>> GetMfgInvoiceDetailsByCustId(int custId)
        {
            try
            {
                var result = await (from e in _unitOfWork.MfgInvs.GetQueryable()
                                    join es in _unitOfWork.MfgInvSubs.GetQueryable()
                                        on e.InvId equals es.InvId

                                    join ds in _unitOfWork.MfgDcSubs.GetQueryable()
                                        on es.RefDcSubId equals ds.DcSubId into dsg
                                    from ds in dsg.DefaultIfEmpty()

                                    join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                                        on (es.RefPoSubId ?? ds.RefPoSubId) equals ps.PoSubId into psg
                                    from ps in psg.DefaultIfEmpty()

                                    join p in _unitOfWork.MfgPos.GetQueryable()
                                        on ps.PoId equals p.PoId into pg
                                    from p in pg.DefaultIfEmpty()

                                    where e.CustId == custId && !e.IsCancel  && es.CrBalQty > 0 && e.Balance > 0
                                    select new
                                    {
                                        es.SlNo,
                                        es.InvSubId,
                                        es.InvId,
                                        e.InvNo,
                                        e.InvDate,

                                        e.DiscAmtOrPer,
                                        e.DiscountPercent,
                                        e.DiscountAmount,
                                        e.FreightCharges,
                                        e.PackingAmtOrPer,
                                        e.PackingPercent,
                                        e.PackingCharges,
                                        e.InsuranceAmtOrPer,
                                        e.InsurancePercent,
                                        e.InsuranceCharges,
                                        e.TCSAmtOrPer,
                                        e.TCSPercent,
                                        e.TCSAmount,
                                        e.OtherCharges,

                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.Specification,
                                        es.Item.MeasureUnit,
                                        es.Item.HSNCode,
                                        es.Item.Category.CategoryName,
                                        es.CrBalQty,
                                        es.UnitPrice,

                                        PoSubId = es.RefPoSubId ?? ds.RefPoSubId,

                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo,
                                        e.Customer

                                    }).ToListAsync();
                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["SlNo"] = r.SlNo,
                    ["InvSubId"] = r.InvSubId,
                    ["InvId"] = r.InvId,
                    ["InvNo"] = r.InvNo,
                    ["InvDate"] = r.InvDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["Specification"] = r.Specification ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Category"] = r.CategoryName ?? string.Empty,
                    ["Qty"] = r.CrBalQty,
                    ["CrBalQty"] = r.CrBalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["PoSubId"]=r.PoSubId,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["DiscAmtOrPer"]=r.DiscAmtOrPer,
                    ["DiscountPercent"]=r.DiscountPercent,
                    ["DiscountAmount"]=r.DiscountAmount,
                    ["FreightCharges"]=r.FreightCharges,
                    ["PackingAmtOrPer"]=r.PackingAmtOrPer,
                    ["PackingPercent"]=r.PackingPercent,
                    ["PackingCharges"]=r.PackingCharges,
                    ["InsuranceAmtOrPer"]=r.InsuranceAmtOrPer,
                    ["InsurancePercent"]=r.InsurancePercent,
                    ["InsuranceCharges"]=r.InsuranceCharges,
                    ["TCSAmtOrPer"]=r.TCSAmtOrPer,
                    ["TCSPercent"]=r.TCSPercent,
                    ["TCSAmount"]=r.TCSAmount,
                    ["OtherCharges"]=r.OtherCharges,
                    ["Customer"] = r.Customer.CustName ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetMfgInvoiceDetailsByCustId details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve GetMfgInvoiceDetailsByCustId. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetLabourInvoiceDetailsByCustId(int custId)
        {
            try
            {
                var result = await (from e in _unitOfWork.LabInvs.GetQueryable()
                                    join es in _unitOfWork.LabInvSubs.GetQueryable()
                                        on e.LabInvId equals es.LabInvId

                                    join ds in _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                                    on es.RefDcSubId equals ds.DcSubId into dsg
                                    from ds in dsg.DefaultIfEmpty()

                                    join d in _unitOfWork.LabourDcOutgoings.GetQueryable()
                                    on ds.DcId equals d.DcId into dg
                                    from d in dg.DefaultIfEmpty()

                                    join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                                    on ds.RefPoSubId equals ps.PoSubId into psg
                                    from ps in psg.DefaultIfEmpty()

                                    join p in _unitOfWork.MfgPos.GetQueryable()
                                    on ps.PoId equals p.PoId into pg
                                    from p in pg.DefaultIfEmpty()


                                    where e.CustId == custId && !e.InvCancel && es.CrBalQty > 0 && e.Balance > 0
                                    select new
                                    {
                                        es.SlNo,
                                        es.LabInvSubId,
                                        es.LabInvId,
                                        e.LabInvNo,
                                        e.LabInvDate,

                                        e.DiscAmtOrPer,
                                        e.DiscountPercent,
                                        e.DiscountAmount,
                                        e.FreightCharges,
                                        e.PackingAmtOrPer,
                                        e.PackingPercent,
                                        e.PackingCharges,
                                        e.InsuranceAmtOrPer,
                                        e.InsurancePercent,
                                        e.InsuranceCharges,
                                        e.TCSAmtOrPer,
                                        e.TCSPercent,
                                        e.TCSAmount,
                                        e.OtherCharges,

                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.Specification,
                                        es.Item.MeasureUnit,
                                        es.Item.HSNCode,
                                        es.Item.Category.CategoryName,
                                        es.CrBalQty,
                                        es.UnitPrice,

                                        PoSubId = ps != null ? ps.PoSubId : (int?)null,
                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo,
                                        e.Customer

                                    }).ToListAsync();
                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["SlNo"] = r.SlNo,
                    ["InvSubId"] = r.LabInvSubId,
                    ["InvId"] = r.LabInvId,
                    ["InvNo"] = r.LabInvNo,
                    ["InvDate"] = r.LabInvDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["Specification"] = r.Specification ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Category"] = r.CategoryName ?? string.Empty,
                    ["Qty"] = r.CrBalQty,
                    ["CrBalQty"] = r.CrBalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["PoSubId"] = r.PoSubId,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["DiscAmtOrPer"] = r.DiscAmtOrPer,
                    ["DiscountPercent"] = r.DiscountPercent,
                    ["DiscountAmount"] = r.DiscountAmount,
                    ["FreightCharges"] = r.FreightCharges,
                    ["PackingAmtOrPer"] = r.PackingAmtOrPer,
                    ["PackingPercent"] = r.PackingPercent,
                    ["PackingCharges"] = r.PackingCharges,
                    ["InsuranceAmtOrPer"] = r.InsuranceAmtOrPer,
                    ["InsurancePercent"] = r.InsurancePercent,
                    ["InsuranceCharges"] = r.InsuranceCharges,
                    ["TCSAmtOrPer"] = r.TCSAmtOrPer,
                    ["TCSPercent"] = r.TCSPercent,
                    ["TCSAmount"] = r.TCSAmount,
                    ["OtherCharges"] = r.OtherCharges,
                    ["Customer"] = r.Customer.CustName ?? string.Empty,

                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetLabourInvoiceDetailsByCustId details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve GetLabourInvoiceDetailsByCustId. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetExportInvoiceDetailsByCustId(int custId)
        {
            try
            {
                var result = await (from e in _unitOfWork.ExpInvs.GetQueryable()
                                    join es in _unitOfWork.ExpInvSubs.GetQueryable()
                                        on e.ExpInvId equals es.ExpInvId

                                   
                                    join ds in _unitOfWork.MfgDcSubs.GetQueryable()
                                    on es.RefDcSubId equals ds.DcSubId into dsg
                                    from ds in dsg.DefaultIfEmpty()

                                    join d in _unitOfWork.MfgDcs.GetQueryable()
                                    on ds.DcId equals d.DcId into dg
                                    from d in dg.DefaultIfEmpty()

                                    join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                                    on ds.RefPoSubId equals ps.PoSubId into psg
                                    from ps in psg.DefaultIfEmpty()

                                    join p in _unitOfWork.MfgPos.GetQueryable()
                                    on ps.PoId equals p.PoId into pg
                                    from p in pg.DefaultIfEmpty()

                                    where e.CustId == custId && !e.IsCancel && es.CrBalQty > 0 && e.Balance > 0
                                    select new
                                    {
                                        es.SlNo,
                                        es.ExpInvSubId,
                                        es.ExpInvId,
                                        e.ExpInvNo,
                                        e.ExpInvDate,

                                        e.DiscAmtOrPer,
                                        e.DiscountPercent,
                                        e.DiscountAmount,
                                        e.FreightCharges,
                                        e.PackingAmtOrPer,
                                        e.PackingPercent,
                                        e.PackingCharges,
                                        e.InsuranceAmtOrPer,
                                        e.InsurancePercent,
                                        e.InsuranceCharges,
                                        e.TCSAmtOrPer,
                                        e.TCSPercent,
                                        e.TCSAmount,
                                        e.OtherCharges,

                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.Specification,
                                        es.Item.MeasureUnit,
                                        es.Item.HSNCode,
                                        es.Item.Category.CategoryName,
                                        es.CrBalQty,
                                        es.UnitPrice,
                                        PoSubId = ps != null ? ps.PoSubId : es.RefPoSubId,
                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo,
                                        e.Customer

                                    }).ToListAsync();
                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["SlNo"] = r.SlNo,
                    ["InvSubId"] = r.ExpInvSubId,
                    ["InvId"] = r.ExpInvId,
                    ["InvNo"] = r.ExpInvNo,
                    ["InvDate"] = r.ExpInvDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["Specification"] = r.Specification ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Category"] = r.CategoryName ?? string.Empty,
                    ["Qty"] = r.CrBalQty,
                    ["CrBalQty"] = r.CrBalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["PoSubId"]=r.PoSubId,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["DiscAmtOrPer"] = r.DiscAmtOrPer,
                    ["DiscountPercent"] = r.DiscountPercent,
                    ["DiscountAmount"] = r.DiscountAmount,
                    ["FreightCharges"] = r.FreightCharges,
                    ["PackingAmtOrPer"] = r.PackingAmtOrPer,
                    ["PackingPercent"] = r.PackingPercent,
                    ["PackingCharges"] = r.PackingCharges,
                    ["InsuranceAmtOrPer"] = r.InsuranceAmtOrPer,
                    ["InsurancePercent"] = r.InsurancePercent,
                    ["InsuranceCharges"] = r.InsuranceCharges,
                    ["TCSAmtOrPer"] = r.TCSAmtOrPer,
                    ["TCSPercent"] = r.TCSPercent,
                    ["TCSAmount"] = r.TCSAmount,
                    ["OtherCharges"] = r.OtherCharges,
                    ["Customer"] = r.Customer.CustName ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetExportInvoiceDetailsByCustId details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve GetExportInvoiceDetailsByCustId. Please try again.");
            }
        }

        public async Task<CreditNoteVM> UpsertCreditNoteAsync(CreditNoteVM invoiceVM, int screenCode,string type)
        {
            if (invoiceVM == null)
                throw new ArgumentNullException(nameof(invoiceVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                CreditNote entity;
                
                if (invoiceVM.CrId == 0 || invoiceVM.CrId==null)
                {
                    entity = _mapper.Map<CreditNote>(invoiceVM);

                    entity.CreditNo = await _unitOfWork.CreditNotes.GetLastCreditNoAsync(invoiceVM.Suffix);
                    
                    
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.Balance = invoiceVM.GrandTotal;
                    entity.CreditNoteSubs = invoiceVM.CreditNoteSubVMs.Select(s => _mapper.Map<CreditNoteSub>(s)).ToList();

                    await _unitOfWork.CreditNotes.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    int? RefInvSubId = 0;
                    bool hasReject = false;
                    foreach (var subVM in entity.CreditNoteSubs)
                    {
                         RefInvSubId = subVM.RefInvSubId ?? subVM.LabInvSubId ?? subVM.ExpInvSubId;

                        if (RefInvSubId > 0)
                        {
                            if (entity.Rejection.Value)
                            {
                                await AdjustInvoiceSubBalanceAsync(RefInvSubId, 0,subVM.RejQty, "CreditNote Creation", type);

                                if (subVM.RefPoSubId>0) 
                                {
                                    hasReject = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId.Value);
                                    if (hasReject)
                                    {
                                        await AdjustPoSubBalanceAsync(subVM.RefPoSubId.Value,subVM.RejQty,0, "CreditNote Reject/Rework");
                                    }
                                }
                                 
                            }
                            else
                            {
                                await AdjustInvoiceSubBalanceAsync(RefInvSubId, 0, subVM.CrDrQty, "CreditNote Creation", type);
                            }

                        }
                       
                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }

                    await UpdatePendingBalanceAsync(type, entity, null);

                    changes.AppendLine("CreditNote Created.");
                }
                else
                {
                    entity = await _unitOfWork.CreditNotes.GetQueryable()
                        .Include(q => q.CreditNoteSubs)
                        .FirstOrDefaultAsync(q => q.CrId == invoiceVM.CrId)
                        ?? throw new InvalidOperationException("CreditNote not found.");

                    var parentChanges = GetPropertyChanges(entity, invoiceVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    var oldEntity = new CreditNote
                    {
                        CrId = entity.CrId,
                        GrandTotal = entity.GrandTotal,
                        Balance = entity.Balance,
                        CreditNoteSubs = entity.CreditNoteSubs
                                        .Select(x => new CreditNoteSub
                                        {
                                            CrSubId = x.CrSubId,
                                            RefInvSubId = x.RefInvSubId,
                                            LabInvSubId = x.LabInvSubId,
                                            ExpInvSubId = x.ExpInvSubId,
                                            CrDrQty = x.CrDrQty,
                                            RejQty = x.RejQty
                                        }).ToList()
                                        };

                    _mapper.Map(invoiceVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.Balance = invoiceVM.GrandTotal;
                    await HandleChildUpdatesAsync(entity, invoiceVM.CreditNoteSubVMs, changes, screenCode,type);
                    await UpdatePendingBalanceAsync(type, entity, oldEntity);
                    changes.AppendLine("CreditNote Updated.");
                }


                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, invoiceVM.CrId == 0 ? "CreditNote Created" : "CreditNote Updated");

                var savedEntity = await _unitOfWork.CreditNotes.GetQueryable()
                    .Include(q => q.CreditNoteSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.Currency)
                    .Include(q => q.CreditNoteSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.CrId == entity.CrId);

                return _mapper.Map<CreditNoteVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert CreditNote: {invoiceVM.CreditNo}");
                throw new InvalidOperationException("Failed to save CreditNote. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(CreditNote existingInv, List<CreditNoteSubVM> incomingSubVMs, StringBuilder changes, int screenCode,string type)
        {
            var existingSubIds = existingInv.CreditNoteSubs.Select(s => s.CrSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.CrSubId).ToHashSet();
            bool hasReject = false;
            // DELETE removed children
            int? RefInvSubId = 0;

            foreach (var sub in existingInv.CreditNoteSubs.Where(s => !incomingSubIds.Contains(s.CrSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - InvSubId: {sub.CrSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.CreditNoteSubs.DeleteAsync(sub.CrSubId);
                await _unitOfWork.SaveAsync();

                RefInvSubId = sub.RefInvSubId ?? sub.LabInvSubId ?? sub.ExpInvSubId;


                if (RefInvSubId > 0)
                {
                    if (existingInv.Rejection.Value)
                    {
                        await AdjustInvoiceSubBalanceAsync(RefInvSubId, sub.RejQty,0, "CreditNote Deletion", type);

                        if (sub.RefPoSubId > 0)
                        {
                            hasReject = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId.Value);
                            if (hasReject)
                            {
                                await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, sub.RejQty, 0, "CreditNote Reject/Rework");
                            }
                        }
                    }
                    else
                    {
                        await AdjustInvoiceSubBalanceAsync(RefInvSubId, sub.CrDrQty,0, "CreditNote Deletion", type);
                    }
                }
                
                else
                    await UpdateCostCenterAsync(sub.CostId, false, changes);
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.CrSubId == 0)
                {
                    var newSub = _mapper.Map<CreditNoteSub>(subVM);
                    newSub.CrId = existingInv.CrId.Value;
                    await _unitOfWork.CreditNoteSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.RejQty}");

                    RefInvSubId = subVM.RefInvSubId ?? subVM.LabInvSubId ?? subVM.ExpInvSubId;

                    if (RefInvSubId > 0)
                    {
                        
                        if (existingInv.Rejection.Value)
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, 0, subVM.RejQty, "CreditNote Deletion", type);

                            if (subVM.RefPoSubId > 0)
                            {
                                hasReject = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId.Value);
                                if (hasReject)
                                {
                                    await AdjustPoSubBalanceAsync(subVM.RefPoSubId.Value, subVM.RejQty, 0, "CreditNote Deletion");
                                }
                            }
                        }
                        else
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, 0, subVM.CrDrQty,"CreditNote Deletion", type);
                        }
                    }
                    else
                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                }
                else
                {
                    var existingSub = existingInv.CreditNoteSubs.FirstOrDefault(s => s.CrSubId == subVM.CrSubId);
                    if (existingSub != null)
                    {
                        // Rollback old CostCenter if needed
                        if ((existingSub.CostId != subVM.CostId) && (existingSub.RefInvSubId == null || existingSub.RefInvSubId == 0))
                            await UpdateCostCenterAsync(existingSub.CostId, false, changes);

                        RefInvSubId = subVM.RefInvSubId ?? subVM.LabInvSubId ?? subVM.ExpInvSubId;

                        if (RefInvSubId > 0)
                        {
                            if (existingInv.Rejection.Value)
                            {
                                await AdjustInvoiceSubBalanceAsync(RefInvSubId, existingSub.RejQty, subVM.RejQty, "CreditNote Deletion", type);

                                if (subVM.RefPoSubId > 0)
                                {
                                    hasReject = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId.Value);
                                    if (hasReject)
                                    {
                                        await AdjustPoSubBalanceAsync(subVM.RefPoSubId.Value, subVM.RejQty, 0, "CreditNote Deletion");
                                    }
                                }
                            }
                            else
                            {
                                await AdjustInvoiceSubBalanceAsync(RefInvSubId, existingSub.CrDrQty, subVM.CrDrQty, "CreditNote Deletion", type);
                            }
                        }
                        
                        // Assign new CostCenter if needed
                        if ((subVM.CostId > 0) && (RefInvSubId == null || subVM.RefInvSubId == 0))
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
                screen: "CreditNote",
                action: action,
                additionalInfo: changes.ToString()
            );
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
                        throw new InvalidOperationException("This CreditNote cannot be deleted because the rejected quantity was returned " +
                            "to the PO and has already been used in another Mfg DC or Mfg invoice.");

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

        public async Task<List<CreditNoteListVM>> GetCreditNoteListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<CreditNoteListVM>("Sp_GetCreditNoteList", status);
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

        public async Task UpdatePendingBalanceAsync(string CreditType, CreditNote newItem, CreditNote oldItem)
        {
            try
            {
                CreditType = CreditType?.ToUpper()?.Trim() ?? "";

                int? invoiceSubId =
                    newItem?.CreditNoteSubs?
                        .FirstOrDefault()?.RefInvSubId
                    ?? newItem?.CreditNoteSubs?
                        .FirstOrDefault()?.LabInvSubId
                    ?? newItem?.CreditNoteSubs?
                        .FirstOrDefault()?.ExpInvSubId
                    ?? oldItem?.CreditNoteSubs?
                        .FirstOrDefault()?.RefInvSubId
                    ?? oldItem?.CreditNoteSubs?
                        .FirstOrDefault()?.LabInvSubId
                    ?? oldItem?.CreditNoteSubs?
                        .FirstOrDefault()?.ExpInvSubId;

                if (invoiceSubId == null || invoiceSubId == 0)
                    return;
                switch (CreditType)
                {
                    case "LABOUR":
                        {
                            var invoiceIds = await (
                                                from labSub in _unitOfWork.LabInvSubs.GetQueryable()
                                                join labInv in _unitOfWork.LabInvs.GetQueryable()
                                                    on labSub.LabInvId equals labInv.LabInvId
                                                where labSub.LabInvSubId == invoiceSubId
                                                select labInv.LabInvId
                                            ).Distinct().ToListAsync();

                            if (!invoiceIds.Any())
                                return;

                            decimal oldTotal = oldItem?.GrandTotal ?? 0;

                            decimal newTotal = newItem?.GrandTotal ?? 0;

                            decimal delta = newTotal - oldTotal;

                            var invoices = await _unitOfWork.LabInvs
                                .GetQueryable()
                                .Where(x => invoiceIds.Contains(x.LabInvId))
                                .ToListAsync();

                            foreach (var inv in invoices)
                            {
                                inv.Balance -= delta;

                                if (inv.Balance < 0)
                                    inv.Balance = 0;

                                inv.LabInvTally = inv.Balance == 0;

                                await _unitOfWork.LabInvs.UpdateAsync(inv);
                            }

                            break;
                        }
                    case "EXPORT":
                        {
                            var invoiceIds = await (
                                                from expSub in _unitOfWork.ExpInvSubs.GetQueryable()
                                                join expInv in _unitOfWork.ExpInvs.GetQueryable()
                                                    on expSub.ExpInvId equals expInv.ExpInvId
                                                where expSub.ExpInvSubId == invoiceSubId
                                                select expInv.ExpInvId
                                            ).Distinct().ToListAsync();
                            if (!invoiceIds.Any())
                                return;
                            decimal oldTotal = oldItem?.GrandTotal ?? 0;
                            decimal newTotal = newItem?.GrandTotal ?? 0;
                            decimal delta = newTotal - oldTotal;
                            var invoices = await _unitOfWork.ExpInvs
                                .GetQueryable()
                                .Where(x => invoiceIds.Contains(x.ExpInvId))
                                .ToListAsync();
                            foreach (var inv in invoices)
                            {
                                inv.Balance -= delta;
                                if (inv.Balance < 0)
                                    inv.Balance = 0;
                                inv.InvTally = inv.Balance == 0;
                                await _unitOfWork.ExpInvs.UpdateAsync(inv);
                            }
                            break;
                        }
                    case "SALES":
                        {
                            var invoiceIds = await (
                                                from salesSub in _unitOfWork.MfgInvSubs.GetQueryable()
                                                join salesInv in _unitOfWork.MfgInvs.GetQueryable()
                                                    on salesSub.InvId equals salesInv.InvId
                                                where salesSub.InvId == invoiceSubId
                                                select salesInv.InvId
                                            ).Distinct().ToListAsync();
                            if (!invoiceIds.Any())
                                return;
                            decimal oldTotal = oldItem?.GrandTotal ?? 0;
                            decimal newTotal = newItem?.GrandTotal ?? 0;
                            decimal delta = newTotal - oldTotal;
                            var invoices = await _unitOfWork.MfgInvs
                                .GetQueryable()
                                .Where(x => invoiceIds.Contains(x.InvId))
                                .ToListAsync();
                            foreach (var inv in invoices)
                            {
                                inv.Balance -= delta;
                                if (inv.Balance < 0)
                                    inv.Balance = 0;
                                inv.InvTally = inv.Balance == 0;
                                await _unitOfWork.MfgInvs.UpdateAsync(inv);
                            }
                            break;
                        }
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in UpdatePendingBalanceAsync");
                throw;
            }
        }



        //Cancel CreditNote
        public async Task ValidateCreditNoteBalanceBeforeRevertAsync(CreditNoteSub sub)
        {
            try
            {
                int RefMfgInvSubId = 0, RefLabSubId = 0, RefExpSubId = 0;

                int.TryParse(sub.RefInvSubId.ToString(), out RefMfgInvSubId);
                int.TryParse(sub.LabInvSubId.ToString(), out RefLabSubId);
                int.TryParse(sub.ExpInvSubId.ToString(), out RefExpSubId);


                if (RefMfgInvSubId > 0 || RefLabSubId > 0 || RefExpSubId > 0)
                {

                    if (RefMfgInvSubId > 0)
                    {
                        var entity = await _unitOfWork.MfgInvSubs.GetAsync(sub.RefInvSubId.Value);
                        if (entity == null)
                            throw new InvalidOperationException($"Invoice not found for RefInvSubId: {sub.RefInvSubId}");

                        if (entity.CrBalQty < sub.Qty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot revert because Mfg Invoice balance ({entity.CrBalQty}) is less than required quantity ({sub.Qty})."
                            );
                        }
                    }
                    if (RefLabSubId > 0)
                    {
                        var entity = await _unitOfWork.LabInvSubs.GetAsync(sub.LabInvSubId.Value);
                        if (entity == null)
                            throw new InvalidOperationException($"Labour Invoice not found for RefLabSubId: {sub.LabInvSubId}");

                        if (entity.CrBalQty < sub.Qty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot revert because Lab Invoice balance ({entity.CrBalQty}) is less than required quantity ({sub.Qty})."
                            );
                        }
                    }

                    if (RefExpSubId > 0)
                    {
                        var entity = await _unitOfWork.ExpInvSubs.GetAsync(sub.ExpInvSubId.Value);
                        if (entity == null)
                            throw new InvalidOperationException($"Export Invoice not found for RefExpSubId: {sub.ExpInvSubId}");

                        if (entity.CrBalQty < sub.Qty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot revert because Export Invoice balance ({entity.CrBalQty}) is less than required quantity ({sub.Qty})."
                            );
                        }
                    }

                }

            }
            catch (Exception ex)
            {

                throw;
            }


        }
        public async Task<List<CreditNoteSub>> GetCreditSubDetailsByCrIdAsync(int CrId)
        {
            try
            {
                var subs = await _unitOfWork.CreditNoteSubs
                                 .GetQueryable()
                                 .Where(s => s.CrId == CrId)
                                 .OrderBy(s => s.SlNo)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching CreditNoteSuc  items for CrId: {CrId}");
                throw new InvalidOperationException("Failed to retrieve CreditNote Sub Invoice sub-items. Please try again.");
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(CreditNoteVM ceditNoteVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingInvoice = await _unitOfWork.CreditNotes.GetAsync(ceditNoteVM.CrId.Value);
                if (existingInvoice == null)
                    throw new InvalidOperationException("CreditNote  not found.");

                var subs = await _unitOfWork.CreditNoteSubs
                    .GetQueryable()
                    .Where(s => s.CrId == ceditNoteVM.CrId)
                    .ToListAsync();

                if (!ceditNoteVM.Cancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateCreditNoteBalanceBeforeRevertAsync(sub);
                    }
                }

                existingInvoice.Cancel = ceditNoteVM.Cancel;
                existingInvoice.CancelReason = ceditNoteVM.CancelReason;
                await _unitOfWork.CreditNotes.UpdateAsync(existingInvoice);
                await _unitOfWork.SaveAsync();

                int RefMfgInvSubId = 0, RefLabSubId = 0, RefExpSubId = 0;

                
                foreach (var sub in subs)
                {
                    int.TryParse(sub.RefInvSubId.ToString(), out RefMfgInvSubId);
                    int.TryParse(sub.LabInvSubId.ToString(), out RefLabSubId);
                    int.TryParse(sub.ExpInvSubId.ToString(), out RefExpSubId);

                    if (existingInvoice.Cancel)
                    {
                        if (RefMfgInvSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.RefInvSubId.Value, sub.Qty, 0, $"CreditNote Cancelled - {existingInvoice.CreditNo}", "Sales");
                        }

                        if (RefLabSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.LabInvSubId.Value, sub.Qty, 0, $"CreditNote  Cancelled - {existingInvoice.CreditNo}", "Labour");

                        }

                        if (RefExpSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.RefPoSubId.Value, sub.Qty, 0, $"CreditNote Cancelled - {existingInvoice.CreditNo}", "Export");

                        }
                    }
                    else
                    {
                        if (RefMfgInvSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.RefInvSubId.Value, 0, sub.Qty, $"CreditNote  Reverted - {existingInvoice.CreditNo}", "Sales");
                        }

                        if (RefLabSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.LabInvSubId.Value, 0, sub.Qty, $"CreditNote  Reverted - {existingInvoice.CreditNo}", "Labour");

                        }
                        if (RefExpSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.ExpInvSubId.Value, 0, sub.Qty, $"CreditNote  Reverted - {existingInvoice.CreditNo}", "Export");
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
    }
}
