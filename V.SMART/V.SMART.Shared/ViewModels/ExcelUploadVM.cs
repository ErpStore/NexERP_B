using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels
{
    public class ExcelUploadVM
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Sl No. must be greater than zero.")]
        public int SlNo { get; set; }
        public int? LineNo { get; set; }
        public string? RFQ { get; set; }

        [Required(ErrorMessage = "Item is required.")]
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string ItemSpeification { get; set; }
        public string? HSNCode { get; set; }
        public string? MeasureUnit { get; set; }
        public string? SACCode { get; set; }
        public string? ItemType { get; set; }
        public string? Category { get; set; }
        public int? categorycode { get; set; }
        public string? PurchaseUnits { get; set; }
        public decimal? ItemWeight { get; set; }
        public decimal? ItemArea { get; set; }
        public string? Remarks { get; set; }
        public decimal? DCQty { get; set; }
        public decimal? RecievedQty { get; set; }
        public decimal? Qty { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? UnitPrice { get; set; }

        [Range(0, 100, ErrorMessage = "Discount percent must be between 0 and 100.")]
        public decimal? LineDiscountPercent { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Discount amount must be non-negative.")]
        public decimal? LineDiscountAmount { get; set; }

        [Range(0, 100, ErrorMessage = "CGST rate must be between 0 and 100.")]
        public decimal? LineCGSTRate { get; set; }

        [Range(0, 100, ErrorMessage = "SGST rate must be between 0 and 100.")]
        public decimal? LineSGSTRate { get; set; }

        [Range(0, 100, ErrorMessage = "IGST rate must be between 0 and 100.")]
        public decimal? LineIGSTRate { get; set; }

        public string? WONo { get; set; }
        public string? RowRemarks { get; set; }

        public string? ProjectNo { get; set; }

        public string? BatchNo { get; set; }

        public string? HeatNo { get; set; }
        public DateTime? DueDate { get; set; }
           public DateTime? PoDate { get; set; }

        public string TransType { get; set; }

        public bool IsNewItem { get; set; } = false;

        [Precision(18, 3)]
        [Range(0, 100, ErrorMessage = "LabourPricePerHour must be between 0 and 100")]
        public decimal? LabourPricePerHour { get; set; } = 0;


        [Precision(18, 3)]
        [Range(0, 100, ErrorMessage = "LabourTimeMinutes must be between 0 and 100")]
        public decimal? LabourTimeMinutes { get; set; } = 0;


        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "LabourPrice must be a non-negative value.")]
        public decimal LabourPrice { get; set; } = 0;

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "AssemblyPrice must be a non-negative value.")]
        public decimal AssemblyPrice { get; set; } = 0;

        [Range(0, 100, ErrorMessage = "HandlingCharges must be between 0 and 100%.")]
        public decimal HandlingCharges { get; set; } = 0;

        // 🔴 Column-level validation errors
        public Dictionary<string, string> ColumnErrors { get; set; } = new();
    }
}
