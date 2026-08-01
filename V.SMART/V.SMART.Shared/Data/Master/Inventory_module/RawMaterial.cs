using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Inventory
{
    public class RawMaterial
    {
        [Key]
        public int RMId { get; set; }

        [Required(ErrorMessage = "Materials is required")]
        [StringLength(50)]
        public string RMName { get; set; }

        [Required(ErrorMessage = "Grade is required")]
        [StringLength(50, ErrorMessage = "Grade cannot exceed 50 characters")]
        public string Grade { get; set; }


        [StringLength(250, ErrorMessage = "Specification cannot exceed 250 characters")]
        public string? Specification { get; set; }

        [Required(ErrorMessage = "Density is required")]
        [Precision(18, 3)]
        [Range(0, 9999999.999, ErrorMessage = "Density must be a valid positive number")]
        public decimal Density { get; set; }


        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
