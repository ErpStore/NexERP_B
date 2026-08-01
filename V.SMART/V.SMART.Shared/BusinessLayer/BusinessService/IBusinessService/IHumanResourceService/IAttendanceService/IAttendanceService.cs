using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AttendanceeVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IAttendanceService
{
    public interface IAttendanceService
    {
        Task<(bool CanDelete, string Message)> CanDeleteIdAsync(int id);
        Task<bool> DeleteAttendanceByIdAsync(int id);
        Task<IEnumerable<AttendanceVM>> GetAllAttendanceAsync();
        Task<AttendanceVM> UpsertAttendanceAsync(AttendanceVM attendanceVM, List<AttendanceVM> detailRows);
        Task<AttendanceVM> GetAttendanceByIdAsync(int attendanceId);
        Task<List<AttendanceVM>> GetAttendanceDetailsByAttendanceIdAsync(int attendanceId);
        Task<Companydetails> GetCompanyDetailsAsync();
        Task<List<AttendanceVM>> GetAttendanceByMonthYearAsync(int monthId,int year,string reportingType);
    }
}
