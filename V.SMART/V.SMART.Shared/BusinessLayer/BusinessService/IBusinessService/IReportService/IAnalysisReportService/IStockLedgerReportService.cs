using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ReportViewModel.AnalysisReportViewModel;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IReportService.IAnalysisReportService
{
    public interface IStockLedgerReportService
    {
        Task<List<StockLedgerReportVM>> GetStockLedgerList(int itemid,int storeid, DateTime fromDate, DateTime toDate);

        //Task<List<StockLedgerReportVM>> GetStockLedgerList(int itemid, int storeid);

        //Task<ItemVM?> GetItemByItemIdAsync(int? itemId);
        Task<List<ItemVM>> GetAllActiveItemsAsync();
        Task<IEnumerable<Store>> GetAllActiveStoresAsync();

        Task<List<ItemVM>> GetStockItemsAsync(int? storeid, DateTime fromDate, DateTime toDate);
    }
 }
