using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel
{
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;

    public class JobOrderSubVM
    {
        public int JobSubId { get; set; }
        public int JobId { get; set; }

        [Required(ErrorMessage = "Sl No is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No must be greater than 0.")]
        public int SlNo { get; set; }

        public ItemVM? SelectedItem { get; set; }

        [Required(ErrorMessage = "Item selection is required.")]
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Measureunit { get; set; }

        [Required(ErrorMessage = "Utilized quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Util qty must be greater than zero.")]
        public decimal UtilQty { get; set; }

        [Required(ErrorMessage = "Required quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Required qty must be greater than zero.")]
        public decimal? ReqQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance qty cannot be negative.")]
        public decimal? BalQty { get; set; }

        [Precision(18,3)]
        public decimal stockQty { get; set; }

        public bool ItemCancel { get; set; }

        [MaxLength(300, ErrorMessage = "Item cancel reason cannot exceed 300 characters.")]
        public string? ItemCancelReason { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }
    }

}
