using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class EmployeeLeaveBalance
    {
        [Key]
        public int LeaveBalanceID { get; set; }

        [Required(ErrorMessage = "EmployeeID is Required...")]
        public int? EmployeeID { get; set; }

        [ForeignKey(nameof(EmployeeID))]
        public Staff? Staff { get; set; }

        [Required(ErrorMessage = "Leave Type Name is Required...")]
        public int? LeaveTypeId { get; set; }

        [ForeignKey(nameof(LeaveTypeId))]
        public LeaveType? LeaveType { get; set; }

        [Required(ErrorMessage = "Leave allocation Year is required")]
        public int Year { get; set; }

        [Precision(18, 2)]
        [Required(ErrorMessage = "Total Allocated Leaves balance is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Total Allocated Leave must be non-negative.")]
        public decimal TotalAllocatedLeave { get; set; }

        [Precision(18, 2)]
        [Range(0, double.MaxValue, ErrorMessage = "Leaves Taken must be non-negative.")]
        public decimal LeavesTaken { get; set; }

        [Precision(18, 2)]
        public decimal Balance { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; } = null;

        /// <summary>
        /// Recalculate Balance (always keep consistent)
        /// </summary>
        public void RecalculateBalance()
        {
            if (LeavesTaken > TotalAllocatedLeave)
            {
                throw new ValidationException("Leaves Taken cannot be greater than Total Allocated Leave.");
            }

            Balance = TotalAllocatedLeave - LeavesTaken;
        }
    }

}