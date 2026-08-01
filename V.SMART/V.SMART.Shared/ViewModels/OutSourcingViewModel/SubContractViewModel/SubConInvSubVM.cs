using V.SMART.Shared.Utility_Constants;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConInvSubVM : ICalculationDocumentSubItem
    {
        public int InvSubId { get; set; }
        public int InvId { get; set; }

        // ===== References =====
        public int? RefSCNSubId { get; set; }
        public string? RefSCNNo { get; set; }
        public DateTime? RefSCNDate { get; set; }

        public int? CostId { get; set; }

        public string? ProjectNo { get; set; }

        // ===== Serial =====
        [Required(ErrorMessage = "Serial number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }

        // ===== Item =====
        [Required(ErrorMessage = "Item-Code / Part Name is required.")]
        public int? ItemId { get; set; }

        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        [StringLength(20)]
        public string? HsnCode { get; set; }

        [StringLength(20)]
        public string? MeasureUnit { get; set; }
        public string? CategoryName { get; set; }
        public string? Specification { get; set; }

        public List<string> DebititNos { get; set; }
        public List<DateTime> DebititDates { get; set; }

        // ===== Quantities =====
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal? Qty { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? BalQty { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? RejectQty { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? ReworkQty { get; set; }

        public decimal DbBalQty { get; set; }

        // ===== Pricing =====
        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Unit price must be greater than zero.")]
        public decimal? UnitPrice { get; set; }

        // ===== Discount =====
        [Range(0, 100)]
        public decimal? LineDiscountPercent { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? LineDiscountAmount { get; set; }

        // ===== GST =====
        [Range(0, 100)]
        public decimal? LineCGSTRate { get; set; }

        [Range(0, 100)]
        public decimal? LineSGSTRate { get; set; }

        [Range(0, 100)]
        public decimal? LineIGSTRate { get; set; }

        // ===== Remarks =====
        [StringLength(300)]
        public string? Remarks { get; set; }

        public bool ItemCancel { get; set; } = false;
        public string? ItemCancelReason { get; set; }


        [StringLength(200)]
        public string? CancelItemReason { get; set; }

        public bool IsEditable { get; set; } = false;

        // =========================================================
        // ================= Computed Properties ==================
        // =========================================================

        public decimal LineGross =>
            (Qty ?? 0m) * (UnitPrice ?? 0m);

        public decimal LineBasicAmount =>
            LineGross - (LineDiscountAmount ?? 0m);

        public decimal LineCGSTAmount =>
            LineBasicAmount * (LineCGSTRate ?? 0m) / 100m;

        public decimal LineSGSTAmount =>
            LineBasicAmount * (LineSGSTRate ?? 0m) / 100m;

        public decimal LineIGSTAmount =>
            LineBasicAmount * (LineIGSTRate ?? 0m) / 100m;

        // =========================================================
        // ========== ICalculationDocumentSubItem ==================
        // =========================================================

        decimal ICalculationDocumentSubItem.Qty => Qty ?? 0m;
        decimal ICalculationDocumentSubItem.UnitPrice => UnitPrice ?? 0m;

        decimal ICalculationDocumentSubItem.LineDiscountPercent =>
            LineDiscountPercent ?? 0m;

        decimal ICalculationDocumentSubItem.LineDiscountAmount =>
            LineDiscountAmount ?? 0m;

        decimal ICalculationDocumentSubItem.LineCGSTRate =>
            LineCGSTRate ?? 0m;

        decimal ICalculationDocumentSubItem.LineSGSTRate =>
            LineSGSTRate ?? 0m;

        decimal ICalculationDocumentSubItem.LineIGSTRate =>
            LineIGSTRate ?? 0m;

        decimal ICalculationDocumentSubItem.LineGross =>
            LineGross;

        decimal ICalculationDocumentSubItem.LineBasicAmount =>
            LineBasicAmount;

        decimal ICalculationDocumentSubItem.LineCGSTAmount =>
            LineCGSTAmount;

        decimal ICalculationDocumentSubItem.LineSGSTAmount =>
            LineSGSTAmount;

        decimal ICalculationDocumentSubItem.LineIGSTAmount =>
            LineIGSTAmount;
    }

}
