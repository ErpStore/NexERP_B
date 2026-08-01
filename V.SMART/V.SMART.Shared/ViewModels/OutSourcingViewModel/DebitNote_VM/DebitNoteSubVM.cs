
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.DebitNote_VM
{
    public class DebitNoteSubVM : ICalculationDocumentSubItem
    {
        public int DbSubId { get; set; }

        public int DbId { get; set; }

        public int SlNo { get; set; }

        public int ItemId { get; set; }

        public ItemVM? SelectedItem { get; set; }

        public string? ItemCode { get; set; }

        public string? ItemName { get; set; }

        public string? Specification { get; set; }

        [StringLength(20, ErrorMessage = "HSN Code cannot exceed 20 characters.")]
        public string? HsnCode { get; set; }

        [StringLength(20, ErrorMessage = "Unit of Measure cannot exceed 20 characters.")]
        public string? MeasureUnit { get; set; }

        public string? Category { get; set; }



        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal? Qty { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Unit price must be non-negative.")]
        public decimal? UnitPrice { get; set; }

        [Precision(18, 3)]
        public decimal RejQty { get; set; }

        [Precision(18, 3)]
        public decimal RewQty { get; set; }


        [Precision(18, 3)]
        public decimal CrDrQty { get; set; }

        [Precision(18, 3)]
        public decimal CrDrUnitPrice { get; set; }

        public decimal? DbBalQty { get; set; }


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


        public bool IsEditable { get; set; } = false;

        public int? RefPurchInvSubId { get; set; }
        public string? RefPurcInvNo { get; set; }
        public DateTime? RefPurchInvDate { get; set; }
        public decimal? PurchDbBalQty { get; set; }

        public int? RefSubConInvSubId { get; set; }
        public string? RefSubConInvNo { get; set; }
        public DateTime? RefSubConInvDate { get; set; }
        public decimal? SubDbBalQty { get; set; }

        public int? RefPoSubId { get; set; }


        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public DebitNoteVM? DebitNote { get; set; }

        public bool IsRejection => DebitNote?.Rejection ?? false;

        // ===== Computed properties =====
        public decimal LineGross => IsRejection ? ((Qty ?? 0m) * (UnitPrice ?? 0m)) : (CrDrQty * CrDrUnitPrice);

        public decimal LineBasicAmount => LineGross - (LineDiscountAmount ?? 0m);
        public decimal LineCGSTAmount => LineBasicAmount * (LineCGSTRate ?? 0m) / 100m;
        public decimal LineSGSTAmount => LineBasicAmount * (LineSGSTRate ?? 0m) / 100m;
        public decimal LineIGSTAmount => LineBasicAmount * (LineIGSTRate ?? 0m) / 100m;

        // ===== ICalculationDocumentSubItem implementation =====

        decimal ICalculationDocumentSubItem.Qty => IsRejection ? Qty ?? 0m : CrDrQty;
        decimal ICalculationDocumentSubItem.UnitPrice => IsRejection ? UnitPrice ?? 0m : CrDrUnitPrice;

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
