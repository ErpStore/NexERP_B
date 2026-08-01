using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel
{
    public class RawMaterialVM
    {
        public int RMId { get; set; }

        [Required(ErrorMessage = "Material Name is required")]
        [StringLength(50, ErrorMessage = "Material Name cannot exceed 50 characters")]
        public string? RMName { get; set; }

        [Required(ErrorMessage = "Grade is required")]
        [StringLength(50, ErrorMessage = "Grade cannot exceed 50 characters")]
        public string? Grade { get; set; }

        [StringLength(250, ErrorMessage = "Specification cannot exceed 250 characters")]
        public string? Specification { get; set; }

        [Required(ErrorMessage = "Density is required")]
        [Range(0.001, 9999999.999, ErrorMessage = "Density must be a valid positive number")]
        public decimal? Density { get; set; }

        // UI Purpose (Audit Display)
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Extra UI helper
        public bool IsEditMode => RMId > 0;
    }
}
