using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.SubContractDC
{
    public class SubConDcOut
    {
        [Key]
        public int DcId { get; set; }

        [StringLength(20)]
        public string? Prefix { get; set; }

        [Required, StringLength(50)]
        public string DcNo { get; set; } = string.Empty;

        [StringLength(25)]
        public string? Suffix { get; set; }

        [Required]
        public DateTime DcDate { get; set; }

        public DateTime DcDateNow { get; set; } = DateTime.Now;

        // ===== Store =====
        [Required]
        public int StoreIssId { get; set; }

        [ForeignKey(nameof(StoreIssId))]
        public virtual Store StoreIssue { get; set; } = null!;

        public int NoOfItems { get; set; }

        [Precision(18, 3)]
        public decimal Qty { get; set; }

        [StringLength(300)]
        public string? MainRemark { get; set; }

        public bool DcTally { get; set; }

        // ===== Vendor =====
        public int? VendorCode { get; set; }

        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? Vendor { get; set; }

        // ===== Shipping =====
        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }

        [ForeignKey(nameof(ShipAddrsId))]
        public virtual VendorInDirect? Consignee { get; set; }

        // ===== Transport =====
        [StringLength(13)]
        public string? VehicleNo { get; set; }

        public int? ModeOfTransportId { get; set; }

        [StringLength(25)]
        public string? TransFrom { get; set; }

        [StringLength(25)]
        public string? TransTo { get; set; }

        // ===== Sub Supply =====
        public int? SubSupplyType { get; set; }

        [StringLength(200)]
        public string? SubSupplyDesc { get; set; }

        // ===== EWay =====
        [StringLength(20)]
        public string? EwayTransId { get; set; }

        [StringLength(100)]
        public string? EwayTransName { get; set; }

        [StringLength(15)]
        public string? EwayTransDocNo { get; set; }

        public DateTime? EwayBillDate { get; set; }

        [StringLength(150)]
        public string? EwayBillNumber { get; set; }

        public double? DcVal { get; set; }

        [StringLength(250)]
        public string? MainRemarks { get; set; }

        public bool DcSign { get; set; }
        public bool Cancel { get; set; }

        [StringLength(250)]
        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }
        public bool ShortClose { get; set; }



        // ===== Flags =====
        public bool IsVendor { get; set; }
        public bool DiffItemsOutIn { get; set; }
        public bool IsManualMaterialOut { get; set; }
        public bool IsWithoutPoDc { get; set; }
        public bool IsPurchaseRework { get; set; }

        // ===== Contact =====
        [StringLength(50)]
        public string? ContactPerson { get; set; }

        [StringLength(13)]
        public string? ContactNumber { get; set; }

        public bool IsOutItemSameAsInItem { get; set; } = false;

        // ===== Audit =====
        [Required, StringLength(100)]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<SubConDcOutSub> SubConDcOutSubs { get; set; } = new List<SubConDcOutSub>();
    }


}
