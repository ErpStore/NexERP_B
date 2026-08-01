using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM
{
    public class InspectionRowVM
    {
        public Guid RowId { get; set; } = Guid.NewGuid();
        public int SlNo { get; set; }
        public string? Desc { get; set; }
        public Decimal? Dimens { get; set; }
        public decimal? LoTol { get; set; }
        public decimal? HiTol { get; set; }
        public String? Comp1{ get; set; }
        public String? Comp2 { get; set; }
        public String? Comp3 { get; set; }
        public String? Comp4 { get; set; }
        public String? Comp5 { get; set; }
        public String? Comp6 { get; set; }
        public String? Comp7 { get; set; }
        public String? Comp8 { get; set; }
        public String? Comp9 { get; set; }
        public String? Comp10 { get; set; }
        public String? Comp11 { get; set; }
        public String? Comp12 { get; set; }
        public String? Comp13 { get; set; }
        public String? Comp14 { get; set; }
        public String? Comp15 { get; set; }
        public string? Instrument { get; set; }
        public string? DefectName { get; set; }

        public string? Remark { get; set; } = string.Empty;
        public bool IsEditable { get; set; }
    }

}
