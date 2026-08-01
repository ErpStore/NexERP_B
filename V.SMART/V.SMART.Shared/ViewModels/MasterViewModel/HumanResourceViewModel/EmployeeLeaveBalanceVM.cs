using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel
{
    public class EmployeeLeaveBalanceVM
    {
        public int LeaveBalanceID { get; set; }

        [Required(ErrorMessage = "Employee is required.")]
        public int? EmployeeID { get; set; }

        public string? EmployeeName { get; set; }

        [Required(ErrorMessage = "Leave Type is required.")]
        public int? LeaveTypeId { get; set; }

        public string? LeaveTypeName { get; set; }

        [Required(ErrorMessage = "Leave Allocation Year is required.")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Total Allocated Leave is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Total Allocated Leave must be non-negative.")]
        public decimal TotalAllocatedLeave { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Leaves Taken must be non-negative.")]
        public decimal LeavesTaken { get; set; }
        public decimal Balance => TotalAllocatedLeave - LeavesTaken;


        // Audit Fields
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }
}
