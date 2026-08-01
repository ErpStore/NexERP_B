using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.StockIssueRequestVM
{
    public class StockIssueRequestSubVM
    {
        public int RequestSubId { get; set; }

        public int RequestId { get; set; }

        [Required(ErrorMessage = "Serial Number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Serial Number must be a positive integer.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Item is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Item.")]
        public int? ItemId { get; set; }
        public ItemVM? SelectedItemVMs { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? UOM { get; set; }
        public decimal? UtlQty { get; set; }


        [Required(ErrorMessage = "Issue Qty is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Issue Qty must be greater than zero.")]
        public decimal? ReqQty { get; set; }

        public decimal? ReqBalQty { get; set; }

        public decimal LineGross => (ReqQty ?? 0m) * (UnitPrice ?? 0m);

        [Required(ErrorMessage = "Unit Price is required.")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Unit-Price cannot be negative.")]
        public decimal? UnitPrice { get; set; }

        public decimal StockQty { get; set; }

        [StringLength(500, ErrorMessage = "Remark cannot exceed 500 characters.")]
        public string? Remark { get; set; }


        [StringLength(50, ErrorMessage = "Batch Number cannot exceed 50 characters.")]
        public string? BatchNo { get; set; }

        [StringLength(50, ErrorMessage = "Make cannot exceed 50 characters.")]
        public string? Make { get; set; }

        [StringLength(50, ErrorMessage = "Rack Number cannot exceed 50 characters.")]
        public string? RackNo { get; set; }

        public int? CostId { get; set; } = null;
        public string? ProjectNo { get; set; }
        public bool IsEditable { get; set; } = false;

    }
}
