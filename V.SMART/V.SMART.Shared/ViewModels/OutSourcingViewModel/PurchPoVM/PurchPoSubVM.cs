using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.OutSourcing;
using V.SMART.Shared.Services;
using V.SMART.Shared.Utility_Constants;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM
{
    [ScreenMapping("Purchase Order")]
    public class PurchPoSubVM : ICalculationDocumentSubItem
    {
        public int PoSubId { get; set; }
        public int PoId { get; set; }

        [Required(ErrorMessage = "Sl No is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No must be greater than zero.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select an item.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid item.")]
        public int? ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? HSNCode { get; set; }
        public string? MeasureUnit { get; set; }
        public int? CategoryCode { get; set; }

        [Precision(18, 3)]
        public decimal? ROL { get; set; }

        public int? RefQuoteSubId { get; set; }
        public string? RefQuoteNo { get; set; }
        public DateTime? RefQuoteDate { get; set; }

        public int? RefMReqSubId { get; set; }
        public string? RefMReqNo { get; set; }
        public DateTime? RefMReqDate { get; set; }

        // ===== Route Card Reference =====
        public int? RCId { get; set; }
        public int? RefRcSubId { get; set; }
        public string? RefRcNo { get; set; }
        public DateTime? RefRcDate { get; set; }

        public int? ProcessId { get; set; }
        public string? ProcessName { get; set; }

        [Precision(18, 3)]
        public decimal? Qty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Balance Quantity cannot be negative.")]
        public decimal? BalQty { get; set; }

        [Precision(18, 3)]
        [Required(ErrorMessage = "Unit Price is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Unit Price must be greater than zero.")]
        public decimal? UnitPrice { get; set; }

        [Precision(18, 3)]
        [Range(0, 100, ErrorMessage = "Discount % must be between 0 and 100.")]
        public decimal? LineDiscountPercent { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Discount Amount cannot be negative.")]
        public decimal? LineDiscountAmount { get; set; }

        [Precision(18, 3)]
        public decimal? LineCGSTRate { get; set; }

        [Precision(18, 3)]
        public decimal? LineSGSTRate { get; set; }

        [Precision(18, 3)]
        public decimal? LineIGSTRate { get; set; }

        // ===== DUE DATE =====
        [Required(ErrorMessage = "Due Date is required.")]
        public DateTime DueDate { get; set; } = DateTime.Now;

        // ===== ITEM CANCEL =====
        public bool ItemCancel { get; set; } = false;

        [StringLength(500, ErrorMessage = "Cancel Reason cannot exceed 500 characters.")]
        public string? ItemCancelReason { get; set; }

        // ===== COST / PROJECT =====
        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        // ===== REMARKS =====
        [StringLength(500, ErrorMessage = "Remark cannot exceed 500 characters.")]
        public string? Remark { get; set; }

        public bool IsEditable { get; set; } = false;

       
        public RouteCardVM? SelectedRouteCard { get; set; }

    
        public List<RouteCardVM> RouteCards { get; set; } = new();


        public bool CanEditItem => (Qty ?? 0) == (BalQty ?? 0) && (RefMReqSubId ?? 0) == 0 && (RefQuoteSubId ?? 0) == 0 && !ItemCancel;

        // ===== COMPUTED VALUES =====
        public decimal LineGross => (Qty ?? 0m) * (UnitPrice ?? 0m);
        public decimal LineBasicAmount => LineGross - (LineDiscountAmount ?? 0m);
        public decimal LineCGSTAmount => LineBasicAmount * (LineCGSTRate ?? 0m) / 100m;
        public decimal LineSGSTAmount => LineBasicAmount * (LineSGSTRate ?? 0m) / 100m;
        public decimal LineIGSTAmount => LineBasicAmount * (LineIGSTRate ?? 0m) / 100m;

        // ===== INTERFACE IMPLEMENTATION =====
        decimal ICalculationDocumentSubItem.Qty => Qty ?? 0m;
        decimal ICalculationDocumentSubItem.UnitPrice => UnitPrice ?? 0m;
        decimal ICalculationDocumentSubItem.LineDiscountPercent => LineDiscountPercent ?? 0m;
        decimal ICalculationDocumentSubItem.LineDiscountAmount => LineDiscountAmount ?? 0m;
        decimal ICalculationDocumentSubItem.LineCGSTRate => LineCGSTRate ?? 0m;
        decimal ICalculationDocumentSubItem.LineSGSTRate => LineSGSTRate ?? 0m;
        decimal ICalculationDocumentSubItem.LineIGSTRate => LineIGSTRate ?? 0m;
        decimal ICalculationDocumentSubItem.LineGross => LineGross;
        decimal ICalculationDocumentSubItem.LineBasicAmount => LineBasicAmount;
        decimal ICalculationDocumentSubItem.LineCGSTAmount => LineCGSTAmount;
        decimal ICalculationDocumentSubItem.LineSGSTAmount => LineSGSTAmount;
        decimal ICalculationDocumentSubItem.LineIGSTAmount => LineIGSTAmount;
    }
}
