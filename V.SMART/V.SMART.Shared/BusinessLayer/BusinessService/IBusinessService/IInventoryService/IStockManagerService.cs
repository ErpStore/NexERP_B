using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService
{
    public interface IStockManagerService
    {
        Task AddOrUpdateStockAsync(int itemId, int storeId, decimal qty, decimal unitPrice, string? batchNo, int screenCode, int subItemRefId,
                            string refNo, DateTime refDate, string? remarks, int? RcSubId = null, bool allowMultipleAdd = false);

        Task IssueOrUpdateStockAsync(int itemId, int storeId, decimal issueQty, decimal unitPrice, string? batchNo, int screenCode,
                                            int? subItemRefId, string? refNo, DateTime? refDate, int? rcSubId = null, bool allowMultipleIssue = false);
        Task DeleteStockAddAsync(int addId);

        Task DeleteStockIssueAsync(int issueId);

        Task<(decimal AddQty, decimal BalQty)> GetQtyBalQtyByStockAddAsync(int screenCode, int storeId, int itemId, int subItemRefId);

        Task<Dictionary<int, decimal>> GetStockForItemsAsync(IEnumerable<int> itemIds, int? storeId);

        Task<decimal> GetStockForItemAsync(int itemId, int? storeId);

        Task<Dictionary<int, decimal>> GetStockRateForItemsAsync(IEnumerable<int> itemIds, int? storeId);

        Task<decimal> GetStockForItemRcSubIdAsync(int itemId, int? storeId, int? rcSubId);


    }
}
