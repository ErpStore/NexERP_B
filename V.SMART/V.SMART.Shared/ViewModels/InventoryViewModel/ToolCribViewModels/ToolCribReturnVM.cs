using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels
{
    public class ToolCribReturnVM
    {
        public int TCReturnId { get; set; }

        [Required(ErrorMessage = "Return number is required.")]
        [StringLength(50, ErrorMessage = "Return number cannot exceed 50 characters.")]
        public string TCReturnNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Return date is required.")]
        public DateTime TCReturnDate { get; set; } = DateTime.Now;

        public DateTime TCReturnDateNow { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(10, ErrorMessage = "Suffix cannot exceed 10 characters.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "From Whom is required.")]
        [StringLength(100, ErrorMessage = "From Whom cannot exceed 100 characters.")]
        public string FromWhom { get; set; } = string.Empty;

        [Required(ErrorMessage = "Number of items is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Number of items must be at least 1.")]
        public int NoOfItems => ToolCribReturnSubVMs?.Count ?? 0;


        [StringLength(300, ErrorMessage = "Main remarks cannot exceed 300 characters.")]
        public string? MainRemarks { get; set; }

        [StringLength(300, ErrorMessage = "Breakage remarks cannot exceed 300 characters.")]
        public string? BreakageRemarks { get; set; }

        [Required(ErrorMessage = "Store selection is required.")]
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }


        [StringLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [StringLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one return item is required.")]
        [ValidateComplexType]
        public List<ToolCribReturnSubVM> ToolCribReturnSubVMs { get; set; } = new();
       
    }

}
