using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Inventory
{
    public class Factor
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Factor name is required.")]
        [MaxLength(250, ErrorMessage = "Factor name cannot exceed 250 characters.")]
        public string Factors { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
    }

}
