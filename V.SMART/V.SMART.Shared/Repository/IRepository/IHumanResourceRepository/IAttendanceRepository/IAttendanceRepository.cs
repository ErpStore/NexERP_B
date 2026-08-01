using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.Attendance;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AttendanceeVM;

namespace V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IAttendanceRepository
{
    public interface IAttendanceRepository : IRepository<Attendance>
    {
        Task<string> GetLastIdAsync();
        Task<List<AttendanceVM>> GetAttendanceDetailsAsync(int attendanceId);
        Task<List<AttendanceVM>> GetAttendanceByMonthYearAsync(int monthId,int year,string reportingType);
        IQueryable<Attendance> Query();
    }
}
