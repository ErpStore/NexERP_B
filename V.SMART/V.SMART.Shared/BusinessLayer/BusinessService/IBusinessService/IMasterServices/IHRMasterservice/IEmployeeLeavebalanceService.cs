using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice
{
    public interface IEmployeeLeavebalanceService
    {
        Task<(List<EmployeeLeaveBalanceVM> leaveBalanceVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters);
        Task<(bool CanDelete, string Message)> CanDeleteEmpLeaveBalAsync(int leaveBalId);
        Task<bool> DeleteEmpLeaveBalAsync(int leaveBalId);
        Task<EmployeeLeaveBalanceVM?> GetBalanceAsync(int employeeId, int leaveTypeId, int year);
        Task UpdateLeaveBalanceFromAttendanceAsync(int year);
    }
}
