using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.Inventory_module
{
    public class AssemblyCharge
    {
        [Key]
        public int AssemblyExtraChargeId { get; set; }

        public int AssmblyID { get; set; }

        public string? Name { get; set; } = string.Empty;


        public bool IsAmount { get; set; }

        [Precision(18, 3)]
        public decimal? Amount { get; set; }

        [Precision(18, 3)]
        public decimal? Percent { get; set; }
        public bool IsLabour { get; set; } = false;
        public bool IsDefault { get; set; } = false;

        [Precision(18, 3)]
        public decimal? CalculatedAmount { get; set; }

    }
}
