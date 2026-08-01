using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel
{
    public class CurrencyVM
    {
        public int CurrId { get; set; }

        [Required(ErrorMessage = "Please Enter Currency Name")]
        [StringLength(100, ErrorMessage = "Currency Name cannot exceed 100 characters")]
        public string? CurrName { get; set; }

        [Required(ErrorMessage = "Please Enter Sub Currency Name")]
        [StringLength(100, ErrorMessage = "Sub Currency Name cannot exceed 100 characters")]
        public string? CurrSub { get; set; }

        [Required(ErrorMessage = "Please Enter Symbol")]
        [RegularExpression(@"[$€₹¥£₩₿₽]",
            ErrorMessage = "Only valid currency symbols are allowed (₹, $, €, £, ¥, ₩, ₿, ₽)")]
        public string? Symbol { get; set; }

        public bool IsSystemDefined { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
