using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;

namespace V.SMART.Shared.Data.Inventory_Stock_.StockIssueRequest
{
    public class StockIssueRequest
    {

        [Key]
        public int RequestId { get; set; }

        [Required]
        [StringLength(20)]
        public string RequestNo { get; set; } = string.Empty;

        [Required]
        public string Suffix { get; set; }

        [Required]
        public DateTime RequestDate { get; set; }

        [Required]
        public DateTime RequestDateNow { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ToWhom { get; set; }

        public int? NoOfItems { get; set; }

        [StringLength(500)]
        public string? MainRemark { get; set; }


        public bool IsWithBom { get; set; } = false;

        public int? AssyId { get; set; }

        [ForeignKey(nameof(AssyId))]
        public Item? AssyItem { get; set; }

        public int? SubAssyId { get; set; }

        [ForeignKey(nameof(SubAssyId))]
        public Item? SubAssyItem { get; set; }

        public int? SubAssyId2 { get; set; }

        [ForeignKey(nameof(SubAssyId2))]
        public Item? SubAssyItem2 { get; set; }


        public int? SubAssyId3 { get; set; }

        [ForeignKey(nameof(SubAssyId3))]
        public Item? SubAssyItem3 { get; set; }

        [Required]
        [Precision(18, 3)]
        public decimal ReqQty { get; set; }


        [Required]
        public int? StoreIssId { get; set; }
        [ForeignKey(nameof(StoreIssId))]
        public virtual Store? Store { get; set; }


        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }

        public bool ReqTally { get; set; } = false;
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
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }





        public virtual ICollection<StockIssueRequestSub> StockIssueRequestSubs { get; set; } = new List<StockIssueRequestSub>();
    }
}
