using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService
{
    public interface ISCNGenService
    {
        // 🔹  Item data reused from Common Service
        Task<int> GetDecimalPlacesAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<IEnumerable<Store>> GetAllActiveStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<List<ItemVM>> GetAllAssembliesAsync();
        Task<List<ItemVM>> GetAllSubAssembliesAsync();
        Task<List<CostCenterVM>> GetAllCostCenterDetails();
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<ItemVM>> GetItemVMsByItemIdsAsync(List<int> itemIds);
        Task<Dictionary<string, int>> GetItemIdsByItemCodesAsync(List<string> itemCodes);
        Task GetOrCreateHSNAsync(string hsnCode);
        Task<Companydetails?> GetCompanyDetailsAsync();
        Task<List<ItemVM>> GetAllSubAssembliesByAssyIdAsync(int assyId);




        // 🔹 Store Transfer Note specific logic
        Task<(List<SCNGenVM> sCNGenVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<decimal> GetStockForItemAsync(int itemId, int storeId);
        Task<int> CreateNewItemAsync(Item newItem);
        Task<bool> CheckIfTransactionsMadeAsync(
                        List<int> itemIds,
                        int storeId,
                        int screenCode,
                        List<int> refSubItemIds);
        Task<SCNGenVM?> GetSCNBySCNGenIdAsync(int scnGenId);
        Task<string> GetSCNGenNumberAsync(string suffix);
        Task<Dictionary<int, decimal>> GetBulkLastUnitPricesAsync(List<int> itemIds);
        Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(
                        int screenCode,
                        int storeId,
                        int itemId,
                        int subItemRefId);
        Task DeleteAndResequenceAsync(SCNGenSubVM subitem, SCNGenVM scnGenVM, int screenCode);
        Task<List<SCNGenSubVM>> GetSCNSubBySCNGenIdAsync(int scnGenId, int storeId);
        Task<SCNGenVM> UpsertSCNAsync(SCNGenVM scnGenVM, int screenCode);
        Task<List<SCNGenSubVM>> GetAssemblyRelatedItemsAsync(int assyId, decimal ReqQty, int storeId);
        Task<IEnumerable<SCNGenVM>> GetAllSCNGenDetailsAsync();
        Task<(bool CanDelete, string Message)> CanDeleteSCNGenAsync(int scnGenId, int screenCode);
        Task<bool> DeleteSCNGenBySCNGenIdAsync(int scnGenId , int screenCode);



    }
}
