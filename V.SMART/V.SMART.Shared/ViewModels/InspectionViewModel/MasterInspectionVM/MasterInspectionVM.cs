using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM
{
    public class MasterInspectionVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Please Select the Insepection Number")]
        public string? InspectNo { get; set; }

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(50, ErrorMessage = "Suffix cannot exceed 50 characters.")]
        public string Suffix { get; set; } = string.Empty;


        public DateTime? InspectDate { get; set; }


        public int? processId { get; set; }
        public string? ProcessName { get; set; }

        public ItemVM? SelectedItem { get; set; }

        [Required(ErrorMessage = "Please Select the Item")]
        public int? ItemId { get; set; }

        [Required(ErrorMessage = "Please Enter the Qty")]
        [Range(0, double.MaxValue, ErrorMessage = "Quantity must be non-negative.")]
        public float? Qty { get; set; }

        public string? InspectData { get; set; }


        [StringLength(250)]
        [Required(ErrorMessage = "Please Select Inspected By")]
        public String? StaffName { get; set; }

        public int? TotRows { get; set; }

        public string? Remarks { get; set; } = string.Empty;

        [StringLength(40)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(40)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }


        //Display Purpose

        public string? ItemName { get; set; }


        [Required(ErrorMessage = "Please Select the Item Code")]
        public string? ItemCode { get; set; }

    }
}
