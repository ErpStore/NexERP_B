using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;

namespace V.SMART.Shared.Data.Production.ProductionComponent
{
    public class ProductionReturnCompTrack
    {
        [Key]
        public int TrackId { get; set; }

        public int RefReturnSubId { get; set; }
        [ForeignKey(nameof(RefReturnSubId))]
        public virtual ProductionReturnCompSub? ProductionReturnCompSub { get; set; }

        public int? RefRcSubId { get; set; }
        [ForeignKey(nameof(RefRcSubId))]
        public virtual RouteCardSub? RouteCardSub { get; set; }

        public int? RefIssueSubId { get; set; }
        [ForeignKey(nameof(RefIssueSubId))]
        public virtual ProductionIssueCompSub? ProductionIssueCompSub { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual MfgPoSub? MfgPoSub { get; set; }


        public int? ItemIdIn { get; set; }
        [ForeignKey(nameof(ItemIdIn))]
        public virtual Item? InComingItem { get; set; }
        public decimal? QtyIn { get; set; }


        public int? ItemIdOut { get; set; }
        [ForeignKey(nameof(ItemIdOut))]
        public virtual Item? OutgoingItem { get; set; }
        public decimal? QtyOut { get; set; }




        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
