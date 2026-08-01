using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService
{
    public interface ICommonService
    {
        #region Users
        Task<List<string>> GetAllActiveUserNamesAsync();
        #endregion

        #region Defects
        Task<List<DefectInfo>> GetAllActiveDefectsAsync();
        #endregion


        #region General / Configuration

        Task<int> GetDecimalPlacesAsync();
        Task<Companydetails?> GetCompanyDetailsAsync();

        #endregion

        #region User Authority

        Task<UserAuthority?> GetUserAuthorityAsync(string userName);

        #endregion

        #region State

        Task<IEnumerable<State>> GetStatesAsync();

        #endregion

        #region Currency

        Task<IEnumerable<Currency>> GetCurrenciesAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);
        Task<decimal?> GetLatestCurrencyValueAsync(int currId);

        #endregion

        #region Customer
        Task<bool> GetLineIdByCustomerAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);
        Task<List<CustomerVM>> GetAllActiveCustomersAsync();

        Task<List<CustomerVM>> GetAllActiveNonExportCustomersAsync();
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);

        Task<IEnumerable<CustomerVM>> SearchExportCustomersAsync(string searchText);
        Task<CustomerVM?> GetExportCustomerByIdAsync(int custId);
        Task<List<CustomerVM>> GetExportAllActiveCustomersAsync();

        #endregion

        #region Vendor
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<List<VendorVM>> GetAllActiveVendorsAsync();
        Task<VendorVM?> GetVendorByVenerCodeAsync(int vendorCode);
        Task<VendorInDirect?> GetConsigneeByAltvendorCodeAsync(int AltVendorCode);
        Task<List<VendorContact>> GetContactPersonsVendorAsync(int VendorCode);
        Task<List<VendorInDirect>> GetConsigneeAddressesVendorAsync(int VendorCode);
        #endregion

        #region staff
        Task<List<Staff>> GetAllStaffAsync();
        #endregion

        #region Categories
        Task<List<Category>> GetAllActiveCategoriesAsync();

        Task<int?> GetItemCategoryCodeByItemIdAsync(int itemId);

        #endregion

        #region Item

        Task<List<ItemVM>> GetAllItemsByCategoryCode(int Id);
        Task<IEnumerable<ItemVM>> SearchRawMaterialItemsAsync(string searchText);
        Task<List<ItemVM>> GetAllActiveItemsByCategooryAsync(string category);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText, int? customerId = null);
        Task<IEnumerable<ItemVM>> SearchComponentItemsAsync(string searchText);
        Task<List<ItemVM>> GetAllActiveItemsAsync();
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<IEnumerable<ItemVM>> SearchOnlyAssyItemsAsync(string searchText);
        Task<IEnumerable<ItemVM>> SearchAssyAndSubAssyItemsAsync(string searchText);
        Task<Dictionary<string, int>> GetItemIdsByItemCodesAsync(List<string> itemCodes);
        Task<List<ItemVM>> GetItemVMsByItemIdsAsync(List<int> itemIds);
        Task<IEnumerable<ItemVM>> SearchSubAssyAndItemsAsync(string searchText);

        #endregion

        #region RawMaterial
        Task<List<RawMaterial>> GetRMAsync();
        #endregion

        #region Terms & Conditions

        Task<List<TermsAndConditions>> GetAllActiveTermsAsync();

        #endregion

        #region Contact Person

        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);

        #endregion

        #region Consignee

        Task<CustomerIndirect?> GetConsigneeByAltCustIdAsync(int altCustId);
        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);

        #endregion

        #region Cost Center

        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<List<CostCenterVM>> GetAllCostCenterDetails();
        #endregion

        #region Correspondence & Attachments

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);

        #endregion

        #region Stores

        Task<IEnumerable<Store>> GetAllAddStoresAsync();
        Task<List<(int StoreId, string StoreName)>> GetAllStoreNameAndIdAsync();
        Task<IEnumerable<Store>> GetAllActiveStoresAsync();
        Task<IEnumerable<Store>> GetAllIssueStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        #endregion

        #region Banks
        Task<List<Banks>> GetAllActiveBanksDetailaAsync();


        #endregion

        #region Assembly Definition

        Task<List<ItemVM>> GetAllAssembliesAsync();
        Task<List<ItemVM>> GetAllSubAssembliesByAssyIdAsync(int assyId);
        Task<List<ItemVM>> GetAllSubAssembliesAsync();
        #endregion

        #region Screens

        Task<List<Screens>> GetAllScreenAsync();
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);

        #endregion

        #region UOMs
        Task<List<UOM>> GetUOMsAsync();
        #endregion

        #region HSN Master

        Task<List<HSNMaster>> GetAllHSNAsynce();
        Task<bool> HSNExistsAsync(string hsnCode);

        #endregion

        #region Process
        Task<IEnumerable<Process>> GetAllProcessAsync();
        Task<IEnumerable<Process>> GetAllProcessDetailsAsync();

        #endregion

        #region Print Settings
        Task<List<PrintSetting>> GetPrintSettingsDetailsAsync(string screenName);

        #endregion

        #region Machine

        Task<List<Machine>> GetAllMachineAsync();

        #endregion

        #region shifts
        Task<IEnumerable<ShiftAllocation>> GetAllShiftsAsync();

        #endregion

        #region Screen Management
        Task<bool> GetScreenPermissionsAsync(string screenName, string displayName);

        #endregion

        #region  Inspection
        Task<List<InspectionRowVM>> GetInspectionRowVMsAsync(int ItemId, int ProcessId);

        Task<List<InspectionRowVM>> GetFinalInspectionRowVMsAsync(int ItemId);
        Task<List<InspectionRowVM>> GetIncomingInspectionRowVMsAsync(int ItemId, int ProcessId);

        Task<int> GetNoOfSamples();
        #endregion


        #region DC Outgoint Type(For Generating Auto Running No.)
        Task<string> GenerateAutoRunningNoAsync(string dcType, string Suffix);

        //Preview AutoDcNumber
        Task<string> GeneratePreviewAutoDcRunningNoAsync(string dcType, string suffix);

        Task<string> GenerateInvoiceAutoRunningNoAsync(string invType, string Suffix);

        //Preview AutoInvoiceNumber
        Task<string> GeneratePreviewInvoiceAutoRunningNoAsync(string invType, string Suffix);
        #endregion


        #region Converting Base64 to QR Code
        Task<string> GenerateQrBase64(string signedQrText);

        Task<bool> CheckPrefixValid(string ScreenName);

        Task<bool> CheckEwayRequired(string ScreenName);
        #endregion


        Task<string> GetBasicDirectory();

        Task<IEnumerable<T>> ExecuteStatusSPAsync<T>(string spName, string? status) where T : class;
        Task<bool> GetRejectionSelectionEnableAsync();
        Task<List<RejectionMasterVM>> GetAllRejectionReasonAsync();
        Task<RejectionMasterVM?> GetRejectionReasonByIdAsync(int? rejectionId);



        #region Candidate
        Task<IEnumerable<CandidateVM>> SearchCandidatesAsync(string searchText);

        Task<CandidateVM?> GetCandidateByIdAsync(int custId);

        Task<List<CandidateVM>> GetAllActiveCadidatesAsync();

        #endregion
    }
}
