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
using V.SMART.Shared.Data.Master.Inventory;

namespace V.SMART.Shared.Data.SalesAndLabour_Module
{
    public class LabInv
    {
        [Key]
        public int LabInvId { get; set; }


        [StringLength(10, ErrorMessage = "Prefix cannot exceed 10 characters")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Labour Invoice Number is Required!..")]
        public string LabInvNo { get; set; }

        [Required]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "Labour Invoice Date is Required!..")]
        public DateTime LabInvDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; } = DateTime.Now;
        public DateTime LabInvDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please Select the customer Name!..")]
        public int? CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public Customer? Customer { get; set; }

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

        public bool LabInvTally { get; set; } = false; 
        public decimal TodayVal { get; set; }
        public bool InvCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }
        public string? CanceledBy { get; set; }
        public bool ShortClose { get; set; } = false;

        [StringLength(300)]
        public string? CancelReason { get; set; }
        public string? PayTerms { get; set; }
        public string? DeliveryTems { get; set; }

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

        public decimal TDSAmount { get; set; }

        [Precision(18, 4)]
        public decimal Balance { get; set; }

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

        public int? ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public Process? Process { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<LabInvSub> LabInvSubs { get; set; } = new List<LabInvSub>();

    }


}
