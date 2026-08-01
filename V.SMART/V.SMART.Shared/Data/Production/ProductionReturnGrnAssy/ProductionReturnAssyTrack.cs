using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Production.ProductionReturnGrnAssy
{
    public class ProductionReturnAssyTrack
    {
        [Key]
        public int TrackId { get; set; }

        public int RefReturnSubId { get; set; }
        [ForeignKey(nameof(RefReturnSubId))]
        public virtual ProductionReturnAssySub? ProductionReturnAssySub { get; set; }

        public int? RefJobOrderId { get; set; }
        [ForeignKey(nameof(RefJobOrderId))]
        public virtual JobOrder? JobOrder { get; set; }

        public int? RefIssueSubId { get; set; }
        [ForeignKey(nameof(RefIssueSubId))]
        public virtual ProductionIssueAssySub? ProductionIssueAssySub { get; set; }

        public int? ReturnItemId { get; set; }
        [ForeignKey(nameof(ReturnItemId))]
        public virtual Item? AssyItem { get; set; }

        public decimal? ReturnQty { get; set; }

        public int? IssueItemId { get; set; }
        [ForeignKey(nameof(IssueItemId))]
        public virtual Item? PartItem { get; set; }
        public decimal? IssueQty { get; set; }


        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

    }
}
