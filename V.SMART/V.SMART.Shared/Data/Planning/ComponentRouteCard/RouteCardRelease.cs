using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Planning.ComponentRouteCard
{
    public class RouteCardRelease
    {
        public int RouteCardReleaseId { get; set; }

        public bool IsCancel { get; set; } = false;
        public bool ShortClose { get; set; } = false;

        [Required]
        public string RcReleaseNo { get; set; }

        [Required]
        public string Suffix { get; set; }

        [Required]
        public DateTime RcReleaseDate { get; set; } = DateTime.Now;

        public bool RcReleaseTally { get; set; } = false;

        [Required]
        public int? CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }


        public int? PoId { get; set; }
        [ForeignKey(nameof(PoId))]
        public virtual MfgPo? MfgPo { get; set; }

        public int? PoSubId { get; set; }
        [ForeignKey(nameof(PoSubId))]
        public virtual MfgPoSub? MfgPoSub { get; set; }

        public int AssemblyItemId { get; set; }
        [ForeignKey(nameof(AssemblyItemId))]
        public virtual Item? Assembly { get; set; }

        [Precision(18, 3)]
        public decimal PoQty { get; set; }

        [Precision(18, 3)]
        public decimal ReleaseQty { get; set; }

        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }

        public string ReleasedBy { get; set; }
        public DateTime ReleasedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; } = DateTime.Now;

        // Navigation
        public ICollection<RouteCardReleaseSub> RouteCardReleaseSubs { get; set; }
    }

}
