using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.Data.SalesAndLabour.Labour_SCN;
using V.SMART.Shared.Data.SalesAndLabour.LabourGRN;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.LabourDC
{
    public class LabourDcOutgoingSub
    {
        [Key]
        public int DcSubId { get; set; }

        [Required]
        public int DcId { get; set; }
        [ForeignKey(nameof(DcId))]
        public LabourDcOutgoing? LabourDcOutgoing { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required]
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; } = null;

        [Range(0.001, double.MaxValue, ErrorMessage = "Qty must be greater than zero.")]
        public decimal Qty { get; set; }

        public decimal BalQty { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        public decimal UnitPrice { get; set; }

        public decimal? RejectQty { get; set; }
        public decimal? ReworkQty { get; set; }

        [StringLength(250)]
        public string? RejectRowRem { get; set; }
        public decimal? AltQty { get; set; }

        [StringLength(250)]
        public string? DcRowRem { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

        public bool ItemCancel { get; set; }

        public string? CancelItemReason { get; set; }

        
        public int? RefSCNSubId { get; set; }
        [ForeignKey(nameof(RefSCNSubId))]
        public LabourSCNSub? LabourSCNSub { get; set; }

        public int? RefGRNSubId { get; set; }
        [ForeignKey(nameof(RefGRNSubId))]
        public LabourGRNSub? LabourGRNSub { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public MfgPoSub? MfgPoSub { get; set; }

        [StringLength(200)]
        public string? RcSubIds { get; set; }

        [StringLength(30)]
        public string? BatchNo { get; set; }

        [StringLength(30)]
        public string? WONo { get; set; }

        [StringLength(30)]
        public string? HeatNo { get; set; }

        [StringLength(50)]
        public string? RCNos { get; set; }


        [Required]
        public string TransType { get; set; } = "Out";

        // [StringLength(50)]
        // public string? TypeOfPo { get; set; }


        // ✅ Navigation Property for Return Component Track
        public ICollection<LabourDcReturnCompTrack>? LabourDcReturnCompTracks { get; set; }

    }


}
