using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IHumanResourceService.IAttendanceService;
using V.SMART.Shared.Data.HumanResource.Attendance;
using V.SMART.Shared.Data.Master.Company_Module;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AttendanceeVM;

namespace V.SMART.Shared.BusinessLayer.BusinessService.HumanResourceService.AttendanceService
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICommonService _commonService;
        private readonly CurrentUserService _currentUserService;
        private readonly ILoggingService _logs;
        private readonly IMapper _mapper;

        public AttendanceService(
            IUnitOfWork unitOfWork,
            ICommonService commonService,
            CurrentUserService userService,
            ILoggingService logs,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _commonService = commonService;
            _currentUserService = userService;
            _logs = logs;
            _mapper = mapper;
        }

        // helper: build Ldetail (1..TotalDays). If value missing, default to "P"
        private string BuildLDetailFromRow(EmployeeAttendanceRow row, int totalDays)
        {
            var sb = new StringBuilder();
            for (int d = 1; d <= totalDays; d++)
            {
                if (row.DailyStatus != null && row.DailyStatus.TryGetValue(d, out var s) && !string.IsNullOrWhiteSpace(s))
                    sb.Append(s);
                else
                    sb.Append("P"); // default present if not provided (adjust as needed)
            }
            return sb.ToString();
        }

        // build InTime CSV
        private string BuildInTimeFromRow(EmployeeAttendanceRow row, int totalDays)
        {
            var list = new List<string>(totalDays);
            for (int d = 1; d <= totalDays; d++)
            {
                if (row.InTime != null && row.InTime.TryGetValue(d, out var t) && !string.IsNullOrWhiteSpace(t))
                    list.Add(t);
                else
                    list.Add(""); // keep empty for missing
            }
            return string.Join(",", list);
        }

        // build OutTime CSV
        private string BuildOutTimeFromRow(EmployeeAttendanceRow row, int totalDays)
        {
            var list = new List<string>(totalDays);
            for (int d = 1; d <= totalDays; d++)
            {
                if (row.OutTime != null && row.OutTime.TryGetValue(d, out var t) && !string.IsNullOrWhiteSpace(t))
                    list.Add(t);
                else
                    list.Add("");
            }
            return string.Join(",", list);
        }

        public async Task<List<AttendanceVM>> GetAttendanceDetailsByAttendanceIdAsync(int attendanceId)
        {
            return await _unitOfWork.Attendances
                .GetAttendanceDetailsAsync(attendanceId);
        }

        public async Task<List<AttendanceVM>> GetAttendanceByMonthYearAsync(
    int monthId,
    int year,string SelectedEmployeeType)
        {
            return await _unitOfWork.Attendances
                .GetAttendanceByMonthYearAsync(monthId, year, SelectedEmployeeType);
        }

        //Companydetails
        public async Task<Companydetails> GetCompanyDetailsAsync()
           => await _commonService.GetCompanyDetailsAsync();

        public async Task<(bool CanDelete, string Message)> CanDeleteIdAsync(int Id)
        {
            
            try
            {
                var attendance = await _unitOfWork.Attendances
                    .GetQueryable()
                    .FirstOrDefaultAsync(a => a.Id == Id);

                if (attendance == null)
                    return (false, "Attendance record not found.");

                // 🔴 Check if salary issued for same Staff + Month + Year
                var salaryExists = await _unitOfWork.Salaries
                    .GetQueryable()
                    .AnyAsync(s =>
                        s.StaffId == attendance.StaffId &&
                        s.SalMonth == attendance.MonthId &&
                        s.SalYear == attendance.Year);

                if (salaryExists)
                {
                    return (false, "Salary already issued for this staff and month. Attendance cannot be deleted.");
                }

                return (true, "Attendance can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Error in CanDeleteIdAsync for Id: {Id}");
                return (false, "Error checking delete eligibility.");
            }
        }

        public async Task<bool> DeleteAttendanceByIdAsync(int id)
        {
            await using var tx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var attendance = await _unitOfWork.Attendances
                    .GetQueryable()
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (attendance == null)
                    return false;

                // 🔴 Check Salary issued
                var salaryExists = await _unitOfWork.Salaries
                    .GetQueryable()
                    .AnyAsync(s =>
                        s.StaffId == attendance.StaffId &&
                        s.SalMonth == attendance.MonthId &&
                        s.SalYear == attendance.Year);

                if (salaryExists)
                    throw new Exception("Salary already issued. Cannot delete attendance.");

                await _unitOfWork.Attendances.DeleteAsync(attendance);
                await _unitOfWork.SaveAsync();
                await tx.CommitAsync();

                await _logs.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Attendance",
                    action: "Delete",
                    additionalInfo: $"Deleted Attendance Id: {attendance.Id}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Attendance: {id}");
                throw;
            }
        }

        public async Task<IEnumerable<AttendanceVM>> GetAllAttendanceAsync()
        {
            try
            {
                var list = await _unitOfWork.Attendances.GetQueryable()
                    .AsNoTracking()
                    .GroupBy(x => new { x.Year, x.MonthId })
                    .Select(g => g.First())
                    .ToListAsync();

                return _mapper.Map<IEnumerable<AttendanceVM>>(list);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "GetAllAttendanceAsync failed.");
                return Enumerable.Empty<AttendanceVM>();
            }
        }

        public async Task<AttendanceVM> GetAttendanceByIdAsync(int id)
        {
            return await _unitOfWork.Attendances
                .Query()
                .Where(a => a.Id == id)
                .Select(a => new AttendanceVM
                {
                    Id = a.Id,

                    StaffId = a.StaffId,
                    StaffName = a.Staff.StaffName,

                    Year = a.Year,
                    MonthId = a.MonthId,

                    TotalDays = DateTime.DaysInMonth(a.Year, a.MonthId),

                    Ldetail = a.Ldetail,
                    InTime = a.InTime,
                    OutTime = a.OutTime,
                    OtDetail = a.OtDetail,
                    Reporting = a.Reporting,
                    AttendanceSource = a.AttendanceSource,

                    CreatedBy = a.CreatedBy,
                    CreatedDate = a.CreatedDate,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedDate = a.ModifiedDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<EmployeeLeaveBalance>>
    GetLeaveHistoryByStaffIdAsync(int employeeId)
        {
            return await _unitOfWork.EmployeeLeaveBalances
                .GetQueryable()
                .Where(x => x.EmployeeID == employeeId)
                .ToListAsync();
        }


        public async Task<AttendanceVM> UpsertAttendanceAsync(AttendanceVM masterVM,List<AttendanceVM> detailRows)
        {
            if (masterVM == null)
                throw new ArgumentNullException(nameof(masterVM));

            detailRows ??= new List<AttendanceVM>();

            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();

            await using var tx = await _unitOfWork.BeginTransactionAsync();

            try
            {
                string CleanTime(string? value)
                {
                    if (string.IsNullOrWhiteSpace(value))
                        return "";

                    value = value.Trim();

                    if (TimeSpan.TryParseExact(value, "hh\\:mm", null, out _))
                        return value;

                    return "";
                }

                string BuildCsv(Dictionary<int, string> dict, int totalDays)
                {
                    var sb = new StringBuilder();

                    for (int d = 1; d <= totalDays; d++)
                    {
                        sb.Append(dict.ContainsKey(d) ? dict[d] : "");
                        sb.Append(',');
                    }

                    return sb.ToString();
                }

                var grouped = detailRows
                .GroupBy(x => x.StaffId)
                .ToList();
                foreach (var grp in grouped)
                {

                    int staffId = grp.Key;

                    var staff = await _unitOfWork.Staffs.GetQueryable()
                        .FirstOrDefaultAsync(s => s.StaffID == staffId);

                    if (staff == null)
                    {
                        throw new Exception($"Staff not found for StaffId: {staffId}");
                    }

                    var existing = await _unitOfWork.Attendances.GetQueryable()
                        .FirstOrDefaultAsync(a =>
                            a.Year == masterVM.Year &&
                            a.MonthId == masterVM.MonthId &&
                            a.StaffId == staffId);

                    int totalDays = masterVM.TotalDays > 0 ? masterVM.TotalDays : 31;

                    var temp = new EmployeeAttendanceRow
                    {
                        StaffId = staffId
                    };
                    //string staffRefId = grp.Key;

                    //var staff = await _unitOfWork.Staffs.GetQueryable()
                    //    .FirstOrDefaultAsync(s => s.StaffRefId == staffRefId);

                    //if (staff == null)
                    //{
                    //    // IMPORTANT: DON'T silently skip
                    //    throw new Exception($"Staff not found for RefId: {staffRefId}");
                    //}

                    //int staffId = staff.StaffID;   // ✅ FIXED

                    //var existing = await _unitOfWork.Attendances.GetQueryable()
                    //    .FirstOrDefaultAsync(a =>
                    //        a.Year == masterVM.Year &&
                    //        a.MonthId == masterVM.MonthId &&
                    //        a.StaffId == staffId);

                    //int totalDays = masterVM.TotalDays > 0 ? masterVM.TotalDays : 31;

                    //var temp = new EmployeeAttendanceRow
                    //{
                    //    StaffId = staffId,
                    //    StaffRefId = staffRefId
                    //};

                    for (int d = 1; d <= totalDays; d++)
                    {
                        temp.DailyStatus[d] = "";
                        temp.InTime[d] = "";
                        temp.OutTime[d] = "";
                        temp.OT[d] = 0m;
                    }

                    foreach (var row in grp)
                    {
                        int day = row.Day;
                        if (day < 1 || day > totalDays) continue;

                        temp.DailyStatus[day] = row.Ldetail ?? "";
                        temp.InTime[day] = CleanTime(row.InTime);
                        temp.OutTime[day] = CleanTime(row.OutTime);
                        temp.OT[day] = decimal.TryParse(row.OtDetail, out var ot) ? ot : 0m;
                    }

                    string ldetailCsv = BuildCsv(temp.DailyStatus, totalDays);
                    string otCsv = BuildCsv(temp.OT.ToDictionary(x => x.Key, x => x.Value.ToString()), totalDays);
                    string inCsv = BuildCsv(temp.InTime, totalDays);
                    string outCsv = BuildCsv(temp.OutTime, totalDays);

                    if (existing == null)
                    {
                        await _unitOfWork.Attendances.CreateAsync(new Attendance
                        {
                            Year = masterVM.Year,
                            MonthId = masterVM.MonthId,
                            StaffId = staffId,
                            Ldetail = ldetailCsv,
                            OtDetail = otCsv,
                            InTime = inCsv,
                            OutTime = outCsv,
                            CreatedBy = currentUser,
                            CreatedDate = now,
                            BioMetricId = masterVM.BioMetricId,
                            Reporting = masterVM.Reporting,
                            AttendanceSource = masterVM.AttendanceSource
                        });
                    }
                    else
                    {
                        existing.Ldetail = ldetailCsv;
                        existing.OtDetail = otCsv;
                        existing.InTime = inCsv;
                        existing.OutTime = outCsv;
                        existing.ModifiedBy = currentUser;
                        existing.ModifiedDate = now;
                        existing.BioMetricId = masterVM.BioMetricId;
                        existing.Reporting = masterVM.Reporting;
                        existing.AttendanceSource = masterVM.AttendanceSource;

                        await _unitOfWork.Attendances.UpdateAsync(existing);
                    }
                }

                // ✅ ONLY ONE SAVE
                await _unitOfWork.SaveAsync();

                await tx.CommitAsync();

                return masterVM;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                await _logs.LogDeveloperError(ex, "UpsertAttendance failed.");
                throw;
            }
        }

        private async Task LogChangesAsync(StringBuilder changes, string action)
        {
            if (changes.Length == 0) return;

            await _logs.LogUserAction(
                UserName: await _currentUserService.GetUsernameAsync(),
                Machine: _currentUserService.MachineName,
                IP_Address: _currentUserService.IpAddress,
                screen: "Attendance",
                action: action,
                additionalInfo: changes.ToString()
            );
        }

        #region Helpers
        private string GetPropertyChanges(Attendance entity, AttendanceVM vm)
        {
            if (entity == null || vm == null) return string.Empty;

            var sb = new StringBuilder();
            if (entity.Year != vm.Year) sb.AppendLine($"Year: {entity.Year} => {vm.Year}");
            if (entity.MonthId != vm.MonthId) sb.AppendLine($"MonthId: {entity.MonthId} => {vm.MonthId}");
            if (entity.StaffId != vm.StaffId) sb.AppendLine($"StaffId: {entity.StaffId} => {vm.StaffId}");
            if (!string.Equals(entity.Reporting, vm.Reporting, StringComparison.OrdinalIgnoreCase))
                sb.AppendLine($"Reporting: {entity.Reporting} => {vm.Reporting}");

            return sb.ToString();
        }
        #endregion
    }
}
