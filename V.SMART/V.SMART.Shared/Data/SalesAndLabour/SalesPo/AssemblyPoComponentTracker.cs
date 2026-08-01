using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesPo
{
    public class AssemblyPoComponentTracker
    {
        public int Id { get; set; }

        public int? PoId { get; set; }

        public int? PoSubId { get; set; }
        [ForeignKey(nameof(PoSubId))]
        public virtual MfgPoSub? MfgPoSub { get; set; }

        public int? AssyId { get; set; }
        [ForeignKey(nameof(AssyId))]
        public virtual Item? Assembly { get; set; }

        public int? ComponentId { get; set; }
        [ForeignKey(nameof(ComponentId))]
        public virtual Item? Component { get; set; }

        public decimal MaxAllowedQty { get; set; }
        public decimal IssuedQty { get; set; }
        public decimal RemainingQty { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
