using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.Vml.Office;
using ExcelDataReader.Log;
using FastReport.Import.RDL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractGRNService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;


namespace V.SMART.Shared.BusinessLayer.BusinessService.OutSourcingService.SubContractGRNService
{
    public class SubConGRNService : ISubConGRNService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly IStockManagerService _stockManagerService;
        private readonly IProductionSCNAssyservice _productionSCNAssyService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public SubConGRNService(
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

        public async Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText)
        {
            return await _commonService.SearchVendorsAsync(searchText);
        }
        public async Task<VendorVM?> GetVendorByVenerCodeAsync(int vendorCode)
       => await _commonService.GetVendorByVenerCodeAsync(vendorCode);

        //Machines
        public Task<List<Machine>> GetAllActiveMachinesAsync()
                => _commonService.GetAllMachineAsync();
        public async Task<bool> IsPOWiseSubConDcOutEnabledAsync()
        => await _commonService.GetScreenPermissionsAsync("Sub-Contract DC-Out", "PO Wise Sub-Contract DC Outgoing");


        // 🔹 Subcontract GRN operations

        public async Task<bool> DeleteSubconGRNByIdAsync(int GRNId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                bool IsPoWise=await IsPOWiseSubConDcOutEnabledAsync();
                var grn = await _unitOfWork.SubConGRNs
                    .GetQueryable()
                    .Include(e => e.SubConGRNSubs)
                        .ThenInclude(s => s.Item)
                    .FirstOrDefaultAsync(e => e.GRNId == GRNId);

                if (grn == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in grn.SubConGRNSubs)
                {
                    changes.AppendLine($"Child Deleted - GRNSubId: {sub.GRNSubId}, Item: {sub.Item?.ItemCode}");

                    if (sub.GRNSubId > 0)
                            await RollbackTracksAddIssueQtyAsync(sub.GRNSubId);


                   
                    if (!grn.Rejection && !grn.Return)
                    {
                        bool isSameItem = false;

                        if (sub.RefPoSubId.HasValue)
                        {
                            isSameItem = await CheckIsSameAsOutItemByPoSubId(sub.RefPoSubId.Value);
                        }

                        if ((grn.IsManual || isSameItem) && sub.RefDcSubId.GetValueOrDefault() > 0 && sub.TransType == "In")
                        {
                            await AdjustDcOutgoingItemBalanceAsync(
                               sub.RefDcSubId.Value,
                               sub.Qty ?? 0,
                               0,
                               "SubContract GRN Delete"
                           );
                        }
                    }
                    if (sub.RefRcSubId.HasValue && sub.RefRcSubId > 0 && sub.TransType == "In")
                    {
                        //await RevertRCSubIssuedToBalQtyAsync(
                        //    sub.RefRcSubId,
                        //    sub.Qty ?? 0,
                        //    0,
                        //    "Subcontract GRN Return/Rejection Delete"
                        //);
                        await AdjustRcInItemBalanceAsync(sub.RefRcSubId, sub.Qty ?? 0,0,"SubContract GRN Deleted");
                        var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

                        var issueSubs = await _unitOfWork.SubConDCOutSubs
                                           .GetQueryable()
                                           .Where(x => issueIds.Contains(x.DcId) 
                                               && x.TransType == "In")
                                           .OrderBy(x => x.DcSubId)
                                           .ToListAsync();

                        if (issueSubs.Any() )
                        {
                            foreach (var Grnsub in issueSubs)
                            {
                                await AdjustDcOutgoingItemBalanceAsync(Grnsub.DcSubId, sub.Qty ?? 0, 0, "SubContract GRN deleted");
                            }

                        }

                    }

                    // 🔹 Stock Delete Entry
                    if (sub.ItemId.HasValue && sub.TransType == "In")
                    {
                        await DeleteStockAddAsync(
                            sub.GRNSubId,
                            sub.ItemId.Value,
                            screenCode,
                            grn.GRNNo
                        );
                    }
                    if (IsPoWise)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustPoSubBalanceAsync(sub.RefPoSubId, sub.Qty ?? 0, 0, "Subcontract GRN Delete");
                        }

                    }
                }

                // Delete child records (CORRECT REPO)
                await _unitOfWork.SubConGRNSubs.DeleteRangeAsync(grn.SubConGRNSubs);

                // Delete parent
                await _unitOfWork.SubConGRNs.DeleteAsync(grn);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Logging
                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Subcontract GRN",
                    action: $"Deleted Subcontract GRN: {grn.GRNNo}",
                    additionalInfo: $"GRN Id: {grn.GRNId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Subcontract GRN: {GRNId}");
                throw;
            }
        }


        public async Task<int> GetPodetailsForVendor(int vendorcode)
        {
            // Get pending PO SubIds
            var poSubIds = await GetDistinctPOSubIdsWithPendingIssueAsync();

            // Count matching PO rows
            return await (
                from p in _unitOfWork.PurchPos.GetQueryable()
                join ps in _unitOfWork.PurchPoSubs.GetQueryable()
                    on p.PoId equals ps.PoId
                join i in _unitOfWork.ItemRepositories.GetQueryable()
                    on ps.ItemId equals i.ItemId
                where p.VendorCode == vendorcode
                      && !p.PoTally
                      && !p.PoCancl
                      && !ps.ItemCancel
                      && ps.BalQty > 0
                      && poSubIds.Contains(ps.PoSubId)   // ✅ correct usage
                select ps.PoSubId
            ).CountAsync();
        }

        public async Task<int> GetDcdetailsForVendor(int vendorCode)
        {
            return await (
                from p in _unitOfWork.SubConDCOuts.GetQueryable()
                join ps in _unitOfWork.SubConDCOutSubs.GetQueryable()
                    on p.DcId equals ps.DcId
                where ps.TransType == "In"
                      && ps.BalQty > 0
                      && p.VendorCode == vendorCode
                      && p.IsWithoutPoDc == true
                      && _unitOfWork.SubConDCOutSubs.GetQueryable().Any(dsOut =>
                             dsOut.DcId == ps.DcId
                          && dsOut.TransType == "Out"
                          && dsOut.BalQty > 0
                      )
                select 1
            ).CountAsync();
        }

        public async Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int VendorCode)
        {
            try
            {

                var poSubIds = await GetDistinctPOSubIdsWithPendingIssueAsync();
                // 1️⃣ Fetch PO data
                var poData = await (
                                from p in _unitOfWork.PurchPos.GetQueryable()
                                join ps in _unitOfWork.PurchPoSubs.GetQueryable()
                                    on p.PoId equals ps.PoId
                                join i in _unitOfWork.ItemRepositories.GetQueryable()
                                    on ps.ItemId equals i.ItemId
                                where p.VendorCode == VendorCode
                                        && !p.PoTally
                                        && !p.PoCancl
                                        && !ps.ItemCancel
                                        && ps.BalQty > 0 && (poSubIds.Contains(ps.PoSubId)) // Only pending issue PO subs
                                select new
                                {
                                    ps.PoSubId,
                                    p.PoId,
                                    p.IsOpenPO,
                                    p.PONo,
                                    p.Suffix,
                                    p.PODate,

                                    ps.ItemId,
                                    ps.Item.ItemCode,
                                    ps.Item.ItemName,
                                    ps.Item.MeasureUnit,
                                    i.CategoryCode,
                                    i.Category.CategoryName,

                                    ps.BalQty,
                                    ps.UnitPrice,
                                    ps.DueDate,

                                    CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                                    ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,
                                    p.MainRemark
                                }
                            ).ToListAsync();

                if (!poData.Any())
                    return new List<Dictionary<string, object>>();

                // 2️⃣ Build Result
                var result = poData.Select(r => new Dictionary<string, object>
                {

                    ["Selected"] = false,
                    ["PoSubId"] = r.PoSubId,
                    ["PoId"] = r.PoId,
                    ["IsOpenPo"] = r.IsOpenPO ? "Yes" : "No",
                    ["TransType"] = "In",

                    ["PoNo"] = $"{r.PONo}{r.Suffix}",
                    ["PoDate"] = r.PODate,

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["UOM"] = r.MeasureUnit ?? string.Empty,

                    ["CategoryCode"] = r.CategoryCode,
                    ["Category"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,

                    ["UnitPrice"] = r.UnitPrice,
                    ["PoDuedate"] = r.DueDate,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                    ["Remark"] = r.MainRemark ?? string.Empty

                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching PO details for CustId: {VendorCode}");
                throw new InvalidOperationException("Failed to retrieve PO details. Please try again.", ex);
            }
        }
        public async Task<List<Dictionary<string, object>>> GetDCDetailsByWithOutPoVendorId(int vendorCode)
        {
            try
            {
                var subQuery = _unitOfWork.SubConDCOutSubs.GetQueryable();

                var dcData = await (
                    from p in _unitOfWork.SubConDCOuts.GetQueryable()

                    join ps in subQuery
                        on p.DcId equals ps.DcId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId

                    where ps.TransType == "In"
                          && ps.BalQty > 0 && p.IsWithoutPoDc
                          && p.VendorCode == vendorCode

                          // Optimized EXISTS
                          && subQuery.Any(dsOut =>
                                dsOut.DcId == ps.DcId
                                && dsOut.TransType == "Out" 
                                && dsOut.BalQty > 0
                          )

                    select new
                    {
                        ps.DcSubId,
                        p.DcId,
                        p.DcNo,
                        p.Suffix,
                        p.DcDate,

                        ps.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.MeasureUnit,

                        i.CategoryCode,
                        CategoryName = i.Category.CategoryName,

                        ps.BalQty,
                        ps.UnitPrice,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,

                        MainRemarks = p.MainRemarks
                    }
                ).ToListAsync();

                if (!dcData.Any())
                    return new List<Dictionary<string, object>>();

                var result = dcData.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,

                    ["DcSubId"] = r.DcSubId,
                    ["DcId"] = r.DcId,
                    ["DcNo"] = $"{r.DcNo}{r.Suffix}",
                    ["DcDate"] = r.DcDate.ToString("dd/MM/yyyy"),

                    ["TransType"] = "In",

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,

                    ["CategoryCode"] = r.CategoryCode,
                    ["CategoryName"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,

                    ["UnitPrice"] = r.UnitPrice,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["Remark"] = r.MainRemarks ?? string.Empty
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DC details for VendorCode: {vendorCode}");
                throw new InvalidOperationException("Failed to retrieve DC details.", ex);
            }
        }
        public async Task<List<Dictionary<string, object>>> GetDCDetailsByVendorId(int vendorCode)
        {
            try
            {
                var subQuery = _unitOfWork.SubConDCOutSubs.GetQueryable();

                var dcData = await (
                    from p in _unitOfWork.SubConDCOuts.GetQueryable()

                    join ps in subQuery
                        on p.DcId equals ps.DcId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId

                    where ps.TransType == "In"
                          && ps.BalQty > 0
                          && p.VendorCode == vendorCode && ps.RcSubId == null

                          // Optimized EXISTS
                          && subQuery.Any(dsOut =>
                                dsOut.DcId == ps.DcId
                                && dsOut.TransType == "Out"
                                && dsOut.BalQty > 0
                          )

                    select new
                    {
                        ps.DcSubId,
                        p.DcId,
                        p.DcNo,
                        p.Suffix,
                        p.DcDate,

                        ps.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.MeasureUnit,

                        i.CategoryCode,
                        CategoryName = i.Category.CategoryName,
                        ps.RefPoSubId,

                        ps.BalQty,
                        ps.UnitPrice,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,

                        MainRemarks = p.MainRemarks
                    }
                ).ToListAsync();

                if (!dcData.Any())
                    return new List<Dictionary<string, object>>();

                var result = dcData.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,

                    ["DcSubId"] = r.DcSubId,
                    ["DcId"] = r.DcId,
                    ["DcNo"] = $"{r.DcNo}{r.Suffix}",
                    ["DcDate"] = r.DcDate.ToString("dd/MM/yyyy"),

                    ["TransType"] = "In",

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,

                    ["CategoryCode"] = r.CategoryCode,
                    ["CategoryName"] = r.CategoryName ?? string.Empty,
                    ["RefPoSubId"] = r.RefPoSubId == 0 ? null : r.RefPoSubId,
                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,

                    ["UnitPrice"] = r.UnitPrice,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["Remark"] = r.MainRemarks ?? string.Empty
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DC details for VendorCode: {vendorCode}");
                throw new InvalidOperationException("Failed to retrieve DC details.", ex);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetDCDetailsByReturnVendorId(int vendorCode)
        {
            try
            {
                var dcData = await (
                    from p in _unitOfWork.SubConDCOuts.GetQueryable()

                    join ps in _unitOfWork.SubConDCOutSubs.GetQueryable()
                        on p.DcId equals ps.DcId

                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                        on ps.ItemId equals i.ItemId

                    where p.VendorCode == vendorCode
                          && ps.TransType == "Out"     // ✅ only OUT items
                          && ps.BalQty > 0  && p.IsWithoutPoDc           // ✅ balance qty only

                    select new
                    {
                        ps.DcSubId,
                        p.DcId,
                        p.DcNo,
                        p.Suffix,
                        p.DcDate,

                        ps.ItemId,
                        i.ItemCode,
                        i.ItemName,
                        i.MeasureUnit,

                        i.CategoryCode,
                        CategoryName = i.Category.CategoryName,

                        ps.BalQty,
                        ps.UnitPrice,

                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,

                        MainRemarks = p.MainRemarks
                    }
                ).ToListAsync();

                if (!dcData.Any())
                    return new List<Dictionary<string, object>>();

                var result = dcData.Select(r => new Dictionary<string, object>
                {
                    ["Selected"] = false,

                    ["DcSubId"] = r.DcSubId,
                    ["DcId"] = r.DcId,
                    ["DcNo"] = $"{r.DcNo}{r.Suffix}",
                    ["DcDate"] = r.DcDate.ToString("dd/MM/yyyy"),

                    ["TransType"] = "In",

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode ?? string.Empty,
                    ["ItemName"] = r.ItemName ?? string.Empty,
                    ["MeasureUnit"] = r.MeasureUnit ?? string.Empty,

                    ["CategoryCode"] = r.CategoryCode,
                    ["CategoryName"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["BalQty"] = r.BalQty,

                    ["UnitPrice"] = r.UnitPrice,

                    ["CostCenterId"] = r.CostCenterId,
                    ["ProjectNo"] = r.ProjectNo ?? string.Empty,

                    ["Remark"] = r.MainRemarks ?? string.Empty
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DC details for VendorCode: {vendorCode}");
                throw new InvalidOperationException("Failed to retrieve DC details.", ex);
            }
        }
        public async Task<List<int>> GetSubConDcOutIdsByDcSubIdAsync(int dcSubId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.DcSubId == dcSubId && x.DcId > 0 && x.BalQty > 0 && !x.ItemCancel)
                    .Select(x => x.DcId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching for DcSubId: {dcSubId}");
                throw new InvalidOperationException("Failed to retrieve DcOutgoing details for the selected DC. Please try again.", ex);
            }
        }

        public async Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId)
        {
            try
            {
                return await _unitOfWork.AssmblyDefs
                    .GetQueryable()
                    .Where(x => x.AssmblyID == assemblyId)
                    .Select(x => new AssemblyDefVM
                    {
                        ItemId = x.ItemId,
                        UtilQty = x.UtilQty
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Assembly Items for AssemblyId: {assemblyId}");
                throw new InvalidOperationException("Failed to retrieve Assembly items. Please try again.", ex);
            }
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
        public async Task<List<ItemVM>> GetRawMaterialItemsAsync(int compItemId)
        {
            try
            {
                return await _unitOfWork.CompMasters
                    .GetQueryable()
                    .Where(x => x.CompItemId == compItemId)
                    .OrderByDescending(c => c.IsDefaultRM)
                    .Select(x => new ItemVM
                    {
                        ItemId = x.RMId ?? 0,
                        Weight = x.Weight
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Raw Material Items for CompItemId: {compItemId}");
                throw new InvalidOperationException("Failed to retrieve raw material items. Please try again.", ex);
            }
        }

        public async Task<List<int>> GetDcIdsByDcSubIdAsync(int DcSubId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.DcSubId == DcSubId)
                    .Select(x => x.DcId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DcIds for DcSubId: {DcSubId}");
                throw new InvalidOperationException("Failed to retrieve DC details for the selected dc. Please try again.", ex);
            }
        }

        public async Task<List<int>> GetIssueIdsByRcSubIdAsync(int rcSubId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.RcSubId == rcSubId)
                    .Select(x => x.DcId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching IssueIds for RcSubId: {rcSubId}");
                throw new InvalidOperationException("Failed to retrieve Issue details for the selected Route Card. Please try again.", ex);
            }
        }

        public async Task<List<int>> GetDcIdsByDcSubIdsAsync(List<int> dcSubIds)
        {
            try
            {
                if (dcSubIds == null || !dcSubIds.Any())
                    return new List<int>();

                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => dcSubIds.Contains(x.DcSubId))
                    .Select(x => x.DcId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DcIds for DcSubIds: {string.Join(",", dcSubIds)}");

                throw new InvalidOperationException(
                    "Failed to retrieve DC details for selected DCs. Please try again.", ex);
            }
        }
        public async Task<List<int>> GetDcIdsByRcSubIdsAsync(List<int> RcSubIds)
        {
            try
            {
                if (RcSubIds == null || !RcSubIds.Any())
                    return new List<int>();

                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => RcSubIds.Contains(x.RcSubId??0))
                    .Select(x => x.DcId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching DcIds for RcSubIds: {string.Join(",", RcSubIds)}");

                throw new InvalidOperationException(
                    "Failed to retrieve DC details for selected DCs. Please try again.", ex);
            }
        }

        public async Task<List<SubConGRNSubVM>> GetSubConGrnSubVMsByDcIdsAsync(List<int> dcIds)
        {
            if (dcIds == null || !dcIds.Any())
                return new List<SubConGRNSubVM>();

            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Include(x => x.Item)
                    .Where(x => dcIds.Contains(x.DcId)
                             && x.TransType == "Out" && x.BalQty > 0)
                    .Select(x => new SubConGRNSubVM
                    {
                        RefDcSubId = x.DcSubId,
                        RefDcNo = $"{x.SubConDcOut.DcNo}{x.SubConDcOut.Suffix}",
                        RefDCDate = x.SubConDcOut.DcDate,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.ItemName,
                        MeasureUnit = x.Item.MeasureUnit,
                        Qty = x.BalQty.GetValueOrDefault(),
                        BalQty = x.BalQty ?? 0m,
                        UnitPrice = x.UnitPrice,
                        RefPoSubId = x.RefPoSubId,
                        Remarks = x.Remark,
                        TransType = "Out",
                        IsEditable = false
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching outgoing data for DcIds: {string.Join(",", dcIds)}");
                throw new InvalidOperationException("Failed to retrieve outgoing DC issue details.", ex);
            }
        }

        public async Task<decimal> GetPossibleAssemblyQtyAsync(int itemId, Dictionary<int, decimal?> issuedLookup, decimal balQty)
        {
            try
            {
                if (itemId <= 0 || balQty <= 0 || issuedLookup == null)
                    return 0;

                var bomData = await _unitOfWork.AssmblyDefs
                    .GetQueryable()
                    .Where(x => x.AssmblyID == itemId)
                    .Select(x => new
                    {
                        x.ItemId,
                        x.UtilQty
                    })
                    .ToListAsync();

                if (!bomData.Any())
                    return 0;

                decimal? minPossible = null;

                foreach (var bom in bomData)
                {
                    if (bom.ItemId <= 0)
                        continue;

                    decimal issuedQty =
                        issuedLookup.TryGetValue(bom.ItemId, out var v)
                        ? (v ?? 0m)
                        : 0m;

                    if (bom.UtilQty > 0)
                    {
                        var possible = issuedQty / bom.UtilQty;

                        if (minPossible == null || possible < minPossible)
                            minPossible = possible;
                    }
                }

                return Math.Min(minPossible ?? 0, balQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error calculating possible assembly qty for ItemId: {itemId}");
                throw new InvalidOperationException("Failed to calculate possible assembly quantity.", ex);
            }
        }

        public async Task<decimal> GetPossibleComponentQtyAsync(int itemId, Dictionary<int, decimal?> issuedLookup, decimal balQty)
        {
            try
            {
                if (itemId <= 0 || balQty <= 0 || issuedLookup == null)
                    return 0;

                var compMasterQuery = _unitOfWork.CompMasters
                    .GetQueryable()
                    .Where(x => x.CompItemId == itemId);

                var compMasterData = await compMasterQuery
                    .Where(x => x.IsDefaultRM)
                    .Select(x => new { x.RMId, x.Weight })
                    .ToListAsync();

                if (!compMasterData.Any())
                {
                    compMasterData = await compMasterQuery
                        .OrderByDescending(x => x.Id)
                        .Take(1)
                        .Select(x => new { x.RMId, x.Weight })
                        .ToListAsync();
                }

                if (!compMasterData.Any())
                    return 0;

                decimal? minPossible = null;

                foreach (var comp in compMasterData)
                {
                    if (!comp.RMId.HasValue)
                        continue;

                    decimal issuedQty =
                        issuedLookup.TryGetValue(comp.RMId.Value, out var v)
                        ? (v ?? 0m)
                        : 0m;

                    if (comp.Weight > 0)
                    {
                        var possible = issuedQty / comp.Weight;

                        if (minPossible == null || possible < minPossible)
                            minPossible = possible;
                    }
                }

                return Math.Min(minPossible ?? 0, balQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error calculating component possible qty for ItemId: {itemId}");
                throw new InvalidOperationException("Failed to calculate component quantity.", ex);
            }
        }

        public async Task<List<SubConDcOutSubVM>> GetSubConDcOutItemsByPoSubAsync(int itemId, int poSubId)
        {
            if (itemId <= 0 || poSubId <= 0)
                return new List<SubConDcOutSubVM>();

            return await _unitOfWork.SubConDCOutSubs.GetQueryable()
                .Where(x =>
                    x.ItemId != itemId &&
                    x.RefPoSubId == poSubId &&
                    !x.ItemCancel
                )
                .Select(x => new SubConDcOutSubVM
                {
                    DcSubId = x.DcSubId,
                    ItemId = x.ItemId,
                    ItemCode = x.Item.ItemCode,
                    ItemName = x.Item.ItemName,
                    MeasureUnit = x.Item.MeasureUnit,
                    Qty = x.Qty,
                    BalQty = x.BalQty,
                    CostId = x.CostId,
                    ProjectNo = x.CostCenter != null ? x.CostCenter.ProjectNo : null,
                    Remark = x.Remark,
                    RefPoSubId = x.RefPoSubId
                })
                .ToListAsync();
        }

        public async Task<List<int>> GetDistinctDcIdsByPoSubIdsAsync(List<int> poSubIds)
        {
            try
            {
                if (poSubIds == null || poSubIds.Count == 0)
                    return new List<int>();

                return await _unitOfWork.SubConDCOutSubs.GetQueryable()
                    .Where(x =>
                        poSubIds.Contains(x.RefPoSubId.Value) &&
                        !x.ItemCancel
                    )
                    .Select(x => x.DcId)
                    .Distinct()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetDistinctDcIdsByPoSubIdsAsync failed.");
                return new List<int>();
            }
        }

        public async Task<List<SubConGRNSubVM>> GetSubConDcOutDataByDcIdsAsync(List<int> dcIds)
        {
            try
            {
                if (dcIds == null || dcIds.Count == 0)
                    return new List<SubConGRNSubVM>();

                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x =>
                        dcIds.Contains(x.DcId) &&
                        x.TransType == "Out" &&
                        !x.ItemCancel
                    )
                    .OrderByDescending(x => x.DcId)
                    .Take(8)
                    .Select(x => new SubConGRNSubVM
                    {
                        RefDcSubId = x.DcSubId,
                        RefDcNo = $"{x.SubConDcOut.DcNo}{x.SubConDcOut.Suffix}",
                        RefDCDate = x.SubConDcOut.DcDate,

                        RefPoSubId = x.RefPoSubId,

                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.ItemName,
                        MeasureUnit = x.Item.MeasureUnit,

                        Qty = x.Qty,
                        BalQty = x.BalQty,
                        UnitPrice = x.UnitPrice,
                        CostId = x.CostId,
                        ProjectNo = x.CostCenter.ProjectNo,
                        Remarks = x.Remark
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetSubConDcOutDataByDcIdsAsync failed.");
                return new List<SubConGRNSubVM>();
            }
        }

        public async Task<int?> GetDefaultRawMaterialItemIdAsync(int compItemId)
        {
            try
            {
                if (compItemId <= 0)
                    return null;

                return await _unitOfWork.CompMasters
                    .GetQueryable()
                    .Where(x => x.CompItemId == compItemId)
                    .OrderByDescending(x => x.IsDefaultRM)   // default RM first
                    .Select(x => (int?)x.RawMaterial.ItemId)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while fetching Default RM ItemId");
                return null;
            }
        }

        public async Task<List<SubConGRNSubVM>> GetSubConDcOutDataByDcIdAndRmIdAsync(List<int> dcIds, int rmItemId)
        {
            try
            {
                if (dcIds == null || dcIds.Count == 0 || rmItemId <= 0)
                    return new List<SubConGRNSubVM>();

                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x =>
                        dcIds.Contains(x.DcId) &&
                        x.ItemId == rmItemId &&
                        x.TransType == "Out" &&
                        !x.ItemCancel
                    )
                    .OrderBy(x => x.DcId)
                    .ThenBy(x => x.DcSubId)
                    .Select(x => new SubConGRNSubVM
                    {
                        RefDcSubId = x.DcSubId,
                        RefPoSubId = x.RefPoSubId,

                        RefDcNo = $"{x.SubConDcOut.DcNo}{x.SubConDcOut.Suffix}",
                        RefDCDate = x.SubConDcOut.DcDate,

                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.ItemName,
                        MeasureUnit = x.Item.MeasureUnit,

                        Qty = x.Qty,
                        BalQty = x.BalQty,
                        UnitPrice = x.UnitPrice,

                        ProjectNo = x.CostCenter.ProjectNo,
                        Remarks = x.Remark,

                        TransType = x.TransType
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetSubConDcOutDataByDcIdAndRmIdAsync failed.");
                return new List<SubConGRNSubVM>();
            }
        }



        //public async Task<List<SubConDcOutSubVM>> GetIssuedQtyByIssueIdsAsync(List<int> issueIds)
        //{
        //    if (issueIds == null || !issueIds.Any())
        //        return new List<SubConDcOutSubVM>();

        //    try
        //    {
        //        return await _unitOfWork.SubConDcOutSubs
        //            .GetQueryable()
        //            .Where(x => x.IssueId.HasValue
        //                     && issueIds.Contains(x.IssueId.Value)
        //                     && x.TransType == "Out")
        //            .GroupBy(x => x.ItemId)
        //            .Select(g => new SubConDcOutSubVM
        //            {
        //                ItemId = g.Key,
        //                BalQty = g.Sum(x => x.BalQty)

        //            })
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"Error fetching issued quantities for IssueIds: {string.Join(",", issueIds)}");
        //        throw new InvalidOperationException("Failed to retrieve issued quantity details. Please try again.", ex);
        //    }
        //}
        public async Task<RouteCardSub?> GetRcDetailsByRcSubIdAsync(int rcSubId)
        {
            try
            {
                return await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Include(x => x.RouteCard)
                    .Where(x => x.RCSubId == rcSubId)
                    .Select(x => x)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetRcDetailsByRcSubIdAsync failed for RCSubId : {rcSubId}");
                throw;
            }
        }



        public async Task<int> GetPendingJobOrdersCountAsync()
        {
            try
            {
                return await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Where(j => j.IssuedQty > 0)
                    .CountAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching pending job orders count");
                throw new InvalidOperationException("Failed to retrieve pending job orders count. Please try again.", ex);
            }
        }




        public async Task<(List<SubConGRNVM> SubconGRNVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.SubConGRNs.GetQueryable()
                .Include(j => j.SubConGRNSubs)
                    .ThenInclude(s => s.Item)
                .Include(j => j.SubConGRNSubs)
                    .ThenInclude(s => s.RouteCardSubs)
                .Include(j => j.vendor)
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
                .OrderByDescending(x => x.GRNId)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Use AutoMapper
            var vmList = _mapper.Map<List<SubConGRNVM>>(list);

            return (vmList, total);
        }

        public static class MaterialReturnFilterBuilder
        {
            public static IQueryable<SubConGRN> ApplyFilter(IQueryable<SubConGRN> query, string field, object value)
            {
                if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                    return query;

                string val = value.ToString().Trim();

                switch (field)
                {
                    case "GRNNo":
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
                                (string.IsNullOrEmpty(part1) || x.GRNNo.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }
                    case "RefDcNo":
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
                                (string.IsNullOrEmpty(part1) || x.PartyDC.StartsWith(part1)) &&
                                (string.IsNullOrEmpty(part2) || x.Suffix.Contains(part2))
                            );
                        }
                    case "PartyDcNo":
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
                                (string.IsNullOrEmpty(part1) || x.PartyDC.StartsWith(part1)) &&
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
                                x.SubConGRNSubs.Any(s =>
                                    (string.IsNullOrEmpty(part1) || s.RouteCardSubs.RouteCard.RCNo.StartsWith(part1)) &&
                                    (string.IsNullOrEmpty(part2) || s.RouteCardSubs.RouteCard.Suffix.Contains(part2))
                                )
                            );
                        }


                    case "ItemCode":
                        return query.Where(x => x.SubConGRNSubs
                            .Any(s => s.Item.ItemCode.Contains(val)));

                    case "ItemName":
                        return query.Where(x => x.SubConGRNSubs
                            .Any(s => s.Item.ItemName.Contains(val)));
                    case "Vendor":
                        return query.Where(x => x.vendor.VendorName.Contains(value.ToString()));

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(val));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x => x.GRNDateNow >= fromDate);
                        break;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x => x.GRNDateNow <= toDate);
                        break;
                }

                return query;
            }

            private static IQueryable<SubConGRN> ApplyStatusFilter(
                IQueryable<SubConGRN> query, string status)
            {
                return status switch
                {
                    "Completed" => query.Where(x => x.GRNTally == true),
                    "Pending" => query.Where(x => x.GRNTally == false),
                    "Cancelled" => query.Where(x => x.Cancel == true),
                    "Short Closed" => query.Where(x => x.ShortClose == true),
                    _ => query
                };
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteSubconGRNAsync(int GRNId, int screenCode)
        {
            try
            {
                var grn = await _unitOfWork.SubConGRNs
                                .GetQueryable()
                                .Include(e => e.SubConGRNSubs)
                                .Where(e => e.GRNId == GRNId).FirstOrDefaultAsync();

                if (grn == null)
                    return (true, "Subcontract GRN can be safely deleted.");

                var grnSubIds = grn.SubConGRNSubs
                    .Select(es => es.GRNSubId)
                    .ToList();

                bool hasSCN = await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .AnyAsync(qs =>
                        qs.RefGRNSubId.HasValue &&
                        grnSubIds.Contains(qs.RefGRNSubId.Value));

                if (hasSCN)
                    return (false, "Cannot delete this Subcontract GRN as a Subcontract SCN exists.");

                if (grn.SubConGRNSubs.Any(es => es.ItemCancel))
                    return (false, "Cannot delete this Subcontract GRN as one or more GRN items are cancelled.");

                if (grn.Cancel || grn.ShortClose)
                    return (false, "Cannot delete this Subcontract GRN as it is Cancelled or Short-Closed.");

                var GRNSubIds = await _unitOfWork.SubConGRNSubs
                               .GetQueryable()
                               .Where(s => s.GRNId == GRNId)
                               .Select(s => s.GRNSubId)
                               .ToListAsync();

                var usedStock = await _unitOfWork.StockAdds.GetQueryable()
                                .Where(sa =>
                                    GRNSubIds.Contains(sa.SubItemRefID) &&
                                    sa.ScreenCode == screenCode &&
                                    sa.BalQty < sa.AddQty)
                                .AnyAsync();

                if (usedStock)
                    return (false, "Cannot delete Subcontract GRN. Some sub-items have already been transacted/issued.");

                return (true, "Subcontract GRN can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteSubconGRNAsync for GRNId: {GRNId}");
                return (false, "Unable to verify item CanDeleteSubconGRNAsync. Please try again or contact support.");
            }
        }


        public async Task<(bool IsValid, string Message)> ValidateDeleteAsync(int jobId, int itemId, decimal qtyReturned, int addStoreId)
        {
            var jobOrderSub = await _unitOfWork.JobOrderSubs
                .GetQueryable()
                .Where(j => j.JobId == jobId && j.ItemId == itemId)
                .FirstOrDefaultAsync();

            if (jobOrderSub != null)
            {
                if (jobOrderSub.BalQty < qtyReturned)
                {
                    return (false, $"Cannot delete. Job Order balance ({jobOrderSub.BalQty}) is less than returned qty ({qtyReturned}).");
                }
            }

            return (true, "OK");
        }

        public async Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(int screenCode, int storeId, int itemId, int subItemRefId)
        {
            try
            {
                return await _stockManagerService.GetQtyBalQtyByStockAddAsync(screenCode, storeId, itemId, subItemRefId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching stock details for Screen Code: {screenCode}, SubItemRefId: {subItemRefId}");
                throw new InvalidOperationException("Failed to retrieve SCNGen sub-item stock details.");
            }
        }

        public async Task<List<int>> GetDistinctRcSubIdsWithPendingIssueAsync()
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                 .GetQueryable()
                 .Where(x => x.TransType == "Out"
                          && x.BalQty > 0
                          && x.RcSubId.HasValue
                          && _unitOfWork.SubConDCOutSubs.GetQueryable().Any(y =>
                                 y.DcId == x.DcId &&
                                 y.TransType == "In" &&
                                 y.RefPoSubId == null))
                 .Select(x => x.RcSubId.Value)
                 .Distinct()
                 .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching distinct RcSubIds with pending issued quantity");
                throw new InvalidOperationException("Failed to retrieve pending Route Card issue details. Please try again.", ex);
            }
        }
        public async Task<List<int>> GetDistinctRcSubIdsWithPendingIssueDcAsync()
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                 .GetQueryable()
                 .Where(x => x.TransType == "Out"
                          && x.BalQty > 0
                          && x.RcSubId.HasValue)
                 .Select(x => x.RcSubId.Value)
                 .Distinct()
                 .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching distinct RcSubIds with pending issued quantity");
                throw new InvalidOperationException("Failed to retrieve pending Route Card issue details. Please try again.", ex);
            }
        }
        public async Task<List<int>> GetDistinctPOSubIdsWithPendingIssueAsync()
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                     .GetQueryable()
                     .Where(x => x.TransType == "In"
                              && x.BalQty > 0
                              && x.RefPoSubId.HasValue
                             ) // filter by vendor
                     .Select(x => x.RefPoSubId.Value)
                     .Distinct()
                     .ToListAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching distinct RcSubIds with pending issued quantity");
                throw new InvalidOperationException("Failed to retrieve pending Route Card issue details. Please try again.", ex);
            }
        }
        public async Task<List<int>> GetDistinctDCSubIdsWithPendingIssueAsync()
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                     .GetQueryable()
                     .Where(x => x.TransType == "In"
                              && x.BalQty > 0
                              && x.RefPoSubId.HasValue
                             ) // filter by vendor
                     .Select(x => x.RefPoSubId.Value)
                     .Distinct()
                     .ToListAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching distinct RcSubIds with pending issued quantity");
                throw new InvalidOperationException("Failed to retrieve pending Route Card issue details. Please try again.", ex);
            }
        }


        public async Task<List<Dictionary<string, object>>> GetAllOpenRcDetailsAsync(int? custId)
        {

            var result = new List<Dictionary<string, object>>();

            try
            {
                bool powise =await IsPOWiseSubConDcOutEnabledAsync();


                List<int> rcSubIds;

                if (powise)
                {
                    rcSubIds = await GetDistinctRcSubIdsWithPendingIssueAsync();
                }
                else
                {
                    rcSubIds = await GetDistinctRcSubIdsWithPendingIssueDcAsync();
                }

                if (!rcSubIds.Any())
                    return result;

                var routeCardSubs = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .Include(x => x.IncomingItem)
                    .Include(x => x.Process)
                    .Include(x => x.Machine)
                    .Include(x => x.RouteCard)
                        .ThenInclude(r => r.CostCenter)
                    .Where(x => rcSubIds.Contains(x.RCSubId))
                    .ToListAsync();

                var groupedData = routeCardSubs
                    .GroupBy(x => new { x.ItemIdIn, x.RCSubId })
                    .Select(g =>
                    {
                        var first = g.First();

                        var seqNo = first.SeqNo;

                        var utility = seqNo == 1
                            ? (first.RouteCard?.RMWeight ?? 1)
                            : 1;

                        return new
                        {
                            ItemId = g.Key.ItemIdIn,
                            RefRcSubId = g.Key.RCSubId,

                            Qty = g.Sum(x => x.IssuedQty),

                            SeqNo = seqNo,
                            Utility = utility,

                            ItemCode = first.IncomingItem?.ItemCode ?? string.Empty,
                            ItemName = first.IncomingItem?.ItemName ?? string.Empty,
                            MeasureUnit = first.IncomingItem?.MeasureUnit ?? string.Empty,
                            CategoryCode = first.IncomingItem?.CategoryCode,
                            Category = first.IncomingItem?.Category?.CategoryName ?? string.Empty,

                            RCNo = first.RouteCard != null
                                ? $"{first.RouteCard.RCNo}{first.RouteCard.Suffix}"
                                : string.Empty,

                            RCDt = first.RouteCard?.RCDate,
                            Suffix = first.RouteCard?.Suffix,

                            ProcessId = first.ProcessId,
                            ProcessName = first.Process?.ProcessName ?? string.Empty,

                            MachineId = first.MachineId,
                            MachineName = first.Machine?.MachineName ?? string.Empty,

                            CostCenterId = first.RouteCard?.CostId,
                            CostCenter = first.RouteCard?.CostCenter?.ProjectNo ?? string.Empty
                        };
                    }).ToList();

                var itemIds = groupedData
                    .Where(x => x.ItemId.HasValue)
                    .Select(x => x.ItemId!.Value)
                    .Distinct()
                    .ToList();

                var unitPriceDict = await GetBulkLastUnitPricesAsync(itemIds, custId);

                foreach (var rcSub in groupedData)
                {
                    result.Add(new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["RefRcSubId"] = rcSub.RefRcSubId,
                        ["RCNo"] = rcSub.RCNo != null ? $"{rcSub.RCNo}{rcSub.Suffix}" : string.Empty,
                        ["RCDt"] = rcSub.RCDt,

                        ["TransType"] = "In",
                        ["SeqNo"] = rcSub.SeqNo,

                        ["ItemId"] = rcSub.ItemId,
                        ["ItemCode"] = rcSub.ItemCode,
                        ["ItemName"] = rcSub.ItemName,
                        ["MeasureUnit"] = rcSub.MeasureUnit,

                        ["CategoryCode"] = rcSub.CategoryCode,
                        ["Category"] = rcSub.Category ?? string.Empty,

                        ["Qty"] = rcSub.Qty,
                        ["Utility"] = rcSub.Utility,

                        ["UnitPrice"] = rcSub.ItemId.HasValue &&
                                        unitPriceDict.TryGetValue(rcSub.ItemId.Value, out var rate)
                                            ? rate
                                            : 0m,

                        ["ProcessId"] = rcSub.ProcessId,
                        ["ProcessName"] = rcSub.ProcessName,
                        ["MachineId"] = rcSub.MachineId,
                        ["MachineName"] = rcSub.MachineName,

                        ["CostCenterId"] = rcSub.CostCenterId,
                        ["CostCenter"] = rcSub.CostCenter
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching open Route Card details");
                throw new InvalidOperationException("Failed to retrieve open Route Card details. Please try again.", ex);
            }
        }


        public async Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int? custId)
        {
            var result = new Dictionary<int, decimal>();

            try
            {
                foreach (var itemId in itemIds.Distinct())
                {
                    decimal rate = 0;

                    rate = await (from qs in _unitOfWork.SubConGRNSubs.GetQueryable()
                                  join q in _unitOfWork.SubConGRNs.GetQueryable()
                                      on qs.GRNId equals q.GRNId
                                  where qs.ItemId == itemId && q.VendorCode == custId
                                  orderby q.GRNId descending
                                  select qs.UnitPrice)
                                  .FirstOrDefaultAsync();

                    if (rate == 0)
                    {
                        rate = await (from qs in _unitOfWork.SubConGRNSubs.GetQueryable()
                                      where qs.ItemId == itemId
                                      orderby qs.GRNSubId descending
                                      select qs.UnitPrice)
                                      .FirstOrDefaultAsync();
                    }

                    if (rate == 0)
                    {
                        rate = await (from isub in _unitOfWork.ItemSubs.GetQueryable()
                                      where isub.ItemId == itemId
                                            && isub.CustomerId == custId
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
                await _logs.LogDeveloperError(ex, $"Error fetching bulk last unit prices for CustId: {custId}");
                throw new InvalidOperationException("Failed to fetch last unit prices. Please try again.", ex);
            }
        }

        public async Task<List<Dictionary<string, object>>> GetAllProductionIssuedItemsAsync()
        {
            try
            {
                var data = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(i => i.BalQty > 0 && i.TransType == "Out")
                    .Select(j => new
                    {
                        j.DcSubId,
                        j.RcSubId,

                        RCNo = j.ComponentRouteCardSub.RouteCard.RCNo,
                        RCSuffix = j.ComponentRouteCardSub.RouteCard.Suffix,
                        RCDate = j.ComponentRouteCardSub.RouteCard.RCDate,

                        IssueNo = j.SubConDcOut.DcNo,
                        IssueSuffix = j.SubConDcOut.Suffix,
                        IssueDate = j.SubConDcOut.DcDate,

                        j.ItemId,
                        ItemCode = j.Item.ItemCode,
                        ItemName = j.Item.ItemName,
                        MeasureUnit = j.Item.MeasureUnit,

                        CategoryCode = j.Item.CategoryCode,
                        CategoryName = j.Item.Category.CategoryName,

                        j.BalQty,
                        j.UnitPrice,

                        j.ProcessId,
                        ProcessName = j.Process.ProcessName,

                        j.MachineId,
                        MachineName = j.Machine.MachineName,

                        CostCenterId = j.CostId,
                        ProjectNo = j.CostCenter.ProjectNo
                    })
                    .ToListAsync();

                return data.Select(r => new Dictionary<string, object>
                {

                    ["Selected"] = false,

                    ["TransType"] = "Out",

                    ["IssueSubId"] = r.DcSubId,
                    ["RefRcSubId"] = r.RcSubId,

                    ["RCNo"] = $"{r.RCNo}{r.RCSuffix}",
                    ["RCDt"] = r.RCDate,

                    ["IssueNo"] = $"{r.IssueNo}{r.IssueSuffix}",
                    ["IssueDt"] = r.IssueDate,

                    ["ItemId"] = r.ItemId,
                    ["ItemCode"] = r.ItemCode,
                    ["ItemName"] = r.ItemName,
                    ["MeasureUnit"] = r.MeasureUnit,

                    ["CategoryCode"] = r.CategoryCode,
                    ["Category"] = r.CategoryName ?? string.Empty,

                    ["Qty"] = r.BalQty,
                    ["UnitPrice"] = r.UnitPrice,

                    ["ProcessId"] = r.ProcessId,
                    ["ProcessName"] = r.ProcessName,

                    ["MachineId"] = r.MachineId,
                    ["MachineName"] = r.MachineName,

                    ["CostCenterId"] = r.CostCenterId,
                    ["CostCenter"] = r.ProjectNo
                }).ToList();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error fetching Production Issue items");
                throw new InvalidOperationException(
                    "Failed to retrieve open Production Issue items. Please try again."
                );
            }
        }

        public async Task<bool> DeleteByDcsubConIncomingSubIdAsync(int returnSubId, int screenCode)
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var SubConGRNs = await _unitOfWork.SubConGRNs
                    .GetQueryable()
                    .Include(e => e.SubConGRNSubs)
                    .Where(e => e.SubConGRNSubs
                        .Any(s => s.GRNSubId == returnSubId))
                    .FirstOrDefaultAsync();

                if (SubConGRNs == null)
                    return false;

                var subItem = SubConGRNs.SubConGRNSubs
                    .FirstOrDefault(s => s.GRNSubId == returnSubId);

                if (subItem == null)
                    return false;

                if (subItem.GRNSubId > 0)
                {
                    if (subItem.TransType == "In")
                    {
                        await RollbackTracksAddIssueQtyAsync(subItem.GRNSubId);
                        await DeleteStockAddAsync(subItem.GRNSubId, subItem.ItemId.Value, screenCode, SubConGRNs.GRNNo);
                    }
                   
                }
                if ((!subItem.RefPoSubId.HasValue || subItem.RefPoSubId <= 0)
                  && (!subItem.RefRcSubId.HasValue || subItem.RefRcSubId <= 0))
                {
                    if (subItem.TransType == "Out")
                    {
                        await AdjustDcOutgoingItemBalanceAsync(subItem.RefDcSubId ?? 0, subItem.Qty ?? 0, 0, "SubContract Dc Incoming  Delete");

                        // First get GRNId from selected GRNSubId
                        var grnId = await _unitOfWork.SubConGRNSubs
                            .GetQueryable()
                            .Where(x => x.GRNSubId == returnSubId)
                            .Select(x => x.GRNId)
                            .FirstOrDefaultAsync();

                        if (grnId > 0)
                        {
                            // Get ALL GRNSubIds under that GRNId
                            var grnSubIds = await _unitOfWork.SubConGRNSubs
                                .GetQueryable()
                                .Where(x => x.GRNId == grnId)
                                .Select(x => x.GRNSubId)
                                .ToListAsync();

                            // Get matching tracks
                            var subConGRNTracks = await _unitOfWork.SubConGRNTracks
                                .GetQueryable()
                                .Where(j =>
                                    grnSubIds.Contains(j.RefGRNSubId) &&
                                    j.RefDCSubId == subItem.RefDcSubId && j.ItemIdOut ==subItem.ItemId)
                                .ToListAsync();

                            // Delete tracks
                            foreach (var track in subConGRNTracks)
                            {
                                await _unitOfWork.SubConGRNTracks.DeleteAsync(track);
                            }
                        }
                    }
                }

                if (subItem.RefPoSubId > 0)
                  await AdjustPoSubBalanceAsync(subItem.RefPoSubId.Value, subItem.Qty ?? 0, 0, "Production Return Assy Delete");

                await _unitOfWork.SubConGRNSubs.DeleteAsync(subItem);
                await _unitOfWork.SaveAsync();

                var remaining = await _unitOfWork.SubConGRNSubs
                  .GetQueryable()
                  .Where(x => x.GRNId == SubConGRNs.GRNId)
                  .OrderBy(x => x.SlNo)
                  .ToListAsync();

                int slno = 1;
                foreach (var item in remaining)
                {
                    item.SlNo = slno++;
                }

                await _unitOfWork.SaveAsync();
               
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteProdAssyReturnByReturnIdAsync(int returnId, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var productionReturnComp = await _unitOfWork.SubConGRNs
                    .GetQueryable()
                    .Include(e => e.SubConGRNSubs)
                    .FirstOrDefaultAsync(e => e.GRNId == returnId);

                if (productionReturnComp == null)
                    return false;

                var changes = new StringBuilder();

                foreach (var sub in productionReturnComp.SubConGRNSubs)
                {
                    if (!productionReturnComp.Return && !productionReturnComp.Rejection)
                    {
                        if (sub.GRNSubId > 0)
                        {
                            await RollbackTracksAddIssueQtyAsync(sub.GRNSubId);
                        }
                        if ((!sub.RefPoSubId.HasValue || sub.RefPoSubId <= 0)
                          && (!sub.RefRcSubId.HasValue || sub.RefRcSubId <= 0))
                        {
                            if (sub.TransType == "In")

                                await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId ?? 0, sub.Qty ?? 0, 0, "SubContract Dc Incoming  create   ");

                        }

                        //if (sub.RefPoSubId > 0)
                        //    await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value, sub.ItemId.Value, sub.Qty ?? 0, 0, "Production Return Assy Delete");
                        //await AdjustJobOrderBalanceAsync(sub.RefJobOrderId, sub.QtyReturned ?? 0, 0, "Production Return Assy Delete");

                    }
                    else
                    {
                        await AdjustProductionCompIssueItemBalanceAsync(sub.RefPoSubId, 0, sub.Qty ?? 0, "Production Return Assy Delete");

                        //if (sub.RefPoSubId > 0)
                        //    await AdjustMfgPoSubItemBalanceAsync(sub.RefPoSubId.Value, sub.ItemId.Value, sub.Qty ?? 0, 0, "Production Return Assy Delete");
                    }
                    await DeleteStockAddAsync(sub.GRNSubId, sub.ItemId.Value, screenCode, productionReturnComp.GRNNo);
                }


                var ProductionReturnComp = await _unitOfWork.SubConGRNs.GetAsync(returnId);
                await _unitOfWork.SubConGRNs.DeleteAsync(ProductionReturnComp);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                await _logs.LogUserAction(UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Production Return Component",
                    action: $"Deleted Return no: {productionReturnComp.GRNNo}",
                    additionalInfo: $"Return Id: {productionReturnComp.GRNId}\n{changes}"
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


        public async Task<SubConGRNVM?> GetSubcontractGRNByGRNIdAsync(int grnId)
        {
            try
            {
                var entity = await _unitOfWork.SubConGRNs
                    .GetQueryable()
                    .AsNoTracking()
                    .Include(v => v.vendor)
                    .Include(q => q.AddStore)
                    .Include(q => q.SubConGRNSubs)
                        .ThenInclude(s => s.Item)
                    .Include(q => q.SubConGRNSubs)
                        .ThenInclude(s => s.CostCenter)
                    .Include(q => q.SubConGRNSubs)
                        .ThenInclude(s => s.Process)
                    .Include(q => q.SubConGRNSubs)
                        .ThenInclude(s => s.Machine)
                    .Include(q => q.SubConGRNSubs)
                        .ThenInclude(s => s.RouteCardSubs)
                    .Include(q => q.SubConGRNSubs)
                        .ThenInclude(s => s.PurchPoSub)
                            .ThenInclude(p => p.PurchPo)
                    .Include(q => q.SubConGRNSubs)
                        .ThenInclude(d => d.SubConDcOutSub)
                            .ThenInclude(s => s.SubConDcOut)
                    .Include(q => q.SubConGRNSubs)
                        .ThenInclude(s => s.RouteCardSubs)
                            .ThenInclude(s => s.RouteCard)
                    .FirstOrDefaultAsync(q => q.GRNId == grnId);

                if (entity == null)
                    return null;

                // ✅ ORDER BY SlNo
                if (entity.SubConGRNSubs != null)
                {
                    entity.SubConGRNSubs = entity.SubConGRNSubs
                                                .OrderBy(x => x.SlNo)
                                                .ToList();
                }

                var vm = _mapper.Map<SubConGRNVM>(entity);
                return vm;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"GetSubcontractGRNByGRNIdAsync({grnId})");
                return null;
            }
        }

        public async Task<string> GetSubconGRNNoAsync(string suffix)
        {
            try
            {
                var lastProdIssue = await _unitOfWork.SubConGRNs
                    .GetQueryable()
                    .Where(q => q.Suffix == suffix)
                    .OrderByDescending(q => q.GRNNo)
                    .FirstOrDefaultAsync();

                int nextNumber = 1;
                if (lastProdIssue != null)
                {
                    var parts = lastProdIssue.GRNNo.Split('/');
                    if (int.TryParse(parts[0], out int lastNumber))
                        nextNumber = lastNumber + 1;
                }

                return $"{nextNumber}";
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error generating Subcontract GRNNo for suffix: {suffix}");
                throw new InvalidOperationException(ex.Message);
            }
        }

        public async Task DeleteAndResequenceAsync(SubConGRNSubVM subitem, SubConGRNVM productionReturnCompVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            var changes = new StringBuilder();

            try
            {
                if (subitem.GRNSubId > 0)
                {
                    var entity = await _unitOfWork.SubConGRNSubs.GetAsync(subitem.GRNSubId);
                    if (entity == null)
                        throw new InvalidOperationException("Sub item not found.");



                    if (!productionReturnCompVM.Rejection && !productionReturnCompVM.IsReturn)
                    {
                        if (subitem.GRNSubId > 0)
                            await RollbackTracksAddIssueQtyAsync(subitem.RefDcSubId.Value);

                        //await AdjustJobOrderBalanceAsync(subitem.RefJobOrderId, subitem.QtyReturned ?? 0, 0, "Production return Assy Delete");
                    }
                    else
                    {
                        await AdjustProductionCompIssueItemBalanceAsync(subitem.RefDcSubId, subitem.Qty.GetValueOrDefault(), 0, "Production Return Component Delete");

                        //await AdjustMfgPoSubItemBalanceAsync(subitem.RefJobOrderId.Value, subitem.ItemId.Value, subitem.QtyReturned ?? 0, 0, "Production Return Assy Delete");
                    }
                    await DeleteStockAddAsync(subitem.GRNSubId, subitem.ItemId.Value, screenCode, productionReturnCompVM.GRNNo);


                    await _unitOfWork.SubConGRNSubs.DeleteAsync(entity.GRNSubId);
                    await _unitOfWork.SaveAsync();

                    await _logs.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "Production Return Assembly",
                        $"Deleted Item: {subitem.ItemCode}",
                        $"Return No: {productionReturnCompVM?.GRNNo}"
                    );
                }
                else
                {
                    productionReturnCompVM.SubConGRNSubVMs.Remove(subitem);
                    return;
                }

                var remaining = await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .Where(x => x.GRNId == productionReturnCompVM.GRNId)
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

        public async Task<List<SubConGRNSub>> GetSubcontractGRNSubByGRNIdAsync(int grnId)
        {
            try
            {
                var subs = await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.GRNId == grnId)
                    .OrderBy(s => s.SlNo)
                    .ToListAsync();

                return subs;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Subcontract GRN items for GRNId: {grnId}");
                throw new InvalidOperationException("Failed to retrieve Subcontract GRN sub-items. Please try again.");
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

        public async Task<SubConGRNSubVM?> GetProductionReturnSubItemDetailByReturnSubIdAsync(int returnSubId)
        {
            try
            {
                return await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .Where(q => q.GRNSubId == returnSubId)
                    .Select(q => new SubConGRNSubVM
                    {
                        Qty = q.Qty.GetValueOrDefault(),
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
        public async Task ValidateSubConDcoutBalanceBeforeRevertAsync(SubConGRNSub sub, SubConGRNVM SubConGRNVMs)
        {
            try
            {
                bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();
                if (sub.RefDcSubId.GetValueOrDefault() > 0)
                {
                    var entity = await _unitOfWork.SubConDCOutSubs.GetAsync(sub.RefDcSubId ?? 0);
                    if (entity == null)
                        throw new InvalidOperationException($"Labour SCN not found for RefDcsubId: {sub.RefDcSubId}");

                    if (entity.BalQty < sub.Qty)
                    {
                        throw new InvalidOperationException(
                            $"Cannot revert because Sub Con GRN balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                        );
                    }
                }
                if (!SubConGRNVMs.IsReturn)
                {
                    if (sub.RefDcSubId.GetValueOrDefault() > 0 && SubConGRNVMs.IsWithoutPoDc)
                    {
                        var entity = await _unitOfWork.SubConDCOutSubs.GetAsync(sub.RefDcSubId ?? 0);
                        if (entity == null)
                            throw new InvalidOperationException($"SubConGRN not found for RefDcSunId: {sub.RefDcSubId}");

                        if (entity.BalQty < sub.Qty)
                        {
                            throw new InvalidOperationException(
                                $"Cannot revert because Sub Con GRN balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                            );
                        }
                    }
                    if (PoWiseDcOut)
                    {
                        if (sub.RefPoSubId.GetValueOrDefault() > 0)
                        {
                            var entity = await _unitOfWork.PurchPoSubs.GetAsync(sub.RefPoSubId ?? 0);

                            if (entity == null)
                                throw new InvalidOperationException($"SubCon PurchPo not found for RefDcOutSubId: {sub.RefPoSubId}");

                            if (entity.BalQty < sub.Qty)
                            {
                                throw new InvalidOperationException(
                                    $"Cannot revert because SubConGRN PurchPo balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                                );
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching ValidateSubConDcoutBalanceBeforeRevertAsync detail for SubCon GRNID: {SubConGRNVMs.GRNId}");
                throw new InvalidOperationException("Failed to retrieve ValidateSubConDcoutBalanceBeforeRevertAsync details.");
            }


        }

        public async Task UpdatedCancelStatusAndAddOrRevertQtyPoWiseAync(SubConGRNVM dcVM, int screenCode)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;
                var currentUser = await _currentUserService.GetUsernameAsync();
                bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();
                var existingDC = await _unitOfWork.SubConGRNs.GetAsync(dcVM.GRNId);
                if (existingDC == null)
                    throw new InvalidOperationException("SubCon GRN not found.");

                var subs = await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .Include(s => s.Item)
                    .Where(s => s.GRNId == dcVM.GRNId)
                    .ToListAsync();

                if (!dcVM.Cancel)
                {
                    foreach (var sub in subs)
                    {
                        await ValidateSubConDcoutBalanceBeforeRevertAsync(sub, dcVM);
                       
                    }
                }

                existingDC.Cancel = dcVM.Cancel;
                existingDC.CancelReason = dcVM.CancelReason;
                await _unitOfWork.SubConGRNs.UpdateAsync(existingDC);
                await _unitOfWork.SaveAsync();

                foreach (var sub in subs)
                {

                    decimal newAcceptedQty, oldAcceptedQty = 0;
                    bool rejRetrack = false;

                    if (existingDC.Cancel)
                    {
                        
                        if (sub.GRNSubId > 0)
                            await RollbackTracksAddIssueQtyAsync(sub.GRNSubId);


                        if (sub.RefRcSubId.GetValueOrDefault() > 0)
                        {
                            await AdjustRcInItemBalanceAsync(sub.RefRcSubId, sub.Qty ?? 0, 0, "SubContract GRN Delete");

                            var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

                            var issueSubs = await _unitOfWork.SubConDCOutSubs
                                               .GetQueryable()
                                               .Where(x => issueIds.Contains(x.DcId) 
                                                  && x.TransType == "In")
                                               .OrderBy(x => x.DcSubId)
                                               .ToListAsync();

                            if (issueSubs.Any())
                            {
                                foreach (var Grnsub in issueSubs)
                                {
                                    await AdjustDcOutgoingItemBalanceAsync(Grnsub.DcSubId, sub.Qty ?? 0, 0, "SubContract GRN Create");
                                }

                            }

                        }

                        if (sub.TransType == "In" && sub.RefDcSubId.GetValueOrDefault() > 0 && !PoWiseDcOut)
                        {
                            await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId.Value, sub.Qty ?? 0, 0, "SubContract GRN Delete");
                        }

                        if (PoWiseDcOut)
                        {

                            if (sub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(sub.RefPoSubId, sub.Qty ?? 0, 0, "Subcontract GRN Delete");
                            }
                        }
                        await DeleteStockAddAsync(sub.GRNSubId, sub.ItemId.Value, screenCode, existingDC.GRNNo);

                    }
                    else
                    {
                        if (sub.RefRcSubId.GetValueOrDefault() > 0)
                        {
                            await CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.GRNSubId, sub.RefRcSubId.Value,sub.RefPoSubId,
                                sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now);

                            await AdjustRcInItemBalanceAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "SubContract GRN Create");

                            var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

                            var issueSubs = await _unitOfWork.SubConDCOutSubs
                                               .GetQueryable()
                                               .Where(x => issueIds.Contains(x.DcId) &&
                                                 x.TransType == "In")
                                               .OrderBy(x => x.DcSubId)
                                               .ToListAsync();

                            if (issueSubs.Any())
                            {
                                foreach (var Grnsub in issueSubs)
                                {
                                    await AdjustDcOutgoingItemBalanceAsync(Grnsub.DcSubId, 0, sub.Qty ?? 0, "SubContract GRN Create");
                                }

                            }

                            if (sub.TransType == "In" && sub.RefDcSubId > 0)
                            {
                                await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
                            }
                        }
                        else if (sub.TransType == "In" && sub.RefDcSubId.GetValueOrDefault() > 0 && !PoWiseDcOut)
                        {
                            await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.GRNSubId, sub.RefDcSubId.Value,
                                sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now, existingDC.IsManual, dcVM.SubConGRNSubVMs,sub?.RefPoSubId);

                            await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
                        }

                        if (PoWiseDcOut)
                        {
                            if (sub.TransType == "In" && sub.RefRcSubId==null)
                            {
                                if (!existingDC.IsManual)
                                {
                                    await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(sub.GRNSubId, sub.RefPoSubId ?? 0,
                                      sub.ItemId ?? 0, sub.Qty ?? 0, screenCode, currentUser, now, sub.RefDcSubId ?? 0);
                                }
                                else if (existingDC.IsManual)
                                {
                                    await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.GRNSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, dcVM.SubConGRNSubVMs, existingDC.Return);

                                    if (sub.RefDcSubId.GetValueOrDefault() > 0 && existingDC.IsWithoutPoDc)
                                    {
                                        await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
                                    }
                                }

                            }

                            if (sub.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(sub.RefPoSubId, 0, sub.Qty ?? 0, "Subcontract GRN Creation");
                            }

                        }

                        if (sub.TransType == "In")
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, existingDC.AddStoreId.Value, sub.Qty.GetValueOrDefault(),
                                sub.UnitPrice, null, screenCode, sub.GRNSubId, existingDC.GRNNo, existingDC.GRNDate, null, sub.RefRcSubId);
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
        public async Task<SubConGRNVM> UpsertSubConGRNAsync(SubConGRNVM subConGRNVMs, int screenCode)
        {
            if (subConGRNVMs == null)
                throw new ArgumentNullException(nameof(subConGRNVMs));

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();
            var changes = new StringBuilder();

           bool PoWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                SubConGRN entity;

                if (subConGRNVMs.GRNId == 0)
                {
                    entity = _mapper.Map<SubConGRN>(subConGRNVMs);

                    var nextNumber = await _unitOfWork.SubConGRNs.GetLastGRNNoAsync(entity.Suffix);
                    entity.GRNNo = nextNumber;
                    entity.Return = subConGRNVMs.IsReturn;
                    entity.CreatedBy = currentUser;
                    entity.CreatedDate = now;

                    entity.SubConGRNSubs = subConGRNVMs.SubConGRNSubVMs
                        .Select(s => _mapper.Map<SubConGRNSub>(s))
                        .ToList();

                    await _unitOfWork.SubConGRNs.CreateAsync(entity);
                    await _unitOfWork.SaveAsync();

                    foreach (var sub in entity.SubConGRNSubs)
                    {
                        if (!entity.Rejection && !entity.Return)
                        {

                            if (sub.RefRcSubId.GetValueOrDefault() > 0)
                            {
                                await CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.GRNSubId, sub.RefRcSubId.Value,sub.RefPoSubId,
                                    sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now);

                                await AdjustRcInItemBalanceAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "SubContract GRN Create");

                          
                                    var issueIds = await GetIssueIdsByRcSubIdAsync(sub.RefRcSubId ?? 0);

                                    var issueSubs = await _unitOfWork.SubConDCOutSubs
                                                       .GetQueryable()
                                                       .Where(x => issueIds.Contains(x.DcId) &&
                                                           (x.BalQty ?? 0) > 0
                                                           && x.TransType == "In")
                                                       .OrderBy(x => x.DcSubId)
                                                       .ToListAsync();

                                    if (issueSubs.Any())
                                    {
                                        foreach (var Grnsub in issueSubs)
                                        {
                                            await AdjustDcOutgoingItemBalanceAsync(Grnsub.DcSubId, 0, sub.Qty ?? 0, "SubContract GRN Create");
                                        }

                                    }

                                if (sub.TransType == "In" && sub.RefDcSubId >0)
                                {
                                    await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
                                }
                            }
                            else if (sub.TransType == "In" && sub.RefDcSubId.GetValueOrDefault() > 0 && !PoWiseDcOut)
                            {
                                await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(sub.GRNSubId, sub.RefDcSubId.Value,
                                    sub.ItemId.Value, sub.Qty.GetValueOrDefault(), screenCode, currentUser, now, entity.IsManual, subConGRNVMs.SubConGRNSubVMs,sub?.RefPoSubId);

                                await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
                            }
          
                            if(PoWiseDcOut)
                            {
                                if (sub.TransType == "In" && sub.RefRcSubId == null)
                                {
                                    if (!entity.IsManual)
                                    {
                                        await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(sub.GRNSubId, sub.RefPoSubId ?? 0,
                                          sub.ItemId ?? 0, sub.Qty ?? 0, screenCode, currentUser, now, sub.RefDcSubId ?? 0);
                                    }
                                    else if (entity.IsManual)
                                    {
                                        await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.GRNSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, subConGRNVMs.SubConGRNSubVMs, entity.Return);
                                       
                                        if(sub.RefDcSubId.GetValueOrDefault() > 0 && entity.IsWithoutPoDc)
                                        {
                                            await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId.Value, 0, sub.Qty ?? 0, "SubContract GRN Create");
                                        }
                                    }
                                 
                                }

                                if (sub.RefPoSubId.GetValueOrDefault() > 0)
                                {
                                    await AdjustPoSubBalanceAsync(sub.RefPoSubId, 0, sub.Qty??0, "Subcontract GRN Creation");
                                }

                            }
                        }
                        else
                        {
                          //  await AdjustProductionCompIssueItemBalanceAsync(sub.RefDcSubId, 0, sub.Qty ?? 0, "SubContract GRN Create");

                            if (sub.RefRcSubId.HasValue && sub.RefRcSubId > 0)
                            {
                                await RevertRCSubIssuedToBalQtyAsync(sub.RefRcSubId, 0, sub.Qty ?? 0, "SubContract GRN Return/Rejection");
                            }
                          
                            if (sub.TransType == "In")
                            {
                                
                                if (entity.IsManual)
                                {
                                    await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(sub.GRNSubId, sub.ItemId.Value, sub.Qty.Value, currentUser, now, subConGRNVMs.SubConGRNSubVMs, entity.Return);
                                   
                                  
                                }
                                   
                            }
                            
                        }

                        if (sub.TransType == "In")
                        {
                            await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId.Value, entity.AddStoreId.Value, sub.Qty.GetValueOrDefault(),
                                sub.UnitPrice, null, screenCode, sub.GRNSubId, entity.GRNNo, entity.GRNDate, null, sub.RefRcSubId);
                        }
                    }

                    if (entity.IsNoInspection)
                    {
                        await CreateProuctionSCNFromProductionGRNCompAsync(entity);
                    }

                    changes.AppendLine("SubContract GRN Created Successfully.");
                }
                else
                {
                    // ---------------- UPDATE ----------------
                    entity = await _unitOfWork.SubConGRNs.GetQueryable()
                        .Include(q => q.SubConGRNSubs)
                        .Include(q => q.SubConGRNTracks)
                        .FirstOrDefaultAsync(q => q.GRNId == subConGRNVMs.GRNId)
                        ?? throw new InvalidOperationException("Subcontract GRN not found.");

                    var parentChanges = GetPropertyChanges(entity, subConGRNVMs);
                    if (!string.IsNullOrEmpty(parentChanges))
                        changes.AppendLine("Parent Changes:\n" + parentChanges);

                    _mapper.Map(subConGRNVMs, entity);
                    entity.ModifiedBy = currentUser;
                    entity.ModifiedDate = now;

                    await HandleChildUpdatesAsync(entity, subConGRNVMs.SubConGRNSubVMs, screenCode, currentUser, now, changes);

                    await _unitOfWork.SaveAsync();

                    changes.AppendLine("Subcontract GRN Updated.");
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();
                await LogChangesAsync(changes, subConGRNVMs.GRNId == 0 ? "Subcontract GRN Created" : "Subcontract GRN Updated");

                var savedEntity = await _unitOfWork.SubConGRNs.GetQueryable()
                    .Include(q => q.SubConGRNSubs).ThenInclude(s => s.Item)
                    .Include(q => q.AddStore)
                    .Include(q => q.SubConGRNSubs).ThenInclude(s => s.CostCenter)
                    .Include(q => q.SubConGRNTracks)
                    .FirstOrDefaultAsync(q => q.GRNId == entity.GRNId);

                return _mapper.Map<SubConGRNVM>(savedEntity!);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to upsert Subcontract GRN: {subConGRNVMs.GRNNo}");
                throw new InvalidOperationException("Failed to save Subcontract GRN. Please try again.");
            }
        }
        private async Task HandleChildUpdatesAsync(SubConGRN existingProdReturnComp, List<SubConGRNSubVM> incomingSubVMs, int screenCode, string currentUser, DateTime now, StringBuilder changes)
        {
            bool poWiseDcOut = await IsPOWiseSubConDcOutEnabledAsync();

            var existingSubs = existingProdReturnComp.SubConGRNSubs.ToList();
            var incomingIds = incomingSubVMs.Select(x => x.GRNSubId).ToHashSet();

            // DELETE
            foreach (var sub in existingSubs.Where(x => !incomingIds.Contains(x.GRNSubId)).ToList())
            {
                changes.AppendLine($"Child Deleted - ReturnSubId: {sub.GRNSubId}, Item: {sub.Item?.ItemCode}");

                if (sub.GRNSubId > 0)
                    await RollbackTracksAddIssueQtyAsync(sub.GRNSubId);

                if (!existingProdReturnComp.Rejection && !existingProdReturnComp.Return)
                {
                    if (sub.RefRcSubId.GetValueOrDefault() > 0)
                        await AdjustRcInItemBalanceAsync(sub.RefRcSubId, sub.Qty ?? 0, 0, "SubContract GRN Delete");

                    else if (sub.RefDcSubId.GetValueOrDefault() > 0)
                        await AdjustDcOutgoingItemBalanceAsync(sub.RefDcSubId.Value, sub.Qty ?? 0, 0, "SubContract GRN Delete");

                    if (poWiseDcOut && sub.RefPoSubId.GetValueOrDefault() > 0)
                        await AdjustPoSubBalanceAsync(sub.RefPoSubId, sub.Qty ?? 0, 0, "SubContract GRN Delete");
                }
                else
                {
                    await AdjustProductionCompIssueItemBalanceAsync(sub.RefDcSubId, sub.Qty ?? 0, 0, "Production return Component delete");

                    if (sub.RefRcSubId.GetValueOrDefault() > 0)
                        await RevertRCSubIssuedToBalQtyAsync(sub.RefRcSubId, sub.Qty ?? 0, 0, "Production return Component Return/Rejection Delete");
                }

                await DeleteStockAddAsync(sub.GRNSubId, sub.ItemId ?? 0, screenCode, existingProdReturnComp.GRNNo);

                await _unitOfWork.SubConGRNSubs.DeleteAsync(sub.GRNSubId);
            }

            // ADD / UPDATE
            foreach (var subVM in incomingSubVMs)
            {
                bool isNew = subVM.GRNSubId == 0;

                var entity = isNew
                    ? _mapper.Map<SubConGRNSub>(subVM)
                    : existingSubs.FirstOrDefault(x => x.GRNSubId == subVM.GRNSubId);

                if (entity == null)
                    continue;

                decimal oldQty = entity.Qty ?? 0;

                if (isNew)
                {
                    entity.GRNId = existingProdReturnComp.GRNId;

                    await _unitOfWork.SubConGRNSubs.CreateAsync(entity);

                    await _unitOfWork.SaveAsync();

                    subVM.GRNSubId = entity.GRNSubId;

                    oldQty = 0;

                    changes.AppendLine($"Child Added - ItemCode: {subVM.ItemCode}, Qty: {subVM.Qty}");
                }
                else
                {
                    if (entity.GRNSubId > 0 && entity.TransType == "In")
                        await RollbackTracksAddIssueQtyAsync(entity.GRNSubId);

                    var subChanges = GetPropertyChanges(entity, subVM);

                    if (!string.IsNullOrWhiteSpace(subChanges))
                        changes.AppendLine($"Child Updated - ItemCode: {subVM.ItemCode}\n{subChanges}");

                    _mapper.Map(subVM, entity);
                }

                decimal newQty = entity.Qty ?? 0;

                if (entity.TransType == "In")
                {
                    if (!existingProdReturnComp.Rejection && !existingProdReturnComp.Return)
                    {
                        if (entity.RefRcSubId.GetValueOrDefault() > 0)
                        {
                            await CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(
                                entity.GRNSubId,
                                entity.RefRcSubId.Value,entity.RefPoSubId,
                                entity.ItemId ?? 0,
                                newQty,
                                screenCode,
                                currentUser,
                                now);

                            await AdjustRcInItemBalanceAsync(entity.RefRcSubId, oldQty, newQty, "SubContract GRN");

                            var issueIds = await GetIssueIdsByRcSubIdAsync(entity.RefRcSubId ?? 0);

                            var issueSubs = await _unitOfWork.SubConDCOutSubs
                                               .GetQueryable()
                                               .Where(x => issueIds.Contains(x.DcId) 
                                                   && x.TransType == "In")
                                               .OrderBy(x => x.DcSubId)
                                               .ToListAsync();

                            if (issueSubs.Any())
                            {
                                foreach (var Grnsub in issueSubs)
                                {
                                    await AdjustDcOutgoingItemBalanceAsync(Grnsub.DcSubId, oldQty, newQty, "SubContract GRN Create");
                                }

                            }

                        }

                       if (poWiseDcOut)
                        {
                            if (entity.TransType == "In" && entity.RefRcSubId == null)
                             {


                                if (existingProdReturnComp.IsManual)
                                {
                                    await CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(
                                        entity.GRNSubId,
                                        entity.ItemId ?? 0,
                                        newQty,
                                        currentUser,
                                        now,
                                        incomingSubVMs,
                                        existingProdReturnComp.Return);
                                }
                                else
                                {
                                    await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(
                                        entity.GRNSubId,
                                        entity.RefPoSubId ?? 0,
                                        entity.ItemId ?? 0,
                                        newQty,
                                        screenCode,
                                        currentUser,
                                        now,
                                        entity.RefDcSubId ?? 0);
                                }
                            }
                            if (entity.RefPoSubId.GetValueOrDefault() > 0)
                            {
                                await AdjustPoSubBalanceAsync(
                                    entity.RefPoSubId,
                                    oldQty,
                                    newQty,
                                    "SubContract GRN");
                            }
                        }
                        else if (entity.RefDcSubId.GetValueOrDefault() > 0)
                        {
                            await CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(
                                entity.GRNSubId,
                                entity.RefDcSubId.Value,
                                entity.ItemId ?? 0,
                                newQty,
                                screenCode,
                                currentUser,
                                now,
                                existingProdReturnComp.IsManual,
                                incomingSubVMs,entity?.RefPoSubId);

                            await AdjustDcOutgoingItemBalanceAsync(
                                entity.RefDcSubId.Value,
                                oldQty,
                                newQty,
                                "SubContract GRN");
                        }
                    }
                    else
                    {
                        await AdjustProductionCompIssueItemBalanceAsync(
                            entity.RefDcSubId,
                            oldQty,
                            newQty,
                            "Production Return/Rejection");

                        if (entity.RefRcSubId.GetValueOrDefault() > 0)
                        {
                            await RevertRCSubIssuedToBalQtyAsync(
                                entity.RefRcSubId,
                                oldQty,
                                newQty,
                                "Production Return/Rejection");

                            var issueIds = await GetIssueIdsByRcSubIdAsync(entity.RefRcSubId ?? 0);

                            var issueSubs = await _unitOfWork.SubConDCOutSubs
                                               .GetQueryable()
                                               .Where(x => issueIds.Contains(x.DcId) 
                                                   && x.TransType == "In")
                                               .OrderBy(x => x.DcSubId)
                                               .ToListAsync();

                            if (issueSubs.Any())
                            {
                                foreach (var Grnsub in issueSubs)
                                {
                                    await AdjustDcOutgoingItemBalanceAsync(Grnsub.DcSubId, oldQty, newQty, "SubContract GRN Create");
                                }

                            }
                        }
                    }

                    await _stockManagerService.AddOrUpdateStockAsync(
                        entity.ItemId ?? 0,
                        existingProdReturnComp.AddStoreId ?? 0,
                        newQty,
                        entity.UnitPrice,
                        null,
                        screenCode,
                        entity.GRNSubId,
                        existingProdReturnComp.GRNNo,
                        existingProdReturnComp.GRNDate,
                        null,
                        entity.RefRcSubId);
                }
            }

            await _unitOfWork.SaveAsync();
        }
       
        private async Task AdjustPoSubBalanceAsync(int? refPoSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refPoSubId.HasValue || refPoSubId == 0) return;

                var isOpenPo = await _unitOfWork.PurchPoSubs
                                .GetQueryable()
                                .Where(s => s.PoSubId == refPoSubId)
                                .Join(_unitOfWork.PurchPos.GetQueryable(),
                                        sub => sub.PoId,
                                        po => po.PoId,
                                        (sub, po) => po.IsOpenPO)
                                .FirstOrDefaultAsync();

                if (!isOpenPo)
                {
                    var poSub = await _unitOfWork.PurchPoSubs.GetAsync(refPoSubId.Value);
                    if (poSub == null) return;

                    if (oldQty > 0)
                        poSub.BalQty += oldQty;

                    if (newQty > poSub.BalQty)
                        throw new InvalidOperationException($"{context}: Qty cannot exceed Quote BalQty.");

                    if (newQty > 0)
                        poSub.BalQty -= newQty;

                    await _unitOfWork.PurchPoSubs.UpdateAsync(poSub);
                    await _unitOfWork.SaveAsync();

                    var totalBalQty = await _unitOfWork.PurchPoSubs
                        .GetQueryable()
                        .Where(e => e.PoId == poSub.PoId && !e.ItemCancel)
                        .SumAsync(e => e.BalQty);

                    var po = await _unitOfWork.PurchPos.GetAsync(poSub.PoId);
                    if (po != null)
                    {
                        po.PoTally = (totalBalQty == 0); // Tally only if all BalQty consumed
                        await _unitOfWork.PurchPos.UpdateAsync(po);
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
        private async Task AdjustDcOutgoingItemBalanceAsync(int? refDcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (refDcSubId == 0) return;

                var subConDcOutSub = await _unitOfWork.SubConDCOutSubs.GetQueryable()
                    .Where(j => j.DcSubId == refDcSubId).FirstOrDefaultAsync();

                if (subConDcOutSub == null) return;

                if (oldQty > 0)
                    subConDcOutSub.BalQty += oldQty;

                if (newQty > subConDcOutSub.Qty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

                if (newQty > 0)
                    subConDcOutSub.BalQty -= newQty;



                await _unitOfWork.SubConDCOutSubs.UpdateAsync(subConDcOutSub);
                await _unitOfWork.SaveAsync();


                var totalBalQty = await _unitOfWork.SubConDCOutSubs
                                       .GetQueryable()
                                       .Where(e => e.DcId == subConDcOutSub.DcId)
                                       .SumAsync(e => e.BalQty);

                var compIssue = await _unitOfWork.SubConDCOuts.GetAsync(subConDcOutSub.DcId);
                if (compIssue != null)
                {
                    compIssue.DcTally = (totalBalQty == 0);
                    await _unitOfWork.SubConDCOuts.UpdateAsync(compIssue);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustDcOutgoingItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustDcOutgoingItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to adjust SubContract DC sub balance. Please contact support.");
            }
        }

        private async Task AdjustRcInItemBalanceAsync(int? refRcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refRcSubId.HasValue || refRcSubId == 0)
                    return;

                var routeCardProcess = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.RCSubId == refRcSubId.Value)
                    ?? throw new InvalidOperationException("Route Card Process not found");

              
                if (oldQty > 0)
                {
                    routeCardProcess.IssuedQty += oldQty;
                    routeCardProcess.WipQty -= oldQty;
                }

                if (newQty > routeCardProcess.IssuedQty)
                    throw new InvalidOperationException(
                        $"{context}: Qty cannot exceed available Issued Qty.");

          
                if (newQty > 0)
                {
                    routeCardProcess.IssuedQty -= newQty;
                    routeCardProcess.WipQty += newQty;
                }

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardProcess);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(
                    ex, $"[AdjustRCSubItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex, $"[AdjustRCSubItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException(
                    "Failed to adjust Route Card sub balance. Please contact support.");
            }
        }

        private async Task RevertRCSubIssuedToBalQtyAsync(int? refRcSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refRcSubId.HasValue || refRcSubId == 0)
                    return;

                var routeCardProcess = await _unitOfWork.RouteCardSubs
                    .GetQueryable()
                    .FirstOrDefaultAsync(x => x.RCSubId == refRcSubId.Value)
                    ?? throw new InvalidOperationException("Route Card Process not found");

                if (oldQty > 0)
                {
                    routeCardProcess.IssuedQty += oldQty;
                    routeCardProcess.BalQty -= oldQty;
                }

                if (newQty > routeCardProcess.IssuedQty)
                    throw new InvalidOperationException(
                        $"{context}: Qty cannot exceed available Issued Qty.");

                if (newQty > 0)
                {
                    routeCardProcess.IssuedQty -= newQty;
                    routeCardProcess.BalQty += newQty;
                }

                await _unitOfWork.RouteCardSubs.UpdateAsync(routeCardProcess);
                await _unitOfWork.SaveAsync();
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[RevertRCSubItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[RevertRCSubItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to revert Route Card sub balance. Please contact support.");
            }
        }

        private async Task<int?> GetEffectivePreviousSeqNoAsync(int rcId, int currentSeqNo)
        {
            try
            {
                var prevSeqNos = await _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(x => x.RCId == rcId && x.SeqNo < currentSeqNo)
                    .Select(x => x.SeqNo)
                    .Distinct()
                    .OrderByDescending(x => x)
                    .ToListAsync();

                foreach (var seq in prevSeqNos)
                {
                    bool allSkipped = await _unitOfWork.RouteCardSubs.GetQueryable()
                        .Where(x => x.RCId == rcId && x.SeqNo == seq)
                        .AllAsync(x => x.IsProcessSkip);

                    if (!allSkipped)
                        return seq;
                }

                return null;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error finding previous SeqNo for RCId={rcId}, currentSeqNo={currentSeqNo}");
                throw;
            }
        }

        // Get the MIN NextProcessQty of all non-skipped processes in previous SeqNo
        private async Task<decimal> GetPrevSeqMinNextQtyAsync(int rcId, int prevSeqNo)
        {
            try
            {
                var query = _unitOfWork.RouteCardSubs.GetQueryable()
                    .Where(x => x.RCId == rcId && x.SeqNo == prevSeqNo && !x.IsProcessSkip);

                if (!await query.AnyAsync())
                    return 0;

                return await query.MinAsync(x => x.NextProcessQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error calculating Min NextProcessQty for RCId={rcId}, prevSeqNo={prevSeqNo}");
                throw;
            }
        }

        private async Task DeleteStockAddAsync(int returnSubId, int itemId, int screenCode, string refNo)
        {
            var addIds = await _unitOfWork.StockAdds
                .GetQueryable()
                .Where(s => s.SubItemRefID == returnSubId && s.ItemId == itemId && s.ScreenCode == screenCode && s.RefNo == refNo)
                .Select(s => s.AddId)
                .ToListAsync();

            foreach (var addId in addIds)
            {
                if (addId > 0)
                    await _stockManagerService.DeleteStockAddAsync(addId);
            }
        }


        private async Task RollbackTracksAddIssueQtyAsync(int refReturnSubId)
        {
            var now = DateTime.Now;

            try
            {
                bool PoWiseGrn = await IsPOWiseSubConDcOutEnabledAsync();
                var tracks = await _unitOfWork.SubConGRNTracks
                    .GetQueryable()
                    .Where(x => x.RefGRNSubId == refReturnSubId)
                    .ToListAsync();

                if (!tracks.Any())
                    return;

                foreach (var track in tracks)
                {
                    var issueSubs = await _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .Where(x => x.DcSubId == track.RefDCSubId)
                        .ToListAsync();

                    foreach (var issueSub in issueSubs)
                    {
                        issueSub.BalQty += track.QtyOut.GetValueOrDefault();

                        await _unitOfWork.SubConDCOutSubs.UpdateAsync(issueSub);
                        await _unitOfWork.SaveAsync();

                        if (issueSub.DcId > 0)
                        {
                            await _logs.LogDeveloperInfo($"[RollbackTracksAddIssueQtyAsync] IssueId missing. IssueSubId={issueSub.DcSubId}");

                            decimal totalBalQty = 0;

                            if (PoWiseGrn)
                            {
                                totalBalQty = await _unitOfWork.SubConDCOutSubs
                                    .GetQueryable()
                                    .Where(s => s.DcId == issueSub.DcId && s.TransType == "Out")
                                    .SumAsync(s => (decimal?)s.BalQty) ?? 0;
                            }
                            else
                            {
                                totalBalQty = await _unitOfWork.SubConDCOutSubs
                                    .GetQueryable()
                                    .Where(s => s.DcId == issueSub.DcId)
                                    .SumAsync(s => (decimal?)s.BalQty) ?? 0;
                            }

                            var issueComp = await _unitOfWork.SubConDCOuts
                                    .GetAsync(issueSub.DcId);

                                if (issueComp != null)
                                {
                                    issueComp.DcTally = totalBalQty == 0;
                                    await _unitOfWork.SubConDCOuts.UpdateAsync(issueComp);
                                }
                            continue;
                        }

                    }
                
                    await _unitOfWork.SubConGRNTracks.DeleteAsync(track);
                }

                await _unitOfWork.SaveAsync();

                await _logs.LogDeveloperInfo($"[RollbackTracksAddIssueQtyAsync] Rollback completed successfully. RefReturnSubId={refReturnSubId}");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[RollbackTracksAddIssueQtyAsync] Failed rollback for RefReturnSubId={refReturnSubId}");
                throw new InvalidOperationException("Failed to rollback issued quantities. Please contact support.");
            }
        }

        private async Task CreateTrackForGRNbyDcOutgoiningInItem(
               int returnSubId,
               int refDcSubId,
               int itemId,
               decimal qtyReturned,
               int screenCode,
               string currentUser,
               DateTime now)
        {
            List<int> dcIds = new();

            if (refDcSubId > 0)
            {

                dcIds = await GetDcIdsByDcSubIdAsync(refDcSubId);

                if (dcIds == null || !dcIds.Any())
                    return;
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
                    decimal componentNeeded = qtyReturned * bom.UtilQty;

                    var issueSubs = await _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .Where(x => dcIds.Contains(x.DcId)
                                 && x.ItemId == bom.ComponentItemId
                                 && x.BalQty > 0)
                        .OrderBy(x => x.DcSubId)
                        .ToListAsync();

                    foreach (var issue in issueSubs)
                    {
                        if (componentNeeded <= 0)
                            break;

                        decimal availableQty = issue.BalQty ?? 0;
                        decimal takeQty = Math.Min(availableQty, componentNeeded);
                        if (takeQty <= 0)
                            continue;

                        // 🔹 Create Track
                        var track = new SubConGRNTrack
                        {

                            RefGRNSubId = returnSubId,
                            RefDCSubId = issue.DcSubId,
                            RefPoSubId = issue.RefPoSubId,

                            // Incoming → Assembly
                            ItemIdIn = itemId,
                            QtyIn = qtyReturned,

                            // Outgoing → Component
                            ItemIdOut = bom.ComponentItemId,
                            QtyOut = takeQty,

                            CreatedBy = currentUser,
                            CreatedDate = now
                        };

                        await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                        // 🔹 Reduce Issue Balance
                        issue.BalQty = availableQty - takeQty;
                        if (issue.BalQty < 0) issue.BalQty = 0;

                        await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);

                        componentNeeded -= takeQty;
                    }

                    // 🔹 Update IssueTally per IssueId
                    foreach (var issueId in issueSubs.Select(x => x.DcId).Distinct())
                    {
                        var totalBalQty = await _unitOfWork.SubConDCOutSubs
                            .GetQueryable()
                            .Where(x => x.DcId == issueId)
                            .SumAsync(x => x.BalQty ?? 0);

                        var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(issueId);
                        if (issueHead != null)
                        {
                            issueHead.DcTally = (totalBalQty == 0);
                            await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
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

                decimal componentNeeded = qtyReturned * compData.Weight;

                var issueSubs = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => dcIds.Contains(x.DcId)
                             && x.ItemId == compData.RawMaterialItemId
                             && x.BalQty > 0)
                    .OrderBy(x => x.DcSubId)
                    .ToListAsync();

                foreach (var issue in issueSubs)
                {
                    if (componentNeeded <= 0)
                        break;

                    decimal availableQty = issue.BalQty ?? 0;
                    decimal takeQty = Math.Min(availableQty, componentNeeded);

                    if (takeQty <= 0)
                        continue;

                    var track = new SubConGRNTrack
                    {
                        RefGRNSubId = returnSubId,
                        RefDCSubId = issue.DcSubId,
                        RefPoSubId = issue.RefPoSubId,

                        ItemIdIn = itemId,
                        QtyIn = qtyReturned,

                        ItemIdOut = compData.RawMaterialItemId,
                        QtyOut = takeQty,

                        CreatedBy = currentUser,
                        CreatedDate = now
                    };

                    await _unitOfWork.SubConGRNTracks.CreateAsync(track);

                    issue.BalQty = availableQty - takeQty;
                    if (issue.BalQty < 0) issue.BalQty = 0;

                    await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);

                    componentNeeded -= takeQty;
                }

                // 🔹 Update IssueTally
                foreach (var issueId in issueSubs.Select(x => x.DcId).Distinct())
                {
                    var totalBalQty = await _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .Where(x => x.DcId == issueId)
                        .SumAsync(x => x.BalQty ?? 0);

                    var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(issueId);
                    if (issueHead != null)
                    {
                        issueHead.DcTally = (totalBalQty == 0);
                        await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
                    }
                }
                await _unitOfWork.SaveAsync();
            }

            await _unitOfWork.SaveAsync();
        }

        private async Task CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync(
                int returnSubId,
                int refDcSubId,
                int itemId,
                decimal qtyReturned,
                int screenCode,
                string currentUser,
                DateTime now, bool isManulSelection, List<SubConGRNSubVM> incomingSubVMs, int? refposubid)
        {

         
            var dcIds = await GetDcIdsByDcSubIdAsync(refDcSubId);
            if (dcIds == null || !dcIds.Any())
                return;

            bool isSameItem = false;

            if (refposubid.HasValue && refposubid.Value > 0)
            {
                isSameItem = await CheckIsSameAsOutItemByPoSubId(refposubid.Value);
            }
            if (isSameItem)
            {
                // Auto create track from same item
                var sameItemOuts = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => dcIds.Contains(x.DcId) &&

                        x.RefPoSubId == refposubid &&
                        x.TransType == "Out" &&
                        x.ItemId == itemId &&
                        (x.BalQty ?? 0) > 0)
                    .OrderBy(x => x.DcSubId)
                    .ToListAsync();

                decimal qtyToReduce = qtyReturned;

                foreach (var outSub in sameItemOuts)
                {
                    if (qtyToReduce <= 0)
                        break;

                    decimal deductQty = Math.Min(outSub.BalQty ?? 0, qtyToReduce);

                    await _unitOfWork.SubConGRNTracks.CreateAsync(new SubConGRNTrack
                    {
                        RefGRNSubId = returnSubId,
                        RefDCSubId = outSub.DcSubId,
                        RefPoSubId = outSub.RefPoSubId,

                        ItemIdIn = itemId,
                        QtyIn = qtyReturned,

                        ItemIdOut = outSub.ItemId,
                        QtyOut = deductQty,

                        CreatedBy = currentUser,
                        CreatedDate = now
                    });

                    outSub.BalQty = (outSub.BalQty ?? 0) - deductQty;

                    await _unitOfWork.SubConDCOutSubs.UpdateAsync(outSub);

                    qtyToReduce -= deductQty;
                }
                await _unitOfWork.SaveAsync();

                var issueIdss = sameItemOuts
                          .Select(x => x.DcId)
                          .Distinct()
                          .ToList();

                foreach (var issueId in issueIdss)
                {
                    var totalBalQty = await _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .Where(x => x.DcId == issueId)
                        .SumAsync(x => x.BalQty ?? 0);

                    var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(issueId);
                    if (issueHead != null)
                    {
                        issueHead.DcTally = totalBalQty == 0;
                        await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
                    }
                }
                await _unitOfWork.SaveAsync();


            }
            else if (!isManulSelection)
            {
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
                        decimal componentNeeded = qtyReturned * bom.UtilQty;

                        var issueSubs = await _unitOfWork.SubConDCOutSubs
                            .GetQueryable()
                            .Where(x => dcIds.Contains(x.DcId)
                                     && x.ItemId == bom.ComponentItemId
                                     && x.BalQty > 0)
                            .OrderBy(x => x.DcSubId)
                            .ToListAsync();

                        foreach (var issue in issueSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = issue.BalQty ?? 0;
                            decimal takeQty = Math.Min(availableQty, componentNeeded);
                            if (takeQty <= 0)
                                continue;

                            // 🔹 Create Track
                            var track = new SubConGRNTrack
                            {

                                RefGRNSubId = returnSubId,
                                RefDCSubId = issue.DcSubId,
                                RefPoSubId = issue.RefPoSubId,

                                // Incoming → Assembly
                                ItemIdIn = itemId,
                                QtyIn = qtyReturned,

                                // Outgoing → Component
                                ItemIdOut = bom.ComponentItemId,
                                QtyOut = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                            // 🔹 Reduce Issue Balance
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);

                            componentNeeded -= takeQty;
                        }

                        // 🔹 Update IssueTally per IssueId
                        foreach (var issueId in issueSubs.Select(x => x.DcId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.SubConDCOutSubs
                                .GetQueryable()
                                .Where(x => x.DcId == issueId)
                                .SumAsync(x => x.BalQty ?? 0);

                            var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(issueId);
                            if (issueHead != null)
                            {
                                issueHead.DcTally = (totalBalQty == 0);
                                await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
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

                    decimal componentNeeded = qtyReturned * compData.Weight;

                    var issueSubs = await _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .Where(x => dcIds.Contains(x.DcId)
                                 && x.ItemId == compData.RawMaterialItemId
                                 && x.BalQty > 0)
                        .OrderBy(x => x.DcSubId)
                        .ToListAsync();

                    foreach (var issue in issueSubs)
                    {
                        if (componentNeeded <= 0)
                            break;

                        decimal availableQty = issue.BalQty ?? 0;
                        decimal takeQty = Math.Min(availableQty, componentNeeded);

                        if (takeQty <= 0)
                            continue;

                        var track = new SubConGRNTrack
                        {
                            RefGRNSubId = returnSubId,
                            RefDCSubId = issue.DcSubId,
                            RefPoSubId = issue.RefPoSubId,

                            ItemIdIn = itemId,
                            QtyIn = qtyReturned,

                            ItemIdOut = compData.RawMaterialItemId,
                            QtyOut = takeQty,

                            CreatedBy = currentUser,
                            CreatedDate = now
                        };

                        await _unitOfWork.SubConGRNTracks.CreateAsync(track);

                        issue.BalQty = availableQty - takeQty;
                        if (issue.BalQty < 0) issue.BalQty = 0;

                        await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);

                        componentNeeded -= takeQty;
                    }

                    // 🔹 Update IssueTally
                    foreach (var issueId in issueSubs.Select(x => x.DcId).Distinct())
                    {
                        var totalBalQty = await _unitOfWork.SubConDCOutSubs
                            .GetQueryable()
                            .Where(x => x.DcId == issueId)
                            .SumAsync(x => x.BalQty ?? 0);

                        var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(issueId);
                        if (issueHead != null)
                        {
                            issueHead.DcTally = (totalBalQty == 0);
                            await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
                        }
                    }
                    await _unitOfWork.SaveAsync();
                }
            }
            else
            {
                // Validate selection
                var selectedRows = incomingSubVMs
                    .Where(x => x.RefDcSubId.HasValue && x.Qty > 0 && x.TransType == "Out")
                    .ToList();

                if (!selectedRows.Any())
                    throw new Exception("No manual issue rows selected.");

                // Validate total OUT qty
                decimal totalOutQty = selectedRows.Sum(x => x.Qty.GetValueOrDefault());
                if (totalOutQty <= 0)
                    throw new Exception("Invalid outgoing quantity.");

                // Create tracks per selected row
                foreach (var row in selectedRows)
                {
                    var issueSub = await _unitOfWork.SubConDCOutSubs.GetAsync(row.RefDcSubId!.Value);
                    if (issueSub == null)
                        throw new Exception($"IssueSub not found : {row.RefDcSubId}");

                    decimal availableQty = issueSub.BalQty ?? 0;
                    //if (availableQty < row.Qty)
                    //    throw new Exception($"Insufficient balance for IssueSubId {issueSub.DcSubId}");

                    if (availableQty < row.Qty && availableQty < row.Qty)
                    {
                        // throw new Exception($"Insufficient balance for IssueSubId {issueSub.SCNSubId}");

                    }

                    // Track (same pattern as AUTO)
                    var track = new SubConGRNTrack
                    {
                        RefGRNSubId = returnSubId,
                        RefDCSubId = issueSub.DcSubId,

                        ItemIdIn = itemId,
                        QtyIn = qtyReturned,              //  SAME AS AUTO

                        ItemIdOut = issueSub.ItemId,      //  COMPONENT / RM
                        QtyOut = row.Qty,                 //  USER SELECTED

                        CreatedBy = currentUser,
                        CreatedDate = now
                    };

                    await _unitOfWork.SubConGRNTracks.CreateAsync(track);

                    // Reduce balance
                    issueSub.BalQty = availableQty - row.Qty;

                    if (issueSub.BalQty < 0) issueSub.BalQty = 0;

                    await _unitOfWork.SubConDCOutSubs.UpdateAsync(issueSub);
                }
                await _unitOfWork.SaveAsync();

                // Update IssueTally
                var issueIdss = selectedRows
                    .Select(x => x.RefDcSubId)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct();

                foreach (var issueId in issueIdss)
                {
                    var totalBalQty = await _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .Where(x => x.DcId == issueId)
                        .SumAsync(x => x.BalQty ?? 0);

                    var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(issueId);
                    if (issueHead != null)
                    {
                        issueHead.DcTally = totalBalQty == 0;
                        await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
                    }
                }
            }
            await _unitOfWork.SaveAsync();
        }


        private async Task CreateTracksForRouteCardSubsAndReduceIssueAndUpdateIssueTallyAsync(int returnSubId, int refRcSubId,int? refposubid ,int itemId,
                        decimal qtyReturned, int screenCode, string currentUser, DateTime now)
        {
            var issueIds = await GetIssueIdsByRcSubIdAsync(refRcSubId);

            if (issueIds == null || !issueIds.Any())
                return;

            var RcSubData = await _unitOfWork.RouteCardSubs.GetQueryable()
                            .Include(x => x.RouteCard)
                            .Where(x => x.RCSubId == refRcSubId).FirstOrDefaultAsync();

            if (RcSubData == null)
                return;

            var utility = 1m;

            if (RcSubData.SeqNo == 1)
            {
                utility = RcSubData.RouteCard.RMWeight;
            }

            decimal rmRequiredQty = qtyReturned * utility;

            var issueSubs = await _unitOfWork.SubConDCOutSubs
                .GetQueryable()
                .Where(x => issueIds.Contains(x.DcId) &&
                    x.ItemId == RcSubData.ItemIdOut &&
                    (x.BalQty ?? 0) > 0
                    && x.TransType == "Out")
                .OrderBy(x => x.DcSubId)
                .ToListAsync();

            foreach (var issueSub in issueSubs)
            {
                if (rmRequiredQty <= 0)
                    break;

                decimal availableQty = issueSub.BalQty ?? 0;
                decimal consumeQty = Math.Min(availableQty, rmRequiredQty);

                if (consumeQty <= 0)
                    continue;

                var track = new SubConGRNTrack
                {
                    RefGRNSubId = returnSubId,
                    RefDCSubId = issueSub.DcSubId,
                    RefRcSubId = refRcSubId,
                    RefPoSubId= refposubid??null,
                    ItemIdIn = itemId,
                    QtyIn = qtyReturned,

                    ItemIdOut = RcSubData.ItemIdOut,
                    QtyOut = consumeQty,

                    CreatedBy = currentUser,
                    CreatedDate = now
                };

                await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                await _unitOfWork.SaveAsync();

                issueSub.BalQty = availableQty - consumeQty;
                if (issueSub.BalQty < 0)
                    issueSub.BalQty = 0;

                await _unitOfWork.SubConDCOutSubs.UpdateAsync(issueSub);
                await _unitOfWork.SaveAsync();

                rmRequiredQty -= consumeQty;
            }

            var affectedIssueIds = issueSubs
                .Select(x => x.DcId)
                .Distinct()
                .ToList();

            foreach (var issueId in affectedIssueIds)
            {
                var remainingBalQty = await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.DcId == issueId && x.TransType == "Out")
                    .SumAsync(x => x.BalQty ?? 0);

                var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(issueId);
                if (issueHead != null)
                {
                    issueHead.DcTally = (remainingBalQty == 0);
                    await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
                    await _unitOfWork.SaveAsync();
                }
            }

            await _unitOfWork.SaveAsync();
        }


        //private async Task AdjustMfgPoSubItemBalanceAsync(int refPoSubId, int itemId, decimal oldQty, decimal newQty, string context)
        //{
        //    try
        //    {
        //        if (refPoSubId == 0) return;

        //        var poSub = await _unitOfWork.PurchPoSubs.GetQueryable()
        //            .Where(j => j.PoSubId == refPoSubId && j.ItemId == itemId).FirstOrDefaultAsync();

        //        if (poSub == null) return;

        //        if (oldQty > 0)
        //            poSub.BalQty += oldQty;

        //        if (newQty > poSub.Qty)
        //            throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

        //        if (newQty > 0)
        //            poSub.BalQty -= newQty;

        //        await _unitOfWork.PurchPoSubs.UpdateAsync(poSub);
        //        await _unitOfWork.SaveAsync();

        //        var totalBalQty = await _unitOfWork.PurchPoSubs
        //                               .GetQueryable()
        //                               .Where(e => e.PoId == poSub.PoId)
        //                               .SumAsync(e => e.BalQty);

        //        var Purchpo = await _unitOfWork.PurchPos.GetAsync(poSub.PoId);
        //        if (Purchpo != null)
        //        {
        //            Purchpo.PoTally = (totalBalQty == 0);
        //            await _unitOfWork.PurchPos.UpdateAsync(Purchpo);
        //            await _unitOfWork.SaveAsync();
        //        }

        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"[AdjustMfgPoSubItemBalanceAsync] Validation failed in {context}");
        //        throw;
        //    }
        //    catch (Exception ex)
        //    {
        //        await _logs.LogDeveloperError(ex, $"[AdjustMfgPoSubItemBalanceAsync] Unexpected error in {context}");
        //        throw new InvalidOperationException("Failed to Adjust MfgPo Sub Balance. Please contact support.");
        //    }
        //}

        private async Task AdjustProductionCompIssueItemBalanceAsync(int? refIssueSubId, decimal oldQty, decimal newQty, string context)
        {
            try
            {
                if (!refIssueSubId.HasValue || refIssueSubId == 0) return;

                var compIssueSub = await _unitOfWork.SubConDCOutSubs.GetQueryable()
                                    .Where(p => p.DcSubId == refIssueSubId).FirstOrDefaultAsync();

                if (compIssueSub == null) return;

                if (oldQty > 0)
                    compIssueSub.BalQty += oldQty;

                if (newQty > compIssueSub.Qty)
                    throw new InvalidOperationException($"{context}: Qty cannot exceed Required BalQty.");

                if (newQty > 0)
                    compIssueSub.BalQty -= newQty;

                await _unitOfWork.SubConDCOutSubs.UpdateAsync(compIssueSub);
                await _unitOfWork.SaveAsync();

                var totalBalQty = await _unitOfWork.SubConDCOutSubs
                                        .GetQueryable()
                                        .Where(e => e.DcId == compIssueSub.DcId)
                                        .SumAsync(e => e.BalQty);

                var compIssue = await _unitOfWork.SubConDCOuts.GetAsync(compIssueSub.DcId);
                if (compIssue != null)
                {
                    compIssue.DcTally = (totalBalQty == 0);
                    await _unitOfWork.SubConDCOuts.UpdateAsync(compIssue);
                    await _unitOfWork.SaveAsync();
                }
            }
            catch (InvalidOperationException ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustProductionCompIssueItemBalanceAsync] Validation failed in {context}");
                throw;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[AdjustProductionCompIssueItemBalanceAsync] Unexpected error in {context}");
                throw new InvalidOperationException("Failed to Adjust Production Issue Sub Balance. Please contact support.");
            }
        }

        private async Task CreateProuctionSCNFromProductionGRNCompAsync(SubConGRN grn)
        {
            var changes = new StringBuilder();

            try
            {
                //var suffix = FinancialYearHelper.GetFinancialYearSuffix(DateTime.Now);


                //var MapStoreForProdSCN = await GetMappedStoreForFormAsync("SCNProductionAssyForm");
                //var addStore = 1;

                //if (MapStoreForProdSCN.StoreId > 0)
                //{
                //    addStore = MapStoreForProdSCN.StoreId;
                //}


                //var scn = new ProductionSCNAssy
                //{
                //    Suffix = suffix,
                //    SCNNo = await _productionSCNAssyService.GetSCNNumberAsync(suffix),
                //    SCNDate = DateTime.Now,
                //    IssueStoreId = grn.AddStoreId,
                //    AddStoreId = addStore,
                //    CreatedBy = grn.CreatedBy,
                //    CreatedDate = DateTime.Now,
                //    MainRemark = "Auto-generated due to No Inspection",
                //};

                //scn.ProductionSCNAssySubs = grn.ProductionReturnCompSubs.Select(s => new ProductionSCNAssySub
                //{
                //    SlNo = s.SlNo,
                //    SCNId = scn.SCNId,
                //    ItemId = s.ItemId.Value,
                //    RefReturnSubId = s.ReturnSubId,
                //    BalQty = s.QtyReturned.GetValueOrDefault(),
                //    AccQty = s.QtyReturned.GetValueOrDefault(),
                //    UnitPrice = s.UnitPrice,
                //    CostId = s.CostId,
                //}).ToList();


                //// Save SCN
                //await _unitOfWork.ProductionSCNAssys.CreateAsync(scn);
                //await _unitOfWork.SaveAsync();

                //foreach (var sub in scn.ProductionSCNAssySubs)
                //{
                //    if (sub.RefReturnSubId > 0)
                //    {
                //        var ReturnAssySub = await _unitOfWork.ProductionReturnCompSubs.GetAsync(sub.RefReturnSubId.Value);

                //        if (ReturnAssySub == null)
                //            throw new InvalidOperationException("Invalid GRN Sub reference.");

                //        ReturnAssySub.BalQty -= sub.AccQty;

                //        await _unitOfWork.ProductionReturnCompSubs.UpdateAsync(ReturnAssySub);
                //        await _unitOfWork.SaveAsync();

                //        var totalBalQty = await _unitOfWork.ProductionReturnCompSubs
                //            .GetQueryable()
                //            .Where(x => x.ReturnId == ReturnAssySub.ReturnId)
                //            .SumAsync(x => x.BalQty);

                //        grn.ReturnTally = (totalBalQty == 0);
                //        await _unitOfWork.ProductionReturnComps.UpdateAsync(grn);
                //        await _unitOfWork.SaveAsync();
                //    }

                //    var ScnScreenCode = await GetScreenCodeByScreenNameAsync("Production SCN Assembly");

                //    await _stockManagerService.IssueOrUpdateStockAsync(sub.ItemId, grn.AddStoreId.Value, sub.AccQty,
                //        sub.UnitPrice, null, ScnScreenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, allowMultipleIssue: true);

                //    await _stockManagerService.AddOrUpdateStockAsync(sub.ItemId, addStore, sub.AccQty,
                //        sub.UnitPrice, null, ScnScreenCode, sub.SCNSubId, scn.SCNNo, scn.SCNDate, sub.Remark, allowMultipleAdd: true);

                //}

                //await LogChangesAsync(changes, "Purchase SCN Created");
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task HandleTrackUpdatesAsync(
            SubConGRN entity,
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

        public async Task<decimal?> GetIssueBalQtyByIssueSubId(int issueSubId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs.GetQueryable()
                        .Where(x => x.DcSubId == issueSubId)
                        .SumAsync(x => x.BalQty);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error loading Production Issue Compo details for IssueSubId: {issueSubId}");
                throw new InvalidOperationException($"Failed to load Issue Comp details for IssueSubId: {issueSubId}", ex);
            }

        }


        public async Task<List<SubConGRNTrackVM>> GetAllExistingRouteCardAsync()
        {
            try
            {
                return await _unitOfWork.RouteCards.GetQueryable()

                    .OrderBy(rc => rc.RCId)
                    .Select(rc => new SubConGRNTrackVM
                    {
                        RefRcSubId = rc.RCId,
                        RefRcNo = rc.RCNo + rc.Suffix
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    "Error fetching existing Route Cards with balance quantity"
                );
                throw;
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



        public async Task<SubConGRNSubVM?> GetProdReturnSubItemDetailByReturnSubIdAsync(int returnSubId)
        {
            try
            {
                return await _unitOfWork.SubConGRNSubs
                    .GetQueryable()
                    .Where(q => q.GRNSubId == returnSubId)
                    .Select(q => new SubConGRNSubVM
                    {
                        Qty = q.Qty.GetValueOrDefault(),
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

        public async Task<SubConGRNSubVM?> GetProdReturnSubItemDetailByOutGoingSubIdAsync(int IssueSubId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(q => q.DcSubId == IssueSubId)
                    .Select(q => new SubConGRNSubVM
                    {
                        Qty = q.Qty,
                        BalQty = q.BalQty.GetValueOrDefault()
                    })
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching Production Return sub item detail for ReturnSubId: {IssueSubId}");
                throw new InvalidOperationException("Failed to retrieve Prouction Return sub-item details.");
            }
        }
        public async Task<(bool CanDelete, string Message)> CanRemoveSubConGRNAsync(int ReturnId, int ReturnSubId)
        {
            try
            {
                var DcSubGrnIds = await _unitOfWork.SubConGRNs
                                     .GetQueryable()
                                     .Where(s => s.GRNId == ReturnId)
                                     .SelectMany(s => s.SubConGRNSubs.Select(sub => sub.GRNSubId))
                                     .ToListAsync();

                if (!DcSubGrnIds.Any())
                    return (true, "Subcontract GRN can be safely deleted.");


                bool Quotation = await _unitOfWork.SubConSCNSubs
                    .GetQueryable()
                    .AnyAsync(qs => DcSubGrnIds.Contains(qs.RefGRNSubId.Value));

                if (Quotation)
                    return (false, "Cannot delete this Subcontract SCN  as a some transaction Made.");

                var sums = await (
                                 from sub in _unitOfWork.SubConGRNSubs.GetQueryable()
                                 where sub.GRNSubId == ReturnSubId
                                 group sub by 1 into g
                                 select new
                                 {
                                     TotalQty = g.Sum(s => (decimal?)s.Qty) ?? 0,
                                     TotalBalQty = g.Sum(s => (decimal?)s.BalQty) ?? 0
                                 }
                             ).FirstOrDefaultAsync();

                bool hasPurchPo = sums != null && sums.TotalQty == sums.TotalBalQty;

                if (!hasPurchPo)
                    return (false, "Cannot delete this Subcontract GRN as some transactions have been made.");

                //var Quote = await _unitOfWork.SubConGRNSubs
                //               .GetQueryable()
                //               .Where(e => e.ReturnId == IssueId)
                //               .Select(e => new
                //               {
                //                   e.IssueId,
                //                   e.Cancel,
                //                   //e.QuoteShortClose,
                //                   SubItems = e.SubConDcOutgoingSubs.Select(s => new
                //                   {
                //                       s.IssueSubId,
                //                       //s.ItemCancel
                //                   }).ToList()
                //               })
                //               .FirstOrDefaultAsync();


                //if (Quote == null)
                //    return (false, "Subcontract Outgoing not found.");


                //if (Quote.Cancel /*|| Quote.QuoteShortClose*/)
                //    return (false, "Main Subcontract Outgoing is already cancelled Or Short Closed and cannot be deleted.");

                ////if (Quote.SubItems.Any(s => s.ItemCancel))
                ////    return (false, "Some Quotation items are cancelled and cannot be deleted.");


                //if (Quote.SubItems.Any())
                //    return (true, "Subcontract Outgoing can be safely deleted (no sub-items).");


                return (true, "Subcontract Outgoing can be safely deleted.");

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteEnquiryAsync for Quoteid: {ReturnId}");
                throw new Exception("Error checking Purchase Quotation delete eligibility", ex);
            }
        }

        public async Task ValidateDcOutogoingItemBalanceBeforeRevertAsync(SubConGRNSub sub)
        {
            if (sub.RefDcSubId.GetValueOrDefault() <= 0)
                return;

            var entity = await _unitOfWork.SubConDCOutSubs.GetAsync(sub.RefDcSubId.Value);

            if (entity == null)
                throw new InvalidOperationException($"Subcontract Dc not found for RefDcSubId: {sub.RefDcSubId}");

            if (entity.BalQty < sub.Qty && !entity.ItemCancel)
            {
                throw new InvalidOperationException(
                    $"Cannot revert because Sun contract Dc balance ({entity.BalQty}) is less than required quantity ({sub.Qty})."
                );
            }
        }

        public async Task<List<Dictionary<string, object>>> GetPoDetailsByVendor(List<int> poIds,int VendorCode, int? storeId)
        {
            try
            {
                // 1️⃣ Fetch PO data
                var poData = await (
                    from p in _unitOfWork.PurchPos.GetQueryable()
                    join ps in _unitOfWork.PurchPoSubs.GetQueryable()
                        on p.PoId equals ps.PoId
                    join i in _unitOfWork.ItemRepositories.GetQueryable()
                                 on ps.ItemId equals i.ItemId
                    // LEFT JOIN RouteCard
                    join r in _unitOfWork.RouteCards.GetQueryable()
                        on ps.RCId equals r.RCId into rcGroup
                    from r in rcGroup.DefaultIfEmpty()

                        // Match RCId + ProcessId + ItemId
                    join rs in _unitOfWork.RouteCardSubs.GetQueryable()
                        on new
                        {
                            RCId = (int?)ps.RCId,
                            ProcessId = (int?)ps.ProcessId,
                            ItemId = (int?)ps.ItemId,
                            RCSubId = (int?)ps.RefRcSubId

                        }
                        equals new
                        {
                            RCId = (int?)rs.RCId,
                            ProcessId = (int?)rs.ProcessId,
                            ItemId = (int?)rs.ItemIdIn,
                            RCSubId = (int?)rs.RCSubId

                        }
                        into rsGroup
                    from rs in rsGroup.DefaultIfEmpty()
                    where p.VendorCode == VendorCode
                          && !p.PoTally
                          && !p.PoCancl
                          && !ps.ItemCancel
                          && ps.BalQty > 0  && poIds.Contains(p.PoId)
                    select new
                    {
                        ps.PoSubId,
                        p.PoId,
                        p.IsOpenPO,
                        p.PONo,
                        p.Suffix,
                        p.PODate,
                        ps.RefRcSubId,
                        ps.ItemId,
                        ps.Item.ItemCode,
                        ps.Item.ItemName,
                        ps.Item.MeasureUnit,
                        i.Category.CategoryCode,
                        i.Category.CategoryName,
                        ps.BalQty,
                        ps.UnitPrice,
                        ps.DueDate,
                        ps.ProcessId,
                        ps.Process.ProcessName,
                        RCNo = rs != null ? r.RCNo + r.Suffix : "",
                        RCDate = rs != null ? r.RCDate : (DateTime?)null,

                        RCSubId = rs != null ? rs.RCSubId : (int?)null,
                        CostCenterId = ps.CostId == 0 ? (int?)null : ps.CostId,
                        ProjectNo = ps.CostCenter != null ? ps.CostCenter.ProjectNo : null,
                        p.MainRemark
                    }
                ).ToListAsync();

                if (!poData.Any())
                    return new List<Dictionary<string, object>>();

                // 2️⃣ Fetch stock for all PO items (single call)
                var itemIds = poData
                    .Select(x => x.ItemId)
                    .Distinct()
                    .ToList();

                var stockMap = await _stockManagerService.GetStockForItemsAsync(itemIds, storeId);

                // 3️⃣ Merge PO + Stock
                var result = poData.Select(r =>
                {
                    stockMap.TryGetValue(r.ItemId, out var stockQty);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,
                        ["PoSubId"] = r.PoSubId,
                        ["PoId"] = r.PoId,
                        ["IsOpenPo"] = r.IsOpenPO ? "Yes" : "No",
                        ["TransType"] = "In",

                        ["PoNo"] = $"{r.PONo}{r.Suffix}",
                        ["PoDate"] = r.PODate,

                        ["ItemId"] = r.ItemId,
                        ["ItemCode"] = r.ItemCode ?? string.Empty,
                        ["ItemName"] = r.ItemName ?? string.Empty,
                        ["UOM"] = r.MeasureUnit ?? string.Empty,
                        ["CategoryCode"] = r.CategoryCode,
                        ["CategoryName"] = r.CategoryName ?? string.Empty,
                        ["Qty"] = r.BalQty,
                        ["BalQty"] = r.BalQty,
                        ["UnitPrice"] = r.UnitPrice,
                        ["PoDuedate"] = r.DueDate,

                        ["RCNo"] = r.RCNo ?? string.Empty,
                        ["RefRcSubId"] = r.RCSubId ?? null,

                        ["RCDt"] = r.RCDate?.ToString("dd/MM/yyyy") ?? string.Empty,

                        ["ProcessName"] = r.ProcessName ?? string.Empty,

                        ["ProcessId"] = r.ProcessId ?? null,
                        ["StockQty"] = stockQty, 
                        ["CostCenterId"] = r.CostCenterId,
                        ["ProjectNo"] = r.ProjectNo ?? string.Empty,
                        ["Remark"] = r.MainRemark ?? string.Empty
                    };
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching PO details for VendorCode: {VendorCode}, StoreId: {storeId}"
                );

                throw new InvalidOperationException(
                    "Failed to retrieve PO details. Please try again.", ex);
            }
        }

        public async Task<List<SubConDcOutVM>> LoadDcOutNumbersPoWiseAsync(
                                                                 List<int> poIds,
                                                                 string check,
                                                                 bool receivedReturn)
        {
            try
            {
                if (poIds == null || poIds.Count == 0)
                    return new List<SubConDcOutVM>();

                List<SubConDcOutVM> dcOuts;

                if (check == "PO")
                {
                    var poSubIds = await _unitOfWork.PurchPoSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(x => poIds.Contains(x.PoId))
                        .Select(x => x.PoSubId)
                        .ToListAsync();

                    if (!receivedReturn)
                    {
                        // SubCon DC Out Pending
                        dcOuts = await (
                            from dc in _unitOfWork.SubConDCOuts.GetQueryable().AsNoTracking()
                            join dcSub in _unitOfWork.SubConDCOutSubs.GetQueryable().AsNoTracking()
                                on dc.DcId equals dcSub.DcId
                            where
                                !dc.DcTally
                                && !dc.Cancel
                                && !dcSub.ItemCancel
                                && dcSub.RefPoSubId.HasValue
                                && poSubIds.Contains(dcSub.RefPoSubId.Value)    // Same DC must contain pending OUT item
                                && _unitOfWork.SubConDCOutSubs.GetQueryable().Any(dcOut =>
                                       dcOut.DcId == dcSub.DcId
                                       && dcOut.TransType == "Out"
                                       && !dcOut.ItemCancel
                                       && dcOut.BalQty > 0)


                            group dc by new
                            {
                                dc.DcId,
                                dc.DcNo,
                                dc.Suffix
                            }
                            into g

                            select new SubConDcOutVM
                            {
                                DcId = g.Key.DcId,
                                DcNo = g.Key.DcNo + g.Key.Suffix
                            }
                        ).ToListAsync();
                    }
                    else
                    {
                        // SubCon DC Return Pending
                        dcOuts = await (
                            from dc in _unitOfWork.SubConDCOuts.GetQueryable().AsNoTracking()

                            join dcIn in _unitOfWork.SubConDCOutSubs.GetQueryable().AsNoTracking()
                                on dc.DcId equals dcIn.DcId

                            where
                                !dc.Cancel
                                && !dcIn.ItemCancel

                                // PO Reference comes from IN row
                                && dcIn.TransType == "In"
                                && dcIn.RefPoSubId.HasValue
                                && poSubIds.Contains(dcIn.RefPoSubId.Value)

                                // Same DC must contain pending OUT item
                                && _unitOfWork.SubConDCOutSubs.GetQueryable().Any(dcOut =>
                                       dcOut.DcId == dcIn.DcId
                                       && dcOut.TransType == "Out"
                                       && !dcOut.ItemCancel
                                       && dcOut.BalQty > 0)

                            group dc by new
                            {
                                dc.DcId,
                                dc.DcNo,
                                dc.Suffix
                            }
                            into g

                            select new SubConDcOutVM
                            {
                                DcId = g.Key.DcId,
                                DcNo = g.Key.DcNo + g.Key.Suffix
                            }
                        ).ToListAsync();
                    }

                    return dcOuts;
                }
                else
                {
                    // DC OUT Wise

                    var dcOutSubIds = await _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(x =>
                            poIds.Contains(x.DcId)
                            && !x.ItemCancel)
                        .Select(x => x.DcSubId)
                        .ToListAsync();

                    if (!receivedReturn)
                    {
                        dcOuts = await (
                            from dc in _unitOfWork.SubConDCOuts.GetQueryable().AsNoTracking()
                            join dcSub in _unitOfWork.SubConDCOutSubs.GetQueryable().AsNoTracking()
                                on dc.DcId equals dcSub.DcId

                            where
                                !dc.DcTally
                                && !dc.Cancel
                                && !dcSub.ItemCancel
                                && dcSub.BalQty > 0
                                && dcOutSubIds.Contains(dcSub.DcSubId) && dcSub.TransType == "Out"

                            group dc by new
                            {
                                dc.DcId,
                                dc.DcNo,
                                dc.Suffix
                            }
                            into g

                            select new SubConDcOutVM
                            {
                                DcId = g.Key.DcId,
                                DcNo = g.Key.DcNo + g.Key.Suffix
                            }
                        ).ToListAsync();
                    }
                    else
                    {
                        dcOuts = await (
                            from dc in _unitOfWork.SubConDCOuts.GetQueryable().AsNoTracking()
                            join dcSub in _unitOfWork.SubConDCOutSubs.GetQueryable().AsNoTracking()
                                on dc.DcId equals dcSub.DcId

                            where
                                !dc.Cancel
                                && !dcSub.ItemCancel
                                && (dcSub.BalQty > 0 )
                                && dcOutSubIds.Contains(dcSub.DcSubId) && dcSub.TransType == "Out"

                            group dc by new
                            {
                                dc.DcId,
                                dc.DcNo,
                                dc.Suffix
                            }
                            into g

                            select new SubConDcOutVM
                            {
                                DcId = g.Key.DcId,
                                DcNo = g.Key.DcNo + g.Key.Suffix
                            }
                        ).ToListAsync();
                    }

                    return dcOuts;
                }
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error fetching SubCon DC Out details for IDs: {string.Join(",", poIds)}");

                throw new InvalidOperationException(
                    "Failed to retrieve SubCon DC Out details.");
            }
        }

        public async Task<int> GetPendingPoCountAsync(int vendorCode)
        {
            try
            {
                return await _unitOfWork.PurchPos
                    .GetQueryable()
                    .Where(p =>
                        p.VendorCode == vendorCode &&
                        !p.PoTally &&
                        !p.PoCancl &&
                        p.Authorized &&
                        p.PurchORSubCon == false &&
                        !p.PoShortClose &&
                        p.PurchPoSubs.Any(ps => !ps.ItemCancel && ps.BalQty > 0)
                    )
                    .CountAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error while fetching Pending PO Count for VendorCode = {vendorCode}");
                return 0;
            }
        }
        public async Task<List<PurchPoVM>> GetOpenPurchPosWiseByVendor(int vendorCode)
        {
            try
            {
                return await _unitOfWork.PurchPos
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(p =>
                        p.VendorCode == vendorCode &&
                        !p.PoTally &&
                        !p.PoCancl &&
                        !p.PurchORSubCon &&     // FIXED
                        !p.PoShortClose &&
                        p.Authorized &&
                        p.PurchPoSubs.Any(ps =>
                            !ps.ItemCancel &&
                            ps.BalQty > 0)
                    )
                    .Select(p => new PurchPoVM
                    {
                        PoId = p.PoId,
                        PONo = p.PONo + p.Suffix
                    })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error in GetOpenPurchPosWiseByVendor for vendorcode: {vendorCode}");

                throw new Exception(
                    "Error checking GetOpenPurchPosWiseByVendor", ex);
            }
        }
        public async Task<int> GetPendingDcOutsPoCountAsync(int VendorCode)
        {
            try
            {
                return await _unitOfWork.SubConDCOuts
                        .GetQueryable()
                        .AsNoTracking()
                        .Where(h =>
                            h.VendorCode == VendorCode &&

                            // Check valid IN items
                            h.SubConDcOutSubs.Any(s =>
                                s.TransType == "In" &&
                                !s.PurchPoSub.ItemCancel &&
                                !s.PurchPoSub.PurchPo.PoCancl &&
                                !s.PurchPoSub.PurchPo.PoShortClose) &&

                            // Check OUT item balance
                            h.SubConDcOutSubs.Any(s => s.BalQty > 0)
                        )
                        .CountAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,
                    $"GetPendingDcOutsPoCountAsync using VendorCode : ({VendorCode})");

                return 0;
            }
        }
        public async Task<List<SubConDcOutSubVM>> GetSubConDcOutQtyByDcIdsAsync(List<int> DcIds)
        {
            if (DcIds == null || DcIds.Count == 0)
                return new List<SubConDcOutSubVM>();

            return await _unitOfWork.SubConDCOutSubs
                .GetQueryable()
                .Where(x =>
                    DcIds.Contains(x.DcId) &&
                   (x.BalQty > 0) &&
                    x.ItemCancel == false && x.TransType == "Out")
                .GroupBy(x => x.ItemId)
                .Select(g => new SubConDcOutSubVM
                {
                    ItemId = g.Key,
                    BalQty = g.Sum(x => x.BalQty)
                })
                .ToListAsync();
        }
        public async Task<List<SubConDcOutSubVM>> GetSubConRcDetailsByDcIdsAsync(List<int> dcIds)
        {
            if (dcIds == null || dcIds.Count == 0)
                return new List<SubConDcOutSubVM>();

            return await _unitOfWork.SubConDCOutSubs
                .GetQueryable()
                .Include(x => x.Item)

                .Include(x => x.ComponentRouteCardSub)

                    .ThenInclude(x => x.Process)
            
                .Include(x => x.PurchPoSub)
                    .ThenInclude(x => x.PurchPo)

                .Where(x =>
                    dcIds.Contains(x.DcId) &&
                    x.BalQty > 0 &&
                    !x.ItemCancel &&
                    x.TransType == "In" &&

                    // ✅ MATCH PROCESS + ITEM
                    x.PurchPoSub != null &&
                    x.ComponentRouteCardSub != null &&

                    x.PurchPoSub.ProcessId ==
                        x.ComponentRouteCardSub.ProcessId &&

                    x.PurchPoSub.ItemId ==
                        x.ComponentRouteCardSub.ItemIdOut)

                .GroupBy(x => new
                {
                    x.ItemId,
                    x.RefPoSubId,
                    x.ProcessId
                })

                .Select(g => new SubConDcOutSubVM
                {
                    DcSubId = g.First().DcSubId,

                    DcId = g.First().DcId,

                    ItemId = g.First().ItemId,

                    ItemCode = g.First().Item.ItemCode,

                    ItemName = g.First().Item.ItemName,

                    MeasureUnit = g.First().Item.MeasureUnit,

                    Qty = g.Sum(x => x.BalQty),

                    BalQty = g.Sum(x => x.BalQty),

                    UnitPrice = g.First().UnitPrice,

                    RefPoSubId = g.First().RefPoSubId,

                    Remark = g.First().Remark,

                    RcSubId = g.First().RcSubId,

                    RefRcNo = g.First().ComponentRouteCardSub.RouteCard.RCNo,

                    RefRcDate = g.First().ComponentRouteCardSub.RouteCard.RCDate,

                    ProcessId = g.First().ProcessId,

                    ProcessName = g.First().ComponentRouteCardSub.Process.ProcessName,

                    IsEditable = false,
                    // ✅ PO DETAILS
                    RefPoNo = g.First().PurchPoSub.PurchPo.PONo,

                    RefPoDate = g.First().PurchPoSub.PurchPo.PODate
                })
                .ToListAsync();
        }
        public async Task<List<SubConDcOutSubVM>> GetDcSubConOutQtyByDcIdsAsync(List<int> SubConOutIds)
        {
            if (SubConOutIds == null || !SubConOutIds.Any())
                return new List<SubConDcOutSubVM>();

            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Include(x => x.Item)
                    .Where(x =>
                        SubConOutIds.Contains(x.DcId) && x.TransType=="Out" &&
                        (x.BalQty > 0))   // OR condition
                    .Select(x => new SubConDcOutSubVM
                    {
                        DcSubId = x.DcSubId,
                        DcId = x.DcId,
                        ItemId = x.ItemId,
                        ItemCode = x.Item.ItemCode,
                        ItemName = x.Item.ItemName,
                        MeasureUnit = x.Item.MeasureUnit,
                        Qty = x.BalQty,   // you can change if needed
                        BalQty = x.BalQty,
                        UnitPrice = x.UnitPrice,
                        RefPoSubId = x.RefPoSubId,
                        Remark = x.Remark,
                        IsEditable = false
                    })
                    .ToListAsync();

            
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error fetching GetDcSubConOutQtyByDcIdsAsync data for SubConDcOutIds: {string.Join(",", SubConOutIds)}");
                return new List<SubConDcOutSubVM>();
            }
        }

        private async Task CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyPoWiseAsync(int dcSubId, int RefPoSubId, int itemId, decimal qty,
                                                                             int screenCode, string currentUser, DateTime now, int RefDcId)
        {
            try
            {

                List<int> DcOutSubIds;
                if (RefPoSubId == 0 && RefDcId == 0)
                    return;

                if (RefPoSubId > 0)
                {
                    DcOutSubIds = await _unitOfWork.SubConDCOutSubs.GetQueryable().Where(x => x.TransType == "Out" &&
                                      _unitOfWork.SubConDCOutSubs.GetQueryable().Where(y => y.RefPoSubId == RefPoSubId)
                                      .Select(y => y.DcId).Contains(x.DcId)).Select(x => x.DcSubId).ToListAsync();
                }
                else
                {
                    DcOutSubIds = RefDcId > 0 ? new List<int> { RefDcId } : new List<int>();
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

                        var IssuedSubs = await _unitOfWork.SubConDCOutSubs
                                        .GetQueryable()
                                        .Where(x =>
                                               x.DcId > 0
                                            && DcOutSubIds.Contains(x.DcSubId)
                                            && x.ItemId == bom.ComponentItemId
                                            && x.BalQty > 0)
                                        .OrderBy(x => x.DcId)
                                        .ToListAsync();

                        foreach (var issue in IssuedSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = issue.BalQty??0;
                            decimal takeQty = Math.Min(availableQty, componentNeeded);
                            if (takeQty <= 0)
                                continue;

                            // 🔹 Create Track
                            var track = new SubConGRNTrack
                            {

                                RefGRNSubId = dcSubId,
                                RefDCSubId = issue.DcSubId,
                                RefPoSubId = issue.RefPoSubId,

                                // Incoming → Assembly
                                ItemIdIn = itemId,
                                QtyIn = qty,

                                // Outgoing → Component
                                ItemIdOut = bom.ComponentItemId,
                                QtyOut = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                            // 🔹 Reduce Issue Balance
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);

                            await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                            await _unitOfWork.SaveAsync();
                            // 🔹 Reduce Issue Balance
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }
                        foreach (var dcid in IssuedSubs.Where(x => x.DcId > 0).Select(x => x.DcId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.SubConDCOutSubs
                                .GetQueryable()
                                .Where(x => x.DcId == dcid && x.TransType=="Out")
                                .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(dcid);
                            if (issueHead != null)
                            {
                                issueHead.DcTally = (totalBalQty == 0);
                                await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
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
                
                    var issueSubs = await _unitOfWork.SubConDCOutSubs
                                       .GetQueryable()
                                       .Where(x =>
                                              x.DcId > 0
                                           && DcOutSubIds.Contains(x.DcSubId)
                                          && x.ItemId == compData.RawMaterialItemId
                                        && x.BalQty > 0)
                                       .OrderBy(x => x.DcId)
                                       .ToListAsync();
                    if (issueSubs.Count > 0)
                    {
                        foreach (var issue in issueSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = issue.BalQty??0;
                            decimal takeQty = Math.Min(availableQty, componentNeeded);
                            if (takeQty <= 0)
                                continue;

                            // 🔹 Create Track
                            var track = new SubConGRNTrack
                            {

                                RefGRNSubId = dcSubId,
                                RefDCSubId = issue.DcSubId,
                                RefPoSubId = issue.RefPoSubId,

                                // Incoming → Assembly
                                ItemIdIn = itemId,
                                QtyIn = qty,

                                // Outgoing → Component
                                ItemIdOut = compData.RawMaterialItemId,
                                QtyOut = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                            // 🔹 Reduce Issue Balance
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);


                            await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                            await _unitOfWork.SaveAsync();
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }

                        // 🔹 Update IssueTally
                        foreach (var scnId in issueSubs.Where(x => x.DcId > 0).Select(x => x.DcId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.SubConDCOutSubs
                                    .GetQueryable()
                                    .Where(x => x.DcId == RefDcId && x.TransType=="Out")
                                    .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(scnId);
                            if (issueHead != null)
                            {
                                issueHead.DcTally = (totalBalQty == 0);
                                await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
                                await _unitOfWork.SaveAsync();
                            }
                        }

                    }
                    else
                    {
                       
                        var issuedSubs = await _unitOfWork.SubConDCOutSubs
                                                .GetQueryable()
                                                .Where(x =>
                                                       x.DcId > 0
                                                   && DcOutSubIds.Contains(x.DcSubId)
                                                   && x.ItemId == compData.RawMaterialItemId
                                                     && x.BalQty > 0)
                                                    .OrderBy(x => x.DcId)
                                                    .ToListAsync();

                        foreach (var issue in issuedSubs)
                        {
                            if (componentNeeded <= 0)
                                break;

                            decimal availableQty = issue.BalQty??0;
                            decimal takeQty = Math.Min(availableQty, componentNeeded);

                            if (takeQty <= 0)
                                continue;

                            var track = new SubConGRNTrack
                            {

                                RefGRNSubId = dcSubId,
                                RefDCSubId = issue.DcSubId,
                                RefPoSubId = issue.RefPoSubId,

                                // Incoming → Assembly
                                ItemIdIn = itemId,
                                QtyIn = qty,

                                // Outgoing → Component
                                ItemIdOut = compData.RawMaterialItemId,
                                QtyOut = takeQty,

                                CreatedBy = currentUser,
                                CreatedDate = now
                            };

                            await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                            await _unitOfWork.SaveAsync();
                            issue.BalQty = availableQty - takeQty;
                            if (issue.BalQty < 0) issue.BalQty = 0;

                            await _unitOfWork.SubConDCOutSubs.UpdateAsync(issue);
                            await _unitOfWork.SaveAsync();
                            componentNeeded -= takeQty;
                        }

                        // 🔹 Update IssueTally
                        foreach (var scnId in issueSubs.Where(x => x.DcId > 0).Select(x => x.DcId).Distinct())
                        {
                            var totalBalQty = await _unitOfWork.SubConDCOutSubs
                                    .GetQueryable()
                                    .Where(x => x.DcId == RefDcId && x.TransType == "Out")
                                    .SumAsync(x => x.BalQty);

                            var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(scnId);
                            if (issueHead != null)
                            {
                                issueHead.DcTally = (totalBalQty == 0);
                                await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
                                await _unitOfWork.SaveAsync();
                            }
                        }

                    }


                    await _unitOfWork.SaveAsync();
                }

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"[CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync] Unexpected error ");
                throw new InvalidOperationException("Failed to a CreateTracksForSubsAndReduceIssueAndUpdateIssueTallyAsync. Please contact support.");
            }
        }
      
        public async Task<decimal?> GetDcOutBalQtyByDcSubId(int DcSubId)
        {
            try
            {
                return await _unitOfWork.SubConDCOutSubs
                    .GetQueryable()
                    .Where(x => x.DcSubId == DcSubId)
                    .Select(x => x.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error loading Dc balance for SubId: {DcSubId}");

                return 0;
            }
        }
        public async Task<decimal?> GetDcOutBalQtyByPoSubId(int POSubId)
        {
            try
            {
                return await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .Where(x => x.PoSubId == POSubId)
                    .Select(x => x.BalQty)
                    .FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(
                    ex,
                    $"Error loading Dc balance for POSubId: {POSubId}");

                return 0;
            }
        }
        private async Task CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync(int DcSubId, int ItemId, decimal Qty, string currentUser, DateTime now, List<SubConGRNSubVM?> SubConGRNSubVMs, bool Materialreturn)
        {
            try
            {
                bool PoWiseGrn = await IsPOWiseSubConDcOutEnabledAsync();

                var selectedRows = SubConGRNSubVMs
                    .Where(x => x.RefDcSubId.HasValue && x.Qty > 0)
                    .ToList();

               if(Materialreturn)
                {
                    selectedRows = selectedRows
                      .Where(x => x.TransType == "In")
                      .ToList();

                }
               else
                {
                    selectedRows = selectedRows
                      .Where(x => x.TransType == "Out")
                      .ToList();
                }  

                if (!selectedRows.Any())
                    throw new Exception("No manual issue rows selected.");

                decimal totalOutQty = selectedRows.Sum(x => x.Qty ?? 0);
                if (totalOutQty <= 0)
                    throw new Exception("Invalid outgoing quantity.");


                foreach (var row in selectedRows)
                {
                    var issueSub = await _unitOfWork.SubConDCOutSubs.GetAsync(row.RefDcSubId!.Value);
                    if (issueSub == null)
                        throw new Exception($"IssueSub not found : {row.RefDcSubId}");

                    decimal availableQty = 0;
                    availableQty = issueSub.BalQty??0;
                   
                    if (availableQty < row.Qty && availableQty < row.Qty)
                    {
                        // throw new Exception($"Insufficient balance for IssueSubId {issueSub.SCNSubId}");
                        return;
                    }

                    var track = new SubConGRNTrack
                    {

                        RefGRNSubId = DcSubId,
                        RefDCSubId = row.RefDcSubId,
                        RefPoSubId = row.RefPoSubId,
                        RefRcSubId=row.RefRcSubId,
                        // Incoming → Assembly
                        ItemIdIn = ItemId,
                        QtyIn = Qty,

                        // Outgoing → Component
                        ItemIdOut = row.ItemId,
                        QtyOut = row.Qty,

                        CreatedBy = currentUser,
                        CreatedDate = now
                    };


                    await _unitOfWork.SubConGRNTracks.CreateAsync(track);
                    await _unitOfWork.SaveAsync();

                    issueSub.BalQty = availableQty - row.Qty ?? 0m;


                    if (issueSub.BalQty < 0) issueSub.BalQty = 0;
                  

                    await _unitOfWork.SubConDCOutSubs.UpdateAsync(issueSub);
                    await _unitOfWork.SaveAsync();
                }


                var issueIdss = selectedRows
                    .Select(x => x.RefDcSubId)
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .Distinct();

                var affectedScnIds = await _unitOfWork.SubConDCOutSubs
                                 .GetQueryable()
                                 .Where(s => selectedRows
                                     .Select(r => r.RefDcSubId!.Value)
                                     .Contains(s.DcSubId))
                                 .Select(s => s.DcId)
                                 .Distinct()
                                 .ToListAsync();

                foreach (var DcId in affectedScnIds)
                {
                    decimal remainingBalQty = 0;

                    if (PoWiseGrn)
                    {
                        remainingBalQty = await _unitOfWork.SubConDCOutSubs
                            .GetQueryable()
                            .Where(s => s.DcId == DcId && s.TransType == "Out")
                            .SumAsync(s => (decimal?)s.BalQty) ?? 0;
                    }
                    else
                    {
                        remainingBalQty = await _unitOfWork.SubConDCOutSubs
                            .GetQueryable()
                            .Where(s => s.DcId == DcId)
                            .SumAsync(s => (decimal?)s.BalQty) ?? 0;
                    }

                    var issueHead = await _unitOfWork.SubConDCOuts.GetAsync(DcId);

                    if (issueHead != null)
                    {
                        issueHead.DcTally = remainingBalQty == 0;
                        await _unitOfWork.SubConDCOuts.UpdateAsync(issueHead);
                    }
                }

                await _unitOfWork.SaveAsync();

            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Failed to GetPropertyChanges in CreateTrackAndReduceIssueAndUpadteIssueTallyManualAsync");
            }
        }

        public async Task<List<Dictionary<string, object>>> LoadPoSubsByReturnMaterialPoIds(
                                                            List<int> poIds,
                                                            List<int> DcoutIds,
                                                            int storeIssId)
        {
            try
            {
                if (DcoutIds == null || DcoutIds.Count == 0)
                    return new List<Dictionary<string, object>>();

                // Get selected PO SubIds
                var poSubIds = await _unitOfWork.PurchPoSubs
                    .GetQueryable()
                    .AsNoTracking()
                    .Where(x => poIds.Contains(x.PoId))
                    .Select(x => x.PoSubId)
                    .ToListAsync();

                // OUT rows + matching IN rows
                var dcOutData = await (
                    from s in _unitOfWork.SubConDCOuts
                        .GetQueryable()
                        .AsNoTracking()

                    join outSub in _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .AsNoTracking()
                        on s.DcId equals outSub.DcId

                    join inSub in _unitOfWork.SubConDCOutSubs
                        .GetQueryable()
                        .AsNoTracking()
                        on outSub.DcId equals inSub.DcId

                    where DcoutIds.Contains(s.DcId)

                          && !s.Cancel

                          // OUT item
                          && !outSub.ItemCancel
                          && outSub.TransType != null
                          && outSub.TransType.ToUpper() == "OUT"
                          && outSub.BalQty > 0

                          // IN item linked to PO
                          && !inSub.ItemCancel
                          && inSub.TransType != null
                          && inSub.TransType.ToUpper() == "IN"

                          && inSub.RefPoSubId.HasValue
                          && poSubIds.Contains(inSub.RefPoSubId.Value)

                    select new
                    {
                        DcSubId = outSub.DcSubId,

                        outSub.DcId,

                        outSub.ItemId,

                        Qty = outSub.BalQty,
                        BalQty = outSub.BalQty,

                        RefPoSubId = inSub.RefPoSubId,

                        outSub.CostId,

                        ProjectNo = outSub.CostCenter.ProjectNo,

                        outSub.Item.ItemCode,
                        outSub.Item.ItemName,
                        outSub.Item.Specification,
                        outSub.Item.MeasureUnit,
                        outSub.Item.HSNCode,

                        Category = outSub.Item.Category.CategoryName,
                        outSub.Item.CategoryCode
                    }
                )
                .Distinct()
                .ToListAsync();

                // PO Details
                var refPoSubIds = dcOutData
                    .Where(x => x.RefPoSubId.HasValue)
                    .Select(x => x.RefPoSubId.Value)
                    .Distinct()
                    .ToList();

                var poDict = await (
                    from poSub in _unitOfWork.PurchPoSubs
                        .GetQueryable()
                        .AsNoTracking()

                    join po in _unitOfWork.PurchPos
                        .GetQueryable()
                        .AsNoTracking()
                        on poSub.PoId equals po.PoId

                    where refPoSubIds.Contains(poSub.PoSubId)

                          && !poSub.ItemCancel
                          && !po.PoCancl

                    select new
                    {
                        poSub.PoSubId,

                        PONo = po.PONo,
                        POSuffix = po.Suffix,
                        PODate = po.PODate,

                        poSub.UnitPrice
                    }
                ).ToDictionaryAsync(x => x.PoSubId);

                // Stock
                var itemIds = dcOutData
                    .Select(x => x.ItemId)
                    .Distinct()
                    .ToList();

                var stockDict = await _stockManagerService
                    .GetStockForItemsAsync(itemIds, storeIssId);

                // Final Result
                var result = dcOutData.Select(x =>
                {
                    stockDict.TryGetValue(x.ItemId, out decimal stockQty);

                    poDict.TryGetValue(x.RefPoSubId ?? 0, out var po);

                    return new Dictionary<string, object>
                    {
                        ["Selected"] = false,

                        ["DcSubId"] = x.DcSubId,

                        ["RefPoSubId"] = x.RefPoSubId,

                        ["PoNo"] = po != null
                            ? (po.PONo ?? "") + (po.POSuffix ?? "")
                            : "",

                        ["PODate"] = po?.PODate,

                        ["UnitPrice"] = po?.UnitPrice ?? 0,

                        ["ItemId"] = x.ItemId,

                        ["ItemCode"] = x.ItemCode ?? "",
                        ["ItemName"] = x.ItemName ?? "",
                        ["Specification"] = x.Specification ?? "",

                        ["UOM"] = x.MeasureUnit ?? "",

                        ["HSNCode"] = x.HSNCode ?? "",

                        ["Category"] = x.Category ?? "",

                        ["Qty"] = x.Qty,
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
        public async Task<List<PurchPoVM>> GetOpenPurchPosByReturnMaterialVendor(int vendorCode)
        {
            try
            {
                var result = await (
                    from po in _unitOfWork.PurchPos.GetQueryable().AsNoTracking()

                    join pos in _unitOfWork.PurchPoSubs.GetQueryable().AsNoTracking()
                        on po.PoId equals pos.PoId

                    join dcIn in _unitOfWork.SubConDCOutSubs.GetQueryable().AsNoTracking()
                        on pos.PoSubId equals dcIn.RefPoSubId

                    where po.VendorCode == vendorCode

                          && !po.PoCancl
                          && !pos.ItemCancel

                          // IN row contains RefPoSubId
                          && dcIn.TransType == "In"
                          && !dcIn.ItemCancel
                          && dcIn.RefPoSubId > 0

                          // SAME DCID must contain pending OUT item
                          && _unitOfWork.SubConDCOutSubs.GetQueryable().Any(dcOut =>
                                 dcOut.DcId == dcIn.DcId
                                 && dcOut.TransType == "Out"
                                 && !dcOut.ItemCancel
                                 && dcOut.BalQty > 0)

                    select new
                    {
                        po.PoId,
                        po.PONo,
                        po.Suffix
                    }
                )
                .Distinct()
                .OrderBy(x => x.PONo)
                .Select(x => new PurchPoVM
                {
                    PoId = x.PoId,
                    PONo = (x.PONo ?? "") + (x.Suffix ?? "")
                })
                .ToListAsync();

                return result;
            }
            catch
            {
                throw;
            }
        }

        public async Task UpsertSubConGRNShortCloseAsync(SubConGRNVM SubConGRNVMs)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var existingGRN = await _unitOfWork.SubConGRNs.GetAsync(SubConGRNVMs.GRNId);
                if (existingGRN == null)
                    throw new InvalidOperationException("Sales GRN not found.");

                existingGRN.ShortClose = SubConGRNVMs.ShortClose;

                await _unitOfWork.SubConGRNs.UpdateAsync(existingGRN);
                await _unitOfWork.SaveAsync();

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

        public async Task<bool> CheckIsSameAsOutItemByPoSubId(int poSubId)
        {
            var dc = await _unitOfWork.SubConDCOutSubs
                 .GetQueryable()
                 .AsNoTracking()
                 .Where(x => x.RefPoSubId == poSubId)
                 .Where(x => !x.SubConDcOut.Cancel && !x.SubConDcOut.ShortClose)
                 .OrderByDescending(x => x.SubConDcOut.DcId)
                 .Select(x => new
                 {
                     x.SubConDcOut.IsOutItemSameAsInItem
                 })
                 .FirstOrDefaultAsync();

            return dc?.IsOutItemSameAsInItem ?? false;
        }

        public async Task<List<SubContractGRNPendingVM>> GetSubContractGrnPendingList(string status)//Shankar
        {
            try
            {
                var result = await _commonService.ExecuteStatusSPAsync<SubContractGRNPendingVM>("Sp_GetSubContractDcGRNPendingList", status);
                return result.ToList();

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> IsDocumentUploaded(int invId)
        {
            try
            {
                return await _unitOfWork.Correspondances.GetQueryable()
                    .AnyAsync(c =>
                        c.ReferenceType == "Sub-Contrect GRN" &&
                        c.DocumentType == "Correspondence" &&
                        c.ReferenceId == invId);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in IsDocumentUploaded()");
                return false;
            }
        }
    }
}
