using AutoMapper;
using V.SMART.Shared.Utility_Constants;
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
using System.Text;
using V.SMART.Shared.ViewModels.ReportViewModel.ToolCribReturnStatusViewModel;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.InventoryService
{
    public class ToolCribReturnService : IToolCribReturnService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;
        private readonly IStockManagerService _stockManagerService;
        public ToolCribReturnService(
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

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);
        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);
        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();


        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        public async Task<ToolCribReturnVM?> GetToolCribReturnByDcIdAsync(int tcReturnId)
        {
            try
            {
                var entity = await _unitOfWork.ToolCribReturns.GetQueryable()
                            .Include(q => q.ToolCribReturnSubs)
                            .ThenInclude(s => s.Item)
                            .Include(q => q.ToolCribReturnSubs)
                            .ThenInclude(s => s.ToolCribIssueSub)
                            .Include(q => q.ToolCribReturnSubs)
                            .ThenInclude(s => s.ToolCribIssueSub.ToolCribIssue)
                            .FirstOrDefaultAsync(q => q.TCReturnId == tcReturnId);
                return _mapper.Map<ToolCribReturnVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetToolCribReturnByDcIdAsync({tcReturnId})");
                return null;
            }
        }


        public async Task<decimal> GetTCIssueItemBalQtyFromTCIssueSubId(int tcIssueSubId)
        {
            try
            {
                return await _unitOfWork.ToolCribIssueSubs.GetQueryable()
                    .Where(e => e.TCIssueSubId == tcIssueSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for TCIssueSubId: {tcIssueSubId}");
                throw new InvalidOperationException("Failed to retrieve Tool Crib Issue balance quantity.");
            }
        }

        public async Task<ToolCribReturnSubVM?> GetTCReturnSubItemDetailByTCReturnSubIdAsync(int tcReturnSubId)
        {
            try
            {
                return await _unitOfWork.ToolCribReturnSubs
                    .GetQueryable()
                    .Where(q => q.TCReturnSubId == tcReturnSubId)
                    .Select(q => new ToolCribReturnSubVM
                    {
                        AccpQty = q.AccpQty,
                        RejQty = q.RejQty,
                        RewQty = q.RewQty,
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ToolCrib Return sub item detail for TCReturnSubId: {tcReturnSubId}");
                throw new InvalidOperationException("Failed to retrieve ToolCrib Return sub-item details.");
            }
        }

        public async Task<ToolCribReturnVM> UpsertToolCribReturnAsync(ToolCribReturnVM tcReturnVM, int screenCode)
        {
            if (tcReturnVM == null)
                throw new ArgumentNullException(nameof(tcReturnVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            try
            {
                ToolCribReturn entity;

                if (tcReturnVM.TCReturnId == 0)
                {
                    entity = _mapper.Map<ToolCribReturn>(tcReturnVM);

                    var NextNumber = await _unitOfWork.ToolCribReturns.GetLastReturnNoAsync(entity.Suffix);
                    entity.TCReturnNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.ToolCribReturnSubs = tcReturnVM.ToolCribReturnSubVMs.Select(s => _mapper.Map<ToolCribReturnSub>(s)).ToList();

                    await _unitOfWork.ToolCribReturns.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.ToolCribReturnSubs)
                    {
                        if (sub.RefTCIssueSubId > 0)
                        {
                            var totQty = sub.AccpQty + sub.RejQty + sub.RewQty;

                            await AdjustToolCribIssueBalanceAsync(sub.RefTCIssueSubId,0,totQty,"Tool Crib return Creation");
                        }

                        await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, entity.StoreId.Value, sub.AccpQty,
                            1, null, screenCode, sub.TCReturnSubId, entity.TCReturnNo, entity.TCReturnDate, sub.Remark);

                        if(sub.RejQty >0)
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, StoreIds.RejectionStore, sub.RejQty,
                                1, null, screenCode, sub.TCReturnSubId, entity.TCReturnNo, entity.TCReturnDate,sub.RejRemark);
                        }

                        if(sub.RewQty >0)
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, StoreIds.ReworkStore, sub.RewQty,
                                1, null, screenCode, sub.TCReturnSubId, entity.TCReturnNo, entity.TCReturnDate,sub.RewRemark);
                        }
                    }

                    changes.AppendLine("Tool-Crib Return Created.");
                }
                else
                {
                    entity = await _unitOfWork.ToolCribReturns.GetQueryable()
                        .Include(q => q.ToolCribReturnSubs)
                        .FirstOrDefaultAsync(q => q.TCReturnId == tcReturnVM.TCReturnId)
                        ?? throw new InvalidOperationException("Tool Crib Return not found.");

                    var parentChanges = GetPropertyChanges(entity, tcReturnVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(tcReturnVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, tcReturnVM.ToolCribReturnSubVMs, changes, screenCode);

                    changes.AppendLine("Tool-Crib Return Updated.");
                }

                await _unitOfWork.SaveAsync();

                await LogChangesAsync(changes, tcReturnVM.TCReturnId == 0 ? "Tool Crib Return Created" : "Tool Crib Return Updated");

                var savedEntity = await _unitOfWork.ToolCribReturns.GetQueryable()
                    .Include(q => q.ToolCribReturnSubs).ThenInclude(s => s.Item)
                    .Include (q => q.Store)
                    .FirstOrDefaultAsync(q => q.TCReturnId == entity.TCReturnId);

                return _mapper.Map<ToolCribReturnVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to upsert Tool Crib Return: {tcReturnVM.TCReturnNo}");
                throw new InvalidOperationException("Failed to save Tool Crib Return. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(ToolCribReturn existingtcr, List<ToolCribReturnSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            var existingSubIds = existingtcr.ToolCribReturnSubs.Select(s => s.TCReturnSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.TCReturnSubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingtcr.ToolCribReturnSubs.Where(s => !incomingSubIds.Contains(s.TCReturnSubId)).ToList())
            {
                if (sub.RefTCIssueSubId > 0)
                {
                    var totQty = sub.AccpQty + sub.RejQty + sub.RewQty;

                    await AdjustToolCribIssueBalanceAsync(sub.RefTCIssueSubId,totQty,0,"Tool Crib return Delete");
                }

                await DeleteStockAddAsync(sub.TCReturnSubId, sub.ItemId.Value, screenCode);

                changes.AppendLine($"Child Deleted - TCReturnSubId: {sub.TCReturnSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.ToolCribReturnSubs.DeleteAsync(sub.TCReturnSubId);
                await _unitOfWork.SaveAsync();
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.TCReturnSubId == 0)
                {
                    var newSub = _mapper.Map<ToolCribReturnSub>(subVM);
                    newSub.TCReturnId = existingtcr.TCReturnId;
                    await _unitOfWork.ToolCribReturnSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.AccpQty}");

                    if (subVM.RefTCIssueSubId > 0)
                    {
                        var totQty = subVM.AccpQty.GetValueOrDefault() + subVM.RejQty.GetValueOrDefault() + subVM.RewQty.GetValueOrDefault();

                        await AdjustToolCribIssueBalanceAsync(
                            subVM.RefTCIssueSubId,
                            0,
                            totQty,
                            "Tool Crib return Creation"
                        );
                    }

                    await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingtcr.StoreId.Value, newSub.AccpQty,
                            1, null, screenCode, newSub.TCReturnSubId, existingtcr.TCReturnNo, existingtcr.TCReturnDate,subVM.Remark);

                    if (subVM.RejQty > 0)
                    {
                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.RejectionStore, newSub.RejQty,
                            1, null, screenCode, newSub.TCReturnSubId, existingtcr.TCReturnNo, existingtcr.TCReturnDate,subVM.RejRemark);
                    }

                    if (subVM.RewQty > 0)
                    {
                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.ReworkStore, newSub.RewQty,
                            1, null, screenCode, newSub.TCReturnSubId, existingtcr.TCReturnNo, existingtcr.TCReturnDate,subVM.RewRemark);
                    }

                }
                else
                {
                    var existingSub = existingtcr.ToolCribReturnSubs.FirstOrDefault(s => s.TCReturnSubId == subVM.TCReturnSubId);
                    if (existingSub != null)
                    {

                        if (subVM.RefTCIssueSubId > 0)
                        {
                            var totQty = subVM.AccpQty.GetValueOrDefault() + subVM.RejQty.GetValueOrDefault() + subVM.RewQty.GetValueOrDefault();

                            await AdjustToolCribIssueBalanceAsync(subVM.RefTCIssueSubId,existingSub.AccpQty + existingSub.RejQty + existingSub.RewQty,totQty,"Tool Crib return Update");
                        }
                        await DeleteStockAddAsync(subVM.TCReturnSubId, subVM.ItemId.Value, screenCode);

                        await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, existingtcr.StoreId.Value , subVM.AccpQty.GetValueOrDefault(),
                           1, null, screenCode, subVM.TCReturnSubId, existingtcr.TCReturnNo, existingtcr.TCReturnDate,subVM.Remark);

                        if (subVM.RejQty > 0)
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.RejectionStore, subVM.RejQty.GetValueOrDefault(),
                                1, null, screenCode, subVM.TCReturnSubId, existingtcr.TCReturnNo, existingtcr.TCReturnDate,subVM.RejRemark);
                        }

                        if (subVM.RewQty > 0)
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(subVM.ItemId.Value, StoreIds.ReworkStore, subVM.RewQty.GetValueOrDefault(),
                                1, null, screenCode, subVM.TCReturnSubId, existingtcr.TCReturnNo, existingtcr.TCReturnDate,subVM.RewRemark);
                        }

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }


        private async Task DeleteStockAddAsync(int tcReturnSubId, int itemId, int screenCode)
        {
            var addIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == tcReturnSubId
                            && s.ItemId == itemId
                            && s.ScreenCode == screenCode)
                .Select(s => s.AddId)
                .ToListAsync();

            if (addIds?.Any() != true)
                return;

            foreach (var addId in addIds.Where(x => x > 0))
                await _stockManagerService.DeleteStockAddAsync(addId);

            await _unitOfWork.SaveAsync();
        }



        private async Task AdjustToolCribIssueBalanceAsync(int? refTciSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refTciSubId.HasValue || refTciSubId == 0) return;

                var issueSub = await _unitOfWork.ToolCribIssueSubs.GetAsync(refTciSubId.Value);
                if (issueSub == null) return;

                if (oldQty > 0)
                    issueSub.BalQty += oldQty;

                if (newQty > issueSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Tool Crib Issue BalQty.");

                if (newQty > 0)
                    issueSub.BalQty -= newQty;

                await _unitOfWork.ToolCribIssueSubs.UpdateAsync(issueSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.ToolCribIssueSubs
                    .GetQueryable()
                    .Where(e => e.TCIssueId == issueSub.TCIssueId)
                    .SumAsync(e => e.BalQty);

                var Issue = await _unitOfWork.ToolCribIssues.GetAsync(issueSub.TCIssueId);
                if (Issue != null)
                {
                    Issue.TCIssueTally = (totalBalQty == 0);
                    await _unitOfWork.ToolCribIssues.UpdateAsync(Issue);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustToolCribIssueBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustToolCribIssueBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Issue Balance. Please contact support.");
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
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
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


        public async Task DeleteAndResequenceAsync(ToolCribReturnSubVM subitem, ToolCribReturnVM tcrVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.TCReturnSubId > 0)
                {
                    var entity = await _unitOfWork.ToolCribReturnSubs.GetAsync(subitem.TCReturnSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.RefTCIssueSubId > 0)
                    {
                        var TotQty = entity.AccpQty + entity.RejQty + entity.RewQty;
                        await AdjustToolCribIssueBalanceAsync(subitem.RefTCIssueSubId, TotQty, 0, "ToolCribReturn Deletion");
                    }

                    await DeleteStockAddAsync(subitem.TCReturnSubId, subitem.ItemId.Value, screenCode);

                    await _unitOfWork.ToolCribReturnSubs.DeleteAsync(entity.TCReturnSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Inv ToolCribReturn",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"ToolCribReturn No: {tcrVM?.TCReturnNo}"
                    );
                }
                else
                {
                    tcrVM.ToolCribReturnSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.ToolCribReturnSubs
                    .GetQueryable()
                    .Where(x => x.TCReturnId == tcrVM.TCReturnId)
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
       

        public async Task<(bool CanDelete, string Message)> CanDeleteToolCribReturnAsync(int tcReturnId, int screenCode)
        {
            try
            {
                var scnGenSubIds = await _unitOfWork.ToolCribReturnSubs
                    .GetQueryable()
                    .Where(s => s.TCReturnId == tcReturnId)
                    .Select(s => s.TCReturnSubId)
                    .ToListAsync();

                if (!scnGenSubIds.Any())
                    return (true, "Tool Crib Return can be safely deleted (no sub-items found).");

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa =>
                        scnGenSubIds.Contains(sa.SubItemRefID) &&
                        sa.ScreenCode == screenCode &&
                        sa.BalQty < sa.AddQty)
                    .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Tool Crib Return. Some sub-items have already been transacted/issued.");

                return (true, "Tool Crib Return can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteToolCribReturnAsync for TcReturnId: {tcReturnId}");
                throw new Exception("Error checking Tool Crib Return delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteToolCribReturnByDcIdAsync(int tcReturnId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var toolcribreturn = await _unitOfWork.ToolCribReturns
                    .GetQueryable()
                    .Include(e => e.ToolCribReturnSubs)
                    .FirstOrDefaultAsync(e => e.TCReturnId == tcReturnId);

                if (toolcribreturn == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in toolcribreturn.ToolCribReturnSubs)
                {
                    if (sub.RefTCIssueSubId > 0)
                    {
                        var TotalQty = sub.AccpQty + sub.RejQty + sub.RewQty;
                        await AdjustToolCribIssueBalanceAsync(sub.RefTCIssueSubId, TotalQty, 0, "ToolCribReturn Deletion");
                    }

                    await DeleteStockAddAsync(sub.TCReturnSubId, sub.ItemId.Value, screenCode);
                }

                var deleted = await _unitOfWork.ToolCribReturns.DeleteAsync(tcReturnId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "ToolCribReturn List",
                    action: $"Deleted ToolCribReturn: {toolcribreturn.TCReturnNo}",
                    additionalInfo: $"Tc Id: {toolcribreturn.TCReturnId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete ToolCribReturn: {tcReturnId}");
                throw;
            }
        }

        public async Task<List<ToolCribReturnSubVM>> GetToolCribReturnSubByDcIdAsync(int tcReturnId)
        {
            try
            {
                var subs = await _unitOfWork.ToolCribReturnSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.ToolCribIssueSub.ToolCribIssue)
                    .Where(s => s.TCReturnId == tcReturnId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<ToolCribReturnSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Inventory ToolCribReturn items for TCReturnId: {tcReturnId}");
                throw new InvalidOperationException("Failed to retrieve ToolCribReturn sub-items. Please try again.");
            }
        }

        public async Task<string> GetToolCribReturnNumberAsync(string suffix)
        {
            try
            {
                var lastReturn = await _unitOfWork.ToolCribReturns
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.TCReturnNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastReturn != null)
                {
                    var parts = lastReturn.TCReturnNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating toolcribreturn number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate toolcribreturn number.");
            }
        }


        public async Task<List<ToolCribReturnSubVM>> GetDistinctRefToolCribIssuesByDcIdAsync(int tcReturnId)
        {
            return await _unitOfWork.ToolCribReturns
                .GetQueryable()
                .Where(s => s.TCReturnId == tcReturnId)
                .GroupBy(s => new { s.TCReturnNo ,s.TCReturnDate})
                .Select(g => new ToolCribReturnSubVM
                {
                    RefTCIssueNo = g.Key.TCReturnNo,
                    RefTCIssueDate = g.Key.TCReturnDate
                })
                .ToListAsync();
        }

        public async Task<List<Dictionary<string, object>>> GetAllOpenToolCribIssuesAsync()
        {
            try
            {

                var result = await (from e in _unitOfWork.ToolCribIssues.GetQueryable()
                                    join es in _unitOfWork.ToolCribIssueSubs.GetQueryable()
                                        on e.TCIssueId equals es.TCIssueId
                                    where  !e.TCIssueTally && es.BalQty > 0 && e.IsReturn 
                                    select new
                                    {
                                        es.TCIssueSubId,
                                        e.TCIssueNo,
                                        e.Suffix,
                                        e.TCIssueDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.MeasureUnit,
                                        es.QtyOut,
                                        es.BalQty,
                                        es.UnitPrice,
                                        es.Remark
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["RefTCIssueSubId"] = r.TCIssueSubId,
                    ["TCIssueNo"] = $"{r.TCIssueNo}{r.Suffix}",
                    ["TCIssueDate"] = r.TCIssueDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,
                    ["Qty"] = r.QtyOut,
                    ["BalQty"] = r.BalQty,
                    ["Rate"] = r.UnitPrice,
                    ["Remarks"] = r.Remark
                }).ToList();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching open ToolCribIssues");
                throw new InvalidOperationException("Failed to retrieve open ToolCribIssues. Please try again.");
            }
        }

        public async Task<IEnumerable<Store?>> GetAllReturnStoresAsync()
        {
            try
            {
                return await _commonService.GetAllAddStoresAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching issue stores");
                return Enumerable.Empty<Store?>();
            }
        }


        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        public async Task<int> GetPendingIssueCountAsync()
        {
            return await _unitOfWork.ToolCribIssues
                .GetQueryable()
                .Where(e => e.TCIssueTally == false && e.IsReturn)
                .CountAsync();
        }
        public async Task<List<Staff>> GetAllStaffAsync()
           => await _commonService.GetAllStaffAsync();
        public async Task<(List<ToolCribReturnVM> returnVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.ToolCribReturns
                         .GetQueryable()
                         .AsSplitQuery()
                        .Include(e => e.ToolCribReturnSubs)
                             .ThenInclude(s => s.Item)
                         .Include(e => e.Store) 
                         .AsQueryable();

            string? status = null;
            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = ToolCribReturnFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.TCReturnId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<ToolCribReturnVM>>(list);

            return (vmList, total);
        }
        public static class ToolCribReturnFilterBuilder
        {
            public static IQueryable<ToolCribReturn> ApplyFilter(IQueryable<ToolCribReturn> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "ReturnNo":
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
                                (string.IsNullOrEmpty(part1) || x.TCReturnNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "FromWhom":
                        return query.Where(x => x.FromWhom.Contains(val));

                    case "ItemCode":
                        return query.Where(x => x.ToolCribReturnSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "StoreName":
                        return query.Where(x => x.Store.StoreName.Contains(val));

                    case "ItemName":
                        return query.Where(x => x.ToolCribReturnSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        return query.Where(x => x.TCReturnDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.TCReturnDate <= DateTime.Parse(value.ToString()));

                }

                return query;
            }

            
        }

        public async Task<IEnumerable<ToolCribIssue>> GetAllToolCribIssueNosAsync()
        {
            try
            {
                return await _unitOfWork.ToolCribIssues.GetQueryable()
                    .Where(s => s.IsReturn == true)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching issue stores");
                return Enumerable.Empty<ToolCribIssue>();
            }
        }


        public async Task<List<ToolCribReturnStatusVM>> GetToolCribReturnStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<ToolCribReturnStatusVM>("Sp_GetToolCribReturnNoteStatusList", status);
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
