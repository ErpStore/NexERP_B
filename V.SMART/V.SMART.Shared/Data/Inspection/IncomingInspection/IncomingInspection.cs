using DocumentFormat.OpenXml.Office.CustomUI;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Item = V.SMART.Shared.Data.Master.Inventory.Item;


namespace V.SMART.Shared.Data.Inspection.IncomingInspection
{
    public class IncomingInspection
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Suffix { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? InspectNo { get; set; }

        public DateTime? InspectDate { get; set; }

        public String? DcType { get; set; }

        public String Grnno { get; set; } = String.Empty;

        [MaxLength(250)]
        public string? DCNo { get; set; }

        // 🔹 Mandatory FK
        public long? ProductionLogId { get; set; }
        [ForeignKey(nameof(ProductionLogId))]
        public ProductionLog? ProductionLog { get; set; } = null!;

        //Purchase Grn
        public int? RefPurchaseGRNSubId { get; set; }
        [ForeignKey(nameof(RefPurchaseGRNSubId))]
        public PurchaseGRNSub? PurchaseGRNSub { get; set; } = null!;

        //Production Return Assy Grn
        public int? RefProductionAssyGRNSubId { get; set; }
        [ForeignKey(nameof(RefProductionAssyGRNSubId))]
        public ProductionReturnAssySub? ProductionReturnSub { get; set; } = null!;


        //Production Return Comp Grn
        public int? RefProductionCompGRNSubId { get; set; }
        [ForeignKey(nameof(RefProductionCompGRNSubId))]
        public ProductionReturnCompSub? ProductionReturnCompSub { get; set; } = null!;


        [MaxLength(50)]
        public DateTime? DcDate { get; set; }

        public Decimal? Qty { get; set; } = 0;
        public Decimal? BalQty { get; set; } = 0;
        public Decimal? AcceptQty { get; set; } = 0;
        public Decimal? RejQty { get; set; } = 0;
        public Decimal? ReWorkQty { get; set; } = 0;

        [MaxLength(250)]
        public int? SuppId { get; set; }

        public int ItemId { get; set; } = 0;
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        //Random or Individual
        [Required(ErrorMessage = "Screen type is required")]
        [Display(Name = "Is Random Inspection?")]
        public bool IsRandom { get; set; }

        //Inspected By
        [StringLength(250, ErrorMessage = "Inspected By cannot exceed 250 characters")]
        [Display(Name = "Inspected By")]
        public String? InspectBy { get; set; }


        [MaxLength(250)]
        public string? Remarks { get; set; }

        public string? InspectionData { get; set; }

        public int TotRows { get; set; } = 0;

        public string? CostCenter { get; set; } =String.Empty;

        public DateTime? InsDateNow { get; set; }

        public DateTime? NCrDate { get; set; }

        [MaxLength(250)]
        public string? NCRNo { get; set; }

        public Decimal NCRQty { get; set; } = 0;

        [MaxLength(250)]
        public string? BatchNo { get; set; }

        public int? SheetSlNo { get; set; }

        [MaxLength(40)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [MaxLength(40)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public int? ProcessId { get; set; }
        [ForeignKey(nameof(ProcessId))]
        public virtual Process? process { get; set; }


        [Required]
        public bool NoDimensionEntry { get; set; } = false;

    }
}
