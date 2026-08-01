using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Rejection_Module;

namespace V.SMART.Shared.Data.Production.ProductionComponent
{
    public class ProductionSCNCompSub
    {
        [Key]
        public int SCNSubId { get; set; }

        public int SCNId { get; set; }
        [ForeignKey(nameof(SCNId))]
        public virtual ProductionSCNComp? ProductionSCNComp { get; set; }

        public int? RefReturnSubId { get; set; }
        [ForeignKey(nameof(RefReturnSubId))]
        public virtual ProductionReturnCompSub? ProductionReturnCompSubs { get; set; }


        public int? RcSubId { get; set; }
        [ForeignKey (nameof(RcSubId))]
        public virtual RouteCardSub? RouteCardSub { get; set; }


        public int? PoSubId { get; set; }
        [ForeignKey(nameof(PoSubId))]
        public virtual MfgPoSub? MfgPoSub { get; set; }


        [Required]
        public int SlNo { get; set; }
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }


        [Precision(18, 3)]
        public decimal AccQty { get; set; }

        [Precision(18, 3)]
        public decimal RejQty { get; set; }
        public string? RejReason { get; set; }

        [Precision(18, 3)]
        public decimal RewQty { get; set; }
        public string? RewReason { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Precision(18, 3)]
        public decimal UnitPrice { get; set; }

        public string? InsNO { get; set; }
        public string? NCRNo { get; set; }


        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }

        [MaxLength(250)]
        public string? Remark { get; set; }

        public decimal SCNBalQty { get; set; }

        public int? RejectionId { get; set; }

        [ForeignKey(nameof(RejectionId))]
        public virtual RejectionMaster? RejectionMaster { get; set; }


    }
}
