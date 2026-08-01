using V.SMART.Shared.Data.Inspection.FinalInspection;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesDC
{
    public class MfgDcSub
    {
        [Key]
        public int DcSubId { get; set; }

        [Required]
        public int DcId { get; set; }

        [ForeignKey(nameof(DcId))]
        public MfgDc MfgDc { get; set; }

        [Required]
        public int SlNo { get; set; }
        public string? LineId { get; set; }

        [Required(ErrorMessage = "Please select ItemCode.")]
        public int? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }


        [Required(ErrorMessage = "Qty is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Precision(18, 3)]
        public decimal Qty { get; set; }

        [Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        public int? RefPoSubId { get; set; }

        [ForeignKey(nameof(RefPoSubId))]
        public virtual MfgPoSub MfgPoSub {  get; set; }

        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }
        public DateTime? PoDueDate { get; set; }

        public string? Remark { get; set; }

        public bool ItemCancel { get; set; }

        [Precision(18, 3)]
        public decimal RejectQty { get; set; }

        [Precision(18, 3)]
        public decimal ReworkQty { get; set; }
        [StringLength(200)]
        public string? RejRowRemark { get; set; }

        [StringLength(150)]
        public string? ItemCancelReason { get; set; }

        [Required]
        public string TypeOfPo { get; set; } = string.Empty;

        [StringLength(200)]
        public string? RcSubIds { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }


        public int? RefSCNSubId { get; set; }

        [ForeignKey(nameof(RefSCNSubId))]
        public virtual PurchaseSCNSub PurchaseSCNSub { get; set; }

        public int? InspectId { get; set; }
        [ForeignKey(nameof(InspectId))]
        public virtual FinalInspection? FinalInspection { get; set; }
    }

}
