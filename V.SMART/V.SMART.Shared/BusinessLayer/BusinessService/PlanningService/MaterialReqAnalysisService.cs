using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IMaterialRequisitionService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.MaterialReqAnalysisVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.PlanningService
{
    public class MaterialReqAnalysisService: IMaterialReqAnalysisService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly IMaterialReqService _materialReqService;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _userService;


        public MaterialReqAnalysisService(IUnitOfWork unitOfWork, ILoggingService loggingService, ICommonService commonService, 
            IStockManagerService stockManagerService, IMaterialReqService materialReqService,CurrentUserService userService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _commonService = commonService;
            _stockManagerService = stockManagerService;
            _materialReqService = materialReqService;
            _userService = userService;
        }
        public async Task<bool> IsItemROlDisplayEnabledAsync()
           => await _commonService.GetScreenPermissionsAsync("Purchase Order", "ROL");

        public async Task<List<CustomerVM>> GetAllActiveCustomersAsync()
                => await _commonService.GetAllActiveCustomersAsync();

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            try
            {
                return await _commonService.SearchCustomersAsync(searchText);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchCustomersAsync");
                return Enumerable.Empty<CustomerVM>();
            }
        }


        // Get stock balances with logging
        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int? storeId)
        {
            try
            {
                return await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetStockForItemsAsync");
                return new Dictionary<int, decimal>();
            }
        }

        // Get pending POs with proper safety
        public async Task<List<MfgPo>> GetPendingPOsByCustomersAsync(List<int> custIds)
        {
            try
            {
                return await _unitOfWork.MfgPos.GetQueryable()
                    .Where(p =>
                        custIds.Contains(p.CustId.Value) &&
                        !p.PoTally &&
                        !p.PoCancl && p.MfgORLab==true &&
                        p.MfgPoSubs.Any(ps => ps.IndentBalQty > 0 && !ps.ItemCancel)
                    )
                    .Select(p => new MfgPo
                    {
                        PoId = p.PoId,
                        PONo = p.PONo + "" + p.Suffix
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetPendingPOsByCustomersAsync");
                return new List<MfgPo>();
            }
        }


        public async Task<List<MRAPoItems>> GetPOItemsAsync(List<int> poIds, int? storeId = null)
        {
            try
            {
                // ✅ Guard clause
                if (poIds == null || !poIds.Any())
                    return new List<MRAPoItems>();

                // ✅ Fetch PO items with safe projection
                var poItems = await _unitOfWork.MfgPoSubs.GetQueryable()
                    .AsNoTracking()
                    .Where(sub =>
                       poIds.Contains(sub.PoId) &&
                                       !sub.ItemCancel &&
                                       sub.BalQty > 0 &&
                                       sub.IndentBalQty > 0)
                    .Select(sub => new MRAPoItems
                    {
                        IsSelected = false,

                        CustId = sub.MfgPo.CustId,
                        custName = sub.MfgPo.Customer != null ? sub.MfgPo.Customer.CustName : string.Empty,

                        CategoryName = sub.Item.Category != null ? sub.Item.Category.CategoryName : string.Empty,
                        ItemCode = sub.Item.ItemCode,
                        ItemName = sub.Item.ItemName,

                        POQty = sub.Qty,
                        IndentBalQty = sub.IndentBalQty,
                        RequiredQty = sub.IndentBalQty,

                        Type = string.Empty,

                        ItemId = sub.ItemId,
                        RefPoSubId = sub.PoSubId,
                        RefPoId = sub.PoId,
                        RefPoNo = sub.MfgPo.PONo + (sub.MfgPo.Suffix ?? string.Empty),

                        CostId = sub.CostId,
                        ProjectNo = sub.CostCenter != null ? sub.CostCenter.ProjectNo : string.Empty,

                        RefPoDate = sub.MfgPo.PODate ?? DateTime.MinValue,
                        IsLabourMaterial = sub.Item.IsLabourItem
                    })
                    .ToListAsync();

                if (!poItems.Any())
                    return new List<MRAPoItems>();

                var itemIds = poItems.Select(x => x.ItemId).Distinct().ToList();
                var stockBalances = await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);

                foreach (var item in poItems)
                {
                    stockBalances.TryGetValue(item.ItemId, out var stockQty);
                    item.StockQty = stockQty;

                    var indentQty = item.IndentBalQty ?? 0;
                    var stock = stockQty;
                }

                return poItems
                    .OrderBy(x => x.custName)
                    .ThenBy(x => x.RefPoNo)
                    .ThenBy(x => x.ItemName)
                    .ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetPOItemsAsync");
                return new List<MRAPoItems>();
            }
        }
        public async Task<List<BOMItem>> GetBOMHierarchyAsync(int assyId)
        {
            var result = new List<BOMItem>();

            try
            {
                async Task AddChildrenAsync(int parentId, decimal parentUtilQty)
                {
                    var children = await _unitOfWork.AssmblyDefs.GetQueryable()
                        .Include(a => a.PartItem)
                        .Where(a => a.AssmblyID == parentId && a.PartItem != null)
                        .Select(a => new BOMItem
                        {
                            Id = a.Id,
                            AssmblyID = a.AssmblyID,
                            ItemID = a.ItemId,
                            UtilQty = a.UtilQty,
                            ItemCode = a.PartItem!.ItemCode,
                            ItemName = a.PartItem.ItemName,
                            categoryCode = a.PartItem.CategoryCode,
                            IsLabourMaterial = a.IsLabourMaterial
                        })
                        .ToListAsync();

                    foreach (var child in children)
                    {
                        child.UtilQty = parentUtilQty * child.UtilQty;

                        result.Add(child);

                        await AddChildrenAsync(
                            child.ItemID,
                            child.UtilQty);
                    }
                }

                await AddChildrenAsync(assyId, 1);

                return result
                    .Where(r => r.categoryCode != 3 && r.categoryCode != 7)
                    .ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetBOMHierarchyAsync");

                return new List<BOMItem>();
            }
        }

        public async Task<List<BOMItem>> GetBOMHierarchyAsync1(int assyId)//Old 22062023 changes
        {
            var result = new List<BOMItem>();

            try
            {
                async Task AddChildrenAsync(int parentId)
                {
                    var children = await _unitOfWork.AssmblyDefs.GetQueryable()
                        .Include(a => a.PartItem)
                        .Where(a => a.AssmblyID == parentId && a.PartItem != null)
                        .Select(a => new BOMItem
                        {
                            Id = a.Id,
                            AssmblyID = a.AssmblyID,
                            ItemID = a.ItemId,
                            UtilQty = a.UtilQty,
                            ItemCode = a.PartItem!.ItemCode,
                            ItemName = a.PartItem.ItemName,
                            categoryCode = a.PartItem.CategoryCode,
                            IsLabourMaterial = a.IsLabourMaterial,

                        })
                        .ToListAsync();

                    foreach (var child in children)
                    {
                        result.Add(child);
                        await AddChildrenAsync(child.ItemID);
                    }
                }

                await AddChildrenAsync(assyId);

                var finalResult = result
                    .Where(r => r.categoryCode != 3 && r.categoryCode != 7)
                    .ToList();

                return finalResult;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetBOMHierarchyAsync");
                return new List<BOMItem>();
            }
        }

        public async Task<List<CompMasterVM>> GetRawMaterialsForComponentAsync(int componentItemId)
        {
            try
            {
                var result = await _unitOfWork.CompMasters.GetQueryable()
                    .Where(r => r.CompItemId == componentItemId)
                    .Select(r => new CompMasterVM
                    {
                        RMId = r.RMId,
                        RMCode = r.RawMaterial.ItemCode,
                        Weight = r.Weight,
                        IsDefaultRM = r.IsDefaultRM
                    })
                    .ToListAsync();

                if (result.Count > 0)
                {
                    var defaultRm = result.FirstOrDefault(r => r.IsDefaultRM);
                    if (defaultRm != null)
                        return new List<CompMasterVM> { defaultRm };

                    return new List<CompMasterVM> { result.First() };
                }

                return new List<CompMasterVM>();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetRawMaterialsForComponentAsync");
                return new List<CompMasterVM>();
            }
        }


        /// <summary>
        /// Auto-generate and save Material Requisition with transaction
        /// </summary>
        public async Task<MaterialReq> AutoGenerateMaterialReqAsync(
            string selectedFilter,
            List<MRAPoItems> poItems,
            List<MRAPoSubItemsVM> mraPOSubItems)
        {
            var suffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);
            var mreqNo = await _unitOfWork.MaterialReqs.GetLastMReqNoAsync(suffix);

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var materialReq = new MaterialReq
                {
                    MReqNo = mreqNo,
                    Suffix = suffix,
                    MreqDate = DateTime.Now,
                    MreqDateNow = DateTime.Now,
                    IsManual = false,
                    IsPurchOrSubCon = true,
                    NoOfItems = 0,
                    Department = "",
                    CreatedBy = await _userService.GetUsernameAsync(),
                    CreatedDate = DateTime.Now,
                    MaterialReqSubs = new List<MaterialReqSub>()
                };

                var itemIds = new List<int>();

                bool IsItemROl = await _commonService.GetScreenPermissionsAsync("Purchase Order", "ROL");

                foreach (var subItem in mraPOSubItems)
                {
                    if (subItem.RMItemId.HasValue && subItem.RMItemId.Value > 0)
                        itemIds.Add(subItem.RMItemId.Value);

                    if (subItem.AssyCompItemId > 0)
                        itemIds.Add(subItem.AssyCompItemId);
                }
                var allItemIds = mraPOSubItems
                                .SelectMany(x => new int?[]
                                {
                                     x.AssyCompItemId,
                                     x.RMItemId
                                })
                                .Where(x => x.HasValue)
                                .Select(x => x.Value)
                                .Distinct()
                                .ToList();

                var stockDB = await GetStockForItemsAsync(allItemIds, null);

                foreach (var subItem in mraPOSubItems)
                {
                    int itemId = 0;
                    decimal qtyReq = 0;
                    decimal purchQty = 0;

                    if (subItem.RMItemId.HasValue && subItem.RMItemId.Value > 0)
                    {
                        if (selectedFilter == "PendingPO")
                        {
                            if (subItem.RMReqQty.GetValueOrDefault() <= 0)
                                continue;
                            purchQty = subItem.RMReqQty.GetValueOrDefault();
                        }
                        else
                        {
                            if (subItem.RMDiffQty.GetValueOrDefault() <= 0)
                                continue;
                            purchQty = subItem.RMDiffQty.GetValueOrDefault();
                        }

                        itemId = subItem.RMItemId.Value;
                        qtyReq = subItem.RMReqQty.GetValueOrDefault();
                    }
                    else if (subItem.AssyCompItemId > 0)
                    {
                        if (selectedFilter == "PendingPO")
                        {
                            if (subItem.AssyCompReqQty.GetValueOrDefault() <= 0)
                                continue;
                            purchQty = subItem.AssyCompReqQty.GetValueOrDefault();
                        }
                        else
                        {
                            if (subItem.AssyCompDiffQty.GetValueOrDefault() <= 0)
                                continue;
                            purchQty = subItem.AssyCompDiffQty.GetValueOrDefault();
                        }

                        itemId = subItem.AssyCompItemId;
                        qtyReq = subItem.AssyCompReqQty.GetValueOrDefault();
                    }

                    if (itemId == 0 || purchQty <= 0)
                        continue;

                    var unitPrices = await _materialReqService.GetBulkLastUnitPricesAsync(new List<int> { itemId },subItem.CustId);

                    var unitPrice = unitPrices.TryGetValue(itemId, out var price) ? price : 0;

                    var parentPo = poItems.FirstOrDefault(p => p.RefPoSubId == subItem.RefPoSubId);

                    if (IsItemROl)
                    {
                        // Get inventory rules
                        var rules = await _materialReqService.GetItemByItemIdAsync(itemId);

                        decimal rol = rules?.ROL ?? 0m;
                        decimal mol = rules?.MOL ?? 0m;
                        decimal lotSize = rules?.LotSize ?? 0m;

                        // Current stock
                        decimal stockQty = 0;
                        if (subItem.AssyCompItemId > 0)
                        {
                            stockDB.TryGetValue(
                                subItem.AssyCompItemId,
                                out stockQty);
                        }

                        // RM Item Stock
                        if (subItem.RMItemId.HasValue)
                        {
                            stockDB.TryGetValue(
                                subItem.RMItemId.Value,
                                out stockQty);
                        }

                        if (lotSize > 0)
                        {
                            if (stockQty <= rol)
                            {
                                decimal reqQty = purchQty;

                                if (lotSize > 0)
                                {
                                    reqQty =
                                        Math.Ceiling(reqQty / lotSize) * lotSize;
                                }

                                purchQty = reqQty;
                            }
                            else
                            {
                                purchQty = lotSize;
                            }
                        }
                      
                    }

                    if(purchQty>0)
                    {
                        materialReq.MaterialReqSubs.Add(new MaterialReqSub
                        {
                            ItemId = itemId,
                            CustId = subItem.CustId,
                            SlNo = materialReq.MaterialReqSubs.Count + 1,
                            PurchQty = purchQty,
                            BalQty = purchQty,
                            UnitPrice = unitPrice,
                            DueDate = DateTime.Now.AddDays(5),
                            RefPoSubId = subItem.RefPoSubId,
                            RefPoId = subItem.RefPoId,
                            MainAssyId = parentPo?.ItemId ?? 0,
                            MfgPoIndentQty = parentPo?.RequiredQty ?? 0,
                            CostId = subItem.CostId
                        });
                    }
                   
                }

                materialReq.NoOfItems = materialReq.MaterialReqSubs.Count;

                if(materialReq.NoOfItems>0)
                {
                    await SetPRAuthorizationStatusAsync(materialReq);

                    await _unitOfWork.MaterialReqs.CreateAsync(materialReq);
                    await _unitOfWork.SaveAsync();

                    await UpdatePoIndentBalanceQtyAsync(poItems);
                }
               

                await transaction.CommitAsync();

                return materialReq;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Failed to auto-generate Material Requisition", ex);
            }
        }


        private async Task SetPRAuthorizationStatusAsync(MaterialReq entity)
        {
            var prAuthorityExists = await _unitOfWork.UserAuthorities
                .AnyAsync(x => x.IsPR == true);

            if (!prAuthorityExists)
            {
                entity.IsAuthorized = true;
                entity.LastApproved = await _userService.GetUsernameAsync();
                entity.ApprovedDate = DateTime.Now;
            }
            else
            {
                entity.IsAuthorized = false;
                entity.LastApproved = null;
                entity.ApprovedDate = null;

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

        private async Task UpdatePoIndentBalanceQtyAsync(List<MRAPoItems> poItems)
        {
            foreach (var item in poItems.Where(x => x.IsSelected))
            {
                var poSub = await _unitOfWork.MfgPoSubs
                    .FirstOrDefaultAsync(p => p.PoSubId == item.RefPoSubId);

                if (poSub != null)
                {
                    poSub.IndentBalQty -= item.RequiredQty.GetValueOrDefault();

                    if (poSub.IndentBalQty < 0)
                        poSub.IndentBalQty = 0;

                    await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                }
            }
            await _unitOfWork.SaveAsync();
        }
        public async Task<Dictionary<int, List<BOMItem>>> GetBOMHierarchiesBatchAsync(
IEnumerable<int> rootAssemblyIds)
        {
            var result = new Dictionary<int, List<BOMItem>>();

            var assemblyIds = await _unitOfWork.AssmblyDefs
                .GetQueryable()
                .AsNoTracking()
                .Select(x => x.AssmblyID)
                .Distinct()
                .ToHashSetAsync();

            foreach (var rootId in rootAssemblyIds.Distinct())
            {
                var leafItems = new List<BOMItem>();

                var processed = new HashSet<int>();
                var pending = new Queue<int>();

                pending.Enqueue(rootId);

                while (pending.Count > 0)
                {
                    var currentId = pending.Dequeue();

                    if (!processed.Add(currentId))
                        continue;

                    var bomRows = await _unitOfWork.AssmblyDefs
                        .GetQueryable()
                        .AsNoTracking()
                        .Include(a => a.PartItem)
                        .Where(a =>
                            a.AssmblyID == currentId &&
                            a.PartItem != null)
                        .Select(a => new BOMItem
                        {
                            Id = a.Id,
                            AssmblyID = a.AssmblyID,
                            ItemID = a.ItemId,
                            UtilQty = a.UtilQty,
                            ItemCode = a.PartItem!.ItemCode,
                            ItemName = a.PartItem.ItemName,
                            categoryCode = a.PartItem.CategoryCode,
                            IsLabourMaterial = a.IsLabourMaterial
                        })
                        .ToListAsync();

                    foreach (var item in bomRows)
                    {
                        if (assemblyIds.Contains(item.ItemID))
                        {
                            pending.Enqueue(item.ItemID);
                        }
                        else
                        {
                            leafItems.Add(item);
                        }
                    }
                }

                result[rootId] = leafItems;
            }

            return result;
        }
        public async Task<List<BOMItem>> GetDirectBOMItemsAsync(int assyId)
        {
            return await _unitOfWork.AssmblyDefs.GetQueryable()
                .Include(a => a.PartItem)
                .Where(a => a.AssmblyID == assyId)
                .Select(a => new BOMItem
                {
                    Id = a.Id,
                    AssmblyID = a.AssmblyID,
                    ItemID = a.ItemId,
                    UtilQty = a.UtilQty,
                    ItemCode = a.PartItem.ItemCode,
                    ItemName = a.PartItem.ItemName,
                    categoryCode = a.PartItem.CategoryCode,
                    IsLabourMaterial = a.IsLabourMaterial
                })
                .ToListAsync();
        }
        public async Task<Dictionary<int, List<CompMasterVM>>> GetRawMaterialsForComponentsBatchAsync(
       IEnumerable<int> componentIds)
        {
            try
            {
                var ids = componentIds
                    .Distinct()
                    .ToList();

                if (!ids.Any())
                    return new Dictionary<int, List<CompMasterVM>>();

                var allMappings = await _unitOfWork.CompMasters
                    .GetQueryable()
                    .AsNoTracking()
                    .Include(x => x.RawMaterial)
                    .Where(x =>
                        ids.Contains(x.CompItemId ?? 0) &&
                        x.RawMaterial != null)
                    .Select(x => new
                    {
                        x.CompItemId,
                        x.RMId,
                        x.Weight,
                        x.IsDefaultRM,
                        RMCode = x.RawMaterial.ItemCode
                    })
                    .ToListAsync();

                var result = new Dictionary<int, List<CompMasterVM>>();

                foreach (var group in allMappings.GroupBy(x => x.CompItemId))
                {
                    var defaultRm =
                        group.FirstOrDefault(x => x.IsDefaultRM);

                    if (defaultRm != null)
                    {
                        result[group.Key.Value] =
                        [
                            new CompMasterVM
               {
                   RMId = defaultRm.RMId,
                   RMCode = defaultRm.RMCode,
                   Weight = defaultRm.Weight,
                   IsDefaultRM = true
               }
                        ];
                    }
                    else
                    {
                        var first = group.First();

                        result[group.Key.Value] =
                        [
                            new CompMasterVM
               {
                   RMId = first.RMId,
                   RMCode = first.RMCode,
                   Weight = first.Weight,
                   IsDefaultRM = first.IsDefaultRM
               }
                        ];
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    nameof(GetRawMaterialsForComponentsBatchAsync));

                return new Dictionary<int, List<CompMasterVM>>();
            }
        }

    }
}
