using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.InventoryViewModel.StockAddVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IStockAddIss_Position
{
    public interface IStockAddIssPosition
    {
        //getAllStores
        Task<List<StockAddVM>> GetStockDynamicAsync(int? storeId, int? categoryCode, int? itemId);


        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<int> GetCorrespondenceAttachmentsCountAsync(int refId, string refType);



        Task<IEnumerable<Store>> GetAllActiveStoresAsync();

        Task<List<Category>> GetAllActiveCategoriesAsync();

        Task<List<Category>> GetAllActiveCategoriesAsync(int? storeId);

        Task<List<ItemVM>> GetStockItemsAsync(int? storeid, int? categorycode);
        Task<List<StockPositionSummaryVM>> GetStockSummaryAsync(int storeId, int categoryCode, int itemId, bool IsDetailsView);

        #region For Stock Position(Internal and external)
        Task<List<StockAddVM>> GetStockDynamicAsync(int? categoryCode, int? itemId);

        Task<List<ItemVM>> GetStockItemsAsync(int? categorycode);

        Task<(List<StockAddVM>, List<StockAddVM>)> GetStockAsync(List<int> itemIds,bool StockBaseRate);
        #endregion
    }
}
