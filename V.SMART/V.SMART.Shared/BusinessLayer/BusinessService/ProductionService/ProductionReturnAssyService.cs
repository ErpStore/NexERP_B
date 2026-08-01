using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop.Implementation;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ProductionService
{
    public class ProductionReturnAssyService : IProductionReturnAssyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly IProductionSCNAssyservice _productionSCNAssyService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public ProductionReturnAssyService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            IStockManagerService stockManagerService,
            IProductionSCNAssyservice productionSCNAssyService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _productionSCNAssyService = productionSCNAssyService;
            _stockManagerService = stockManagerService;
            _logs = logs;
            _mapper = mapper;
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



        // 🔹 Production Return operations


        public async Task<int> GetPendingJobOrdersCountAsync()
        {
            return await _unitOfWork.JobOrders
                .GetQueryable()
                .Where(j => j.JobBalQty > 0)
                .CountAsync();
        }


        public async Task<List<ProductionReturnAssySubVM>> GetDistinctRefJobNoByReturnIdAsync(int returnId)
        {
            try
            {
                return await _unitOfWork.ProductionReturnAssySubs
                            .GetQueryable()
                            .Where(s => s.ReturnId == returnId)
                            .GroupBy(s => new
                            {
                                s.JobOrder.JobNo,
                                s.JobOrder.Suffix,
                                s.JobOrder.JobDate,
                                DeptCode = s.JobOrder.Staff.DepartmentCode
                            })
                            .Select(g => new ProductionReturnAssySubVM
                            {
                                RefJobOrderNo = !string.IsNullOrEmpty(g.Key.DeptCode)
                                    ? g.Key.DeptCode + "/" + g.Key.JobNo + (g.Key.Suffix ?? "")
                                    : g.Key.JobNo + (g.Key.Suffix ?? ""),

                                RefJobOrderDate = g.Key.JobDate
                            })
                            .ToListAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetDistinctRefJobNoByReturnIdAsync for ReturnId: {returnId}");
                throw new Exception("Error checking Production Return Assy GetDistinctRefJobNoByReturnIdAsync eligibility", ex);
            }
            //return await _unitOfWork.ProductionReturnAssySubs
            //    .GetQueryable()
            //    .Where(s => s.ReturnId == returnId)
            //    .GroupBy(s => new { s.JobOrder.JobNo,s.JobOrder.Suffix , s.JobOrder.JobDate })
            //    .Select(g => new ProductionReturnAssySubVM
            //    {
            //        RefJobOrderNo = g.Key.JobNo + g.Key.Suffix,
            //        RefJobOrderDate = g.Key.JobDate
            //    })
            //    .ToListAsync();
        }

        public async Task<(List<ProductionReturnAssyVM> returnAssyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ProductionReturnAssys.GetQueryable()
                .Include(j => j.ProductionReturnAssySubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.ProductionReturnAssySubs)
                    .ThenInclude(s => s.JobOrder)
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
            var vmList = _mapper.Map<List<ProductionReturnAssyVM>>(list);

            return (vmList, total);
        }

        public static class MaterialReturnFilterBuilder
        {
            public static IQueryable<ProductionReturnAssy> ApplyFilter(
                IQueryable<ProductionReturnAssy> query, string field, object value)
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
                                    x.ProductionReturnAssySubs.Any(s =>
                                   (
                                       // Full format
                                       (s.JobOrder.Staff != null &&
                                           s.JobOrder.Staff.DepartmentCode != null &&
                                           (s.JobOrder.Staff.DepartmentCode + "/" +
                                           s.JobOrder.JobNo + (s.JobOrder.Suffix ?? ""))
                                           .StartsWith(input))
                                   )
                                   ||
                                   (
                                       // Without dept
                                       (s.JobOrder.JobNo + (s.JobOrder.Suffix ?? ""))
                                           .StartsWith(input)
                                   )
                                   ||
                                   (
                                       // Only suffix
                                       s.JobOrder.Suffix != null &&
                                       s.JobOrder.Suffix.Contains(input)
                                   )
                                   ||
                                    (
                                        // ✅ Only JobNo
                                        s.JobOrder.JobNo.StartsWith(input)
                                    )
                                    ||
                                    (
                                        // ✅ Only Suffix
                                        (s.JobOrder.Suffix != null && s.JobOrder.Suffix.Contains(input))
                                    )
                                    ||
                                    (
                                        // ✅ Only DepartmentCode
                                        (s.JobOrder.Staff != null &&
                                            s.JobOrder.Staff.DepartmentCode != null &&
                                            s.JobOrder.Staff.DepartmentCode.StartsWith(input))
                                    )
                                  )
                                );

                            //return query.Where(x =>
                            //    x.ProductionReturnAssySubs.Any(s =>
                            //        (string.IsNullOrEmpty(part1) || s.JobOrder.JobNo.StartsWith(part1)) &&
                            //        (string.IsNullOrEmpty(part2) || s.JobOrder.Suffix.Contains(part2))
                            //    )
                            //);
                        }


                    case "ItemCode":
                        return query.Where(x => x.ProductionReturnAssySubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.ProductionReturnAssySubs
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

            private static IQueryable<ProductionReturnAssy> ApplyStatusFilter(
                IQueryable<ProductionReturnAssy> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.ReturnTally == true),
                    "Pending" => query.Where(x => x.ReturnTally == false),
                    _ => query
                };
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteProductionReturnAssyAsync(int returnId, int screenCode)
        {
            try
            {
                var returnAssy = await _unitOfWork.ProductionReturnAssys
                    .GetQueryable()
                    .FirstOrDefaultAsync(r => r.ReturnId == returnId);

                if (returnAssy == null)
                    return (false, "Production Return Assy not found.");

                // Load all sub-ids once for both flow types
                var returnSubIds = await _unitOfWork.ProductionReturnAssySubs
                    .GetQueryable()
                    .Where(s => s.ReturnId == returnId)
                    .Select(s => s.ReturnSubId)
                    .ToListAsync();

                // ---------- CASE 1: Normal Return (Not Rejection & Not Return Type) ----------
                if (!returnAssy.Rejection && !returnAssy.Return)
                {
                    bool hasSCNTrans = await _unitOfWork.ProductionSCNAssySubs
                        .GetQueryable()
                        .AnyAsync(qs => qs.RefReturnSubId.HasValue
                                     && returnSubIds.Contains(qs.RefReturnSubId.Value));

                    if (hasSCNTrans)
                        return (false, "Cannot delete this Production Return as Production SCN transaction exists.");

                    // Check stock usage
                    bool usedStock = await _unitOfWork.StockAdds
                        .GetQueryable()
                        .AnyAsync(sa =>
                            returnSubIds.Contains(sa.SubItemRefID) &&
                            sa.ScreenCode == screenCode &&
                            sa.BalQty < sa.AddQty);

                    if (usedStock)
                        return (false, "Cannot delete this Production Return. Some items have already been transacted in another screen.");
                }
                else
                {
                    // ---------- CASE 2: Rejection / Return Flow ----------
                    var returnSubs = await _unitOfWork.ProductionReturnAssySubs
                        .GetQueryable()
                        .Where(s => s.ReturnId == returnId)
                        .ToListAsync();

                    foreach (var sub in returnSubs)
                    {
                        var result = await ValidateDeleteAsync(
                            sub.RefJobOrderId ?? 0,
                            sub.ItemId ?? 0,
                            sub.QtyReturned.GetValueOrDefault(),
                            returnAssy.AddStoreId ?? 0
                        );

                        if (!result.IsValid)
                            return (false, result.Message);
                    }

                    bool usedStock = await _unitOfWork.StockAdds
                        .GetQueryable()
                        .AnyAsync(sa =>
                            returnSubIds.Contains(sa.SubItemRefID) &&
                            sa.ScreenCode == screenCode &&
                            sa.BalQty < sa.AddQty);

                    if (usedStock)
                        return (false, "Cannot delete this Production Return. Some items have already been transacted in another screen.");
                }

                return (true, "Production Return Assy can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteProductionReturnAssyAsync for ReturnId: {returnId}");
                throw new Exception("Error checking Production Return Assy delete eligibility", ex);
            }
        }


        public async Task<(bool IsValid, string Message)> ValidateDeleteAsync(int jobId,int itemId,decimal qtyReturned,int addStoreId)
        {
            var jobOrderSub = await _unitOfWork.JobOrderSubs
                .GetQueryable()
                .Where(j => j.JobId == jobId && j.ItemId == itemId)
                .FirstOrDefaultAsync();

            if (jobOrderSub != null)
            {
                if (jobOrderSub.BalQty < qtyReturned)
                {
                    return (false,$"Cannot delete. Job Order balance ({jobOrderSub.BalQty}) is less than returned qty ({qtyReturned}).");
                }
            }

            return (true, "OK");
        }

        public async Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(
                        int screenCode,
                        int storeId,
                        int itemId,
                        int subItemRefId)
        {
            try
            {
                return await _stockManagerService.GetQtyBalQtyByStockAddAsync(
                    screenCode,
                    storeId,
                    itemId,
                    subItemRefId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching stock details for Screen Code: {screenCode}, SubItemRefId: {subItemRefId}");
                throw new InvalidOperationException("Failed to retrieve SCNGen sub-item stock details.");
            }
        }


        public async Task<List<Dictionary<string, object>>> GetAllOpenJobOrdersAsync()
        {
            try
            {
                var jobs = await _unitOfWork.JobOrders.GetQueryable()
                   
                    .Include(j => j.AssyItem)
                    .Include(j => j.JobOrderSubs).ThenInclude(s => s.CostCenter)
                    .Include(j => j.Staff)
                    .Where(j => !j.Cancel && j.JobBalQty > 0)
                    .ToListAsync();

                var jobIds = jobs.Select(j => j.JobId).ToList();

                var jobSubs = await _unitOfWork.JobOrderSubs.GetQueryable()
                    .Include(s => s.CostCenter)
                    .Where(s => jobIds.Contains(s.JobId) && !s.ItemCancel)
                    .ToListAsync();

                var issuedData = await _unitOfWork.ProductionIssueAssySubs.GetQueryable()
                    .Where(t => jobIds.Contains(t.RefJobId))
                    .GroupBy(t => new { t.RefJobId, t.ItemId })
                    .Select(g => new {
                        JobId = g.Key.RefJobId,
                        ItemId = g.Key.ItemId,
                        TotalIssuedQty = g.Sum(x => x.BalQty)
                    })
                    .ToListAsync();

                var result = new List<Dictionary<string, object>>();

                foreach (var job in jobs)
                {
                    var subs = jobSubs.Where(s => s.JobId == job.JobId).ToList();
                    decimal? minPossible = null;

                    // First active sub (avoid null CostCenter crash)
                    var firstSub = subs.FirstOrDefault();

                    int? costCenterId = firstSub?.CostCenter?.Id;
                    string costCenterName = firstSub?.CostCenter?.ProjectNo ?? "";

                    foreach (var sub in subs)
                    {
                        var issued = issuedData.FirstOrDefault(i =>
                            i.JobId == job.JobId && i.ItemId == sub.ItemId);

                        decimal totalIssued = issued?.TotalIssuedQty ?? 0;

                        if (sub.UtilQty > 0)
                        {
                            var possible = totalIssued / sub.UtilQty;

                            if (minPossible == null || possible < minPossible)
                                minPossible = possible;
                        }
                    }

                    result.Add(new Dictionary<string, object>
                    {
                        ["Selected"] = false,
                        ["RefJobId"] = job.JobId,
                        //["RefJobNo"] = $"{job.JobNo}{job.Suffix}",
                        ["RefJobNo"] = !string.IsNullOrWhiteSpace(job.Staff?.DepartmentCode)
                                         ? $"{job.Staff?.DepartmentCode}/{job.JobNo ?? ""}{job.Suffix ?? ""}"
                                         : $"{job.JobNo ?? ""}{job.Suffix ?? ""}",
                        ["RefJobDate"] = job.JobDate,

                        ["Assy/SubAssyId"] = job.AssyId,
                        ["Assy/SubAssyCode"] = job.AssyItem?.ItemCode,
                        ["ItemName"] = job.AssyItem?.ItemName,
                        ["MeasureUnit"] = job.AssyItem?.MeasureUnit,

                        ["JobOrderQty"] = job.JobBalQty,
                        ["PossibleQty"] = Math.Round(minPossible ?? 0m, 3),

                        ["CostCenterId"] = costCenterId,
                        ["CostCenter"] = costCenterName
                    });
                }


                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching open JobOrder");
                throw new InvalidOperationException("Failed to retrieve open Job Orders.");
            }
        }


        public async Task<List<Dictionary<string, object>>> GetAllProductionIssuedItemsAsync()
        {
            try
            {
                var result = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Where(i => i.BalQty > 0)
                    .Select(j => new
                    {
                        j.RefJobId,
                        j.IssueSubId,
                        j.JobOrder.JobNo,
                        j.JobOrder.Suffix,
                        j.JobOrder.JobDate,
                        j.JobOrder.Staff,
                        j.ItemId,
                        j.Item.ItemCode,
                        j.Item.ItemName,
                        j.Item.MeasureUnit,
                        j.BalQty,
                        j.CostId,
                        j.CostCenter.ProjectNo
                    })
                    .ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefJobId"] = r.RefJobId,
                    ["RefIssueSubId"] = r.IssueSubId,
                    //["RefJobNo"] = $"{r.JobNo}{r.Suffix}",
                    ["RefJobNo"] = !string.IsNullOrWhiteSpace(r.Staff?.DepartmentCode)
                                   ? $"{r.Staff?.DepartmentCode}/{r.JobNo ?? ""}{r.Suffix ?? ""}"
                                   : $"{r.JobNo ?? ""}{r.Suffix ?? ""}",
                    ["RefJobDate"] = r.JobDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,
                    ["IssueBalQty"] = r.BalQty,
                    ["CostCenterId"] = r.CostId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching Issue items");
                throw new InvalidOperationException("Failed to retrieve open Production issue Items. Please try again.");
            }
        }


        public async Task<bool> DeleteProdAssyReturnByReturnIdAsync(int returnId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var productionReturnAssy = await _unitOfWork.ProductionReturnAssys
                    .GetQueryable()
                    .Include(e => e.ProductionReturnAssySubs)
                    .FirstOrDefaultAsync(e => e.ReturnId == returnId);

                if (productionReturnAssy == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in productionReturnAssy.ProductionReturnAssySubs)
                {
                    if (!productionReturnAssy.Return && !productionReturnAssy.Rejection)
                    {
                        if (sub.ReturnSubId > 0)
                        {
                            await RollbackTracksAddIssueQtyAsync(sub.ReturnSubId);
                        }

                        await AdjustJobOrderBalanceAsync(sub.RefJobOrderId,sub.QtyReturned ?? 0,0,"Production Return Assy Delete");
                    }
                    else
                    {
                        await AdjustProductionAssyIssueItemBalanceAsync(sub.RefIssueSubId,sub.QtyReturned ?? 0,0,"Production Return Assy Delete");

                        await AdjustJobOrderSubItemBalanceAsync(sub.RefJobOrderId,sub.ItemId.Value,sub.QtyReturned ?? 0,0,"Production Return Assy Delete");
                    }
                    await DeleteStockAddAsync(sub.ReturnSubId,sub.ItemId.Value,screenCode,productionReturnAssy.ReturnNo);
                }


                var ProductionReturnAssy = await _unitOfWork.ProductionReturnAssys.GetAsync(returnId);
                await _unitOfWork.ProductionReturnAssys.DeleteAsync(ProductionReturnAssy);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Production Return Assembly",
                    action: $"Deleted Return no: {productionReturnAssy.ReturnNo}",
                    additionalInfo: $"Return Id: {productionReturnAssy.ReturnId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Production Return Assy: {returnId}");
                throw;
            }
        }


        public async Task<ProductionReturnAssyVM?> GetProductionByReturnIdAsync(int returnId)
        {
            try
            {
                var entity = await _unitOfWork.ProductionReturnAssys.GetQueryable()
                    .Include(q => q.AddStore)
                    .Include(q => q.ProductionReturnAssySubs)
                    .Include(q => q.ProductionReturnAssySubs)
                    .ThenInclude(s => s.Item)
                    .Include(q => q.ProductionReturnAssySubs)
                    .ThenInclude(s => s.CostCenter)
                    .Include(q => q.ProductionReturnAssySubs)
                    .ThenInclude(s => s.JobOrder)
                     .ThenInclude(s => s.Staff)
                    .FirstOrDefaultAsync(q => q.ReturnId == returnId);

                if (entity == null)
                    return null;

                var vm = _mapper.Map<ProductionReturnAssyVM>(entity);
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
                var lastProdIssue = await _unitOfWork.ProductionReturnAssys
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q =>Convert.ToInt32(q.ReturnNo))
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

        public async Task DeleteAndResequenceAsync(ProductionReturnAssySubVM subitem, ProductionReturnAssyVM productionReturnAssyVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.ReturnSubId > 0)
                {
                    var entity = await _unitOfWork.ProductionReturnAssySubs.GetAsync(subitem.ReturnSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");



                    if (!productionReturnAssyVM.Rejection && !productionReturnAssyVM.Return)
                    {
                        if (subitem.ReturnSubId > 0)
                            await RollbackTracksAddIssueQtyAsync(subitem.ReturnSubId);

                        await AdjustJobOrderBalanceAsync(subitem.RefJobOrderId, subitem.QtyReturned ?? 0, 0, "Production return Assy Delete");
                    }
                    else
                    {
                        await AdjustProductionAssyIssueItemBalanceAsync(subitem.RefIssueSubId, subitem.QtyReturned ?? 0, 0, "Production Return Assy Delete");

                        await AdjustJobOrderSubItemBalanceAsync(subitem.RefJobOrderId, subitem.ItemId.Value, subitem.QtyReturned ?? 0, 0, "Production Return Assy Delete");
                    }
                    await DeleteStockAddAsync(subitem.ReturnSubId, subitem.ItemId.Value, screenCode, productionReturnAssyVM.ReturnNo);


                    await _unitOfWork.ProductionReturnAssySubs.DeleteAsync(entity.ReturnSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Production Return Assembly",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Return No: {productionReturnAssyVM?.ReturnNo}"
                    );
                }
                else
                {
                    productionReturnAssyVM.ProductionReturnAssySubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.ProductionReturnAssySubs
                    .GetQueryable()
                    .Where(x => x.ReturnId == productionReturnAssyVM.ReturnId)
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

        public async Task<List<ProductionReturnAssySubVM>> GetProductionReturnSubByReturnIdAsync(int returnId)
        {
            try
            {
                var subs = await _unitOfWork.ProductionReturnAssySubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.ReturnId == returnId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ProductionReturnAssySubVM>>(subs);
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
        public async Task<ProductionReturnAssySubVM?> GetProductionReturnSubItemDetailByReturnSubIdAsync(int returnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionReturnAssySubs
                    .GetQueryable()
                    .Where(q => q.ReturnSubId == returnSubId)
                    .Select(q => new ProductionReturnAssySubVM
                    {
                        QtyReturned = q.QtyReturned,
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

        public async Task<ProductionReturnAssyVM> UpsertProductionReturnAsync(ProductionReturnAssyVM prodReturnAssyVM, int screenCode)
        {
            if (prodReturnAssyVM == null)
                throw new ArgumentNullException(nameof(prodReturnAssyVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ProductionReturnAssy entity;

                if (prodReturnAssyVM.ReturnId == 0)
                {
                    entity = _mapper.Map<ProductionReturnAssy>(prodReturnAssyVM);

                    var nextNumber = await _unitOfWork.ProductionReturnAssys.GetLastReturnNoAsync(entity.Suffix);
                    entity.ReturnNo = nextNumber;
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.ProductionReturnAssySubs = prodReturnAssyVM.ProductionReturnAssySubVMs
                        .Select(s => _mapper.Map<ProductionReturnAssySub>(s))
                        .ToList();

                    await _unitOfWork.ProductionReturnAssys.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.ProductionReturnAssySubs)
                    {

                        if (!entity.Rejection && !entity.Return)
                        {
                            await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.ReturnSubId, sub.RefJobOrderId.Value, sub.ItemId.Value, sub.QtyReturned.GetValueOrDefault(), screenCode, currentUser, now);

                            await AdjustJobOrderBalanceAsync(sub.RefJobOrderId, 0, sub.QtyReturned ?? 0, "Production return Assy Create");
                        }
                        else
                        {
                            await AdjustProductionAssyIssueItemBalanceAsync(sub.RefIssueSubId, 0 , sub.QtyReturned ?? 0, "Production return Assy Create");

                            await AdjustJobOrderSubItemBalanceAsync(sub.RefJobOrderId, sub.ItemId.Value,  0 , sub.QtyReturned ?? 0, "Production return Assy Create");
                        }

                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, entity.AddStoreId.Value, sub.QtyReturned.GetValueOrDefault(),
                                sub.UnitPrice, null, screenCode, sub.ReturnSubId, entity.ReturnNo, entity.ReturnDate, null);
                    }

                    if(entity.IsNoInspection)
                    {
                        await CreateProuctionSCNFromProductionGRNAssyAsync(entity);
                    }
                    changes.AppendLine("Production Return Assembly Created Successfully.");
                }
                else
                {
                    // ---------------- UPDATE ----------------
                    entity = await _unitOfWork.ProductionReturnAssys.GetQueryable()
                        .Include(q => q.ProductionReturnAssySubs)
                        .Include(q => q.ProductionReturnAssyTracks)
                        .FirstOrDefaultAsync(q => q.ReturnId == prodReturnAssyVM.ReturnId)
                        ?? throw new InvalidOperationException("Production Return Assembly not found.");

                    var parentChanges = GetPropertyChanges(entity, prodReturnAssyVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(prodReturnAssyVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, prodReturnAssyVM.ProductionReturnAssySubVMs, screenCode, currentUser,now,changes);

                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Production Return Assembly Updated.");
                }

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await LogChangesAsync(changes, prodReturnAssyVM.ReturnId == 0 ? "Production Return Assembly Created" : "Production Return Assembly Updated");

                var savedEntity = await _unitOfWork.ProductionReturnAssys.GetQueryable()
                    .Include(q => q.ProductionReturnAssySubs).ThenInclude(s => s.Item)
                    .Include(q => q.AddStore)
                    .Include(q => q.ProductionReturnAssySubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.ProductionReturnAssyTracks)
                    .FirstOrDefaultAsync(q => q.ReturnId == entity.ReturnId);

                return _mapper.Map<ProductionReturnAssyVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Production Return Assembly: {prodReturnAssyVM.ReturnNo}");
                throw new InvalidOperationException("Failed to save Production Return Assembly. Please try again.");
            }
        }



        // -------------------------------------------------------------------
        // Child update: NO delete of existing child rows. Add new or update existing.
        // -------------------------------------------------------------------
        private async Task HandleChildUpdatesAsync(ProductionReturnAssy existingProdReturnAssy, List<ProductionReturnAssySubVM> incomingSubVMs,int screenCode, string currentUser,DateTime now, StringBuilder changes)
        {
            var existingSubIds = existingProdReturnAssy.ProductionReturnAssySubs.Select(s => s.ReturnSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.ReturnSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingProdReturnAssy.ProductionReturnAssySubs.Where(s => !incomingSubIds.Contains(s.ReturnSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - ReturnSubId: {sub.ReturnSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ProductionReturnAssySubs.DeleteAsync(sub.ReturnSubId);
                await _unitOfWork.SaveAsync();

                if (sub.ReturnSubId > 0)
                    await RollbackTracksAddIssueQtyAsync(sub.ReturnSubId);

                if (!existingProdReturnAssy.Rejection && !existingProdReturnAssy.Return)
                {
                    await AdjustJobOrderBalanceAsync(sub.RefJobOrderId, sub.QtyReturned ?? 0,0, "Production return Assy Delete");
                }
                else
                {
                    await AdjustProductionAssyIssueItemBalanceAsync(sub.RefIssueSubId, sub.QtyReturned ?? 0, 0, "Production return Assy Delete");

                    await AdjustJobOrderSubItemBalanceAsync(sub.RefJobOrderId, sub.ItemId.Value, sub.QtyReturned ?? 0, 0,"Production return Assy Delete");
                }

                await DeleteStockAddAsync(sub.ReturnSubId, sub.ItemId.Value, screenCode, existingProdReturnAssy.ReturnNo);

            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.ReturnSubId == 0)
                {
                    var newSub = _mapper.Map<ProductionReturnAssySub>(subVM);
                    newSub.ReturnId = existingProdReturnAssy.ReturnId;
                    await _unitOfWork.ProductionReturnAssySubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();



                    if (!existingProdReturnAssy.Rejection && !existingProdReturnAssy.Return)
                    {
                        await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(newSub.ReturnSubId, newSub.RefJobOrderId.Value, newSub.ItemId.Value,
                                                                newSub.QtyReturned.GetValueOrDefault(), screenCode, currentUser, now);

                    }
                    else
                    {
                        await AdjustProductionAssyIssueItemBalanceAsync(newSub.RefIssueSubId, 0, newSub.QtyReturned ?? 0, "Production return Assy Create");

                        await AdjustJobOrderSubItemBalanceAsync(newSub.RefJobOrderId, newSub.ItemId.Value,  0, newSub.QtyReturned ?? 0, "Production return Assy Create");
                    }
                    await _stockManagerService.AddOrUpdateStockAsync(newSub.ItemId.Value, existingProdReturnAssy.AddStoreId.Value, newSub.QtyReturned.GetValueOrDefault(),
                        newSub.UnitPrice , null, screenCode, newSub.ReturnSubId, existingProdReturnAssy.ReturnNo, existingProdReturnAssy.ReturnDate, null);

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.QtyReturned}");
                }
                else
                {
                    var existingSub = existingProdReturnAssy.ProductionReturnAssySubs.FirstOrDefault(s => s.ReturnSubId == subVM.ReturnSubId);
                    if (existingSub != null)
                    {
                        if (!existingProdReturnAssy.Rejection && !existingProdReturnAssy.Return)
                        {
                            if (existingSub.ReturnSubId > 0)
                                await RollbackTracksAddIssueQtyAsync(existingSub.ReturnSubId);

                            await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(existingSub.ReturnSubId, subVM.RefJobOrderId.Value, subVM.ItemId.Value,
                                                                                            subVM.QtyReturned.GetValueOrDefault(), screenCode, currentUser, now);
                            if (subVM.RefJobOrderId > 0)
                                await AdjustJobOrderBalanceAsync(subVM.RefJobOrderId, existingSub.QtyReturned.GetValueOrDefault(), subVM.QtyReturned ?? 0, "Production return Assy  Update");

                        }
                        else
                        {

                            await AdjustProductionAssyIssueItemBalanceAsync(subVM.RefIssueSubId, existingSub.QtyReturned.GetValueOrDefault(), subVM.QtyReturned ?? 0, "Production return Assy Update");

                            await AdjustJobOrderSubItemBalanceAsync(subVM.RefJobOrderId, subVM.ItemId.Value, existingSub.QtyReturned.GetValueOrDefault(), subVM.QtyReturned ?? 0, "Production return Assy Update");
                        }

                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingProdReturnAssy.AddStoreId.Value, subVM.QtyReturned.GetValueOrDefault(),
                            subVM.UnitPrice, null, screenCode, subVM.ReturnSubId, existingProdReturnAssy.ReturnNo, existingProdReturnAssy.ReturnDate, null);

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        private async Task DeleteStockAddAsync(int returnSubId, int itemId, int screenCode, string refNo)
        {
            var addIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == returnSubId && s.ItemId == itemId && s.ScreenCode == screenCode && s.RefNo == refNo)
                .Select(s => s.AddId)
                .ToListAsync();

            foreach(var addId in addIds)
            {
                if (addId > 0)
                    await _stockManagerService.DeleteStockAddAsync(addId);
            }
        }


        private async Task RollbackTracksAddIssueQtyAsync(int refReturnSubId)
        {

            var tracks = await _unitOfWork.ProductionReturnAssyTracks.GetQueryable()
                                .Where(p => p.RefReturnSubId == refReturnSubId).ToListAsync();

            foreach(var tr in tracks)
            {
                var productionIssueSub = await _unitOfWork.ProductionIssueAssySubs.GetQueryable()
                    .Where(p => p.IssueSubId == tr.RefIssueSubId).ToListAsync();

                foreach (var issue  in productionIssueSub)
                {
                    issue.BalQty += tr.IssueQty.GetValueOrDefault();

                    await _unitOfWork.ProductionIssueAssySubs.UpdateAsync(issue);
                    await _unitOfWork.SaveAsync();


                    //Update IssueTally
                    var totalBalQty = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Where(e => e.IssueId == issue.IssueId)
                    .SumAsync(e => e.BalQty);

                    var issueAssy = await _unitOfWork.ProductionIssueAssys.GetAsync(issue.IssueId);
                    if (issueAssy != null)
                    {
                        issueAssy.IssueTally = (totalBalQty == 0);
                        await _unitOfWork.ProductionIssueAssys.UpdateAsync(issueAssy);
                        await _unitOfWork.SaveAsync();
                    }
                }

                await _unitOfWork.ProductionReturnAssyTracks.DeleteAsync(tr);
                await _unitOfWork.SaveAsync();
            }
        }


        private async Task CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(int ReturnSubId, int refJobId, int itemId, decimal QtyReturned, int screenCode, string currentUser, DateTime now)
        {
            var jobSubs = await _unitOfWork.JobOrderSubs.GetQueryable()
                .Where(x => x.JobId == refJobId)
                .ToListAsync();

            foreach (var comp in jobSubs)
            {
                decimal utilQty = comp.UtilQty;
                if (utilQty <= 0)
                    continue;

                decimal componentNeeded = QtyReturned * utilQty;

                var issueTracks = await _unitOfWork.ProductionIssueAssySubs.GetQueryable()
                    .Where(t => t.RefJobId == refJobId && t.ItemId == comp.ItemId)
                    .OrderBy(t => t.IssueSubId)
                    .ToListAsync();

                foreach (var issue in issueTracks)
                {
                    if (componentNeeded <= 0)
                        break;

                    decimal takeQty = Math.Min(issue.BalQty, componentNeeded);
                    if (takeQty <= 0)
                        continue;

                    var track = new ProductionReturnAssyTrack
                    {
                        RefReturnSubId = ReturnSubId,
                        RefIssueSubId = issue.IssueSubId,

                        IssueItemId = issue.ItemId,
                        IssueQty = takeQty,

                        RefJobOrderId = refJobId,
                        ReturnItemId = itemId,
                        ReturnQty = QtyReturned,

                        CreatedBy = currentUser,
                        CreatedDate = now
                    };

                    await _unitOfWork.ProductionReturnAssyTracks.CreateAsync(track);
                    await _unitOfWork.SaveAsync();

                    issue.BalQty -= takeQty;
                    if (issue.BalQty < 0) issue.BalQty = 0;

                    await _unitOfWork.ProductionIssueAssySubs.UpdateAsync(issue);
                    await _unitOfWork.SaveAsync();

                    componentNeeded -= takeQty;

                    //Update IssueTally

                    var totalBalQty = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Where(e => e.IssueId == issue.IssueId)
                    .SumAsync(e => e.BalQty);

                    var issueAssy = await _unitOfWork.ProductionIssueAssys.GetAsync(issue.IssueId);
                    if (issueAssy != null)
                    {
                        issueAssy.IssueTally = (totalBalQty == 0);
                        await _unitOfWork.ProductionIssueAssys.UpdateAsync(issueAssy);
                        await _unitOfWork.SaveAsync();
                    }

                }
            }
        }


        private async Task AdjustJobOrderSubItemBalanceAsync(int? refJobSubId, int itemId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refJobSubId.HasValue || refJobSubId == 0) return;

                var jobOrderSub = await _unitOfWork.JobOrderSubs.GetQueryable()
                    .Where(j => j.JobId == refJobSubId && j.ItemId == itemId).FirstOrDefaultAsync();

                if (jobOrderSub == null) return;

                if (oldQty > 0)
                    jobOrderSub.BalQty -= oldQty;

                if (newQty > jobOrderSub.ReqQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

                if (newQty > 0)
                    jobOrderSub.BalQty += newQty;

                await _unitOfWork.JobOrderSubs.UpdateAsync(jobOrderSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.JobOrderSubs
                                                        .GetQueryable()
                                                        .Where(e => e.JobId == jobOrderSub.JobId)
                                                        .SumAsync(e => e.BalQty);

                var jobOrder = await _unitOfWork.JobOrders.GetAsync(jobOrderSub.JobId);
                if (jobOrder != null)
                {
                    jobOrder.JobTally = (totalBalQty == 0);
                    await _unitOfWork.JobOrders.UpdateAsync(jobOrder);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustJobOrderSubItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustJobOrderSubItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust JobOrder Sub Balance. Please contact support.");
            }
        }

        private async Task AdjustProductionAssyIssueItemBalanceAsync(int? refIssueSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refIssueSubId.HasValue || refIssueSubId == 0) return;

                var assyIssueSub = await _unitOfWork.ProductionIssueAssySubs.GetQueryable()
                                    .Where(p => p.IssueSubId == refIssueSubId).FirstOrDefaultAsync();

                if (assyIssueSub == null) return;

                if (oldQty > 0)
                    assyIssueSub.BalQty += oldQty;

                if (newQty > assyIssueSub.IssueQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

                if (newQty > 0)
                    assyIssueSub.BalQty -= newQty;

                await _unitOfWork.ProductionIssueAssySubs.UpdateAsync(assyIssueSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.ProductionIssueAssySubs
                                        .GetQueryable()
                                        .Where(e => e.IssueId == assyIssueSub.IssueId)
                                        .SumAsync(e => e.BalQty);

                var AssyIssue = await _unitOfWork.ProductionIssueAssys.GetAsync(assyIssueSub.IssueId);
                if (AssyIssue != null)
                {
                    AssyIssue.IssueTally = (totalBalQty == 0);
                    await _unitOfWork.ProductionIssueAssys.UpdateAsync(AssyIssue);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustProductionAssyIssueItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustProductionAssyIssueItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Production Issue Sub Balance. Please contact support.");
            }
        }


        private async Task AdjustJobOrderBalanceAsync(int? rejJobId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!rejJobId.HasValue || rejJobId == 0) return;

                var jobOrder = await _unitOfWork.JobOrders.GetAsync(rejJobId.Value);
                if (jobOrder == null) return;

                if (oldQty > 0)
                    jobOrder.JobBalQty += oldQty;

                if (newQty > jobOrder.JobBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed JobOrder BalQty.");

                if (newQty > 0)
                    jobOrder.JobBalQty -= newQty;

                await _unitOfWork.JobOrders.UpdateAsync(jobOrder);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.JobOrderSubs
                          .GetQueryable()
                          .Where(e => e.JobId == rejJobId && !e.ItemCancel)
                          .SumAsync(e => e.BalQty);

                var jobOrders = await _unitOfWork.JobOrders
                    .GetAsync(rejJobId.GetValueOrDefault());

                if (jobOrders != null)
                {
                    jobOrders.JobTally =
                        totalBalQty <= 0 &&
                        (jobOrders.JobBalQty) <= 0;

                    await _unitOfWork.JobOrders.UpdateAsync(jobOrders);
                    await _unitOfWork.SaveAsync();
                }

            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustJobOrderBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustJobOrderBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust JobOrder Balance. Please contact support.");
            }
        }


        private async Task CreateProuctionSCNFromProductionGRNAssyAsync(ProductionReturnAssy grn)
        {
            var changes = new StringBuilder();

            try
            {
                var suffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);


                var MapStoreForProdSCN = await GetMappedStoreForFormAsync("SCNProductionAssyForm");
                var addStore = 1;

                if (MapStoreForProdSCN.StoreId > 0)
                {
                    addStore = MapStoreForProdSCN.StoreId;
                }
                

                var scn = new ProductionSCNAssy
                {
                    Suffix = suffix,
                    SCNNo = await _productionSCNAssyService.GetSCNNumberAsync(suffix),
                    SCNDate = DateTime.Now,
                    IssueStoreId = grn.AddStoreId,
                    AddStoreId = addStore,
                    CreatedBy = grn.CreatedBy,
                    CreatedDate = DateTime.Now,
                    MainRemark = "Auto-generated due to No Inspection",
                };

                scn.ProductionSCNAssySubs = grn.ProductionReturnAssySubs.Select(s => new ProductionSCNAssySub
                {
                    SlNo = s.SlNo,
                    SCNId = scn.SCNId,
                    ItemId = s.ItemId.Value,
                    RefReturnSubId = s.ReturnSubId,
                    BalQty = s.QtyReturned.GetValueOrDefault(),
                    AccQty = s.QtyReturned.GetValueOrDefault(),
                    UnitPrice = s.UnitPrice,
                    CostId = s.CostId,
                }).ToList();


                // Save SCN
                await _unitOfWork.ProductionSCNAssys.CreateAsync(scn);
                await _unitOfWork.SaveAsync();

                foreach (var sub in scn.ProductionSCNAssySubs)
                {
                    if (sub.RefReturnSubId > 0)
                    {
                        var ReturnAssySub = await _unitOfWork.ProductionReturnAssySubs.GetAsync(sub.RefReturnSubId.Value);

                        if (ReturnAssySub == null)
                            throw new InvalidOperationException("Invalid GRN Sub reference.");

                        ReturnAssySub.BalQty -= sub.AccQty;

                        await _unitOfWork.ProductionReturnAssySubs.UpdateAsync(ReturnAssySub);
                        await _unitOfWork.SaveAsync();

                        var totalBalQty = await _unitOfWork.ProductionReturnAssySubs
                            .GetQueryable()
                            .Where(x => x.ReturnId == ReturnAssySub.ReturnId)
                            .SumAsync(x => x.BalQty);

                        grn.ReturnTally = (totalBalQty == 0);
                        await _unitOfWork.ProductionReturnAssys.UpdateAsync(grn);
                        await _unitOfWork.SaveAsync();
                    }

                    var ScnScreenCode = await GetScreenCodeByScreenNameAsync("Production SCN Assembly");

                    await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, grn.AddStoreId.Value, sub.AccQty,
                        sub.UnitPrice, null, ScnScreenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, allowMultipleIssue: true);

                    await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, addStore, sub.AccQty,
                        sub.UnitPrice, null, ScnScreenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, sub.Remark, allowMultipleAdd: true);

                }

                await LogChangesAsync(changes, "Purchase SCN Created");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task HandleTrackUpdatesAsync(
            ProductionReturnAssy entity,
            List<int>? selectedJobOrderIds,
            StringBuilder changes, string currentUser)
        {
            //selectedJobOrderIds ??= new List<int>();

            //// Load existing DB tracks for this Issue
            //var existingTracks = entity.ProductionReturnAssyTracks?.ToList() ?? new List<ProductionReturnAssyTrack>();

            //// 1️⃣ Identify tracks to delete (those no longer selected)
            //var toDelete = existingTracks
            //    .Where(t => !selectedJobOrderIds.Contains(t.RefJobOrderId ?? 0))
            //    .ToList();

            //foreach (var del in toDelete)
            //{
            //    await _unitOfWork.ProductionIssueAssyTracks.DeleteAsync(del);
            //    changes.AppendLine($"Removed Track for JobOrderId {del.RefJobOrderId}");
            //}

            //// 2️⃣ Identify new tracks to add
            //var toAdd = selectedJobOrderIds
            //    .Where(jobId => !existingTracks.Any(t => t.RefJobOrderId == jobId))
            //    .Select(jobId => new ProductionIssueAssyTrack
            //    {
            //        RefIssueId = entity.IssueId,
            //        RefJobOrderId = jobId,
            //        CreatedBy = currentUser,
            //        CreatedDate = DateTime.Now
            //    })
            //    .ToList();

            //foreach (var add in toAdd)
            //{
            //    await _unitOfWork.ProductionIssueAssyTracks.CreateAsync(add);
            //    changes.AppendLine($"Added Track for JobOrderId {add.RefJobOrderId}");
            //}

            //// Update entity’s collection (keep it in sync)
            //entity.ProductionIssueAssyTracks = existingTracks
            //    .Where(t => !toDelete.Contains(t))
            //    .Concat(toAdd)
            //    .ToList();
        }

        public async Task<decimal> GetIssueBalQtyByIssueSubId(int issueSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueAssySubs.GetQueryable()
                        .Where(x => x.IssueSubId == issueSubId)
                        .SumAsync(x => x.BalQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading Production Issue Assy details for IssueSubId: {issueSubId}");
                throw new InvalidOperationException($"Failed to load Issue Assy details for IssueSubId: {issueSubId}", ex);
            }
        }


        public async Task<ProductionReturnAssyVM> GetJobOrderDetailsByIdAsync(int jobId)
        {
            try
            {
                // 1️⃣ Get Job Order with Assembly Item
                var job = await _unitOfWork.JobOrders.GetQueryable()
                    .Include(j => j.AssyItem)
                    .FirstOrDefaultAsync(j => j.JobId == jobId && j.JobBalQty > 0);

                if (job == null)
                    return null;

                // 2️⃣ Get sub-components used for this Job Order
                var jobSubs = await _unitOfWork.JobOrderSubs.GetQueryable()
                    .Include(s => s.CostCenter)
                    .Where(s => !s.ItemCancel && s.JobId == jobId)
                    .ToListAsync();

                if (!jobSubs.Any())
                    return null;

                var subItemIds = jobSubs.Select(s => s.ItemId).ToList();

                // 3️⃣ Get total issued quantity per component
                var issuedData = await _unitOfWork.ProductionIssueAssySubs.GetQueryable()
                    .Where(t => t.RefJobId == jobId && t.ItemId != null && subItemIds.Contains((int)t.ItemId))
                    .GroupBy(t => t.ItemId)
                    .Select(g => new
                    {
                        ItemId = g.Key,
                        TotalIssuedQty = g.Sum(x => x.BalQty)
                    })
                    .ToListAsync();

                //// 4️⃣ Get total assemblies already returned
                //var totalReturnedAssemblies = await _unitOfWork.ProductionReturnAssySubs.GetQueryable()
                //    .Where(r => r.RefJobOrderId == jobId && r.ItemId == job.AssyId)
                //    .SumAsync(r => (decimal?)r.QtyReturned) ?? 0;

                decimal? minAssembliesPossible = null;

                foreach (var sub in jobSubs)
                {
                    var issued = issuedData.FirstOrDefault(i => i.ItemId == sub.ItemId);
                    decimal totalIssuedQty = issued?.TotalIssuedQty ?? 0;
                    decimal utilQty = sub.UtilQty;

                    if (utilQty > 0)
                    {
                        var assembliesFromComp = totalIssuedQty == 0 ? 0 : totalIssuedQty / utilQty;

                        if (minAssembliesPossible == null || assembliesFromComp < minAssembliesPossible)
                            minAssembliesPossible = assembliesFromComp;
                    }
                }

                // 6️⃣ Remaining assemblies available for return
                decimal possibleAssemblies = minAssembliesPossible ?? 0;
                decimal remainingReturnableAssemblies = possibleAssemblies;

                if (remainingReturnableAssemblies <= 0)
                    return null;

                // 7️⃣ Get CostId and ProjectNo from the first JobOrderSub
                var firstSub = jobSubs.FirstOrDefault();
                var costId = firstSub?.CostId ?? null;
                var projectNo = firstSub?.CostCenter?.ProjectNo ?? string.Empty;

                // 8️⃣ Get ALL IssueIds and IssueSubIds linked with this JobOrder
                var issueTracks = await _unitOfWork.ProductionIssueAssySubs.GetQueryable()
                    .Where(t => t.RefJobId == jobId)
                    .Select(t => new
                    {
                        IssueId = t.IssueId,
                        IssueSubId = t.IssueSubId
                    })
                    .Distinct()
                    .ToListAsync();

                // Extract Issue IDs (only values > 0)
                var issueIds = issueTracks
                    .Where(t => t.IssueId > 0)
                    .Select(t => t.IssueId)
                    .Distinct()
                    .ToList();

                // Extract Issue Sub IDs (only values > 0)
                var issueSubIds = issueTracks
                    .Where(t => t.IssueSubId > 0)
                    .Select(t => t.IssueSubId)
                    .Distinct()
                    .ToList();


                // 9️⃣ Prepare result — only assembly item, not sub-components
                var assemblyItem = job.AssyItem;

                var result = new ProductionReturnAssyVM
                {
                    SelectedIssueIds = issueIds,
                    SelectedIssueSubIds = issueSubIds,
                    SelectedJobOrderIds = new List<int> { jobId },
                    ProductionReturnAssySubVMs = new List<ProductionReturnAssySubVM>
                    {
                        new ProductionReturnAssySubVM
                        {
                            SlNo = 1,
                            RefJobOrderNo = $"{job.JobNo}{job.Suffix}",
                            RefJobOrderDate = job.JobDate,
                            ItemId = assemblyItem?.ItemId ?? 0,
                            ItemCode = assemblyItem?.ItemCode,
                            ItemName = assemblyItem?.ItemName,
                            MeasureUnit = assemblyItem?.MeasureUnit,
                            RefJobOrderId = job.JobId,
                            CostId = costId,
                            ProjectNo = projectNo,
                            QtyReturned = 0,
                            PossibleQty = remainingReturnableAssemblies,
                            BalQty = job.JobBalQty,
                            Remark = string.Empty,
                            IsEditable = true
                        }
                    }
                };

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading Job Order details for JobId: {jobId}");
                throw new InvalidOperationException($"Failed to load Job Order details for ID: {jobId}", ex);
            }
        }


        public async Task<IEnumerable<MfgQuoteVM>> GetAllQuoteAsync()
        {
            try
            {
                var quotes = await _unitOfWork.MfgQuotes.GetQueryable()
                    .Include(q => q.Customer)
                    .AsNoTracking()
                    .ToListAsync();

                var quoteIds = quotes.Select(q => q.QuoteId).ToList();

                var subItems = await _unitOfWork.MfgQuoteSubs.GetQueryable()
                    .Where(s => quoteIds.Contains(s.QuoteId))
                    .AsNoTracking()
                    .ToListAsync();

                var quoteVMs = _mapper.Map<IEnumerable<MfgQuoteVM>>(quotes);

                foreach (var quoteVM in quoteVMs)
                {
                    quoteVM.MfgQuoteSubVM = subItems
                        .Where(s => s.QuoteId == quoteVM.QuoteId)
                        .Select(s => _mapper.Map<MfgQuoteSubVM>(s))
                        .ToList();
                }

                return quoteVMs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllQuoteAsync");
                return Enumerable.Empty<MfgQuoteVM>();
            }
        }

        public async Task<MfgQuote?> GetLastQuoteAsync(int custId)
        {
            try
            {
                return await _unitOfWork.MfgQuotes.GetLatestAsync(
                    q => q.CustId == custId,
                    q => q.QuoteId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetLastQuoteAsync for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve last quotation. Please try again.");
            }
        }

        //private async Task HandleChildUpdatesAsync(ProductionReturnAssy existingProdReturnAssy, List<ProductionReturnAssySubVM> incomingSubVMs, StringBuilder changes)
        //{
        //    var existingSubIds = existingProdReturnAssy.ProductionReturnAssySubs.Select(s => s.ReturnSubId).ToHashSet();
        //    var incomingSubIds = incomingSubVMs.Select(s => s.ReturnSubId).ToHashSet();

        //    // DELETE removed children
        //    foreach (var sub in existingProdReturnAssy.ProductionReturnAssySubs.Where(s => !incomingSubIds.Contains(s.ReturnSubId)).ToList())
        //    {
        //        changes.AppendLine($"Child Deleted - ReturnSubId: {sub.ReturnSubId}, Item: {sub.Item?.ItemCode}");
        //        await _unitOfWork.ProductionReturnAssySubs.DeleteAsync(sub.ReturnSubId);
        //        await _unitOfWork.SaveAsync();

        //        //if (sub.ReturnSubId > 0)
        //        //    await AdjustJobOrderBalanceAsync(sub.RefJobOrderSubId, sub.IssueQty, 0, "Production Issue Assembly Deletion");
        //    }

        //    foreach (var subVM in incomingSubVMs)
        //    {
        //        if (subVM.ReturnSubId == 0)
        //        {
        //            var newSub = _mapper.Map<ProductionReturnAssySub>(subVM);
        //            newSub.ReturnId = existingProdReturnAssy.ReturnId;
        //            await _unitOfWork.ProductionReturnAssySubs.CreateAsync(newSub);
        //            await _unitOfWork.SaveAsync();

        //            changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Issue Qty: {subVM.QtyReturned}");

        //            //if (subVM.RefJobOrderSubId > 0)
        //            //    await AdjustJobOrderBalanceAsync(subVM.RefJobOrderSubId, 0, subVM.IssueQty ?? 0, "Production Issue Assembly Creation");

        //        }
        //        else
        //        {
        //            var existingSub = existingProdReturnAssy.ProductionReturnAssySubs.FirstOrDefault(s => s.ReturnSubId == subVM.ReturnSubId);
        //            if (existingSub != null)
        //            {
        //                //if (subVM.RefJobOrderSubId > 0)
        //                //    await AdjustJobOrderBalanceAsync(subVM.RefJobOrderSubId, existingSub.IssueQty, subVM.IssueQty ?? 0, "Production Issue Assembly Update");

        //                var subChanges = GetPropertyChanges(existingSub, subVM);
        //                if (!string.IsNullOrEmpty(subChanges))
        //                    changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

        //                _mapper.Map(subVM, existingSub);
        //            }
        //        }
        //    }
        //}

        public async Task<List<ProductionReturnAssyTrackVM>> GetAllExistingJobOrderAsync()
        {
            try
            {
                var jobOrders = await _unitOfWork.JobOrders.GetQueryable()
                    .Where(j => j.JobBalQty > 0)
                    .Select(j => new
                    {
                        j.JobId,
                        j.JobNo,
                        j.Suffix,
                        ItemCode = j.AssyItem.ItemCode
                    })
                    .ToListAsync();

                var result = jobOrders
                    .Select(j => new ProductionReturnAssyTrackVM
                    {
                        RefJobOrderId = j.JobId,
                        RefJobNo = $"{j.JobNo}{j.Suffix} ({j.ItemCode})"
                    })
                    .OrderBy(j => j.RefJobNo)
                    .ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching Job Orders where JobTally = 0");
                throw;
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
                changes.AppendLine($"CostCenter Updated - ProjectNo {cost.ProjectNo}: AssToQuote set to '{assign}'");
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

        public async Task<bool> HasAnyPOTransactionMadeAsync(int quoteId)
        {
            try
            {
                var quoteSubIds = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(s => s.QuoteId == quoteId)
                    .Select(s => s.QuoteSubId)
                    .ToListAsync();

                if (!quoteSubIds.Any())
                    return false;

                return await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefQuoteSubId.HasValue && quoteSubIds.Contains(qs.RefQuoteSubId.Value));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyPOTransactionMadeAsync for QuoteId: {quoteId}");
                throw;
            }
        }

        public async Task<bool> HasAnyItemOrQuoteCancelAsync(int quoteId)
        {
            try
            {
                var isQuoteCancelled = await _unitOfWork.MfgQuotes
                    .AnyAsync(q => q.QuoteId == quoteId && q.IsCancel == true);

                var isItemCancelled = await _unitOfWork.MfgQuoteSubs
                    .AnyAsync(i => i.QuoteId == quoteId && i.ItemCancel == true);

                return isQuoteCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrQuoteCancelAsync for QuoteId: {quoteId}");
                throw;
            }
        }



        public async Task<bool> DeleteQuotationByQuoteIdAsync(int QuoteId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the quotation with its sub-items
                var quotation = await _unitOfWork.MfgQuotes
                    .GetQueryable()
                    .Include(e => e.MfgQuoteSub)
                    .FirstOrDefaultAsync(e => e.QuoteId == QuoteId);

                if (quotation == null)
                {
                    // Quotation not found
                    return false;
                }

                var changes = new StringBuilder();

                foreach (var sub in quotation.MfgQuoteSub)
                {
                    if (sub.RefEnqSubId > 0)
                    {
                        //await AdjustEnquiryBalanceAsync(sub.RefEnqSubId, sub.Qty, 0, "Quote Deletion");
                    }
                    else
                    {
                        await UpdateCostCenterAsync(sub.CostId, false, changes);
                    }
                }

                // Delete the quotation
                var deleted = await _unitOfWork.MfgQuotes.DeleteAsync(QuoteId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Quotation List",
                    action: $"Deleted Quotation: {quotation.QuoteNo}",
                    additionalInfo: $"Quotation Id: {quotation.QuoteId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Quotation: {QuoteId}");
                throw;
            }
        }

        public async Task<List<Dictionary<string, object>>> GetEnquiryDetailsByCustId(int custId)
        {
            try
            {
                var result = await (from e in _unitOfWork.EnquirySaless.GetQueryable()
                                    join es in _unitOfWork.EnquirySalesSubs.GetQueryable()
                                        on e.EnquiryId equals es.EnquiryId
                                    where e.CustId == custId && !e.EnquiryTally && !e.Cancel && es.BalQty > 0 && !es.ItemCancel
                                    select new
                                    {
                                        es.EnquirySubId,
                                        e.EnquiryNo,
                                        e.EnquiryDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.UOM,
                                        es.Qty,
                                        es.BalQty,
                                        es.UnitPrice,
                                        CostCenterId = es.CostCenterId == 0 ? (int?)null : es.CostCenterId,
                                        es.CostCenter.ProjectNo
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefEnqSubId"] = r.EnquirySubId,
                    ["EnquiryNo"] = r.EnquiryNo,
                    ["EnquiryDate"] = r.EnquiryDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.UOM ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["Rate"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching enquiry details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve enquiry details. Please try again.");
            }
        }
        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.ProductionReturnAssySubs.GetQueryable()
                                    where qs.ItemId == itemId
                                    orderby qs.ReturnSubId descending
                                    select qs.UnitPrice)
                                    .FirstOrDefaultAsync();

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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }

        public async Task<string> GetPrefixFromDb()
        {
            try
            {
                var prefix = await _unitOfWork.MfgQuotes.GetQueryable()
                            .AsNoTracking()
                            .OrderByDescending(q => q.QuoteId)
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
        public async Task<string> GetProductionIssueNumberAsync(string suffix)
        {
            try
            {
                var lastProdIssue = await _unitOfWork.ProductionIssueAssys
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.IssueNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastProdIssue != null)
                {
                    var parts = lastProdIssue.IssueNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Production Issue number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Production Issue number.");
            }
        }
        public async Task<decimal> GetEnquiryItemBalQtyFromEnqSubId(int enqSubId)
        {
            try
            {
                return await _unitOfWork.EnquirySalesSubs.GetQueryable()
                    .Where(e => e.EnquirySubId == enqSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for EnqSubId: {enqSubId}");
                throw new InvalidOperationException("Failed to retrieve enquiry balance quantity.");
            }
        }

        public async Task<ProductionReturnAssySubVM?> GetProdReturnSubItemDetailByReturnSubIdAsync(int returnSubId)
        {
            try
            {
                return await _unitOfWork.ProductionReturnAssySubs
                    .GetQueryable()
                    .Where(q => q.ReturnSubId == returnSubId)
                    .Select(q => new ProductionReturnAssySubVM
                    {
                        QtyReturned = q.QtyReturned,
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

        public async Task UpdateItemCancelAndAddorRevertAsync(MfgQuoteSubVM subItem, int quoteId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.MfgQuoteSubs.GetQueryable().Where
                                (x => x.QuoteSubId == subItem.QuoteSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with QuoteSubId {subItem.QuoteSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateEnquiryBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.MfgQuoteSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    //await AdjustEnquiryBalanceAsync(
                    //    subEntity.RefEnqSubId,
                    //    subEntity.Qty,
                    //    0,
                    //    $"Quotation Item Cancel - {subItem.ItemCode}"
                    //);
                }
                else
                {
                    //await AdjustEnquiryBalanceAsync(
                    //    subEntity.RefEnqSubId,
                    //    0,
                    //    subEntity.Qty,
                    //    $"Quotation Item Revert Cancel - {subItem.ItemCode}"
                    //);
                }

                await UpdateProductionIssueTallyStatusAsync(quoteId);

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

        public async Task UpdateProductionIssueTallyStatusAsync(int issueId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.ProductionIssueAssys
                    .GetQueryable()
                    .Where(x => x.IssueId == issueId)
                    .SumAsync(x => (decimal?)x.IssueQty) ?? 0;

                var productionIssueAssy = await _unitOfWork.ProductionIssueAssys.GetAsync(issueId);
                if (productionIssueAssy == null)
                    return;

                productionIssueAssy.IssueTally = (totalBalQty == 0);

                await _unitOfWork.ProductionIssueAssys.UpdateAsync(productionIssueAssy);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateProductionIssueTallyStatusAsync] Error updating IssueId {issueId}");
                throw new InvalidOperationException("Failed to update Production Tally status. Please contact support.");
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(MfgQuoteVM quoteVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingQuote = await _unitOfWork.MfgQuotes.GetAsync(quoteVM.QuoteId);
                if (existingQuote == null)
                    throw new InvalidOperationException("Manufacturing Quotation not found.");

                var subs = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(s => s.QuoteId == quoteVM.QuoteId)
                    .ToListAsync();

                if (!quoteVM.IsCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateEnquiryBalanceBeforeRevertAsync(sub);
                    }
                }

                existingQuote.IsCancel = quoteVM.IsCancel;
                existingQuote.CancelReason = quoteVM.CancelReason;
                await _unitOfWork.MfgQuotes.UpdateAsync(existingQuote);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingQuote.IsCancel)
                    {
                        if (sub.RefEnqSubId.GetValueOrDefault() > 0)
                        {
                            //await AdjustEnquiryBalanceAsync(
                            //    sub.RefEnqSubId.Value,
                            //    sub.Qty,
                            //    0,
                            //    $"Manufacturing Quotation Cancelled - {existingQuote.QuoteNo}"
                            //);
                        }
                    }
                    else
                    {
                        if (sub.RefEnqSubId.GetValueOrDefault() > 0)
                        {
                            //await AdjustEnquiryBalanceAsync(
                            //    sub.RefEnqSubId.Value,
                            //    0,
                            //    sub.Qty,
                            //    $"Manufacturing Quotation Reverted - {existingQuote.QuoteNo}"
                            //);
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
        private async Task ValidateEnquiryBalanceBeforeRevertAsync(MfgQuoteSub sub)
        {
            if (sub.RefEnqSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.EnquirySalesSubs.GetAsync(sub.RefEnqSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"enquiry not found for RefEnqSubId: {sub.RefEnqSubId}");

            if (entity.BalQty < sub.Qty)
            {
                throw new InvalidOperationException(
                    $"Cannot revert because Enquiry balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                );
            }
        }


        public async Task<List<ProductionReturnAssyStatusVM>> GetProductionReturnAssyStatusListAsync(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<ProductionReturnAssyStatusVM>("Sp_GetProductionReturnAssyStatusList", status);
                return result.ToList();


            }
            catch (Exception ex)
            {

                throw;
            }
        }


    }
}
