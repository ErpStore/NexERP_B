using V.SMART.Shared.Utility_Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM
{
    public class LabInvSubVM : ICalculationDocumentSubItem
    {
        public int LabInvSubId { get; set; }
        public int LabInvId { get; set; }

        [Required(ErrorMessage = "Serial number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }

        [StringLength(50, ErrorMessage = "RFQ cannot exceed 50 characters.")]
        public string? RFQ { get; set; }

        public int? RefDcSubId { get; set; }
        public string? RefDcNo { get; set; }
        public DateTime? RefDcDate { get; set; }
        public int? LineNo { get; set; }
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }

        [Required(ErrorMessage = "Item-Code/PartName is Required")]
        public int? ItemId { get; set; }

        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        [StringLength(20, ErrorMessage = "HSN Code cannot exceed 20 characters.")]
        public string? SACCode { get; set; }

        [StringLength(20, ErrorMessage = "Unit of Measure cannot exceed 20 characters.")]
        public string? MeasureUnit { get; set; }
        public decimal? LabourCost { get; set; }

        public int? CostId { get; set; }

        public string? ProjectNo { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal? Qty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance quantity cannot be negative.")]
        public decimal? BalQty { get; set; }

        [Precision(18, 3)]
        public decimal? CrBalQty { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Unit price must be non-negative.")]
        public decimal? UnitPrice { get; set; }

        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100.")]
        public decimal? LineDiscountPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be non-negative.")]
        public decimal? LineDiscountAmount { get; set; }

        [Range(0, 100, ErrorMessage = "CGST rate must be between 0 and 100.")]
        public decimal? LineCGSTRate { get; set; }

        [Range(0, 100, ErrorMessage = "SGST rate must be between 0 and 100.")]
        public decimal? LineSGSTRate { get; set; }

        [Range(0, 100, ErrorMessage = "IGST rate must be between 0 and 100.")]
        public decimal? LineIGSTRate { get; set; }

        [StringLength(300, ErrorMessage = "Remarks cannot exceed 300 characters.")]
        public string? Remarks { get; set; }
        public bool ItemCancel { get; set; } = false;
        public string? CancelItemReason { get; set; }

        public bool IsEditable { get; set; } = false;

        public List<string> CreditNos { get; set; }
        public List<DateTime> CreditDates { get; set; }

        // ===== Computed properties =====
        public decimal LineGross => (Qty ?? 0m) * (UnitPrice ?? 0m);
        public decimal LineBasicAmount => LineGross - (LineDiscountAmount ?? 0m);
        public decimal LineCGSTAmount => LineBasicAmount * (LineCGSTRate ?? 0m) / 100m;
        public decimal LineSGSTAmount => LineBasicAmount * (LineSGSTRate ?? 0m) / 100m;
        public decimal LineIGSTAmount => LineBasicAmount * (LineIGSTRate ?? 0m) / 100m;

        // ===== ICalculationDocumentSubItem implementation =====
        decimal ICalculationDocumentSubItem.Qty => Qty ?? 0m;
        decimal ICalculationDocumentSubItem.UnitPrice => UnitPrice ?? 0m;
        decimal ICalculationDocumentSubItem.LineDiscountPercent => LineDiscountPercent ?? 0m;
        decimal ICalculationDocumentSubItem.LineDiscountAmount => LineDiscountAmount ?? 0m;
        decimal ICalculationDocumentSubItem.LineCGSTRate => LineCGSTRate ?? 0m;
        decimal ICalculationDocumentSubItem.LineSGSTRate => LineSGSTRate ?? 0m;
        decimal ICalculationDocumentSubItem.LineIGSTRate => LineIGSTRate ?? 0m;
        decimal ICalculationDocumentSubItem.LineGross => LineGross;
        decimal ICalculationDocumentSubItem.LineBasicAmount => LineBasicAmount;
        decimal ICalculationDocumentSubItem.LineCGSTAmount => LineCGSTAmount;
        decimal ICalculationDocumentSubItem.LineSGSTAmount => LineSGSTAmount;
        decimal ICalculationDocumentSubItem.LineIGSTAmount => LineIGSTAmount;
    }
}
