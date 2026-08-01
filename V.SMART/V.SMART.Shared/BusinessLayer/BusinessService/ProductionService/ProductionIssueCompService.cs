using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Vml.Office;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ProductionService
{
    public class ProductionIssueCompService : IProductionIssueCompService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManagerService;

        private readonly IReportExecutor _report;

        public ProductionIssueCompService(
                 IUnitOfWork unitOfWork,
                 ICommonService commonService,
                 CurrentUserService userService,
                 IStockManagerService stockManagerService,
                 ILoggingService logs,
                 IMapper mapper, IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;
            _report = report;

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

        //Stores
        public async Task<List<Store>> GetAllActiveStoresAsync()
        {
            var result = await _commonService.GetAllIssueStoresAsync();
            return result.ToList();
        }

        //Machines
        public Task<List<Machine>> GetAllActiveMachinesAsync()
            => _commonService.GetAllMachineAsync();

        //Stock
        public async Task<decimal> GetStockForItemsAsync(int itemId, int storeId)
        {
            return await _stockManagerService.GetStockForItemAsync(itemId, storeId);
        }

        public async Task<bool> IsPOWiseProductionDcOutEnabledAsync()
       => await _commonService.GetScreenPermissionsAsync("Production Issue WO Component", "PO Wise Production Issue WO Component");

        public async Task<bool> GetIsSameItemasInAsync()
           => await _commonService.GetScreenPermissionsAsync("Production Issue WO Component", "Restrict Out Item Same As In Item");
        public async Task<bool> GetIsLabourGrnLinkAsync()
           => await _commonService.GetScreenPermissionsAsync("Production Issue WO Component", "Labour GRN Link With Production Component");
        //Material Issue Operations
        public async Task<List<ItemVM>> SearchAssemblyItemsAsync(string value)
        {
            return await _unitOfWork.ItemRepositories
                .GetQueryable()
                .Include(x=>x.Category)
                .Where(x =>
                    x.ItemName.Contains(value)
                    || x.ItemCode.Contains(value))
                .Where(x =>
                    x.Category.CategoryName == "ASSEMBLY"
                    || x.Category.CategoryName == "SUB-ASSEMBLY")
                .Take(50)
                .Select(x => new ItemVM
                {
                    ItemId = x.ItemId,
                    ItemCode = x.ItemCode,
                    ItemName = x.ItemName,
                    MeasureUnit = x.MeasureUnit,
                    CategoryCode=x.CategoryCode,
                    CategoryName=x.Category.CategoryName,

                })
                .ToListAsync();
        }
        public async Task<decimal> GetMfgPoProdCompBalQtyFromPoSubId(int poSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(e => e.PoSubId == poSubId)
                    .Select(e => e.ProdCompBalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for PoSubId: {poSubId}");
                throw new InvalidOperationException("Failed to retrieve Purchase Po balance quantity.");
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

        public async Task<ProductionIssueCompSubVM?> GetProductionIssueSubItemDetailByIssueSubIdAsync(int issueSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(q => q.IssueSubId == issueSubId)
                    .Select(q => new ProductionIssueCompSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching production sub item detail for IssueSubId: {issueSubId}");
                throw new InvalidOperationException("Failed to retrieve production sub-item details.");
            }
        }

        public async Task<List<ProductionIssueCompSubVM>> GetDistinctRefRouteCardByIssueIdAsync(int issueId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(s => s.IssueId == issueId && s.ComponentRouteCardSub != null)
                    .Select(s => s.ComponentRouteCardSub.RouteCard)
                    .Where(rc => rc != null)
                    .GroupBy(rc => new { rc.RCNo, rc.Suffix, rc.RCDate })
                    .Select(g => new ProductionIssueCompSubVM
                    {
                        RcNo = g.Key.RCNo + g.Key.Suffix,
                        RcDate = g.Key.RCDate
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error Occured in GetDistinctRefRouteCardByIssueIdAsync()");
                return new List<ProductionIssueCompSubVM>();
            }
        }
        public async Task<ProductionIssueCompVM?> GetProductionByIssueIdAsync(int issueId)
        {
            try
            {
                var entity = await _unitOfWork.ProductionIssueComps.GetQueryable()
                    .Include(q => q.StoreIssue)
                    .Include(q => q.Customer)
                    .Include(q => q.ProductionIssueCompSubs)
                    .ThenInclude(s => s.Item)
                    .Include(q => q.ProductionIssueCompSubs)
                    .ThenInclude(s => s.CostCenter)
                    .Include(q => q.ProductionIssueCompSubs)
                    .ThenInclude(s => s.MfgPoSub)
                    .ThenInclude(s => s.MfgPo)
                    .Include(q => q.ProductionIssueCompSubs)
                    .ThenInclude(s => s.ComponentRouteCardSub)
                    .ThenInclude(s => s.RouteCard)
                    .Include(q => q.ProductionIssueCompSubs)
                    .ThenInclude(s => s.Process)
                    .Include(q => q.ProductionIssueCompSubs)
                    .ThenInclude(s => s.Machine)
                    .FirstOrDefaultAsync(q => q.IssueId == issueId);

                if (entity == null)
                    return null;

                var vm = _mapper.Map<ProductionIssueCompVM>(entity);

                var itemIds = vm.ProductionIssueCompSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && vm.StoreIssId.HasValue)
                {
                    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, vm.StoreIssId.Value);

                    foreach (var sub in vm.ProductionIssueCompSubVMs)
                    {
                        if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                            sub.StockQty = qty;
                        else
                            sub.StockQty = 0m;
                    }
                }
                return vm;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetProductionByIssueIdAsync({issueId})");
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
                var lastGrn = await _unitOfWork.ProductionIssueComps
                            .GetQueryable()
                            .Where(q => q.Suffix == suffix)
                            .OrderByDescending(q => Convert.ToInt32(q.IssueNo))
                            .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastGrn != null)
                {
                    var parts = lastGrn.IssueNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Material Issue number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Material Issue number.");
            }
        }
        public async Task<List<Dictionary<string, object>>> GetAllOpenRcAsync(int storeId, int? custId)
        {
            try
            {
                // =============================
                // BASE QUERY
                // =============================
                var query = _unitOfWork.RouteCards.GetQueryable()
                    .Include(r => r.CostCenter)
                    .Include(r => r.RouteCardSubs)
                        .ThenInclude(s => s.IncomingItem)
                    .Include(r => r.RouteCardSubs)
                        .ThenInclude(s => s.OutgoingItem)
                    .Include(r => r.RouteCardSubs)
                        .ThenInclude(s => s.Machine)
                    .Include(r => r.RouteCardSubs)
                        .ThenInclude(s => s.Process)
                    .Where(r => r.RcStatus < 2);

                // =============================
                // APPLY CUSTOMER FILTER
                // =============================
                if (custId.HasValue && custId > 0)
                {
                    query = query.Where(r => r.CustId == custId.Value);
                }

                var routeCards = await query.ToListAsync();

                var finalList = new List<Dictionary<string, object>>();

                // =============================
                // GET ALL ITEM IDS FOR STOCK RATE
                // =============================
                var allItemIds = routeCards
                    .SelectMany(r => r.RouteCardSubs)
                    .Where(s => s.ItemIdOut.HasValue && s.BalQty > 0)
                    .Select(s => s.ItemIdOut!.Value)
                    .Distinct()
                    .ToList();

                var stockRateDict =
                    await _stockManagerService.GetStockRateForItemsAsync(allItemIds, storeId);

                // =============================
                // MAIN LOOP
                // =============================
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

                        // =============================
                        // STOCK CALCULATION
                        // =============================
                        if (s.SeqNo == 1)
                        {
                            stockQty = await GetAvailableStockByItemIdAsync(
                                s.ItemIdOut.Value, storeId);
                        }
                        else
                        {
                            var sourceRcSubIds = await GetIssueSourceRCSubIdsAsync(
                                rc.RCId,
                                s.SeqNo ?? 0,
                                s.RCSubId);

                            foreach (var srcSubId in sourceRcSubIds)
                            {
                                var availableStock =
                                    await GetAvailableStockByItemIdAndRcAndScreenAsync(
                                        s.ItemIdOut.Value, storeId, srcSubId);

                                stockQty += availableStock;
                            }
                        }

                        // =============================
                        // UNIT PRICE
                        // =============================
                        var unitPrice = stockRateDict.TryGetValue(
                            s.ItemIdOut.Value, out var rate)
                            ? rate
                            : 0m;

                        // =============================
                        // QTY IN
                        // =============================
                        var qtyIn = s.SeqNo == 1 ? rc.RcQty : s.BalQty;

                        // =============================
                        //  QTY PER UNIT (NEW LOGIC)
                        // =============================
                        decimal qtyPerUnit = 0;

                        if (rc != null && rc.RcQty > 0)
                        {
                            qtyPerUnit = s.SeqNo == 1
                                ? rc.RMWeight
                                : 1;
                        }

                        // =============================
                        // FINAL RESULT
                        // =============================
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
                            ["QtyPerUnit"] = Math.Round(qtyPerUnit, 3),

                            ["StockQty"] = Math.Round(stockQty, 3),
                            ["UnitPrice"] = Math.Round(unitPrice, 2),

                            ["ProcessCost"] = s.ProcessCost,

                            ["CostCenterId"] = rc.CostId,
                            ["CostCenter"] = rc.CostCenter?.ProjectNo ?? ""
                        });
                    }
                }

                return finalList;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching open Route Card");
                throw new InvalidOperationException("Failed to retrieve open Route Card. Please try again.", ex);
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
                list.Where(x => x.ProcessStatus != 3)
                    .OrderBy(x => x.SeqNo)
                    .Select(x => x.SeqNo)
                    .FirstOrDefault()
                ??
                list.Where(x => x.BalQty > 0 && x.ProcessStatus != 3)
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


        public async Task<ProductionIssueCompVM> UpsertProdIssueComp(ProductionIssueCompVM issueVM, int screenCode)
        {
            if (issueVM == null)
                throw new ArgumentNullException(nameof(issueVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ProductionIssueComp entity;

                // =====================================================
                // CREATE
                // =====================================================
                if (issueVM.IssueId == 0)
                {
                    entity = _mapper.Map<ProductionIssueComp>(issueVM);

                    entity.IssueNo = await _unitOfWork
                        .ProductionIssueComps
                        .GetLastIssueNoAsync(entity.Suffix);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.ProductionIssueCompSubs = issueVM
                        .ProductionIssueCompSubVMs
                        .Select(s => _mapper.Map<ProductionIssueCompSub>(s))
                        .ToList();

                    await _unitOfWork.ProductionIssueComps.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();


                    foreach (var sub in entity.ProductionIssueCompSubs)
                    {

                        if (sub.TransType == "In")
                        {
                            if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustRcBalanceAsync(sub.RcSubId, 0, sub.Qty.GetValueOrDefault(), "Subcontract DC Creation");
                            }

                            if (!PoWiseDcOut && !sub.RefGRNSubId.HasValue)
                            {
                                if (sub.RefPoSubId.GetValueOrDefault() > 0)
                                {
                                    await AdjustPoSubBalanceAsync(sub.RefPoSubId.GetValueOrDefault(), 0, sub.Qty ?? 0, "Production Issue Component Creation");
                                }
                            }
                        }
                        else
                        {
                            if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await IssueStockBySeqLogicAsync(sub, entity, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(
                                       sub.ItemId.Value,
                                       entity.StoreIssId,
                                       sub.Qty.GetValueOrDefault(),
                                       sub.UnitPrice,
                                       null,
                                       screenCode,
                                       sub.IssueSubId,
                                       entity.IssueNo,
                                       entity.IssueDate,
                                       allowMultipleIssue: false);
                            }
                        }

                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGrnOutBalanceAsync(sub.RefGRNSubId.GetValueOrDefault(), 0, sub.Qty ?? 0, "Labour Dc Create");
                        }


                    }

                    changes.AppendLine("Production Issue Component Created.");
                }

                // =====================================================
                // UPDATE
                // =====================================================
                else
                {
                    entity = await _unitOfWork.ProductionIssueComps.GetQueryable()
                        .Include(x => x.ProductionIssueCompSubs)
                        .ThenInclude(x => x.ComponentRouteCardSub)
                        .FirstOrDefaultAsync(x => x.IssueId == issueVM.IssueId)
                        ?? throw new InvalidOperationException("Production Issue Component not found.");

                    var parentChanges = GetPropertyChanges(entity, issueVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(issueVM, entity);

                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await _unitOfWork.SaveAsync();

                    await HandleChildUpdatesAsync(entity,issueVM.ProductionIssueCompSubVMs,changes,screenCode);

                    changes.AppendLine("Production Issue Component Updated.");
                }

                await LogChangesAsync(
                    changes,
                    issueVM.IssueId == 0
                        ? "Production Issue Component Created"
                        : "Production Issue Component Updated");

                await transaction.CommitAsync();

                var savedEntity = await _unitOfWork.ProductionIssueComps.GetQueryable()
                    .Include(x => x.StoreIssue)
                    .Include(x => x.ProductionIssueCompSubs)
                        .ThenInclude(x => x.Item)
                    .FirstOrDefaultAsync(x => x.IssueId == entity.IssueId);

                return _mapper.Map<ProductionIssueCompVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed to upsert Production Issue Component");
                throw;
            }
        }



        private async Task HandleChildUpdatesAsync(ProductionIssueComp existingIssue, List<ProductionIssueCompSubVM> incomingSubVMs,StringBuilder changes, int screenCode)
        {
            var existingSubs = existingIssue.ProductionIssueCompSubs.ToList();
            var incomingIds = incomingSubVMs.Select(x => x.IssueSubId).ToHashSet();
            bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
            // =====================================================
            // DELETE
            // =====================================================
            foreach (var sub in existingSubs.Where(x => !incomingIds.Contains(x.IssueSubId)))
            {

                if (sub.TransType == "In")
                {
                    if (sub.RcSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustRcBalanceAsync(sub.RcSubId, sub.Qty ?? 0, 0, "Production Issue Component Delete");
                    }

                    if (!PoWiseDcOut && !sub.RefGRNSubId.HasValue)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustPoSubBalanceAsync(sub.RefPoSubId.GetValueOrDefault(), sub.Qty ?? 0, 0, "Production Issue Component deleteion");
                        }
                    }

                }
                else
                {
                    await DeleteStockIssueAndTrackAsync(sub.IssueSubId, sub.ItemId.Value, screenCode);
                }

                if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                {
                    await AdjustGrnOutBalanceAsync(sub.RefGRNSubId.GetValueOrDefault(), sub.Qty ?? 0, 0, "Production Issue Delete");
                }


                await _unitOfWork.ProductionIssueCompSubs.DeleteAsync(sub.IssueSubId);
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
                if (subVM.IssueSubId == 0)
                {
                    var newSub = _mapper.Map<ProductionIssueCompSub>(subVM);
                    newSub.IssueId = existingIssue.IssueId;

                    await _unitOfWork.ProductionIssueCompSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    if (newSub.TransType == "In")
                    {
                        if (newSub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(newSub.RcSubId, 0, newSub.Qty.GetValueOrDefault(), "Subcontract DC Creation");
                        }

                        if (!PoWiseDcOut && !newSub.RefGRNSubId.HasValue)
                        {
                            if (newSub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(newSub.RefPoSubId.GetValueOrDefault(), 0, newSub.Qty ?? 0, "Production Issue Component Creation");
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
                            await _stockManagerService.IssueOrUpdateStockAsync(
                                   newSub.ItemId.Value,
                                   existingIssue.StoreIssId,
                                   newSub.Qty.GetValueOrDefault(),
                                   newSub.UnitPrice,
                                   null,
                                   screenCode,
                                   newSub.IssueSubId,
                                   existingIssue.IssueNo,
                                   existingIssue.IssueDate,
                                   allowMultipleIssue: false);
                        }
                    }

                    if (newSub.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustGrnOutBalanceAsync(newSub.RefGRNSubId.GetValueOrDefault(), 0, newSub.Qty ?? 0, "Production Issue Delete");
                    }

                    changes.AppendLine($"Child Added - Item: {newSub.ItemId}");
                }

                // -------------------------
                // UPDATE
                // -------------------------
                else
                {
                    var existingSub = existingSubs
                        .FirstOrDefault(x => x.IssueSubId == subVM.IssueSubId);

                    if (existingSub == null)
                        continue;

                    if (existingSub.TransType == "In")
                    {
                        if (existingSub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(existingSub.RcSubId, existingSub.Qty??0, subVM.Qty.GetValueOrDefault(), "Subcontract DC Update");
                        }

                        if (!PoWiseDcOut && !subVM.RefGRNSubId.HasValue)
                        {
                            if (existingSub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(subVM.RefPoSubId, existingSub.Qty??0, subVM.Qty.GetValueOrDefault(), "Subcontract DC Update");
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
                            subVM.BatchNo, screenCode, subVM.IssueSubId, existingIssue.IssueNo, existingIssue.IssueDate, null, false);
                        }
                    }
                    if (existingSub.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustGrnOutBalanceAsync(existingSub.RefGRNSubId.GetValueOrDefault(), existingSub.Qty ?? 0, subVM.Qty ?? 0, "Production Issue Update");
                    }


                    changes.AppendLine($"Child Updated - Item: {existingSub.ItemId}");
                    if (existingSub == null)
                        continue;

                    _mapper.Map(subVM, existingSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Updated - Item: {existingSub.ItemId}");
                }
            }
        }
        private async Task AdjustGrnOutBalanceAsync(int? refGRnSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refGRnSubId.HasValue || refGRnSubId == 0)
                    return;

                var grnOutSubs = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNSubId == refGRnSubId)
                    .OrderBy(x => x.GRNId)
                    .ThenBy(x => x.SlNo)
                    .ToListAsync();

                if (!grnOutSubs.Any())
                    return;

                var PoSub = await _unitOfWork.LabourGRNSubs.GetAsync(refGRnSubId.Value);
                if (PoSub == null) return;

                if (oldQty > 0)
                    PoSub.ProdCompBalQty += oldQty;

                if (newQty > PoSub.ProdCompBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed PO ProdCompBalQty.");

                if (newQty > 0)
                    PoSub.ProdCompBalQty -= newQty;


                await _unitOfWork.LabourGRNSubs.UpdateRangeAsync(grnOutSubs);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustGRNBalance] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustGRNBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust GRN balance. Please contact support.");
            }
        }

        private async Task IssueStockBySeqLogicAsync(ProductionIssueCompSub sub,ProductionIssueComp parent,int screenCode)
        {
            decimal remainingQty = sub.Qty ?? 0;
            if (remainingQty <= 0)
                return;

            // =====================================================
            // SEQ = 1
            // =====================================================
            if (sub.ComponentRouteCardSub.SeqNo == 1)
            {
                await _stockManagerService.IssueOrUpdateStockAsync(
                    sub.ItemId.Value,
                    parent.StoreIssId,
                    remainingQty,
                    sub.UnitPrice,
                    null,
                    screenCode,
                    sub.IssueSubId,
                    parent.IssueNo,
                    parent.IssueDate,
                    allowMultipleIssue: false);

                return;
            }

            // =====================================================
            // SEQ > 1 → RESET FIRST
            // =====================================================
            var existingIssues = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(x =>
                    x.SubItemRefID == sub.IssueSubId &&
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
                    sub.ItemId.Value,
                    parent.StoreIssId,
                    rcSubId);

                if (available <= 0)
                    continue;

                var issueQty = Math.Min(available, remainingQty);

                await _stockManagerService.IssueOrUpdateStockAsync(
                    sub.ItemId.Value,
                    parent.StoreIssId,
                    issueQty,
                    sub.UnitPrice,
                    null,
                    screenCode,
                    sub.IssueSubId,
                    parent.IssueNo,
                    parent.IssueDate,
                    rcSubId,
                    allowMultipleIssue: false);

                remainingQty -= issueQty;
            }

            if (remainingQty > 0)
                throw new InvalidOperationException(
                    $"Insufficient RC stock after adjustment. Remaining Qty: {remainingQty}");
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


        public async Task<(List<ProductionIssueCompVM> issueCompVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ProductionIssueComps.GetQueryable()
                .Include(j => j.StoreIssue)
                .Include(j => j.ProductionIssueCompSubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.ProductionIssueCompSubs)
                    .ThenInclude(s => s.ComponentRouteCardSub)
                .Include(j => j.ProductionIssueCompSubs)
                    .ThenInclude(s => s.CostCenter)
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
                .OrderByDescending(x => x.IssueId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<ProductionIssueCompVM>>(list);

            return (vmList, total);
        }

        public static class MaterialIssueFilterBuilder
        {
            public static IQueryable<ProductionIssueComp> ApplyFilter(
                IQueryable<ProductionIssueComp> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "IssueNo":
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
                                (string.IsNullOrEmpty(part1) || x.IssueNo.StartsWith(part1)) &&
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
                                x.ProductionIssueCompSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.ComponentRouteCardSub.RouteCard.RCNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.ComponentRouteCardSub.RouteCard.Suffix.Contains(part2))
                                )
                            );
                        }


                    case "ItemCode":
                        return query.Where(x => x.ProductionIssueCompSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.ProductionIssueCompSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.IssueDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.IssueDateNow <= toDate);
                        break;
                }

                return query;
            }

            private static IQueryable<ProductionIssueComp> ApplyStatusFilter(
                IQueryable<ProductionIssueComp> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.IssueTally == true),
                    "Pending" => query.Where(x => x.IssueTally == false),
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
                var productionIssueAssy = await _unitOfWork.ProductionIssueComps
                    .GetQueryable()
                    .Include(e => e.ProductionIssueCompSubs)
                    .FirstOrDefaultAsync(e => e.IssueId == issueId);

                if (productionIssueAssy == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in productionIssueAssy.ProductionIssueCompSubs)
                {

                    if (sub.TransType == "In")
                    {
                        if (sub.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(sub.RcSubId.GetValueOrDefault(), sub.Qty.Value, 0, "Production Issue Component");
                        }


                        bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();

                        if (!PoWiseDcOut && !sub.RefGRNSubId.HasValue)
                        {
                            if (sub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(sub.RefPoSubId.GetValueOrDefault(), sub.Qty.Value, 0, "Production Issue Component");
                            }
                        }


                    }
                    else
                    {
                        await DeleteStockIssueAndTrackAsync(sub.IssueSubId, sub.ItemId.Value, screenCode);

                    }

                    if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustGrnOutBalanceAsync(sub.RefGRNSubId.GetValueOrDefault(), sub.Qty ?? 0, 0, "Labour Dc Delete");
                    }


                }

                var ProductionIssueComp = await _unitOfWork.ProductionIssueComps.GetAsync(issueId);
                await _unitOfWork.ProductionIssueComps.DeleteAsync(ProductionIssueComp);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Production Issue Component",
                    action: $"Deleted Issue no: {ProductionIssueComp.IssueNo}",
                    additionalInfo: $"Issue Id: {ProductionIssueComp.IssueId}\n{changes}"
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

        public async Task<(bool CanDelete, string Message)> CanDeleteProductionDcgoingAsync(int IssueId, int screenCode)
        {
            try
            {
                var subConDc = await _unitOfWork.ProductionIssueComps
                                .GetQueryable()
                                .Include(e => e.ProductionIssueCompSubs)
                                .Where(e => e.IssueId == IssueId).FirstOrDefaultAsync();

                if (subConDc == null)
                    return (true, "Production Material Issue can be safely deleted.");

                var dcSubIds = subConDc.ProductionIssueCompSubs
                    .Select(es => es.IssueSubId)
                    .ToList();

                bool hasGRN = await _unitOfWork.ProductionReturnCompSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefIssueSubId.HasValue &&
                        dcSubIds.Contains(qs.RefIssueSubId.Value));

                if (hasGRN)
                    return (false, "Cannot delete this Production Issue as a Production GRN exists.");

                var RcSubIds = subConDc.ProductionIssueCompSubs
                    .Select(es => es.RcSubId)
                    .ToList();

                bool hasRC = await _unitOfWork.RouteCardSubs
                             .GetQueryable()
                             .AnyAsync(qs => RcSubIds.Contains(qs.RCSubId));
                if (hasRC)
                    return (false, "Cannot delete this Production Issue as a RouteCard exists.");


                if (subConDc.Cancel || subConDc.ShortClose)
                    return (false, "Cannot delete this  Production Issue as it is Cancelled or Short-Closed.");

                return (true, " Production Issue can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteProductionDcgoingAsync for v: {IssueId}");
                return (false, "Unable to verify item CanDeleteProductionDcgoingAsync. Please try again or contact support.");
            }
        }


        public async Task<ProductionIssueCompVM?> GetDcSubConByIdAsync(int issueId)
        {
            try
            {
                var entity = await _unitOfWork.ProductionIssueComps.GetQueryable()
                            .AsNoTracking()
                            .AsSplitQuery()
                            .Include(q => q.ProductionIssueCompSubs)
                            .FirstOrDefaultAsync(q => q.IssueId == issueId);

                return _mapper.Map<ProductionIssueCompVM?>(entity);

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetDcSubConByIdAsync({issueId})");
                return null;
            }
        }

        public async Task<int> GetPendingPoCountAsync(int CustId)
        {
            //return await _unitOfWork.MfgPos
            //             .GetQueryable()
            //             .Where(p =>
            //                 p.CustId == CustId &&
            //                 p.PoTally == false &&
            //                 (p.PoCancl == false || p.PoCancl == null) &&
            //                 p.Authorized == true &&
            //                 p.PurchORSubCon == false &&
            //                 p.IsOpenPO == false && p.PurchPoShortClose == false &&
            //                 p.PurchPoSubs.Any(ps =>
            //                     (ps.ItemCancel == false || ps.ItemCancel == null) &&
            //                     ps.BalQty > 0
            //                 )
            //             )
            //             .CountAsync();
            return 0;

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

        public async Task<ProductionIssueCompSubVM?> GetSubItemDetailByIssueIdAsync(int issueSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(q => q.IssueSubId == issueSubId)
                    .Select(q => new ProductionIssueCompSubVM
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

        public async Task<List<ProductionIssueCompSubVM>> GetDcSubConByDcidAsync(int issueId)
        {
            try
            {
                var subs = await _unitOfWork.ProductionIssueCompSubs
                                .GetQueryable()
                                .Where(s => s.IssueId == issueId)
                                .OrderBy(s => s.SlNo)
                                .ProjectTo<ProductionIssueCompSubVM>(_mapper.ConfigurationProvider)
                                .AsNoTracking()
                                .ToListAsync();
                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Purchase GRN items for IssueId: {issueId}");
                throw new InvalidOperationException("Failed to retrieve Purchase GRN sub-items. Please try again.");
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
                        from qs in _unitOfWork.ProductionIssueCompSubs.GetQueryable()
                        join q in _unitOfWork.ProductionIssueComps.GetQueryable()
                            on qs.IssueId equals q.IssueId
                        where qs.ItemId == itemId
                        select new { qs.UnitPrice, q.CustId, q.IssueId };

                    if (custId.HasValue)
                        issueQuery = issueQuery.Where(x => x.CustId == custId);

                    rate = await issueQuery
                        .OrderByDescending(x => x.IssueId)
                        .Select(x => x.UnitPrice)
                        .FirstOrDefaultAsync();

                    // 2️⃣ Last Issue price (ANY customer)
                    if (rate == 0)
                    {
                        rate = await _unitOfWork.ProductionIssueCompSubs.GetQueryable()
                            .Where(x => x.ItemId == itemId)
                            .OrderByDescending(x => x.IssueSubId)
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



        public async Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId, int? storeId)
        {
            try
            {
                // 1️⃣ Fetch PO data
                var poData = await (
                    from p in _unitOfWork.MfgPos.GetQueryable()
                    join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                        on p.PoId equals ps.PoId
                    where p.CustId == custId
                          && !p.PoTally
                          && !p.PoCancl
                          && !ps.ItemCancel
                          && ps.ProdCompBalQty > 0
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
                        ps.Item.CategoryCode,
                        ps.Item.Category.CategoryName,

                        ps.ProdCompBalQty,
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
                        ["Qty"] = r.ProdCompBalQty,
                        ["BalQty"] = r.ProdCompBalQty,
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
                    $"Error fetching PO details for CustId: {custId}, StoreId: {storeId}"
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



        private async Task DeleteStockIssueAndTrackAsync(int tcIssueSubId, int itemId, int screenCode)
        {
            var issueId = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(s => s.SubItemRefID == tcIssueSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.IssueId)
                .FirstOrDefaultAsync();

            if (issueId > 0)
                await _stockManagerService.DeleteStockIssueAsync(issueId);

            await _unitOfWork.SaveAsync();
        }

        private async Task AdjustRcBalanceAsync(int? refRcSubId,decimal oldQty,decimal newQty,string context)
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

                routeCardSub.ProcessStatus =await GetProcessStatusAsync(routeCardSub);

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardSub);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex,$"[AdjustRcBalanceAsync] RouteCard validation failed | Context: {context} | RcSubId: {refRcSubId}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"[AdjustRcBalanceAsync] Unexpected error while adjusting RouteCard balance | Context: {context} | RcSubId: {refRcSubId}");
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

        private async Task DeleteStockAddAsync(int GRNSubId, int itemId, int screenCode, string refNo)
        {
            var addIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == GRNSubId && s.ItemId == itemId && s.ScreenCode == screenCode && s.RefNo == refNo)
                .Select(s => s.AddId)
                .ToListAsync();

            foreach (var addId in addIds)
            {
                if (addId > 0)
                    await _stockManagerService.DeleteStockAddAsync(addId);
            }
            //await _unitOfWork.SaveAsync();
        }

   
    

        public async Task DeleteAndResequenceAsync(ProductionIssueCompSubVM subitem, ProductionIssueCompVM compIssueVM,int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();
            bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
            try
            {
                if (subitem.IssueSubId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.ProductionIssueCompSubs.GetAsync(subitem.IssueSubId);

                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");


                    if (subitem.TransType == "In")
                    {
                        if (!PoWiseDcOut)
                        {
                            if (subitem.RefPoSubId.GetValueOrDefault() > 0 )
                            {
                                await AdjustPoSubBalanceAsync(subitem.RefPoSubId.Value, subitem.Qty.GetValueOrDefault(), 0, $"Production Issue Item Deleted - {compIssueVM.IssueNo}");
                            }
                        }
                        if (subitem.RcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcBalanceAsync(subitem.RcSubId.Value, subitem.Qty.GetValueOrDefault(), 0, $"Production Issue Cancelled - {compIssueVM.IssueNo}");
                        }
                    }
                    else
                    {
                        await DeleteStockIssueAndTrackAsync(subitem.IssueSubId, subitem.ItemId.Value, screenCode);
                    }

                    // Delete from DB
                    await _unitOfWork.ProductionIssueCompSubs.DeleteAsync(entity.IssueSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Production Comp Material Issue",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"IssueNo No: {compIssueVM?.IssueNo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    compIssueVM.ProductionIssueCompSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.IssueId == compIssueVM.IssueId)
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


        public async Task<(List<ProductionIssueCompVM> dcout, int totalCount)> GetPagedDcSubConOutAsync(int pageNumber, int pageSize, string search)
        {

            var query = _unitOfWork.ProductionIssueComps
                        .GetQueryable()
                        .AsNoTracking()
                        .AsSplitQuery()
                        .Include(q => q.ProductionIssueCompSubs)
                        //.Include(q => q.Vendor)
                        .Include(q => q.StoreIssue);

            // ✅ Get total count (fast, SQL COUNT(*))
            int totalCount = await query.CountAsync();

            // ✅ Fetch only required records with pagination
            var entities = await query
                .OrderByDescending(i => i.IssueId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // ✅ Map in-memory (fast, avoids huge SQL)
            var data = _mapper.Map<List<ProductionIssueCompVM>>(entities);

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
                    UtilQty = a.UtilQty
                }
            ).ToListAsync();
        }

        public async Task<List<ProductionIssueCompStatusVM>> GetProductionIssueComponentStatusListAsync(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<ProductionIssueCompStatusVM>("Sp_GetProductionIssueCompStatusList", status);
                return result.ToList();


            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<List<BOMAssemblySpVM>> GetBOMAssemblyItems(int assId,int processId, int storeId)
        {
            try
            {
                var result = await _report.ExecuteAsync<BOMAssemblySpVM>(
                   "GetBOMAssemblyItems",

                   new SqlParameter("@AssId", assId),

                   new SqlParameter("@ProcessId", processId),

                   new SqlParameter("@StoreId ", storeId)
                  
               );
                return result.ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"GetBOMAssemblyItems Error: {ex.Message}", ex);
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
                    var PoSub = await _unitOfWork.MfgPoSubs.GetAsync(refPoSubId.Value);
                    if (PoSub == null) return;

                    if (oldQty > 0)
                        PoSub.ProdCompBalQty += oldQty;

                    if (newQty > PoSub.ProdCompBalQty)
                        throw new InvalidOperationException($"{context}: Qty cannot exceed PO ProdCompBalQty.");

                    if (newQty > 0)
                        PoSub.ProdCompBalQty -= newQty;

                    await _unitOfWork.MfgPoSubs.UpdateAsync(PoSub);
                    await _unitOfWork.SaveAsync();

                    var totalBalQty = await _unitOfWork.MfgPoSubs
                        .GetQueryable()
                        .Where(e => e.PoId == PoSub.PoId && !e.ItemCancel)
                        .SumAsync(e => e.ProdCompBalQty);

                    var po = await _unitOfWork.MfgPos.GetAsync(PoSub.PoId);
                    if (po != null)
                    {
                        //po.PoTally = (totalBalQty == 0);
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

        public async Task<bool> IsBOMAsync(int rcSubId, int processId)
        {
            try
            {
                return await _unitOfWork.RouteCardSubs.GetQueryable()
                    .AnyAsync(x =>
                        x.RCSubId == rcSubId &&
                        x.ProcessId == processId &&
                        x.IsBOM);
            }
            catch (Exception ex)
            {
                throw new Exception($"IsBOMAsync Error: {ex.Message}", ex);
            }
        }
        public async Task UpdatedCancelStatusAndAddOrRevertQty(ProductionIssueCompVM dcOutVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingDc = await _unitOfWork.ProductionIssueComps.GetAsync(dcOutVM.IssueId);
                if (existingDc == null)
                    throw new InvalidOperationException("Subcontract Delivery Challan not found.");

                bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
                bool IslabGrnLinkEnabled = await GetIsLabourGrnLinkAsync();
                var subs = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(s => s.IssueId == dcOutVM.IssueId)
                    .ToListAsync();

                if (!dcOutVM.Cancel)
                {
                    foreach (var sub in subs)
                    {
                        if (sub.TransType == "In")
                        {
                            if (!PoWiseDcOut && !sub.RefGRNSubId.HasValue && !IslabGrnLinkEnabled)
                            {
                                if (sub.RefPoSubId.GetValueOrDefault() > 0 )
                                {
                                    await ValidatePoBalanceBeforeRevertAsync(sub);
                                }

                            }

                            if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await ValidateRcBalanceBeforeRevertAsync(sub);
                            }
                            if(IslabGrnLinkEnabled && sub.RefGRNSubId.GetValueOrDefault()>0)
                            {
                                await ValidateLabGrnBalanceBeforeRevertAsync(sub);
                            }
                        }
                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGrnOutBalanceAsync(sub.RefGRNSubId.GetValueOrDefault(), sub.Qty ?? 0, 0, "Labour Dc Create");
                        }
                    }
                }

                existingDc.Cancel = dcOutVM.Cancel;
                existingDc.CancelReason = dcOutVM.CancelReason;
                existingDc.CancelDate = dcOutVM.CancelDate;
                existingDc.CancelBy = dcOutVM.CancelBy;

                await _unitOfWork.ProductionIssueComps.UpdateAsync(existingDc);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingDc.Cancel)
                    {
                        if (sub.TransType == "In")
                        {
                            if (!PoWiseDcOut && !sub.RefGRNSubId.HasValue)
                            {
                                if (sub.RefPoSubId.GetValueOrDefault() > 0 )
                                {
                                    await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, sub.Qty??0, 0, $"ProductionComp Cancelled - {existingDc.IssueNo}");
                                }
                            }
                            if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustRcBalanceAsync(sub.RcSubId.Value, sub.Qty??0, 0, $"Production Comp Cancelled - {existingDc.IssueNo}");
                            }
                        }
                        else
                        {
                            await DeleteStockIssueAndTrackAsync(sub.IssueSubId, sub.ItemId.Value, screenCode);
                        }

                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGrnOutBalanceAsync(sub.RefGRNSubId.GetValueOrDefault(), sub.Qty ?? 0, 0, "Labour Dc Create");
                        }
                    }
                    else
                    {
                        if (sub.TransType == "In")
                        {
                            if (!PoWiseDcOut && !sub.RefGRNSubId.HasValue)
                            {
                                if (sub.RefPoSubId.GetValueOrDefault() > 0 && !sub.RefGRNSubId.HasValue )
                                {
                                    await AdjustPoSubBalanceAsync(sub.RefPoSubId.Value, 0, sub.Qty ?? 0, $"Delivery Challan Reverted - {existingDc.IssueNo}");
                                }
                            }
                            if (sub.RcSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustRcBalanceAsync(sub.RcSubId.Value, 0, sub.Qty ?? 0, $"Delivery Challan Reverted - {existingDc.IssueNo}");
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
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId??0, existingDc.StoreIssId, sub.Qty ?? 0, sub.UnitPrice,
                                    sub.BatchNo, screenCode, sub.IssueSubId, existingDc.IssueNo, existingDc.IssueDate, null, false);
                            }
                        }

                        if (sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustGrnOutBalanceAsync(sub.RefGRNSubId.GetValueOrDefault(), 0, sub.Qty ?? 0, "Labour Dc Create");
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

        public async Task ValidatePoBalanceBeforeRevertAsync(ProductionIssueCompSub sub)
        {
            try
            {
                if (sub == null)
                    throw new ArgumentNullException(nameof(sub));

                if (sub.RefPoSubId.GetValueOrDefault() <= 0)
                    return;

                var entity = await _unitOfWork.MfgPoSubs.GetAsync(sub.RefPoSubId.Value);

                if (entity == null)
                    throw new InvalidOperationException(
                        $"Sales  PO not found for RefPoSubId: {sub.RefPoSubId}");

                if (entity.BalQty < sub.Qty && !entity.ItemCancel)
                {
                    throw new InvalidOperationException(
                        $"Cannot revert because Subcontract PO balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ValidatePoBalanceBeforeRevertAsync | RefPoSubId: {sub?.RefPoSubId}");
                throw;
            }
        }

        public async Task ValidateRcBalanceBeforeRevertAsync(ProductionIssueCompSub sub)
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
                await _logs.LogDeveloperError(ex, $"Error in ValidateRcBalanceBeforeRevertAsync | RcSubId: {sub?.RcSubId}");
                throw;
            }
        }

        public async Task ValidateLabGrnBalanceBeforeRevertAsync(ProductionIssueCompSub sub)
        {
            try
            {
                if (sub == null)
                    throw new ArgumentNullException(nameof(sub));

                if (sub.RefGRNSubId.GetValueOrDefault() <= 0)
                    return;

                var entity = await _unitOfWork.LabourGRNSubs.GetAsync(sub.RefGRNSubId.Value);

                if (entity == null)
                    throw new InvalidOperationException($"Labour GRN Item not found for RefGRNSubId: {sub.RefGRNSubId}");

                if (entity.ProdCompBalQty < sub.Qty)
                {
                    throw new InvalidOperationException($"Cannot revert because Labour GRN Item balance ({entity.ProdCompBalQty}) is less than required quantity ({sub.Qty}).");
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ValidateLabGrnBalanceBeforeRevertAsync |LabSubId: {sub?.RefGRNSubId}");
                throw;
            }
        }


        public async Task ProductionCompShortCloseAsync(ProductionIssueCompVM ProductionIssueCompVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingDc = await _unitOfWork.ProductionIssueComps.GetAsync(ProductionIssueCompVM.IssueId);
                if (existingDc == null)
                    throw new InvalidOperationException("Delivery Challan not found.");

                existingDc.ShortClose = ProductionIssueCompVM.ShortClose;

                await _unitOfWork.ProductionIssueComps.UpdateAsync(existingDc);
                await _unitOfWork.SaveAsync();

                await UpdateIssueTallyStatusAsync(ProductionIssueCompVM.IssueId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[ProductionCompShortCloseAsync] Validation issue");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[ProductionCompShortCloseAsync] Unexpected error");

            }
        }
        public async Task UpdateIssueTallyStatusAsync(int IssueID)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.IssueId == IssueID)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var dcOut = await _unitOfWork.ProductionIssueComps.GetAsync(IssueID);
                if (dcOut == null)
                    return;

                if (dcOut.ShortClose || dcOut.Cancel)
                    return;

                dcOut.IssueTally = (totalBalQty == 0);

                await _unitOfWork.ProductionIssueComps.UpdateAsync(dcOut);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateIssueTallyStatusAsync] Error updating IssueID:- {IssueID}");
                throw new InvalidOperationException("Failed to update Delivery Challan Tally status. Please contact support.");
            }
        }
        public async Task<bool> IsProductionTransactionsMatchedwilecancelAsync(int issueid, ProductionIssueCompVM ProductionIssueCompVM)
        {
            try
            {
                var DcSubIds = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.IssueId == issueid)
                    .Select(x => x.IssueSubId)
                    .ToListAsync();

                bool hasDc = DcSubIds.Any();


                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = ProductionIssueCompVM?.ProductionIssueCompSubVMs?
                            .Where(x => x.TransType == "Out")
                            .ToList();

                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.Qty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty ?? 0);

                    qtyMismatch = totalQty != totalBalQty;
                }

                // If either transactions exist OR quantity mismatch → return true
                return qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for issueid: {issueid}");
                return false;
            }
        }
        public async Task<List<ProductionIssueCompSub>> GetProductionComSubDetailsByIssueIdAsync(int IssueId)
        {
            if (IssueId <= 0)
                return new List<ProductionIssueCompSub>();

            try
            {
                var query = _unitOfWork.ProductionIssueCompSubs
                                       .GetQueryable()
                                       .Where(s => s.IssueId == IssueId)
                                       .OrderBy(s => s.SlNo)
                                       .AsNoTracking();

                var subs = await query.ToListAsync();

                return subs ?? new List<ProductionIssueCompSub>();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetProductionComSubDetailsByIssueIdAsync for IssueId: {IssueId}");
                return new List<ProductionIssueCompSub>();
            }
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
        public async Task<List<Dictionary<string, object>>> GetPOGRNSCNItemDetailsByCustId(int custId, int storeId)
        {
            try
            {
                // GRN DATA
                var grnData = await _unitOfWork.LabourGRNSubs
                              .GetQueryable()
                              .Where(x =>
                                  !x.ItemCancel
                                  && !x.LabourGRN.DcCancel
                                  && !x.LabourGRN.ShortClose
                                  && x.TransType == "Out" && x.ProdCompBalQty > 0
                                  && x.LabourGRN.CustId == custId
                                  && _unitOfWork.LabourGRNSubs
                                      .GetQueryable()
                                      .Any(i =>
                                          i.GRNId == x.GRNId &&
                                          i.GroupId == x.GroupId &&
                                          i.TransType == "In" &&
                                          i.BalQty == 0))
                      .Select(x => new
                      {
                          x.GRNSubId,
                          x.GRNId,
                          GRNNo = x.LabourGRN.GRNNo,
                          GRNSuffix = x.LabourGRN.Suffix,
                          GRNDate = x.LabourGRN.GRNDate,
                          x.GroupId,

                          x.Qty,
                          x.ProdCompBalQty,
                          x.UnitPrice,

                          // PO DETAILS
                          PONo = x.MfgPoSub != null ? x.MfgPoSub.MfgPo.PONo : null,
                          POSuffix = x.MfgPoSub != null ? x.MfgPoSub.MfgPo.Suffix : null,
                          PODate = x.MfgPoSub != null ? x.MfgPoSub.MfgPo.PODate : (DateTime?)null,
                          PoSubId = x.RefPoSubId,
                          LineNo = x.MfgPoSub != null ? x.MfgPoSub.LineNo : null,

                          // ITEM
                          x.ItemId,
                          ItemCode = x.Item.ItemCode,
                          ItemName = x.Item.ItemName,
                          Specification = x.Item.Specification,
                          MeasureUnit = x.Item.MeasureUnit,

                          categoryCode = x.Item.CategoryCode,
                          Category = x.Item.Category.CategoryName,

                          // DC
                          RefDcNo = x.LabourGRN.RefDcNo,
                          RefDcDate = x.LabourGRN.RefDcDate,

                          x.CostId,
                          ProjectNo = x.CostCenter.ProjectNo,
                          x.RowRemarks
                      })
                    .ToListAsync();

                // 🔹 STOCK
                var itemIds = grnData
                    .Where(x => x.ItemId != null)
                    .Select(x => x.ItemId.Value)
                    .Distinct()
                    .ToList();

                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);

                // 🔹 FINAL RESULT
                var result = grnData.Select(r =>
                {
                    decimal stockQty = 0;

                    if (r.ItemId != null)
                        stockDict.TryGetValue(r.ItemId.Value, out stockQty);

                    return new Dictionary<string, object>
                    {

                        ["Selected"] = false,

                        ["GRNSubId"] = r.GRNSubId,

                        ["GRNId"] = r.GRNId,
                        ["GroupId"] = r.GroupId,
                        ["GRNNo"] = $"{r.GRNNo}{r.GRNSuffix}",
                        ["GRNDate"] = r.GRNDate.ToString("dd/MM/yyyy"),

                        ["CategoryCode"] = r.categoryCode,
                        ["Category"] = r.Category,

                        ["ItemId"] = r.ItemId,
                        ["ItemCode"] = r.ItemCode ?? "",
                        ["ItemName"] = r.ItemName ?? "",
                        ["ItemSpecification"] = r.Specification ?? "",
                        ["MeasureUnit"] = r.MeasureUnit ?? "",

                        ["Qty"] = r.Qty,
                        ["BalQty"] = r.ProdCompBalQty,
                        ["UnitPrice"] = r.UnitPrice,

                        ["StockQty"] = stockQty,

                        ["RefDcNo"] = r.RefDcNo,
                        ["RefDcDate"] = r.RefDcDate?.ToString("dd/MM/yyyy"),

                        ["RefPoSubId"] = r.PoSubId,
                        ["RefPoNo"] = r.PONo != null ? $"{r.PONo}{r.POSuffix}" : "",
                        ["RefPoDate"] = r.PODate?.ToString("dd/MM/yyyy"),
                        ["LineNo"] = r.LineNo,

                        ["CostId"] = r.CostId,
                        ["ProjectNo"] = r.ProjectNo,

                        ["Remarks"] = r.RowRemarks,

                        ["Source"] = r.PoSubId != null ? "PO" : "GRN"
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching SCN details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve SCN details. Please try again.");
            }
        }
        public async Task<bool> CheckIsSameAsOutItemBylabGrnSubId(int GrnSubId)
        {
            return await _unitOfWork.LabourGRNSubs
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.GRNSubId == GrnSubId)
                .Select(x => x.LabourGRN.IsOutItemSameAsInItem)
                .FirstOrDefaultAsync();
        }
        public async Task<List<LabourGRNSubVM>> GetLabourGRNInItemsAsync(List<int> grnIds)
        {
            if (grnIds == null || !grnIds.Any())
                return new List<LabourGRNSubVM>();

            try
            {
                return await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Include(x => x.Item)
                    .Where(x =>
                        x.TransType == "In" &&
                        !x.ItemCancel &&
                        grnIds.Contains(x.GRNId) &&
                        x.ProdCompBalQty > 0)
                    .Select(x => new LabourGRNSubVM
                    {
                        // GRN
                        GRNSubId = x.GRNSubId,
                        GRNId = x.GRNId,
                        GroupId = x.GroupId,

                        // Item
                        ItemId = x.ItemId,
                        ItemCode = x.Item != null ? x.Item.ItemCode : "",
                        ItemName = x.Item != null ? x.Item.ItemName : "",
                        MeasureUnit = x.Item != null ? x.Item.MeasureUnit : "",
                        HSNCode = x.Item != null ? x.Item.HSNCode : "",

                        // Qty
                        Qty = x.ProdCompBalQty,
                        ProdCompBalQty = x.ProdCompBalQty,
                        UnitPrice = x.UnitPrice,

                        IsEditable = false
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    "GetLabourGRNInItemsAsync failed.");

                throw;
            }
        }



    }
}
