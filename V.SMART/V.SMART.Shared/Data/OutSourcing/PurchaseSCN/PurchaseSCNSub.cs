using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Rejection_Module;

namespace V.SMART.Shared.Data.OutSourcing.PurchaseSCN
{
    public class PurchaseSCNSub
    {
        [Key]
        public int SCNSubId { get; set; }

        [Required]
        public int SCNId { get; set; }
        [ForeignKey(nameof(SCNId))]
        public PurchaseSCN PurchaseSCN { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select ItemCode.")]
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        [Precision(18,3)]
        public decimal UnitConvert { get; set; }

        [Required(ErrorMessage = "Accept Qty is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Accept Qty must be greater than 0.")]
        [Precision(18, 3)]
        public decimal AcceptQty { get; set; }
        public decimal BalQty { get; set; }
        [Precision(18, 3)]
        public decimal RejectQty { get; set; }
        public decimal RejectBalQty { get; set; }

        [Precision(18, 3)]
        public decimal ReworkQty { get; set; }

        [Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        public int? RefGRNSubId { get; set; }

        [ForeignKey(nameof(RefGRNSubId))]
        public PurchaseGRNSub? PurchaseGRNSub { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual PurchPoSub? PurchPoSub { get; set; }

        public int? CostId { get; set; }

        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

        public bool ItemCancel { get; set; } = false;

        [MaxLength(250)]
        public string? ItemCancelReason { get; set; }

        [MaxLength(500)]
        public string? Remark { get; set; }

        [MaxLength(50)]
        public string? InspectionNo { get; set; }

        [MaxLength(50)]
        public string? NCRNo { get; set; }


        [Precision(18, 3)]
        public decimal QtyConvert { get; set; }

        [MaxLength(30)]
        public string? HeatNo { get; set; }

        [MaxLength(30)]
        public string? BatchNo { get; set; }

        public int? RejectionId { get; set; }

        [ForeignKey(nameof(RejectionId))]
        public virtual RejectionMaster? RejectionMaster { get; set; }


    }
}
