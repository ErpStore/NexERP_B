using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace V.SMART.Shared.ViewModels.InspectionViewModel.FinalInspectionVM
{
    public class FinalInspectionVM
    {
        //Id
        public int Id { get; set; }
        // Required fields

        // Inspection Number
        [Required(ErrorMessage = "Inspection Number is required")]
        [StringLength(250, MinimumLength = 1, ErrorMessage = "Inspection Number must be between 1 and 250 characters")]
        [Display(Name = "Inspection Number *")]
        [RegularExpression(@"^[A-Za-z0-9\-_/]+$", ErrorMessage = "Inspection Number can only contain letters, numbers, hyphens, underscores, and forward slashes")]
        public string InspectNo { get; set; }


        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(50, ErrorMessage = "Suffix cannot exceed 50 characters.")]
        public string Suffix { get; set; } = string.Empty;


        // Inspection Date
        [Required(ErrorMessage = "Inspection Date is required")]
        [Display(Name = "Inspection Date *")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime InspectDate { get; set; } = DateTime.Now;

        // Quantity
        [Required(ErrorMessage = "Quantity is required")]
        [Display(Name = "Quantity *")]
        public Decimal? Qty { get; set; }
        public Decimal? BalQty { get; set; }


        // Customer
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Customer")]
        [Display(Name = "Customer *")]
        public int? CustId { get; set; }

        // Item
        [Required(ErrorMessage = "Item is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Item")]
        [Display(Name = "Item *")]
        public int? ItemId { get; set; }

        // Staff
        [StringLength(250, ErrorMessage = "Inspected By cannot exceed 250 characters")]
        [Display(Name = "Inspected By")]
        public string InspectBy { get; set; }

        // Remarks
        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters")]
        [Display(Name = "Remarks")]
        public string Remarks { get; set; }=String.Empty;

        // Description
        [StringLength(250, ErrorMessage = "Discription cannot exceed 250 characters")]
        [Display(Name = "Discription")]
        public string? Description { get; set; } = String.Empty;

        // PONo
        [StringLength(250, ErrorMessage = "PONo cannot exceed 250 characters")]
        [Display(Name = "PONo")]
        public string? PONo { get; set; } = String.Empty;

        // Total Rows
        [Display(Name = "Total Rows")]
        [Range(0, int.MaxValue, ErrorMessage = "Total Rows must be a positive number")]
        public int? TotRows { get; set; }

        // DC Details
        [Display(Name = "DC Type")]
        [Range(0, 255, ErrorMessage = "DC Type must be between 0 and 255")]
        public String? DcType { get; set; }

        public int DcId { get; set; }

        // DC Number
        [StringLength(250, ErrorMessage = "DC Number cannot exceed 250 characters")]
        [Display(Name = "DC Number")]
        public string DCNo { get; set; }

        // DC Date
        [Display(Name = "DC Date")]
        [StringLength(250, ErrorMessage = "DC Date cannot exceed 250 characters")]
        public DateTime DcDate { get; set; }

        //MfgDcSubId
        public int? RefMfgDcSubID { get; set; }

        // Cost Center
        [Display(Name = "Cost Center")]
        [StringLength(250, ErrorMessage = "Cost Center cannot exceed 250 characters")]
        public string CostCenter { get; set; } = String.Empty;

        // Batch Number
        [Display(Name = "Batch Number")]
        [StringLength(250, ErrorMessage = "Batch Number cannot exceed 250 characters")]
        public string BatchNo { get; set; } = String.Empty;

        // Inspection type
        [Required(ErrorMessage = "Inspection Type is required")]
        [Display(Name = "Inspection Type *")]
        public bool IsRandom { get; set; } = true;

        // Sheet SlNo.
        [Required(ErrorMessage = "Sheet  Sl is required")]
        public int SheetSlNo { get; set; } = 1;


        // NCR related fields
        [Display(Name = "NCR Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? NCrDate { get; set; }

        [StringLength(250, ErrorMessage = "NCR Number cannot exceed 250 characters")]
        [Display(Name = "NCR Number")]
        public string NCRNo { get; set; }= String.Empty;

        // Inspection Data
        public string? InspectData { get; set; }

        // Actual inspection date
        [Display(Name = "Actual Inspection Date")]
        [DataType(DataType.DateTime)]
        public DateTime? InsDateNow { get; set; } = DateTime.Now;

        // Quantity breakdown fields
        [Display(Name = "Accepted Quantity")]
        [Range(0, float.MaxValue, ErrorMessage = "Accepted Quantity must be a positive number")]
        public Decimal? AcceptQty { get; set; }

        //NCR Quantity
        [Display(Name = "NCR Quantity")]
        [Range(0, float.MaxValue, ErrorMessage = "NCR Quantity must be a positive number")]
        public Decimal? NCRQty { get; set; }

        // Rejected Quantity
        [Display(Name = "Rejected Quantity")]
        [Range(0, float.MaxValue, ErrorMessage = "Rejected Quantity must be a positive number")]
        public Decimal? RejQty { get; set; }


        // Rework Quantity
        [Display(Name = "Rework Quantity")]
        [Range(0, float.MaxValue, ErrorMessage = "Rework Quantity must be a positive number")]
        public Decimal? ReWork { get; set; }


        // For display purposes

       
        public string? SheetName { get; set; }
        public string? CustomerName { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemUOM { get; set; }
        public string InspectionType =>IsRandom == true ? "Random Inspection" : "Individual Inspection";


        // Audit fields (readonly in views)
        [Display(Name = "Created By")]
        [ReadOnly(true)]
        public string CreatedBy { get; set; }

        [Display(Name = "Created Date")]
        [ReadOnly(true)]
        public DateTime? CreatedDate { get; set; }

        [Display(Name = "Modified By")]
        [ReadOnly(true)]
        public string? ModifiedBy { get; set; }=String.Empty;

        [Display(Name = "Modified Date")]
        [ReadOnly(true)]
        public DateTime? ModifiedDate { get; set; }

    }
}
