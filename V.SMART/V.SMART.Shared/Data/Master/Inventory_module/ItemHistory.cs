using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.Inventory_module
{
    public class ItemHistory
    {
        [Key]
        public long HistoryId { get; set; }

        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? DrawingNo { get; set; }
        public string? Barcode { get; set; }
        public string? QRCode { get; set; }
        public string? BtOtorMfg { get; set; }
        public string? Remarks { get; set; }
        public int? CategoryCode { get; set; }
        public string? MeasureUnit { get; set; }
        public string? PurchaseUnit { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? UnitConvert { get; set; }

        public string? RackNo { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? ROL { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? MOL { get; set; }

        public int? LeadTime { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? PartWeight { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? TotalArea { get; set; }

        public string? BinLocation { get; set; }
        public bool? ConsiderAsRm { get; set; }
        public string? Make { get; set; }
        public int? RmId { get; set; }
        public string? Shape { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Density { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Length { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Width { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Height { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Thickness { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? WallThickness { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? OuterDia { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? InnerDia { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Weight { get; set; }

        public string? BatchNo { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? Rate { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? AltRate { get; set; }

        public string? HSNCode { get; set; }
        public string? SACCode { get; set; }
        public string? IsServcS { get; set; }
        public string? IsServcL { get; set; }

        [Column(TypeName = "decimal(5,3)")]
        public decimal? CGST { get; set; }

        [Column(TypeName = "decimal(5,3)")]
        public decimal? SGST { get; set; }

        [Column(TypeName = "decimal(5,3)")]
        public decimal? IGST { get; set; }

        public string? Rev { get; set; }
        public int? RevItemId { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? LabourPricePerHour { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? CycleTime { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? LabourTimeMinutes { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? LabourPrice { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? AssemblyPrice { get; set; }

        public bool? IsLabourItem { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal? HandlingCharges { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        public string? ActionType { get; set; }
        public DateTime? ActionDate { get; set; }

        public string? DeletedBy { get; set; }
        public bool? Hide { get; set; }
    }
}
