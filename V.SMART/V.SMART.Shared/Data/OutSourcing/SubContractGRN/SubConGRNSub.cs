using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.SubContractGRN
{
    public class SubConGRNSub
    {
        [Key]
        public int GRNSubId { get; set; }

        public int GRNId { get; set; }
        [ForeignKey(nameof(GRNId))]
        public virtual SubConGRN? SubConGRNs { get; set; }

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


        public int? RefDcSubId { get; set; }
        [ForeignKey(nameof(RefDcSubId))]
        public virtual SubConDcOutSub? SubConDcOutSub { get; set; }


        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual PurchPoSub? PurchPoSub { get; set; }


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

        public bool ItemCancel { get; set; } = false;

        [StringLength(250)]
        public string? ItemCancelReason { get; set; }
    }
}
