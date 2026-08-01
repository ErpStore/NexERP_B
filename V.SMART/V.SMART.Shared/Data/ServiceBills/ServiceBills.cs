
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Accounts;

namespace V.SMART.Shared.Data.CashFlow.ServiceBills
{
    public class ServiceBills
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        public bool IncomingServics { get; set; }

        [Required]
        [StringLength(450)]
        public string InvNo { get; set; } = null!;

        [Required]
        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime InvDate { get; set; }

        [Required]
        public DateTime InvDateNow { get; set; }


        // -------------------- Customer --------------------
        [Required]
        public int CustVendorId { get; set; }

        public int? NoOfItems { get; set; }

        public string? MainRemark { get; set; }


        // -------------------- Ref Details --------------------
        public string? DcNo { get; set; }
        public DateTime? DcDate { get; set; }


        // -------------------- Transport --------------------
        public string? ModeofTrasnport { get; set; }
        [RegularExpression(@"^[A-Z]{2}[0-9]{1,2}[A-Z]{1,2}[0-9]{4}$", ErrorMessage = "Invalid Vehicle No (e.g., KA01AB1234).")]
        public string? VehicleNo { get; set; } = string.Empty;


        // -------------------- Currency --------------------
        [Required]
        public int CurrId { get; set; }

        [ForeignKey(nameof(CurrId))]
        public Currency? Currency { get; set; }

 
        [Column(TypeName = "decimal(18,2)")]
        public decimal TodayVal { get; set; }


        // -------------------- Cancel -------------------- 
        public bool IsCancel { get; set; }
        public bool ShortClose { get; set; } = false;
        public string? CancelReason { get; set; }
        public string? PayTerms { get; set; }
        public string? Remarks { get; set; }


        // -------------------- Amounts --------------------
        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalGrossAmount { get; set; }

        public bool IsItemWiseDiscAmtOrPerReq { get; set; }
        public bool DiscAmtOrPer { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal DiscountAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalBasicAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal FreightCharges { get; set; }

        // -------------------- Packing --------------------
        public bool PackingAmtOrPer { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal PackingPercent { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal PackingCharges { get; set; }

        // -------------------- Insurance --------------------
        public bool InsuranceAmtOrPer { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal InsurancePercent { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal InsuranceCharges { get; set; }

        // -------------------- GST --------------------
        [Column(TypeName = "decimal(18,3)")]
        public decimal CGstRate { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal SGstRate { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal IGstRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalCGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalSGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalIGSTAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal OtherCharges { get; set; }

        // -------------------- TCS --------------------
        public bool TCSAmtOrPer { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TCSPercent { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TCSAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal TotalTaxable { get; set; }


        //Debit Note / Credit Note Detais

        [Column(TypeName = "decimal(18,4)")]
        public decimal DebitAmount { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal DebitRem{ get; set; }

        [Column(TypeName ="decimal(18,4)")]
        public decimal Debit { get; set; }


        // -------------------- Round Off --------------------
        public bool IsRoundOffEnabled { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal RoundOff { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal GrandTotal { get; set; }

        public bool ServiceTally { get; set; } = false;
        public decimal Balance { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate{ get; set; }

        public virtual ICollection<ServiceBillsSub> ServiceBillSub { get; set; } = new List<ServiceBillsSub>();
    }
}
