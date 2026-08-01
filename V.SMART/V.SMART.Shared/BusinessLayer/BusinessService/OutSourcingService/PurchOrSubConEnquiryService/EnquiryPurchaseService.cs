
using AutoMapper;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Text;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchOrSubConEnquiryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISalesService;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.PurchOrSubConEnquiryService
{
    public class EnquiryPurchaseService : IEnquiryPurchaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;

        public EnquiryPurchaseService(IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            ICommonService commonService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
            _mapper = mapper;
        }

        #region Common Services

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
            => await _commonService.GetItemByItemIdAsync(itemId);

        public async Task<Companydetails?> GetCompanyDetailsAsync()
            => await _commonService.GetCompanyDetailsAsync();

        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();
        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
            => await _commonService.SearchVendorsAsync(searchText);
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<VendorVM?> GetVendorByIdAsync(int VendorCode)
            => await _commonService.GetVendorByVenerCodeAsync(VendorCode);

        public async Task<List<VendorContact>> GetContactPersonsVendorAsync(int VendorCode)
            => await _commonService.GetContactPersonsVendorAsync(VendorCode);

        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        #endregion

        #region Enquiry Operations

        public async Task<EnquiryPurchaseSubVM?> GetEnqSubItemDetailByEnquirySubIdAsync(int enqSubId)
        {
            try
            {
                return await _unitOfWork.EnquiryPurchaseSubs
                    .GetQueryable()
                    .Where(q => q.EnquirySubId == enqSubId)
                    .Select(q => new EnquiryPurchaseSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Enquiry sub item detail for EnqSubId: {enqSubId}");
                throw new InvalidOperationException("Failed to retrieve Enquiry sub-item details.");
            }
        }

        public async Task<bool> IsEnquiryTransactionsMatchedAsync(int enquiryId, EnquiryPurchaseVM enqVMs)
        {
            try
            {
                var enqAssignIds = await _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetQueryable()
                    .Where(x => x.EnquiryId == enquiryId)
                    .Select(x => x.EnqPurchVendorId)
                    .ToListAsync();

                bool hasVendorAssignments = enqAssignIds != null && enqAssignIds.Any();

                bool hasTransactions = false;

                if (hasVendorAssignments)
                {
                    hasTransactions = await _unitOfWork.PurchaseQuotesSubs
                        .GetQueryable()
                        .AnyAsync(pqs => enqAssignIds.Contains(pqs.RefEnqVendorAssignId.Value));
                }

                bool qtyMismatch = false;

                var list = enqVMs?.EnquiryPurchaseVendorAssignVMs;
                if (list != null && list.Count > 0)
                {
                    decimal totalQty = 0, totalBalQty = 0;

                    foreach (var item in list)
                    {
                        totalQty += item.Qty.GetValueOrDefault();
                        totalBalQty += item.BalQty.GetValueOrDefault();
                    }

                    qtyMismatch = (totalQty != totalBalQty);
                }
                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"Error while checking transactions for EnquiryId: {enquiryId}");

                throw new InvalidOperationException("Failed to verify enquiry transactions.");
            }
        }



        public async Task<List<EnquiryPurchaseVendorAssignVM>> GetAssignedVendorsByEnquiryIdAsync(int enquiryId)
        {
            try
            {
                var assignedVendorCodes = await _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetQueryable()
                    .Where(ev => ev.EnquiryId == enquiryId)
                    .Select(ev => ev.VendorCode)
                    .ToListAsync();

                if (!assignedVendorCodes.Any())
                    return new List<EnquiryPurchaseVendorAssignVM>();

                var vendors = await _unitOfWork.Vendors
                    .GetQueryable()
                    .Where(v => assignedVendorCodes.Contains(v.VendorCode))
                    .Select(v => new EnquiryPurchaseVendorAssignVM
                    {
                        VendorCode = v.VendorCode,
                        VendorName = v.VendorName,
                        VendorPhoneNo = v.ContactNo,
                        VendorGSTNo = v.GSTNo,
                        IsAssigned = true
                    })
                    .ToListAsync();

                return vendors;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,
                    $"Error fetching assigned vendors for EnquiryId: {enquiryId}");
                throw new InvalidOperationException("Failed to load assigned vendor details.");
            }
        }



        public async Task<EnquiryPurchaseVM?> GetEnquiryDetailsByEnqIdAsync(int enquiryId)
        {
            try
            {
                var entity = await _unitOfWork.EnquiryPurchases
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.EnquiryPurchaseSub)
                        .ThenInclude(s => s.Item)
                    .Include(q => q.EnquiryPurchaseSub)
                        .ThenInclude(s => s.MaterialReqSub)
                            .ThenInclude(m => m.MaterialReq)
                    .Include(q => q.EnquiryPurchaseSub)
                        .ThenInclude(s => s.CostCenter)
                    .Include(q => q.EnquiryPurchaseVendorAssigns)
                        .ThenInclude(ev => ev.Vendor)
                            .ThenInclude(v => v.VendorContacts)

                    .FirstOrDefaultAsync(q => q.EnquiryId == enquiryId);


                return _mapper.Map<EnquiryPurchaseVM?>(entity);


            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get enquiry details for EnquiryId: {enquiryId}");
                throw;
            }
        }

        #region Getting the Reference Vendors or Suppliers
        public async Task<List<EnquiryPurchaseVendorAssignVM>> GetRefVendors(int EnquiryId)
        {
            try
            {
                var refVendors = await _unitOfWork.EnquiryPurchaseVendorAssigns
                                   .GetQueryable()
                                   .Where(x => x.EnquiryId == EnquiryId)
                                   .GroupBy(x => x.VendorCode)
                                   .Select(g => g.First())
                                   .ToListAsync();
                return _mapper.Map<List<EnquiryPurchaseVendorAssignVM>>(refVendors);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error Occured in GetRefVendors()");
                return new List<EnquiryPurchaseVendorAssignVM>();
            }
        }

        #endregion
        public async Task<string> GetPrefixFromDbAsync()
        {
            try
            {
                var prefix = await _unitOfWork.EnquiryPurchases
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

        public async Task<string> GenerateEnquiryNumberAsync(string suffix)
        {
            try
            {
                var lastEnquiry = await _unitOfWork.EnquiryPurchases
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.EnquiryNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastEnquiry != null)
                {
                    var parts = lastEnquiry.EnquiryNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error generating Enquity number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Enquiry number.");
            }
        }

        public async Task<EnquiryPurchaseVM> UpsertEnquiryPurchaseShortCloseAsync(EnquiryPurchaseVM enquiryPurchaseVM)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                EnquiryPurchase entity;

                entity = await _unitOfWork.EnquiryPurchases
                             .GetQueryable()
                             .Include(e => e.EnquiryPurchaseSub)
                             .FirstOrDefaultAsync(e => e.EnquiryId == enquiryPurchaseVM.EnquiryId)
                             ?? throw new InvalidOperationException("Enquiry not found.");

                _mapper.Map(enquiryPurchaseVM, entity);

                var parentChanges = GetPropertyChanges(entity, enquiryPurchaseVM);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                _mapper.Map(enquiryPurchaseVM, entity);
                entity.ModifiedBy = currentUser;
                entity.ModifiedDate = now;

                await _unitOfWork.EnquiryPurchases.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();

                foreach (var sub in entity.EnquiryPurchaseSub)
                {
                    if (entity.EnquiryShortClose)
                    {
                        await UpdateEnquiryPurchaseVendorAssignItemStatus(sub.EnquirySubId, 4);
                    }
                    else
                    {
                        await UpdateEnquiryPurchaseVendorAssignItemStatus(sub.EnquirySubId, 1);
                    }
                }

                await transaction.CommitAsync();
                await LogChangesAsync(changes, enquiryPurchaseVM.EnquiryId == 0 ? "Enquiry purchase Created" : "Enquiry purchase Updated");


                // Return updated entity
                var savedEntity = await _unitOfWork.EnquiryPurchases
                    .GetQueryable()
                    .Include(e => e.EnquiryPurchaseSub).ThenInclude(s => s.Item)
                    .Include(e => e.EnquiryPurchaseSub).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(e => e.EnquiryId == entity.EnquiryId);

                return _mapper.Map<EnquiryPurchaseVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Error in UpsertEnquiryPurchaseAsync for EnquiryId: {enquiryPurchaseVM.EnquiryId}");
                throw;
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(EnquiryPurchaseVM enquiryPurchaseVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingEnq = await _unitOfWork.EnquiryPurchases.GetAsync(enquiryPurchaseVM.EnquiryId);
                if (existingEnq == null)
                    throw new InvalidOperationException("Puchase Enquiry not found.");

                var subs = await _unitOfWork.EnquiryPurchaseSubs
                    .GetQueryable()
                    .Where(s => s.EnquiryId == enquiryPurchaseVM.EnquiryId)
                    .ToListAsync();

                if (!enquiryPurchaseVM.Cancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateMreqBalanceBeforeRevertAsync(sub);
                    }
                }

                existingEnq.Cancel = enquiryPurchaseVM.Cancel;
                existingEnq.CancellationRemarks = enquiryPurchaseVM.CancellationRemarks;
                existingEnq.CancelDate = enquiryPurchaseVM.CancelDate;
                existingEnq.CancelBy = enquiryPurchaseVM.CancelBy;


                await _unitOfWork.EnquiryPurchases.UpdateAsync(existingEnq);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingEnq.Cancel)
                    {
                        if (sub.RefMReqSubId.Value > 0)
                        {
                            await AdjustMaterialReqBalanceAsync(sub.RefMReqSubId.Value,sub.Qty.GetValueOrDefault(),0,$"Purchase Enquiry Cancelled - {existingEnq.EnquiryNo}");
                        }

                        await UpdateEnquiryPurchaseVendorAssignItemStatus(sub.EnquirySubId, 3);
                    }
                    else
                    {
                        if (sub.RefMReqSubId.Value > 0)
                        {
                            await AdjustMaterialReqBalanceAsync(sub.RefMReqSubId.Value, 0, sub.Qty.GetValueOrDefault(), $"Purchase Enquiry Reverted - {existingEnq.EnquiryNo}");
                        }

                        await UpdateEnquiryPurchaseVendorAssignItemStatus(sub.EnquirySubId, 1);

                    }
                }

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

        public async Task<decimal> GetMreqBalQtyByMreqSubId(int mreqSubId)
        {
            try
            {

                return await _unitOfWork.MaterialReqSubs.GetQueryable()
               .Where(e => e.MreqSubId == mreqSubId)
               .Select(e => e.BalQty)
               .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching BalQty for MreqSubId: {mreqSubId}");
                throw new InvalidOperationException("Failed to retrieve Material requisition balance quantity.");
            }
        }

        public async Task<EnquiryPurchaseVM> UpsertEnquiryPurchaseAsync(EnquiryPurchaseVM enquiryPurchaseVM)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                EnquiryPurchase entity;

                if (enquiryPurchaseVM.EnquiryId == 0)
                {
                    entity = _mapper.Map<EnquiryPurchase>(enquiryPurchaseVM);

                    var NextNumber = await _unitOfWork.EnquiryPurchases.GetLastEnqNoAsync(entity.Suffix);
                    entity.EnquiryNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.EnquiryPurchaseSub = enquiryPurchaseVM.EnquiryPurchaseSubVMs
                        .Select(subVM => _mapper.Map<EnquiryPurchaseSub>(subVM))
                        .ToList();

                    await _unitOfWork.EnquiryPurchases.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var subVM in enquiryPurchaseVM.EnquiryPurchaseSubVMs)
                    {
                        if (subVM.RefMReqSubId > 0)
                            await AdjustMaterialReqBalanceAsync(subVM.RefMReqSubId, 0, subVM.Qty ?? 0, "Purchase Enquiry Creation");
                    }

                    if (enquiryPurchaseVM.SelectedVendors != null && enquiryPurchaseVM.SelectedVendors.Any())
                    {
                        await AutoManageVendorAssignmentsAsync(entity.EnquiryId, enquiryPurchaseVM.SelectedVendors);
                    }

                    changes.AppendLine($"Enquiry Created by {currentUser}. Enquiry Number: {entity.EnquiryNo}");
                }
                else
                {
                    entity = await _unitOfWork.EnquiryPurchases
                        .GetQueryable()
                        .Include(e => e.EnquiryPurchaseSub)
                        .FirstOrDefaultAsync(e => e.EnquiryId == enquiryPurchaseVM.EnquiryId)
                        ?? throw new InvalidOperationException("Enquiry not found.");

                    var parentChanges = GetPropertyChanges(entity, enquiryPurchaseVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(enquiryPurchaseVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, enquiryPurchaseVM.EnquiryPurchaseSubVMs, changes);

                    await AutoManageVendorAssignmentsAsync(entity.EnquiryId, enquiryPurchaseVM.SelectedVendors);

                    await _unitOfWork.EnquiryPurchases.UpdateAsync(entity);
                }

                await _unitOfWork.SaveAsync();

                await UpdateEnquiryTallyStatusAsync(entity.EnquiryId);

                await transaction.CommitAsync();

                var savedEntity = await _unitOfWork.EnquiryPurchases
                    .GetQueryable()
                    .Include(e => e.EnquiryPurchaseSub)
                    .ThenInclude(s => s.Item)
                    .FirstOrDefaultAsync(e => e.EnquiryId == entity.EnquiryId);
                return _mapper.Map<EnquiryPurchaseVM>(savedEntity!);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Error in UpsertEnquiryPurchaseAsync for EnquiryId: {enquiryPurchaseVM.EnquiryId}");
                throw;
            }
        }

        public async Task AutoManageVendorAssignmentsAsync(int enquiryId, List<VendorVM> selectedVendors)
        {
            try
            {
                var subItems = await _unitOfWork.EnquiryPurchaseSubs.GetQueryable()
                    .Where(x => x.EnquiryId == enquiryId)
                    .ToListAsync();

                if (subItems == null || !subItems.Any())
                    return;

                var existingAssignments = await _unitOfWork.EnquiryPurchaseVendorAssigns.GetQueryable()
                    .Where(x => x.EnquiryId == enquiryId)
                    .ToListAsync();

                var selectedVendorCodes = selectedVendors.Select(s => s.VendorCode).ToList();

                var deleteList = existingAssignments
                    .Where(a => !selectedVendorCodes.Contains(a.VendorCode.Value))
                    .ToList();

                if (deleteList.Any())
                {
                    await _unitOfWork.EnquiryPurchaseVendorAssigns.DeleteRangeAsync(deleteList);
                    await _unitOfWork.SaveAsync();
                }

                foreach (var vendor in selectedVendors)
                {
                    foreach (var sub in subItems)
                    {
                        var existing = existingAssignments
                            .FirstOrDefault(a =>
                                a.EnquirySubId == sub.EnquirySubId &&
                                a.VendorCode == vendor.VendorCode
                            );

                        if (existing == null)
                        {
                            var newAssign = new EnquiryPurchaseVendorAssign
                            {
                                EnquiryId = enquiryId,
                                EnquirySubId = sub.EnquirySubId,
                                VendorCode = vendor.VendorCode,

                                Qty = sub.Qty.GetValueOrDefault(),
                                BalQty = sub.Qty.GetValueOrDefault(),

                                ItemStatus = 1,
                                Rate = null,
                                Remarks = null
                            };

                            await _unitOfWork.EnquiryPurchaseVendorAssigns.CreateAsync(newAssign);
                        }
                        else
                        {
                            existing.Qty = sub.Qty.GetValueOrDefault();
                            existing.BalQty = sub.Qty.GetValueOrDefault();

                            if(existing.ItemStatus <= 2)
                            {
                                existing.ItemStatus = 1;
                            }

                            await _unitOfWork.EnquiryPurchaseVendorAssigns.UpdateAsync(existing);
                        }

                        await _unitOfWork.SaveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"[AutoManageVendorAssignmentsAsync] Failed to auto manage vendor assignments for EnquiryId: {enquiryId}");
                throw new InvalidOperationException("Error auto-managing vendor assignments. Please contact system administrator.");
            }
        }

        private async Task AdjustMaterialReqBalanceAsync(int? refreqSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refreqSubId.HasValue || refreqSubId == 0) return;

                var mreqSub = await _unitOfWork.MaterialReqSubs.GetAsync(refreqSubId.Value);
                if (mreqSub == null) return;

                if (oldQty > 0)
                    mreqSub.BalQty += oldQty;

                if (newQty > mreqSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Material Req. BalQty.");

                if (newQty > 0)
                    mreqSub.BalQty -= newQty;

                await _unitOfWork.MaterialReqSubs.UpdateAsync(mreqSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Where(e => e.MreqId == mreqSub.MreqId && !e.ItemCancel)
                    .SumAsync(e => e.BalQty);

                var MaterialReq = await _unitOfWork.MaterialReqs.GetAsync(mreqSub.MreqId);
                if (MaterialReq != null)
                {
                    MaterialReq.PRTally = (totalBalQty == 0);
                    await _unitOfWork.MaterialReqs.UpdateAsync(MaterialReq);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[AdjustMaterialReqBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[AdjustMaterialReqBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Material Req Balance. Please contact support.");
            }
        }

        private async Task HandleChildUpdatesAsync(EnquiryPurchase existingEnq, List<EnquiryPurchaseSubVM> incomingSubVMs, StringBuilder changes)
        {
            var existingSubIds = existingEnq.EnquiryPurchaseSub.Select(s => s.EnquirySubId).ToHashSet();
            var incomingSubIds = incomingSubVMs.Select(s => s.EnquirySubId).ToHashSet();

            // DELETE removed children
            foreach (var sub in existingEnq.EnquiryPurchaseSub.Where(s => !incomingSubIds.Contains(s.EnquirySubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - EnqSubId: {sub.EnquirySubId}, Item: {sub.Item?.ItemCode}");
                await _unitOfWork.EnquiryPurchaseSubs.DeleteAsync(sub.EnquirySubId);
                await _unitOfWork.SaveAsync();

                if (sub.RefMReqSubId > 0)
                    await AdjustMaterialReqBalanceAsync(sub.RefMReqSubId, sub.Qty.GetValueOrDefault(), 0, "Purchase Enquiry Deletion");
            }

            // ADD or UPDATE children
            foreach (var subVM in incomingSubVMs)
            {
                if (subVM.EnquirySubId == 0)
                {
                    var newSub = _mapper.Map<EnquiryPurchaseSub>(subVM);
                    newSub.EnquiryId = existingEnq.EnquiryId;

                    await _unitOfWork.EnquiryPurchaseSubs.CreateAsync(newSub);
                    await _unitOfWork.SaveAsync();

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                    if (subVM.RefMReqSubId > 0)
                        await AdjustMaterialReqBalanceAsync(subVM.RefMReqSubId, 0, subVM.Qty ?? 0, "Enquiry Purchase Creation");

                }
                else
                {
                    var existingSub = existingEnq.EnquiryPurchaseSub.FirstOrDefault(s => s.EnquirySubId == subVM.EnquirySubId);
                    if (existingSub != null)
                    {
                        if (subVM.RefMReqSubId > 0)
                            await AdjustMaterialReqBalanceAsync(subVM.RefMReqSubId, existingSub.Qty.GetValueOrDefault(), subVM.Qty ?? 0, "Enquiry Purchase Update");

                        var subChanges = GetPropertyChanges(existingSub, subVM);
                        if (!string.IsNullOrEmpty(subChanges))
                            changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                        _mapper.Map(subVM, existingSub);
                    }
                }
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
                screen: "Enquiry Purchase",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<IEnumerable<EnquiryPurchaseVM>> GetAllEnquirySalesAsync()
        {
            var entityList = await _unitOfWork.EnquiryPurchases
                         .GetQueryable()
                         .AsSplitQuery()
                         .Include(e => e.EnquiryPurchaseVendorAssigns)
                             .ThenInclude(v => v.Vendor)
                         .Include(e => e.EnquiryPurchaseVendorAssigns)
                         .Include(e => e.EnquiryPurchaseSub)
                             .ThenInclude(s => s.Item)
                         .Include(e => e.EnquiryPurchaseSub)
                             .ThenInclude(s => s.CostCenter)
                         .OrderByDescending(e => e.EnquiryId)
                         .ToListAsync();


            var vmList = _mapper.Map<List<EnquiryPurchaseVM>>(entityList);

            foreach (var vm in vmList)
            {
                var pendingExists = await _unitOfWork.EnquiryPurchaseVendorAssigns
                .GetQueryable()
                .AnyAsync(s => s.EnquiryId == vm.EnquiryId && s.ItemStatus == 0);

                bool pendingExist = await _unitOfWork.EnquiryPurchaseVendorAssigns
                                  .GetQueryable()
                                  .AnyAsync(s => s.EnquiryId == vm.EnquiryId);

                vm.EnquiryShortClose = pendingExist;
            }

            return vmList;  // ✔ IEnumerable
        }

        public async Task<bool> DeleteEnquiryAsync(int enquiryId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var enquiry = await _unitOfWork.EnquiryPurchases
                    .GetQueryable()
                    .Include(e => e.EnquiryPurchaseSub)
                    .FirstOrDefaultAsync(e => e.EnquiryId == enquiryId);

                if (enquiry == null)
                    return false;

                foreach (var sub in enquiry.EnquiryPurchaseSub)
                {
                    if (sub.RefMReqSubId > 0 && sub.Qty.HasValue)
                    {
                        await AdjustMaterialReqBalanceAsync(sub.RefMReqSubId,sub.Qty.Value,0,"Purchase Enquiry Deletion");
                    }
                }

                await _unitOfWork.EnquiryPurchases.DeleteAsync(enquiry);
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase Enquiry List",
                    action: $"Deleted Purchase Enquiry: {enquiry.EnquiryNo}",
                    additionalInfo: $"Enquiry Id: {enquiry.EnquiryId}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete Purchase EnquiryId: {enquiryId}");
                throw;
            }
        }

        #endregion

        #region Enquiry Subitem Operations

        public async Task<List<Dictionary<string, object>>> GetMreqDetails(bool isPurchOrSubCon)
        {
            try
            {

                var result = await (from e in _unitOfWork.MaterialReqs.GetQueryable()
                                    join es in _unitOfWork.MaterialReqSubs.GetQueryable()
                                        on e.MreqId equals es.MreqId
                                    join c in _unitOfWork.CostCenters.GetQueryable()
                                       on es.CostId equals c.ProjectTypeId into costGroup
                                    from c in costGroup.DefaultIfEmpty()
                                    where es.BalQty > 0 &&
                                           e.IsAuthorized &&
                                           e.PRTally == false &&
                                           !e.MreqCancel &&
                                           !e.ShortClose &&
                                           e.IsPurchOrSubCon == isPurchOrSubCon &&
                                           (es.ItemCancel == false || es.ItemCancel == null)
                                    select new
                                    {
                                        es.MreqSubId,
                                        e.MReqNo,
                                        e.Suffix,
                                        e.MreqDate,
                                        es.ItemId,
                                        es.Item.ItemCode,
                                        es.Item.ItemName,
                                        es.Item.MeasureUnit,
                                        es.Item.HSNCode,
                                        es.PurchQty,
                                        es.BalQty,
                                        es.DueDate,
                                        es.CostId,
                                        es.CostCenter.ProjectNo


                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["MatReqSubId"] = r.MreqSubId,
                    ["MreqNo"] = $"{r.MReqNo}{r.Suffix}",

                    ["MreqDate"] = r.MreqDate == DateTime.MinValue
                    ? string.Empty
                    : r.MreqDate.ToString("dd/MM/yyyy"),

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit,
                    ["HSNCODE"] = r.HSNCode,
                    ["Qty"] = r.PurchQty,
                    ["BalQty"] = r.BalQty,
                    ["Rate"] = null,
                    ["CostCenterId"] = r.CostId,

                    ["DueDate"] = r.DueDate == DateTime.MinValue
                         ? string.Empty
                         : r.DueDate.ToString("dd/MM/yyyy"),

                    ["CostCenterName"] = r.ProjectNo ?? string.Empty
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching PR details");
                throw new InvalidOperationException("Failed to retrieve PR details. Please try again.");
            }
        }

        public async Task ValidateBeforeRevertAsync(int enqSubId)
        {
            try
            {
                var sub = await _unitOfWork.EnquiryPurchaseSubs.GetAsync(enqSubId);

                if (sub == null)
                    throw new InvalidOperationException("Purchase Enquiry Item not found.");

                if (sub.RefMReqSubId > 0)
                    await ValidateMreqBalanceBeforeRevertAsync(sub);

            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "[ValidateBeforeRevertAsync]");
                throw new InvalidOperationException("Failed to validate item cancel/revert. Please contact support.");
            }
        }

        public async Task<(bool CanDelete, string Message)> CheckAnyTransectionsExists(int enquirySubId)
        {
            try
            {
                var list = await _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetQueryable()
                    .Where(x => x.EnquirySubId == enquirySubId)
                    .ToListAsync();

                if (!list.Any())
                {
                    return (true, "No vendor assignments found. Safe to delete.");
                }

                var totalQty = list.Sum(x => x.Qty);
                var totalBalQty = list.Sum(x => x.BalQty);

                if (totalQty == totalBalQty)
                {
                    return (true, "All assigned quantities are unused. Safe to delete.");
                }

                return (false, "Cannot Cancel/delete. Item quantity has already been used.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in CheckAnyTransectionsExists");
                throw;
            }
        }

        public async Task<(bool CanDelete, string Message)> CanRemoveEnquiryAsync(int EnquiryId, int enqsubid)
        {
            try
            {
                var enquirySubIds = await _unitOfWork.EnquiryPurchases
                                     .GetQueryable()
                                     .Where(s => s.EnquiryId == EnquiryId)
                                     .SelectMany(s => s.EnquiryPurchaseSub.Select(sub => sub.EnquirySubId))
                                     .ToListAsync();

                if (!enquirySubIds.Any())
                    return (true, "Purchase Enquiry can be safely deleted.");

                bool Quotation = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .AnyAsync(qs => enquirySubIds.Contains(qs.RefEnqVendorAssignId.Value));

                if (Quotation)
                    return (false, "Cannot delete this Enquiry  as a some transaction Made.");

                return (true, "Enquiry can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanDeleteEnquiryAsync for EnquiryId: {EnquiryId}");
                throw new Exception("Error checking Purchase Enquiry  delete eligibility", ex);
            }
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(EnquiryPurchaseSubVM subItem, int enqId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.EnquiryPurchaseSubs.GetQueryable().Where
                                (x => x.EnquirySubId == subItem.EnquirySubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with EnquirySubId {subItem.EnquirySubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateMreqBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;

                await _unitOfWork.EnquiryPurchaseSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    await AdjustMaterialReqBalanceAsync(subEntity.RefMReqSubId,subEntity.Qty.GetValueOrDefault(),0,$"Purchase Enquiry Item Cancel - {subItem.ItemCode}");

                    await UpdateEnquiryPurchaseVendorAssignItemStatus(subItem.EnquirySubId, 3);
                }
                else
                {
                    await AdjustMaterialReqBalanceAsync(subEntity.RefMReqSubId,0,subEntity.Qty.GetValueOrDefault(),$"Purchase Enquiry Item Revert Cancel - {subItem.ItemCode}");
                    await UpdateEnquiryPurchaseVendorAssignItemStatus(subItem.EnquirySubId, 1);
                }

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

        private async Task UpdateEnquiryPurchaseVendorAssignItemStatus(int enquirySubId, byte newStatus)
        {
            try
            {
                var assignRows = await _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetQueryable()
                    .Where(x => x.EnquirySubId == enquirySubId)
                    .ToListAsync();

                if (assignRows == null || assignRows.Count == 0)
                    return;

                foreach (var row in assignRows)
                {
                    row.ItemStatus = newStatus;
                }

                await _unitOfWork.EnquiryPurchaseVendorAssigns.UpdateRangeAsync(assignRows);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _loggingService.LogDeveloperError(ex,$"[UpdateEnquiryPurchaseVendorAssignItemStatus] Validation error for EnquirySubId {enquirySubId}");
                throw;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"[UpdateEnquiryPurchaseVendorAssignItemStatus] Unexpected error for EnquirySubId {enquirySubId}");
                throw new InvalidOperationException("Failed to update vendor item status. Please contact support.");
            }
        }

        public async Task UpdateEnquiryTallyStatusAsync(int enqId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetQueryable()
                    .Where(x => x.EnquiryId == enqId && x.ItemStatus == 1)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var enquiry = await _unitOfWork.EnquiryPurchases.GetAsync(enqId);
                if (enquiry == null)
                    return;

                enquiry.EnquiryTally = (totalBalQty == 0);

                await _unitOfWork.EnquiryPurchases.UpdateAsync(enquiry);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[UpdateQuotationTallyStatusAsync] Error updating EnquiryId {enqId}");
                throw new InvalidOperationException("Failed to update Enquiry Tally status. Please contact support.");
            }
        }

        public async Task ValidateMreqBalanceBeforeRevertAsync(EnquiryPurchaseSub sub)
        {
            try
            {
                if (sub.RefMReqSubId.GetValueOrDefault() <= 0)
                    return;

                var entity = await _unitOfWork.MaterialReqSubs.GetAsync(sub.RefMReqSubId.Value);

                if (entity == null)
                    throw new InvalidOperationException($"Material Requisition not found for RefMReqSubId: {sub.RefMReqSubId}");

                if (entity.BalQty < sub.Qty)
                {
                    throw new InvalidOperationException($"Cannot revert because Material Requisition balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
                }
            }
            catch (InvalidOperationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[ValidateMreqBalanceBeforeRevertAsync] Error for RefMReqSubId {sub.RefMReqSubId}");
                throw new InvalidOperationException("An error occurred while validating Material Requisition balance. Please contact support.");
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteEnquiryAsync(int enquiryId)
        {
            try
            {
                var enquirySubIds = await _unitOfWork.EnquiryPurchaseSubs
                    .GetQueryable()
                    .Where(s => s.EnquiryId == enquiryId)
                    .Select(s => s.EnquirySubId)
                    .ToListAsync();

                if (!enquirySubIds.Any())
                    return (true, "Purchase Enquiry can be safely deleted (no sub-items).");

                bool hasQuote = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .AnyAsync(q => enquirySubIds.Contains(q.RefEnqVendorAssignId.Value));

                if (hasQuote)
                    return (false, "Cannot cancel/delete this Enquiry because quotations exist.");

                var vendorSummary = await _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetQueryable()
                    .Where(v => enquirySubIds.Contains(v.EnquirySubId.Value))
                    .GroupBy(g => 1)
                    .Select(g => new
                    {
                        TotalQty = g.Sum(x => x.Qty),
                        TotalBalQty = g.Sum(x => x.BalQty)
                    })
                    .FirstOrDefaultAsync();

                if (vendorSummary != null && vendorSummary.TotalQty != vendorSummary.TotalBalQty)
                {
                    return (false, "Cannot delete/Cancel this Enquiry because vendor assignments have transactions.");
                }

                var enquiryMeta = await _unitOfWork.EnquiryPurchases
                    .GetQueryable()
                    .Where(e => e.EnquiryId == enquiryId)
                    .Select(e => new
                    {
                        e.Cancel,
                        SubItems = e.EnquiryPurchaseSub.Select(s => new
                        {
                            s.EnquirySubId,
                            s.ItemCancel
                        }).ToList()
                    })
                    .FirstOrDefaultAsync();

                if (enquiryMeta == null)
                    return (false, "Enquiry not found.");

                if (enquiryMeta.Cancel)
                    return (false, "Enquiry is already cancelled and cannot be deleted.");

                if (enquiryMeta.SubItems.Any(s => s.ItemCancel))
                    return (false, "One or more sub-items are cancelled. Cannot delete enquiry.");

                return (true, "Purchase Enquiry can be safely deleted.");

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanDeleteEnquiryAsync, EnquiryId={enquiryId}");
                throw new Exception("Error checking Purchase Enquiry delete eligibility", ex);
            }
        }

        public async Task DeleteAndResequenceAsync(EnquiryPurchaseSubVM subitem, EnquiryPurchaseVM enquiryPurchaseVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                if (subitem.EnquirySubId == 0)
                {
                    enquiryPurchaseVM.EnquiryPurchaseSubVMs.Remove(subitem);
                    await ResetSlno(enquiryPurchaseVM);
                    return;
                }

                var entity = await _unitOfWork.EnquiryPurchaseSubs.GetAsync(subitem.EnquirySubId);
                if (entity == null)
                    throw new InvalidOperationException("Sub item not found.");

                if (entity.RefMReqSubId > 0)
                {
                    await AdjustMaterialReqBalanceAsync(entity.RefMReqSubId,entity.Qty.Value,0,"Purchase Enquiry Deletion");
                }

                await _unitOfWork.EnquiryPurchaseSubs.DeleteAsync(entity);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    await _currentUserService.GetUsernameAsync(),
                    _currentUserService.MachineName,
                    _currentUserService.IpAddress,
                    "Purchase Enquiry",
                    $"Deleted Item: {subitem.ItemCode}",
                    $"Enquiry No: {enquiryPurchaseVM?.EnquiryNo}"
                );

                var remaining = await _unitOfWork.EnquiryPurchaseSubs
                    .GetQueryable()
                    .Where(x => x.EnquiryId == enquiryPurchaseVM.EnquiryId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                    item.SlNo = slno++;

                await _unitOfWork.SaveAsync();

                await UpdateEnquiryTallyStatusAsync(enquiryPurchaseVM.EnquiryId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<List<EnquiryPurchaseSub>> GetEnqPurchSubByEnqIdAsync(int enqId)
        {
            try
            {
                var subs = await _unitOfWork.EnquiryPurchaseSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.EnquiryId == enqId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Enquiry Purchase Sub items for EnquiryId: {enqId}");
                throw new InvalidOperationException("Failed to retrieve Enquiry sub-items. Please try again.");
            }
        }

        private async Task ResetSlno(EnquiryPurchaseVM enquiry)
        {
            int slno = 1;
            foreach (var item in enquiry.EnquiryPurchaseSubVMs.OrderBy(x => x.SlNo))
                item.SlNo = slno++;
        }


        public async Task<(List<EnquiryPurchaseVM> enquiryPurchaseVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.EnquiryPurchases
                    .GetQueryable()
                    .AsSplitQuery()
                    .Include(e => e.EnquiryPurchaseVendorAssigns)
                        .ThenInclude(v => v.Vendor)
                    .Include(e => e.EnquiryPurchaseSub)
                        .ThenInclude(s => s.MaterialReqSub)
                            .ThenInclude(m => m.MaterialReq)
                    .Include(e => e.EnquiryPurchaseSub)
                        .ThenInclude(s => s.Item)
                    .Include(e => e.EnquiryPurchaseSub)
                        .ThenInclude(s => s.CostCenter)
                        .AsQueryable();

            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {
                    query = EnquiryFilterBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.EnquiryId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<EnquiryPurchaseVM>>(list);

            return (vmList, total);
        }

        public static class EnquiryFilterBuilder
        {
            public static IQueryable<EnquiryPurchase> ApplyFilter(
                IQueryable<EnquiryPurchase> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    
                    case "EnquiryNo":
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
                                (string.IsNullOrEmpty(part1) || x.EnquiryNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }

                    case "MreqNo":
                        {
                            string part1 = val;
                            string part2 = string.Empty;

                            int slashIndex = val.IndexOf('/');
                            if (slashIndex > -1)
                            {
                                part1 = val[..slashIndex].Trim();
                                part2 = val[(slashIndex + 1)..].Trim();
                            }

                            return query.Where(x =>
                                x.EnquiryPurchaseSub.Any(s =>
                                    s.MaterialReqSub != null &&
                                    s.MaterialReqSub.MaterialReq != null &&
                                    (string.IsNullOrEmpty(part1) ||
                                        s.MaterialReqSub.MaterialReq.MReqNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) ||
                                        s.MaterialReqSub.MaterialReq.Suffix.StartsWith(part2)))
                            );
                        }

                    case "VendorName":
                        return query.Where(x => x.EnquiryPurchaseVendorAssigns
                            .Any(s => s.Vendor.VendorName.Contains(val)));

                    case "ItemCode":
                        return query.Where(x => x.EnquiryPurchaseSub
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.EnquiryPurchaseSub
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.EnquiryDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.EnquiryDateNow <= toDate);
                        break;
                }

                return query;
            }

            private static IQueryable<EnquiryPurchase> ApplyStatusFilter(
                IQueryable<EnquiryPurchase> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.EnquiryTally == true),
                    "Short Closed" => query.Where(x => x.EnquiryShortClose == true),
                    "Cancelled" => query.Where(x => x.Cancel == true),
                    "Pending" => query.Where(x => x.EnquiryTally == false && x.Cancel == false && x.EnquiryShortClose == false),
                    _ => query
                };
            }
        }

        #endregion




        public async Task<List<PurchaseEnquiryPendingList>> GetPurchaseandSuContractEnquiryPendingList(string status)//Shankar
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<PurchaseEnquiryPendingList>("Sp_GetPurchasesandSubcontractEnquiryPendingList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }
    }

}
