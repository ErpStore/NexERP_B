using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer
{
    public class StoreInterTrans
    {
        [Key]
        public int ISTId { get; set; }

        [Required]
        public string ISTNo { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public String Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime ISTDate { get; set; } = DateTime.Now;

        [Required]
        public int StoreAddId { get; set; }
        [ForeignKey(nameof(StoreAddId))]
        public Store? StoreAdd { get; set; }

        [Required]
        public int StoreIssueId { get; set; }
        [ForeignKey(nameof(StoreIssueId))]
        public Store? StoreIssue { get; set; }

        public int? CostId { get; set; }

        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }

        [StringLength(500, ErrorMessage = "Main Remark cannot exceed 500 characters.")]
        public string? MainRemark { get; set; }

        [Required]
        public int NoOfItems { get; set; }


        [Required]
        public string CreatedBy { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100, ErrorMessage = "Modified By cannot exceed 50 characters.")]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<StoreInterTransSub> StoreInterTransSubs { get; set; } = new List<StoreInterTransSub>();
    }
}
