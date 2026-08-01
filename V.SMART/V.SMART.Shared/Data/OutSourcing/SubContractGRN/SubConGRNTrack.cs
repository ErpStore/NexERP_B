using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.SubContractGRN
{
    public class SubConGRNTrack
    {
        [Key]
        public int TrackId { get; set; }

        public int RefGRNSubId { get; set; }
        [ForeignKey(nameof(RefGRNSubId))]
        public virtual SubConGRNSub? SubConGRNSub { get; set; }

        public int? RefRcSubId { get; set; }
        [ForeignKey(nameof(RefRcSubId))]
        public virtual RouteCardSub? RouteCardSub { get; set; }

        public int? RefDCSubId { get; set; }
        [ForeignKey(nameof(RefDCSubId))]
        public virtual SubConDcOutSub? SubConDcOutSub { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual PurchPoSub? PurchPoSub { get; set; }

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
