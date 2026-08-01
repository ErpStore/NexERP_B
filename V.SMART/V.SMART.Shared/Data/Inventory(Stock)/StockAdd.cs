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

namespace V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote
{
    public class StockAdd
    {
        [Key]
        public int AddId { get; set; }

        [Required(ErrorMessage = "Add Date is required.")]
        public DateTime AddDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Item is required.")]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; }

        [Required(ErrorMessage = "Store is required.")]
        public int StoreId { get; set; }

        [ForeignKey(nameof(StoreId))]
        public virtual Store Store { get; set; }

        [Required(ErrorMessage = "Quantity is required.")]
        [Range(0.001, 9999999, ErrorMessage = "Quantity must be greater than 0.")]
        [Precision(18, 3)]
        public decimal AddQty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Required(ErrorMessage = "UnitPrice is required.")]
        [Range(0, 9999999, ErrorMessage = "UnitPrice cannot be negative.")]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        [StringLength(50, ErrorMessage = "Batch number cannot exceed 50 characters.")]
        public string? BatchNo { get; set; }

        [Required(ErrorMessage = "Screen selection is required.")]
        public int ScreenCode { get; set; }

        [ForeignKey(nameof(ScreenCode))]
        public virtual Screens Screen { get; set; }

        [Required]
        public int SubItemRefID { get; set; }

        [Required]
        public string? RefNo { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? RefDate { get; set; }

        public int? RcSubID { get; set; }

        public string? Remarks { get; set; }

        public string? AddedBy { get; set; }
        public DateTime? CreatedDate { get; set; } = DateTime.Now;

    }

}
