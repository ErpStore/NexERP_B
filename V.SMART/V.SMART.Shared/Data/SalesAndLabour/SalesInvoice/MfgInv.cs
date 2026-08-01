using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesInvoice
{
    public class MfgInv
    {
        [Key]
        public int InvId { get; set; }

        [StringLength(12)]
        public string? Prefix { get; set; }

        [Required]
        [StringLength(15)]
        public string InvNo { get; set; }

        [Required]
        [StringLength(12)]
        public string Suffix { get; set; }

        [Required]
        public DateTime InvDate { get; set; } = DateTime.Now;
        public DateTime InvDateNow { get; set; } = DateTime.Now;

        [Required]
        public int? CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }
        [StringLength(80)]
        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }

        [ForeignKey(nameof(ShipAddrsId))]
        public CustomerIndirect? Consinee { get; set; }

        public int? NoOfItems { get; set; }

        [StringLength(50)]
        public string? DeliveryTems { get; set; }

        [StringLength(50)]
        public string? PayTerms { get; set; }

        [StringLength(250)]
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Please Select the Currency!..")]
        public int CurrId { get; set; } = 1;

        [ForeignKey(nameof(CurrId))]
        public Currency Currency { get; set; }

        public decimal TodayVal { get; set; }
        public bool IsCancel { get; set; } = false;

        [StringLength(120)]
        public string? CancelReason { get; set; }

        public bool InvTally { get; set; } = false;

        
        public int? StoreIssId { get; set; }
        [ForeignKey(nameof(StoreIssId))]
        public virtual Store? StoreIssue { get; set; }

        public bool DcCumInv { get; set; } = false;

        [StringLength(15)]
        public string? EWayNo { get; set; }

        public DateTime? EWayDate { get; set; }

        [StringLength(13, ErrorMessage = "Invalid vehicle number length")]
        [RegularExpression(@"^$|^[A-Z]{2}[ -]?\d{1,2}[ -]?[A-Z]{1,2}[ -]?\d{4}$", ErrorMessage = "Invalid vehicle number format (e.g., KA01AB1234)")]
        public string? VehicleNo { get; set; }

        public int? TransportMode { get; set; }

        [StringLength(80, ErrorMessage = "From location cannot exceed 80 characters.")]
        public string? TransFrom { get; set; }

        [StringLength(80, ErrorMessage = "To location cannot exceed 80 characters.")]
        public string? TransTo { get; set; }


        public int? SubSupplyType { get; set; }

        [StringLength(200, ErrorMessage = "SubSupplyDesc cannot exceed 200 characters.")]
        public string? SubSupplyDesc { get; set; }

        [StringLength(20)]
        public string? TransId { get; set; }

        [StringLength(100)]
        public string? TransName { get; set; }

        [StringLength(15)]
        public string? TransDocNo { get; set; }

        [StringLength(10)]
        public string? DriverNo { get; set; }

        [StringLength(20)]
        public string? LcNo { get; set; }

        public DateTime? LcDate { get; set; }

        public DateTime? LcExpiryDate { get; set; }

        public bool PoShortClose { get; set; } = false;

        public bool? IsManualInvNo { get; set; } = false;


        //For E-Invoice
        [StringLength(53)]
        public string? ACKNO { get; set; }

        [StringLength(160)]
        public string? IRNNo { get; set; }

        public string? SignedInvoiceQrCode { get; set; }

        public DateTime? ACKNODate { get; set; }

        [StringLength(5)]
        public string? InvType { get; set; }

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

        [Precision(18, 4)]
        public decimal Balance { get; set; }

        // -------- TDS --------
        [Precision(18, 4)]
        public decimal TDSAmount { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<MfgInvSub> MfgInvSubs { get; set; } = new List<MfgInvSub>();



    }
}
