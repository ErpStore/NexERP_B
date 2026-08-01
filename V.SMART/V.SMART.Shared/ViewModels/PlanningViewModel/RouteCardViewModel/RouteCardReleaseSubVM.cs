using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel
{
    public class RouteCardReleaseSubVM
    {
        public int RouteCardReleaseSubId { get; set; }

        public int RouteCardReleaseId { get; set; }

        // -------------------- Component --------------------

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Serial Number must be greater than 0.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Component Item is required.")]
        public int ComponentItemId { get; set; }

        [StringLength(50, ErrorMessage = "Component Code cannot exceed 50 characters.")]
        public string? ComponentItemCode { get; set; }

        [Required(ErrorMessage = "Component Name is required.")]
        [StringLength(150, ErrorMessage = "Component Name cannot exceed 150 characters.")]
        public string? ComponentItemName { get; set; }

        [StringLength(20, ErrorMessage = "UOM cannot exceed 20 characters.")]
        public string? ComponentItemUOM { get; set; }

        // -------------------- Quantity --------------------

        [Range(0, double.MaxValue, ErrorMessage = "Cumulative Utilized Qty cannot be negative.")]
        public decimal CumulativeUtilQty { get; set; }

        [Required(ErrorMessage = "Required Quantity is mandatory.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Required Quantity must be greater than 0.")]
        public decimal RequiredQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Issued Quantity cannot be negative.")]
        public decimal IssuedQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Remaining Quantity cannot be negative.")]
        public decimal RemainingQty { get; set; }

        public bool IsCompleted { get; set; }

        // Calculated Property
        public decimal BalanceToIssue => RequiredQty - IssuedQty;

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

    }
}
