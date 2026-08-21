using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.General;
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

namespace V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice
{
    public class PerformaInv
    {
        [Key]
        public int InvId { get; set; }

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

        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal TodayVal { get; set; }

        public string? Prefix { get; set; }

        [Required, StringLength(50)]
        public string InvNo { get; set; }

        [Required]
        public string Suffix { get; set; }

        [Required]
        public DateTime InvDate { get; set; }
        public DateTime InvDateNow { get; set; } = DateTime.Now;

        [Range(1, int.MaxValue)]
        public int NoOfItems { get; set; }

        [StringLength(500)]
        public string? PayTerms { get; set; }

        [StringLength(500)]
        public string? DeliveryTerms { get; set; }

        [StringLength(500)]
        public string? MainRemark { get; set; }

        public bool InvCancl { get; set; } = false;
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }

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

        public int? BankId { get; set; }
        [ForeignKey(nameof(BankId))]
        public virtual Banks? Banks { get; set; }


        // Audit Fields
        [Required]
        [StringLength(50)]
        public string? CreatedBy { get; set; }
        [Required]
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        public virtual ICollection<PerformaInvSub> PerformaInvSubs { get; set; } = new List<PerformaInvSub>();

    }

}
