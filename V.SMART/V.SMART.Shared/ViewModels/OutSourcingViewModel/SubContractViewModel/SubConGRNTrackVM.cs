using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConGRNTrackVM
    {
        public int TrackId { get; set; }

        [Required(ErrorMessage = "GRN reference is required.")]
        public int RefGRNSubId { get; set; }
        public int GRNId { get; set; }
        public string? GRNNo { get; set; }

        public int? RefRcSubId { get; set; }

        public string? RefRcNo { get; set; }

        public int? RefDCSubId { get; set; }
        public string? DCNo { get; set; }

        [Required(ErrorMessage = "Incoming item is required.")]
        public int? ItemIdIn { get; set; }
        public string? InItemCode { get; set; }
        public string? InItemName { get; set; }
        public string? InUOM { get; set; }

        [Required(ErrorMessage = "Incoming quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Incoming quantity must be greater than zero.")]
        public decimal QtyIn { get; set; }

        public int? ItemIdOut { get; set; }
        public string? OutItemCode { get; set; }
        public string? OutItemName { get; set; }
        public string? OutUOM { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Outgoing quantity cannot be negative.")]
        public decimal? QtyOut { get; set; }

        public bool IsMatched => QtyOut.HasValue && QtyOut > 0;
        public bool IsExcess => QtyIn > (QtyOut ?? QtyIn);
        public bool IsShort => QtyOut.HasValue && QtyIn < QtyOut;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public bool IsEditable { get; set; } = false;
        public bool IsSelected { get; set; }
    }
}
