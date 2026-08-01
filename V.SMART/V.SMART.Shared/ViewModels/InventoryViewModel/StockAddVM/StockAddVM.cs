using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.StockAddVM
{
    public class StockAddVM
    {

        // --- Stock Addition Info ---
        public int AddId { get; set; }

        [Required(ErrorMessage = "Add Date is required.")]
        public DateTime AddDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Item is required.")]
        public int ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }

        // --- Item Details ---
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal? Weight { get; set; }
        public decimal? ROL { get; set; }
        public decimal? MOL { get; set; }
        public string? Category { get; set; }
        public string? Customer { get; set; }

        // --- Store Info ---
        [Required(ErrorMessage = "Store is required.")]
        public int StoreId { get; set; }
        public string? AddStoreName { get; set; }

        // --- Quantity & Pricing ---
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, 9999999, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal AddQty { get; set; }

        public decimal BalQty { get; set; }

        [Required(ErrorMessage = "UnitPrice is required.")]
        [Range(0, 9999999, ErrorMessage = "UnitPrice cannot be negative.")]
        public decimal UnitPrice { get; set; }

        [StringLength(50, ErrorMessage = "Batch number cannot exceed 50 characters.")]
        public string? BatchNo { get; set; }

        // --- Screen Info ---
        [Required(ErrorMessage = "Screen selection is required.")]
        public int ScreenCode { get; set; }
        public string? ScreenName { get; set; }

        // --- Reference Info ---
        public int SubItemRefID { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? Remarks { get; set; }

        // --- Audit Info ---
        public string? AddedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        public int SlNo { get; set; }

        public string ItemDescription { get; set; } = "";
        public Dictionary<int, double> StoreQty { get; set; } = new();
        public Dictionary<int, double> StoreValue { get; set; } = new();
        public double TotalQty { get; set; }
        public double TotalAmount { get; set; }

    }
}
