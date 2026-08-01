using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM
{
    public class EnquiryPurchaseVendorAssignVM
    {
        public bool IsSelected { get; set; }

        public int SubId { get; set; }  // PK
        public int EnquiryId { get; set; }
        public int VendorCode { get; set; }


        public string? VendorName { get; set; }
        public string? VendorAddress { get; set; }
        public string? VendorPhoneNo { get; set; }
        public string? VendorGSTNo { get; set; }
        public bool IsAssigned { get; set; }

        public int EnquirySubId { get; set; } 
        public decimal? Qty { get; set; }
        public decimal? BalQty { get; set; }
        public decimal? Rate { get; set; }
        public string? Remarks { get; set; }
        public byte ItemStatus { get; set; } = 1; //1-pending,2-Completed,3-canceled,4-short-Closed
    }
}
