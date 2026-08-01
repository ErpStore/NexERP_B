using V.SMART.Shared.Utility_Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.PerformaInvoiceVM
{
    public class PerformaInvSubVM : ICalculationDocumentSubItem
    {
        public int InvSubId { get; set; }
        public int InvId { get; set; }

        [Required(ErrorMessage = "Slno. is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No. must be greater than zero.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage ="Item-Code is required")]
        public int? ItemId { get; set; }

        [Required(ErrorMessage = "Item-Code is required")]
        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? HSNCode { get; set; }
        public string? MeasureUnit { get; set; }

        public int? RefPoSubId { get; set; }
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal? Qty { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal? UnitPrice { get; set; }

        [Precision(18, 3)]
        public decimal? LineDiscountPercent { get; set; }

        [Precision(18, 4)]
        public decimal? LineDiscountAmount { get; set; }

        [Precision(18, 4)]
        public decimal? LineCGSTRate { get; set; }

        [Precision(18, 4)]
        public decimal? LineSGSTRate { get; set; }

        [Precision(18, 4)]
        public decimal? LineIGSTRate { get; set; }

        public DateTime? DueDate { get; set; } = DateTime.Now;

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        [StringLength(500,ErrorMessage = "Remarks Should not Exceed more than 500 Characters")]
        public string? Remark { get; set; }
        public bool IsEditable { get; set; } = false;

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
