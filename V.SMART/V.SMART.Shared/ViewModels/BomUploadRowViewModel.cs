namespace V.SMART.Shared.ViewModels
{
    public class BomUploadRowViewModel
    {
        public int SlNo { get; set; }

        public string? MainAssembly { get; set; }
        public string? SubAssembly1 { get; set; }
        public string? SubAssembly2 { get; set; }
        public string? SubAssembly3 { get; set; }

        public string? Category { get; set; }

        public string? PartNumber { get; set; }
        public string? PartName { get; set; }
        public string? Specification { get; set; }

        public decimal? Qty { get; set; }
        public string? UOM { get; set; }

        public decimal? Rate { get; set; }
        public decimal? ROL { get; set; }

        public string? HSNCode { get; set; }
        public string? SACCode { get; set; }

        public string? ItemType { get; set; }
        public string? Shape { get; set; }
        public string? Material { get; set; }
        public string? MaterialGrade { get; set; }

        public decimal? Density { get; set; }

        // Dimensional Fields
        public decimal? ODLengthComp { get; set; }
        public decimal? IDWidthComp { get; set; }
        public decimal? ThicknessLengthHeightComp { get; set; }
        public decimal? WallThicknessComp { get; set; }

        public decimal? ODLength_RM { get; set; }
        public decimal? IDWidth_RM { get; set; }
        public decimal? ThicknessLengthHeight_RM { get; set; }
        public decimal? WallThickness_RM { get; set; }

        public string? RMUOM { get; set; }
        public decimal? RMQty { get; set; }
        public decimal? RMWt { get; set; }

        public string? Remarks { get; set; }

    }

}
