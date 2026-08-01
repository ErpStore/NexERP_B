using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;

namespace V.SMART.Shared.Data.Production.ProductionIssueWOAssy
{
    public class ProductionIssueAssySub
    {
        [Key]
        public int IssueSubId { get; set; }

        [Required]
        public int IssueId { get; set; }

        [ForeignKey(nameof(IssueId))]
        public virtual ProductionIssueAssy? ProductionIssueAssyParent { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required]
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [Precision(18, 3)]
        public decimal IssueQty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        [MaxLength(300)]
        public string? Remark { get; set; }

        [MaxLength(100)]
        public string? BatchNo { get; set; }

        public int RefJobOrderSubId { get; set; }
        [ForeignKey(nameof(RefJobOrderSubId))]
        public virtual JobOrderSub? JobOrderSub { get; set; }

        public  int RefJobId { get; set; }
        [ForeignKey(nameof(RefJobId))]
        public virtual JobOrder? JobOrder { get; set; }

        public int? AssyId { get; set; }
        [ForeignKey(nameof(AssyId))]
        public virtual Item? AssyItem { get; set; }


        public int? CostId { get; set; }

        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }

        public int? RcSubId { get; set; }



        [ForeignKey(nameof(RcSubId))]
        public virtual RouteCardSub? RouteCardSub { get; set; }

    }
}
