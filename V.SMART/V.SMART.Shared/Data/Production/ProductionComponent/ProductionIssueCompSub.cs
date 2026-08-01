using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;

namespace V.SMART.Shared.Data.Production.ProductionComponent
{
    public class ProductionIssueCompSub
    {

        [Key]
        public int IssueSubId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int SlNo { get; set; }

        [Required]
        public int? IssueId { get; set; }
        [ForeignKey(nameof(IssueId))]
        public virtual ProductionIssueComp? ProductionIssueComp { get; set; }


        [Required]
        public string TransType { get; set; } = "Out";

        [Required]
        public int? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [Required, Precision(18, 3)]
        public decimal? Qty { get; set; }

        [Precision(18, 3)]
        public decimal? BalQty { get; set; }

        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        public decimal QtyPerUnit { get; set; }

        [Precision(18, 4)]
        public decimal ProcessCost { get; set; }

        // References
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
        public virtual MfgPoSub? MfgPoSub { get; set; }

        public int? RefGRNSubId { get; set; }
        [ForeignKey(nameof(RefGRNSubId))]
        public virtual LabourGRNSub? LabourGRNSub { get; set; }


        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }


        [MaxLength(300)]
        public string? Remark { get; set; }

        [MaxLength(100)]
        public string? BatchNo { get; set; }

        public string? HeatNo { get; set; }


    }

}
