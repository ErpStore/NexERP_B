using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel
{
    public class ShiftAllocationVM
    {

        public int ShiftId { get; set; }

        [Required(ErrorMessage = "Shift Code is required.")]
        [StringLength(20, ErrorMessage = "Shift Code cannot be longer than 20 characters.")]
        public string ShiftCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shift Name is required.")]
        [StringLength(100, ErrorMessage = "Shift Name cannot be longer than 100 characters.")]
        public string ShiftName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shift Start Time is required.")]
        public TimeOnly? ShiftStartTime { get; set; }

        [Required(ErrorMessage = "Shift End Time is required.")]
        public TimeOnly? ShiftEndTime { get; set; }

        [Required(ErrorMessage = "Break Start Time is required.")]
        public TimeOnly? BreakStartTime { get; set; }

        [Required(ErrorMessage = "Break End Time is required.")]
        public TimeOnly? BreakEndTime { get; set; }

        public bool OverTimeAllowed { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Late Mark Allowed must be non-negative.")]
        public int? LateMarkAllowed { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Early Leave Allowed must be non-negative.")]
        public int? EarlyLeaveAllowed { get; set; } = 0;

        // =========================
        // AUDIT FIELDS
        // =========================
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

}
