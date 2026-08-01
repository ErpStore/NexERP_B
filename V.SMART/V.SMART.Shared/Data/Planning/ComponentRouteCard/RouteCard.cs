

using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Planning.ComponentRouteCard
{
    public class RouteCard
    {
        [Key]
        public int RCId { get; set; }
        public bool MfgOrLab { get; set; } = true;
        public bool IsManual { get; set; } = false;

        [Required]
        public string RCNo { get; set; }

        [Required]
        public string Suffix { get; set; }

        [Required]
        public DateTime RCDate { get; set; } = DateTime.Now;

        public DateTime? RCDateNow { get; set; } = DateTime.Now;

        [Required]
        // Qty
        public decimal RcQty { get; set; }

        [Required]
        public byte RcType { get; set; } = 1;  // 1=Auto, 2=Manual
        public byte RcStatus { get; set; } = 0;  // 0=Pending 1=InProgress 2=Completed 3=Cancelled

        public int? CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }

        public int? RefPoId { get; set; }
        [ForeignKey(nameof(RefPoId))]
        public virtual MfgPo? MfgPo { get; set; }

        public int? RefRcReleseId { get; set; }
        [ForeignKey(nameof(RefRcReleseId))]
        public virtual RouteCardRelease? RcRelease { get; set; }

        public string? SelectedItemType { get; set; } = string.Empty;

        [Required]
        public int? CompItemId { get; set; }
        [ForeignKey(nameof(CompItemId))]
        public virtual Item? ComponentItem { get; set; }

        public int? AssyId { get; set; }
        [ForeignKey(nameof(AssyId))]
        public virtual Item? AssemblyItem { get; set; }

        public int? SubAssyId { get; set; }
        [ForeignKey(nameof(SubAssyId))]
        public virtual Item? SubAssemblyItem { get; set; }

        public int? SubAssyId2 { get; set; }
        [ForeignKey(nameof(SubAssyId2))]
        public virtual Item? SubAssemblyItem2 { get; set; }

        public int? SubAssyId3 { get; set; }
        [ForeignKey(nameof(SubAssyId3))]
        public virtual Item? SubAssemblyItem3 { get; set; }

        public int? RMId { get; set; }
        [ForeignKey(nameof(RMId))]
        public virtual Item? RawMaterialItem { get; set; }

        [Precision(18, 3)]
        public decimal RMReqQty { get; set; }

        [Precision(18, 3)]
        public decimal RMWeight { get; set; }
        public string? RMBatchNo { get; set; }

        public string? MainRemarks { get; set; }

        // Stores
        public int? IssueStoreId { get; set; }
        [ForeignKey(nameof(IssueStoreId))]
        public virtual Store? IssueStore { get; set; }

        public int? AddStoreId { get; set; }
        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? AddStore { get; set; }

        // Media
        public byte[]? BarCodeImage { get; set; }
        public byte[]? QrCodeImage { get; set; }

        //Cost-Center
        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }

        // Audit
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<RouteCardSub> RouteCardSubs { get; set; } = new List<RouteCardSub>();
    }

}
