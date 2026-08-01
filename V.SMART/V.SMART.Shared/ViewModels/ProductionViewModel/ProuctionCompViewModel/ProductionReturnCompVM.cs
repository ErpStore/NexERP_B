using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel
{
    public class ProductionReturnCompVM
    {
        public int ReturnId { get; set; }

        public bool IsNoInspection { get; set; } = false;
        public bool Rejection { get; set; } = false;
        public bool Return { get; set; } = false;
        public bool IsManual { get; set; }

        [Required(ErrorMessage = "Return Number is required.")]
        [MaxLength(50)]
        public string ReturnNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [MaxLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Return Date is required.")]
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Current Return Date is required.")]
        public DateTime ReturnDateNow { get; set; } = DateTime.Now;


        public int? CustId { get; set; }
        public string? CustName { get; set; }

        public CustomerVM? SelectedCustomer { get; set; }


        [MaxLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? MainRemark { get; set; }

        [Required(ErrorMessage = "Return By is required.")]
        [MaxLength(50, ErrorMessage = "Return By cannot exceed 50 characters.")]
        public string ReturnBy { get; set; } = string.Empty;


        [Required(ErrorMessage = "Add Store is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Add Store.")]
        public int? AddStoreId { get; set; }

        public string? AddStoreName { get; set; }

        public bool ReturnTally { get; set; } = false;

        [MaxLength(100)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        [MaxLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool Cancel { get; set; } = false;

        [StringLength(250)]
        public string? CancelBy { get; set; }

        public DateTime? CancelDate { get; set; }

        [StringLength(500)]
        public string? CancelReason { get; set; }

        public bool ShortClose { get; set; } = false;
        public bool IsWithoutPoDc { get; set; } = false;

        public int? ShiftId { get; set; }

        public int? ProcessId { get; set; }


        [MinLength(1, ErrorMessage = "At least one Production Return item is required.")]
        [ValidateComplexType]
        public List<ProductionReturnCompSubVM> ProductionReturnCompSubVMs { get; set; } = new();
        public List<ProductionReturnCompTrackVM> ProductionReturnCompTrackVMs { get; set; } = new();

        // For selection UI
        public List<int> SelectedIssueIds { get; set; } = new();
        public List<int> SelectedIssueSubIds { get; set; } = new();
        public List<int> SelectedJobOrderIds { get; set; } = new();
    }
}
