using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConGRNSubVM
    {
        public int GRNSubId { get; set; }

        [Required]
        public int GRNId { get; set; }

        public int SlNo { get; set; }

        [Required(ErrorMessage = "Transaction type is required.")]
        [StringLength(10, ErrorMessage = "Transaction type cannot exceed 10 characters.")]
        public string TransType { get; set; } = "In";

        [Required(ErrorMessage = "Please select an item.")]
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }

        [Required(ErrorMessage = "Received quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Received quantity must be greater than zero.")]
        public decimal? Qty { get; set; }

        public decimal? PossibleQty { get; set; }

        public decimal? BalQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal? UnitPrice { get; set; }
        
        public int? CategoryCode { get; set; }
        public string? CategoryName { get; set; }

        public int? RefDcSubId { get; set; }
        public string? RefDcNo { get; set; }
        public DateTime? RefDCDate { get; set; }


        public int? RefPoSubId { get; set; }
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }

        public int? RefRcSubId { get; set; }
        public string? RefRCNo { get; set; }
        public DateTime? RefRcDate { get; set; }

        public int? ProcessId { get; set; }
        public string? ProcessName { get; set; }

        public int? MachineId { get; set; }
        public string? MachineName { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remarks { get; set; }

        public bool ItemCancel { get; set; } = false;

        [StringLength(250)]
        public string? ItemCancelReason { get; set; }

        public bool IsEditable { get; set; } = false;
    }
}
