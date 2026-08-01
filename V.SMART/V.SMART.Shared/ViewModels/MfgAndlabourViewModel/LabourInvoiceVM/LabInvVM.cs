using DocumentFormat.OpenXml.Presentation;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM
{
    public class LabInvVM : ICalculationDocument
    {
        public int LabInvId { get; set; }

        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Quote number is required.")]
        public string LabInvNo { get; set; }

        [Required(ErrorMessage ="Suffix is Required")]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Labour Invoice Date is Required!..")]
        public DateTime LabInvDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; } = DateTime.Now;
        public DateTime LabInvDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Customer selection is required.")]
        public int CustId { get; set; }

        public string? CustName { get; set; }
        public string? CustAddress { get; set; }

        [Phone(ErrorMessage = "Invalid contact number.")]
        public string? ContactNo { get; set; }

        public string? GSTNo { get; set; }
        public string? PANNo { get; set; }
        public string? KindOfAttention { get; set; }
        public bool IsSameAsShipping { get; set; } = true;
        public bool LabInvTally { get; set; } = false; 
        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstin { get; set; }
        public string? ShippingContactNo { get; set; }

        public int NoOfItems => LabourInvoiceSubVM?.Count ?? 0;
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        public int CurrId { get; set; } = 1;

        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Today's value must be non-negative.")]
        public decimal TodayVal { get; set; }

        public bool InvCancel { get; set; } = false;
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CanceledBy { get; set; }
   
        public bool ShortClose { get; set; } = false;

        [StringLength(300, ErrorMessage = "Payment Terms cannot exceed 300 characters.")]
        public string? PayTerms { get; set; }

        [StringLength(300, ErrorMessage = "Delivery Terms cannot exceed 300 characters.")]
        public string? DeliveryTems { get; set; }

        // Calculation properties
        [Range(0, double.MaxValue, ErrorMessage = "Total Gross Amount must be non-negative.")]
        public decimal TotalGrossAmount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "TDS Amount  must be non-negative.")]
        public decimal TDSAmount { get; set; }
        public decimal Balance { get; set; }
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

        //For E-Invoice
        public bool? IsManualInvNo { get; set; } = false;

        public string? ACKNO { get; set; }
        public string? IRNNo { get; set; }
        public string? SignedInvoiceQrCode { get; set; }
        public DateTime? ACKNODate { get; set; }
        public string? InvType { get; set; } = "INV";
        //--------------------------------------------
        public int? ProcessId { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        [MinLength(1, ErrorMessage = "At least one quote item is required.")]
        [ValidateComplexType]
        public List<LabInvSubVM> LabourInvoiceSubVM { get; set; } = new();

        // ICalculationDocument implementation
        // Use this attribute to tell the validator to check the collection.
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => LabourInvoiceSubVM;
        public bool HasItemWiseTax =>
            CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;


        public decimal TotalQty => LabourInvoiceSubVM?.Sum(x => x.Qty) ?? 0;
        public decimal AvgUnitPrice => (LabourInvoiceSubVM?.Any() == true ? LabourInvoiceSubVM.Average(x => x.UnitPrice) : 0) ?? 0;
        public decimal TotalLineGross => LabourInvoiceSubVM?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount => LabourInvoiceSubVM?.Sum(x => x.LineDiscountAmount) ?? 0;
        public decimal TotalLineBasicAmount => LabourInvoiceSubVM?.Sum(x => x.LineBasicAmount) ?? 0;

    }


}
