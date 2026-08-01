using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.EWayModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.ReportViewModel.OutSourcingRptVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.ISubContractDcOutservice
{
    public interface ISubConDcOutService
    {
        Task<CustomerVM?> GetCustomerByIdAsync(int CustId);
        Task<List<Store>> GetAllActiveStoresAsync();
        Task<VendorVM?> GetVendorByIdAsync(int VendorCode);
        Task<List<VendorContact>> GetContactPersonsVendorAsync(int Vendorcode);
        Task<List<VendorInDirect>> GetConsigneeAddressesVendorAsync(int VendorCode);
        Task<IEnumerable<CustomerVM>> SearchCustomers(string searchText);
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetDecimalPlacesAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllAddStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<(List<SubConDcOutVM> dcout, int totalCount)> GetPagedDcSubConOutAsync(int pageNumber, int pageSize, string search);
        Task<SubConDcOutVM> GetDcSubConByIdAsync(int DcId);
        Task DeleteAndResequenceAsync(SubConDcOutSubVM subitem, SubConDcOutVM subConDcOutVM, int screenCode);
        Task<decimal> GetStockForItemsAsync(int itemId, int storeId);

        Task<decimal> GetAvailableStockByItemIdAndRcAndScreenAsync(int itemId, int? storeId, int rcSubId);

        Task<List<Machine>> GetAllActiveMachinesAsync();



        /////////////

        Task<(List<SubConDcOutVM> subConDcOutVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteSubconDcOutgoingAsync(int dcId, int screenCode);
        Task<bool> DeleteSubconDcByDcIdAsync(int dcId, int screenCode);

        Task UpdateItemCancelAndAddorRevertAsync(SubConDcOutSubVM subItem,int screenCode);
        Task<(bool CanItemCancel, string Message)> CanSubconDcItemCancelCheckAsync(SubConDcOutSubVM subItem);
        Task<bool> IsSubConDcTransactionsMatchedAsync(int DcId, SubConDcOutVM subConDcOutVM);
        Task<decimal> GetRouteCardSubBalQtyFromRcSubId(int rcSubId);
        Task<decimal> GetPurchPoItemBalQtyFromPoSubId(int poSubId);
        Task<CompMasterVM?> GetDefaultRawMaterialItemAsync(int compItemId);
        Task<SubConDcOutSubVM?> GetSubConDcOutDetailsByDcSubIdAsync(int dcSubId);
        Task<List<SubConDcOutSubVM>> GetSubConDcOutSubsByDcIdAsync(int dcId);
        Task<decimal> GetRcItemBalQtyFromRcSubId(int rcSubId);
        Task<SubConDcOutSubVM?> GetSubItemDetailByIssueIdAsync(int issueSubId);

        Task<SubConDcOutVM?> GetSubConDcOutByDcIdAsync(int dcId);
        Task<bool> DeleteProdCompIssueByIssueIdAsync(int issueId, int screenCode);
        Task<SubConDcOutVM> UpsertDeliveryChallan(SubConDcOutVM issueVM, int screenCode);
        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId, int? storeId);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int? custId);
        Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId);
        Task<int> GetPendingRcCountAsync();
        Task<string> GetMaterialIssueNumberAsync(string suffix);
        Task<List<Dictionary<string, object>>> GetAllOpenRcAsync(int storeId,int vendorcode);
        Task<int> GetPendingPoCountAsync(int VendorCode);
        Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId);
        Task<bool> IsOpenPoAsync(int poSubId);

        ///////////Cancel DeleteValidation
        Task<(bool CanDelete, string Message)> CanDeleteSubConDcOutgoing(int IssueId);

        Task<(bool CanDelete, string Message)> CanRemoveSubConDcOutAsync(int IssueId, int IssuedIdSub);

        Task ValidatePurchaseEnquiryBalanceBeforeRevertAsync(SubConDcOutSubVM sub);

        Task UpdatedCancelStatusAndAddOrRevertQty(SubConDcOutVM quoteVM, int screenCode);

        Task UpdateDcTallyStatusAsync(int QuoteId);

        Task<SubConDcOut> UpsertPurchaseQuoteShortCloseAsync(SubConDcOutVM dcSubCon);

        Task<List<SubConDcOutSub>> GetSubConDcSubDetailsByDcIdAsync(int dcId);

        Task ValidatePoBalanceBeforeRevertAsync(SubConDcOutSub sub);
        Task ValidateRcBalanceBeforeRevertAsync(SubConDcOutSub sub);


        //------------**********Dc Details for EWAY *****-----------------------\\
        Task UpsertSubConDcShortCloseAsync(SubConDcOutVM subConDcOutVM);
        Task<List<EWayDocument>> GetSubConDcByVendorCodeAsync(int VendorCode);
        Task<bool> IsSubConDcTransactionsMatchedwilecancelAsync(int DcId, SubConDcOutVM subConDcOutVM);
        Task<bool> GetIsSameItemasInAsync();

        Task<bool> IsDocumentUploaded(int Dcid);

        Task<List<SubContractDcPendingVM>> GetSubContractDcoutPendingList(string status);
    }
}
