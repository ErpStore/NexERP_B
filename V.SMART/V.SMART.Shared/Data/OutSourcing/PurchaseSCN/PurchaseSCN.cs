using V.SMART.Shared.Data.Inventory_Stock_;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.PurchaseSCN
{
    public class PurchaseSCN
    {
        [Key]
        public int SCNId { get; set; }

        [Required]
        [MaxLength(10)]
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "SCN Number is required?,SCN No. should not be empty")]
        [StringLength(15)]
        public string SCNNo { get; set; } = string.Empty;

        [Required]
        public DateTime SCNDate { get; set; } = DateTime.Now;
        public DateTime SCNDateNow { get; set; } = DateTime.Now;

        [Required]
        public int? VendorCode { get; set; }
        [ForeignKey(nameof(VendorCode))]
        public Vendor? Vendor { get; set; }

        [Required(ErrorMessage = "Please select a store to Add.")]
        public int? AddStoreId { get; set; }

        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? StoreAdd { get; set; }   // ✅ Correct FK

        [Required(ErrorMessage = "Please select a store to Issue.")]
        public int? StoreIssId { get; set; }

        [ForeignKey(nameof(StoreIssId))]
        public virtual Store? StoreIssue { get; set; }  // ✅ Correct FK
        
        public bool SCNCancel { get; set; } = false;
        public bool SCNTally { get; set; } = false;
        public bool ShortClose { get; set; } = false;


        [MaxLength(500)]
        public string? MainRemarks { get; set; }

        [MaxLength(80)]
        public string? KindofAttention { get; set; }

        public int NoOfItems { get; set; }

        [MaxLength(250)]
        public string? CancelReason { get; set; }
        public string? CancelBy { get; set; }

        public DateTime? CancelDate { get; set; }


        [MaxLength(50)]
        public string? CreatedBy { get; set; } = string.Empty;

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(50)]
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

        // Optional navigation collection if you have a sub table
        public ICollection<PurchaseSCNSub>? PurchaseSCNSubs { get; set; }= new List<PurchaseSCNSub>();
    }
}
