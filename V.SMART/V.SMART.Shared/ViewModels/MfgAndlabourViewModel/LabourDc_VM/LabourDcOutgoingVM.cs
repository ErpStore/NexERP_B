using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourDc_VM
{
    public class LabourDcOutgoingVM
    {
        public int DcId { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
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

        [StringLength(10, ErrorMessage = "Prefix Length cannot be exceed 10 Characters")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Dc No is required.")]
        [StringLength(15, ErrorMessage = "DC No Length cannot be exceed 15 Characters")]
        public string DcNo { get; set; }

        [Required(ErrorMessage = "Suffix is required.")]
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Dc Date is required.")]
        public DateTime DcDate { get; set; } = DateTime.Now;
        public DateTime DcDateNow { get; set; } = DateTime.Now;


        [Required(ErrorMessage = "Mode of Transport is required.")]
        public int? TransportMode { get; set; }

        [RegularExpression(@"^[A-Z]{2}[0-9]{1,2}[A-Z]{1,2}[0-9]{4}$",
            ErrorMessage = "Invalid Vehicle No (e.g., KA01AB1234).")]
        public string? VehicleNo { get; set; } = string.Empty;

        [StringLength(50, ErrorMessage = "Transport From cannot exceed 50 characters.")]
        public string? TransFrom { get; set; }

        [StringLength(50, ErrorMessage = "Transport To cannot exceed 50 characters.")]
        public string? TransTo { get; set; }

        public int NoOfItems { get; set; }

        public bool DcCancel { get; set; } = false;

        public bool DcTally { get; set; } = false;

        [MaxLength(250, ErrorMessage = "Dc Remark Length cannot be exceed 250 Characters")]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "StoreIssue is required.")]
        public int? StoreIssId { get; set; }
        public string? StoreName { get; set; }


        public string? EwayNo { get; set; }
        public DateTime? EwayDate { get; set; }


        public bool DcSign { get; set; } = false;

        public bool NonReturnDc { get; set; } = false;

        public bool WithoutPoDc { get; set; } = false;

        public bool ReceivedReturnRM { get; set; } = false;

        public bool SCNRejectReworkReturn { get; set; } = false;

        public bool ShortClose { get; set; } = false;

        public bool Manual { get; set; } = false;

        [StringLength(30, ErrorMessage = "Transport Id cannot exceed 30 characters.")]
        public string? TransId { get; set; }


        [StringLength(100, ErrorMessage = "Transport EwayTransName cannot exceed 100 characters.")]
        public string? TransName { get; set; }


        [StringLength(100, ErrorMessage = "Transport DocNo cannot exceed 30 characters.")]
        public string? TransDocNo { get; set; }

        [Required(ErrorMessage = "Select the Sub Supply type for E-Way Bill generation.")]
        public int? SubSupplyType { get; set; }

        [StringLength(100, ErrorMessage = "Sub Supply Description cannot exceed 100 characters")]
        public string? SubSupplyDesc { get; set; }


        [MaxLength(150)]
        public string? CancelReason { get; set; }

        public DateTime? CanceledDate { get; set; }

        [MaxLength(50)]
        public string CanceledBy { get; set; } = string.Empty;


        [MaxLength(250, ErrorMessage = "RejectionReason cannot exceed 250 characters")]
        public string? RejectionReason { get; set; }

        public bool ReduceInvAsPerRej { get; set; }

        public bool? IsManualDcNo { get; set; } = false;


        [MaxLength(50)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }


        // ---------------- Calculated Totals ----------------
        public decimal TotalQty => LabourDcOutgoingSubVMs?.Sum(x => x.Qty ?? 0) ?? 0;

        public decimal TotalBalQty => LabourDcOutgoingSubVMs?.Sum(x => x.BalQty) ?? 0;

        public decimal AverageUnitPrice => LabourDcOutgoingSubVMs != null && LabourDcOutgoingSubVMs.Count > 0
            ? LabourDcOutgoingSubVMs.Average(x => x.UnitPrice ?? 0)
            : 0;


        [MinLength(1, ErrorMessage = "At least one DC item is required.")]
        [ValidateComplexType]
        public List<LabourDcOutgoingSubVM> LabourDcOutgoingSubVMs { get; set; } = new List<LabourDcOutgoingSubVM>();

        // -----------------------------
        // Custom / Business Validation
        // -----------------------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Normal DC requires customer
            if (!CustId.HasValue || CustId.Value == 0)
            {
                yield return new ValidationResult(
                    "Customer is required when not a Purchase Return.",
                    new[] { nameof(CustId) });
            }
            // Shipping check
            if (!IsSameAsShipping && (!ShippingId.HasValue || ShippingId.Value == 0))
            {
                yield return new ValidationResult(
                    "Consignee (Shipping) is required when 'IsSameAsShipping' is false.",
                    new[] { nameof(ShippingId) });
            }

            // Issue store (reinforced although already [Required])
            if (!StoreIssId.HasValue || StoreIssId.Value == 0)
            {
                yield return new ValidationResult("Issue Store is required.", new[] { nameof(StoreIssId) });
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
            if (LabourDcOutgoingSubVMs != null)
            {
                for (int i = 0; i < LabourDcOutgoingSubVMs.Count; i++)
                {
                    var sub = LabourDcOutgoingSubVMs[i];
                    var results = new List<ValidationResult>();
                    var ctx = new ValidationContext(sub, serviceProvider: null, items: null);

                    Validator.TryValidateObject(sub, ctx, results, validateAllProperties: true);
                    results.AddRange(sub.Validate(ctx)); // also run IValidatableObject

                    foreach (var r in results)
                    {
                        var memberNames = r.MemberNames != null && r.MemberNames.Any()
                            ? r.MemberNames.Select(m => $"{nameof(LabourDcOutgoingSubVMs)}[{i}].{m}")
                            : new[] { $"{nameof(LabourDcOutgoingSubVMs)}[{i}]" };

                        yield return new ValidationResult(r.ErrorMessage, memberNames);
                    }
                }
            }
        }
    }
}
