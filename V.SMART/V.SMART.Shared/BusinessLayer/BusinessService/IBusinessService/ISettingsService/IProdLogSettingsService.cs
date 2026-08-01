using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService
{
    public interface IProdLogSettingsService
    {
        Task<bool> IsDuplicateProdTypeNameAsync(string prodType, int? Id = null);
        Task<bool> DeleteProductionLogSettingsAsync(int id);
        Task<ProductionLogSetting?> GetProductionLogSettingsAsync(int id);
        Task<(List<ProductionLogSetting> prodLogSettings, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters);
        Task<ProductionLogSetting> CreateProdLogSettingsAsync(ProductionLogSetting productionLogSetting);
        Task<ProductionLogSetting> UpdateProdLogSettingsAsync(ProductionLogSetting productionLogSetting);
        Task<IEnumerable<Category>> GetAllCategoriesAsync(string? keyword = null);
    }
}
