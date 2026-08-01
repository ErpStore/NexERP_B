using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel
{
    public class IncomeVM
    {
        public int IncomeCode { get; set; }

        [Required(ErrorMessage = "Income Name is required.")]
        [StringLength(250, ErrorMessage = "Income Name cannot exceed 250 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s\-_.()]*$",
            ErrorMessage = "Income Name can only contain letters, numbers, spaces, and basic symbols (- _ . ( )).")]
        public string IncomeName { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters.")]
        public string? IncomeDesc { get; set; }

        [Required(ErrorMessage = "Please specify whether the income is direct or indirect.")]
        public bool IsDirect { get; set; }

        // Computed Property (For Grid Display)
        public string IncomeType => IsDirect ? "Direct" : "Indirect";

        [StringLength(250, ErrorMessage = "CreatedBy cannot exceed 250 characters.")]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;

        [StringLength(250, ErrorMessage = "ModifiedBy cannot exceed 250 characters.")]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }

}
