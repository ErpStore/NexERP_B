using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_.ToolCrib
{
    public class ToolCribReturnSub
    {
        [Key]
        public int TCReturnSubId { get; set; }
        public int TCReturnId { get; set; }
        [ForeignKey(nameof(TCReturnId))]
        public ToolCribReturn ToolCribReturn { get; set; }

        [Required]
        public int SlNo { get; set; }


        public int? RefTCIssueSubId { get; set; }

        [ForeignKey(nameof(RefTCIssueSubId))]
        public ToolCribIssueSub? ToolCribIssueSub { get; set; }


        [Required(ErrorMessage = "Please select ItemCode")]
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }


        [Precision(18, 3)]
        public decimal AccpQty { get; set; }


        [Precision(18, 3)]
        public decimal RejQty { get; set; }
        [StringLength(100)]
        public string? RejRemark { get; set; }
        [Precision(18, 3)]

        public decimal RewQty { get; set; }
        [StringLength(100)]
        public string? RewRemark { get; set; }

        [StringLength(100)]
        public string? Remark { get; set; }


    }

}
