using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.Labour_SCN
{
    public class LabourSCN
    {
        [Key]
        public int SCNId { get; set; }

        [Required]
        [StringLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "SCN Number is required?,SCN No. should not be empty")]
        [StringLength(15)]
        public string SCNNo { get; set; } = string.Empty;

        [Required]
        public DateTime SCNDate { get; set; } = DateTime.Now;
        public DateTime SCNDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please select a Customer first.")]
        public int CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public Customer? Customer { get; set; }

        [Required(ErrorMessage = "Please select a store to Issue.")]
        public int? StoreIssId { get; set; }

        [ForeignKey(nameof(StoreIssId))]
        public virtual Store? StoreIssue { get; set; }  // ✅ Correct FK


        [Required(ErrorMessage = "Please select a store to Add.")]
        public int? AddStoreId { get; set; }

        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? StoreAdd { get; set; }

        public bool SCNCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }
        public bool ShortClose { get; set; } = false;
        public bool SCNTally { get; set; } = false;

        [StringLength(300)]
        public string? MainRemarks { get; set; }

        [StringLength(200)]
        public string? KindofAttention { get; set; }

        [Range(0, 9999)]
        public int NoOfItems { get; set; }

        [StringLength(300)]
        public string? CancelReason { get; set; }

        [MaxLength(50)]
        public string CanceledBy { get; set; } = string.Empty;
        public bool IsLevel1 { get; set; }
        public bool IsLevel2 { get; set; }
        public bool IsLevel3 { get; set; }
        public bool IsLevel4 { get; set; }

        [StringLength(50)]
        public string? Level1Sign { get; set; }

        [StringLength(50)]
        public string? Level2Sign { get; set; }

        [StringLength(50)]
        public string? Level3Sign { get; set; }

        [StringLength(50)]
        public string? Level4Sign { get; set; }

        public bool Authorized { get; set; }

        [StringLength(50)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovalDate { get; set; }

        public bool IsRejected { get; set; }

        [StringLength(300)]
        public string? RejectReason { get; set; }


        [MaxLength(50)]
        public string? CreatedBy { get; set; } = string.Empty;

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public ICollection<LabourSCNSub>? LabourSCNSubs { get; set; } = new List<LabourSCNSub>();

    }
}
