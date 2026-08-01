using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice
{
    public interface ILeaveApplicationService
    {
        Task<LeaveApplication> GetLeaveApplicationWithDetailsAsync(int leaveAppID);
        Task<(IEnumerable<Staff> Staffs, IEnumerable<LeaveType> LeaveTypes, IEnumerable<EmployeeLeaveBalance> EmployeeLeaveBalances, string LeaveApplicationNo)> GetFormDataAsync();
        Task<string> CreateOrUpdateLeaveApplicationAsync(LeaveApplication leaveApplication);
        void RecalculateTotalDays(LeaveApplication leaveApplication);
        Task<List<EmployeeLeaveBalance>> GetLeaveHistoryByStaffIdAsync(int employeeId);
    }
}
