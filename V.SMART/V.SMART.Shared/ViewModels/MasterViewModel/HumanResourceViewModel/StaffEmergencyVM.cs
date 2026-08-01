using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel
{
    public class StaffEmergencyVM
    {
        public int SlNo { get; set; }
        public int StaffID { get; set; }

        [Required(ErrorMessage = "Contact name is required")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Relation is required")]
        [StringLength(50, ErrorMessage = "Relation cannot exceed 50 characters")]
        public string? Relation { get; set; }

        [StringLength(50, ErrorMessage = "Landline cannot exceed 50 characters")]
        public string? Landline { get; set; }

        [Required(ErrorMessage = "Mobile number is required")]
        [StringLength(50, ErrorMessage = "Mobile number cannot exceed 50 characters")]
        public string? MobileNo { get; set; }

        [StringLength(250, ErrorMessage = "Address cannot exceed 250 characters")]
        public string? Address { get; set; }
    }

}
