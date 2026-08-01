using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice
{
    public interface IShiftAllocationService
    {

        Task<(List<ShiftAllocationVM> shiftAllocationVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteShiftAllocationAsync(int shiftId);
        Task<bool> DeleteShiftAllocationByShiftIdAsync(int shiftId);


    }
}
