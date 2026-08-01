using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.LabourGRN
{
    public class LabourGRN
    {
        [Key]
        public int GRNId { get; set; }
        public bool IsOutItemSameAsInItem { get; set; } = false;

        [Required(ErrorMessage = "Customer is required.")]
        public int CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }

        [Required(ErrorMessage = "GRN No is required.")]
        [MaxLength(30)]
        public string GRNNo { get; set; } = string.Empty;


        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(10)]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "GRN Date is required.")]
        public DateTime GRNDate { get; set; }= DateTime.Now;


        public DateTime GRNDateNow { get; set; } = DateTime.Now;


        [Required(ErrorMessage = "Please eneter customer DcNo first.")]
        public string? RefDcNo { get; set; }

        public DateTime? RefDcDate { get; set; }=DateTime.Now;

        
        public bool DcTally { get; set; } = false;

        public bool DcCancel { get; set; } = false;
        public DateTime? CancelDate { get; set; }

        public bool ShortClose { get; set; } = false;


        [Required(ErrorMessage = "Add Store is required.")]
        public int AddStoreId { get; set; }
        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? Store { get; set; }

        
        public int? NoOfItems { get; set; }

        
        [MaxLength(30)]
        public string? TransportMode { get; set; }

        [MaxLength(20)]
        public string? VehicleNo { get; set; }

        [MaxLength(100)]
        public string? TransFrom { get; set; }

        [MaxLength(100)]
        public string? TransTo { get; set; }

        
        [MaxLength(200)]
        public string? Remarks { get; set; }

        [MaxLength(100)]
        public string? KindofAttention { get; set; }

        [MaxLength(150)]
        public string? CancelReason { get; set; }

        [StringLength(150)]
        public string? CancelBy { get; set; }


        [Required]
        [MaxLength(50)]
        public string CreatedBy { get; set; } = string.Empty;

        [Required]
        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool WithoutPoGRN { get; set; } = false;

        public string? BatchNo { get; set; }

        public virtual ICollection<LabourGRNSub> LabourGRNSubs { get; set; } = new List<LabourGRNSub>();
    }
}