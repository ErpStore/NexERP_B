using AutoMapper;
using AutoMapper.QueryableExtensions;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.ToolCribIssueStatusViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService
{
    public class ToolCribIssueService : IToolCribIssueService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManagerService;

        public ToolCribIssueService(
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

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsByCategoryAsync(int categoryCode,string searchText)
            => await _unitOfWork.ItemRepositories.SearchItemsByCategoryAsync(categoryCode,searchText);
        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);
        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();
        public async Task<List<Staff>> GetAllStaffAsync()
            => await _commonService.GetAllStaffAsync();

        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
           => await _commonService.GetMappedStoreForFormAsync(formName);

        public async Task<IEnumerable<Store?>> GetAllIssueStoresAsync()
        {
            try
            {
                return await _commonService.GetAllIssueStoresAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching issue stores");
                return Enumerable.Empty<Store>();
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            try
            {
                return await _commonService.GetAllActiveCategoriesAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching issue stores");
                return new List<Category>();
            }
        }

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        //Stock
        public async Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId)
        {
            try
            {
                return await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Get Stock For Items");
                throw new InvalidOperationException("Failed to GetStockForItemsAsync");
            }
        }

        public async Task<ToolCribIssueSubVM?> GetTCSubItemDetailByTCIssueSubIdAsync(int issueSubId)
        {
            try
            {
                return await _unitOfWork.ToolCribIssueSubs
                    .GetQueryable()
                    .Where(q => q.TCIssueSubId == issueSubId)
                    .Select(q => new ToolCribIssueSubVM
                    {
                        QtyOut = q.QtyOut,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ToolCrib sub item detail for TCIssueSubId: {issueSubId}");
                throw new InvalidOperationException("Failed to retrieve Min sub-item details.");
            }
        }

        public async Task<(List<ToolCribIssueVM> issueVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ToolCribIssues
                         .GetQueryable()
                         .AsSplitQuery()
                        .Include(e => e.ToolCribIssueSubs)
                             .ThenInclude(s => s.Item).ThenInclude(i=>i.Category)
                         .Include(e => e.Store)
                         .AsQueryable();

            string? status = null;
            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = ToolCribIssueFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.TCIssueId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<ToolCribIssueVM>>(list);

            return (vmList, total);
        }

        public static class ToolCribIssueFilterBuilder
        {
            public static IQueryable<ToolCribIssue> ApplyFilter(IQueryable<ToolCribIssue> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "IssueNo":
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
                                (string.IsNullOrEmpty(part1) || x.TCIssueNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "IssueWhom":
                        return query.Where(x => x.IssuedToWhom.Contains(val));

                    case "ItemCode":
                        return query.Where(x => x.ToolCribIssueSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.ToolCribIssueSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "StoreName":
                        return query.Where(x => x.Store.StoreName.Contains(val));

                    case "Category":
                        return query.Where(x => x.ToolCribIssueSubs.Any(s=>s.Item.Category.CategoryName.Contains(val)));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        return query.Where(x => x.TCIssueDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.TCIssueDate <= DateTime.Parse(value.ToString()));

                }

                return query;
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

                    rate = await (from ss in _unitOfWork.ToolCribIssueSubs.GetQueryable()
                                  where ss.ItemId == itemId
                                  orderby ss.TCIssueId descending
                                  select ss.UnitPrice)
                                    .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId
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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for ItemId: {itemIds}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }

        public async Task<bool> HasAnyTCReturnTransactionMadeAsync(int tcIssueId)
        {
            try
            {
                var returnSubIds = await _unitOfWork.ToolCribIssueSubs
                    .GetQueryable()
                    .Where(s => s.TCIssueId == tcIssueId)
                    .Select(s => s.TCIssueSubId)
                    .ToListAsync();

                if (!returnSubIds.Any())
                    return false;

                return await _unitOfWork.ToolCribReturnSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefTCIssueSubId.HasValue && returnSubIds.Contains(qs.RefTCIssueSubId.Value));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyTCReturnTransactionMadeAsync for TCIssueId: {tcIssueId}");
                throw;
            }
        }


        public async Task<bool> DeleteToolCribIssueByTCIssueIdAsync(int tcIssueId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var toolCribIssue = await _unitOfWork.ToolCribIssues
                    .GetQueryable()
                    .Include(e => e.ToolCribIssueSubs)
                    .FirstOrDefaultAsync(e => e.TCIssueId == tcIssueId);

                if (toolCribIssue == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in toolCribIssue.ToolCribIssueSubs)
                {
                    await DeleteStockIssueAndTrackAsync(sub.TCIssueSubId, sub.ItemId.Value, screenCode);
                }

                var deleted = await _unitOfWork.ToolCribIssues.DeleteAsync(tcIssueId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Tool Crib Issue",
                    action: $"Deleted TCIssue no: {toolCribIssue.TCIssueNo}",
                    additionalInfo: $"TCIssue Id: {toolCribIssue.TCIssueId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Tool Crib Issue: {tcIssueId}");
                throw;
            }
        }

        public async Task<ToolCribIssueVM?> GetToolCribIssueByTCIdAsync(int tcId)
        {
            try
            {
                var entity = await _unitOfWork.ToolCribIssues.GetQueryable()
                    .Include(q => q.Store)
                    .Include(q => q.Category)
                    .Include(q => q.ToolCribIssueSubs)
                    .Include(q => q.ToolCribIssueSubs).ThenInclude(s => s.Item)
                    .FirstOrDefaultAsync(q => q.TCIssueId == tcId);

                if (entity == null)
                    return null;

                // Map entity → ViewModel
                var scnVM = _mapper.Map<ToolCribIssueVM>(entity);

                var itemIds = scnVM.ToolCribIssueSubVMs
                    .Where(s => s.ItemId.HasValue)
                    .Select(s => s.ItemId!.Value)
                    .Distinct()
                    .ToList();

                if (itemIds.Count > 0 && scnVM.StoreId.HasValue)
                {
                    var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, scnVM.StoreId.Value);

                    foreach (var sub in scnVM.ToolCribIssueSubVMs)
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
                await _logs.LogDeveloperError(ex, $"GetToolCribIssueByTCIdAsync({tcId})");
                return null;
            }
        }

        public async Task<bool> HasAnyToolCribReturnTransactionMadeAsync(int tcId)
        {
            try
            {
                var issueSubIds = await _unitOfWork.ToolCribIssueSubs
                    .GetQueryable()
                    .Where(s => s.TCIssueId == tcId)
                    .Select(s => s.TCIssueSubId)
                    .ToListAsync();

                if (!issueSubIds.Any())
                    return false;

                return await _unitOfWork.ToolCribReturnSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefTCIssueSubId.HasValue && issueSubIds.Contains(qs.RefTCIssueSubId.Value));
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyToolCribReturnTransactionMadeAsync for TCIssueId: {tcId}");
                throw;
            }
        }

        public async Task<ToolCribIssueSubVM?> GetToolCribIssueSubItemDetailByTCSubIdAsync(int TCIssueSubId)
        {
            try
            {
                return await _unitOfWork.ToolCribIssueSubs
                    .GetQueryable()
                    .Where(t => t.TCIssueSubId == TCIssueSubId)
                    .Select(t => new ToolCribIssueSubVM
                    {
                        QtyOut = t.QtyOut,
                        BalQty = t.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ToolCribIssue sub item detail for TCIssueSubId: {TCIssueSubId}");
                throw new InvalidOperationException("Failed to retrieve Po sub-item details.");
            }
        }

        public async Task<ToolCribIssueVM> UpsertToolCribisueAsync(ToolCribIssueVM tcIssueVM, int screenCode)
        {
            if (tcIssueVM == null)
                throw new ArgumentNullException(nameof(tcIssueVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                ToolCribIssue entity;

                if (tcIssueVM.TCIssueId == 0)
                {
                    entity = _mapper.Map<ToolCribIssue>(tcIssueVM);

                    var nextNumber = await _unitOfWork.ToolCribIssues.GetLastToolCribIssueNoAsync(entity.Suffix);
                    entity.TCIssueNo = nextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.ToolCribIssueSubs = tcIssueVM.ToolCribIssueSubVMs
                        .Select(s => _mapper.Map<ToolCribIssueSub>(s))
                        .ToList();

                    await _unitOfWork.ToolCribIssues.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in entity.ToolCribIssueSubs)
                    {
                        await _stockManagerService.IssueOrUpdateStockAsync(
                            subVM.ItemId!.Value,
                            entity.StoreId!.Value,
                            subVM.QtyOut,
                            subVM.UnitPrice,
                            null,
                            screenCode,
                            subVM.TCIssueSubId,
                            entity.TCIssueNo,
                            entity.TCIssueDate
                        );
                    }

                    changes.AppendLine("Tool Crib Issue Created.");
                }

                else
                {
                    entity = await _unitOfWork.ToolCribIssues.GetQueryable()
                        .Include(q => q.ToolCribIssueSubs)
                        .FirstOrDefaultAsync(q => q.TCIssueId == tcIssueVM.TCIssueId)
                        ?? throw new InvalidOperationException("Tool Crib Issue not found.");

                    var parentChanges = GetPropertyChanges(entity, tcIssueVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(tcIssueVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, tcIssueVM.ToolCribIssueSubVMs, changes, screenCode);

                    changes.AppendLine("Tool Crib Issue Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateTCIssueTallyStatusAsync(tcIssueVM.TCIssueId);

                await LogChangesAsync(changes, tcIssueVM.TCIssueId == 0 ? "Tool Crib Issue Created" : "Tool Crib Issue Updated");

                await transaction.CommitAsync();

                var savedEntity = await _unitOfWork.ToolCribIssues.GetQueryable()
                    .Include(q => q.Store)
                    .Include(q => q.ToolCribIssueSubs)
                        .ThenInclude(s => s.Item)
                    .FirstOrDefaultAsync(q => q.TCIssueId == entity.TCIssueId);

                return _mapper.Map<ToolCribIssueVM>(savedEntity!);
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Tool Crib Issue: {tcIssueVM.TCIssueNo}");
                throw new InvalidOperationException("Failed to save Tool Crib Issue. All changes have been reverted.");
            }
        }

        public async Task UpdateTCIssueTallyStatusAsync(int tcIssueId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.ToolCribIssueSubs
                    .GetQueryable()
                    .Where(x => x.TCIssueId == tcIssueId )
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var TCIssue = await _unitOfWork.ToolCribIssues.GetAsync(tcIssueId);
                if (TCIssue == null)
                    return;

                TCIssue.TCIssueTally = (totalBalQty == 0);

                await _unitOfWork.ToolCribIssues.UpdateAsync(TCIssue);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateTCIssueTallyStatusAsync] Error updating TcissueId {tcIssueId}");
                throw new InvalidOperationException("Failed to update TCIssueTally status. Please contact support.");
            }
        }

        private async Task HandleChildUpdatesAsync(ToolCribIssue existingTCIssue, List<ToolCribIssueSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingTCIssue.ToolCribIssueSubs.Select(s => s.TCIssueSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.TCIssueSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingTCIssue.ToolCribIssueSubs.Where(s => !incomingSubIds.Contains(s.TCIssueSubId)).ToList())
            {
                await DeleteStockIssueAndTrackAsync(sub.TCIssueSubId, sub.ItemId.Value, screenCode);

                changes.AppendLine($"Child Deleted - TCIssueSubId: {sub.TCIssueSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ToolCribIssueSubs.DeleteAsync(sub.TCIssueSubId);
                await _unitOfWork.SaveAsync();
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.TCIssueSubId == 0)
                {
                    var newSub = _mapper.Map<ToolCribIssueSub>(subVM);
                    newSub.TCIssueId = existingTCIssue.TCIssueId;
                    await _unitOfWork.ToolCribIssueSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode},Issue Qty: {subVM.QtyOut}");

                    await _stockManagerService.IssueOrUpdateStockAsync(newSub.ItemId.Value, existingTCIssue.StoreId.Value, newSub.QtyOut,
                        newSub.UnitPrice, null, screenCode, newSub.TCIssueSubId, existingTCIssue.TCIssueNo, existingTCIssue.TCIssueDate);

                }
                else
                {
                    var existingSub = existingTCIssue.ToolCribIssueSubs.FirstOrDefault(s => s.TCIssueSubId == subVM.TCIssueSubId);
                    if (existingSub != null)
                    {
                        await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId.Value, existingTCIssue.StoreId.Value, subVM.QtyOut.GetValueOrDefault(),
                        subVM.UnitPrice.GetValueOrDefault(), null, screenCode, subVM.TCIssueSubId, existingTCIssue.TCIssueNo, existingTCIssue.TCIssueDate);

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

        public async Task DeleteAndResequenceAsync(ToolCribIssueSubVM subitem, ToolCribIssueVM tcIssueVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.TCIssueSubId > 0)
                {
                    var existingTCIssueSubItem = await _unitOfWork.ToolCribIssueSubs.GetAsync(subitem.TCIssueSubId);
                    if (existingTCIssueSubItem == null)
                        throw new InvalidOperationException("Sub item not found.");

                    await DeleteStockIssueAndTrackAsync(existingTCIssueSubItem.TCIssueSubId, existingTCIssueSubItem.ItemId.Value, screenCode);

                    await _unitOfWork.ToolCribIssueSubs.DeleteAsync(existingTCIssueSubItem);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Tool Crib Issue",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"TCIssue No: {tcIssueVM?.TCIssueNo}"
                    );
                }
                else
                {
                    tcIssueVM.ToolCribIssueSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.ToolCribIssueSubs
                    .GetQueryable()
                    .Where(x => x.TCIssueId == tcIssueVM.TCIssueId)
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

        public async Task<List<string>> GetAllExistingUserNamesinTCIssueAsync()
        {
            var result = await _unitOfWork.ToolCribIssues.GetQueryable()
                         .Select(m => m.IssuedToWhom)
                         .Distinct()
                         .ToListAsync();
            return result;
        }

        public async Task<List<ToolCribIssueSubVM>> GetToolCribIssueSubByTCIdAsync(int tcId)
        {
            try
            {
                var subs = await _unitOfWork.ToolCribIssueSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    //.Include(s => s.CostCenter)
                    .Where(s => s.TCIssueId == tcId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ToolCribIssueSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Inventory ToolCribIssue items for TCIssueId: {tcId}");
                throw new InvalidOperationException("Failed to retrieve Tool Crib Issue sub-items. Please try again.");
            }
        }

        public async Task<string> GetTCNumberAsync(string suffix)
        {
            var lastIssueNo = await _unitOfWork.ToolCribIssues
                .GetQueryable()
                .Where(d => d.Suffix == suffix)
                .OrderByDescending(d => d.TCIssueNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastIssueNo != null)
            {
                var parts = lastIssueNo.TCIssueNo.Split('/');
                if (int.TryParse(parts[0], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{nextNumber}";
        }

        public async Task<List<ToolCribIssueStatusVM>> GetToolCribIssueStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<ToolCribIssueStatusVM>("Sp_GetToolCribIssueNoteStatusList", status);
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
    }

}
