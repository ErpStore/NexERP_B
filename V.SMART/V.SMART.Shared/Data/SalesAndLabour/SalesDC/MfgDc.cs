using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesDC
{
    public class MfgDc
    {
        [Key]
        public int DcId { get; set; }
        public int? CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }
        public bool IsSameAsShipping { get; set; }= true;
        public int? ShippingId { get; set; }

        [ForeignKey(nameof(ShippingId))]
        public virtual CustomerIndirect? Consignee { get; set; }

        [Required]
        public int? IssueStoreId { get; set; }
        [ForeignKey(nameof(IssueStoreId))]
        public virtual Store? Store { get; set; }


        public bool IsPurchaseReturn { get; set; } = false;
        public int? VendorCode { get; set; }
        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? Vendor { get; set; }
        public int? RefGRNId { get; set; }

        [ForeignKey(nameof(RefGRNId))]
        public string? RefGRNNo { get; set; }

        [Required]
        public int? ModeofTrasnport { get; set; } =1;

        [Required(ErrorMessage = "Vehicle No is required.")]
        [RegularExpression(@"^[A-Z]{2}[0-9]{1,2}[A-Z]{1,2}[0-9]{4}$", ErrorMessage = "Invalid Vehicle No (e.g., KA01AB1234).")]
        public string VehicleNo { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TransFrom { get; set; }

        [StringLength(50)]
        public string? TransTo { get; set; }

        [StringLength(100)]
        public string? TransId { get; set; }

        [StringLength(100)]
        public string? TransDocNo { get; set; }

        [StringLength(100)]
        public string? TransName { get; set; }

        [StringLength(20)]
        public int? SubSuppType { get; set; } = 1;
        [StringLength(100)]
        public string? SubSupplyDesc { get; set; }

        public string? EwayBillNo { get; set; }
        public DateTime? EwayBillDate { get; set; }


        public bool IsWithoutPoDc { get; set; } = false;

        public bool DcShortClose { get; set; } = false;

        [StringLength(10)]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "DC Number is required.")]
        [StringLength(15)]
        public string DcNo { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "DC Date is required.")]
        public DateTime DcDate { get; set; } = DateTime.Now;
        public DateTime DcDateNow { get; set; } = DateTime.Now;

        [StringLength(300)]
        public string? Towards { get; set; }

        [StringLength(500)]
        public string? MainRemarks { get; set; }

        [StringLength(200)]
        public string? RejRemark { get; set; }

        public bool DcCancel { get; set; }= false;
        [StringLength(120, ErrorMessage = "Cancel Reason cannot exceed 120 characters.")]
        public string? CancelReason { get; set; }

        public bool DcTally {  get; set; }= false;

        public bool ReduceInvAsPerRej { get; set; } = false;

        public bool? IsManualDcNo { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<MfgDcSub> MfgDcSubs { get; set; } = new List<MfgDcSub>();
    }
}
