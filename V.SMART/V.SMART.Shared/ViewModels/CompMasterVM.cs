using DocumentFormat.OpenXml.Office.CoverPageProps;
using V.SMART.Shared.Data.Master.Inventory_module;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels
{
    public class CompMasterVM
    {
        public int Id { get; set; }

        // Component Item
        public int? CompItemId { get; set; }
        public string? CompItemCode { get; set; }
        public string? CompItemName { get; set; }

        // Raw Material
        [Required(ErrorMessage = "Please select Raw-Material")]
        public int? RMId { get; set; }
        public string? RMCode { get; set; }
        public string? RMName { get; set; }

        public string? MeasureUnit { get; set; }

        public ItemVM? SelectedRawMaterialVM { get; set; }

        // Weight
        [Required(ErrorMessage = "Raw-Material Weight is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Weight must be greater than zero.")]
        public decimal Weight { get; set; }

        // Other Details
        [StringLength(200, ErrorMessage = "Other Details cannot exceed 200 characters.")]
        public string? OtherDet { get; set; }


        // Flags
        public bool LabWORM { get; set; } = false;
        public bool isGroupingEnabled { get; set; } = false;
        public int? selectedFactorId { get; set; }
        public int? SelectedGroupingId {get; set;}


        //Previous Process Selection Wise Load
        public int? PrevProcessFlowRMId { get; set; }
        public int? PrevProcessFlowRMCode { get; set; }


        public bool IsDefaultRM { get; set; }

        [Precision(18, 3)]
        public decimal EBLength { get; set; }

        [Precision(18, 3)]
        public decimal EBWidth { get; set; }

        public decimal RMCuttingSize { get; set; }


        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [ValidateComplexType]
        public List<ProcessFlowChartVM> ProcessFlowCharts { get; set; } = new();
    }


}
