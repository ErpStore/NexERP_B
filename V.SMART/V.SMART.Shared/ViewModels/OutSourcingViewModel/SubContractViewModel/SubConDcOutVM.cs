using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel
{
    public class SubConDcOutVM
    {
        public int DcId { get; set; }

        [StringLength(20, ErrorMessage = "Prefix cannot exceed 20 characters.")]
        public string? Prefix { get; set; }

        [Required(ErrorMessage = "Delivery Challan number is required.")]
        [StringLength(50, ErrorMessage = "Delivery Challan number cannot exceed 50 characters.")]
        public string DcNo { get; set; } = string.Empty;

        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(25, ErrorMessage = "Suffix cannot exceed 25 characters.")]
        public string Suffix { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery Challan date is required.")]
        public DateTime DcDate { get; set; } = DateTime.Now;

        public DateTime DcDateNow { get; set; } = DateTime.Now;

        // ===== Store =====
        [Required(ErrorMessage = "Issue store is required.")]
        public int? StoreIssId { get; set; }

        public string? StoreIssueName { get; set; }

        // ===== Calculated Fields =====
        public int NoOfItems => SubConDcOutSubVMs.Count;

        public decimal Qty => SubConDcOutSubVMs.Sum(x => x.Qty ?? 0);

        // ===== Vendor =====
        [Required(ErrorMessage = "Subcontract vendor is required.")]
        public int? VendorCode { get; set; }

        public VendorVM? SelectedVendor { get; set; }

        [StringLength(150, ErrorMessage = "Vendor name cannot exceed 150 characters.")]
        public string? VendorName { get; set; }

        [StringLength(300, ErrorMessage = "Vendor address cannot exceed 300 characters.")]
        public string? VendorAddress { get; set; }

        [StringLength(20, ErrorMessage = "GST number cannot exceed 20 characters.")]
        public string? VendorGstNo { get; set; }

        [StringLength(15, ErrorMessage = "Contact number cannot exceed 15 digits.")]
        public string? VendorContactNo { get; set; }

        // ===== Shipping =====
        public bool IsSameAsShipping { get; set; } = true;

        public int? ShipAddrsId { get; set; }

        [StringLength(150, ErrorMessage = "Shipping name cannot exceed 150 characters.")]
        public string? ShippingName { get; set; }

        [StringLength(300, ErrorMessage = "Shipping address cannot exceed 300 characters.")]
        public string? ShippingAddress { get; set; }

        [StringLength(20, ErrorMessage = "Shipping GSTIN cannot exceed 20 characters.")]
        public string? ShippingGstin { get; set; }

        [StringLength(15, ErrorMessage = "Shipping contact cannot exceed 15 digits.")]
        public string? ShippingContactNo { get; set; }

        // ===== Transport =====
        [StringLength(13, ErrorMessage = "Vehicle number cannot exceed 13 characters.")]
        [RegularExpression(@"^$|^[A-Z]{2}[ -]?\d{1,2}[ -]?[A-Z]{1,2}[ -]?\d{4}$",
            ErrorMessage = "Invalid vehicle number format (Example: KA01AB1234).")]
        public string? VehicleNo { get; set; }

        [Required(ErrorMessage ="Mode Of Transport selection is required")]
        public int? ModeOfTransportId { get; set; }

        [Required(ErrorMessage = "Transport From is required")]
        [StringLength(25, ErrorMessage = "From location cannot exceed 25 characters.")]
        public string? TransFrom { get; set; }

        [Required(ErrorMessage = "Transport To is required")]
        [StringLength(25, ErrorMessage = "To location cannot exceed 25 characters.")]
        public string? TransTo { get; set; }

        // ===== Sub Supply =====
        [Required(ErrorMessage ="Sub Supply type selection is required")]
        public int? SubSupplyType { get; set; }

        [StringLength(200, ErrorMessage = "Sub-supply description cannot exceed 200 characters.")]
        public string? SubSupplyDesc { get; set; }

        // ===== EWay =====
        [StringLength(20, ErrorMessage = "Transport ID cannot exceed 20 characters.")]
        public string? EwayTransId { get; set; }

        [StringLength(100, ErrorMessage = "Transport name cannot exceed 100 characters.")]
        public string? EwayTransName { get; set; }

        [StringLength(15, ErrorMessage = "Transport document number cannot exceed 15 characters.")]
        public string? EwayTransDocNo { get; set; }

        public DateTime? EwayBillDate { get; set; }

        [StringLength(150, ErrorMessage = "E-Way Bill number cannot exceed 150 characters.")]
        public string? EwayBillNumber { get; set; }

        // ===== Value =====
        [Range(0, double.MaxValue, ErrorMessage = "DC value cannot be negative.")]
        public double? DcVal { get; set; }

        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters.")]
        public string? MainRemarks { get; set; }

        // ===== Flags =====
        public bool IsVendor { get; set; }
        public bool IsManualMaterialOut { get; set; }
        public bool DiffItemsOutIn { get; set; }
        public bool IsWithoutPoDc { get; set; }
        public bool IsPurchaseRework { get; set; }

        // ===== Contact =====
        [StringLength(50, ErrorMessage = "Contact person name cannot exceed 50 characters.")]
        public string? ContactPerson { get; set; }

        [StringLength(13, ErrorMessage = "Contact number cannot exceed 13 digits.")]
        public string? ContactNumber { get; set; }

        // ===== Status =====
        public bool DcSign { get; set; }
        public bool DcTally { get; set; }
        public bool Cancel { get; set; }

        public bool ShortClose { get; set; }

        [StringLength(250, ErrorMessage = "Cancel reason cannot exceed 250 characters.")]
        public string? CancelReason { get; set; }

        public DateTime? CancelDate { get; set; }
        public string? CancelBy { get; set; }

        public bool IsSelected { get; set; }
        // ===== Audit =====
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public bool IsOutItemSameAsInItem { get; set; } = false;

        // ===== Grid =====
        [MinLength(1, ErrorMessage = "At least one Subcontract DC-Out item is required.")]
        [ValidateComplexType]
        public List<SubConDcOutSubVM> SubConDcOutSubVMs { get; set; } = new();
    }

}
