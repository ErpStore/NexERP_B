using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.StoreInterTransferViewModel
{
    public class StoreInterTransSubVM
    {
        public int ISTSubId { get; set; }

        public int ISTId { get; set; }

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

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal? Qty { get; set; }
        public double AddQty { get; set; }
        public decimal BalQty { get; set; }
        public string? Status { get; set; }

        public decimal LineGross => (Qty ?? 0m) * (UnitPrice ?? 0m);

        public decimal? UnitPrice { get; set; }

        // NEW
        public string? BatchNo { get; set; }
        public decimal StockQty { get; set; }

        [StringLength(500, ErrorMessage = "Remark cannot exceed 500 characters.")]
        public string? Remark { get; set; }

        public int? CostId { get; set; } = null;
        public string? ProjectNo { get; set; }
        public bool IsEditable { get; set; } = false;
    }
}
