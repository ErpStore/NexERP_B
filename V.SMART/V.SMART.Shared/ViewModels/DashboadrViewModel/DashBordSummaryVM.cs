using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;

namespace V.SMART.Shared.ViewModels.DashboadrViewModel
{
    public class DashBordSummaryVM
    {
        // =====================================================
        // CUSTOMER SECTION
        // =====================================================
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int InActiveCustomers { get; set; }

        public List<CustomerVM> LastCustomers { get; set; } = new();

        // =====================================================
        // VENDOR SECTION
        // =====================================================
        public int TotalVendors { get; set; }
        public int ActiveVendors { get; set; }
        public int InActiveVendors { get; set; }

        public List<VendorVM> LastVendors { get; set; } = new();

        // =====================================================
        // ITEM SECTION
        // =====================================================
        public int TotalItems { get; set; }
        public int ActiveItems { get; set; }
        public int InActiveItems { get; set; }

        public List<ItemVM> LastItems { get; set; } = new();

        // =====================================================
        // SALES / PURCHASE
        // =====================================================
        public decimal SalesThisMonth { get; set; }
        public decimal PurchaseThisMonth { get; set; }

        // =====================================================
        // OTHER KPI
        // =====================================================
        public int PendingOrders { get; set; }
        public int LowStockItems { get; set; }
    }
}
