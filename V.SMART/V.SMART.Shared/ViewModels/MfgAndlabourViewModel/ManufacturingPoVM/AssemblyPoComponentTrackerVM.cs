using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM
{
    public class AssemblyPoComponentTrackerVM
    {
        public int Id { get; set; }

        public int PoId { get; set; }
        public int PoSubId { get; set; }

        public int AssyId { get; set; }

        public int ComponentId { get; set; }

        public decimal MaxAllowedQty { get; set; }
        public decimal IssuedQty { get; set; }
        public decimal RemainingQty { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
