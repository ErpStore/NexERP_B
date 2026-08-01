using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.PurchAndSubConViewModel.Purch_QuotationVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM
{
    public class PurchaseInvoiceVM : ICalculationDocument
    {
        public int InvId { get; set; }
        [Required(ErrorMessage = "Vendor/Supplier selection is required.")]
        public int? VendorCode { get; set; }

        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }
        public string? VendorGstNo { get; set; }

       // [Phone(ErrorMessage = "Invalid contact number.")]
        public string? VendorContactNo { get; set; }

        public string? KindofAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstin { get; set; }
        public string? ShippingContactNo { get; set; }

        [Required(ErrorMessage = "Invoice Number is required.")]
        [StringLength(50, ErrorMessage = "Invoice No Length cannot be exceed 50 Characters")]
        public string InvNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Invoice Date is required.")]
        public DateTime InvDate { get; set; } = DateTime.Now;

        public DateTime InvDateNow { get; set; } = DateTime.Now;

        [MaxLength(250, ErrorMessage = "GRN Remark Length cannot be exceed 250 Characters")]
        public string? MainRemarks { get; set; }

        public int NoOfItems => PurchaseInvoiceSubVMs.Count;

       
        [Required(ErrorMessage = "Currency is required.")]
        public int CurrId { get; set; } = 1;

        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Today's value must be non-negative.")]
        public decimal TodayVal { get; set; }

        public bool InvTally { get; set; } = false;
        public bool IsAcptRejRewQtyRequired { get; set; } = false;

        public bool InvCancel { get; set; } = false;
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CanceledBy { get; set; }
        public bool ShortClose { get; set; }


        [RegularExpression(@"^[0-9]{12}$", ErrorMessage = "E-Way Bill Number must be exactly 12 digits.")]
        public string? EWayNo { get; set; }
        public DateTime? EWayDate { get; set; }

        // Calculation properties
        [Range(0, double.MaxValue, ErrorMessage = "Total Gross Amount must be non-negative.")]
        public decimal TotalGrossAmount { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; } = false;  // true for amount, false for percent
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

        public decimal Balance { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "TDS Amount  must be non-negative.")]
        public decimal TDSAmount { get; set; }

        public int? TermsId { get; set; }

        public int? BankId { get; set; }
        public string? BankName { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }






        [MinLength(1, ErrorMessage = "At least one Invoice item is required.")]
        [ValidateComplexType]
        public List<PurchaseInvoiceSubVM> PurchaseInvoiceSubVMs { get; set; } = new List<PurchaseInvoiceSubVM>();

        // ICalculationDocument implementation
        // Use this attribute to tell the validator to check the collection.
        [ValidateComplexType]
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => PurchaseInvoiceSubVMs;
        public bool HasItemWiseTax =>
            CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;


        public decimal TotalQty => PurchaseInvoiceSubVMs?.Sum(x => x.Qty) ?? 0;
        public decimal AvgUnitPrice => (PurchaseInvoiceSubVMs?.Any() == true ? PurchaseInvoiceSubVMs.Average(x => x.UnitPrice) : 0) ?? 0;
        public decimal TotalLineGross => PurchaseInvoiceSubVMs?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount =>
            Math.Round(PurchaseInvoiceSubVMs?.Sum(x => x.LineDiscountAmount ?? 0m) ?? 0m, 3);

        public decimal TotalLineBasicAmount => PurchaseInvoiceSubVMs?.Sum(x => x.LineBasicAmount) ?? 0;

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
            

            // Validate nested items with proper member names
            if (PurchaseInvoiceSubVMs != null)
            {
                for (int i = 0; i < PurchaseInvoiceSubVMs.Count; i++)
                {
                    var sub = PurchaseInvoiceSubVMs[i];
                    var results = new List<ValidationResult>();
                    var ctx = new ValidationContext(sub, serviceProvider: null, items: null);

                    Validator.TryValidateObject(sub, ctx, results, validateAllProperties: true);
                    results.AddRange(sub.Validate(ctx)); // also run IValidatableObject

                    foreach (var r in results)
                    {
                        var memberNames = r.MemberNames != null && r.MemberNames.Any()
                            ? r.MemberNames.Select(m => $"{nameof(PurchaseInvoiceSubVMs)}[{i}].{m}")
                            : new[] { $"{nameof(PurchaseInvoiceSubVMs)}[{i}]" };

                        yield return new ValidationResult(r.ErrorMessage, memberNames);
                    }
                }
            }
        }

    }
}
