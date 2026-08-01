using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.LabourGRN
{
    public class LabourGRNSub
    {
        [Key]
        public int GRNSubId { get; set; }
        public int? GroupId { get; set; }

        [Required]
        public int GRNId { get; set; }

        [ForeignKey(nameof(GRNId))]
        public LabourGRN? LabourGRN { get; set; }

        public bool isOpenPo { get; set; } = false;

        [Required]
        public int SlNo { get; set; }

        
        [Required(ErrorMessage = "Outgoing Item is required.")]
        public int? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; } = null;

        [StringLength(500)]
        public string? ItemSpecification { get; set; }

        [Precision(18, 3)]
        [Range(0.001, double.MaxValue, ErrorMessage = "Qty must be greater than zero.")]
        public decimal Qty { get; set; }

        [Precision(18, 3)]
        [Range(0.001, double.MaxValue, ErrorMessage = "Qty must be greater than zero.")]
        public decimal? DCQty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        public decimal ProdCompBalQty { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal? UnitPrice { get; set; }

        [MaxLength(30)]
        public string? WONo { get; set; }

        
        [MaxLength(150)]
        public string? RowRemarks { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

        [MaxLength(30)]
        public string? HeatNo { get; set; }

        [MaxLength(30)]
        public string? BatchNo { get; set; }

        public bool ItemCancel { get; set; }

        public string? CancelItemReason { get; set; }//CostId

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual MfgPoSub MfgPoSub { get; set; }

        [Required]
        public string TransType { get; set; } = "In";

    }
}
