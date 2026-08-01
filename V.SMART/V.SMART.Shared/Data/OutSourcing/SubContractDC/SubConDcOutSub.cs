using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.OutSourcing.SubContractDC
{
    public class SubConDcOutSub
    {
        [Key]
        public int DcSubId { get; set; }

        public bool IsOpenPo { get; set; } = false;

        [Required]
        [Range(1, int.MaxValue)]
        public int SlNo { get; set; }

        [Required]
        public int DcId { get; set; }

        [ForeignKey(nameof(DcId))]
        public virtual SubConDcOut SubConDcOut { get; set; } = null!;

        // ===== Transaction =====
        [Required]
        [MaxLength(10)]
        public string TransType { get; set; } = "Out";

        // ===== Item =====
        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; } = null!;

        // ===== Quantity =====
        [Required]
        [Precision(18, 3)]
        public decimal Qty { get; set; }

        [Precision(18, 3)]
        public decimal? BalQty { get; set; }

        // ===== Pricing =====
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        [Precision(18, 4)]
        public decimal ProcessCost { get; set; }

        // ===== References =====
        public int? RcSubId { get; set; }

        [ForeignKey(nameof(RcSubId))]
        public virtual RouteCardSub? ComponentRouteCardSub { get; set; }

        public int? ProcessId { get; set; }

        [ForeignKey(nameof(ProcessId))]
        public virtual Process? Process { get; set; }

        public int? MachineId { get; set; }

        [ForeignKey(nameof(MachineId))]
        public virtual Machine? Machine { get; set; }

        public int? RefPoSubId { get; set; }

        [ForeignKey(nameof(RefPoSubId))]
        public virtual PurchPoSub? PurchPoSub { get; set; }

        public int? CostId { get; set; }

        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }


        // ===== Additional =====

        public bool ItemCancel { get; set; } = false;

        [StringLength(300)]
        public string? ItemCancelReason { get; set; }



        [MaxLength(300)]
        public string? Remark { get; set; }

        [MaxLength(100)]
        public string? BatchNo { get; set; }

        [MaxLength(50)]
        public string? HeatNo { get; set; }

        public int? GroupId { get; set; }
    }

}
