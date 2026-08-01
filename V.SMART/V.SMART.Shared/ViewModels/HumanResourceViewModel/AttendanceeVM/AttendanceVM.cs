using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.HumanResourceViewModel.AttendanceeVM
{
    public class AttendanceVM
    {
        // ===============================
        // MASTER FIELDS
        // ===============================
        public int Id { get; set; }
        
        public int Year { get; set; }
        [Required(ErrorMessage ="MonthName is Required...")]
        [Range(1,12,ErrorMessage ="Please Select a MonthName")]
        public int MonthId { get; set; }
        public string MonthName
        {
            get
            {
                return MonthId > 0
                    ? CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(MonthId)
                    : "N/A";
            }
        }

        public int StaffId { get; set; }
        public string StaffRefId { get; set; }

        // 🔥 FIXED StaffName (computed)
        private string? _staffName;
        public string? StaffName
        {
            get
            {
                // Priority 1: value set from service
                if (!string.IsNullOrWhiteSpace(_staffName))
                    return _staffName;

                // Priority 2: value from built employee row
                if (EmployeeList != null && EmployeeList.Any())
                    return EmployeeList.First().Name;

                return "N/A";
            }
            set => _staffName = value;
        }


        public int Day { get; set; }
        public string? Ldetail { get; set; }     
        public string? OtDetail { get; set; }    

        //[Required(ErrorMessage ="InTime is Required...")]
        [Column(TypeName = "nvarchar(max)")]
        public string? InTime { get; set; }      

        //[Required(ErrorMessage ="OutTime is Required...")]
        [Column(TypeName = "nvarchar(max)")]
        public string? OutTime { get; set; }     
        public decimal OT { get; set; }

        public bool AttendanceCancel { get; set; } = false;

        //22/12/25
        public DateTime? InTimeDT
        {
            get => ParseTime(InTime);
            set => InTime = value?.ToString("HH:mm");
        }

        public DateTime? OutTimeDT
        {
            get => ParseTime(OutTime);
            set => OutTime = value?.ToString("HH:mm");
        }


        // 🔥 ADD THIS METHOD (THIS FIXES YOUR ERROR)
        private static DateTime? ParseTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time))
                return null;

            if (DateTime.TryParseExact(
                    time,
                    "HH:mm",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var result))
            {
                return DateTime.Today.Add(result.TimeOfDay);
            }

            return null;
        }
        //22/12/25

       
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string? BioMetricId { get; set; }
        public string? AttendanceSource { get; set; }
        public string? Reporting { get; set; }
        public string WorkingHrs { get; set; }

        public int TotalDays { get; set; } = 31;
        //public int TotalDays { get; set; }
        public List<EmployeeAttendanceRow> EmployeeList { get; set; } = new();

        public List<HolidayItem> HolidayList { get; set; } = new();

        public List<WeekDayGroup> CalendarHolidayList { get; set; } = new();

        public void BuildEmployeeFromCommaData()
        {
            var row = new EmployeeAttendanceRow
            {
                StaffId = StaffId,
                Name = StaffName ?? string.Empty,
                Reporting = Reporting ?? "N/A",
                AttendanceSource = AttendanceSource,

            };

            var statusArr = (Ldetail ?? "").Split(',');
            var inArr = (InTime ?? "").Split(',');
            var outArr = (OutTime ?? "").Split(',');
            var otArr = (OtDetail ?? "").Split(',');

            for (int d = 1; d <= TotalDays; d++)
            {
                row.DailyStatus[d] = statusArr.Length >= d ? statusArr[d - 1] : "";
                row.InTime[d] = inArr.Length >= d ? inArr[d - 1] : "";
                row.OutTime[d] = outArr.Length >= d ? outArr[d - 1] : "";

                // Convert string to decimal? safely
                row.OT[d] = (otArr.Length >= d && decimal.TryParse(otArr[d - 1], out var otValue))
                            ? otValue
                            : 0;
            }

            EmployeeList = new List<EmployeeAttendanceRow> { row };
        }

        public void BuildCommaDataFromEmployee()
        {
            if (!EmployeeList.Any())
                return;

            var emp = EmployeeList.First();

            Ldetail = string.Join(",", Enumerable.Range(1, TotalDays)
                .Select(d => emp.DailyStatus.ContainsKey(d) ? emp.DailyStatus[d] : ""));

            InTime = string.Join(",", Enumerable.Range(1, TotalDays)
                .Select(d => emp.InTime.ContainsKey(d) ? emp.InTime[d] : ""));

            OutTime = string.Join(",", Enumerable.Range(1, TotalDays)
                .Select(d => emp.OutTime.ContainsKey(d) ? emp.OutTime[d] : ""));

            OtDetail = string.Join(",", Enumerable.Range(1, TotalDays)
                .Select(d => emp.OT.ContainsKey(d) ? (emp.OT[d].ToString() ?? "0") : "0"));

        }

        //====================================
        //VALIDATION FOR DAILY TIMES
        //====================================
        public bool ValidateAttendance(out string message)
        {
            message = "";

            if (!EmployeeList.Any())
                return true;

            var emp = EmployeeList.First();

            for (int d = 1; d <= TotalDays; d++)
            {
                if (!emp.InTime.ContainsKey(d) || string.IsNullOrWhiteSpace(emp.InTime[d]))
                {
                    message = $"InTime missing for Day {d}";
                    return false;
                }

                if (!emp.OutTime.ContainsKey(d) || string.IsNullOrWhiteSpace(emp.OutTime[d]))
                {
                    message = $"OutTime missing for Day {d}";
                    return false;
                }
            }

            return true;
        }

        public class HolidayItem
        {
            public string HolidayName { get; set; } = string.Empty;
            public bool IsChecked { get; set; }
        }
    }


    public class EmployeeAttendanceRow
    {
        public int StaffId { get; set; }
        public string StaffRefId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Reporting { get; set; } = string.Empty;
        public string Status { get; set; }
        public bool OtCount { get; set; }
        public string AttendanceSource { get; set; } = string.Empty;
        public Dictionary<int, string> DailyStatus { get; set; } = new();
        public string WorkingHrs { get; set; }   // ✅ STORE IT HERE
        public Dictionary<int, string> InTime { get; set; } = new();
        public Dictionary<int, string> OutTime { get; set; } = new();
        public Dictionary<int, decimal> OT { get; set; } = new();
    }

    public class WeekDayGroup
    {
        public DayOfWeek WeekDay { get; set; }
        public List<DayCheck> Days { get; set; } = new();
    }

    public class DayCheck
    {
        //public int Day { get; set; }
        //public bool IsChecked { get; set; }

        public int Day { get; set; }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    OnChanged?.Invoke(Day, value);
                }
            }
        }

        public Action<int, bool>? OnChanged { get; set; }

        public class BiometricRow
        {
            public int SlNo { get; set; }
            public int EmployeeId { get; set; }
            public string BiometricId { get; set; } = string.Empty;
            public DateTime PunchDate { get; set; }
            public string PunchTimeIn { get; set; } = string.Empty;
            public string PunchTimeOut { get; set; } = string.Empty;
            public Dictionary<int, string> DayStatus { get; set; } = new();
        }

        public class MonthItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}
