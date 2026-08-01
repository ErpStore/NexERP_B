using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.PurchasePo
{
    public class PurchPoSub
    {

        [Key]
        public int PoSubId { get; set; }

        [Required]
        public int PoId { get; set; }

        [ForeignKey(nameof(PoId))]
        public virtual PurchPo PurchPo { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; }


        public int? RefQuoteSubId { get; set; }
        [ForeignKey(nameof(RefQuoteSubId))]
        public virtual PurchaseQuoteSub? PurchaseQuoteSub { get; set; }

        public int? RefMReqSubId { get; set; }
        [ForeignKey(nameof(RefMReqSubId))]
        public virtual MaterialReqSub? MaterialReqSub { get; set; }

   
        // ===== References =====
        public int? RCId { get; set; }

        [ForeignKey(nameof(RCId))]
        public virtual RouteCard? RouteCardS { get; set; }

        public int? RefRcSubId { get; set; }

        [ForeignKey(nameof(RefRcSubId))]
        public virtual RouteCardSub? RouteCardSub { get; set; }
        public int? ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual Process? Process { get; set; }


        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal Qty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal BalQty { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Precision(18, 4)]
        public decimal LineGross { get; set; }

        [Precision(18, 3)]
        public decimal? LineDiscountPercent { get; set; }

        [Precision(18, 4)]
        public decimal? LineDiscountAmount { get; set; }

        [Precision(18, 4)]
        public decimal LineBasicAmount { get; set; }

        [Precision(18, 4)]
        public decimal? LineCGSTRate { get; set; }

        [Precision(18, 4)]
        public decimal? LineSGSTRate { get; set; }

        [Precision(18, 4)]
        public decimal? LineIGSTRate { get; set; }

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Now;

        public bool ItemCancel { get; set; } = false;
        public string? ItemCancelReason { get; set; }


        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter CostCenter { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        

    }
}
