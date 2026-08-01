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
    public class MfgQuoteVM : ICalculationDocument
    {
        public int QuoteId { get; set; }

        [Required(ErrorMessage = "Please select whether it is Manufacturing or Labour.")]
        public bool MfgOrLab { get; set; } = true;

        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Quote number is required.")]
        public string QuoteNo { get; set; }

        [Required(ErrorMessage ="Suffix is Required")]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Quote date is required.")]
        public DateTime QuoteDate { get; set; } = DateTime.Now;
        public DateTime QuoteDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; } = DateTime.Now;

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
        public bool QuotationTally { get; set; } = false; 
        public int? ShipAddrsId { get; set; }
        public string? ShippingName { get; set; }
        public string? ShippingAddress { get; set; }
        public string? ShippingGstin { get; set; }
        public string? ShippingContactNo { get; set; }

        public int NoOfItems => MfgQuoteSubVM?.Count ?? 0;
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Currency is required.")]
        public int CurrId { get; set; } = 1;

        public string? CurrName { get; set; }
        public string? Symbol { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Today's value must be non-negative.")]
        public decimal TodayVal { get; set; }
        public bool IsCancel { get; set; } = false;


        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }
        public bool ShortClose { get; set; } = false;

        [StringLength(300, ErrorMessage = "Payment Terms cannot exceed 300 characters.")]
        public string? PayTerms { get; set; }

        [StringLength(300, ErrorMessage = "Delivery Terms cannot exceed 300 characters.")]
        public string? DeliveryTems { get; set; }

        public int? RevisionQuoteNo { get; set; }
        public bool IsRevisionNo { get; set; } = false;
        public int? RefRevisionQuoteId { get; set; }

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

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }



        [MinLength(1, ErrorMessage = "At least one quote item is required.")]
        [ValidateComplexType]
        public List<MfgQuoteSubVM> MfgQuoteSubVM { get; set; } = new();

        // ICalculationDocument implementation
        // Use this attribute to tell the validator to check the collection.
        public IEnumerable<ICalculationDocumentSubItem> CalculationDocumentSubItem => MfgQuoteSubVM;
        public bool HasItemWiseTax =>
            CalculationDocumentSubItem?.Any(i => i.LineCGSTRate > 0 || i.LineSGSTRate > 0 || i.LineIGSTRate > 0) == true;


        public decimal TotalQty => MfgQuoteSubVM?.Sum(x => x.Qty) ?? 0;
        public decimal AvgUnitPrice => (MfgQuoteSubVM?.Any() == true ? MfgQuoteSubVM.Average(x => x.UnitPrice) : 0) ?? 0;
        public decimal TotalLineGross => MfgQuoteSubVM?.Sum(x => x.LineGross) ?? 0;
        public decimal TotalLineDiscountAmount => MfgQuoteSubVM?.Sum(x => x.LineDiscountAmount) ?? 0;
        public decimal TotalLineBasicAmount => MfgQuoteSubVM?.Sum(x => x.LineBasicAmount) ?? 0;

    }


}
