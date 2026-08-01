using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConDcOutSubVM
    {
        public int DcSubId { get; set; }

        [Required(ErrorMessage = "Serial number is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }

        public int DcId { get; set; }

        public bool IsOpenPo { get; set; } = false;

        // =============Transaction ============

        [Required(ErrorMessage = "Transaction type is mandatory.")]
        [StringLength(10, ErrorMessage = "Transaction type cannot exceed 10 characters.")]
        public string TransType { get; set; } = "Out";


        // =============== Item =================
        [Required(ErrorMessage = "Please select an item.")]
        public int? ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }

        public string? ItemSpecification { get; set; }
        public string? HSNCode { get; set; }
        public string? Category { get; set; }
        public int CategoryCode { get; set; }



        // =============== Quantity ================
        [Required(ErrorMessage = "Issued quantity is required.")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Issued quantity must be greater than zero.")]
        public decimal? Qty { get; set; }

        public decimal? BalQty { get; set; }

        public decimal? StockQty { get; set; }

        // ================= Pricing ==================

        [Required(ErrorMessage = "Unit Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit Price must be greater than zero.")]
        public decimal? UnitPrice { get; set; }



        [Range(0, double.MaxValue, ErrorMessage = "Process cost cannot be negative.")]
        public decimal? ProcessCost { get; set; }

        // ===== Route Card Reference =====
        public int? RcSubId { get; set; }
        public string? RefRcNo { get; set; }
        public DateTime? RefRcDate { get; set; }

        public int? ProcessId { get; set; }
        public string? ProcessName { get; set; }

        public int? MachineId { get; set; }
        public string? MachineName { get; set; }

        // ===== PO Reference =====
        public int? RefPoSubId { get; set; }
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }

        // ===== Cost =====
        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public int? GroupId { get; set; }

        // ===== Additional =====


        public bool ItemCancel { get; set; } = false;

        [StringLength(300, ErrorMessage = "Item cancel reason cannot exceed 300 characters.")]
        public string? ItemCancelReason { get; set; }


        [StringLength(100, ErrorMessage = "Batch number cannot exceed 100 characters.")]
        public string? BatchNo { get; set; }

        [StringLength(50, ErrorMessage = "Heat number cannot exceed 50 characters.")]
        public string? HeatNo { get; set; }

        [StringLength(300, ErrorMessage = "Remarks cannot exceed 300 characters.")]
        public string? Remark { get; set; }

        public bool IsEditable { get; set; } = false;
        public bool IsSelected { get; set; }

    }


}
