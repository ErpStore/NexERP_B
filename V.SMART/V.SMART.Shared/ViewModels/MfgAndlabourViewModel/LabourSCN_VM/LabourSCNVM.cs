using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourSCN_VM
{
    public class LabourSCNVM
    {
       
        public int SCNId { get; set; }


        [Required(ErrorMessage = "Please select a Customer first.")]
        public int? CustId { get; set; }
        public string? CustName { get; set; }
        public string? CustAddress { get; set; }
        public string? CustGst { get; set; }
        public string? CustContactNo { get; set; }
        public string? KindOfAttention { get; set; }


        // 🔹 Consignee / Shipping
        public bool IsSameAsShipping { get; set; } = true;
        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstin { get; set; }
        public string? ShippingContactNo { get; set; }


        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "SCN Number is required.")]
        [StringLength(15, ErrorMessage = "SCN No Length cannot be exceed 15 Characters")]
        public string SCNNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "SCN Date is required.")]
        public DateTime SCNDate { get; set; } = DateTime.Now;
        public DateTime SCNDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please select StoreIssue")]
        public int? StoreIssId { get; set; }
        public string? IssueStoreName { get; set; }

        [Required(ErrorMessage = "Please select  StoreAdd")]
        public int? AddStoreId { get; set; }
        public string? AddStoreName { get; set; }

        //UI
        public string? RefCustDcNo { get; set; }

        public bool ShortClose { get; set; } = false;
        public bool SCNCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }

        public string CanceledBy { get; set; } = string.Empty;
        public bool SCNTally { get; set; } = false;

        [MaxLength(300, ErrorMessage = "Remark Length cannot be exceed 300 Characters")]
        public string? MainRemarks { get; set; }

        public string? KindofAttention { get; set; }

        public int NoOfItems => LabourSCNSubVMs.Count;

        public string? CancelReason { get; set; }

        public bool IsLevel1 { get; set; }
        public bool IsLevel2 { get; set; }
        public bool IsLevel3 { get; set; }
        public bool IsLevel4 { get; set; }

        
        public string? Level1Sign { get; set; }
        public string? Level2Sign { get; set; }
        public string? Level3Sign { get; set; }
        public string? Level4Sign { get; set; }
        public bool Authorized { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public bool IsRejected { get; set; }
        public string? RejectReason { get; set; }

        public string? CreatedBy { get; set; } = string.Empty;
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool IsSelected { get; set; }

        // ---------------- Calculated Totals ----------------
        public decimal TotalQty => LabourSCNSubVMs?.Sum(x => x.AcceptQty ?? 0m) ?? 0m;

        // public decimal TotalBalQty => PurchaseSCNSubVMs?.Sum(x => x.BalQty ?? 0m) ?? 0m;

        public decimal AverageUnitPrice => LabourSCNSubVMs != null && LabourSCNSubVMs.Count > 0
            ? LabourSCNSubVMs.Average(x => x.UnitPrice ?? 0m)
            : 0m;



        [MinLength(1, ErrorMessage = "At least one SCN item is required.")]
        [ValidateComplexType]
        public List<LabourSCNSubVM> LabourSCNSubVMs { get; set; } = new List<LabourSCNSubVM>();

        // -----------------------------
        // Custom / Business Validation
        // -----------------------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Vendor validation
            if (!CustId.HasValue || CustId.Value == 0)
            {
                yield return new ValidationResult(
                    "Customer is required",
                    new[] { nameof(CustId) });
            }

            // Issue store (reinforced although already [Required])
            if (!AddStoreId.HasValue || AddStoreId.Value == 0)
            {
                yield return new ValidationResult("Store Add is required.", new[] { nameof(AddStoreId) });
            }

            if (!StoreIssId.HasValue || StoreIssId.Value == 0)
            {
                yield return new ValidationResult("Store Issue is required.", new[] { nameof(StoreIssId) });
            }

            // ✅ Correct comparison: ensure they are not same
            if (StoreIssId.HasValue && AddStoreId.HasValue && StoreIssId.Value == AddStoreId.Value)
            {
                yield return new ValidationResult(
                    "Issue Store and Add Store cannot be the same.",
                    new[] { nameof(StoreIssId), nameof(AddStoreId) });
            }

            // Validate nested items
            if (LabourSCNSubVMs != null)
            {
                for (int i = 0; i < LabourSCNSubVMs.Count; i++)
                {
                    var sub = LabourSCNSubVMs[i];
                    var results = new List<ValidationResult>();
                    var ctx = new ValidationContext(sub, serviceProvider: null, items: null);

                    Validator.TryValidateObject(sub, ctx, results, validateAllProperties: true);
                    results.AddRange(sub.Validate(ctx)); // also run IValidatableObject

                    foreach (var r in results)
                    {
                        var memberNames = r.MemberNames != null && r.MemberNames.Any()
                            ? r.MemberNames.Select(m => $"{nameof(LabourSCNSubVMs)}[{i}].{m}")
                            : new[] { $"{nameof(LabourSCNSubVMs)}[{i}]" };

                        yield return new ValidationResult(r.ErrorMessage, memberNames);
                    }
                }
            }
        }

    }
}
