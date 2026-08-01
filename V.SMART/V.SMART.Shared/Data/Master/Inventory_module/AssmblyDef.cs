using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Inventory_module
{
    [Index(nameof(AssmblyID), nameof(ItemId), IsUnique = true)]
    public class AssmblyDef
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AssmblyID { get; set; } 

        [ForeignKey(nameof(AssmblyID))]
        public Item? AssemblyItem { get; set; }

        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? PartItem { get; set; }

        [Precision(18, 3)]
        public decimal UtilQty { get; set; }

        [Required]
        public int SlNo { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        public bool Cancel { get; set; }
        public bool IsComponent { get; set; }

        public int? ProcessId { get; set; }
        public Process? Process { get; set; }

        public decimal StockConvertedQty { get; set; }
        public decimal PRConvertedQty { get; set; }

        public string? Remarks { get; set; }
        public bool Completed { get; set; }

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        /// SNS<summary>
        [Precision(18, 3)]
        public decimal? UnitPrice { get; set; }

        [Precision(18, 3)]
        public decimal? MaterialCost { get; set; }

        [Precision(18, 3)]
        public decimal? LaborCost { get; set; }

        [Precision(18, 3)]
        public decimal? TotalCost { get; set; }

        [Precision(18, 3)]
        public decimal? AssemblyCost { get; set; }
        //This will for labour costcalculation
        public bool IsLabourMaterial { get; set; } = false;
        /// </summary>

    }

}
