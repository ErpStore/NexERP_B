using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService
{
    public interface IStoreMappingService
    {
        Task<IEnumerable<Store>> GetAllStoreIssueAsync();
        Task<IEnumerable<Store>> GetAllActiveStoresAsync();
        Task<List<StoreMap>> GetAllScreenStoreMapAsync();
        Task SaveScreenStoreMapsAsync(IEnumerable<StoreMap> storeMaps);
    }
}
