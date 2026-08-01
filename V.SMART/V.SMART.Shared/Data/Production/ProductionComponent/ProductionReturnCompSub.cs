using V.SMART.Shared.Data.Inspection.IncomingInspection;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Production.ProductionComponent
{
    public class ProductionReturnCompSub
    {
        [Key]
        public int ReturnSubId { get; set; }

        public int ReturnId { get; set; }
        [ForeignKey(nameof(ReturnId))]
        public virtual ProductionReturnComp? ProductionReturnComp { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required]
        public string TransType { get; set; }

        public int? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [Precision(18, 3)]
        public decimal? Qty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Precision(18, 3)]
        public decimal UnitPrice { get; set; }

        public int? CategoryCode { get; set; }


        public int? RefIssueSubId { get; set; }
        [ForeignKey(nameof(RefIssueSubId))]
        public virtual ProductionIssueCompSub? ProductionIssueCompSub { get; set; }


        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual MfgPoSub? MfgPoSub { get; set; }


        public int? RefRcSubId { get; set; }
        [ForeignKey(nameof(RefRcSubId))]
        public virtual RouteCardSub? RouteCardSubs { get; set; }


        public int? ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual Process? Process { get; set; }


        public int? MachineId { get; set; }
        [ForeignKey(nameof(MachineId))]
        public virtual Machine? Machine { get; set; }



        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }


        [MaxLength(250)]
        public string? Remarks { get; set; }


        public int? InspectId { get; set; }
        [ForeignKey(nameof(InspectId))]
        public virtual IncomingInspection? IncomingInspection { get; set; }

        public TimeOnly? StartTime { get; set; }

        public TimeOnly? EndTime { get; set; }

        public TimeOnly? PlatedTime { get; set; }
    }
}
