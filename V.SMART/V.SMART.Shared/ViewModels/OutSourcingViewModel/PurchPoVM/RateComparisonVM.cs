using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM
{
    public class RateComparisonVM
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public string ItemCode { get; set; }
        public string UOM { get; set; }
        public string HsnCode { get; set; }
        public int VendorCode { get; set; }
        public string VendorName { get; set; }
        public decimal Rate { get; set; }
        public decimal Qty { get; set; }
        public decimal RateDifference { get; set; }
        public bool IsLowest { get; set; }
        public int QuoteId { get; set; }
        public int EnquiryId { get; set; }
        public string EnquiryNo { get; set; }
        public int EnquirySubId { get; set; }
        public string QuoteNo { get; set; }
        public int QuoteSubId { get; set; }
        public DateTime? QuoteDate { get; set; }
        public DateTime DueDate { get; set; } 
        public decimal? Discount { get; set; }
        public decimal? TaxPercent { get; set; }
        public decimal NetRate { get; set; }
        public int DeliveryDays { get; set; }
        public bool IsSelected { get; set; }

        public bool IsDisabled { get; set; }   // <--- ADD THIS


    }
}
