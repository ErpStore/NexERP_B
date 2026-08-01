using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.PurchasePo
{
    public class PurchPo
    {
        [Key]
        public int PoId { get; set; }

        [Required]
        public bool PurchORSubCon { get; set; } = true;  // true - Purchase 

        public bool? IsServiceBill { get; set; } = false;

        [Required]
        public int? VendorCode { get; set; }

        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? Vendor { get; set; }

        public string? KindOfAttention { get; set; }
        public bool IsSameAsShipping { get; set; } = true;
        public int? ShipAddrsId { get; set; }

        [ForeignKey(nameof(ShipAddrsId))]
        public virtual VendorInDirect? Consinee { get; set; }

        public int? CurrId { get; set; }
        [ForeignKey(nameof(CurrId))]
        public virtual Currency Currency { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal TodayVal { get; set; }


        [StringLength(10)]
        public string? Prefix { get; set; }

        [Required, StringLength(50)]
        public string PONo { get; set; }

        public int? RevisionNo { get; set; }
        public bool IsRevision { get; set; } = false;
        public int? RefRevisonPoid { get; set; }

        [Required]
        [StringLength(10)]
        public string Suffix { get; set; }

        [Required]
        public DateTime PODate { get; set; }  = DateTime.Now;
        public DateTime PODateNow { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue)]
        public int NoOfItems { get; set; }

        [StringLength(500)]
        public string? PayTerms { get; set; }

        [StringLength(500)]
        public string? DeliveryTerms { get; set; }

        [StringLength(500)]
        public string? MainRemark { get; set; }

        public int? TermsId { get; set; }

        [ForeignKey(nameof(TermsId))]
        public TermsAndConditions TermsAndConditions { get; set; }

        public bool isRejTrackReq { get; set; } = false;
        public bool IsOpenPO { get; set; } = false;
        public bool PoTally { get; set; } = false;
        public bool PoCancl { get; set; } = false;

        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }


        public bool RejAndRet { get; set; } = false;
        public bool PoShortClose { get; set; } = false;

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

        public bool AdvacewithOutTax { get; set; } = true;
        public bool AdvanceAmtOrPer { get; set; } = true;
        public decimal AdvancePercent { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal AdvanceAmountBal { get; set; }

        // Audit Fields
        [Required]
        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [Required]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Approval Fields
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
        public DateTime? ApprovalDate { get; set; }
        public bool IsRejected { get; set; } = false;
        public string? RejectReason { get; set; }
        public virtual ICollection<PurchPoSub> PurchPoSubs { get; set; } = new List<PurchPoSub>();
       

    }
}
