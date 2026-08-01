using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.StoreInterTransferViewModel
{
    public class StoreInterTransVM
    {
        public int ISTId { get; set; }

        [Required(ErrorMessage = "Inter Store Transfer Number is required.")]
        [StringLength(10, ErrorMessage = "Inter Store Transfer Number cannot exceed 10 characters.")]
        public string ISTNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is Required")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Inter Store Transfer Date is required.")]
        public DateTime ISTDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Issue Store is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Issue From Store.")]
        public int? StoreIdFrom { get; set; }

        public string StoreIssName { get; set; }

        [Required(ErrorMessage = "Add Store is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Add To Store.")]
        public int? StoreIdTo { get; set; }
        public string StoreAddName { get; set; }
        public string Status { get; set; }

        public int NoOfItems => StoreInterTransSubVMs?.Count ?? 0;

        [StringLength(500, ErrorMessage = "Main Remark cannot exceed 500 characters.")]
        public string? MainRemark { get; set; }

        [StringLength(100, ErrorMessage = "Created By cannot exceed 100 characters.")]
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100, ErrorMessage = "Modified By cannot exceed 100 characters.")]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }


        [MinLength(1, ErrorMessage = "At least one Row item is required.")]
        [ValidateComplexType]
        public List<StoreInterTransSubVM> StoreInterTransSubVMs { get; set; } = new();
    }
}
