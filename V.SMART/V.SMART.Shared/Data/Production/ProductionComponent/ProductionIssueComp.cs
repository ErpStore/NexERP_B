using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Production.ProductionComponent
{
    public class ProductionIssueComp
    {
        [Key]
        public int IssueId { get; set; }

        [Required, MaxLength(50)]
        public string IssueNo { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime IssueDateNow { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? IssueToWhom { get; set; }

        [Required]
        public int StoreIssId { get; set; }
        [ForeignKey(nameof(StoreIssId))]
        public virtual Store StoreIssue { get; set; } = null!;

        public int? CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }

        [Required]
        public int NoOfItems { get; set; }

        [Precision(18, 3)]
        public decimal IssueQty { get; set; }

        [MaxLength(300)]
        public string? MainRemark { get; set; }

        public bool IssueTally { get; set; }

        public bool Cancel { get; set; }

        [StringLength(250)]
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }
        public bool ShortClose { get; set; }
        public bool IsWithoutPoDc { get; set; }
        public bool IsOutItemSameAsInItem { get; set; } = false;


        // Audit

        [Required, MaxLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        // Navigation
        public virtual ICollection<ProductionIssueCompSub> ProductionIssueCompSubs { get; set; } = new List<ProductionIssueCompSub>();
    }

}
