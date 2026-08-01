using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Vml.Office;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ProductionService
{
    public class ProductionReturnCompService : IProductionReturnCompService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly IProductionSCNAssyservice _productionSCNAssyService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        IProductionSCNCompService _scnService;
        public ProductionReturnCompService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            IStockManagerService stockManagerService,
            IProductionSCNAssyservice productionSCNAssyService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper,IProductionSCNCompService scnService)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _productionSCNAssyService = productionSCNAssyService;
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;
            _scnService = scnService;
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

        //Stores
        public async Task<List<Store>> GetAllActiveStoresAsync()
        {
            var result = await _commonService.GetAllAddStoresAsync();
            return result.ToList();
        }

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        public async Task<bool> IsPOWiseProductionDcOutEnabledAsync()
         => await _commonService.GetScreenPermissionsAsync("Production Issue WO Component", "PO Wise Production Issue WO Component");

        public async Task<bool> GetIsSameItemasInAsync()
           => await _commonService.GetScreenPermissionsAsync("Production Issue WO Component", "Restrict Out Item Same As In Item");
        public async Task<IEnumerable<CustomerVM>> SearchCustomers(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        public async Task<List<ShiftAllocation>> GetAllShifts()
        {
            var shifts = await _commonService.GetAllShiftsAsync();
            return shifts.ToList();
        }
        public async Task<List<Process>> GetAllProcess()
        {
            return (await _commonService.GetAllProcessAsync()).ToList();
        }

        public async Task<bool> IsProductionLogAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.AnyAsync(i =>
                               i.ScreenName == "Production Return Component" &&
                               i.Display == "Production Log Configuration" &&
                               i.Required);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error checking Production Log configuration");
                throw new InvalidOperationException("Failed to check Production Log configuration. Please try again.", ex);
            }
        }


        //Machines
        public Task<List<Machine>> GetAllActiveMachinesAsync()
            => _commonService.GetAllMachineAsync();

        // 🔹 Production Return operations
        public async Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId)
        {
            try
            {
                // 1️⃣ Fetch PO data
                var poData = await (
                    from p in _unitOfWork.MfgPos.GetQueryable()
                    join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                        on p.PoId equals ps.PoId
                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId
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
                        i.CategoryCode,
                        i.Category.CategoryName,

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

                // 2️⃣ Build Result
                var result = poData.Select(r => new Dictionary<string, object>
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
                    ["Category"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.ProdCompBalQty,
                    ["BalQty"] = r.ProdCompBalQty,

                    ["UnitPrice"] = r.UnitPrice,
                    ["PoDuedate"] = r.DueDate,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Remark"] = r.MainRemark ?? string.Empty

                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PO details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.", ex);
            }
        }


        public async Task<List<Dictionary<string, object>>> GetPoDetailsBySelectedPoidCustId(List<int> poIds)
        {
            try
            {

              
                // 1️⃣ Fetch PO data
                var poData = await (
                    from p in _unitOfWork.MfgPos.GetQueryable()
                    join ps in _unitOfWork.MfgPoSubs.GetQueryable()
                        on p.PoId equals ps.PoId
                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId
                    where poIds.Contains(p.PoId) && !p.PoTally
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
                        i.CategoryCode,
                        i.Category.CategoryName,

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

                // 2️⃣ Build Result
                var result = poData.Select(r => new Dictionary<string, object>
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
                    ["Category"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.ProdCompBalQty,
                    ["BalQty"] = r.ProdCompBalQty,

                    ["UnitPrice"] = r.UnitPrice,
                    ["PoDuedate"] = r.DueDate,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Remark"] = r.MainRemark ?? string.Empty

                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PO details ");
                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.", ex);
            }
        }
        public async Task<List<int>> GetIssueIdsByPoSubIdAsync(int poSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.RefPoSubId == poSubId && x.IssueId.HasValue)
                    .Select(x => x.IssueId.Value)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching IssueIds for PoSubId: {poSubId}");
                throw new InvalidOperationException("Failed to retrieve Issue details for the selected PO. Please try again.", ex);
            }
        }

        public async Task<List<int>> GetIssueIdsByRcSubIdAsync(int rcSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.RcSubId == rcSubId && x.IssueId.HasValue)
                    .Select(x => x.IssueId.Value)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching IssueIds for RcSubId: {rcSubId}");
                throw new InvalidOperationException("Failed to retrieve Issue details for the selected Route Card. Please try again.", ex);
            }
        }

        public async Task<List<ProductionIssueCompSubVM>> GetIssuedQtyByIssueIdsAsync(List<int> issueIds)
        {
            if (issueIds == null || !issueIds.Any())
                return new List<ProductionIssueCompSubVM>();

            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.IssueId.HasValue
                             && issueIds.Contains(x.IssueId.Value)
                             && x.TransType == "Out")
                    .GroupBy(x => x.ItemId)
                    .Select(g => new ProductionIssueCompSubVM
                    {
                        ItemId = g.Key,
                        BalQty = g.Sum(x => x.BalQty)
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching issued quantities for IssueIds: {string.Join(",", issueIds)}");
                throw new InvalidOperationException("Failed to retrieve issued quantity details. Please try again.", ex);
            }
        }
        public async Task<RouteCardSub?> GetRcDetailsByRcSubIdAsync(int rcSubId)
        {
            try
            {
                return await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Include(x => x.RouteCard)
                    .Where(x => x.RCSubId == rcSubId)
                    .Select(x => x)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetRcDetailsByRcSubIdAsync failed for RCSubId : {rcSubId}");
                throw;
            }
        }



        public async Task<int> GetPendingJobOrdersCountAsync()
        {
            try
            {
                return await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Where(j => j.IssuedQty > 0)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching pending job orders count");
                throw new InvalidOperationException("Failed to retrieve pending job orders count. Please try again.", ex);
            }
        }



        public async Task<(List<ProductionReturnCompVM> returnCompVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ProductionReturnComps.GetQueryable()
                .Include(j => j.ProductionReturnCompSubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.ProductionReturnCompSubs)
                    .ThenInclude(s => s.RouteCardSubs)
                .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = MaterialReturnFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.ReturnId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<ProductionReturnCompVM>>(list);

            return (vmList, total);
        }

        public static class MaterialReturnFilterBuilder
        {
            public static IQueryable<ProductionReturnComp> ApplyFilter(
                IQueryable<ProductionReturnComp> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "ReturnNo":
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
                                (string.IsNullOrEmpty(part1) || x.ReturnNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "JobNo":
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
                                x.ProductionReturnCompSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.RouteCardSubs.RouteCard.RCNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.RouteCardSubs.RouteCard.Suffix.Contains(part2))
                                )
                            );
                        }


                    case "ItemCode":
                        return query.Where(x => x.ProductionReturnCompSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.ProductionReturnCompSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.ReturnDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.ReturnDateNow <= toDate);
                        break;
                }

                return query;
            }

            private static IQueryable<ProductionReturnComp> ApplyStatusFilter(
                IQueryable<ProductionReturnComp> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.ReturnTally == true),
                    "Pending" => query.Where(x => x.ReturnTally == false),
                    "Cancelled" => query.Where(x => x.Cancel == true),
                    "Short Closed" => query.Where(x => x.ShortClose == true),
                    _ => query
                };
            }
        }
        public async Task<bool> DeleteByDcsubConIncomingSubIdAsync(int returnSubId, int screenCode)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
                var SubConGRNs = await _unitOfWork.ProductionReturnComps
                    .GetQueryable()
                    .Include(e => e.ProductionReturnCompSubs)
                    .Where(e => e.ProductionReturnCompSubs
                        .Any(s => s.ReturnSubId == returnSubId))
                    .FirstOrDefaultAsync();

                if (SubConGRNs == null)
                    return false;

                var subItem = SubConGRNs.ProductionReturnCompSubs
                    .FirstOrDefault(s => s.ReturnSubId == returnSubId);

                if (subItem == null)
                    return false;

                if (subItem.ReturnSubId > 0)
                {
                    if (subItem.TransType == "In")
                    {
                        //await RollbackTracksAddIssueQtyAsync(subItem.ReturnSubId);
                        //await DeleteStockAddAsync(subItem.ReturnSubId, subItem.ItemId.Value, screenCode, SubConGRNs.ReturnNo);

                        //if (PoWiseDcOut)
                        //{
                        //    if (subItem.RefPoSubId > 0 && subItem.TransType == "In")
                        //        await AdjustMfgPoSubItemBalanceAsync(subItem.RefPoSubId.Value, subItem.Qty ?? 0, 0, "Production Return Assy Delete");
                        //}


                        await RollbackTracksAddIssueQtyAsync(subItem.ReturnSubId);

                        if (!SubConGRNs.Rejection && !SubConGRNs.Return)
                        {
                            if (subItem.RefRcSubId.GetValueOrDefault() > 0)
                                await AdjustRcBalanceAsync(subItem.RefRcSubId, subItem.Qty ?? 0, 0, "SubContract GRN Delete");

                            else if (subItem.RefIssueSubId.GetValueOrDefault() > 0)
                                await AdjustProductionCompIssueItemBalanceAsync(subItem.RefIssueSubId.Value, subItem.Qty ?? 0, 0, "SubContract GRN Delete");

                            if (PoWiseDcOut && subItem.RefPoSubId.GetValueOrDefault() > 0)
                                await AdjustMfgPoSubItemBalanceAsync(subItem.RefPoSubId.Value, subItem.ItemId.Value, subItem.Qty ?? 0, 0, "SubContract GRN Delete");
                        }
                        else
                        {
                            await AdjustProductionCompIssueItemBalanceAsync(subItem.RefIssueSubId, subItem.Qty ?? 0, 0, "Production return Component delete");

                            if (subItem.RefRcSubId.GetValueOrDefault() > 0)
                                await RevertRCSubIssuedToBalQtyAsync(subItem.RefRcSubId, subItem.Qty ?? 0, 0, "Production return Component Return/Rejection Delete");
                        }

                        await DeleteStockAddAsync(subItem.ReturnSubId, subItem.ItemId ?? 0, screenCode, SubConGRNs.ReturnNo);

                        await _unitOfWork.ProductionReturnCompSubs.DeleteAsync(subItem.ReturnSubId);
                    }


                }
                if ((!subItem.RefPoSubId.HasValue || subItem.RefPoSubId <= 0)
                  && (!subItem.RefRcSubId.HasValue || subItem.RefRcSubId <= 0))
                {
                    if (subItem.TransType == "Out")
                    {
                        await AdjustProductionCompIssueItemBalanceAsync(subItem.RefIssueSubId ?? 0, subItem.Qty ?? 0, 0, "Production Dc Incoming  Delete");

                        // First get GRNId from selected GRNSubId
                        var grnId = await _unitOfWork.ProductionReturnCompSubs
                            .GetQueryable()
                            .Where(x => x.ReturnSubId == returnSubId)
                            .Select(x => x.ReturnId)
                            .FirstOrDefaultAsync();

                        if (grnId > 0)
                        {
                            // Get ALL GRNSubIds under that GRNId
                            var grnSubIds = await _unitOfWork.ProductionReturnCompSubs
                                .GetQueryable()
                                .Where(x => x.ReturnId == grnId)
                                .Select(x => x.ReturnSubId)
                                .ToListAsync();

                            // Get matching tracks
                            var subConGRNTracks = await _unitOfWork.ProductionReturnCompTracks
                                .GetQueryable()
                                .Where(j =>
                                    grnSubIds.Contains(j.RefReturnSubId) &&
                                    j.RefIssueSubId == subItem.RefIssueSubId && j.ItemIdOut == subItem.ItemId)
                                .ToListAsync();

                            // Delete tracks
                            foreach (var track in subConGRNTracks)
                            {
                                await _unitOfWork.ProductionReturnCompTracks.DeleteAsync(track);
                            }
                        }
                    }
                }
             
                await _unitOfWork.ProductionReturnCompSubs.DeleteAsync(subItem);
                await _unitOfWork.SaveAsync();

                var remaining = await _unitOfWork.ProductionReturnCompSubs
                  .GetQueryable()
                  .Where(x => x.ReturnId == SubConGRNs.ReturnId)
                  .OrderBy(x => x.SlNo)
                  .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<(bool CanDelete, string Message)> CanDeleteProductionReturnCompAsync(int returnId, int screenCode)
        {
            try
            {
                var grn = await _unitOfWork.ProductionReturnComps
                              .GetQueryable()
                              .Include(e => e.ProductionReturnCompSubs)
                              .Where(e => e.ReturnId == returnId).FirstOrDefaultAsync();

                if (grn == null)
                    return (true, "Production Return can be safely deleted.");

                var grnSubIds = grn.ProductionReturnCompSubs
                    .Select(es => es.ReturnSubId)
                    .ToList();

                bool hasSCN = await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefGRNSubId.HasValue &&
                        grnSubIds.Contains(qs.RefGRNSubId.Value));

                if (hasSCN)
                    return (false, "Cannot delete this Production Return as a Production SCN exists.");

                if (grn.Cancel || grn.ShortClose)
                    return (false, "Cannot delete this Production Return as it is Cancelled or Short-Closed.");

                var RcSubIds = grn.ProductionReturnCompSubs
                    .Select(es => es.RefRcSubId)
                    .ToList();

                //bool hasRC = await _unitOfWork.RouteCardSubs
                //             .GetQueryable()
                //             .AnyAsync(qs => RcSubIds.Contains(qs.RCSubId));
                //if (hasRC)
                //    return (false, "Cannot delete this Production Return as a RouteCard exists.");

                var GRNSubIds = await _unitOfWork.ProductionReturnCompSubs
                               .GetQueryable()
                               .Where(s => s.ReturnId == returnId)
                               .Select(s => s.ReturnSubId)
                               .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                                .Where(sa =>
                                    GRNSubIds.Contains(sa.SubItemRefID) &&
                                    sa.ScreenCode == screenCode &&
                                    sa.BalQty < sa.AddQty)
                                .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Production Return. Some sub-items have already been transacted/issued.");

                return (true, "Production Return can be safely deleted.");
              
               
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteProductionReturnAssyAsync for ReturnId: {returnId}");
                throw new Exception("Error checking Production Return Assy delete eligibility", ex);
            }
        }


        public async Task<(bool IsValid, string Message)> ValidateDeleteAsync(int jobId, int itemId, decimal qtyReturned, int addStoreId)
        {
            var jobOrderSub = await _unitOfWork.JobOrderSubs
                .GetQueryable()
                .Where(j => j.JobId == jobId && j.ItemId == itemId)
                .FirstOrDefaultAsync();

            if (jobOrderSub != null)
            {
                if (jobOrderSub.BalQty < qtyReturned)
                {
                    return (false, $"Cannot delete. Job Order balance ({jobOrderSub.BalQty}) is less than returned qty ({qtyReturned}).");
                }
            }

            return (true, "OK");
        }

        public async Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(int screenCode, int storeId, int itemId, int subItemRefId)
        {
            try
            {
                return await _stockManagerService.GetQtyBalQtyByStockAddAsync(screenCode, storeId, itemId, subItemRefId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching stock details for Screen Code: {screenCode}, SubItemRefId: {subItemRefId}");
                throw new InvalidOperationException("Failed to retrieve SCNGen sub-item stock details.");
            }
        }

        public async Task<List<int>> GetDistinctRcSubIdsWithPendingIssueAsync()
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.TransType == "Out"
                             && x.BalQty > 0
                             && x.RcSubId.HasValue)
                    .Select(x => x.RcSubId.Value)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching distinct RcSubIds with pending issued quantity");
                throw new InvalidOperationException("Failed to retrieve pending Route Card issue details. Please try again.", ex);
            }
        }


        public async Task<List<Dictionary<string, object>>> GetAllOpenRcDetailsAsync(int? custId)
        {

            var result = new List<Dictionary<string, object>>();

            try
            {
                var rcSubIds = await GetDistinctRcSubIdsWithPendingIssueAsync();

                if (!rcSubIds.Any())
                    return result;

                var routeCardSubs = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Include(x => x.IncomingItem)
                    .Include(x => x.Process)
                    .Include(x => x.Machine)
                    .Include(x => x.RouteCard)
                        .ThenInclude(r => r.CostCenter)
                    .Where(x => rcSubIds.Contains(x.RCSubId))
                    .ToListAsync();

                var groupedData = routeCardSubs
                    .GroupBy(x => new { x.ItemIdIn, x.RCSubId })
                    .Select(g =>
                    {
                        var first = g.First();

                        var seqNo = first.SeqNo;

                        var utility = seqNo == 1
                            ? (first.RouteCard?.RMWeight ?? 1)
                            : 1;

                        return new
                        {
                            ItemId = g.Key.ItemIdIn,
                            RefRcSubId = g.Key.RCSubId,

                            Qty = g.Sum(x => x.IssuedQty),

                            SeqNo = seqNo,
                            Utility = utility,

                            ItemCode = first.IncomingItem?.ItemCode ?? string.Empty,
                            ItemName = first.IncomingItem?.ItemName ?? string.Empty,
                            MeasureUnit = first.IncomingItem?.MeasureUnit ?? string.Empty,
                            CategoryCode = first.IncomingItem?.CategoryCode,
                            Category = first.IncomingItem?.Category?.CategoryName ?? string.Empty,

                            RCNo = first.RouteCard != null
                                ? $"{first.RouteCard.RCNo}{first.RouteCard.Suffix}"
                                : string.Empty,

                            RCDt = first.RouteCard?.RCDate,

                            ProcessId = first.ProcessId,
                            ProcessName = first.Process?.ProcessName ?? string.Empty,

                            MachineId = first.MachineId,
                            MachineName = first.Machine?.MachineName ?? string.Empty,

                            CostCenterId = first.RouteCard?.CostId,
                            CostCenter = first.RouteCard?.CostCenter?.ProjectNo ?? string.Empty
                        };
                    }).ToList();

                var itemIds = groupedData
                    .Where(x => x.ItemId.HasValue)
                    .Select(x => x.ItemId!.Value)
                    .Distinct()
                    .ToList();

                var unitPriceDict = await GetBulkLastUnitPricesAsync(itemIds, custId);

                foreach (var rcSub in groupedData)
                {
                    result.Add(new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["RefRcSubId"] = rcSub.RefRcSubId,
                        ["RCNo"] = rcSub.RCNo,
                        ["RCDt"] = rcSub.RCDt,

                        ["TransType"] = "In",
                        ["SeqNo"] = rcSub.SeqNo,

                        ["ItemId"] = rcSub.ItemId,
                        ["ItemCode"] = rcSub.ItemCode,
                        ["ItemName"] = rcSub.ItemName,
                        ["MeasureUnit"] = rcSub.MeasureUnit,

                        ["CategoryCode"] = rcSub.CategoryCode,
                        ["Category"] = rcSub.Category ?? string.Empty,

                        ["Qty"] = rcSub.Qty,
                        ["Utility"] = rcSub.Utility,

                        ["UnitPrice"] = rcSub.ItemId.HasValue &&
                                        unitPriceDict.TryGetValue(rcSub.ItemId.Value, out var rate)
                                            ? rate
                                            : 0m,

                        ["ProcessId"] = rcSub.ProcessId,
                        ["ProcessName"] = rcSub.ProcessName,
                        ["MachineId"] = rcSub.MachineId,
                        ["MachineName"] = rcSub.MachineName,

                        ["CostCenterId"] = rcSub.CostCenterId,
                        ["CostCenter"] = rcSub.CostCenter
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching open Route Card details");
                throw new InvalidOperationException("Failed to retrieve open Route Card details. Please try again.", ex);
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

                    rate = await (from qs in _unitOfWork.ProductionReturnCompSubs.GetQueryable()
                                  join q in _unitOfWork.ProductionReturnComps.GetQueryable()
                                      on qs.ReturnId equals q.ReturnId
                                  where qs.ItemId == itemId && q.CustId == custId
                                  orderby q.ReturnId descending
                                  select qs.UnitPrice)
                                  .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.ProductionReturnCompSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.ReturnSubId descending
                                      select qs.UnitPrice)
                                      .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId
                                            && isub.CustomerId == custId
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
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.", ex);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetAllProductionIssuedItemsAsync()
        {
            try
            {
                var data = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(i => i.BalQty > 0 && i.TransType == "Out")
                    .Select(j => new
                    {
                        j.IssueSubId,
                        j.RcSubId,

                        RCNo = j.ComponentRouteCardSub.RouteCard.RCNo,
                        RCSuffix = j.ComponentRouteCardSub.RouteCard.Suffix,
                        RCDate = j.ComponentRouteCardSub.RouteCard.RCDate,

                        IssueNo = j.ProductionIssueComp.IssueNo,
                        IssueSuffix = j.ProductionIssueComp.Suffix,
                        IssueDate = j.ProductionIssueComp.IssueDate,

                        j.ItemId,
                        ItemCode = j.Item.ItemCode,
                        ItemName = j.Item.ItemName,
                        MeasureUnit = j.Item.MeasureUnit,

                        CategoryCode = j.Item.CategoryCode,
                        CategoryName = j.Item.Category.CategoryName,

                        j.BalQty,
                        j.UnitPrice,

                        j.ProcessId,
                        ProcessName = j.Process.ProcessName,

                        j.MachineId,
                        MachineName = j.Machine.MachineName,

                        CostCenterId = j.CostId,
                        ProjectNo = j.CostCenter.ProjectNo
                    })
                    .ToListAsync();

                return data.Select(r => new Dictionary<string, object>
                {

                    ["Selected"] = false,

                    ["TransType"] = "Out",

                    ["IssueSubId"] = r.IssueSubId,
                    ["RefRcSubId"] = r.RcSubId,

                    ["RCNo"] = $"{r.RCNo}{r.RCSuffix}",
                    ["RCDt"] = r.RCDate,

                    ["IssueNo"] = $"{r.IssueNo}{r.IssueSuffix}",
                    ["IssueDt"] = r.IssueDate,

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode,
                    ["ItemName"] = r.ItemName,
                    ["MeasureUnit"] = r.MeasureUnit,

                    ["CategoryCode"] = r.CategoryCode,
                    ["Category"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,

                    ["ProcessId"] = r.ProcessId,
                    ["ProcessName"] = r.ProcessName,

                    ["MachineId"] = r.MachineId,
                    ["MachineName"] = r.MachineName,

                    ["CostCenterId"] = r.CostCenterId,
                    ["CostCenter"] = r.ProjectNo
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching Production Issue items");
                throw new InvalidOperationException(
                    "Failed to retrieve open Production Issue items. Please try again."
                );
            }
        }

        public async Task<bool> DeleteProdAssyReturnByReturnIdAsync(int returnId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                bool IsPoWise = await IsPOWiseProductionDcOutEnabledAsync();
                var grn = await _unitOfWork.ProductionReturnComps
                    .GetQueryable()
                    .Include(e => e.ProductionReturnCompSubs)
                        .ThenInclude(s => s.Item)
                    .FirstOrDefaultAsync(e => e.ReturnId == returnId);

                if (grn == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in grn.ProductionReturnCompSubs)
                {
                    changes.AppendLine($"Child Deleted - ReturnSubId: {sub.ReturnSubId}, Item: {sub.Item?.ItemCode}");

                    if (sub.ReturnSubId > 0)
                        await RollbackTracksAddIssueQtyAsync(sub.ReturnSubId);



                    if (!grn.Rejection && !grn.Return)
                    {
                        bool isSameItem = false;

                        if (sub.RefPoSubId.HasValue)
                        {
                            isSameItem = await CheckIsSameAsOutItemByPoSubId(sub.RefPoSubId.Value);
                        }

                        

                        bool grnexit = false;

                        if(sub.RefIssueSubId.HasValue)
                        {
                            grnexit = await CheckLabourGrnExist(sub.RefIssueSubId.Value);
                        }
                       

                        if ((grn.IsManual || isSameItem || grnexit) && sub.RefIssueSubId.GetValueOrDefault() > 0 && sub.TransType == "In")
                        {
                            await AdjustProductionCompIssueItemBalanceAsync(
                               sub.RefIssueSubId.Value,
                               sub.Qty ?? 0,
                               0,
                               "Production Return Delete"
                           );
                        }
                    }
                    if (sub.RefRcSubId.HasValue && sub.RefRcSubId > 0 && sub.TransType == "In")
                    {

                        await AdjustRcBalanceAsync(sub.RefRcSubId, sub.Qty ?? 0, 0, "Production Return Deleted");
                        var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

                        var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                                           .GetQueryable()
                                           .Where(x => issueIds.Contains(x.IssueId.Value)
                                               && x.TransType == "In")
                                           .OrderBy(x => x.IssueId)
                                           .ToListAsync();

                        if (issueSubs.Any())
                        {
                            foreach (var Grnsub in issueSubs)
                            {
                                await AdjustProductionCompIssueItemBalanceAsync(Grnsub.IssueSubId, sub.Qty ?? 0, 0, "Production Return deleted");
                            }

                        }

                    }

                    // 🔹 Stock Delete Entry
                    if (sub.ItemId.HasValue && sub.TransType == "In")
                    {
                        await DeleteStockAddAsync(
                            sub.ReturnSubId,
                            sub.ItemId.Value,
                            screenCode,
                            grn.ReturnNo
                        );
                    }
                    if (IsPoWise)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0 && sub.TransType == "In")
                        {
                            await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value,sub.ItemId.Value, sub.Qty ?? 0, 0, "Production GRN Delete");
                        }

                    }
                }

                // Delete child records (CORRECT REPO)
                await _unitOfWork.ProductionReturnCompSubs.DeleteRangeAsync(grn.ProductionReturnCompSubs);

                // Delete parent
                await _unitOfWork.ProductionReturnComps.DeleteAsync(grn);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Logging
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Production Return",
                    action: $"Deleted Production Return: {grn.ReturnNo}",
                    additionalInfo: $"Production Id: {grn.ReturnId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Production Return ID: {returnId}");
                throw;
            }

        }
        public async Task<bool> CheckLabourGrnExist(int? refIssueSubId)
        {
            try
            {
                if (!refIssueSubId.HasValue || refIssueSubId == 0)
                    return false;

                var compIssueSub = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(p => p.IssueSubId == refIssueSubId);

                if (compIssueSub == null)
                    return false;

                var isExist = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .AnyAsync(x => x.GRNSubId == compIssueSub.RefGRNSubId);

                return isExist;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public async Task<ProductionReturnCompVM?> GetProductionByReturnIdAsync(int returnId)
        {
            try
            {
                var entity = await _unitOfWork.ProductionReturnComps.GetQueryable()
                    .Include(q => q.AddStore)
                    .Include(q => q.ProductionReturnCompSubs)
                    .Include(q => q.ProductionReturnCompSubs)
                    .ThenInclude(s => s.Item)
                    .Include(q => q.ProductionReturnCompSubs)
                    .ThenInclude(s => s.CostCenter)
                    .Include(q => q.ProductionReturnCompSubs)
                    .ThenInclude(s => s.Process)
                    .Include(q => q.ProductionReturnCompSubs)
                    .ThenInclude(s => s.Machine)
                    .Include(q => q.ProductionReturnCompSubs)
                    .ThenInclude(s => s.RouteCardSubs)
                    .FirstOrDefaultAsync(q => q.ReturnId == returnId);
                if (entity == null)
                    return null;

                var vm = _mapper.Map<ProductionReturnCompVM>(entity);
                return vm;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetProductionByReturnIdAsync({returnId})");
                return null;
            }
        }

        public async Task<string> GetProductionReturnNumberAsync(string suffix)
        {
            try
            {
                var lastProdIssue = await _unitOfWork.ProductionReturnComps
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => Convert.ToInt32(q.ReturnNo))
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastProdIssue != null)
                {
                    var parts = lastProdIssue.ReturnNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Production Return number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Production Return number.");
            }
        }

        public async Task DeleteAndResequenceAsync(ProductionReturnCompSubVM subitem, ProductionReturnCompVM productionReturnCompVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.ReturnSubId > 0)
                {
                    var entity = await _unitOfWork.ProductionReturnCompSubs.GetAsync(subitem.ReturnSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");



                    if (!productionReturnCompVM.Rejection && !productionReturnCompVM.Return)
                    {
                        if (subitem.ReturnSubId > 0)
                            await RollbackTracksAddIssueQtyAsync(subitem.ReturnSubId);

                        //await AdjustJobOrderBalanceAsync(subitem.RefJobOrderId, subitem.QtyReturned ?? 0, 0, "Production return Assy Delete");
                    }
                    else
                    {
                        await AdjustProductionCompIssueItemBalanceAsync(subitem.RefIssueSubId, subitem.Qty ?? 0, 0, "Production Return Component Delete");

                        //await AdjustMfgPoSubItemBalanceAsync(subitem.RefJobOrderId.Value, subitem.ItemId.Value, subitem.QtyReturned ?? 0, 0, "Production Return Assy Delete");
                    }
                    await DeleteStockAddAsync(subitem.ReturnSubId, subitem.ItemId.Value, screenCode, productionReturnCompVM.ReturnNo);


                    await _unitOfWork.ProductionReturnCompSubs.DeleteAsync(entity.ReturnSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Production Return Assembly",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Return No: {productionReturnCompVM?.ReturnNo}"
                    );
                }
                else
                {
                    productionReturnCompVM.ProductionReturnCompSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.ProductionReturnCompSubs
                    .GetQueryable()
                    .Where(x => x.ReturnId == productionReturnCompVM.ReturnId)
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

        public async Task<List<ProductionReturnCompSubVM>> GetProductionReturnSubByReturnIdAsync(int returnId)
        {
            try
            {
                var subs = await _unitOfWork.ProductionReturnCompSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.ReturnId == returnId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ProductionReturnCompSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production Return sub items for Return: {returnId}");
                throw new InvalidOperationException("Failed to retrieve Production Return sub-items. Please try again.");
            }
        }

        public async Task<decimal> GetJobOrderBalQtyFromJobId(int jobId)
        {
            try
            {
                return await _unitOfWork.JobOrders.GetQueryable()
                    .Where(e => e.JobId == jobId)
                    .Select(e => e.JobOrderQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for JobId: {jobId}");
                throw new InvalidOperationException("Failed to retrieve Job Order Balance quantity.");
            }
        }
        public async Task<ProductionReturnCompSubVM?> GetProductionReturnSubItemDetailByReturnSubIdAsync(int returnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionReturnCompSubs
                    .GetQueryable()
                    .Where(q => q.ReturnSubId == returnSubId)
                    .Select(q => new ProductionReturnCompSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching production sub item detail for ReturnSubid: {returnSubId}");
                throw new InvalidOperationException("Failed to retrieve production sub-item details.");
            }
        }


        public async Task<ProductionReturnCompVM> UpsertProductionReturnAsync(ProductionReturnCompVM prodReturnCompVM, int screenCode)
        {
            if (prodReturnCompVM == null)
                throw new ArgumentNullException(nameof(prodReturnCompVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ProductionReturnComp entity;

                if (prodReturnCompVM.ReturnId == 0)
                {
                    entity = _mapper.Map<ProductionReturnComp>(prodReturnCompVM);

                    var nextNumber = await _unitOfWork.ProductionReturnComps.GetLastReturnNoAsync(entity.Suffix);
                    entity.ReturnNo = nextNumber;
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.ProductionReturnCompSubs = prodReturnCompVM.ProductionReturnCompSubVMs
                        .Select(s => _mapper.Map<ProductionReturnCompSub>(s))
                        .ToList();

                    await _unitOfWork.ProductionReturnComps.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.ProductionReturnCompSubs)
                    {

                        if (!entity.Rejection && !entity.Return)
                        {

                            if (sub.RefRcSubId.GetValueOrDefault() > 0)
                            {
                                await CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.ReturnSubId, sub.RefRcSubId.Value,
                                    sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now);

                                await UpsertRCSubWipIssuedQtyAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "Production return Component Create");


                                var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

                                var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                                                   .GetQueryable()
                                                   .Where(x => issueIds.Contains(x.IssueId.Value) &&
                                                       (x.BalQty ?? 0) > 0
                                                       && x.TransType == "In")
                                                   .OrderBy(x => x.IssueSubId)
                                                   .ToListAsync();

                                if (issueSubs.Any())
                                {
                                    foreach (var Grnsub in issueSubs)
                                    {
                                        await AdjustProductionCompIssueItemBalanceAsync(Grnsub.IssueSubId, 0, sub.Qty ?? 0, "Production Return Create");
                                    }

                                }

                                if (sub.TransType == "In" && sub.RefIssueSubId > 0)
                                {
                                    await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, 0, sub.Qty ?? 0, "Production Return Create");
                                }
                            }
                            else if (sub.TransType == "In" && sub.RefIssueSubId.GetValueOrDefault() > 0 && !PoWiseDcOut)
                            {
                                await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.ReturnSubId, sub.RefIssueSubId.Value,
                                    sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now, entity.IsManual, prodReturnCompVM.ProductionReturnCompSubVMs, sub?.RefPoSubId);

                                await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId, 0, sub.Qty ?? 0, "Production return Component Create");
                            }

                            if (PoWiseDcOut)
                            {
                                if (sub.TransType == "In" && sub.RefRcSubId == null)
                                {
                                    if (!entity.IsManual)
                                    {
                                        await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(sub.ReturnSubId, sub.RefPoSubId ?? 0,
                                          sub.ItemId ?? 0, sub.Qty ?? 0, screenCode, currentUser, now, sub.RefIssueSubId ?? 0);
                                    }
                                    else if (entity.IsManual)
                                    {
                                        await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.ReturnSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, prodReturnCompVM.ProductionReturnCompSubVMs, entity.Return);

                                        if (sub.RefIssueSubId.GetValueOrDefault() > 0 && entity.IsWithoutPoDc)
                                        {
                                            await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
                                        }
                                    }

                                }

                                if (sub.RefPoSubId.GetValueOrDefault() > 0)
                                {
                                    await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value, sub.ItemId.Value, sub.Qty ?? 0, 0, "Production return Component Create");
                                }

                            }
                        }
                        else
                        {
                            //  await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId, 0, sub.Qty ?? 0, "SubContract GRN Create");

                            if (sub.RefRcSubId.HasValue && sub.RefRcSubId > 0)
                            {
                                await RevertRCSubIssuedToBalQtyAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "SubContract GRN Return/Rejection");
                            }

                            if (sub.TransType == "In")
                            {

                                if (entity.IsManual)
                                {
                                    await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.ReturnSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, prodReturnCompVM.ProductionReturnCompSubVMs, entity.Return);
                                }

                            }

                        }

                        if (sub.TransType == "In")
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, entity.AddStoreId.Value, sub.Qty.GetValueOrDefault(),
                                sub.UnitPrice, null, screenCode, sub.ReturnSubId, entity.ReturnNo, entity.ReturnDate, null, sub.RefRcSubId);
                        }

                     
                    
                    }

                    if (entity.IsNoInspection)
                    {
                        await CreateSCNFromGRNAsync(entity);
                    }

                    changes.AppendLine("Production Return Component Created Successfully.");
                }
                else
                {
                    // ---------------- UPDATE ----------------
                    entity = await _unitOfWork.ProductionReturnComps.GetQueryable()
                        .Include(q => q.ProductionReturnCompSubs)
                        .Include(q => q.ProductionReturnCompTracks)
                        .FirstOrDefaultAsync(q => q.ReturnId == prodReturnCompVM.ReturnId)
                        ?? throw new InvalidOperationException("Production Return Component not found.");

                    var parentChanges = GetPropertyChanges(entity, prodReturnCompVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(prodReturnCompVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, prodReturnCompVM.ProductionReturnCompSubVMs, screenCode, currentUser, now, changes);

                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Production Return Component Updated.");
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, prodReturnCompVM.ReturnId == 0 ? "Production Return Component Created" : "Production Return Component Updated");

                var savedEntity = await _unitOfWork.ProductionReturnComps.GetQueryable()
                    .Include(q => q.ProductionReturnCompSubs).ThenInclude(s => s.Item)
                    .Include(q => q.AddStore)
                    .Include(q => q.ProductionReturnCompSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.ProductionReturnCompTracks)
                    .FirstOrDefaultAsync(q => q.ReturnId == entity.ReturnId);

                return _mapper.Map<ProductionReturnCompVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Production Return Component: {prodReturnCompVM.ReturnNo}");
                throw new InvalidOperationException("Failed to save Production Return Component. Please try again.");
            }
        }
        private async Task HandleChildUpdatesAsync(ProductionReturnComp existingProdReturnComp, List<ProductionReturnCompSubVM> incomingSubVMs, int screenCode, string currentUser, DateTime now, StringBuilder changes)
        {
            bool poWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();

            var existingSubs = existingProdReturnComp.ProductionReturnCompSubs.ToList();
            var incomingIds = incomingSubVMs.Select(x => x.ReturnSubId).ToHashSet();

            // DELETE
            foreach (var sub in existingSubs.Where(x => !incomingIds.Contains(x.ReturnSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - ReturnSubId: {sub.ReturnSubId}, Item: {sub.Item?.ItemCode}");

                if (sub.ReturnSubId > 0)
                    await RollbackTracksAddIssueQtyAsync(sub.ReturnSubId);

                if (!existingProdReturnComp.Rejection && !existingProdReturnComp.Return)
                {
                    if (sub.RefRcSubId.GetValueOrDefault() > 0)
                        await AdjustRcBalanceAsync(sub.RefRcSubId, sub.Qty ?? 0, 0, "SubContract GRN Delete");

                    else if (sub.RefIssueSubId.GetValueOrDefault() > 0)
                        await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, sub.Qty ?? 0, 0, "SubContract GRN Delete");

                    if (poWiseDcOut && sub.RefPoSubId.GetValueOrDefault() > 0)
                        await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value,sub.ItemId.Value , sub.Qty ?? 0, 0, "SubContract GRN Delete");
                }
                else
                {
                    await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId, sub.Qty ?? 0, 0, "Production return Component delete");

                    if (sub.RefRcSubId.GetValueOrDefault() > 0)
                        await RevertRCSubIssuedToBalQtyAsync(sub.RefRcSubId, sub.Qty ?? 0, 0, "Production return Component Return/Rejection Delete");
                }

                await DeleteStockAddAsync(sub.ReturnSubId, sub.ItemId ?? 0, screenCode, existingProdReturnComp.ReturnNo);

                await _unitOfWork.ProductionReturnCompSubs.DeleteAsync(sub.ReturnSubId);
            }

            // ADD / UPDATE
            foreach (var subVM in incomingSubVMs)
            {
                bool isNew = subVM.ReturnSubId == 0;

                var entity = isNew
                    ? _mapper.Map<ProductionReturnCompSub>(subVM)
                    : existingSubs.FirstOrDefault(x => x.ReturnSubId == subVM.ReturnSubId);

                if (entity == null)
                    continue;

                decimal oldQty = entity.Qty ?? 0;

                if (isNew)
                {
                    entity.ReturnId = existingProdReturnComp.ReturnId;

                    await _unitOfWork.ProductionReturnCompSubs.CreateAsync(entity);

                    await _unitOfWork.SaveAsync();

                    subVM.ReturnSubId = entity.ReturnSubId;

                    oldQty = 0;

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");
                }
                else
                {
                    if (entity.ReturnSubId > 0 && entity.TransType == "In")
                        await RollbackTracksAddIssueQtyAsync(entity.ReturnSubId);

                    var subChanges = GetPropertyChanges(entity, subVM);

                    if (!string.IsNullOrWhiteSpace(subChanges))
                        changes.AppendLine($"Child Updated - ItemCode: {subVM.ItemCode}\n{subChanges}");

                    _mapper.Map(subVM, entity);
                }

                decimal newQty = entity.Qty ?? 0;

                if (entity.TransType == "In")
                {
                    if (!existingProdReturnComp.Rejection && !existingProdReturnComp.Return)
                    {
                        if (entity.RefRcSubId.GetValueOrDefault() > 0)
                        {
                            await CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(entity.ReturnSubId, entity.RefRcSubId.Value,
                                   entity.ItemId.Value, newQty, screenCode, currentUser, now);

                            await UpsertRCSubWipIssuedQtyAsync(entity.RefRcSubId, oldQty, newQty, "Production return Component Create");


                            var issueIds = await GetIssueIdsByRcSubIdAsync(entity.RefRcSubId ?? 0);

                            var issueSubs = await _unitOfWork.SubConDCOutSubs
                                               .GetQueryable()
                                               .Where(x => issueIds.Contains(x.DcId)
                                                   && x.TransType == "In")
                                               .OrderBy(x => x.DcSubId)
                                               .ToListAsync();

                            if (issueSubs.Any())
                            {
                                foreach (var Grnsub in issueSubs)
                                {
                                    await AdjustProductionCompIssueItemBalanceAsync(Grnsub.DcSubId, oldQty, newQty, "SubContract GRN Create");
                                }

                            }

                        }

                        if (poWiseDcOut)
                        {
                            if (entity.TransType == "In" && entity.RefRcSubId == null)
                            {


                                if (existingProdReturnComp.IsManual)
                                {
                                    await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(
                                        entity.ReturnSubId,
                                        entity.ItemId ?? 0,
                                        newQty,
                                        currentUser,
                                        now,
                                        incomingSubVMs,
                                        existingProdReturnComp.Return);


                                    if (entity.RefIssueSubId.GetValueOrDefault() > 0 && existingProdReturnComp.IsWithoutPoDc)
                                    {
                                        await AdjustProductionCompIssueItemBalanceAsync(
                                                    entity.RefIssueSubId.Value,
                                                    oldQty,
                                                    newQty,
                                                    "SubContract GRN");
                                    }
                                }
                                else
                                {
                                    await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(
                                        entity.ReturnSubId,
                                        entity.RefPoSubId ?? 0,
                                        entity.ItemId ?? 0,
                                        newQty,
                                        screenCode,
                                        currentUser,
                                        now,
                                        entity.RefIssueSubId ?? 0);
                                }
                            }
                            if (entity.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustMfgPoSubItemBalanceAsync(
                                    entity.RefPoSubId.Value,entity.ItemId.Value,
                                    oldQty,
                                    newQty,
                                    "SubContract GRN");
                            }
                        }
                        else if (entity.RefIssueSubId.GetValueOrDefault() > 0)
                        {
                            await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(
                                entity.ReturnSubId,
                                entity.RefIssueSubId.Value,
                                entity.ItemId ?? 0,
                                newQty,
                                screenCode,
                                currentUser,
                                now,
                                existingProdReturnComp.IsManual,
                                incomingSubVMs, entity?.RefPoSubId);

                            await AdjustProductionCompIssueItemBalanceAsync(
                                entity.RefIssueSubId.Value,
                                oldQty,
                                newQty,
                                "SubContract GRN");
                        }
                    }
                    else
                    {
                        await AdjustProductionCompIssueItemBalanceAsync(
                            entity.RefIssueSubId,
                            oldQty,
                            newQty,
                            "Production Return/Rejection");

                        if (entity.RefRcSubId.GetValueOrDefault() > 0)
                        {
                            await RevertRCSubIssuedToBalQtyAsync(
                                entity.RefRcSubId,
                                oldQty,
                                newQty,
                                "Production Return/Rejection");

                            var issueIds = await GetIssueIdsByRcSubIdAsync(entity.RefRcSubId ?? 0);

                            var issueSubs = await _unitOfWork.SubConDCOutSubs
                                               .GetQueryable()
                                               .Where(x => issueIds.Contains(x.DcId)
                                                   && x.TransType == "In")
                                               .OrderBy(x => x.DcSubId)
                                               .ToListAsync();

                            if (issueSubs.Any())
                            {
                                foreach (var Grnsub in issueSubs)
                                {
                                    await AdjustProductionCompIssueItemBalanceAsync(Grnsub.DcSubId, oldQty, newQty, "SubContract GRN Create");
                                }

                            }
                        }
                    }

                    await _stockManagerService.AddOrUpdateStockAsync(
                        entity.ItemId ?? 0,
                        existingProdReturnComp.AddStoreId ?? 0,
                        newQty,
                        entity.UnitPrice,
                        null,
                        screenCode,
                        entity.ReturnSubId,
                        existingProdReturnComp.ReturnNo,
                        existingProdReturnComp.ReturnDate,
                        null,
                        entity.RefRcSubId);
                }
            }

            await _unitOfWork.SaveAsync();
        }
       
        private async Task CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(int DcSubId, int ItemId, decimal Qty, string currentUser, DateTime now, List<ProductionReturnCompSubVM?> SubConGRNSubVMs, bool Materialreturn)
        {
            try
            {
                bool PoWiseGrn = await IsPOWiseProductionDcOutEnabledAsync();

                var selectedRows = SubConGRNSubVMs
                    .Where(x => x.RefIssueSubId.HasValue && x.Qty > 0)
                    .ToList();

                if (Materialreturn)
                {
                    selectedRows = selectedRows
                      .Where(x => x.TransType == "In")
                      .ToList();

                }
                else
                {
                    selectedRows = selectedRows
                      .Where(x => x.TransType == "Out")
                      .ToList();
                }

                if (!selectedRows.Any())
                    throw new Exception("No manual issue rows selected.");

                decimal totalOutQty = selectedRows.Sum(x => x.Qty ?? 0);
                if (totalOutQty <= 0)
                    throw new Exception("Invalid outgoing quantity.");


                foreach (var row in selectedRows)
                {
                    var issueSub = await _unitOfWork.ProductionIssueCompSubs.GetAsync(row.RefIssueSubId!.Value);
                    if (issueSub == null)
                        throw new Exception($"IssueSub not found : {row.RefIssueSubId}");

                    decimal availableQty = 0;
                    availableQty = issueSub.BalQty ?? 0;

                    if (availableQty < row.Qty && availableQty < row.Qty)
                    {
                        // throw new Exception($"Insufficient balance for IssueSubId {issueSub.SCNSubId}");
                        return;
                    }

                    var track = new ProductionReturnCompTrack
                    {

                        RefReturnSubId = DcSubId,
                        RefIssueSubId = row.RefIssueSubId,
                        RefPoSubId = row.RefPoSubId,
                        RefRcSubId = row.RefRcSubId,
                        // Incoming → Assembly
                        ItemIdIn = ItemId,
                        QtyIn = Qty,

                        // Outgoing → Component
                        ItemIdOut = row.ItemId,
                        QtyOut = row.Qty,

                        CreatedBy = currentUser,
                        CreatedDate = now
                    };


                    await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);
                    await _unitOfWork.SaveAsync();

                    issueSub.BalQty = availableQty - row.Qty ?? 0m;


                    if (issueSub.BalQty < 0) issueSub.BalQty = 0;


                    await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issueSub);
                    await _unitOfWork.SaveAsync();
                }


                var issueIdss = selectedRows
                    .Select(x => x.RefIssueSubId)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct();

                var affectedScnIds = await _unitOfWork.ProductionIssueCompSubs
                                 .GetQueryable()
                                 .Where(s => selectedRows
                                     .Select(r => r.RefIssueSubId!.Value)
                                     .Contains(s.IssueSubId))
                                 .Select(s => s.IssueId)
                                 .Distinct()
                                 .ToListAsync();

                foreach (var DcId in affectedScnIds)
                {
                    decimal remainingBalQty = 0;

                    if (PoWiseGrn)
                    {
                        remainingBalQty = await _unitOfWork.ProductionIssueCompSubs
                            .GetQueryable()
                            .Where(s => s.IssueId == DcId && s.TransType == "Out")
                            .SumAsync(s => (decimal?)s.BalQty) ?? 0;
                    }
                    else
                    {
                        remainingBalQty = await _unitOfWork.ProductionIssueCompSubs
                            .GetQueryable()
                            .Where(s => s.IssueId == DcId)
                            .SumAsync(s => (decimal?)s.BalQty) ?? 0;
                    }

                    var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(DcId??0);

                    if (issueHead != null)
                    {
                        issueHead.IssueTally = remainingBalQty == 0;
                        await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                    }
                }

                await _unitOfWork.SaveAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to GetPropertyChanges in CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync");
            }
        }



        private async Task CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(int dcSubId, int RefPoSubId, int itemId, decimal qty,
                                                                          int screenCode, string currentUser, DateTime now, int RefDcId)
        {
            try
            {

                List<int> DcOutSubIds;
                if (RefPoSubId == 0 && RefDcId == 0)
                    return;

                if (RefPoSubId > 0)
                {
                    DcOutSubIds = await _unitOfWork.ProductionIssueCompSubs.GetQueryable().Where(x => x.TransType == "Out" &&
                                      _unitOfWork.ProductionIssueCompSubs.GetQueryable().Where(y => y.RefPoSubId == RefPoSubId)
                                      .Select(y => y.IssueId).Contains(x.IssueId)).Select(x => x.IssueSubId).ToListAsync();
                }
                else
                {
                    DcOutSubIds = RefDcId > 0 ? new List<int> { RefDcId } : new List<int>();
                }

                var categoryCode = await _commonService.GetItemCategoryCodeByItemIdAsync(itemId);

                if (categoryCode == 3 || categoryCode == 7)
                {
                    var bomData = await _unitOfWork.AssmblyDefs.GetQueryable()
                        .Where(x => x.AssmblyID == itemId && x.UtilQty > 0)
                        .Select(x => new
                        {
                            ComponentItemId = x.ItemId,
                            x.UtilQty
                        })
                        .ToListAsync();

                    foreach (var bom in bomData)
                    {
                        decimal componentNeeded = qty * bom.UtilQty;

                        var IssuedSubs = await _unitOfWork.ProductionIssueCompSubs
                                        .GetQueryable()
                                        .Where(x =>
                                               x.IssueId > 0
                                            && DcOutSubIds.Contains(x.IssueSubId)
                                            && x.ItemId == bom.ComponentItemId
                                            && x.BalQty > 0)
                                        .OrderBy(x => x.IssueId)
                                        .ToListAsync();

                        foreach (var issue in IssuedSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = issue.BalQty ?? 0;
                            decimal takeQty = Math.Min(availableQty, componentNeeded);
                            if (takeQty <= 0)
                                continue;

                            // 🔹 Create Track
                            var track = new ProductionReturnCompTrack
                            {

                                RefReturnSubId = dcSubId,
                                RefIssueSubId = issue.IssueSubId,
                                RefPoSubId = RefPoSubId == 0 ? null : RefPoSubId,

                                // Incoming → Assembly
                                ItemIdIn = itemId,
                                QtyIn = qty,

                                // Outgoing → Component
                                ItemIdOut = bom.ComponentItemId,
                                QtyOut = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);
                            // 🔹 Reduce Issue Balance
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issue);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }
                        foreach (var dcid in IssuedSubs.Where(x => x.IssueId > 0).Select(x => x.IssueId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                                .GetQueryable()
                                .Where(x => x.IssueId == dcid && x.TransType == "Out")
                                .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(dcid??0);
                            if (issueHead != null)
                            {
                                issueHead.IssueTally = (totalBalQty == 0);
                                await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                                await _unitOfWork.SaveAsync();
                            }
                        }
                    }

                    await _unitOfWork.SaveAsync();
                }
                else if (categoryCode == 2)
                {
                    var compData = await _unitOfWork.CompMasters.GetQueryable()
                        .Where(x => x.CompItemId == itemId && x.Weight > 0)
                        .Select(x => new
                        {
                            RawMaterialItemId = x.RMId,
                            x.Weight
                        })
                        .FirstOrDefaultAsync();

                    if (compData == null)
                        return;

                    decimal componentNeeded = qty * compData.Weight;

                    var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                                       .GetQueryable()
                                       .Where(x =>
                                              x.IssueId > 0
                                           && DcOutSubIds.Contains(x.IssueSubId)
                                          && x.ItemId == compData.RawMaterialItemId
                                        && x.BalQty > 0)
                                       .OrderBy(x => x.IssueId)
                                       .ToListAsync();
                    if (issueSubs.Count > 0)
                    {
                        foreach (var issue in issueSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = issue.BalQty ?? 0;
                            decimal takeQty = Math.Min(availableQty, componentNeeded);
                            if (takeQty <= 0)
                                continue;

                            // 🔹 Create Track
                            var track = new ProductionReturnCompTrack
                            {

                                RefReturnSubId = dcSubId,
                                RefIssueSubId = issue.IssueSubId,
                                RefPoSubId = RefPoSubId == 0 ? null : RefPoSubId,

                                // Incoming → Assembly
                                ItemIdIn = itemId,
                                QtyIn = qty,

                                // Outgoing → Component
                                ItemIdOut = compData.RawMaterialItemId,
                                QtyOut = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);
                            // 🔹 Reduce Issue Balance
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issue);


                            //await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);
                            await _unitOfWork.SaveAsync();
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issue);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }

                        // 🔹 Update IssueTally
                        foreach (var scnId in issueSubs.Where(x => x.IssueId > 0).Select(x => x.IssueId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                                    .GetQueryable()
                                    .Where(x => x.IssueId == RefDcId && x.TransType == "Out")
                                    .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(scnId??0);
                            if (issueHead != null)
                            {
                                issueHead.IssueTally = (totalBalQty == 0);
                                await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                                await _unitOfWork.SaveAsync();
                            }
                        }

                    }
                    else
                    {

                        var issuedSubs = await _unitOfWork.ProductionIssueCompSubs
                                                .GetQueryable()
                                                .Where(x =>
                                                       x.IssueId > 0
                                                   && DcOutSubIds.Contains(x.IssueSubId)
                                                   && x.ItemId == compData.RawMaterialItemId
                                                     && x.BalQty > 0)
                                                    .OrderBy(x => x.IssueId)
                                                    .ToListAsync();

                        foreach (var issue in issuedSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = issue.BalQty ?? 0;
                            decimal takeQty = Math.Min(availableQty, componentNeeded);

                            if (takeQty <= 0)
                                continue;

                            var track = new ProductionReturnCompTrack
                            {

                                RefReturnSubId = dcSubId,
                                RefIssueSubId = issue.IssueSubId,
                                RefPoSubId = RefPoSubId == 0 ? null : RefPoSubId,

                                // Incoming → Assembly
                                ItemIdIn = itemId,
                                QtyIn = qty,

                                // Outgoing → Component
                                ItemIdOut = compData.RawMaterialItemId,
                                QtyOut = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);
                            await _unitOfWork.SaveAsync();
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issue);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }

                        // 🔹 Update IssueTally
                        foreach (var scnId in issueSubs.Where(x => x.IssueId > 0).Select(x => x.IssueId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                                    .GetQueryable()
                                    .Where(x => x.IssueId == RefDcId && x.TransType == "Out")
                                    .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(scnId??0);
                            if (issueHead != null)
                            {
                                issueHead.IssueTally = (totalBalQty == 0);
                                await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                                await _unitOfWork.SaveAsync();
                            }
                        }

                    }


                    await _unitOfWork.SaveAsync();
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync] Unexpected error ");
                throw new InvalidOperationException("Failed to a CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync. Please contact support.");
            }
        }
        public async Task<List<int>> GetIssueIdsByIssueSubIdAsync(int IssueSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.IssueSubId == IssueSubId)
                    .Select(x => x.IssueId ?? 0)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DcIds for IssueSubId: {IssueSubId}");
                throw new InvalidOperationException("Failed to retrieve DC details for the selected dc. Please try again.", ex);
            }
        }

        public async Task<bool> CheckIsSameAsOutItemByPoSubId(int poSubId)
        {
            var dc = await _unitOfWork.ProductionIssueCompSubs
                 .GetQueryable()
                 .AsNoTracking()
                 .Where(x => x.RefPoSubId == poSubId)
                 .Where(x => !x.ProductionIssueComp.Cancel && !x.ProductionIssueComp.ShortClose)
                 .OrderByDescending(x => x.ProductionIssueComp.IssueId)
                 .Select(x => new
                 {
                     x.ProductionIssueComp.IsOutItemSameAsInItem
                 })
                 .FirstOrDefaultAsync();

            return dc?.IsOutItemSameAsInItem ?? false;
        }
        private async Task CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(
            int returnSubId,
            int refDcSubId,
            int itemId,
            decimal qtyReturned,
            int screenCode,
            string currentUser,
            DateTime now, bool isManulSelection, List<ProductionReturnCompSubVM> incomingSubVMs, int? refposubid)
        {


            var dcIds = await GetIssueIdsByIssueSubIdAsync(refDcSubId);
            if (dcIds == null || !dcIds.Any())
                return;

            bool isSameItem = false;

            if (refposubid.HasValue)
            {
                isSameItem = await CheckIsSameAsOutItemByPoSubId(refposubid.Value);
            }
            if (isSameItem)
            {
                // Auto create track from same item
                var sameItemOuts = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => dcIds.Contains(x.IssueId??0) &&

                        x.RefPoSubId == refposubid &&
                        x.TransType == "Out" &&
                        x.ItemId == itemId &&
                        (x.BalQty ?? 0) > 0)
                    .OrderBy(x => x.IssueSubId)
                    .ToListAsync();

                decimal qtyToReduce = qtyReturned;

                foreach (var outSub in sameItemOuts)
                {
                    if (qtyToReduce <= 0)
                        break;

                    decimal deductQty = Math.Min(outSub.BalQty ?? 0, qtyToReduce);

                    await _unitOfWork.ProductionReturnCompTracks.CreateAsync(new ProductionReturnCompTrack
                    {
                        RefReturnSubId = returnSubId,
                        RefIssueSubId = outSub.IssueSubId,
                        RefPoSubId = outSub.RefPoSubId,

                        ItemIdIn = itemId,
                        QtyIn = qtyReturned,

                        ItemIdOut = outSub.ItemId,
                        QtyOut = deductQty,

                        CreatedBy = currentUser,
                        CreatedDate = now
                    });

                   
                    outSub.BalQty = (outSub.BalQty ?? 0) - deductQty;

                    await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(outSub);

                    qtyToReduce -= deductQty;
                }
                await _unitOfWork.SaveAsync();

                var issueIdss = sameItemOuts
                          .Select(x => x.IssueId)
                          .Distinct()
                          .ToList();

                foreach (var issueId in issueIdss)
                {
                    var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .Where(x => x.IssueId == issueId)
                        .SumAsync(x => x.BalQty ?? 0);

                    var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(issueId??0);
                    if (issueHead != null)
                    {
                        issueHead.IssueTally = totalBalQty == 0;
                        await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                    }
                }
                await _unitOfWork.SaveAsync();


            }
            else if (!isManulSelection)
            {
                var categoryCode = await _commonService.GetItemCategoryCodeByItemIdAsync(itemId);

                if (categoryCode == 3 || categoryCode == 7)
                {
                    var bomData = await _unitOfWork.AssmblyDefs.GetQueryable()
                        .Where(x => x.AssmblyID == itemId && x.UtilQty > 0)
                        .Select(x => new
                        {
                            ComponentItemId = x.ItemId,
                            x.UtilQty
                        })
                        .ToListAsync();

                    foreach (var bom in bomData)
                    {
                        decimal componentNeeded = qtyReturned * bom.UtilQty;

                        var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                            .GetQueryable()
                            .Where(x => dcIds.Contains(x.IssueId??0)
                                     && x.ItemId == bom.ComponentItemId
                                     && x.BalQty > 0)
                            .OrderBy(x => x.IssueSubId)
                            .ToListAsync();

                        foreach (var issue in issueSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = issue.BalQty ?? 0;
                            decimal takeQty = Math.Min(availableQty, componentNeeded);
                            if (takeQty <= 0)
                                continue;

                            // 🔹 Create Track
                            var track = new ProductionReturnCompTrack
                            {

                                RefReturnSubId = returnSubId,
                                RefIssueSubId = issue.IssueSubId,
                                RefPoSubId = issue.RefPoSubId,

                                // Incoming → Assembly
                                ItemIdIn = itemId,
                                QtyIn = qtyReturned,

                                // Outgoing → Component
                                ItemIdOut = bom.ComponentItemId,
                                QtyOut = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);
                            // 🔹 Reduce Issue Balance
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issue);

                            componentNeeded -= takeQty;
                        }

                        // 🔹 Update IssueTally per IssueId
                        foreach (var issueId in issueSubs.Select(x => x.IssueId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                                .GetQueryable()
                                .Where(x => x.IssueId == issueId)
                                .SumAsync(x => x.BalQty ?? 0);

                            var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(issueId??0);
                            if (issueHead != null)
                            {
                                issueHead.IssueTally = (totalBalQty == 0);
                                await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                            }
                        }
                    }

                    await _unitOfWork.SaveAsync();
                }
                else if (categoryCode == 2)
                {
                    var compData = await _unitOfWork.CompMasters.GetQueryable()
                        .Where(x => x.CompItemId == itemId && x.Weight > 0)
                        .Select(x => new
                        {
                            RawMaterialItemId = x.RMId,
                            x.Weight
                        })
                        .FirstOrDefaultAsync();

                    if (compData == null)
                        return;

                    decimal componentNeeded = qtyReturned * compData.Weight;

                    var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .Where(x => dcIds.Contains(x.IssueId??0)
                                 && x.ItemId == compData.RawMaterialItemId
                                 && x.BalQty > 0)
                        .OrderBy(x => x.IssueSubId)
                        .ToListAsync();

                    foreach (var issue in issueSubs)
                    {
                        if (componentNeeded <= 0)
                            break;

                        decimal availableQty = issue.BalQty ?? 0;
                        decimal takeQty = Math.Min(availableQty, componentNeeded);

                        if (takeQty <= 0)
                            continue;

                        var track = new ProductionReturnCompTrack
                        {
                            RefReturnSubId = returnSubId,
                            RefIssueSubId = issue.IssueSubId,
                            RefPoSubId = issue.RefPoSubId,

                            ItemIdIn = itemId,
                            QtyIn = qtyReturned,

                            ItemIdOut = compData.RawMaterialItemId,
                            QtyOut = takeQty,

                            CreatedBy = currentUser,
                            CreatedDate = now
                        };

                        await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);

                        issue.BalQty = availableQty - takeQty;
                        if (issue.BalQty < 0) issue.BalQty = 0;

                        await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issue);

                        componentNeeded -= takeQty;
                    }

                    // 🔹 Update IssueTally
                    foreach (var issueId in issueSubs.Select(x => x.IssueId??0).Distinct())
                    {
                        var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                            .GetQueryable()
                            .Where(x => x.IssueId == issueId)
                            .SumAsync(x => x.BalQty ?? 0);

                        var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(issueId);
                        if (issueHead != null)
                        {
                            issueHead.IssueTally = (totalBalQty == 0);
                            await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                        }
                    }
                    await _unitOfWork.SaveAsync();
                }
            }
            else
            {
                // Validate selection
                var selectedRows = incomingSubVMs
                    .Where(x => x.RefIssueSubId.HasValue && x.Qty > 0 && x.TransType == "Out")
                    .ToList();



                if (!selectedRows.Any())
                    throw new Exception("No manual issue rows selected.");

                // Validate total OUT qty
                decimal totalOutQty = selectedRows.Sum(x => x.Qty.GetValueOrDefault());
                if (totalOutQty <= 0)
                    throw new Exception("Invalid outgoing quantity.");

                // Create tracks per selected row
                foreach (var row in selectedRows)
                {
                    var issueSub = await _unitOfWork.ProductionIssueCompSubs.GetAsync(row.RefIssueSubId!.Value);
                    if (issueSub == null)
                        throw new Exception($"IssueSub not found : {row.RefIssueSubId}");

                    decimal availableQty = issueSub.BalQty ?? 0;

                    //if (availableQty < row.Qty)
                    //    throw new Exception($"Insufficient balance for IssueSubId {issueSub.DcSubId}");
                    if (availableQty < row.Qty && availableQty < row.Qty)
                    {
                        // throw new Exception($"Insufficient balance for IssueSubId {issueSub.SCNSubId}");

                    }
                    // Track (same pattern as AUTO)
                    var track = new ProductionReturnCompTrack
                    {
                        RefReturnSubId = returnSubId,
                        RefIssueSubId = issueSub.IssueSubId,

                        ItemIdIn = itemId,
                        QtyIn = qtyReturned,              //  SAME AS AUTO

                        ItemIdOut = issueSub.ItemId,      //  COMPONENT / RM
                        QtyOut = row.Qty,                 //  USER SELECTED

                        CreatedBy = currentUser,
                        CreatedDate = now
                    };

                    await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);

                    // Reduce balance
                    issueSub.BalQty = availableQty - row.Qty;

                    if (issueSub.BalQty < 0) issueSub.BalQty = 0;

                    await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issueSub);
                }
                await _unitOfWork.SaveAsync();

                // Update IssueTally
                var issueIdss = selectedRows
                    .Select(x => x.RefIssueSubId)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct();

                foreach (var issueId in issueIdss)
                {
                    var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .Where(x => x.IssueId == issueId)
                        .SumAsync(x => x.BalQty ?? 0);

                    var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(issueId);
                    if (issueHead != null)
                    {
                        issueHead.IssueTally = totalBalQty == 0;
                        await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                    }
                }
            }
            await _unitOfWork.SaveAsync();
        }



        private async Task UpsertRCSubWipIssuedQtyAsync(int? refRcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refRcSubId.HasValue || refRcSubId == 0)
                    return;

                var routeCardProcess = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.RCSubId == refRcSubId.Value)
                    ?? throw new InvalidOperationException("Route Card Process not found");

                // 🔄 Revert OLD qty
                if (oldQty > 0)
                {
                    routeCardProcess.IssuedQty += oldQty;
                    routeCardProcess.WipQty -= oldQty;
                }

                // ❌ Validation
                if (newQty > routeCardProcess.IssuedQty)
                    throw new InvalidOperationException(
                        $"{context}: Qty cannot exceed available Issued Qty.");

                // 🔄 Apply NEW qty
                if (newQty > 0)
                {
                    routeCardProcess.IssuedQty -= newQty;
                    routeCardProcess.WipQty += newQty;
                }

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardProcess);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(
                    ex, $"[AdjustRCSubItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex, $"[AdjustRCSubItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException(
                    "Failed to adjust Route Card sub balance. Please contact support.");
            }
        }


        private async Task RevertRCSubIssuedToBalQtyAsync(int? refRcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refRcSubId.HasValue || refRcSubId == 0)
                    return;

                var routeCardProcess = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.RCSubId == refRcSubId.Value)
                    ?? throw new InvalidOperationException("Route Card Process not found");

                if (oldQty > 0)
                {
                    routeCardProcess.IssuedQty += oldQty;
                    routeCardProcess.BalQty -= oldQty;
                }

                if (newQty > routeCardProcess.IssuedQty)
                    throw new InvalidOperationException(
                        $"{context}: Qty cannot exceed available Issued Qty.");

                if (newQty > 0)
                {
                    routeCardProcess.IssuedQty -= newQty;
                    routeCardProcess.BalQty += newQty;
                }

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardProcess);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[RevertRCSubItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[RevertRCSubItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to revert Route Card sub balance. Please contact support.");
            }
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

        private async Task DeleteStockAddAsync(int returnSubId, int itemId, int screenCode, string refNo)
        {
            var addIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == returnSubId && s.ItemId == itemId && s.ScreenCode == screenCode && s.RefNo == refNo)
                .Select(s => s.AddId)
                .ToListAsync();

            foreach (var addId in addIds)
            {
                if (addId > 0)
                    await _stockManagerService.DeleteStockAddAsync(addId);
            }
        }


        private async Task RollbackTracksAddIssueQtyAsync(int refReturnSubId)
        {
            var now = DateTime.Now;

            try
            {
                var tracks = await _unitOfWork.ProductionReturnCompTracks
                    .GetQueryable()
                    .Where(x => x.RefReturnSubId == refReturnSubId)
                    .ToListAsync();

                if (!tracks.Any())
                    return;

                foreach (var track in tracks)
                {
                    var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .Where(x => x.IssueSubId == track.RefIssueSubId)
                        .ToListAsync();

                    foreach (var issueSub in issueSubs)
                    {
                        issueSub.BalQty += track.QtyOut.GetValueOrDefault();

                        await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issueSub);

                        if (!issueSub.IssueId.HasValue)
                        {
                            await _logs.LogDeveloperInfo($"[RollbackTracksAddIssueQtyAsync] IssueId missing. IssueSubId={issueSub.IssueSubId}");
                            continue;
                        }

                        var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                            .GetQueryable()
                            .Where(x =>
                                x.IssueId == issueSub.IssueId.Value &&
                                x.TransType == "Out")
                            .SumAsync(x => x.BalQty);

                        var issueComp = await _unitOfWork.ProductionIssueComps
                            .GetAsync(issueSub.IssueId.Value);

                        if (issueComp != null)
                        {
                            issueComp.IssueTally = totalBalQty == 0;
                            await _unitOfWork.ProductionIssueComps.UpdateAsync(issueComp);
                            await _unitOfWork.SaveAsync();
                        }
                    }

                    await _unitOfWork.ProductionReturnCompTracks.DeleteAsync(track);
                    await _unitOfWork.SaveAsync();
                }

                await _unitOfWork.SaveAsync();
                await _logs.LogDeveloperInfo($"[RollbackTracksAddIssueQtyAsync] Rollback completed successfully. RefReturnSubId={refReturnSubId}");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[RollbackTracksAddIssueQtyAsync] Failed rollback for RefReturnSubId={refReturnSubId}");
                throw new InvalidOperationException("Failed to rollback issued quantities. Please contact support.");
            }
        }


        private async Task CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(
            int returnSubId,
            int refPoSubId,
            int itemId,
            decimal qtyReturned,
            int screenCode,
            string currentUser,
            DateTime now)
        {
            var issueIds = await GetIssueIdsByPoSubIdAsync(refPoSubId);
            if (issueIds == null || !issueIds.Any())
                return;

            var categoryCode = await _commonService.GetItemCategoryCodeByItemIdAsync(itemId);

            if (categoryCode == 3 || categoryCode == 7)
            {
                var bomData = await _unitOfWork.AssmblyDefs.GetQueryable()
                    .Where(x => x.AssmblyID == itemId && x.UtilQty > 0)
                    .Select(x => new
                    {
                        ComponentItemId = x.ItemId,
                        x.UtilQty
                    })
                    .ToListAsync();

                foreach (var bom in bomData)
                {
                    decimal componentNeeded = qtyReturned * bom.UtilQty;

                    var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .Where(x => x.IssueId.HasValue
                                 && issueIds.Contains(x.IssueId.Value)
                                 && x.ItemId == bom.ComponentItemId
                                 && x.BalQty > 0)
                        .OrderBy(x => x.IssueSubId)
                        .ToListAsync();

                    foreach (var issue in issueSubs)
                    {
                        if (componentNeeded <= 0)
                            break;

                        decimal availableQty = issue.BalQty ?? 0;
                        decimal takeQty = Math.Min(availableQty, componentNeeded);
                        if (takeQty <= 0)
                            continue;

                        // 🔹 Create Track
                        var track = new ProductionReturnCompTrack
                        {
                            RefReturnSubId = returnSubId,
                            RefIssueSubId = issue.IssueSubId,

                            // Incoming → Assembly
                            ItemIdIn = itemId,
                            QtyIn = qtyReturned,

                            // Outgoing → Component
                            ItemIdOut = bom.ComponentItemId,
                            QtyOut = takeQty,

                            CreatedBy = currentUser,
                            CreatedDate = now
                        };

                        await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);

                        // 🔹 Reduce Issue Balance
                        issue.BalQty = availableQty - takeQty;
                        if (issue.BalQty < 0) issue.BalQty = 0;

                        await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issue);

                        componentNeeded -= takeQty;
                    }

                    // 🔹 Update IssueTally per IssueId
                    foreach (var issueId in issueSubs.Select(x => x.IssueId!.Value).Distinct())
                    {
                        var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                            .GetQueryable()
                            .Where(x => x.IssueId == issueId)
                            .SumAsync(x => x.BalQty ?? 0);

                        var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(issueId);
                        if (issueHead != null)
                        {
                            issueHead.IssueTally = (totalBalQty == 0);
                            await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                        }
                    }
                }

                await _unitOfWork.SaveAsync();
            }
            else if (categoryCode == 2)
            {
                var compData = await _unitOfWork.CompMasters.GetQueryable()
                    .Where(x => x.CompItemId == itemId && x.Weight > 0)
                    .Select(x => new
                    {
                        RawMaterialItemId = x.RMId,
                        x.Weight
                    })
                    .FirstOrDefaultAsync();

                if (compData == null)
                    return;

                decimal componentNeeded = qtyReturned * compData.Weight;

                var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.IssueId.HasValue
                             && issueIds.Contains(x.IssueId.Value)
                             && x.ItemId == compData.RawMaterialItemId
                             && x.BalQty > 0)
                    .OrderBy(x => x.IssueSubId)
                    .ToListAsync();

                foreach (var issue in issueSubs)
                {
                    if (componentNeeded <= 0)
                        break;

                    decimal availableQty = issue.BalQty ?? 0;
                    decimal takeQty = Math.Min(availableQty, componentNeeded);
                    if (takeQty <= 0)
                        continue;

                    var track = new ProductionReturnCompTrack
                    {
                        RefReturnSubId = returnSubId,
                        RefIssueSubId = issue.IssueSubId,

                        // Incoming → Component
                        ItemIdIn = itemId,
                        QtyIn = qtyReturned,

                        // Outgoing → Raw Material
                        ItemIdOut = compData.RawMaterialItemId,
                        QtyOut = takeQty,

                        CreatedBy = currentUser,
                        CreatedDate = now
                    };

                    await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);

                    issue.BalQty = availableQty - takeQty;
                    if (issue.BalQty < 0) issue.BalQty = 0;

                    await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issue);

                    componentNeeded -= takeQty;
                }

                // 🔹 Update IssueTally
                foreach (var issueId in issueSubs.Select(x => x.IssueId!.Value).Distinct())
                {
                    var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .Where(x => x.IssueId == issueId)
                        .SumAsync(x => x.BalQty ?? 0);

                    var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(issueId);
                    if (issueHead != null)
                    {
                        issueHead.IssueTally = (totalBalQty == 0);
                        await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                    }
                }

                await _unitOfWork.SaveAsync();
            }
        }


        private async Task CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(int returnSubId, int refRcSubId, int itemId,
                        decimal qtyReturned, int screenCode, string currentUser, DateTime now)
        {
            var issueIds = await GetIssueIdsByRcSubIdAsync(refRcSubId);

            if (issueIds == null || !issueIds.Any())
                return;

            var RcSubData = await _unitOfWork.RouteCardSubs.GetQueryable()
                            .Include(x => x.RouteCard)
                            .Where(x => x.RCSubId == refRcSubId).FirstOrDefaultAsync();

            if (RcSubData == null)
                return;

            var utility = 1m;

            if (RcSubData.SeqNo == 1)
            {
                utility = RcSubData.RouteCard.RMWeight;
            }

            decimal rmRequiredQty = qtyReturned * utility;
            
            //var issueSubs = await _unitOfWork.ProductionIssueCompSubs
            //    .GetQueryable()
            //    .Where(x =>
            //        x.IssueId.HasValue &&
            //        issueIds.Contains(x.IssueId.Value) &&
            //        x.ItemId == RcSubData.ItemIdOut &&
            //        (x.BalQty ?? 0) > 0
            //        && x.TransType == "Out")
            //    .OrderBy(x => x.IssueSubId)
            //    .ToListAsync();

            var query = _unitOfWork.ProductionIssueCompSubs
                .GetQueryable()
                .Where(x =>
                    x.IssueId.HasValue &&
                    issueIds.Contains(x.IssueId.Value) &&
                    (x.BalQty ?? 0) > 0 &&
                    x.TransType == "Out");

            if (!RcSubData.IsBOM)
            {
                query = query.Where(x => x.ItemId == RcSubData.ItemIdOut);
            }

            var issueSubs = await query
                .OrderBy(x => x.IssueSubId)
                .ToListAsync();


            decimal availableQty = 0;
            decimal consumeQty = 0;
            foreach (var issueSub in issueSubs)
            {
                if (!RcSubData.IsBOM)
                {
                    if (rmRequiredQty <= 0)
                        break;

                    availableQty = issueSub.BalQty ?? 0;
                    consumeQty = Math.Min(availableQty, rmRequiredQty);

                    if (consumeQty <= 0)
                        continue;
                }
                else
                {
                    if (rmRequiredQty <= 0)
                        rmRequiredQty= issueSub.BalQty ?? 0;

                    availableQty = issueSub.BalQty ?? 0;
                    consumeQty = Math.Min(availableQty, rmRequiredQty);

                    if (consumeQty <= 0)
                        continue;
                }

                    var track = new ProductionReturnCompTrack
                    {
                        RefReturnSubId = returnSubId,
                        RefIssueSubId = issueSub.IssueSubId,
                        RefRcSubId = refRcSubId,

                        ItemIdIn = itemId,
                        QtyIn = qtyReturned,

                        ItemIdOut = RcSubData.ItemIdOut,
                        QtyOut = consumeQty,

                        CreatedBy = currentUser,
                        CreatedDate = now
                    };

                await _unitOfWork.ProductionReturnCompTracks.CreateAsync(track);
                await _unitOfWork.SaveAsync();

                issueSub.BalQty = availableQty - consumeQty;
                if (issueSub.BalQty < 0)
                    issueSub.BalQty = 0;

                await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(issueSub);
                await _unitOfWork.SaveAsync();

                rmRequiredQty -= consumeQty;
            }

            var affectedIssueIds = issueSubs
                .Select(x => x.IssueId!.Value)
                .Distinct()
                .ToList();

            foreach (var issueId in affectedIssueIds)
            {
                var remainingBalQty = await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.IssueId == issueId && x.TransType == "Out")
                    .SumAsync(x => x.BalQty ?? 0);

                var issueHead = await _unitOfWork.ProductionIssueComps.GetAsync(issueId);
                if (issueHead != null)
                {
                    issueHead.IssueTally = (remainingBalQty == 0);
                    await _unitOfWork.ProductionIssueComps.UpdateAsync(issueHead);
                    await _unitOfWork.SaveAsync();
                }
            }

            await _unitOfWork.SaveAsync();
        }


        private async Task AdjustMfgPoSubItemBalanceAsync(int refPoSubId, int itemId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (refPoSubId == 0) return;

                var poSub = await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(j => j.PoSubId == refPoSubId && j.ItemId == itemId).FirstOrDefaultAsync();

                if (poSub == null) return;

                if (oldQty > 0)
                    poSub.ProdCompBalQty -= oldQty;

                if (newQty > poSub.Qty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

                if (newQty > 0)
                    poSub.ProdCompBalQty += newQty;

                await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();

            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustMfgPoSubItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustMfgPoSubItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust MfgPo Sub Balance. Please contact support.");
            }
        }

        private async Task AdjustProductionCompIssueItemBalanceAsync(int? refIssueSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refIssueSubId.HasValue || refIssueSubId == 0) return;

                var compIssueSub = await _unitOfWork.ProductionIssueCompSubs.GetQueryable()
                                    .Where(p => p.IssueSubId == refIssueSubId).FirstOrDefaultAsync();

                if (compIssueSub == null) return;

                if (oldQty > 0)
                    compIssueSub.BalQty += oldQty;

                if (newQty > compIssueSub.Qty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

                if (newQty > 0)
                    compIssueSub.BalQty -= newQty;

                await _unitOfWork.ProductionIssueCompSubs.UpdateAsync(compIssueSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.ProductionIssueCompSubs
                                        .GetQueryable()
                                        .Where(e => e.IssueId == compIssueSub.IssueId)
                                        .SumAsync(e => e.BalQty);

                var compIssue = await _unitOfWork.ProductionIssueComps.GetAsync(compIssueSub.IssueId.Value);
                if (compIssue != null)
                {
                    compIssue.IssueTally = (totalBalQty == 0);
                    await _unitOfWork.ProductionIssueComps.UpdateAsync(compIssue);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustProductionCompIssueItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustProductionCompIssueItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Production Issue Sub Balance. Please contact support.");
            }
        }

        private async Task CreateSCNFromGRNAsync(ProductionReturnComp grn)
        {
            var changes = new StringBuilder();

            try
            {
                var suffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);
                var screenCode = await GetScreenCodeByScreenNameAsync("Production SCN Component");

                var MapStoreForProdSCN = await GetMappedStoreForFormAsync("SCNProductionForm");
                var addStore = MapStoreForProdSCN.StoreId > 0 ? MapStoreForProdSCN.StoreId : 1;

                var scn = new ProductionSCNComp
                {
                    Suffix = suffix,
                    SCNNo = await _scnService.GetSCNNumberAsync(suffix),
                    SCNDate = DateTime.Now,
                    
                    IssueStoreId = grn.AddStoreId,
                    AddStoreId = addStore,
                    CreatedBy = grn.CreatedBy,
                    CreatedDate = DateTime.Now,
                    MainRemark = "Auto-generated due to No Inspection",
                    ProductionSCNCompSubs = new List<ProductionSCNCompSub>()
                };
                // ---------- SCN Subs ----------
                var returnSubs = grn.ProductionReturnCompSubs
                                .Where(x => x.TransType == "In")
                                .ToList();

                foreach (var s in returnSubs)
                {
                    var item = await _unitOfWork.ItemRepositories.GetAsync(s.ItemId.GetValueOrDefault())
                        ?? throw new InvalidOperationException($"Item not found : {s.ItemId}");

                    var baseQty = s.Qty;

                    var unitPrice = (item.UnitConvert > 0 && item.AltRate > 0)
                        ? item.AltRate
                        : s.UnitPrice;

                    scn.ProductionSCNCompSubs.Add(new ProductionSCNCompSub
                    {
                        SlNo = s.SlNo,
                        ItemId = s.ItemId.GetValueOrDefault(),
                        RefReturnSubId = s.ReturnSubId,
                        BalQty = baseQty ?? 0,
                        AccQty = baseQty ?? 0,
                        UnitPrice = unitPrice,
                        RcSubId = s.RefRcSubId ?? null,
                        CostId = s.CostId
                    });
                }

                await _unitOfWork.ProductionSCNComps.CreateAsync(scn);
                await _unitOfWork.SaveAsync();

                changes.AppendLine($"SCN Created : {scn.SCNNo}");

                foreach (var sub in scn.ProductionSCNCompSubs)
                {

                    if (sub.RefReturnSubId > 0)
                    {
                        var totQty = sub.AccQty + sub.RejQty + sub.RewQty;
                        await _scnService.AdjustReturnCompBalanceAsync(sub.RefReturnSubId, 0, totQty, "Production SCN Component Creation");

                        //Issue From DifferStore
                        await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, grn.AddStoreId.Value, sub.AccQty,
                            sub.UnitPrice, null, screenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, sub.RcSubId, allowMultipleIssue: true);

                        //Add To Differ Store
                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, addStore, sub.AccQty,
                            sub.UnitPrice, null, screenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, sub.Remark, sub.RcSubId, allowMultipleAdd: true);

                       

                    }

                    if (sub.RcSubId.GetValueOrDefault() > 0)
                    {
                        await _scnService.SyncRCSubProcessQtyAsync(sub.RcSubId,
                                                        0, 0, 0,
                                                        sub.AccQty, sub.RejQty, sub.RewQty,
                                                        "QC Create");

                    }


                  

                }
                await _unitOfWork.SaveAsync();

                await LogChangesAsync(changes, "Production SCN Created");
            }
            catch (Exception ex)
            {
                changes.AppendLine($"ERROR : {ex.Message}");
                await LogChangesAsync(changes, "Production SCN Creation Failed");
                throw;
            }
        }

        private async Task CreateProuctionSCNFromProductionGRNCompAsync(ProductionReturnComp grn)
        {
            var changes = new StringBuilder();

            try
            {
                //var suffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);


                //var MapStoreForProdSCN = await GetMappedStoreForFormAsync("SCNProductionAssyForm");
                //var addStore = 1;

                //if (MapStoreForProdSCN.StoreId > 0)
                //{
                //    addStore = MapStoreForProdSCN.StoreId;
                //}


                //var scn = new ProductionSCNAssy
                //{
                //    Suffix = suffix,
                //    SCNNo = await _productionSCNAssyService.GetSCNNumberAsync(suffix),
                //    SCNDate = DateTime.Now,
                //    IssueStoreId = grn.AddStoreId,
                //    AddStoreId = addStore,
                //    CreatedBy = grn.CreatedBy,
                //    CreatedDate = DateTime.Now,
                //    MainRemark = "Auto-generated due to No Inspection",
                //};

                //scn.ProductionSCNAssySubs = grn.ProductionReturnCompSubs.Select(s => new ProductionSCNAssySub
                //{
                //    SlNo = s.SlNo,
                //    SCNId = scn.SCNId,
                //    ItemId = s.ItemId.Value,
                //    RefReturnSubId = s.ReturnSubId,
                //    BalQty = s.QtyReturned.GetValueOrDefault(),
                //    AccQty = s.QtyReturned.GetValueOrDefault(),
                //    UnitPrice = s.UnitPrice,
                //    CostId = s.CostId,
                //}).ToList();


                //// Save SCN
                //await _unitOfWork.ProductionSCNAssys.CreateAsync(scn);
                //await _unitOfWork.SaveAsync();

                //foreach (var sub in scn.ProductionSCNAssySubs)
                //{
                //    if (sub.RefReturnSubId > 0)
                //    {
                //        var ReturnAssySub = await _unitOfWork.ProductionReturnCompSubs.GetAsync(sub.RefReturnSubId.Value);

                //        if (ReturnAssySub == null)
                //            throw new InvalidOperationException("Invalid GRN Sub reference.");

                //        ReturnAssySub.BalQty -= sub.AccQty;

                //        await _unitOfWork.ProductionReturnCompSubs.UpdateAsync(ReturnAssySub);
                //        await _unitOfWork.SaveAsync();

                //        var totalBalQty = await _unitOfWork.ProductionReturnCompSubs
                //            .GetQueryable()
                //            .Where(x => x.ReturnId == ReturnAssySub.ReturnId)
                //            .SumAsync(x => x.BalQty);

                //        grn.ReturnTally = (totalBalQty == 0);
                //        await _unitOfWork.ProductionReturnComps.UpdateAsync(grn);
                //        await _unitOfWork.SaveAsync();
                //    }

                //    var ScnScreenCode = await GetScreenCodeByScreenNameAsync("Production SCN Assembly");

                //    await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, grn.AddStoreId.Value, sub.AccQty,
                //        sub.UnitPrice, null, ScnScreenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, allowMultipleIssue: true);

                //    await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, addStore, sub.AccQty,
                //        sub.UnitPrice, null, ScnScreenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, sub.Remark, allowMultipleAdd: true);

                //}

                //await LogChangesAsync(changes, "Purchase SCN Created");
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<decimal?> GetIssueBalQtyByIssueSubId(int issueSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs.GetQueryable()
                        .Where(x => x.IssueSubId == issueSubId)
                        .SumAsync(x => x.BalQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading Production Issue Compo details for IssueSubId: {issueSubId}");
                throw new InvalidOperationException($"Failed to load Issue Comp details for IssueSubId: {issueSubId}", ex);
            }

        }


        public async Task<List<ProductionReturnCompTrackVM>> GetAllExistingRouteCardAsync()
        {
            try
            {
                return await _unitOfWork.RouteCards.GetQueryable()
                    //.Where(rc => rc.RcBalQty > 0)
                    .OrderBy(rc => rc.RCId)
                    .Select(rc => new ProductionReturnCompTrackVM
                    {
                        RefRcSubId = rc.RCId,
                        RefRcNo = rc.RCNo + rc.Suffix
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching existing Route Cards with balance quantity");
                throw;
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
                screen: "Production Return Assembly",
                action: action,
                additionalInfo: changes.ToString()
            );
        }



        public async Task<ProductionReturnCompSubVM?> GetProdReturnSubItemDetailByReturnSubIdAsync(int returnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionReturnCompSubs
                    .GetQueryable()
                    .Where(q => q.ReturnSubId == returnSubId)
                    .Select(q => new ProductionReturnCompSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production Return sub item detail for ReturnSubId: {returnSubId}");
                throw new InvalidOperationException("Failed to retrieve Prouction Return sub-item details.");
            }
        }

        public async Task<List<ProductionReturnCompStatusVM>> GetProductionReturnComponentStatusListAsync(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<ProductionReturnCompStatusVM>("Sp_GetProductionReturnCompStatusList", status);
                return result.ToList();


            }
            catch (Exception ex)
            {

                throw;
            }
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
                    routeCardSub.IssuedQty += oldQty;
                    routeCardSub.WipQty -= oldQty;
                }

                if (newQty > routeCardSub.IssuedQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Route Card Item Balance Qty.");

                if (newQty > 0)
                {
                    routeCardSub.IssuedQty -= newQty;
                    routeCardSub.WipQty += newQty;
                }

                routeCardSub.ProcessStatus = await GetProcessStatusAsync(routeCardSub);

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardSub);
                await _unitOfWork.SaveAsync();
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

        public async Task<List<ProductionIssueCompVM>> LoadDcOutNumbersPoWiseAsync( List<int> poIds,string check,bool receivedReturn)
        {
            try
            {
                if (poIds == null || poIds.Count == 0)
                    return new List<ProductionIssueCompVM>();

                List<ProductionIssueCompVM> dcOuts;

                if (check == "PO")
                {
                    var poSubIds = await _unitOfWork.MfgPoSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(x => poIds.Contains(x.PoId))
                        .Select(x => x.PoSubId)
                        .ToListAsync();

                    if (!receivedReturn)
                    {
                        // SubCon DC Out Pending
                        dcOuts = await (
                            from dc in _unitOfWork.ProductionIssueComps.GetQueryable().AsNoTracking()
                            join dcSub in _unitOfWork.ProductionIssueCompSubs.GetQueryable().AsNoTracking()
                                on dc.IssueId equals dcSub.IssueId
                            where
                                !dc.IssueTally
                                && !dc.Cancel
                              
                                && dcSub.RefPoSubId.HasValue
                                && poSubIds.Contains(dcSub.RefPoSubId.Value)    // Same DC must contain pending OUT item
                                && _unitOfWork.ProductionIssueCompSubs.GetQueryable().Any(dcOut =>
                                       dcOut.IssueId == dcSub.IssueId
                                       && dcOut.TransType == "Out"
                                  
                                       && dcOut.BalQty > 0)


                            group dc by new
                            {
                                dc.IssueId,
                                dc.IssueNo,
                                dc.Suffix
                            }
                            into g

                            select new ProductionIssueCompVM
                            {
                                IssueId = g.Key.IssueId,
                                IssueNo = g.Key.IssueNo + g.Key.Suffix
                            }
                        ).ToListAsync();
                    }
                    else
                    {
                        // SubCon DC Return Pending
                        dcOuts = await (
                            from dc in _unitOfWork.ProductionIssueComps.GetQueryable().AsNoTracking()

                            join dcIn in _unitOfWork.ProductionIssueCompSubs.GetQueryable().AsNoTracking()
                                on dc.IssueId equals dcIn.IssueId

                            where
                                !dc.Cancel
                                && dcIn.TransType == "In"
                                && dcIn.RefPoSubId.HasValue
                                && poSubIds.Contains(dcIn.RefPoSubId.Value)

                                // Same DC must contain pending OUT item
                                && _unitOfWork.ProductionIssueCompSubs.GetQueryable().Any(dcOut =>
                                       dcOut.IssueId == dcIn.IssueId
                                       && dcOut.TransType == "Out"
                                     
                                       && dcOut.BalQty > 0)

                            group dc by new
                            {
                                dc.IssueId,
                                dc.IssueNo,
                                dc.Suffix
                            }
                            into g

                            select new ProductionIssueCompVM
                            {
                                IssueId = g.Key.IssueId,
                                IssueNo = g.Key.IssueNo + g.Key.Suffix
                            }
                        ).ToListAsync();
                    }

                    return dcOuts;
                }
                else
                {
                    // DC OUT Wise

                    var dcOutSubIds = await _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(x =>
                            poIds.Contains(x.IssueId??0)
                   )
                        .Select(x => x.IssueSubId)
                        .ToListAsync();

                    if (!receivedReturn)
                    {
                        dcOuts = await (
                            from dc in _unitOfWork.ProductionIssueComps.GetQueryable().AsNoTracking()
                            join dcSub in _unitOfWork.ProductionIssueCompSubs.GetQueryable().AsNoTracking()
                                on dc.IssueId equals dcSub.IssueId

                            where
                                !dc.IssueTally
                                && !dc.Cancel
                        
                                && dcSub.BalQty > 0
                                && dcOutSubIds.Contains(dcSub.IssueSubId) && dcSub.TransType == "Out"

                            group dc by new
                            {
                                dc.IssueId,
                                dc.IssueNo,
                                dc.Suffix
                            }
                            into g

                            select new ProductionIssueCompVM
                            {
                                IssueId = g.Key.IssueId,
                                IssueNo = g.Key.IssueNo + g.Key.Suffix
                            }
                        ).ToListAsync();
                    }
                    else
                    {
                        dcOuts = await (
                            from dc in _unitOfWork.ProductionIssueComps.GetQueryable().AsNoTracking()
                            join dcSub in _unitOfWork.ProductionIssueCompSubs.GetQueryable().AsNoTracking()
                                on dc.IssueId equals dcSub.IssueId

                            where
                                !dc.Cancel
                           
                                && (dcSub.BalQty > 0)
                                && dcOutSubIds.Contains(dcSub.IssueSubId) && dcSub.TransType == "Out"

                            group dc by new
                            {
                                dc.IssueId,
                                dc.IssueNo,
                                dc.Suffix
                            }
                            into g

                            select new ProductionIssueCompVM
                            {
                                IssueId = g.Key.IssueId,
                                IssueNo = g.Key.IssueNo + g.Key.Suffix
                            }
                        ).ToListAsync();
                    }

                    return dcOuts;
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching SubCon DC Out details for IDs: {string.Join(",", poIds)}");

                throw new InvalidOperationException(
                    "Failed to retrieve SubCon DC Out details.");
            }
        }

        public async Task<List<Dictionary<string, object>>> LoadPoSubsByReturnMaterialPoIds(List<int> poIds,List<int> DcoutIds, int storeIssId)
        {
            try
            {
                if (DcoutIds == null || DcoutIds.Count == 0)
                    return new List<Dictionary<string, object>>();

                // Get selected PO SubIds
                var poSubIds = await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(x => poIds.Contains(x.PoId))
                    .Select(x => x.PoSubId)
                    .ToListAsync();

                // OUT rows + matching IN rows
                var dcOutData = await (
                    from s in _unitOfWork.ProductionIssueComps
                        .GetQueryable()
                        .AsNoTracking()

                    join outSub in _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .AsNoTracking()
                        on s.IssueId equals outSub.IssueId

                    join inSub in _unitOfWork.ProductionIssueCompSubs
                        .GetQueryable()
                        .AsNoTracking()
                        on outSub.IssueId equals inSub.IssueId

                    where DcoutIds.Contains(s.IssueId)

                          && !s.Cancel

                          && outSub.TransType != null
                          && outSub.TransType.ToUpper() == "OUT"
                          && outSub.BalQty > 0

                          && inSub.TransType != null
                          && inSub.TransType.ToUpper() == "IN"

                          && inSub.RefPoSubId.HasValue
                          && poSubIds.Contains(inSub.RefPoSubId.Value)

                    select new
                    {
                        IssueSubId = outSub.IssueSubId,

                        outSub.IssueId,

                        outSub.ItemId,

                        Qty = outSub.BalQty,
                        BalQty = outSub.BalQty,

                        RefPoSubId = inSub.RefPoSubId,

                        outSub.CostId,

                        ProjectNo = outSub.CostCenter.ProjectNo,

                        outSub.Item.ItemCode,
                        outSub.Item.ItemName,
                        outSub.Item.Specification,
                        outSub.Item.MeasureUnit,
                        outSub.Item.HSNCode,

                        Category = outSub.Item.Category.CategoryName,
                        outSub.Item.CategoryCode
                    }
                )
                .Distinct()
                .ToListAsync();

                // PO Details
                var refPoSubIds = dcOutData
                    .Where(x => x.RefPoSubId.HasValue)
                    .Select(x => x.RefPoSubId.Value)
                    .Distinct()
                    .ToList();

                var poDict = await (
                    from poSub in _unitOfWork.MfgPoSubs
                        .GetQueryable()
                        .AsNoTracking()

                    join po in _unitOfWork.MfgPos
                        .GetQueryable()
                        .AsNoTracking()
                        on poSub.PoId equals po.PoId

                    where refPoSubIds.Contains(poSub.PoSubId)

                          && !poSub.ItemCancel
                          && !po.PoCancl

                    select new
                    {
                        poSub.PoSubId,

                        PONo = po.PONo,
                        POSuffix = po.Suffix,
                        PODate = po.PODate,

                        poSub.UnitPrice
                    }
                ).ToDictionaryAsync(x => x.PoSubId);

                // Stock
                var itemIds = dcOutData
                  .Where(x => x.ItemId.HasValue)
                  .Select(x => x.ItemId!.Value)
                  .Distinct()
                  .ToList();

                var stockDict = await _stockManagerService
                    .GetStockForItemsAsync(itemIds, storeIssId);

                // Final Result
                var result = dcOutData.Select(x =>
                {
                    stockDict.TryGetValue(x.ItemId??0, out decimal stockQty);

                    poDict.TryGetValue(x.RefPoSubId ?? 0, out var po);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["IssueSubId"] = x.IssueSubId,

                        ["RefPoSubId"] = x.RefPoSubId,

                        ["PoNo"] = po != null
                            ? (po.PONo ?? "") + (po.POSuffix ?? "")
                            : "",

                        ["PODate"] = po?.PODate,

                        ["UnitPrice"] = po?.UnitPrice ?? 0,

                        ["ItemId"] = x.ItemId,

                        ["ItemCode"] = x.ItemCode ?? "",
                        ["ItemName"] = x.ItemName ?? "",
                        ["Specification"] = x.Specification ?? "",

                        ["UOM"] = x.MeasureUnit ?? "",

                        ["HSNCode"] = x.HSNCode ?? "",

                        ["Category"] = x.Category ?? "",

                        ["Qty"] = x.Qty,
                        ["BalQty"] = x.BalQty,

                        ["StockQty"] = stockQty,

                        ["CategoryCode"] = x.CategoryCode,

                        ["CostCenterId"] = x.CostId,

                        ["ProjectNo"] = x.ProjectNo ?? ""
                    };
                }).ToList();

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetPendingPoCountAsync(int CustId)
        {
            try
            {
                return await _unitOfWork.MfgPos
                    .GetQueryable()
                    .Where(p =>
                        p.CustId == CustId &&
                        !p.PoTally &&
                        !p.PoCancl &&
                        p.IsAuthorized &&
                        !p.ShortClose &&
                        p.MfgPoSubs.Any(ps => !ps.ItemCancel && ps.BalQty > 0)
                    )
                    .CountAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while fetching Pending PO Count for CustId = {CustId}");
                return 0;
            }
        }

        public async Task<int> GetPendingDcOutsPoCountAsync(int CustId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueComps
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(h =>
                            h.CustId == CustId &&

                            // Check valid IN items
                            h.ProductionIssueCompSubs.Any(s =>
                                s.TransType == "In" &&
                                !s.MfgPoSub.ItemCancel &&
                                !s.MfgPoSub.MfgPo.PoCancl &&
                                !s.MfgPoSub.MfgPo.ShortClose) &&

                            // Check OUT item balance
                            h.ProductionIssueCompSubs.Any(s => s.BalQty > 0)
                        )
                        .CountAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"GetPendingDcOutsPoCountAsync using CustId : ({CustId})");

                return 0;
            }
        }

        public async Task<List<MfgPoVM>> GetOpenPurchPosByReturnMaterialVendor(int CustId)
        {
            try
            {
                var result = await (
                    from po in _unitOfWork.MfgPos.GetQueryable().AsNoTracking()

                    join pos in _unitOfWork.MfgPoSubs.GetQueryable().AsNoTracking()
                        on po.PoId equals pos.PoId

                    join dcIn in _unitOfWork.ProductionIssueCompSubs.GetQueryable().AsNoTracking()
                        on pos.PoSubId equals dcIn.RefPoSubId

                    where po.CustId == CustId

                          && !po.PoCancl
                          && !pos.ItemCancel

                          // IN row contains RefPoSubId
                          && dcIn.TransType == "In"
                          
                          && dcIn.RefPoSubId > 0

                          // SAME DCID must contain pending OUT item
                          && _unitOfWork.ProductionIssueCompSubs.GetQueryable().Any(dcOut =>
                                 dcOut.IssueId == dcIn.IssueId
                                 && dcOut.TransType == "Out"
                             
                                 && dcOut.BalQty > 0)

                    select new
                    {
                        po.PoId,
                        po.PONo,
                        po.Suffix
                    }
                )
                .Distinct()
                .OrderBy(x => x.PONo)
                .Select(x => new MfgPoVM
                {
                    PoId = x.PoId,
                    PONo = (x.PONo ?? "") + (x.Suffix ?? "")
                })
                .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<ProductionIssueCompSubVM>> GetProductionIssueQtyByDcIdsAsync(List<int> IssueIds)
        {
            if (IssueIds == null || IssueIds.Count == 0)
                return new List<ProductionIssueCompSubVM>();

            return await _unitOfWork.ProductionIssueCompSubs
                .GetQueryable()
                .Where(x =>
                    IssueIds.Contains(x.IssueId.Value) &&
                   (x.BalQty > 0) 
                   && x.TransType == "Out")
                .GroupBy(x => x.ItemId)
                .Select(g => new ProductionIssueCompSubVM
                {
                    ItemId = g.Key,
                    BalQty = g.Sum(x => x.BalQty)
                })
                .ToListAsync();
        }
        public async Task<List<ProductionIssueCompSubVM>> GetProductionCompQtyByIssueIdsAsync(List<int> IssueIds)
        {
            if (IssueIds == null || !IssueIds.Any())
                return new List<ProductionIssueCompSubVM>();

            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Include(x => x.Item)
                    .Where(x =>
                        IssueIds.Contains(x.IssueId.Value) && x.TransType == "Out" &&
                        (x.BalQty > 0))   // OR condition
                    .Select(x => new ProductionIssueCompSubVM
                    {
                        IssueSubId = x.IssueSubId,
                        IssueId = x.IssueId.Value,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.ItemName,
                        MeasureUnit = x.Item.MeasureUnit,
                        Qty = x.BalQty,   // you can change if needed
                        BalQty = x.BalQty,
                        UnitPrice = x.UnitPrice,
                        RefPoSubId = x.RefPoSubId,
                        Remark = x.Remark,
                        IsEditable = false
                    })
                    .ToListAsync();


            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetProductionCompQtyByIssueIdsAsync data for ProductionIssueCompSubVM: {string.Join(",", IssueIds)}");
                return new List<ProductionIssueCompSubVM>();
            }
        }

        public async Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId)
        {
            try
            {
                return await _unitOfWork.AssmblyDefs
                    .GetQueryable()
                    .Where(x => x.AssmblyID == assemblyId)
                    .Select(x => new AssemblyDefVM
                    {
                        ItemId = x.ItemId,
                        UtilQty = x.UtilQty
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Assembly Items for AssemblyId: {assemblyId}");
                throw new InvalidOperationException("Failed to retrieve Assembly items. Please try again.", ex);
            }
        }
        public async Task<decimal> GetRawMaterialWeightAsync(int compItemId)
        {
            return await _unitOfWork.CompMasters
                .GetQueryable()
                .AsNoTracking()
                .Where(c => c.CompItemId == compItemId)
                .OrderByDescending(c => c.IsDefaultRM)
                .Select(c => c.Weight)
                .FirstOrDefaultAsync();
        }
        public async Task<List<ItemVM>> GetRawMaterialItemsAsync(int compItemId)
        {
            try
            {
                return await _unitOfWork.CompMasters
                    .GetQueryable()
                    .Where(x => x.CompItemId == compItemId)
                    .OrderByDescending(c => c.IsDefaultRM)
                    .Select(x => new ItemVM
                    {
                        ItemId = x.RMId ?? 0,
                        Weight = x.Weight
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Raw Material Items for CompItemId: {compItemId}");
                throw new InvalidOperationException("Failed to retrieve raw material items. Please try again.", ex);
            }
        }

        public async Task<int> GetProductionIssuedetailsForVendor(int CustId)
        {
            return await (
                from p in _unitOfWork.ProductionIssueComps.GetQueryable()
                join ps in _unitOfWork.ProductionIssueCompSubs.GetQueryable()
                    on p.IssueId equals ps.IssueId
                where ps.TransType == "In"
                      && ps.BalQty > 0
                      && p.CustId == CustId
                      && p.IsWithoutPoDc == true
                      && _unitOfWork.ProductionIssueCompSubs.GetQueryable().Any(dsOut =>
                             dsOut.IssueId == ps.IssueId
                          && dsOut.TransType == "Out"
                          && dsOut.BalQty > 0
                      )
                select 1
            ).CountAsync();
        }

       
        public async Task<List<Dictionary<string, object>>> GetProductionDetailsByWithOutPoCustId(int CustId)
        {
            try
            {
                var subQuery = _unitOfWork.ProductionIssueCompSubs.GetQueryable();

                var dcData = await (
                    from p in _unitOfWork.ProductionIssueComps.GetQueryable()

                    join ps in subQuery
                        on p.IssueId equals ps.IssueId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId

                    where ps.TransType == "In"
                          && ps.BalQty > 0 && p.IsWithoutPoDc
                          && p.CustId == CustId && !p.Cancel && !p.ShortClose

                          // Optimized EXISTS
                          && subQuery.Any(dsOut =>
                                dsOut.IssueId == ps.IssueId
                                && dsOut.TransType == "Out"
                                && dsOut.BalQty > 0
                          )

                    select new
                    {
                        ps.IssueSubId,
                        p.IssueId,
                        p.IssueNo,
                        p.Suffix,
                        p.IssueDate,

                        ps.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.MeasureUnit,

                        i.CategoryCode,
                        CategoryName = i.Category.CategoryName,

                        ps.BalQty,
                        ps.UnitPrice,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,

                        MainRemarks = p.MainRemark
                    }
                ).ToListAsync();

                if (!dcData.Any())
                    return new List<Dictionary<string, object>>();

                var result = dcData.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,

                    ["IssueSubId"] = r.IssueSubId,
                    ["IssueId"] = r.IssueId,
                    ["IssueNo"] = $"{r.IssueNo}{r.Suffix}",
                    ["IssueDate"] = r.IssueDate.ToString("dd/MM/yyyy"),

                    ["TransType"] = "In",

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,

                    ["CategoryCode"] = r.CategoryCode,
                    ["CategoryName"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,

                    ["UnitPrice"] = r.UnitPrice,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["Remark"] = r.MainRemarks ?? string.Empty
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching production details for CustId: {CustId}");
                throw new InvalidOperationException("Failed to retrieve production details.", ex);
            }
        }
        public async Task<List<Dictionary<string, object>>> GetDCDetailsBy()
        {
            try
            {
                var subQuery = _unitOfWork.ProductionIssueCompSubs.GetQueryable();

                var dcData = await (
                    from p in _unitOfWork.ProductionIssueComps.GetQueryable()

                    join ps in subQuery
                        on p.IssueId equals ps.IssueId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId

                    where ps.TransType == "In"
                          && ps.BalQty > 0
                          && p.CustId == null && ps.RcSubId == null && !p.Cancel && !p.ShortClose

                          // Optimized EXISTS
                          && subQuery.Any(dsOut =>
                                dsOut.IssueId == ps.IssueId
                                && dsOut.TransType == "Out"
                                && dsOut.BalQty > 0
                          )

                    select new
                    {
                        ps.IssueSubId,
                        p.IssueId,
                        p.IssueNo,
                        p.Suffix,
                        p.IssueDate,

                        ps.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.MeasureUnit,

                        i.CategoryCode,
                        CategoryName = i.Category.CategoryName,

                        ps.BalQty,
                        ps.UnitPrice,
                        ps.RefPoSubId,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,

                        MainRemarks = p.MainRemark
                    }
                ).ToListAsync();

                if (!dcData.Any())
                    return new List<Dictionary<string, object>>();

                var result = dcData.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,

                    ["IssueSubId"] = r.IssueSubId,
                    ["IssueId"] = r.IssueId,
                    ["IssueNo"] = $"{r.IssueNo}{r.Suffix}",
                    ["IssueDate"] = r.IssueDate.ToString("dd/MM/yyyy"),


                    ["TransType"] = "In",

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,

                    ["CategoryCode"] = r.CategoryCode,
                    ["CategoryName"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,

                    ["UnitPrice"] = r.UnitPrice,
                    ["RefPoSubId"] = r.RefPoSubId,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["Remark"] = r.MainRemarks ?? string.Empty
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DC details");
                throw new InvalidOperationException("Failed to retrieve DC details.", ex);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetDCDetailsByCustId(int CustId)
        {
            try
            {
                var subQuery = _unitOfWork.ProductionIssueCompSubs.GetQueryable();

                var dcData = await (
                    from p in _unitOfWork.ProductionIssueComps.GetQueryable()

                    join ps in subQuery
                        on p.IssueId equals ps.IssueId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId

                    where ps.TransType == "In"
                          && ps.BalQty > 0
                          && p.CustId == CustId && ps.RcSubId == null && !p.Cancel && !p.ShortClose

                          // Optimized EXISTS
                          && subQuery.Any(dsOut =>
                                dsOut.IssueId == ps.IssueId
                                && dsOut.TransType == "Out"
                                && dsOut.BalQty > 0
                          )

                    select new
                    {
                        ps.IssueSubId,
                        p.IssueId,
                        p.IssueNo,
                        p.Suffix,
                        p.IssueDate,

                        ps.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.MeasureUnit,

                        i.CategoryCode,
                        CategoryName = i.Category.CategoryName,

                        ps.BalQty,
                        ps.UnitPrice,
                        ps.RefPoSubId,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,

                        MainRemarks = p.MainRemark
                    }
                ).ToListAsync();

                if (!dcData.Any())
                    return new List<Dictionary<string, object>>();

                var result = dcData.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,

                    ["IssueSubId"] = r.IssueSubId,
                    ["IssueId"] = r.IssueId,
                    ["IssueNo"] = $"{r.IssueNo}{r.Suffix}",
                    ["IssueDate"] = r.IssueDate.ToString("dd/MM/yyyy"),


                    ["TransType"] = "In",

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,

                    ["CategoryCode"] = r.CategoryCode,
                    ["CategoryName"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,

                    ["UnitPrice"] = r.UnitPrice,
                    ["RefPoSubId"] = r.RefPoSubId,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["Remark"] = r.MainRemarks ?? string.Empty
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DC details for CustId: {CustId}");
                throw new InvalidOperationException("Failed to retrieve DC details.", ex);
            }
        }
        public async Task<List<Dictionary<string, object>>> GetProductionDetailsByReturnCustId(int CustId)
        {
            try
            {
                var dcData = await (
                    from p in _unitOfWork.ProductionIssueComps.GetQueryable()

                    join ps in _unitOfWork.ProductionIssueCompSubs.GetQueryable()
                        on p.IssueId equals ps.IssueId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId

                    where p.CustId == CustId
                          && ps.TransType == "Out"     // ✅ only OUT items
                          && ps.BalQty > 0 && p.IsWithoutPoDc           // ✅ balance qty only

                    select new
                    {
                        ps.IssueSubId,
                        p.IssueId,
                        p.IssueNo,
                        p.Suffix,
                        p.IssueDate,


                        ps.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.MeasureUnit,

                        i.CategoryCode,
                        CategoryName = i.Category.CategoryName,

                        ps.BalQty,
                        ps.UnitPrice,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,

                        MainRemarks = p.MainRemark
                    }
                ).ToListAsync();

                if (!dcData.Any())
                    return new List<Dictionary<string, object>>();

                var result = dcData.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,

                    ["IssueSubId"] = r.IssueSubId,
                    ["IssueId"] = r.IssueId,
                    ["IssueNo"] = $"{r.IssueNo}{r.Suffix}",
                    ["IssueDate"] = r.IssueDate.ToString("dd/MM/yyyy"),


                    ["TransType"] = "In",

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,

                    ["CategoryCode"] = r.CategoryCode,
                    ["CategoryName"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,

                    ["UnitPrice"] = r.UnitPrice,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["Remark"] = r.MainRemarks ?? string.Empty
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DC details for CustId: {CustId}");
                throw new InvalidOperationException("Failed to retrieve DC details.", ex);
            }
        }
        public async Task<List<int>> GetProductionConDcOutIdsByDcSubIdAsync(int IssueSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => x.IssueSubId == IssueSubId && x.IssueId > 0 && x.BalQty > 0 )
                    .Select(x => x.IssueId.Value)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching for IssueSubId: {IssueSubId}");
                throw new InvalidOperationException("Failed to retrieve DcOutgoing details for the selected DC. Please try again.", ex);
            }
        }

        public async Task<List<int>> GetIssueIdsByIssueSubIdsAsync(List<int> IssueSubIds)
        {
            try
            {
                if (IssueSubIds == null || !IssueSubIds.Any())
                    return new List<int>();

                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Where(x => IssueSubIds.Contains(x.IssueSubId))
                    .Select(x => x.IssueId.Value)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching IssueIds for IssueSubIds: {string.Join(",", IssueSubIds)}");

                throw new InvalidOperationException(
                    "Failed to retrieve production details for selected DCs. Please try again.", ex);
            }
        }
        public async Task<List<ProductionReturnCompSubVM>> GetproductionGrnSubVMsByIssueIdsAsync(List<int> dcIds)
        {
            if (dcIds == null || !dcIds.Any())
                return new List<ProductionReturnCompSubVM>();

            try
            {
                return await _unitOfWork.ProductionIssueCompSubs
                    .GetQueryable()
                    .Include(x => x.Item)
                    .Where(x => dcIds.Contains(x.IssueId.Value)
                             && x.TransType == "Out" && x.BalQty > 0)
                    .Select(x => new ProductionReturnCompSubVM
                    {
                        RefIssueSubId = x.IssueSubId,
                        RefIssueNo = $"{x.ProductionIssueComp.IssueNo}{x.ProductionIssueComp.Suffix}",
                        RefIssueDate = x.ProductionIssueComp.IssueDate,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.ItemName,
                        MeasureUnit = x.Item.MeasureUnit,
                        Qty = x.BalQty.GetValueOrDefault(),
                        BalQty = x.BalQty ?? 0m,
                        UnitPrice = x.UnitPrice,
                        RefPoSubId = x.RefPoSubId,
                        Remark = x.Remark,
                        TransType = "Out",


                        IsEditable = false
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching outgoing data for DcIds: {string.Join(",", dcIds)}");
                throw new InvalidOperationException("Failed to retrieve outgoing DC issue details.", ex);
            }
        }

        public async Task<(bool CanDelete, string Message)> CanRemoveProductionReturnAsync(int ReturnId, int ReturnSubId)
        {
            try
            {
                var DcSubGrnIds = await _unitOfWork.ProductionReturnComps
                                     .GetQueryable()
                                     .Where(s => s.ReturnId == ReturnId)
                                     .SelectMany(s => s.ProductionReturnCompSubs.Select(sub => sub.ReturnSubId))
                                     .ToListAsync();

                if (!DcSubGrnIds.Any())
                    return (true, "Production Return can be safely deleted.");


                bool Quotation = await _unitOfWork.ProductionSCNCompSubs
                    .GetQueryable()
                    .AnyAsync(qs => DcSubGrnIds.Contains(qs.RefReturnSubId.Value));

                if (Quotation)
                    return (false, "Cannot delete this Production SCN  as a some transaction Made.");

                var sums = await (
                                 from sub in _unitOfWork.ProductionReturnCompSubs.GetQueryable()
                                 where sub.ReturnSubId == ReturnSubId
                                 group sub by 1 into g
                                 select new
                                 {
                                     TotalQty = g.Sum(s => (decimal?)s.Qty) ?? 0,
                                     TotalBalQty = g.Sum(s => (decimal?)s.BalQty) ?? 0
                                 }
                             ).FirstOrDefaultAsync();

                bool hasPurchPo = sums != null && sums.TotalQty == sums.TotalBalQty;

                if (!hasPurchPo)
                    return (false, "Cannot delete this Production GRN as some transactions have been made.");

             
                return (true, "Production Outgoing can be safely deleted.");

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteEnquiryAsync for ReturnId: {ReturnId}");
                throw new Exception("Error checking Production Return delete eligibility", ex);
            }
        }
        public async Task UpdatedCancelStatusAndAddOrRevertQtyPoWiseAync(ProductionReturnCompVM dcVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();
                bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
                var existingDC = await _unitOfWork.ProductionReturnComps.GetAsync(dcVM.ReturnId);
                if (existingDC == null)
                    throw new InvalidOperationException("Production GRN not found.");

                var subs = await _unitOfWork.ProductionReturnCompSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.ReturnId == dcVM.ReturnId)
                    .ToListAsync();

                if (!dcVM.Cancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateProductionReturnBalanceBeforeRevertAsync(sub, dcVM);

                    }
                }

                existingDC.Cancel = dcVM.Cancel;
                existingDC.CancelReason = dcVM.CancelReason;
                await _unitOfWork.ProductionReturnComps.UpdateAsync(existingDC);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {

                    decimal newAcceptedQty, oldAcceptedQty = 0;
                    bool rejRetrack = false;

                    if (existingDC.Cancel)
                    {

                        if (sub.ReturnSubId > 0)
                            await RollbackTracksAddIssueQtyAsync(sub.ReturnSubId);


                        if (sub.RefRcSubId.GetValueOrDefault() > 0 && sub.TransType == "In")
                        {
                            await AdjustRcBalanceAsync(sub.RefRcSubId, sub.Qty ?? 0, 0, "SubContract GRN Delete");

                            var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

                            var issueSubs = await _unitOfWork.SubConDCOutSubs
                                               .GetQueryable()
                                               .Where(x => issueIds.Contains(x.DcId)
                                                  && x.TransType == "In")
                                               .OrderBy(x => x.DcSubId)
                                               .ToListAsync();

                            if (issueSubs.Any())
                            {
                                foreach (var Grnsub in issueSubs)
                                {
                                    await AdjustProductionCompIssueItemBalanceAsync(Grnsub.DcSubId, sub.Qty ?? 0, 0, "SubContract GRN Create");
                                }

                            }

                        }

                        if (sub.TransType == "In" && sub.RefIssueSubId.GetValueOrDefault() > 0 && !PoWiseDcOut)
                        {
                            await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, sub.Qty ?? 0, 0, "SubContract GRN Delete");
                        }

                        if (PoWiseDcOut)
                        {

                            if (sub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value, sub.ItemId.Value, sub.Qty ?? 0, 0, "Subcontract GRN Delete");
                            }
                        }
                        await DeleteStockAddAsync(sub.ReturnSubId, sub.ItemId.Value, screenCode, existingDC.ReturnNo);

                    }
                    else
                    {
                        if (!existingDC.Rejection && !existingDC.Return)
                        {

                            if (sub.RefRcSubId.GetValueOrDefault() > 0)
                            {
                                await CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.ReturnSubId, sub.RefRcSubId.Value,
                                    sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now);

                                await UpsertRCSubWipIssuedQtyAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "Production return Component Create");


                                var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

                                var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                                                   .GetQueryable()
                                                   .Where(x => issueIds.Contains(x.IssueId.Value) &&
                                                       (x.BalQty ?? 0) > 0
                                                       && x.TransType == "In")
                                                   .OrderBy(x => x.IssueSubId)
                                                   .ToListAsync();

                                if (issueSubs.Any())
                                {
                                    foreach (var Grnsub in issueSubs)
                                    {
                                        await AdjustProductionCompIssueItemBalanceAsync(Grnsub.IssueSubId, 0, sub.Qty ?? 0, "Production Return Create");
                                    }

                                }

                                if (sub.TransType == "In" && sub.RefIssueSubId > 0)
                                {
                                    await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, 0, sub.Qty ?? 0, "Production Return Create");
                                }
                            }
                            else if (sub.TransType == "In" && sub.RefIssueSubId.GetValueOrDefault() > 0 && !PoWiseDcOut)
                            {
                                await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.ReturnSubId, sub.RefIssueSubId.Value,
                                    sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now, existingDC.IsManual, dcVM.ProductionReturnCompSubVMs, sub?.RefPoSubId);

                                await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId, 0, sub.Qty ?? 0, "Production return Component Create");
                            }

                            if (PoWiseDcOut)
                            {
                                if (sub.TransType == "In" && sub.RefRcSubId == null)
                                {
                                    if (!existingDC.IsManual)
                                    {
                                        await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(sub.ReturnSubId, sub.RefPoSubId ?? 0,
                                          sub.ItemId ?? 0, sub.Qty ?? 0, screenCode, currentUser, now, sub.RefIssueSubId ?? 0);
                                    }
                                    else if (existingDC.IsManual)
                                    {
                                        await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.ReturnSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, dcVM.ProductionReturnCompSubVMs, existingDC.Return);

                                        if (sub.RefIssueSubId.GetValueOrDefault() > 0 && existingDC.IsWithoutPoDc)
                                        {
                                            await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
                                        }
                                    }

                                }

                                if (sub.RefPoSubId.GetValueOrDefault() > 0)
                                {
                                    await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value, sub.ItemId.Value, sub.Qty ?? 0, 0, "Production return Component Create");
                                }

                            }
                        }
                        else
                        {


                            if (sub.RefRcSubId.HasValue && sub.RefRcSubId > 0)
                            {
                                await RevertRCSubIssuedToBalQtyAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "SubContract GRN Return/Rejection");
                            }

                            if (sub.TransType == "In")
                            {

                                if (existingDC.IsManual)
                                {
                                    await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.ReturnSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, dcVM.ProductionReturnCompSubVMs, existingDC.Return);
                                }

                            }

                        }

                        if (sub.TransType == "In")
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, existingDC.AddStoreId.Value, sub.Qty.GetValueOrDefault(),
                                sub.UnitPrice, null, screenCode, sub.ReturnSubId, existingDC.ReturnNo, existingDC.ReturnDate, null, sub.RefRcSubId);
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

        //public async Task UpdatedCancelStatusAndAddOrRevertQtyPoWiseAync(ProductionReturnCompVM dcVM, int screenCode)
        //{
        //    using var transaction = await _unitOfWork.BeginTransactionAsync();
        //    try
        //    {
        //        var now = DateTime.Now;
        //        var currentUser = await _currentUserService.GetUsernameAsync();
        //        bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
        //        var existingDC = await _unitOfWork.ProductionReturnComps.GetAsync(dcVM.ReturnId);
        //        if (existingDC == null)
        //            throw new InvalidOperationException("Production GRN not found.");

        //        var subs = await _unitOfWork.ProductionReturnCompSubs
        //            .GetQueryable()
        //            .Include(s => s.Item)
        //            .Where(s => s.ReturnId == dcVM.ReturnId)
        //            .ToListAsync();

        //        if (!dcVM.Cancel)
        //        {
        //            foreach (var sub in subs)
        //            {
        //                await ValidateProductionReturnBalanceBeforeRevertAsync(sub, dcVM);

        //            }
        //        }

        //        existingDC.Cancel = dcVM.Cancel;
        //        existingDC.CancelReason = dcVM.CancelReason;
        //        await _unitOfWork.ProductionReturnComps.UpdateAsync(existingDC);
        //        await _unitOfWork.SaveAsync();

        //        foreach (var sub in subs)
        //        {

        //            decimal newAcceptedQty, oldAcceptedQty = 0;
        //            bool rejRetrack = false;

        //            if (existingDC.Cancel)
        //            {

        //                if (sub.ReturnSubId > 0)
        //                    await RollbackTracksAddIssueQtyAsync(sub.ReturnSubId);


        //                if (sub.RefRcSubId.GetValueOrDefault() > 0 && sub.TransType == "In")
        //                {
        //                    await AdjustRcBalanceAsync(sub.RefRcSubId, sub.Qty ?? 0, 0, "SubContract GRN Delete");

        //                    var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

        //                    var issueSubs = await _unitOfWork.SubConDCOutSubs
        //                                       .GetQueryable()
        //                                       .Where(x => issueIds.Contains(x.DcId)
        //                                          && x.TransType == "In")
        //                                       .OrderBy(x => x.DcSubId)
        //                                       .ToListAsync();

        //                    if (issueSubs.Any())
        //                    {
        //                        foreach (var Grnsub in issueSubs)
        //                        {
        //                            await AdjustProductionCompIssueItemBalanceAsync(Grnsub.DcSubId, sub.Qty ?? 0, 0, "SubContract GRN Create");
        //                        }

        //                    }

        //                }

        //                if (sub.TransType == "In" && sub.RefIssueSubId.GetValueOrDefault() > 0 && !PoWiseDcOut)
        //                {
        //                    await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, sub.Qty ?? 0, 0, "SubContract GRN Delete");
        //                }

        //                if (PoWiseDcOut)
        //                {

        //                    if (sub.RefPoSubId.GetValueOrDefault() > 0)
        //                    {
        //                        await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value,sub.ItemId.Value, sub.Qty ?? 0, 0, "Subcontract GRN Delete");
        //                    }
        //                }
        //                await DeleteStockAddAsync(sub.ReturnSubId, sub.ItemId.Value, screenCode, existingDC.ReturnNo);

        //            }
        //            else
        //            {
        //                if (!existingDC.Rejection && !existingDC.Return)
        //                {

        //                    if (sub.RefRcSubId.GetValueOrDefault() > 0)
        //                    {
        //                        await CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.ReturnSubId, sub.RefRcSubId.Value,
        //                            sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now);

        //                        await UpsertRCSubWipIssuedQtyAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "Production return Component Create");


        //                        var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

        //                        var issueSubs = await _unitOfWork.ProductionIssueCompSubs
        //                                           .GetQueryable()
        //                                           .Where(x => issueIds.Contains(x.IssueId.Value) &&
        //                                               (x.BalQty ?? 0) > 0
        //                                               && x.TransType == "In")
        //                                           .OrderBy(x => x.IssueSubId)
        //                                           .ToListAsync();

        //                        if (issueSubs.Any())
        //                        {
        //                            foreach (var Grnsub in issueSubs)
        //                            {
        //                                await AdjustProductionCompIssueItemBalanceAsync(Grnsub.IssueSubId, 0, sub.Qty ?? 0, "Production Return Create");
        //                            }

        //                        }

        //                        if (sub.TransType == "In" && sub.RefIssueSubId > 0)
        //                        {
        //                            await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, 0, sub.Qty ?? 0, "Production Return Create");
        //                        }
        //                    }
        //                    else if (sub.TransType == "In" && sub.RefIssueSubId.GetValueOrDefault() > 0 && !PoWiseDcOut)
        //                    {
        //                        await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.ReturnSubId, sub.RefIssueSubId.Value,
        //                            sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now, existingDC.IsManual, dcVM.ProductionReturnCompSubVMs, sub?.RefPoSubId);

        //                        await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId, 0, sub.Qty ?? 0, "Production return Component Create");
        //                    }

        //                    if (PoWiseDcOut)
        //                    {
        //                        if (sub.TransType == "In" && sub.RefRcSubId == null)
        //                        {
        //                            if (!existingDC.IsManual)
        //                            {
        //                                await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(sub.ReturnSubId, sub.RefPoSubId ?? 0,
        //                                  sub.ItemId ?? 0, sub.Qty ?? 0, screenCode, currentUser, now, sub.RefIssueSubId ?? 0);
        //                            }
        //                            else if (existingDC.IsManual)
        //                            {
        //                                await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.ReturnSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, dcVM.ProductionReturnCompSubVMs, existingDC.Return);

        //                                if (sub.RefIssueSubId.GetValueOrDefault() > 0 && existingDC.IsWithoutPoDc)
        //                                {
        //                                    await AdjustProductionCompIssueItemBalanceAsync(sub.RefIssueSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
        //                                }
        //                            }

        //                        }

        //                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
        //                        {
        //                            await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value, sub.ItemId.Value, sub.Qty ?? 0, 0, "Production return Component Create");
        //                        }

        //                    }
        //                }
        //                else
        //                {
                           

        //                    if (sub.RefRcSubId.HasValue && sub.RefRcSubId > 0)
        //                    {
        //                        await RevertRCSubIssuedToBalQtyAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "SubContract GRN Return/Rejection");
        //                    }

        //                    if (sub.TransType == "In")
        //                    {

        //                        if (existingDC.IsManual)
        //                        {
        //                            await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.ReturnSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, dcVM.ProductionReturnCompSubVMs, existingDC.Return);
        //                        }

        //                    }

        //                }

        //                if (sub.TransType == "In")
        //                {
        //                    await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, existingDC.AddStoreId.Value, sub.Qty.GetValueOrDefault(),
        //                        sub.UnitPrice, null, screenCode, sub.ReturnSubId, existingDC.ReturnNo, existingDC.ReturnDate, null, sub.RefRcSubId);
        //                }

        //            }
        //        }

        //        await transaction.CommitAsync();
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        await transaction.RollbackAsync();
        //        await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Validation issue");
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        await transaction.RollbackAsync();
        //        await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Unexpected error");
        //        throw new InvalidOperationException("Failed to update cancel/revert status. Please contact support.");
        //    }
        //}
        public async Task ValidateProductionReturnBalanceBeforeRevertAsync(ProductionReturnCompSub sub, ProductionReturnCompVM ProductionReturnCompVMs)
        {
            try
            {
                bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
                if (sub.RefIssueSubId.GetValueOrDefault() > 0)
                {
                    var entity = await _unitOfWork.ProductionIssueCompSubs.GetAsync(sub.RefIssueSubId ?? 0);
                    if (entity == null)
                        throw new InvalidOperationException($"production return  not found for RefIssueSubId: {sub.RefIssueSubId}");

                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because production Return balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
                if (!ProductionReturnCompVMs.Return)
                {
                    if (sub.RefIssueSubId.GetValueOrDefault() > 0 && ProductionReturnCompVMs.IsWithoutPoDc)
                    {
                        var entity = await _unitOfWork.ProductionIssueCompSubs.GetAsync(sub.RefIssueSubId ?? 0);
                        if (entity == null)
                            throw new InvalidOperationException($"Production not found for RefIssueSubId: {sub.RefIssueSubId}");

                        if (entity.BalQty < sub.Qty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot revert because Production return balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                            );
                        }
                    }
                    if (PoWiseDcOut)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            var entity = await _unitOfWork.MfgDcSubs.GetAsync(sub.RefPoSubId ?? 0);

                            if (entity == null)
                                throw new InvalidOperationException($"Purchase Order not found for RefPoSubId: {sub.RefPoSubId}");

                            if (entity.BalQty < sub.Qty)
                            {
                                throw new InvalidOperationException(
                                    $"Cannot revert because Purchase Order balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                                );
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ValidateSubConDcoutBalanceBeforeRevertAsync detail for production Returnid: {ProductionReturnCompVMs.ReturnId}");
                throw new InvalidOperationException("Failed to retrieve ValidateSubConDcoutBalanceBeforeRevertAsync details.");
            }


        }

        public async Task UpsertProductionReturnShortCloseAsync(ProductionReturnCompVM ProductionReturnCompVMs)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingGRN = await _unitOfWork.ProductionReturnComps.GetAsync(ProductionReturnCompVMs.ReturnId);
                if (existingGRN == null)
                    throw new InvalidOperationException("production return not found.");

                existingGRN.ShortClose = ProductionReturnCompVMs.ShortClose;

                await _unitOfWork.ProductionReturnComps.UpdateAsync(existingGRN);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertProductionReturnShortCloseAsync] Validation issue");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertProductionReturnShortCloseAsync] Unexpected error");

            }
        }
        public async Task<List<ProductionReturnCompSub>> GetProductionReturnByReturnidAsync(int ReturnId)
        {
            try
            {
                var subs = await _unitOfWork.ProductionReturnCompSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.ReturnId == ReturnId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetProductionReturnByReturnidAsync items for ReturnId: {ReturnId}");
                throw new InvalidOperationException("Failed to retrieve SGetProductionReturnByReturnidAsync. Please try again.");
            }
        }

        public async Task ValidateProductionIssueBalanceBeforeRevertAsync(ProductionReturnCompSub sub, ProductionReturnCompVM ProductionReturnCompVMs)
        {
            try
            {
                bool PoWiseDcOut = await IsPOWiseProductionDcOutEnabledAsync();
                if (sub.RefIssueSubId.GetValueOrDefault() > 0)
                {
                    var entity = await _unitOfWork.ProductionIssueCompSubs.GetAsync(sub.RefIssueSubId ?? 0);
                    if (entity == null)
                        throw new InvalidOperationException($"Production Component Issue not found for RefIssueSubId: {sub.RefIssueSubId}");

                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Production Component Issues balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
                if (!ProductionReturnCompVMs.Return)
                {
                    if (sub.RefIssueSubId.GetValueOrDefault() > 0 && ProductionReturnCompVMs.IsWithoutPoDc)
                    {
                        var entity = await _unitOfWork.ProductionIssueCompSubs.GetAsync(sub.RefIssueSubId ?? 0);
                        if (entity == null)
                            throw new InvalidOperationException($"Production issues not found for RefDcSunId: {sub.RefIssueSubId}");

                        if (entity.BalQty < sub.Qty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot revert because Production Issue balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                            );
                        }
                    }
                    if (PoWiseDcOut)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            var entity = await _unitOfWork.PurchPoSubs.GetAsync(sub.RefPoSubId ?? 0);

                            if (entity == null)
                                throw new InvalidOperationException($"sales PurchPo not found for RefPoSubId: {sub.RefPoSubId}");

                            if (entity.BalQty < sub.Qty)
                            {
                                throw new InvalidOperationException(
                                    $"Cannot revert because sales order balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                                );
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ValidateProductionIssueBalanceBeforeRevertAsync detail for ProductionReturn Id: {ProductionReturnCompVMs.ReturnId}");
                throw new InvalidOperationException("Failed to retrieve ValidateProductionIssueBalanceBeforeRevertAsync details.");
            }


        }

        public async Task<bool> DeleteByProductionReturnSubIdAsync(int returnSubId, int screenCode)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try

            {
                bool IsPoWise = await IsPOWiseProductionDcOutEnabledAsync();

                var PriductionGrns = await _unitOfWork.ProductionReturnComps
                    .GetQueryable()
                    .Include(e => e.ProductionReturnCompSubs)
                    .Where(e => e.ProductionReturnCompSubs
                        .Any(s => s.ReturnSubId == returnSubId))
                    .FirstOrDefaultAsync();

                if (PriductionGrns == null)
                    return false;

                var subItem = PriductionGrns.ProductionReturnCompSubs
                    .FirstOrDefault(s => s.ReturnSubId == returnSubId);

                if (subItem == null)
                    return false;

                if (subItem.ReturnSubId > 0)
                    await RollbackTracksAddIssueQtyAsync(subItem.ReturnSubId);



                if (!PriductionGrns.Rejection && !PriductionGrns.Return)
                {
                    bool isSameItem = false;

                    if (subItem.RefPoSubId.HasValue)
                    {
                        isSameItem = await CheckIsSameAsOutItemByPoSubId(subItem.RefPoSubId.Value);
                    }

                    bool grnexit = false;

                    if (subItem.RefIssueSubId.HasValue)
                    {
                        grnexit = await CheckLabourGrnExist(subItem.RefIssueSubId.Value);
                    }


                    if ((PriductionGrns.IsManual || isSameItem || grnexit) && subItem.RefIssueSubId.GetValueOrDefault() > 0 && subItem.TransType == "In")
                    {
                        await AdjustProductionCompIssueItemBalanceAsync(
                           subItem.RefIssueSubId.Value,
                           subItem.Qty ?? 0,
                           0,
                           "Production Return Delete"
                       );
                    }
                }
                if (subItem.RefRcSubId.HasValue && subItem.RefRcSubId > 0 && subItem.TransType == "In")
                {

                    await AdjustRcBalanceAsync(subItem.RefRcSubId, subItem.Qty ?? 0, 0, "Production Return Deleted");
                    var issueIds = await GetIssueIdsByRcSubIdAsync(subItem.RefRcSubId ?? 0);

                    var issueSubs = await _unitOfWork.ProductionIssueCompSubs
                                       .GetQueryable()
                                       .Where(x => issueIds.Contains(x.IssueId.Value)
                                           && x.TransType == "In")
                                       .OrderBy(x => x.IssueId)
                                       .ToListAsync();

                    if (issueSubs.Any())
                    {
                        foreach (var GrnsubItem in issueSubs)
                        {
                            await AdjustProductionCompIssueItemBalanceAsync(GrnsubItem.IssueSubId, subItem.Qty ?? 0, 0, "Production Return deleted");
                        }

                    }

                }

                // 🔹 Stock Delete Entry
                if (subItem.ItemId.HasValue && subItem.TransType == "In")
                {
                    await DeleteStockAddAsync(
                        subItem.ReturnSubId,
                        subItem.ItemId.Value,
                        screenCode,
                        PriductionGrns.ReturnNo
                    );
                }
                if (IsPoWise)
                {
                    if (subItem.RefPoSubId.GetValueOrDefault() > 0 && subItem.TransType == "In")
                    {
                        await AdjustMfgPoSubItemBalanceAsync(subItem.RefPoSubId.Value, subItem.ItemId.Value, subItem.Qty ?? 0, 0, "Production GRN Delete");
                    }

                }
                await _unitOfWork.ProductionReturnCompSubs.DeleteAsync(subItem);
                await _unitOfWork.SaveAsync();

                var remaining = await _unitOfWork.ProductionReturnCompSubs
                  .GetQueryable()
                  .Where(x => x.ReturnId == PriductionGrns.ReturnId)
                  .OrderBy(x => x.SlNo)
                  .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    }
}
