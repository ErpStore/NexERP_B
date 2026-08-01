using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.SubContractInvoice
{
    public class SubConInv
    {

        [Key]
        public int InvId { get; set; }

        [Required(ErrorMessage = "Invoice Number is required?,Invoice No. should not be empty")]
        [MaxLength(20)]
        public string InvNo { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Invoice Date is required.")]
        public DateTime InvDate { get; set; } = DateTime.Now;
        public DateTime InvDateNow { get; set; } = DateTime.Now;
        // datetime

        public int? VendorCode { get; set; }
        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? Vendor { get; set; }


        [MaxLength(50)]
        public string? KindOfAttention { get; set; }

        public bool IsSameAsShipping { get; set; }

        public int? ShipAddrsId { get; set; }
        [ForeignKey(nameof(ShipAddrsId))]
        public virtual VendorInDirect? Consinee { get; set; }

        public int NoOfItems { get; set; }

        [MaxLength(500)]
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Please Select the Currency!..")]
        public int? CurrId { get; set; } = 1;

        [ForeignKey(nameof(CurrId))]
        public virtual Currency? Currency { get; set; }

        public bool InvTally { get; set; } = false;

        public bool IsAcptRejRewQtyRequired { get; set; } = false;

        public decimal TodayVal { get; set; }

        public bool InvCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }

        public string? EWayNo { get; set; }
        public DateTime? EWayDate { get; set; }

        [Precision(18, 4)]
        public decimal TotalGrossAmount { get; set; }

        // -------- Discount --------
        public bool IsItemWiseDiscAmtOrPerReq { get; set; } = false;
        public bool? DiscAmtOrPer { get; set; } = true;
        [Precision(18, 4)]
        public decimal DiscountPercent { get; set; }
        [Precision(18, 4)]
        public decimal DiscountAmount { get; set; }


        //Basic
        [Precision(18, 4)]
        public decimal TotalBasicAmount { get; set; }


        // -------- F Charges --------
        [Precision(18, 4)]
        public decimal FreightCharges { get; set; }

        //Packing
        public bool PackingAmtOrPer { get; set; } = true;
        [Precision(18, 4)]
        public decimal PackingPercent { get; set; }
        [Precision(18, 4)]
        public decimal PackingCharges { get; set; }

        //Insurance
        public bool InsuranceAmtOrPer { get; set; } = true;
        [Precision(18, 4)]
        public decimal InsurancePercent { get; set; }
        [Precision(18, 4)]
        public decimal InsuranceCharges { get; set; }


        // -------- GST --------
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

        // -------- Other Charges --------
        [Precision(18, 4)]
        public decimal OtherCharges { get; set; }

        // -------- TCS --------
        [MaxLength(10)]
        public bool TCSAmtOrPer { get; set; } = true;

        [Precision(18, 4)]
        public decimal TCSPercent { get; set; }

        [Precision(18, 4)]
        public decimal TCSAmount { get; set; }

        [Precision(18, 4)]
        public decimal TotalTaxable { get; set; }

        // -------- Round off --------
        public bool IsRoundOffEnabled { get; set; } = true;

        [Precision(18, 4)]
        public decimal RoundOff { get; set; }

        [Precision(18, 4)]
        public decimal GrandTotal { get; set; }

        [Precision(18, 4)]
        public decimal Balance { get; set; }

        // -------- TDS --------
        [Precision(18, 4)]
        public decimal TDSAmount { get; set; }

        // -------- Terms & Audit --------
        public int? TermsId { get; set; }

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [MaxLength(200)]
        public string? CancelReason { get; set; }

        public virtual ICollection<SubConInvSub> SubConInvSubs { get; set; } = new List<SubConInvSub>();
    }
}
