using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel
{
    public class ProductionSCNCompSubVM : IValidatableObject
    {
        public int SCNSubId { get; set; }
        public int SCNId { get; set; }

        public int? RefReturnSubId { get; set; }

        [MaxLength(50)]
        public string? RefReturnNo { get; set; }

        public DateTime? RefReturnDate { get; set; }


        public int? RcSubId { get; set; }
        public string? RcNo { get; set; }
        public DateTime? RcDate { get; set; }
        public string? Process { get; set; }

        public int? PoSubId { get; set; }
        public string? PoNo { get; set; }
        public DateTime? PoDate { get; set; }


        public int SlNo { get; set; }

        [Required(ErrorMessage = "Item selection is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Item.")]
        public int? ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }

        [Precision(18, 3)]
        public decimal? AccQty { get; set; }

        [Precision(18, 3)]
        public decimal? RejQty { get; set; }

        [MaxLength(200)]
        public string? RejReason { get; set; }

        [Precision(18, 3)]
        public decimal? StockQty { get; set; }

        [Precision(18, 3)]
        public decimal? RewQty { get; set; }

        [MaxLength(200)]
        public string? RewReason { get; set; }

        [Precision(18, 3)]
        public decimal? BalQty { get; set; }

        [Precision(18, 3)]
        public decimal? UnitPrice { get; set; }

        [MaxLength(50)]
        public string? InsNo { get; set; }

        [MaxLength(50)]
        public string? NCRNo { get; set; }

        public int? CostId { get; set; }

        [MaxLength(50)]
        public string? ProjectNo { get; set; }

        [MaxLength(250)]
        public string? Remark { get; set; }

        public int? RejectionId { get; set; }

        public string? RejectionCode { get; set; }

        public string? RejectionDescription { get; set; }

        // UI-only field
        public bool IsEditable { get; set; } = false;

        public decimal? QtyReceived { get; set; }

        public decimal? SCNBalQty { get; set; }

        // ---------------------------------------------------------
        // 📌 CONDITIONAL OBJECT-LEVEL VALIDATION
        // ---------------------------------------------------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Rejected Reason Required
            if (RejQty.HasValue && RejQty.Value > 0)
            {
                if (string.IsNullOrWhiteSpace(RejReason))
                {
                    yield return new ValidationResult(
                        "Rejection Reason is required when Rejected Qty is greater than 0.",
                        new[] { nameof(RejReason) }
                    );
                }
            }

            // Rework Reason Required
            if (RewQty.HasValue && RewQty.Value > 0)
            {
                if (string.IsNullOrWhiteSpace(RewReason))
                {
                    yield return new ValidationResult(
                        "Rework Reason is required when Rework Qty is greater than 0.",
                        new[] { nameof(RewReason) }
                    );
                }
            }
        }
    }
}
