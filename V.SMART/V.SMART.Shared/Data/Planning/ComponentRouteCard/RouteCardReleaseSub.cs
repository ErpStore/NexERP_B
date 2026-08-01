

using DocumentFormat.OpenXml.Bibliography;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Planning.ComponentRouteCard
{
    public class RouteCardReleaseSub
    {
        [Key]
        public int RouteCardReleaseSubId { get; set; }
        public int SlNo { get; set; }
        public int RouteCardReleaseId { get; set; }

        [ForeignKey(nameof(RouteCardReleaseId))]
        public virtual RouteCardRelease RouteCardRelease { get; set; }

        // Component
        public int ComponentItemId { get; set; }

        [ForeignKey(nameof(ComponentItemId))]
        public virtual Item? Item { get; set; }

        [Precision(18, 3)]
        public decimal CumulativeUtilQty { get; set; }

        [Precision(18, 3)]
        public decimal RequiredQty { get; set; }

        [Precision(18, 3)]
        public decimal IssuedQty { get; set; } = 0;

        [Precision(18, 3)]
        public decimal RemainingQty { get; set; }

        public bool IsCompleted { get; set; } = false;

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }


    }

}
