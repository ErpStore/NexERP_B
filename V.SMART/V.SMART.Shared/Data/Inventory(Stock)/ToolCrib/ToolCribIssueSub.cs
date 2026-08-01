using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_.ToolCrib
{
    public class ToolCribIssueSub
    {
        [Key]
        public int TCIssueSubId { get; set; }

        [Required]
        public int TCIssueId { get; set; }

        [ForeignKey(nameof(TCIssueId))]
        public ToolCribIssue ToolCribIssue { get; set; }

        [Required]
        public int SlNo { get; set; }


        [Required]
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }


        [Required(ErrorMessage = "Please Enter the Quantity")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Precision(18, 3)]
        public decimal QtyOut { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Required(ErrorMessage = "Please Enter the Price")]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        [StringLength(250, ErrorMessage = "Remark should not exceed 250 characters.")]
        public string? Remark { get; set; }
    }

}
