using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.RatingsVM
{
    public class RatingVM
    {
        public long? PartyId { get; set; }

        public string? CustName { get; set; } = string.Empty;

        public long? POId { get; set; }

        public string? PONo { get; set; } = string.Empty;

        public DateTime? PODate { get; set; }

        public DateTime? DCDate { get; set; }

        public DateTime? InvDate { get; set; }

        public DateTime? PoDueDate { get; set; }

        public long? TotalDays { get; set; }

        public long? Rating { get; set; }

        
       public long? DeliveryRating { get; set; }

        
        public long? QualityRating { get; set; }


        public long? VendorRating { get; set; }


        public long? SalesRating { get; set; }


        
        // Item Details
        public long? ItemId { get; set; }

        public string? ItemCode { get; set; } = string.Empty;

        public string? ItemName { get; set; } = string.Empty;

        // WO / DC Details
        public string? WoNo { get; set; } = string.Empty;

        public string? DcNo { get; set; } = string.Empty;

        // Quantities
        public decimal? Qty { get; set; }

        public decimal? DcQty { get; set; }
      
        public decimal? INVQty { get; set; }

        public decimal? RejectQty { get; set; }

        public decimal? ReworkQty { get; set; }

        // Invoice Details
        public string? InvNo { get; set; } = string.Empty;
    }
}
