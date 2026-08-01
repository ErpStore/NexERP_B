using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Utility_Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation
{
    public class MfgQuote
    {
        [Key]
        public int QuoteId { get; set; }

        [Required(ErrorMessage = "Please select Either Manufacturing Or Labour-Work")]
        public bool MfgOrLab { get; set; } = true;

        [StringLength(10, ErrorMessage = "Prefix cannot exceed 10 characters")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Quotation Number is Required!..")]
        public string QuoteNo { get; set; }

        [Required]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Quotation Date is Required!..")]
        public DateTime QuoteDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; } = DateTime.Now;
        public DateTime QuoteDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please Select the customer Name!..")]
        public int? CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public Customer Customer { get; set; }

        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }

        [ForeignKey(nameof(ShipAddrsId))]
        public CustomerIndirect? Consinee { get; set; }

        public int? NoOfItems { get; set; }
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Please Select the Currency!..")]
        public int CurrId { get; set; } = 1;

        [ForeignKey(nameof(CurrId))]
        public Currency Currency { get; set; }

        public bool QuotationTally { get; set; } = false; 
        public decimal TodayVal { get; set; }
        public bool IsCancel { get; set; } = false;
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }

        public bool ShortClose { get; set; } = false;


        public string? PayTerms { get; set; }
        public string? DeliveryTems { get; set; }

        public int? RevisionQuoteNo { get; set; }
        public bool IsRevisionNo { get; set; } = false;
        public int? RefRevisionQuoteId { get; set; }


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

        public int? TermsId { get; set; }

        [ForeignKey(nameof(TermsId))]
        public TermsAndConditions TermsAndConditions { get; set; }

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

        public virtual ICollection<MfgQuoteSub> MfgQuoteSub { get; set; } = new List<MfgQuoteSub>();

    }


}
