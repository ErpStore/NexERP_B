using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConGRNVM
    {
        public int GRNId { get; set; }
        public bool IsNoInspection { get; set; }
        public bool IsManual { get; set; }
        public bool Rejection { get; set; }
        public bool IsReturn { get; set; }

        [Required(ErrorMessage = "GRN number is required.")]
        [StringLength(50, ErrorMessage = "GRN number cannot exceed 50 characters.")]
        public string GRNNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(10, ErrorMessage = "Suffix cannot exceed 10 characters.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "GRN date is required.")]
        public DateTime GRNDate { get; set; } = DateTime.Now;

        public DateTime GRNDateNow { get; set; } = DateTime.Now;

        public VendorVM? SelectedVendor { get; set; }

        [Required(ErrorMessage = "Please select a vendor.")]
        public int? VendorCode { get; set; }
        public string? VendorName { get; set; }
       
        public string? VendorAddress { get; set; }
        public string? VendorGstNo { get; set; }
        public string? VendorContactNo { get; set; }
        public string? KindofAttention { get; set; }
        public int? AddStoreId { get; set; }
        public string? StoreName { get; set; }

        [StringLength(250, ErrorMessage = "Main remark cannot exceed 250 characters.")]
        public string? MainRemark { get; set; }

        [StringLength(50, ErrorMessage = "Return by name cannot exceed 50 characters.")]

        [Required(ErrorMessage = "Party DC No. is required.")]
        public string PartyDC { get; set; } = string.Empty;

        [Required(ErrorMessage = "Party DC Date is required.")]
        [DataType(DataType.Date)]
        public DateTime? PartyDCDate { get; set; }

        public bool GRNTally { get; set; } = false;

        public bool Cancel { get; set; } = false;

        [StringLength(250)]
        public string? CancelBy { get; set; }

        public DateTime? CancelDate { get; set; }

        [StringLength(500)]
        public string? CancelReason { get; set; }

        public bool ShortClose { get; set; } = false;

        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public bool IsWithoutPoDc { get; set; } = false;


        [MinLength(1, ErrorMessage = "At least one Sub Contract GRN item is required.")]
        [ValidateComplexType]
        public List<SubConGRNSubVM> SubConGRNSubVMs { get; set; } = new();
        public List<SubConGRNTrackVM> SubConGRNTrackVMs { get; set; } = new();

        public int NoOfItems => SubConGRNSubVMs?.Count ?? 0;
        public decimal TotalQty => SubConGRNSubVMs?.Sum(x => x.Qty) ?? 0;
    }
}
