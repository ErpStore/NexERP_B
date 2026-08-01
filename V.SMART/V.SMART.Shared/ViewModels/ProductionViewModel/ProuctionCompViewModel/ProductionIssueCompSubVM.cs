using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel
{
    public class ProductionIssueCompSubVM
    {

        public int IssueSubId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int SlNo { get; set; }

        [Required]
        public int IssueId { get; set; }

        [Required]
        public string TransType { get; set; } = "Out";


        public ItemVM? SelectedItem { get; set; }

        [Required]
        public int? ItemId { get; set; }

        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? MeasureUnit { get; set; }


        [Required, Precision(18, 3)]
        public decimal? Qty { get; set; }

        [Precision(18, 3)]
        public decimal? BalQty { get; set; }

        [Precision(18, 4)]
        public decimal? UnitPrice { get; set; }

        [Precision(18, 4)]
        public decimal ProcessCost { get; set; }

        [Precision(18, 3)]
        public decimal? StockQty { get; set; }

        public decimal QtyPerUnit { get; set; }

        // References
        public int? RcSubId { get; set; }
        public string? RcNo { get; set; }
        public DateTime? RcDate { get; set; }

        public int? ProcessId { get; set; }
        public string? ProcessName { get; set; }

        public int? MachineId { get; set; }
        public string? MachineName { get; set; }



        public int? RefPoSubId { get; set; }
        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }

        public int? RefGRNSubId { get; set; }
        public string? RefGRNNo { get; set; }
        public DateTime? RefGRNDate { get; set; }


        public int? CostId { get; set; }
        public string? ProjectNo { get; set; }

        [MaxLength(300)]
        public string? Remark { get; set; }

        [MaxLength(100)]
        public string? BatchNo { get; set; }
        public string? HeatNo { get; set; }
        public bool IsEditable { get; set; } = false;

        public bool IsBOM { get; set; } = false;

        public int? GroupId { get; set; }

        public bool ItemCancel { get; set; } = false;

        public string? Category { get; set; }
        public int CategoryCode { get; set; }


    }


}
