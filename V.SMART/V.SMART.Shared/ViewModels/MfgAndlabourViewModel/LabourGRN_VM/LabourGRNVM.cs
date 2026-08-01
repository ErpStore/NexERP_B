using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.LabourGRN_VM
{
    public class LabourGRNVM
    {
        public int GRNId { get; set; }

        public bool IsOutItemSameAsInItem { get; set; } = false;

        [Required(ErrorMessage = "Customer is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Customer.")]
        public int CustId { get; set; }

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


        [Required(ErrorMessage = "GRN No is required.")]
        public string GRNNo { get; set; } = string.Empty;

        public string Suffix { get; set; }

        [Required(ErrorMessage = "GRN Date is required.")]
        public DateTime GRNDate { get; set; } = DateTime.Now;

        public DateTime GRNDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please eneter customer DcNo first.")]
        public string? RefDcNo { get; set; }

        public DateTime? RefDcDate { get; set; } = DateTime.Now;


        public bool DcTally { get; set; } = false;

        public bool DcCancel { get; set; } = false;

        public DateTime? CancelDate { get; set; }

        [StringLength(150)]
        public string? CancelBy { get; set; }

        public bool ShortClose { get; set; } = false;

        [Required(ErrorMessage = "Please select add store first.")]
        public int AddStoreId { get; set; }
        public string? StoreName { get; set; }

        public int? NoOfItems { get; set; }

        public string? TransportMode { get; set; }

        public string? VehicleNo { get; set; }

        public string? TransFrom { get; set; }

        public string? TransTo { get; set; }

        public string? Remarks { get; set; }

        public string? KindofAttention { get; set; }

        public string? CancelReason { get; set; }

        public string CreatedBy { get; set; } = string.Empty;
       
        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool WithoutPoGRN { get; set; } = false;

        public bool IsSelected { get; set; }

        public string? BatchNo { get; set; }

        // ---------------- Calculated Totals ----------------

        public decimal TotalDnQty =>
            LabourGRNSubVMs == null ? 0 :
            (IsOutItemSameAsInItem
                ? LabourGRNSubVMs.Where(x => x.TransType == "In").Sum(x => x.DCQty ?? 0)
                : LabourGRNSubVMs.Sum(x => x.DCQty ?? 0));

        public decimal TotalQty =>
            LabourGRNSubVMs == null ? 0 :
            (IsOutItemSameAsInItem
                ? LabourGRNSubVMs.Where(x => x.TransType == "In").Sum(x => x.Qty ?? 0)
                : LabourGRNSubVMs.Sum(x => x.Qty ?? 0));


        public decimal TotalBalQty => LabourGRNSubVMs?.Sum(x => x.BalQty) ?? 0;

        public decimal AverageUnitPrice =>
                     LabourGRNSubVMs?
                         .Where(x => !IsOutItemSameAsInItem || x.TransType == "In")
                         .Select(x => x.UnitPrice ?? 0)
                         .DefaultIfEmpty(0)
                         .Average() ?? 0;


        [MinLength(1, ErrorMessage = "At least one GRN item is required.")]
        [ValidateComplexType]
        public List<LabourGRNSubVM> LabourGRNSubVMs { get; set; } = new List<LabourGRNSubVM>();


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Purchase Return validations
            if (CustId == 0)
            {
                yield return new ValidationResult(
                    "Customer is required",
                    new[] { nameof(CustId) });
            }
            //DcNo and Invoice no validation
            if (string.IsNullOrWhiteSpace(RefDcNo))
            {
                yield return new ValidationResult(
                    "Please Ref DC No must be entered.",
                    new[] { nameof(RefDcNo)});
            }

            // Add store (reinforced although already [Required])
            if (AddStoreId == 0)
            {
                yield return new ValidationResult("Add Store is required.", new[] { nameof(AddStoreId) });
            }

            // Validate nested items with proper member names
            if (LabourGRNSubVMs != null)
            {
                for (int i = 0; i < LabourGRNSubVMs.Count; i++)
                {
                    var sub = LabourGRNSubVMs[i];
                    var results = new List<ValidationResult>();
                    var ctx = new ValidationContext(sub, serviceProvider: null, items: null);

                    Validator.TryValidateObject(sub, ctx, results, validateAllProperties: true);
                    results.AddRange(sub.Validate(ctx)); // also run IValidatableObject

                    foreach (var r in results)
                    {
                        var memberNames = r.MemberNames != null && r.MemberNames.Any()
                            ? r.MemberNames.Select(m => $"{nameof(LabourGRNSubVMs)}[{i}].{m}")
                            : new[] { $"{nameof(LabourGRNSubVMs)}[{i}]" };

                        yield return new ValidationResult(r.ErrorMessage, memberNames);
                    }
                }
            }
        }

    }
}
