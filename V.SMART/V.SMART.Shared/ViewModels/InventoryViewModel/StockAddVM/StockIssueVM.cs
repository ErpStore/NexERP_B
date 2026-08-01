using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.StockAddVM
{
    public class StockIssueVM
    {
       
        public int IssueId { get; set; }

        [Required(ErrorMessage = "Issue Date is required.")]
        public DateTime IssueDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Item is required.")]
        public int ItemId { get; set; }
        public ItemVM? SelectedItem { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Specification { get; set; }
        public string? MeasureUnit { get; set; }
        public string? HSNCode { get; set; }
        public decimal? Weight { get; set; }
        public string? Category { get; set; }

       

        [Required(ErrorMessage = "Store is required.")]
        public int StoreId { get; set; }
        public string? IssueStoreName { get; set; }


        [Required(ErrorMessage = "IssueQty is required.")]
        [Range(0.001, 9999999, ErrorMessage = "IssueQty must be greater than 0.")]
        
        public decimal IssueQty { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [Range(0, 9999999, ErrorMessage = "Rate cannot be negative.")]
       
        public decimal UnitPrice { get; set; }

        [StringLength(50, ErrorMessage = "Batch number cannot exceed 50 characters.")]
        public string? BatchNo { get; set; }

        [Required(ErrorMessage = "Screen selection is required.")]
        public int ScreenCode { get; set; }
        public string? ScreenName { get; set; }


        public int? SubItemRefID { get; set; }

        [StringLength(50, ErrorMessage = "Reference number cannot exceed 50 characters.")]
        public string? RefNo { get; set; }

        [DataType(DataType.Date)]
        public DateTime? RefDate { get; set; }
        public string? Remarks { get; set; }

        public string? IssuedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;
    }
}
