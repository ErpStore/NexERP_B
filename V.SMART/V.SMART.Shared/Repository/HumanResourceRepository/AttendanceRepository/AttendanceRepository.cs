using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data;
using V.SMART.Shared.Data.HumanResource.Attendance;
using V.SMART.Shared.Repository.IRepository.IHumanResourceRepository.IAttendanceRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AttendanceeVM;

namespace V.SMART.Shared.Repository.HumanResourceRepository.AttendanceRepository
{
    public class AttendanceRepository : Repository<Attendance>,IAttendanceRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly CurrentUserService _currentUserService;

        // Updated constructor to include CurrentUserService
        public AttendanceRepository(ApplicationDbContext db, ILoggingService logs, CurrentUserService currentUserService)
            : base(db, logs)
        {
            _db = db;
            _currentUserService = currentUserService;
        }

        // Return IQueryable for querying Attendances
        public IQueryable<Attendance> GetQueryable()
        {
            return _db.Attendance.AsQueryable();
        }

        public IQueryable<Attendance> Query()
        {
            return _db.Attendance
                .Include(a => a.Staff);
        }

        // Get the last Attendance ID
        public async Task<string> GetLastIdAsync()
        {
            var lastId = await _db.Attendance
                .OrderByDescending(x => x.Id)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            return (lastId == 0 ? 1 : lastId + 1).ToString();
        }


        public async Task<List<AttendanceVM>> GetAttendanceDetailsAsync(int attendanceId)
        {
            var selected = await _db.Attendance
                .FirstOrDefaultAsync(x => x.Id == attendanceId);

            if (selected == null)
                return new List<AttendanceVM>();

            return await (
                from a in _db.Attendance
                join s in _db.Staff on a.StaffId equals s.StaffID
                where a.MonthId == selected.MonthId
                   && a.Year == selected.Year
                select new AttendanceVM
                {
                    Id = a.Id,
                    StaffId = a.StaffId,
                    StaffName = s.StaffName,
                    WorkingHrs = s.WorkingHrs,

                    Year = a.Year,
                    MonthId = a.MonthId,

                    Ldetail = a.Ldetail,
                    InTime = a.InTime,
                    OutTime = a.OutTime,
                    OtDetail = a.OtDetail
                })
                .ToListAsync();
        }

      
        public async Task<List<AttendanceVM>> GetAttendanceByMonthYearAsync(int monthId,int year,string reportingType)
        {
            var result = await (
                from s in _db.Staff

                where s.Reporting != null
                      && s.Reporting == reportingType

                join a in _db.Attendance
                    .Where(x => x.MonthId == monthId
                             && x.Year == year)
                on s.StaffID equals a.StaffId
                into attendanceGroup

                from a in attendanceGroup.DefaultIfEmpty()

                orderby s.StaffName

                select new AttendanceVM
                {
                    Id = a != null ? a.Id : 0,

                    StaffId = s.StaffID,
                    StaffName = s.StaffName,
                    WorkingHrs = s.WorkingHrs,

                    MonthId = monthId,
                    Year = year,

                    Ldetail = a != null ? a.Ldetail : null,
                    InTime = a != null ? a.InTime : null,
                    OutTime = a != null ? a.OutTime : null,
                    OtDetail = a != null ? a.OtDetail : null
                })
                .ToListAsync();

            return result;
        }
    }
}
