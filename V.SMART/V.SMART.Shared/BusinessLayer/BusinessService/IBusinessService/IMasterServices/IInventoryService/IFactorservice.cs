using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService
{
    public interface IFactorservice
    {
        Task<(List<Factor> factors, int TotalCount)>
           SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
           Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteFactorsAsync(int factorId);
        Task<bool> DeleteFactorAsync(int factorId);

    }
}
