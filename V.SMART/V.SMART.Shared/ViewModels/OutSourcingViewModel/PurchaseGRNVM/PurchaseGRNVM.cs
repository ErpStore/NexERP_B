using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.Icons;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM
{
    public class PurchaseGRNVM : IValidatableObject
    {
        public int GRNId { get; set; }

        public int? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }
        public string? VendorGstNo { get; set; }
        public string? VendorContactNo { get; set; }
        public string? KindofAttention { get; set; }

        [Required(ErrorMessage = "Add Store is required.")]
        public int? AddStoreId { get; set; }
        public string? StoreName { get; set; }


        public bool IsWithoutPoGRN { get; set; } = false;
        public bool IsWithoutInspection { get; set; } = false;

        [Required(ErrorMessage = "GRN Number is required.")]
        [StringLength(15, ErrorMessage = "GRN No Length cannot be exceed 15 Characters")]
        public string GRNNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "GRN Date is required.")]
        public DateTime GRNDate { get; set; } = DateTime.Now;
        public DateTime GRNDateNow { get; set; } = DateTime.Now;

   
        public string? RefDcNo { get; set; } = string.Empty;
        public DateTime? RefDcDate { get; set; } = DateTime.Now;

    
        public string? RefInvNo { get; set; } = string.Empty;
        public DateTime? RefInvDate { get; set; } = DateTime.Now;

        [MaxLength(250, ErrorMessage = "GRN Remark Length cannot be exceed 250 Characters")]
        public string? MainRemarks { get; set; }
        public int NoOfItems => PurchaseGRNSubVMs.Count;
        public bool GRNCancel { get; set; } = false;
        public bool ShortClose { get; set; }

        public bool GRNTally { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }

        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // ---------------- Calculated Totals ----------------
        public decimal TotalQty => PurchaseGRNSubVMs?.Sum(x => x.Qty ?? 0) ?? 0;

        public decimal TotalBalQty => PurchaseGRNSubVMs?.Sum(x => x.BalQty) ?? 0;

        public decimal AverageUnitPrice => PurchaseGRNSubVMs != null && PurchaseGRNSubVMs.Count > 0
            ? PurchaseGRNSubVMs.Average(x => x.UnitPrice ?? 0)
            : 0;


        [MinLength(1, ErrorMessage = "At least one GRN item is required.")]
        [ValidateComplexType]
        public List<PurchaseGRNSubVM> PurchaseGRNSubVMs { get; set; } = new List<PurchaseGRNSubVM>();


        // -----------------------------
        // Custom / Business Validation
        // -----------------------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Purchase Return validations
            if (!VendorCode.HasValue || VendorCode.Value == 0)
            {
                yield return new ValidationResult(
                    "Vendor is required",
                    new[] { nameof(VendorCode) });
            }
            //DcNo and Invoice no validation
            if (string.IsNullOrWhiteSpace(RefDcNo) && string.IsNullOrWhiteSpace(RefInvNo))
            {
                yield return new ValidationResult(
                    "Either Ref DC No or Ref Invoice No must be entered.",
                    new[] { nameof(RefDcNo), nameof(RefInvNo) });
            }

        }

    }

}
