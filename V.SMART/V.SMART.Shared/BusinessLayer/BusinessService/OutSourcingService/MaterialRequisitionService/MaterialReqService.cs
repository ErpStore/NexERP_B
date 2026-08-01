using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IMaterialRequisitionService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.MaterialRequisitionService
{
    public class MaterialReqService : IMaterialReqService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public MaterialReqService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            IStockManagerService stockManagerService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _stockManagerService = stockManagerService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        // Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        // Users
        public async Task<List<string>> GetAllActiveUserNamesAsync()
            => await _commonService.GetAllActiveUserNamesAsync();

        // Vendors
        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
        {
            return await _commonService.SearchVendorsAsync(searchText);
        }

        public async Task<VendorVM?> GetVenderByVendorCodeAsync(int? vendorCode)
            => await _commonService.GetVendorByVenerCodeAsync(vendorCode.Value);

        // Customers
        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);
        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
                => await _commonService.GetCustomerByIdAsync(custId);

        //Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        // Stores

        public async Task<List<(int StoreId, string StoreName)>> GetAllStoreNameAndIdAsync()
        {
            return await _commonService.GetAllStoreNameAndIdAsync();
        }
        public async Task<List<Staff>> GetAllStaffAsync()
           => await _commonService.GetAllStaffAsync();


        public async Task<bool> GetLineIdByCustomerAsync(int custId)
        {
            return await _commonService.GetLineIdByCustomerAsync(custId);
        }
        public async Task<List<CustomerVM>> GetCustomersAsync(int? custId = null)
        {
            if (custId.HasValue && custId.Value > 0)
            {
                var customer = await _commonService.GetCustomerByIdAsync(custId.Value);
                return customer != null ? new List<CustomerVM> { customer } : new List<CustomerVM>();
            }
            return await _commonService.GetAllActiveCustomersAsync();
        }

        public async Task<Currency?> GetCurrencyByIdAsync(int currId)
            => (await _commonService.GetCurrencyByIdAsync(currId));
        // Get latest currency rate (from CurrencyToday Service)
        public async Task<decimal?> GetLatestCurrencyValueAsync(int currId)
            => await _commonService.GetLatestCurrencyValueAsync(currId);


        // Contacts
        public async Task<List<ContactPerson>> GetContactPersonsAsync(int custId)
            => await _commonService.GetContactPersonsAsync(custId);

        // Consignee addresses
        public async Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId)
            => await _commonService.GetConsigneeAddressesAsync(custId);

        // CostCeneter
        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();
        public async Task<bool> CheckAssemblyExistsAsync(int itemId)
        {
            try
            {
                if (itemId <= 0)
                    throw new ArgumentException("Invalid ItemId. Must be greater than zero.", nameof(itemId));

                return await _unitOfWork.AssmblyDefs
                    .AnyAsync(a => a.AssmblyID == itemId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error checking Assembly existence for ItemId: {itemId}");
                throw;
            }
        }

        //Stock
        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int? storeId)
        {
            return await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);
        }

        // Material Requisition operations
        public async Task<bool> IsMreqTransactionsMatchedAsync(int mreqId, MaterialReqVM materialReqVM)
        {
            try
            {
                var mreqSubIds = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Where(x => x.MreqId == mreqId)
                    .Select(x => x.MreqSubId)
                    .ToListAsync();

                bool hasEnq = mreqSubIds.Any();

                bool hasTransactions = false;

                if (hasEnq)
                {
                    hasTransactions = await _unitOfWork.EnquiryPurchaseSubs
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.RefMReqSubId.HasValue &&
                            mreqSubIds.Contains(pqs.RefMReqSubId.Value));

                    if(!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.PurchPoSubs
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.RefMReqSubId.HasValue &&
                            mreqSubIds.Contains(pqs.RefMReqSubId.Value));
                    }
                }

                bool qtyMismatch = false;

                var list = materialReqVM?.MaterialReqSubVMs;
                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.PurchQty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty ?? 0);

                    qtyMismatch = totalQty != totalBalQty;
                }

                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for mreqId: {mreqId}");
                throw new InvalidOperationException("Failed to verify Material Req / PR transactions.", ex);
            }
        }


        public async Task<List<Dictionary<string, object>>> GetPoDetailsAsync()
        {
            try
            {
                var result = await (from e in _unitOfWork.MfgPos.GetQueryable()
                    join es in _unitOfWork.MfgPoSubs.GetQueryable()
                        on e.PoId equals es.PoId
                    where  !e.PoTally && !e.PoCancl && !e.ShortClose && es.IndentBalQty > 0 && !es.ItemCancel
                    select new
                    {
                        es.PoId,
                        es.PoSubId,
                        e.PONo,
                        e.Suffix,
                        e.PODate,
                        e.CustId,
                        e.Customer.CustName,
                        es.ItemId,
                        es.Item.ItemCode,
                        es.Item.ItemName,
                        es.Item.HSNCode,
                        es.Item.MeasureUnit,
                        es.Item.Category.CategoryName,
                        es.Qty,
                        es.IndentBalQty,
                        es.UnitPrice,
                        CostCenterId = es.CostId == 0 ? (int?)null : es.CostId,
                        es.CostCenter.ProjectNo
                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefPoId"] = r.PoId,
                    ["RefPoSubId"] = r.PoSubId,
                    ["CustId"] = r.CustId,
                    ["CustName"] = r.CustName,
                    ["PoNo"] = $"{r.PONo}{r.Suffix}",
                    ["PoDate"] = r.PODate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,
                    ["Category"] = r.CategoryName,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.IndentBalQty,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching po details");
                throw new InvalidOperationException("Failed to retrieve po details. Please try again.");
            }
        }

        public async Task<string> GetMReqNumberAsync(string suffix)
        {
            try
            {
                var lastSCN = await _unitOfWork.MaterialReqs
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.MReqNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastSCN != null)
                {
                    var parts = lastSCN.MReqNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Material Requisition number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Material Requisition number.");
            }
        }

        public async Task<List<string>> GetAllDepartment()
        {
            var result = await _unitOfWork.MaterialReqs.GetQueryable()
                         .Select(m => m.Department)
                         .Distinct()
                         .ToListAsync();
            return result;
        }

        public async Task<(List<MaterialReqVM> mreqVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.MaterialReqs.GetQueryable()
                .Include(j => j.MaterialReqSubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.MaterialReqSubs)
                    .ThenInclude(s => s.Customer)
                .Include(j => j.MaterialReqSubs)
                    .ThenInclude(s => s.Vendor)
                .Include(j => j.MaterialReqSubs)
                    .ThenInclude(s => s.MfgPo)
                .AsQueryable();

            // Apply dynamic filters
            if (filters != null)
            {
                foreach (var f in filters)
                    query = MreqFilterBuilder.ApplyFilter(query, f.Key, f.Value);
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.MreqId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (_mapper.Map<List<MaterialReqVM>>(list), total);
        }


        public static class MreqFilterBuilder
        {
            public static IQueryable<MaterialReq> ApplyFilter(
                IQueryable<MaterialReq> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                var val = value.ToString().Trim();

                switch (field)
                {
                    case "MreqNo":
                        {
                            string input = val;
                            string part1 = input;
                            string part2 = "";

                            int slashIndex = input.IndexOf('/');

                            if (slashIndex > -1)
                            {
                                part1 = input.Substring(0, slashIndex).Trim();
                                part2 = input.Substring(slashIndex + 1).Trim();
                            }

                            return query.Where(x =>
                                (string.IsNullOrEmpty(part1) || x.MReqNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "Customer":
                        return query.Where(x =>
                            x.MaterialReqSubs.Any(s => s.Customer.CustName.Contains(val)));

                    case "ItemCode":
                        return query.Where(x =>
                            x.MaterialReqSubs.Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x =>
                            x.MaterialReqSubs.Any(s => s.Item.ItemName.Contains(val)));

                    case "VendorName":
                        return query.Where(x =>
                            x.MaterialReqSubs.Any(s => s.Vendor.VendorName.Contains(val)));

                    case "SalesOrderNo":
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
                                x.MaterialReqSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.MfgPo.PONo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.MfgPo.Suffix.Contains(part2))
                                ));
                        }

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.MreqDate >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.MreqDate <= toDate);
                        break;

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "ApprovalStatus":
                        return ApplyApprovalStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<MaterialReq> ApplyStatusFilter(
                IQueryable<MaterialReq> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.PRTally == true),
                    "Pending" => query.Where(x => x.PRTally == false && x.MreqCancel == false && x.ShortClose == false),
                    "Cancelled" => query.Where(x => x.MreqCancel == true),
                    "Short Closed" => query.Where(x => x.ShortClose == true),
                    _ => query
                };
            }

            private static IQueryable<MaterialReq> ApplyApprovalStatusFilter(
                IQueryable<MaterialReq> query, string status)
            {
                return status switch
                {
                    "Approved" => query.Where(x => x.IsAuthorized == true),
                    "Rejected" => query.Where(x => x.IsRejected == true),
                    "Pending" => query.Where(x => x.IsAuthorized == false && x.IsRejected == false),
                    _ => query
                };
            }

        }



        public async Task<IEnumerable<MaterialReqVM>> GetAllMreqDetailsAsync()
        {
            try
            {
                var entities = await _unitOfWork.MaterialReqs.GetAllWithIncludeAsync(q => true,
                    q => q.MaterialReqSubs);

                return _mapper.Map<IEnumerable<MaterialReqVM>>(entities);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllMreqDetailsAsync");
                return Enumerable.Empty<MaterialReqVM>();
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteMaterialReqAsync(int mreqId)
        {
            try
            {
                var mreq = await _unitOfWork.MaterialReqs.GetAsync(mreqId);
                if (mreq == null)
                    return (false, "Material Requisition not found.");

                var mreqSubIds = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Where(s => s.MreqId == mreqId)
                    .Select(s => s.MreqSubId)
                    .ToListAsync();

                if (!mreqSubIds.Any())
                    return (true, "Material Requisition can be safely deleted.");

                bool hasPurchEnq = await _unitOfWork.EnquiryPurchaseSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefMReqSubId.HasValue && mreqSubIds.Contains(qs.RefMReqSubId.Value));

                if (hasPurchEnq)
                    return (false, "Cannot delete this Material Requisition because it is referenced in Purchase Enquiry.");

                bool hasPurchPo = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefMReqSubId.HasValue && mreqSubIds.Contains(qs.RefMReqSubId.Value));

                if (hasPurchPo)
                    return (false, "Cannot delete this Material Requisition because it is referenced in Purchase Order.");

                if (mreq.MreqCancel || mreq.ShortClose)
                    return (false, "Cannot delete this Material Requisition because it is Cancelled or Short Closed.");

                bool hasCancelledItems = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .AnyAsync(s => s.MreqId == mreqId && s.ItemCancel);

                if (hasCancelledItems)
                    return (false, "Cannot delete this Material Requisition because one or more items are cancelled.");

                return (true, "Material Requisition can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteMaterialReqAsync for MreqId: {mreqId}");
                throw new InvalidOperationException("Error checking Material Requisition delete eligibility.", ex);
            }
        }


        public async Task<bool> DeleteMreqByMreqIdAsync(int mreqId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var mreq = await _unitOfWork.MaterialReqs
                    .GetQueryable()
                    .Include(e => e.MaterialReqSubs)
                    .FirstOrDefaultAsync(e => e.MreqId == mreqId);

                if (mreq == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in mreq.MaterialReqSubs)
                {
                    if (sub.RefBomId.GetValueOrDefault() > 0)
                    {
                        await AdjustBOMBalanceAsync(sub.RefBomId.GetValueOrDefault(), sub.PurchQty, 0, "PR Deletion");
                    }

                    if(sub.RefPoSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustPoBalanceAsync(sub.RefPoSubId.GetValueOrDefault(),sub.ItemId, sub.PurchQty, 0, "PR Deletion");
                    }

                }

                //If PR is Generated By MRA Important Delete
                await RevertIndentBalanceGroupedAsync(mreq);

                await _unitOfWork.MaterialReqs.DeleteAsync(mreq);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Material-Requisition List",
                    action: $"Deleted Material Requisition: {mreq.MReqNo}",
                    additionalInfo: $"Mreq Id: {mreq.MreqId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Material Requisition: {mreqId}");
                throw;
            }
        }

        public async Task<MaterialReqVM?> GetMreqByMreqIdAsync(int mreqId)
        {
            try
            {
                var entity = await _unitOfWork.MaterialReqs.GetQueryable()
                    .Include(q => q.MaterialReqSubs)
                        .ThenInclude(s => s.Item)
                            .ThenInclude(i => i.Category)
                    .Include(q => q.MaterialReqSubs)
                        .ThenInclude(s => s.Vendor)
                    .Include(q => q.MaterialReqSubs)
                        .ThenInclude(s => s.CostCenter)
                    .Include(q => q.MaterialReqSubs)
                        .ThenInclude(s => s.MfgPoSub)
                            .ThenInclude(p => p.MfgPo)
                    .Include(q => q.MaterialReqSubs)
                        .ThenInclude(s => s.Customer)
                    .FirstOrDefaultAsync(q => q.MreqId == mreqId);

                if (entity == null)
                    return null;

                var existingMreq = _mapper.Map<MaterialReqVM>(entity);

                var itemIds = existingMreq.MaterialReqSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count == 0)
                    return existingMreq;

                var stockDict = await _stockManagerService.GetStockForItemsAsync(
                    itemIds,
                    existingMreq.StoreId.GetValueOrDefault());

                foreach (var sub in existingMreq.MaterialReqSubVMs)
                {
                    if (sub.ItemId.HasValue && stockDict.TryGetValue(sub.ItemId.Value, out var qty))
                        sub.StockQty = qty;
                    else
                        sub.StockQty = 0m;
                }

                return existingMreq;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetMreqByMreqIdAsync({mreqId})");
                return null;
            }
        }

        public async Task DeleteAndResequenceAsync(MaterialReqSubVM subitem, MaterialReqVM mreqVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.MreqSubId > 0)
                {
                    var entity = await _unitOfWork.MaterialReqSubs.GetAsync(subitem.MreqSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if(entity.RefBomId.GetValueOrDefault() >0)
                    {
                        await AdjustBOMBalanceAsync(entity.RefBomId, entity.PurchQty, 0, "PR Line Deletion");
                    }

                    await _unitOfWork.MaterialReqSubs.DeleteAsync(entity.MreqSubId);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Material Requisition",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Mreq No: {mreqVM?.MReqNo}"
                    );
                }
                else
                {
                    mreqVM.MaterialReqSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Where(x => x.MreqId == mreqVM.MreqId)
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
        private async Task AdjustBOMBalanceAsync(int? refBOMId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refBOMId.HasValue || refBOMId == 0)
                    return;

                var bomEntity = await _unitOfWork.AssmblyDefs.GetAsync(refBOMId.Value);
                if (bomEntity == null)
                    return;

                if (oldQty > 0)
                {
                    bomEntity.BalQty += oldQty;
                    bomEntity.PRConvertedQty -= oldQty;
                }

                if (newQty > 0)
                {
                    if (newQty > bomEntity.BalQty)
                        newQty = bomEntity.BalQty;

                    bomEntity.BalQty -= newQty;
                    bomEntity.PRConvertedQty += newQty;
                }

                if (bomEntity.BalQty < 0)
                    bomEntity.BalQty = 0;

                if (bomEntity.PRConvertedQty < 0)
                    bomEntity.PRConvertedQty = 0;

                await _unitOfWork.AssmblyDefs.UpdateAsync(bomEntity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustBOMBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust BOM balance. Please contact support.");
            }
        }

        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int? custId)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.MaterialReqSubs.GetQueryable()
                                  where qs.ItemId == itemId && qs.CustId == custId
                                  orderby qs.MreqSubId descending
                                  select qs.UnitPrice)
                                      .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.MaterialReqSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.MreqSubId descending
                                      select qs.UnitPrice)
                                      .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        var query3 = from isub in _unitOfWork.ItemSubs.GetQueryable()
                                     where isub.ItemId == itemId
                                     && (custId == null || isub.CustomerId == custId)
                                     orderby isub.Id descending
                                     select isub.Rate;

                        rate = await query3.FirstOrDefaultAsync();
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

        public async Task<decimal> GetPOItemIndentBalQtyFromPoSubId(int poSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(e => e.PoSubId == poSubId)
                    .Select(e => e.IndentBalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching IndentBalQty for PoSubId: {poSubId}");
                throw new InvalidOperationException("Failed to retrieve Indent balance quantity.");
            }
        }

        public async Task<MaterialReqSubVM?> GetMaterialReqSubItemDetailByMreqSubIdAsync(int mreqSubId)
        {
            try
            {
                return await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Where(q => q.MreqSubId == mreqSubId)
                    .Select(q => new MaterialReqSubVM
                    {
                        PurchQty = q.PurchQty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Material Req sub item detail for MreqSubId: {mreqSubId}");
                throw new InvalidOperationException("Failed to retrieve Material Req sub-item details.");
            }
        }

        public async Task<(bool CanItemCancel, string Message)> CanPRItemCancelCheckAsync(MaterialReqSubVM subItem)
        {
            try
            {
                var Enq = await _unitOfWork.EnquiryPurchaseSubs
                    .GetQueryable()
                    .Where(qs =>  qs.RefMReqSubId == subItem.MreqSubId && !qs.ItemCancel).FirstOrDefaultAsync();

                if(Enq != null)
                {
                    bool allActiveItems = await _unitOfWork.EnquiryPurchaseVendorAssigns.GetQueryable()
                        .Where(e => e.EnquirySubId == Enq.EnquirySubId)
                        .AllAsync(e => e.ItemStatus < 3);


                    if (allActiveItems)
                        return (false, "Cannot cancel this Item as a Enquiry transaction exists.");
                }

                bool hasPo = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefMReqSubId.HasValue && qs.RefMReqSubId == subItem.MreqSubId && !qs.ItemCancel);

                if (hasPo)
                    return (false, "Cannot cancel this Item as a Purchase Order transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanPRItemCancelCheckAsync for MreqSubId: {subItem.MreqSubId}");
                throw new Exception("Error checking PR Item cancel eligibility", ex);
            }
        }

        public async Task ValidateBeforeRevertAsync(int mreqSubId)
        {
            try
            {
                var sub = await _unitOfWork.MaterialReqSubs.GetAsync(mreqSubId);

                if (sub == null)
                    throw new InvalidOperationException("Material Request Item not found.");

                if (sub.RefBomId > 0)
                    await ValidateBOMBalanceBeforeRevertAsync(sub);

                if (sub.RefPoSubId > 0)
                    await ValidatePoBalanceBeforeRevertAsync(sub);
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



        public async Task UpsertPRShortCloseAsync(MaterialReqVM materialReqVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingPR = await _unitOfWork.MaterialReqs.GetAsync(materialReqVM.MreqId);
                if (existingPR == null)
                    throw new InvalidOperationException("Material Requisition not found.");

                existingPR.ShortClose = materialReqVM.ShortClose;

                await _unitOfWork.MaterialReqs.UpdateAsync(existingPR);
                await _unitOfWork.SaveAsync();

                await UpdatePRTallyStatusAsync(existingPR.MreqId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertPRShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertPRShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }

        public async Task UpdatePRTallyStatusAsync(int mreqId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Where(x => x.MreqId == mreqId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var materialReq = await _unitOfWork.MaterialReqs.GetAsync(mreqId);
                if (materialReq == null)
                    return;

                if (materialReq.ShortClose || materialReq.MreqCancel)
                    return;

                materialReq.PRTally = (totalBalQty == 0);

                await _unitOfWork.MaterialReqs.UpdateAsync(materialReq);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdatePRTallyStatusAsync] Error updating MreqId {mreqId}");
                throw new InvalidOperationException("Failed to update PR Tally status. Please contact support.");
            }
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(MaterialReqSubVM subItem)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var subEntity = await _unitOfWork.MaterialReqSubs.GetQueryable().Where
                                (x => x.MreqSubId == subItem.MreqSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with MreqSubId {subItem.MreqSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    if (subItem.RefBomId.GetValueOrDefault() > 0)
                        await ValidateBOMBalanceBeforeRevertAsync(subEntity);

                    else if (subItem.RefPoSubId > 0)
                        await ValidatePoBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.MaterialReqSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    if(subEntity.RefPoSubId > 0)
                    {
                        await AdjustPoBalanceAsync(
                            subEntity.RefPoSubId,
                            subEntity.ItemId.GetValueOrDefault(),
                            subEntity.PurchQty,
                            0,
                            $"PR Item Cancel - {subItem.ItemCode}"
                        );
                    }
                    else if(subEntity.RefBomId.GetValueOrDefault() > 0)
                    {
                        await AdjustBOMBalanceAsync(
                           subEntity.RefBomId,
                           subEntity.PurchQty,
                           0,
                           $"PR Item Cancel - {subItem.ItemCode}"
                       );
                    }
                }
                else
                {
                    if (subEntity.RefPoSubId > 0)
                    {
                        await AdjustPoBalanceAsync(
                            subEntity.RefPoSubId,
                            subEntity.ItemId.GetValueOrDefault(),
                            0,
                            subEntity.PurchQty,
                            $"PR Item Revert Cancel - {subItem.ItemCode}"
                        );
                    }
                    else if (subEntity.RefBomId.GetValueOrDefault() > 0)
                    {
                        await AdjustBOMBalanceAsync(
                            subEntity.RefBomId,
                            0,
                            subEntity.PurchQty,
                            $"PR Item Revert Cancel - {subItem.ItemCode}"
                        );
                    }
                        
                }
                await UpdatePRTallyStatusAsync(subItem.MreqId);

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
       
        public async Task<List<MaterialReqSub>> GetMaterialReqSubByMreqIdAsync(int mreqId)
        {
            try
            {
                var subs = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.MreqId == mreqId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Material Requisition items for MreqId: {mreqId}");
                throw new InvalidOperationException("Failed to retrieve Material Requisition sub-items. Please try again.");
            }
        }

        public async Task<MaterialReqVM> UpsertMaterialReqAsync(MaterialReqVM mreqVM)
        {
            if (mreqVM == null)
                throw new ArgumentNullException(nameof(mreqVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                MaterialReq entity;

                if (mreqVM.MreqId == 0)
                {
                    entity = _mapper.Map<MaterialReq>(mreqVM);

                    var NextNumber = await _unitOfWork.MaterialReqs.GetLastMReqNoAsync(entity.Suffix);
                    entity.MReqNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.MaterialReqSubs = mreqVM.MaterialReqSubVMs.Select(s => _mapper.Map<MaterialReqSub>(s)).ToList();

                    await SetPRAuthorizationStatusAsync(entity, currentUser);

                    await _unitOfWork.MaterialReqs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in mreqVM.MaterialReqSubVMs)
                    {
                        if (subVM.RefPoSubId > 0)
                            await AdjustPoBalanceAsync(subVM.RefPoSubId, subVM.ItemId, 0, subVM.PurchQty.GetValueOrDefault(), "MR Creation");
                    }

                    changes.AppendLine("Material Requisition Created.");
                }
                else
                {
                    entity = await _unitOfWork.MaterialReqs.GetQueryable()
                        .Include(q => q.MaterialReqSubs)
                        .FirstOrDefaultAsync(q => q.MreqId == mreqVM.MreqId)
                        ?? throw new InvalidOperationException("Material Requisition not found.");

                    var parentChanges = GetPropertyChanges(entity, mreqVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(mreqVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await SetPRAuthorizationStatusAsync(entity, currentUser);

                    await HandleChildUpdatesAsync(entity, mreqVM.MaterialReqSubVMs, changes);

                    changes.AppendLine("Material Requisition Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdatePRTallyStatusAsync(mreqVM.MreqId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, mreqVM.MreqId == 0 ? "Material Requisition Created" : "Material Requisition Updated");

                var savedEntity = await _unitOfWork.MaterialReqs.GetQueryable()
                    .Include(q => q.MaterialReqSubs)
                    .ThenInclude(s => s.Item)
                    .Include(q => q.MaterialReqSubs)
                    .ThenInclude(s => s.Customer)
                    .Include(q => q.MaterialReqSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.MreqId == entity.MreqId);

                return _mapper.Map<MaterialReqVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Material Requisition: {mreqVM.MReqNo}");
                throw new InvalidOperationException("Failed to save Material Requisition. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(MaterialReq existingMreq, List<MaterialReqSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {
                var existingSubIds = existingMreq.MaterialReqSubs.Select(s => s.MreqSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.MreqSubId).ToHashSet();

                foreach (var sub in existingMreq.MaterialReqSubs.Where(s => !incomingSubIds.Contains(s.MreqSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - MreqSubId: {sub.MreqSubId}, Item: {sub.Item?.ItemCode}");

                    await _unitOfWork.MaterialReqSubs.DeleteAsync(sub.MreqSubId);

                    decimal oldQty = sub.PurchQty;

                    if (sub.RefBomId.HasValue && sub.RefBomId.Value > 0)
                        await AdjustBOMBalanceAsync(sub.RefBomId.Value, oldQty, 0, "MR Deletion");
                    else if (sub.RefPoSubId > 0)
                        await AdjustPoBalanceAsync(sub.RefPoSubId, sub.ItemId, oldQty, 0, "MR Deletion");
                }

                foreach (var subVM in incomingSubVMs)
                {
                    decimal newQty = subVM.PurchQty ?? 0m;

                    if (subVM.MreqSubId == 0)
                    {
                        var newSub = _mapper.Map<MaterialReqSub>(subVM);
                        newSub.MreqId = existingMreq.MreqId;

                        await _unitOfWork.MaterialReqSubs.CreateAsync(newSub);

                        if (subVM.RefBomId.HasValue && subVM.RefBomId.Value > 0)
                            await AdjustBOMBalanceAsync(subVM.RefBomId.Value, 0, newQty, "MR Creation");
                        else if (subVM.RefPoSubId > 0)
                            await AdjustPoBalanceAsync(subVM.RefPoSubId, subVM.ItemId, 0, newQty, "MR Creation");

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, PurchQty: {newQty}");
                    }
                    else
                    {
                        var existingSub = existingMreq.MaterialReqSubs.FirstOrDefault(s => s.MreqSubId == subVM.MreqSubId);
                        if (existingSub != null)
                        {
                            decimal oldQty = existingSub.PurchQty;

                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                            if (subVM.RefBomId.HasValue && subVM.RefBomId.Value > 0)
                                await AdjustBOMBalanceAsync(subVM.RefBomId.Value, oldQty, newQty, "MR Update");
                            else if (subVM.RefPoSubId > 0)
                                await AdjustPoBalanceAsync(subVM.RefPoSubId, subVM.ItemId, oldQty, newQty, "MR Update");

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }

                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "[HandleChildUpdatesAsync] Failed to update MaterialReq children");
                throw new InvalidOperationException("Failed to update Material Request details. Please contact support.");
            }
        }


        private async Task AdjustPoBalanceAsync(int? refpoSubId,int? itemId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refpoSubId.HasValue || refpoSubId == 0) return;

                if (!itemId.HasValue || itemId == 0) return;

                var isPoExist = false;

                isPoExist = await _unitOfWork.MfgPoSubs.GetQueryable()
                              .AnyAsync(p => p.PoSubId == refpoSubId.Value && p.ItemId == itemId.Value);

                if (!isPoExist) return;

                var poSub = await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(p => p.PoSubId == refpoSubId.Value && p.ItemId == itemId.Value).FirstOrDefaultAsync();

                if (poSub == null) return;

                if (oldQty > 0)
                    poSub.IndentBalQty += oldQty;

                if (newQty > poSub.IndentBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Sales Order Indent BalQty.");

                if (newQty > 0)
                    poSub.IndentBalQty -= newQty;

                await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();

            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust Po balance. Please contact support.");
            }
        }


        private async Task SetPRAuthorizationStatusAsync(MaterialReq entity, string currentUser)
        {
            var prAuthorityExists = await _unitOfWork.UserAuthorities
                .AnyAsync(x => x.IsPR == true);

            if (!prAuthorityExists)
            {
                entity.IsAuthorized = true;
                entity.LastApproved = currentUser;
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

        private async Task RevertIndentBalanceGroupedAsync(MaterialReq mreq)
        {
            var groupedSubs = mreq.MaterialReqSubs
                .Where(s => s.RefPoSubId > 0)
                .GroupBy(s => new { s.RefPoId, s.RefPoSubId, s.MainAssyId, s.MfgPoIndentQty })
                .Select(g => new
                {
                    g.Key.RefPoId,
                    g.Key.RefPoSubId,
                    g.Key.MainAssyId,
                    g.Key.MfgPoIndentQty
                })
                .ToList();

            foreach (var group in groupedSubs)
            {
                var poSub = await _unitOfWork.MfgPoSubs.FirstOrDefaultAsync(p => p.PoSubId == group.RefPoSubId);

                if (poSub != null)
                {
                    poSub.IndentBalQty += group.MfgPoIndentQty;

                    if (poSub.IndentBalQty > poSub.Qty)
                        poSub.IndentBalQty = poSub.Qty;

                    await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                    await _unitOfWork.SaveAsync();

                }
            }
            await _unitOfWork.SaveAsync();

        }

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

        public async Task UpdatedCancelStatusAndAddOrRevertQty(MaterialReqVM materialReqVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingPr = await _unitOfWork.MaterialReqs.GetAsync(materialReqVM.MreqId);
                if (existingPr == null)
                    throw new InvalidOperationException("Material Requisition not found.");

                var subs = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Where(s => s.MreqId == materialReqVM.MreqId)
                    .ToListAsync();

                if (!materialReqVM.MreqCancel)
                {
                    foreach (var sub in subs)
                    {
                        if(sub.RefBomId.GetValueOrDefault()> 0)
                            await ValidateBOMBalanceBeforeRevertAsync(sub);

                        else if(sub.RefPoSubId > 0)
                            await ValidatePoBalanceBeforeRevertAsync(sub);
                    }
                }

                existingPr.MreqCancel = materialReqVM.MreqCancel;
                existingPr.CancelReason = materialReqVM.CancelReason;
                existingPr.CancelDate = materialReqVM.CancelDate;
                existingPr.CancelBy = materialReqVM.CancelBy;

                await _unitOfWork.MaterialReqs.UpdateAsync(existingPr);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    decimal purchQty = sub.PurchQty; 

                    if (existingPr.MreqCancel)
                    {
                        if (sub.RefPoSubId.HasValue && sub.RefPoSubId > 0)
                        {
                            await AdjustPoBalanceAsync(
                                sub.RefPoSubId.Value,
                                sub.ItemId,
                                purchQty,
                                0,
                                $"PR Cancelled - {existingPr.MReqNo}"
                            );
                        }
                        else if (sub.RefBomId.HasValue && sub.RefBomId > 0)
                        {
                            await AdjustBOMBalanceAsync(
                                sub.RefBomId.Value,
                                purchQty,
                                0,
                                $"PR Cancelled - {existingPr.MReqNo}"
                            );
                        }
                    }
                    else
                    {
                        if (sub.RefPoSubId.HasValue && sub.RefPoSubId > 0)
                        {
                            await AdjustPoBalanceAsync(
                                sub.RefPoSubId.Value,
                                sub.ItemId,
                                0,
                                purchQty,
                                $"PR Reverted - {existingPr.MReqNo}"
                            );
                        }
                        else if (sub.RefBomId.HasValue && sub.RefBomId > 0)
                        {
                            await AdjustBOMBalanceAsync(
                                sub.RefBomId.Value,
                                0,
                                purchQty,
                                $"PR Reverted - {existingPr.MReqNo}"
                            );
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


        public async Task ValidateBOMBalanceBeforeRevertAsync(MaterialReqSub sub)
        {
            if (sub.RefBomId.GetValueOrDefault() <= 0)
                return;

            var bomEntity = await _unitOfWork.AssmblyDefs.GetAsync(sub.RefBomId.Value);
            if (bomEntity == null)
                throw new InvalidOperationException($"BOM not found for RefBomId: {sub.RefBomId}");

            if (bomEntity.BalQty < sub.PurchQty)
            {
                throw new InvalidOperationException($"Cannot revert because BOM balance ({bomEntity.BalQty}) is less than required quantity ({sub.PurchQty}).");
            }
        }

        public async Task ValidatePoBalanceBeforeRevertAsync(MaterialReqSub sub)
        {
            if (sub.RefPoSubId.GetValueOrDefault() <= 0 && sub.ItemId.GetValueOrDefault() <= 0)
                return;

            var isPoExist = await _unitOfWork.MfgPoSubs.GetQueryable()
                           .AnyAsync(p => p.PoSubId == sub.RefPoSubId && p.ItemId == sub.ItemId);

            if (!isPoExist) return;

            var poEntity = await _unitOfWork.MfgPoSubs.GetQueryable()
                .Where(m => m.PoSubId == sub.RefPoSubId && m.ItemId == sub.ItemId).FirstOrDefaultAsync();

            if (poEntity == null)
                throw new InvalidOperationException($"Po not found for RefPoSubId: {sub.RefPoSubId}");

            if (poEntity.IndentBalQty < sub.PurchQty)
            {
                throw new InvalidOperationException($"Cannot revert because PO Indent balance ({poEntity.IndentBalQty}) is less than required quantity ({sub.PurchQty}).");
            }
        }
        public async Task<decimal> GetROLByItemIdAsync(int itemId)
        {
            try
            {
                var rol = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .Where(i => i.ItemId == itemId)
                    .Select(i => i.ROL)
                    .FirstOrDefaultAsync();

                return rol ?? 0m;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching ROL value");
                throw new InvalidOperationException("Failed to fetch ROL.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetRolitemDetailsAsync(int storeid)
        {
            try
            {
                //var result = await (
                //        from I in _unitOfWork.ItemRepositories.GetQueryable()

                //        join PS in _unitOfWork.PurchPoSubs.GetQueryable()
                //             on I.ItemId equals PS.ItemId into poSubGroup
                //        from PS in poSubGroup.DefaultIfEmpty()

                //        join SA in _unitOfWork.StockAdds.GetQueryable()
                //            on I.ItemId equals SA.ItemId into stockGroup

                //        let totalQty = stockGroup
                //            .Where(x => x.StoreId == storeid)
                //            .Sum(x => (decimal?)x.BalQty) ?? 0

                //        where totalQty < I.ROL

                //        select new
                //        {
                //            I.ItemId,
                //            I.ItemCode,
                //            I.ItemName,
                //            I.HSNCode,
                //            I.MeasureUnit,
                //            CategoryName = I.Category.CategoryName,
                //            I.ROL,
                //            I.MOL,
                //            BalQty = totalQty,
                //            StoreId = storeid
                //        }

                //    ).ToListAsync();
                var items = _unitOfWork.ItemRepositories.GetQueryable();
                var stockAdds = _unitOfWork.StockAdds.GetQueryable();
                var poSubs = _unitOfWork.PurchPoSubs.GetQueryable();

                var result = await (
                    from i in items

                    let totalStockQty =
                        stockAdds
                            .Where(sa =>
                                sa.ItemId == i.ItemId &&
                                sa.StoreId == storeid)
                            .Sum(sa => (decimal?)sa.BalQty) ?? 0

                    let totalPoBalQty =
                        poSubs
                            .Where(ps => ps.ItemId == i.ItemId)
                            .Sum(ps => (decimal?)ps.BalQty) ?? 0

                    where totalStockQty < i.ROL

                    select new
                    {
                        i.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.HSNCode,
                        i.MeasureUnit,

                        CategoryName = i.Category != null
                            ? i.Category.CategoryName
                            : "",

                        i.ROL,
                        i.MOL,

                        BalQty = totalStockQty,
                        POBalQty = totalPoBalQty,

                        StoreId = storeid
                    }
                ).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["Itemid"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,
                    ["Category"] = r.CategoryName ?? string.Empty,
                    ["HSNCode"] = r.HSNCode ?? string.Empty,
                    ["ROL"] = r.ROL,
                    ["MOL"] = r.MOL,
                    ["StockQty"] = r.BalQty,
                    ["POBalQty"] = r.BalQty
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching po details");
                throw new InvalidOperationException("Failed to retrieve po details. Please try again.");
            }
        }


        public async Task<List<Dictionary<string, object>>> GetRolitemDetailAsync()
        {
            //var result = await (
            //               from SA in _unitOfWork.StockAdds.GetQueryable()

            //               join I in _unitOfWork.ItemRepositories.GetQueryable()
            //               on SA.ItemId equals I.ItemId
            //               join S in _unitOfWork.Stores.GetQueryable()
            //                on SA.StoreId equals S.StoreId

            //               group new { SA, I, S } by new
            //               {
            //                   I.ItemId,
            //                   I.ItemCode,
            //                   I.ItemName,
            //                   I.HSNCode,
            //                   I.MeasureUnit,
            //                   CategoryName = I.Category.CategoryName,
            //                   I.ROL,
            //                   I.MOL,
            //                   S.StoreName,
            //                   I.CategoryCode,
            //                   SA.StoreId
            //               }
            //               into g

            //               select new
            //               {
            //                   g.Key.ItemId,
            //                   g.Key.ItemCode,
            //                   g.Key.ItemName,
            //                   g.Key.HSNCode,
            //                   g.Key.MeasureUnit,
            //                   g.Key.CategoryName,
            //                   g.Key.ROL,
            //                   g.Key.MOL,
            //                   g.Key.StoreName,
            //                   g.Key.CategoryCode,
            //                   g.Key.StoreId,
            //                   TotalBalQty = g.Sum(x => x.SA.BalQty),

            //               }
            //               ).ToListAsync();
            var poSubs = _unitOfWork.PurchPoSubs.GetQueryable();

            var result = await (
                from SA in _unitOfWork.StockAdds.GetQueryable()

                join I in _unitOfWork.ItemRepositories.GetQueryable()
                    on SA.ItemId equals I.ItemId

                join S in _unitOfWork.Stores.GetQueryable()
                    on SA.StoreId equals S.StoreId

                group new { SA, I, S } by new
                {
                    I.ItemId,
                    I.ItemCode,
                    I.ItemName,
                    I.HSNCode,
                    I.MeasureUnit,
                    CategoryName = I.Category.CategoryName,
                    I.ROL,
                    I.MOL,
                    S.StoreName,
                    I.CategoryCode,
                    SA.StoreId
                }
                into g

                select new
                {
                    g.Key.ItemId,
                    g.Key.ItemCode,
                    g.Key.ItemName,
                    g.Key.HSNCode,
                    g.Key.MeasureUnit,
                    g.Key.CategoryName,
                    g.Key.ROL,
                    g.Key.MOL,
                    g.Key.StoreName,
                    g.Key.CategoryCode,
                    g.Key.StoreId,

                    TotalBalQty = g.Sum(x => x.SA.BalQty),

                    POBalQty = poSubs
                        .Where(ps => ps.ItemId == g.Key.ItemId)
                        .Sum(ps => (decimal?)ps.BalQty) ?? 0
                }
            ).ToListAsync();
            return result.Select(r => new Dictionary<string, object>
            {
                ["Selected"] = false,
                ["Itemid"] = r.ItemId,  
                ["ItemCode"] = r.ItemCode ?? string.Empty,
                ["ItemName"] = r.ItemName ?? string.Empty,
                ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,
                ["Category"] = r.CategoryName ?? string.Empty,
                ["HSNCode"] = r.HSNCode ?? string.Empty,
                ["ROL"] = r.ROL,
                ["MOL"] = r.MOL,
                ["StockQty"] = r.TotalBalQty,
                ["StoreName"] = r.StoreName,
                ["StoreId"] = r.StoreId,
                ["CategoryCode"] = r.CategoryCode,
                ["POBalQty"]= r.POBalQty

            }).ToList();

        }


        public async Task<List<MaterialReqPendingVM>> GetMreqReqPendingList(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<MaterialReqPendingVM>("Sp_GetMaterialReq", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }

}
