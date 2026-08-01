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

namespace V.SMART.Shared.Data.Planning.AssyJobOrder
{
    public class JobOrderSub
    {
        [Key]
        public int JobSubId { get; set; }

        public int JobId { get; set; }

        [ForeignKey(nameof(JobId))]
        public virtual JobOrder JobOrder { get; set; }
        [Required]
        public int SlNo { get; set; }

        [Required]
        public int? ItemId { get; set; }
        [ForeignKey (nameof(ItemId))]
        public Item? Item { get; set; }

        [Precision(18, 3)]
        public decimal UtilQty { get; set; }

        [Precision(18, 3)]
        public decimal ReqQty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        public bool ItemCancel { get; set; }
        [StringLength(300)]
        public string? ItemCancelReason { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }
    }

}
