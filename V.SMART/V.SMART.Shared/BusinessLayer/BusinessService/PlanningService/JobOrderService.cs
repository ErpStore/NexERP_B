using AutoMapper;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IPlanningService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.JobOrderStatusVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.PlanningService
{
    public class JobOrderService : IJobOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly IStockManagerService _stockManagerService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

       

        public JobOrderService(
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


        // 🔹 Customers

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }

        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetCustomerByIdAsync(custId);

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<IEnumerable<ItemVM>> SaerchAssyOrSubAssy(string searchText)
            => await _commonService.SearchAssyAndSubAssyItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        public async Task<decimal> GetAvailableStockByItemIdAsync(int itemId, int? storeId)
            => await _stockManagerService.GetStockForItemAsync(itemId, storeId);

        public async Task<bool> IsAssignJobOrderDepartmenWise()
            => await _commonService.GetScreenPermissionsAsync("Job Order", "Assign Job Order Department-wise");
        public async Task<bool> IsJobOrderBothAssemblyAndSubAssembly()
          => await _commonService.GetScreenPermissionsAsync("Job Order", "JobOrder Generate Assembly and SubAssembly");
        public async Task<bool> IsLabourMaterialValidateEnabledAsync()
           => await _commonService.GetScreenPermissionsAsync("BOM", "Customer Material Validation");



        // 🔹 Job-Order operations

        public async Task<(List<JobOrderVM> jobOrderVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber, int pageSize,Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.JobOrders.GetQueryable().AsSplitQuery()
                        .Include(j => j.Customer)
                        .Include(j => j.JobOrderSubs)
                            .ThenInclude(s => s.Item)
                        .Include(j => j.MfgPo)
                          .ThenInclude(j => j.MfgPoSubs)
                        .Include(j => j.AssyItem)
                        .Include(j => j.Staff)
                        .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = JobOrderFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.JobId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<JobOrderVM>>(list);

            return (vmList, total);
        }

        public static class JobOrderFilterBuilder
        {
            public static IQueryable<JobOrder> ApplyFilter(
                IQueryable<JobOrder> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "JobNo":
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

                            //return query.Where(x =>
                            //    (string.IsNullOrEmpty(part1) || x.JobNo.StartsWith(part1)) &&
                            //    (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            //);
                            return query.Where(x =>
                                        (
                                            // Full format: Dept/JobNo+Suffix
                                            (x.Staff != null &&
                                             x.Staff.DepartmentCode != null &&
                                             (x.Staff.DepartmentCode + "/" +
                                              x.JobNo + (x.Suffix ?? ""))
                                                .StartsWith(input))
                                        )
                                        ||
                                        (
                                            // JobNo + Suffix
                                            (x.JobNo + (x.Suffix ?? ""))
                                                .StartsWith(input)
                                        )
                                        ||
                                        (
                                            //  Only JobNo
                                            x.JobNo.StartsWith(input)
                                        )
                                        ||
                                        (
                                            //  Only Suffix
                                            (x.Suffix != null && x.Suffix.Contains(input))
                                        )
                                        ||
                                        (
                                            //  Only DepartmentCode
                                            (x.Staff != null &&
                                             x.Staff.DepartmentCode != null &&
                                             x.Staff.DepartmentCode.StartsWith(input))
                                        )
                                    );
                        }


                    case "CustomerName":
                        return query.Where(x => x.Customer.CustName.Contains(val));

                    case "AssemblyName":
                        return query.Where(x => x.AssyItem.ItemCode.Contains(val));

                    case "ItemName":
                        return query.Where(x => x.JobOrderSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "ItemCode":
                        return query.Where(x => x.JobOrderSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "MfgPo":
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

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.JobDate >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.JobDate <= toDate);
                        break;

                    case "Status":
                        return ApplyStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<JobOrder> ApplyStatusFilter(
                IQueryable<JobOrder> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.JobTally == true),
                    "Pending" => query.Where(x => x.JobTally == false && x.Cancel == false),
                    "Cancelled" => query.Where(x => x.Cancel == true),
                    _ => query
                };
            }
        }

        public async Task<JobOrderVM?> GetJobOrderByJobIdAsync(int jobId)
        {
            try
            {
                var entity = await _unitOfWork.JobOrders.GetQueryable()
                    .Include(q => q.JobOrderSubs)
                    .Include(q => q.JobOrderSubs).ThenInclude(s => s.Item)
                    .Include(q => q.JobOrderSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.Customer)
                    .Include(q => q.AssyItem)
                    .Include(q => q.MfgPo).ThenInclude(s=>s.MfgPoSubs)
                    .Include(q => q.Staff)
                    .FirstOrDefaultAsync(q => q.JobId == jobId);

                return _mapper.Map<JobOrderVM?>(entity);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetJobOrderByJobIdAsync({jobId})");
                return null;
            }
        }

        public async Task UpdateItemCancelAsync(JobOrderSubVM subItem)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.JobOrderSubs.GetQueryable().Where
                                (x => x.JobSubId == subItem.JobSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with JobSubId {subItem.JobSubId} not found.");

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.JobOrderSubs.UpdateAsync(subEntity);

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdateItemCancelAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAsync for ItemCode {subItem.ItemCode}");
                throw new InvalidOperationException("Failed to update Item cancel/revert status. Please contact support.");
            }
        }

        public async Task<List<JobOrderSubVM>> GetJobOrderSubByJobIdAsync(int jobId)
        {
            try
            {
                var subs = await _unitOfWork.JobOrderSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.JobId == jobId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<JobOrderSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Job Order items for JobId: {jobId}");
                throw new InvalidOperationException("Failed to retrieve Job Order sub-items. Please try again.");
            }
        }

        public async Task<bool> UpdatedCancelStatusAsync(JobOrderVM jobVM, string cancelReason)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingJobOrder = await _unitOfWork.JobOrders.GetAsync(jobVM.JobId);
                if (existingJobOrder == null)
                    return false;

                if(!jobVM.Cancel)
                {
                    existingJobOrder.Cancel = true;
                    existingJobOrder.CancelReason = cancelReason;
                }
                else
                {
                    existingJobOrder.Cancel = false;
                    existingJobOrder.CancelReason = string.Empty;
                }
                await _unitOfWork.JobOrders.UpdateAsync(existingJobOrder);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Unexpected error");
                return false;
            }
        }


        public async Task AdjustPoWorkOrderBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refPoSubId.HasValue || refPoSubId == 0) return;

                var poSub = await _unitOfWork.MfgPoSubs.GetAsync(refPoSubId.Value);
                if (poSub == null) return;

                if (oldQty > 0)
                    poSub.WOBalQty += oldQty;

                if (newQty > poSub.WOBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Quote WOBalQty.");

                if (newQty > 0)
                    poSub.WOBalQty -= newQty;

                await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustQuoteBalance] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustQuoteBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust Quote balance. Please contact support.");
            }
        }

        public async Task<JobOrderVM?> GetJobQtyAndJobBalQtyByJobId(int jobId)
        {
            return await _unitOfWork.JobOrders.GetQueryable()
                .Where(x => x.JobId == jobId)
                .Select(x => new JobOrderVM
                {
                    JobOrderQty = x.JobOrderQty,
                    JobBalQty = x.JobBalQty
                })
                .FirstOrDefaultAsync();
        }

        public async Task<decimal?> GetMfgPoWOBalQtyByRefPoSubId(int poSubId)
        {
            return await _unitOfWork.MfgPoSubs.GetQueryable()
                .Where(x => x.PoSubId == poSubId)
                .Select(x => x.WOBalQty)
                .FirstOrDefaultAsync();
        }


        public async Task<JobOrderVM> UpsertJobOrderAsync(JobOrderVM jobOrderVM)
        {
            if (jobOrderVM == null)
                throw new ArgumentNullException(nameof(jobOrderVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                JobOrder entity;

                if (jobOrderVM.JobId == 0)
                {
                    entity = _mapper.Map<JobOrder>(jobOrderVM);

                    // 🔹 Get last number with locking from repository
                    var NextNumber = await _unitOfWork.JobOrders.GetLastJobNoAsync(entity.Suffix);
                    entity.JobNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.JobOrderSubs = jobOrderVM.JobOrderSubVMs.Select(s => _mapper.Map<JobOrderSub>(s)).ToList();

                    await _unitOfWork.JobOrders.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Job Order Created.");
                }
                else
                {
                    entity = await _unitOfWork.JobOrders.GetQueryable()
                        .Include(q => q.JobOrderSubs)
                        .FirstOrDefaultAsync(q => q.JobId == jobOrderVM.JobId)
                        ?? throw new InvalidOperationException("Job Order not found.");

                    var parentChanges = GetPropertyChanges(entity, jobOrderVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(jobOrderVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, jobOrderVM.JobOrderSubVMs, changes);

                    changes.AppendLine("Job Order Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdateJobTallyStatusAsync(jobOrderVM.JobId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, jobOrderVM.JobId == 0 ? "Job Order Created" : "Job Order Updated");

                var savedEntity = await _unitOfWork.JobOrders.GetQueryable()
                    .Include(q => q.JobOrderSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Customer)
                    .Include(q => q.JobOrderSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.JobId == entity.JobId);

                return _mapper.Map<JobOrderVM>(savedEntity!);
                
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Job Order : {jobOrderVM.JobNo}");
                throw new InvalidOperationException("Failed to save Job Order. Please try again.");
            }
        }

        public async Task UpdateJobTallyStatusAsync(int jobId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.JobOrderSubs
                    .GetQueryable()
                    .Where(x => x.JobId == jobId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var jobOrder = await _unitOfWork.JobOrders.GetAsync(jobId);
                if (jobOrder == null)
                    return;

                jobOrder.JobTally = (totalBalQty == 0);

                await _unitOfWork.JobOrders.UpdateAsync(jobOrder);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateJobTallyStatusAsync] Error updating JobId {jobId}");
                throw new InvalidOperationException("Failed to update Job Order Tally status. Please contact support.");
            }
        }

        private async Task HandleChildUpdatesAsync(JobOrder existingJO, List<JobOrderSubVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingJO.JobOrderSubs.Select(s => s.JobSubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.JobSubId).ToHashSet();

            foreach (var sub in existingJO.JobOrderSubs.Where(s => !incomingSubIds.Contains(s.JobSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - JobSubId: {sub.JobSubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.JobOrderSubs.DeleteAsync(sub.JobSubId);
                await _unitOfWork.SaveAsync();
            }

            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.JobSubId == 0)
                {
                    var newSub = _mapper.Map<JobOrderSub>(subVM);
                    newSub.JobId = existingJO.JobId;
                    await _unitOfWork.JobOrderSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}");
                }
                else
                {
                    var existingSub = existingJO.JobOrderSubs.FirstOrDefault(s => s.JobSubId == subVM.JobSubId);
                    if (existingSub != null)
                    {
                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
            }
        }

        public async Task<List<MfgPoVM>> GetPendingPOsByCustomerAsync(int custId)
        {
            try
            {
                return await _unitOfWork.MfgPos.GetQueryable()
                   .Include(p => p.MfgPoSubs)
                   .Where(p => p.CustId == custId && !p.PoTally && !p.PoCancl 
                            && p.MfgPoSubs.Any(ps => ps.WOBalQty > 0 && !ps.ItemCancel))
                   .Select(p => new MfgPoVM
                   {
                       PoId = p.PoId,
                       PONo = $"{p.PONo}{p.Suffix}",
                   })
                   .ToListAsync();
            }
            catch(Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to GetPendingPOsByCustomerAsync");
                throw;
            }
        }

        public async Task<List<JobOrderAssyPoItemVM>> GetAssemblyItemsByPoIdAsync(int poId)
        {
            try
            {
                // 1. Get PO Assembly Items
                //var poItems = await _unitOfWork.MfgPoSubs.GetQueryable()
                //    .Where(ps => ps.PoId == poId
                //             && ps.WOBalQty > 0
                //             && !ps.ItemCancel
                //             && !ps.MfgPo.PoTally
                //             && !ps.MfgPo.PoCancl)
                //    .Join(_unitOfWork.ItemRepositories.GetQueryable(),
                //        ps => ps.ItemId,
                //        i => i.ItemId,
                //        (ps, i) => new { ps, i })
                //    .Where(x => x.i.CategoryCode == 3)
                //    .Select(x => new JobOrderAssyPoItemVM
                //    {
                //        RefPoSubId = x.ps.PoSubId,
                //        RefPoNo = $"{x.ps.MfgPo.PONo}{x.ps.MfgPo.Suffix}",
                //        RefPoDate = x.ps.MfgPo.PODate,
                //        AssyItemId = x.ps.ItemId,
                //        AssyItemCode = x.i.ItemCode,
                //        PoQty = x.ps.Qty,
                //        WoBalQty = x.ps.WOBalQty,
                //        ReqQty = x.ps.WOBalQty,
                //        StockQty = 0m, // to fill after
                //        CostId = x.ps.CostId,
                //        ProjectNo = x.ps.CostCenter != null ? x.ps.CostCenter.ProjectNo : null,
                //    })
                //    .ToListAsync();

                bool BothAssmlyAndSub = await IsJobOrderBothAssemblyAndSubAssembly();

                // 1. Get PO Assembly Items
                var poItems = await _unitOfWork.MfgPoSubs.GetQueryable()
                    .Where(ps => ps.PoId == poId
                             && ps.WOBalQty > 0
                             && !ps.ItemCancel
                             && !ps.MfgPo.PoTally
                             && !ps.MfgPo.PoCancl)
                    .Join(_unitOfWork.ItemRepositories.GetQueryable(),
                        ps => ps.ItemId,
                        i => i.ItemId,
                        (ps, i) => new { ps, i })
                    .Where(x => BothAssmlyAndSub ? (x.i.CategoryCode == 3 || x.i.CategoryCode == 7)
                            : x.i.CategoryCode == 3)
                    .Select(x => new JobOrderAssyPoItemVM
                    {
                        RefPoSubId = x.ps.PoSubId,
                        RefPoNo = $"{x.ps.MfgPo.PONo}{x.ps.MfgPo.Suffix}",
                        RefPoDate = x.ps.MfgPo.PODate,
                        AssyItemId = x.ps.ItemId,
                        AssyItemCode = x.i.ItemCode,
                        AssyItemName = x.i.ItemName,
                        PoQty = x.ps.Qty,
                        WoBalQty = x.ps.WOBalQty,
                        ReqQty = x.ps.WOBalQty,
                        StockQty = 0m, // to fill after
                        CostId = x.ps.CostId,
                        ProjectNo = x.ps.CostCenter != null ? x.ps.CostCenter.ProjectNo : null,
                        LineNo = x.ps.LineNo
                    })
                    .ToListAsync();

                if (!poItems.Any())
                    return poItems;

                var itemIds = poItems
                    .Select(x => x.AssyItemId.Value)
                    .Distinct()
                    .ToList();

                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, null);

                foreach (var item in poItems)
                {
                    if (item.AssyItemId.HasValue && stockDict.TryGetValue(item.AssyItemId.Value, out var stock))
                        item.StockQty = stock;
                    else
                        item.StockQty = 0;
                }

                return poItems;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed: GetAssemblyItemsByPoIdAsync");
                throw;
            }
        }

        public async Task<List<JobOrderPreviewVM>> GetBOMHierarchyAsync(List<int> assyIds, List<JobOrderAssyPoItemVM> selectedAssys)
        {
            var result = new List<BOMItem>();

            try
            {
                bool Customermateril = await IsLabourMaterialValidateEnabledAsync();

                async Task AddChildrenAsync(int assyId, decimal reqParentQty, int refPoSubId, string refPoNo, decimal poQty, int? CostId, string? ProjectNo,int? LineNo,int? StaffId,string? StaffName,string? DepartmentCode)
                {
                    var query = _unitOfWork.AssmblyDefs.GetQueryable()
                            .Include(a => a.PartItem)
                            .Where(a => a.AssmblyID == assyId && a.PartItem != null);

                    if (Customermateril)
                    {
                        query = query.Where(a => a.IsLabourMaterial);
                    }

                    var children = await query
                           .Select(a => new BOMItem
                           {
                               Id = a.Id,
                               AssmblyID = a.AssmblyID,
                               ItemID = a.ItemId,
                               UtilQty = a.UtilQty,
                               ItemCode = a.PartItem.ItemCode,
                               ItemName = a.PartItem.ItemName,
                               categoryCode = a.PartItem.CategoryCode,
                               MeasureUnit = a.PartItem.MeasureUnit,
                               ReqQty = a.UtilQty * reqParentQty,
                            
                               RefPoSubId = refPoSubId,
                               RefPoNo = refPoNo,
                               PoQty = poQty,
                               JobOrderQty = reqParentQty,
                               CostId = CostId,
                               ProjectNo = ProjectNo,
                               LineNo = LineNo,
                               StaffID=StaffId,
                               StaffName=StaffName,
                               DepartmentCode=DepartmentCode
                               
                           })
                        .ToListAsync();

                    //var children = await _unitOfWork.AssmblyDefs.GetQueryable()
                    //    .Include(a => a.PartItem)
                    //    .Where(a => a.AssmblyID == assyId && a.PartItem != null)
                    //    .Select(a => new BOMItem
                    //    {
                    //        Id = a.Id,
                    //        AssmblyID = a.AssmblyID,
                    //        ItemID = a.ItemId,
                    //        UtilQty = a.UtilQty,
                    //        ItemCode = a.PartItem.ItemCode,
                    //        ItemName = a.PartItem.ItemName,
                    //        categoryCode = a.PartItem.CategoryCode,
                    //        MeasureUnit = a.PartItem.MeasureUnit,
                    //        ReqQty = a.UtilQty * reqParentQty,

                    //        RefPoSubId = refPoSubId,
                    //        RefPoNo = refPoNo,
                    //        PoQty = poQty,
                    //        JobOrderQty = reqParentQty,
                    //        CostId = CostId,
                    //        ProjectNo =ProjectNo 
                    //    })
                    //    .ToListAsync();

                    foreach (var child in children)
                    {
                        result.Add(child);
                        await AddChildrenAsync(child.ItemID, reqParentQty, refPoSubId, refPoNo, poQty, CostId, ProjectNo, LineNo,StaffId,StaffName,DepartmentCode);
                    }
                }

                foreach (var assy in selectedAssys)
                {
                    // 1. Load and add Assembly item itself
                    var parentItem = await _unitOfWork.ItemRepositories.GetQueryable()
                        .Where(i => i.ItemId == assy.AssyItemId)
                        .Select(i => new BOMItem
                        {
                            Id = 0,
                            AssmblyID = assy.AssyItemId.Value,
                            ItemID = i.ItemId,
                            UtilQty = 1,
                            ItemCode = i.ItemCode,
                            ItemName = i.ItemName,
                            categoryCode = i.CategoryCode,
                            MeasureUnit = i.MeasureUnit,
                            ReqQty = assy.ReqQty,

                            RefPoSubId = assy.RefPoSubId.GetValueOrDefault(),
                            RefPoNo = assy.RefPoNo,
                            PoQty = assy.PoQty,
                            JobOrderQty = assy.ReqQty,
                            CostId = assy.CostId,
                            ProjectNo = assy.ProjectNo,
                            LineNo = assy.LineNo,
                            StaffID= assy.StaffID,
                            StaffName= assy.StaffName,
                            DepartmentCode = assy.DepartmentCode
                        })
                        .FirstOrDefaultAsync();

                    if (parentItem != null)
                        result.Add(parentItem);

                    // 2. Then load children
                    await AddChildrenAsync(
                        assy.AssyItemId!.Value,
                        assy.ReqQty,
                        assy.RefPoSubId.GetValueOrDefault(),
                        assy.RefPoNo,
                        assy.PoQty,
                        assy.CostId,
                        assy.ProjectNo,
                        assy.LineNo,
                        assy.StaffID,
                        assy.StaffName,
                        assy.DepartmentCode
                    );
                }

                var filtered = result.Where(r => r.categoryCode == 3 || r.categoryCode == 7).ToList();

                string fySuffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);
                int nextNo = await GetPreviewJobNoAsync(fySuffix);

                var itemIds = filtered.Select(x => x.ItemID).Distinct().ToList();
                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, null);

                var final = filtered.Select((x, index) => new JobOrderPreviewVM
                {
                    TempJobNo = $"{(nextNo + index).ToString("D3")}{fySuffix}",
                    SubAssyId = x.ItemID,
                    SubAssyCode = $"{x.ItemCode} - {x.ItemName}",
                    MeasureUnit = x.MeasureUnit ?? "",
                    UtilQty = x.UtilQty,
                    ReqQty = x.ReqQty,
                    StockQty = stockDict.ContainsKey(x.ItemID) ? stockDict[x.ItemID] : 0,
                    IsSelected = true,

                    RefPoSubId = x.RefPoSubId,
                    RefPoNo = x.RefPoNo,
                    PoQty = x.PoQty,
                    JobOrderQty= x.JobOrderQty,

                    CostId = x.CostId,
                    ProjectNo = x.ProjectNo,
                    LineNo = x.LineNo

                }).ToList();

                return final;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while loading BOM for Job Order Preview");
                return new List<JobOrderPreviewVM>();
            }
        }


        public async Task<List<JobOrder>> GetReferenceJobListByParentJobId(int parentJobId, int refPoSubId)
        {
            try
            {
                var jobOrders = await _unitOfWork.JobOrders
                    .GetQueryable()
                    .Include(j => j.JobOrderSubs)
                    .Where(j =>
                        j.ParentJobId == parentJobId &&
                        j.RefPoSubId == refPoSubId)
                    .ToListAsync();

                return jobOrders;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error in GetReferenceJobListByParentJobId(parentJobId: {parentJobId}, refPoSubId: {refPoSubId})"
                );

                return new List<JobOrder>();
            }
        }

        public async Task<bool> HasProductionIssueForJobAsync(int itemId, int jobId)
        {
            try
            {
                return await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .AnyAsync(s => s.AssyId == itemId &&
                                   s.RefJobId == jobId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,$"Error in HasProductionIssueForJobAsync (itemId: {itemId}, jobId: {jobId})");
                return false;
            }
        }

        public async Task<List<BOMItem>> GetBOMItemsByAssyIdAsync(int assyId)
        {
            return await _unitOfWork.AssmblyDefs.GetQueryable()
                .Where(x => x.AssmblyID == assyId && !x.Cancel)
                .OrderBy(x => x.SlNo)
                .Select(x => new BOMItem
                {
                    ItemID = x.ItemId,
                    UtilQty = x.UtilQty
                })
                .ToListAsync();
        }

        public async Task<int> GetPreviewJobNoAsync(string suffix)
        {
            var lastNo = await _unitOfWork.JobOrders.GetQueryable()
                .Where(x => x.Suffix == suffix)
                .OrderByDescending(x => x.JobId)
                .Select(x => x.JobNo)
                .FirstOrDefaultAsync();

            int nextNumber = 1;

            if (!string.IsNullOrEmpty(lastNo))
            {
                var parts = lastNo.Split("/");
                nextNumber = int.Parse(parts.Last()) + 1;
            }
            return nextNumber;
        }

        public async Task<List<JobOrderSubVM>> GetItemDetailsByAssySubAssyIdAsync(int assyId, decimal joQty)
        {
            var bomItems = await _unitOfWork.AssmblyDefs.GetQueryable()
                .Where(x => x.AssmblyID == assyId)
                .Include(x => x.PartItem)
                .ToListAsync();

            var itemIds = bomItems.Select(x => x.ItemId).Distinct().ToList();

            var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, null);

            var result = bomItems.Select((x, index) => new JobOrderSubVM
            {
                SlNo = index + 1,
                ItemId = x.ItemId,
                ItemCode = x.PartItem?.ItemCode,
                ItemName = x.PartItem?.ItemName,
                Measureunit = x.PartItem?.MeasureUnit,

                UtilQty = x.UtilQty,
                ReqQty = x.UtilQty * joQty,
                BalQty = x.UtilQty * joQty,

                stockQty = stockDict.TryGetValue(x.ItemId, out var stock) ? stock : 0,

                ItemCancel = false
            }).ToList();

            return result;
        }

        public async Task<bool> CancelJobOrdersRecursiveAsync(List<int> jobIds, JobOrderVM ParentJobOrderVM, string cancelReason)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {

                if (!ParentJobOrderVM.Cancel)
                {

                    foreach (var jobId in jobIds)
                    {
                        var jobOrder = await _unitOfWork.JobOrders
                            .GetQueryable()
                            .Include(e => e.JobOrderSubs)
                            .FirstOrDefaultAsync(e => e.JobId == jobId);

                        if (jobOrder == null)
                            continue;

                        jobOrder.Cancel = true;
                        jobOrder.CancelReason = cancelReason;

                        await _unitOfWork.JobOrders.UpdateAsync(jobOrder);
                        await _unitOfWork.SaveAsync();

                        await _logs.LogUserAction(
                           UserName: await _currentUserService.GetUsernameAsync(),
                           Machine: _currentUserService.MachineName,
                           IP_Address: _currentUserService.IpAddress,
                           screen: "Job Order",
                           action: $"Cancelled Job Order No: {jobOrder.JobNo}",
                           additionalInfo: $"Job Id: {jobOrder.JobId}");
                    }

                    var isPOAssyItem = await _unitOfWork.MfgPoSubs.GetQueryable()
                                        .AnyAsync(p => p.PoId == ParentJobOrderVM.RefPoId && p.ItemId == ParentJobOrderVM.AssyId);
                    if (isPOAssyItem)
                    {
                        await AdjustPoWorkOrderBalanceAsync(ParentJobOrderVM.RefPoSubId, ParentJobOrderVM.JobOrderQty, 0, "Job order Cancel");
                    }

                }
                else
                {
                    foreach (var jobId in jobIds)
                    {
                        var jobOrder = await _unitOfWork.JobOrders
                            .GetQueryable()
                            .Include(e => e.JobOrderSubs)
                            .FirstOrDefaultAsync(e => e.JobId == jobId);

                        if (jobOrder == null)
                            continue;

                        jobOrder.Cancel = false;
                        jobOrder.CancelReason = string.Empty;

                        await _unitOfWork.JobOrders.UpdateAsync(jobOrder);
                        await _unitOfWork.SaveAsync();

                        await _logs.LogUserAction(
                        UserName: await _currentUserService.GetUsernameAsync(),
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Job Order",
                        action: $"Reverted Job Order No: {jobOrder.JobNo}",
                        additionalInfo: $"Job Id: {jobOrder.JobId}");

                    }

                    var isPOAssyItem = await _unitOfWork.MfgPoSubs.GetQueryable()
                                        .AnyAsync(p => p.PoId == ParentJobOrderVM.RefPoId && p.ItemId == ParentJobOrderVM.AssyId);
                    if (isPOAssyItem)
                    {
                        await AdjustPoWorkOrderBalanceAsync(ParentJobOrderVM.RefPoSubId, 0,ParentJobOrderVM.JobOrderQty, "Job order Revert");
                    }

                }


                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed recursive job order delete");
                return false;
            }
        }

        public async Task<bool> DeleteJobOrdersRecursiveAsync(List<int> jobIds, JobOrderVM? parentJobVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var jobId in jobIds)
                {
                    var jobOrder = await _unitOfWork.JobOrders
                        .GetQueryable()
                        .Include(e => e.JobOrderSubs)
                        .FirstOrDefaultAsync(e => e.JobId == jobId);

                    if (jobOrder == null)
                        continue;

                    await _unitOfWork.JobOrders.DeleteAsync(jobOrder);
                }
                await _unitOfWork.SaveAsync();


                if(parentJobVM != null)
                {
                    var isPOAssyItem = await _unitOfWork.MfgPoSubs.GetQueryable()
                                        .AnyAsync(p => p.PoId == parentJobVM.RefPoId && p.ItemId == parentJobVM.AssyId);
                    if (isPOAssyItem)
                    {
                        await AdjustPoWorkOrderBalanceAsync(parentJobVM.RefPoSubId, parentJobVM.JobOrderQty, 0, "Job order Delete");
                    }
                }


                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "Failed recursive job order delete");
                return false;
            }
        }

        public async Task<bool> DeleteJobOrderByJobIdAsync(int jobId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var jobOrder = await _unitOfWork.JobOrders
                    .GetQueryable()
                    .Include(e => e.JobOrderSubs)
                    .FirstOrDefaultAsync(e => e.JobId == jobId);

                if (jobOrder == null)
                    return false;

                var changes = new StringBuilder();

                var isPOAssyItem = await _unitOfWork.MfgPoSubs.GetQueryable()
                                    .AnyAsync(p => p.PoId == jobOrder.RefPoId && p.ItemId == jobOrder.AssyId);

                if(isPOAssyItem)
                {
                    await AdjustPoWorkOrderBalanceAsync(jobOrder.RefPoSubId, jobOrder.JobOrderQty, 0, "Job order Delete");
                }

                await _unitOfWork.JobOrders.DeleteAsync(jobOrder);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Job Order List",
                    action: $"Deleted Job Order: {jobOrder.JobNo}",
                    additionalInfo: $"Job Id: {jobOrder.JobId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Job Order. JobId: {jobId}");
                throw;
            }
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

        public async Task<bool> IsPoExists(int poId, int? assyId)
        {
            if (assyId == null)
                return false;

            try
            {
                var exists = await _unitOfWork.MfgPoSubs.GetQueryable()
                    .AnyAsync(j => j.PoId == poId && j.ItemId == assyId);

                return exists;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error checking PO existence for RefPoId: {poId}, AssyId: {assyId}");
                return false;
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteJobOrderAsync(int jobId)
        {
            try
            {
                var jobOrder = await _unitOfWork.JobOrders.GetQueryable().Where(j => j.JobId == jobId).FirstOrDefaultAsync();

                if (jobOrder == null)
                    return (true, "Job Order can be safely deleted.");

                bool hapProductionIssueAssyExists = await _unitOfWork.ProductionIssueAssySubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefJobId==jobOrder.JobId && qs.AssyId == jobOrder.AssyId);

                if (hapProductionIssueAssyExists)
                    return (false, "Cannot delete this Job Order as a Production Assembly Issue transaction exists.");

                return (true, "Job Order can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteJobOrderAsync for JobId: {jobId}");
                throw new Exception("Error checking Job Order delete eligibility", ex);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetExistingBOMDetailsAsync(int jobId)
        {
            try
            {
                var result = await _unitOfWork.JobOrderSubs.GetQueryable()
                    .Where(j => j.JobId == jobId)
                    .Select(j => new
                    {
                        j.ItemId,
                        j.Item.ItemCode,
                        j.Item.ItemName,
                        j.Item.MeasureUnit,
                        j.UtilQty
                    })
                    .ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode,
                    ["ItemName"] = r.ItemName,
                    ["UOM"] = r.MeasureUnit,
                    ["UtilQty"] = r.UtilQty,

                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching Existing BOM Details");
                throw new InvalidOperationException("Failed to retrieve Existing BOM Details. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetLatestBOMFromAssemblyDefAsync(int AssyId)
        {
            try
            {
                var result = await _unitOfWork.AssmblyDefs.GetQueryable()
                    .Where(j => j.AssmblyID == AssyId)
                    .Select(j => new
                    {
                        j.ItemId,
                        j.PartItem.ItemCode,
                        j.PartItem.ItemName,
                        j.PartItem.MeasureUnit,
                        j.UtilQty
                    })
                    .ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode,
                    ["ItemName"] = r.ItemName,
                    ["UOM"] = r.MeasureUnit,
                    ["UtilQty"] = r.UtilQty,

                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching Existing BOM Details");
                throw new InvalidOperationException("Failed to retrieve Existing BOM Details. Please try again.");
            }
        }

        public async Task<List<StaffVM>> GetStaffOnlyProductionDepartment()
        {
            try
            {
                var result = await _unitOfWork.Staffs.GetQueryable()
                    .Where(x => x.Department != null &&
                                EF.Functions.Like(x.Department, "%PRODUCTION%") && x.Status == "Active")
                    .Select(x => new StaffVM
                    {
                        StaffID = x.StaffID,
                        StaffName = x.StaffName,
                        DepartmentCode = x.DepartmentCode
                    })
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetStaffOnlyProductionDepartment");
                throw;
            }
        }

        public async Task<int?> GetStaffIdByCurrentuserAsync()
        {
            try
            {
                int userId = await _currentUserService.GetUserIdAsync();

                var user = await _unitOfWork.Users
                    .GetQueryable()
                    .Include(u => u.Staff)
                    .FirstOrDefaultAsync(u => u.UserId == userId);


                return user?.StaffId ?? 0;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error GetDerpartmentCodeByCurrentuserAsync");
                throw new InvalidOperationException("Failed to GetDerpartmentCodeByCurrentuserAsync");
            }

        }

        public async Task<List<JobOrderStatusListVM>> GetJobOrderStatusListAsync(string status)
        {
            try
            {

                var result = await _commonService.ExecuteStatusSPAsync<JobOrderStatusListVM>("Sp_GetJobOrderAssemblySubAssemblyList", status);
                return result.ToList();

               
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> IsManualyJobOrderHiden()
        {
            try
            {
                var userId = await _currentUserService.GetUserIdAsync();


                var user = await _unitOfWork.Users.GetQueryable()
                    .FirstOrDefaultAsync(x => x.UserId == userId);

                return user?.HideManualJobOrderButton ?? false;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[IsManualyJobOrderHiden] Error ");
                throw new InvalidOperationException("Failed to IsManualyJobOrderHiden. Please contact support.");
            }
        }
    }
}
