using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class ToolCribReturnSubVM : IValidatableObject
    {
        public int TCReturnSubId { get; set; }
        public int TCReturnId { get; set; }

        public int? RefTCIssueSubId { get; set; }
        public string? RefTCIssueNo { get; set; }
        public DateTime? RefTCIssueDate { get; set; }

        [Required(ErrorMessage = "Serial number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be at least 1.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select an Item Code.")]
        public int? ItemId { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Accepted quantity cannot be negative.")]
        public decimal? AccpQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Rejected quantity cannot be negative.")]
        public decimal? RejQty { get; set; }

        [StringLength(100, ErrorMessage = "Rejection remark cannot exceed 100 characters.")]
        public string? RejRemark { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Rework quantity cannot be negative.")]
        public decimal? RewQty { get; set; }

        [StringLength(100, ErrorMessage = "Rework remark cannot exceed 100 characters.")]
        public string? RewRemark { get; set; }

        [StringLength(100, ErrorMessage = "Remark cannot exceed 100 characters.")]
        public string? Remark { get; set; }

        public bool IsEditable { get; set; } = false;



        // ✅ Custom Validation Rules
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var results = new List<ValidationResult>();

            if (RejQty > 0 && string.IsNullOrWhiteSpace(RejRemark))
            {
                results.Add(new ValidationResult(
                    "Rejection remark is required when Rejected Quantity is greater than 0.",
                    new[] { nameof(RejRemark) }));
            }

            if (RewQty > 0 && string.IsNullOrWhiteSpace(RewRemark))
            {
                results.Add(new ValidationResult(
                    "Rework remark is required when Rework Quantity is greater than 0.",
                    new[] { nameof(RewRemark) }));
            }

            return results;
        }
    }


}
