using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Text;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseGRN_Service;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseSCNService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseGRNStatusViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.PurchaseGRN_Service
{
    public class PurchaseGRNService : IPurchaseGRNService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        private readonly IStockManagerService _stockManagerService;

        IPurchaseSCNService _scnService;

        public PurchaseGRNService(
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
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;
            _scnService = scnService;
        }
        //Companydetails
        public async Task<Companydetails> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();
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

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
           => await _commonService.GetItemByItemIdAsync(itemId);

        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
           => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        // 🔹 Sores
        public async Task<List<Store>> GetAllAddStoresAsync()
            => (await _commonService.GetAllAddStoresAsync()).ToList();

        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
            => await _commonService.GetAllActiveStoresAsync();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
           => await _commonService.GetMappedStoreForFormAsync(formName);

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        //GRN Operations


        public async Task<bool> IsGRNTransactionsMatchedAsync(int grnId, PurchaseGRNVM PurchGRNVMs, int screenCode)
        {
            try
            {
                // ============================
                // CONDITION 1: Check GRN Transactions Exist
                // ============================
                var grnSubIds = await _unitOfWork.PurchaseGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNId == grnId)
                    .Select(x => x.GRNSubId)
                    .ToListAsync();

                bool hasTransactions = false;

                
                bool stockUsed = await _unitOfWork.StockAdds
                    .GetQueryable()
                    .AnyAsync(sa =>
                        grnSubIds.Contains(sa.SubItemRefID) && sa.ScreenCode == screenCode &&
                        sa.BalQty < sa.AddQty
                    );

                bool qtyMismatch = false;

                var list = PurchGRNVMs?.PurchaseGRNSubVMs;

                if (list != null && list.Count > 0)
                {
                    decimal totalQty = 0, totalBalQty = 0;

                    foreach (var item in list)
                    {
                        totalQty += item.Qty.GetValueOrDefault();
                        totalBalQty += item.BalQty.GetValueOrDefault();
                    }

                    qtyMismatch = totalQty != totalBalQty;
                }

                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for GrnId: {grnId}");
                throw new InvalidOperationException("Failed to verify GRN transactions.");
            }
        }


        public async Task<string> GetGRNnNumberAsync(string suffix)
        {
            try
            {
                var lastGrn = await _unitOfWork.PurchaseGRNs
                            .GetQueryable()
                            .Where(q => q.Suffix == suffix)
                            .OrderByDescending(q =>Convert.ToInt32(q.GRNNo))
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
                await _logs.LogDeveloperError(ex, $"Error generating Purchase GRN number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Purchase number.");
            }
        }

        public async Task<(List<PurchaseGRNVM> grns, int totalCount)> GetPagedGRNAsync(int pageNumber, int pageSize, string search)
        {

            var query = _unitOfWork.PurchaseGRNs
                        .GetQueryable()
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(q => q.Vendor)
                        .Include(q => q.PurchaseGRNSubs).ThenInclude(p => p.PurchPoSub).ThenInclude(s => s.PurchPo)
                        .Include(q => q.Store);

            // ✅ Get total count (fast, SQL COUNT(*))
            int totalCount = await query.CountAsync();

            // ✅ Fetch only required records with pagination
            var entities = await query
                .OrderByDescending(i => i.GRNId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ Map in-memory (fast, avoids huge SQL)
            var data = _mapper.Map<List<PurchaseGRNVM>>(entities);

            return (data, totalCount);
        }

        public async Task<IEnumerable<PurchaseGRNVM>> GetAllPurchaseGRNDetailsAsync()
        {
            try
            {
                // Fetch PurchaseGRN with related entities(Vendor, Store, and Sub - table)
                var entities = await _unitOfWork.PurchaseGRNs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.Vendor)
                    .Include(q => q.Store)
                    .Include(q => q.PurchaseGRNSubs)
                    .ToListAsync();

                if (entities == null || entities.Count == 0)
                    return Enumerable.Empty<PurchaseGRNVM>();

                // Map to ViewModel
                return _mapper.Map<IEnumerable<PurchaseGRNVM>>(entities);


            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllPurchaseGRNDetailsAsync in purchaseGRNList");
                return Enumerable.Empty<PurchaseGRNVM>();
            }
        }

        public async Task<PurchaseGRNVM?> GetPurchaseGRNByIdAsync(int GRNId)
        {
            try
            {
                var query = _unitOfWork.PurchaseGRNs.GetQueryable()
                    .AsNoTracking()
                    .Include(q => q.PurchaseGRNSubs)
                        .ThenInclude(s => s.Item)
                            .ThenInclude(i => i.Category)
                    .Include(q => q.PurchaseGRNSubs)
                        .ThenInclude(s => s.PurchPoSub)
                            .ThenInclude(p => p.PurchPo)
                    .Include(q => q.Vendor)
                    .Include(q => q.Store)
                    .AsSplitQuery();

                var entity = await query.FirstOrDefaultAsync(q => q.GRNId == GRNId);

                return _mapper.Map<PurchaseGRNVM>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetPurchaseGRNByIdAsync({GRNId})");
                return null;
            }
        }


        //public async Task<int> GetPendingPoCountAsync(int VendorCode)
        //{
        //    return await _unitOfWork.PurchPos
        //                 .GetQueryable()
        //                 .Where(p =>
        //                     p.VendorCode == VendorCode &&
        //                     p.PoTally == false &&
        //                     (p.PoCancl == false || p.PoCancl == null) &&
        //                     p.Authorized == true &&
        //                     p.PurchORSubCon == true &&
        //                     p.IsOpenPO == false && p.PoShortClose==false &&
        //                     p.PurchPoSubs.Any(ps =>
        //                         (ps.ItemCancel == false || ps.ItemCancel == null) &&
        //                         ps.BalQty > 0
        //                     )
        //                 )
        //                 .CountAsync();

        //}

        public async Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId)
        {
            try
            {
                return await _unitOfWork.PurchPoSubs.GetQueryable()
                    .Where(e => e.PoSubId == poSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for MfgPoSubId: {poSubId}");
                throw new InvalidOperationException("Failed to retrieve PO balance quantity.");
            }
        }

        public async Task ValidateBeforeRevertAsync(int grnSubId)
        {
            try
            {
                var sub = await _unitOfWork.PurchaseGRNSubs.GetAsync(grnSubId);

                if (sub == null)
                    throw new InvalidOperationException("Purchase GRN Item not found.");

                if (sub.RefPoSubId > 0)
                    await ValidatePurchasePoBalanceBeforeRevertAsync(sub);
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

        public async Task<PurchaseGRNSubVM?> GetGRNSubItemDetailByGRNSubIdAsync(int GRNSubId)
        {
            try
            {
                return await _unitOfWork.PurchaseGRNSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(q => q.GRNSubId == GRNSubId)
                    .Select(q => new PurchaseGRNSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty,
                        ExtraQty = q.ExtraQty,
                        ExtraBalQty = q.ExtraBalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Dc sub item detail for DcSubId: {GRNSubId}");
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }

        public async Task<List<PurchaseGRNSub>> GetGRNSubByGRNIdAsync(int grnId)
        {
            try
            {
                var subs = await _unitOfWork.PurchaseGRNSubs
                                .GetQueryable()
                                .Where(s => s.GRNId == grnId)
                                .OrderBy(s => s.SlNo)
                                .AsNoTracking()
                                .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Purchase GRN items for GRNId: {grnId}");
                throw new InvalidOperationException("Failed to retrieve Purchase GRN sub-items. Please try again.");
            }
        }


        public async Task UpdatedCancelStatusAndAddOrRevertQty(PurchaseGRNVM purchaseGRNVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingGrn = await _unitOfWork.PurchaseGRNs.GetAsync(purchaseGRNVM.GRNId);
                if (existingGrn == null)
                    throw new InvalidOperationException("Purchase GRN not found.");

                var subs = await _unitOfWork.PurchaseGRNSubs
                    .GetQueryable()
                    .Where(s => s.GRNId == purchaseGRNVM.GRNId)
                    .ToListAsync();

                if (!purchaseGRNVM.GRNCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidatePurchasePoBalanceBeforeRevertAsync(sub);
                    }
                }

                existingGrn.GRNCancel = purchaseGRNVM.GRNCancel;
                existingGrn.CancelReason = purchaseGRNVM.CancelReason;
                existingGrn.CancelDate = purchaseGRNVM.CancelDate;
                existingGrn.CancelBy = purchaseGRNVM.CancelBy;

                await _unitOfWork.PurchaseGRNs.UpdateAsync(existingGrn);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingGrn.GRNCancel)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.IsOpenPo)
                        {
                            await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, sub.Qty, 0, $"Purchase GRN Cancelled - {existingGrn.GRNNo}{existingGrn.Suffix}");
                        }

                        await DeleteStockAddAsync(sub.GRNSubId, screenCode);
                    }
                    else
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.IsOpenPo)
                        {
                            await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, 0, sub.Qty, $"Purchase GRN Reverted - {existingGrn.GRNNo}{existingGrn.Suffix}");
                        }

                        decimal StockAddQty = sub.Qty + sub.ExtraQty;
                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, purchaseGRNVM.AddStoreId.Value, sub.Qty, sub.UnitPrice,
                            sub.BatchNo, screenCode, sub.GRNSubId, purchaseGRNVM.GRNNo, purchaseGRNVM.GRNDate, sub.Remark);

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

        public async Task UpdateItemCancelAsync(PurchaseGRNSubVM subItem)
        {
            if (subItem == null)
                throw new ArgumentNullException(nameof(subItem), "Subitem cannot be null.");

            if (subItem.GRNSubId == 0)
                throw new ArgumentException("Cannot update unsaved subitem.");

            try
            {
                var entity = await _unitOfWork.PurchaseGRNSubs
                    .FirstOrDefaultAsync(x => x.GRNSubId == subItem.GRNSubId);

                if (entity == null)
                    throw new KeyNotFoundException($"Subitem with DcSubId {subItem.GRNSubId} not found.");

                entity.ItemCancel = subItem.ItemCancel;
                entity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.PurchaseGRNSubs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAsync for ItemCode {subItem.ItemCode}");
                throw;
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

                    rate = await (from qs in _unitOfWork.PurchPoSubs.GetQueryable()
                                  join q in _unitOfWork.PurchPos.GetQueryable() on qs.PoId equals q.PoId
                                  where qs.ItemId == itemId && q.VendorCode == VendorCode
                                  orderby q.PoId descending
                                  select qs.UnitPrice)
                                    .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.PurchPoSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.PoSubId descending
                                      select qs.UnitPrice)
                                        .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId && isub.VendorId == VendorCode
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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for CustId: {VendorCode}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetPoDetailsByVendorCode(int VendorCode)
        {
            try
            {
                var poQuery = _unitOfWork.PurchPoSubs.GetQueryable();

                var result = await (from p in _unitOfWork.PurchPos.GetQueryable()
                                    join ps in poQuery on p.PoId equals ps.PoId
                                    where p.VendorCode == VendorCode
                                          && !p.PoTally
                                          && !p.PoCancl
                                          && !p.PoShortClose
                                          && !ps.ItemCancel
                                          && p.Authorized
                                          && p.PurchORSubCon == true
                                          &&((!p.IsOpenPO && ps.BalQty> 0) || (p.IsOpenPO))

                                    select new
                                    {

                                        ps.PoSubId,
                                        p.PoId,
                                        p.IsOpenPO,
                                        p.PONo,
                                        p.Suffix,
                                        p.PODate,

                                        ps.ItemId,
                                        ps.Item.ItemCode,
                                        ps.Item.ItemName,
                                        ps.Item.MeasureUnit,
                                        ps.Item.HSNCode,
                                        CategoryName = ps.Item.Category != null ? ps.Item.Category.CategoryName : string.Empty,

                                        ps.Qty,
                                        ps.BalQty,
                                        ps.UnitPrice,
                                        ps.DueDate,

                                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : string.Empty,

                                        p.MainRemark,

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

                                        ps.LineGross,
                                        ps.LineDiscountPercent,
                                        ps.LineDiscountAmount,
                                        ps.LineBasicAmount,
                                        p.Vendor,

                                        TotalQty = poQuery.Where(x => x.PoId == p.PoId).Sum(x => x.Qty)
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
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["CategoryName"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["PoDuedate"] = r.DueDate,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Remark"] = r.MainRemark ?? string.Empty,

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

                    ["LineGross"] = r.LineGross,
                    ["SubDiscPercent"] = r.LineDiscountPercent,
                    ["SubDiscAmt"] = r.LineDiscountAmount,
                    ["LineBasicAmt"] = r.LineBasicAmount,
                    ["Vendor"] = r.Vendor.VendorName,

                    ["TotalQty"] = r.TotalQty
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PO details for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.");
            }
        }

        public async Task<bool> IsOpenPoAsync(int poSubId)
        {
            return await _unitOfWork.PurchPoSubs
                .GetQueryable()
                .AnyAsync(x => x.PoSubId == poSubId && x.PurchPo.IsOpenPO);
        }


        public async Task<PurchaseGRNVM> UpsertGRNAsync(PurchaseGRNVM purchgrnVM, int screenCode)
        {
            if (purchgrnVM == null)
                throw new ArgumentNullException(nameof(purchgrnVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                PurchaseGRN entity;

                if (purchgrnVM.GRNId == 0)
                {
                    entity = _mapper.Map<PurchaseGRN>(purchgrnVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.PurchaseGRNs.GetLastGRNNoAsync(entity.Suffix);
                    entity.GRNNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.NoOfItems = purchgrnVM.PurchaseGRNSubVMs.Count();
                    entity.PurchaseGRNSubs = purchgrnVM.PurchaseGRNSubVMs.Select(s => _mapper.Map<PurchaseGRNSub>(s)).ToList();

                    await _unitOfWork.PurchaseGRNs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.PurchaseGRNSubs)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.IsOpenPo)
                        {
                            await AdjustPoSubBalanceAsync(sub.RefPoSubId, 0, sub.Qty, "Purchase GRN Creation");
                        }
                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, entity.AddStoreId.GetValueOrDefault(), (sub.Qty + sub.ExtraQty), sub.UnitPrice,
                                                                         sub.BatchNo, screenCode, sub.GRNSubId, entity.GRNNo, entity.GRNDate, null);
                    }
                    if (entity.IsWithoutInspection)
                    {
                        await CreateSCNFromGRNAsync(entity);
                    }


                    changes.AppendLine("Purchase GRN Created.");
                }
                else
                {
                    entity = await _unitOfWork.PurchaseGRNs.GetQueryable()
                        .Include(q => q.PurchaseGRNSubs)
                        .FirstOrDefaultAsync(q => q.GRNId == purchgrnVM.GRNId)
                        ?? throw new InvalidOperationException("Purchase GRN found.");

                    var parentChanges = GetPropertyChanges(entity, purchgrnVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(purchgrnVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.NoOfItems = entity.PurchaseGRNSubs.Count();
                    await HandleChildUpdatesAsync(entity, purchgrnVM.PurchaseGRNSubVMs, changes, screenCode);

                    changes.AppendLine("Purchase GRN Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateGRNTallyStatusAsync(purchgrnVM.GRNId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, purchgrnVM.GRNId == 0 ? "Purchase GRN Created" : "Purchase GRN  Updated");

                var savedEntity = await _unitOfWork.PurchaseGRNs.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.PurchaseGRNSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                    .Include(q => q.Vendor)
                    .Include(q => q.Store)
                    .FirstOrDefaultAsync(q => q.GRNId == entity.GRNId);

                return _mapper.Map<PurchaseGRNVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Purchase GRN: {purchgrnVM.GRNNo}");
                throw new InvalidOperationException("Failed to save Purchase GRN. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(PurchaseGRN existingGRN, List<PurchaseGRNSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingGRN.PurchaseGRNSubs.Select(s => s.GRNSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.GRNSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingGRN.PurchaseGRNSubs.Where(s => !incomingSubIds.Contains(s.GRNSubId)).ToList())
            {
                await DeleteStockAddAsync(sub.GRNSubId, screenCode);

                changes.AppendLine($"Child Deleted - GRNSubId: {sub.GRNSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.PurchaseGRNSubs.DeleteAsync(sub.GRNSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.IsOpenPo)
                {
                    await AdjustPoSubBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "GRN Deletion");
                }

            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                decimal.TryParse((subVM.Qty.Value + subVM.ExtraQty).ToString(), out decimal qty);

                if (subVM.GRNSubId == 0)
                {
                    var newSub = _mapper.Map<PurchaseGRNSub>(subVM);
                    newSub.GRNId = existingGRN.GRNId;
                    await _unitOfWork.PurchaseGRNSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    if (subVM.RefPoSubId.GetValueOrDefault() > 0 && !subVM.IsOpenPo)
                    {
                        await AdjustPoSubBalanceAsync(subVM.RefPoSubId, 0, subVM.Qty ?? 0, "GRN Creation");
                    }

                    await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingGRN.AddStoreId.Value, qty,
                           subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, newSub.GRNSubId, existingGRN.GRNNo, existingGRN.GRNDate, null);
                }
                else
                {
                    var existingSub = existingGRN.PurchaseGRNSubs.FirstOrDefault(s => s.GRNSubId == subVM.GRNSubId);
                    if (existingSub != null)
                    {

                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingGRN.AddStoreId.Value, qty,
                           subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.GRNSubId, existingGRN.GRNNo, existingGRN.GRNDate, null);

                        if (subVM.RefPoSubId > 0 && !subVM.IsOpenPo)
                            await AdjustPoSubBalanceAsync(subVM.RefPoSubId, existingSub.Qty, subVM.Qty ?? 0, "GRN Update");

                        var subChanges = GetPropertyChanges(existingSub, subVM);

                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        public async Task UpdateGRNTallyStatusAsync(int grnId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.PurchaseGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNId == grnId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)(x.BalQty + x.ExtraBalQty)) ?? 0;

                var purchaseGRN = await _unitOfWork.PurchaseGRNs.GetAsync(grnId);

                if (purchaseGRN == null)
                    return;

                if (purchaseGRN.ShortClose || purchaseGRN.GRNCancel)
                    return;

                purchaseGRN.GRNTally = (totalBalQty == 0);

                await _unitOfWork.PurchaseGRNs.UpdateAsync(purchaseGRN);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateGRNTallyStatusAsync] Error updating GRN:- {grnId}");
                throw new InvalidOperationException("Failed to update Purchase GRN Tally status. Please contact support.");
            }
        }

        private async Task AdjustPoSubBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refPoSubId.HasValue || refPoSubId == 0) return;

                var poSub = await _unitOfWork.PurchPoSubs.GetAsync(refPoSubId.Value);
                if (poSub == null) return;

                if (oldQty > 0)
                    poSub.BalQty += oldQty;

                if (newQty > poSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed PO BalQty.");

                if (newQty > 0)
                    poSub.BalQty -= newQty;

                await _unitOfWork.PurchPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(e => e.PoId == poSub.PoId && !e.ItemCancel)
                    .SumAsync(e => e.BalQty);

                var po = await _unitOfWork.PurchPos.GetAsync(poSub.PoId);
                if (po != null)
                {
                    po.PoTally = (totalBalQty == 0);
                    await _unitOfWork.PurchPos.UpdateAsync(po);
                    await _unitOfWork.SaveAsync();
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

        // Get property changes for logging
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
        // Logging
        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            try
            {
                if (changes.Length == 0) return;

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase GRN",
                    action: action,
                    additionalInfo: changes.ToString()
                );

            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to LogChangesAsync in Purchase GRN");
            }
        }

        public async Task<bool> IsDuplicateDcNoAsync(int vendorCode, string dcNo, string suffix)
        {
            try
            {
                return await _unitOfWork.PurchaseGRNs
                .GetQueryable()
                .AnyAsync(x => x.VendorCode == vendorCode && x.RefDcNo == dcNo && x.Suffix == suffix);
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to check duplicate in PurchaseGRN Dc No '{dcNo}'");
                return false;
            }
        }

        public async Task<bool> IsDuplicateInvNoAsync(int vendorCode, string InvNo, string suffix)
        {
            try
            {
                // Return false early if user didn't provide invoice number
                if (string.IsNullOrWhiteSpace(InvNo))
                    return false;

                return await _unitOfWork.PurchaseGRNs
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.VendorCode == vendorCode &&
                        x.Suffix == suffix &&
                        !string.IsNullOrWhiteSpace(x.RefInvNo) &&   // ensure it's not empty or null
                        x.RefInvNo.Trim().ToLower() == InvNo.Trim().ToLower());

            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to check duplicate in PurchaseGRNInv No '{InvNo}'");
                return false;
            }
        }

        private async Task DeleteStockAddAsync(int GRNSubId, int screenCode)
        {
            var addIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == GRNSubId && s.ScreenCode == screenCode)
                .Select(s => s.AddId)
                .ToListAsync();

            foreach (var addId in addIds)
            {
                if (addId > 0)
                    await _stockManagerService.DeleteStockAddAsync(addId);
            }
        }

        public async Task<bool> DeletePurchaseGRNByIdAsync(int GRNId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var grn = await _unitOfWork.PurchaseGRNs
                    .GetQueryable()
                    .Include(e => e.PurchaseGRNSubs)
                    .FirstOrDefaultAsync(e => e.GRNId == GRNId);

                if (grn == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in grn.PurchaseGRNSubs)
                {
                    if (sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.IsOpenPo)
                    {
                        await AdjustPoSubBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "GRN Deletion");
                    }
                    await DeleteStockAddAsync(sub.GRNSubId, screenCode);

                }

                await _unitOfWork.PurchaseGRNs.DeleteAsync(grn);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase GRN",
                    action: $"Deleted GRN: {grn.GRNNo}",
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

        public async Task<(bool CanDelete, string Message)> ToCheckStockQtyIssued(int GRNId, int screenCode, string refNo)
        {
            try
            {
                var GRNSubIds = await _unitOfWork.PurchaseGRNSubs
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
                    return (false, "Cannot delete Purchase GRN. Some sub-items have already been transacted/issued.");

                return (true, "Purchase GRN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ToCheck_Stock_Qty_Issued for GRNId: {GRNId}");
                throw new Exception("Error checking PurchaseGRN delete eligibility", ex);
            }
        }

        public async Task<(bool CanItemCancel, string Message)> CanGRNItemCancelCheckAsync(PurchaseGRNSubVM subItemVM)
        {
            try
            {
                bool hasPo = await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefGRNSubId.HasValue && qs.RefGRNSubId == subItemVM.GRNSubId && !qs.ItemCancel);

                if (hasPo)
                    return (false, "Cannot cancel this Item as a SCN transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanGRNItemCancelCheckAsync for GRNSubId: {subItemVM.GRNSubId}");
                throw new Exception("Error checking Purchase GRN Item cancel eligibility", ex);
            }
        }

        public async Task UpsertPurchGRNShortCloseAsync(PurchaseGRNVM purchaseGRNVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingGRN = await _unitOfWork.PurchaseGRNs.GetAsync(purchaseGRNVM.GRNId);
                if (existingGRN == null)
                    throw new InvalidOperationException("Purchase GRN not found.");

                existingGRN.ShortClose = purchaseGRNVM.ShortClose;

                await _unitOfWork.PurchaseGRNs.UpdateAsync(existingGRN);
                await _unitOfWork.SaveAsync();

                await UpdateGRNTallyStatusAsync(purchaseGRNVM.GRNId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertPurchGRNShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertPurchGRNShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }



        public async Task UpdateItemCancelAndAddorRevertAsync(PurchaseGRNSubVM subItem, PurchaseGRNVM purchaseGRNVM, int screenCode)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.PurchaseGRNSubs.GetQueryable().Where
                                (x => x.GRNSubId == subItem.GRNSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with GRNSubId {subItem.GRNSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidatePurchasePoBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;

                await _unitOfWork.PurchaseGRNSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    if (subEntity.RefPoSubId.GetValueOrDefault() > 0 && !subEntity.IsOpenPo)
                    {
                        await AdjustPoSubBalanceAsync(subEntity.RefPoSubId, subEntity.Qty, 0, $"Purchase GRN Item Cancel - {subItem.ItemCode}");
                    }

                    await DeleteStockAddAsync(subItem.GRNSubId, screenCode);
                }
                else
                {
                    if (subEntity.RefPoSubId.GetValueOrDefault() > 0 && !subEntity.IsOpenPo)
                    {
                        await AdjustPoSubBalanceAsync(subEntity.RefPoSubId, 0, subEntity.Qty, $"Purchase GRN Revert Cancel - {subItem.ItemCode}");
                    }

                    decimal stockAddQty = subEntity.Qty + subEntity.ExtraQty;
                    await _stockManagerService.AddOrUpdateStockAsync(subItem.ItemId.Value, purchaseGRNVM.AddStoreId.Value, stockAddQty,
                            subItem.UnitPrice.GetValueOrDefault(), subItem.BatchNo, screenCode, subItem.GRNSubId, purchaseGRNVM.GRNNo, purchaseGRNVM.GRNDate, subItem.Remark);
                }

                await UpdateGRNTallyStatusAsync(subItem.GRNId);

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


        public async Task ValidatePurchasePoBalanceBeforeRevertAsync(PurchaseGRNSub sub)
        {
            if (sub.RefPoSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.PurchPoSubs.GetAsync(sub.RefPoSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"enquiry not found for RefEnqSubId: {sub.RefPoSubId}");

            if (entity.BalQty < sub.Qty)
            {
                throw new InvalidOperationException($"Cannot revert because PO balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
            }
        }

        public async Task<bool> HasAnyItemOrPurchaseGRNCancelAsync(int GRNId)
        {
            try
            {
                var isGRNCancelled = await _unitOfWork.PurchaseGRNs
                    .AnyAsync(q => q.GRNId == GRNId && q.GRNCancel == true);

                var isItemCancelled = await _unitOfWork.PurchaseGRNSubs
                    .AnyAsync(i => i.GRNId == GRNId && i.ItemCancel == true);

                return isGRNCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrQuoteCancelAsync for GRNId: {GRNId}");
                throw;
            }
        }

        public async Task DeleteAndResequenceAsync(PurchaseGRNSubVM subitem, PurchaseGRNVM grnVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.GRNSubId > 0)
                {
                    var entity = await _unitOfWork.PurchaseGRNSubs.GetAsync(subitem.GRNSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.RefPoSubId.GetValueOrDefault() > 0 && !entity.IsOpenPo)
                    {
                        await AdjustPoSubBalanceAsync(subitem.RefPoSubId, entity.Qty, 0, "GRN Deletion");
                    }

                    await DeleteStockAddAsync(subitem.GRNSubId, screenCode);

                    await _unitOfWork.PurchaseGRNSubs.DeleteAsync(entity);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Purchse GRN",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"GRN No: {grnVM?.GRNNo}"
                    );
                }
                else
                {
                    grnVM.PurchaseGRNSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.PurchaseGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNId == grnVM.GRNId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                // Update Quotation Tally Status
                await UpdateGRNTallyStatusAsync(grnVM.GRNId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> GetPurchaseGRNByIdIsCancelAsync(int GRNId)
        {
            return await _unitOfWork.PurchaseGRNs
                .GetQueryable()
                .Where(e => e.GRNId == GRNId)
                .AnyAsync(e =>
                    e.GRNCancel == true ||
                    e.PurchaseGRNSubs.Any(s => s.ItemCancel == true)
                );
        }

        //Auot Generated Purchase SCN
        private async Task CreateSCNFromGRNAsync(PurchaseGRN grn)
        {
            var changes = new StringBuilder();

            try
            {
                var suffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);
                var screenCode = await _scnService
                    .GetScreenCodeByScreenNameAsync("Purchase SCN");

                var mapStore = await GetMappedStoreForFormAsync("SCNQualityForm");
                var addStore = mapStore.StoreId > 0 ? mapStore.StoreId : 1;

                var scn = new PurchaseSCN
                {
                    Suffix = suffix,
                    SCNNo = await _scnService.GetSCNNumberAsync(suffix),
                    SCNDate = DateTime.Now,
                    VendorCode = grn.VendorCode,
                    StoreIssId = grn.AddStoreId,
                    AddStoreId = addStore,
                    CreatedBy = grn.CreatedBy,
                    CreatedDate = DateTime.Now,
                    MainRemarks = "Auto-generated due to No Inspection",
                    PurchaseSCNSubs = new List<PurchaseSCNSub>()
                };

                await SetSCNAuthorizationStatusAsync(scn, grn.CreatedBy);

                // ---------- SCN Subs ----------
                foreach (var s in grn.PurchaseGRNSubs)
                {
                    var item = await _unitOfWork.ItemRepositories.GetAsync(s.ItemId)
                        ?? throw new InvalidOperationException($"Item not found : {s.ItemId}");

                    var baseQty = s.Qty + s.ExtraQty;

                    var unitPrice = (item.UnitConvert > 0 && item.AltRate > 0)
                        ? item.AltRate
                        : s.UnitPrice;

                    decimal qtyConvert = (item.UnitConvert > 0) ? (baseQty * item.UnitConvert).Value : baseQty;
                   
                    scn.PurchaseSCNSubs.Add(new PurchaseSCNSub
                    {
                        SlNo = s.SlNo,
                        ItemId = s.ItemId,
                        RefGRNSubId = s.GRNSubId,

                        BalQty = baseQty,
                        AcceptQty = baseQty,
                        QtyConvert = qtyConvert,
                        HeatNo = s.HeatNo,
                        BatchNo = s.BatchNo,
                        UnitPrice = unitPrice,
                        CostId = s.CostId
                    });
                }

                await _unitOfWork.PurchaseSCNs.CreateAsync(scn);
                await _unitOfWork.SaveAsync();

                changes.AppendLine($"SCN Created : {scn.SCNNo}");

                foreach (var sub in scn.PurchaseSCNSubs)
                {

                    await AdjustGRNSubBalanceAsync(sub.RefGRNSubId, 0, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), "Purchase SCN Creation");

                    await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId,grn.AddStoreId.Value,sub.AcceptQty,sub.UnitPrice,sub.BatchNo, screenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, allowMultipleIssue: true);
                                               
                    await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId,addStore,sub.QtyConvert,sub.UnitPrice,sub.BatchNo,screenCode,sub.SCNSubId,scn.SCNNo,scn.SCNDate, sub.Remark, allowMultipleAdd: true);

                }

                await LogChangesAsync(changes, "Purchase SCN Created");
            }
            catch (Exception ex)
            {
                changes.AppendLine($"ERROR : {ex.Message}");
                await LogChangesAsync(changes, "Purchase SCN Creation Failed");
                throw;
            }
        }

        private async Task SetSCNAuthorizationStatusAsync(PurchaseSCN entity, string currentUser)
        {
            var SCNAuthorityExists = await _unitOfWork.UserAuthorities
                .AnyAsync(x => x.IsPurchSCN == true);

            if (!SCNAuthorityExists)
            {
                entity.Authorized = true;
                entity.ApprovedBy = currentUser;
                entity.ApprovalDate = DateTime.Now;
            }
            else
            {

                entity.Authorized = false;
                entity.ApprovedBy = null;
                entity.ApprovalDate = null;

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

        private async Task AdjustGRNSubBalanceAsync(int? refGRNSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refGRNSubId.HasValue || refGRNSubId == 0)
                    return;

                var grnSub = await _unitOfWork.PurchaseGRNSubs.GetAsync(refGRNSubId.Value);
                if (grnSub == null) return;

                decimal BalQty = grnSub.BalQty;
                decimal ExtraBalQty = grnSub.ExtraBalQty;

                decimal Qty = grnSub.Qty;
                decimal ExtraQty = grnSub.ExtraQty;

                if (oldQty > 0)
                {
                    decimal restoreQty = oldQty;

                    decimal missingExtra = ExtraQty - ExtraBalQty;
                    if (missingExtra > 0)
                    {
                        decimal restoreToExtra = Math.Min(restoreQty, missingExtra);
                        ExtraBalQty += restoreToExtra;
                        restoreQty -= restoreToExtra;
                    }

                    if (restoreQty > 0)
                        BalQty += restoreQty;
                }

                if (newQty > 0)
                {
                    decimal consumeQty = newQty;

                    if (BalQty >= consumeQty)
                    {
                        BalQty -= consumeQty;
                        consumeQty = 0;
                    }
                    else
                    {
                        consumeQty -= BalQty;
                        BalQty = 0;
                    }

                    if (consumeQty > 0)
                    {
                        ExtraBalQty -= consumeQty;
                        if (ExtraBalQty < 0)
                            throw new InvalidOperationException($"{context}: Qty exceeds GRN total available quantity.");
                    }
                }

                grnSub.BalQty = BalQty;
                grnSub.ExtraBalQty = ExtraBalQty;

                await _unitOfWork.PurchaseGRNSubs.UpdateAsync(grnSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.PurchaseGRNSubs
                    .GetQueryable()
                    .Where(e => e.GRNId == grnSub.GRNId)
                    .SumAsync(e => e.BalQty + e.ExtraBalQty);

                var grn = await _unitOfWork.PurchaseGRNs.GetAsync(grnSub.GRNId);
                if (grn != null)
                {
                    grn.GRNTally = (totalBalQty == 0);
                    await _unitOfWork.PurchaseGRNs.UpdateAsync(grn);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustGRNSubBalanceAsync] Error in {context}");
                throw new InvalidOperationException("Failed to adjust GRN balance. Please contact support.");
            }
        }



        public async Task<(bool CanDelete, string Message)> CanDeleteGRNAsync(int grnId, int screenCode)
        {
            try
            {
                var grn = await _unitOfWork.PurchaseGRNs
                                .GetQueryable()
                                .Include(e => e.PurchaseGRNSubs)
                                .Where(e => e.GRNId == grnId).FirstOrDefaultAsync();

                if (grn == null)
                    return (true, "Purchase GRN can be safely deleted.");

                var grnSubIds = grn.PurchaseGRNSubs
                    .Select(es => es.GRNSubId)
                    .ToList();

                bool hasSCN = await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefGRNSubId.HasValue &&
                        grnSubIds.Contains(qs.RefGRNSubId.Value));

                if (hasSCN)
                    return (false, "Cannot delete this Purchase GRN as a SCN transaction exists.");


                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                                .Where(sa =>
                                grnSubIds.Contains(sa.SubItemRefID) &&
                                sa.ScreenCode == screenCode &&
                                sa.BalQty < sa.AddQty)
                                .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Purchase GRN. Some sub-items have already been transacted/issued.");


                if (grn.GRNCancel || grn.ShortClose)
                    return (false, "Cannot delete this Purchase GRN as it is Cancelled or Short Closed.");


                if (grn.PurchaseGRNSubs.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Purchase GRN as one or more GRN items are cancelled.");


                return (true, "Purchase GRN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteGRNAsync for GRNId: {grnId}");
                throw new Exception("Error checking PurchaseGRN delete eligibility", ex);
            }
        }



        public async Task<(List<PurchaseGRNVM> grnVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.PurchaseGRNs
                         .GetQueryable()
                         .AsSplitQuery()
                         .Include(e => e.Vendor)
                        .Include(e => e.PurchaseGRNSubs)
                             .ThenInclude(s => s.Item)
                         .Include(e => e.PurchaseGRNSubs)
                             .ThenInclude(s => s.CostCenter)
                         .AsQueryable();

            string? status = null;
            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = PurchaseGRNFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.GRNId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<PurchaseGRNVM>>(list);

            return (vmList, total);
        }


        public static class PurchaseGRNFilterBuilder
        {
            public static IQueryable<PurchaseGRN> ApplyFilter(
                IQueryable<PurchaseGRN> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "GRNNo":
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
                                (string.IsNullOrEmpty(part1) || x.GRNNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "Vendor":
                        return query.Where(x => x.Vendor.VendorName.Contains(val));

                    case "ItemName":
                        return query.Where(x => x.PurchaseGRNSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "ItemCode":
                        return query.Where(x => x.PurchaseGRNSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "PONo":
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
                                x.PurchaseGRNSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.PurchPoSub.PurchPo.PONo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.PurchPoSub.PurchPo.Suffix.Contains(part2))
                                ));
                        }

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "ModifiedBy":
                        return query.Where(x => x.ModifiedBy.Contains(val));

                    case "FromDate":
                        return query.Where(x => x.GRNDateNow >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.GRNDateNow <= DateTime.Parse(value.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val);

                }

                return query;
            }

            private static IQueryable<PurchaseGRN> ApplyStatusFilter(
                IQueryable<PurchaseGRN> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.GRNTally == true),
                    "Short Closed" => query.Where(x => x.ShortClose == true),
                    "Cancelled" => query.Where(x => x.GRNCancel == true),
                    "Pending" => query.Where(x => x.GRNTally == false && x.GRNCancel == false && x.ShortClose == false),
                    _ => query
                };
            }
        }

        public async Task<List<PurchaseGRNStatusVM>> GetPurchaseGRNsStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<PurchaseGRNStatusVM>("Sp_GetPurchaseGRNsPendingList", status);
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


        //public async Task<(List<PurchaseGRNVM> Grns, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        //{
        //    var query = _unitOfWork.PurchaseGRNs.GetQueryable()
        //        .AsNoTracking()
        //        .Include(j => j.PurchaseGRNSubs).ThenInclude(s => s.Item)
        //        .Include(j => j.Vendor)
        //        .Include(j => j.Store)
        //        .AsQueryable();

        //    if (filters != null)
        //    {
        //        foreach (var f in filters)
        //        {
        //            query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
        //        }
        //    }

        //    var total = await query.CountAsync();

        //    var list = await query
        //        .OrderByDescending(x => x.GRNId)
        //        .Skip((pageNumber - 1) * pageSize)
        //        .Take(pageSize)
        //        .ToListAsync();

        //    // Use AutoMapper
        //    var vmList = _mapper.Map<List<PurchaseGRNVM>>(list);

        //    return (vmList, total);
        //}


        //public static class DynamicWhereBuilder
        //{
        //    public static IQueryable<PurchaseGRN> ApplyFilter(IQueryable<PurchaseGRN> query, string field, object value)
        //    {
        //        if (value == null) return query;

        //        switch (field)
        //        {
        //            case "GRNNo":
        //                return query.Where(x => x.GRNNo.Contains(value.ToString()));

        //            case "Vendor":
        //                return query.Where(x => x.Vendor.VendorName.Contains(value.ToString()));

        //            case "ItemCode":
        //                return query.Where(x => x.PurchaseGRNSubs.Any(s => s.Item.ItemCode.Contains(value.ToString())));

        //            case "ItemName":
        //                return query.Where(x => x.PurchaseGRNSubs.Any(s => s.Item.ItemName.Contains(value.ToString())));

        //            case "CreatedBy":
        //                return query.Where(x => x.CreatedBy.Contains(value.ToString()));

        //            case "FromDate":
        //                return query.Where(x => x.CreatedDate >= DateTime.Parse(value.ToString()));

        //            case "ToDate":
        //                return query.Where(x => x.CreatedDate <= DateTime.Parse(value.ToString()));
        //        }

        //        return query;
        //    }
        //}

    }
}
