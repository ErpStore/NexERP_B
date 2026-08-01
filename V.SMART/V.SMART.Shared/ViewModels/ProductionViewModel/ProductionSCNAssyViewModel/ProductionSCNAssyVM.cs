using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel
{
    public class ProductionSCNAssyVM
    {
        public int SCNId { get; set; }

        [Required(ErrorMessage = "SCN No is required.")]
        [MaxLength(50, ErrorMessage = "SCN No cannot exceed 50 characters.")]
        public string SCNNo { get; set; } = string.Empty;

        [MaxLength(10, ErrorMessage = "Suffix cannot exceed 10 characters.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "SCN Date is required.")]
        public DateTime SCNDate { get; set; } = DateTime.Now;

        public DateTime SCNDateNow { get; set; } = DateTime.Now;

        [MaxLength(250, ErrorMessage = "Main Remark cannot exceed 250 characters.")]
        public string? MainRemark { get; set; }

        // Add Store
        [Required(ErrorMessage = "Add Store selection is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Add Store.")]
        public int? AddStoreId { get; set; }

        [MaxLength(100)]
        public string? AddStoreName { get; set; }

        // Issue From Store
        [Required(ErrorMessage = "Issue From Store selection is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Issue Store.")]
        public int? IssueStoreId { get; set; }

        [MaxLength(100)]
        public string? IssueStoreName { get; set; }

        // Created 
        [MaxLength(100)]
        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Modified
        [MaxLength(100)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one Production SCN item is required.")]
        [ValidateComplexType]
        public List<ProductionSCNAssySubVM> ProductionSCNAssySubVMs { get; set; } = new();
    }

}
