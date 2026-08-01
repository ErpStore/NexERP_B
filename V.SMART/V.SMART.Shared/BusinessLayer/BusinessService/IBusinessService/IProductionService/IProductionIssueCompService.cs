using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.ProdCompStatusVM;



namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IProductionService
{
    public interface IProductionIssueCompService
    {
        Task<CustomerVM?> GetCustomerByIdAsync(int CustId);
        Task<List<Store>> GetAllActiveStoresAsync();
        Task<List<VendorContact>> GetContactPersonsVendorAsync(int Vendorcode);
        Task<List<VendorInDirect>> GetConsigneeAddressesVendorAsync(int VendorCode);
        Task<IEnumerable<CustomerVM>> SearchCustomers(string searchText);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<int> GetDecimalPlacesAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<List<Store>> GetAllAddStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<(List<ProductionIssueCompVM> dcout, int totalCount)> GetPagedDcSubConOutAsync(int pageNumber, int pageSize, string search);
        Task<ProductionIssueCompVM> GetDcSubConByIdAsync(int DcId);
        Task<List<ProductionIssueCompSubVM>> GetDcSubConByDcidAsync(int Dcid);
        Task DeleteAndResequenceAsync(ProductionIssueCompSubVM subitem, ProductionIssueCompVM dcVM,int screenCode);
        Task<decimal> GetStockForItemsAsync(int itemId, int storeId);

        Task<decimal> GetAvailableStockByItemIdAndRcAndScreenAsync(int itemId, int? storeId, int rcSubId);

        Task<List<Machine>> GetAllActiveMachinesAsync();


        /////////////

        Task<decimal> GetMfgPoProdCompBalQtyFromPoSubId(int poSubId);
        Task<List<Dictionary<string, object>>> GetAllOpenRcAsync(int storeId, int? custId);
        Task<List<ProductionIssueCompSubVM>> GetDistinctRefRouteCardByIssueIdAsync(int issueId);
        Task<ProductionIssueCompSubVM?> GetProductionIssueSubItemDetailByIssueSubIdAsync(int issueSubId);
        Task<decimal> GetRcItemBalQtyFromRcSubId(int rcSubId);
        Task<ProductionIssueCompSubVM?> GetSubItemDetailByIssueIdAsync(int issueSubId);
        Task<ProductionIssueCompVM?> GetProductionByIssueIdAsync(int issueId);
        Task<bool> DeleteProdCompIssueByIssueIdAsync(int issueId, int screenCode);
        Task<(List<ProductionIssueCompVM> issueCompVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
                    Dictionary<string, object>? filters);
        Task<ProductionIssueCompVM> UpsertProdIssueComp(ProductionIssueCompVM issueVM, int screenCode);
        Task<List<Dictionary<string, object>>> GetPoDetailsByCustId(int custId, int? storeId);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int? custId);
        Task<List<AssemblyDefVM>> GetAssemblyItemsAsync(int assemblyId);
        Task<int> GetPendingRcCountAsync();
        Task<string> GetMaterialIssueNumberAsync(string suffix);
        Task<int> GetPendingPoCountAsync(int VendorCode);
        Task<decimal> GetPoItemBalQtyFromPoSubId(int poSubId);
        Task<bool> IsOpenPoAsync(int poSubId);
      
        Task<List<ProductionIssueCompStatusVM>> GetProductionIssueComponentStatusListAsync(string status);

        Task<List<BOMAssemblySpVM>> GetBOMAssemblyItems(int assId, int processId, int storeId);

        Task<bool> IsBOMAsync(int rcSubId, int processId);

        Task UpdatedCancelStatusAndAddOrRevertQty(ProductionIssueCompVM dcOutVM, int screenCode);
        Task ValidatePoBalanceBeforeRevertAsync(ProductionIssueCompSub sub);
        Task ValidateRcBalanceBeforeRevertAsync(ProductionIssueCompSub sub);
        Task ProductionCompShortCloseAsync(ProductionIssueCompVM ProductionIssueCompVM);
        Task UpdateIssueTallyStatusAsync(int IssueId);
        Task<bool> IsProductionTransactionsMatchedwilecancelAsync(int issueid, ProductionIssueCompVM ProductionIssueCompVM);
        Task<List<ProductionIssueCompSub>> GetProductionComSubDetailsByIssueIdAsync(int IssueId);
        Task<bool> GetIsSameItemasInAsync();
        Task<CompMasterVM?> GetDefaultRawMaterialItemAsync(int compItemId);

        Task<(bool CanDelete, string Message)> CanDeleteProductionDcgoingAsync(int IssueId, int screenCode);
        Task<List<ItemVM>> SearchAssemblyItemsAsync(string value);

        Task<List<Dictionary<string, object>>> GetPOGRNSCNItemDetailsByCustId(int custId, int storeId);

        Task<bool> CheckIsSameAsOutItemBylabGrnSubId(int GrnSubId);

        Task<List<LabourGRNSubVM>> GetLabourGRNInItemsAsync(List<int> grnIds);

        Task<bool> GetIsLabourGrnLinkAsync();
        Task ValidateLabGrnBalanceBeforeRevertAsync(ProductionIssueCompSub sub);



    }
}
