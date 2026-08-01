using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Vml.Office;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractDcOutservice;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractDcOutService
{
    public class SubConDcOutService: ISubConDcOutService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManagerService;
        public SubConDcOutService(
                 IUnitOfWork unitOfWork,
                 ICommonService commonService,
                 CurrentUserService userService,
                 IStockManagerService stockManagerService,
                 ILoggingService logs,
                 IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;

        }
       

        public async Task<Companydetails> GetCompanyDetailsAsync()
          => await _commonService.GetCompanyDetailsAsync();

        // 🔹 Vendors
        public async Task<CustomerVM?> GetCustomerByIdAsync(int CustId)
           => await _commonService.GetCustomerByIdAsync(CustId);

        public async Task<IEnumerable<CustomerVM>> SearchCustomers(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }
        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
        {
            return await _commonService.SearchVendorsAsync(searchText);
        }
        public async Task<VendorVM?> GetVendorByIdAsync(int VendorCode)
           => await _commonService.GetVendorByVenerCodeAsync(VendorCode);



        // 🔹 Contacts
        public async Task<List<VendorContact>> GetContactPersonsVendorAsync(int Vendorcode)
                => await _commonService.GetContactPersonsVendorAsync(Vendorcode);

        public async Task<List<VendorInDirect>> GetConsigneeAddressesVendorAsync(int VendorCode)
           => await _commonService.GetConsigneeAddressesVendorAsync(VendorCode);

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


        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
           => await _commonService.GetMappedStoreForFormAsync(formName);

        public async Task<decimal> GetAvailableStockByItemIdAsync(int itemId, int? storeId)
            => await _stockManagerService.GetStockForItemAsync(itemId, storeId);

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        public async Task<bool> GetIsSameItemasInAsync()
     => await _commonService.GetScreenPermissionsAsync("Sub-Contract DC-Out", "Restrict Out Item Same As In Item");

        //Stores
        public async Task<List<Store>> GetAllActiveStoresAsync()
        {
            var result = await _commonService.GetAllIssueStoresAsync();
            return result.ToList();
        }

        //Machines
        public Task<List<Machine>> GetAllActiveMachinesAsync()
            => _commonService.GetAllMachineAsync();
        public async Task<bool> IsPOWiseSubConDcOutEnabledAsync()
         => await _commonService.GetScreenPermissionsAsync("Sub-Contract DC-Out", "PO Wise Sub-Contract DC Outgoing");


        //Stock
        public async Task<decimal> GetStockForItemsAsync(int itemId, int storeId)
        {
            return await _stockManagerService.GetStockForItemAsync(itemId, storeId);
        }
   

        //SubContract DC-OUT Operations

        public async Task<(bool CanDelete, string Message)> CanDeleteSubconDcOutgoingAsync(int dcId, int screenCode)
        {
            try
            {
                var subConDc = await _unitOfWork.SubConDCOuts
                                .GetQueryable()
                                .Include(e => e.SubConDcOutSubs)
                                .Where(e => e.DcId == dcId).FirstOrDefaultAsync();

                if (subConDc == null)
                    return (true, "Subcontract Dc can be safely deleted.");

                var dcSubIds = subConDc.SubConDcOutSubs
                    .Select(es => es.DcSubId)
                    .ToList();

                bool hasGRN = await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefDcSubId.HasValue &&
                        dcSubIds.Contains(qs.RefDcSubId.Value));

                if (hasGRN)
                    return (false, "Cannot delete this Subcontract DC as a GRN exists.");

                if (subConDc.SubConDcOutSubs.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Subcontract DC as one or more DC items are cancelled.");

                if (subConDc.Cancel || subConDc.ShortClose)
                    return (false, "Cannot delete this Subcontract DC as it is Cancelled or Short-Closed.");

                return (true, "Subcontract DC can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteSubconDcOutgoingAsync for DcId: {dcId}");
                return (false, "Unable to verify item CanDeleteSubconDcOutgoingAsync. Please try again or contact support.");
            }
        }


        public async Task<bool> DeleteSubconDcByDcIdAsync(int dcId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var dc = await _unitOfWork.SubConDCOuts
                    .GetQueryable()
                    .Include(e => e.SubConDcOutSubs)
                    .FirstOrDefaultAsync(e => e.DcId == dcId);

                if (dc == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in dc.SubConDcOutSubs)
                {
                    if (sub.TransType == "In")
                    {
                        if (sub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(sub.RcSubId, sub.Qty, 0, "Subcontract DC Delete");
                        }

                        bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();

                        if (!PoWiseDcOut)
                        {
                            if (sub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Subcontract DC Delete");
                            }
                        }
                       
                    }
                    else
                    {
                        await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);
                    }
                }


                //----------------AutoDcRunning-------------------------------------
                var runningRow = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.DcType == "SUBCONDCOUT" &&
                                x.Suffix == dc.Suffix);
                if (runningRow != null)
                {
                    long oldDcNo = 0;
                    long.TryParse(dc.DcNo.ToString(), out oldDcNo);
                    if (runningRow.LastNumber == oldDcNo)
                    {
                        runningRow.LastNumber = (oldDcNo - 1);
                        await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                    }
                }
                //----------------------------------------------------------------------------

                await _unitOfWork.SubConDCOuts.DeleteAsync(dc);

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Subcontract Delivery Challan",
                    action: $"Deleted Delivery Challan: {dc.DcNo}",
                    additionalInfo: $"Dc Id: {dc.DcId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Dc: {dcId}");
                throw;
            }
        }


        public async Task<(bool CanItemCancel, string Message)> CanSubconDcItemCancelCheckAsync(SubConDcOutSubVM subItem)
        {
            try
            {
                bool hasDc = await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefDcSubId.HasValue && qs.RefDcSubId == subItem.DcSubId && !qs.ItemCancel);

                if (hasDc)
                    return (false, "Cannot cancel this Item as a Subcontract GRN transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanLabourGRNItemCancelCheckAsync for DcSubId: {subItem.DcSubId}");
                return (false, "Unable to verify item cancellation status. Please try again or contact support.");
            }
        }
        public async Task<bool> IsSubConDcTransactionsMatchedwilecancelAsync(int DcId, SubConDcOutVM subConDcOutVM)
        {
            try
            {
                var DcSubIds = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.DcId == DcId)
                    .Select(x => x.DcSubId)
                    .ToListAsync();

                bool hasDc = DcSubIds.Any();


                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = subConDcOutVM?.SubConDcOutSubVMs?
                            .Where(x => x.TransType == "Out")
                            .ToList();

                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.Qty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty ?? 0);

                    qtyMismatch = totalQty != totalBalQty;
                }

                // If either transactions exist OR quantity mismatch → return true
                return  qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for DcId: {DcId}");
                return false;
            }
        }

        public async Task<bool> IsSubConDcTransactionsMatchedAsync(int DcId, SubConDcOutVM subConDcOutVM)
        {
            try
            {
                var DcSubIds = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.DcId == DcId)
                    .Select(x => x.DcSubId)
                    .ToListAsync();

                bool hasDc = DcSubIds.Any();

                bool hasTransactions = false;

                if (hasDc)
                {
                    // Check GRN references
                    hasTransactions = await _unitOfWork.SubConGRNSubs
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.RefDcSubId.HasValue &&
                            DcSubIds.Contains(pqs.RefDcSubId.Value));
                }

                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = subConDcOutVM?.SubConDcOutSubVMs?
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
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for DcId: {DcId}");
                return false;
            }
        }

        public async Task<decimal> GetPurchPoItemBalQtyFromPoSubId(int poSubId)
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
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for PoSubId: {poSubId}");
                throw new InvalidOperationException("Failed to retrieve Purchase Po balance quantity.");
            }
        }

        public async Task<decimal> GetRouteCardSubBalQtyFromRcSubId(int rcSubId)
        {
            try
            {
                return await _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(e => e.RCSubId == rcSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for RCSubId: {rcSubId}");
                throw new InvalidOperationException("Failed to retrieve Routecard Sub balance quantity.");
            }
        }

        public async Task<decimal> GetRcItemBalQtyFromRcSubId(int rcSubId)
        {
            try
            {
                return await _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(e => e.RCSubId == rcSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for RcSubId: {rcSubId}");
                throw new InvalidOperationException("Failed to retrieve Route Card balance quantity.");
            }
        }

        public async Task<SubConDcOutSubVM?> GetSubConDcOutDetailsByDcSubIdAsync(int dcSubId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(q => q.DcSubId == dcSubId)
                    .Select(q => new SubConDcOutSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Subcontract DC Sub item detail for DcSubId: {dcSubId}");
                throw new InvalidOperationException("Failed to retrieve SubContract DC sub-item details.");
            }
        }

        public async Task<SubConDcOutVM?> GetSubConDcOutByDcIdAsync(int dcId)
        {
            try
            {
                var entity = await _unitOfWork.SubConDCOuts.GetQueryable()
                    .Include(q => q.StoreIssue)
                    .Include(q => q.Vendor)
                    .Include(q => q.SubConDcOutSubs.OrderBy(s => s.SlNo)) // FIX HERE
                        .ThenInclude(s => s.Item)
                    .Include(q => q.SubConDcOutSubs)
                        .ThenInclude(s => s.CostCenter)
                    .Include(q => q.SubConDcOutSubs)
                        .ThenInclude(s => s.PurchPoSub)
                            .ThenInclude(s => s.PurchPo)
                    .Include(q => q.SubConDcOutSubs)
                        .ThenInclude(s => s.ComponentRouteCardSub)
                            .ThenInclude(s => s.RouteCard)
                    .Include(q => q.SubConDcOutSubs)
                        .ThenInclude(s => s.Process)
                    .Include(q => q.SubConDcOutSubs)
                        .ThenInclude(s => s.Machine)
                    .FirstOrDefaultAsync(q => q.DcId == dcId);

                if (entity == null)
                    return null;

                var vm = _mapper.Map<SubConDcOutVM>(entity);

                // Ensure order after mapping also (safe side)
                vm.SubConDcOutSubVMs = vm.SubConDcOutSubVMs
                    .OrderBy(s => s.SlNo)
                    .ToList();

                var itemIds = vm.SubConDcOutSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && vm.StoreIssId.HasValue)
                {
                    var stockDict = await _stockManagerService
                        .GetStockForItemsAsync(itemIds, vm.StoreIssId.Value);

                    foreach (var sub in vm.SubConDcOutSubVMs)
                    {
                        if (sub.ItemId.HasValue &&
                            stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                        {
                            sub.StockQty = qty;
                        }
                        else
                        {
                            sub.StockQty = 0m;
                        }
                    }
                }

                return vm;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetSubConDcOutByDcIdAsync({dcId})");
                return null;
            }
        }
        public async Task<int> GetPendingRcCountAsync()
        {
            return await _unitOfWork.RouteCards
                .GetQueryable()
                .Include(j => j.RouteCardSubs)
                .Where(j => j.RcStatus < 2)
                .CountAsync();
        }
        public async Task<string> GetMaterialIssueNumberAsync(string suffix)
        {
            try
            {
                string nextNumber = await _commonService.GeneratePreviewAutoDcRunningNoAsync("SUBCONDCOUT", suffix);

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Material Issue number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Material Issue number.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetAllOpenRcAsync(int storeId, int vendorcode)
        {
            try
            {
                var routeCards = await _unitOfWork.RouteCards.GetQueryable()
                    .Include(r => r.CostCenter)
                    .Include(r => r.RouteCardSubs)
                        .ThenInclude(s => s.IncomingItem)
                    .Include(r => r.RouteCardSubs)
                        .ThenInclude(s => s.OutgoingItem)
                    .Include(r => r.RouteCardSubs)
                        .ThenInclude(s => s.Machine)
                    .Include(r => r.RouteCardSubs)
                        .ThenInclude(s => s.Process)
                    .Where(r => r.RcStatus < 2)
                    .ToListAsync();

                var finalList = new List<Dictionary<string, object>>();

                var allItemIds = routeCards
                    .SelectMany(r => r.RouteCardSubs)
                    .Where(s => s.ItemIdOut.HasValue && s.BalQty > 0)
                    .Select(s => s.ItemIdOut!.Value)
                    .Distinct()
                    .ToList();

                var RoutCradItems = routeCards
                  .SelectMany(r => r.RouteCardSubs)
                  .Where(s => s.ItemIdIn.HasValue && s.BalQty > 0)
                  .Select(s => s.ItemIdIn!.Value)
                  .Distinct()
                  .ToList();


                var stockRateDict =
                    await _stockManagerService.GetStockRateForItemsAsync(allItemIds, storeId);

                foreach (var rc in routeCards)
                {
                    var subs = rc.RouteCardSubs
                        .Where(s => s.BalQty > 0 && s.ProcessStatus < 2 && !s.IsProcessSkip)
                        .ToList();

                    var validSubs = GetCurrentHierarchySubs(subs);

                    foreach (var s in validSubs)
                    {
                        if (!s.ItemIdOut.HasValue)
                            continue;

                        decimal stockQty = 0m;

                        // ==========================================
                        // ✅ SEQ NO CHECK CORRECTLY ON SUB TABLE
                        // ==========================================
                        if (s.SeqNo == 1)
                        {
                            stockQty = await GetAvailableStockByItemIdAsync(
                                s.ItemIdOut.Value, storeId);
                        }
                        else
                        {
                            var sourceRcSubIds =
                                await GetIssueSourceRCSubIdsAsync(
                                    rc.RCId,
                                    s.SeqNo ?? 0,
                                    s.RCSubId);

                            foreach (var srcSubId in sourceRcSubIds)
                            {
                                var availableStock = await GetAvailableStockByItemIdAndRcAndScreenAsync(s.ItemIdOut.Value, storeId, srcSubId);
                                stockQty += availableStock;
                            }
                        }
                        var poDetail = await _unitOfWork.PurchPos
                                  .GetQueryable()
                                  .AsNoTracking()
                                  .Include(x => x.PurchPoSubs)
                                  .Where(x =>
                                      x.PurchORSubCon == false &&
                                      x.VendorCode == vendorcode &&
                                      x.PurchPoSubs.Any(p =>
                                          p.ProcessId == s.ProcessId && p.RefRcSubId == s.RCSubId &&
                                          p.ItemId == s.ItemIdIn && p.RCId == s.RCId && p.BalQty > 0 &&
                                          s.BalQty > 0))
                                  .SelectMany(x => x.PurchPoSubs
                                      .Where(p =>
                                          p.ProcessId == s.ProcessId &&
                                          p.ItemId == s.ItemIdIn && p.RCId == s.RCId)
                                      .Select(p => new
                                      {
                                          x.PoId,
                                          x.PONo,
                                          x.PODate,
                                          x.IsOpenPO,
                                          x.Suffix,
                                          p.PoSubId,
                                          p.ProcessId,
                                          p.ItemId,
                                          p.Qty,
                                          p.BalQty
                                      }))
                                  .FirstOrDefaultAsync();


                        var unitPrice = stockRateDict.TryGetValue(
                            s.ItemIdOut.Value, out var rate)
                            ? rate
                            : 0m;

                        var NextRcqty = await GetnextProcessQtyRCSubIdsAsync(   rc.RCId,  s.SeqNo ?? 0, s.RCSubId);

                        var qtyIn = s.SeqNo == 1 ? Math.Min(rc.RcQty, NextRcqty.FirstOrDefault()) : Math.Min(NextRcqty.FirstOrDefault(), s.BalQty);

                        // var qtyIn = s.SeqNo == 1 ? rc.RcQty : s.BalQty;

                        finalList.Add(new Dictionary<string, object>
                        {
                            ["Selected"] = false,

                            ["RcId"] = rc.RCId,
                            ["RcSubId"] = s.RCSubId,
                            ["RefRCNo"] = $"{rc.RCNo}{rc.Suffix}",
                            ["RefRCDate"] = rc.RCDate,

                            ["ItemIdIn"] = s.ItemIdIn,
                            ["ItemCodeIn"] = s.IncomingItem?.ItemCode ?? "",
                            ["ItemNameIn"] = s.IncomingItem?.ItemName ?? "",
                            ["MeasureUnitIn"] = s.IncomingItem?.MeasureUnit ?? "",
                            ["QtyIn"] = Math.Round(qtyIn, 3),

                            ["ItemIdOut"] = s.ItemIdOut,
                            ["ItemCodeOut"] = s.OutgoingItem?.ItemCode ?? "",
                            ["ItemNameOut"] = s.OutgoingItem?.ItemName ?? "",
                            ["MeasureUnitOut"] = s.OutgoingItem?.MeasureUnit ?? "",

                            ["ProcessId"] = s.ProcessId,
                            ["ProcessName"] = s.Process?.ProcessName ?? "",

                            ["MachineId"] = s.MachineId,
                            ["MachineName"] = s.Machine?.MachineName ?? "",

                            ["QtyOut"] = Math.Round(s.BalQty, 3),
                            ["StockQty"] = Math.Round(stockQty, 3),
                            ["UnitPrice"] = Math.Round(unitPrice, 2),

                            ["ProcessCost"] = s.ProcessCost,

                            ["CostCenterId"] = rc.CostId,
                            ["CostCenter"] = rc.CostCenter?.ProjectNo ?? "",

                            ["HasPo"] = poDetail != null,

                            ["PoId"] = poDetail?.PoId ?? null,
                            ["PoSubId"] = poDetail?.PoSubId ?? null,

                            ["PoNo"] = $"{poDetail?.PONo ?? ""}{poDetail?.Suffix ?? ""}",


                            ["PoDate"] = poDetail?.PODate

                        });
                    }
                }

                return finalList;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching open Route Card");
                throw new InvalidOperationException(
                    "Failed to retrieve open Route Card. Please try again.", ex);
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
        private async Task<List<decimal>> GetRCSubIdsBySeqNoNextProcessQtyAsync(int seqNo, int rcId, int currentRcSubId)
        {
            return await _unitOfWork.RouteCardSubs
                .GetQueryable()
                .Where(x => x.RCId == rcId && x.SeqNo == seqNo && x.RCSubId != currentRcSubId)
                .Select(x => x.NextProcessQty)
                .ToListAsync();
        }
        private async Task<List<decimal>> GetnextProcessQtyRCSubIdsAsync(int rcId, int seqNo, int currentRcSubId)
        {
            var RcNextProQty = new List<decimal>();

            // SAME SEQ (PARALLEL)
            var sameSeqIds = await GetRCSubIdsBySeqNoNextProcessQtyAsync(seqNo, rcId, currentRcSubId);
            if (sameSeqIds.Any())
                RcNextProQty.AddRange(sameSeqIds);

            // PREVIOUS EFFECTIVE SEQ
            var prevSeqNo = await GetEffectivePreviousSeqNoAsync(rcId, seqNo);
            if (prevSeqNo.HasValue)
            {
                var prevIdss = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Where(x => x.RCId == rcId &&
                        x.SeqNo == prevSeqNo.Value &&
                        !x.IsProcessSkip)
                    .Select(x => x.NextProcessQty)
                    .ToListAsync();

                RcNextProQty.AddRange(prevIdss);
            }

            return RcNextProQty.Distinct().ToList();
        }

        private async Task<List<int>> GetIssueSourceRCSubIdsAsync(int rcId, int seqNo, int currentRcSubId)
        {
            var rcSubIds = new List<int>();

            // SAME SEQ (PARALLEL)
            var sameSeqIds = await GetRCSubIdsBySeqNoAsync(seqNo, rcId, currentRcSubId);
            if (sameSeqIds.Any())
                rcSubIds.AddRange(sameSeqIds);

            // PREVIOUS EFFECTIVE SEQ
            var prevSeqNo = await GetEffectivePreviousSeqNoAsync(rcId, seqNo);
            if (prevSeqNo.HasValue)
            {
                var prevIds = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Where(x => x.RCId == rcId &&
                        x.SeqNo == prevSeqNo.Value &&
                        !x.IsProcessSkip)
                    .Select(x => x.RCSubId)
                    .ToListAsync();

                rcSubIds.AddRange(prevIds);
            }

            return rcSubIds.Distinct().ToList();
        }

        private async Task<List<int>> GetRCSubIdsBySeqNoAsync(int seqNo, int rcId, int currentRcSubId)
        {
            return await _unitOfWork.RouteCardSubs
                .GetQueryable()
                .Where(x => x.RCId == rcId && x.SeqNo == seqNo && x.RCSubId != currentRcSubId)
                .Select(x => x.RCSubId)
                .ToListAsync();
        }

        private async Task<int?> GetEffectivePreviousSeqNoAsync(int rcId, int currentSeqNo)
        {
            try
            {
                var prevSeqNos = await _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(x => x.RCId == rcId && x.SeqNo < currentSeqNo)
                    .Select(x => x.SeqNo)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToListAsync();

                foreach (var seq in prevSeqNos)
                {
                    bool allSkipped = await _unitOfWork.RouteCardSubs.GetQueryable()
                        .Where(x => x.RCId == rcId && x.SeqNo == seq)
                        .AllAsync(x => x.IsProcessSkip);

                    if (!allSkipped)
                        return seq;
                }

                return null;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error finding previous SeqNo for RCId={rcId}, currentSeqNo={currentSeqNo}");
                throw;
            }
        }


        private List<RouteCardSub> GetCurrentHierarchySubs(List<RouteCardSub> list)
        {
            if (!list.Any())
                return new List<RouteCardSub>();

            List<RouteCardSub> hierarchy = new();

            int? currentSeq =
                list.Where(x => x.ProcessStatus != 3 && !x.IsProcessSkip)
                    .OrderBy(x => x.SeqNo)
                    .Select(x => x.SeqNo)
                   
                    .FirstOrDefault()
                ??
                list.Where(x => x.BalQty > 0 && x.ProcessStatus != 3 && !x.IsProcessSkip)
                    .OrderBy(x => x.SeqNo)
                    .Select(x => x.SeqNo)
                    .FirstOrDefault();

            if (currentSeq == null)
                return hierarchy;

            while (currentSeq != null)
            {
                var currentSeqProcesses = list
                    .Where(x => x.SeqNo == currentSeq && !x.IsProcessSkip)
                    .ToList();

                if (!currentSeqProcesses.Any())
                    break;

                hierarchy.AddRange(currentSeqProcesses);

                bool canMoveNext;

                if (currentSeqProcesses.Count == 1)
                {
                    canMoveNext = currentSeqProcesses[0].NextProcessQty > 0;
                }
                else
                {
                    var minNextQty = currentSeqProcesses.Min(x => x.NextProcessQty);
                    canMoveNext = minNextQty > 0;

                    if (canMoveNext)
                    {
                        hierarchy.RemoveAll(x => x.SeqNo == currentSeq);
                        hierarchy.AddRange(currentSeqProcesses.Where(x => x.NextProcessQty == minNextQty));
                    }
                }

                if (!canMoveNext)
                    break;

                var nextSeq = list
                    .Where(x => x.SeqNo > currentSeq)
                    .OrderBy(x => x.SeqNo)
                    .Select(x => x.SeqNo)
                    .FirstOrDefault();

                if (nextSeq == null)
                    break;

                bool allCompleted = list
                    .Where(x => x.SeqNo == nextSeq)
                    .All(x => x.ProcessStatus == 3);

                if (allCompleted)
                    break;

                currentSeq = nextSeq;
            }

            return hierarchy;
        }

        public async Task<SubConDcOutVM> UpsertDeliveryChallan(SubConDcOutVM subConDcVM, int screenCode)
        {
            if (subConDcVM == null)
                throw new ArgumentNullException(nameof(subConDcVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                SubConDcOut entity;

                if (subConDcVM.DcId == 0)
                {
                    entity = _mapper.Map<SubConDcOut>(subConDcVM);

                    // entity.DcNo = await _unitOfWork.SubConDCOuts.GetLastDcNoAsync(entity.Suffix);
                    entity.DcNo = await _commonService.GenerateAutoRunningNoAsync("SUBCONDCOUT", entity.Suffix);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.SubConDcOutSubs = subConDcVM
                        .SubConDcOutSubVMs
                        .Select(s => _mapper.Map<SubConDcOutSub>(s))
                        .ToList();

                    await _unitOfWork.SubConDCOuts.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.SubConDcOutSubs)
                    {
                        if(sub.TransType == "In")
                        {
                            if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustRcBalanceAsync(sub.RcSubId, 0, sub.Qty, "Subcontract DC Creation");
                            }
                  
                            if (!PoWiseDcOut)
                                {
                                    if (sub.RefPoSubId.GetValueOrDefault() > 0)
                                    {
                                        await AdjustPoSubBalanceAsync(sub.RefPoSubId, 0, sub.Qty, "Subcontract DC Creation");
                                    }
                                }
                        }
                        else
                        {
                            if(sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await IssueStockBySeqLogicAsync(sub, entity, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.StoreIssId, sub.Qty, sub.UnitPrice,
                                    sub.BatchNo, screenCode, sub.DcSubId, entity.DcNo, entity.DcDate, null, false);
                            }
                        }
                    }
                    changes.AppendLine("Subcontract DC Created.");
                }
                else
                {
                    entity = await _unitOfWork.SubConDCOuts.GetQueryable()
                        .Include(x => x.SubConDcOutSubs)
                        .ThenInclude(x => x.ComponentRouteCardSub)
                        .FirstOrDefaultAsync(x => x.DcId == subConDcVM.DcId)
                        ?? throw new InvalidOperationException("Subcontract DC not found.");

                    var parentChanges = GetPropertyChanges(entity, subConDcVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(subConDcVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await _unitOfWork.SaveAsync();

                    await HandleChildUpdatesAsync(entity,subConDcVM.SubConDcOutSubVMs,changes,screenCode);
                    await _unitOfWork.SaveAsync();
                    changes.AppendLine("Subcontract DC Updated.");
                }

                decimal totalBalQty = 0;

                if (!PoWiseDcOut)
                {
                    totalBalQty = await _unitOfWork.SubConDCOutSubs
                            .GetQueryable()
                            .Where(e => e.DcId == entity.DcId)
                            .SumAsync(e => e.BalQty ?? 0);
                }
                else
                {
                    totalBalQty = await _unitOfWork.SubConDCOutSubs
                           .GetQueryable()
                           .Where(e => e.DcId == entity.DcId && e.TransType == "Out")
                           .SumAsync(e => e.BalQty ?? 0);
               }

                var compIssue = await _unitOfWork.SubConDCOuts.GetAsync(entity.DcId);

                if (compIssue != null)
                {
                    compIssue.DcTally = (totalBalQty == 0);
                    await _unitOfWork.SubConDCOuts.UpdateAsync(compIssue);
                    await _unitOfWork.SaveAsync();
                }


                await LogChangesAsync(
                    changes,
                    subConDcVM.DcId == 0
                        ? "Subcontract DC Created"
                        : "Subcontract DC Updated");

                await transaction.CommitAsync();

                var savedEntity = await _unitOfWork.SubConDCOuts.GetQueryable()
                    .Include(x => x.StoreIssue)
                    .Include(x => x.SubConDcOutSubs)
                        .ThenInclude(x => x.Item)
                    .FirstOrDefaultAsync(x => x.DcId == entity.DcId);

                return _mapper.Map<SubConDcOutVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed to Upsert Subcontract Delivery Challan");
                throw;
            }
        }


        private async Task HandleChildUpdatesAsync(SubConDcOut existingIssue, 
                List<SubConDcOutSubVM> incomingSubVMs, StringBuilder changes,int screenCode)
        {
            var existingSubs = existingIssue.SubConDcOutSubs.ToList();
            var incomingIds = incomingSubVMs.Select(x => x.DcSubId).ToHashSet();
            bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();
            // =====================================================
            // DELETE
            // =====================================================
            foreach (var sub in existingSubs.Where(x => !incomingIds.Contains(x.DcSubId)))
            {

                if (sub.TransType == "In")
                {
                    if (sub.RcSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustRcBalanceAsync(sub.RcSubId, sub.Qty, 0, "Subcontract DC Delete");
                    }
                
                    if (!PoWiseDcOut)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustPoSubBalanceAsync(sub.RefPoSubId, sub.Qty, 0, "Subcontract DC Delete");
                        }
                    }
                    
                }
                else
                {
                    await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);
                }

                await _unitOfWork.SubConDCOutSubs.DeleteAsync(sub.DcSubId);
                await _unitOfWork.SaveAsync();

                changes.AppendLine($"Child Deleted - Item: {sub.Item?.ItemCode}");
            }

            // =====================================================
            // ADD / UPDATE
            // =====================================================
            foreach (var subVM in incomingSubVMs)
            {
                // -------------------------
                // ADD
                // -------------------------
                if (subVM.DcSubId == 0)
                {
                    var newSub = _mapper.Map<SubConDcOutSub>(subVM);
                    newSub.DcId = existingIssue.DcId;

                    await _unitOfWork.SubConDCOutSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    if (newSub.TransType == "In")
                    {
                        if (newSub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(newSub.RcSubId, 0, newSub.Qty, "Subcontract DC Creation");
                        }
                       
                        if (!PoWiseDcOut)
                        {
                            if (newSub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(newSub.RefPoSubId, 0, newSub.Qty, "Subcontract DC Creation");
                            }
                        }
                        
                    }
                    else
                    {
                        if (newSub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await IssueStockBySeqLogicAsync(newSub, existingIssue, screenCode);
                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingIssue.StoreIssId, newSub.Qty, newSub.UnitPrice,
                                newSub.BatchNo, screenCode, newSub.DcSubId, existingIssue.DcNo, existingIssue.DcDate, null, false);
                        }
                    }

                    changes.AppendLine($"Child Added - Item: {newSub.ItemId}");
                }

                // -------------------------
                // UPDATE
                // -------------------------
                else
                {
                    var existingSub = existingSubs.FirstOrDefault(x => x.DcSubId == subVM.DcSubId);

                    if (existingSub == null)
                        continue;

                    if (existingSub.TransType == "In")
                    {
                        if (existingSub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(existingSub.RcSubId, existingSub.Qty, subVM.Qty.GetValueOrDefault(), "Subcontract DC Update");
                        }
                       
                        if (!PoWiseDcOut)
                        {
                            if (existingSub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(subVM.RefPoSubId, existingSub.Qty, subVM.Qty.GetValueOrDefault(), "Subcontract DC Update");
                            }
                        }
                        
                    }
                    else
                    {
                        if (existingSub.RcSubId.GetValueOrDefault() > 0)
                        {
                            existingSub.Qty = subVM.Qty ?? existingSub.Qty;
                            await IssueStockBySeqLogicAsync(existingSub, existingIssue, screenCode);
                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingIssue.StoreIssId, subVM.Qty??0, existingSub.UnitPrice,
                            subVM.BatchNo, screenCode, subVM.DcSubId, existingIssue.DcNo, existingIssue.DcDate, null, false);
                        }
                    }

                   
                    changes.AppendLine($"Child Updated - Item: {existingSub.ItemId}");
                    if (existingSub == null)
                        continue;

                     _mapper.Map(subVM, existingSub);
                    await _unitOfWork.SaveAsync();

                }
            }
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
                    var poSub = await _unitOfWork.PurchPoSubs.GetAsync(refPoSubId.Value);
                    if (poSub == null) return;

                    if (oldQty > 0)
                        poSub.BalQty += oldQty;

                    if (newQty > poSub.BalQty)
                        throw new InvalidOperationException($"{context}: Qty cannot exceed Quote BalQty.");

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
                        po.PoTally = (totalBalQty == 0); // Tally only if all BalQty consumed
                        await _unitOfWork.PurchPos.UpdateAsync(po);
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
        private async Task IssueStockBySeqLogicAsync(SubConDcOutSub sub, SubConDcOut parent, int screenCode)
        {
            decimal remainingQty = sub.Qty;
            if (remainingQty <= 0)
                return;

            // =====================================================
            // SEQ = 1
            // =====================================================
            if (sub.ComponentRouteCardSub.SeqNo == 1)
            {
                await _stockManagerService.IssueOrUpdateStockAsync(
                    sub.ItemId,
                    parent.StoreIssId,
                    remainingQty,
                    sub.UnitPrice,
                    null,
                    screenCode,
                    sub.DcSubId,
                    parent.DcNo,
                    parent.DcDate,
                    allowMultipleIssue: false);

                return;
            }

            // =====================================================
            // SEQ > 1 → RESET FIRST
            // =====================================================
            var existingIssues = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(x =>
                    x.SubItemRefID == sub.DcSubId &&
                    x.ScreenCode == screenCode)
                .ToListAsync();

            foreach (var issue in existingIssues)
                await _stockManagerService.DeleteStockIssueAsync(issue.IssueId);

            // =====================================================
            // RE-ISSUE
            // =====================================================
            var issueSubIds = await GetIssueSourceRCSubIdsAsync(
                sub.ComponentRouteCardSub.RCId,
                sub.ComponentRouteCardSub.SeqNo.Value,
                sub.RcSubId.Value);

            foreach (var rcSubId in issueSubIds)
            {
                if (remainingQty <= 0)
                    break;

                var available = await GetAvailableStockByItemIdAndRcAndScreenAsync(
                    sub.ItemId,
                    parent.StoreIssId,
                    rcSubId);

                if (available <= 0)
                    continue;

                var issueQty = Math.Min(available, remainingQty);

                await _stockManagerService.IssueOrUpdateStockAsync(
                    sub.ItemId,
                    parent.StoreIssId,
                    issueQty,
                    sub.UnitPrice,
                    null,
                    screenCode,
                    sub.DcSubId,
                    parent.DcNo,
                    parent.DcDate,
                    rcSubId,
                    allowMultipleIssue: false);

                remainingQty -= issueQty;
            }

            if (remainingQty > 0)
                throw new InvalidOperationException($"Insufficient RC stock after adjustment. Remaining Qty: {remainingQty}");
        }


        private async Task<byte> GetProcessStatusAsync(RouteCardSub routeCardProcess)
        {
            try
            {
                if (routeCardProcess == null)
                    throw new ArgumentNullException(nameof(routeCardProcess));

                decimal balQty = routeCardProcess.BalQty;

                decimal accQty = routeCardProcess.AccQty;
                decimal rejQty = routeCardProcess.RejQty;
                decimal rewQty = routeCardProcess.RewQty;

                decimal issuedQty = routeCardProcess.IssuedQty;

                decimal usedQty = accQty + rejQty + rewQty;
                decimal compareQty = 0;

                // =========================
                // DETERMINE COMPARISON QTY
                // =========================
                if (routeCardProcess.SeqNo == 1)
                {
                    compareQty = routeCardProcess.RouteCard.RcQty;
                }
                else
                {
                    int? prevSeqNo = await GetEffectivePreviousSeqNoAsync(routeCardProcess.RCId, routeCardProcess.SeqNo.Value);

                    if (prevSeqNo == null)
                        return 0;

                    compareQty = await GetPrevSeqMinNextQtyAsync(routeCardProcess.RCId, prevSeqNo.Value);
                }

                // =========================
                // STATUS DECISION
                // =========================

                // Completed
                if (usedQty == compareQty && compareQty > 0 && balQty == 0)
                    return 3;

                // Partially Completed
                if ((rejQty > 0 || rewQty > 0) && balQty > 0)
                    return 2;

                // In Progress
                if ((usedQty > 0 || issuedQty > 0) && balQty > 0)
                    return 1;

                // Not Started
                return 0;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Failed to calculate Process Status | RCId: {routeCardProcess?.RCId}, " +
                    $"SeqNo: {routeCardProcess?.SeqNo}, " +
                    $"IssuedQty: {routeCardProcess?.IssuedQty}, " +
                    $"AccQty: {routeCardProcess?.AccQty}, " +
                    $"RejQty: {routeCardProcess?.RejQty}, " +
                    $"RewQty: {routeCardProcess?.RewQty}"
                );

                throw new InvalidOperationException("Failed to calculate process status. Please try again.", ex);
            }
        }



        // Get the MIN NextProcessQty of all non-skipped processes in previous SeqNo
        private async Task<decimal> GetPrevSeqMinNextQtyAsync(int rcId, int prevSeqNo)
        {
            try
            {
                var query = _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(x => x.RCId == rcId && x.SeqNo == prevSeqNo && !x.IsProcessSkip);

                if (!await query.AnyAsync())
                    return 0;

                return await query.MinAsync(x => x.NextProcessQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error calculating Min NextProcessQty for RCId={rcId}, prevSeqNo={prevSeqNo}");
                throw;
            }
        }

        public async Task<(List<SubConDcOutVM> subConDcOutVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.SubConDCOuts.GetQueryable()
                .Include(j => j.StoreIssue)
                .Include(j => j.SubConDcOutSubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.SubConDcOutSubs)
                    .ThenInclude(s => s.ComponentRouteCardSub)
                .Include(j => j.SubConDcOutSubs)
                    .ThenInclude(s => s.CostCenter)
                .Include(j => j.Vendor)
                .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = DcFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.DcId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            
            // Use AutoMapper
            var vmList = _mapper.Map<List<SubConDcOutVM>>(list);

            return (vmList, total);
        }

        public static class DcFilterBuilder
        {
            public static IQueryable<SubConDcOut> ApplyFilter(IQueryable<SubConDcOut> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "DcNo":
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
                                (string.IsNullOrEmpty(part1) || x.DcNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "RCNo":
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
                                x.SubConDcOutSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.ComponentRouteCardSub.RouteCard.RCNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.ComponentRouteCardSub.RouteCard.Suffix.Contains(part2))
                                )
                            );
                        }


                    case "ItemCode":
                        return query.Where(x => x.SubConDcOutSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.SubConDcOutSubs
                            .Any(s => s.Item.ItemName.Contains(val)));
                    case "Vendor":
                        return query.Where(x => x.Vendor.VendorName.Contains(value.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.DcDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.DcDateNow <= toDate);
                        break;
                }

                return query;
            }

            private static IQueryable<SubConDcOut> ApplyStatusFilter(IQueryable<SubConDcOut> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.DcTally == true),
                    "Pending" => query.Where(x => x.DcTally == false),
                    "Cancelled" => query.Where(x => x.Cancel == true),
                    "Short Closed" => query.Where(x => x.ShortClose == true),
                    _ => query
                };
            }
        }


        public async Task<bool> DeleteProdCompIssueByIssueIdAsync(int issueId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var productionIssueAssy = await _unitOfWork.SubConDCOuts
                    .GetQueryable()
                    .Include(e => e.SubConDcOutSubs)
                    .FirstOrDefaultAsync(e => e.DcId == issueId);

                if (productionIssueAssy == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in productionIssueAssy.SubConDcOutSubs)
                {
                    //if (sub.RefJobOrderSubId > 0)
                    //{
                    //    await AdjustJobOrderBalanceAsync(sub.RefJobOrderSubId, sub.IssueQty, 0, "Production Issue Assembly Deletion");
                    //}

                    if (sub.TransType == "Out" && sub.Qty > 0)
                    {
                        await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);
                    }
                }

                //----------------AutoDcRunning-------------------------------------
                var runningRow = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.DcType == "SUBCONDCOUT" &&
                                x.Suffix == productionIssueAssy.Suffix);
                if (runningRow != null)
                {
                    long oldDcNo = 0;
                    long.TryParse(productionIssueAssy.DcNo.ToString(), out oldDcNo);
                    if (runningRow.LastNumber == oldDcNo && runningRow.LastNumber > 1)
                    {
                        runningRow.LastNumber = (oldDcNo - 1);
                        await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                    }
                }
                //----------------------------------------------------------------------------

                var ProductionIssueComp = await _unitOfWork.SubConDCOuts.GetAsync(issueId);
                await _unitOfWork.SubConDCOuts.DeleteAsync(ProductionIssueComp);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Production Issue Component",
                    action: $"Deleted Issue no: {ProductionIssueComp.DcNo}",
                    additionalInfo: $"Issue Id: {ProductionIssueComp.DcId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Production Issue Component: {issueId}");
                throw;
            }
        }


        public async Task<SubConDcOutVM?> GetDcSubConByIdAsync(int issueId)
        {
            try
            {
                var entity = await _unitOfWork.SubConDCOuts.GetQueryable()
                            .AsNoTracking()
                            .AsSplitQuery()
                            .Include(q => q.SubConDcOutSubs)
                            .FirstOrDefaultAsync(q => q.DcId == issueId);

                return _mapper.Map<SubConDcOutVM?>(entity);

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetDcSubConByIdAsync({issueId})");
                return null;
            }
        }


        public async Task<int> GetPendingPoCountAsync(int vendorCode)
        {
            try
            {
                return await _unitOfWork.PurchPos
                    .GetQueryable()
                    .Where(p =>
                        p.VendorCode == vendorCode &&
                        !p.PoTally &&
                        !p.PoCancl &&
                        p.Authorized &&
                        p.PurchORSubCon == false &&
                        !p.PoShortClose &&
                        p.PurchPoSubs.Any(ps => !ps.ItemCancel && ps.BalQty > 0)
                    )
                    .CountAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error while fetching Pending PO Count for VendorCode = {vendorCode}");
                return 0;
            }
        }

        public async Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId)
        {
            try
            {
                //return await _unitOfWork.PurchPoSubs.GetQueryable()
                //    .Where(e => e.PoSubId == poSubId)
                //    .Select(e => e.BalQty)
                //    .FirstOrDefaultAsync();

                return 0;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for MfgPoSubId: {poSubId}");
                throw new InvalidOperationException("Failed to retrieve PO balance quantity.");
            }
        }

        public async Task<SubConDcOutSubVM?> GetSubItemDetailByIssueIdAsync(int issueSubId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(q => q.DcSubId == issueSubId)
                    .Select(q => new SubConDcOutSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Dc sub item detail for IssueSubId: {issueSubId}");
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }

        public async Task<List<SubConDcOutSubVM>> GetSubConDcOutSubsByDcIdAsync(int dcId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(s => s.DcId == dcId)
                    .OrderBy(s => s.SlNo)
                    .ProjectTo<SubConDcOutSubVM>(_mapper.ConfigurationProvider)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error fetching Sub-Contract DC Out sub-items for DcId: {dcId}");

                throw new InvalidOperationException("Failed to retrieve Sub-Contract DC Out items. Please try again.");
            }
        }


        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int? custId)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                var distinctItemIds = itemIds.Distinct().ToList();

                foreach (var itemId in distinctItemIds)
                {
                    decimal rate = 0;

                    // 1️⃣ Last Issue price (Customer specific IF custId exists)
                    var issueQuery =
                        from qs in _unitOfWork.SubConDCOutSubs.GetQueryable()
                        join q in _unitOfWork.SubConDCOuts.GetQueryable()
                            on qs.DcId equals q.DcId
                        where qs.ItemId == itemId
                        select new { qs.UnitPrice, q.VendorCode, q.DcId };

                    if (custId.HasValue)
                        issueQuery = issueQuery.Where(x => x.VendorCode == custId);

                    rate = await issueQuery
                        .OrderByDescending(x => x.DcId)
                        .Select(x => x.UnitPrice)
                        .FirstOrDefaultAsync();

                    // 2️⃣ Last Issue price (ANY customer)
                    if (rate == 0)
                    {
                        rate = await _unitOfWork.SubConDCOutSubs.GetQueryable()
                            .Where(x => x.ItemId == itemId)
                            .OrderByDescending(x => x.DcSubId)
                            .Select(x => x.UnitPrice)
                            .FirstOrDefaultAsync();
                    }

                    // 3️⃣ Customer Item Master rate
                    if (rate == 0 && custId.HasValue)
                    {
                        rate = await _unitOfWork.ItemSubs.GetQueryable()
                            .Where(x => x.ItemId == itemId && x.CustomerId == custId)
                            .Select(x => x.Rate)
                            .FirstOrDefaultAsync();
                    }

                    // 4️⃣ Item Master default rate
                    if (rate == 0)
                    {
                        rate = await _unitOfWork.ItemRepositories.GetQueryable()
                            .Where(x => x.ItemId == itemId)
                            .Select(x => x.Rate)
                            .FirstOrDefaultAsync();
                    }

                    result[itemId] = rate;
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching bulk last unit prices. CustId: {custId?.ToString() ?? "NULL"}"
                );

                throw new InvalidOperationException(
                    "Failed to fetch last unit prices. Please try again."
                );
            }
        }



        public async Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int VendorCode, int? storeId)
        {
            try
            {
                // 1️⃣ Fetch PO data
                var poData = await (
                    from p in _unitOfWork.PurchPos.GetQueryable()
                    join ps in _unitOfWork.PurchPoSubs.GetQueryable()
                        on p.PoId equals ps.PoId
                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                                 on ps.ItemId equals i.ItemId

                    where p.VendorCode == VendorCode
                          && !p.PoTally
                          && !p.PoCancl
                          && !ps.ItemCancel
                          && ps.BalQty > 0 && ps.RCId == null
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
                        i.Category.CategoryCode,
                        i.Category.CategoryName,
                        ps.BalQty,
                        ps.UnitPrice,
                        ps.DueDate,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,
                        p.MainRemark
                    }
                ).ToListAsync();

                if (!poData.Any())
                    return new List<Dictionary<string, object>>();

                // 2️⃣ Fetch stock for all PO items (single call)
                var itemIds = poData
                    .Select(x => x.ItemId)
                    .Distinct()
                    .ToList();

                var stockMap = await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);

                // 3️⃣ Merge PO + Stock
                var result = poData.Select(r =>
                {
                    stockMap.TryGetValue(r.ItemId, out var stockQty);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,
                        ["PoSubId"] = r.PoSubId,
                        ["PoId"] = r.PoId,
                        ["IsOpenPo"] = r.IsOpenPO ? "Yes" : "No",
                        ["TransType"] = "In",

                        ["PoNo"] = $"{r.PONo}{r.Suffix}",
                        ["PoDate"] = r.PODate,

                        ["ItemId"] = r.ItemId,
                        ["ItemCode"] = r.ItemCode ?? string.Empty,
                        ["ItemName"] = r.ItemName ?? string.Empty,
                        ["UOM"] = r.MeasureUnit ?? string.Empty,
                        ["CategoryCode"] = r.CategoryCode,
                        ["CategoryName"] = r.CategoryName ?? string.Empty,
                        ["Qty"] = r.BalQty,
                        ["BalQty"] = r.BalQty,
                        ["UnitPrice"] = r.UnitPrice,
                        ["PoDuedate"] = r.DueDate,

                        ["StockQty"] = stockQty, // 🔥 HERE
                        ["CostCenterId"] = r.CostCenterId,
                        ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                        ["Remark"] = r.MainRemark ?? string.Empty
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching PO details for VendorCode: {VendorCode}, StoreId: {storeId}"
                );

                throw new InvalidOperationException(
                    "Failed to retrieve PO details. Please try again.", ex);
            }
        }



        public async Task<bool> IsOpenPoAsync(int poSubId)
        {
            //return await _unitOfWork.PurchPoSubs
            //    .GetQueryable()
            //    .Where(s => s.PoSubId == poSubId)
            //    .Join(_unitOfWork.PurchPos.GetQueryable(),
            //          sub => sub.PoId,
            //          po => po.PoId,
            //          (sub, po) => po.IsOpenPO)
            //    .FirstOrDefaultAsync();
            return false;
        }



        private async Task DeleteStockIssueAndTrackAsync(int dcSubId, int itemId, int screenCode)
        {
            var issueId = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(s => s.SubItemRefID == dcSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.IssueId)
                .FirstOrDefaultAsync();

            if (issueId > 0)
                await _stockManagerService.DeleteStockIssueAsync(issueId);

            await _unitOfWork.SaveAsync();
        }

        private async Task AdjustRcBalanceAsync(int? refRcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refRcSubId.HasValue || refRcSubId == 0)
                    return;

                var routeCardSub = await _unitOfWork.RouteCardSubs.GetAsync(refRcSubId.Value);
                if (routeCardSub == null)
                    return;

                if (oldQty > 0)
                {
                    routeCardSub.BalQty += oldQty;
                    routeCardSub.IssuedQty -= oldQty;
                }

                if (newQty > routeCardSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Route Card Item Balance Qty.");

                if (newQty > 0)
                {
                    routeCardSub.BalQty -= newQty;
                    routeCardSub.IssuedQty += newQty;
                }

                routeCardSub.ProcessStatus = await GetProcessStatusAsync(routeCardSub);

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardSub);
                await _unitOfWork.SaveAsync();

                //if (routeCardSub.RCId > 0)
                //{
                //    var rc = await _unitOfWork.RouteCards.GetAsync(routeCardSub.RCId);

                //    if (rc != null)
                //    {
                //        bool isProcessCompleted =
                //            routeCardSub.IsFinalProcess &&
                //            routeCardSub.TotalQty ==
                //            (routeCardSub.AccQty + routeCardSub.RejQty + routeCardSub.RewQty) &&
                //            routeCardSub.BalQty == 0;

                //        rc.RcStatus = isProcessCompleted ? (byte)2 : (byte)1; // 2 completed  // 1 inProgress

                //        await _unitOfWork.RouteCards.UpdateAsync(rc);
                //        await _unitOfWork.SaveAsync();
                //    }
                //}


            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustRcBalanceAsync] RouteCard validation failed | Context: {context} | RcSubId: {refRcSubId}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustRcBalanceAsync] Unexpected error while adjusting RouteCard balance | Context: {context} | RcSubId: {refRcSubId}");
                throw new InvalidOperationException("Failed to adjust Route Card balance. Please contact support.");
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

        public async Task DeleteAndResequenceAsync(SubConDcOutSubVM subitem, SubConDcOutVM subConDcOutVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();
            bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();
            try
            {
                if (subitem.DcSubId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.SubConDCOutSubs.GetAsync(subitem.DcSubId);

                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");


                    if (subitem.TransType == "In")
                    {
                        if (!PoWiseDcOut)
                        {
                            if (subitem.RefPoSubId.GetValueOrDefault() > 0 && !subitem.IsOpenPo)
                            {
                                await AdjustPoSubBalanceAsync(subitem.RefPoSubId.Value, subitem.Qty.GetValueOrDefault(), 0, $"Subcontract Dc Item Deleted - {subConDcOutVM.DcNo}");
                            }
                        }
                        if (subitem.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(subitem.RcSubId.Value, subitem.Qty.GetValueOrDefault(), 0, $"Subcontract Dc Cancelled - {subConDcOutVM.DcNo}");
                        }
                    }
                    else
                    {
                        await DeleteStockIssueAndTrackAsync(subitem.DcSubId, subitem.ItemId.Value, screenCode);
                    }

                    // Delete from DB
                    await _unitOfWork.SubConDCOutSubs.DeleteAsync(entity.DcSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Sub Contract Outgoing DC",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"DcNo No: {subConDcOutVM?.DcNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    subConDcOutVM.SubConDcOutSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.DcId == subConDcOutVM.DcId)
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


        //Auot Generated Purchase SCN

        public async Task<(List<SubConDcOutVM> dcout, int totalCount)> GetPagedDcSubConOutAsync(int pageNumber, int pageSize, string search)
        {

            var query = _unitOfWork.SubConDCOuts
                        .GetQueryable()
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(q => q.SubConDcOutSubs)
                        //.Include(q => q.Vendor)
                        .Include(q => q.StoreIssue);

            // ✅ Get total count (fast, SQL COUNT(*))
            int totalCount = await query.CountAsync();

            // ✅ Fetch only required records with pagination
            var entities = await query
                .OrderByDescending(i => i.DcId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ Map in-memory (fast, avoids huge SQL)
            var data = _mapper.Map<List<SubConDcOutVM>>(entities);

            return (data, totalCount);
        }



        public async Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId)
        {
            return await (from i in _unitOfWork.ItemRepositories.GetQueryable().AsNoTracking()
                          join a in _unitOfWork.AssmblyDefs.GetQueryable().AsNoTracking()
                          on i.ItemId equals a.ItemId
                          where a.AssmblyID == assemblyId
                          select new AssemblyDefVM
                          {
                              ItemId = i.ItemId,
                              PartItemCode = i.ItemCode,
                              PartItemName = i.ItemName,
                              UtilQty = a.UtilQty,
                              UOM = i.MeasureUnit,


                          }
            ).ToListAsync();
        }

        public async Task<CompMasterVM?> GetDefaultRawMaterialItemAsync(int compItemId)
        {
            try
            {
                var data = await _unitOfWork.CompMasters
                    .GetQueryable()
                    .Include(x => x.RawMaterial)
                    .Where(x => x.CompItemId == compItemId)
                    .OrderByDescending(x => x.IsDefaultRM)
                    .Select(x => new
                    {
                        CompWeight = x.Weight,
                        RM = x.RawMaterial
                    })
                    .FirstOrDefaultAsync();

                if (data == null || data.RM == null)
                    return null;

                return new CompMasterVM
                {
                    RMId = data.RM.ItemId,
                    RMCode = data.RM.ItemCode,
                    RMName = data.RM.ItemName,
                    MeasureUnit = data.RM.MeasureUnit,
                    Weight = data.CompWeight
                };
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while fetching Default RM Item");
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
        //                     p.PurchORSubCon == false &&
        //                     p.IsOpenPO == false && p.PurchPoShortClose == false &&
        //                     p.PurchPoSubs.Any(ps =>
        //                         (ps.ItemCancel == false || ps.ItemCancel == null) &&
        //                         ps.BalQty > 0
        //                     )
        //                 )
        //                 .CountAsync();

        //}

        ////////////////Cancel deletion Reveert
        public async Task<(bool CanDelete, string Message)> CanDeleteSubConDcOutgoing(int IssueId)
        {
            try
            {
                var PurchaseQuoteSubIds = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(s => s.DcId == IssueId)
                    .Select(s => s.DcSubId)
                    .ToListAsync();

                if (!PurchaseQuoteSubIds.Any())
                    return (true, "SubConDc Out can be safely deleted.");

                var PoSubs = await _unitOfWork.SubConGRNTracks.GetQueryable()
                            .Where(p => p.RefDCSubId.HasValue && PurchaseQuoteSubIds.Contains(p.RefDCSubId.Value))
                           .ToListAsync();

                if (PoSubs.Any())
                {
                    return (false, "DcSubContract Incoming already created. You cannot delete this SubConDcOut.");
                }

                var sums = await (from sub in _unitOfWork.SubConDCOutSubs.GetQueryable()
                                  join enq in _unitOfWork.SubConDCOuts.GetQueryable()
                                      on sub.DcId equals enq.DcId
                                  where enq.DcId == IssueId
                                  group sub by 1 into g
                                  select new
                                  {
                                      TotalQty = g.Sum(s => s.Qty),
                                      TotalBalQty = g.Sum(s => s.BalQty)
                                  })
                   .FirstOrDefaultAsync();


                bool hasPurchPo = sums != null && sums.TotalQty == sums.TotalBalQty;
                if (!hasPurchPo)
                    return (false, "Cannot delete this DcSubContractOutgoing  as a some transaction Made.");


                var Quote = await _unitOfWork.SubConDCOuts
                              .GetQueryable()
                              .Where(e => e.DcId == IssueId)
                              .Select(e => new
                              {
                                  e.DcId,
                                  e.Cancel, // Assuming "IsCancelled" or "Cancel" flag exists on main Enquiry
                                  SubItems = e.SubConDcOutSubs.Select(s => new
                                  {
                                      s.DcSubId,
                                      //s.item // Assuming cancel flag exists on sub-items too
                                  }).ToList()
                              })
                              .FirstOrDefaultAsync();


                if (Quote == null)
                    return (false, "Quotation not found.");

                if (Quote.Cancel)
                    return (false, "Main Quotation is already cancelled and cannot be deleted.");

                //if (Quote.SubItems.Any(s => s.ItemCancel))
                //    return (false, "Some Quotation items are cancelled and cannot be deleted.");

                //bool QuoteShortClose = await _unitOfWork.SubConDcOuts.GetQueryable()
                //   .Where(qs => qs.IssueId == IssueId)
                //   .Select(q => q.QuoteShortClose)
                //   .FirstOrDefaultAsync();

                //if (QuoteShortClose)
                //    return (false, "Purchase quotation is short closed. You cannot delete it.");

                if (Quote.SubItems.Any())
                    return (true, "Purchase Quotation can be safely deleted (no sub-items).");


                return (true, "Purchase Quote can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeletePurchaseQuote for QuoteId: {IssueId}");
                throw new Exception("Error checking PurchaseQuote delete eligibility", ex);
            }
        }
        public async Task<(bool CanDelete, string Message)> CanRemoveSubConDcOutAsync(int IssueId, int IssuedIdSub)
        {
            try
            {
                var QuoteSubIds = await _unitOfWork.SubConDCOuts
                                     .GetQueryable()
                                     .Where(s => s.DcId == IssueId)
                                     .SelectMany(s => s.SubConDcOutSubs.Select(sub => sub.DcSubId))
                                     .ToListAsync();

                if (!QuoteSubIds.Any())
                    return (true, "Subcontract Outgoing can be safely deleted.");


                bool Quotation = await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .AnyAsync(qs => QuoteSubIds.Contains(qs.RefDcSubId.Value));

                if (Quotation)
                    return (false, "Cannot delete this Subcontract GRN  as a some transaction Made.");

                var sums = await (
                                 from sub in _unitOfWork.SubConDCOutSubs.GetQueryable()
                                 where sub.DcSubId == IssuedIdSub
                                 group sub by 1 into g
                                 select new
                                 {
                                     TotalQty = g.Sum(s => (decimal?)s.Qty) ?? 0,
                                     TotalBalQty = g.Sum(s => (decimal?)s.BalQty) ?? 0
                                 }
                             ).FirstOrDefaultAsync();

                bool hasPurchPo = sums != null && sums.TotalQty == sums.TotalBalQty;

                if (!hasPurchPo)
                    return (false, "Cannot delete this Subcontract Outgoing as some transactions have been made.");

                var Quote = await _unitOfWork.SubConDCOuts
                               .GetQueryable()
                               .Where(e => e.DcId == IssueId)
                               .Select(e => new
                               {
                                   e.DcId,
                                   e.Cancel,
                                   //e.QuoteShortClose,
                                   SubItems = e.SubConDcOutSubs.Select(s => new
                                   {
                                       s.DcSubId,
                                       //s.ItemCancel
                                   }).ToList()
                               })
                               .FirstOrDefaultAsync();


                if (Quote == null)
                    return (false, "Subcontract Outgoing not found.");


                if (Quote.Cancel /*|| Quote.QuoteShortClose*/)
                    return (false, "Main Subcontract Outgoing is already cancelled Or Short Closed and cannot be deleted.");

                //if (Quote.SubItems.Any(s => s.ItemCancel))
                //    return (false, "Some Quotation items are cancelled and cannot be deleted.");


                if (Quote.SubItems.Any())
                    return (true, "Subcontract Outgoing can be safely deleted (no sub-items).");


                return (true, "Subcontract Outgoing can be safely deleted.");

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteEnquiryAsync for Quoteid: {IssueId}");
                throw new Exception("Error checking Purchase Quotation delete eligibility", ex);
            }
        }


        public async Task UpdateItemCancelAndAddorRevertAsync(
            SubConDcOutSubVM subItem,
            int screenCode)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {

                var existingSubconDc = await _unitOfWork.SubConDCOuts.GetAsync(subItem.DcId);
                if (existingSubconDc == null)
                    throw new InvalidOperationException("Subcontract Delivery Challan not found.");

                bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();

                var subentity = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.DcSubId == subItem.DcSubId);

                if (subentity == null)
                    throw new KeyNotFoundException($"Subitem with DcSubid {subItem.DcSubId} not found.");

                if (!subItem.ItemCancel && subItem.TransType == "In")
                {
                    if (!PoWiseDcOut)
                    {
                        if (subItem.RefPoSubId.GetValueOrDefault() > 0 && !subItem.IsOpenPo)
                        {
                            await ValidatePoBalanceBeforeRevertAsync(subentity);
                        }
                    }
                    if(subItem.RcSubId.GetValueOrDefault() > 0)
                    {
                        await ValidateRcBalanceBeforeRevertAsync(subentity);
                    }
                }

                subentity.ItemCancel = subItem.ItemCancel;
                subentity.ItemCancelReason = subItem.ItemCancelReason;

                await _unitOfWork.SubConDCOutSubs.UpdateAsync(subentity);
                await _unitOfWork.SaveAsync();

                bool isCancel = subItem.ItemCancel;

                // ================= CANCEL LOGIC =================

                if (isCancel)
                {
                    if (subentity.TransType == "In")
                    {
                        if (!PoWiseDcOut)
                        {
                            if (subentity.RefPoSubId.GetValueOrDefault() > 0 && !subentity.IsOpenPo)
                            {
                                await AdjustPoSubBalanceAsync(subentity.RefPoSubId.Value, subentity.Qty, 0, $"Subcontract Dc Item Cancelled - {existingSubconDc.DcNo}");
                            }
                        }
                        else if (subentity.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(subentity.RcSubId.Value, subentity.Qty, 0, $"Subcontract Dc Item Cancelled - {existingSubconDc.DcNo}");
                        }
                    }
                    else
                    {
                        await DeleteStockIssueAndTrackAsync(subentity.DcSubId, subentity.ItemId, screenCode);
                    }
                }
                else
                {
                    if (subentity.TransType == "In")
                    {
                        if (!PoWiseDcOut)
                        {
                            if (subentity.RefPoSubId.GetValueOrDefault() > 0 && !subentity.IsOpenPo)
                            {
                                await AdjustPoSubBalanceAsync(subentity.RefPoSubId.Value, 0, subentity.Qty, $"Subcontract Dc Itemn Reverted - {existingSubconDc.DcNo}");
                            }
                        }
                        else if (subentity.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(subentity.RcSubId.Value, 0, subentity.Qty, $"Subcontract Dc Item Reverted - {existingSubconDc.DcNo}");
                        }
                    }
                    else
                    {
                        if (subentity.RcSubId.GetValueOrDefault() > 0)
                        {
                            await IssueStockBySeqLogicAsync(subentity, existingSubconDc, screenCode);
                        }
                        else
                        {
                            await _stockManagerService.IssueOrUpdateStockAsync(subentity.ItemId, existingSubconDc.StoreIssId, subentity.Qty, subentity.UnitPrice,
                                subentity.BatchNo, screenCode, subentity.DcSubId, existingSubconDc.DcNo, existingSubconDc.DcDate, null, false);
                        }
                    }
                }


                await _unitOfWork.SaveAsync();

                // Update GRN tally/status
                await UpdateDcTallyStatusAsync(existingSubconDc.DcId);

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


        public async Task ValidatePurchaseEnquiryBalanceBeforeRevertAsync(SubConDcOutSubVM sub)
        {
            if (sub.RefPoSubId <= 0)
                return;

            var entity = await _unitOfWork.SubConDCOutSubs.GetAsync(sub.RefPoSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"Quote not found for RefEnqSubId: {sub.RefPoSubId}");

            if (entity.BalQty < sub.Qty)
            {
                throw new InvalidOperationException(
                    $"Cannot revert because Enquiry balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                );
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(SubConDcOutVM dcOutVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingDc = await _unitOfWork.SubConDCOuts.GetAsync(dcOutVM.DcId);
                if (existingDc == null)
                    throw new InvalidOperationException("Subcontract Delivery Challan not found.");

                bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();
                var subs = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(s => s.DcId == dcOutVM.DcId)
                    .ToListAsync();

                if (!dcOutVM.Cancel)
                {
                    foreach (var sub in subs)
                    {
                        if (sub.TransType == "In")
                        {
                            if(!PoWiseDcOut)
                            {
                                if (sub.RefPoSubId.GetValueOrDefault() <= 0 && !sub.IsOpenPo)
                                {
                                    await ValidatePoBalanceBeforeRevertAsync(sub);
                                }

                            }

                            if (sub.RcSubId.GetValueOrDefault() <= 0)
                            {
                                await ValidateRcBalanceBeforeRevertAsync(sub);
                            }
                        }
                    }
                }

                existingDc.Cancel = dcOutVM.Cancel;
                existingDc.CancelReason = dcOutVM.CancelReason;
                existingDc.CancelDate = dcOutVM.CancelDate;
                existingDc.CancelBy = dcOutVM.CancelBy;

                await _unitOfWork.SubConDCOuts.UpdateAsync(existingDc);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingDc.Cancel)
                    {
                        if(sub.TransType == "In")
                        {
                            if (!PoWiseDcOut)
                            {
                                if (sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.IsOpenPo)
                                {
                                    await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, sub.Qty, 0, $"Subcontract Dc Cancelled - {existingDc.DcNo}");
                                }
                            }
                           if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustRcBalanceAsync(sub.RcSubId.Value, sub.Qty, 0, $"Subcontract Dc Cancelled - {existingDc.DcNo}");
                            }
                        }
                        else
                        {
                            await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);
                        }
                    }
                    else
                    {
                        if (sub.TransType == "In")
                        {
                            if (!PoWiseDcOut)
                            {
                                if (sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.IsOpenPo)
                                {
                                    await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, 0, sub.Qty, $"Delivery Challan Reverted - {existingDc.DcNo}");
                                }
                            }
                           if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustRcBalanceAsync(sub.RcSubId.Value, 0, sub.Qty, $"Delivery Challan Reverted - {existingDc.DcNo}");
                            }
                        }
                        else
                        {
                            if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await IssueStockBySeqLogicAsync(sub, existingDc, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingDc.StoreIssId, sub.Qty, sub.UnitPrice,
                                    sub.BatchNo, screenCode, sub.DcSubId, existingDc.DcNo, existingDc.DcDate, null, false);
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

        public async Task UpdateDcTallyStatusAsync(int dcId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.DcId == dcId)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var dcOut = await _unitOfWork.SubConDCOuts.GetAsync(dcId);
                if (dcOut == null)
                    return;

                if (dcOut.ShortClose || dcOut.Cancel)
                    return;

                dcOut.DcTally = (totalBalQty == 0);

                await _unitOfWork.SubConDCOuts.UpdateAsync(dcOut);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateDcTallyStatusAsync] Error updating DcId:- {dcId}");
                throw new InvalidOperationException("Failed to update Delivery Challan Tally status. Please contact support.");
            }
        }

        public async Task<SubConDcOut> UpsertPurchaseQuoteShortCloseAsync(SubConDcOutVM PurchQuote)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                SubConDcOut entity;

                entity = await _unitOfWork.SubConDCOuts
                             .GetQueryable()
                             .Include(e => e.SubConDcOutSubs)
                             .FirstOrDefaultAsync(e => e.DcId == PurchQuote.DcId)
                             ?? throw new InvalidOperationException("Purchase Order not found.");

                _mapper.Map(PurchQuote, entity);

                var parentChanges = GetPropertyChanges(entity, PurchQuote);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                _mapper.Map(PurchQuote, entity);
                entity.ModifiedBy = currentUser;
                entity.ModifiedDate = now;

                await _unitOfWork.SubConDCOuts.UpdateAsync(entity);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
                await LogChangesAsync(changes, PurchQuote.DcId == 0 ? "Short Closed purchase Quotation Created" : "ReOpen the purchase quotation");

                // Return updated entity
                var savedEntity = await _unitOfWork.SubConDCOuts
                    .GetQueryable()
                    .Include(e => e.SubConDcOutSubs).ThenInclude(s => s.Item)
                    .Include(e => e.SubConDcOutSubs).ThenInclude(s => s.CostCenter)
                    .Include(e => e.Vendor)
                    .FirstOrDefaultAsync(e => e.DcId == entity.DcId);

                return _mapper.Map<SubConDcOut>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpsertPurchaseOrderShortCloseAsync for QuoteId: {PurchQuote.DcId}");
                throw;
            }

        }


        //------------**********Dc Details for EWAY *****-----------------------\\

        public async Task<List<EWayDocument>> GetSubConDcByVendorCodeAsync(int VendorCode)
        {
            try
            {
                var data = await _unitOfWork.SubConDCOuts.GetQueryable()
                    .AsNoTracking()
                    .Where(dc =>
                        dc.VendorCode == VendorCode &&
                        !string.IsNullOrEmpty(dc.DcNo) &&
                        (dc.EwayBillNumber == null || dc.EwayBillNumber == "0" || dc.EwayBillNumber == "")
                    )
                    .OrderBy(dc => dc.DcId)
                    .Select(dc => new EWayDocument
                    {
                        Id = dc.DcId,
                        DocNo = dc.DcNo + dc.Suffix,
                        Suffix = dc.Suffix,
                        DocDate = dc.DcDate,
                        CustName = dc.Vendor.VendorName
                    })
                    .ToListAsync();


                return data;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetSubConDcByVendorCodeAsync({VendorCode})");
                return new List<EWayDocument>();
            }
        }


        public async Task<List<SubConDcOutSub>> GetSubConDcSubDetailsByDcIdAsync(int dcId)
        {
            if (dcId <= 0)
                return new List<SubConDcOutSub>();

            try
            {
                var query = _unitOfWork.SubConDCOutSubs
                                       .GetQueryable()
                                       .Where(s => s.DcId == dcId)
                                       .OrderBy(s => s.SlNo)
                                       .AsNoTracking();

                var subs = await query.ToListAsync();

                return subs ?? new List<SubConDcOutSub>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error fetching SubConDcOutSub for DCId: {dcId}");
                return new List<SubConDcOutSub>();
            }
        }

        public async Task ValidatePoBalanceBeforeRevertAsync(SubConDcOutSub sub)
        {
            try
            {
                if (sub == null)
                    throw new ArgumentNullException(nameof(sub));

                if (sub.RefPoSubId.GetValueOrDefault() <= 0)
                    return;

                var entity = await _unitOfWork.PurchPoSubs.GetAsync(sub.RefPoSubId.Value);

                if (entity == null)
                    throw new InvalidOperationException(
                        $"Subcontract PO not found for RefPoSubId: {sub.RefPoSubId}");

                if (entity.BalQty < sub.Qty && !entity.ItemCancel)
                {
                    throw new InvalidOperationException(
                        $"Cannot revert because Subcontract PO balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error in ValidatePoBalanceBeforeRevertAsync | RefPoSubId: {sub?.RefPoSubId}");
                throw;
            }
        }

        public async Task ValidateRcBalanceBeforeRevertAsync(SubConDcOutSub sub)
        {
            try
            {
                if (sub == null)
                    throw new ArgumentNullException(nameof(sub));

                if (sub.RcSubId.GetValueOrDefault() <= 0)
                    return;

                var entity = await _unitOfWork.RouteCardSubs.GetAsync(sub.RcSubId.Value);

                if (entity == null)
                    throw new InvalidOperationException($"Routecard Item not found for RcSubId: {sub.RcSubId}");

                if (entity.BalQty < sub.Qty)
                {
                    throw new InvalidOperationException($"Cannot revert because RouteCard Item balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error in ValidateRcBalanceBeforeRevertAsync | RcSubId: {sub?.RcSubId}");
                throw;
            }
        }

        public async Task UpsertSubConDcShortCloseAsync(SubConDcOutVM subConDcOutVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingDc = await _unitOfWork.SubConDCOuts.GetAsync(subConDcOutVM.DcId);
                if (existingDc == null)
                    throw new InvalidOperationException("Delivery Challan not found.");

                existingDc.ShortClose = subConDcOutVM.ShortClose;

                await _unitOfWork.SubConDCOuts.UpdateAsync(existingDc);
                await _unitOfWork.SaveAsync();

                await UpdateDcTallyStatusAsync(subConDcOutVM.DcId);

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

        public async Task<bool> IsDocumentUploaded(int Dcid)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Sub-Contract DC-Out" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == Dcid);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }


        public async Task<List<SubContractDcPendingVM>> GetSubContractDcoutPendingList(string status)//Shankar
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<SubContractDcPendingVM>("Sp_GetSubContractDcoutPendingList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }


    }
}
