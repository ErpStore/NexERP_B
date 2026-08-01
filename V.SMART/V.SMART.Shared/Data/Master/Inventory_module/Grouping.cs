using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Inventory
{
    public class Grouping
    {
        [Key]
        public int ID { get; set; }

        [Required(ErrorMessage = "FRC Number is required.")]
        [MaxLength(100)]
        public string FRCNo { get; set; }

        [MaxLength(50)]
        public string? Prefix { get; set; }

        [Range(1, 365, ErrorMessage = "Lead Time must be between 1 and 365 days.")]
        public int LeadTime { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Navigation Property (VERY IMPORTANT)
        public ICollection<GroupingSub> GroupingSubs { get; set; } = new List<GroupingSub>();
    }
}
