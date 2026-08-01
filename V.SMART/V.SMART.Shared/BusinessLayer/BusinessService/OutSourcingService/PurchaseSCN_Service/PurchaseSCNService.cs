using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchaseSCNService;
using V.SMART.Shared.BusinessLayer.BusinessService.InventoryService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.MaterialIssueNoteVM;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.PurchAndSubConViewModel.Purch_QuotationVM;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.Purchase_Invoice_Service.PurchaseInvoiceService;
using V.SMART.Shared.ViewModels.ReportViewModel.PurchaseSCNStatusViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.PurchaseSCN_Service
{
    public class PurchaseSCNService : IPurchaseSCNService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManager;

        

        public PurchaseSCNService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,
            IStockManagerService stockManager)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
            _stockManager = stockManager;
        }

        //Companydetails
        public async Task<Companydetails> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();
        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

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
        //Correspond
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
          => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        // 🔹 Sores
        public async Task<IEnumerable<Store>> GetAllAddStoresAsync()
            => await _commonService.GetAllAddStoresAsync();
        // 🔹 Sores
        public async Task<List<Store>> GetAllIssueStoresAsync()
            => (await _commonService.GetAllIssueStoresAsync()).ToList();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
          => await _commonService.GetMappedStoreForFormAsync(formName);

        //Stock Manager
        public async Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId)
            => await _stockManager.GetStockForItemAsync(ItemId, StoreId);

        public async Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync()
         => await _commonService.GetAllRejectionReasonAsync();

        public async Task<bool> GetRejectionSelectionEnableAsync()
            => await _commonService.GetRejectionSelectionEnableAsync();

        //------SCN Operation

        public async Task<bool> IsSCNTransactionsMatchedAsync(int ScnId, PurchaseSCNVM scnVMs)
        {
            try
            {
                var scnSubIds = await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == ScnId)
                    .Select(x => x.SCNSubId)
                    .ToListAsync();

                bool hasScn = scnSubIds.Any();

                bool hasTransactions = false;

                if (hasScn)
                {
                    hasTransactions = await _unitOfWork.PurchaseInvoiceSubs
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.RefSCNSubId.HasValue &&
                            scnSubIds.Contains(pqs.RefSCNSubId.Value));
                }

                bool qtyMismatch = false;

                var list = scnVMs?.PurchaseSCNSubVMs;
                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.AcceptQty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty ?? 0);

                    qtyMismatch = totalQty != totalBalQty;
                }

                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for ScnId: {ScnId}");
                throw new InvalidOperationException("Failed to verify SCN transactions.", ex);
            }
        }


        public async Task<(List<PurchaseSCNVM> scns, int totalCount)> GetPagedSCNAsync(int pageNumber, int pageSize, string search)
        {
            var query = _unitOfWork.PurchaseSCNs
                .GetQueryable()
                .AsNoTracking()
                .AsSplitQuery()
                .Include(q => q.Vendor)
                .Include(q => q.StoreAdd)
                .Include(q => q.StoreIssue)
                .Include(q => q.PurchaseSCNSubs);

            // ✅ Get total count (fast, SQL COUNT(*))
            int totalCount = await query.CountAsync();

            // ✅ Fetch only required records with pagination
            var entities = await query
                .OrderByDescending(i => i.SCNId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ Map in-memory (fast, avoids huge SQL)
            var data = _mapper.Map<List<PurchaseSCNVM>>(entities);

            return (data, totalCount);
        }

        public async Task<string> GetSCNNumberAsync(string suffix)
        {
            var lastGrn = await _unitOfWork.PurchaseSCNs.GetLastSCNNoAsync(suffix);
            return lastGrn;
        }

        public async Task ValidateBeforeRevertAsync(int scnSubId)
        {
            try
            {
                var sub = await _unitOfWork.PurchaseSCNSubs.GetAsync(scnSubId);

                if (sub == null)
                    throw new InvalidOperationException("Purchase SCN Item not found.");

                if (sub.RefGRNSubId > 0)
                    await ValidateGRNBalanceBeforeRevertAsync(sub);
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

        public async Task<(bool CanItemCancel, string Message)> CanSCNItemCancelCheckAsync(PurchaseSCNSubVM subItem)
        {
            try
            {
                bool hasInv = await _unitOfWork.PurchaseInvoiceSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefSCNSubId.HasValue && qs.RefSCNSubId == subItem.SCNSubId && !qs.ItemCancel);

                if (hasInv)
                    return (false, "Cannot cancel this Item as a Purchase Invoice transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanQuoteItemCancelCheckAsync for SCNSubId: {subItem.SCNSubId}");
                throw new Exception("Error checking Purchase SCN Item cancel eligibility", ex);
            }
        }


        public async Task<PurchaseSCNVM> GetPurchaseSCNByIdAsync(int SCNId)
        {
            try
            {
                var entity = await _unitOfWork.PurchaseSCNs.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.PurchaseSCNSubs)
                    .Include(q => q.PurchaseSCNSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                    .Include(q => q.PurchaseSCNSubs).ThenInclude(s => s.PurchaseGRNSub.PurchaseGRN)
                    .Include(q => q.Vendor)
                    .Include(q => q.StoreAdd)
                    .Include(q => q.StoreIssue)
                    .FirstOrDefaultAsync(q => q.SCNId == SCNId);

                var scnVM = _mapper.Map<PurchaseSCNVM?>(entity);

                var itemIds = scnVM.PurchaseSCNSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && scnVM.StoreIssId.HasValue)
                {
                    var stockDict = await _stockManager.GetStockForItemsAsync(itemIds, scnVM.StoreIssId.Value);

                    foreach (var sub in scnVM.PurchaseSCNSubVMs)
                    {
                        if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                            sub.StockQty = qty;
                        else
                            sub.StockQty = 0m;
                    }
                }

                return scnVM;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetPurchaseSCNByIdAsync({SCNId})");
                return null;
            }
        }

        public async Task<List<PurchaseSCNSub>> GetSCNSubBySCNIdAsync(int scnId)
        {
            try
            {
                var subs = await _unitOfWork.PurchaseSCNSubs
                                 .GetQueryable()
                                 .Where(s => s.SCNId == scnId)
                                 .OrderBy(s => s.SlNo)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Purchase SCN items for SCNId: {scnId}");
                throw new InvalidOperationException("Failed to retrieve Purchase SCN sub-items. Please try again.");
            }
        }

        public async Task<IEnumerable<PurchaseSCNVM>> GetAllPurchaseSCNDetailsAsync()
        {
            try
            {
                // Fetch PurchaseGRN with related entities (Vendor, Store, and Sub-table)
                var entities = await _unitOfWork.PurchaseSCNs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.Vendor)
                    .Include(q => q.StoreAdd)
                    .Include(q => q.StoreIssue)
                    .Include(q => q.PurchaseSCNSubs)
                    .ToListAsync();

                if (entities == null || entities.Count == 0)
                    return Enumerable.Empty<PurchaseSCNVM>();

                // Map to ViewModel
                return _mapper.Map<IEnumerable<PurchaseSCNVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllPurchaseSCNDetailsAsync in purchaseSCNList");
                return Enumerable.Empty<PurchaseSCNVM>();
            }
        }

        public async Task<int> GetPendingGRNCountAsync(int VendorCode)
        {
            try
            {
                var count = await _unitOfWork.PurchaseGRNs
                             .GetQueryable()
                             .AsNoTracking()
                             .Where(h =>
                                 h.VendorCode == VendorCode &&
                                 !h.GRNTally &&
                                 !h.GRNCancel &&
                                 h.PurchaseGRNSubs.Any(s =>
                                     (s.BalQty > 0 || s.ExtraBalQty > 0) &&
                                     !s.ItemCancel
                                 )
                             )
                             .CountAsync();

                return count;
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, "[GetPendingGRNCountAsync] Business/validation error");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "[GetPendingGRNCountAsync] Unexpected system error");
                return 0;
            }
        }


        public async Task<decimal> GetGRNItemBalQtyFromGRNSubId(int GRNSubId)
        {
            try
            {
                var sum = await _unitOfWork.PurchaseGRNSubs.GetQueryable()
                                .Where(e => e.GRNSubId == GRNSubId)
                                .Select(e => (e.BalQty) + (e.ExtraBalQty))
                                .SumAsync();
                return sum;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for PurchaseGRNSubId: {GRNSubId}");
                throw new InvalidOperationException("Failed to retrieve GRN balance quantity.");
            }
        }

        public async Task<PurchaseSCNSubVM?> GetSCNSubItemDetailBySCNSubIdAsync(int SCNSubId)
        {
            try
            {
                return await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(q => q.SCNSubId == SCNSubId)
                    .Select(q => new PurchaseSCNSubVM
                    {
                        BalQty = q.BalQty,
                        AcceptQty = q.AcceptQty,
                        RejectQty = q.RejectQty,
                        ReworkQty = q.ReworkQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Dc sub item detail for SCNSubId: {SCNSubId}");
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
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

                    rate = await (from qs in _unitOfWork.PurchaseGRNSubs.GetQueryable()
                                  join q in _unitOfWork.PurchaseGRNs.GetQueryable() on qs.GRNId equals q.GRNId
                                  where qs.ItemId == itemId && q.VendorCode == VendorCode
                                  orderby q.GRNId descending
                                  select qs.UnitPrice).FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.PurchaseGRNSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.GRNSubId descending
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

        public async Task<List<Dictionary<string, object>>> GetGRNDetailsByVendorCode(int vendorCode, int storeId)
        {
            try
            {
                var grnData = await _unitOfWork.PurchaseGRNs.GetQueryable()
                    .Where(g =>
                        g.VendorCode == vendorCode &&
                        !g.ShortClose &&
                        !g.GRNTally &&
                        !g.GRNCancel &&
                        g.AddStoreId == storeId && !g.ShortClose)
                    .SelectMany(g => g.PurchaseGRNSubs
                        .Where(gs => !gs.ItemCancel && (gs.BalQty > 0 || gs.ExtraBalQty > 0))
                        .Select(gs => new
                        {
                            gs.GRNSubId,
                            gs.RefPoSubId,
                            g.GRNNo,
                            g.Suffix,
                            g.GRNDate,

                            gs.ItemId,
                            ItemCode = gs.Item != null ? gs.Item.ItemCode : "",
                            ItemName = gs.Item != null ? gs.Item.ItemName : "",
                            Specification = gs.Item != null ? gs.Item.Specification : "",
                            UOM = gs.Item != null ? gs.Item.MeasureUnit : "",
                            HSN = gs.Item != null ? gs.Item.HSNCode : "",
                            CategoryName = gs.Item != null && gs.Item.Category != null ? gs.Item.Category.CategoryName : "",

                            UnitConvert = gs.Item != null ? gs.Item.UnitConvert : 0,
                            AltRate = gs.Item != null ? gs.Item.AltRate : 0,

                            gs.Qty,
                            gs.BalQty,
                            gs.UnitPrice,

                            g.RefDcNo,
                            g.RefDcDate,

                            g.RefInvNo,
                            g.RefInvDate,

                            CostCenterId = gs.CostId == 0 ? (int?)null : gs.CostId,
                            ProjectNo = gs.CostCenter != null ? gs.CostCenter.ProjectNo : "",

                            Remarks = g.MainRemarks ?? "",

                            gs.ExtraQty,
                            gs.ExtraBalQty,

                            gs.BatchNo,
                            gs.HeatNo,

                            StoreName = g.Store != null ? g.Store.StoreName : ""
                        }))
                    .AsNoTracking()
                    .ToListAsync();

                var itemIds = grnData.Select(x => x.ItemId).Distinct().ToList();
                var stockDict = await _stockManager.GetStockForItemsAsync(itemIds, storeId);

                var result = grnData.Select(r =>
                {
                    stockDict.TryGetValue(r.ItemId, out decimal stockQty);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["PoSubId"] = r.RefPoSubId,
                        ["GRNSubId"] = r.GRNSubId,
                        ["GRNNo"] = $"{r.GRNNo}{r.Suffix}",
                        ["GRNDate"] = r.GRNDate,

                        ["ItemId"] = r.ItemId,
                        ["ItemCode"] = r.ItemCode,
                        ["ItemName"] = r.ItemName,
                        ["Specification"] = r.Specification,
                        ["UOM"] = r.UOM,
                        ["HSNCode"] = r.HSN,
                        ["Category"] = r.CategoryName,

                        ["Qty"] = r.Qty + r.ExtraQty,
                        ["BalQty"] = r.BalQty + r.ExtraBalQty,
                        ["UnitPrice"] = r.UnitPrice,

                        ["UnitConvert"] = r.UnitConvert,
                        ["AltRate"] = r.AltRate,

                        ["StockQty"] = stockQty,

                        ["RefDcNo"] = r.RefDcNo ?? "",
                        ["RefDcDate"] = r.RefDcDate,

                        ["RefInvNo"] = r.RefInvNo ?? "",
                        ["RefInvDate"] = r.RefInvDate,

                        ["CostId"] = r.CostCenterId,
                        ["ProjectNo"] = r.ProjectNo,

                        ["MainRemarks"] = r.Remarks,
                        ["StoreName"] = r.StoreName,

                        ["BatchNo"] = r.BatchNo ?? "",
                        ["HeatNo"] = r.HeatNo ?? ""
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GRN details for Vendor: {vendorCode}, Store: {storeId}");
                throw new InvalidOperationException("Failed to retrieve GRN details. Please try again.");
            }
        }


        public async Task<PurchaseSCNVM> UpsertSCNAsync(PurchaseSCNVM purchscnVM, int screenCode)
        {
            if (purchscnVM == null)
                throw new ArgumentNullException(nameof(purchscnVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                PurchaseSCN entity;

                if (purchscnVM.SCNId == 0)
                {
                    entity = _mapper.Map<PurchaseSCN>(purchscnVM);

                    var NextNumber = await _unitOfWork.PurchaseSCNs.GetLastSCNNoAsync(entity.Suffix);
                    entity.SCNNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.PurchaseSCNSubs = purchscnVM.PurchaseSCNSubVMs.Select(s => _mapper.Map<PurchaseSCNSub>(s)).ToList();

                    await SetSCNAuthorizationStatusAsync(entity, currentUser);

                    await _unitOfWork.PurchaseSCNs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.PurchaseSCNSubs)
                    {
                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGRNSubBalanceAsync(sub.RefGRNSubId, 0, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), "Purchase SCN Creation");
                        }

                        //======= Accecpt=============
                        if (sub.AcceptQty > 0)
                        {
                            await _stockManager.IssueOrUpdateStockAsync(sub.ItemId, entity.StoreIssId.Value, sub.AcceptQty,
                                sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, allowMultipleIssue: true);

                            await _stockManager.AddOrUpdateStockAsync(sub.ItemId, entity.AddStoreId.GetValueOrDefault(), sub.QtyConvert, sub.UnitPrice,
                                  sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, sub.Remark, allowMultipleAdd: true);
                        }

                        //=======Reject=============
                        if (sub.RejectQty > 0 && sub.ItemId > 0 && entity.StoreIssId.HasValue)
                        {
                            await _stockManager.IssueOrUpdateStockAsync(sub.ItemId,entity.StoreIssId.Value,sub.RejectQty,sub.UnitPrice,sub.BatchNo,
                                screenCode,sub.SCNSubId,entity.SCNNo,entity.SCNDate,allowMultipleIssue: true);

                            await _stockManager.AddOrUpdateStockAsync(sub.ItemId,6,sub.RejectQty,sub.UnitPrice,sub.BatchNo,screenCode,
                                sub.SCNSubId,entity.SCNNo,entity.SCNDate,null,allowMultipleAdd: true);

                            // For Rejection And rework Stock Adjustment
                            if (sub.RefPoSubId.GetValueOrDefault() > 0 &&
                                   sub.RefGRNSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustGrnRejectAndPoBalanceAsync(sub.RefGRNSubId, sub.RefPoSubId, 0, sub.RejectQty, "Purchase SCN Rejected");
                            }
                        }

                        //==========Rework============
                        if (sub.ReworkQty > 0)
                        {
                            await _stockManager.IssueOrUpdateStockAsync(sub.ItemId, entity.StoreIssId.Value, sub.ReworkQty,
                                sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, allowMultipleIssue: true);

                            await _stockManager.AddOrUpdateStockAsync(sub.ItemId, 7, sub.ReworkQty, sub.UnitPrice,
                                sub.BatchNo, screenCode, sub.SCNSubId, entity.SCNNo, entity.SCNDate, null, allowMultipleAdd: true);
                        }
                    }

                    changes.AppendLine("Purchase SCN Created.");
                }
                else
                {
                    entity = await _unitOfWork.PurchaseSCNs.GetQueryable()
                        .Include(q => q.PurchaseSCNSubs)
                        .FirstOrDefaultAsync(q => q.SCNId == purchscnVM.SCNId)
                        ?? throw new InvalidOperationException("Purchase SCN found.");

                    var parentChanges = GetPropertyChanges(entity, purchscnVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(purchscnVM, entity);

                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.NoOfItems = entity.PurchaseSCNSubs.Count();

                    await SetSCNAuthorizationStatusAsync(entity, currentUser);

                    await HandleChildUpdatesAsync(entity, purchscnVM.PurchaseSCNSubVMs, changes, screenCode);

                    changes.AppendLine("Purchase SCN Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateSCNTallyStatusAsync(purchscnVM.SCNId);
                await transaction.CommitAsync();

                await LogChangesAsync(changes, purchscnVM.SCNId == 0 ? "Purchase SCN Created" : "Purchase SCN  Updated");

                var savedEntity = await _unitOfWork.PurchaseSCNs.GetQueryable()
                    .Include(q => q.PurchaseSCNSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Vendor)
                    .Include(q => q.StoreAdd)
                    .Include(q => q.StoreIssue)
                    .FirstOrDefaultAsync(q => q.SCNId == entity.SCNId);

                return _mapper.Map<PurchaseSCNVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Purchase SCN: {purchscnVM.SCNNo}");
                throw new InvalidOperationException("Failed to save Purchase SCN. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(PurchaseSCN existingSCN, List<PurchaseSCNSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingSCN.PurchaseSCNSubs.Select(s => s.SCNSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.SCNSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingSCN.PurchaseSCNSubs.Where(s => !incomingSubIds.Contains(s.SCNSubId)).ToList())
            {
                await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                changes.AppendLine($"Child Deleted - SCNSubId: {sub.SCNSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.PurchaseSCNSubs.DeleteAsync(sub.SCNSubId);
                await _unitOfWork.SaveAsync();

                var totQty = (sub.AcceptQty + sub.RejectQty + sub.ReworkQty);
                if (sub.RefGRNSubId > 0)
                {
                    await AdjustGRNSubBalanceAsync(sub.RefGRNSubId, totQty, 0, "SCN Deletion");
                }

                // For Rejection And rework Stock Adjustment
                if (sub.RefPoSubId.GetValueOrDefault() > 0 && sub.RefGRNSubId.GetValueOrDefault() > 0)
                {
                    await AdjustGrnRejectAndPoBalanceAsync(sub.RefGRNSubId, sub.RefPoSubId, sub.RejectQty,0, "Purchase SCN Rejected Deletion");
                }

            }

            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.SCNSubId == 0)
                {
                    var newSub = _mapper.Map<PurchaseSCNSub>(subVM);
                    newSub.SCNId = existingSCN.SCNId;
                    await _unitOfWork.PurchaseSCNSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.AcceptQty}");


                    if (subVM.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        var totQty = (subVM.AcceptQty.GetValueOrDefault() + subVM.RejectQty.GetValueOrDefault() + subVM.ReworkQty.GetValueOrDefault());
                        await AdjustGRNSubBalanceAsync(subVM.RefGRNSubId, 0,totQty, "SCN Creation");
                    }

                    // For Rejection And rework Stock Adjustment
                    if (subVM.RefPoSubId.GetValueOrDefault() > 0 &&
                               subVM.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustGrnRejectAndPoBalanceAsync(subVM.RefGRNSubId, subVM.RefPoSubId, 0, subVM.RejectQty.GetValueOrDefault(), "Purchase SCN Rejected Creation");
                    }


                    if (subVM.AcceptQty.GetValueOrDefault() > 0)
                    {
                        await _stockManager.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId.Value, subVM.AcceptQty.GetValueOrDefault(),
                            subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                        await _stockManager.AddOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.AddStoreId.Value, subVM.QtyConvert, subVM.UnitPrice.GetValueOrDefault(),
                                                                             subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                    }

                    if (subVM.RejectQty.GetValueOrDefault() > 0)
                    {
                        await _stockManager.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId.Value, subVM.RejectQty.GetValueOrDefault(),
                            subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                        await _stockManager.AddOrUpdateStockAsync(subVM.ItemId.Value, 6, subVM.RejectQty.Value, subVM.UnitPrice.GetValueOrDefault(),
                                                                    subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);

                    }

                    if (subVM.ReworkQty.GetValueOrDefault() > 0)
                    {
                        await _stockManager.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId.Value, subVM.ReworkQty.GetValueOrDefault(),
                            subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                        await _stockManager.AddOrUpdateStockAsync(subVM.ItemId.Value, 7, subVM.ReworkQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(),
                                                                    subVM.BatchNo, screenCode, newSub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                    }
                }
                else
                {
                    var existingSub = existingSCN.PurchaseSCNSubs.FirstOrDefault(s => s.SCNSubId == subVM.SCNSubId);
                    if (existingSub != null)
                    {
                        if (subVM.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGRNSubBalanceAsync(subVM.RefGRNSubId, (existingSub.AcceptQty + existingSub.RejectQty + existingSub.ReworkQty),
                                (subVM.AcceptQty.GetValueOrDefault() + subVM.RejectQty.GetValueOrDefault() + subVM.ReworkQty.GetValueOrDefault()), "SCN Update");
                        }

                        // For Rejection And rework Stock Adjustment
                        if (subVM.RefPoSubId.GetValueOrDefault() > 0 &&
                            subVM.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGrnRejectAndPoBalanceAsync(subVM.RefGRNSubId, subVM.RefPoSubId, existingSub.RejectQty,
                                    subVM.RejectQty.GetValueOrDefault(), "Purchase SCN Rejected");
                        }

                        await DeleteStockIssueAndTrackAsync(subVM.SCNSubId, subVM.ItemId.Value, screenCode);
                        await DeleteStockAddAsync(subVM.SCNSubId, subVM.ItemId.Value, screenCode);

                        if (subVM.AcceptQty.GetValueOrDefault() > 0)
                        {
                            await _stockManager.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId.Value, subVM.AcceptQty.GetValueOrDefault(),
                            subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManager.AddOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.AddStoreId.GetValueOrDefault(), subVM.QtyConvert, subVM.UnitPrice.GetValueOrDefault(),
                                 subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }

                        if (subVM.RejectQty.GetValueOrDefault() > 0)
                        {
                            await _stockManager.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId.Value, subVM.RejectQty.GetValueOrDefault(),
                                subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManager.AddOrUpdateStockAsync(subVM.ItemId.Value, 6, subVM.RejectQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(),
                                subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }

                        if (subVM.ReworkQty.GetValueOrDefault() > 0)
                        {
                            await _stockManager.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingSCN.StoreIssId.Value, subVM.ReworkQty.GetValueOrDefault(),
                                subVM.UnitPrice.GetValueOrDefault(), subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManager.AddOrUpdateStockAsync(subVM.ItemId.Value, 7, subVM.ReworkQty.GetValueOrDefault(), subVM.UnitPrice.GetValueOrDefault(),
                                subVM.BatchNo, screenCode, subVM.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        await _unitOfWork.PurchaseSCNSubs.UpdateAsync(existingSub);
                        await _unitOfWork.SaveAsync();


                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        public async Task UpdateSCNTallyStatusAsync(int scnId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == scnId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var purchaseSCN = await _unitOfWork.PurchaseSCNs.GetAsync(scnId);

                if (purchaseSCN == null)
                    return;

                if (purchaseSCN.ShortClose || purchaseSCN.SCNCancel)
                    return;

                purchaseSCN.SCNTally = (totalBalQty == 0);

                await _unitOfWork.PurchaseSCNs.UpdateAsync(purchaseSCN);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateSCNTallyStatusAsync] Error updating SCN:- {scnId}");
                throw new InvalidOperationException("Failed to update Purchase SCN Tally status. Please contact support.");
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
                    .Where(e => e.GRNId == grnSub.GRNId && !e.ItemCancel)
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


        private async Task AdjustGrnRejectAndPoBalanceAsync(
                    int? refGrnSubId,
                    int? refPoSubId,
                    decimal oldRejectQty,
                    decimal newRejectQty,
                    string context)
        {
            try
            {
                if (!refGrnSubId.HasValue || !refPoSubId.HasValue)
                    return;

                var grnSub = await _unitOfWork.PurchaseGRNSubs.GetAsync(refGrnSubId.Value);
                if (grnSub == null || grnSub.Qty <= 0)
                    return;

                var poSub = await _unitOfWork.PurchPoSubs.GetAsync(refPoSubId.Value);
                if (poSub == null)
                    return;

                var po = await _unitOfWork.PurchPos.GetAsync(poSub.PoId);
                if (po == null || po.IsOpenPO || !po.isRejTrackReq)
                    return;

                // 🔐 Only POQty can affect PO
                decimal maxPOQty = grnSub.Qty;

                decimal oldAffectQty = Math.Min(oldRejectQty, maxPOQty);
                decimal newAffectQty = Math.Min(newRejectQty, maxPOQty);

                decimal deltaQty = newAffectQty - oldAffectQty;
                if (deltaQty == 0)
                    return;

                // ================= GRN =================
                if (deltaQty > 0)
                {
                    decimal availablePO = maxPOQty - grnSub.SCNRejRevertedPOQty;
                    decimal applyQty = Math.Min(deltaQty, availablePO);

                    if (applyQty <= 0)
                        throw new InvalidOperationException($"{context}: PO reject exceeds GRN PO qty.");

                    grnSub.SCNRejRevertedPOQty += applyQty;
                }
                else
                {
                    decimal revertQty = Math.Abs(deltaQty);

                    if (revertQty > grnSub.SCNRejRevertedPOQty)
                        throw new InvalidOperationException($"{context}: Invalid GRN revert.");

                    grnSub.SCNRejRevertedPOQty -= revertQty;
                }

                await _unitOfWork.PurchaseGRNSubs.UpdateAsync(grnSub);
                await _unitOfWork.SaveAsync();

                // ================= PO =================
                decimal poDeltaQty = newAffectQty - oldAffectQty;

                await AdjustPoSubBalanceByDeltaAsync(
                    refPoSubId,
                    poDeltaQty,
                    context);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustGrnRejectAndPoBalance] {context}");
                throw;
            }
        }


        private async Task AdjustPoSubBalanceByDeltaAsync(
                     int? refPoSubId,
                     decimal deltaQty,
                     string context)
        {
            if (!refPoSubId.HasValue || deltaQty == 0)
                return;

            var poSub = await _unitOfWork.PurchPoSubs.GetAsync(refPoSubId.Value);
            if (poSub == null)
                return;

            var po = await _unitOfWork.PurchPos.GetAsync(poSub.PoId);
            if (po == null || po.IsOpenPO)
                return;

            // 🔁 deltaQty > 0 → revert PO
            // 🔁 deltaQty < 0 → consume PO
            decimal finalBalQty = poSub.BalQty + deltaQty;

            if (finalBalQty < 0)
                throw new InvalidOperationException(
                    $"{context}: PO balance cannot go negative.");

            poSub.BalQty = finalBalQty;

            await _unitOfWork.PurchPoSubs.UpdateAsync(poSub);
            await _unitOfWork.SaveAsync();

            var totalBalQty = await _unitOfWork.PurchPoSubs
                .GetQueryable()
                .Where(e => e.PoId == poSub.PoId && !e.ItemCancel)
                .SumAsync(e => e.BalQty);

            po.PoTally = (totalBalQty == 0);
            await _unitOfWork.PurchPos.UpdateAsync(po);
            await _unitOfWork.SaveAsync();
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
                    screen: "Purchase SCN",
                    action: action,
                    additionalInfo: changes.ToString()
                );
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to LogChangesAsync in Purchase SCN");
            }
        }


        public async Task DeleteAndResequenceAsync(PurchaseSCNSubVM subitem, PurchaseSCNVM scnVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.SCNSubId > 0)
                {
                    var entity = await _unitOfWork.PurchaseSCNSubs.GetAsync(subitem.SCNSubId);

                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.RefGRNSubId > 0)
                    {
                        await AdjustGRNSubBalanceAsync(subitem.RefGRNSubId, entity.AcceptQty + entity.RejectQty + entity.RejectQty, 0, "SCN Deletion");

                        // For Rejection And rework Stock Adjustment
                        if (subitem.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGrnRejectAndPoBalanceAsync(subitem.RefGRNSubId, subitem.RefPoSubId, subitem.RejectQty.GetValueOrDefault(), 0, "Purchase SCN Rejected Deletion");
                        }

                        await DeleteStockIssueAndTrackAsync(subitem.SCNSubId, subitem.ItemId.Value, screenCode);
                        await DeleteStockAddAsync(subitem.SCNSubId, subitem.ItemId.Value, screenCode);
                    }

                    await _unitOfWork.PurchaseSCNSubs.DeleteAsync(entity);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Purchase SCN",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"SCN No: {scnVM?.SCNNo}");
                }
                else
                {
                    scnVM.PurchaseSCNSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNId == scnVM.SCNId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                await UpdateSCNTallyStatusAsync(scnVM.SCNId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private async Task DeleteStockIssueAndTrackAsync(int ScnSubId, int itemId, int screenCode)
        {
            try
            {
                var issueIds = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(s => s.SubItemRefID == ScnSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.IssueId)
                .ToListAsync();

                foreach (var issueid in issueIds)
                {
                    if (issueid > 0)
                        await _stockManager.DeleteStockIssueAsync(issueid);

                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to DeleteStockIssueAndTrackAsync in Purchase SCN");
            }
        }

        private async Task DeleteStockAddAsync(int ScnSubId, int itemId, int screenCode)
        {
            try
            {
                var AddIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == ScnSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.AddId)
                .ToListAsync();

                foreach (var addId in AddIds)
                {
                    if (addId > 0)
                        await _stockManager.DeleteStockAddAsync(addId);

                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to DeleteStockIssueAndTrackAsync in Purchase SCN");
            }
        }

        public async Task<bool> DeletePurchaseSCNByIdAsync(int SCNId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var scn = await _unitOfWork.PurchaseSCNs
                    .GetQueryable()
                    .Include(e => e.PurchaseSCNSubs)
                    .FirstOrDefaultAsync(e => e.SCNId == SCNId);

                if (scn == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in scn.PurchaseSCNSubs)
                {
                    if (sub.RefGRNSubId > 0)
                    {
                        await AdjustGRNSubBalanceAsync(sub.RefGRNSubId, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), 0, "SCN Deletion");

                        // For Rejection And rework Stock Adjustment
                        if (sub.RefPoSubId.GetValueOrDefault() > 0 )
                        {
                            await AdjustGrnRejectAndPoBalanceAsync(sub.RefGRNSubId, sub.RefPoSubId, sub.RejectQty, 0, "Purchase SCN Rejected Deletion");
                        }
                    }

                    await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                    await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                }

                var deleted = await _unitOfWork.PurchaseSCNs.DeleteAsync(SCNId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase SCN",
                    action: $"Deleted SCN: {scn.SCNNo}",
                    additionalInfo: $"SCN Id: {scn.SCNId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete SCN: {SCNId}");
                throw;
            }
        }

        public async Task<bool> HasAnyItemOrPurchaseSCNCancelAsync(int SCNId)
        {
            try
            {
                var isGRNCancelled = await _unitOfWork.PurchaseSCNs
                    .AnyAsync(q => q.SCNId == SCNId && q.SCNCancel == true);

                var isItemCancelled = await _unitOfWork.PurchaseSCNSubs
                    .AnyAsync(i => i.SCNId == SCNId && i.ItemCancel == true);

                return isGRNCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrQuoteCancelAsync for SCNId: {SCNId}");
                throw;
            }
        }

        public async Task<(bool CanDelete, string Message)> ToCheckStockQtyIssued(int SCNId, int screenCode, string refNo)
        {
            try
            {
                var SCNSubIds = await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .Where(s => s.SCNId == SCNId)
                    .Select(s => s.SCNSubId)
                    .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa =>
                        SCNSubIds.Contains(sa.SubItemRefID) &&
                        sa.ScreenCode == screenCode && sa.RefNo == refNo &&
                        sa.BalQty < sa.AddQty)
                    .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Purchase SCN. Some sub-items have already been transacted/issued.");

                return (true, "Purchase SCN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ToCheck_Stock_Qty_Issued for SCNId: {SCNId}");
                throw new Exception("Error checking PurchaseSCN delete eligibility", ex);
            }
        }

        public async Task<bool> GetPurchaseSCNByIdIsCancelAsync(int SCNId)
        {
            return await _unitOfWork.PurchaseSCNs
                .GetQueryable()
                .Where(e => e.SCNId == SCNId)
                .AnyAsync(e =>
                    e.SCNCancel == true ||
                    e.PurchaseSCNSubs.Any(s => s.ItemCancel == true)
                );
        }

        //Item Cancel

        public async Task UpsertSCNShortCloseAsync(PurchaseSCNVM purchaseSCNVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingSCN = await _unitOfWork.PurchaseSCNs.GetAsync(purchaseSCNVM.SCNId);
                if (existingSCN == null)
                    throw new InvalidOperationException("Purchase SCN not found.");

                existingSCN.ShortClose = purchaseSCNVM.ShortClose;

                await _unitOfWork.PurchaseSCNs.UpdateAsync(existingSCN);
                await _unitOfWork.SaveAsync();

                await UpdateSCNTallyStatusAsync(purchaseSCNVM.SCNId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSCNShortCloseAsync] Validation issue");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSCNShortCloseAsync] Unexpected error");
            }
        }


        public async Task UpdateItemCancelAndAddorRevertAsync(PurchaseSCNSubVM subItem, PurchaseSCNVM scnVM ,int screenCode)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.PurchaseSCNSubs.GetQueryable().Where
                                (x => x.SCNSubId == subItem.SCNSubId).FirstOrDefaultAsync();
                var existingSCN = await _unitOfWork.PurchaseSCNs.GetAsync(subItem.SCNId);

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with SCNSubId {subItem.SCNSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateGRNBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;

                await _unitOfWork.PurchaseSCNSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    await AdjustGRNSubBalanceAsync(subEntity.RefGRNSubId, (subEntity.AcceptQty + subEntity.RejectQty + subEntity.ReworkQty), 0, $"Purchase SCN Item Cancel - {subItem.ItemCode}");

                    // For Rejection And rework Stock Adjustment
                    if (subEntity.RefPoSubId.GetValueOrDefault() > 0 &&
                            subEntity.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustGrnRejectAndPoBalanceAsync(subEntity.RefGRNSubId, subEntity.RefPoSubId, subEntity.RejectQty, 0, "Purchase SCN Rejected Cancel");
                    }

                    await DeleteStockIssueAndTrackAsync(subItem.SCNSubId, subItem.ItemId.Value, screenCode);
                    await DeleteStockAddAsync(subItem.SCNSubId, subItem.ItemId.Value, screenCode);
                }
                else
                {
                    await AdjustGRNSubBalanceAsync(subEntity.RefGRNSubId, 0, (subEntity.AcceptQty + subEntity.RejectQty + subEntity.ReworkQty), $"Purchase SCN Revert Cancel - {subItem.ItemCode}");

                    // For Rejection And rework Stock Adjustment
                    if (subItem.RefPoSubId.GetValueOrDefault() > 0 && subItem.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustGrnRejectAndPoBalanceAsync(subItem.RefGRNSubId, subItem.RefPoSubId, 0, subItem.RejectQty.GetValueOrDefault(), "Purchase SCN Rejected Cancel");
                    }

                    await _stockManager.IssueOrUpdateStockAsync(subItem.ItemId.Value, existingSCN.StoreIssId.Value, subItem.AcceptQty.Value,
                        subItem.UnitPrice.Value, subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate);

                    // Qty To Convert is Accept Qty
                    await _stockManager.AddOrUpdateStockAsync(subItem.ItemId.Value, scnVM.AddStoreId.Value, subItem.QtyConvert, subItem.UnitPrice.Value,
                        subItem.BatchNo, screenCode, subItem.SCNSubId, scnVM.SCNNo, scnVM.SCNDate, subItem.Remark);

                    if (subItem.RejectQty > 0)
                    {
                        await _stockManager.IssueOrUpdateStockAsync(subItem.ItemId.Value, existingSCN.StoreIssId.Value, subItem.RejectQty.Value,
                            subItem.UnitPrice.Value, subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                        await _stockManager.AddOrUpdateStockAsync(subItem.ItemId.Value, 6, (subItem.RejectQty.Value), subItem.UnitPrice.Value,
                            subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                    }

                    if (subItem.ReworkQty > 0)
                    {
                        await _stockManager.IssueOrUpdateStockAsync(subItem.ItemId.Value, existingSCN.StoreIssId.Value, subItem.ReworkQty.Value,
                            subItem.UnitPrice.Value, subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                        await _stockManager.AddOrUpdateStockAsync(subItem.ItemId.Value, 7, (subItem.ReworkQty.Value), subItem.UnitPrice.Value,
                            subItem.BatchNo, screenCode, subItem.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                    }
                }

                await UpdateSCNTallyStatusAsync(subItem.SCNId);

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

        public async Task ValidateGRNBalanceBeforeRevertAsync(PurchaseSCNSub sub)
        {
            if (sub.RefGRNSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.PurchaseGRNSubs.GetAsync(sub.RefGRNSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"Purchase GRN not found for RefGRNSubId: {sub.RefGRNSubId}");

            decimal ExactQty = entity.BalQty + entity.ExtraBalQty;

            if (ExactQty < sub.AcceptQty)
            {
                throw new InvalidOperationException($"Cannot revert because GRN balance ({ExactQty}) is less than required quantity ({sub.AcceptQty}).");
            }
        }

        //SCN Cancel
        public async Task UpdatedCancelStatusAndAddOrRevertQty(PurchaseSCNVM scnVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingSCN = await _unitOfWork.PurchaseSCNs.GetAsync(scnVM.SCNId);
                if (existingSCN == null)
                    throw new InvalidOperationException("Purchase SCN not found.");

                var subs = await _unitOfWork.PurchaseSCNSubs
                    .GetQueryable()
                    .Where(s => s.SCNId == scnVM.SCNId)
                    .ToListAsync();

                if (!scnVM.SCNCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateGRNBalanceBeforeRevertAsync(sub);
                    }
                }

                existingSCN.SCNCancel = scnVM.SCNCancel;
                existingSCN.CancelReason = scnVM.CancelReason;
                existingSCN.CancelDate = scnVM.CancelDate;
                existingSCN.CancelBy = scnVM.CancelBy;

                await _unitOfWork.PurchaseSCNs.UpdateAsync(existingSCN);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingSCN.SCNCancel)
                    {
                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGRNSubBalanceAsync(sub.RefGRNSubId.Value, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), 0, $"Purchase SCN Cancelled - {existingSCN.SCNNo}");
                        }

                        // For Rejection And rework Stock Adjustment
                        if (sub.RefPoSubId.GetValueOrDefault() > 0 &&
                                sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGrnRejectAndPoBalanceAsync(sub.RefGRNSubId, sub.RefPoSubId,sub.RejectQty, 0, "Purchase SCN Rejected Cancel");
                        }

                        await DeleteStockIssueAndTrackAsync(sub.SCNSubId, sub.ItemId, screenCode);
                        await DeleteStockAddAsync(sub.SCNSubId, sub.ItemId, screenCode);

                    }
                    else
                    {
                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGRNSubBalanceAsync(sub.RefGRNSubId.Value, 0, (sub.AcceptQty + sub.RejectQty + sub.ReworkQty), $"Purchase SCN Reverted - {existingSCN.SCNNo}");
                        }

                        // For Rejection And rework Stock Adjustment
                        if (sub.RefPoSubId.GetValueOrDefault() > 0 && sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGrnRejectAndPoBalanceAsync(sub.RefGRNSubId, sub.RefPoSubId, 0, sub.RejectQty, "Purchase SCN Rejected Cancel");
                        }

                        //Issueing is Noramat Acc Qty  %%% Important %%%
                        await _stockManager.IssueOrUpdateStockAsync(sub.ItemId, existingSCN.StoreIssId.Value, sub.AcceptQty,
                            sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                        //Adding is QtyToConvert %%% Important %%%
                        await _stockManager.AddOrUpdateStockAsync(sub.ItemId, existingSCN.AddStoreId.Value, sub.QtyConvert, sub.UnitPrice,
                            sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, sub.Remark, allowMultipleAdd: true);

                        if (sub.RejectQty > 0)
                        {
                            await _stockManager.IssueOrUpdateStockAsync(sub.ItemId, existingSCN.StoreIssId.Value, sub.RejectQty,
                                sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManager.AddOrUpdateStockAsync(sub.ItemId, 6, (sub.RejectQty), sub.UnitPrice,
                                sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
                        }

                        if (sub.ReworkQty > 0)
                        {
                            await _stockManager.IssueOrUpdateStockAsync(sub.ItemId, existingSCN.StoreIssId.Value, sub.ReworkQty,
                                sub.UnitPrice, sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, allowMultipleIssue: true);

                            await _stockManager.AddOrUpdateStockAsync(sub.ItemId, 7, (sub.ReworkQty), sub.UnitPrice,
                                sub.BatchNo, screenCode, sub.SCNSubId, existingSCN.SCNNo, existingSCN.SCNDate, null, allowMultipleAdd: true);
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

        private async Task DeleteStockAddAsync(int SCNSubId, int itemId, int screenCode, string refNo)
        {
            var addId = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == SCNSubId && s.ItemId == itemId && s.ScreenCode == screenCode && s.RefNo == refNo)
                .Select(s => s.AddId)
                .FirstOrDefaultAsync();

            if (addId > 0)
                await _stockManager.DeleteStockAddAsync(addId);

            await _unitOfWork.SaveAsync();
        }

        public async Task<List<PurchaseSCNSubVM>> GetDistinctRefGRNBySCNIdAsync(int SCNId)
        {
            return await _unitOfWork.PurchaseSCNSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(s => s.SCNId == SCNId)
                        .Include(s => s.PurchaseGRNSub)
                            .ThenInclude(g => g.PurchaseGRN)
                        .Where(s => s.PurchaseGRNSub != null &&
                                    s.PurchaseGRNSub.PurchaseGRN != null)
                        .GroupBy(s => new
                        {
                            s.PurchaseGRNSub.PurchaseGRN.GRNNo,
                            s.PurchaseGRNSub.PurchaseGRN.GRNDate
                        })
                        .Select(g => new PurchaseSCNSubVM
                        {
                            RefGRNNo = g.Key.GRNNo,
                            RefGRNDate = g.Key.GRNDate
                        })
                        .ToListAsync();
        }


        public async Task<(bool CanDelete, string Message)> NeedTocheckRejection(int refGRNSubId, decimal rejQty)
        {
            var grnSub = await _unitOfWork.PurchaseGRNSubs.GetQueryable()
                                .Where(g => g.GRNSubId == refGRNSubId)
                                .FirstOrDefaultAsync();

            if (grnSub != null)
            {
                var poSub = await _unitOfWork.PurchPoSubs.GetQueryable()
                        .Where(p => p.PoSubId == grnSub.RefPoSubId)
                        .FirstOrDefaultAsync();

                if (poSub != null)
                {
                    var po = await _unitOfWork.PurchPos.GetQueryable()
                            .Where(p => p.PoId == poSub.PoId)
                            .FirstOrDefaultAsync();

                    if (po != null && po.isRejTrackReq)
                    {
                        // Cannot delete (Reject Qty already tracked in GRN)
                        if (poSub.BalQty < rejQty)
                        {
                            return (false, "SCN deletion is not allowed because the rejected quantity is already linked to a GRN created against this Purchase Order.");
                        }
                    }
                }
            }

            // Allowed to delete
            return (true, string.Empty);
        }

        public async Task<(bool CanDelete, string Message)> CanDeletePurchaseSCNAsync(int scnId)
        {
            try
            {
                var purchaseSCN = await _unitOfWork.PurchaseSCNs
                                .GetQueryable()
                                .Include(e => e.PurchaseSCNSubs)
                                .Where(e => e.SCNId == scnId).FirstOrDefaultAsync();

                if (purchaseSCN == null)
                    return (true, "Purchase SCN can be safely deleted.");

                var purchSubIds = purchaseSCN.PurchaseSCNSubs
                    .Select(es => es.SCNSubId)
                    .ToList();

                bool hasInvoice = await _unitOfWork.PurchaseInvoiceSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefSCNSubId.HasValue &&
                        purchSubIds.Contains(qs.RefSCNSubId.Value));

                if (hasInvoice)
                    return (false, "Cannot delete this Purchase SCN as a Purchase Invoice transaction exists.");

                if (purchaseSCN.SCNCancel || purchaseSCN.ShortClose)
                    return (false, "Cannot delete this Purchase SCN as it is Cancelled or Short Closed.");


                if (purchaseSCN.PurchaseSCNSubs.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Purchase SCN as one or more SCN items are cancelled.");


                return (true, "Sa can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeletePurchaseSCNAsync for ScnId: {scnId}");
                throw new Exception("Error checking Purchase SCN delete eligibility", ex);
            }
        }


        public async Task<(List<PurchaseSCNVM> scnVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber,int pageSize,Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.PurchaseSCNs
                .GetQueryable()
                .AsSplitQuery()
                .Include(e => e.Vendor)
                .Include(e => e.StoreAdd)
                .Include(e => e.StoreIssue)
                .Include(e => e.PurchaseSCNSubs)
                    .ThenInclude(s => s.Item)
                .Include(e => e.PurchaseSCNSubs)
                    .ThenInclude(s => s.PurchaseGRNSub)
                        .ThenInclude(g => g.PurchaseGRN)
                .Include(e => e.PurchaseSCNSubs)
                    .ThenInclude(s => s.CostCenter)
                .AsQueryable();

            if (filters != null && filters.Any())
            {
                foreach (var filter in filters)
                {
                    query = PurchaseSCNFilterBuilder.ApplyFilter(query,filter.Key,filter.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.SCNId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var vmList = _mapper.Map<List<PurchaseSCNVM>>(list);

            return (vmList, total);
        }

        public async Task<List<PurchaseSCNStatusVM>> GetPurchaseSCNsStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<PurchaseSCNStatusVM>("Sp_GetPurchaseSCNsPendingList", status);
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

        public static class PurchaseSCNFilterBuilder
        {
            public static IQueryable<PurchaseSCN> ApplyFilter(
                IQueryable<PurchaseSCN> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "SCNNo":
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
                                (string.IsNullOrEmpty(part1) || x.SCNNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "Vendor":
                        return query.Where(x => x.Vendor.VendorName.Contains(val));

                    case "ItemName":
                        return query.Where(x => x.PurchaseSCNSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "ItemCode":
                        return query.Where(x => x.PurchaseSCNSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "GRNNo":
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
                                x.PurchaseSCNSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.PurchaseGRNSub.PurchaseGRN.GRNNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.PurchaseGRNSub.PurchaseGRN.Suffix.Contains(part2))
                                ));
                        }

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "ModifiedBy":
                        return query.Where(x => x.ModifiedBy.Contains(val));

                    case "FromDate":
                        return query.Where(x => x.SCNDateNow >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.SCNDateNow <= DateTime.Parse(value.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<PurchaseSCN> ApplyStatusFilter(
                IQueryable<PurchaseSCN> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.SCNTally == true),
                    "Short Closed" => query.Where(x => x.ShortClose == true),
                    "Cancelled" => query.Where(x => x.SCNCancel == true),
                    "Pending" => query.Where(x => x.SCNTally == false && x.SCNCancel == false && x.ShortClose == false),
                    _ => query
                };
            }
        }
        public async Task<bool> IsDocumentUploaded(int invId)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                            .AnyAsync(c =>
                                c.ReferenceType == "Purchase SCN" &&
                                c.DocumentType == "Correspondence" &&
                                c.ReferenceId == invId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }

    }
}
