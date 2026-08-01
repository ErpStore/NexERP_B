using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IOutSourcingService.IMaterialRequisitionService
{
    public interface IMaterialReqService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<List<string>> GetAllActiveUserNamesAsync();
        Task<int> GetDecimalPlacesAsync();
        Task<bool> GetLineIdByCustomerAsync(int custId);

        Task<VendorVM?> GetVenderByVendorCodeAsync(int? vendorCode);
        Task<IEnumerable<VendorVM>> SearchVendorsAsync(string searchText);
        Task<List<CustomerVM>> GetCustomersAsync(int? custId = null);
        Task<CustomerVM?> GetCustomerByIdAsync(int custId);
        Task<IEnumerable<CustomerVM>> SearchCustomersAsync(string searchText);

        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<(int StoreId, string StoreName)>> GetAllStoreNameAndIdAsync();
        Task<List<Staff>> GetAllStaffAsync();
        Task<Currency?> GetCurrencyByIdAsync(int currId);
        Task<decimal?> GetLatestCurrencyValueAsync(int currId);
        Task<List<ContactPerson>> GetContactPersonsAsync(int custId);
        Task<List<CustomerIndirect>> GetConsigneeAddressesAsync(int custId);
        Task<List<CostCenterVM>> GetCostCenterDetailsByCustId(int custId, HashSet<int> usedCostCenterIds);
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<bool> CheckAssemblyExistsAsync(int itemId);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int? storeId);


        // 🔹 Material-Requisition specific logic

        Task ValidateBeforeRevertAsync(int mreqSubId);
        Task ValidateBOMBalanceBeforeRevertAsync(MaterialReqSub sub);
        Task ValidatePoBalanceBeforeRevertAsync(MaterialReqSub sub);
        Task<List<MaterialReqSub>> GetMaterialReqSubByMreqIdAsync(int mreqId);

        Task<(bool CanItemCancel, string Message)> CanPRItemCancelCheckAsync(MaterialReqSubVM subItem);
        Task UpsertPRShortCloseAsync(MaterialReqVM materialReqVM);
        Task<bool> IsMreqTransactionsMatchedAsync(int mreqId, MaterialReqVM materialReqVM);
        Task<List<Dictionary<string, object>>> GetPoDetailsAsync();
        Task<(List<MaterialReqVM> mreqVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<string> GetMReqNumberAsync(string suffix);
        Task<List<string>> GetAllDepartment();
        Task<IEnumerable<MaterialReqVM>> GetAllMreqDetailsAsync();
        Task<(bool CanDelete, string Message)> CanDeleteMaterialReqAsync(int mreqId);
        Task<bool> DeleteMreqByMreqIdAsync(int mreqId);
        Task<MaterialReqVM?> GetMreqByMreqIdAsync(int mreqId);
        Task<decimal> GetPOItemIndentBalQtyFromPoSubId(int poSubId);
        Task<MaterialReqSubVM?> GetMaterialReqSubItemDetailByMreqSubIdAsync(int mreqSubId);
        Task UpdateItemCancelAndAddorRevertAsync(MaterialReqSubVM subItem);
        Task DeleteAndResequenceAsync(MaterialReqSubVM subitem, MaterialReqVM mreqVM);
        Task<MaterialReqVM> UpsertMaterialReqAsync(MaterialReqVM mreqVM);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds, int? custId);
        Task UpdatedCancelStatusAndAddOrRevertQty(MaterialReqVM materialReqVM);
        Task<decimal> GetROLByItemIdAsync(int itemId);
        Task<List<Dictionary<string, object>>> GetRolitemDetailAsync();
        Task<List<Dictionary<string, object>>> GetRolitemDetailsAsync(int storeid);

        Task<List<MaterialReqPendingVM>> GetMreqReqPendingList(string status);

    }
}
