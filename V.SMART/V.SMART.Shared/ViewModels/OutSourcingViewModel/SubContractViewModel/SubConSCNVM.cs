using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConSCNVM
    {
        public int SCNId { get; set; }

        [Required(ErrorMessage = "SCN number is required.")]
        [StringLength(50, ErrorMessage = "SCN number cannot exceed 50 characters.")]
        public string SCNNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(10, ErrorMessage = "Suffix cannot exceed 10 characters.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "SCN date is required.")]
        public DateTime SCNDate { get; set; } = DateTime.Now;

        public DateTime SCNDateNow { get; set; } = DateTime.Now;

        public VendorVM? SelectedVendor { get; set; }

        [Required(ErrorMessage = "Please select a subcontract vendor.")]
        public int? VendorCode { get; set; }
        public string? VendorName { get; set; }

        public string? VendorAddress { get; set; }
        public string? VendorGSTNo { get; set; }
        public string? VendorContactNo { get; set; }


        [Required(ErrorMessage = "Please select the receiving store.")]
        public int? AddStoreId { get; set; }
        public string? AddStoreName { get; set; }

        [Required(ErrorMessage = "Please select the issuing store.")]
        public int? IssueStoreId { get; set; }
        public string? IssueStoreName { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? MainRemark { get; set; }

        [StringLength(500, ErrorMessage = "Detailed remarks cannot exceed 500 characters.")]
        public string? MainRemarks { get; set; }

        public bool SCNCancel { get; set; }
        public bool SCNTally { get; set; }

        [StringLength(250, ErrorMessage = "Cancel reason cannot exceed 250 characters.")]
        public string? CancelReason { get; set; }

        [StringLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public DateTime? CancelDate { get; set; }

        public string? CanceledBy { get; set; } = string.Empty;
        public bool ShortClose { get; set; } = false;

        [MinLength(1, ErrorMessage = "At least one Sub Contract SCN item is required.")]
        [ValidateComplexType]
        public List<SubConSCNSubVM> SubConSCNSubVMs { get; set; } = new();

        public int NoOfItems => SubConSCNSubVMs?.Count ?? 0;
    }

}
