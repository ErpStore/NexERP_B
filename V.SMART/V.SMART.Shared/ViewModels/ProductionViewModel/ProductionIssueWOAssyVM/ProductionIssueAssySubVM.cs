using V.SMART.Shared.Data.Master.Accounts_Module;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM
{
    public class ProductionIssueAssySubVM
    {
        public int IssueSubId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Serial Number (SlNo) must be greater than 0.")]
        [Required(ErrorMessage = "Serial Number (SlNo) is required.")]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "IssueId is required.")]
        public int IssueId { get; set; }

        [Required(ErrorMessage = "Item selection is required.")]
        public int? ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }

        [Required(ErrorMessage = "Issue Qty is required.")]
        [Precision(18, 3)]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Issue Qty must be greater than 0.0001.")]
        public decimal? IssueQty { get; set; }


        [Precision(18, 3)]
        public decimal? BalQty { get; set; }

        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        [Precision(18, 3)]
        public decimal? StockQty { get; set; }

        [MaxLength(300, ErrorMessage = "Remark cannot exceed 300 characters.")]
        public string? Remark { get; set; }

        [MaxLength(100, ErrorMessage = "Batch Number cannot exceed 100 characters.")]
        public string? BatchNo { get; set; }

        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        public int RefJobOrderSubId { get; set; }

        public int RefJobId { get; set; }   
        public string? JobOrderNo { get; set; }
        public DateTime? JobOrderDate { get; set; }

        public int AssyId { get; set; }
        public string? AssyItemCode { get; set; }


        public bool IsStockExceeded { get; set; }
        public bool IsEditable { get; set; } = false;
    }
}
