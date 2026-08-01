using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM
{
    public class EnquiryPurchaseSubVM
    {
        public int EnquirySubId { get; set; }
        public int EnquiryId { get; set; }

        [Required(ErrorMessage = "Sl. No. is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl. No. must be greater than zero.")]
        public int SlNo { get; set; }

        public ItemVM? SelectedItem { get; set; }

        [Required(ErrorMessage = "Please select an Item.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Item.")]
        public int? ItemId { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? UOM { get; set; }
        public string? HsnCode { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than zero.")]
        public decimal? Qty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance Quantity cannot be negative.")]
        public decimal? BalQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Unit Price cannot be negative.")]
        public decimal UnitPrice { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? Remark { get; set; }

        public string? RefMReqNo { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RefMReqDate { get; set; }

        public int? RefMReqSubId { get; set; }

        [Required(ErrorMessage = "Due Date is required.")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; } = DateTime.Now;

        public int? CostCenterId { get; set; }
        public string? ProjectNo { get; set; }
        public bool IsEditable { get; set; } = false;

        public bool CanEditItem =>
            (Qty ?? 0) == (BalQty ?? 0) &&
            (RefMReqSubId ?? 0) == 0 &&
            !ItemCancel;

        public bool ItemCancel { get; set; } = false;

        [StringLength(500, ErrorMessage = "Cancel Reason cannot exceed 500 characters.")]
        public string? ItemCancelReason { get; set; }
    }


}
