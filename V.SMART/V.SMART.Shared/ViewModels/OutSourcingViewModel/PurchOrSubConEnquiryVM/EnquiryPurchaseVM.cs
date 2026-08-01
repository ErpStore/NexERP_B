using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM
{

    public class EnquiryPurchaseVM : IValidatableObject
    {
        public int EnquiryId { get; set; }

        [Required(ErrorMessage = "Please select Purchase or Subcontract.")]
        public bool PurchORSubCon { get; set; } = true;

        [StringLength(10, ErrorMessage = "Prefix cannot exceed 10 characters.")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Enquiry Number is required.")]
        [StringLength(50, ErrorMessage = "Enquiry Number cannot exceed 50 characters.")]
        public string EnquiryNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(10, ErrorMessage = "Suffix cannot exceed 10 characters.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Enquiry Date is required.")]
        public DateTime EnquiryDate { get; set; } = DateTime.Now;

        public DateTime EnquiryDateNow { get; set; } = DateTime.Now;

        public int NoOfItems => EnquiryPurchaseSubVMs?.Count ?? 0;

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? MainRemarks { get; set; }

        [StringLength(100, ErrorMessage = "Kind of Attention cannot exceed 100 characters.")]
        public string? KindOfAttention { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total Value must be zero or positive.")]
        public decimal TotalValue { get; set; }


        public bool EnquiryShortClose { get; set; } = false;
        public bool EnquiryTally { get; set; } = false;

        public bool Cancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }

        [StringLength(500, ErrorMessage = "Cancellation Remarks cannot exceed 500 characters.")]
        public string? CancellationRemarks { get; set; }

        public string? CancelBy { get; set; }

        [StringLength(100, ErrorMessage = "Contact Person Name cannot exceed 100 characters.")]
        public string? POCName { get; set; }

        [RegularExpression(@"^[0-9]{10}$", ErrorMessage = "Contact Number must be exactly 10 digits.")]
        public string? POCContactNo { get; set; }

        public DateTime? ExpactedReplayDate { get; set; }

        [StringLength(50, ErrorMessage = "Created By cannot exceed 50 characters.")]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50, ErrorMessage = "Modified By cannot exceed 50 characters.")]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [MinLength(1, ErrorMessage = "Please select at least one Vendor.")]
        public List<VendorVM> SelectedVendors { get; set; } = new();

        [MinLength(1, ErrorMessage = "At least one Enquiry Item is required.")]
        [ValidateComplexType]
        public List<EnquiryPurchaseSubVM> EnquiryPurchaseSubVMs { get; set; } = new();

        public List<EnquiryPurchaseVendorAssignVM> EnquiryPurchaseVendorAssignVMs { get; set; } = new();

        // ================= CUSTOM VALIDATION =================
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {

            if (ExpactedReplayDate.HasValue && ExpactedReplayDate < EnquiryDate)
            {
                yield return new ValidationResult(
                    "Expected Reply Date cannot be earlier than Enquiry Date.",
                    new[] { nameof(ExpactedReplayDate) }
                );
            }

            if (EnquiryPurchaseSubVMs == null || EnquiryPurchaseSubVMs.Count == 0)
            {
                yield return new ValidationResult(
                    "Please add at least one Enquiry Item.",
                    new[] { nameof(EnquiryPurchaseSubVMs) }
                );
            }

            if (EnquiryPurchaseSubVMs.Count > 0 && (SelectedVendors == null || SelectedVendors.Count == 0))
            {
                yield return new ValidationResult(
                    "Please select at least one Vendor before saving the Enquiry.",
                    new[] { nameof(SelectedVendors) }
                );
            }
        }
    }


}
