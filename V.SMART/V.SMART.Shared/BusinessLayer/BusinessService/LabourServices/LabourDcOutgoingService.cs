using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.ExtendedProperties;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Vml.Office;
using FastReport;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILabourServices;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.ITrackReportService;
using V.SMART.Shared.Data.DcAutoRunning;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.ReportViewModel.GRNPendingVM;
using ZXing;
using static MudBlazor.Icons;

namespace V.SMART.Shared.BusinessLayer.BusinessService.LabourServices
{
    public class LabourDcOutgoingService : ILabourDcOutgoingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        private readonly IStockManagerService _stockManagerService;
   
        public LabourDcOutgoingService(
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

        //Screen
        public async Task<int> GetScreenCodeByScreenNameAsync(string screenName)
            => await _commonService.GetScreenCodeByScreenNameAsync(screenName);

        // 🔹 Customer
        public async Task<CustomerVM?> GetCustomerByIdAsync(int CustId)
           => await _commonService.GetCustomerByIdAsync(CustId);

        public async Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText)
        {
            return await _commonService.SearchCustomersAsync(searchText);
        }
        // 🔹 ContactPerson
        public async Task<List<ContactPerson>> GetContactPersonsCustomerAsync(int CustId)
            => await _commonService.GetContactPersonsAsync(CustId);

        // 🔹 Consignee addresses
        public async Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId)
            => await _commonService.GetConsigneeAddressesAsync(custId);

        //Mapped Store
        public async Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName)
            => await _commonService.GetMappedStoreForFormAsync(formName);

        public async Task<IEnumerable<Store>> GetAllActiveStoresAsync()
            => await _commonService.GetAllActiveStoresAsync();

        public Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType)
           => _commonService.GetCorrespondenceAttachmentsCountAsync(refId, refType);

        // 🔹 Items
        public async Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText)
            => await _commonService.SearchItemsAsync(searchText);

        public async Task<ItemVM?> GetItemByItemIdAsync(int? itemId)
           => await _commonService.GetItemByItemIdAsync(itemId);

        //Companydetails
        public async Task<Companydetails> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();

        // 🔹 Decimal places
        public async Task<int> GetDecimalPlacesAsync()
            => await _commonService.GetDecimalPlacesAsync();

        //Stock Manager
        public async Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId)
            => await _stockManagerService.GetStockForItemAsync(ItemId, StoreId);



        public async Task<decimal> GetStockForItemRcSubIdAsync(int ItemId, int StoreId, int? RcSubId)
            => await _stockManagerService.GetStockForItemRcSubIdAsync(ItemId, StoreId, RcSubId);
        public async Task<bool> IsPOWiseLabDcOutEnabledAsync()
         => await _commonService.GetScreenPermissionsAsync("Labour GRN", "PO Wise Labour DC Outgoing");

        //--------------------------------Dc List operations--------------------------------

        public async Task<List<int>> GetSCNIdsByGRNSubIdAsync(int refGRNSubId)
        {
            try
            {
                if (refGRNSubId <= 0)
                    return new List<int>();

                // STEP 1: Get GRNIds from SCN (Out Items)
                var grnIds = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNSubId == refGRNSubId)
                    .Select(x => x.GRNId)
                    .Distinct()
                    .ToListAsync();

                if (!grnIds.Any())
                    return new List<int>();

                // STEP 2: Get GRNSubIds (TransType = "In")
                var grnSubIds = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => grnIds.Contains(x.GRNId) && x.TransType == "In")
                    .Select(x => x.GRNSubId)
                    .ToListAsync();

                if (!grnSubIds.Any())
                    return new List<int>();

                // STEP 3: Get SCNIds using GRNSubIds
                var scnIds = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Where(x => x.RefGRNSubId.HasValue && grnSubIds.Contains(x.RefGRNSubId.Value))
                    .Select(x => x.SCNId)
                    .Distinct()
                    .ToListAsync();

                return scnIds;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching SCN IDs for RefGRNSubId: {refGRNSubId}"
                );
                throw;
            }
        }

        public async Task<List<int>> GetSCNIdsByGRNIds(List<int> grnIds)
        {
            try
            {
                if (grnIds == null || !grnIds.Any())
                    return new List<int>();

                // STEP 1: Get GRNSubIds (TransType = "In")
                var grnSubIds = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => grnIds.Contains(x.GRNId) && x.TransType == "In")
                    .Select(x => x.GRNSubId)
                    .ToListAsync();

                if (!grnSubIds.Any())
                    return new List<int>();

                // STEP 2: Get SCNIds using GRNSubIds
                var scnIds = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Where(x => grnSubIds.Contains(x.RefGRNSubId.Value))
                    .Select(x => x.SCNId)
                    .Distinct()
                    .ToListAsync();

                return scnIds;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching SCNIds from GRNIds");
                throw new InvalidOperationException("Failed to retrieve SCN details.");
            }
        }

        public async Task<List<Dictionary<string, object>>> GetPOGRNSCNItemDetailsByCustId(int custId, int storeId)
        {
            try
            {
                
                var grnData = await (from outSub in _unitOfWork.LabourGRNSubs.GetQueryable()

                join inSub in _unitOfWork.LabourGRNSubs.GetQueryable()
                on new
                {
                    outSub.GRNId,
                    outSub.GroupId,
                    Type = "In"
                }
                equals new
                {
                    inSub.GRNId,
                    inSub.GroupId,
                    Type = inSub.TransType
                }
                where outSub.TransType == "Out"

                      // SCN must exist for that exact IN record
                      && _unitOfWork.LabourSCNSubs.GetQueryable()
                            .Any(scn => scn.RefGRNSubId == inSub.GRNSubId)

                      && outSub.BalQty > 0
                      && !outSub.ItemCancel
                      && !outSub.LabourGRN.DcCancel
                      && !outSub.LabourGRN.ShortClose
                      && outSub.LabourGRN.CustId == custId
                select new
                {
                    outSub.GRNSubId,
                    outSub.GRNId,
                    outSub.GroupId,

                    GRNNo = outSub.LabourGRN.GRNNo,
                    GRNSuffix = outSub.LabourGRN.Suffix,
                    GRNDate = outSub.LabourGRN.GRNDate,

                    outSub.Qty,
                    outSub.BalQty,
                    outSub.UnitPrice,

                    // PO DETAILS
                    PONo = outSub.MfgPoSub != null
                                ? outSub.MfgPoSub.MfgPo.PONo
                                : null,

                    POSuffix = outSub.MfgPoSub != null
                                ? outSub.MfgPoSub.MfgPo.Suffix
                                : null,

                    PODate = outSub.MfgPoSub != null
                                ? outSub.MfgPoSub.MfgPo.PODate
                                : (DateTime?)null,

                    PoSubId = outSub.RefPoSubId,

                    LineNo = outSub.MfgPoSub != null
                                ? outSub.MfgPoSub.LineNo
                                : null,

                    // ITEM
                    outSub.ItemId,
                    ItemCode = outSub.Item.ItemCode,
                    ItemName = outSub.Item.ItemName,
                    Specification = outSub.Item.Specification,
                    MeasureUnit = outSub.Item.MeasureUnit,

                    categoryCode = outSub.Item.CategoryCode,
                    Category = outSub.Item.Category.CategoryName,

                    // DC
                    RefDcNo = outSub.LabourGRN.RefDcNo,
                    RefDcDate = outSub.LabourGRN.RefDcDate,

                    outSub.CostId,
                    ProjectNo = outSub.CostCenter.ProjectNo,

                    outSub.RowRemarks,
                    Customer= outSub.LabourGRN.Customer.CustName,
                } ).Distinct().ToListAsync();

                // 🔹 STOCK
                var itemIds = grnData
                    .Where(x => x.ItemId != null)
                    .Select(x => x.ItemId.Value)
                    .Distinct()
                    .ToList();

                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);

                var result = grnData.Where(r =>
                {
                    if (r.ItemId == null)
                        return false;

                    return stockDict.TryGetValue(r.ItemId.Value, out decimal qty)&& qty > 0;
                })
                .Select(r =>
                {
                    decimal stockQty = 0;

                    if (r.ItemId != null)
                        stockDict.TryGetValue(r.ItemId.Value, out stockQty);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["GRNSubId"] = r.GRNSubId,

                        ["GRNId"] = r.GRNId,
                        ["GRNNo"] = $"{r.GRNNo}{r.GRNSuffix}",
                        ["GRNDate"] = r.GRNDate.ToString("dd/MM/yyyy"),

                        ["CategoryCode"] = r.categoryCode,
                        ["Category"] = r.Category,

                        ["ItemId"] = r.ItemId,
                        ["ItemCode"] = r.ItemCode ?? "",
                        ["ItemName"] = r.ItemName ?? "",
                        ["ItemSpecification"] = r.Specification ?? "",
                        ["MeasureUnit"] = r.MeasureUnit ?? "",

                        ["Qty"] = r.Qty,
                        ["BalQty"] = r.BalQty,
                        ["UnitPrice"] = r.UnitPrice,

                        ["StockQty"] = stockQty,

                        ["RefDcNo"] = r.RefDcNo,
                        ["RefDcDate"] = r.RefDcDate?.ToString("dd/MM/yyyy"),

                        ["RefPoSubId"] = r.PoSubId,
                        ["RefPoNo"] = r.PONo != null ? $"{r.PONo}{r.POSuffix}" : "",
                        ["RefPoDate"] = r.PODate?.ToString("dd/MM/yyyy"),
                        ["LineNo"] = r.LineNo,

                        ["CostId"] = r.CostId,
                        ["ProjectNo"] = r.ProjectNo,

                        ["Remarks"] = r.RowRemarks,

                        ["Source"] = r.PoSubId != null ? "PO" : "GRN",

                        ["Customer"] = r.Customer
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching SCN details for CustId: {custId}");
                throw new InvalidOperationException("Failed to retrieve SCN details. Please try again.");
            }
        }

        public async Task<bool> GetPriceLockStatusAsync()
        {
            try
            {
                return await _unitOfWork.ScreenManagements.GetQueryable()
                    .Where(x => x.Display == "Rate Control" && x.ScreenName == "Labour Delivery Challan")
                    .Select(x => x.Required)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching price lock status for Labour Delivery Challan");
                return false;
            }
        }
        public async Task<(List<LabourDcOutgoingVM> Grns, int TotalCount)> SearchWithDynamicFilterAsync(
            int pageNumber,
            int pageSize,
            Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.LabourDcOutgoings
                    .GetQueryable()
                    .AsNoTracking();

                // Apply Dynamic Filters
                if (filters?.Any() == true)
                {
                    foreach (var filter in filters)
                    {
                        query = DynamicWhereBuilder.ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                // Total count
                var totalCount = await query.CountAsync();

                if (totalCount == 0)
                    return (new List<LabourDcOutgoingVM>(), 0);

                // Get Page Ids
                var dcIds = await query
                    .OrderByDescending(x => x.DcId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => x.DcId)
                    .ToListAsync();

                // Fetch full data
                var dcEntities = await _unitOfWork.LabourDcOutgoings
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(x => dcIds.Contains(x.DcId))

                    // Subs + Item
                    .Include(x => x.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.Item)

                    // SCN → GRN → PO
                    .Include(x => x.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.LabourSCNSub)
                            .ThenInclude(scn => scn.LabourGRNSub)
                                .ThenInclude(grn => grn.MfgPoSub)
                                    .ThenInclude(po => po.MfgPo)


                    // Master Tables
                    .Include(x => x.Customer)
                    .Include(x => x.Store)

                    .OrderByDescending(x => x.DcId)
                    .ToListAsync();

                // Maintain same order as paging
                dcEntities = dcEntities
                    .OrderByDescending(x => x.DcId)
                    .ToList();

                // Map to VM
                var vmList = _mapper.Map<List<LabourDcOutgoingVM>>(dcEntities);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync");
                return (new List<LabourDcOutgoingVM>(), 0);
            }
        }

        public async Task<bool> HasAnyItemOrLabourDcCancelAsync(int DcId)
        {
            try
            {
                var isGRNCancelled = await _unitOfWork.LabourDcOutgoings
                    .AnyAsync(q => q.DcId == DcId && q.DcCancel == true);

                var isItemCancelled = await _unitOfWork.LabourDcOutgoingSubs
                    .AnyAsync(i => i.DcId == DcId && i.ItemCancel == true);

                return isGRNCancelled || isItemCancelled;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasAnyItemOrLabourDcCancelAsync for DcId: {DcId}");
                throw;
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteLabourDcOutgoingAsync(int DcId, int screenCode, string refNo)
        {
            try
            {
                var dcOutgoing = await _unitOfWork.LabourDcOutgoings
                               .GetQueryable()
                               .Include(e => e.LabourDcOutgoingSubs)
                               .Where(e => e.DcId == DcId).FirstOrDefaultAsync();

                if (dcOutgoing == null)
                    return (true, "Labour GRN can be safely deleted.");

                var dcOutgoingSubIds = dcOutgoing.LabourDcOutgoingSubs
                    .Select(es => es.DcSubId)
                    .ToList();

                bool hasPo = await _unitOfWork.LabInvSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefDcSubId.HasValue &&
                        dcOutgoingSubIds.Contains(qs.RefDcSubId.Value));

                if (hasPo)
                    return (false, "Cannot delete this Labour DcOutgoing as a Labour Invoice exists.");

                if (dcOutgoing.DcCancel || dcOutgoing.ShortClose)
                    return (false, "Cannot delete this Labour DcOutgoing as it is Cancelled or Short-Closed.");


                if (dcOutgoing.LabourDcOutgoingSubs.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Labour DcOutgoing as one or more DcOutgoing items are cancelled.");


                var DcSubIds = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Where(s => s.DcId == DcId)
                    .Select(s => s.DcSubId)
                    .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(sa =>
                        DcSubIds.Contains(sa.SubItemRefID) &&
                        sa.ScreenCode == screenCode && sa.RefNo == refNo &&
                        sa.BalQty < sa.AddQty)
                    .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Labour Dcoutgoing. Some sub-items have already been transacted/issued.");

                return (true, "Labour DcOutgoing can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in ToCheckStockQtyIssued for DcId: {DcId}");
                throw new Exception("Error checking LabourDCoutgoing delete eligibility", ex);
            }
        }

        public async Task<bool> DeleteLabourDcByIdAsync(int DcId, int screenCode)

        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var grn = await _unitOfWork.LabourDcOutgoings
                    .GetQueryable()
                    .Include(e => e.LabourDcOutgoingSubs)
                    .FirstOrDefaultAsync(e => e.DcId == DcId);

                if (grn == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in grn.LabourDcOutgoingSubs)
                {
                    decimal oldAcceptedQty = 0;

                    bool rejRetrack = false;

                    if (sub.RefPoSubId.GetValueOrDefault() > 0 || sub.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);
                        if (rejRetrack)
                        {
                            oldAcceptedQty = sub.Qty - sub.RejectQty.GetValueOrDefault();
                        }
                        else
                        {
                            oldAcceptedQty = sub.Qty;
                        }
                        await AdjustGrnOutBalanceAsync(sub.RefGRNSubId, oldAcceptedQty, 0, "Labour DcOutgoing Deleted");
                    }


                    if (sub.DcSubId > 0)
                    {
                        await RollbackTracksAddIssueQtyAsync(sub.DcSubId, grn.SCNRejectReworkReturn);

                    }

                    await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);

                }

                //----------------AutoDcRunning-------------------------------------
                var runningRow = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.DcType == "LABOURDCOUTGOING" &&
                                x.Suffix == grn.Suffix);
                if (runningRow != null)
                {
                    long oldDcNo = 0;
                    long.TryParse(grn.DcNo.ToString(), out oldDcNo);
                    if (runningRow.LastNumber == oldDcNo)
                    {
                        runningRow.LastNumber = (oldDcNo - 1);
                        await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                    }
                }
                //----------------------------------------------------------------------------



                var deleted = await _unitOfWork.LabourDcOutgoings.DeleteAsync(DcId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Labour DcOutgoing",
                    action: $"Deleted Labour DcOutgoing: {grn.DcNo}",
                    additionalInfo: $"Dc Id: {grn.DcId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Dc: {DcId}");
                throw;
            }
        }

        private async Task AdjustSCNSubBalanceAsync(int? refSCNSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refSCNSubId.HasValue || refSCNSubId == 0) return;

                var scnSub = await _unitOfWork.LabourSCNSubs.GetAsync(refSCNSubId.Value);
                if (scnSub == null) return;

                if (oldQty > 0)
                    scnSub.BalQty += oldQty;

                if (newQty > scnSub.BalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Quote BalQty.");

                if (newQty > 0)
                    scnSub.BalQty -= newQty;

                await _unitOfWork.LabourSCNSubs.UpdateAsync(scnSub);
                await _unitOfWork.SaveAsync();


                var totalBalQty = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Where(e => e.SCNId == scnSub.SCNId)
                    .SumAsync(e => e.BalQty);

                var po = await _unitOfWork.LabourSCNs.GetAsync(scnSub.SCNId);
                if (po != null)
                {
                    po.SCNTally = (totalBalQty == 0);
                    await _unitOfWork.LabourSCNs.UpdateAsync(po);
                    await _unitOfWork.SaveAsync();
                }


            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustSCNSubBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustSCNSubBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust SCN balance. Please contact support.");
            }
        }

        private async Task DeleteStockIssueAndTrackAsync(int ScnSubId, int itemId, int screenCode)
        {
            try
            {
                var issueIds = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(s => s.SubItemRefID == ScnSubId && s.ItemId == itemId && s.ScreenCode == screenCode)
                .Select(s => s.IssueId)
                .ToListAsync();

                foreach (var issueid in issueIds)
                {
                    if (issueid > 0)
                        await _stockManagerService.DeleteStockIssueAsync(issueid);

                    await _unitOfWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to DeleteStockIssueAndTrackAsync in Purchase SCN");
            }
        }

        private async Task RollbackTracksAddIssueQtyAsync(int refDcSubId, bool SCNRejReqReturn)
        {

            try
            {
                var tracks = await _unitOfWork.LabourDcReturnCompTracks.GetQueryable()
                                .Where(p => p.RefDcSubId == refDcSubId).ToListAsync();

                foreach (var tr in tracks)
                {
                    var labscnSub = await _unitOfWork.LabourSCNSubs.GetQueryable()
                        .Where(p => p.SCNSubId == tr.RefSCNSubId).ToListAsync();

                    foreach (var scnsub in labscnSub)
                    {
                        if (SCNRejReqReturn)
                        {
                            scnsub.SCNBalQty += tr.QtyIn.GetValueOrDefault();

                        }
                        else
                        {
                            scnsub.BalQty += tr.QtyIn.GetValueOrDefault();

                        }


                        if (scnsub.BalQty > scnsub.AcceptQty)
                        {
                            scnsub.BalQty = scnsub.AcceptQty;
                        }
                        if (scnsub.SCNBalQty > (scnsub.AcceptQty + scnsub.RejectQty + scnsub.ReworkQty))
                        {
                            scnsub.SCNBalQty = (scnsub.AcceptQty + scnsub.RejectQty + scnsub.ReworkQty);
                        }

                        await _unitOfWork.LabourSCNSubs.UpdateAsync(scnsub);
                        await _unitOfWork.SaveAsync();


                        //Update IssueTally
                        var totalBalQty = await _unitOfWork.LabourSCNSubs
                        .GetQueryable()
                        .Where(e => e.SCNId == scnsub.SCNId)
                        .SumAsync(e => e.BalQty);

                        var issueAssy = await _unitOfWork.LabourSCNs.GetAsync(scnsub.SCNId);
                        if (issueAssy != null)
                        {
                            issueAssy.SCNTally = (totalBalQty == 0);
                            await _unitOfWork.LabourSCNs.UpdateAsync(issueAssy);
                            await _unitOfWork.SaveAsync();
                        }
                    }

                    await _unitOfWork.LabourDcReturnCompTracks.DeleteAsync(tr);
                    await _unitOfWork.SaveAsync();
                }

            }
            catch (Exception)
            {

                throw;
            }
        }



        //-------------------------------Dc Upsert operation----------------------
        public async Task<string> GetLastDcNumberAsync(string suffix)
        {
            try
            {
                string nextNumber = await _commonService.GeneratePreviewAutoDcRunningNoAsync("LABOURDCOUTGOING", suffix);

                return $"{nextNumber}";

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Labour Dc number for suffix: {suffix}");
                throw new InvalidOperationException("Failed to generate Labour Dc number.");
            }
        }

        public async Task<LabourDcOutgoingVM?> GetLabourDcByIdAsync(int DcId)
        {
            try
            {
                var entity = await _unitOfWork.LabourDcOutgoings
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()

                    // Subs + Item + Category
                    .Include(g => g.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.Item)
                            .ThenInclude(i => i.Category)

                    // Return Component Tracks
                    .Include(g => g.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.LabourDcReturnCompTracks)
                            .ThenInclude(t => t.LabourGRNSub)
                                .ThenInclude(grn => grn.LabourGRN)

                    // PO
                    .Include(g => g.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.MfgPoSub)
                            .ThenInclude(p => p.MfgPo)

                    // SCN
                    .Include(g => g.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.LabourSCNSub)
                            .ThenInclude(scn => scn.LabourSCN)

                    // GRN
                    .Include(g => g.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.LabourGRNSub)
                            .ThenInclude(grnSub => grnSub.LabourGRN)

                    // Cost Center
                    .Include(g => g.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.CostCenter)

                    // Master Tables
                    .Include(g => g.Customer)
                    .Include(g => g.Store)

                    .FirstOrDefaultAsync(g => g.DcId == DcId);

                if (entity == null)
                    return null;

                var vm = _mapper.Map<LabourDcOutgoingVM>(entity);

                return vm;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetLabourDcByIdAsync({DcId})");
                return null;
            }
        }


        public async Task<List<MfgPoVM>> GetOpenMfgPosByReturnMaterialCustomer(int custId)
        {
            var query =
             from m in _unitOfWork.MfgPos.GetQueryable().AsNoTracking()
             join ms in _unitOfWork.MfgPoSubs.GetQueryable().AsNoTracking()
                 on m.PoId equals ms.PoId
             where m.CustId == custId
                && _unitOfWork.LabourGRNSubs.GetQueryable()
                     .Where(g => g.RefPoSubId > 0)
                     .Where(g =>
                         _unitOfWork.LabourGRNSubs.GetQueryable()
                             .Join(_unitOfWork.LabourSCNSubs.GetQueryable(),
                                 grn => grn.GRNSubId,
                                 scnSub => scnSub.RefGRNSubId,
                                 (grn, scnSub) => new { grn, scnSub })
                             .Join(_unitOfWork.LabourSCNs.GetQueryable(),
                                 x => x.scnSub.SCNId,
                                 scn => scn.SCNId,
                                 (x, scn) => new { x.grn, x.scnSub, scn })
                             .Where(x =>
                                 x.scn.CustId == custId &&
                                 (x.scnSub.BalQty > 0 || x.scnSub.SCNBalQty > 0)
                             )
                             .Select(x => x.grn.GRNId)
                             .Contains(g.GRNId)
                     )
                     .Select(g => g.RefPoSubId)
                     .Contains(ms.PoSubId)
             select new
             {
                 m.PoId,
                 m.PONo,
                 m.Suffix
             };

            var result = await query
                .Distinct()
                .Select(x => new MfgPoVM
                {
                    PoId = x.PoId,
                    PONo = x.PONo + x.Suffix
                })
                .ToListAsync();

            return result;
        }

        public async Task<List<MfgPoVM>> GetOpenMfgPosByCustomer(int custId)
        {
            try
            {
                return await _unitOfWork.LabourGRNSubs
                            .GetQueryable()
                            .AsNoTracking()
                            .Where(h =>
                                h.MfgPoSub.MfgPo.CustId == custId &&
                                h.TransType == "Out" &&
                                h.BalQty > 0 &&
                                !h.MfgPoSub.ItemCancel &&
                                !h.MfgPoSub.MfgPo.PoCancl &&
                                !h.MfgPoSub.MfgPo.ShortClose
                            )
                            .Select(p => new MfgPoVM
                            {
                                PoId = p.MfgPoSub.MfgPo.PoId,
                                PONo = p.MfgPoSub.MfgPo.PONo + p.MfgPoSub.MfgPo.Suffix
                            })
                            .Distinct()
                            .OrderBy(x => x.PONo)
                            .ToListAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<LabourGRNVM>> GeLabourDCWithOutPoByCustomer(int custId)
        {
            try
            {
                var scns = await (
                        from grn in _unitOfWork.LabourGRNs.GetQueryable().AsNoTracking()
                        join grnSub in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                            on grn.GRNId equals grnSub.GRNId
                        join scnSub in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                            on grnSub.GRNSubId equals scnSub.RefGRNSubId
                        join scn in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                            on scnSub.SCNId equals scn.SCNId
                        where
                            !grn.DcCancel && grn.WithoutPoGRN
                            && !grnSub.ItemCancel && scnSub.RefGRNSubId.HasValue
                            && grn.CustId == custId && (scnSub.BalQty > 0 || scnSub.SCNBalQty > 0) && !scn.SCNCancel && !scn.ShortClose && !scnSub.ItemCancel
                        group grn by new
                        {
                            grn.GRNId,
                            grn.RefDcNo
                        }
                        into g
                        select new LabourGRNVM
                        {
                            GRNId = g.Key.GRNId,
                            RefDcNo = g.Key.RefDcNo
                        }
                    ).ToListAsync();

                return scns;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GeLabourDCWithOutPoByCustomer  item detail for custId: {custId}");
                throw new InvalidOperationException("Failed to retrieve GeLabourDCWithOutPoByCustomer Number details.");
            }
        }

        public async Task<List<LabourGRNVM>> GeLabourDCByCustomer(int custId)
        {
            try
            {
                var scns = await (
                        from grn in _unitOfWork.LabourGRNs.GetQueryable().AsNoTracking()
                        join grnSub in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                            on grn.GRNId equals grnSub.GRNId
                        join scnSub in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                            on grnSub.GRNSubId equals scnSub.RefGRNSubId
                        join scn in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                            on scnSub.SCNId equals scn.SCNId
                        where
                            !grn.DcCancel && !grn.WithoutPoGRN
                            && !grnSub.ItemCancel && scnSub.RefGRNSubId.HasValue && !scn.ShortClose
                            && grn.CustId == custId && (scnSub.BalQty > 0 || scnSub.SCNBalQty > 0) && !scn.SCNCancel && !scnSub.ItemCancel
                        group grn by new
                        {
                            grn.GRNId,
                            grn.RefDcNo
                        }
                        into g
                        select new LabourGRNVM
                        {
                            GRNId = g.Key.GRNId,
                            RefDcNo = g.Key.RefDcNo
                        }
                    ).ToListAsync();

                return scns;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GeLabourDCByCustomer  item detail for custId: {custId}");
                throw new InvalidOperationException("Failed to retrieve GeLabourDCByCustomer Number details.");
            }
        }

        public async Task<List<LabourGRNVM>> LoadGrnNumbersAsync(List<int> poIds)
        {
            try
            {
                if (poIds == null || !poIds.Any())
                    return new List<LabourGRNVM>();

                var grns = await (
                    from scnSub in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()

                    join scn in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                        on scnSub.SCNId equals scn.SCNId

                    join grnSub in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                        on scnSub.RefGRNSubId equals grnSub.GRNSubId

                    join grn in _unitOfWork.LabourGRNs.GetQueryable().AsNoTracking()
                        on grnSub.GRNId equals grn.GRNId

                    join poSub in _unitOfWork.MfgPoSubs.GetQueryable().AsNoTracking()
                        on grnSub.RefPoSubId equals poSub.PoSubId

                    where poIds.Contains(poSub.PoId)
                          && scn.Authorized
                          && !scn.SCNCancel
                          && !scnSub.ItemCancel
                          && scnSub.BalQty > 0
                          && !grnSub.ItemCancel
                          && !grn.DcCancel

                    group grn by new { grn.GRNId, grn.GRNNo, grn.Suffix, grn.RefDcNo } into g

                    select new LabourGRNVM
                    {
                        GRNId = g.Key.GRNId,
                        GRNNo = g.Key.GRNNo + g.Key.Suffix,
                        RefDcNo = g.Key.RefDcNo
                    }

                ).ToListAsync();

                return grns;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading GRN numbers for POIds: {string.Join(",", poIds)}");
                throw new InvalidOperationException("Failed to load GRN numbers.");
            }
        }




        public async Task<List<LabourSCNVM>> LoadScnNumbersAsync(List<int> ids, string check, bool receivedReturn)
        {
            try
            {
                if (ids == null || !ids.Any())
                    return new List<LabourSCNVM>();

                var query =
                    from scn in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()

                    join scnSub in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                        on scn.SCNId equals scnSub.SCNId

                    join grnIn in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                        on scnSub.RefGRNSubId equals grnIn.GRNSubId

                    where scn.Authorized
                        && !scn.SCNCancel
                        && !scn.ShortClose
                        && !scnSub.ItemCancel
                        && scnSub.RefGRNSubId.HasValue
                        && grnIn.TransType == "In"
                        && !grnIn.ItemCancel

                    select new
                    {
                        scn,
                        scnSub,
                        grnIn
                    };

                if (check == "PO")
                {
                    query =
                        from q in query

                        join grnOut in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                            on q.grnIn.GRNId equals grnOut.GRNId

                        join poSub in _unitOfWork.MfgPoSubs.GetQueryable().AsNoTracking()
                            on grnOut.RefPoSubId equals poSub.PoSubId

                        where grnOut.TransType == "Out"
                            && grnOut.RefPoSubId.HasValue
                            && ids.Contains(poSub.PoId)

                        select new
                        {
                            q.scn,
                            q.scnSub,
                            q.grnIn
                        };
                }
                else
                {
                    query = query.Where(x => ids.Contains(x.grnIn.GRNId));
                }

                if (!receivedReturn)
                {
                    query = query.Where(x =>
                        !x.scn.SCNTally &&
                        x.scnSub.BalQty > 0);
                }
                else
                {
                    query = query.Where(x =>
                        x.scnSub.SCNBalQty > 0 || x.scnSub.BalQty > 0);
                }

                var scns = await query
                    .GroupBy(x => new
                    {
                        x.scn.SCNId,
                        x.scn.SCNNo,
                        x.scn.Suffix,
                        x.grnIn.LabourGRN.RefDcNo
                    })
                    .Select(g => new LabourSCNVM
                    {
                        SCNId = g.Key.SCNId,
                        SCNNo = g.Key.SCNNo + g.Key.Suffix,
                        RefCustDcNo = g.Key.RefDcNo
                    })
                    .ToListAsync();

                return scns;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"Error fetching SCN Number details for IDs: {string.Join(",", ids)}");

                throw new InvalidOperationException("Failed to retrieve SCN Number details.");
            }
        }

        public async Task<int> GetPendingScnGrnPoCountAsync(int CustId)
        {
            try
            {
                return await _unitOfWork.LabourGRNSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(h =>
                            h.MfgPoSub.MfgPo.CustId == CustId &&
                            !h.MfgPoSub.ItemCancel &&
                            !h.MfgPoSub.MfgPo.PoCancl &&
                            !h.MfgPoSub.MfgPo.ShortClose &&
                            h.BalQty > 0)
                        .CountAsync();

            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"GetPendingScnGrnPoCountAsync using CustId : ({CustId})");
                return 0;
            }
        }

        public async Task<LabourDcOutgoingVM> GetPreviousDataAsync(int custId)
        {
            try
            {
                // Get previous DC data for the customer where required fields are not null/empty
                var previousData = await _unitOfWork.LabourDcOutgoings
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(h =>
                        h.CustId == custId &&
                        (!string.IsNullOrEmpty(h.VehicleNo) ||
                         !string.IsNullOrEmpty(h.TransFrom) ||
                         !string.IsNullOrEmpty(h.TransTo))
                    )
                    .OrderByDescending(h => h.DcId) // latest DC first
                    .Select(h => new LabourDcOutgoingVM
                    {
                        VehicleNo = h.VehicleNo,
                        TransFrom = h.TransFrom,
                        TransTo = h.TransTo,
                        TransportMode = h.TransportMode,
                        TransDocNo = h.TransDocNo,
                        TransId = h.TransId,
                        TransName = h.TransName,
                        SubSupplyType = h.SubSupplyType,
                        SubSupplyDesc = h.SubSupplyDesc
                    })
                    .FirstOrDefaultAsync(); // take only one previous DC

                return previousData; // could be null if no previous DC exists
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetPreviousDataAsync using CustId : ({custId})");
                return null;
            }
        }

        public async Task<bool> GetLabourDcByIdIsCancelAsync(int DcId)
        {
            return await _unitOfWork.LabourDcOutgoings
                .GetQueryable()
                .Where(e => e.DcId == DcId)
                .AnyAsync(e =>
                    e.DcCancel == true ||
                    e.LabourDcOutgoingSubs.Any(s => s.ItemCancel == true)
                );
        }
        public async Task<bool> GetLabourDcByIdIsReduceInvoicevalueAsync(int dcId)
        {
            return await _unitOfWork.LabourDcOutgoings
                .GetQueryable()
                .Where(x => x.DcId == dcId)
                .Select(x => x.ReduceInvAsPerRej)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> GetLabourDcByIdIsCancelOutItemByAsync(int dcId)
        {
            return await _unitOfWork.LabourDcOutgoings
                .GetQueryable()
                .AnyAsync(dc =>
                    dc.DcId == dcId &&
                    (
                        dc.DcCancel ||
                        dc.LabourDcOutgoingSubs.Any(sub =>
                            sub.TransType == "Out" &&
                            sub.ItemCancel
                        )
                    )
                );
        }

        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int CustId)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.MfgPoSubs.GetQueryable()
                                  join q in _unitOfWork.MfgPos.GetQueryable() on qs.PoId equals q.PoId
                                  where qs.ItemId == itemId && q.CustId == CustId
                                  orderby q.PoId descending
                                  select qs.UnitPrice)
                                    .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.MfgPoSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.PoSubId descending
                                      select qs.UnitPrice)
                                        .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId && isub.VendorId == CustId
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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for CustId: {CustId}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.");
            }
        }

        public async Task<bool> IsOpenPoAsync(int poSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs
                .GetQueryable()
                .Where(s => s.PoSubId == poSubId)
                .Join(_unitOfWork.MfgPos.GetQueryable(),
                      sub => sub.PoId,
                      po => po.PoId,
                      (sub, po) => po.IsOpenPO && po.MfgORLab == false)
                .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                return false;
                throw;
            }

        }

        public async Task<decimal> GetScnItemBalQtyFromSCNSubId(int PoSubId)
        {
            try
            {
                return await _unitOfWork.MfgDcSubs.GetQueryable()
                    .Where(e => e.RefPoSubId == PoSubId)
                    .Select(e => e.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching BalQty for GetScnItemBalQtyFrom PoSubId: {PoSubId}");
                throw new InvalidOperationException("Failed to retrieve SCN balance quantity.");
            }
        }

        public async Task<LabourDcOutgoingSubVM?> GetDcSubItemDetailByDcSubIdAsync(int dcSubId)
        {
            try
            {
                var dc = await _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                    .FirstOrDefaultAsync(x => x.DcSubId == dcSubId);

                if (dc == null)
                    return null;


                bool invExists = await (
                    from sub in _unitOfWork.LabInvSubs.GetQueryable()
                    join inv in _unitOfWork.LabInvs.GetQueryable()
                        on sub.LabInvId equals inv.LabInvId
                    where sub.RefDcSubId == dcSubId && !sub.ItemCancel && !inv.InvCancel
                    select sub
                ).AnyAsync();

                decimal realTimeBalQty;

                if (invExists)
                {

                    decimal invoicedQty = await (
                        from sub in _unitOfWork.LabInvSubs.GetQueryable()
                        join inv in _unitOfWork.LabInvs.GetQueryable()
                            on sub.LabInvId equals inv.LabInvId
                        where sub.RefDcSubId == dcSubId && !sub.ItemCancel && !inv.InvCancel
                        select (decimal?)sub.Qty
                    ).SumAsync() ?? 0m;

                    realTimeBalQty = dc.Qty - invoicedQty;
                }
                else
                {
                    realTimeBalQty = dc.Qty;
                }

                return new LabourDcOutgoingSubVM
                {
                    Qty = dc.Qty,
                    RejectQty = dc.RejectQty,
                    ReworkQty = dc.ReworkQty,
                    BalQty = realTimeBalQty
                };
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching Dc sub item detail for DcSubId: {dcSubId}"
                );
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }


        public async Task UpdateItemCancelAsync(LabourDcOutgoingSubVM subItem)
        {
            if (subItem == null)
                throw new ArgumentNullException(nameof(subItem), "Subitem cannot be null.");

            if (subItem.DcSubId == 0)
                throw new ArgumentException("Cannot update unsaved subitem.");

            try
            {
                var entity = await _unitOfWork.LabourDcOutgoingSubs
                    .FirstOrDefaultAsync(x => x.DcSubId == subItem.DcSubId);

                if (entity == null)
                    throw new KeyNotFoundException($"Subitem with DcSubId {subItem.DcSubId} not found.");

                entity.ItemCancel = subItem.ItemCancel;

                await _unitOfWork.LabourDcOutgoingSubs.UpdateAsync(entity);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in UpdateItemCancelAsync for ItemCode {subItem.ItemCode}");
                throw;
            }
        }

        public async Task<List<LabourDcOutgoingSubVM>> GetDcSubByDcIdAsync(int dcId)
        {
            try
            {
                var subs = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Include(s => s.CostCenter)
                    .Where(s => s.DcId == dcId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return _mapper.Map<List<LabourDcOutgoingSubVM>>(subs);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Labour DC items for DcId: {dcId}");
                throw new InvalidOperationException("Failed to retrieve Labour DC sub-items. Please try again.");
            }
        }

        //Dc cancel
        public async Task UpdatedCancelStatusAndAddOrRevertQty(LabourDcOutgoingVM dcVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();

                var existingDC = await _unitOfWork.LabourDcOutgoings.GetAsync(dcVM.DcId);
                if (existingDC == null)
                    throw new InvalidOperationException("Labour Dc Outgoing not found.");

                var subs = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.DcId == dcVM.DcId)
                    .ToListAsync();

                if (!dcVM.DcCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateLabourSCNBalanceBeforeRevertAsync(sub, dcVM);
                    }
                }

                existingDC.DcCancel = dcVM.DcCancel;
                existingDC.CancelReason = dcVM.CancelReason;
                await _unitOfWork.LabourDcOutgoings.UpdateAsync(existingDC);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {

                    decimal newAcceptedQty, oldAcceptedQty = 0;
                    bool rejRetrack = false;

                    if (existingDC.DcCancel)
                    {
                        if (sub.DcSubId > 0)
                        {
                            await RollbackTracksAddIssueQtyAsync(sub.DcSubId, existingDC.SCNRejectReworkReturn);
                        }

                        if (sub.RefPoSubId.GetValueOrDefault() > 0 || sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                            if (rejRetrack)
                            {
                                oldAcceptedQty = sub.Qty - sub.RejectQty.GetValueOrDefault();
                            }
                            else
                            {
                                oldAcceptedQty = sub.Qty;
                            }
                            await AdjustGrnOutBalanceAsync(sub.RefGRNSubId, oldAcceptedQty, 0, "Labour Dc Outgoing updated");
                        }
                        await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);
                    }
                    else
                    {
                        if (sub.RefPoSubId > 0 || sub.RefGRNSubId > 0)
                        {
                            if (!dcVM.Manual)
                            {
                                await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.DcSubId, sub.RefPoSubId ?? 0,
                                    sub.ItemId, sub.Qty, screenCode, currentUser, now, sub.RefGRNSubId ?? 0, dcVM.SCNRejectReworkReturn);
                            }
                        }

                        if (sub.TransType == "Out")
                            await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.DcSubId, sub.ItemId, sub.Qty, currentUser, now, dcVM.LabourDcOutgoingSubVMs, dcVM.ReceivedReturnRM, dcVM.SCNRejectReworkReturn);


                        if (sub.RefPoSubId.GetValueOrDefault() > 0 || sub.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                            if (rejRetrack)
                            {
                                newAcceptedQty = sub.Qty - sub.RejectQty.GetValueOrDefault();
                            }
                            else
                            {
                                newAcceptedQty = sub.Qty;
                            }

                            await AdjustGrnOutBalanceAsync(sub.RefGRNSubId, 0, newAcceptedQty, "Labour DcOutGoing Create");
                        }


                        if (sub.TransType == "Out")
                        {
                            if (sub.RefPoSubId > 0 && (sub.RcSubIds != "" && sub.RcSubIds != null) && sub.Item.CategoryCode == 2)
                            {

                                await IssueStockBySeqLogicAsync(sub, existingDC, screenCode);

                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingDC.StoreIssId ?? 0, sub.Qty,
                                                    sub.UnitPrice, sub.BatchNo, screenCode, sub.DcSubId, existingDC.DcNo, existingDC.DcDate);

                            }
                        }

                        //await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingDC.StoreIssId ?? 0, sub.Qty,
                        //   sub.UnitPrice, sub.BatchNo, screenCode, sub.DcSubId, existingDC.DcNo, existingDC.DcDate, allowMultipleIssue: true);

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
        public async Task ValidateLabourSCNBalanceBeforeRevertPoWiseAsync(LabourDcOutgoingSub sub, LabourDcOutgoingVM LabourDcOutgoingVMs)
        {
            if (sub.RefSCNSubId.GetValueOrDefault() > 0)
            {
                var entity = await _unitOfWork.LabourSCNSubs.GetAsync(sub.RefSCNSubId ?? 0);
                if (entity == null)
                    throw new InvalidOperationException($"Labour SCN not found for RefPoSubId: {sub.RefSCNSubId}");

                if (LabourDcOutgoingVMs.SCNRejectReworkReturn)
                {
                    if (entity.SCNBalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Labour SCN balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
                else
                {
                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Labour SCN balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }


            }
            if (!LabourDcOutgoingVMs.ReceivedReturnRM)
            {
                if (sub.RefGRNSubId.GetValueOrDefault() > 0 && LabourDcOutgoingVMs.WithoutPoDc)
                {
                    var entity = await _unitOfWork.LabourGRNSubs.GetAsync(sub.RefGRNSubId ?? 0);
                    if (entity == null)
                        throw new InvalidOperationException($"Labour GRN not found for RefGRNSubId: {sub.RefGRNSubId}");

                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Labour GRN balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
                if (sub.RefPoSubId.GetValueOrDefault() > 0)
                {
                    var entity = await _unitOfWork.MfgPoSubs.GetAsync(sub.RefPoSubId ?? 0);
                    if (entity == null)
                        throw new InvalidOperationException($"Labour MfgPo not found for RefGRNSubId: {sub.RefPoSubId}");

                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Labour MfgPo balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
            }

        }
        public async Task ValidateLabourSCNBalanceBeforeRevertAsync(LabourDcOutgoingSub sub, LabourDcOutgoingVM LabourDcOutgoingVMs)
        {
            if (sub.RefSCNSubId.GetValueOrDefault() > 0)
            {
                var entity = await _unitOfWork.LabourSCNSubs.GetAsync(sub.RefSCNSubId ?? 0);
                if (entity == null)
                    throw new InvalidOperationException($"Labour SCN not found for RefPoSubId: {sub.RefSCNSubId}");

                if (LabourDcOutgoingVMs.SCNRejectReworkReturn)
                {
                    if (entity.SCNBalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Labour SCN balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
                else
                {
                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Labour SCN balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }


            }
            if (!LabourDcOutgoingVMs.ReceivedReturnRM)
            {
                if (sub.RefGRNSubId.GetValueOrDefault() > 0 && LabourDcOutgoingVMs.WithoutPoDc)
                {
                    var entity = await _unitOfWork.LabourGRNSubs.GetAsync(sub.RefGRNSubId ?? 0);
                    if (entity == null)
                        throw new InvalidOperationException($"Labour GRN not found for RefGRNSubId: {sub.RefGRNSubId}");

                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Labour GRN balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }

                if (sub.RefPoSubId.GetValueOrDefault() > 0)
                {

                    var entity = await _unitOfWork.LabourGRNSubs
                                 .GetQueryable()
                                 .FirstOrDefaultAsync(x => x.RefPoSubId == sub.RefPoSubId && x.TransType == "Out");

                    if (entity == null)
                        throw new InvalidOperationException($"Labour MfgPo not found for RefGRNSubId: {sub.RefPoSubId}");

                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Labour MfgPo balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
            }

        }

        public async Task UpdateItemCancelAndAddorRevertAsync(LabourDcOutgoingSubVM subItem, int screenCode, LabourDcOutgoingVM outgoingVM)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();

                decimal newAcceptedQty, oldAcceptedQty = 0;
                bool rejRetrack = false;

                var subEntity = await _unitOfWork.LabourDcOutgoingSubs.GetQueryable().Where
                                (x => x.DcSubId == subItem.DcSubId).FirstOrDefaultAsync();

                if (subEntity == null)
                    throw new KeyNotFoundException($"Subitem with DcSubId {subItem.DcSubId} not found.");

                if (!subItem.ItemCancel)
                {
                    await ValidateLabourSCNBalanceBeforeRevertAsync(subEntity, outgoingVM);
                }

                subEntity.ItemCancel = subItem.ItemCancel;
                subEntity.CancelItemReason = subItem.CancelItemReason;
                await _unitOfWork.LabourDcOutgoingSubs.UpdateAsync(subEntity);
                await _unitOfWork.SaveAsync();

                if (subItem.ItemCancel)
                {
                    //await AdjustSCNSubBalanceAsync(subEntity.RefSCNSubId, subEntity.Qty, 0, $"Labour SCN Item Cancel - {subItem.ItemCode}");
                    if (subItem.DcSubId > 0)
                    {
                        if (subItem.TransType == "Out")
                        {
                            await RollbackTracksAddIssueQtyAsync(subItem.DcSubId, outgoingVM.SCNRejectReworkReturn);

                        }
                        else
                        {
                            await AdjustSCNSubBalanceAsync(subItem.RefSCNSubId, subItem.Qty ?? 0, 0, " Labour DcOutgoing Cancel Item");

                        }

                    }
                    if (subItem.RefPoSubId.GetValueOrDefault() > 0 || subItem.RefGRNSubId.GetValueOrDefault() > 0)
                    {

                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(subItem.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            oldAcceptedQty = subItem.Qty.GetValueOrDefault() - subItem.RejectQty.GetValueOrDefault();

                        }
                        else
                        {
                            oldAcceptedQty = subItem.Qty.GetValueOrDefault();
                        }

                        await AdjustGrnOutBalanceAsync(subItem.RefGRNSubId, oldAcceptedQty, 0, "Labour DcOutGoing Create");

                    }

                    // await DeleteStockAddAsync(subItem.GRNSubId, subItem.ItemIdIN.Value, screenCode, GRNNo);
                    await DeleteStockIssueAndTrackAsync(subEntity.DcSubId, subEntity.ItemId, screenCode);
                }
                else
                {
                    // await AdjustSCNSubBalanceAsync(subEntity.RefSCNSubId, 0, subEntity.Qty, $"Labour SCN Revert Cancel - {subItem.ItemCode}");
                    if (subItem.RefPoSubId > 0 || subItem.RefGRNSubId > 0)
                    {
                        await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(subItem.DcSubId, subItem.RefPoSubId ?? 0,
                                subItem.ItemId, subItem.Qty ?? 0, screenCode, currentUser, now, subItem.RefGRNSubId ?? 0,
                                outgoingVM.SCNRejectReworkReturn);
                    }

                    if (subItem.TransType == "Out")
                    {
                        await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(subItem.DcSubId, subItem.ItemId, subItem.Qty ?? 0, currentUser, now, new List<LabourDcOutgoingSubVM?> { subItem }, outgoingVM.ReceivedReturnRM, outgoingVM.SCNRejectReworkReturn);
                    }
                    else
                    {
                        await AdjustSCNSubBalanceAsync(subItem.RefSCNSubId, 0, subItem.Qty ?? 0, " Labour DcOutgoing Cancel Item");

                    }

                    if (subItem.RefPoSubId > 0 || subItem.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(subItem.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            newAcceptedQty = subItem.Qty.GetValueOrDefault() - subItem.RejectQty.GetValueOrDefault();

                        }
                        else
                        {
                            newAcceptedQty = subItem.Qty.GetValueOrDefault();
                        }

                        await AdjustGrnOutBalanceAsync(subItem.RefGRNSubId, 0, newAcceptedQty, "Labour Dc Create");
                    }

                    await _stockManagerService.IssueOrUpdateStockAsync(subEntity.ItemId, outgoingVM.StoreIssId.Value, subEntity.Qty,
                            subEntity.UnitPrice, subEntity.BatchNo, screenCode, subEntity.DcSubId, outgoingVM.DcNo, outgoingVM.DcDate, allowMultipleIssue: true);
                }

                await UpdateDcTallyStatusAsync(subItem.DcId);

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

        public async Task UpdateDcTallyStatusAsync(int dcId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Where(x => x.DcId == dcId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var quotation = await _unitOfWork.LabourDcOutgoings.GetAsync(dcId);
                if (quotation == null)
                    return;

                quotation.DcTally = (totalBalQty == 0);

                await _unitOfWork.LabourDcOutgoings.UpdateAsync(quotation);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateGRNTallyStatusAsync] Error updating Labour Dc Dcid:- {dcId}");
                throw new InvalidOperationException("Failed to update labour Dc Tally status. Please contact support.");
            }
        }

        public async Task DeleteAndResequenceAsync(LabourDcOutgoingSubVM subitem, LabourDcOutgoingVM dcVM, int ScreenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.DcSubId > 0)
                {
                    decimal newAcceptedQty, oldAcceptedQty = 0;

                    bool rejRetrack = false;
                    var entity = await _unitOfWork.LabourDcOutgoingSubs.GetAsync(subitem.DcSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.DcSubId > 0)
                    {
                        if (entity.TransType == "Out")
                        {
                            await RollbackTracksAddIssueQtyAsync(entity.DcSubId, dcVM.SCNRejectReworkReturn);
                        }
                        else
                        {
                            await AdjustSCNSubBalanceAsync(entity.RefSCNSubId, entity.Qty, 0, " Labour DcOutgoing Cancel Item");

                        }

                    }

                    if (entity.RefPoSubId.GetValueOrDefault() > 0 || entity.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(entity.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            newAcceptedQty = entity.Qty - entity.RejectQty.GetValueOrDefault();
                        }
                        else
                        {
                            newAcceptedQty = entity.Qty;
                        }

                        await AdjustGrnOutBalanceAsync(entity.RefGRNSubId, newAcceptedQty, 0, "Labour Dc Outgoing updated");
                    }


                    if (entity.TransType == "Out")
                    {
                        await DeleteStockIssueAndTrackAsync(entity.DcSubId, entity.ItemId, ScreenCode);
                    }
                    await _unitOfWork.LabourDcOutgoingSubs.DeleteAsync(entity.DcSubId);
                    await _unitOfWork.SaveAsync();

                    await UpdateDcTallyStatusAsync(entity.DcId);
                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Labour Dc",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Dc No: {dcVM?.DcNo}"
                    );
                }
                else
                {

                    dcVM.LabourDcOutgoingSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Where(x => x.DcId == dcVM.DcId)
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
        public async Task<List<Dictionary<string, object>>> LoadPoSubsByReturnMaterialRejRewPoIds(List<int> poIds, List<int> scnIds, int storeIssId)
        {
            try
            {
                if (scnIds == null || scnIds.Count == 0)
                    return new List<Dictionary<string, object>>();



                var scnData = await (
                    from s in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                    join ss in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                        on s.SCNId equals ss.SCNId
                    where scnIds.Contains(s.SCNId)
                          && !s.SCNCancel

                          && s.Authorized
                          && !ss.ItemCancel
                          && ss.SCNBalQty > 0
                    select new
                    {
                        ss.ItemId,
                        ss.SCNBalQty,
                        ss.CostId,
                        ProjectNo = ss.CostCenter.ProjectNo,
                        ss.Item.ItemCode,
                        ss.Item.ItemName,
                        ss.Item.Specification,
                        ss.Item.MeasureUnit,
                        ss.Item.HSNCode,
                        Category = ss.Item.Category.CategoryName,
                        ss.Item.CategoryCode
                    }
                ).ToListAsync();

                var itemIds = scnData.Select(x => x.ItemId).Distinct().ToList();



                var poDict = await (from p in _unitOfWork.MfgPoSubs.GetQueryable().AsNoTracking()
                                    where poIds.Contains(p.PoId)
                                          && !p.ItemCancel
                                          && !p.MfgPo.PoCancl
                                    select new
                                    {
                                        p.ItemId,
                                        p.PoSubId,
                                        p.UnitPrice,
                                        p.MfgPo.PONo,
                                        p.MfgPo.PODate
                                    })
                .GroupBy(x => x.ItemId)
                .Select(g => g.FirstOrDefault())
                .ToDictionaryAsync(x => x.ItemId, x => x);

                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeIssId);

                var result = scnData.Select(x =>
                {
                    stockDict.TryGetValue(x.ItemId, out decimal stockQty);
                    poDict.TryGetValue(x.ItemId, out var po);

                    return new Dictionary<string, object>
                    {

                        ["Selected"] = false,

                        ["RefPoSubId"] = po?.PoSubId,
                        ["PoNo"] = po?.PONo,
                        ["PODate"] = po?.PODate,
                        ["UnitPrice"] = po?.UnitPrice ?? 0,

                        ["ItemId"] = x.ItemId,
                        ["ItemCode"] = x.ItemCode ?? "",
                        ["ItemName"] = x.ItemName ?? "",
                        ["Specification"] = x.Specification ?? "",
                        ["UOM"] = x.MeasureUnit ?? "",
                        ["HSNCode"] = x.HSNCode ?? "",
                        ["Category"] = x.Category ?? "",

                        ["Qty"] = x.SCNBalQty,
                        ["BalQty"] = x.SCNBalQty,

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

        public async Task<List<Dictionary<string, object>>> LoadPoSubsByReturnMaterialPoIds(List<int> poIds, List<int> scnIds, int storeIssId, bool SCNRejRewReturn)
        {
            try
            {
                if (scnIds == null || scnIds.Count == 0)
                    return new List<Dictionary<string, object>>();

                var scnData = await (
                    from s in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                    join ss in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                        on s.SCNId equals ss.SCNId
                    where scnIds.Contains(s.SCNId)
                          && !s.SCNCancel
                          && s.Authorized
                          && !ss.ItemCancel
                          && (SCNRejRewReturn ? ss.SCNBalQty > 0 : ss.BalQty > 0)
                    select new
                    {
                        ss.ItemId,
                        BalQty = SCNRejRewReturn ? ss.SCNBalQty : ss.BalQty,
                        ss.CostId,
                        ProjectNo = ss.CostCenter.ProjectNo,
                        ss.Item.ItemCode,
                        ss.Item.ItemName,
                        ss.Item.Specification,
                        ss.Item.MeasureUnit,
                        ss.Item.HSNCode,
                        Category = ss.Item.Category.CategoryName,
                        ss.Item.CategoryCode
                    }
                ).ToListAsync();

                var itemIds = scnData.Select(x => x.ItemId).Distinct().ToList();

                var poDict = await (
                    from p in _unitOfWork.MfgPoSubs.GetQueryable().AsNoTracking()
                    where poIds.Contains(p.PoId)
                          && !p.ItemCancel
                          && !p.MfgPo.PoCancl
                    select new
                    {
                        p.ItemId,
                        p.PoSubId,
                        p.UnitPrice,
                        p.MfgPo.PONo,
                        p.MfgPo.PODate,
                        p.LineNo
                    }
                ).GroupBy(x => x.ItemId)
                 .Select(g => g.FirstOrDefault())
                 .ToDictionaryAsync(x => x.ItemId, x => x);

                var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeIssId);

                var result = scnData.Select(x =>
                {
                    stockDict.TryGetValue(x.ItemId, out decimal stockQty);
                    poDict.TryGetValue(x.ItemId, out var po);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["RefPoSubId"] = po?.PoSubId,
                        ["PoNo"] = po?.PONo,
                        ["PODate"] = po?.PODate,
                        ["LineNo"] = po?.LineNo,
                        ["UnitPrice"] = po?.UnitPrice ?? 0,

                        ["ItemId"] = x.ItemId,
                        ["ItemCode"] = x.ItemCode ?? "",
                        ["ItemName"] = x.ItemName ?? "",
                        ["Specification"] = x.Specification ?? "",
                        ["UOM"] = x.MeasureUnit ?? "",
                        ["HSNCode"] = x.HSNCode ?? "",
                        ["Category"] = x.Category ?? "",

                        ["Qty"] = x.BalQty,
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

        public async Task<List<Dictionary<string, object>>> LoadPoSubsByPoIds(List<int> poIds, int storeIssId)
        {
            try
            {
                if (poIds == null || !poIds.Any())
                    return new();

                var poLines = await (
                    from g in _unitOfWork.MfgPoSubs.GetQueryable().AsNoTracking()
                    where g.MfgPo != null
                        && poIds.Contains(g.PoId)
                        && !g.ItemCancel
                        && !g.MfgPo.PoCancl
                        && !g.MfgPo.MfgORLab && g.BalQty > 0 

                    select new
                    {
                        g.PoId,
                        g.PoSubId,
                        g.LineNo,
                        PoNo = g.MfgPo.PONo,
                        g.MfgPo.Suffix,
                        g.MfgPo.PODate,
                        g.MfgPo.IsOpenPO,
                        g.BalQty,
                        g.Qty,

                        g.ItemId,
                        ItemCode = g.Item.ItemCode,
                        ItemName = g.Item.ItemName,
                        g.Item.Specification,
                        MeasureUnit = g.Item.MeasureUnit,
                        g.Item.HSNCode,

                        Category = g.Item.Category.CategoryName,
                        g.Item.CategoryCode,

                        g.UnitPrice,
                        g.Remark,
                        CostCenterId = g.CostId,
                        ProjectNo = g.CostCenter.ProjectNo
                    }).ToListAsync();

                if (!poLines.Any())
                    return new();

                var poSubIds = poLines.Select(x => x.PoSubId).Distinct().ToList();

                // ------------------------------------------------
                // PO Balance from GRN OUT
                // ------------------------------------------------
                var grnBalDict = await _unitOfWork.LabourGRNSubs.GetQueryable()
                    .AsNoTracking()
                    .Where(x =>
                        x.RefPoSubId.HasValue &&
                        poSubIds.Contains(x.RefPoSubId.Value) &&
                        x.TransType == "Out" &&
                        !x.ItemCancel)
                    .GroupBy(x => x.RefPoSubId.Value)
                    .Select(g => new
                    {
                        PoSubId = g.Key,
                        BalQty = g.Sum(x => x.BalQty)
                    })
                    .ToDictionaryAsync(x => x.PoSubId, x => x.BalQty);

                // ------------------------------------------------
                // Route Card
                // ------------------------------------------------
                var poIdList = poLines.Select(x => x.PoId).Distinct().ToList();

                var rcSubs = await _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(rcs =>
                        rcs.RouteCard != null &&
                        rcs.IsFinalProcess &&
                        rcs.RouteCard.RefPoId.HasValue &&
                        poIdList.Contains(rcs.RouteCard.RefPoId.Value))
                    .Select(rcs => new
                    {
                        rcs.RCSubId,
                        rcs.ItemIdIn,
                        rcs.RouteCard.RefPoId
                    })
                    .ToListAsync();

                var rcSubIds = rcSubs.Select(x => x.RCSubId).ToList();

                // ------------------------------------------------
                // RC STOCK
                // ------------------------------------------------
                var rcStockDict = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(s =>
                        s.StoreId == storeIssId &&
                        s.RcSubID.HasValue &&
                        rcSubIds.Contains(s.RcSubID.Value))
                    .GroupBy(s => s.ItemId)
                    .Select(g => new
                    {
                        ItemId = g.Key,
                        BalQty = g.Sum(x => x.BalQty)
                    })
                    .Where(x => x.BalQty > 0)
                    .ToDictionaryAsync(x => x.ItemId, x => x.BalQty);

                // ------------------------------------------------
                // NORMAL STOCK
                // ------------------------------------------------

                var stockDict = await _unitOfWork.StockAdds.GetQueryable()
                    .Where(s =>
                        s.StoreId == storeIssId &&
                        !s.RcSubID.HasValue)
                    .GroupBy(s => s.ItemId)
                    .Select(g => new
                    {
                        ItemId = g.Key,
                        BalQty = g.Sum(x => x.BalQty)
                    })
                    .Where(x => x.BalQty > 0)
                    .ToDictionaryAsync(x => x.ItemId, x => x.BalQty);

                // ------------------------------------------------
                // BUILD RESULT
                // ------------------------------------------------
                var data = new List<Dictionary<string, object>>();

                foreach (var x in poLines)
                {
                    decimal poBalance = 0;
                    grnBalDict.TryGetValue(x.PoSubId, out poBalance);

                    if (poBalance <= 0)
                        continue;

                    decimal stockQty = 0;

                    bool hasRouteCard = rcSubs.Any(r => r.RefPoId == x.PoId);

                    if (hasRouteCard && x.CategoryCode != 7 && x.CategoryCode != 3)
                        rcStockDict.TryGetValue(x.ItemId, out stockQty);
                    else
                        stockDict.TryGetValue(x.ItemId, out stockQty);

                    data.Add(new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["IsOpenPo"] = x.IsOpenPO,
                        ["PoSubId"] = x.PoSubId,
                        ["PoId"] = x.PoId,
                        ["PoNo"] = x.PoNo + x.Suffix,
                        ["LineNo"] = x.LineNo,
                        ["PODate"] = x.PODate?.ToString("dd/MM/yyyy"),

                        ["ItemId"] = x.ItemId,
                        ["ItemCode"] = x.ItemCode ?? "",
                        ["ItemName"] = x.ItemName ?? "",
                        ["ItemSpecification"] = x.Specification ?? "",
                        ["UOM"] = x.MeasureUnit ?? "",
                        ["HSNCode"] = x.HSNCode ?? "",

                        ["CategoryCode"] = x.CategoryCode,
                        ["Category"] = x.Category ?? "",

                        ["Qty"] = x.Qty,
                        ["BalQty"] = x.BalQty,

                        ["UnitPrice"] = x.UnitPrice,
                        ["Stock Qty"] = stockQty,

                        ["CostCenterId"] = x.CostCenterId,
                        ["ProjectNo"] = x.ProjectNo ?? "",
                        ["Remark"] = x.Remark ?? "",

                        ["RefRCSubID"] = hasRouteCard ? string.Join(",", rcSubIds) : ""
                    });
                }

                return data;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error loading PO subs");
                throw;
            }
        }

        public async Task<List<Dictionary<string, object>>> ScnGrnbyreturnItemRetyurnByCustomer(int custId, List<int> grnIds, int storeIssId, bool ScnRejRewReturn)
        {
            try
            {
                if (grnIds == null || grnIds.Count == 0)
                    return new List<Dictionary<string, object>>();

                var grnSubIds = await (
                               from l in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                               join ls in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                                   on l.SCNId equals ls.SCNId
                               join gs in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                                   on ls.RefGRNSubId equals gs.GRNSubId
                               where
                                   grnIds.Contains(gs.GRNId)
                                   && !l.SCNCancel

                                   && l.Authorized
                                   && !ls.ItemCancel
                                   && (ScnRejRewReturn ? ls.SCNBalQty > 0 : ls.BalQty > 0)
                               select gs.GRNSubId
                           ).Distinct().ToListAsync();


                var validGrnIds = await _unitOfWork.LabourGRNSubs.GetQueryable()
                                 .AsNoTracking()
                                 .Where(x => grnSubIds.Contains(x.GRNSubId))
                                 .Select(x => x.GRNId)
                                 .Distinct()
                                 .ToListAsync();

                var result = await (
                                      from g in _unitOfWork.LabourGRNs.GetQueryable().AsNoTracking()
                                      join gs in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                                          on g.GRNId equals gs.GRNId

                                      join ls in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                                          on gs.GRNSubId equals ls.RefGRNSubId

                                      join l in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                                          on ls.SCNId equals l.SCNId

                                      where
                                          grnIds.Contains(g.GRNId)
                                          && gs.TransType == "In"
                                          && !gs.ItemCancel
                                          && !l.SCNCancel

                                          && l.Authorized
                                          && !ls.ItemCancel
                                          && (ScnRejRewReturn ? ls.SCNBalQty > 0 : ls.BalQty > 0)

                                      select new
                                      {
                                          gs.GRNSubId,
                                          g.GRNId,
                                          g.GRNNo,
                                          g.GRNDate,
                                          g.RefDcNo,
                                          g.RefDcDate,
                                          g.Suffix,
                                          g.Remarks,

                                          // Item
                                          gs.ItemId,
                                          gs.Item.ItemCode,
                                          gs.Item.ItemName,
                                          gs.Item.Specification,
                                          gs.Item.MeasureUnit,
                                          gs.Item.HSNCode,
                                          CategoryName = gs.Item.Category.CategoryName,
                                          gs.Item.CategoryCode,
                                          gs.RowRemarks,

                                          // Scn Bal Qty Because Shwo the Return item balnace qty
                                          BalQty = ScnRejRewReturn ? ls.SCNBalQty : ls.BalQty,

                                          gs.UnitPrice,

                                          // Cost Center
                                          CostCenterId = gs.CostId > 0 ? (int?)gs.CostId : null,
                                          ProjectNo = gs.CostCenter.ProjectNo,
                                          TotalQty = gs.Qty,

                                          // Optional
                                          SCNSubId = ls.SCNSubId,
                                          GRNSubIdIn = _unitOfWork.LabourSCNSubs
                                                                           .GetQueryable()
                                                                           .Where(ls =>
                                                                               _unitOfWork.LabourGRNSubs
                                                                                   .GetQueryable()
                                                                                   .Where(gs => gs.GRNId == g.GRNId)
                                                                                   .Select(gs => gs.GRNSubId)
                                                                                   .Contains(ls.RefGRNSubId ?? 0)
                                                                           )
                                                                           .Select(ls => ls.RefGRNSubId)
                                                                           .FirstOrDefault()
                                      }
                                  ).ToListAsync();


                var itemIds = result.Where(x => x.ItemId.HasValue).Select(x => x.ItemId.Value).Distinct().ToList();

                //var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeIssId);


                var stockDict = await _unitOfWork.StockAdds.GetQueryable()
                               .Where(s =>
                                   s.StoreId == storeIssId &&
                                   !s.RcSubID.HasValue)
                               .GroupBy(s => s.ItemId)
                               .Select(g => new
                               {
                                   ItemId = g.Key,
                                   BalQty = g.Sum(x => x.BalQty),
                               })
                               .Where(x => x.BalQty > 0)
                               .ToDictionaryAsync(x => x.ItemId, x => x.BalQty);

                return result.Select(r =>
                {
                    stockDict.TryGetValue(r.ItemId ?? 0, out var stockQty);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["GRNSubId"] = r.GRNSubId,
                        ["GRNId"] = r.GRNId,
                        ["Suffix"] = r.Suffix,
                        ["GRNNo"] = r.GRNNo,
                        ["GRNDate"] = r.GRNDate,
                        ["RefDcNo"] = r.RefDcNo,
                        ["RefDcDate"] = r.RefDcDate,

                        ["ItemId"] = r.ItemId,
                        ["ItemCode"] = r.ItemCode ?? string.Empty,
                        ["ItemName"] = r.ItemName ?? string.Empty,
                        ["Specification"] = r.Specification ?? string.Empty,
                        ["UOM"] = r.MeasureUnit ?? string.Empty,
                        ["HSNCode"] = r.HSNCode ?? string.Empty,
                        ["Category"] = r.CategoryName ?? string.Empty,
                        ["CategoryCode"] = r.CategoryCode,

                        ["Qty"] = r.BalQty,
                        ["BalQty"] = r.BalQty,
                        ["UnitPrice"] = r.UnitPrice,
                        ["Stock Qty"] = stockQty,

                        ["CostCenterId"] = r.CostCenterId,
                        ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                        ["Remark"] = r.RowRemarks ?? string.Empty,
                        ["TotalQty"] = r.TotalQty,
                        ["GRNSubIdIn"] = r.GRNSubIdIn
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching SCN / GRN / PO details for CustId: {custId}");
                throw;
            }
        }

        public async Task<List<Dictionary<string, object>>> ScnGrnPoDetailsByCustomer(int custId, List<int> grnIds, int storeIssId)
        {
            try
            {
                if (grnIds == null || grnIds.Count == 0)
                    return new List<Dictionary<string, object>>();

                var grnSubIds = await (
                               from l in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                               join ls in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                                   on l.SCNId equals ls.SCNId
                               join gs in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                                   on ls.RefGRNSubId equals gs.GRNSubId
                               where
                                   grnIds.Contains(gs.GRNId)
                                   && !l.SCNCancel
                                   && !l.SCNTally
                                   && l.Authorized
                                   && !ls.ItemCancel
                                   && ls.BalQty > 0
                               select gs.GRNSubId
                           ).Distinct().ToListAsync();


                var validGrnIds = await _unitOfWork.LabourGRNSubs.GetQueryable()
                                 .AsNoTracking()
                                 .Where(x => grnSubIds.Contains(x.GRNSubId))
                                 .Select(x => x.GRNId)
                                 .Distinct()
                                 .ToListAsync();

                var result = await (
                             from g in _unitOfWork.LabourGRNs.GetQueryable().AsNoTracking()
                             join gs in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                                 on g.GRNId equals gs.GRNId
                             where
                                 validGrnIds.Contains(g.GRNId)
                                 && gs.TransType == "Out" && !gs.ItemCancel && gs.BalQty > 0
                             select new
                             {

                                 gs.GRNSubId,
                                 g.GRNId,
                                 GRNNo = g != null ? g.GRNNo : null,
                                 GRNDate = g != null ? g.GRNDate : (DateTime?)null,
                                 RefDcNo = g != null ? g.RefDcNo : null,
                                 RefDcDate = g != null ? g.RefDcDate : (DateTime?)null,
                                 g.Suffix,
                                 g.Remarks,
                                 // Item
                                 ItemId = gs != null ? gs.ItemId : null,
                                 ItemCode = gs != null ? gs.Item.ItemCode : null,
                                 ItemName = gs != null ? gs.Item.ItemName : null,
                                 Specification = gs != null ? gs.Item.Specification : null,
                                 MeasureUnit = gs != null ? gs.Item.MeasureUnit : null,
                                 HSNCode = gs != null ? gs.Item.HSNCode : null,
                                 CategoryName = gs != null ? gs.Item.Category.CategoryName : null,
                                 CategoryCode = gs != null ? gs.Item.CategoryCode : null,
                                 gs.RowRemarks,
                                 // Qty
                                 gs.BalQty,
                                 gs.UnitPrice,

                                 // Cost Center
                                 CostCenterId = gs.CostId > 0 ? (int?)gs.CostId : null,
                                 ProjectNo = gs.CostCenter != null ? gs.CostCenter.ProjectNo : null,
                                 TotalQty = gs.Qty,

                                 GRNSubIdIn = _unitOfWork.LabourSCNSubs
                                                 .GetQueryable()
                                                 .Where(ls =>
                                                     _unitOfWork.LabourGRNSubs
                                                         .GetQueryable()
                                                         .Where(gs => gs.GRNId == g.GRNId)
                                                         .Select(gs => gs.GRNSubId)
                                                         .Contains(ls.RefGRNSubId ?? 0)
                                                 )
                                                 .Select(ls => ls.RefGRNSubId)
                                                 .FirstOrDefault()

                             }).ToListAsync();

                var itemIds = result.Where(x => x.ItemId.HasValue).Select(x => x.ItemId.Value).Distinct().ToList();

                //var stockDict = await _stockManagerService.GetStockForItemsAsync(itemIds, storeIssId);


                var stockDict = await _unitOfWork.StockAdds.GetQueryable()
                   .Where(s =>
                       s.StoreId == storeIssId &&
                        !s.RcSubID.HasValue)
                       .GroupBy(s => s.ItemId)
                       .Select(g => new
                       {
                           ItemId = g.Key,
                           BalQty = g.Sum(x => x.BalQty),
                       })
                       .Where(x => x.BalQty > 0)
                       .ToDictionaryAsync(x => x.ItemId, x => x.BalQty);

                return result.Select(r =>
                {
                    stockDict.TryGetValue(r.ItemId ?? 0, out var stockQty);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["GRNSubId"] = r.GRNSubId,
                        ["GRNId"] = r.GRNId,
                        ["Suffix"] = r.Suffix,
                        ["GRNNo"] = r.GRNNo,
                        ["GRNDate"] = r.GRNDate,
                        ["RefDcNo"] = r.RefDcNo,
                        ["RefDcDate"] = r.RefDcDate,

                        ["ItemId"] = r.ItemId,
                        ["ItemCode"] = r.ItemCode ?? string.Empty,
                        ["ItemName"] = r.ItemName ?? string.Empty,
                        ["Specification"] = r.Specification ?? string.Empty,
                        ["UOM"] = r.MeasureUnit ?? string.Empty,
                        ["HSNCode"] = r.HSNCode ?? string.Empty,
                        ["Category"] = r.CategoryName ?? string.Empty,
                        ["CategoryCode"] = r.CategoryCode,

                        ["Qty"] = r.BalQty,
                        ["BalQty"] = r.BalQty,
                        ["UnitPrice"] = r.UnitPrice,
                        ["Stock Qty"] = stockQty,

                        ["CostCenterId"] = r.CostCenterId,
                        ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                        ["Remark"] = r.RowRemarks ?? string.Empty,
                        ["TotalQty"] = r.TotalQty,
                        ["GRNSubIdIn"] = r.GRNSubIdIn
                    };
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching SCN / GRN / PO details for CustId: {custId}");
                throw;
            }
        }

        public async Task<bool> IsDuplicateDcNoAsync(int CustId, string dcNo, string suffix)
        {
            try
            {
                return await _unitOfWork.LabourDcOutgoings
                .GetQueryable()
                .AnyAsync(x => x.CustId == CustId && x.DcNo == dcNo && x.Suffix == suffix);
            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to check duplicate in Labour  Dc No '{dcNo}'");
                return false;
            }
        }

        public async Task<LabourDcOutgoingSubVM?> GetSCNSubItemDetailByDcSubIdAsync(int DcSubId)
        {
            try
            {

                return await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Where(q => q.DcSubId == DcSubId)
                    .Select(q => new LabourDcOutgoingSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty
                    })
                    .FirstOrDefaultAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Dc sub item detail for DcSubId: {DcSubId}");
                throw new InvalidOperationException("Failed to retrieve DC sub-item details.");
            }
        }

        public async Task<LabourDcOutgoingVM> UpsertDcAsync(LabourDcOutgoingVM labourDcVM, int screenCode)
        {
            if (labourDcVM == null)
                throw new ArgumentNullException(nameof(labourDcVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                LabourDcOutgoing entity;

                if (labourDcVM.DcId == 0)
                {
                    entity = _mapper.Map<LabourDcOutgoing>(labourDcVM);

                    //entity.DcNo = await _unitOfWork.LabourDcOutgoings.GetLastDcNoAsync(entity.Suffix);

                    //--------AutoDcrunning
                    if (labourDcVM.NonReturnDc)
                    {
                        entity.DcNo = await GetNonReturnableDcNo(labourDcVM.Suffix);
                    }
                    else if (labourDcVM.IsManualDcNo ?? true)
                    {
                        var runningRow = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.DcType == "LABOURDCOUTGOING" &&
                                x.Suffix == entity.Suffix);

                        if (runningRow == null)
                        {
                            var dcrunn = new DcRunningNumber
                            {
                                LastNumber = Convert.ToInt64(entity.DcNo),
                                DcType = "LABOURDCOUTGOING",
                                Suffix = entity.Suffix
                            };

                            await _unitOfWork.DcRunningNumbers.CreateAsync(dcrunn);
                        }
                        else
                        {
                            runningRow.LastNumber = Convert.ToInt64(entity.DcNo);
                            await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                        }

                        await _unitOfWork.SaveAsync();
                    }
                    else
                    {
                        entity.DcNo = await _commonService.GenerateAutoRunningNoAsync("LABOURDCOUTGOING", entity.Suffix);
                    }
                    //-----------------------------------------------------------------------------------------

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.NoOfItems = labourDcVM.LabourDcOutgoingSubVMs.Count();

                    entity.LabourDcOutgoingSubs = labourDcVM.LabourDcOutgoingSubVMs
                        .Select(s => _mapper.Map<LabourDcOutgoingSub>(s))
                        .ToList();

                    await _unitOfWork.LabourDcOutgoings.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.LabourDcOutgoingSubs)
                    {
                        int refPoSubId = sub.RefPoSubId ?? 0;
                        int refGrnSubId = sub.RefGRNSubId ?? 0;

                        // ================= TRACK CREATION =================

                        if ((refPoSubId > 0 || refGrnSubId > 0) && sub.TransType == "Out" && !labourDcVM.Manual)
                        {
                            await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(
                               sub.DcSubId,
                               sub.ItemId,
                               sub.Qty,
                               currentUser,
                               now,
                               labourDcVM.LabourDcOutgoingSubVMs,
                               labourDcVM.ReceivedReturnRM,
                               labourDcVM.SCNRejectReworkReturn);
                        }

                        // ================= MANUAL TRACK =================

                        if (labourDcVM.Manual && sub.TransType == "Out")
                        {
                            await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(
                                sub.DcSubId,
                                sub.ItemId,
                                sub.Qty,
                                currentUser,
                                now,
                                labourDcVM.LabourDcOutgoingSubVMs,
                                labourDcVM.ReceivedReturnRM,
                                labourDcVM.SCNRejectReworkReturn);
                        }

                        // ================= STOCK ISSUE =================

                        if (sub.TransType == "Out")
                        {
                            if (refGrnSubId > 0 && !string.IsNullOrWhiteSpace(sub.RcSubIds) && sub.Item?.Category?.CategoryCode == 2)
                            {
                                await IssueStockBySeqLogicAsync(sub, entity, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(
                                    sub.ItemId,
                                    entity.StoreIssId ?? 0,
                                    sub.Qty,
                                    sub.UnitPrice,
                                    sub.BatchNo,
                                    screenCode,
                                    sub.DcSubId,
                                    entity.DcNo,
                                    entity.DcDate);
                            }
                        }

                        // ================= BALANCE UPDATE =================

                        if (refGrnSubId > 0)
                        {
                            await AdjustGrnOutBalanceAsync(refGrnSubId, 0, sub.Qty, "Labour Dc Create");
                        }
                    }

                    changes.AppendLine("Labour Dc Created.");
                }
                else
                {
                    entity = await _unitOfWork.LabourDcOutgoings.GetQueryable()
                        .Include(q => q.LabourDcOutgoingSubs)
                        .FirstOrDefaultAsync(q => q.DcId == labourDcVM.DcId)
                        ?? throw new InvalidOperationException("labour GRN found.");

                    var parentChanges = GetPropertyChanges(entity, labourDcVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(labourDcVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.NoOfItems = entity.LabourDcOutgoingSubs.Count();

                    await HandleChildUpdatesAsync(entity, labourDcVM.LabourDcOutgoingSubVMs, changes, screenCode);

                    changes.AppendLine("Labour Dc Updated.");
                }

                await _unitOfWork.SaveAsync();
                await UpdateDcTallyStatusAsync(labourDcVM.DcId);
                await transaction.CommitAsync();

                await LogChangesAsync(changes, labourDcVM.DcId == 0 ? "Labour Dc Created" : "Labour Dc  Updated");

                var savedEntity = await _unitOfWork.LabourDcOutgoings.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.LabourDcOutgoingSubs)
                        .ThenInclude(s => s.Item)
                        .ThenInclude(c => c.Category)
                    .Include(q => q.Customer)
                    .Include(q => q.Store)
                    .FirstOrDefaultAsync(q => q.DcId == entity.DcId);

                return _mapper.Map<LabourDcOutgoingVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Labour Dc: {labourDcVM.DcNo}");
                throw new InvalidOperationException("Failed to save Labour Dc. Please try again.");
            }
        }

        private async Task CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(int DcSubId, int ItemId, decimal Qty, string currentUser, DateTime now, List<LabourDcOutgoingSubVM?> LabourDcOutgoingSubs, bool Materialreturn, bool SCNRejRewReturn)
        {
            try
            {
                var selectedRows = LabourDcOutgoingSubs
                    .Where(x => x.RefSCNSubId.HasValue && x.Qty > 0)
                    .ToList();

                if (Materialreturn)
                {
                    selectedRows = selectedRows
                        .Where(x => x.TransType == "Out")
                        .ToList();
                }
                else
                {
                    selectedRows = selectedRows
                        .Where(x => x.TransType == "In")
                        .ToList();
                }

                if (!selectedRows.Any())
                    throw new Exception("No manual issue rows selected.");

                decimal totalOutQty = selectedRows.Sum(x => x.Qty ?? 0);
                if (totalOutQty <= 0)
                    throw new Exception("Invalid outgoing quantity.");


                foreach (var row in selectedRows)
                {
                    var issueSub = await _unitOfWork.LabourSCNSubs.GetAsync(row.RefSCNSubId!.Value);
                    if (issueSub == null)
                        throw new Exception($"IssueSub not found : {row.RefSCNSubId}");

                    decimal availableQty = 0;
                    if (SCNRejRewReturn)
                    {
                        availableQty = issueSub.SCNBalQty;

                    }
                    else
                    {
                        availableQty = issueSub.BalQty;
                    }



                    if (availableQty < row.Qty && availableQty < row.Qty)
                    {
                        throw new Exception($"Insufficient balance for IssueSubId {issueSub.SCNSubId}");

                    }

                    var track = new LabourDcReturnCompTrack
                    {
                        RefDcSubId = DcSubId,
                        RefSCNSubId = issueSub.SCNSubId,
                        RefGRNSubId = issueSub.RefGRNSubId.Value,
                        ItemIdOut = ItemId,
                        QtyOut = Qty,
                        ItemIdIn = row.ItemId,
                        QtyIn = row.Qty,
                        CreatedBy = currentUser,
                        CreatedDate = now
                    };

                    await _unitOfWork.LabourDcReturnCompTracks.CreateAsync(track);
                    await _unitOfWork.SaveAsync();

                    if (SCNRejRewReturn)
                    {
                        issueSub.SCNBalQty = availableQty - row.Qty ?? 0m;

                    }
                    else
                    {
                        issueSub.BalQty = availableQty - row.Qty ?? 0m;

                    }


                    if (issueSub.BalQty < 0) issueSub.BalQty = 0;
                    if (issueSub.SCNBalQty < 0) issueSub.SCNBalQty = 0;

                    await _unitOfWork.LabourSCNSubs.UpdateAsync(issueSub);
                    await _unitOfWork.SaveAsync();
                }


                var issueIdss = selectedRows
                    .Select(x => x.RefSCNSubId)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct();

                var affectedScnIds = await _unitOfWork.LabourSCNSubs
                                 .GetQueryable()
                                 .Where(s => selectedRows
                                     .Select(r => r.RefSCNSubId!.Value)
                                     .Contains(s.SCNSubId))
                                 .Select(s => s.SCNId)
                                 .Distinct()
                                 .ToListAsync();

                foreach (var scnId in affectedScnIds)
                {

                    var remainingBalQty = await _unitOfWork.LabourSCNSubs
                        .GetQueryable()
                        .Where(s => s.SCNId == scnId)
                        .SumAsync(s => s.BalQty);

                    var issueHead = await _unitOfWork.LabourSCNs.GetAsync(scnId);
                    if (issueHead != null)
                    {
                        issueHead.SCNTally = remainingBalQty == 0;
                        await _unitOfWork.LabourSCNs.UpdateAsync(issueHead);
                    }
                }

                await _unitOfWork.SaveAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to GetPropertyChanges in CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync");
            }
        }

        private string GetPropertyChanges<TSource, TTarget>(TSource entity, TTarget vm)
        {
            try
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
            catch (Exception ex)
            {

                _logs.LogDeveloperError(ex, $"Failed to GetPropertyChanges in Purchase GRN");
                return null;
            }
        }

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            try
            {
                if (changes.Length == 0) return;

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Labour Dc",
                    action: action,
                    additionalInfo: changes.ToString()
                );

            }
            catch (Exception ex)
            {

                await _logs.LogDeveloperError(ex, $"Failed to LogChangesAsync in Purchase GRN");
            }
        }

        private async Task HandleChildUpdatesAsync(LabourDcOutgoing existingDc, List<LabourDcOutgoingSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            try
            {
                var existingSubIds = existingDc.LabourDcOutgoingSubs.Select(s => s.DcSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.DcSubId).ToHashSet();
                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();
                decimal newAcceptedQty, oldAcceptedQty = 0;
                bool rejRetrack = false;

                // DELETE removed children
                foreach (var sub in existingDc.LabourDcOutgoingSubs.Where(s => !incomingSubIds.Contains(s.DcSubId)).ToList())
                {
                    newAcceptedQty = 0;
                    oldAcceptedQty = 0;
                    rejRetrack = false;

                    changes.AppendLine($"Child Deleted - DcSubId: {sub.DcSubId}, Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.LabourDcOutgoingSubs.DeleteAsync(sub.DcSubId);
                    await _unitOfWork.SaveAsync();

                    if (sub.DcSubId > 0)
                    {
                        await RollbackTracksAddIssueQtyAsync(sub.DcSubId, existingDc.SCNRejectReworkReturn);
                    }

                    if (sub.RefPoSubId.GetValueOrDefault() > 0 || sub.RefGRNSubId.GetValueOrDefault() > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            newAcceptedQty = sub.Qty - sub.RejectQty.GetValueOrDefault();
                        }
                        else
                        {
                            newAcceptedQty = sub.Qty;
                        }
                        await AdjustGrnOutBalanceAsync(sub.RefGRNSubId, newAcceptedQty, 0, "Labour Dc Delete");
                    }

                    await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);

                }

                // ADD or UPDATE children

                foreach (var subVM in incomingSubVMs)
                {
                    decimal.TryParse((subVM.Qty).ToString(), out decimal qty);

                    newAcceptedQty = 0;
                    oldAcceptedQty = 0;
                    rejRetrack = false;

                    if (subVM.DcSubId == 0)
                    {
                        var newSub = _mapper.Map<LabourDcOutgoingSub>(subVM);
                        newSub.DcId = existingDc.DcId;

                        await _unitOfWork.LabourDcOutgoingSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        int newDcSubId = newSub.DcSubId;

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                        // ================= TRACK CREATION =================

                        if ((subVM.RefPoSubId > 0 || subVM.RefGRNSubId > 0) && !existingDc.Manual)
                        {

                            await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(
                                newDcSubId,
                                subVM.ItemId,
                                subVM.Qty ?? 0,
                                currentUser,
                                now,
                                incomingSubVMs,
                                existingDc.ReceivedReturnRM,
                                existingDc.SCNRejectReworkReturn);

                        }

                        // ================= MANUAL TRACK =================

                        if (existingDc.Manual && subVM.TransType == "Out")
                        {
                            await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(
                                newDcSubId,
                                subVM.ItemId,
                                subVM.Qty ?? 0,
                                currentUser,
                                now,
                                incomingSubVMs,
                                existingDc.ReceivedReturnRM,
                                existingDc.SCNRejectReworkReturn);
                        }


                        // ================= BALANCE UPDATE =================

                        if (subVM.RefPoSubId.GetValueOrDefault() > 0 || subVM.RefGRNSubId.GetValueOrDefault() > 0)
                        {
                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId ?? 0);

                            if (rejRetrack)
                                newAcceptedQty = subVM.Qty.GetValueOrDefault() - subVM.RejectQty.GetValueOrDefault();
                            else
                                newAcceptedQty = subVM.Qty.GetValueOrDefault();

                            await AdjustGrnOutBalanceAsync(subVM.RefGRNSubId, 0, newAcceptedQty, "Labour Dc Create");

                        }

                        // ================= STOCK ISSUE =================

                        if (subVM.TransType == "Out")
                        {
                            if (subVM.RefPoSubId > 0 &&
                                !string.IsNullOrWhiteSpace(subVM.RcSubIds) &&
                                subVM.CategoryCode == 2)
                            {
                                await IssueStockBySeqLogicAsync(newSub, existingDc, screenCode);
                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(
                                    subVM.ItemId,
                                    existingDc.StoreIssId ?? 0,
                                    subVM.Qty.GetValueOrDefault(),
                                    subVM.UnitPrice ?? 0,
                                    subVM.BatchNo,
                                    screenCode,
                                    newDcSubId,
                                    existingDc.DcNo,
                                    existingDc.DcDate);
                            }
                        }
                    }
                    else
                    {
                        var existingSub = existingDc.LabourDcOutgoingSubs.FirstOrDefault(s => s.DcSubId == subVM.DcSubId);
                        if (existingSub != null)
                        {
                            if (existingSub.DcSubId > 0)
                            {
                                await RollbackTracksAddIssueQtyAsync(existingSub.DcSubId, existingDc.SCNRejectReworkReturn);

                                if (subVM.RefPoSubId.GetValueOrDefault() > 0 || existingSub.RefGRNSubId.GetValueOrDefault() > 0)
                                {

                                    rejRetrack = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId ?? 0);

                                    if (rejRetrack)
                                    {
                                        oldAcceptedQty = existingSub.Qty - existingSub.RejectQty.GetValueOrDefault();
                                        newAcceptedQty = subVM.Qty.GetValueOrDefault() - subVM.RejectQty.GetValueOrDefault();

                                    }
                                    else
                                    {
                                        oldAcceptedQty = existingSub.Qty;
                                        newAcceptedQty = subVM.Qty.GetValueOrDefault();
                                    }

                                    await AdjustGrnOutBalanceAsync(existingSub.RefGRNSubId, oldAcceptedQty, newAcceptedQty, "Labour Dc Create");
                                }
                            }

                            if (subVM.RefPoSubId > 0 || subVM.RefGRNSubId > 0)
                            {
                                if (!existingDc.Manual)
                                {
                                    await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(subVM.DcSubId, subVM.RefPoSubId ?? 0,
                                           subVM.ItemId, subVM.Qty ?? 0, screenCode, currentUser, now, subVM.RefGRNSubId ?? 0,
                                           existingDc.SCNRejectReworkReturn);
                                }
                            }

                            if (existingDc.Manual)
                            {
                                if (subVM.TransType == "Out")
                                    await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(subVM.DcSubId, subVM.ItemId, subVM.Qty ?? 0, currentUser, now, incomingSubVMs, existingDc.ReceivedReturnRM, existingDc.SCNRejectReworkReturn);
                            }

                            if (subVM.TransType == "Out")
                            {

                                if (subVM.RefPoSubId > 0 && (subVM.RcSubIds != "" && subVM.RcSubIds != null) && subVM.CategoryCode == 2)
                                {
                                    existingSub.Qty = subVM.Qty ?? existingSub.Qty;
                                    await IssueStockBySeqLogicAsync(existingSub, existingDc, screenCode);

                                }
                                else
                                {
                                    await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId, existingDc.StoreIssId ?? 0, subVM.Qty.GetValueOrDefault(),
                                   subVM.UnitPrice ?? 0, subVM.BatchNo, screenCode, subVM.DcSubId, existingDc.DcNo, existingDc.DcDate);
                                }


                            }


                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in HandleChildUpdatesAsync()");
            }
        }

        public async Task<string> GetNonReturnableDcNo(string Suffix)
        {
            try
            {
                var NRDcNo = await _unitOfWork.LabourDcOutgoings.GetNextNrDcNoAsync(Suffix);
                return NRDcNo;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetNonReturnableDcNo()");
                return string.Empty;
            }
        }

        public async Task<string> GetPrefixFromDbAsync()
        {
            try
            {
                var prefix = await _unitOfWork.LabourDcOutgoings
                    .GetQueryable()
                    .AsNoTracking()
                    .OrderByDescending(q => q.DcId)
                    .Select(q => q.Prefix)
                    .FirstOrDefaultAsync();

                return prefix ?? string.Empty;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in GetPrefixFromDbAsync()");
                return string.Empty;
            }
        }

        public async Task<List<LabourSCNSubVM>> GetReceivedQtyByScnIdsAsync(List<int> scnIds)
        {
            if (scnIds == null || scnIds.Count == 0)
                return new List<LabourSCNSubVM>();

            return await _unitOfWork.LabourSCNSubs
                .GetQueryable()
                .Where(x =>
                    scnIds.Contains(x.SCNId) &&
                   (x.BalQty > 0 || x.SCNBalQty > 0) &&
                    x.ItemCancel == false)
                .GroupBy(x => x.ItemId)
                .Select(g => new LabourSCNSubVM
                {
                    ItemId = g.Key,
                    BalQty = g.Sum(x => x.BalQty)
                })
                .ToListAsync();
        }

        public async Task<List<LabourSCNSubVM>> GetReceivedQtyreturnByScnIdsAsync(List<int> scnIds)
        {
            if (scnIds == null || scnIds.Count == 0)
                return new List<LabourSCNSubVM>();

            return await _unitOfWork.LabourSCNSubs
                .GetQueryable()
                .Where(x =>
                    scnIds.Contains(x.SCNId) &&
                    x.SCNBalQty > 0 &&
                    x.ItemCancel == false)
                .GroupBy(x => x.ItemId)
                .Select(g => new LabourSCNSubVM
                {
                    ItemId = g.Key,
                    BalQty = g.Sum(x => x.SCNBalQty)



                })
                .ToListAsync();
        }

        public async Task<List<LabourSCNSubVM>> GetReceivedQtyByScnIdsByDetailsAsync(List<int> scnIds)
        {
            if (scnIds == null || scnIds.Count == 0)
                return new List<LabourSCNSubVM>();

            return await _unitOfWork.LabourSCNSubs
                .GetQueryable()
                .Where(x =>
                    scnIds.Contains(x.SCNId) &&
                    x.ItemCancel == false)
                .Select(x => new LabourSCNSubVM
                {
                    SCNId = x.SCNId,
                    SCNSubId = x.SCNSubId,
                    ItemId = x.ItemId,
                    ItemName = x.Item.ItemName,
                    ItemCode = x.Item.ItemCode,
                    BalQty = x.BalQty,
                    AcceptQty = x.AcceptQty,
                    RefGRNNo = x.LabourGRNSub.LabourGRN.GRNNo

                })
                .ToListAsync();
        }


        public async Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId)
        {
            return await _unitOfWork.AssmblyDefs
             .GetQueryable().Where(x => x.AssmblyID == assemblyId)
             .Select(x => new AssemblyDefVM
             {
                 ItemId = x.ItemId,
                 UtilQty = x.UtilQty
             })
             .ToListAsync();
        }

        public async Task<List<ItemVM>> GetRawMaterialItemsAsync(int compItemId)
        {
            return await _unitOfWork.CompMasters
             .GetQueryable().Where(x => x.CompItemId == compItemId)
             .OrderByDescending(c => c.IsDefaultRM)
             .Select(x => new ItemVM
             {
                 ItemId = x.RMId ?? 0,
                 Weight = x.Weight
             })
             .ToListAsync();
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

        private async Task CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(int dcSubId, int RefPoSubId, int itemId, decimal qty,
            int screenCode, string currentUser, DateTime now, int RefGRNSubId, bool SCNRejRew)
        {
            if (RefPoSubId == 0 && RefGRNSubId == 0)
                return;

            List<int> grnSubIds;

            if (RefPoSubId > 0)
            {
                grnSubIds = await _unitOfWork.LabourGRNSubs.GetQueryable()
                    .Where(x => x.TransType == "In" &&
                        _unitOfWork.LabourGRNSubs.GetQueryable()
                        .Where(y => y.RefPoSubId == RefPoSubId)
                        .Select(y => y.GRNId)
                        .Contains(x.GRNId))
                    .Select(x => x.GRNSubId)
                    .ToListAsync();
            }
            else
            {
                // Step 1: Get GRNIds from RefGRNSubId
                var grnIds = await _unitOfWork.LabourGRNSubs.GetQueryable()
                                .Where(x => x.GRNSubId == RefGRNSubId)
                                .Select(x => x.GRNId)
                                .ToListAsync();

                // Step 2: Get all GRNSubIds for those GRNIds
                grnSubIds = await _unitOfWork.LabourGRNSubs.GetQueryable()
                                    .Where(x => grnIds.Contains(x.GRNId))
                                    .Select(x => x.GRNSubId)
                                    .ToListAsync();
            }

            var categoryCode = await _commonService.GetItemCategoryCodeByItemIdAsync(itemId);

            bool isSameItem = await CheckIsSameAsOutItemByPoSubId(RefPoSubId);

            if (isSameItem)
            {
                await ProcessItemAsync(dcSubId, RefPoSubId, itemId, qty, itemId, qty,
                    grnSubIds, SCNRejRew, currentUser, now);
                return;
            }

            if (categoryCode == 3 || categoryCode == 7)
            {
                var bomData = await _unitOfWork.AssmblyDefs.GetQueryable()
                    .Where(x => x.AssmblyID == itemId && x.UtilQty > 0)
                    .Select(x => new { x.ItemId, x.UtilQty })
                    .ToListAsync();

                foreach (var bom in bomData)
                {
                    decimal componentQty = qty * bom.UtilQty;

                    await ProcessItemAsync(dcSubId, RefPoSubId, itemId, qty,
                        bom.ItemId, componentQty,
                        grnSubIds, SCNRejRew, currentUser, now);
                }
            }
            else if (categoryCode == 2)
            {
                var compData = await _unitOfWork.CompMasters.GetQueryable()
                    .Where(x => x.CompItemId == itemId && x.Weight > 0)
                    .Select(x => new { x.RMId, x.Weight })
                    .FirstOrDefaultAsync();

                if (compData == null)
                    return;

                decimal componentQty = qty * compData.Weight;

                await ProcessItemAsync(dcSubId, RefPoSubId, itemId, qty,
                    compData.RMId.Value, componentQty,
                    grnSubIds, SCNRejRew, currentUser, now);
            }
        }

        private decimal GetAvailableQty(LabourSCNSub sub, bool scnRejRew)
        {
            return scnRejRew ? sub.SCNBalQty : sub.BalQty;
        }

        private void ReduceBalance(LabourSCNSub sub, decimal qty, bool scnRejRew)
        {
            if (scnRejRew)
                sub.SCNBalQty -= qty;
            else
                sub.BalQty -= qty;

            if (sub.BalQty < 0) sub.BalQty = 0;
            if (sub.SCNBalQty < 0) sub.SCNBalQty = 0;
        }

        private async Task<List<LabourSCNSub>> GetSCNSubsAsync(List<int> grnSubIds, int itemId, bool scnRejRew)
        {
            return await _unitOfWork.LabourSCNSubs
                .GetQueryable()
                .Where(x =>
                    x.SCNId > 0 &&
                    grnSubIds.Contains(x.RefGRNSubId ?? 0) &&
                    x.ItemId == itemId &&
                    (scnRejRew ? x.SCNBalQty > 0 : x.BalQty > 0))
                .OrderBy(x => x.SCNId)
                .ToListAsync();
        }


        private async Task CreateTrackAsync(
                int dcSubId,
                int refPoSubId,
                int itemOut,
                decimal qtyOut,
                int itemIn,
                decimal qtyIn,
                LabourSCNSub scn,
                string user,
                DateTime now)
        {
            var track = new LabourDcReturnCompTrack
            {
                RefDcSubId = dcSubId,
                RefSCNSubId = scn.SCNSubId,
                RefGRNSubId = scn.RefGRNSubId.Value,
                RefPoSubId = refPoSubId > 0 ? refPoSubId : null,
                ItemIdOut = itemOut,
                QtyOut = qtyOut,
                ItemIdIn = itemIn,
                QtyIn = qtyIn,
                CreatedBy = user,
                CreatedDate = now
            };

            await _unitOfWork.LabourDcReturnCompTracks.CreateAsync(track);
        }

        private async Task UpdateSCNTallyAsync(List<int> scnIds, bool scnRejRew)
        {
            foreach (var scnId in scnIds.Distinct())
            {
                decimal totalBal = scnRejRew
                    ? await _unitOfWork.LabourSCNSubs.GetQueryable()
                        .Where(x => x.SCNId == scnId)
                        .SumAsync(x => x.SCNBalQty)
                    : await _unitOfWork.LabourSCNSubs.GetQueryable()
                        .Where(x => x.SCNId == scnId)
                        .SumAsync(x => x.BalQty);

                var head = await _unitOfWork.LabourSCNs.GetAsync(scnId);

                if (head != null)
                {
                    head.SCNTally = totalBal == 0;
                    await _unitOfWork.LabourSCNs.UpdateAsync(head);
                }
            }

            await _unitOfWork.SaveAsync();
        }


        private async Task ProcessItemAsync(
                        int dcSubId,
                        int refPoSubId,
                        int itemOut,
                        decimal qtyOut,
                        int itemIn,
                        decimal neededQty,
                        List<int> grnSubIds,
                        bool scnRejRew,
                        string user,
                        DateTime now)
        {
            var scnSubs = await GetSCNSubsAsync(grnSubIds, itemIn, scnRejRew);

            foreach (var scn in scnSubs)
            {
                if (neededQty <= 0)
                    break;

                decimal available = GetAvailableQty(scn, scnRejRew);

                decimal takeQty = Math.Min(available, neededQty);

                if (takeQty <= 0)
                    continue;

                await CreateTrackAsync(dcSubId, refPoSubId, itemOut, qtyOut, itemIn, takeQty, scn, user, now);

                ReduceBalance(scn, takeQty, scnRejRew);

                await _unitOfWork.LabourSCNSubs.UpdateAsync(scn);

                neededQty -= takeQty;
            }

            await _unitOfWork.SaveAsync();

            await UpdateSCNTallyAsync(scnSubs.Select(x => x.SCNId).ToList(), scnRejRew);
        }


        private async Task AdjustGrnOutBalanceAsync(int? refGRnSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refGRnSubId.HasValue || refGRnSubId == 0)
                    return;

                var grnOutSubs = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNSubId == refGRnSubId && x.TransType == "Out")
                    .OrderBy(x => x.GRNId)
                    .ThenBy(x => x.SlNo)
                    .ToListAsync();

                if (!grnOutSubs.Any())
                    return;

                // ======================
                // RESTORE OLD QTY
                // ======================

                if (oldQty > 0)
                {
                    foreach (var grn in grnOutSubs)
                    {
                        grn.BalQty += oldQty;
                        break; // restore once
                    }
                }

                // ======================
                // VALIDATE TOTAL BAL QTY
                // ======================

                var totalBalQty = grnOutSubs.Sum(x => x.BalQty);

                if (newQty > totalBalQty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed GRN BalQty.");

                // ======================
                // REDUCE NEW QTY (FIFO)
                // ======================

                decimal remaining = newQty;

                foreach (var grn in grnOutSubs)
                {
                    if (remaining <= 0)
                        break;

                    decimal takeQty = Math.Min(grn.BalQty, remaining);

                    grn.BalQty -= takeQty;

                    remaining -= takeQty;
                }

                await _unitOfWork.LabourGRNSubs.UpdateRangeAsync(grnOutSubs);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustGRNBalance] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustGRNBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust GRN balance. Please contact support.");
            }
        }

        public async Task<List<int>> GetReceivedSCNIdsByRefPoSubId(int refPoSubId)
        {
            try
            {
                var scnSubIds = await
                (
                    from scn in _unitOfWork.LabourSCNSubs.GetQueryable()
                    join grnIn in _unitOfWork.LabourGRNSubs.GetQueryable()
                        on scn.RefGRNSubId equals grnIn.GRNSubId
                    join grnOut in _unitOfWork.LabourGRNSubs.GetQueryable()
                        on grnIn.GRNId equals grnOut.GRNId
                    where grnOut.RefPoSubId == refPoSubId
                       && grnOut.TransType == "Out"
                       && !scn.ItemCancel
                       && !grnIn.ItemCancel
                       && !grnOut.ItemCancel
                    select scn.SCNId
                )
                .Distinct()
                .ToListAsync();

                return scnSubIds;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error loading SCN received data for RefPoSubId: {refPoSubId}"
                );
                return new List<int>();
            }
        }
        public async Task<List<int>> GetReceivedSCNSubIdsByRefRefGrnSubId(int refGrnSubId)
        {
            try
            {
                var scnSubIds = await
                (
                    from scn in _unitOfWork.LabourSCNSubs.GetQueryable()
                    join grnIn in _unitOfWork.LabourGRNSubs.GetQueryable()
                        on scn.RefGRNSubId equals grnIn.GRNSubId
                    join grnOut in _unitOfWork.LabourGRNSubs.GetQueryable()
                        on grnIn.GRNId equals grnOut.GRNId
                    where grnOut.GRNSubId == refGrnSubId
                       && grnOut.TransType == "Out"
                       && !scn.ItemCancel
                       && !grnIn.ItemCancel
                       && !grnOut.ItemCancel
                    select scn.SCNId
                )
                .Distinct()
                .ToListAsync();

                return scnSubIds;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error loading SCN received data for refGrnSubId: {refGrnSubId}"
                );
                return new List<int>();
            }
        }


        public async Task<List<LabourSCNSubVM>> GetReceivedQtyByScnSubIdsAsync(List<int> scnIds)
        {
            if (scnIds == null || scnIds.Count == 0)
                return new List<LabourSCNSubVM>();

            return await _unitOfWork.LabourSCNSubs
                .GetQueryable()
                .Where(x =>
                    scnIds.Contains(x.SCNId)
                    && x.BalQty > 0
                    && !x.ItemCancel)
                .GroupBy(x => x.SCNSubId)
                .Select(g => new LabourSCNSubVM
                {
                    SCNSubId = g.Key,
                    ItemId = g.Select(x => x.ItemId).FirstOrDefault(),
                    BalQty = g.Sum(x => x.BalQty)
                })
                .ToListAsync();
        }



        public static class DynamicWhereBuilder
        {
            public static IQueryable<LabourDcOutgoing> ApplyFilter(IQueryable<LabourDcOutgoing> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString()!.Trim();

                switch (field)
                {

                    case "DcNo":
                        {
                            string part1 = val;
                            string part2 = string.Empty;

                            int slashIndex = val.IndexOf('/');
                            if (slashIndex > -1)
                            {
                                part1 = val[..slashIndex].Trim();
                                part2 = val[(slashIndex + 1)..].Trim();
                            }

                            return query.Where(x => (string.IsNullOrEmpty(part1) || x.DcNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || (x.Suffix != null && x.Suffix.Contains(part2)))
                            );
                        }

                    case "Customer":
                        return query.Where(x => x.Customer.CustName.Contains(val.ToString()));


                    case "PoNo":
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
                                x.LabourDcOutgoingSubs.Any(s =>
                                    s.MfgPoSub != null &&
                                    s.MfgPoSub.MfgPo != null &&
                                    (string.IsNullOrEmpty(part1) ||
                                        s.MfgPoSub.MfgPo.PONo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) ||
                                        (x.Suffix != null && x.Suffix.Contains(part2)))
                                )
                            );
                        }

                    case "DCNo":
                        {
                            string dcNo = val?.Trim() ?? "";

                            return query.Where(x =>
                                x.LabourDcOutgoingSubs.Any(s =>
                                    s.LabourDcReturnCompTracks != null &&
                                    s.LabourGRNSub != null &&
                                    s.LabourGRNSub.LabourGRN != null &&
                                    (string.IsNullOrEmpty(dcNo) ||
                                     s.LabourGRNSub.LabourGRN.RefDcNo.StartsWith(dcNo))
                                )
                            );
                        }

                    case "SCNNo":
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
                                x.LabourDcOutgoingSubs.Any(s =>
                                    s.LabourSCNSub != null &&
                                    s.LabourSCNSub.LabourSCN != null &&
                                    (string.IsNullOrEmpty(part1) ||
                                        s.LabourSCNSub.LabourSCN.SCNNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) ||
                                        (x.Suffix != null && x.Suffix.Contains(part2)))
                                )
                            );
                        }


                    case "ItemCode":
                        return query.Where(x => x.LabourDcOutgoingSubs.Any(s => s.Item.ItemCode.Contains(val.ToString())));

                    case "ItemName":
                        return query.Where(x => x.LabourDcOutgoingSubs.Any(s => s.Item.ItemName.Contains(val.ToString())));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val.ToString()));

                    case "FromDate":
                        return query.Where(x => x.CreatedDate >= DateTime.Parse(val.ToString()));

                    case "ToDate":
                        return query.Where(x => x.CreatedDate <= DateTime.Parse(val.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val.ToString());

                }

                return query;
            }
        }
        private static IQueryable<LabourDcOutgoing> ApplyStatusFilter(
               IQueryable<LabourDcOutgoing> query, string status)
        {
            try
            {
                return status switch
                {
                    "Completed" =>
                        query.Where(x => x.DcTally && !x.ShortClose),

                    "Pending" =>
                        query.Where(x =>
                            !x.DcTally &&
                            !x.DcCancel &&
                            !x.ShortClose),

                    "Cancelled" =>
                        query.Where(x => x.DcCancel),

                    "Short Closed" =>
                        query.Where(x => x.ShortClose),

                    _ => query
                };
            }
            catch
            {
                return query;
            }
        }

        public async Task<Dictionary<int, int?>> GetPoSubIdByScnSubIdsAsync(List<int> scnSubIds)
        {
            var query =
                from scn in _unitOfWork.LabourSCNSubs.GetQueryable()

                join grnIn in _unitOfWork.LabourGRNSubs.GetQueryable()
                    on scn.RefGRNSubId equals grnIn.GRNSubId

                join grnOut in _unitOfWork.LabourGRNSubs.GetQueryable()
                    on new { grnIn.GRNId, grnIn.ItemId }
                    equals new { grnOut.GRNId, grnOut.ItemId }

                where scnSubIds.Contains(scn.SCNSubId)
                      && grnOut.TransType == "Out"

                select new
                {
                    scn.SCNSubId,
                    grnOut.RefPoSubId
                };

            // Print Generated SQL Query
            Console.WriteLine(query.ToQueryString());

            return await query.ToDictionaryAsync(x => x.SCNSubId, x => x.RefPoSubId);
        }

        public async Task<bool> CheckIsSameAsOutItemByPoSubId(int poSubId)
        {
            return await _unitOfWork.LabourGRNSubs
                .GetQueryable()
                .AsNoTracking()
                .Where(x => x.RefPoSubId == poSubId)
                .Select(x => x.LabourGRN.IsOutItemSameAsInItem)
                .FirstOrDefaultAsync();
        }
        public async Task<List<LabourSCNSubVM>> GetLabourSCNQtyByReceivedIdsAsync(List<int> ReceivedIds)
        {
            if (ReceivedIds == null || !ReceivedIds.Any())
                return new List<LabourSCNSubVM>();

            try
            {


                return await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Include(x => x.Item)
                    .Where(x =>
                        ReceivedIds.Contains(x.SCNId) &&
                        (x.BalQty > 0 || x.SCNBalQty > 0))   // OR condition
                    .Select(x => new LabourSCNSubVM
                    {
                        SCNSubId = x.SCNSubId,
                        SCNId = x.SCNId,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.ItemName,
                        MeasureUnit = x.Item.MeasureUnit,
                        HSNCode = x.Item.HSNCode,

                        AcceptQty = x.BalQty,   // you can change if needed
                        BalQty = x.BalQty,
                        SCNBalQty = x.SCNBalQty,

                        UnitPrice = x.UnitPrice,
                        RefGRNSubId = x.RefGRNSubId,
                        Remark = x.Remark,
                        IsEditable = false
                    })
                    .ToListAsync();

                //return await _unitOfWork.LabourSCNSubs
                //    .GetQueryable()
                //    .Include(x => x.Item)
                //    .Where(x =>
                //        ReceivedIds.Contains(x.SCNId) &&
                //               ( x.BalQty > 0 || x.SCNBalQty>0))
                //    .Select(x => new LabourSCNSubVM
                //    {
                //        SCNSubId = x.SCNSubId,        // FIX naming
                //        SCNId = x.SCNId,
                //        ItemId = x.ItemId,
                //        ItemCode = x.Item.ItemCode,  // FIX: was ItemName
                //        ItemName = x.Item.ItemName,
                //        MeasureUnit = x.Item.MeasureUnit, // FIX: string
                //        HSNCode = x.Item.HSNCode,
                //        AcceptQty = x.BalQty,
                //        BalQty = x.BalQty,
                //        SCNBalQty=x.SCNBalQty,
                //        UnitPrice = x.UnitPrice,
                //        RefGRNSubId = x.RefGRNSubId,
                //        Remark = x.Remark,
                //        IsEditable = false
                //    })
                //    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching LabourSCN data for SCNIDS: {string.Join(",", ReceivedIds)}");
                return new List<LabourSCNSubVM>();
            }
        }
        public async Task<decimal?> GetSCNBalQtyBySCNSubId(
    int scnSubId,
    bool scnRejRewReturn)
        {
            try
            {
                decimal scnQty;

                if (scnRejRewReturn)
                {
                    scnQty = await _unitOfWork.LabourSCNSubs
                        .GetQueryable()
                        .Where(x => x.SCNSubId == scnSubId && !x.ItemCancel)
                        .Select(x => x.SCNBalQty)
                        .FirstOrDefaultAsync();
                }
                else
                {
                    scnQty = await _unitOfWork.LabourSCNSubs
                        .GetQueryable()
                        .Where(x => x.SCNSubId == scnSubId && !x.ItemCancel)
                        .Select(x => x.BalQty)
                        .FirstOrDefaultAsync();
                }

                return Math.Max(0, scnQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading SCN balance for SCNSubId: {scnSubId}");

                return 0;
            }
        }


        public async Task<decimal?> GetLaboourGRnBalQtyByGRNSubId(int grnSubId)
        {
            try
            {
                return await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNSubId == grnSubId)
                    .Select(x => x.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error loading GRN balance for GRNSubId: {grnSubId}");

                return 0;
            }
        }

        public async Task<decimal> GetLabourGrnItemBalQtyByGrnSubId(int grnSubId)
        {
            try
            {
                return await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNSubId == grnSubId && x.TransType == "Out")
                    .SumAsync(x => x.BalQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading GRN balance for GRNSubId: {grnSubId}");
                return 0;
            }
        }

        //public async Task UpsertLabourGRNQtyAsync(int? RefGrnSubId, int itemId, decimal oldQty, decimal newQty, string context)
        //{
        //    try
        //    {
        //        if (RefGrnSubId == 0) return;

        //        var GrnSub = await _unitOfWork.LabourGRNSubs.GetQueryable()
        //            .Where(j => j.GRNSubId == RefGrnSubId && j.ItemId == itemId).FirstOrDefaultAsync();

        //        if (GrnSub == null) return;

        //        if (oldQty > 0)
        //            GrnSub.BalQty += oldQty;

        //        if (newQty > GrnSub.Qty)
        //            throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

        //        if (newQty > 0)
        //            GrnSub.BalQty -= newQty;

        //        await _unitOfWork.LabourGRNSubs.UpdateAsync(GrnSub);
        //        await _unitOfWork.SaveAsync();


        //        var totalBalQty = await _unitOfWork.LabourGRNSubs
        //            .GetQueryable()
        //            .Where(e => e.GRNId == GrnSub.GRNId)
        //            .SumAsync(e => e.BalQty);

        //        var DC = await _unitOfWork.LabourGRNs.GetAsync(GrnSub.GRNId);
        //        if (DC != null)
        //        {
        //            DC.DcTally = (totalBalQty == 0);
        //            await _unitOfWork.LabourGRNs.UpdateAsync(DC);
        //            await _unitOfWork.SaveAsync();
        //        }

        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        await _logs.LogDeveloperError(
        //            ex, $"[UpsertLabourGRNQtyAsync] Validation failed in {context}");

        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(
        //            ex, $"[UpsertLabourGRNQtyAsync] Unexpected error in {context}");

        //    }
        //}

        public async Task<LabourSCNSubVM?> GetScnDetailsBySCNSubIdAsync(int scnSubId)
        {
            try
            {
                // 1️⃣ Get SCN Qty
                var scn = await _unitOfWork.LabourSCNSubs
                    .GetQueryable()
                    .Where(x => x.SCNSubId == scnSubId)
                    .Select(x => new
                    {
                        x.AcceptQty
                    })
                    .FirstOrDefaultAsync();

                if (scn == null)
                    return null;

                // 2️⃣ Get already issued / transacted qty
                var issuedQty = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Where(x =>
                        x.RefSCNSubId == scnSubId &&
                        x.TransType == "In")
                    .SumAsync(x => x.Qty);

                // 3️⃣ Calculate balance
                var balQty = scn.AcceptQty - issuedQty;

                return new LabourSCNSubVM
                {
                    AcceptQty = scn.AcceptQty,
                    BalQty = balQty < 0 ? 0 : balQty
                };
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching SCN sub item detail for SCNSubId: {scnSubId}"
                );
                return null;
            }
        }
        public async Task UpsertlabourDcOutgoingShortCloseAsync(LabourDcOutgoingVM LabourDcOutgoingVMs)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingGRN = await _unitOfWork.LabourDcOutgoings.GetAsync(LabourDcOutgoingVMs.DcId);
                if (existingGRN == null)
                    throw new InvalidOperationException("Sales GRN not found.");

                existingGRN.ShortClose = LabourDcOutgoingVMs.ShortClose;

                await _unitOfWork.LabourDcOutgoings.UpdateAsync(existingGRN);
                await _unitOfWork.SaveAsync();

                await UpdateLabourDcOutgoingTallyStatusAsync(LabourDcOutgoingVMs.DcId);

                await transaction.CommitAsync();
            }
            catch (InvalidOperationException ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertlabourDcOutgoingShortCloseAsync] Validation issue");

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, "[UpsertlabourDcOutgoingShortCloseAsync] Unexpected error");

            }
        }
        public async Task UpdateLabourDcOutgoingTallyStatusAsync(int DcId)
        {
            try
            {
                decimal totalBalQty = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Where(x => x.DcId == DcId && !x.ItemCancel)
                    .SumAsync(x => (decimal?)x.BalQty) ?? 0;

                var LabDcOut = await _unitOfWork.LabourDcOutgoings.GetAsync(DcId);
                if (LabDcOut == null)
                    return;

                if (LabDcOut.ShortClose || LabDcOut.DcCancel)
                    return;

                LabDcOut.DcCancel = (totalBalQty == 0);

                await _unitOfWork.LabourDcOutgoings.UpdateAsync(LabDcOut);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[UpdateLabourDcOutgoingTallyStatusAsync] Error updating Dcid {DcId}");

            }
        }

        public async Task<(bool CanDelete, string Message)> CanCancelAndRevokelabourDcOutgoingAsync(int Dcid, int DcSubId)
        {
            try
            {
                var DcSubIds = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Where(x => x.DcId == Dcid &&
                               (DcSubId == 0 || x.DcSubId == DcSubId))
                    .Select(x => x.DcSubId)
                    .ToListAsync();

                if (!DcSubIds.Any())
                    return (true, "Labour Dc Outgoing can be safely deleted.");


                bool hasTransaction = await _unitOfWork.LabInvSubs
                    .GetQueryable()
                    .AnyAsync(x =>
                        x.RefDcSubId.HasValue &&
                        DcSubIds.Contains(x.RefDcSubId ?? 0));

                if (hasTransaction)
                    return (false, "Cannot cancel Labour Dc Outgoing because transactions are already made.");


                return (true, "Labour Dc Outgoing can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex, $"Error in CanCancelAndRevokelabourDcOutgoingAsync for DCID: {Dcid}");

                return (false, "Error while validating Labour DCID.");
            }
        }

        public async Task<List<LabourDcOutgoingSub>> GetDcOutgoingSubDetailsBydcIdAsync(int DcId)
        {
            try
            {
                var subs = await _unitOfWork.LabourDcOutgoingSubs
                                 .GetQueryable()
                                 .Where(s => s.DcId == DcId)
                                 .OrderBy(s => s.SlNo)
                                 .AsNoTracking()
                                 .AsSplitQuery()
                                 .ToListAsync();

                return subs;

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching LabourDcOutgoingSub items for dcid: {DcId}");
                throw new InvalidOperationException("Failed to retrieve LabourDcOutgoingSub sub-items. Please try again.");
            }
        }

        public async Task<bool> HasRejectReworkPoAsync(List<int> PosubIds)
        {
            try
            {
                return await (from poSub in _unitOfWork.MfgPoSubs.GetQueryable()
                              join po in _unitOfWork.MfgPos.GetQueryable()
                                  on poSub.PoId equals po.PoId
                              where PosubIds.Contains(poSub.PoSubId)
                                    && po.isRejTrackReq == true
                              select poSub.PoSubId
                        ).AnyAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasRejectReworkPoAsync");
                return false;
            }

        }

        public async Task<bool> HasRejectReworkPoByPoSubidAsync(int PosubId)
        {
            try
            {
                return await (from poSub in _unitOfWork.MfgPoSubs.GetQueryable()
                              join po in _unitOfWork.MfgPos.GetQueryable()
                                  on poSub.PoId equals po.PoId
                              where poSub.PoSubId == PosubId
                                    && po.isRejTrackReq == true
                              select poSub.PoSubId
                        ).AnyAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in HasRejectReworkPoByPoSubidAsync");
                return false;
            }
        }
        public async Task<bool> CanlabourDcOutgoingTrasByDcsubIdAsync(List<int> dcSubIds)
        {
            try
            {
                if (dcSubIds == null || dcSubIds.Count == 0)
                    return false;

                return await (
                        from sub in _unitOfWork.LabInvSubs.GetQueryable()
                        join inv in _unitOfWork.LabInvs.GetQueryable()
                            on sub.LabInvId equals inv.LabInvId
                        join dcSub in _unitOfWork.LabourDcOutgoingSubs.GetQueryable()
                            on sub.RefDcSubId equals dcSub.DcSubId
                        join dc in _unitOfWork.LabourDcOutgoings.GetQueryable()
                            on dcSub.DcId equals dc.DcId
                        where
                            sub.RefDcSubId.HasValue &&
                            dcSubIds.Contains(sub.RefDcSubId.Value) &&
                            !sub.ItemCancel &&
                            !inv.InvCancel &&
                            dc.ReduceInvAsPerRej == true   // 🔥 IMPORTANT CONDITION
                        select sub.LabInvSubId
                    ).AnyAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanlabourDcOutgoingTrasByDcsubIdAsync");
                return false;
            }


        }
        public async Task<int> GetCountAsync()
        {
            return await _unitOfWork.LabourDcOutgoings
                .GetQueryable()
                .CountAsync();
        }


        private async Task IssueStockBySeqLogicAsync(LabourDcOutgoingSub sub, LabourDcOutgoing parent, int screenCode)
        {
            decimal remainingQty = sub.Qty;
            if (remainingQty <= 0)
                return;


            var existingIssues = await _unitOfWork.StockIssues
                .GetQueryable()
                .Where(x =>
                    x.SubItemRefID == sub.DcSubId &&
                    x.ScreenCode == screenCode)
                .ToListAsync();

            foreach (var issue in existingIssues)
                await _stockManagerService.DeleteStockIssueAsync(issue.IssueId);

            // =====================================================
            // RE-ISSUE
            // =====================================================
            //var issueSubIds = await GetIssueSourceRCSubIdsAsync(
            //    sub.ComponentRouteCardSub.RCId,
            //    sub.ComponentRouteCardSub.SeqNo.Value,
            //    sub.RcSubId.Value);

            var ids = sub.RcSubIds
                    .Replace("'", "")
                    .Split(',', StringSplitOptions.RemoveEmptyEntries);

            foreach (var rcSubId in ids)
            {
                if (remainingQty <= 0)
                    break;

                int.TryParse(rcSubId.ToString(), out int rsubid);

                var available = await GetStockForItemRcSubIdAsync(
                    sub.ItemId,
                    parent.StoreIssId ?? 0,
                    rsubid);

                if (available <= 0)
                    continue;

                var issueQty = Math.Min(available, remainingQty);

                await _stockManagerService.IssueOrUpdateStockAsync(
                    sub.ItemId,
                    parent.StoreIssId.Value,
                    issueQty,
                    sub.UnitPrice,
                    null,
                    screenCode,
                    sub.DcSubId,
                    parent.DcNo,
                    parent.DcDate,
                    rsubid,
                    allowMultipleIssue: true);

                remainingQty -= issueQty;
            }

            if (remainingQty > 0)
                throw new InvalidOperationException(
                    $"Insufficient RC stock after adjustment. Remaining Qty: {remainingQty}");
        }


        //------------**********Dc Details for EWAY *****-----------------------\\

        public async Task<List<EWayDocument>> GetLabourDcByCustidAsync(int custId)
        {
            try
            {
                var data = await _unitOfWork.LabourDcOutgoings.GetQueryable()
                    .AsNoTracking()
                    .Where(dc =>
                        dc.CustId == custId &&
                        !string.IsNullOrEmpty(dc.DcNo) &&
                        (dc.EwayNo == null || dc.EwayNo == "0" || dc.EwayNo == "")
                    )
                    .OrderBy(dc => dc.DcId)
                    .Select(dc => new EWayDocument
                    {
                        Id = dc.DcId,
                        DocNo = dc.DcNo + dc.Suffix,
                        Suffix = dc.Suffix,
                        DocDate = dc.DcDate,
                        CustName = dc.Customer.CustName
                    })
                    .ToListAsync();


                return data;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetLabourDcByCustidAsync({custId})");
                return new List<EWayDocument>();
            }
        }

        public async Task<LabourDcOutgoingVM> UpsertPoWiseDcAsync(LabourDcOutgoingVM labourDcVM, int screenCode)
        {

            if (labourDcVM == null)
                throw new ArgumentNullException(nameof(labourDcVM));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                LabourDcOutgoing entity;

                if (labourDcVM.DcId == 0)
                {
                    entity = _mapper.Map<LabourDcOutgoing>(labourDcVM);


                    //--------AutoDcrunning
                    if (labourDcVM.IsManualDcNo ?? true)
                    {
                        var runningRow = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.DcType == "LABOURDCOUTGOING" &&
                                x.Suffix == entity.Suffix);

                        if (runningRow == null)
                        {
                            var dcrunn = new DcRunningNumber
                            {
                                LastNumber = Convert.ToInt64(entity.DcNo),
                                DcType = "LABOURDCOUTGOING",
                                Suffix = entity.Suffix
                            };

                            await _unitOfWork.DcRunningNumbers.CreateAsync(dcrunn);
                        }
                        else
                        {
                            runningRow.LastNumber = Convert.ToInt64(entity.DcNo);
                            await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                        }

                        await _unitOfWork.SaveAsync();
                    }
                    else
                    {
                        entity.DcNo = await _commonService.GenerateAutoRunningNoAsync("LABOURDCOUTGOING", entity.Suffix);
                    }
                    //-----------------------------------------------------------------------------------------


                    //entity.DcNo = NextNumber;

                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;
                    entity.NoOfItems = labourDcVM.LabourDcOutgoingSubVMs.Count();
                    entity.LabourDcOutgoingSubs = labourDcVM.LabourDcOutgoingSubVMs.Select(s => _mapper.Map<LabourDcOutgoingSub>(s)).ToList();

                    await _unitOfWork.LabourDcOutgoings.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();
                    int refpoid = 0, grnsubidIn = 0, i = 0;
                    foreach (var sub in entity.LabourDcOutgoingSubs)
                    {
                        var row1 = labourDcVM.LabourDcOutgoingSubVMs?.ElementAtOrDefault(i);
                        int.TryParse(sub.RefPoSubId.ToString(), out refpoid);
                        int.TryParse(row1?.GRNSubIdIn.ToString(), out grnsubidIn);

                        if (refpoid > 0 || grnsubidIn > 0)
                        {
                            if (!labourDcVM.Manual)
                            {
                                await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(sub.DcSubId, refpoid,
                                       sub.ItemId, sub.Qty, screenCode, currentUser, now, grnsubidIn, labourDcVM.SCNRejectReworkReturn);
                            }

                        }

                        if (labourDcVM.Manual)
                        {
                            if (sub.TransType == "Out")
                                await CreateTrackAndReduceIssueAndUpadteIssueTallyManualPoWiseAsync(sub.DcSubId, sub.ItemId, sub.Qty, currentUser, now, labourDcVM.LabourDcOutgoingSubVMs, labourDcVM.ReceivedReturnRM, labourDcVM.SCNRejectReworkReturn);

                        }
                        if (labourDcVM.WithoutPoDc)
                            await UpsertLabourGRNQtyAsync(sub.RefGRNSubId ?? 0, sub.ItemId, 0, sub.Qty, "Labour GRN create   ");

                        if (sub.RefPoSubId > 0)
                        {
                            await AdjustPoSubBalanceAsync(sub.RefPoSubId, 0, sub.Qty, "Labour Dc Create");
                        }


                        if (sub.TransType == "Out")
                        {
                            if ((refpoid > 0 && sub.RcSubIds != "") && row1.CategoryCode == 2)
                            {
                                await IssueStockBySeqLogicAsync(sub, entity, screenCode);

                            }
                            else
                            {

                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, entity.StoreIssId ?? 0, sub.Qty,
                                                    sub.UnitPrice, sub.BatchNo, screenCode, sub.DcSubId, entity.DcNo, entity.DcDate);

                            }
                        }


                        i++;
                    }

                    changes.AppendLine("Labour Dc Created.");
                }
                else
                {

                    entity = await _unitOfWork.LabourDcOutgoings.GetQueryable()
                        .Include(q => q.LabourDcOutgoingSubs)
                        .FirstOrDefaultAsync(q => q.DcId == labourDcVM.DcId)
                        ?? throw new InvalidOperationException("labour GRN found.");

                    var parentChanges = GetPropertyChanges(entity, labourDcVM);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(labourDcVM, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;
                    entity.NoOfItems = entity.LabourDcOutgoingSubs.Count();

                    if (entity.DcCancel)
                    {
                        foreach (var sub in entity.LabourDcOutgoingSubs)
                        {
                            await ValidateLabourSCNBalanceBeforeRevertAsync(sub, labourDcVM);
                        }
                    }

                    await HandleChildUpdatesPoWiseAsync(entity, labourDcVM.LabourDcOutgoingSubVMs, changes, screenCode);

                    changes.AppendLine("Labour Dc Updated.");
                }


                await _unitOfWork.SaveAsync();
                await UpdateDcTallyStatusAsync(labourDcVM.DcId);
                await transaction.CommitAsync();

                await LogChangesAsync(changes, labourDcVM.DcId == 0 ? "Labour Dc Created" : "Labour Dc  Updated");

                var savedEntity = await _unitOfWork.LabourDcOutgoings.GetQueryable()
                    .AsNoTracking()
                    .AsSplitQuery()
                    .Include(q => q.LabourDcOutgoingSubs).ThenInclude(s => s.Item).ThenInclude(c => c.Category)
                    .Include(q => q.Customer)
                    .Include(q => q.Store)
                    .FirstOrDefaultAsync(q => q.DcId == entity.DcId);

                return _mapper.Map<LabourDcOutgoingVM>(savedEntity!);

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Labour Dc: {labourDcVM.DcNo}");
                throw new InvalidOperationException("Failed to save Labour Dc. Please try again.");
            }

        }
        private async Task HandleChildUpdatesPoWiseAsync(LabourDcOutgoing existingDc, List<LabourDcOutgoingSubVM> incomingSubVMs, StringBuilder changes, int screenCode)
        {
            try
            {
                var existingSubIds = existingDc.LabourDcOutgoingSubs.Select(s => s.DcSubId).ToHashSet();
                var incomingSubIds = incomingSubVMs.Select(s => s.DcSubId).ToHashSet();
                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();
                decimal newAcceptedQty, oldAcceptedQty = 0;
                bool rejRetrack = false;
                // DELETE removed children
                foreach (var sub in existingDc.LabourDcOutgoingSubs.Where(s => !incomingSubIds.Contains(s.DcSubId)).ToList())
                {
                    newAcceptedQty = 0;
                    oldAcceptedQty = 0;
                    rejRetrack = false;

                    changes.AppendLine($"Child Deleted - DcSubId: {sub.DcSubId}, Item: {sub.Item?.ItemCode}");
                    await _unitOfWork.LabourDcOutgoingSubs.DeleteAsync(sub.DcSubId);
                    await _unitOfWork.SaveAsync();

                    if (sub.DcSubId > 0)
                    {
                        await RollbackTracksAddIssueQtyAsync(sub.DcSubId, existingDc.SCNRejectReworkReturn);
                        if (existingDc.WithoutPoDc)
                            await UpsertLabourGRNQtyAsync(sub.RefGRNSubId ?? 0, sub.ItemId, sub.Qty, 0, "Labour GRN create   ");

                    }
                    if (sub.RefPoSubId > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            newAcceptedQty = sub.Qty - sub.RejectQty.GetValueOrDefault();

                        }
                        else
                        {
                            newAcceptedQty = sub.Qty;
                        }

                        await AdjustPoSubBalanceAsync(sub.RefPoSubId, newAcceptedQty, 0, "Labour Dc");
                    }

                    await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);

                }

                // ADD or UPDATE children
                foreach (var subVM in incomingSubVMs)
                {
                    decimal.TryParse((subVM.Qty).ToString(), out decimal qty);

                    newAcceptedQty = 0;
                    oldAcceptedQty = 0;
                    rejRetrack = false;

                    if (subVM.DcSubId == 0)
                    {
                        var newSub = _mapper.Map<LabourDcOutgoingSub>(subVM);
                        newSub.DcId = existingDc.DcId;
                        await _unitOfWork.LabourDcOutgoingSubs.CreateAsync(newSub);
                        await _unitOfWork.SaveAsync();

                        changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");

                        if (subVM.RefPoSubId > 0 || subVM.RefGRNSubId > 0)
                        {
                            if (!existingDc.Manual)
                            {
                                await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(subVM.DcSubId, subVM.RefPoSubId ?? 0,
                                    subVM.ItemId, subVM.Qty ?? 0, screenCode, currentUser, now, subVM.RefGRNSubId ?? 0, existingDc.SCNRejectReworkReturn);

                            }

                        }
                        if (existingDc.Manual)
                        {
                            if (subVM.TransType == "Out")
                                await CreateTrackAndReduceIssueAndUpadteIssueTallyManualPoWiseAsync(subVM.DcSubId, subVM.ItemId, subVM.Qty ?? 0, currentUser, now, incomingSubVMs, existingDc.ReceivedReturnRM, existingDc.SCNRejectReworkReturn);
                        }
                        if (existingDc.WithoutPoDc)
                            await UpsertLabourGRNQtyAsync(subVM.RefGRNSubId ?? 0, subVM.ItemId, 0, subVM.Qty ?? 0, "Labour GRN create   ");

                        if (subVM.RefPoSubId > 0)
                        {
                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId ?? 0);

                            if (rejRetrack)
                            {
                                newAcceptedQty = subVM.Qty.GetValueOrDefault() - subVM.RejectQty.GetValueOrDefault();

                            }
                            else
                            {
                                newAcceptedQty = subVM.Qty.GetValueOrDefault();
                            }

                            await AdjustPoSubBalanceAsync(subVM.RefPoSubId, 0, newAcceptedQty, "Labour Dc Create");
                        }
                        if (subVM.TransType == "Out")
                        {
                            if (subVM.RefPoSubId > 0 && subVM.RcSubIds != "" && subVM.CategoryCode == 2)
                            {

                                await IssueStockBySeqLogicAsync(newSub, existingDc, screenCode);


                            }
                            else
                            {

                                await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId, existingDc.StoreIssId ?? 0, subVM.Qty.GetValueOrDefault(),
                                  subVM.UnitPrice ?? 0, subVM.BatchNo, screenCode, newSub.DcSubId, existingDc.DcNo, existingDc.DcDate);

                            }

                        }

                    }
                    else
                    {
                        var existingSub = existingDc.LabourDcOutgoingSubs.FirstOrDefault(s => s.DcSubId == subVM.DcSubId);
                        if (existingSub != null)
                        {
                            if (existingSub.DcSubId > 0)
                            {
                                await RollbackTracksAddIssueQtyAsync(existingSub.DcSubId, existingDc.SCNRejectReworkReturn);
                                if (subVM.RefPoSubId > 0)
                                {

                                    rejRetrack = await HasRejectReworkPoByPoSubidAsync(subVM.RefPoSubId ?? 0);

                                    if (rejRetrack)
                                    {
                                        oldAcceptedQty = existingSub.Qty - existingSub.RejectQty.GetValueOrDefault();
                                        newAcceptedQty = subVM.Qty.GetValueOrDefault() - subVM.RejectQty.GetValueOrDefault();

                                    }
                                    else
                                    {
                                        oldAcceptedQty = existingSub.Qty;
                                        newAcceptedQty = subVM.Qty.GetValueOrDefault();
                                    }

                                    await AdjustPoSubBalanceAsync(existingSub.RefPoSubId, oldAcceptedQty, newAcceptedQty, "Labour Dc Create");
                                }

                                if (existingDc.WithoutPoDc)
                                    await UpsertLabourGRNQtyAsync(existingSub.RefGRNSubId ?? 0, existingSub.ItemId, existingSub.Qty, subVM.Qty ?? 0, "Labour GRN create   ");

                            }
                            if (subVM.RefPoSubId > 0 || subVM.RefGRNSubId > 0)
                            {
                                if (!existingDc.Manual)
                                {
                                    await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(subVM.DcSubId, subVM.RefPoSubId ?? 0,
                                           subVM.ItemId, subVM.Qty ?? 0, screenCode, currentUser, now, subVM.RefGRNSubId ?? 0, existingDc.SCNRejectReworkReturn);
                                }
                            }

                            if (existingDc.Manual)
                            {
                                if (subVM.TransType == "Out")
                                    await CreateTrackAndReduceIssueAndUpadteIssueTallyManualPoWiseAsync(subVM.DcSubId, subVM.ItemId, subVM.Qty ?? 0, currentUser, now, incomingSubVMs, existingDc.ReceivedReturnRM, existingDc.SCNRejectReworkReturn);
                            }

                            if (subVM.TransType == "Out")
                            {

                                if (subVM.RefPoSubId > 0 && (subVM.RcSubIds != "" && subVM.RcSubIds != null) && subVM.CategoryCode == 2)
                                {
                                    existingSub.Qty = subVM.Qty ?? existingSub.Qty;
                                    await IssueStockBySeqLogicAsync(existingSub, existingDc, screenCode);

                                }
                                else
                                {
                                    await _stockManagerService.IssueOrUpdateStockAsync(subVM.ItemId, existingDc.StoreIssId ?? 0, subVM.Qty.GetValueOrDefault(),
                                   subVM.UnitPrice ?? 0, subVM.BatchNo, screenCode, subVM.DcSubId, existingDc.DcNo, existingDc.DcDate);
                                }


                            }


                            var subChanges = GetPropertyChanges(existingSub, subVM);
                            if (!string.IsNullOrEmpty(subChanges))
                                changes.AppendLine($"Child Updated - ItemCode {subVM.ItemCode}:\n{subChanges}");

                            _mapper.Map(subVM, existingSub);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in HandleChildUpdatesAsync()");
            }
        }

        public async Task UpsertLabourGRNQtyAsync(int? RefGrnSubId, int itemId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (RefGrnSubId == 0) return;

                var GrnSub = await _unitOfWork.LabourGRNSubs.GetQueryable()
                    .Where(j => j.GRNSubId == RefGrnSubId && j.ItemId == itemId).FirstOrDefaultAsync();

                if (GrnSub == null) return;

                if (oldQty > 0)
                    GrnSub.BalQty += oldQty;

                if (newQty > GrnSub.Qty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

                if (newQty > 0)
                    GrnSub.BalQty -= newQty;

                await _unitOfWork.LabourGRNSubs.UpdateAsync(GrnSub);
                await _unitOfWork.SaveAsync();


                var totalBalQty = await _unitOfWork.LabourGRNSubs
                    .GetQueryable()
                    .Where(e => e.GRNId == GrnSub.GRNId)
                    .SumAsync(e => e.BalQty);

                var DC = await _unitOfWork.LabourGRNs.GetAsync(GrnSub.GRNId);
                if (DC != null)
                {
                    DC.DcTally = (totalBalQty == 0);
                    await _unitOfWork.LabourGRNs.UpdateAsync(DC);
                    await _unitOfWork.SaveAsync();
                }

            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(
                    ex, $"[UpsertLabourGRNQtyAsync] Validation failed in {context}");

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex, $"[UpsertLabourGRNQtyAsync] Unexpected error in {context}");

            }
        }
        private async Task AdjustPoSubBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refPoSubId.HasValue || refPoSubId == 0) return;

                var isOpenPo = await _unitOfWork.MfgPoSubs
                                .GetQueryable()
                                .Where(s => s.PoSubId == refPoSubId)
                                .Join(_unitOfWork.MfgPos.GetQueryable(),
                                        sub => sub.PoId,
                                        po => po.PoId,
                                        (sub, po) => po.IsOpenPO)
                                .FirstOrDefaultAsync();

                if (!isOpenPo)
                {
                    var poSub = await _unitOfWork.MfgPoSubs.GetAsync(refPoSubId.Value);
                    if (poSub == null) return;

                    if (oldQty > 0)
                        poSub.BalQty += oldQty;

                    if (newQty > poSub.BalQty)
                        throw new InvalidOperationException($"{context}: Qty cannot exceed Quote BalQty.");

                    if (newQty > 0)
                        poSub.BalQty -= newQty;

                    await _unitOfWork.MfgPoSubs.UpdateAsync(poSub);
                    await _unitOfWork.SaveAsync();


                    var totalBalQty = await _unitOfWork.MfgPoSubs
                        .GetQueryable()
                        .Where(e => e.PoId == poSub.PoId)
                        .SumAsync(e => e.BalQty);

                    var po = await _unitOfWork.MfgPos.GetAsync(poSub.PoId);
                    if (po != null)
                    {
                        po.PoTally = (totalBalQty == 0);
                        await _unitOfWork.MfgPos.UpdateAsync(po);
                        await _unitOfWork.SaveAsync();
                    }
                }

            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPOBalance] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustPoBalance] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust Po balance. Please contact support.");
            }
        }
        public async Task DeleteAndResequencePoWiseAsync(LabourDcOutgoingSubVM subitem, LabourDcOutgoingVM dcVM, int ScreenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.DcSubId > 0)
                {
                    decimal newAcceptedQty, oldAcceptedQty = 0;

                    bool rejRetrack = false;
                    var entity = await _unitOfWork.LabourDcOutgoingSubs.GetAsync(subitem.DcSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");

                    if (entity.DcSubId > 0)
                    {
                        if (entity.TransType == "Out")
                        {
                            await RollbackTracksAddIssueQtyAsync(entity.DcSubId, dcVM.SCNRejectReworkReturn);
                        }
                        else
                        {
                            await AdjustSCNSubBalanceAsync(entity.RefSCNSubId, entity.Qty, 0, " Labour DcOutgoing Cancel Item");

                        }


                        if (dcVM.WithoutPoDc)
                            await UpsertLabourGRNQtyAsync(entity.RefGRNSubId ?? 0, entity.ItemId, entity.Qty, 0, "Labour DcOutGoing Cancel  ");
                    }
                    if (entity.RefPoSubId > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(entity.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            newAcceptedQty = entity.Qty - entity.RejectQty.GetValueOrDefault();
                        }
                        else
                        {
                            newAcceptedQty = entity.Qty;
                        }

                        await AdjustPoSubBalanceAsync(entity.RefPoSubId, newAcceptedQty, 0, "Po updated");
                    }

                    if (entity.TransType == "Out")
                    {
                        await DeleteStockIssueAndTrackAsync(entity.DcSubId, entity.ItemId, ScreenCode);
                    }
                    await _unitOfWork.LabourDcOutgoingSubs.DeleteAsync(entity.DcSubId);
                    await _unitOfWork.SaveAsync();

                    await UpdateDcTallyStatusAsync(entity.DcId);
                    // Log action
                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Labour Dc",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Dc No: {dcVM?.DcNo}"
                    );
                }
                else
                {

                    dcVM.LabourDcOutgoingSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Where(x => x.DcId == dcVM.DcId)
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
        public async Task<decimal?> GetLaboourPoBalQtyByPoSubId(int PoSubId)
        {
            try
            {
                return await _unitOfWork.MfgPoSubs.GetQueryable()
                        .Where(x => x.PoSubId == PoSubId)
                        .SumAsync(x => x.BalQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading GetLaboourPoBalQtyByPoSubId details for PoSubId: {PoSubId}");
                return 0m;
            }

        }
        public async Task<List<int>> GetReceivedSCNSubIdsByRefPoSubId(int refPoSubId)
        {
            try
            {
                var scnSubIds = await
                (
                    from scn in _unitOfWork.LabourSCNSubs.GetQueryable()
                    join grnIn in _unitOfWork.LabourGRNSubs.GetQueryable()
                        on scn.RefGRNSubId equals grnIn.GRNSubId
                    join grnOut in _unitOfWork.LabourGRNSubs.GetQueryable()
                        on grnIn.GRNId equals grnOut.GRNId
                    where grnOut.RefPoSubId == refPoSubId
                       && grnOut.TransType == "Out"
                       && !scn.ItemCancel
                       && !grnIn.ItemCancel
                       && !grnOut.ItemCancel
                    select scn.SCNId
                )
                .Distinct()
                .ToListAsync();

                return scnSubIds;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error loading SCN received data for RefPoSubId: {refPoSubId}"
                );
                return new List<int>();
            }
        }
        public async Task<bool> DeleteLabourDcPoWiseByIdAsync(int DcId, int screenCode)

        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var grn = await _unitOfWork.LabourDcOutgoings
                    .GetQueryable()
                    .Include(e => e.LabourDcOutgoingSubs)
                    .FirstOrDefaultAsync(e => e.DcId == DcId);

                if (grn == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in grn.LabourDcOutgoingSubs)
                {
                    decimal oldAcceptedQty = 0;

                    bool rejRetrack = false;

                    if (sub.RefPoSubId > 0)
                    {
                        rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                        if (rejRetrack)
                        {
                            oldAcceptedQty = sub.Qty - sub.RejectQty.GetValueOrDefault();

                        }
                        else
                        {
                            oldAcceptedQty = sub.Qty;
                        }
                        await AdjustPoSubBalanceAsync(sub.RefPoSubId, oldAcceptedQty, 0, "Labour DcOutgoing Deleted");
                    }
                    if (sub.DcSubId > 0)
                    {
                        await RollbackTracksAddIssueQtyAsync(sub.DcSubId, grn.SCNRejectReworkReturn);
                        if (grn.WithoutPoDc)
                            await UpsertLabourGRNQtyAsync(sub.RefGRNSubId ?? 0, sub.ItemId, sub.Qty, 0, "Labour DcOutgoing Delete  ");
                    }

                    await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);

                }

                //----------------AutoDcRunning-------------------------------------
                var runningRow = await _unitOfWork.DcRunningNumbers
                            .GetQueryable()
                            .FirstOrDefaultAsync(x =>
                                x.DcType == "LABOURDCOUTGOING" &&
                                x.Suffix == grn.Suffix);
                if (runningRow != null)
                {
                    long oldDcNo = 0;
                    long.TryParse(grn.DcNo.ToString(), out oldDcNo);
                    if (runningRow.LastNumber == oldDcNo && runningRow.LastNumber > 1)
                    {
                        runningRow.LastNumber = (oldDcNo - 1);
                        await _unitOfWork.DcRunningNumbers.UpdateAsync(runningRow);
                    }
                }
                //----------------------------------------------------------------------------

                var deleted = await _unitOfWork.LabourDcOutgoings.DeleteAsync(DcId);
                if (!deleted) return false;

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Labour DcOutgoing",
                    action: $"Deleted Labour DcOutgoing: {grn.DcNo}",
                    additionalInfo: $"Dc Id: {grn.DcId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Dc: {DcId}");
                throw;
            }
        }
        public async Task UpdatedCancelStatusAndAddOrRevertQtyPoWiseAync(LabourDcOutgoingVM dcVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();

                var existingDC = await _unitOfWork.LabourDcOutgoings.GetAsync(dcVM.DcId);
                if (existingDC == null)
                    throw new InvalidOperationException("Labour Dc Outgoing not found.");

                var subs = await _unitOfWork.LabourDcOutgoingSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.DcId == dcVM.DcId)
                    .ToListAsync();

                if (!dcVM.DcCancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateLabourSCNBalanceBeforeRevertAsync(sub, dcVM);
                    }
                }

                existingDC.DcCancel = dcVM.DcCancel;
                existingDC.CancelReason = dcVM.CancelReason;
                await _unitOfWork.LabourDcOutgoings.UpdateAsync(existingDC);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {

                    decimal newAcceptedQty, oldAcceptedQty = 0;
                    bool rejRetrack = false;

                    if (existingDC.DcCancel)
                    {
                        if (sub.DcSubId > 0)
                        {
                            await RollbackTracksAddIssueQtyAsync(sub.DcSubId, existingDC.SCNRejectReworkReturn);

                            if (dcVM.WithoutPoDc)
                                await UpsertLabourGRNQtyAsync(sub.RefGRNSubId ?? 0, sub.ItemId, sub.Qty, 0, "Labour DcOutGoing Cancel  ");
                        }
                        if (sub.RefPoSubId > 0)
                        {


                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                            if (rejRetrack)
                            {
                                oldAcceptedQty = sub.Qty - sub.RejectQty.GetValueOrDefault();

                            }
                            else
                            {
                                oldAcceptedQty = sub.Qty;
                            }

                            await AdjustPoSubBalanceAsync(sub.RefPoSubId, oldAcceptedQty, 0, "Po updated");
                        }

                        await DeleteStockIssueAndTrackAsync(sub.DcSubId, sub.ItemId, screenCode);

                    }
                    else
                    {
                        if (sub.RefPoSubId > 0 || sub.RefGRNSubId > 0)
                        {
                            if (!dcVM.Manual)
                            {
                                await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.DcSubId, sub.RefPoSubId ?? 0,
                                    sub.ItemId, sub.Qty, screenCode, currentUser, now, sub.RefGRNSubId ?? 0, dcVM.SCNRejectReworkReturn);
                            }
                        }

                        if (sub.TransType == "Out")
                            await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.DcSubId, sub.ItemId, sub.Qty, currentUser, now, dcVM.LabourDcOutgoingSubVMs, dcVM.ReceivedReturnRM, dcVM.SCNRejectReworkReturn);

                        if (dcVM.WithoutPoDc)
                            await UpsertLabourGRNQtyAsync(sub.RefGRNSubId ?? 0, sub.ItemId, 0, sub.Qty, "Labour DcOutGoing create   ");


                        if (sub.RefPoSubId > 0)
                        {

                            rejRetrack = await HasRejectReworkPoByPoSubidAsync(sub.RefPoSubId ?? 0);

                            if (rejRetrack)
                            {
                                newAcceptedQty = sub.Qty - sub.RejectQty.GetValueOrDefault();

                            }
                            else
                            {
                                newAcceptedQty = sub.Qty;
                            }

                            await AdjustPoSubBalanceAsync(sub.RefPoSubId, 0, newAcceptedQty, "Labour DcOutGoing Create");
                        }

                        if (sub.TransType == "Out")
                        {
                            if (sub.RefPoSubId > 0 && (sub.RcSubIds != "" && sub.RcSubIds != null) && sub.Item.CategoryCode == 2)
                            {

                                await IssueStockBySeqLogicAsync(sub, existingDC, screenCode);

                            }
                            else
                            {
                                await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingDC.StoreIssId ?? 0, sub.Qty,
                                                    sub.UnitPrice, sub.BatchNo, screenCode, sub.DcSubId, existingDC.DcNo, existingDC.DcDate);

                            }
                        }

                        //await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, existingDC.StoreIssId ?? 0, sub.Qty,
                        //   sub.UnitPrice, sub.BatchNo, screenCode, sub.DcSubId, existingDC.DcNo, existingDC.DcDate, allowMultipleIssue: true);

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
        public async Task<List<MfgPoVM>> GetOpenMfgPosWiseByCustomer(int custId)
        {
            try
            {
                return await _unitOfWork.MfgPos
                 .GetQueryable()
                 .AsNoTracking()
                 .Where(p =>
                     p.CustId == custId &&
                     !p.PoTally &&
                     !p.PoCancl && !p.MfgORLab &&

                     p.MfgPoSubs.Any(ps =>
                         ps.BalQty > 0 &&
                         !ps.ItemCancel
                     )
                 )
                 .Select(p => new MfgPoVM
                 {
                     PoId = p.PoId,
                     PONo = p.PONo + p.Suffix
                 })
                 .ToListAsync();

            }
            catch (Exception ex)
            {

                throw;
            }
        }
        private async Task CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(int dcSubId, int RefPoSubId, int itemId, decimal qty,
                                                                             int screenCode, string currentUser, DateTime now, int RefGRNId, bool SCNRejRew)
        {
            try
            {
                List<int> grnSubIds;
                if (RefPoSubId == 0 && RefGRNId == 0)
                    return;

                if (RefPoSubId > 0)
                {
                    grnSubIds = await _unitOfWork.LabourGRNSubs.GetQueryable().Where(x => x.TransType == "In" &&
                                      _unitOfWork.LabourGRNSubs.GetQueryable().Where(y => y.RefPoSubId == RefPoSubId)
                                      .Select(y => y.GRNId).Contains(x.GRNId)).Select(x => x.GRNSubId).ToListAsync();
                }
                else
                {
                    grnSubIds = RefGRNId > 0 ? new List<int> { RefGRNId } : new List<int>();
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

                        List<LabourSCNSub> receivedSubs;
                        if (SCNRejRew)
                        {
                            receivedSubs = await _unitOfWork.LabourSCNSubs
                                     .GetQueryable()
                                     .Where(x =>
                                            x.SCNId > 0
                                         && grnSubIds.Contains(x.RefGRNSubId ?? 0)
                                         && x.ItemId == bom.ComponentItemId
                                         && x.SCNBalQty > 0)
                                     .OrderBy(x => x.SCNId)
                                     .ToListAsync();

                        }
                        else
                        {
                            receivedSubs = await _unitOfWork.LabourSCNSubs
                                    .GetQueryable()
                                    .Where(x =>
                                           x.SCNId > 0
                                        && grnSubIds.Contains(x.RefGRNSubId ?? 0)
                                        && x.ItemId == bom.ComponentItemId
                                        && x.BalQty > 0)
                                    .OrderBy(x => x.SCNId)
                                    .ToListAsync();
                        }

                        //var receivedSubs = await _unitOfWork.LabourSCNSubs
                        //                .GetQueryable()
                        //                .Where(x =>
                        //                       x.SCNId > 0
                        //                    && grnSubIds.Contains(x.RefGRNSubId ?? 0)
                        //                    && x.ItemId == bom.ComponentItemId
                        //                    && x.BalQty > 0)
                        //                .OrderBy(x => x.SCNId)
                        //                .ToListAsync();

                        foreach (var receve in receivedSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = 0; ;

                            if (SCNRejRew)
                            {
                                availableQty = receve.SCNBalQty;
                            }
                            else
                            {
                                availableQty = receve.BalQty;

                            }


                            decimal takeQty = Math.Min(availableQty, componentNeeded);


                            if (takeQty <= 0)
                                continue;

                            var track = new LabourDcReturnCompTrack
                            {
                                RefDcSubId = dcSubId,
                                RefSCNSubId = receve.SCNSubId,
                                RefGRNSubId = receve.RefGRNSubId.Value,
                                RefPoSubId = RefPoSubId > 0 ? RefPoSubId : null,
                                ItemIdOut = itemId,
                                QtyOut = qty,


                                ItemIdIn = bom.ComponentItemId,
                                QtyIn = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.LabourDcReturnCompTracks.CreateAsync(track);
                            await _unitOfWork.SaveAsync();
                            // 🔹 Reduce Issue Balance
                            if (SCNRejRew)
                            {
                                receve.SCNBalQty = availableQty - takeQty;
                            }
                            else
                            {
                                receve.BalQty = availableQty - takeQty;
                            }
                            if (receve.BalQty < 0) receve.BalQty = 0;
                            if (receve.SCNBalQty < 0) receve.SCNBalQty = 0;

                            await _unitOfWork.LabourSCNSubs.UpdateAsync(receve);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }
                        foreach (var scnId in receivedSubs.Where(x => x.SCNId > 0).Select(x => x.SCNId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.LabourSCNSubs
                                .GetQueryable()
                                .Where(x => x.SCNId == scnId)
                                .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.LabourSCNs.GetAsync(scnId);
                            if (issueHead != null)
                            {
                                issueHead.SCNTally = (totalBalQty == 0);
                                await _unitOfWork.LabourSCNs.UpdateAsync(issueHead);
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

                    List<LabourSCNSub> issueSubs;
                    if (SCNRejRew)
                    {
                        issueSubs = await _unitOfWork.LabourSCNSubs
                                       .GetQueryable()
                                       .Where(x =>
                                              x.SCNId > 0
                                           && grnSubIds.Contains(x.RefGRNSubId ?? 0)
                                           && x.ItemId == compData.RawMaterialItemId
                                           && x.SCNBalQty > 0)
                                       .OrderBy(x => x.SCNId)
                                       .ToListAsync();
                    }
                    else
                    {
                        issueSubs = await _unitOfWork.LabourSCNSubs
                                       .GetQueryable()
                                       .Where(x =>
                                              x.SCNId > 0
                                           && grnSubIds.Contains(x.RefGRNSubId ?? 0)
                                           && x.ItemId == compData.RawMaterialItemId
                                           && x.BalQty > 0)
                                       .OrderBy(x => x.SCNId)
                                       .ToListAsync();
                    }

                    if (issueSubs.Count > 0)
                    {
                        foreach (var issue in issueSubs)
                        {
                            if (componentNeeded <= 0)
                                break;


                            decimal availableQty = 0;
                            decimal takeQty = 0;

                            if (SCNRejRew)
                            {
                                availableQty = issue.SCNBalQty;
                            }
                            else
                            {
                                availableQty = issue.BalQty;

                            }

                            takeQty = Math.Min(availableQty, componentNeeded);

                            if (takeQty <= 0)
                                continue;

                            var track = new LabourDcReturnCompTrack
                            {
                                RefDcSubId = dcSubId,
                                RefGRNSubId = issue.RefGRNSubId.Value,
                                RefSCNSubId = issue.SCNSubId,
                                RefPoSubId = RefPoSubId > 0 ? RefPoSubId : null,

                                // Incoming → Component
                                ItemIdOut = itemId,
                                QtyOut = qty,

                                // Outgoing → Raw Material
                                ItemIdIn = compData.RawMaterialItemId,
                                QtyIn = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.LabourDcReturnCompTracks.CreateAsync(track);
                            await _unitOfWork.SaveAsync();
                            if (SCNRejRew)
                            {
                                issue.SCNBalQty = availableQty - takeQty;

                            }
                            else
                            {
                                issue.BalQty = availableQty - takeQty;
                            }

                            if (issue.BalQty < 0) issue.BalQty = 0;
                            if (issue.SCNBalQty < 0) issue.SCNBalQty = 0;

                            await _unitOfWork.LabourSCNSubs.UpdateAsync(issue);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }

                        // 🔹 Update IssueTally
                        foreach (var scnId in issueSubs.Where(x => x.SCNId > 0).Select(x => x.SCNId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.LabourSCNSubs
                                    .GetQueryable()
                                    .Where(x => x.SCNId == scnId)
                                    .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.LabourSCNs.GetAsync(scnId);
                            if (issueHead != null)
                            {
                                issueHead.SCNTally = (totalBalQty == 0);
                                await _unitOfWork.LabourSCNs.UpdateAsync(issueHead);
                                await _unitOfWork.SaveAsync();
                            }
                        }

                    }
                    else
                    {
                        List<LabourSCNSub> receiveSubs;

                        if (SCNRejRew)
                        {
                            receiveSubs = await _unitOfWork.LabourSCNSubs
                                   .GetQueryable()
                                   .Where(x =>
                                          x.SCNId > 0
                                       && grnSubIds.Contains(x.RefGRNSubId ?? 0)
                                       && x.SCNBalQty > 0)
                                   .OrderBy(x => x.SCNId)
                                   .ToListAsync();

                        }
                        else
                        {
                            receiveSubs = await _unitOfWork.LabourSCNSubs
                                   .GetQueryable()
                                   .Where(x =>
                                          x.SCNId > 0
                                       && grnSubIds.Contains(x.RefGRNSubId ?? 0)
                                       && x.BalQty > 0)
                                   .OrderBy(x => x.SCNId)
                                   .ToListAsync();
                        }


                        foreach (var issue in receiveSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = 0;
                            decimal takeQty = 0;

                            if (SCNRejRew)
                            {

                                availableQty = issue.SCNBalQty;
                                takeQty = Math.Min(availableQty, componentNeeded);

                            }
                            else
                            {
                                availableQty = issue.BalQty;
                                takeQty = Math.Min(availableQty, componentNeeded);
                            }


                            if (takeQty <= 0)
                                continue;

                            var track = new LabourDcReturnCompTrack
                            {
                                RefDcSubId = dcSubId,
                                RefGRNSubId = issue.RefGRNSubId.Value,
                                RefSCNSubId = issue.SCNSubId,
                                RefPoSubId = RefPoSubId > 0 ? RefPoSubId : null,

                                // Incoming → Component
                                ItemIdOut = itemId,
                                QtyOut = qty,

                                // Outgoing → Raw Material
                                ItemIdIn = compData.RawMaterialItemId,
                                QtyIn = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.LabourDcReturnCompTracks.CreateAsync(track);
                            await _unitOfWork.SaveAsync();

                            if (SCNRejRew)
                            {
                                issue.SCNBalQty = availableQty - takeQty;
                            }
                            else
                            {
                                issue.BalQty = availableQty - takeQty;
                            }



                            if (issue.BalQty < 0) issue.BalQty = 0;
                            if (issue.SCNBalQty < 0) issue.SCNBalQty = 0;

                            await _unitOfWork.LabourSCNSubs.UpdateAsync(issue);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }

                        foreach (var scnId in receiveSubs.Where(x => x.SCNId > 0).Select(x => x.SCNId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.LabourSCNSubs
                                    .GetQueryable()
                                    .Where(x => x.SCNId == scnId)
                                    .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.LabourSCNs.GetAsync(scnId);
                            if (issueHead != null)
                            {
                                issueHead.SCNTally = (totalBalQty == 0);
                                await _unitOfWork.LabourSCNs.UpdateAsync(issueHead);
                                await _unitOfWork.SaveAsync();
                            }
                        }

                    }


                    await _unitOfWork.SaveAsync();
                }

            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(ex, $"Failed to GetPropertyChanges in CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync");

            }

        }

        private async Task CreateTrackAndReduceIssueAndUpadteIssueTallyManualPoWiseAsync(int DcSubId, int ItemId, decimal Qty, string currentUser, DateTime now, List<LabourDcOutgoingSubVM?> LabourDcOutgoingSubs, bool Materialreturn, bool SCNRejRewReturn)
        {
            try
            {
                var selectedRows = LabourDcOutgoingSubs
                    .Where(x => x.RefSCNSubId.HasValue && x.Qty > 0)
                    .ToList();

                if (Materialreturn)
                {
                    selectedRows = selectedRows
                        .Where(x => x.TransType == "Out")
                        .ToList();
                }
                else
                {
                    selectedRows = selectedRows
                        .Where(x => x.TransType == "In")
                        .ToList();
                }

                if (!selectedRows.Any())
                    throw new Exception("No manual issue rows selected.");

                decimal totalOutQty = selectedRows.Sum(x => x.Qty ?? 0);
                if (totalOutQty <= 0)
                    throw new Exception("Invalid outgoing quantity.");


                foreach (var row in selectedRows)
                {
                    var issueSub = await _unitOfWork.LabourSCNSubs.GetAsync(row.RefSCNSubId!.Value);
                    if (issueSub == null)
                        throw new Exception($"IssueSub not found : {row.RefSCNSubId}");

                    decimal availableQty = 0;
                    if (SCNRejRewReturn)
                    {
                        availableQty = issueSub.SCNBalQty;

                    }
                    else
                    {
                        availableQty = issueSub.BalQty;
                    }



                    if (availableQty < row.Qty && availableQty < row.Qty)
                    {
                        throw new Exception($"Insufficient balance for IssueSubId {issueSub.SCNSubId}");

                    }

                    var track = new LabourDcReturnCompTrack
                    {
                        RefDcSubId = DcSubId,
                        RefSCNSubId = issueSub.SCNSubId,
                        RefGRNSubId = issueSub.RefGRNSubId.Value,
                        ItemIdOut = ItemId,
                        QtyOut = Qty,
                        ItemIdIn = row.ItemId,
                        QtyIn = row.Qty,
                        CreatedBy = currentUser,
                        CreatedDate = now
                    };

                    await _unitOfWork.LabourDcReturnCompTracks.CreateAsync(track);
                    await _unitOfWork.SaveAsync();

                    if (SCNRejRewReturn)
                    {
                        issueSub.SCNBalQty = availableQty - row.Qty ?? 0m;

                    }
                    else
                    {
                        issueSub.BalQty = availableQty - row.Qty ?? 0m;

                    }


                    if (issueSub.BalQty < 0) issueSub.BalQty = 0;
                    if (issueSub.SCNBalQty < 0) issueSub.SCNBalQty = 0;

                    await _unitOfWork.LabourSCNSubs.UpdateAsync(issueSub);
                    await _unitOfWork.SaveAsync();
                }


                var issueIdss = selectedRows
                    .Select(x => x.RefSCNSubId)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct();

                var affectedScnIds = await _unitOfWork.LabourSCNSubs
                                 .GetQueryable()
                                 .Where(s => selectedRows
                                     .Select(r => r.RefSCNSubId!.Value)
                                     .Contains(s.SCNSubId))
                                 .Select(s => s.SCNId)
                                 .Distinct()
                                 .ToListAsync();

                foreach (var scnId in affectedScnIds)
                {

                    var remainingBalQty = await _unitOfWork.LabourSCNSubs
                        .GetQueryable()
                        .Where(s => s.SCNId == scnId)
                        .SumAsync(s => s.BalQty);

                    var issueHead = await _unitOfWork.LabourSCNs.GetAsync(scnId);
                    if (issueHead != null)
                    {
                        issueHead.SCNTally = remainingBalQty == 0;
                        await _unitOfWork.LabourSCNs.UpdateAsync(issueHead);
                    }
                }

                await _unitOfWork.SaveAsync();

            }
            catch (Exception ex)
            {
                _logs.LogDeveloperError(ex, $"Failed to GetPropertyChanges in CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync");

            }

        }

        public async Task<List<LabourDcOutgoingStatusVM>> GetLabourDcStatusListAsync(string status)
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<LabourDcOutgoingStatusVM>("Sp_GetLabourDcOutgoingStatusList", status);
                return result.ToList();
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> IsDocumentUploaded(int dcId)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Labour-DcOutgoing" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == dcId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }

        public async Task<List<LabourSCNVM>> LoadScnNumbersPoWiseAsync(List<int> poIds, string check, bool ReceivedReturn)
        {
            try
            {
                if (poIds == null || poIds.Count == 0)
                    return new List<LabourSCNVM>();


                List<LabourSCNVM> scns;   // DECLARE HERE

                if (check == "PO")
                {

                    var poSubIds = await _unitOfWork.MfgPoSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(ps => poIds.Contains(ps.PoId))
                        .Select(ps => ps.PoSubId)
                        .ToListAsync();

                    if (!ReceivedReturn)
                    {
                        scns = await (
                                from scn in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                                join scnSub in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                                    on scn.SCNId equals scnSub.SCNId
                                join grnSub in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                                    on scnSub.RefGRNSubId equals grnSub.GRNSubId
                                where
                                    scn.Authorized
                                    && !scn.SCNTally
                                    && !scn.SCNCancel
                                    && !scnSub.ItemCancel
                                    && scnSub.BalQty > 0 

                                    && _unitOfWork.LabourGRNSubs.GetQueryable()
                                        .Any(x => x.GRNId == grnSub.GRNId
                                            && x.RefPoSubId.HasValue
                                            && poSubIds.Contains(x.RefPoSubId ?? 0))
                                group scn by new { scn.SCNId, scn.SCNNo, scn.Suffix } into g
                                select new LabourSCNVM
                                {
                                    SCNId = g.Key.SCNId,
                                    SCNNo = g.Key.SCNNo + g.Key.Suffix
                                }
                            ).ToListAsync();
                    }
                    else
                    {
                        scns = await (
                                from scn in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                                join scnSub in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                                    on scn.SCNId equals scnSub.SCNId
                                join grnSub in _unitOfWork.LabourGRNSubs.GetQueryable().AsNoTracking()
                                    on scnSub.RefGRNSubId equals grnSub.GRNSubId
                                where
                                    scn.Authorized
                                    && !scn.SCNCancel
                                    && !scnSub.ItemCancel
                                    && (scnSub.SCNBalQty > 0 || scnSub.BalQty > 0) 

                                    && _unitOfWork.LabourGRNSubs.GetQueryable()
                                        .Any(x => x.GRNId == grnSub.GRNId
                                            && x.RefPoSubId.HasValue
                                            && poSubIds.Contains(x.RefPoSubId ?? 0))
                                group scn by new { scn.SCNId, scn.SCNNo, scn.Suffix } into g
                                select new LabourSCNVM
                                {
                                    SCNId = g.Key.SCNId,
                                    SCNNo = g.Key.SCNNo + g.Key.Suffix
                                }
                            ).ToListAsync();
                    }

                    return scns;

                }
                else
                {
                    var grnSubIds = await _unitOfWork.LabourGRNSubs

                            .GetQueryable()
                            .AsNoTracking()
                            .Where(ps => poIds.Contains(ps.GRNId) && ps.TransType == "In" && !ps.ItemCancel)
                            .Select(ps => ps.GRNSubId)
                            .ToListAsync();


                    if (!ReceivedReturn)
                    {
                        scns = await (
                          from scn in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                          join scnSub in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                              on scn.SCNId equals scnSub.SCNId
                          where
                              scn.Authorized
                              && !scn.SCNTally
                              && !scn.SCNCancel
                              && !scnSub.ItemCancel
                              && scnSub.BalQty > 0 
                              && scnSub.RefGRNSubId.HasValue
                              && grnSubIds.Contains(scnSub.RefGRNSubId ?? 0)
                          group scn by new
                          {
                              scn.SCNId,
                              scn.SCNNo,
                              scn.Suffix
                          }
                          into g
                          select new LabourSCNVM
                          {
                              SCNId = g.Key.SCNId,
                              SCNNo = g.Key.SCNNo + g.Key.Suffix
                          }
                      ).ToListAsync();
                    }
                    else
                    {
                        scns = await (
                          from scn in _unitOfWork.LabourSCNs.GetQueryable().AsNoTracking()
                          join scnSub in _unitOfWork.LabourSCNSubs.GetQueryable().AsNoTracking()
                              on scn.SCNId equals scnSub.SCNId
                          where
                              scn.Authorized
                              && !scn.SCNCancel
                              && !scnSub.ItemCancel
                              && (scnSub.SCNBalQty > 0 || scnSub.BalQty > 0) 
                              && scnSub.RefGRNSubId.HasValue
                              && grnSubIds.Contains(scnSub.RefGRNSubId ?? 0)
                          group scn by new
                          {
                              scn.SCNId,
                              scn.SCNNo,
                              scn.Suffix
                          }
                          into g
                          select new LabourSCNVM
                          {
                              SCNId = g.Key.SCNId,
                              SCNNo = g.Key.SCNNo + g.Key.Suffix
                          }
                      ).ToListAsync();

                    }
                    return scns;

                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching SCN Number  item detail for POID: {poIds}");
                throw new InvalidOperationException("Failed to retrieve SCN Number details.");
            }
        }

    }
}
