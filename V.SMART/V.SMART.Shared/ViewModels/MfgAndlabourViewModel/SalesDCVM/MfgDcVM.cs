using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM
{
    public class MfgDcVM : IValidatableObject
    {
        public int DcId { get; set; }

        public int? CustId { get; set; }
        public string? CustName { get; set; }
        public string? CustAddress { get; set; }
        public string? CustGstNo { get; set; }
        public string? ContactNo { get; set; }
        public string? KindofAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;
        public int? ShippingId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstNo { get; set; }
        public string? ShippingPhone { get; set; }

        [Required(ErrorMessage = "Issue Store is required.")]
        public int? IssueStoreId { get; set; }
        public string? StoreName { get; set; }


        public bool IsPurchaseReturn { get; set; } = false;
        public int? VendorCode { get; set; }
        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }
        public string? VendorGstNo { get; set; }
        public string? VendorContactNo { get; set; }

        public int? RefGRNId { get; set; }
        public string? RefGRNNo { get; set; }

        [Required(ErrorMessage = "Mode of Transport is required.")]
        public int? ModeofTrasnport { get; set; } = 1;

        [Required(ErrorMessage = "Vehicle No is required.")]
        [RegularExpression(@"^[A-Z]{2}[0-9]{1,2}[A-Z]{1,2}[0-9]{4}$",
            ErrorMessage = "Invalid Vehicle No (e.g., KA01AB1234).")]
        public string VehicleNo { get; set; } = string.Empty;

        [Required, StringLength(50, ErrorMessage = "Transport From cannot exceed 50 characters.")]
        public string? TransFrom { get; set; }

        [Required, StringLength(50, ErrorMessage = "Transport To cannot exceed 50 characters.")]
        public string? TransTo { get; set; }

        [StringLength(100, ErrorMessage = "Transport Id cannot exceed 100 characters.")]
        public string? TransId { get; set; }

        [StringLength(100, ErrorMessage = "Transport DocNo cannot exceed 100 characters.")]
        public string? TransDocNo { get; set; }

        [StringLength(100, ErrorMessage = "Transport Name cannot exceed 100 characters.")]
        public string? TransName { get; set; }

        [Required(ErrorMessage = "Select the Sub Supply type for E-Way Bill generation.")]
        public int? SubSuppType { get; set; } = 1;

        [StringLength(100, ErrorMessage = "SubSupply Description  cannot exceed 100 characters.")]
        public string? SubSupplyDesc { get; set; }

        public string? EwayBillNo { get; set; }
        public DateTime? EwayBillDate { get; set; }

        public bool IsWithoutPoDc { get; set; } = false;

        public bool DcShortClose { get; set; } = false;

        [StringLength(10,ErrorMessage = "Prefix Length cannot be exceed 10 Characters")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "DC Number is required.")]
        [StringLength(15, ErrorMessage = "DC No Length cannot be exceed 15 Characters")]
        public string DcNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "DC Date is required.")]
        public DateTime DcDate { get; set; } = DateTime.Now;

        public DateTime DcDateNow { get; set; } = DateTime.Now;

        [MaxLength(300, ErrorMessage ="Towards Length cannot be exceed 300 Characters")]
        public string? Towards { get; set; }

        [MaxLength(500, ErrorMessage = "Dc Remark Length cannot be exceed 500 Characters")]
        public string? MainRemarks { get; set; }

        [MaxLength(200, ErrorMessage = "Rejection Remark Length cannot be exceed 200 Characters")]
        public string? RejRemark { get; set; }

        public bool DcCancel { get; set; } = false;
        [StringLength(120, ErrorMessage = "Cancel Reason cannot exceed 120 characters.")]

        public string? CancelReason { get; set; }

        public bool DcTally { get; set; }= false;

        public bool ReduceInvAsPerRej { get; set; }= false;

        public bool? IsManualDcNo { get; set; } = false;

        public bool Selected { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        // ---------------- Calculated Totals ----------------
        public decimal TotalQty => MfgDcSubVMs?.Sum(x => x.Qty ?? 0) ?? 0;

        public decimal TotalBalQty => MfgDcSubVMs?.Sum(x => x.BalQty) ?? 0;

        public decimal AverageUnitPrice => MfgDcSubVMs != null && MfgDcSubVMs.Count > 0
            ? MfgDcSubVMs.Average(x => x.UnitPrice ?? 0)
            : 0;


        [MinLength(1, ErrorMessage = "At least one DC item is required.")]
        [ValidateComplexType]
        public List<MfgDcSubVM> MfgDcSubVMs { get; set; } = new List<MfgDcSubVM>();


        // -----------------------------
        // Custom / Business Validation
        // -----------------------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Purchase Return validations
            if (IsPurchaseReturn)
            {
                if (!VendorCode.HasValue || VendorCode.Value == 0)
                {
                    yield return new ValidationResult(
                        "Vendor is required for Purchase Return.",
                        new[] { nameof(VendorCode) });
                }

                //if (!RefGRNId.HasValue || RefGRNId.Value == 0)
                //{
                //    yield return new ValidationResult(
                //        "GRN selection is required for Purchase Return.",
                //        new[] { nameof(RefGRNId) });
                //}
            }
            else
            {
                // Normal DC requires customer
                if (!CustId.HasValue || CustId.Value == 0)
                {
                    yield return new ValidationResult(
                        "Customer is required when not a Purchase Return.",
                        new[] { nameof(CustId) });
                }
            }

            // Shipping check
            if (!IsSameAsShipping && (!ShippingId.HasValue || ShippingId.Value == 0))
            {
                yield return new ValidationResult(
                    "Consignee (Shipping) is required when 'IsSameAsShipping' is false.",
                    new[] { nameof(ShippingId) });
            }

            // Issue store (reinforced although already [Required])
            if (!IssueStoreId.HasValue || IssueStoreId.Value == 0)
            {
                yield return new ValidationResult("Issue Store is required.", new[] { nameof(IssueStoreId) });
            }

            // Transport group validation: all-or-nothing
            bool hasTransId = !string.IsNullOrWhiteSpace(TransId);
            bool hasTransDocNo = !string.IsNullOrWhiteSpace(TransDocNo);
            bool hasTransName = !string.IsNullOrWhiteSpace(TransName);

            if (hasTransId || hasTransDocNo || hasTransName)
            {
                if (!hasTransId)
                    yield return new ValidationResult("Transport Id is required when any transport details are provided.", new[] { nameof(TransId) });

                if (!hasTransDocNo)
                    yield return new ValidationResult("Transport DocNo is required when any transport details are provided.", new[] { nameof(TransDocNo) });

                if (!hasTransName)
                    yield return new ValidationResult("Transport Name is required when any transport details are provided.", new[] { nameof(TransName) });
            }

            // Validate nested items with proper member names
            if (MfgDcSubVMs != null)
            {
                for (int i = 0; i < MfgDcSubVMs.Count; i++)
                {
                    var sub = MfgDcSubVMs[i];
                    var results = new List<ValidationResult>();
                    var ctx = new ValidationContext(sub, serviceProvider: null, items: null);

                    Validator.TryValidateObject(sub, ctx, results, validateAllProperties: true);
                    results.AddRange(sub.Validate(ctx)); // also run IValidatableObject

                    foreach (var r in results)
                    {
                        var memberNames = r.MemberNames != null && r.MemberNames.Any()
                            ? r.MemberNames.Select(m => $"{nameof(MfgDcSubVMs)}[{i}].{m}")
                            : new[] { $"{nameof(MfgDcSubVMs)}[{i}]" };

                        yield return new ValidationResult(r.ErrorMessage, memberNames);
                    }
                }
            }
        }
    }

}
