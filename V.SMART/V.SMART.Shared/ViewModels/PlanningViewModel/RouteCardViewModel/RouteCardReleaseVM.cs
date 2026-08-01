using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel
{
    public class RouteCardReleaseVM
    {
        public int RouteCardReleaseId { get; set; }

        public bool IsCancel { get; set; } = false;
        public bool ShortClose { get; set; } = false;

        // -------------------- Release Details --------------------

        [Required(ErrorMessage = "Release No is required.")]
        [StringLength(20, ErrorMessage = "Release No cannot exceed 20 characters.")]
        public string RcReleaseNo { get; set; } = string.Empty;

        [StringLength(10, ErrorMessage = "Suffix cannot exceed 10 characters.")]
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Release Date is required.")]
        public DateTime RcReleaseDate { get; set; } = DateTime.Now;
        public bool RcReleaseTally { get; set; } = false;

        // -------------------- Customer --------------------

        [Required(ErrorMessage = "Customer is required.")]
        public int? CustId { get; set; }

        [StringLength(100, ErrorMessage = "Customer Name cannot exceed 100 characters.")]
        public string? CustomerName { get; set; }

        public CustomerVM? SelectedCustomer { get; set; }

        // -------------------- PO Details --------------------

        [Required(ErrorMessage = "PO is required.")]
        public int? PoId { get; set; }

        [StringLength(30, ErrorMessage = "PO Number cannot exceed 30 characters.")]
        public string? PoNumber { get; set; }

        public int? PoSubId { get; set; }

        // -------------------- Assembly --------------------

        [Required(ErrorMessage = "Assembly Item is required.")]
        public int? AssemblyItemId { get; set; }

        [StringLength(100, ErrorMessage = "Assembly Item Name cannot exceed 100 characters.")]
        public string? AssemblyItemCode { get; set; }

        // -------------------- Quantity --------------------

        [Range(0.01, double.MaxValue, ErrorMessage = "PO Quantity must be greater than 0.")]
        public decimal PoQty { get; set; }

        [Required(ErrorMessage = "Release Quantity is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Release Quantity must be greater than 0.")]
        public decimal ReleaseQty { get; set; }



        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }


        // -------------------- Audit --------------------

        [StringLength(50, ErrorMessage = "Released By cannot exceed 50 characters.")]
        public string? ReleasedBy { get; set; }

        public DateTime ReleasedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // -------------------- Child List --------------------
        [Required(ErrorMessage = "At least one component is required.")]
        [MinLength(1, ErrorMessage = "At least one component must be added.")]
        public List<RouteCardReleaseSubVM> RouteCardReleaseSubs { get; set; } = new();
    }
}
