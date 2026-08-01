using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel
{
    public class StaffEducationVM
    {
        public int Slno { get; set; }
        public int StaffID { get; set; }

        [Required(ErrorMessage = "Degree is required")]
        [StringLength(50, ErrorMessage = "Degree cannot exceed 50 characters")]
        public string? Degree { get; set; }

        [Required(ErrorMessage = "Institution is required")]
        [StringLength(150, ErrorMessage = "Institution cannot exceed 150 characters")]
        public string? Institution { get; set; }

        [Required(ErrorMessage = "From year is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Enter valid 4 digit year")]
        public string? FromYear { get; set; }

        [Required(ErrorMessage = "To year is required")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "Enter valid 4 digit year")]
        public string? ToYear { get; set; }
    }

}
