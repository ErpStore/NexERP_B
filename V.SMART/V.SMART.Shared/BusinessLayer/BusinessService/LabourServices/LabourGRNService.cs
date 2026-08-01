using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.InkML;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.LabourServices
{
    public class LabourGRNService : ILabourGRNService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManagerService;
        private readonly IExcelTemplateService _excelTemplateService;
    

        public LabourGRNService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            IStockManagerService stockManagerService,
            ILoggingService logs,
            IMapper mapper,
            IExcelTemplateService excelTemplateService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;
            _excelTemplateService = excelTemplateService;
           
        }

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        // 🔹 Customer
        public async Task<CustomerVM?> GetCustomerByIdAsync(int CustId)
           => await _commonService.GetCustomerByIdAsync(CustId);

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }
        // 🔹 ContactPerson
        public async Task<List<ContactPerson>> GetContactPersonsCustomerAsync(int CustId)
            => await _commonService.GetContactPersonsAsync(CustId);

        //Mapped Store
        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
            => await _commonService.GetAllActiveStoresAsync();

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
           => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
           => await _commonService.GetItemByItemIdAsync(itemId);

        //Companydetails
        public async Task<Companydetails> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();

        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        public async Task<bool> IsPOWiseLabDcOutEnabledAsync()
          => await _commonService.GetScreenPermissionsAsync("Labour GRN", "PO Wise Labour DC Outgoing");


        //--------------------------------GRN List operations--------------------------------
        public async Task<(List<LabourGRNVM> Grns, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var baseQuery = _unitOfWork.LabourGRNs.GetQueryable().AsNoTracking().AsQueryable();

                if (filters != null)
                {
                    foreach (var f in filters)
                    {
                        baseQuery = DynamicWhereBuilder.ApplyFilter(
                            baseQuery, f.Key, f.Value);
                    }
                }

                var total = await baseQuery.CountAsync();

                if (total == 0)
                    return (new List<LabourGRNVM>(), 0);

                var grnIds = await baseQuery
                    .OrderByDescending(x => x.GRNId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.GRNId)
                    .ToListAsync();

                var grnList = await _unitOfWork.LabourGRNs.GetQueryable()
                    .AsNoTracking()
                    .Where(x => grnIds.Contains(x.GRNId))
                    .Include(j => j.LabourGRNSubs)
                        .ThenInclude(s => s.Item)
                    .Include(j => j.LabourGRNSubs)
                        .ThenInclude(s => s.MfgPoSub)
                            .ThenInclude(p => p.MfgPo)
                    .Include(j => j.Customer)
                    .Include(j => j.Store)
                    .OrderByDescending(x => x.GRNId)
                    .ToListAsync();


                var vmList = _mapper.Map<List<LabourGRNVM>>(grnList);

                return (vmList, total);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex, "Error in SearchWithDynamicFilterAsync");

                return (new List<LabourGRNVM>(), 0);
            }
        }



        public async Task<(bool CanDelete, string Message)> ToCheckStockQtyIssued(int GRNId, int screenCode, string refNo)
        {
            try
            {
                var GRNSubIds = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(s => s.GRNId == GRNId)
                    .Select(s => s.GRNSubId)
                    .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa =>
                        GRNSubIds.Contains(sa.SubItemRefID) &&
                        sa.ScreenCode == screenCode && sa.RefNo == refNo &&
                        sa.BalQty < sa.AddQty)
                    .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Labour GRN. Some sub-items have already been transacted/issued.");

                return (true, "Labour GRN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ToCheckStockQtyIssued for GRNId: {GRNId}");
                throw new Exception("Error checking LabourGRN delete eligibility", ex);
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteLabourGRNAsync(int GRNId, int screenCode)
        {
            try
            {
                var grn = await _unitOfWork.LabourGRNs
                                .GetQueryable()
                                .Include(e => e.LabourGRNSubs)
                                .Where(e => e.GRNId == GRNId).FirstOrDefaultAsync();

                if (grn == null)
                    return (true, "Labour GRN can be safely deleted.");

                var grnSubIds = grn.LabourGRNSubs
                    .Select(es => es.GRNSubId)
                    .ToList();

                bool hasSCN = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefGRNSubId.HasValue &&
                        grnSubIds.Contains(qs.RefGRNSubId.Value));

                if (hasSCN)
                    return (false, "Cannot delete this Labour GRN as a Labour SCN exists.");

                if (grn.LabourGRNSubs.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Labour GRN as one or more GRN items are cancelled.");

                if (grn.DcCancel || grn.ShortClose)
                    return (false, "Cannot delete this Labour GRN as it is Cancelled or Short-Closed.");

                var GRNSubIds = await _unitOfWork.LabourGRNSubs
                               .GetQueryable()
                               .Where(s => s.GRNId == GRNId)
                               .Select(s => s.GRNSubId)
                               .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                                .Where(sa =>
                                    GRNSubIds.Contains(sa.SubItemRefID) &&
                                    sa.ScreenCode == screenCode &&
                                    sa.BalQty < sa.AddQty)
                                .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Labour GRN. Some sub-items have already been transacted/issued.");

                return (true, "Labour GRN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteLabourGRNAsync for GRNId: {GRNId}");
                return (false, "Unable to verify item CanDeleteLabourGRNAsync. Please try again or contact support.");
            }
        }


        public async Task<bool> DeleteLabourGRNByIdAsync(int GRNId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var grn = await _unitOfWork.LabourGRNs
                    .GetQueryable()
                    .Include(e => e.LabourGRNSubs)
                    .FirstOrDefaultAsync(e => e.GRNId == GRNId);

                if (grn == null)
                    return false;

                bool IsPoWiseDcOutEnable = await IsPOWiseLabDcOutEnabledAsync();
                var changes = new StringBuilder();

                foreach (var sub in grn.LabourGRNSubs)
                {
                    if (sub.TransType == "In")
                    {
                        await DeleteStockAddAsync(sub.GRNSubId, sub.ItemId.Value, screenCode);
                    }
                    else if (sub.TransType == "Out" && sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.isOpenPo)
                    {
                        if (!IsPoWiseDcOutEnable)
                        {
                            await AdjustPoSubBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Labour GRN Deletion");
                        }
                    }
                }

                await _unitOfWork.LabourGRNs.DeleteAsync(grn);

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Labour GRN",
                    action: $"Deleted Labour GRN: {grn.GRNNo}",
                    additionalInfo: $"GRN Id: {grn.GRNId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete GRN: {GRNId}");
                throw;
            }
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
                    var poSub = await _unitOfWork.MfgPoSubs.GetAsync(refPoSubId.Value);
                    if (poSub == null) return;

                    if (oldQty > 0)
                        poSub.BalQty += oldQty;

                    if (newQty > poSub.BalQty)
                        throw new InvalidOperationException($"{context}: Qty cannot exceed Quote BalQty.");

                    if (newQty > 0)
                        poSub.BalQty -= newQty;

                    await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                    await _unitOfWork.SaveAsync();

                    var totalBalQty = await _unitOfWork.MfgPoSubs
                        .GetQueryable()
                        .Where(e => e.PoId == poSub.PoId && !e.ItemCancel)
                        .SumAsync(e => e.BalQty);

                    var po = await _unitOfWork.MfgPos.GetAsync(poSub.PoId);

                    await _unitOfWork.MfgPos.ReloadAsync(po);

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
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust Po balance. Please contact support.");
            }
        }

        private async Task DeleteStockAddAsync(int GRNSubId, int itemId, int screenCode)
        {
            var addIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == GRNSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.AddId)
                .ToListAsync();

            foreach (var addId in addIds)
            {
                if (addId > 0)
                    await _stockManagerService.DeleteStockAddAsync(addId);
            }
        }
        //-------------------------------------------------------------------------------------------
        public async Task<string> GetLastGRNNumberAsync(string suffix)
        {
            try
            {
                var lastGrn = await _unitOfWork.LabourGRNs
                            .GetQueryable()
                            .Where(q => q.Suffix == suffix)
                            .OrderByDescending(q => q.GRNNo)
                            .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastGrn != null)
                {
                    var parts = lastGrn.GRNNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Labour GRN number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Labour number.");
            }
        }

        public async Task<int> GetPendingPoCountAsync(int CustId)
        {
            return await _unitOfWork.MfgPos
                         .GetQueryable()
                         .Where(p =>
                             p.CustId == CustId &&
                             p.PoTally == false &&
                             (p.PoCancl == false || p.PoCancl == null) &&
                             p.MfgORLab == false &&
                             p.IsOpenPO == false &&
                             p.MfgPoSubs.Any(ps =>
                                 (ps.ItemCancel == false || ps.ItemCancel == null)
                             )
                         )
                         .CountAsync();

        }

        public async Task<bool> GetLabourGRNByIdIsCancelAsync(int GRNId)
        {
            return await _unitOfWork.LabourGRNs
                .GetQueryable()
                .Where(e => e.GRNId == GRNId)
                .AnyAsync(e =>
                    e.DcCancel == true ||
                    e.LabourGRNSubs.Any(s => s.ItemCancel == true)
                );
        }

        public async Task<bool> IsLabourGRNTransactionsMatchedAsync(int GRNId, LabourGRNVM LabGrn)
        {
            try
            {
                var GrnSubIds = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNId == GRNId)
                    .Select(x => x.GRNSubId)
                    .ToListAsync();

                bool hasGrn = GrnSubIds.Any();

                bool hasTransactions = false;

                //if (hasGrn)
                //{
                //    // Check GRN references
                //    hasTransactions = await _unitOfWork.LabourSCNSubs
                //        .GetQueryable()
                //        .AnyAsync(pqs =>
                //            pqs.RefGRNSubId.HasValue &&
                //            GrnSubIds.Contains(pqs.RefGRNSubId.Value));
                //}

                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = LabGrn?.LabourGRNSubVMs?
                            .Where(x => x.TransType == "In")
                            .ToList();

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
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for GRNID: {GRNId}");
                return false;
            }
        }
        public async Task<LabourGRNVM> GetLabourGRNByIdAsync(int GRNId)
        {
            try
            {
                
                bool isSameItemAsIn = await GetIsSameItemasInAsync();

                var query = _unitOfWork.LabourGRNs.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(g => g.LabourGRNSubs)
                        .ThenInclude(s => s.Item)
                        .ThenInclude(i => i.Category)
                    .Include(g => g.LabourGRNSubs)
                        .ThenInclude(s => s.MfgPoSub)
                        .ThenInclude(m => m.MfgPo)
                    .Include(g => g.Customer)
                    .Include(g => g.Store)
                    .Where(g => g.GRNId == GRNId);

                var entity = await query.FirstOrDefaultAsync();

                if (entity == null)
                    return null;


                entity.LabourGRNSubs = entity.LabourGRNSubs
                    .OrderBy(s => s.SlNo)
                    .ToList();


                return _mapper.Map<LabourGRNVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetLabourGRNByIdAsync({GRNId})");
                return null;
            }
        }

        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int CustId)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.LabourGRNSubs.GetQueryable()
                                  join q in _unitOfWork.LabourGRNs.GetQueryable() on qs.GRNId equals q.GRNId
                                  where qs.ItemId == itemId && q.CustId == CustId
                                  orderby q.GRNId descending
                                  select qs.UnitPrice.GetValueOrDefault())
                                    .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.LabourGRNSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.GRNSubId descending
                                      select qs.UnitPrice.GetValueOrDefault())
                                        .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId && isub.VendorId == CustId
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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for CustId: {CustId}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }

        public async Task<bool> IsOpenPoAsync(int poSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs
                .GetQueryable()
                .Where(s => s.PoSubId == poSubId)
                .Join(_unitOfWork.MfgPos.GetQueryable(),
                      sub => sub.PoId,
                      po => po.PoId,
                      (sub, po) => po.IsOpenPO)
                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }

        }

        public async Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(e => e.PoSubId == poSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for MfgPoSubId: {poSubId} in labour grn");
                throw new InvalidOperationException("Failed to retrieve PO balance quantity. in labour grn");
            }
        }

        public async Task<LabourGRNSubVM?> GetDcSubItemDetailByDcSubIdAsync(int dcSubId)
        {
            try
            {
                return await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(q => q.GRNSubId == dcSubId)
                    .Select(q => new LabourGRNSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty,
                        ProdCompBalQty=q.ProdCompBalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Dc sub item detail for DcSubId: {dcSubId}");
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(
            LabourGRNSubVM subItem,
            int screenCode,
            string GRNNo,
            int AddStoreId,
            DateTime GRNDate)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                bool PoWiseLabdcOut = await IsPOWiseLabDcOutEnabledAsync();

                var entity = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.GRNSubId == subItem.GRNSubId);

                if (entity == null)
                    throw new KeyNotFoundException($"Subitem with GRNSubId {subItem.GRNSubId} not found.");

                if (!PoWiseLabdcOut)
                {
                    if (!subItem.ItemCancel && subItem.TransType == "Out")
                    {
                        await ValidateMfgPoBalanceBeforeRevertAsync(entity);
                    }
                }

                entity.ItemCancel = subItem.ItemCancel;
                entity.CancelItemReason = subItem.CancelItemReason;

                await _unitOfWork.LabourGRNSubs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();

                var groupItems = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNId == entity.GRNId && x.GroupId == entity.GroupId)
                    .ToListAsync();

                var outItems = groupItems.Where(x => x.TransType == "Out").ToList();
                var inItems = groupItems.Where(x => x.TransType == "In").ToList();

                bool isCancel = subItem.ItemCancel;

                // ================= CANCEL LOGIC =================
                if (isCancel)
                {
                    // ------ Cancel OUT item ------
                    if (entity.TransType == "Out")
                    {
                        if (entity.RefPoSubId.GetValueOrDefault() > 0 && !entity.isOpenPo)
                        {
                            if (!PoWiseLabdcOut)
                            {
                                await AdjustPoSubBalanceAsync(entity.RefPoSubId, entity.Qty, 0, $"Labour GRN Cancel");
                            }
                        }

                        int activeOutCount = outItems.Count(x => !x.ItemCancel && x.GRNSubId != entity.GRNSubId);

                        if (activeOutCount == 0)
                        {
                            foreach (var inItem in inItems.Where(x => !x.ItemCancel))
                            {
                                inItem.ItemCancel = true;
                                await DeleteStockAddAsync(inItem.GRNSubId, inItem.ItemId.Value, screenCode);
                                await _unitOfWork.LabourGRNSubs.UpdateAsync(inItem);
                            }
                        }
                    }

                    // ------ Cancel IN item ------
                    else if (entity.TransType == "In")
                    {
                        await DeleteStockAddAsync(entity.GRNSubId, entity.ItemId.Value, screenCode);

                        int activeInCount = inItems.Count(x => !x.ItemCancel && x.GRNSubId != entity.GRNSubId);

                        // If this was the last active IN → cancel all OUT items
                        if (activeInCount == 0)
                        {
                            foreach (var outItem in outItems.Where(x => !x.ItemCancel))
                            {
                                outItem.ItemCancel = true;

                                if (outItem.RefPoSubId.GetValueOrDefault() > 0 && !outItem.isOpenPo)
                                {
                                    if (!PoWiseLabdcOut)
                                    {
                                        await AdjustPoSubBalanceAsync(outItem.RefPoSubId, outItem.Qty, 0, $"Labour GRN Cancel - related OUT");
                                    }
                                }

                                await _unitOfWork.LabourGRNSubs.UpdateAsync(outItem);
                            }
                        }
                    }
                }

                // ================= REVERT CANCEL LOGIC =================
                else
                {
                    // ------ Revert OUT item ------
                    if (entity.TransType == "Out")
                    {
                        if (entity.RefPoSubId.GetValueOrDefault() > 0 && !entity.isOpenPo)
                        {
                            await AdjustPoSubBalanceAsync(entity.RefPoSubId, 0, entity.Qty, $"Labour GRN Revert Cancel");
                        }
                    }

                    // ------ Revert IN item ------
                    else if (entity.TransType == "In")
                    {
                        await _stockManagerService.AddOrUpdateStockAsync(entity.ItemId.Value, AddStoreId, entity.Qty, entity.UnitPrice.GetValueOrDefault(),
                            entity.BatchNo, screenCode, entity.GRNSubId, GRNNo, GRNDate, entity.RowRemarks);
                    }
                }

                await _unitOfWork.SaveAsync();

                // Update GRN tally/status
                await UpdateGRNTallyStatusAsync(entity.GRNId);

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

        private async Task ValidateMfgPoBalanceBeforeRevertAsync(LabourGRNSub sub)
        {
            if (sub.RefPoSubId.GetValueOrDefault() <= 0)
                return;

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

        public async Task UpdateGRNTallyStatusAsync(int grnId)
        {
            try
            {


                var labourGRN = await _unitOfWork.LabourGRNs.GetAsync(grnId);
                if (labourGRN == null)
                    return;

                decimal totalBalQty = 0;
                if (labourGRN.WithoutPoGRN)
                {
                    totalBalQty = await _unitOfWork.LabourGRNSubs
                                  .GetQueryable()
                                  .Where(x => x.GRNId == grnId && !x.ItemCancel && x.TransType == "Out")
                                  .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                }
                else
                {
                    totalBalQty = await _unitOfWork.LabourGRNSubs
                                 .GetQueryable()
                                 .Where(x => x.GRNId == grnId && !x.ItemCancel && x.TransType == "In")
                                 .SumAsync(x => (decimal?)x.BalQty) ?? 0;
                }



                labourGRN.DcTally = (totalBalQty == 0);

                await _unitOfWork.LabourGRNs.UpdateAsync(labourGRN);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateGRNTallyStatusAsync] Error updating GRN:- {grnId}");
                throw new InvalidOperationException("Failed to update labour GRN Tally status. Please contact support.");
            }
        }

        public async Task<List<LabourGRNSubVM>> GetGRNSubByGRNIdAsync(int grnId)
        {
            try
            {
                var subs = await _unitOfWork.LabourGRNSubs
                                .GetQueryable()
                                .Where(s => s.GRNId == grnId)
                                .OrderBy(s => s.SlNo)
                                .ProjectTo<LabourGRNSubVM>(_mapper.ConfigurationProvider)
                                .AsNoTracking()
                                .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching labour GRN items for GRNId: {grnId}");
                throw new InvalidOperationException("Failed to retrieve labour GRN sub-items. Please try again.");
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(LabourGRNVM grnVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingGRN = await _unitOfWork.LabourGRNs.GetAsync(grnVM.GRNId);
                if (existingGRN == null)
                    throw new InvalidOperationException("Labour GRN not found.");


                var subs = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(s => s.GRNId == grnVM.GRNId)
                    .ToListAsync();

                bool PoWiseLabdcOut = await IsPOWiseLabDcOutEnabledAsync();

                if (!grnVM.DcCancel && !PoWiseLabdcOut)
                {
                    foreach (var sub in subs)
                    {
                        if (sub.TransType == "Out")
                        {
                            await ValidateMfgPoBalanceBeforeRevertAsync(sub);
                        }
                    }
                }

                existingGRN.DcCancel = grnVM.DcCancel;
                existingGRN.CancelReason = grnVM.CancelReason;
                existingGRN.CancelDate = grnVM.CancelDate;
                existingGRN.CancelBy = grnVM.CancelBy;


                await _unitOfWork.LabourGRNs.UpdateAsync(existingGRN);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingGRN.DcCancel)
                    {
                        if (!PoWiseLabdcOut)
                        {
                            if (sub.RefPoSubId.GetValueOrDefault() > 0 && sub.TransType == "Out" && !sub.isOpenPo)
                            {
                                await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, sub.Qty, 0, $"Labour GRN Cancelled - {existingGRN.GRNNo}");
                            }
                        }

                        if (sub.TransType == "In")
                        {
                            await DeleteStockAddAsync(sub.GRNSubId, sub.ItemId.Value, screenCode);
                        }
                    }
                    else
                    {
                        if (!PoWiseLabdcOut)
                        {
                            if (sub.RefPoSubId.GetValueOrDefault() > 0 && sub.TransType == "Out" && !sub.isOpenPo)
                            {
                                await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, 0, sub.Qty, $"Labour GRN Reverted - {existingGRN.GRNNo}");
                            }
                        }

                        if (sub.TransType == "In")
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, existingGRN.AddStoreId, sub.Qty, sub.UnitPrice.Value,
                                sub.BatchNo, screenCode, sub.GRNSubId, existingGRN.GRNNo, existingGRN.GRNDate, sub.RowRemarks);
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

        public async Task DeleteAndResequenceAsync(LabourGRNSubVM subitem, LabourGRNVM dcVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                bool PoWiseLabdcOut = await IsPOWiseLabDcOutEnabledAsync();
                if (subitem.GRNSubId > 0)
                {
                    var entity = await _unitOfWork.LabourGRNSubs.GetAsync(subitem.GRNSubId);

                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    // OUT ITEM DELETE
                    if (entity.TransType == "Out")
                    {
                        var outItems = await _unitOfWork.LabourGRNSubs.GetQueryable()
                            .Where(x => x.GRNId == entity.GRNId &&
                                        x.GroupId == entity.GroupId &&
                                        x.TransType == "Out")
                            .ToListAsync();

                        var inItems = await _unitOfWork.LabourGRNSubs.GetQueryable()
                            .Where(x => x.GRNId == entity.GRNId &&
                                        x.GroupId == entity.GroupId &&
                                        x.TransType == "In")
                            .ToListAsync();
                        if (!PoWiseLabdcOut)
                        {

                            if (entity.TransType == "Out" && entity.RefPoSubId.GetValueOrDefault() > 0 && !entity.isOpenPo)
                            {
                                await AdjustPoSubBalanceAsync(entity.RefPoSubId.GetValueOrDefault(), entity.Qty, 0, "Labour GRN Deletion");
                            }
                        }

                        // Delete selected OUT item
                        await _unitOfWork.LabourGRNSubs.DeleteAsync(entity.GRNSubId);
                        await _unitOfWork.SaveAsync();

                        // If this was the last OUT item → delete related IN items
                        if (outItems.Count == 1)
                        {
                            foreach (var inItem in inItems)
                            {
                                await DeleteStockAddAsync(inItem.GRNSubId, inItem.ItemId.Value, screenCode);
                                await _unitOfWork.LabourGRNSubs.DeleteAsync(inItem.GRNSubId);
                            }
                        }
                    }


                    // IN ITEM DELETE
                    else if (entity.TransType == "In")
                    {
                        var inItems = await _unitOfWork.LabourGRNSubs.GetQueryable()
                            .Where(x => x.GRNId == entity.GRNId &&
                                        x.GroupId == entity.GroupId &&
                                        x.TransType == "In")
                            .ToListAsync();

                        var outItems = await _unitOfWork.LabourGRNSubs.GetQueryable()
                            .Where(x => x.GRNId == entity.GRNId &&
                                        x.GroupId == entity.GroupId &&
                                        x.TransType == "Out")
                            .ToListAsync();

                        await DeleteStockAddAsync(entity.GRNSubId, entity.ItemId.Value, screenCode);
                        await _unitOfWork.LabourGRNSubs.DeleteAsync(entity.GRNSubId);

                        // If this was the last IN item → delete related OUT items
                        if (inItems.Count == 1)
                        {
                            foreach (var outItem in outItems)
                            {
                                if (outItem.TransType == "Out" && outItem.RefPoSubId.GetValueOrDefault() > 0 && !outItem.isOpenPo)
                                {
                                    if (!PoWiseLabdcOut)
                                    {
                                        await AdjustPoSubBalanceAsync(outItem.RefPoSubId.GetValueOrDefault(), outItem.Qty, 0, "Labour GRN Deletion");
                                    }
                                }
                                await _unitOfWork.LabourGRNSubs.DeleteAsync(outItem.GRNSubId);
                            }
                        }
                    }

                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Labour GRN",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"GRN No: {dcVM?.GRNNo}");
                }
                else
                {
                    dcVM.LabourGRNSubVMs.Remove(subitem);
                    return;
                }

                // Resequence SlNo
                var remaining = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNId == dcVM.GRNId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                int groupId = 1;
                bool inStarted = false;

                foreach (var item in remaining)
                {
                    // Update SlNo
                    item.SlNo = slno++;

                    // Update GroupId logic
                    if (item.TransType == "Out")
                    {
                        if (inStarted)
                        {
                            groupId++;
                            inStarted = false;
                        }

                        item.GroupId = groupId;
                    }
                    else if (item.TransType == "In")
                    {
                        item.GroupId = groupId;
                        inStarted = true;
                    }
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
        public async Task<LabourGRNSubVM?> GetGRNSubItemDetailByGRNSubIdAsync(int GRNSubId)
        {
            try
            {
                return await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(q => q.GRNSubId == GRNSubId)
                    .Select(q => new LabourGRNSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Dc sub item detail for DcSubId: {GRNSubId}");
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }

        public async Task<bool> IsDuplicateDcNoAsync(int CustId, string dcNo, string suffix)
        {
            try
            {
                return await _unitOfWork.LabourGRNs
                .GetQueryable()
                .AnyAsync(x => x.CustId == CustId && x.RefDcNo == dcNo && x.Suffix == suffix);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to check duplicate in Labour GRN Dc No '{dcNo}'");
                return false;
            }
        }

        public async Task<LabourGRNVM> UpsertGRNAsync(LabourGRNVM labourgrnVM, int screenCode)
        {
            if (labourgrnVM == null)
                throw new ArgumentNullException(nameof(labourgrnVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            bool PoWiseLabdcOut = await IsPOWiseLabDcOutEnabledAsync();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                LabourGRN entity;
                if (labourgrnVM.GRNId == 0)
                {
                    entity = _mapper.Map<LabourGRN>(labourgrnVM);

                    // Get last number with locking from repository
                    var NextNumber = await _unitOfWork.LabourGRNs.GetLastGRNNoAsync(entity.Suffix);

                    entity.GRNNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.NoOfItems = labourgrnVM.LabourGRNSubVMs.Count();
                    entity.LabourGRNSubs = labourgrnVM.LabourGRNSubVMs.Select(s => _mapper.Map<LabourGRNSub>(s)).ToList();

                    await _unitOfWork.LabourGRNs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.LabourGRNSubs)
                    {
                        if (!PoWiseLabdcOut)
                        {
                            if (sub.TransType == "Out" && sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.isOpenPo)
                            {
                                await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, 0, sub.Qty, "Labour GRN Creation");
                            }
                        }

                        if (sub.TransType == "In")
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, entity.AddStoreId, sub.Qty,
                                  sub.UnitPrice ?? 0, sub.BatchNo, screenCode, sub.GRNSubId, entity.GRNNo, entity.GRNDate, null, null);
                        }
                    }

                    changes.AppendLine("Labour GRN Created.");
                }
                else
                {
                    entity = await _unitOfWork.LabourGRNs.GetQueryable()
                        .Include(q => q.LabourGRNSubs)
                        .FirstOrDefaultAsync(q => q.GRNId == labourgrnVM.GRNId)
                        ?? throw new InvalidOperationException("labour GRN found.");

                    var parentChanges = GetPropertyChanges(entity, labourgrnVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(labourgrnVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.NoOfItems = entity.LabourGRNSubs.Count();

                    await HandleChildUpdatesAsync(entity, labourgrnVM.LabourGRNSubVMs, changes, screenCode);

                    changes.AppendLine("Labour GRN Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateGRNTallyStatusAsync(labourgrnVM.GRNId);


                //string? Transty = null;
                //int? RefGRnsubid = 0;
                //if (labourgrnVM.GRNId > 0)
                //{
                //    entity = await _unitOfWork.LabourGRNs.GetQueryable()
                //    .Include(q => q.LabourGRNSubs)
                //    .FirstOrDefaultAsync(q => q.GRNId == labourgrnVM.GRNId)
                //    ?? throw new InvalidOperationException("Labour GRN not found.");

                //    // Order children in memory
                //    entity.LabourGRNSubs = entity.LabourGRNSubs
                //        .OrderBy(s => s.SlNo)
                //        .ToList();
                //}

                //int groupId = 0;

                //bool inSeenInCurrentGroup = false;

                //foreach (var sub in entity.LabourGRNSubs)
                //{
                //    if (sub.TransType == "Out")
                //    {
                //        if (groupId == 0 || inSeenInCurrentGroup)
                //        {
                //            groupId++;

                //            inSeenInCurrentGroup = false;

                //        }

                //        sub.GroupId = groupId;
                //        await _unitOfWork.LabourGRNSubs.UpdateAsync(sub);
                //        continue;
                //    }

                //    if (sub.TransType == "In")
                //    {
                //        if (groupId == 0)
                //            continue;

                //        inSeenInCurrentGroup = true;
                //        sub.GroupId = groupId;

                //        await _unitOfWork.LabourGRNSubs.UpdateAsync(sub);
                //    }
                //}

                await transaction.CommitAsync();

                await LogChangesAsync(changes, labourgrnVM.GRNId == 0 ? "Labour GRN Created" : "Labour GRN  Updated");

                var savedEntity = await _unitOfWork.LabourGRNs.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.LabourGRNSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                    .Include(q => q.Customer)
                    .Include(q => q.Store)
                    .FirstOrDefaultAsync(q => q.GRNId == entity.GRNId);

                return _mapper.Map<LabourGRNVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Labour GRN: {labourgrnVM.GRNNo}");
                throw new InvalidOperationException("Failed to save Labour GRN. Please try again.");
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
                    screen: "Labour GRN",
                    action: action,
                    additionalInfo: changes.ToString()
                );

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to LogChangesAsync in Purchase GRN");
            }
        }

        private async Task HandleChildUpdatesAsync(LabourGRN existingGRN, List<LabourGRNSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingGRN.LabourGRNSubs.Select(s => s.GRNSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.GRNSubId).ToHashSet();
            bool PoWiseLabdcOut = await IsPOWiseLabDcOutEnabledAsync();
            // DELETE removed children
            foreach (var sub in existingGRN.LabourGRNSubs.Where(s => !incomingSubIds.Contains(s.GRNSubId)).ToList())
            {
                if (!PoWiseLabdcOut)
                {

                    if (sub.TransType == "Out" && sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.isOpenPo)
                    {
                        await AdjustPoSubBalanceAsync(sub.RefPoSubId.GetValueOrDefault(), sub.Qty, 0, "Labour GRN Deletion");
                    }
                }

                if (sub.TransType == "In")
                {
                    await DeleteStockAddAsync(sub.GRNSubId, sub.ItemId.Value, screenCode);
                }

                changes.AppendLine($"Child Deleted - GRNSubId: {sub.GRNSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.LabourGRNSubs.DeleteAsync(sub.GRNSubId);
                await _unitOfWork.SaveAsync();

            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                decimal.TryParse((subVM.Qty).ToString(), out decimal qty);
                if (subVM.GRNSubId == 0)
                {
                    var newSub = _mapper.Map<LabourGRNSub>(subVM);
                    newSub.GRNId = existingGRN.GRNId;
                    await _unitOfWork.LabourGRNSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    if (!PoWiseLabdcOut)
                    {
                        if (subVM.TransType == "Out" && subVM.RefPoSubId.GetValueOrDefault() > 0 && !subVM.isOpenPo)
                        {
                            await AdjustPoSubBalanceAsync(subVM.RefPoSubId.GetValueOrDefault(), 0, subVM.Qty.GetValueOrDefault(), "Labour GRN Creation");
                        }
                    }

                    if (subVM.TransType == "In")
                    {
                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingGRN.AddStoreId, qty,
                           subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, newSub.GRNSubId, existingGRN.GRNNo, existingGRN.GRNDate, null);
                    }

                }
                else
                {
                    var existingSub = existingGRN.LabourGRNSubs.FirstOrDefault(s => s.GRNSubId == subVM.GRNSubId);
                    if (existingSub != null)
                    {
                        if (!PoWiseLabdcOut)
                        {
                            if (subVM.TransType == "Out" && subVM.RefPoSubId.GetValueOrDefault() > 0 && !subVM.isOpenPo)
                            {
                                await AdjustPoSubBalanceAsync(subVM.RefPoSubId.GetValueOrDefault(), existingSub.Qty, subVM.Qty.GetValueOrDefault(), "Labour GRN Update");
                            }
                        }

                        if (subVM.TransType == "In")
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingGRN.AddStoreId, qty,
                           subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.GRNSubId, existingGRN.GRNNo, existingGRN.GRNDate, null);
                        }

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        public async Task<List<Dictionary<string, object>>> GetPoDetailsByVendorCode(int CustId)
        {
            try
            {
                var poQuery = _unitOfWork.MfgPoSubs.GetQueryable();

                var result = await (from p in _unitOfWork.MfgPos.GetQueryable()
                                    join ps in poQuery on p.PoId equals ps.PoId
                                    where p.CustId == CustId
                                          && !p.PoTally
                                          && !p.PoCancl
                                          && !ps.ItemCancel
                                          && p.MfgORLab == false
                                          && p.IsAuthorized == true
                                          && ((!p.IsOpenPO && ps.BalQty > 0) || (p.IsOpenPO)) && !p.ShortClose

                                    select new
                                    {
                                        ps.PoSubId,
                                        p.PoId,
                                        p.IsOpenPO,
                                        p.PONo,
                                        p.Suffix,
                                        p.PODate,
                                        ps.LineNo,

                                        ps.ItemId,
                                        ps.Item.ItemCode,
                                        ps.Item.ItemName,
                                        ps.ItemSpecification,
                                        ps.Item.MeasureUnit,
                                        ps.Item.HSNCode,

                                        CategoryName = ps.Item.Category != null
                                            ? ps.Item.Category.CategoryName
                                            : string.Empty,

                                        ps.Item.CategoryCode,

                                        ps.Qty,
                                        ps.BalQty,
                                        ps.UnitPrice,
                                        ps.DueDate,

                                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : string.Empty,

                                        p.MainRemark,

                                        // MAIN PO LEVEL
                                        p.DiscountPercent,
                                        p.DiscountAmount,
                                        p.PackingPercent,
                                        p.PackingCharges,
                                        p.InsurancePercent,
                                        p.InsuranceCharges,
                                        p.FreightCharges,
                                        p.TodayVal,
                                        p.TotalGrossAmount,
                                        p.TotalBasicAmount,

                                        // SUB LEVEL
                                        ps.LineGross,
                                        ps.LineDiscountPercent,
                                        ps.LineDiscountAmount,
                                        ps.LineBasicAmount,
                                        p.Customer,

                                        TotalQty = poQuery
                                            .Where(x => x.PoId == p.PoId)
                                            .Sum(x => x.Qty)
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["PoSubId"] = r.PoSubId,
                    ["PoId"] = r.PoId,
                    ["IsOpenPo"] = r.IsOpenPO ? "Yes" : "No",
                    ["PoNo"] = $"{r.PONo}{r.Suffix}",
                    ["PoDate"] = r.PODate,

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["ItemSpecification"] = r.ItemSpecification ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Category"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["PoDuedate"] = r.DueDate,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Remark"] = r.MainRemark ?? string.Empty,

                    // MAIN PO VALUES
                    ["MainDiscPercent"] = r.DiscountPercent,
                    ["MainDiscAmt"] = r.DiscountAmount,
                    ["PackingPercent"] = r.PackingPercent,
                    ["PackingAmt"] = r.PackingCharges,
                    ["InsurancePercent"] = r.InsurancePercent,
                    ["InsuranceAmt"] = r.InsuranceCharges,
                    ["FreightAmt"] = r.FreightCharges,
                    ["TodayVal"] = r.TodayVal,

                    ["TotalGrossAmt"] = r.TotalGrossAmount,
                    ["TotalBasicAmt"] = r.TotalBasicAmount,

                    // SUB ITEM VALUES
                    ["LineGross"] = r.LineGross,
                    ["SubDiscPercent"] = r.LineDiscountPercent,
                    ["SubDiscAmt"] = r.LineDiscountAmount,
                    ["LineBasicAmt"] = r.LineBasicAmount,

                    ["TotalQty"] = r.TotalQty,
                    ["CategoryCode"] = r.CategoryCode,
                    ["LineNo"] = r.LineNo,
                    ["Customer"] = r.Customer.CustName ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PO details for CustId: {CustId}");
                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.");
            }
        }

        public async Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId)
        {
            try
            {
                return await _unitOfWork.AssmblyDefs
                    .GetQueryable()
                    .Where(a => a.AssmblyID == assemblyId)
                    .Select(a => new AssemblyDefVM
                    {
                        ItemId = a.PartItem.ItemId,
                        PartItemCode = a.PartItem.ItemCode,
                        PartItemName = a.PartItem.ItemName,
                        UOM = a.PartItem.MeasureUnit,
                        PartSpecification = a.PartItem.Specification,
                        HSNCode = a.PartItem.HSNCode,
                        CategoryName = a.PartItem.Category.CategoryName,
                        UtilQty = a.UtilQty,
                        ReqQty = a.UtilQty,
                        IsLabourMaterial = a.IsLabourMaterial
                    })
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetAssemblyItemsAsync. AssemblyId: {assemblyId}");
                return new List<AssemblyDefVM>();
            }
        }

        public async Task<ItemVM?> GetRawMaterialItemAsync(int compItemId)
        {
            return await _unitOfWork.CompMasters
                .GetQueryable()
                .AsNoTracking()
                .Where(c => c.CompItemId == compItemId && c.RMId.HasValue)
                .OrderByDescending(c => c.IsDefaultRM)
                .Select(c => new ItemVM
                {
                    ItemId = c.RMId.Value,
                    ItemCode = c.RawMaterial.ItemCode,
                    ItemName = c.RawMaterial.ItemName,
                    MeasureUnit = c.RawMaterial.MeasureUnit,
                    Specification = c.RawMaterial.Specification,
                    CategoryName = c.RawMaterial.Category.CategoryName,
                    Weight = c.Weight
                })
                .FirstOrDefaultAsync();
        }


        public static class DynamicWhereBuilder
        {
            public static IQueryable<LabourGRN> ApplyFilter(IQueryable<LabourGRN> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString()!.Trim();

                switch (field)
                {

                    case "GRNNo":
                        {
                            string part1 = val;
                            string part2 = string.Empty;

                            int slashIndex = val.IndexOf('/');
                            if (slashIndex > -1)
                            {
                                part1 = val[..slashIndex].Trim();
                                part2 = val[(slashIndex + 1)..].Trim();
                            }

                            return query.Where(x => (string.IsNullOrEmpty(part1) || x.GRNNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2)))
                            );
                        }

                    case "RefDcNo":
                        return query.Where(x => x.RefDcNo.Contains(val.ToString()));

                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(val.ToString()));

                    case "ItemCode":
                        return query.Where(x => x.LabourGRNSubs.Any(s => s.Item.ItemCode.Contains(val.ToString())));

                    case "ItemName":
                        return query.Where(x => x.LabourGRNSubs.Any(s => s.Item.ItemName.Contains(val.ToString())));

                    case "PoNo":
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
                                x.LabourGRNSubs.Any(s =>
                                    s.MfgPoSub != null &&
                                    s.MfgPoSub.MfgPo != null &&
                                    (string.IsNullOrEmpty(part1) ||
                                        s.MfgPoSub.MfgPo.PONo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) ||
                                        (x.Suffix != null && x.Suffix.Contains(part2)))
                                )
                            );
                        }

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val.ToString()));

                    case "FromDate":
                        return query.Where(x => x.CreatedDate >= DateTime.Parse(val.ToString()));

                    case "ToDate":
                        return query.Where(x => x.CreatedDate <= DateTime.Parse(val.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val.ToString());

                }

                return query;
            }
            private static IQueryable<LabourGRN> ApplyStatusFilter(IQueryable<LabourGRN> query, string status)
            {
                try
                {
                    return status switch
                    {
                        "Completed" =>
                            query.Where(x => x.DcTally && !x.ShortClose),

                        "Pending" =>
                            query.Where(x =>
                                !x.DcTally &&
                                !x.DcCancel &&
                                !x.ShortClose),

                        "Cancelled" =>
                            query.Where(x => x.DcCancel),

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


        public async Task<(bool CanItemCancel, string Message)> CanLabourGRNItemCancelCheckAsync(LabourGRNSubVM subItem)
        {
            try
            {
                bool hasGrn = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefGRNSubId.HasValue && qs.RefGRNSubId == subItem.GRNSubId && !qs.ItemCancel);
                if (hasGrn)
                    return (false, "Cannot cancel this Item as a Labour SCN transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanLabourGRNItemCancelCheckAsync for GRNSubId: {subItem.GRNSubId}");
                return (false, "Unable to verify item cancellation status. Please try again or contact support.");
            }
        }

        public async Task UpsertlabourGRNShortCloseAsync(LabourGRNVM LabourGRnVms)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingGRN = await _unitOfWork.LabourGRNs.GetAsync(LabourGRnVms.GRNId);
                if (existingGRN == null)
                    throw new InvalidOperationException("Sales Enquiry not found.");

                existingGRN.ShortClose = LabourGRnVms.ShortClose;

                await _unitOfWork.LabourGRNs.UpdateAsync(existingGRN);
                await _unitOfWork.SaveAsync();

                await UpdateLabourGrnTallyStatusAsync(LabourGRnVms.GRNId);

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
        public async Task UpdateLabourGrnTallyStatusAsync(int GRNId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNId == GRNId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var gRN = await _unitOfWork.LabourGRNs.GetAsync(GRNId);
                if (gRN == null)
                    return;

                if (gRN.ShortClose || gRN.DcCancel)
                    return;

                gRN.DcTally = (totalBalQty == 0);

                await _unitOfWork.LabourGRNs.UpdateAsync(gRN);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateLabourGrnTallyStatusAsync] Error updating GRNID {GRNId}");
                throw new InvalidOperationException("Failed to update DC Tally status. Please contact support.");

            }
        }

        public async Task<List<LabourGRNSub>> GetlabourGRNSubDetailsByDcIdAsync(int DCID)
        {
            try
            {
                var subs = await _unitOfWork.LabourGRNSubs
                                 .GetQueryable()
                                 .Where(s => s.GRNId == DCID)
                                 .OrderBy(s => s.SlNo)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetlabourGRNSubDetailsByDcIdAsync items for DCid: {DCID}");
                return null;
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

        public async Task<bool> GetPriceLockStatusAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(x => x.Display == "Rate Control" && x.ScreenName == "Labour GRN")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching price lock status for Labour GRN");
                return false;
            }
        }

        public async Task<bool> GetIsSameItemasInAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(x => x.Display == "Restrict Out Item Same As In Item" && x.ScreenName == "Labour GRN")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching price lock status for Labour GRN");
                return false;
            }
        }

        public async Task ValidatePoBalanceBeforeRevertAsync(LabourGRNSub sub)
        {
            if (sub.RefPoSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.MfgPoSubs.GetAsync(sub.RefPoSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"Labour Po not found for RefPoSubId: {sub.RefPoSubId}");

            if (entity.BalQty < sub.Qty && !entity.ItemCancel)
            {
                throw new InvalidOperationException(
                    $"Cannot revert because Labour Po balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                );
            }
        }


        public async Task<List<LabourGRNStatusListVM>> GetLabourGRNStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<LabourGRNStatusListVM>("Sp_GetLabourGRNStatusList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> BatchNoOption()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(x => x.Display == "Batch Number" && x.ScreenName == "Labour GRN")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in EinvoiceCancelOption()");
                return false;
            }
        }

        public async Task<bool> IsDocumentUploaded(int grnId)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Labour-GRN" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == grnId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }

    }

}
