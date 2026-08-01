using V.SMART.Shared.Utility_Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Services;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM
{
    [ScreenMapping("Sales Order")]
    public class MfgPoSubVM : ICalculationDocumentSubItem
    {
        public int PoSubId { get; set; }
        public int PoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No. must be greater than zero.")]
        public int SlNo { get; set; }
        public int? LineNo { get; set; }

        [Required(ErrorMessage = "Item is required.")]
        public int? ItemId { get; set; }

        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        [StringLength(500, ErrorMessage = "Item Specification cannot exceed 500 characters.")]
        public string? ItemSpecification { get; set; }
        public string? HSNCode { get; set; }
        public string? MeasureUnit { get; set; }
        public int? RefQuoteSubId { get; set; }
        public string? RefQuoteNo { get; set; }
        public DateTime? RefQuoteDate { get; set; }

        public decimal? Qty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Bal Qty must be non-negative.")]
        public decimal? BalQty { get; set; }

        [Range(0, double.MaxValue)]
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

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "PO date is required.")]
        public DateTime PoDate { get; set; } = DateTime.Now;

        public string? WoNO { get; set; }

        [Range(0, double.MaxValue)]
        public decimal IndentBalQty { get; set; }

        [Range(0, double.MaxValue)]
        public decimal WOBalQty { get; set; }

        [Range(0, double.MaxValue)]
        public decimal PerformaBalQty { get; set; }

        [Range(0, double.MaxValue)]
        public decimal RouteCardBalQty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal ProdCompBalQty { get; set; }

        public bool ItemCancel { get; set; } = false;
        public string? ItemCancelReason { get; set; }


        public bool IsAnnexure { get; set; } = false;

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        [StringLength(500, ErrorMessage = "Max length should be 500 characters.")]
        public string? Remark { get; set; }

        public bool IsEditable { get; set; } = false;

        public decimal? LabourCost { get; set; }

        public bool CanEditItem => (BalQty ?? 0) == (Qty ?? 0) &&
            IndentBalQty == Qty.GetValueOrDefault() &&
            WOBalQty == Qty.GetValueOrDefault() &&
            PerformaBalQty == Qty.GetValueOrDefault() &&
            RouteCardBalQty == Qty.GetValueOrDefault() &&
            ProdCompBalQty == Qty.GetValueOrDefault() &&
            (RefQuoteSubId ?? 0) == 0 &&
            !ItemCancel;


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


        public void InitializeBalances()
        {
            var baseQty = Qty ?? 0m;
            BalQty = baseQty;
            IndentBalQty = baseQty;
            WOBalQty = baseQty;
            PerformaBalQty = baseQty;
            RouteCardBalQty = baseQty;
        }
    }

}
