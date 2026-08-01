using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.Credit_Note
{
    public class CreditNote
    {
        [Key]
        public int? CrId { get; set; }

        [StringLength(12)]
        public string? Prefix { get; set; }

        [Required]
        [StringLength(15)]
        public string? CreditNo { get; set; }

        [Required]
        [StringLength(12)]
        public string? Suffix { get; set; }

        [Required]
        public DateTime CreditDate { get; set; } = DateTime.Now;
        public DateTime CreditDateNow { get; set; } = DateTime.Now;

        public int CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public virtual Customer Customer { get; set; } = null!;

        [StringLength(80)]
        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }

        [ForeignKey(nameof(ShipAddrsId))]
        public CustomerIndirect? Consinee { get; set; }

        public int? NoOfItems { get; set; }

        [StringLength(250)]
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Please Select the Currency!..")]
        public int? CurrId { get; set; } = 1;

        [ForeignKey(nameof(CurrId))]
        public Currency Currency { get; set; }

        public decimal TodayVal { get; set; }
       
        public bool? IsManualInvNo { get; set; } = false;

        public bool? Rejection { get; set; } = false;

        public bool? RateDifference { get; set; } = false;

        public bool? Sales { get; set; } = false;

        public bool? Labour { get; set; } = false;

        public bool? Export { get; set; } = false;


        //For E-Invoice
        [StringLength(53)]
        public string? ACKNO { get; set; }

        [StringLength(160)]
        public string? IRNNo { get; set; }

        public string? SignedInvoiceQrCode { get; set; }

        public DateTime? ACKNODate { get; set; }

        [StringLength(5)]
        public string? InvType { get; set; }
        //--------------------------------------------

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

        public decimal GrandTotal { get; set; }

        [Precision(18, 4)]
        public decimal Balance { get; set; }

        [Precision(18, 4)]
        public decimal RoundOff { get; set; }
        public bool IsRoundOffEnabled { get; set; } = true;

        public bool Cancel { get; set; } = false;
        [StringLength(120, ErrorMessage = "Cancel Reason cannot exceed 120 characters.")]
        public string? CancelReason { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<CreditNoteSub> CreditNoteSubs { get; set; } = new List<CreditNoteSub>();

    }
}
