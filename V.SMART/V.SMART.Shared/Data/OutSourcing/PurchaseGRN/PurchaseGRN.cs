using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.Icons;

namespace V.SMART.Shared.Data.OutSourcing.PurchaseGRN
{
    public class PurchaseGRN
    {
        [Key]
        public int GRNId { get; set; }

        [Required]
        [StringLength(10)]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "GRN Number is required?,GRN No. should not be empty")]
        [StringLength(15)]
        public string GRNNo { get; set; }=string.Empty;

        public DateTime GRNDate { get; set; }= DateTime.Now;
        public DateTime GRNDateNow { get; set; } = DateTime.Now;

        public int? VendorCode { get; set; }
        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? Vendor { get; set; }

       
        [StringLength(50)]
        public string? RefDcNo { get; set; }= string.Empty;
        public DateTime? RefDcDate { get; set; }= DateTime.Now;

        [StringLength(50)]
        public string? RefInvNo { get; set; } = string.Empty;
        public DateTime? RefInvDate { get; set; } = DateTime.Now;

        [StringLength(250)]
        public string? MainRemarks { get; set; }

        [Required(ErrorMessage = "Please select a store to add.")]
        public int? AddStoreId { get; set; }
        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? Store { get; set; }

        public int? NoOfItems { get; set; }

        public bool GRNCancel { get; set; } = false;
        public bool ShortClose { get; set; }
        public bool GRNTally { get; set; } = false;

        public bool IsWithoutPoGRN { get; set; }

        [StringLength(80)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
        [StringLength(80)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public bool IsWithoutInspection { get; set; } = false;
        public string? KindofAttention { get; set; }

        public string? CancelReason { get; set; }
        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }

        public virtual ICollection<PurchaseGRNSub> PurchaseGRNSubs { get; set; } = new List<PurchaseGRNSub>();
    }
}
