using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.LabourDC
{
    public class LabourDcReturnCompTrack
    {
        [Key]
        public int TrackId { get; set; }

        public int? RefDcSubId { get; set; }
        [ForeignKey(nameof(RefDcSubId))]
        public virtual LabourDcOutgoingSub? LabourDcOutgoingSub { get; set; }

        public int? RefSCNSubId { get; set; }
        [ForeignKey(nameof(RefSCNSubId))]
        public virtual LabourSCNSub? LabourSCNSub { get; set; }

        public int RefGRNSubId { get; set; }
        [ForeignKey(nameof(RefGRNSubId))]
        public virtual LabourGRNSub? LabourGRNSub { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual MfgPoSub MfgPoSub { get; set; }


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
