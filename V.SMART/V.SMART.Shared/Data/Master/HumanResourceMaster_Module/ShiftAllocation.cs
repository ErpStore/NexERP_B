using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class ShiftAllocation
    {
        [Key]
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

        public TimeSpan? TotalWorkingHours { get; set; }
        public bool OverTimeAllowed { get; set; } = false;

        [Range(0, int.MaxValue, ErrorMessage = "Late Mark Allowed must be a non-negative number.")]
        public int? LateMarkAllowed { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Early Leave Allowed must be a non-negative number.")]
        public int? EarlyLeaveAllowed { get; set; } = 0;

        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ModifiedBy { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime? ModifiedDate { get; set; }
    }

}
