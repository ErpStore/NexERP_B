using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_
{
    public class StockIssueTrack
    {
        [Key]
        public int IssueTrackId { get; set; }

        [Required]
        public int? IssueId { get; set; }
        [ForeignKey(nameof(IssueId))]
        public StockIssue StockIssue { get; set; } = null!;

        [Required]
        public int? AddId { get; set; } // The StockAdd entry used
        [ForeignKey(nameof(AddId))]
        public StockAdd StockAdd { get; set; } = null!;

        [Required]
        [Precision(18, 3)]
        public decimal UsedQty { get; set; } // How much from this AddId was used
    }
}
