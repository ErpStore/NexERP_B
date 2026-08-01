using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Inventory
{
    public class Process
    {
        [Key]
        public int ProcessId { get; set; }

        [Required(ErrorMessage = "Process Name is required.")]
        [StringLength(250, ErrorMessage = "Process Name cannot exceed 250 characters.")]
        public string ProcessName { get; set; }

        public string? ProcessType { get; set; }
        public string? ProcessFormula { get; set; }

        public string? Standard { get; set; }

        [Required(ErrorMessage = "Hourly Rate is required.")]
        [Range(0, float.MaxValue, ErrorMessage = "Hourly Rate must be a positive number.")]
        public float HrlyRate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
    }
}
