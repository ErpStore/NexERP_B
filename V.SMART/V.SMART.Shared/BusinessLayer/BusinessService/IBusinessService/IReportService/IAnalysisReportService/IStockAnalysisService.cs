using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService
{
    public interface IStockAnalysisService
    {
        //getAllStores
        


        Task<ItemVM?> GetItemByItemIdAsync(int? itemId);

        Task<IEnumerable<Store>> GetAllActiveStoresAsync();

        Task<List<Category>> GetAllActiveCategoriesAsync();

        Task<List<Category>> GetAllActiveCategoriesAsync(int? storeId);

        Task<List<ItemVM>> GetStockItemsAsync(int? storeid, int? categorycode);

        Task<List<StockAnalysisVM>> GetStockAnalysisAsync(int StoreId, int? category, DateTime fromDate, DateTime toDate);
    }
}
