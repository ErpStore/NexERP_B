using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConSCNSubVM
    {
        public int SCNSubId { get; set; }

        [Required(ErrorMessage = "SCN reference is required.")]
        public int SCNId { get; set; }

        public int? RefGRNSubId { get; set; }
        public string? RefGRNNo { get; set; }
        public DateTime? RefGRNDate { get; set; }

        public int? RcSubId { get; set; }
        public string? RefRcNo { get; set; }
        public string? Process { get; set; }
        public DateTime? RefRcDate { get; set; }

        public int? PoSubId { get; set; }
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }

        [Required(ErrorMessage = "Serial number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }

        public ItemVM? SelectedItem { get; set; }

        [Required(ErrorMessage = "Item is required.")]
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }

        [Required(ErrorMessage = "Accepted quantity is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Accepted quantity cannot be negative.")]
        public decimal? AccQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Rejected quantity cannot be negative.")]
        public decimal? RejQty { get; set; }

        [StringLength(200, ErrorMessage = "Rejection reason cannot exceed 200 characters.")]
        public string? RejReason { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Rework quantity cannot be negative.")]
        public decimal? RewQty { get; set; }

        [StringLength(200, ErrorMessage = "Rework reason cannot exceed 200 characters.")]
        public string? RewReason { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance quantity cannot be negative.")]
        public decimal BalQty { get; set; }

        [Precision(18, 4)]
        public decimal? StockQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal? UnitPrice { get; set; }

        [StringLength(50, ErrorMessage = "Inspection number cannot exceed 50 characters.")]
        public string? InsNO { get; set; }

        [StringLength(50, ErrorMessage = "NCR number cannot exceed 50 characters.")]
        public string? NCRNo { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remark { get; set; }

        public int? RejectionId { get; set; }

        public string? RejectionCode { get; set; }

        public string? RejectionDescription { get; set; }

        public decimal? SCNBalQty { get; set; }

        // 🔹 UI Helpers
        public bool IsEditable { get; set; } = true;

        public decimal? QtyReceived { get; set; }
    }
}
