using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.MaterialRequisitionViewModel
{
    public class MaterialReqSubVM
    {
        public int MreqSubId { get; set; }
        public int MreqId { get; set; }

        [Required(ErrorMessage = "Sl.No is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl.No must be greater than zero.")]
        public int SlNo { get; set; }

        public int? CustId { get; set; }

        public string? CustomerName { get; set; }

        public CustomerVM? SelectedCustomerVM { get; set; }

        [Required(ErrorMessage = "Item is required.")]
        public int? ItemId { get; set; }

        public ItemVM? SelectedItemVM { get; set; }

        public string? ItemCode { get; set; }

        public string? ItemName { get; set; }

        public string? UOM { get; set; }

        public string? CategoryName { get; set; }


        [Required(ErrorMessage = "Purchase quantity is mandatory.")]
        [Range(0, double.MaxValue, ErrorMessage = "Purchase quantity cannot be negative.")]
        public decimal? PurchQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Balance quantity cannot be negative.")]
        public decimal? BalQty { get; set; }
        public decimal? POBalQty { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Stock quantity cannot be negative.")]
        public decimal StockQty { get; set; }

        [Required(ErrorMessage = "Unit Price is required.")]
        [Range(0, double.MaxValue, ErrorMessage = "Unit price cannot be negative.")]
        public decimal? UnitPrice { get; set; }

        [Required(ErrorMessage = "Due date is required.")]
        public DateTime DueDate { get; set; } = DateTime.Now;

        public bool ItemCancel { get; set; }

        [StringLength(250, ErrorMessage = "Cancel reason cannot exceed 250 characters.")]
        public string? ItemCancelReason { get; set; }

        public int? VendorCode { get; set; }

        public string? VendorName { get; set; }

        public VendorVM? SelectedVendorVM { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public int? RefPoSubId { get; set; }
        public int? RefPoId { get; set; }

        public string? RefPoNo { get; set; }

        public DateTime? RefPoDate { get; set; }

        public decimal TotalGross => PurchQty.GetValueOrDefault() * UnitPrice.GetValueOrDefault();

        // Remarks
        [StringLength(300, ErrorMessage = "Remark cannot exceed 300 characters.")]
        public string? Remark { get; set; }

        // BOM / Assembly Reference
        public int? MainAssyId { get; set; }
        public int? RefBomId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Indent quantity cannot be negative.")]
        public decimal MfgPoIndentQty { get; set; }

        public bool IsEditable { get; set; }

        // Business Rule Validation (ERP Logic)
        public bool CanEditItem =>
            (BalQty ?? 0) == (PurchQty ?? 0) &&
            (RefPoSubId ?? 0) == 0 &&
            (RefPoId ?? 0) == 0 &&
            (MainAssyId ?? 0) == 0 &&
            (RefBomId ?? 0) == 0 &&
            !ItemCancel;
    }


}
