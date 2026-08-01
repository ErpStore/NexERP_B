using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ReportViewModel.TrakReportsViewModel.LabourTrackViewModel
{
    public class ItemModificationReportVM
    {
        public long SlNo { get; set; }

        public string? ItemCode { get; set; }

        public string? ItemName { get; set; }

        public string? CategoryName { get; set; }

        public string? MeasureUnit { get; set; }

        public string? HSNCode { get; set; }

        public string? SACCode { get; set; }

        public decimal? Rate { get; set; }

        public decimal? ROL { get; set; }

        public decimal? MOL { get; set; }

        public string? Specification { get; set; }

        public decimal? CGST { get; set; }

        public decimal? SGST { get; set; }

        public decimal? IGST { get; set; }

        public string? Remarks { get; set; }

        public string? DrawingNo { get; set; }

        public int? LeadTime { get; set; }

        public string? RackNo { get; set; }

        public decimal? PartWeight { get; set; }

        public decimal? TotalArea { get; set; }

        public string? Make { get; set; }

        public long? RmId { get; set; }

        public string? Shape { get; set; }

        public decimal? Density { get; set; }

        public decimal? Length { get; set; }

        public decimal? Width { get; set; }

        public decimal? Height { get; set; }

        public decimal? Thickness { get; set; }

        public decimal? WallThickness { get; set; }

        public decimal? OuterDia { get; set; }

        public decimal? InnerDia { get; set; }

        public decimal? Weight { get; set; }

        public string? BatchNo { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public string? DeletedBy { get; set; }

        public DateTime? ActionDate { get; set; }
    }
}

