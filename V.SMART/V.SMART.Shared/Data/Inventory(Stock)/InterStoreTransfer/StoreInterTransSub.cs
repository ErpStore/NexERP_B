using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer
{
    public class StoreInterTransSub
    {
        [Key]
        public int ISTSubId { get; set; }

        [Required]
        public int ISTId { get; set; }

        [ForeignKey(nameof(ISTId))]
        public StoreInterTrans StoreInterTrans { get; set; } = null!;

        [Required]
        public int SlNo { get; set; }

        [Required]
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [Required]
        [Precision(18, 3)]
        public decimal Qty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Required]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        // NEW
        public string? BatchNo { get; set; }

        [StringLength(500, ErrorMessage = "Remark cannot exceed 500 characters.")]
        public string? Remark { get; set; }

    }
}
