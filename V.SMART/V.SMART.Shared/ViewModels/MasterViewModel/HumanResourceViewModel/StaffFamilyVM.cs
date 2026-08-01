using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel
{
    public class StaffFamilyVM
    {
        public int Slno { get; set; }
        public int StaffID { get; set; }

        [Required(ErrorMessage = "Family member name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Relation is required")]
        [StringLength(50, ErrorMessage = "Relation cannot exceed 50 characters")]
        public string? Relation { get; set; }

        [StringLength(20, ErrorMessage = "Landline cannot exceed 20 characters")]
        public string? Landline { get; set; }

        [StringLength(20, ErrorMessage = "Mobile number cannot exceed 20 characters")]
        public string? Mobno { get; set; }
    }

}
