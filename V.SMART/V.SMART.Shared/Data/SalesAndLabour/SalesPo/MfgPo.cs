using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesPo
{
    public class MfgPo
    {
        [Key]
        public int PoId { get; set; }

        [Required]
        public bool MfgORLab { get; set; } = false;
        public bool? IsServiceBill { get; set; } = false;


        [Required]
        public int? CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }

        public string? KindOfAttention { get; set; }
        public bool IsSameAsShipping { get; set; } = true;
        public int? ShipAddrsId { get; set; }

        [ForeignKey(nameof(ShipAddrsId))]
        public virtual CustomerIndirect? Consinee { get; set; }


        public int? CurrId { get; set; }
        [ForeignKey(nameof(CurrId))]
        public virtual Currency Currency { get; set; }

        [Precision(18,4)]
        [Range(0, double.MaxValue)]
        public decimal TodayVal { get; set; }

        public int? PoTypeId { get; set; }
        [ForeignKey(nameof(PoTypeId))]
        public virtual PoType? PoType { get; set; }

        [StringLength(50)]
        public string? SaleOrderNo { get; set; }

        [Required, StringLength(50)]
        public string PONo { get; set; }

        [Required]
        public string Suffix { get; set; }

        [Required]
        public DateTime? PODate { get; set; }
        public DateTime PODateNow { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue)]
        public int NoOfItems { get; set; }

        [StringLength(500)]
        public string? PayTerms { get; set; }

        [StringLength(500)]
        public string? DeliveryTerms { get; set; }

        [StringLength(500)]
        public string? MainRemark { get; set; }


        public bool isRejTrackReq { get; set; } = false;
        public bool IsOpenPO { get; set; } = false;
        public bool PoTally { get; set; } = false;

        public bool PoCancl { get; set; } = false;
        public DateTime? CancelDate { get; set; }
        public string? CancelReason { get; set; }
        public string? CancelBy { get; set; }


        public bool ShortClose { get; set; } = false;
        public bool RejAndRet { get; set; } = false;


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


        //Order Acceptance Details
        [StringLength(50)]
        public string? OANo { get; set; }
        public DateTime? OADate { get; set; } = DateTime.Now;
        public string? OAWONO { get; set; }
        public DateTime? OAWODate { get; set; } = DateTime.Now;
        public string? OARemarks { get; set; }
        public string? OATerms { get; set; }


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


        // Audit Fields
        [Required]
        [StringLength(50)]
        public string? CreatedBy { get; set; }

        [Required]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        public virtual ICollection<MfgPoSub> MfgPoSubs { get; set; } = new List<MfgPoSub>();
        public virtual ICollection<AssemblyPoComponentTracker> AssemblyPoComponentTrackers { get; set; } = new List<AssemblyPoComponentTracker>();
    }

}
