using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.LabourDC
{
    public class LabourDcOutgoing
    {

        [Key]
        public int DcId { get; set; }

        [Required(ErrorMessage = "Customer is required.")]
        public int CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }

        [StringLength(10)]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Dc No is required.")]
        [MaxLength(20)]
        public string DcNo { get; set; }

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(10)]
        public string? Suffix { get; set; }

        [Required(ErrorMessage = "Dc Date is required.")]
        public DateTime DcDate { get; set; }=DateTime.Now;
        public DateTime DcDateNow { get; set; } = DateTime.Now;


        [Required(ErrorMessage = "Mode of Transport is required.")]
        public int? TransportMode { get; set; }   

        [StringLength(20)]
        public string? VehicleNo { get; set; }=string.Empty;

        [StringLength(50)]
        public string? TransFrom { get; set; }

        [StringLength(50)]
        public string? TransTo { get; set; }

        [Range(0, int.MaxValue)]
        public int NoOfItems { get; set; }

        public bool DcCancel { get; set; } = false;

        public bool DcTally { get; set; }=false;    

        [StringLength(250)]
        public string? Remarks { get; set; }

        [Required(ErrorMessage = "StoreIssue is required.")]
        public int? StoreIssId { get; set; }
        [ForeignKey(nameof(StoreIssId))]
        public virtual Store? Store { get; set; }

        [StringLength(20)]
        public string? EwayNo { get; set; }

        public DateTime? EwayDate { get; set; }

        public bool DcSign { get; set; } = false;

        public bool NonReturnDc { get; set; } = false;

        public bool ReceivedReturnRM { get; set; } = false;
        public bool SCNRejectReworkReturn { get; set; } = false;

        [StringLength(30)]
        public string? TransId { get; set; }

        [StringLength(100)]
        public string? TransName { get; set; }

        [StringLength(30)]
        public string? TransDocNo { get; set; }

        public int? SubSupplyType { get; set; }

        [StringLength(100)]
        public string? SubSupplyDesc { get; set; }

        [MaxLength(100)]
        public string? KindofAttention { get; set; }

        [MaxLength(150)]
        public string? CancelReason { get; set; }

        public bool IsSameAsShipping { get; set; } = true;
        public int? ShippingId { get; set; }

        [ForeignKey(nameof(ShippingId))]
        public virtual CustomerIndirect? Consignee { get; set; }

        public bool WithoutPoDc { get; set; } = false;

        public bool Manual { get; set; } = false;

        public bool ShortClose { get; set; } = false;

        public DateTime? CanceledDate { get; set; }

        [MaxLength(250)]
        public string? RejectionReason { get; set; }
        public bool ReduceInvAsPerRej { get; set; }

        public bool? IsManualDcNo { get; set; } = false;

        [MaxLength(50)]
        public string CanceledBy { get; set; } = string.Empty;

        [MaxLength(50)]
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }= DateTime.Now;

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<LabourDcOutgoingSub> LabourDcOutgoingSubs { get; set; } = new List<LabourDcOutgoingSub>();
        public virtual ICollection<LabourDcReturnCompTrack> LabourDcReturnCompTracks { get; set; } = new List<LabourDcReturnCompTrack>();
    }


}
