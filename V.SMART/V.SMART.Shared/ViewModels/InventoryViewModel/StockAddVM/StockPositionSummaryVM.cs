using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.StockAddVM
{
    public class StockPositionSummaryVM
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }

        public decimal? ROL { get; set; }
        public decimal? MOL { get; set; }

        public int StoreId { get; set; }
        public string? AddStoreName { get; set; }

        public int CategoryCode { get; set; }
        public string? Category { get; set; }

        public decimal BalQty { get; set; }
        public decimal UnitPrice { get; set; }
        public int SlNo { get; set; }

        public int ScreenCode { get; set; }
        public string? ScreenName { get; set; }
        public string? AddedBy { get; set; }

        // --- Reference Info ---
        public int SubItemRefID { get; set; }
        public string? RefNo { get; set; }
        public DateTime? RefDate { get; set; }
        public string? Remarks { get; set; }
        public DateTime AddDate { get; set; } = DateTime.Now;

        public string ItemDescription { get; set; } = "";

        public decimal TotalQty { get; set; }
        public decimal TotalAmount { get; set; }
        public int AttachmentCount { get; set; }
        public string? CustomerNames { get; set; }

        public string? Make { get; set; }

       

        public Dictionary<int, decimal> StoreQty { get; set; } = new();
        public Dictionary<int, decimal> StoreValue { get; set; } = new();
    }
}
