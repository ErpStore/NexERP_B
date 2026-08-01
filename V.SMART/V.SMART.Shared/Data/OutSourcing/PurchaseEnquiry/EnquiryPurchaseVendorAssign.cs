using V.SMART.Shared.Data.Master.General;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry
{
    public class EnquiryPurchaseVendorAssign
    {
        [Key]
        public int EnqPurchVendorId { get; set; }

        public int? EnquiryId { get; set; }
        [ForeignKey(nameof(EnquiryId))]
        public virtual EnquiryPurchase Enquiry { get; set; }

        public int? VendorCode { get; set; }
        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor Vendor { get; set; }

        public int? EnquirySubId { get; set; }

        [ForeignKey(nameof(EnquirySubId))]
        public virtual EnquiryPurchaseSub EnquiryPurchaseSub { get; set; }

        public decimal Qty { get; set; }
        public decimal BalQty { get; set; }
        public decimal? Rate { get; set; }
        public string? Remarks { get; set; }
        public byte ItemStatus { get; set; } = 1; //1-pending,2-Completed,3-canceled,4-short-Closed
    }

}
