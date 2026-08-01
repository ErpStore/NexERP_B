using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM
{
    public class MfgPoVM : ICalculationDocument, IValidatableObject
    {
        [Key]
        public int PoId { get; set; }

        [Required(ErrorMessage = "Please select Manufacturing or Labour.")]
        public bool MfgORLab { get; set; } = true;

        public bool? IsServiceBill { get; set; } = false;

        [Required(ErrorMessage = "Customer is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Customer.")]
        public int CustId { get; set; }

         public bool IsSelected { get; set; } = false;

        // 🔹 Customer
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


        // 🔹 Currency
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Currency.")]
        public int? CurrId { get; set; }
        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Today’s Value must be non-negative.")]
        public decimal TodayVal { get; set; }

        // 🔹 PO Type
        public int? PoTypeId { get; set; }
        public string PoTypename { get; set; } = string.Empty;

        // 🔹 PO Details
        [StringLength(50, ErrorMessage = "Sale Order No cannot exceed 50 characters.")]
        public string? SaleOrderNo { get; set; }

        [Required(ErrorMessage = "PO Number is required.")]
        [StringLength(50, ErrorMessage = "PO Number cannot exceed 50 characters.")]
        public string PONo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(50, ErrorMessage = "Suffix cannot exceed 50 characters.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "PO Date is required.")]
        public DateTime PODate { get; set; } = DateTime.Now;

        public DateTime PODateNow { get; set; } = DateTime.Now;

        public int NoOfItems => MfgPOSubVMs?.Count ?? 0;

        [StringLength(500, ErrorMessage = "Payment Terms cannot exceed 500 characters.")]
        public string? PayTerms { get; set; }

        [StringLength(500, ErrorMessage = "Delivery Terms cannot exceed 500 characters.")]
        public string? DeliveryTerms { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string? MainRemark { get; set; }

        // 🔹 PO Status
        public bool isRejTrackReq { get; set; } = false;
        public bool IsOpenPO { get; set; } = false;
        public bool PoTally { get; set; } = false;

        public bool PoCancl { get; set; } = false;
        public DateTime? CancelDate { get; set; }
        public string? CancelReason { get; set; }
        public string? CancelBy { get; set; }

        public bool ShortClose { get; set; } = false;
        public bool RejAndRet { get; set; } = false;

        // 🔹 Calculation
        [Range(0, double.MaxValue, ErrorMessage = "Total Gross Amount must be non-negative.")]
        public decimal TotalGrossAmount { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; } = false;
        public bool DiscAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100.")]
        public decimal DiscountPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be non-negative.")]
        public decimal DiscountAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total Basic Amount must be non-negative.")]
        public decimal TotalBasicAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Freight charges must be non-negative.")]
        public decimal FreightCharges { get; set; }

        public bool PackingAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "Packing percent must be between 0 and 100.")]
        public decimal PackingPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Packing charges must be non-negative.")]
        public decimal PackingCharges { get; set; }

        public bool InsuranceAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "Insurance percent must be between 0 and 100.")]
        public decimal InsurancePercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Insurance charges must be non-negative.")]
        public decimal InsuranceCharges { get; set; }

        [Range(0, 100, ErrorMessage = "CGST rate must be between 0 and 100.")]
        public decimal CGstRate { get; set; }

        [Range(0, 100, ErrorMessage = "SGST rate must be between 0 and 100.")]
        public decimal SGstRate { get; set; }

        [Range(0, 100, ErrorMessage = "IGST rate must be between 0 and 100.")]
        public decimal IGstRate { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total CGST amount must be non-negative.")]
        public decimal TotalCGSTAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total SGST amount must be non-negative.")]
        public decimal TotalSGSTAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total IGST amount must be non-negative.")]
        public decimal TotalIGSTAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Other charges must be non-negative.")]
        public decimal OtherCharges { get; set; }

        public bool TCSAmtOrPer { get; set; } = true;

        [Range(0, 100, ErrorMessage = "TCS percent must be between 0 and 100.")]
        public decimal TCSPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "TCS amount must be non-negative.")]
        public decimal TCSAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total taxable amount must be non-negative.")]
        public decimal TotalTaxable { get; set; }

        public bool IsRoundOffEnabled { get; set; } = true;
        public decimal RoundOff { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Grand total must be non-negative.")]
        public decimal GrandTotal { get; set; }

        // 🔹 Order Acceptance

        public string? OANo { get; set; }

        public DateTime OADate { get; set; } = DateTime.Now;

        public string? OAWONO { get; set; }
        public DateTime OAWODate { get; set; } = DateTime.Now;

        [StringLength(500, ErrorMessage = "Order Acceptance Remarks cannot exceed 500 characters.")]
        public string? OARemarks { get; set; }

        public string? OATerms { get; set; }

        //New Fields 
        public bool IsLevel1 { get; set; } = false;
        public bool IsLevel2 { get; set; } = false;
        public bool IsLevel3 { get; set; } = false;
        public bool IsAuthorized { get; set; } = false;

        [StringLength(100)]
        public string? Level1Sign { get; set; }
        [StringLength(100)]
        public string? Level2Sign { get; set; }
        [StringLength(100)]
        public string? Level3Sign { get; set; }
        public bool IsRejected { get; set; } = false;
        [StringLength(250)]
        public string? RejectReason { get; set; }

        [StringLength(100)]
        public string? LastApproved { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovalDate { get; set; }


        // 🔹 Audit
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // 🔹 Sub Items
        [MinLength(1, ErrorMessage = "At least one PO item is required.")]
        [ValidateComplexType]
        public List<MfgPoSubVM> MfgPOSubVMs { get; set; } = new();
        public List<AssemblyPoComponentTracker> AssemblyPoComponentTrackerVMs { get; set; } = new();



        // ================= VALIDATION LOGIC =================
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if(IsOpenPO)
            {
                var duplicateItems = MfgPOSubVMs
                .GroupBy(x => x.ItemId)
                .Where(g => g.Key != null && g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

                if (duplicateItems.Any())
                {
                    errors.Add(new ValidationResult(
                        $"Duplicate items are not allowed. Duplicates found for Item IDs: {string.Join(", ", duplicateItems)}.",
                        new[] { nameof(MfgPOSubVMs) }));
                }
            }

            foreach (var sub in MfgPOSubVMs)
            {
                if (!IsOpenPO && (sub.Qty ?? 0) <= 0)
                {
                    errors.Add(new ValidationResult(
                        $"Qty must be greater than zero for item '{sub.ItemCode ?? sub.ItemName ?? ""}'.",
                        new[] { nameof(sub.Qty) }));
                }

                if ((sub.UnitPrice ?? 0) <= 0)
                {
                    errors.Add(new ValidationResult(
                        $"Rate is required and must be greater than zero for item '{sub.ItemName ?? sub.ItemCode ?? "Unknown"}'.",
                        new[] { nameof(sub.UnitPrice) }));
                }
            }

            return errors;
        }

        // 🔹 CalculationDocument interface
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => MfgPOSubVMs;
        public bool HasItemWiseTax => CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;

        // Computed totals
        public decimal TotalQty => MfgPOSubVMs?.Sum(x => x.Qty) ?? 0;
        public decimal AvgUnitPrice => MfgPOSubVMs != null && MfgPOSubVMs.Any() ? MfgPOSubVMs.Average(x => x.UnitPrice ?? 0m) : 0;
        public decimal TotalLineGross => MfgPOSubVMs?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount => MfgPOSubVMs?.Sum(x => x.LineDiscountAmount ?? 0) ?? 0;
        public decimal TotalLineBasicAmount => MfgPOSubVMs?.Sum(x => x.LineBasicAmount) ?? 0m;
    }

}
