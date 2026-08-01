using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Vml.Office;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IPurchOrSubConPoService;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;

using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using Microsoft.Data.SqlClient;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.PurchOrSubConPoService
{
    public class PuchPoService : IPurchPoService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;

        public PuchPoService(
            IUnitOfWork unitOfWork,
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
        // 🔹 Consignee addresses
        public async Task<List<VendorInDirect>> GetConsigneeAddressesVendorAsync(int VendorCode)
            => await _commonService.GetConsigneeAddressesVendorAsync(VendorCode);

        public async Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds)
            => await _commonService.GetCostCenterDetailsByCustId(custId, usedCostCenterIds);

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
            => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);
        // 🔹 Currency
        public async Task<List<Currency>> GetCurrenciesAsync()
            => (await _commonService.GetCurrenciesAsync()).ToList();
        public async Task<Currency?> GetCurrencyByIdAsync(int currId)
            => (await _commonService.GetCurrencyByIdAsync(currId));
        // 🔹 Get latest currency rate (from CurrencyToday Service)
        public async Task<decimal?> GetLatestCurrencyValueAsync(int currId)
            => await _commonService.GetLatestCurrencyValueAsync(currId);
        // 🔹 Terms
        public async Task<List<TermsAndConditions>> GetTermsAsync()
            => await _commonService.GetAllActiveTermsAsync();
        public async Task<bool> IsItemROlDisplayEnabledAsync()
        => await _commonService.GetScreenPermissionsAsync("Purchase Order", "ROL");

        #endregion

        #region Purchpo Operations 

        public async Task<List<PurchPoSub>> GetPoSubByPoIdAsync(int poId)
        {
            try
            {
                var subs = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.PoId == poId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Purchase Order Sub items for POId: {poId}");
                throw new InvalidOperationException("Failed to retrieve Purchase Order sub-items. Please try again.");
            }
        }

        public async Task ValidateBeforeRevertAsync(int poSubId)
        {
            try
            {
                var sub = await _unitOfWork.PurchPoSubs.GetAsync(poSubId);

                if (sub == null)
                    throw new InvalidOperationException("Purchase PO Item not found.");

                if (sub.RefQuoteSubId > 0)
                    await ValidateQuotationBalanceBeforeRevertAsync(sub);

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

        public async Task<(bool CanItemCancel, string Message)> CanPoGRNItemCancelCheckAsync(PurchPoSubVM subItem)
        {
            try
            {
                bool hasGrn = await _unitOfWork.PurchaseGRNSubs
                    .GetQueryable()
                    .AnyAsync(qs => qs.RefPoSubId.HasValue && qs.RefPoSubId == subItem.PoSubId && !qs.ItemCancel);

                if (hasGrn)
                    return (false, "Cannot cancel this Item as a Purchase GRN transaction exists.");

                return (true, "Item can be safely Cancell.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanPoGRNItemCancelCheckAsync for PoSubId: {subItem.PoSubId}");
                throw new Exception("Error checking Purchase GRN Item cancel eligibility", ex);
            }
        }

        public async Task<bool> IsPoTransactionsMatchedAsync(int poId, PurchPoVM PurchPoVMs)
        {
            try
            {
                // ============================
                // CONDITION 1: Check GRN Transactions Exist
                // ============================
                var poSubIds = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(x => x.PoId == poId)
                    .Select(x => x.PoSubId)
                    .ToListAsync();

                bool hasTransactions = false;

                //if (poSubIds != null && poSubIds.Any())
                //{
                //    hasTransactions = await _unitOfWork.PurchaseGRNSubs
                //        .GetQueryable()
                //        .AnyAsync(pqs => poSubIds.Contains(pqs.RefPoSubId.Value));
                //}

                // ============================
                // CONDITION 2: Compare Qty & BalQty
                // ============================
                bool qtyMismatch = false;

                var list = PurchPoVMs?.PurchPoSubVMs;

                if (list != null && list.Count > 0)
                {
                    decimal totalQty = 0, totalBalQty = 0;

                    foreach (var item in list)
                    {
                        totalQty += item.Qty.GetValueOrDefault();
                        totalBalQty += item.BalQty.GetValueOrDefault();
                    }

                    qtyMismatch = totalQty != totalBalQty;
                }

                // ============================
                // RESULT: If ANY condition true
                // ============================
                return hasTransactions || qtyMismatch;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,
                    $"Error while checking transactions for PoId: {poId}");

                throw new InvalidOperationException("Failed to verify PO transactions.");
            }
        }


        public async Task<PurchPoVM?> GetPurchPoByIdAsync(int PoId)
        {
            try
            {
                var entity = await _unitOfWork.PurchPos.GetQueryable()
                             .AsNoTracking().Include(q => q.PurchPoSubs)
                             .ThenInclude(s => s.Item)
                             .Include(q => q.PurchPoSubs)
                             .ThenInclude(s => s.CostCenter)
                             .Include(q => q.PurchPoSubs)
                             .ThenInclude(s => s.Process)
                             .Include(q => q.PurchPoSubs)
                             .ThenInclude(s => s.RouteCardS)
                             .Include(q => q.PurchPoSubs)
                             .ThenInclude(s => s.PurchaseQuoteSub)
                             .ThenInclude(s => s.PurchaseQuote)
                             .Include(q => q.PurchPoSubs)
                             .ThenInclude(s => s.MaterialReqSub)
                             .ThenInclude(s => s.MaterialReq)
                             .Include(q => q.Vendor)
                             .ThenInclude(c => c.VendorContacts)
                             .FirstOrDefaultAsync(q => q.PoId == PoId);

                var terms = await _unitOfWork.TermsAndConditions.GetQueryable()
                         .AsNoTracking()
                         .FirstOrDefaultAsync(t => t.Id == entity.TermsId);

                var vl = _mapper.Map<PurchPoVM?>(entity);

                if (terms != null)
                    vl.Details = terms.Details;

                return _mapper.Map<PurchPoVM?>(vl);

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in getGetPurchPoByIdAsync()");
                throw;
            }
        }
        public async Task<PurchPo?> GetLastPoAsync(int VendorCode)
        {
            try
            {
                return await _unitOfWork.PurchPos.GetLatestAsync(
                    q => q.VendorCode == VendorCode,
                    q => q.PoId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in GetLastPoAsync for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve last MfgPo. Please try again.");
            }
        }

        public async Task<int> GetPendingQuoteCountAsync(int VendorCode)
        {
            try
            {
                return await _unitOfWork.PurchaseQuotes
                .GetQueryable()
                .Include(e => e.PurchaseQuoteSub)
                .Where(e => e.VendorCode == VendorCode && e.QuoteShortClose == false && e.QuotationTally == false && e.PurchaseQuoteSub.Any(s =>
                 s.ItemCancel == false && s.BalQty > 0))
                .CountAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in GetPendingQuoteCountAsync for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve GetPendingQuoteCountAsync Please try again.");
            }

        }
        public async Task<PurchPoSubVM?> GetPoSubItemDetailByPoSubIdAsync(int PoSubId)
        {
            try
            {
                return await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(q => q.PoSubId == PoSubId)
                    .Select(q => new PurchPoSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Po sub item detail for PoSubId: {PoSubId}");
                throw new InvalidOperationException("Failed to retrieve Po sub-item details.");
            }
        }

        public async Task UpdateItemCancelAndAddorRevertAsync(PurchPoSubVM subItem, int poId)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subEntity = await _unitOfWork.PurchPoSubs.GetQueryable().Where
                                (x => x.PoSubId == subItem.PoSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with PoSubId {subItem.PoSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    if(subItem.RefQuoteSubId.GetValueOrDefault() > 0)
                        await ValidateQuotationBalanceBeforeRevertAsync(subEntity);

                    if (subItem.RefMReqSubId.GetValueOrDefault() > 0)
                        await ValidateMreqBalanceBeforeRevertAsync(subEntity);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.ItemCancelReason = subItem.ItemCancelReason;

                await _unitOfWork.PurchPoSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();


                if (subItem.ItemCancel)
                {
                    if (subItem.RefQuoteSubId.GetValueOrDefault() > 0)
                        await AdjustQuoteBalanceAsync(subEntity.RefQuoteSubId, subEntity.Qty, 0, $"Purchase PO Item Cancel - {subItem.ItemCode}");

                    if (subItem.RefMReqSubId.GetValueOrDefault() > 0)
                        await AdjustMreqBalanceAsync(subEntity.RefMReqSubId, subEntity.Qty, 0, $"Purchase PO Item Cancel - {subItem.ItemCode}");
                }
                else
                {
                    if (subItem.RefQuoteSubId.GetValueOrDefault() > 0)
                        await AdjustQuoteBalanceAsync(subEntity.RefQuoteSubId, 0, subEntity.Qty, $"Purchase Po Revert Item Cancel - {subItem.ItemCode}");

                    if (subItem.RefMReqSubId.GetValueOrDefault() > 0)
                        await AdjustMreqBalanceAsync(subEntity.RefMReqSubId, 0, subEntity.Qty, $"Purchase Po Revert Item Cancel - {subItem.ItemCode}");
                }

                await UpdatePoTallyStatusAsync(poId);

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


        public async Task DeleteAndResequenceAsync(PurchPoSubVM subitem, PurchPoVM poVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.PoSubId > 0) // persisted subitem
                {
                    var entity = await _unitOfWork.PurchPoSubs.GetAsync(subitem.PoSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    // Restore balance qty
                    if (entity.RefQuoteSubId > 0)
                    {
                        await AdjustQuoteBalanceAsync(subitem.RefQuoteSubId, entity.Qty, 0, "PO Deletion");
                    }
                    else if (entity.RefMReqSubId > 0)
                    {
                        await AdjustMreqBalanceAsync(subitem.RefMReqSubId, entity.Qty, 0, "PO Deletion");
                    }


                    // Delete from DB
                    await _unitOfWork.PurchPoSubs.DeleteAsync(entity);
                    await _unitOfWork.SaveAsync();

                    // Log action
                    await _loggingService.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Purchase PO",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"PO No: {poVM?.PONo}"
                    );
                }
                else
                {
                    // Not yet persisted → just remove from VM
                    poVM.PurchPoSubVMs.Remove(subitem);
                    return;
                }

                // Resequence persisted subitems
                var remaining = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(x => x.PoId == poVM.PoId)
                    .OrderBy(x => x.SlNo)
                    .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();

                await UpdatePoTallyStatusAsync(poVM.PoId);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }


        public async Task<List<PurchPoSubVM>> GetPOSubByPoIdAsync(int poId)
        {
            try
            {
                var subs = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.PoId == poId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<PurchPoSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Manufacturing PO items for PoId: {poId}");
                throw new InvalidOperationException("Failed to retrieve Sales Order sub-items. Please try again.");
            }
        }
        public async Task<decimal> GetmatReqItemBalQtyFromMreqSubId(int MatreqSubId)
        {
            try
            {
                return await _unitOfWork.MaterialReqSubs.GetQueryable()
                    .Where(e => e.MreqSubId == MatreqSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching BalQty for Purchposubid: {MatreqSubId}");
                throw new InvalidOperationException("Failed to retrieve Enquiry balance quantity.");
            }
        }

        public async Task<PurchPoSubVM?> GetPurchPoSubItemDetailByPoSubIdAsync(int poSubid)
        {
            try
            {
                return await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(q => q.PoSubId == poSubid)
                    .Select(q => new PurchPoSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Purchase Po sub item detail for poSubId: {poSubid}");
                throw new InvalidOperationException("Failed to retrieve Po sub-item details.");
            }
        }


        public async Task<decimal> GetQuoteItemBalQtyFromQuoteSubId(int QuoteSubId)
        {
            try
            {
                return await _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                    .Where(e => e.QuoteSubId == QuoteSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching BalQty for Purchposubid: {QuoteSubId}");
                throw new InvalidOperationException("Failed to retrieve Purchase Quatation balance quantity.");
            }
        }

        public async Task<PurchPoVM> RevisePODraftAsync(int poId)
        {
            try
            {
                var entity = await _unitOfWork.PurchPos.GetQueryable()
                            .AsNoTracking()
                            .Include(q => q.PurchPoSubs).ThenInclude(s => s.Item)
                            .Include(q => q.PurchPoSubs).ThenInclude(s => s.CostCenter)
                            .Include(q => q.PurchPoSubs).ThenInclude(s => s.PurchaseQuoteSub).ThenInclude(s => s.PurchaseQuote)
                            .Include(q => q.PurchPoSubs).ThenInclude(s => s.MaterialReqSub).ThenInclude(s => s.MaterialReq)
                            .Include(q => q.Vendor).ThenInclude(c => c.VendorContacts)
                            .FirstOrDefaultAsync(q => q.PoId == poId);

                if (entity == null)
                    throw new Exception("Purchase Order not found.");

                var lastRev = await _unitOfWork.PurchPos.GetQueryable()
                    .Where(x => x.RefRevisonPoid == poId || x.PoId == poId)
                    .MaxAsync(x => (int?)x.RevisionNo) ?? 0;

                var newRev = lastRev + 1;

                var newPo = _mapper.Map<PurchPo>(entity);

                newPo.PoId = 0;
                newPo.RevisionNo = newRev;
                newPo.IsRevision = true;
                newPo.PODate = DateTime.Now;
                newPo.ApprovalDate = null;
                newPo.RefRevisonPoid = poId;

                foreach (var sub in newPo.PurchPoSubs)
                {
                    sub.PoSubId = 0;
                    sub.PoId = 0;
                }

                var vm = _mapper.Map<PurchPoVM>(newPo);
                return vm;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to revise Purchase PO: {poId}");
                throw new InvalidOperationException("Failed to revise Purchase Order. Please try again.");
            }
        }

        public async Task<PurchPoVM> UpsertPurchaseOrderShortCloseAsync(PurchPoVM Purchpo)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                PurchPo entity;

                entity = await _unitOfWork.PurchPos
                             .GetQueryable()
                             .Include(e => e.PurchPoSubs)
                             .FirstOrDefaultAsync(e => e.PoId == Purchpo.PoId)
                             ?? throw new InvalidOperationException("Purchase Order not found.");

                _mapper.Map(Purchpo, entity);

                var parentChanges = GetPropertyChanges(entity, Purchpo);
                if (!string.IsNullOrEmpty(parentChanges))
                    changes.AppendLine("Parent Changes:\n" + parentChanges);

                _mapper.Map(Purchpo, entity);
                entity.ModifiedBy = currentUser;
                entity.ModifiedDate = now;

                await _unitOfWork.PurchPos.UpdateAsync(entity);

                await UpdatePoTallyStatusAsync(Purchpo.PoId);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
                await LogChangesAsync(changes, Purchpo.PoId == 0 ? "Short Closed purchase Order Created" : "ReOpen the purchase Purchase Order");

                // Return updated entity
                var savedEntity = await _unitOfWork.PurchPos
                    .GetQueryable()
                    .Include(e => e.PurchPoSubs).ThenInclude(s => s.Item)
                    .Include(e => e.PurchPoSubs).ThenInclude(s => s.CostCenter)
                    .Include(e => e.Vendor)
                    .FirstOrDefaultAsync(e => e.PoId == entity.PoId);

                return _mapper.Map<PurchPoVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Error in UpsertPurchaseOrderShortCloseAsync for PoId: {Purchpo.PoId}");
                throw;
            }

        }
        public async Task<PurchPoVM> UpsertPoAsync(PurchPoVM purchPoVM)
        {
            if (purchPoVM == null)
                throw new ArgumentNullException(nameof(purchPoVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                PurchPo entity;

                if (purchPoVM.PoId == 0)
                {
                    entity = _mapper.Map<PurchPo>(purchPoVM);
                    
                    if (!entity.IsRevision)
                    {
                        var nextPoNumber = await _unitOfWork.PurchPos.GetLastPONoAsync(entity.Suffix);
                        entity.PONo = nextPoNumber;
                    }

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.PurchPoSubs = purchPoVM.PurchPoSubVMs.Select(s => _mapper.Map<PurchPoSub>(s)).ToList();

                    if (entity.IsRevision && entity.RevisionNo.HasValue && entity.RevisionNo > 0 && entity.RefRevisonPoid.HasValue)
                    {
                        var previousPo = await _unitOfWork.PurchPos.GetQueryable()
                                        .Include(p => p.PurchPoSubs)
                                        .Where(x => x.PoId == entity.RefRevisonPoid.Value).FirstOrDefaultAsync();

                        if (previousPo != null)
                        {
                            foreach (var sub in previousPo.PurchPoSubs)
                            {
                                if (sub.RefQuoteSubId.GetValueOrDefault() > 0)
                                {
                                    await AdjustQuoteBalanceAsync(sub.RefQuoteSubId.Value, sub.Qty, 0, $"PO Revised - {entity.PONo}");
                                }

                                if (sub.RefMReqSubId.GetValueOrDefault() > 0)
                                {
                                    await AdjustMreqBalanceAsync(sub.RefMReqSubId.Value, sub.Qty, 0, $"PO Revised - {entity.PONo}");
                                }
                            }

                            previousPo.PoCancl = true;
                            previousPo.CancelReason = $"Auto closed due to revision - {entity.PONo}{entity.RevisionNo}{entity.Suffix}";

                            await _unitOfWork.PurchPos.UpdateAsync(previousPo);
                            await _unitOfWork.SaveAsync();
                        }

                    }

                    await SetPoAuthorizationStatusAsync(entity, currentUser);

                    await _unitOfWork.PurchPos.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    await LogChangesAsync(changes, "Purchase PO Created");


                    foreach (var subVm in purchPoVM.PurchPoSubVMs)
                    {
                        if (subVm.RefQuoteSubId > 0)
                        {
                            await AdjustQuoteBalanceAsync(subVm.RefQuoteSubId, 0, subVm.Qty ?? 0, "PO Creation");
                        }
                        else if (subVm.RefMReqSubId > 0)
                        {
                            await AdjustMreqBalanceAsync(subVm.RefMReqSubId, 0, subVm.Qty ?? 0, "PO Creation");
                        }
                    }
                    

                    changes.AppendLine("Purchase Order Created.");
                }
                else
                {
                    entity = await _unitOfWork.PurchPos.GetQueryable()
                        .Include(q => q.PurchPoSubs)
                        .FirstOrDefaultAsync(q => q.PoId == purchPoVM.PoId)
                        ?? throw new InvalidOperationException("PO not found.");

                    var parentChanges = GetPropertyChanges(entity, purchPoVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(purchPoVM, entity);

                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await SetPoAuthorizationStatusAsync(entity, currentUser);

                    await HandleChildUpdatesAsync(entity, purchPoVM.PurchPoSubVMs, changes);

                    changes.AppendLine("Purchase Order Updated.");
                }

                await _unitOfWork.SaveAsync();

                await UpdatePoTallyStatusAsync(purchPoVM.PoId);

                await transaction.CommitAsync();

                await LogChangesAsync(changes, purchPoVM.PoId == 0 ? "Purchase PO update" : "Purchase PO  Updated");
                
                var savedEntity = await _unitOfWork.PurchPos.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.PurchPoSubs).ThenInclude(s => s.Item)
                    .Include(q => q.Vendor)
                    .Include(q => q.Currency)
                    .Include(q => q.PurchPoSubs).ThenInclude(s => s.CostCenter)
                    .FirstOrDefaultAsync(q => q.PoId == entity.PoId);

                return _mapper.Map<PurchPoVM>(savedEntity!);
            }
            catch (Exception ex)

            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to upsert Purchase PO: {purchPoVM.PONo}");
                throw new InvalidOperationException("Failed to save Purchase PO. Please try again.");
            }
        }

        public async Task UpdatePoTallyStatusAsync(int poId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(x => x.PoId == poId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var po = await _unitOfWork.PurchPos.GetAsync(poId);
                if (po == null)
                    return;

                if (po.PoShortClose || po.PoCancl)
                    return;

                po.PoTally = (totalBalQty == 0);

                await _unitOfWork.PurchPos.UpdateAsync(po);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[UpdatePoTallyStatusAsync] Error updating PoId {poId}");
                throw new InvalidOperationException("Failed to update Purchase PO Tally status. Please contact support.");
            }
        }

        private async Task AdjustQuoteBalanceAsync(
            int? refQuoteSubId,
            decimal oldQty,
            decimal newQty,
            string context)
        {
            try
            {
                if (!refQuoteSubId.HasValue || refQuoteSubId == 0) return;

                var quoteSub = await _unitOfWork.PurchaseQuotesSubs.GetAsync(refQuoteSubId.Value);
                if (quoteSub == null) return;

                bool isRevert = newQty < oldQty; // AUTO DETECT REVERT MODE

                // ============================
                // STEP 1: Adjust BalQty
                // ============================
                if (oldQty > 0)
                    quoteSub.BalQty += oldQty;

                if (!isRevert && newQty > quoteSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed BalQty.");

                if (newQty > 0)
                    quoteSub.BalQty -= newQty;

                await _unitOfWork.PurchaseQuotesSubs.UpdateAsync(quoteSub);
                await _unitOfWork.SaveAsync();

                // ============================
                // STEP 2: Update Quotation Tally
                // ============================
                var totalBalQty = await _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                    .Where(e => e.QuoteId == quoteSub.QuoteId && !e.ItemCancel)
                    .SumAsync(e => e.BalQty);

                var quotation = await _unitOfWork.PurchaseQuotes.GetAsync(quoteSub.QuoteId);
                if (quotation != null)
                {
                    quotation.QuotationTally = (totalBalQty == 0);
                    await _unitOfWork.PurchaseQuotes.UpdateAsync(quotation);
                    await _unitOfWork.SaveAsync();
                }

                // ============================
                // STEP 3: Resolve Vendor Assignments
                // ============================
                if (!quoteSub.RefEnqVendorAssignId.HasValue)
                {
                    return;
                }
                else
                {
                    var enqVendorAssign = await _unitOfWork.EnquiryPurchaseVendorAssigns
                    .GetAsync(quoteSub.RefEnqVendorAssignId.Value);

                    IQueryable<EnquiryPurchaseVendorAssign> vendorQuery =
                        _unitOfWork.EnquiryPurchaseVendorAssigns
                        .GetQueryable()
                        .Where(x => x.EnquiryId == enqVendorAssign.EnquiryId &&
                                    x.EnquirySubId == enqVendorAssign.EnquirySubId);

                    // ONLY FILTER IN UPDATE MODE
                    if (!isRevert)
                    {
                        vendorQuery = vendorQuery.Where(x =>
                            x.EnqPurchVendorId != enqVendorAssign.EnqPurchVendorId);
                    }

                    var relatedAssigns = await vendorQuery.ToListAsync();

                    // ============================
                    // STEP 4: Update/Revert Statuses
                    // ============================
                    foreach (var assign in relatedAssigns)
                    {
                        var vendorQuoteSub = await _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                            .Where(x => x.RefEnqVendorAssignId == assign.EnqPurchVendorId)
                            .FirstOrDefaultAsync();

                        if (vendorQuoteSub == null)
                            continue;

                        if (isRevert)
                        {
                            if (quoteSub.BalQty == assign.Qty)
                            {
                                vendorQuoteSub.ItemShortClose = false;
                                assign.ItemStatus = 0;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else
                        {
                            // UPDATE MODE — short close others
                            vendorQuoteSub.ItemShortClose = true;
                            assign.ItemStatus = 4;
                        }

                        await _unitOfWork.PurchaseQuotesSubs.UpdateAsync(vendorQuoteSub);
                        await _unitOfWork.SaveAsync();

                        await _unitOfWork.EnquiryPurchaseVendorAssigns.UpdateAsync(assign);
                        await _unitOfWork.SaveAsync();

                    }
                }
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[AdjustQuoteBalance] Error in {context}");
                throw new InvalidOperationException("Failed to adjust Purchase Quote balance.");
            }
        }


        private async Task AdjustMreqBalanceAsync(int? refMreqSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refMreqSubId.HasValue || refMreqSubId == 0) return;

                var materialReqSub = await _unitOfWork.MaterialReqSubs.GetAsync(refMreqSubId.Value);
                if (materialReqSub == null) return;

                if (oldQty > 0)
                    materialReqSub.BalQty += oldQty;

                if (newQty > materialReqSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Materail Req BalQty.");

                if (newQty > 0)
                    materialReqSub.BalQty -= newQty;

                await _unitOfWork.MaterialReqSubs.UpdateAsync(materialReqSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.MaterialReqSubs
                    .GetQueryable()
                    .Where(e => e.MreqId == materialReqSub.MreqId && !e.ItemCancel)
                    .SumAsync(e => e.BalQty);

                var materialReq = await _unitOfWork.MaterialReqs.GetAsync(materialReqSub.MreqId);
                if (materialReq != null)
                {
                    materialReq.PRTally = (totalBalQty == 0);
                    await _unitOfWork.MaterialReqs.UpdateAsync(materialReq);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[AdjustMreqBalanceAsync] Validation failed in {context}");
                throw; 
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"[AdjustMreqBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust PR/MR balance. Please contact support.");
            }
        }

        public async Task<bool> IsDuplicatePoAsync(string poNo, string suffix, int? currentPoId = null, int? vendorcode = null, int? RevesionNo = null)
        {
            if (string.IsNullOrWhiteSpace(poNo))
                return false;

            try
            {
                return await _unitOfWork.PurchPos
                    .GetQueryable()
                    .AnyAsync(x => x.PONo == poNo
                                && x.Suffix == suffix
                                && x.VendorCode == vendorcode
                                && (currentPoId == null || x.PoId != currentPoId) && (x.RevisionNo == RevesionNo));
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in IsDuplicatePoAsync for PoNO: {poNo}");
                throw new InvalidOperationException("Failed to check duplicate PO.");
            }
        }

        private async Task SetPoAuthorizationStatusAsync(PurchPo entity, string currentUser)
        {
            var PoAuthorityExists = await _unitOfWork.UserAuthorities
                .AnyAsync(x => x.IsPO == true);

            if (!PoAuthorityExists)
            {
                entity.Authorized = true;
                entity.ApprovedBy = currentUser;
                entity.ApprovalDate = DateTime.Now;
            }
            else
            {
                entity.Authorized = false;
                entity.ApprovedBy = null;
                entity.ApprovalDate = null;

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

        public async Task<string> GetPrefixFromDbAsync(bool IsPurchase)
        {
            try
            {
                string prefix = string.Empty;
                if (IsPurchase)
                {
                    prefix = await _unitOfWork.PurchPos
                    .GetQueryable()
                    .AsNoTracking().Where(q=>q.PurchORSubCon)
                    .OrderByDescending(q => q.PoId)
                    .Select(q => q.Prefix)
                    .FirstOrDefaultAsync();

                }
                else
                {
                    prefix = await _unitOfWork.PurchPos
                   .GetQueryable()
                   .AsNoTracking().Where(q => !q.PurchORSubCon)
                   .OrderByDescending(q => q.PoId)
                   .Select(q => q.Prefix)
                   .FirstOrDefaultAsync();
                }
                    
                return prefix ?? string.Empty;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GetPrefixFromDbAsync()");
                return string.Empty;
            }
        }

        public async Task<string> GeneratePoNumberAsync()
        {
            try
            {

                var today = DateTime.Today;
                int fyStartYear = (today.Month >= 4) ? today.Year : today.Year - 1;
               
                var highest = await _unitOfWork.PurchPos
                            .GetQueryable()
                            .Where(e => e.PODate >= new DateTime(fyStartYear, 4, 1)
                                     && e.PODate <= new DateTime(fyStartYear + 1, 3, 31))
                            .Select(e => new { PONo = Convert.ToInt32(e.PONo) })
                            .OrderByDescending(e => e.PONo)
                            .Select(e => e.PONo)
                            .FirstOrDefaultAsync();

                if (string.IsNullOrEmpty(highest.ToString()))
                    return "0001"; // first enquiry number


                if (!int.TryParse(highest.ToString(), out int numericPart))
                    numericPart = 0;


                return (numericPart + 1).ToString("D4");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in GenerateEnquiryNumberAsync()");
                return string.Empty;
            }
        }

        
        public async Task UpdateOldMaterialReqSubBalQtyByID(int matReqSubId, decimal qtyToAdd)
        {
            var existing = await _unitOfWork.MaterialReqSubs
                .FirstOrDefaultAsync(m => m.MreqSubId == matReqSubId);

            if (existing != null)
            {
                existing.BalQty -= qtyToAdd;

                await _unitOfWork.MaterialReqSubs.UpdateAsync(existing);
                await _unitOfWork.SaveAsync();
            }

            var totalBalQty = await _unitOfWork.MaterialReqSubs
                          .GetQueryable()
                          .Where(e => e.MreqId == existing.MreqId)
                          .SumAsync(e => e.BalQty);

            var MaterilaReq = await _unitOfWork.MaterialReqs.GetAsync(existing.MreqId);
            if (MaterilaReq != null)
            {
                MaterilaReq.PRTally = (totalBalQty == 0);
                await _unitOfWork.MaterialReqs.UpdateAsync(MaterilaReq);
                await _unitOfWork.SaveAsync();

            }
        }
        public async Task UpdateOldQuotationSubBalQtyByID(int quotesubid, decimal qtyToAdd)
        {
            var existing = await _unitOfWork.PurchaseQuotesSubs
                .FirstOrDefaultAsync(m => m.QuoteSubId == quotesubid);

            if (existing != null)
            {
                existing.BalQty -= qtyToAdd;
                //existing.ItemStatus = "PO";
                await _unitOfWork.PurchaseQuotesSubs.UpdateAsync(existing);
                await _unitOfWork.SaveAsync();
            }

            var totalBalQty = await _unitOfWork.PurchaseQuotesSubs
                          .GetQueryable()
                          .Where(e => e.QuoteId == existing.QuoteId)
                          .SumAsync(e => e.BalQty);

            var MaterilaReq = await _unitOfWork.PurchaseQuotes.GetAsync(existing.QuoteId);
            if (MaterilaReq != null)
            {
                MaterilaReq.QuotationTally = (totalBalQty == 0);
                await _unitOfWork.PurchaseQuotes.UpdateAsync(MaterilaReq);
                await _unitOfWork.SaveAsync();

            }
        }


        private async Task HandleChildUpdatesAsync(PurchPo existingPo, List<PurchPoSubVM> incomingSubVMs, StringBuilder changes)
        {
            try
            {
                var existingSubIds = existingPo.PurchPoSubs.Select(s => s.PoSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.PoSubId).ToHashSet();

                foreach (var sub in existingPo.PurchPoSubs.Where(s => !incomingSubIds.Contains(s.PoSubId)).ToList())
                {
                    changes.AppendLine($"Child Deleted - EnquirySubId: {sub.PoSubId}, Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.PurchPoSubs.DeleteAsync(sub.PoSubId);
                    await _unitOfWork.SaveAsync();

                    if (sub.RefQuoteSubId.GetValueOrDefault() > 0)
                        await AdjustQuoteBalanceAsync(sub.RefQuoteSubId.Value, sub.Qty, 0, "PO Item Deletion");

                    if (sub.RefMReqSubId.GetValueOrDefault() > 0)
                        await AdjustMreqBalanceAsync(sub.RefMReqSubId.Value, sub.Qty, 0, "PO Item Deletion");
                }

                foreach (var subVM in incomingSubVMs)
                {
                    if (subVM.PoSubId == 0)
                    {
                        var newSub = _mapper.Map<PurchPoSub>(subVM);
                        newSub.PoId = existingPo.PoId;
                        await _unitOfWork.PurchPoSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                        if (subVM.RefQuoteSubId.GetValueOrDefault() > 0)
                            await AdjustQuoteBalanceAsync(subVM.RefQuoteSubId, 0, subVM.Qty ?? 0, "PO Creation");

                        else if (subVM.RefMReqSubId.GetValueOrDefault() > 0)
                            await AdjustMreqBalanceAsync(subVM.RefMReqSubId, 0, subVM.Qty ?? 0, "Po Creation");
                        
                    }
                    else
                    {
                        var existingSub = existingPo.PurchPoSubs.FirstOrDefault(s => s.PoSubId == subVM.PoSubId);
                        if (existingSub != null)
                        {

                            if (subVM.RefQuoteSubId.GetValueOrDefault() > 0)
                                await AdjustQuoteBalanceAsync(subVM.RefQuoteSubId, existingSub.Qty, subVM.Qty ?? 0, "Po Update");

                            if (subVM.RefMReqSubId.GetValueOrDefault() > 0)
                                await AdjustMreqBalanceAsync(subVM.RefMReqSubId, existingSub.Qty, subVM.Qty ?? 0, "PO Update");

                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");
                            _mapper.Map(subVM, existingSub);

                            await _unitOfWork.PurchPoSubs.UpdateAsync(existingSub);
                        }
                    }
                }
                await _unitOfWork.SaveAsync();
            }

            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error HandleChildUpdatesAsync sub item detail for PoId: {existingPo.PoId}");
                throw new InvalidOperationException("Failed to HandleChildUpdatesAsync.");
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
                screen: "Purchase Order",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        public async Task<IEnumerable<PurchPoVM>> GetAllPurchPoAsync()
        {
            try
            {
                return await _unitOfWork.PurchPos
                    .GetQueryable()
                    .Include(e => e.Vendor)
                    .OrderByDescending(c => c.PoId)
                    .ProjectTo<PurchPoVM>(_mapper.ConfigurationProvider)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Failed to get all EnquirySales");
                throw;
            }
        }

        public async Task<bool> DeletePOByPOIdAsync(int poId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var po = await _unitOfWork.PurchPos
                    .GetQueryable()
                    .Include(e => e.PurchPoSubs)
                    .FirstOrDefaultAsync(e => e.PoId == poId);

                if (po == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in po.PurchPoSubs)
                {
                    if (sub.RefQuoteSubId.GetValueOrDefault() > 0)
                        await AdjustQuoteBalanceAsync(sub.RefQuoteSubId.Value, sub.Qty, 0, "PO Item Deletion");
                    else if (sub.RefMReqSubId.GetValueOrDefault() > 0)
                        await AdjustMreqBalanceAsync(sub.RefMReqSubId.Value, sub.Qty, 0, "PO Item Deletion");
                }

                var deleted = await _unitOfWork.PurchPos.DeleteAsync(poId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase-Order List",
                    action: $"Deleted Purchase Po: {po.PONo}",
                    additionalInfo: $"Po Id: {po.PoId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, $"Failed to delete PO: {poId}");
                throw;
            }
        }

        #endregion

        #region MaterialReqs Subitem Operations

        public async Task<List<Dictionary<string, object>>> GetMaterialReqsByVendorCode(int VendorCode)
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
                                           e.PRTally == false &&
                                           !e.MreqCancel && !e.ShortClose &&
                                           !es.ItemCancel
                                           && e.IsAuthorized
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
                                        es.CostCenter.CostCenterName
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["MatReqSubId"] = r.MreqSubId,
                    ["MreqNo"] = $"{r.MReqNo}{r.Suffix}",
                    ["MreqDate"] = r.MreqDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit,
                    ["HSNCODE"] = r.HSNCode,
                    ["Qty"] = r.PurchQty,
                    ["BalQty"] = r.BalQty,
                    ["Rate"] = null,
                    ["CostCenterId"] = r.CostId,
                    ["DueDate"] = r.DueDate,
                    ["CostCenterName"] = r.CostCenterName ?? string.Empty
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching enquiry details for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve enquiry details. Please try again.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetQuoteDetailsByVendorCode(int VendorCode)
        {
            try
            {
                var result = await (from q in _unitOfWork.PurchaseQuotes.GetQueryable()
                                    join qs in _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                                        on q.QuoteId equals qs.QuoteId
                                    where q.VendorCode == VendorCode && !q.QuotationTally && !q.IsCancel && qs.BalQty > 0 && !qs.ItemShortClose && !qs.ItemCancel
                                    select new
                                    {
                                        qs.QuoteSubId,
                                        q.QuoteNo,
                                        q.Suffix,
                                        q.QuoteDate,
                                        qs.ItemId,
                                        qs.Item.ItemCode,
                                        qs.Item.ItemName,
                                        qs.Item.MeasureUnit,
                                        qs.Item.HSNCode,
                                        qs.Qty,
                                        qs.BalQty,
                                        qs.UnitPrice,
                                        CostCenterId = qs.CostId == 0 ? (int?)null : qs.CostId,
                                        qs.CostCenter.ProjectNo
                                    }).ToListAsync();

                return result.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,
                    ["QuoteSubId"] = r.QuoteSubId,
                    ["QuoteNo"] = $"{r.QuoteNo}{r.Suffix}",
                    ["QuoteDate"] = r.QuoteDate,
                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,
                    ["HSNCODE"] = r.HSNCode ?? string.Empty,
                    ["Qty"] = r.Qty,
                    ["BalQty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,
                    ["CostCenterId"] = r.CostCenterId ?? (int?)null,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                }).ToList();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Quotation details for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve Quotation details. Please try again.");
            }
        }
        public async Task<bool> HasAnyPurchPoTransactionMadeAsync(int Poid)
        {
            try
            {
                var PoSubIds = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(s => s.PoId == Poid)
                    .Select(s => s.PoSubId)
                    .ToListAsync();

                if (!PoSubIds.Any())
                    return false;

                return await _unitOfWork.PurchPoSubs

                    .GetQueryable()
                    .AnyAsync(qs => qs.RefQuoteSubId.HasValue && PoSubIds.Contains(qs.RefQuoteSubId.Value));
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in HasAnyPurchPoTransactionMadeAsync for EnquiryId: {Poid}");
                throw;
            }
        }

        public async Task UpdatedCancelStatusAndAddOrRevertQty(PurchPoVM poVM)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var existingPo = await _unitOfWork.PurchPos.GetAsync(poVM.PoId);
                if (existingPo == null)
                    throw new InvalidOperationException("Purchase PO not found.");

                var subs = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(s => s.PoId == poVM.PoId)
                    .ToListAsync();

                if (!poVM.PoCancl)
                {
                    foreach (var sub in subs)
                    {
                        if(sub.RefQuoteSubId.GetValueOrDefault()>0 )
                            await ValidateQuotationBalanceBeforeRevertAsync(sub);
                        if (sub.RefMReqSubId.GetValueOrDefault() > 0)
                            await ValidateMreqBalanceBeforeRevertAsync(sub);
                    }
                }

                existingPo.PoCancl = poVM.PoCancl;
                existingPo.CancelReason = poVM.CancelReason;
                existingPo.CancelDate = poVM.CancelDate;
                existingPo.CancelBy = poVM.CancelBy;


                await _unitOfWork.PurchPos.UpdateAsync(existingPo);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {
                    if (existingPo.PoCancl)
                    {
                        if (sub.RefQuoteSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustQuoteBalanceAsync(
                                sub.RefQuoteSubId.Value,
                                sub.Qty,
                                0,
                                $"Purchase PO Cancelled - {existingPo.PONo}"
                            );
                        }

                        if (sub.RefMReqSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustMreqBalanceAsync(
                                sub.RefMReqSubId.Value,
                                sub.Qty,
                                0,
                                $"Purchase PO Cancelled - {existingPo.PONo}"
                            );
                        }

                    }
                    else
                    {
                        if (sub.RefQuoteSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustQuoteBalanceAsync(
                                sub.RefQuoteSubId.Value,
                                0,
                                sub.Qty,
                                $"Purchase PO Reverted - {existingPo.PONo}"
                            );
                        }

                        if (sub.RefMReqSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustMreqBalanceAsync(
                                sub.RefMReqSubId.Value,
                                0,
                                sub.Qty,
                                $"Purchase PO Reverted - {existingPo.PONo}"
                            );
                        }
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

        private async Task ValidateQuotationBalanceBeforeRevertAsync(PurchPoSub? sub)
        {
            if (sub.RefQuoteSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.PurchaseQuotesSubs.GetAsync(sub.RefQuoteSubId.Value);
            if (entity == null)
                throw new InvalidOperationException($"Quotation not found for RefQuoteSubId: {sub.RefQuoteSubId}");

            if (entity.BalQty < sub.Qty)
            {
                throw new InvalidOperationException($"Cannot revert because Quotation balance ({entity.BalQty}) is less than required quantity ({sub.Qty}).");
            }

        }

        private async Task ValidateMreqBalanceBeforeRevertAsync(PurchPoSub? sub)
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

        public async Task<(bool CanDelete, string Message)> CanDeletePurchaseOrderAsync(int poId)
        {
            try
            {

                var poSubIds = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(s => s.PoId == poId)
                    .Select(s => s.PoSubId)
                    .ToListAsync();

                if (!poSubIds.Any())
                    return (true, "Purchase Order can be safely deleted.");

                var sums = await _unitOfWork.PurchPoSubs.GetQueryable().Where(x => x.PoId == poId)
                         .GroupBy(x => 1).Select(g => new
                         {
                             TotalQty = g.Sum(s => s.Qty),
                             TotalBalQty = g.Sum(s => s.BalQty)
                         }).FirstOrDefaultAsync();

                bool hasPurchPo = sums != null && sums.TotalQty == sums.TotalBalQty;
                if (!hasPurchPo)
                    return (false, "Cannot delete this Purchase Order as a some transaction Made.");

                bool isPoRevised = await _unitOfWork.PurchPos.GetQueryable().AnyAsync(qs => qs.RefRevisonPoid == poId);

                if (isPoRevised)
                    return (false, "Cannot delete this Purchase Order as a revised PO already exists.");

                var PO = await _unitOfWork.PurchPos .GetQueryable().Where(e => e.PoId == poId)
                              .Select(e => new
                              {
                                  e.PoId,
                                  e.PoCancl,
                                  e.PoShortClose,
                                  SubItems = e.PurchPoSubs.Select(s => new
                                  {
                                      s.PoSubId,
                                      s.ItemCancel
                                  }).ToList()
                              }).FirstOrDefaultAsync();

                var grn = await _unitOfWork.PurchaseGRNSubs.GetQueryable().AnyAsync(grn=> poSubIds.Contains(grn.RefPoSubId.GetValueOrDefault()));

                var dcout = await _unitOfWork.SubConDCOutSubs.GetQueryable().AnyAsync(dc=> poSubIds.Contains(dc.RefPoSubId.GetValueOrDefault()));

                if (grn || dcout)
                    return (false, "Cannot delete this Purchase Order as a some transaction Made.");

                if (PO == null)
                    return (false, "Purchase Order not found.");

                if (PO.PoCancl)
                    return (false, "Main Purchase Order is already cancelled and cannot be deleted.");

                if (PO.SubItems.Any(s => s.ItemCancel))
                    return (false, "Some Purchase Order items are cancelled and cannot be deleted.");

                if (PO.PoShortClose)
                    return (false, "Some Purchase Order items are Short Closed cannot be deleted.");

                if (PO.SubItems.Any())
                    return (true, "Purchase Order can be safely deleted (no sub-items).");


                return (true, "Purchase Order can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanDeletePurchaseOrderAsync for PoId: {poId}");
                throw new Exception("Error checking Purchase Order delete eligibility", ex);
            }
        }

        #endregion
        public async Task<bool> DeletePOIdAsync(int PosubId, int enquiryId, int matReqSubId)
        {
            try
            {
                var oldItem = await _unitOfWork.PurchPoSubs
                    .FirstOrDefaultAsync(x => x.PoSubId == PosubId && x.PoId == enquiryId);

                if (oldItem != null)
                {

                    if (oldItem.RefMReqSubId == matReqSubId)
                    {
                        var qtyToRestore = oldItem.Qty;
                        await RestoreOldMaterialReqSubBalQtyByID(matReqSubId, qtyToRestore);
                    }
                    await _unitOfWork.PurchPoSubs.DeleteAsync(oldItem.PoSubId);
                    await _unitOfWork.SaveAsync();


                    await _loggingService.LogUserAction(
                        UserName: await _currentUserService.GetUsernameAsync(),
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Puarchase Po Upsert",
                        action: $"Deleted Purchase Po: {oldItem.Item}",
                        additionalInfo: $"po Id: {oldItem.PoId}");
                }

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Row Item In enquiry purchasesub with Id: {enquiryId}");
                return false;
            }
        }
        public async Task<bool> DeletePOIdQuoteAsync(int PosubId, int poid, int RefQuoteSubid)
        {
            try
            {
                var oldItem = await _unitOfWork.PurchPoSubs
                    .FirstOrDefaultAsync(x => x.PoSubId == PosubId && x.PoId == poid);

                if (oldItem != null)
                {
                    if (oldItem.RefQuoteSubId == RefQuoteSubid)
                    {
                        var qtyToRestore = oldItem.Qty;
                        await RestoreOldQuoteSubBalQtyByID(RefQuoteSubid, qtyToRestore);
                    }
                    await _unitOfWork.PurchPoSubs.DeleteAsync(oldItem.PoSubId);
                    await _unitOfWork.SaveAsync();


                    await _loggingService.LogUserAction(
                        UserName: await _currentUserService.GetUsernameAsync(),
                        Machine: _currentUserService.MachineName,
                        IP_Address: _currentUserService.IpAddress,
                        screen: "Puarchase Po Upsert",
                        action: $"Deleted Purchase Po: {oldItem.Item}",
                        additionalInfo: $"po Id: {oldItem.PoId}");
                }
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Row Item In Quatation purchasesub with Id: {RefQuoteSubid}");
                return false;
            }
        }
        public async Task<PurchPo?> GetPurchPOAsync(int PoId)
        {
            try
            {
                var enquiry = await _unitOfWork.PurchPos.GetAsync(PoId);
                if (enquiry == null)
                    return null;

                var subItems = await _unitOfWork.PurchPoSubs

                    .FindAsync(s => s.PoId == PoId);

                enquiry.PurchPoSubs = subItems.ToList();

                return enquiry;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get GetEnquiryAsync in Purchase PO from PurchPoService with POid: {PoId}");
                return null;
            }
        }
        public async Task RestoreOldMaterialReqSubBalQtyByID(int matReqSubId, decimal qtyToAdd)
        {
            try
            {
                var existing = await _unitOfWork.MaterialReqSubs
                               .FirstOrDefaultAsync(m => m.MreqSubId == matReqSubId);

                if (existing != null)
                {
                    existing.BalQty += qtyToAdd;

                    await _unitOfWork.MaterialReqSubs.UpdateAsync(existing);
                    await _unitOfWork.SaveAsync();
                }
                var totalBal = await _unitOfWork.MaterialReqSubs
               .GetQueryable()
               .Where(s => s.MreqId == existing.MreqId)
               .SumAsync(s => s.BalQty);

                var Matreq = await _unitOfWork.MaterialReqs
                    .FirstOrDefaultAsync(q => q.MreqId == existing.MreqId);

                if (Matreq != null)
                {

                    Matreq.PRTally = totalBal <= 0;
                    await _unitOfWork.MaterialReqs.UpdateAsync(Matreq);
                    await _unitOfWork.SaveAsync();
                }
                await _loggingService.LogUserAction(
                      UserName: await _currentUserService.GetUsernameAsync(),
                      Machine: _currentUserService.MachineName,
                      IP_Address: _currentUserService.IpAddress,
                      screen: "Puarchase Po Upsert",
                      action: $"RestoreOldMaterialReqSubBalQtyByID Purchase Po: {existing.Item}",
                      additionalInfo: $"MreqId: {existing.MreqId}");

            }
            catch (Exception ex)
            {

                await _loggingService.LogDeveloperError(ex, $"Error RestoreOldMaterialReqSubBalQtyByID: {matReqSubId}");
                throw new InvalidOperationException("Failed RestoreOldMaterialReqSubBalQtyByID Purchase Po");
            }


        }

        public async Task RestoreOldQuoteSubBalQtyByID(int QuoteSubid, decimal qtyToAdd)
        {
            try
            {

                var existing = await _unitOfWork.PurchaseQuotesSubs
                    .FirstOrDefaultAsync(m => m.QuoteSubId == QuoteSubid);

                if (existing == null)
                    throw new InvalidOperationException("QuoteSub not found!");


                existing.BalQty += qtyToAdd;
                //existing.ItemStatus = "Quote";

                await _unitOfWork.PurchaseQuotesSubs.UpdateAsync(existing);
                await _unitOfWork.SaveAsync();

                var totalBal = await _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                    .Where(s => s.QuoteId == existing.QuoteId)
                    .SumAsync(s => s.BalQty);

                var currentQuote = await _unitOfWork.PurchaseQuotes
                    .FirstOrDefaultAsync(q => q.QuoteId == existing.QuoteId);

                if (currentQuote != null)
                {
                    currentQuote.QuotationTally = totalBal <= 0;
                    await _unitOfWork.PurchaseQuotes.UpdateAsync(currentQuote);
                    await _unitOfWork.SaveAsync();
                }

                if (existing.RefEnqVendorAssignId > 0)
                {
                    int enquirySubId = existing.RefEnqVendorAssignId.Value;

                    var currentVendor = await _unitOfWork.PurchaseQuotes.GetQueryable()
                        .Where(q => q.QuoteId == existing.QuoteId)
                        .Select(q => q.VendorCode)
                        .FirstOrDefaultAsync();

                    var otherVendorSubs = await
                        (from sub in _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                         join q in _unitOfWork.PurchaseQuotes.GetQueryable()
                            on sub.QuoteId equals q.QuoteId
                         where sub.RefEnqVendorAssignId == enquirySubId  // same enquiry sub
                               && sub.ItemId == existing.ItemId   // same item
                               && q.VendorCode != currentVendor   // exclude current vendor
                         select sub).ToListAsync();


                    if (otherVendorSubs.Any())
                    {
                        foreach (var sub in otherVendorSubs)
                        {
                            //if (sub.ItemStatus == "ShortClose")
                            //    sub.ItemStatus = "Quote";  // Re-open
                        }
                        await _unitOfWork.PurchaseQuotesSubs.UpdateRangeAsync(otherVendorSubs);
                        await _unitOfWork.SaveAsync();
                    }


                    var vendorQuoteIds = otherVendorSubs.Select(s => s.QuoteId).Distinct().ToList();

                    var vendorQuotes = await _unitOfWork.PurchaseQuotes.GetQueryable()
                        .Where(q => vendorQuoteIds.Contains(q.QuoteId))
                        .Include(q => q.PurchaseQuoteSub)
                        .ToListAsync();

                    foreach (var vendorQuote in vendorQuotes)
                    {
                        if (vendorQuote.QuoteShortClose)
                        {
                            //bool hasActiveItems =
                            //    vendorQuote.PurchaseQuoteSub.Any(s => s.ItemStatus == "Quote");

                            //if (hasActiveItems)
                            //    vendorQuote.QuoteShortClose = false; // re-open whole vendor quotation

                            //await _unitOfWork.PurchaseQuotes.UpdateAsync(vendorQuote);
                        }
                    }

                    await _unitOfWork.SaveAsync();
                }

                // 🔹 Log action
                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase PO Upsert",
                    action: $"Restored QuoteSub Balance: {existing.Item}",
                    additionalInfo: $"QuoteId: {existing.QuoteId}"
                );
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error RestoreOldQuoteSubBalQtyByID: {QuoteSubid}");
                throw new InvalidOperationException("Failed RestoreOldQuoteSubBalQtyByID Purchase PO");
            }
        }

        public async Task RestoreOldMaterialReqSubBalQty(int PoId)
        {
            try
            {

                var oldEnquiry = await GetPurchPOAsync(PoId);
                if (oldEnquiry == null || oldEnquiry.PurchPoSubs == null) return;


                var oldMatReqSubIds = oldEnquiry.PurchPoSubs
                    .Where(x => x.RefMReqSubId.HasValue && x.RefMReqSubId.Value > 0 && x.ItemCancel == false)
                    .Select(x => x.RefMReqSubId.Value)
                    .Distinct()
                    .ToList();

                if (!oldMatReqSubIds.Any())
                    return;

                var materialReqSubs = await _unitOfWork.MaterialReqSubs.GetQueryable()
                    .Where(m => oldMatReqSubIds.Contains(m.MreqSubId)).ToListAsync();


                foreach (var sub in oldEnquiry.PurchPoSubs)
                {
                    var existing = materialReqSubs.FirstOrDefault(m => m.MreqSubId == sub.RefMReqSubId && m.ItemCancel == false);
                    if (existing != null)
                    {

                        var qtyToAdd = sub.Qty;

                        existing.BalQty += qtyToAdd;

                        await _unitOfWork.MaterialReqSubs.UpdateAsync(existing);
                    }
                }

                await _unitOfWork.SaveAsync();


                // ✅ Get distinct parent QuoteIds from the affected subs
                var affectedQuoteIds = materialReqSubs
                    .Select(s => s.MreqId)
                    .Distinct()
                    .ToList();

                // ✅ Update parent Quote tally based on total balance from DB
                foreach (var MreqId in affectedQuoteIds)
                {
                    var totalBal = await _unitOfWork.MaterialReqSubs
                        .GetQueryable()
                        .Where(s => s.MreqId == MreqId)
                        .SumAsync(s => s.BalQty);

                    var quote = await _unitOfWork.MaterialReqs
                        .FirstOrDefaultAsync(q => q.MreqId == MreqId);

                    if (quote != null)
                    {
                        quote.PRTally = totalBal <= 0;
                        await _unitOfWork.MaterialReqs.UpdateAsync(quote);
                    }
                }

                await _unitOfWork.SaveAsync();
                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Puarchase Po Upsert",
                    action: $"RestoreOldMaterialReqSubBalQty Purchase Po: {PoId}",
                    additionalInfo: $"PoId: {PoId}");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error RestoreOldMaterialReqSubBalQty: {PoId}");
                throw new InvalidOperationException("Failed RestoreOldMaterialReqSubBalQty Purchase Po");
            }
        }
        public async Task RestoreOldQuoteSubBalQty(int PoId)
        {
            try
            {
                var oldPo = await GetPurchPOAsync(PoId);
                if (oldPo == null || oldPo.PurchPoSubs == null) return;


                var oldQuoteSubIds = oldPo.PurchPoSubs
                    .Where(x => x.RefQuoteSubId.HasValue && x.RefQuoteSubId.Value > 0 && !x.ItemCancel)
                    .Select(x => x.RefQuoteSubId.Value)
                    .Distinct()
                    .ToList();

                if (!oldQuoteSubIds.Any())
                    return;

                var quoteSubs = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(qs => oldQuoteSubIds.Contains(qs.QuoteSubId))
                    .ToListAsync();

                foreach (var poSub in oldPo.PurchPoSubs)
                {
                    var existing = quoteSubs.FirstOrDefault(q => q.QuoteSubId == poSub.RefQuoteSubId);
                    if (existing == null) continue;

                    existing.BalQty += poSub.Qty;
                    //existing.ItemStatus = "Quote";
                    await _unitOfWork.PurchaseQuotesSubs.UpdateAsync(existing);
                    await _unitOfWork.SaveAsync();

                    if (existing.RefEnqVendorAssignId > 0)
                    {
                        var enquiryNo = existing.RefEnqVendorAssignId;

                        var relatedSubs = await _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                            .Where(s => s.RefEnqVendorAssignId == enquiryNo)
                            .ToListAsync();


                        var otherVendorSubs = relatedSubs
                            .Where(s => s.ItemId == existing.ItemId && s.QuoteId != existing.QuoteId)
                            .ToList();

                        if (otherVendorSubs.Any())
                        {
                            foreach (var sub in otherVendorSubs)
                            {
                                //if (sub.ItemStatus == "ShortClose")
                                //    sub.ItemStatus = "Quote"; // Re-open
                            }

                            await _unitOfWork.PurchaseQuotesSubs.UpdateRangeAsync(otherVendorSubs);
                        }

                        // Re-open vendor quotations that were fully ShortClosed
                        var quoteIdss = relatedSubs.Select(s => s.QuoteId).Distinct().ToList();

                        var quotes = await _unitOfWork.PurchaseQuotes.GetQueryable()
                            .Where(q => quoteIdss.Contains(q.QuoteId))
                            .Include(q => q.PurchaseQuoteSub)
                            .ToListAsync();

                        foreach (var q in quotes)
                        {
                            if (q.QuoteShortClose)
                            {
                                //bool hasActiveItems = q.PurchaseQuoteSub.Any(s => s.ItemStatus == "Quote");

                                //if (hasActiveItems)
                                //    q.QuoteShortClose = false; // Re-open vendor quote

                                await _unitOfWork.PurchaseQuotes.UpdateAsync(q);
                            }
                        }
                    }

                    // 5️⃣ Final Commit
                    await _unitOfWork.SaveAsync();


                }

                await _unitOfWork.SaveAsync();

                // 🔹 Step 2: Recalculate QuotationTally for all affected quotes
                var quoteIds = quoteSubs.Select(s => s.QuoteId).Distinct().ToList();
                var affectedQuotes = await _unitOfWork.PurchaseQuotes
                    .GetQueryable()
                    .Where(q => quoteIds.Contains(q.QuoteId))
                    .ToListAsync();

                foreach (var quote in affectedQuotes)
                {
                    var totalBal = await _unitOfWork.PurchaseQuotesSubs
                        .GetQueryable()
                        .Where(s => s.QuoteId == quote.QuoteId)
                        .SumAsync(s => s.BalQty);

                    quote.QuotationTally = totalBal <= 0 ? true : false;
                    quote.QuoteShortClose = false;
                    await _unitOfWork.PurchaseQuotes.UpdateAsync(quote);
                }

                await _unitOfWork.SaveAsync();

                // 🔹 Step 3: Get all enquiry numbers for these restored items
                var refEnqNos = await _unitOfWork.PurchaseQuotesSubs
                    .GetQueryable()
                    .Where(s => oldQuoteSubIds.Contains(s.QuoteSubId))
                    .Select(s => s.RefEnqVendorAssignId)
                    .Distinct()
                    .ToListAsync();

                // 🔹 Step 4: For each enquiry, restore related VendorSubs for that vendor only
                foreach (var enquiryNo in refEnqNos)
                {
                    // Find enquiry ID
                    var enquiryId = await _unitOfWork.EnquiryPurchaseSubs
                        .GetQueryable()
                        .Where(e => e.EnquirySubId == enquiryNo)
                        .Select(e => e.EnquiryId)
                        .FirstOrDefaultAsync();

                    if (enquiryId <= 0) continue;

                    // Find the vendor(s) used in this restored PO (from the quote reference)
                    var vendorCodes = await (
                        from q in _unitOfWork.PurchaseQuotes.GetQueryable()
                        join s in _unitOfWork.PurchaseQuotesSubs.GetQueryable()
                            on q.QuoteId equals s.QuoteId
                        where s.RefEnqVendorAssignId == enquiryNo && oldQuoteSubIds.Contains(s.QuoteSubId)
                        select q.VendorCode
                    ).Distinct().ToListAsync();

                    // Get all vendor subs for this enquiry
                    var allVendorSubs = await _unitOfWork.EnquiryPurchaseVendorAssigns
                        .GetQueryable()
                        .Where(v => v.EnquiryId == enquiryId)
                        .ToListAsync();

                    // Determine affected EnquirySubIds (those items from restored PO)
                    var restoredEnquirySubIds = await _unitOfWork.EnquiryPurchaseSubs
                        .GetQueryable()
                        .Where(es => es.EnquiryId == enquiryId && refEnqNos.Contains(es.EnquirySubId))
                        .Select(es => es.EnquirySubId)
                        .ToListAsync();

                    // Update only matching vendors and items
                    foreach (var vendorSub in allVendorSubs)
                    {
                        if (vendorCodes.Contains(vendorSub.VendorCode) && restoredEnquirySubIds.Contains(vendorSub.EnquirySubId.Value))
                        {
                            vendorSub.ItemStatus = 1;
                        }
                        else
                        {
                            vendorSub.ItemStatus = 1;
                        }
                    }

                    await _unitOfWork.EnquiryPurchaseVendorAssigns.UpdateRangeAsync(allVendorSubs);
                    await _unitOfWork.SaveAsync();
                }

                // 🔹 Step 5: Logging
                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Purchase PO Restore",
                    action: $"Restored Quote BalQty & Vendor Status for PO: {PoId}",
                    additionalInfo: $"PO ID: {PoId}"
                );
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in RestoreOldQuoteSubBalQty for PO: {PoId}");
                throw new Exception("Error restoring Purchase Order balances.", ex);
            }
        }
        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int VendorCode)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.PurchPoSubs.GetQueryable()
                                  join q in _unitOfWork.PurchPos.GetQueryable() on qs.PoId equals q.PoId
                                  where qs.ItemId == itemId && q.VendorCode == VendorCode
                                  orderby q.PoId descending
                                  select qs.UnitPrice)
                                 .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.PurchPoSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.PoSubId descending
                                      select qs.UnitPrice)
                                     .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId && isub.VendorId == VendorCode
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
                await _loggingService.LogDeveloperError(ex, $"Error fetching bulk last unit prices for VendorCode: {VendorCode}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }

        }
        public async Task<List<RateComparisonVM>> GetRateComparisonAsync()
        {
            // 1️⃣ Load all required data from PurchaseQuotes
            var quotes = await _unitOfWork.PurchaseQuotes.GetQueryable()
                .Where(pq => pq.QuotationTally == false)
                .Include(pq=> pq.Vendor)
                .Include(pq => pq.PurchaseQuoteSub)
                    .ThenInclude(sub => sub.Item)
                .Include(pq => pq.PurchaseQuoteSub)
                    .ThenInclude(sub => sub.EnquiryPurchaseVendorAssign)
                        .ThenInclude(ep => ep.EnquiryPurchaseSub)
                            .ThenInclude(es => es.EnquiryPurchase)
                .ToListAsync();

            // 2️⃣ Filter VALID sub-items (business logic rules)
            var validSubs = quotes
                .SelectMany(pq => pq.PurchaseQuoteSub
                    .Where(sub =>
                        sub.BalQty > 0 &&                     // Must have balance qty
                        !sub.ItemShortClose &&               // Must NOT be short closed
                        sub.EnquiryPurchaseVendorAssign != null &&
                        sub.EnquiryPurchaseVendorAssign.ItemStatus != 3 &&
                        sub.EnquiryPurchaseVendorAssign.ItemStatus != 4
                    )
                    .Select(sub => new
                    {
                        pq.QuoteId,
                        pq.QuoteNo,
                        pq.QuoteDate,
                        pq.VendorCode,
                        pq.Vendor.VendorName,

                        sub.QuoteSubId,
                        sub.ItemId,
                        ItemName = sub.Item.ItemName,
                        ItemCode = sub.Item.ItemCode,
                        UOM = sub.Item.MeasureUnit,
                        HSNCode = sub.Item.HSNCode,
                        sub.UnitPrice,
                        Qty = sub.BalQty,

                        EnquiryId = sub.EnquiryPurchaseVendorAssign.EnquiryPurchaseSub.EnquiryPurchase.EnquiryId,
                        EnquiryNo = sub.EnquiryPurchaseVendorAssign.EnquiryPurchaseSub.EnquiryPurchase.EnquiryNo,
                        Suffix = sub.EnquiryPurchaseVendorAssign.EnquiryPurchaseSub.EnquiryPurchase.Suffix,
                        ExpectedDate = sub.EnquiryPurchaseVendorAssign.EnquiryPurchaseSub.EnquiryPurchase.ExpactedReplayDate,

                        VendorAssignId = sub.EnquiryPurchaseVendorAssign.EnqPurchVendorId
                    })
                )
                .Where(x => x.VendorCode.HasValue)
                .ToList();

            // 3️⃣ Now group by Enquiry + Item and ensure more than 1 vendor exists
            var grouped = validSubs
                .GroupBy(x => new { x.EnquiryId, x.ItemId })
                .Where(g => g.Select(v => v.VendorCode.Value).Distinct().Count() > 1)
                .ToList();

            // 4️⃣ Build final output
            var result = new List<RateComparisonVM>();

            foreach (var group in grouped)
            {
                foreach (var row in group)
                {
                    result.Add(new RateComparisonVM
                    {
                        EnquiryId = row.EnquiryId,
                        EnquiryNo = $"{row.EnquiryNo}{row.Suffix}",
                        QuoteId = row.QuoteId,
                        QuoteNo = row.QuoteNo,
                        QuoteSubId = row.QuoteSubId,
                        QuoteDate = row.QuoteDate,
                        DueDate = row.ExpectedDate ?? DateTime.Now,

                        ItemId = row.ItemId.Value,
                        ItemName = row.ItemName,
                        ItemCode = row.ItemCode,
                        UOM = row.UOM,
                        HsnCode = row.HSNCode,

                        VendorCode = row.VendorCode.Value,
                        VendorName = row.VendorName,
                        Rate = row.UnitPrice,
                        Qty = row.Qty
                    });
                }
            }

            // 5️⃣ Order output in meaningful manner
            return result
                .OrderBy(x => x.EnquiryNo)
                .ThenBy(x => x.ItemName)
                .ThenBy(x => x.Rate)
                .ToList();
        }
      
        public async Task<(List<PurchPoVM> purchPos, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.PurchPos
                         .GetQueryable()
                         .AsSplitQuery()
                         .Include(e => e.Vendor)
                        .Include(e => e.PurchPoSubs)
                             .ThenInclude(s => s.Item)
                         .Include(e => e.PurchPoSubs)
                             .ThenInclude(s => s.CostCenter)
                         .AsQueryable();

            string? status = null;
            // Apply Dynamic Filters
            if (filters != null)
            {
                foreach (var f in filters)
                {

                    query = PurchaseOrderFilterBuilder.ApplyFilter(query, f.Key, f.Value);

                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.PoId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<PurchPoVM>>(list);

            return (vmList, total);
        }
        public static class PurchaseOrderFilterBuilder
        {
            public static IQueryable<PurchPo> ApplyFilter(IQueryable<PurchPo> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "PONumber":
                        {
                            var input = val?.ToString()?.Trim();

                            if (string.IsNullOrEmpty(input))
                                return query;

                            string poNo = input.Split('/')[0];

                            int revisionNo = 0;
                            string suffix = "";

                            var parts = input.Split('/', StringSplitOptions.RemoveEmptyEntries);

                            foreach (var part in parts)
                            {
                                if (part.StartsWith("R", StringComparison.OrdinalIgnoreCase))
                                {
                                    int.TryParse(part.Substring(1), out revisionNo);
                                }
                                else if (part.Contains('-')) // Financial Year
                                {
                                    suffix = "/" + part;
                                }
                            }

                            return query.Where(x =>
                                x.PONo.StartsWith(poNo) &&
                                (revisionNo == 0 || x.RevisionNo == revisionNo) &&
                                (string.IsNullOrEmpty(suffix) || x.Suffix == suffix)
                            );
                        }

                    case "VendorName":
                        return query.Where(x => x.Vendor.VendorName.Contains(val));

                    case "ItemName":
                        return query.Where(x => x.PurchPoSubs
                            .Any(s => s.Item.ItemName.Contains(val)));

                    case "ItemCode":
                        return query.Where(x => x.PurchPoSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "QuoteNo":
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
                                x.PurchPoSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.PurchaseQuoteSub.PurchaseQuote.QuoteNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.PurchaseQuoteSub.PurchaseQuote.Suffix.Contains(part2))
                                ));
                        }

                    case "IndentNo":
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
                                x.PurchPoSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.MaterialReqSub.MaterialReq.MReqNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.MaterialReqSub.MaterialReq.Suffix.Contains(part2))
                                ));
                        }

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "ModifiedBy":
                        return query.Where(x => x.ModifiedBy.Contains(val));

                    case "FromDate":
                        return query.Where(x => x.PODate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.PODate <= DateTime.Parse(value.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "ApprovalStatus":
                        return ApplyApprovalStatusFilter(query, val);
                }

                return query;
            }

            private static IQueryable<PurchPo> ApplyStatusFilter(
                IQueryable<PurchPo> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.PoTally == true),
                    "Short Closed" => query.Where(x => x.PoShortClose == true),
                    "Cancelled" => query.Where(x => x.PoCancl == true),
                    "Pending" => query.Where(x => x.PoTally == false && x.PoCancl == false && x.PoShortClose == false),
                    _ => query
                };
            }

            private static IQueryable<PurchPo> ApplyApprovalStatusFilter(
                IQueryable<PurchPo> query, string status)
            {
                return status switch
                {
                    "Approved" => query.Where(x => x.Authorized == true),
                    "Rejected" => query.Where(x => x.IsRejected == true),
                    "Pending" => query.Where(x => x.Authorized == false && x.IsRejected == false),
                    _ => query
                };
            }
        }

        public async Task<Dictionary<int, decimal>> GetROlForItemsAsync(List<int> itemIds)
        {
            try
            {
                var distinctIds = itemIds.Distinct().ToList();

                var data = await _unitOfWork.ItemRepositories
                               .GetQueryable()
                               .Where(i => distinctIds.Contains(i.ItemId))
                               .Select(i => new { i.ItemId, i.ROL })
                               .ToListAsync();
                var result = data.ToDictionary(x => x.ItemId, x => x.ROL ?? 0);

                foreach (var id in distinctIds)
                {
                    if (!result.ContainsKey(id))
                        result[id] = 0;
                }

                return result;

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error fetching bulk ROL values");
                throw new InvalidOperationException("Failed to fetch ROL values. Please try again.");
            }
        }

        public async Task<List<PurchasePoPendingListVM>> GetPurchasePosPendingListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<PurchasePoPendingListVM>("Sp_GetPurchasePosPendingList", status);
                return result.ToList();

            }
            catch(InvalidOperationException ex)
            {
                throw new InvalidOperationException(ex.Message);
            }
            catch (Exception ex)
            {

                throw;
            }
          
        }

        public async Task<List<ProcessFlowChartVM>> GetProcessByCompAsync(int itemId)
        {
            try
            {
                return await _unitOfWork.ProcessFlowCharts
                    .GetQueryable()
                    .Include(x => x.Process)
                    .Where(x => x.CompItemId == itemId)
                    .Select(x => new ProcessFlowChartVM
                    {
                        Id = x.ProcId, // IMPORTANT
                        ProcId = x.ProcId,
                        ProcessName = x.Process.ProcessName,
                        CompItemId = x.CompItemId
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    $"Error fetching Process Flow Chart for ItemId: {itemId}");

                throw new InvalidOperationException(
                    "Failed to retrieve Process Flow Chart details. Please try again.");
            }
        }
        public async Task<List<ProcessFlowChartVM>> GetProcessByRcIdAsync(int rcId)
        {
            try
            {

                return await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Include(x => x.Process)
                    .Where(x =>
                        x.RCId == rcId &&
                        x.BalQty > 0)

                    .Select(x => new ProcessFlowChartVM
                    {
                        Id = x.ProcessId,
                        ProcessName = x.Process.ProcessName
                    })
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    $"Error fetching Process for RCId: {rcId}");

                throw;
            }
        }
        
        public async Task<List<RouteCardVM>> GetRcdetailsByCompIdAsync(int itemId)
        {
            try
            {
                var routeCards = await _unitOfWork.RouteCards.GetQueryable()
                   .AsNoTracking()
                   .Where(r => r.CompItemId == itemId && r.RcStatus == 1 && r.RouteCardSubs.Any(x => x.BalQty > 0))
                   .Select(r => new RouteCardVM
                   {
                       RCId = r.RCId,
                       RCNo = r.RCNo + r.Suffix
                   })
                   .ToListAsync();

                return routeCards;

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error fetching Route Card details for ItemId: {itemId}");

                throw;
            }
        }
        public async Task<int?> GetRcdetailsByCompIdRcsubidAsync(int itemId, int processId, int rcid)
        {
            try
            {
                var rcSubId = await (
                    from rs in _unitOfWork.RouteCardSubs.GetQueryable().AsNoTracking()

                    join r in _unitOfWork.RouteCards.GetQueryable().AsNoTracking()
                        on rs.RCId equals r.RCId

                    where rs.ItemIdIn == itemId
                          && rs.ProcessId == processId && rs.RCId == rcid
                          && rs.BalQty > 0
                          && r.RcStatus == 1

                    select (int?)rs.RCSubId

                ).FirstOrDefaultAsync();

                return rcSubId;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    $"Error fetching Route Card details for ItemId: {itemId}");

                return null;
            }
        }
        public async Task<decimal?> GetRcBalQtyyCompIdRcsubidAsync(int rcSubId)
        {
            try
            {
                var balQty = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(x => x.RCSubId == rcSubId)
                    .Select(x => (decimal?)x.TotalQty)
                    .FirstOrDefaultAsync();

                return balQty;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    $"Error fetching Route Card details for Rcsubid: {rcSubId}");

                return null;
            }
        }
        public async Task<string?> GetPoNoAndSuffixByRcSubAsync(int rcSubId)
        {
            try
            {
                var result = await (
                    from ps in _unitOfWork.PurchPoSubs.GetQueryable().AsNoTracking()

                    join p in _unitOfWork.PurchPos.GetQueryable().AsNoTracking()
                        on ps.PoId equals p.PoId

                    where ps.RefRcSubId == rcSubId

                    select p.PONo + p.Suffix

                ).FirstOrDefaultAsync();

                return result;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    $"Error fetching PO details for Rcsubid: {rcSubId}");

                return null;
            }
        }
    }
   


    }
