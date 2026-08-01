using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels
{
    public class ToolCribIssueVM
    {
        public int TCIssueId { get; set; }

        [Required(ErrorMessage = "ToolCribIssue Number is Required!..")]
        [StringLength(20,ErrorMessage ="Issue Number cannot be Exceed 20 Characters")]
        public string TCIssueNo { get; set; }

        [Required(ErrorMessage="Suffix is required")]
        [StringLength(20, ErrorMessage = "Suffix cannot be Exceed 20 Characters")]
        public string Suffix { get; set; }

        [Required(ErrorMessage = "ToolCribIssue Date is Required!..")]
        public DateTime TCIssueDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "ToolCribIssue Date is Required!..")]
        public DateTime TCIssueDateNow { get; set; } = DateTime.Now;

        public bool TCIssueTally { get; set; } = false;


        // Category Info
        [Required(ErrorMessage = "Please select Category!..")]
        public int? CategoryCode { get; set; }
        public string? CategoryName { get; set; }


        // Store Info
        [Required(ErrorMessage = "Please select Store!..")]
        public int? StoreId { get; set; }
        public string? StoreName { get; set; }


        [Required(ErrorMessage = "Please select IssuedToWhom!..")]
        [MaxLength (100, ErrorMessage ="Allow Only 100 Characters")]
        public string? IssuedToWhom { get; set; }
        public bool IsReturn { get; set; } = false;

        [StringLength(20, ErrorMessage = "MainRemarks cannot be Exceed 300 Characters")]
        public string? MainRemarks { get; set; }

        [StringLength(20, ErrorMessage = "Description cannot be Exceed 300 Characters")]
        public string? Description { get; set; }

        public int NoOfItems => ToolCribIssueSubVMs?.Count ?? 0;

        public string? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        [MinLength(1, ErrorMessage = "At least one ToolCribissue item is required.")]
        [ValidateComplexType]
        public List<ToolCribIssueSubVM> ToolCribIssueSubVMs { get; set; } = new();
    }


}
