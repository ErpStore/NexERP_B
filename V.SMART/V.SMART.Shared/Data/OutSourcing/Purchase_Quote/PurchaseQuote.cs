using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.Icons;

namespace V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote
{
    public  class PurchaseQuote
    {
        [Key]
        public int QuoteId { get; set; }

        [Required(ErrorMessage = "Please select Either Purchase Or Subcontract")]
        public bool PurchOrSub { get; set; } = true;

        [Required(ErrorMessage = "Quotation Number is Required!..")]
        public string QuoteNo { get; set; }

        [Required]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Quotation Date is Required!..")]
        public DateTime QuoteDate { get; set; } = DateTime.Now;

        public DateTime QuoteDateNow { get; set; } = DateTime.Now;


        [Required(ErrorMessage = "Please Select the Vendor/Supplier Name!..")]
        public int? VendorCode { get; set; }

        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? Vendor { get; set; }

        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true; 
        
        public int? ShipAddrsId { get; set; }

        [ForeignKey(nameof(ShipAddrsId))]
        public VendorInDirect? Consignee { get; set; }

        public int? NoOfItems { get; set; }              
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Please Select the Currency!..")]
        public int CurrId { get; set; } = 1;

        [ForeignKey(nameof(CurrId))]
        public Currency Currency { get; set; }

        public decimal TodayVal { get; set; }      
        
        public int? TermsId { get; set; }

        [ForeignKey(nameof(TermsId))]
        public TermsAndConditions TermsAndConditions { get; set; }

        public bool QuoteShortClose { get; set; } = false;
        public bool QuotationTally { get; set; } = false;

        public bool IsCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; } 
        public string? CancelReason { get; set; }
        public string? CancelledBy { get; set; }


        // Calculation fields
        [Precision(18, 4)]
        public decimal TotalGrossAmount { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; } = false;

        public bool DiscAmtOrPer { get; set; } = true;

        [Precision(18, 4)]
        public decimal DiscountPercent { get; set; }

        [Precision(18, 4)]
        public decimal DiscountAmount { get; set; }

        [Precision(18, 4)]
        public decimal TotalBasicAmount { get; set; }

        [Precision(18, 4)]
        public decimal FreightCharges { get; set; }

        public bool PackingAmtOrPer { get; set; } = true;

        [Precision(18, 4)]
        public decimal PackingPercent { get; set; }

        [Precision(18, 4)]
        public decimal PackingCharges { get; set; }

        public bool InsuranceAmtOrPer { get; set; } = true;

        [Precision(18, 4)]
        public decimal InsurancePercent { get; set; }

        [Precision(18, 4)]
        public decimal InsuranceCharges { get; set; }

        [Precision(18, 3)]
        public decimal CGstRate { get; set; }

        [Precision(18, 3)]
        public decimal SGstRate { get; set; }

        [Precision(18, 3)]
        public decimal IGstRate { get; set; }

        [Precision(18, 4)]
        public decimal TotalCGSTAmount { get; set; }

        [Precision(18, 4)]
        public decimal TotalSGSTAmount { get; set; }

        [Precision(18, 4)]
        public decimal TotalIGSTAmount { get; set; }

        [Precision(18, 4)]
        public decimal OtherCharges { get; set; }

        public bool TCSAmtOrPer { get; set; } = true;

        [Precision(18, 4)]
        public decimal TCSPercent { get; set; }

        [Precision(18, 4)]
        public decimal TCSAmount { get; set; }

        [Precision(18, 4)]
        public decimal TotalTaxable { get; set; }
        public bool IsRoundOffEnabled { get; set; } = true;

        [Precision(18, 4)]
        public decimal RoundOff { get; set; }

        public decimal GrandTotal { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<PurchaseQuoteSub> PurchaseQuoteSub { get; set; } = new List<PurchaseQuoteSub>();
    }
}
