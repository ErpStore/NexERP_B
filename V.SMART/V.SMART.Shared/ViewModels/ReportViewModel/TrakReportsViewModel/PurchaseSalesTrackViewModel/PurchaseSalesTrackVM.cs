using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.PurchaseSalesTrackViewModel
{
    public class PurchaseSalesTrackVM
    {
        public  int SlNo { get; set; }
        //Purchase
        
        public string? VendorName { get; set; }

        public string? PONo { get; set; }
        public DateTime? PODate { get; set; }
        public string? GRNNO { get; set; }
        public DateTime? GRNDate { get; set; }
        public string? ItemCode { get; set; } 
        public string? ItemName { get; set; } 
        public string? UOM { get; set; } 
        public string? HSNCode { get; set; } 
        public decimal? POQty { get; set; } 
        //public decimal? PoBalQty { get; set; } 
      
       // public decimal? GRNQty { get; set; } 
        // decimal? GRNBalQty { get; set; } 
        //public string? SCNNo { get; set; } 
        //public DateTime? SCNDate { get; set; } 
        //public decimal? SCNQty { get; set; } 
        public decimal? AccQty { get; set; } 
        public decimal? RejQty { get; set; } 
        public decimal? Unit_Price { get; set; } 
        public decimal?Purch_Discount { get; set; } 
        public decimal? BalQty { get; set; } 
        //public decimal? UsedQty { get; set; }

        //Sales
        public string? CustName { get; set; }
        public string? MfgPONo { get; set; }
        public DateTime? MfgpoDate { get; set; }
        //public decimal? MfgPoQty { get; set; }
       // public decimal? MfgPoBalQty { get; set; }
        public string? DcNo { get; set; }
        public DateTime? DcDate { get; set; }
        public decimal? DCQty { get; set; }
        //public decimal? DCBalQty { get; set; }
        public string? Invno { get; set; }
        public DateTime? InvDate { get; set; }
        public decimal? Qty { get; set; }
        public decimal? Sales_price { get; set; }
        public decimal? Discount { get; set; }
        public decimal? PurchaseAmount { get; set; }
        public decimal? SalesAmount { get; set; }
        public decimal? Profit { get; set; }
        public int? ItemId { get; set; }
        public int? CustId { get; set; }
        public int? VendorCode { get; set; }
        //public string? Process { get; set; }

    }
}
