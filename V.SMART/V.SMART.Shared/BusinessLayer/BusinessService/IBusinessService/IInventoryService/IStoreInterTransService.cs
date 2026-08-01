using V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.StoreInterTransferViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService
{
    public interface IStoreInterTransService
    {
        // 🔹 Customer + Item data reused from Common Service
        Task<decimal> GetStockQtyFromStockManager(int ItemId, int StoreId);
        Task<int> GetDecimalPlacesAsync();
        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);
        Task<List<ItemVM>> GetItemVMsByItemIdsAsync(List<int> itemIds);
        Task<Dictionary<string, int>> GetItemIdsByItemCodesAsync(List<string> itemCodes);
        Task<IEnumerable<ItemVM>> SearchItemsAsync(string searchText);
        Task<List<StoreInterTrans>> InstantSearchInterStoreTransAsync(StoreInterTransVM search);

        Task<IEnumerable<Store>> GetAllActiveStoresAsync();
        Task<(int StoreId, string StoreName)> GetMappedStoreForFormAsync(string formName);
        Task<List<CostCenterVM>> GetAllCostCenterDetails();
        Task<int> GetScreenCodeByScreenNameAsync(string screenName);
        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int storeId);
        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<string>> GetUOMsAsync();
        Task<Companydetails?> GetCompanyDetailsAsync();

        // 🔹 Store Transfer Note specific logic
        Task<IEnumerable<StoreInterTransVM>> GetAllISTDetailsAsync();
        Task<(bool CanDelete, string Message)> CanDeleteISTAsync(int istId);
        Task<bool> DeleteISTByISTIdAsync(int istId, int screenCode);
        Task<StoreInterTransVM?> GetISTByISTIdAsync(int istId);
        Task<string> GetISTInterStoreNumberAsync(string suffix);
        Task<StoreInterTransSubVM?> GetISTSubItemDetailByISTSubIdAsync(int istSubId);
        Task DeleteAndResequenceAsync(StoreInterTransSubVM subitem, StoreInterTransVM istVM, int screenCode);
        Task<List<StoreInterTransSubVM>> GetISTSubByISTIdAsync(int istId, int storeId);
        Task<StoreInterTransVM> UpsertISTAsync(StoreInterTransVM istVM, int screenCode);
        Task<(bool CanDelete, string Message)> ToCheck_Stock_Qty_Issued(int ISTId, int screenCode, string refNo);


        Task<bool> IsRouteCardStockExistsAsync(int itemId);

    }
}
