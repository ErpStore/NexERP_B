using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_.MaterialIssueNote
{
    public class MaterialIssNote
    {
        [Key]
        public int MINId { get; set; }

        [Required]
        [StringLength(20)]
        public string IssueNo { get; set; } = string.Empty;

        [Required]
        public string Suffix { get; set; }

        [Required]
        public DateTime IssueDate { get; set; }

        [Required]
        public DateTime IssueDateNow { get; set; } = DateTime.Now;

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


        // Audit Fields
        [StringLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }= DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        public virtual ICollection<MaterialIssNoteSub> MaterialIssNoteSubs { get; set; } = new List<MaterialIssNoteSub>();

    }
}
