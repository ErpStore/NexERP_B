using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel
{
    public class GroupingSubVM
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "Serial Number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Serial Number must be greater than 0.")]
        public int? SlNo { get; set; }

        [Required(ErrorMessage = "Factor is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Factor.")]
        public int? Factors { get; set; }

        [StringLength(250, ErrorMessage = "Factor Name cannot exceed 250 characters.")]
        public string? FactorName { get; set; }

        [Required(ErrorMessage = "Process is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Process.")]
        public int? Processid { get; set; }

        [StringLength(250, ErrorMessage = "Process Name cannot exceed 250 characters.")]
        public string? ProcessName { get; set; }
    }
}
