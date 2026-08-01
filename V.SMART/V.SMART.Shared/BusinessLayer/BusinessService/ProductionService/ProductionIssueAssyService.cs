using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdAssStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.ProductionService
{
    public class ProductionIssueAssyService : IProductionIssueAssyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManagerService;

        public ProductionIssueAssyService(
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

        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        //Stores
        public async Task<List<Store>> GetAllActiveStoresAsync()
        {
            var result = await _commonService.GetAllIssueStoresAsync();
            return result.ToList();
        }

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        public async Task<decimal> GetStockForItemAsync(int itemId, int storeId)
            => await _stockManagerService.GetStockForItemAsync(itemId, storeId);
        public async Task<bool> IsMonthwiseNumberGenerationEnabledAsync()
         => await _commonService.GetScreenPermissionsAsync("Production Issue WO Assembly", "Month-wise Number Generation");

        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId)
        {
            return await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);
        }

        // 🔹 Production issue operations

        public async Task<int> GetPendingJobOrdersCountAsync()
        {
            return await _unitOfWork.JobOrders
                .GetQueryable()
                .Include(j => j.JobOrderSubs)
                .Where(j => j.JobTally == false
                            && !j.Cancel
                            && j.JobOrderSubs.Any(s => !s.ItemCancel))
                .CountAsync();
        }


        public async Task<(List<ProductionIssueAssyVM> issueAssyVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, 
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ProductionIssueAssys.GetQueryable()
                .Include (j =>j.StoreIssue)
                .Include(j => j.ProductionIssueAssySubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.ProductionIssueAssySubs)
                    .ThenInclude(s => s.JobOrder)
                      .ThenInclude(s => s.Staff)
                .Include(j => j.ProductionIssueAssySubs)
                    .ThenInclude(s => s.AssyItem)
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
            var vmList = _mapper.Map<List<ProductionIssueAssyVM>>(list);

            return (vmList, total);
        }

        public static class MaterialIssueFilterBuilder
        {
            public static IQueryable<ProductionIssueAssy> ApplyFilter(
                IQueryable<ProductionIssueAssy> query, string field, object value)
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
                                  (string.IsNullOrEmpty(part1) || x.DepartmentCode.StartsWith(part1)) ||
                                  (string.IsNullOrEmpty(part1) || x.MonthCode.StartsWith(part1)) ||
                                  (string.IsNullOrEmpty(part1) || x.IssueNo.StartsWith(part1)) ||
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

                            //return query.Where(x =>
                            //    x.ProductionIssueAssySubs.Any(s =>
                            //        (string.IsNullOrEmpty(part1) || s.JobOrder.JobNo.StartsWith(part1)) &&
                            //        (string.IsNullOrEmpty(part2) || s.JobOrder.Suffix.Contains(part2))
                            //    )
                            //);
                              return query.Where(x =>
                                    x.ProductionIssueAssySubs.Any(s =>
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
                        }

                    case "AssemblyName":
                        return query.Where(x => x.ProductionIssueAssySubs
                            .Any(s => s.AssyItem.ItemCode.Contains(val)));

                    case "ItemCode":
                        return query.Where(x => x.ProductionIssueAssySubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.ProductionIssueAssySubs
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

            private static IQueryable<ProductionIssueAssy> ApplyStatusFilter(
                IQueryable<ProductionIssueAssy> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.IssueTally == true),
                    "Pending" => query.Where(x => x.IssueTally == false),
                    _ => query
                };
            }
        }


        public async Task<List<Dictionary<string, object>>> GetAllOpenJobOrdersAsync(int storeId)
        {
            try
            {
                var result = await _unitOfWork.JobOrders.GetQueryable()
                    .AsNoTracking()
                    .Include(j => j.AssyItem)
                    .Include(j => j.JobOrderSubs).ThenInclude(s => s.Item)
                    .Include(j => j.JobOrderSubs).ThenInclude(s => s.CostCenter)
                    .Include(j => j.Staff)
                    .Where(j => !j.JobTally
                                && !j.Cancel
                                && j.JobOrderSubs.Any(s => s.BalQty > 0))
                    .ToListAsync();

                var finalList = new List<Dictionary<string, object>>();

                var allItemIds = result.SelectMany(j => j.JobOrderSubs)
                    .Where(s => s.ItemId != null && s.BalQty > 0)
                    .Select(s => s.ItemId.Value)
                    .Distinct()
                    .ToList();

                var stockDict = await _stockManagerService.GetStockForItemsAsync(allItemIds, storeId);

                foreach (var j in result)
                {
                    foreach (var s in j.JobOrderSubs.Where(x => x.ItemId != null && x.BalQty > 0))
                    {
                        var stockQty = stockDict.ContainsKey(s.ItemId.Value)
                            ? stockDict[s.ItemId.Value]
                            : 0m;

                        finalList.Add(new Dictionary<string, object>
                        {
                            ["Selected"] = false,

                            ["RefJobId"] = j.JobId,
                            ["RefJobSubId"] = s.JobSubId,
                            //["RefJobNo"] = $"{j.JobNo}{j.Suffix}",
                            ["RefJobNo"] = !string.IsNullOrWhiteSpace(j.Staff?.DepartmentCode)
                                         ? $"{j.Staff?.DepartmentCode}/{j.JobNo ?? ""}{j.Suffix ?? ""}"
                                         : $"{j.JobNo ?? ""}{j.Suffix ?? ""}",

                            ["RefJobDate"] = j.JobDate,

                            ["Assy/SubAssyId"] = j.AssyId,
                            ["Assy/SubAssyCode"] = j.AssyItem?.ItemCode ?? string.Empty,
                            ["Assy/SubAssyName"] = j.AssyItem?.ItemName ?? string.Empty,
                            ["Assy/SubAssyUom"] = j.AssyItem?.MeasureUnit ?? string.Empty,
                            ["JobOrderQty"] = j.JobOrderQty,

                            ["ItemId"] = s.Item?.ItemId,
                            ["ItemCode"] = s.Item?.ItemCode ?? string.Empty,
                            ["ItemName"] = s.Item?.ItemName ?? string.Empty,
                            ["MeasureUnit"] = s.Item?.MeasureUnit ?? string.Empty,

                            // Apply max 3 decimals
                            ["ReqQty"] = Math.Round(s.BalQty, 3, MidpointRounding.AwayFromZero),
                            ["StockQty"] = Math.Round(stockQty, 3, MidpointRounding.AwayFromZero),

                            ["CostCenterId"]= s.CostId,
                            ["CostCenter"]= s.CostCenter?.ProjectNo ?? string.Empty
                        });
                    }
                }
                return finalList;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching open Job Orders");
                throw new InvalidOperationException("Failed to retrieve open Job Orders. Please try again.");
            }
        }




        public async Task<ProductionIssueAssyVM?> GetProductionByIssueIdAsync(int issueId)
        {
            try
            {
                var entity = await _unitOfWork.ProductionIssueAssys.GetQueryable()
                    .Include(q => q.StoreIssue)
                    .Include(q => q.ProductionIssueAssySubs)
                    .ThenInclude(s => s.Item)
                    .Include(q => q.ProductionIssueAssySubs)
                    .ThenInclude(s => s.CostCenter)
                    .Include(q => q.ProductionIssueAssySubs)
                    .ThenInclude(s => s.JobOrder)
                     .ThenInclude(s => s.Staff)
                    .Include(q => q.ProductionIssueAssySubs)
                    .ThenInclude(s => s.AssyItem)
                    .FirstOrDefaultAsync(q => q.IssueId == issueId);

                if (entity == null)
                    return null;

                // Map entity → ViewModel
                var vm = _mapper.Map<ProductionIssueAssyVM>(entity);

                var itemIds = vm.ProductionIssueAssySubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && vm.StoreIssId.HasValue)
                {
                    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, vm.StoreIssId.Value);

                    foreach (var sub in vm.ProductionIssueAssySubVMs)
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

        public async Task DeleteAndResequenceAsync(ProductionIssueAssySubVM subitem, ProductionIssueAssyVM productionIssueAssyVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.IssueSubId > 0)
                {
                    var subEntity = await _unitOfWork.ProductionIssueAssySubs.GetAsync(subitem.IssueSubId);
                    if (subEntity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    await _unitOfWork.ProductionIssueAssySubs.DeleteAsync(subEntity.IssueSubId);
                    await _unitOfWork.SaveAsync();

                    if (subEntity.RefJobOrderSubId > 0)
                    {
                        await AdjustJobOrderBalanceAsync(subitem.RefJobOrderSubId, subEntity.IssueQty, 0, "Production Issue Item Deletion");
                    }

                    await DeleteStockIssueAndTrackAsync(subitem.IssueSubId, subitem.ItemId.Value, screenCode);

                    await UpdatePoductionIssueAssyTallyStatusAsync(productionIssueAssyVM.IssueId);

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Production Issue Assembly",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Issue No: {productionIssueAssyVM?.IssueNo}"
                    );
                }
                else
                {
                    productionIssueAssyVM.ProductionIssueAssySubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Where(x => x.IssueId == productionIssueAssyVM.IssueId)
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

        private async Task AdjustJobOrderBalanceAsync(int? refJobOrderSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refJobOrderSubId.HasValue || refJobOrderSubId == 0) return;

                var jobSub = await _unitOfWork.JobOrderSubs.GetAsync(refJobOrderSubId.Value);
                if (jobSub == null) return;

                if (oldQty > 0)
                    jobSub.BalQty += oldQty;

                if (newQty > jobSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Job Order Item BalQty.");

                if (newQty > 0)
                    jobSub.BalQty -= newQty;

                await _unitOfWork.JobOrderSubs.UpdateAsync(jobSub);
                await _unitOfWork.SaveAsync();

                //var totalBalQty = await _unitOfWork.JobOrderSubs
                //    .GetQueryable()
                //    .Where(e => e.JobId == jobSub.JobId && !e.ItemCancel)
                //    .SumAsync(e => e.BalQty);

                //var jobOrder = await _unitOfWork.JobOrders.GetAsync(jobSub.JobId);
                //if (jobOrder != null)
                //{
                //    jobOrder.JobTally = (totalBalQty == 0);
                //    await _unitOfWork.JobOrders.UpdateAsync(jobOrder);
                //    await _unitOfWork.SaveAsync();
                //}
            }
            catch (InvalidOperationException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustJobOrderBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Job Order Item Balance. Please contact support.");
            }
        }


        public async Task<List<ProductionIssueAssySubVM>> GetProductionIssueSubByIssueIdAsync(int issueId)
        {
            try
            {
                var subs = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.IssueId == issueId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ProductionIssueAssySubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production issue sub items for IssueId: {issueId}");
                throw new InvalidOperationException("Failed to retrieve Production issue sub-items. Please try again.");
            }
        }

        public async Task<decimal> GetJobOrderItemBalQtyFromJobSubId(int jobSubId)
        {
            try
            {
                return await _unitOfWork.JobOrderSubs.GetQueryable()
                    .Where(e => e.JobSubId == jobSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for JobSubId: {jobSubId}");
                throw new InvalidOperationException("Failed to retrieve Job Order balance quantity.");
            }
        }
        public async Task<ProductionIssueAssySubVM?> GetProductionIssueSubItemDetailByIssueSubIdAsync(int issueSubId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Where(q => q.IssueSubId == issueSubId)
                    .Select(q => new ProductionIssueAssySubVM
                    {
                        IssueQty = q.IssueQty,
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

        public async Task<ProductionIssueAssyVM> UpsertProdIssueAssy(ProductionIssueAssyVM issueVM, int screenCode,bool IsMonthwiseNumber)
        {
            if (issueVM == null)
                throw new ArgumentNullException(nameof(issueVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ProductionIssueAssy entity;

                if (issueVM.IssueId == 0)
                {
                    entity = _mapper.Map<ProductionIssueAssy>(issueVM);

                    //var nextNumber = await _unitOfWork.ProductionIssueAssys.GetLastIssueNoAsync(entity.Suffix);
                    //entity.IssueNo = nextNumber;

                    if (IsMonthwiseNumber)
                    {
                        entity.DepartmentCode = await GetDerpartmentCodeByCurrentuserAsync();
                        entity.MonthCode = DateTime.Now.Month.ToString("D2");
                        entity.IssueNo = await _unitOfWork.ProductionIssueAssys.GetMonthWiseProductionIssueNumberAsync(entity.Suffix);

                    }
                    else
                    {
                        var nextNumber = await _unitOfWork.ProductionIssueAssys.GetLastIssueNoAsync(entity.Suffix);
                        entity.IssueNo = nextNumber;
                    }

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.ProductionIssueAssySubs = issueVM.ProductionIssueAssySubVMs
                        .Select(s => _mapper.Map<ProductionIssueAssySub>(s))
                        .ToList();

                    await _unitOfWork.ProductionIssueAssys.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.ProductionIssueAssySubs)
                    {
                        if (sub.RefJobOrderSubId > 0)
                            await AdjustJobOrderBalanceAsync(sub.RefJobOrderSubId, 0, sub.IssueQty, "Production Issue Assembly create");

                        await _stockManagerService.IssueOrUpdateStockAsync(
                            sub.ItemId,
                            entity.StoreIssId,
                            sub.IssueQty,
                            sub.UnitPrice,
                            null,
                            screenCode,
                            sub.IssueSubId,
                            entity.IssueNo,
                            entity.IssueDate
                        );
                    }
                   
                    changes.AppendLine("Production Issue Assembly Created.");
                }

                else
                {
                    entity = await _unitOfWork.ProductionIssueAssys.GetQueryable()
                        .Include(q => q.ProductionIssueAssySubs)
                        .FirstOrDefaultAsync(q => q.IssueId == issueVM.IssueId)
                        ?? throw new InvalidOperationException("Production Issue Assy not found.");

                    var parentChanges = GetPropertyChanges(entity, issueVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(issueVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, issueVM.ProductionIssueAssySubVMs, changes, screenCode);

                    changes.AppendLine("Production Issue Assembly Issue Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdatePoductionIssueAssyTallyStatusAsync(issueVM.IssueId);

                await LogChangesAsync(changes, issueVM.IssueId == 0 ? "Prouction Issue Assembly Created" : "Production Issue Assembly Updated");

                await transaction.CommitAsync();

                var savedEntity = await _unitOfWork.ProductionIssueAssys.GetQueryable()
                    .Include(q => q.StoreIssue)
                    .Include(q => q.ProductionIssueAssySubs)
                        .ThenInclude(s => s.Item)
                    .FirstOrDefaultAsync(q => q.IssueId == entity.IssueId);

                return _mapper.Map<ProductionIssueAssyVM>(savedEntity!);
            }
            catch (InvalidOperationException)
            {
                await transaction.RollbackAsync();
                throw; 
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert production Issue Assy: {issueVM.IssueNo}");

                throw new InvalidOperationException("Failed to save Production Issue Assembly. All changes have been reverted.",ex); 
            }
        }

        public async Task UpdatePoductionIssueAssyTallyStatusAsync(int issueId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Where(x => x.IssueId == issueId)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var Issue = await _unitOfWork.ProductionIssueAssys.GetAsync(issueId);
                if (Issue == null)
                    return;

                Issue.IssueTally = (totalBalQty == 0);

                await _unitOfWork.ProductionIssueAssys.UpdateAsync(Issue);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdatePoductionIssueAssyTallyStatusAsync] Error updating IssueId {issueId}");
                throw new InvalidOperationException("Failed to update IssueTally status. Please contact support.");
            }
        }

        private async Task HandleChildUpdatesAsync(ProductionIssueAssy existingIssue, List<ProductionIssueAssySubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingIssue.ProductionIssueAssySubs.Select(s => s.IssueSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.IssueSubId).ToHashSet();

            foreach (var sub in existingIssue.ProductionIssueAssySubs.Where(s => !incomingSubIds.Contains(s.IssueSubId)).ToList())
            {
                await DeleteStockIssueAndTrackAsync(sub.IssueSubId, sub.ItemId, screenCode);

                changes.AppendLine($"Child Deleted - IssueSubId: {sub.IssueSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ProductionIssueAssySubs.DeleteAsync(sub.IssueSubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefJobOrderSubId > 0)
                    await AdjustJobOrderBalanceAsync(sub.RefJobOrderSubId, sub.IssueQty, 0, "Production Issue Assembly delete");
            }

            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.IssueSubId == 0)
                {
                    var newSub = _mapper.Map<ProductionIssueAssySub>(subVM);
                    newSub.IssueId = existingIssue.IssueId;
                    await _unitOfWork.ProductionIssueAssySubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode},Issue Qty: {subVM.IssueQty}");

                    await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId, existingIssue.StoreIssId, newSub.IssueQty,
                         newSub.UnitPrice, null, screenCode, newSub.IssueSubId, existingIssue.IssueNo, existingIssue.IssueDate);

                    if (subVM.RefJobOrderSubId > 0)
                        await AdjustJobOrderBalanceAsync(subVM.RefJobOrderSubId, 0, subVM.IssueQty ?? 0, "Production Issue Assembly Creation");

                }
                else
                {
                    var existingSub = existingIssue.ProductionIssueAssySubs.FirstOrDefault(s => s.IssueSubId == subVM.IssueSubId);
                    if (existingSub != null)
                    {

                        if (subVM.RefJobOrderSubId > 0)
                            await AdjustJobOrderBalanceAsync(subVM.RefJobOrderSubId, existingSub.IssueQty, subVM.IssueQty ?? 0, "Production Issue Assembly Update");

                        await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingIssue.StoreIssId, subVM.IssueQty.GetValueOrDefault(),
                        subVM.UnitPrice, null, screenCode, subVM.IssueSubId, existingIssue.IssueNo, existingIssue.IssueDate);

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
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
                screen: "Tool Crib Issue",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<ProductionIssueAssyVM> GetJobOrderDetailsByIdAsync(int jobId, int storeId)
        {
            try
            {
                var jobOrder = await _unitOfWork.JobOrders.GetQueryable()
                    .Include(j => j.AssyItem)
                    .FirstOrDefaultAsync(j => j.JobId == jobId && !j.Cancel);

                if (jobOrder == null)
                {
                    await _logs.LogDeveloperError(null, $"No JobOrder found for JobId: {jobId}");
                    return null;
                }

                var jobSubs = await _unitOfWork.JobOrderSubs.GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.JobId == jobId && !s.ItemCancel)
                    .ToListAsync();

                if (!jobSubs.Any())
                {
                    await _logs.LogDeveloperError(null, $"No JobOrderSubs found for JobId: {jobId}");
                    return null;
                }

                var itemIds = jobSubs
                    .Where(sub => sub.ItemId.HasValue)
                    .Select(sub => sub.ItemId.Value)
                    .Distinct()
                    .ToList();

                // 1️⃣ Fetch stock
                var storeStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(s => s.StoreId == storeId && itemIds.Contains(s.ItemId))
                    .GroupBy(s => s.ItemId)
                    .Select(g => new
                    {
                        ItemId = g.Key,
                        StockQty = g.Sum(x => x.BalQty)
                    })
                    .ToDictionaryAsync(x => x.ItemId, x => x.StockQty);

                // 2️⃣ Fetch Unit Prices (Your new dictionary)
                var unitPriceDict = await GetBulkLastUnitPricesAsync(itemIds);

                int slno = 1;

                // 3️⃣ Build the ViewModel
                var vm = new ProductionIssueAssyVM
                {
                    ProductionIssueAssySubVMs = jobSubs.Select(sub => new ProductionIssueAssySubVM
                    {
                        SlNo = slno++,
                        RefJobId = jobOrder.JobId,
                        JobOrderNo = $"{jobOrder.JobNo}{jobOrder.Suffix}",
                        JobOrderDate = jobOrder.JobDate,
                        AssyId = jobOrder.AssyId.Value,
                        AssyItemCode = jobOrder.AssyItem?.ItemName,

                        ItemId = sub.ItemId,
                        ItemCode = sub.Item?.ItemCode,
                        ItemName = sub.Item?.ItemName,
                        MeasureUnit = sub.Item?.MeasureUnit,

                        IssueQty = sub.BalQty,
                        BalQty = sub.BalQty,

                        // ⭐️ Unit Price from dictionary
                        UnitPrice = unitPriceDict.TryGetValue(sub.ItemId ?? 0, out var price) ? price : 0,

                        RefJobOrderSubId = sub.JobSubId,
                        CostId = sub.CostId,
                        ProjectNo = sub.CostCenter?.ProjectNo,

                        StockQty = storeStock.TryGetValue(sub.ItemId ?? 0, out var qty) ? qty : 0,

                        IsEditable = true
                    }).ToList()
                };

                return vm;
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

        public async Task<bool> HasAnyProdReturnAssyTransactionMadeAsync(int issueId)
        {
            try
            {
                var IssueSubIds = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Where(s => s.IssueId == issueId)
                    .Select(s => s.IssueSubId)
                    .ToListAsync();

                if (!IssueSubIds.Any())
                    return false;

                return await _unitOfWork.ProductionReturnAssyTracks
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefIssueSubId.HasValue && IssueSubIds.Contains(qs.RefIssueSubId.Value));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyProdReturnAssyTransactionMadeAsync for IssueId: {issueId}");
                throw;
            }
        }

        public async Task<bool> DeleteProdAssyIssueByIssueIdAsync(int issueId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var productionIssueAssy = await _unitOfWork.ProductionIssueAssys
                    .GetQueryable()
                    .Include(e => e.ProductionIssueAssySubs)
                    .FirstOrDefaultAsync(e => e.IssueId == issueId);

                if (productionIssueAssy == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in productionIssueAssy.ProductionIssueAssySubs)
                {
                    if (sub.RefJobOrderSubId > 0)
                    {
                        await AdjustJobOrderBalanceAsync(sub.RefJobOrderSubId, sub.IssueQty, 0, "Production Issue Assembly Deletion");
                    }
                    await DeleteStockIssueAndTrackAsync(sub.IssueSubId, sub.ItemId, screenCode);
                }

                var ProductionIssueAssy = await _unitOfWork.ProductionIssueAssys.GetAsync(issueId);
                await _unitOfWork.ProductionIssueAssys.DeleteAsync(ProductionIssueAssy);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Production Issue Assembly",
                    action: $"Deleted Issue no: {productionIssueAssy.IssueNo}",
                    additionalInfo: $"Issue Id: {productionIssueAssy.IssueId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Production Issue Assy: {issueId}");
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
                var quotation = await _unitOfWork.MfgQuotes
                    .GetQueryable()
                    .Include(e => e.MfgQuoteSub)
                    .FirstOrDefaultAsync(e => e.QuoteId == QuoteId);

                if (quotation == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                foreach (var sub in quotation.MfgQuoteSub)
                {
                    if (sub.RefEnqSubId > 0)
                    {
                        //await AdjustEnquiryBalanceAsync(sub.RefEnqSubId, sub.Qty, 0, "Quote Deletion");
                    }
                }

                var deleted = await _unitOfWork.MfgQuotes.DeleteAsync(QuoteId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

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
            try
            {
                itemIds = itemIds.Distinct().ToList();
                var result = new Dictionary<int, decimal>();

                var lastRates = await _unitOfWork.ProductionIssueAssySubs.GetQueryable()
                    .Where(q => itemIds.Contains(q.ItemId))
                    .GroupBy(q => q.ItemId)
                    .Select(g => new
                    {
                        ItemId = g.Key,
                        UnitPrice = g.OrderByDescending(x => x.IssueSubId)
                                     .Select(x => x.UnitPrice)
                                     .FirstOrDefault()
                    })
                    .ToListAsync();

                foreach (var r in lastRates)
                    result[r.ItemId] = r.UnitPrice;

                var missingItemIds = itemIds
                    .Where(id => !result.ContainsKey(id) || result[id] == 0)
                    .ToList();

                if (missingItemIds.Any())
                {
                    var fallbackRates = await _unitOfWork.ItemRepositories.GetQueryable()
                        .Where(i => missingItemIds.Contains(i.ItemId))
                        .Select(i => new { i.ItemId, i.Rate })
                        .ToListAsync();

                    foreach (var f in fallbackRates)
                        result[f.ItemId] = f.Rate;
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices.");
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

        public async Task<MfgQuoteSubVM?> GetQuoteSubItemDetailByQuoteSubIdAsync(int quoteSubId)
        {
            try
            {
                return await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .Where(q => q.QuoteSubId == quoteSubId)
                    .Select(q => new MfgQuoteSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching quote sub item detail for QuoteSubId: {quoteSubId}");
                throw new InvalidOperationException("Failed to retrieve quote sub-item details.");
            }
        }
        public async Task<List<ProductionIssueAssySubVM>> GetDistinctRefJobNoByIssueIdAsync(int issueId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .Where(s => s.IssueId == issueId)
                    .GroupBy(s => new
                    {
                        s.JobOrder.JobNo,
                        s.JobOrder.Suffix,
                        s.JobOrder.JobDate,
                        DeptCode = s.JobOrder.Staff.DepartmentCode
                    })
                    .Select(g => new ProductionIssueAssySubVM
                    {
                        JobOrderNo = !string.IsNullOrEmpty(g.Key.DeptCode)
                            ? g.Key.DeptCode + "/" + g.Key.JobNo + (g.Key.Suffix ?? "")
                            : g.Key.JobNo + (g.Key.Suffix ?? ""),

                        JobOrderDate = g.Key.JobDate
                    })
                    .ToListAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetDistinctRefJobNoByIssueIdAsyn IssueId: {issueId}");
                throw new InvalidOperationException("Failed to retrieve GetDistinctRefJobNoByIssueIdAsync");
            }
            //return await _unitOfWork.ProductionIssueAssySubs
            //    .GetQueryable()
            //    .Where(s => s.IssueId == issueId)
            //    .GroupBy(s => new { s.JobOrder.JobNo, s.JobOrder.Suffix, s.JobOrder.JobDate })
            //    .Select(g => new ProductionIssueAssySubVM
            //    {
            //        JobOrderNo = $"{g.Key.JobNo}{g.Key.Suffix}", 
            //        JobOrderDate = g.Key.JobDate
            //    })
            //    .ToListAsync();
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
        public async Task<string> GetMonthWiseProductionIssueNumberAsync(string suffix)
        {
            try
            {
                string deptCode = "";
                int userId = await _currentUserService.GetUserIdAsync();


                var user = await _unitOfWork.Users
                    .GetQueryable()
                    .Include(u => u.Staff)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user?.Staff?.DepartmentCode != null)
                {
                    deptCode = user.Staff.DepartmentCode;
                }
                else
                {

                    deptCode = "AD";
                }

                var today = DateTime.Now;
                string monthCode = today.Month.ToString("D2");
                var last = await _unitOfWork.ProductionIssueAssys
                    .GetQueryable()
                    .Where(x =>
                        x.DepartmentCode == deptCode &&
                        x.MonthCode == monthCode &&
                        x.Suffix == suffix)
                    .OrderByDescending(x => x.IssueNo)
                    .FirstOrDefaultAsync();
                int nextNumber = 1;
                if (last != null)
                {
                    var parts = last.IssueNo.Split('/');
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
        public async Task<string> GetDerpartmentCodeByCurrentuserAsync()
        {
            try
            {
                string deptCode = "";
                int userId = await _currentUserService.GetUserIdAsync();

                var user = await _unitOfWork.Users
                    .GetQueryable()
                    .Include(u => u.Staff)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user?.Staff?.DepartmentCode != null)
                {
                    deptCode = user.Staff.DepartmentCode;
                }
                else
                {

                    deptCode = "AD";
                }

                return deptCode;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error GetDerpartmentCodeByCurrentuserAsync");
                throw new InvalidOperationException("Failed to GetDerpartmentCodeByCurrentuserAsync");
            }

        }
        public async Task<List<StaffVM>> GetStaffDetailsByJobOrderIdAsync(List<int> jobOrderIds)
        {
            try
            {
                var result = await (
                    from j in _unitOfWork.JobOrders.GetQueryable()
                    join s in _unitOfWork.Staffs.GetQueryable()
                        on j.StaffID equals s.StaffID into staffJoin
                    from s in staffJoin.DefaultIfEmpty()
                    where jobOrderIds.Contains(j.JobId)
                    select new StaffVM
                    {
                        StaffID = j.StaffID ?? 0,
                        StaffName = s != null ? s.StaffName : null,
                        DepartmentCode = s.DepartmentCode
                    }
                ).Distinct()
                .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching GetStaffDetailsByJobOrderIdAsync for JobIds: {string.Join(",", jobOrderIds)}"
                );

                throw new InvalidOperationException(
                    "Failed to retrieve staff details for selected Job Orders."
                );
            }
        }


        public async Task<List<ProductionIssueAssyStatusVM>> GetProductionIssueAssyStatusListAsync(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<ProductionIssueAssyStatusVM>("Sp_GetProductionIssueAssyStatusList", status);
                return result.ToList();


            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
