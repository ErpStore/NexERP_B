using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels
{
    public class ToolCribIssueSubVM
    {
        public int TCIssueSubId { get; set; }
        public int TCIssueId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Serial number must be greater than zero.")]
        public int SlNo { get; set; }


        [Required(ErrorMessage = "ItemCode/PartTime is Required")]
        public int? ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? HSNCode { get; set; }
        public string? MeasureUnit { get; set; }


        [Precision(18,3)]
        [Required(ErrorMessage = "Please Enter the Quantity")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        public decimal? QtyOut { get; set; }


        [Precision (18,3)]
        public decimal? BalQty { get; set; }

        [Precision (18,3)]
        public decimal? StockQty { get; set; }


        //[Required(ErrorMessage = "Please Enter the Price")]
        //[Range(0.0001, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal? UnitPrice { get; set; }


        [StringLength(250, ErrorMessage = "Remark should not exceed 250 characters.")]
        public string? Remark { get; set; }

        public bool IsEditable { get; set; } = false;
    }


}
