
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;

namespace V.SMART.Shared.Data.HumanResource.Attendance
{
    public class Attendance
    {
        [Key]
        public int Id { get; set; }

        public int Year { get; set; }

        public int StaffId { get; set; }
        [ForeignKey(nameof(StaffId))]
        public Staff Staff { get; set; }

        [Required]
        public int MonthId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? Ldetail { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? OtDetail { get; set; }

        public string? CreatedBy { get; set; }

        //public DateTime? CreatedDate { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? InTime { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? OutTime { get; set; }

        public string? BioMetricId { get; set; }
        public string? AttendanceSource { get; set; }
        public string? Reporting { get; set; }
        public class HolidayItem
        {
            public string HolidayName { get; set; }
            public bool IsChecked { get; set; }
        }

    }
}
