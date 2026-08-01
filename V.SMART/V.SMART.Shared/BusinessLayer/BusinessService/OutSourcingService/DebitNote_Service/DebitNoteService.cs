using AutoMapper;


using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IDebitNote_Service;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.OutSourcing.Debit_Note;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.DebitNote_VM;
using V.SMART.Shared.ViewModels.ReportViewModel.DebitNoteListViewModel;

namespace IQSMART.Shared.BusinessLayer.BusinessService.OutSourcingService.DebitNote_Service
{
    public class DebitNoteService: IDebitNoteService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;



        public DebitNoteService(
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

        public async Task<List<VendorVM>> GetVendorsAsync(int? VendorCode = null)
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

        public async Task<VendorVM?> GetVendorsByIdAsync(int VendorCode)
            => await _commonService.GetVendorByVenerCodeAsync(VendorCode);

        // 🔹 Contacts
        public async Task<List<VendorContact>> GetContactPersonsAsync(int VendorCode)
            => await _commonService.GetContactPersonsVendorAsync(VendorCode);


        // 🔹 Consignee addresses
        public async Task<List<VendorInDirect>> GetConsigneeAddressesAsync(int VendorCode)
            => await _commonService.GetConsigneeAddressesVendorAsync (VendorCode);

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


        //DebitNote Operation

        public async Task<(List<DebitNoteVM> debitNoteVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.DebitNotes.GetQueryable().AsSplitQuery().AsNoTracking()
                .Include(j => j.Vendor)
                .Include(j => j.DebitNoteSubs).ThenInclude(s => s.Item)
                .Include(q => q.Currency)
                .Include(q => q.Vendor).ThenInclude(c => c.VendorInDirects)
                .Include(q => q.DebitNoteSubs).ThenInclude(s => s.CostCenter)
                .Include(j => j.DebitNoteSubs).ThenInclude(ps => ps.PurchaseInvoiceSub).ThenInclude(p => p.PurchaseInvoice)
                .Include(j => j.DebitNoteSubs).ThenInclude(ps => ps.SubConInvSub).ThenInclude(p => p.SubConInv)
                .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = DebitNoteFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.DbId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<DebitNoteVM>>(list);

            return (vmList, total);
        }

        public static class DebitNoteFilterBuilder
        {
            public static IQueryable<DebitNote> ApplyFilter(
                IQueryable<DebitNote> query, string field, object value)
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
                                (string.IsNullOrEmpty(part1) || x.DebitNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }
                    case "Vendor":
                        return query.Where(x => x.Vendor.VendorName.Contains(value.ToString()));

                    case "ItemCode":
                        return query.Where(x => x.DebitNoteSubs.Any(s => s.Item.ItemCode.Contains(value.ToString())));
                    case "ItemName":
                        return query.Where(x => x.DebitNoteSubs.Any(s => s.Item.ItemName.Contains(value.ToString())));
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
                var prefix = await _unitOfWork.DebitNotes.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.DbId)
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

        public async Task<string> GetDebitNoteNumberAsync(string suffix)
        {
            try
            {
                var lastQuote = await _unitOfWork.DebitNotes
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => Convert.ToInt32(q.DebitNo))
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastQuote != null)
                {
                    var parts = lastQuote.DebitNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating GetDebitNoteNumberAsync for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate GetDebitNoteNumberAsync.");
            }
        }

        public async Task<int> GetPendingInvCountAsync(int VendorCode, string type)
        {
            try
            {
                int count = 0;

                if (type == "Purchase")
                {
                    count = await _unitOfWork.PurchaseInvoices.GetQueryable()
                            .Where(m => m.VendorCode == VendorCode && m.Balance > 0
                            && _unitOfWork.PurchaseInvoiceSubs.GetQueryable()
                            .Any(s => s.InvId == m.InvId && !m.InvCancel && s.DbBalQty > 0)).CountAsync();
                }
                else if (type == "SubContract")
                {
                    count = await _unitOfWork.SubConInvs.GetQueryable()
                            .Where(m => m.VendorCode == VendorCode && m.Balance > 0
                            && _unitOfWork.SubConInvSubs.GetQueryable()
                            .Any(s => s.InvId == m.InvId && !m.InvCancel && s.DbBalQty > 0)).CountAsync();

                }
                
                return count;
            }
            catch (Exception ex)
            {

                throw;
            }

        }

        public async Task<DebitNote?> GetLastdebitNoteAsync(int VendorCode)
        {
            try
            {
                return await _unitOfWork.DebitNotes.GetLatestAsync(
                    q => q.VendorCode == VendorCode,
                    q => q.DbId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastdebitNoteAsync for CustId: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve last Debitnote. Please try again.");
            }
        }


        public async Task<bool> DeleteDebitNoteByDbIdAsync(int DbId, int screenCode, string type)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var debit = await _unitOfWork.DebitNotes
                    .GetQueryable()
                    .Include(e => e.DebitNoteSubs)
                    .FirstOrDefaultAsync(e => e.DbId == DbId);

                if (debit == null)
                {
                    return false;
                }

                int? RefInvSubId = 0;

                var changes = new StringBuilder();
                bool hasReject = false;
                foreach (var sub in debit.DebitNoteSubs)
                {
                    RefInvSubId = sub.RefPurchInvSubId ?? sub.RefSubConInvSubId;
                    if (RefInvSubId > 0)
                    {
                        if (debit.Rejection.Value)
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, sub.RejQty, 0, "DebitNote Deletion", type);

                            if (sub.RefPoSubId > 0)
                            {
                                hasReject = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId.Value);
                                if (hasReject)
                                {
                                    await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, 0, sub.RejQty, "DebitNote Reject/Rework");
                                }
                            }

                        }
                        else
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, sub.CrDrQty, 0, "DebitNote Deletion", type);
                        }

                    }

                    else
                    {
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                    }
                }



                //----------------------------------------------------------------------------
                // Delete the quotation
                var deleted = await _unitOfWork.DebitNotes.DeleteAsync(DbId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "DebitNote List",
                    action: $"Deleted DebitNote: {debit.DebitNo}",
                    additionalInfo: $"DebitNote Id: {debit.DbId}\n{changes}"
                );

                return true;
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPOBalance] Validation failed in DebitNote Deletion");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete DebitNote CrId: {DbId}");
                throw;
            }
        }

        public async Task<bool> HasRejectReworkPoByPoSubidAsync(int PosubId)
        {
            try
            {
                return await (from poSub in _unitOfWork.PurchPoSubs.GetQueryable()
                              join po in _unitOfWork.PurchPos.GetQueryable()
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
                if (type == "Purchase")
                {
                    invSub = await _unitOfWork.PurchaseInvoiceSubs.GetAsync(refinvSubId.Value);
                }
                else if (type == "SubContract")
                {
                    invSub = await _unitOfWork.SubConInvSubs.GetAsync(refinvSubId.Value);
                }
               

                if (invSub == null)
                    return;


                if (oldQty > 0)
                    invSub.DbBalQty += oldQty;


                if (newQty > invSub.DbBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Invoice CrBalQty.");


                if (newQty > 0)
                    invSub.DbBalQty -= newQty;

                if (type == "Purchase")
                    await _unitOfWork.PurchaseInvoiceSubs.UpdateAsync(invSub);
                else if (type == "SubContract")
                    await _unitOfWork.SubConInvSubs.UpdateAsync(invSub);
                

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

        public async Task<DebitNoteVM?> GetDebitNoteByDbIdAsync(int DbId)
        {
            try
            {
                var entity = await _unitOfWork.DebitNotes.GetQueryable()
                    .Include(q => q.DebitNoteSubs)
                    .Include(q => q.DebitNoteSubs).ThenInclude(s => s.Item)
                    .Include(q => q.DebitNoteSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.DebitNoteSubs).ThenInclude(s => s.PurchaseInvoiceSub).ThenInclude(m => m.PurchaseInvoice)
                    .Include(q => q.DebitNoteSubs).ThenInclude(s => s.SubConInvSub).ThenInclude(m => m.SubConInv)
                    .Include(q => q.Vendor).ThenInclude(c => c.VendorInDirects)
                    .Include(q => q.Currency)
                    .FirstOrDefaultAsync(q => q.DbId == DbId);

                return _mapper.Map<DebitNoteVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetDebitNoteByDbIdAsync({DbId})");
                return null;
            }
        }

        public async Task<decimal> GetInvItemBalQtyFromDcSubId(int invSubId, string type)
        {
            try
            {
                switch (type)
                {
                    case "Purchase":
                        return await _unitOfWork.PurchaseInvoiceSubs.GetQueryable()
                            .Where(e => e.InvSubId == invSubId)
                            .Select(e => e.DbBalQty)
                            .FirstOrDefaultAsync();

                    case "SubContract":
                        return await _unitOfWork.SubConInvSubs.GetQueryable()
                            .Where(e => e.InvSubId == invSubId)
                            .Select(e => e.DbBalQty)
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

        public async Task<List<DebitNoteSubVM>> GetDebitNoteSubByDbIdAsync(int DbId)
        {
            try
            {
                var subs = await _unitOfWork.DebitNoteSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.DbId == DbId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<DebitNoteSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DebitNoteSub items for DbId: {DbId}");
                throw new InvalidOperationException("Failed to retrieve Invoice sub-items. Please try again.");
            }
        }

        public async Task<DebitNoteSubVM?> GetDebitNoteSubItemDetailByDbSubIdAsync(int DbSubId, string reason)
        {
            try
            {
                // Fetch sub-item by CrSubId
                var subItem = await _unitOfWork.DebitNoteSubs
                    .GetQueryable()
                    .Where(q => q.DbSubId == DbSubId)
                    .Select(q => new DebitNoteSubVM
                    {
                        Qty = q.Qty,
                        RejQty = q.RejQty,
                        CrDrQty = q.CrDrQty,
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
                    subItem.RejQty = 0;
                }

                return subItem;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Debit Note sub item detail for DbSubId: {DbSubId}");
                throw new InvalidOperationException("Failed to retrieve Debit Note sub-item details.");
            }
        }

        public async Task DeleteAndResequenceAsync(DebitNoteSubVM subitem, DebitNoteVM quote, int screenCode, string type)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                int? RefInvSubId = 0;
                if (subitem.DbSubId > 0) // persisted subitem
                {
                    bool hasReject = false;
                    var entity = await _unitOfWork.DebitNoteSubs.GetAsync(subitem.DbSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");


                    RefInvSubId = entity.RefPurchInvSubId ?? entity.RefSubConInvSubId ;

                    // Restore balance qty
                    if (RefInvSubId > 0)
                    {
                        if (quote.Rejection.Value)
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, entity.RejQty, 0, "DebitNote Deletion", type);

                            if (entity.RefPoSubId > 0)
                            {
                                hasReject = await HasRejectReworkPoByPoSubidAsync(entity.RefPoSubId.Value);
                                if (hasReject)
                                {
                                    await AdjustPoSubBalanceAsync(entity.RefPoSubId.Value, entity.RejQty, 0, "DebitNote Deletion");
                                }
                            }
                        }
                        else
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, entity.CrDrQty, 0, "DebitNote Deletion", type);
                        }
                    }
                    else
                    {
                        await UpdateCostCenterAsync(subitem.CostId, false, changes);
                    }

                    // Delete from DB
                    await _unitOfWork.DebitNoteSubs.DeleteAsync(entity.DbSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Debit Note",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"DebitNote No: {quote?.DebitNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    quote.DebitNoteSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.DebitNoteSubs
                    .GetQueryable()
                    .Where(x => x.DbId == quote.DbId)
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

        public async Task<List<Dictionary<string, object>>> GetPurchaseInvoiceDetailsByVendorCode(int VendorCode)
        {
            try
            {
                var result = await (from e in _unitOfWork.PurchaseInvoices.GetQueryable()
                                    join es in _unitOfWork.PurchaseInvoiceSubs.GetQueryable()
                                        on e.InvId equals es.InvId

                                    join ds in _unitOfWork.PurchaseSCNSubs.GetQueryable()
                                        on es.RefSCNSubId equals ds.SCNSubId into dsg
                                    from ds in dsg.DefaultIfEmpty()

                                    join ps in _unitOfWork.PurchPoSubs.GetQueryable()
                                        on (es.RefPoSubId ?? ds.RefPoSubId) equals ps.PoSubId into psg
                                    from ps in psg.DefaultIfEmpty()

                                    join p in _unitOfWork.PurchPos.GetQueryable()
                                        on ps.PoId equals p.PoId into pg
                                    from p in pg.DefaultIfEmpty()

                                    where e.VendorCode == VendorCode && !e.InvCancel && es.DbBalQty > 0 && e.Balance > 0
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
                                        es.DbBalQty,
                                        es.UnitPrice,

                                        PoSubId = es.RefPoSubId ?? ds.RefPoSubId,

                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo,
                                        e.Vendor

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
                    ["Qty"] = r.DbBalQty,
                    ["CrBalQty"] = r.DbBalQty,
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
                    ["Vendor/Supplier"] = r.Vendor.VendorName,

                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetPurchaseInvoiceDetailsByVendorCode details for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve GetPurchaseInvoiceDetailsByVendorCode. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetSubContractInvoiceDetailsByVendorCode(int VendorCode)
        {
            try
            {
                var result = await (from e in _unitOfWork.SubConInvs.GetQueryable()
                                    join es in _unitOfWork.SubConInvSubs.GetQueryable()
                                        on e.InvId equals es.InvId

                                    join ds in _unitOfWork.SubConSCNSubs.GetQueryable()
                                    on es.RefSCNSubId equals ds.SCNSubId into dsg
                                    from ds in dsg.DefaultIfEmpty()

                                    join d in _unitOfWork.SubConSCNs.GetQueryable()
                                    on ds.SCNId equals d.SCNId into dg
                                    from d in dg.DefaultIfEmpty()

                                   

                                    join ps in _unitOfWork.PurchPoSubs.GetQueryable()
                                    on ds.PoSubId equals ps.PoSubId into psg
                                    from ps in psg.DefaultIfEmpty()

                                    join p in _unitOfWork.PurchPos.GetQueryable()
                                    on ps.PoId equals p.PoId into pg
                                    from p in pg.DefaultIfEmpty()


                                    where e.VendorCode == VendorCode && !e.InvCancel && es.DbBalQty > 0 && e.Balance > 0
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
                                        es.DbBalQty,
                                        es.UnitPrice,

                                        PoSubId = ps != null ? ps.PoSubId : (int?)null,
                                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                                        es.CostCenter.ProjectNo,
                                        e.Vendor
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
                    ["Qty"] = r.DbBalQty,
                    ["CrBalQty"] = r.DbBalQty,
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
                    ["Vendor/Supplier"] = r.Vendor.VendorName,

                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetSubContractInvoiceDetailsByVendorCode details for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve GetSubContractInvoiceDetailsByVendorCode. Please try again.");
            }
        }


        public async Task<DebitNoteVM> UpsertDebitNoteAsync(DebitNoteVM invoiceVM, int screenCode, string type)
        {
            if (invoiceVM == null)
                throw new ArgumentNullException(nameof(invoiceVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                DebitNote entity;

                if (invoiceVM.DbId == 0 || invoiceVM.DbId == null)
                {
                    entity = _mapper.Map<DebitNote>(invoiceVM);

                    entity.DebitNo = await _unitOfWork.DebitNotes.GetLastDebitNoAsync(invoiceVM.Suffix);


                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.Balance = invoiceVM.GrandTotal;
                    entity.DebitNoteSubs = invoiceVM.DebitNoteSubVMs.Select(s => _mapper.Map<DebitNoteSub>(s)).ToList();

                    await _unitOfWork.DebitNotes.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    int? RefInvSubId = 0;
                    bool hasReject = false;
                    foreach (var subVM in entity.DebitNoteSubs)
                    {
                        RefInvSubId = subVM.RefPurchInvSubId ?? subVM.RefSubConInvSubId ;

                        if (RefInvSubId > 0)
                        {
                            if (entity.Rejection.Value)
                            {
                                await AdjustInvoiceSubBalanceAsync(RefInvSubId, 0, subVM.RejQty, "DebitNote Creation", type);

                                if (subVM.RefPoSubId > 0)
                                {
                                    hasReject = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId.Value);
                                    if (hasReject)
                                    {
                                        await AdjustPoSubBalanceAsync(subVM.RefPoSubId.Value, subVM.RejQty, 0, "DebitNote Reject/Rework");
                                    }
                                }

                            }
                            else
                            {
                                await AdjustInvoiceSubBalanceAsync(RefInvSubId, 0, subVM.CrDrQty, "DebitNote Creation", type);
                            }

                        }

                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                    }

                    changes.AppendLine("CreditNote Created.");
                }
                else
                {
                    entity = await _unitOfWork.DebitNotes.GetQueryable()
                        .Include(q => q.DebitNoteSubs)
                        .FirstOrDefaultAsync(q => q.DbId == invoiceVM.DbId)
                        ?? throw new InvalidOperationException("DebitNote not found.");

                    var parentChanges = GetPropertyChanges(entity, invoiceVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(invoiceVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.Balance = invoiceVM.GrandTotal;
                    await HandleChildUpdatesAsync(entity, invoiceVM.DebitNoteSubVMs, changes, screenCode, type);

                    changes.AppendLine("DebitNote Updated.");
                }


                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, invoiceVM.DbId == 0 ? "DebitNote Created" : "DebitNote Updated");

                var savedEntity = await _unitOfWork.DebitNotes.GetQueryable()
                    .Include(q => q.DebitNoteSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Vendor)
                    .Include(q => q.Currency)
                    .Include(q => q.DebitNoteSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.DbId == entity.DbId);

                return _mapper.Map<DebitNoteVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert DebitNote: {invoiceVM.DebitNo}");
                throw new InvalidOperationException("Failed to save DebitNote. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(DebitNote existingInv, List<DebitNoteSubVM> incomingSubVMs, StringBuilder changes, int screenCode, string type)
        {
            var existingSubIds = existingInv.DebitNoteSubs.Select(s => s.DbSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.DbSubId).ToHashSet();
            bool hasReject = false;
            // DELETE removed children
            int? RefInvSubId = 0;

            foreach (var sub in existingInv.DebitNoteSubs.Where(s => !incomingSubIds.Contains(s.DbSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - InvSubId: {sub.DbSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.DebitNoteSubs.DeleteAsync(sub.DbSubId);
                await _unitOfWork.SaveAsync();

                RefInvSubId = sub.RefPurchInvSubId ?? sub.RefSubConInvSubId;


                if (RefInvSubId > 0)
                {
                    if (existingInv.Rejection.Value)
                    {
                        await AdjustInvoiceSubBalanceAsync(RefInvSubId, sub.RejQty, 0, "CreditNote Deletion", type);

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
                        await AdjustInvoiceSubBalanceAsync(RefInvSubId, sub.CrDrQty, 0, "CreditNote Deletion", type);
                    }
                }

                else
                    await UpdateCostCenterAsync(sub.CostId, false, changes);
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.DbSubId == 0)
                {
                    var newSub = _mapper.Map<DebitNoteSub>(subVM);
                    newSub.DbId = existingInv.DbId.Value;
                    await _unitOfWork.DebitNoteSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.RejQty}");

                    RefInvSubId = subVM.RefPurchInvSubId ?? subVM.RefSubConInvSubId ;

                    if (RefInvSubId > 0)
                    {

                        if (existingInv.Rejection.Value)
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, 0, subVM.RejQty, "DebitNote Deletion", type);

                            if (subVM.RefPoSubId > 0)
                            {
                                hasReject = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId.Value);
                                if (hasReject)
                                {
                                    await AdjustPoSubBalanceAsync(subVM.RefPoSubId.Value, subVM.RejQty, 0, "DebitNote Deletion");
                                }
                            }
                        }
                        else
                        {
                            await AdjustInvoiceSubBalanceAsync(RefInvSubId, 0, subVM.CrDrQty, "DebitNote Deletion", type);
                        }
                    }
                    else
                        await UpdateCostCenterAsync(subVM.CostId, true, changes);
                }
                else
                {
                    var existingSub = existingInv.DebitNoteSubs.FirstOrDefault(s => s.DbSubId == subVM.DbSubId);
                    if (existingSub != null)
                    {
                        // Rollback old CostCenter if needed
                        if ((existingSub.CostId != subVM.CostId) && (existingSub.RefPurchInvSubId == null || existingSub.RefPurchInvSubId == 0))
                            await UpdateCostCenterAsync(existingSub.CostId, false, changes);

                        RefInvSubId = subVM.RefPurchInvSubId ?? subVM.RefSubConInvSubId;

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
                        if ((subVM.CostId > 0) && (RefInvSubId == null || subVM.RefPurchInvSubId == 0))
                            await UpdateCostCenterAsync(subVM.CostId, true, changes);


                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }

            }
        }


        //CostCenter update helper
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

                var isOpenPo = await _unitOfWork.PurchPoSubs
                                .GetQueryable()
                                .Where(s => s.PoSubId == refPoSubId)
                                .Join(_unitOfWork.PurchPos.GetQueryable(),
                                        sub => sub.PoId,
                                        po => po.PoId,
                                        (sub, po) => po.IsOpenPO)
                                .FirstOrDefaultAsync();

                if (!isOpenPo)
                {
                    var PoSub = await _unitOfWork.PurchPoSubs.GetAsync(refPoSubId.Value);
                    if (PoSub == null) return;

                    if (oldQty > 0)
                        PoSub.BalQty += oldQty;

                    if (newQty > PoSub.BalQty)
                        throw new InvalidOperationException("This CreditNote cannot be deleted because the rejected quantity was returned " +
                            "to the PO and has already been used in another Mfg DC or Mfg invoice.");

                    if (newQty > 0)
                        PoSub.BalQty -= newQty;

                    await _unitOfWork.PurchPoSubs.UpdateAsync(PoSub);
                    await _unitOfWork.SaveAsync();

                    var totalBalQty = await _unitOfWork.PurchPoSubs
                        .GetQueryable()
                        .Where(e => e.PoId == PoSub.PoId && !e.ItemCancel)
                        .SumAsync(e => e.BalQty);

                    var po = await _unitOfWork.PurchPos.GetAsync(PoSub.PoId);
                    if (po != null)
                    {
                        po.PoTally = (totalBalQty == 0);
                        await _unitOfWork.PurchPos.UpdateAsync(po);
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


        public async Task<List<DebitNoteListVM>> GetDebitNoteListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<DebitNoteListVM>("Sp_GetDebitNoteList", status);
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

        //Cancellation validation for DebitNote Sub items
        public async Task ValidateCreditNoteBalanceBeforeRevertAsync(DebitNoteSub sub)
        {
            try
            {
                int RefpurSubId = 0, RefSubSubId = 0;

                int.TryParse(sub.RefPurchInvSubId.ToString(), out RefpurSubId);
                int.TryParse(sub.RefSubConInvSubId.ToString(), out RefSubSubId);

                if (RefpurSubId > 0 || RefSubSubId > 0)
                {

                    if (RefpurSubId > 0)
                    {
                        var entity = await _unitOfWork.PurchaseInvoiceSubs.GetAsync(sub.RefPurchInvSubId.Value);
                        if (entity == null)
                            throw new InvalidOperationException($"DebitNote not found for RefPurchInvSubId: {sub.RefPurchInvSubId}");

                        if (entity.DbBalQty < sub.Qty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot revert because purchase Invoce balance ({entity.DbBalQty}) is less than required quantity ({sub.Qty})."
                            );
                        }
                    }
                    if (RefSubSubId > 0)
                    {
                        var entity = await _unitOfWork.SubConInvSubs.GetAsync(sub.RefSubConInvSubId.Value);
                        if (entity == null)
                            throw new InvalidOperationException($"Labour Invoice not found for RefLabSubId: {sub.RefSubConInvSubId}");

                        if (entity.DbBalQty < sub.Qty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot revert because Subcon Invoice balance ({entity.DbBalQty}) is less than required quantity ({sub.Qty})."
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
        public async Task<List<DebitNoteSub>> GetCreditSubDetailsByCrIdAsync(int DbId)
        {
            try
            {
                var subs = await _unitOfWork.DebitNoteSubs
                                 .GetQueryable()
                                 .Where(s => s.DbId == DbId)
                                 .OrderBy(s => s.SlNo)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DebitNote Sub items for DbId: {DbId}");
                throw new InvalidOperationException("Failed to retrieve DebitNote Sub Invoice sub-items. Please try again.");
            }
        }


        public async Task UpdatedCancelStatusAndAddOrRevertQty(DebitNoteVM debitNoteVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingInvoice = await _unitOfWork.DebitNotes.GetAsync(debitNoteVM.DbId.Value);
                if (existingInvoice == null)
                    throw new InvalidOperationException("DebitNote  not found.");

                var subs = await _unitOfWork.DebitNoteSubs
                    .GetQueryable()
                    .Where(s => s.DbId == debitNoteVM.DbId)
                    .ToListAsync();

                if (!debitNoteVM.Cancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateCreditNoteBalanceBeforeRevertAsync(sub);
                    }
                }

                existingInvoice.Cancel = debitNoteVM.Cancel;
                existingInvoice.CancelReason = debitNoteVM.CancelReason;
                await _unitOfWork.DebitNotes.UpdateAsync(existingInvoice);
                await _unitOfWork.SaveAsync();

                int RefpurInvSubId = 0, RefsubSubId = 0;


                foreach (var sub in subs)
                {
                    int.TryParse(sub.RefPurchInvSubId.ToString(), out RefpurInvSubId);
                    int.TryParse(sub.RefSubConInvSubId.ToString(), out RefsubSubId);
                    

                    if (existingInvoice.Cancel)
                    {
                        if (RefpurInvSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.RefPurchInvSubId.Value, sub.Qty, 0, $"DebitNote Cancelled - {existingInvoice.DebitNo}", "Purchase");
                        }

                        if (RefsubSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.RefSubConInvSubId.Value, sub.Qty, 0, $"DebitNote  Cancelled - {existingInvoice.DebitNo}", "SubContract");

                        }

                    }
                    else
                    {
                        if (RefpurInvSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.RefPurchInvSubId.Value, 0, sub.Qty, $"DebitNote  Reverted - {existingInvoice.DebitNo}", "Purchase");
                        }

                        if (RefsubSubId > 0)
                        {
                            await AdjustInvoiceSubBalanceAsync(sub.RefSubConInvSubId.Value, 0, sub.Qty, $"DebitNote  Reverted - {existingInvoice.DebitNo}", "SubContract");

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

