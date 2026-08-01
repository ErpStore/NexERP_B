using DocumentFormat.OpenXml.Bibliography;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote
{
    public class SCNGenSub
    {
        [Key]
        public int SCNGenSubId { get; set; }

        public bool IsFromBOM { get; set; } = false;

        public int? RefBomId { get; set; }

        [ForeignKey(nameof(RefBomId))]
        public AssmblyDef? AssemblyDef { get; set; }

        [Required]
        public int SCNGenId { get; set; }

        [ForeignKey(nameof(SCNGenId))]
        public SCNGen SCNGen { get; set; } = null!;

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
        public decimal UtlQty { get; set; }

        [Required]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        [StringLength(500, ErrorMessage = "Remark cannot exceed 500 characters.")]
        public string? Remark { get; set; }

        [StringLength(50, ErrorMessage = "Batch Number cannot exceed 50 characters.")]
        public string? BatchNo { get; set; }

        [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters.")]
        public string? Make { get; set; }

        [StringLength(50, ErrorMessage = "Rack Number cannot exceed 50 characters.")]
        public string? RackNo { get; set; }

        public int? CostId { get; set; }

        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }

    }

}
