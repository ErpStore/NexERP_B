using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PurchAndSubConViewModel.Purch_QuotationVM
{
    public class PurchaseQuoteVM: ICalculationDocument
    {
        public int QuoteId { get; set; }

        [Required(ErrorMessage = "Please select whether it is Purchase or Subcontract.")]
        public bool PurchOrSub { get; set; } = true;

        [Required(ErrorMessage = "Quote number is required.")]
        public string QuoteNo { get; set; }

        [Required(ErrorMessage = "Suffix is Required")]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Quote date is required.")]
        public DateTime QuoteDate { get; set; } = DateTime.Now;
        public DateTime QuoteDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vendor/Supplier selection is required.")]
        public int VendorCode { get; set; }

        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }

        [Phone(ErrorMessage = "Invalid contact number.")]
        public string? ContactNo { get; set; }

        public string? GSTNo { get; set; }
        public string? PANNo { get; set; }
        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;
        public bool QuotationTally { get; set; } = false;

        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstin { get; set; }
        public string? ShippingContactNo { get; set; }

        public int NoOfItems =>PurchaseQuoteSubVM?.Count ?? 0;

        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        public int CurrId { get; set; } = 1;

        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Today's value must be non-negative.")]
        public decimal TodayVal { get; set; }

        public bool QuoteShortClose { get; set; } = false;
        public bool IsCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }

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

        public int? TermsId { get; set; }
        public string? Details { get; set; }

        public string CancelReason { get; set; }
        public string? CancelledBy { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }=DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ValidateComplexType]
        [MinLength(1, ErrorMessage = "At least one quote item is required.")]
        public List<PurchaseQuoteSubVM> PurchaseQuoteSubVM { get; set; } = new();


        // ICalculationDocument implementation
        // Use this attribute to tell the validator to check the collection.

        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => PurchaseQuoteSubVM;
        public bool HasItemWiseTax =>
            CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;


        public decimal TotalQty => PurchaseQuoteSubVM?.Sum(x => x.Qty) ?? 0;
        public decimal AvgUnitPrice => (PurchaseQuoteSubVM?.Any() == true ? PurchaseQuoteSubVM.Average(x => x.UnitPrice) : 0) ?? 0;
        public decimal TotalLineGross => PurchaseQuoteSubVM?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount => PurchaseQuoteSubVM?.Sum(x => x.LineDiscountAmount) ?? 0;
        public decimal TotalLineBasicAmount => PurchaseQuoteSubVM?.Sum(x => x.LineBasicAmount) ?? 0;
    }
}
