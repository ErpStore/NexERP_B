using DocumentFormat.OpenXml.Bibliography;
using V.SMART.Shared.Data.Inspection.IncomingInspection;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.PurchaseGRN
{
    public class PurchaseGRNSub
    {
        [Key]
        public int GRNSubId { get; set; }

        [Required]
        public int GRNId { get; set; }   // Foreign key to PurchaseGRN
        [ForeignKey(nameof(GRNId))]
        public PurchaseGRN PurchaseGRN { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select ItemCode.")]
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        [Required(ErrorMessage = "Qty is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Precision(18, 3)]
        public decimal Qty { get; set; }
        [Precision(18, 3)]
        public decimal BalQty { get; set; }
        [Precision(18, 3)]
        public decimal DNQty { get; set; }

        [Precision(18, 3)]
        public decimal ExtraQty { get; set; }

        [Precision(18, 3)]
        public decimal ExtraBalQty { get; set; }

        [Precision(18, 3)]
        public decimal SCNRejRevertedPOQty { get; set; }


        [Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        public bool IsOpenPo { get; set; } = false;
        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual PurchPoSub? PurchPoSub { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

        public bool ItemCancel { get; set; }

        public string? Remark { get; set; }

        public string? HeatNo { get; set; }

        [MaxLength(30)]
        public string? BatchNo { get; set; }

        public string? ItemCancelReason { get; set; }


        public int? InspectId { get; set; }
        [ForeignKey(nameof(InspectId))]
        public virtual IncomingInspection? IncomingInspection { get; set; }

    }
}
