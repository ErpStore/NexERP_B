using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.PlanningService
{
    public class RcReleaseService : IRcReleaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public RcReleaseService(
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

        #region Common Service Methods

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetCustomerByIdAsync(custId);

        public async Task<List<CustomerVM>> GetAllActiveCustomersAsync()
               => await _commonService.GetAllActiveCustomersAsync();

        #endregion


        public async Task<decimal> GetPendingRcBalQtyByPoSubIdAsync(int poSubId)
        {
            if (poSubId <= 0)
                return 0;

            var rcBalQty = await _unitOfWork.MfgPoSubs
                .GetQueryable()
                .Where(x => x.PoSubId == poSubId)
                .Select(x => x.RouteCardBalQty)
                .FirstOrDefaultAsync();

            return rcBalQty;
        }

        public async Task<decimal> GetExactRcReleaseQtyByRcReleaseIdAsync(int rcReleaseId)
        {
            try
            {
                var qty = await _unitOfWork.RcReleases
                    .GetQueryable()
                    .Where(x => x.RouteCardReleaseId == rcReleaseId)
                    .Select(x => (decimal?)x.ReleaseQty)
                    .SumAsync();

                return qty ?? 0;
            }
            catch (Exception ex)
            {
                throw new Exception("Error while fetching RC Release Qty", ex);
            }
        }
        public async Task<List<RouteCardReleaseSub>> GetRcReleaseSubByRcReleaseIdAsync(int RcReleaseId)
        {
            try
            {
                var subs = await _unitOfWork.RcReleaseSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.RouteCardReleaseId == RcReleaseId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Route Card Release Sub items for RcReleaseId: {RcReleaseId}");
                throw new InvalidOperationException("Failed to retrieve Route Card Release sub-items. Please try again.");
            }
        }


        public async Task<bool> IsRcReleaseTransactionsMatchedAsync(int RcreleaseId, RouteCardReleaseVM rcReleaseVMs)
        {
            try
            {

                // Check Route Card Transaction references
                bool hasTransactions = await _unitOfWork.RouteCards
                    .GetQueryable()
                    .AnyAsync(pqs =>
                        pqs.RefRcReleseId.HasValue &&
                        pqs.RefRcReleseId == rcReleaseVMs.RouteCardReleaseId);


                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = rcReleaseVMs?.RouteCardReleaseSubs;
                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.RequiredQty);
                    decimal totalBalQty = list.Sum(x => x.RemainingQty);

                    qtyMismatch = totalQty != totalBalQty;
                }

                // If either transactions exist OR quantity mismatch → return true
                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while checking transactions for RcReleaseId: {RcreleaseId}");
                throw new InvalidOperationException("Failed to verify Route Card Release transactions.", ex);
            }
        }


        public async Task UpdatedCancelStatusAndAddOrRevertQty(RouteCardReleaseVM RcReleaseVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingRc = await _unitOfWork.RcReleases.GetAsync(RcReleaseVM.RouteCardReleaseId);

                if (existingRc == null)
                    throw new InvalidOperationException("RC Release not found.");

                var poRcBalQty = await GetPendingRcBalQtyByPoSubIdAsync(existingRc.PoSubId.Value);

                if (existingRc.ReleaseQty > poRcBalQty)
                {
                    throw new InvalidOperationException($"Released Qty ({existingRc.ReleaseQty}) cannot be greater than PO RouteCard Balance Qty ({poRcBalQty}).");
                }

                existingRc.IsCancel = RcReleaseVM.IsCancel;
                existingRc.CancelDate = RcReleaseVM.CancelDate;
                existingRc.CancelReason = RcReleaseVM.CancelReason;
                existingRc.CancelBy = RcReleaseVM.CancelBy;

                await _unitOfWork.RcReleases.UpdateAsync(existingRc);
                await _unitOfWork.SaveAsync();


                if (existingRc.IsCancel)
                {
                    if (existingRc.PoSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustRouteCardBalanceAsync(existingRc.PoSubId, existingRc.ReleaseQty, 0, $"Route Card Release Cancelled - {existingRc.RcReleaseNo}");
                    }
                }
                else
                {
                    if (existingRc.PoSubId.GetValueOrDefault() > 0)
                    {
                        await AdjustRouteCardBalanceAsync(existingRc.PoSubId.Value, 0, existingRc.ReleaseQty, $"Route card Release Reverted - {existingRc.RcReleaseNo}");
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

        public async Task UpsertRcReleaseShortCloseAsync(RouteCardReleaseVM rcRelaseVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingRcRelase = await _unitOfWork.RcReleases.GetAsync(rcRelaseVM.RouteCardReleaseId);
                if (existingRcRelase == null)
                    throw new InvalidOperationException("Route Card Relase not found.");

                existingRcRelase.ShortClose = rcRelaseVM.ShortClose;

                await _unitOfWork.RcReleases.UpdateAsync(existingRcRelase);
                await _unitOfWork.SaveAsync();

                await UpdateRcReleaseTallyStatusAsync(rcRelaseVM.RouteCardReleaseId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSalesQuoteShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertSalesQuoteShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }



        public async Task<RouteCardReleaseVM?> GetRcReleaseByRcIdAsync(int rcReleaseId)
        {
            try
            {
                var entity = await _unitOfWork.RcReleases.GetQueryable()
                    .Include(q => q.RouteCardReleaseSubs)
                    .Include(q => q.MfgPo)
                    .Include(q => q.RouteCardReleaseSubs).ThenInclude(s => s.Item)
                    .Include(q => q.RouteCardReleaseSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.Customer)
                    .Include(q => q.Assembly)
                    .FirstOrDefaultAsync(q => q.RouteCardReleaseId == rcReleaseId);

                return _mapper.Map<RouteCardReleaseVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetRcReleaseByRcIdAsync({rcReleaseId})");
                return null;
            }
        }


        public async Task<List<MfgPoVM>> GetPendingPOsByCustomerAsync(int custId)
        {
            try
            {
                return await _unitOfWork.MfgPos.GetQueryable()
                   .Include(p => p.MfgPoSubs)
                   .Where(p => p.CustId == custId && !p.PoTally && !p.PoCancl
                            && p.MfgPoSubs.Any(ps => ps.RouteCardBalQty > 0 && !ps.ItemCancel))
                   .Select(p => new MfgPoVM
                   {
                       PoId = p.PoId,
                       PONo = $"{p.PONo}{p.Suffix}",
                   })
                   .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to GetPendingPOsByCustomerAsync");
                throw;
            }
        }
        public async Task<List<MfgPoSubVM>> GetPendingPoItemsAsync(int poId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs
                    .GetQueryable()
                    .Where(x => x.PoId == poId
                             && x.RouteCardBalQty > 0
                             && !x.ItemCancel
                             && x.Item != null
                             && (x.Item.CategoryCode == 3 || x.Item.CategoryCode == 7))
                    .Select(x => new MfgPoSubVM
                    {
                        PoSubId = x.PoSubId,
                        PoId = x.PoId,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.ItemName,
                        Qty = x.Qty,
                        RouteCardBalQty = x.RouteCardBalQty
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in GetPendingPoItemsAsync. PoId: {poId}");
                return new List<MfgPoSubVM>();
            }
        }


        public async Task<List<RouteCardReleaseSubVM>> GetAssemblyComponentsForReleaseAsync(int assemblyId, decimal releaseQty)
        {
            var result = new List<RouteCardReleaseSubVM>();

            var bomHierarchy = await LoadBomHierarchyAsync(assemblyId);

            int slNo = 1; // Auto increment counter

            foreach (var comp in bomHierarchy)
            {
                bool isSubOrAssembly = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .AnyAsync(x => x.ItemId == comp.ItemID && (x.CategoryCode != 2 ));

                if (isSubOrAssembly)
                    continue;

                var item = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.ItemId == comp.ItemID);

                decimal requiredQty = Math.Round(releaseQty * comp.CumulativeUtilQty, 3);

                result.Add(new RouteCardReleaseSubVM
                {
                    SlNo = slNo++, // Auto Increment Here
                    ComponentItemId = comp.ItemID,
                    ComponentItemCode = item?.ItemCode,
                    ComponentItemName = item?.ItemName,
                    ComponentItemUOM = item?.MeasureUnit,
                    CumulativeUtilQty = comp.CumulativeUtilQty,
                    RequiredQty = requiredQty,
                    IssuedQty = 0,
                    RemainingQty = requiredQty,
                    IsCompleted = false
                });
            }
            return result;
        }

        public async Task<RouteCardReleaseVM> UpsertRouteCardReleaseAsync(RouteCardReleaseVM releaseVM)
        {
            if (releaseVM == null)
                throw new ArgumentNullException(nameof(releaseVM));

            if (releaseVM.RouteCardReleaseSubs == null || !releaseVM.RouteCardReleaseSubs.Any())
                throw new InvalidOperationException("At least one component is required.");

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                RouteCardRelease entity;

                if (releaseVM.RouteCardReleaseId == 0)
                {
                    entity = _mapper.Map<RouteCardRelease>(releaseVM);

                    // Correct repository for Release number
                    var nextNumber = await _unitOfWork.RcReleases.GetLastRcReleaseNoAsync(entity.Suffix);

                    entity.RcReleaseNo = nextNumber;
                    entity.ReleasedBy = currentUser;
                    entity.ReleasedDate = now;

                    entity.RouteCardReleaseSubs = releaseVM.RouteCardReleaseSubs
                                                  .Select(s => _mapper.Map<RouteCardReleaseSub>(s))
                                                  .ToList();

                    await _unitOfWork.RcReleases.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    if (entity.PoSubId > 0)
                        await AdjustRouteCardBalanceAsync(entity.PoSubId, 0, entity.ReleaseQty, "Route Card Release Creation");

                    changes.AppendLine("Route Card Release Created.");
                }
                else
                {
                    entity = await _unitOfWork.RcReleases
                        .GetQueryable()
                        .Include(q => q.RouteCardReleaseSubs)
                        .FirstOrDefaultAsync(q => q.RouteCardReleaseId == releaseVM.RouteCardReleaseId)
                        ?? throw new InvalidOperationException("Route Card Release not found.");

                    if (entity.PoSubId > 0)
                        await AdjustRouteCardBalanceAsync(entity.PoSubId, entity.ReleaseQty, releaseVM.ReleaseQty, "Route Card Release Creation");

                    var parentChanges = GetPropertyChanges(entity, releaseVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(releaseVM, entity);

                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, releaseVM.RouteCardReleaseSubs, changes);

                    changes.AppendLine("Route Card Release Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateRcReleaseTallyStatusAsync(entity.RouteCardReleaseId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, releaseVM.RouteCardReleaseId == 0 ? "Route Card Release Created" : "Route Card Release Updated");

                var savedEntity = await _unitOfWork.RcReleases
                    .GetQueryable()
                    .Include(q => q.RouteCardReleaseSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .FirstOrDefaultAsync(q => q.RouteCardReleaseId == entity.RouteCardReleaseId);

                return _mapper.Map<RouteCardReleaseVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Route Card Release: {releaseVM.RcReleaseNo}");
                throw new InvalidOperationException("Failed to save Route Card Release. Please try again.");
            }
        }

        private async Task HandleChildUpdatesAsync(RouteCardRelease existingRcRelease, List<RouteCardReleaseSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {
                var existingSubs = existingRcRelease.RouteCardReleaseSubs.ToList();

                // ---------------- DELETE ----------------

                foreach (var sub in existingSubs
                    .Where(s => !incomingSubVMs.Any(i => i.RouteCardReleaseSubId == s.RouteCardReleaseSubId)))
                {
                    changes.AppendLine($"Child Deleted - Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.RcReleaseSubs.DeleteAsync(sub.RouteCardReleaseSubId);
                    await _unitOfWork.SaveAsync();
                }

                // ---------------- ADD / UPDATE ----------------
                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.RouteCardReleaseSubId == 0)
                    {
                        var newSub = _mapper.Map<RouteCardReleaseSub>(subVM);
                        newSub.RouteCardReleaseId = existingRcRelease.RouteCardReleaseId;
                        await _unitOfWork.RcReleaseSubs.CreateAsync(newSub);
                        changes.AppendLine($"Child Added - ItemCode: {subVM.ComponentItemCode}, Qty: {subVM.RequiredQty}");
                    }
                    else
                    {
                        var existingSub = existingSubs.FirstOrDefault(s => s.RouteCardReleaseSubId == subVM.RouteCardReleaseSubId);

                        if (existingSub != null)
                        {
                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - {subVM.ComponentItemCode}:\n{subChanges}");

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "[HandleChildUpdatesAsync - Route Card Release]");
                throw new InvalidOperationException("Failed to update Route Card Release details.");
            }
        }



        private async Task AdjustRouteCardBalanceAsync(int? poSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!poSubId.HasValue || poSubId == 0) return;

                var poSub = await _unitOfWork.MfgPoSubs.GetAsync(poSubId.Value);
                if (poSub == null) return;

                if (oldQty > 0)
                    poSub.RouteCardBalQty += oldQty;

                if (newQty > poSub.RouteCardBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Sales Order RC BalQty.");

                if (newQty > 0)
                    poSub.RouteCardBalQty -= newQty;

                await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();

            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustRouteCardBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustRouteCardBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Route Card Balance. Please contact support.");
            }
        }

        public async Task UpdateRcReleaseTallyStatusAsync(int rcReleaseId)
        {
            try
            {
                decimal totalRemainingQty = await _unitOfWork.RcReleaseSubs
                    .GetQueryable()
                    .Where(x => x.RouteCardReleaseId == rcReleaseId)
                    .SumAsync(x => (decimal?)x.RemainingQty) ?? 0;

                var rcRelease = await _unitOfWork.RcReleases.GetAsync(rcReleaseId);
                if (rcRelease == null)
                    return;

                rcRelease.RcReleaseTally = (totalRemainingQty == 0);

                await _unitOfWork.RcReleases.UpdateAsync(rcRelease);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateRcReleaseTallyStatusAsync] Error updating RcReleaseId {rcReleaseId}");
                throw new InvalidOperationException("Failed to update Rc Release Tally status. Please contact support.");
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
                screen: "Route Card Release",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        private async Task<List<BOMItem>> LoadBomHierarchyAsync(int assemblyId)
        {
            var result = new List<BOMItem>();
            await LoadRecursive(assemblyId, result);
            return result;
        }

        private async Task LoadRecursive(int assemblyId, List<BOMItem> result, decimal parentUtil = 1)
        {
            var children = await _unitOfWork.AssmblyDefs
                .GetQueryable()
                .Where(x => x.AssmblyID == assemblyId)
                .ToListAsync();

            foreach (var child in children)
            {
                decimal util = Math.Round(child.UtilQty, 3);
                decimal cumUtil = Math.Round(parentUtil * util, 3);

                var bomItem = new BOMItem
                {
                    ItemID = child.ItemId,
                    UtilQty = util,
                    CumulativeUtilQty = cumUtil
                };

                result.Add(bomItem);

                bool isSubAssembly = await _unitOfWork.ItemRepositories
                    .GetQueryable()
                    .AnyAsync(i => i.ItemId == child.ItemId && (i.CategoryCode == 3 || i.CategoryCode == 7));

                if (isSubAssembly)
                {
                    await LoadRecursive(child.ItemId, result, cumUtil);
                }
            }
        }


        public async Task<(List<RouteCardReleaseVM> rcReleaseVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.RcReleases.GetQueryable()
                    .Include(j => j.MfgPo)
                    .Include(j => j.Assembly)
                    .AsQueryable().AsNoTracking();

                // Apply Dynamic Filters
                if (filters != null)
                {
                    foreach (var f in filters)
                    {
                        query = RcReleaseFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                    }
                }

                var total = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.RouteCardReleaseId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // AutoMapper mapping
                var vmList = _mapper.Map<List<RouteCardReleaseVM>>(list);

                return (vmList, total);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"SearchWithDynamicFilterAsync on Route card Release Load");
                return (new List<RouteCardReleaseVM>(), 0);
            }
        }

        public static class RcReleaseFilterBuilder
        {
            public static IQueryable<RouteCardRelease> ApplyFilter(
                IQueryable<RouteCardRelease> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "RcReleaseNo":
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
                                (string.IsNullOrEmpty(part1) || x.RcReleaseNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "RefPoNo":
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
                                (string.IsNullOrEmpty(part1) || x.MfgPo.PONo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.MfgPo.Suffix.Contains(part2))
                            );
                        }


                        //case "CustName":
                        //    return query.Where(x => x.Customer.CustName.Contains(val));

                        //case "AssyCode":
                        //    return query.Where(x => x.AssemblyItem.ItemCode.Contains(val));

                        //case "SubAssyCode":
                        //    return query.Where(x => x.SubAssemblyItem.ItemCode.Contains(val));

                        //case "SubAssyCode2":
                        //    return query.Where(x => x.SubAssemblyItem2.ItemCode.Contains(val));

                        //case "SubAssyCode3":
                        //    return query.Where(x => x.SubAssemblyItem3.ItemCode.Contains(val));

                        //case "CompItemCode":
                        //    return query.Where(x => x.ComponentItem.ItemCode.Contains(val));

                        //case "ItemName":
                        //    return query.Where(x => x.ComponentItem.ItemName.Contains(val));

                        //case "RMCode":
                        //    return query.Where(x => x.ComponentItem.ItemCode.Contains(val));

                        //case "RMName":
                        //    return query.Where(x => x.ComponentItem.ItemName.Contains(val));

                        //case "CreatedBy":
                        //    return query.Where(x => x.CreatedBy.Contains(val));

                        //case "FromDate":
                        //    if (DateTime.TryParse(val, out var fromDate))
                        //        return query.Where(x => x.RCDateNow >= fromDate);
                        //    break;

                        //case "ToDate":
                        //    if (DateTime.TryParse(val, out var toDate))
                        //        return query.Where(x => x.RCDateNow <= toDate);
                        //    break;

                        //case "Status":
                        //    return ApplyStatusFilter(query, val);

                        //case "RCStatus":
                        //    return ApplyRCStatusFilter(query, val);
                }

                return query;
            }

            //private static IQueryable<RouteCard> ApplyStatusFilter(
            //    IQueryable<RouteCard> query, string status)
            //{
            //    return status switch
            //    {
            //        "Pending" => query.Where(x => x.RcStatus == 0),
            //        "Work-In-Progress" => query.Where(x => x.RcStatus == 1),
            //        "Completed" => query.Where(x => x.RcStatus == 2),
            //        "Cancelled" => query.Where(x => x.RcStatus == 3),
            //        _ => query
            //    };
            //}

            //private static IQueryable<RouteCard> ApplyRCStatusFilter(
            //    IQueryable<RouteCard> query, string status)
            //{
            //    return status switch
            //    {
            //        "Manual" => query.Where(x => x.RcStatus == 1),
            //        "Auto" => query.Where(x => x.RcStatus == 0),
            //        _ => query
            //    };
            //}
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteRcReleaseAsync(int rcReleaseId)
        {
            try
            {
                var rcRelease = await _unitOfWork.RcReleases.GetAsync(rcReleaseId);

                bool hasRouteCard = await _unitOfWork.RouteCards
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefRcReleseId.HasValue &&
                        qs.RefRcReleseId == rcReleaseId);

                if (hasRouteCard)
                    return (false, "Cannot delete this Route Card Release as a Route Card transaction exists.");

                if (rcRelease.IsCancel || rcRelease.ShortClose)
                    return (false, "Cannot delete this Route Card Release as it is Cancelled or Short Closed.");

                return (true, "Route Card Release can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteRcReleaseAsync for RcReleaseId: {rcReleaseId}");
                throw new Exception("Error checking Route card release delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteRouteCardReleaseByRcReleaseIdAsync(int rcReleaseId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var routeCard = await _unitOfWork.RcReleases
                    .GetQueryable()
                    .Where(e => e.RouteCardReleaseId == rcReleaseId).FirstOrDefaultAsync();

                if (routeCard == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                if (routeCard.PoSubId > 0)
                {
                    await AdjustRouteCardBalanceAsync(routeCard.PoSubId, routeCard.ReleaseQty, 0, "Route Card Release Delete");
                }

                var subs = await _unitOfWork.RcReleaseSubs
                            .GetQueryable()
                            .Where(x => x.RouteCardReleaseId == rcReleaseId)
                            .ToListAsync();

                if (subs.Any())
                {
                    await _unitOfWork.RcReleaseSubs.DeleteRangeAsync(subs);
                }

                await _unitOfWork.RcReleases.DeleteAsync(routeCard);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Route Card Release",
                    action: $"Deleted  Rc Release No: {routeCard.RcReleaseNo}",
                    additionalInfo: $"RouteCard Release Id: {routeCard.RouteCardReleaseId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Route Card Release: {rcReleaseId}");
                throw;
            }
        }

        public async Task<string> GetRcReleaseNoAsync(string suffix)
        {
            try
            {
                var lastRcRelease = await _unitOfWork.RcReleases
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.RcReleaseNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastRcRelease != null)
                {
                    var parts = lastRcRelease.RcReleaseNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Route Card Release number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate route card Release number.");
            }
        }

    }

}
