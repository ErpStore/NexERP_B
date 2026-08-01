using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel
{
    public class ItemSubVM
    {
        public int Id { get; set; }


        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; } 


        public int? VendorId { get; set; }
        public string? VendorName { get; set; }

        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }


        [Required(ErrorMessage = "Please Select Customer Or Vendor")]
        public int ClassId { get; set; }


        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be greater than 0.01")]
        public decimal Rate { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // ===================== CUSTOM VALIDATION =====================

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ClassId == 1 && CustomerId == null)
            {
                yield return new ValidationResult(
                    "Please select a Customer.",
                    new[] { nameof(CustomerId) }
                );
            }

            if (ClassId == 2 && VendorId == null)
            {
                yield return new ValidationResult(
                    "Please select a Vendor.",
                    new[] { nameof(VendorId) }
                );
            }
        }
    }
}
