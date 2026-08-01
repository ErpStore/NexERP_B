using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel
{
    
    public class AssemblyChargeVM
    {
        public int AssemblyExtraChargeId { get; set; }

        public int AssmblyID { get; set; }

        [Required(ErrorMessage = "Charges Head Name is required")]
        public string? Name { get; set; } = string.Empty;

        public bool IsAmount { get; set; }
        public bool IsLabour { get; set; } = false;
        public bool IsDefault { get; set; } = false;

        [Precision(18, 3)]
        public decimal? Amount { get; set; }

        [Precision(18, 3)]
        public decimal? Percent { get; set; }

        [Precision(18, 3)]
        public decimal? CalculatedAmount { get; set; }
    }
}
