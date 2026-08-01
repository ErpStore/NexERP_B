using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class LeaveType
    {
        [Key]
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Leave Name is Required...")]
        [StringLength(200, ErrorMessage = "Leave Name cannot exceed 200 Characters...")]
        public string LeaveName { get; set; }
        public string AttendanceCode { get; set; }

        [StringLength(250, ErrorMessage = "Leave Description cannot exceed 250 Characters...")]
        public string? LeaveDescription { get; set; }

        [Required(ErrorMessage = "MaxAllowed Leave Per Year is Required...")]
        [Precision(18, 2)]
        public decimal MaxAllowedPerYear { get; set; }

        public bool PaidLeave { get; set; }
        public bool DeductSalary { get; set; }

        public bool IsCarryForward { get; set; }

        public bool IsCashable { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; } = null;
    }
}
