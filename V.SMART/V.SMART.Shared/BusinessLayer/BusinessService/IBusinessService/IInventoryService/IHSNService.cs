using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService
{
    public interface IHSNService
    {
        Task<(List<HSNMaster> hSNs, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters);

        Task<bool> DeleteHSNAsync(int slNo);

    }
}
