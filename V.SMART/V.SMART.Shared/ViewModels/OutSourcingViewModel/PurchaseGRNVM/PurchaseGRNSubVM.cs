using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM
{
    public class PurchaseGRNSubVM
    {
        public int GRNSubId { get; set; }

        [Required(ErrorMessage = "GRNId is required.")]
        public int GRNId { get; set; }

        [Required(ErrorMessage = "Sl No is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No must be greater than 0.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select ItemCode.")]
        public int? ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal? Weight { get; set; }
        public decimal? QtyConvert { get; set; }
        public decimal? AltRate { get; set; }

        public string? Category { get; set; }


        public bool IsOpenPo { get; set; } = false;

        [Required(ErrorMessage = "Qty is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal? Qty { get; set; }

        [Required(ErrorMessage = "DNQty is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "DN Quantity must be greater than 0.")]
        public decimal? DNQty { get; set; }


        [Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal? UnitPrice { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "BalQty cannot be negative.")]
        public decimal? BalQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Extra Qty cannot be negative.")]
        public decimal ExtraQty { get; set; }

        [Precision(18, 3)]
        public decimal ExtraBalQty { get; set; }

        public int? RefPoSubId { get; set; }
        public string? RefPoNo { get; set; }
        public decimal? POBalQty { get; set; }
        public DateTime? RefPoDate { get; set; }
        public DateTime? PoDueDate { get; set; }

        public string? Remark { get; set; }
        public bool ItemCancel { get; set; } = false;
        public string? HeatNo { get; set; }
        public string? BatchNo { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public bool IsEditable { get; set; } = false;
        public string? ItemCancelReason { get; set; }

        public int? InspectId { get; set; }
        public string? InspectNo { get; set; }
        public DateTime? InspectDate { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Qty vs BalQty
            if (BalQty < 0)
                yield return new ValidationResult("BalQty cannot be negative.", new[] { nameof(BalQty) });

            if (BalQty > Qty)
                yield return new ValidationResult("BalQty cannot exceed Qty.", new[] { nameof(BalQty) });

        }
    }
}
