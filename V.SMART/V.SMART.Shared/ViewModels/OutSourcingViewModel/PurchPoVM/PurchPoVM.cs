using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM
{
    public class PurchPoVM : ICalculationDocument, IValidatableObject
    {
        [Key]
        public int PoId { get; set; }

        [Required(ErrorMessage = "Please select PO Type (Manufacturing / Labour).")]
        public bool PurchORSubCon { get; set; } = true;

        [Required(ErrorMessage = "Vendor is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Vendor.")]
        public int VendorCode { get; set; }

        public bool? IsServiceBill { get; set; } = false;
        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }
        public string? VendorPhoneNo { get; set; }
        public string? VendorGSTNO { get; set; }
        public string? CustContactNo { get; set; }
        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;
        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstin { get; set; }
        public string? ShippingContactNo { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Currency.")]
        public int? CurrId { get; set; }
        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Today's Value cannot be negative.")]
        public decimal TodayVal { get; set; }

        [StringLength(10, ErrorMessage = "Prefix cannot exceed 10 characters.")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "PO Number is required.")]
        [StringLength(50, ErrorMessage = "PO Number cannot exceed 50 characters.")]
        public string PONo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(50, ErrorMessage = "Suffix cannot exceed 50 characters.")]
        public string Suffix { get; set; } = string.Empty;

        public int? RevisionNo { get; set; }
        public bool IsRevision { get; set; } = false;
        public int? RefRevisonPoid { get; set; }

        [Required(ErrorMessage = "PO Date is required.")]
        public DateTime PODate { get; set; } = DateTime.Now;

        public DateTime PODateNow { get; set; } = DateTime.Now;

        public int NoOfItems => PurchPoSubVMs?.Count ?? 0;

        [StringLength(500, ErrorMessage = "Payment Terms cannot exceed 500 characters.")]
        public string? PayTerms { get; set; }

        [StringLength(500, ErrorMessage = "Delivery Terms cannot exceed 500 characters.")]
        public string? DeliveryTerms { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? MainRemark { get; set; }

        public bool PoShortClose { get; set; } = false;
        public bool isRejTrackReq { get; set; } = false;
        public bool IsOpenPO { get; set; } = false;
        public bool PoTally { get; set; } = false;
        public bool PoCancl { get; set; } = false;

        [StringLength(250, ErrorMessage = "Cancel Reason cannot exceed 250 characters.")]
        public string? CancelReason { get; set; }

        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }
        public bool RejAndRet { get; set; } = false;


        [Range(0, double.MaxValue, ErrorMessage = "Total Gross Amount cannot be negative.")]
        public decimal TotalGrossAmount { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; } = false;
        public bool DiscAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "Discount % must be between 0 and 100.")]
        public decimal DiscountPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount Amount cannot be negative.")]
        public decimal DiscountAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total Basic Amount cannot be negative.")]
        public decimal TotalBasicAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Freight Charges cannot be negative.")]
        public decimal FreightCharges { get; set; }

        public bool PackingAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "Packing % must be between 0 and 100.")]
        public decimal PackingPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Packing Charges cannot be negative.")]
        public decimal PackingCharges { get; set; }

        public bool InsuranceAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "Insurance % must be between 0 and 100.")]
        public decimal InsurancePercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Insurance Charges cannot be negative.")]
        public decimal InsuranceCharges { get; set; }

        [Range(0, 100, ErrorMessage = "CGST % must be between 0 and 100.")]
        public decimal CGstRate { get; set; }

        [Range(0, 100, ErrorMessage = "SGST % must be between 0 and 100.")]
        public decimal SGstRate { get; set; }

        [Range(0, 100, ErrorMessage = "IGST % must be between 0 and 100.")]
        public decimal IGstRate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total CGST Amount cannot be negative.")]
        public decimal TotalCGSTAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total SGST Amount cannot be negative.")]
        public decimal TotalSGSTAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total IGST Amount cannot be negative.")]
        public decimal TotalIGSTAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Other Charges cannot be negative.")]
        public decimal OtherCharges { get; set; }

        public bool TCSAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "TCS % must be between 0 and 100.")]
        public decimal TCSPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "TCS Amount cannot be negative.")]
        public decimal TCSAmount { get; set; }

        public bool AdvanceAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "Advance % must be between 0 and 100.")]
        public decimal AdvancePercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Advance Amount cannot be negative.")]
        public decimal AdvanceAmount { get; set; }

        public bool AdvacewithOutTax { get; set; } = true;
        public decimal AdvanceAmountBal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total Taxable Amount cannot be negative.")]
        public decimal TotalTaxable { get; set; }

        public bool IsRoundOffEnabled { get; set; } = true;
        public decimal RoundOff { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Grand Total cannot be negative.")]
        public decimal GrandTotal { get; set; }


        [StringLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool IsLevel1 { get; set; } = false;
        public bool IsLevel2 { get; set; } = false;
        public bool IsLevel3 { get; set; } = false;
        public bool IsLevel4 { get; set; } = false;

        public string? Level1Sign { get; set; }
        public string? Level2Sign { get; set; }
        public string? Level3Sign { get; set; }
        public string? Level4Sign { get; set; }

        public bool Authorized { get; set; } = false;
        public string? ApprovedBy { get; set; }
        public string? LastApproved { get; set; }
        public DateTime? ApprovedDate { get; set; }

        public bool IsRejected { get; set; } = false;
        public string? RejectReason { get; set; }

        public int? TermsId { get; set; }
        public string? Details { get; set; }
        public bool IsSelected { get; set; } = false;

        [MinLength(1, ErrorMessage = "At least one PO item must be added.")]
        [ValidateComplexType]
        public List<PurchPoSubVM> PurchPoSubVMs { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            foreach (var sub in PurchPoSubVMs)
            {
                string itemName = sub.ItemCode ?? sub.ItemName ?? "";

                if (!IsOpenPO && (sub.Qty ?? 0) <= 0)
                {
                    errors.Add(new ValidationResult(
                        $"Quantity must be greater than zero for item: {itemName}.",
                        new[] { nameof(sub.Qty) }));
                }

                if (sub.DueDate < PODate.Date)
                {
                    errors.Add(new ValidationResult(
                        $"Due Date cannot be earlier than PO Date for item: {itemName}.",
                        new[] { nameof(sub.DueDate) }));
                }
            }

            if (AdvanceAmount > GrandTotal)
            {
                errors.Add(new ValidationResult(
                    "Advance Amount cannot be greater than Grand Total.",
                    new[] { nameof(AdvanceAmount) }));
            }

            return errors;
        }

        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => PurchPoSubVMs;

        public bool HasItemWiseTax =>
            CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;

        public decimal TotalQty => PurchPoSubVMs?.Sum(x => x.Qty ?? 0m) ?? 0m;
        public decimal AvgUnitPrice => PurchPoSubVMs != null && PurchPoSubVMs.Any()
            ? PurchPoSubVMs.Average(x => x.UnitPrice ?? 0m)
            : 0m;

        public decimal TotalLineGross => PurchPoSubVMs?.Sum(x => x.LineGross) ?? 0m;
        public decimal TotalLineDiscountAmount => PurchPoSubVMs?.Sum(x => x.LineDiscountAmount ?? 0m) ?? 0m;
        public decimal TotalLineBasicAmount => PurchPoSubVMs?.Sum(x => x.LineBasicAmount) ?? 0m;
    }


}
