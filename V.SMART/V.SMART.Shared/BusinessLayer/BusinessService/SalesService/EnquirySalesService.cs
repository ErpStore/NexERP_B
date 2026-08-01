using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Wordprocessing;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.FeasibilityVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel;
using static MudBlazor.Icons;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SalesService
{
    public class EnquirySalesService : IEnquirySalesService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;
        private readonly IExcelTemplateService _excelTemplateService;
        private readonly V.SMART.Shared.Services.ReportViewer.ReportService _reportService;
        private readonly IReportExecutor _report;
        public EnquirySalesService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            ICommonService commonService,
            IMapper mapper,
            IExcelTemplateService excelTemplateService,
            V.SMART.Shared.Services.ReportViewer.ReportService reportService, IReportExecutor report)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
            _mapper = mapper;
            _excelTemplateService = excelTemplateService;
            _reportService = reportService;
            _report = report;
        }

        #region Common Services

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();
        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
            => await _commonService.SearchCustomersAsync(searchText);
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? custId = null)
            => await _commonService.SearchItemsAsync(searchText,custId);

        public async Task<CustomerVM?> GetCustomerByIdAsync(int custId)
            => await _commonService.GetCustomerByIdAsync(custId);

        public async Task<List<ContactPerson>> GetContactPersonsAsync(int custId)
            => await _commonService.GetContactPersonsAsync(custId);

        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        #endregion

        #region Enquiry Operations


        public async Task<(List<EnquirySalesVM> enquirySalesVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.EnquirySaless.GetQueryable()
                .Include(j => j.Customer)
                .Include(j => j.EnquirySalesSub)
                    .ThenInclude(s => s.Item)
                .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = EnqFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.EnquiryId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<EnquirySalesVM>>(list);

            return (vmList, total);
        }

        public static class EnqFilterBuilder
        {
            public static IQueryable<EnquirySales> ApplyFilter(
                IQueryable<EnquirySales> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "EnqNo":
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
                                (string.IsNullOrEmpty(part1) || x.EnquiryNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }


                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(val));

                    case "ItemName":
                        return query.Where(x => x.EnquirySalesSub
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "ItemCode":
                        return query.Where(x => x.EnquirySalesSub
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.EnquiryDate >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.EnquiryDate <= toDate);
                        break;

                    case "Status":
                        return ApplyStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<EnquirySales> ApplyStatusFilter(
                IQueryable<EnquirySales> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.EnquiryTally == true),
                    "Pending" => query.Where(x => x.EnquiryTally == false && x.Cancel == false && x.ShortClose == false),
                    "Cancelled" => query.Where(x => x.Cancel == true),
                    "Short Closed" => query.Where(x => x.ShortClose == true),
                    _ => query
                };
            }
        }

        public async Task<bool> IsEnquiryTransactionsMatchedAsync(int enquiryId, EnquirySalesVM enqVMs)
        {
            try
            {
                var enqSubIds = await _unitOfWork.EnquirySalesSubs
                    .GetQueryable()
                    .Where(x => x.EnquiryId == enquiryId)
                    .Select(x => x.EnquirySubId)
                    .ToListAsync();

                bool hasEnquiry = enqSubIds.Any();
                bool hasTransactions = false;

                if (hasEnquiry)
                {
                    // Check Manufacturing Quotation references
                    hasTransactions = await _unitOfWork.MfgQuoteSubs
                        .GetQueryable()
                        .AnyAsync(pqs =>
                            pqs.RefEnqSubId.HasValue &&
                            enqSubIds.Contains(pqs.RefEnqSubId.Value));

                    // Check Feasibility entries
                    if (!hasTransactions)
                    {
                        hasTransactions = await _unitOfWork.EnquiryFeasibilities
                            .GetQueryable()
                            .AnyAsync(f => f.RefEnqId == enquiryId);
                    }
                }

                // Quantity mismatch check
                bool qtyMismatch = false;

                var list = enqVMs?.EnquirySalesSubVM;
                if (list != null && list.Any())
                {
                    decimal totalQty = list.Sum(x => x.Qty ?? 0);
                    decimal totalBalQty = list.Sum(x => x.BalQty ?? 0);

                    qtyMismatch = totalQty != totalBalQty;
                }

                // If either transactions exist OR quantity mismatch → return true
                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error while checking transactions for EnquiryId: {enquiryId}");
                throw new InvalidOperationException("Failed to verify enquiry transactions.", ex);
            }
        }



        public async Task<(bool CanDelete, string Message)> CanDeleteSalesEnquiryAsync(int enqId)
        {
            try
            {
                var enquiry = await _unitOfWork.EnquirySaless
                                .GetQueryable()
                                .Include(e => e.EnquirySalesSub)
                                .Where(e => e.EnquiryId == enqId).FirstOrDefaultAsync();

                if (enquiry == null)
                    return (true, "Sales Enquiry can be safely deleted.");

                var enqSubIds = enquiry.EnquirySalesSub
                    .Select(es => es.EnquirySubId)
                    .ToList();

                // ===== Check for existing Feasibility transactions =====
                bool hasEnqFeasibility = await _unitOfWork.EnquiryFeasibilities
                    .GetQueryable()
                    .AnyAsync(f => f.RefEnqId == enqId);

                if (hasEnqFeasibility)
                    return (false, "Cannot delete this Sales Enquiry as an Enquiry Feasibility transaction exists.");

                // ===== Check for existing Quotation transactions =====
                bool hasQuotation = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefEnqSubId.HasValue &&
                        enqSubIds.Contains(qs.RefEnqSubId.Value));

                if (hasQuotation)
                    return (false, "Cannot delete this Sales Enquiry as a Quotation transaction exists.");

                // ===== Check if Enquiry is Cancelled or Short Closed =====
                if (enquiry.Cancel || enquiry.ShortClose)
                    return (false, "Cannot delete this Sales Enquiry as it is Cancelled or Short Closed.");


                // ===== Check if any Enquiry Sub Item is Cancelled =====
                if (enquiry.EnquirySalesSub.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Sales Enquiry as one or more enquiry items are cancelled.");


                return (true, "Sales Enquiry can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error in CanDeleteSalesEnquiryAsync for EnquiryId: {enqId}");
                throw new Exception("Error checking Sales Enquiry delete eligibility",ex);
            }
        }



        public async Task<EnquirySalesVM?> GetEnquiryDetailsByEnqIdAsync(int enquiryId)
        {
            try
            {
                var entity = await _unitOfWork.EnquirySaless
                    .GetQueryable()
                    .Include(q => q.EnquirySalesSub)
                        .ThenInclude(s => s.Item)
                    .Include(q => q.EnquirySalesSub)
                        .ThenInclude(s => s.CostCenter)
                    .Include(q => q.Customer)
                        .ThenInclude(c => c.ContactPersons)
                    .FirstOrDefaultAsync(q => q.EnquiryId == enquiryId);

                return _mapper.Map<EnquirySalesVM?>(entity);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get enquiry details for EnquiryId: {enquiryId}");
                throw;
            }
        }

        public async Task<string> GetPrefixFromDbAsync()
        {
            try
            {
                var prefix = await _unitOfWork.EnquirySaless
                    .GetQueryable()
                    .AsNoTracking()
                    .OrderByDescending(q => q.EnquiryId)
                    .Select(q => q.Prefix)
                    .FirstOrDefaultAsync();

                return prefix ?? string.Empty;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetPrefixFromDbAsync()");
                return string.Empty;
            }
        }

        public async Task<EnquirySalesVM> UpsertEnquirySalesAsync(EnquirySalesVM vm)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                EnquirySales entity;

                if (vm.EnquiryId == 0)
                {
                    // INSERT
                    entity = _mapper.Map<EnquirySales>(vm);

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.EnquirySalesSub = vm.EnquirySalesSubVM
                        .Select(s => _mapper.Map<EnquirySalesSub>(s))
                        .ToList();

                    await _unitOfWork.EnquirySaless.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Enquiry Created by {currentUser}. Enquiry Number: {entity.EnquiryNo}");

                    foreach (var subvm in vm.EnquirySalesSubVM)
                        await UpdateCostCenterAssignmentAsync(subvm.CostCenterId, true, changes);
                }
                else
                {
                    // UPDATE
                    entity = await _unitOfWork.EnquirySaless
                        .GetQueryable()
                        .Include(e => e.EnquirySalesSub)
                        .FirstOrDefaultAsync(e => e.EnquiryId == vm.EnquiryId)
                        ?? throw new InvalidOperationException("Enquiry not found.");

                    var parentChanges = GetPropertyChanges(entity, vm);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(vm, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, vm.EnquirySalesSubVM, changes);

                }
                await _unitOfWork.SaveAsync();

                await UpdateEnquiryTallyStatusAsync(vm.EnquiryId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, vm.EnquiryId == 0 ? "Enquiry Sales Created" : "Enquiry Sales Updated");

                // Return updated entity
                var savedEntity = await _unitOfWork.EnquirySaless
                    .GetQueryable()
                    .Include(e => e.EnquirySalesSub).ThenInclude(s => s.Item)
                    .Include(e => e.EnquirySalesSub).ThenInclude(s => s.CostCenter)
                    .Include(e => e.Customer)
                    .FirstOrDefaultAsync(e => e.EnquiryId == entity.EnquiryId);

                return _mapper.Map<EnquirySalesVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Error in UpsertEnquirySalesAsync for EnquiryId: {vm.EnquiryId}");
                throw;
            }
        }

        private async Task HandleChildUpdatesAsync(EnquirySales existingEnquiry, List<EnquirySalesSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {
                var existingSubIds = existingEnquiry.EnquirySalesSub.Select(s => s.EnquirySubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.EnquirySubId).ToHashSet();

                foreach (var sub in existingEnquiry.EnquirySalesSub.Where(s => !incomingSubIds.Contains(s.EnquirySubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - EnquirySubId: {sub.EnquirySubId}, Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.EnquirySalesSubs.DeleteAsync(sub.EnquirySubId);
                    await _unitOfWork.SaveAsync();
                    await UpdateCostCenterAssignmentAsync(sub.CostCenterId, false, changes);
                }

                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.EnquirySubId == 0)
                    {
                        var newSub = _mapper.Map<EnquirySalesSub>(subVM);
                        newSub.EnquiryId = existingEnquiry.EnquiryId;
                        await _unitOfWork.EnquirySalesSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();
                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");
                        await UpdateCostCenterAssignmentAsync(subVM.CostCenterId, true, changes);
                    }
                    else
                    {
                        var existingSub = existingEnquiry.EnquirySalesSub.FirstOrDefault(s => s.EnquirySubId == subVM.EnquirySubId);
                        if (existingSub != null)
                        {
                            if(existingSub.CostCenterId != subVM.CostCenterId)
                            {
                                await UpdateCostCenterAssignmentAsync(existingSub.CostCenterId, false, changes);
                            }

                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");
                            _mapper.Map(subVM, existingSub);
                            await UpdateCostCenterAssignmentAsync(subVM.CostCenterId, true, changes);
                        }
                    }
                }
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "[HandleChildUpdatesAsync - Enquiry] Failed to update enquiry children");
                throw new InvalidOperationException("Failed to update Enquiry details. Please contact support.");
            }
        }

        private async Task UpdateCostCenterAssignmentAsync(int? costCenterId, bool assign, StringBuilder changes)
        {
            if (!costCenterId.HasValue) return;

            var cost = await _unitOfWork.CostCenters.GetAsync(costCenterId.Value);
            if (cost != null && cost.AssToEnq != assign)
            {
                cost.AssToEnq = assign;
                await _unitOfWork.CostCenters.UpdateAsync(cost);
                await _unitOfWork.SaveAsync();
                changes.AppendLine($"CostCenter Updated - ProjectNo {cost.ProjectNo}: AssToEnq set to '{assign}'");
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

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _loggingService.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Enquiry Sales",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<bool> IsDuplicateEnquiryAsync(string enquiryNo, string suffix,int? enquiryId = null, int? custId = null)
        {
            try
            {
                return await _unitOfWork.EnquirySaless
                    .AnyAsync(e =>
                        e.EnquiryNo == enquiryNo && e.Suffix == suffix &&
                        (!enquiryId.HasValue || e.EnquiryId != enquiryId) &&
                        (!custId.HasValue || e.CustId == custId));
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to check duplicate EnquiryNo '{enquiryNo}'");
                throw;
            }
        }

        public async Task<IEnumerable<EnquirySalesVM>> GetAllEnquirySalesAsync()
        {
            try
            {
                return await _unitOfWork.EnquirySaless
                    .GetQueryable()
                    .Include(e => e.Customer)
                    .OrderByDescending(c => c.EnquiryId)
                    .ProjectTo<EnquirySalesVM>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to get all EnquirySales");
                throw;
            }
        }

        public async Task<bool> DeleteEnquiryAsync(int enquiryId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var enquiry = await _unitOfWork.EnquirySaless
                    .GetQueryable()
                    .Include(e => e.EnquirySalesSub)
                    .FirstOrDefaultAsync(e => e.EnquiryId == enquiryId);

                if (enquiry == null) return false;

                var changes = new StringBuilder();
                foreach (var sub in enquiry.EnquirySalesSub)
                    await UpdateCostCenterAssignmentAsync(sub.CostCenterId, false, changes);

                await _unitOfWork.EnquirySaless.DeleteAsync(enquiry);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "EnquirySales List",
                    action: $"Deleted EnquirySales: {enquiry.EnquiryNo}",
                    additionalInfo: $"Enquiry Id: {enquiry.EnquiryId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete EnquirySales: {enquiryId}");
                throw;
            }
        }

        #endregion

        public async Task<byte[]> CreateTemplateAsync(string uploadtype)
        {
            try
            {
                var bytes = await _excelTemplateService.DownloadTemplate(uploadtype);
                return bytes;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error creating Excel template");
                return Array.Empty<byte>();
            }
        }

        #region Enquiry Subitem Operations

        public async Task<List<EnquiryFeasibilityVM>> GetFeasibilityDetailsByEnqIdAsync(string enquiryNo)
        {
            try
            {
                var entities = await _unitOfWork.EnquiryFeasibilities
               .GetQueryable()
               .Where(q => q.RefEnqNo == enquiryNo)
               .ToListAsync();

                return entities.Select(e => new EnquiryFeasibilityVM
                {
                    RefEnqNo = e.RefEnqNo,
                    RefEnqId = e.RefEnqId,
                    FesId = e.FesId,
                }).ToList();
            }
            catch (Exception ex)
            {

                await _loggingService.LogDeveloperError(ex, $"Failed to get enquiry details for enquiryNo: {enquiryNo}");
                throw;
            }
        }

        public async Task<(bool CanItemCancel, string Message)> CanEnquiryItemCancelCheckAsync(EnquirySalesSubVM subItem)
        {
            try
            {
                bool hasQuote = await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefEnqSubId.HasValue && qs.RefEnqSubId == subItem.EnquirySubId && !qs.ItemCancel);

                if (hasQuote)
                    return (false, "Cannot cancel this Item as a Sales Quotation transaction exists.");

                bool hasFeas = await _unitOfWork.EnquiryFeasibilities
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefEnqId == subItem.EnquiryId);

                if (hasFeas)
                    return (false, "Cannot cancel this Item as a Enquiry Feasibility transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanEnquiryItemCancelCheckAsync for EnquirySubId: {subItem.EnquirySubId}");
                throw new Exception("Error checking Sales Enquiry Item cancel eligibility", ex);
            }
        }


        public async Task UpdateItemCancelAndAddorRevertAsync(EnquirySalesSubVM subItem, int enqId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var subEntity = await _unitOfWork.EnquirySalesSubs.GetQueryable().Where
                                (x => x.EnquirySubId == subItem.EnquirySubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with EnqSubId {subItem.EnquirySubId} not found.");

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;
                await _unitOfWork.EnquirySalesSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                await UpdateEnquiryTallyStatusAsync(enqId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "[UpdateItemCancelAndAddorRevertAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Error in UpdateItemCancelAndAddorRevertAsync for ItemCode {subItem.ItemCode}");
                throw new InvalidOperationException("Failed to update Item cancel/revert status. Please contact support.");
            }
        }

        public async Task UpdateEnquiryTallyStatusAsync(int enqId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.EnquirySalesSubs
                    .GetQueryable()
                    .Where(x => x.EnquiryId == enqId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var enquiry = await _unitOfWork.EnquirySaless.GetAsync(enqId);
                if (enquiry == null)
                    return;

                enquiry.EnquiryTally = (totalBalQty == 0);

                await _unitOfWork.EnquirySaless.UpdateAsync(enquiry);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[UpdateEnquiryTallyStatusAsync] Error updating EnquiryId {enqId}");
                throw new InvalidOperationException("Failed to update Enquiry Tally status. Please contact support.");
            }
        }

        public async Task DeleteEnqSubItemAndResequenceAsync(EnquirySalesSubVM subitem, EnquirySalesVM enquiry)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (subitem.EnquirySubId > 0)
                {
                    var entity = await _unitOfWork.EnquirySalesSubs.GetAsync(subitem.EnquirySubId);
                    if (entity == null)
                        throw new InvalidOperationException("Subitem not found.");

                    if (entity.CostCenterId.HasValue)
                    {
                        var costCenter = await _unitOfWork.CostCenters.GetAsync(entity.CostCenterId.Value);
                        if (costCenter != null && costCenter.AssToEnq)
                        {
                            costCenter.AssToEnq = false;
                            await _unitOfWork.CostCenters.UpdateAsync(costCenter);
                            await _unitOfWork.SaveAsync();
                        }
                    }

                    await _unitOfWork.EnquirySalesSubs.DeleteAsync(entity.EnquirySubId);
                    await _unitOfWork.SaveAsync();

                    await _loggingService.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Enquiry Sales",
                        $"Deleted Subitem: {subitem.ItemCode}",
                        $"Enquiry No: {enquiry?.EnquiryNo}"
                    );
                }
                else
                {
                    enquiry.EnquirySalesSubVM.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.EnquirySalesSubs
                    .GetQueryable()
                    .Where(x => x.EnquiryId == enquiry.EnquiryId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                // Update Enquiry Tally Status
                await UpdateEnquiryTallyStatusAsync(enquiry.EnquiryId);

                await transaction.CommitAsync();

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Error in DeleteEnqSubItemAndResequenceAsync");
                throw;
            }
        }

        public async Task<List<EnquirySalesSubVM>> GetEnqSubByEnquiryIdAsync(int enqId)
        {
            try
            {
                var subs = await _unitOfWork.EnquirySalesSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.EnquiryId == enqId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<EnquirySalesSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in GetEnqSubByEnquiryIdAsync for EnquiryId: {enqId}");
                throw;
            }
        }

        public async Task<bool> HasAnyQuotationTransactionMadeAsync(int enquiryId)
        {
            try
            {
                var enquirySubIds = await _unitOfWork.EnquirySalesSubs
                    .GetQueryable()
                    .Where(s => s.EnquiryId == enquiryId)
                    .Select(s => s.EnquirySubId)
                    .ToListAsync();

                if (!enquirySubIds.Any())
                    return false;

                return await _unitOfWork.MfgQuoteSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefEnqSubId.HasValue && enquirySubIds.Contains(qs.RefEnqSubId.Value));
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in HasAnyQuotationTransactionMadeAsync for EnquiryId: {enquiryId}");
                throw;
            }
        }

        public async Task<bool> HasAnyEnqFesibilityTransactionMadeAsync(int enquiryId)
        {
            try
            {
                return await _unitOfWork.EnquiryFeasibilities
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefEnqId == enquiryId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    $"Error in HasAnyEnqFesibilityTransactionMadeAsync for EnquiryId: {enquiryId}"
                );
                throw;
            }
        }


        public async Task<EnquirySalesSub?> GetEnqSubItemDetailByEnqSubIdAsync(int subEnqId)
        {
            if (subEnqId <= 0)
                return null;

            try
            {
                return await _unitOfWork.EnquirySalesSubs.GetAsync(subEnqId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in GetEnqSubItemDetailByEnqSubIdAsync for SubEnqId: {subEnqId}");
                throw;
            }
        }

        public async Task UpsertSalesEnquiryShortCloseAsync(EnquirySalesVM enquirySalesVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingEnq = await _unitOfWork.EnquirySaless.GetAsync(enquirySalesVM.EnquiryId);
                if (existingEnq == null)
                    throw new InvalidOperationException("Sales Enquiry not found.");

                existingEnq.ShortClose = enquirySalesVM.ShortClose;

                await _unitOfWork.EnquirySaless.UpdateAsync(existingEnq);
                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "[UpsertSalesEnquiryShortCloseAsync] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "[UpsertSalesEnquiryShortCloseAsync] Unexpected error");
                throw new InvalidOperationException("Failed to update short-Close/re-open status. Please contact support.");
            }
        }

        


        public async Task UpdatedCancelStatusAndAddOrRevertQty(EnquirySalesVM enquirySalesVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingEnq = await _unitOfWork.EnquirySaless.GetAsync(enquirySalesVM.EnquiryId);
                if (existingEnq == null)
                    throw new InvalidOperationException("Sales Enquiry not found.");

                existingEnq.Cancel = enquirySalesVM.Cancel;
                existingEnq.CancelReason = enquirySalesVM.CancelReason;
                existingEnq.CancelDate = enquirySalesVM.CancelDate;
                existingEnq.CancelBy = enquirySalesVM.CancelBy;

                await _unitOfWork.EnquirySaless.UpdateAsync(existingEnq);
                await _unitOfWork.SaveAsync();
                
                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Validation issue");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "[UpdatedCancelStatusAndAddOrRevertQty] Unexpected error");
                throw new InvalidOperationException("Failed to update cancel/revert status. Please contact support.");
            }
        }

        #endregion

        #region for Print work
        public async Task<byte[]?> GenerateAndOpenPdf(int refId, string refTemplatefrx, string paramName, bool cancel, string screenName, string procedureName)
        {
            try
            {
                return await _reportService.Generate_Report(refId, refTemplatefrx, paramName, cancel, screenName, procedureName);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in GenerateAndOpenPdf {screenName}");

                return null;
            }
        }
        #endregion
        public async Task<DataTable> GetSalesandLabeourEnquiryPendingList(string status)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@Status", status ?? (object)DBNull.Value),

                };
                var result = await _report.ExecuteDataTableAsync("Sp_GetSalesandlabourPendingList", parameters);
                return result ?? new DataTable();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }

    
}
