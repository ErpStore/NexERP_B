using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractGRNService
{
    public interface ISubConGRNService
    {
        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<VendorVM?> GetVendorByVenerCodeAsync(int vendorCode);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<TermsAndConditions>> GetTermsAsync();
        Task<List<Currency>> GetCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);
        Task<int> GetPodetailsForVendor(int vendorcode);
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<int> GetDecimalPlacesAsync();
        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);
        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllActiveStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<List<Machine>> GetAllActiveMachinesAsync();


        // Subcontract GRN - specific logic
        Task<List<SubConGRNSubVM>> GetSubConGrnSubVMsByDcIdsAsync(List<int> dcIds);
        Task<List<int>> GetDcIdsByDcSubIdsAsync(List<int> dcSubIds);
        Task<List<int>> GetSubConDcOutIdsByDcSubIdAsync(int dcSubId);
        Task<List<ItemVM>> GetRawMaterialItemsAsync(int compItemId);
        Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId);
        Task<List<SubConGRNSub>> GetSubcontractGRNSubByGRNIdAsync(int grnId);
        Task<SubConGRNVM?> GetSubcontractGRNByGRNIdAsync(int grnId);
        Task<string> GetSubconGRNNoAsync(string suffix);
        Task<bool> DeleteSubconGRNByIdAsync(int GRNId, int screenCode);
        Task<(bool CanDelete, string Message)> CanDeleteSubconGRNAsync(int GRNId, int screenCode);

        Task<(List<SubConGRNVM> SubconGRNVMs, int TotalCount)>
                          SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                          Dictionary<string, object>? filters);
        Task<List<int>> GetDcIdsByDcSubIdAsync(int DcSubId);
        Task<List<Dictionary<string, object>>> GetDCDetailsByVendorId(int vendorCode);
        Task<List<SubConGRNSubVM>> GetSubConDcOutDataByDcIdsAsync(List<int> dcIds);
        Task<List<SubConGRNSubVM>> GetSubConDcOutDataByDcIdAndRmIdAsync(List<int> dcIds, int rmItemId);
        Task<int?> GetDefaultRawMaterialItemIdAsync(int compItemId);
        Task<List<int>> GetDistinctDcIdsByPoSubIdsAsync(List<int> poSubIds);
        Task<List<SubConDcOutSubVM>> GetSubConDcOutItemsByPoSubAsync(int itemId, int poSubId);
        Task<decimal> GetPossibleAssemblyQtyAsync(int itemId, Dictionary<int, decimal?> issuedLookup, decimal balQty);
        Task<decimal> GetPossibleComponentQtyAsync(int itemId, Dictionary<int, decimal?> issuedLookup, decimal balQty);
        Task<RouteCardSub?> GetRcDetailsByRcSubIdAsync(int rcSubId);
        Task<List<Dictionary<string, object>>> GetAllOpenRcDetailsAsync(int? custId);
        Task<SubConGRNVM> UpsertSubConGRNAsync(SubConGRNVM prodReturnCompVM, int screenCode);

        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId);
        Task<List<int>> GetIssueIdsByRcSubIdAsync(int rcSubId);
        Task<List<SubConGRNTrackVM>> GetAllExistingRouteCardAsync();
        Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(
                        int screenCode,
                        int storeId,
                        int itemId,
                        int subItemRefId);
        Task<int> GetPendingJobOrdersCountAsync();
        Task<(bool IsValid, string Message)> ValidateDeleteAsync(int jobId, int itemId, decimal qtyReturned, int addStoreId);
        Task<decimal?> GetIssueBalQtyByIssueSubId(int issueSubId);
        Task<List<Dictionary<string, object>>> GetAllProductionIssuedItemsAsync();

        Task<decimal> GetJobOrderBalQtyFromJobId(int jobId);
        Task<SubConGRNSubVM?> GetProductionReturnSubItemDetailByReturnSubIdAsync(int returnSubId);
        Task<SubConGRNSubVM?> GetProdReturnSubItemDetailByReturnSubIdAsync(int returnSubId);
        Task<bool> DeleteProdAssyReturnByReturnIdAsync(int returnId, int screenCode);
        Task<int> GetDcdetailsForVendor(int vendorCode);

        Task<SubConGRNSubVM?> GetProdReturnSubItemDetailByOutGoingSubIdAsync(int IssueSubId);

        Task<bool> DeleteByDcsubConIncomingSubIdAsync(int returnSubId, int screenCode);

        Task<(bool CanDelete, string Message)> CanRemoveSubConGRNAsync(int ReturnId, int ReturnSubId);
       
        Task<bool> IsPOWiseSubConDcOutEnabledAsync();
        Task<decimal> GetRawMaterialWeightAsync(int compItemId);
        Task<List<Dictionary<string, object>>> GetPoDetailsByVendor(List<int> poIds,  int VendorCode, int? storeId);
        Task<List<SubConDcOutVM>> LoadDcOutNumbersPoWiseAsync( List<int> poIds,  string check,  bool receivedReturn);
        Task<int> GetPendingPoCountAsync(int vendorCode);
        Task<List<PurchPoVM>> GetOpenPurchPosWiseByVendor(int VendorCode);
        Task<int> GetPendingDcOutsPoCountAsync(int VendorCode);
        Task<List<SubConDcOutSubVM>> GetSubConDcOutQtyByDcIdsAsync(List<int> DcIds);
        Task<List<SubConDcOutSubVM>> GetDcSubConOutQtyByDcIdsAsync(List<int> SubConOutIds);
        Task<List<Dictionary<string, object>>> GetDCDetailsByWithOutPoVendorId(int vendorCode);
        Task<List<Dictionary<string, object>>> GetDCDetailsByReturnVendorId(int vendorCode);
        Task<decimal?> GetDcOutBalQtyByDcSubId(int DcSubId);
        Task<decimal?> GetDcOutBalQtyByPoSubId(int POSubId);
        Task<List<Dictionary<string, object>>> LoadPoSubsByReturnMaterialPoIds(List<int> poIds, List<int> DcoutIds, int storeIssId);

        Task<List<PurchPoVM>> GetOpenPurchPosByReturnMaterialVendor(int vendorCode);
        Task UpdatedCancelStatusAndAddOrRevertQtyPoWiseAync(SubConGRNVM dcVM, int screenCode);
        Task UpsertSubConGRNShortCloseAsync(SubConGRNVM SubConGRNVMs);
       Task ValidateSubConDcoutBalanceBeforeRevertAsync(SubConGRNSub sub, SubConGRNVM SubConGRNVMs);
        Task ValidateDcOutogoingItemBalanceBeforeRevertAsync(SubConGRNSub sub);
        Task<List<int>> GetDcIdsByRcSubIdsAsync(List<int> RcSubIds);
        Task<List<SubConDcOutSubVM>> GetSubConRcDetailsByDcIdsAsync(List<int> dcIds);
        Task<List<int>> GetDistinctRcSubIdsWithPendingIssueDcAsync();
        Task<bool> CheckIsSameAsOutItemByPoSubId(int poSubId);

        Task<List<SubContractGRNPendingVM>> GetSubContractGrnPendingList(string status);//Shankar;

        Task<bool> IsDocumentUploaded(int invId);
    }
}
