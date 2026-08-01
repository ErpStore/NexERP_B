using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM
{
    using System.ComponentModel.DataAnnotations;

    public class EnquirySalesSubVM
    {
        public int EnquirySubId { get; set; }

        [Required(ErrorMessage = "Enquiry Id is required.")]
        public int EnquiryId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }

        // For UI binding (MudAutocomplete etc.)
        public ItemVM? SelectedItem { get; set; }

        [Required(ErrorMessage = "Item-Code is Required!..")]
        [Range(1, int.MaxValue, ErrorMessage = "Invalid Item selected.")]
        public int? ItemId { get; set; }

        [StringLength(250, ErrorMessage = "Item Code cannot exceed 250 characters.")]
        public string? ItemCode { get; set; }

        public string? ItemName { get; set; }

        [Required(ErrorMessage = "Unit of Measurement is required.")]
        [StringLength(20, ErrorMessage = "UOM cannot exceed 20 characters.")]
        public string UOM { get; set; } = string.Empty;

        public string? HsnCode { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal? Qty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance quantity cannot be negative.")]
        public decimal? BalQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit Price cannot be negative.")]
        public decimal UnitPrice { get; set; }

        [StringLength(250, ErrorMessage = "Remark should not exceed 250 characters.")]
        public string? Remark { get; set; }

        public int? CostCenterId { get; set; }

        public bool ItemCancel { get; set; }
        public string? ItemCancelReason { get; set; }

        public string? ProjectNo { get; set; }
        public bool IsEditable { get; set; } = false;
        public bool CanEditItem => (Qty ?? 0) == (BalQty ?? 0) && !ItemCancel;

        public bool IsEstiItemTally { get; set; }

    }

}
