using V.SMART.Shared.Data.Master.Inventory_module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface IDefectInfoService
    {
        Task<List<DefectInfo>> GetAllActiveDefectsAsync();

        Task<String> UpsertDefect(DefectInfo DefectInfos);

        Task<bool> DeleteDefect(int DefectCode);
    }
}
