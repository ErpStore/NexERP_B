using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel
{
    public class ProductionReturnAssySubVM
    {
        public int ReturnSubId { get; set; }

        [Required(ErrorMessage = "Return ID is required.")]
        public int ReturnId { get; set; }

        [Required(ErrorMessage = "Serial number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than 0.")]
        public int SlNo { get; set; }


        [Required(ErrorMessage = "Item selection is required.")]
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }

        public int? RefIssueSubId { get; set; }


        public int? RefJobOrderId { get; set; }
        public string? RefJobOrderNo { get; set; }  
        public DateTime? RefJobOrderDate { get; set; }


        [Precision(18, 3)]
        [Range(0.001, double.MaxValue, ErrorMessage = "Returned quantity must be greater than zero.")]
        public decimal? QtyReturned { get; set; }

        [Precision(18, 3)]
        public decimal? PossibleQty { get; set; }

        [Precision(18, 3)]
        public decimal? BalQty { get; set; }

        [Precision(18, 3)]
        public decimal UnitPrice { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }


        [MaxLength(250, ErrorMessage = "Remark cannot exceed 250 characters.")]
        public string? Remark { get; set; }

        // UI-only fields
        public bool IsEditable { get; set; } = false;

        public int? InspectId { get; set; }
        public string? InspectNo { get; set; }
        public DateTime? InspectDate { get; set; }

    }

}
