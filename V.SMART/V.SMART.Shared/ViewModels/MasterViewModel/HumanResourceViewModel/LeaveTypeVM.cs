using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel
{
    public class LeaveTypeVM
    {
        public int LeaveTypeId { get; set; }

        [Required(ErrorMessage = "Leave Name is Required...")]
        [StringLength(200, ErrorMessage = "Leave Name cannot exceed 200 Characters...")]
        public string LeaveName { get; set; } = string.Empty;

        public string AttendanceCode { get; set; }

        [StringLength(250, ErrorMessage = "Leave Description cannot exceed 250 Characters...")]
        public string? LeaveDescription { get; set; }

        [Required(ErrorMessage = "Max Allowed Leave Per Year is Required...")]
        [Range(0, 365, ErrorMessage = "Max Allowed Leave must be between 0 and 365.")]
        public decimal MaxAllowedPerYear { get; set; }

        public bool PaidLeave { get; set; } = false;

        public bool DeductSalary { get; set; } = false;

        public bool IsCarryForward { get; set; } = false;

        public bool IsCashable { get; set; } = false;

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
