using FastReport.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.InspectionViewModel.IncomingInspectionVM
{
    public class IncomingInspectionVM
    {
        //Primary Id
        public int Id { get; set; }

        //Inspection No.
        [Required(ErrorMessage = "Inspection Number is Required")]
        [Display(Name = "Inspection No")]
        [StringLength(250)]
        public string? InspectNo { get; set; }

        //Suffix
        [Required(ErrorMessage = "Suffix is required.")]
        [StringLength(50, ErrorMessage = "Suffix cannot exceed 50 characters.")]
        public string Suffix { get; set; } = string.Empty;

        //Inspection Date
        [Display(Name = "Inspection Date")]
        public DateTime? InspectDate { get; set; }

        // Inspection type
        [Required(ErrorMessage = "Inspection Type is required")]
        [Display(Name = "Inspection Type *")]
        public bool IsRandom { get; set; } = true;

        //DC Type
        [Required(ErrorMessage = "DC Type is Required")]
        [Display(Name = "DC Type")]
        public String? DcType { get; set; }

        public bool status { get; set; }

        //DCNo
        [Required(ErrorMessage = "DC No is Required")]
        [Display(Name = "DC No")]
        [StringLength(250)]
        public string? DCNo { get; set; }


       // DC Date
        [Required(ErrorMessage = "DC Date is Required")]
        [Display(Name = "DC Date")]
        [StringLength(50)]
        public DateTime? DcDate { get; set; }


        //Supplier
        [Display(Name = "Supplier")]
        [StringLength(250)]
        public int? SuppId { get; set; }


        //Item
        [Required(ErrorMessage = "Item is required")]
        public int? ItemId { get; set; }

        //Inspected By
        [StringLength(250, ErrorMessage = "Inspected By cannot exceed 250 characters")]
        [Display(Name = "Inspected By")]
        public String? InspectBy { get; set; }


        // Sheet SlNo.
        [Required(ErrorMessage = "Sheet  Sl is required")]
        public int? SheetSlNo { get; set; }


        //Qty
        [Required(ErrorMessage = "Quantity is Required")]
        [Display(Name = "Quantity")]
        [Range(0, float.MaxValue)]
        public Decimal? Qty { get; set; }


        // Bal Qty
        [Required(ErrorMessage = "Quantity is Required")]
        [Display(Name = "Quantity")]
        [Range(0, float.MaxValue)]
        public Decimal BalQty { get; set; }

        //Accepted Qty
        [Required(ErrorMessage = "Accepted Quantity is Required")]
        [Display(Name = "Accepted Qty")]
        public Decimal? AcceptQty { get; set; }

        //Rejected Qty
        [Display(Name = "Rejected Qty")]
        [Range(0, float.MaxValue)]
        public Decimal? RejQty { get; set; }


        //Rework Qty
        [Display(Name = "Rework Qty")]
        public Decimal? ReWorkQty { get; set; }

        //Remarks
        [StringLength(250)]
        public string? Remarks { get; set; }

        //Inspection Data
        public string? InspectionData { get; set; }

        //Total Rows
        [Display(Name = "Total Rows")]
        public int TotRows { get; set; }


        //CostCenter 
        [Display(Name = "Cost Center")]
        public String CostCenter { get; set; }

        //INspection Date
        [Display(Name = "Inspection Date (System)")]
        public DateTime? InsDateNow { get; set; }


        //GRN Number
        [Display(Name = "GRN No")]
        [Required(ErrorMessage = "GRN Number By is required")]
        public String Grnno { get; set; }

        //Ref Grnsub Ids
        public long? ProductionLogId { get; set; }

        public int? RefPurchaseGRNSubId {  get; set; }
        public int? RefProductionAssyGRNId { get; set; }

        public int? RefProductionCompGRNSubId { get; set; }

        //Accessing Purpose
        public int? Commongrnid {  get; set; }

        //NCR Date
        [Display(Name = "NCR Date")]
        public DateTime? NCrDate { get; set; }

        //NCR No
        [Display(Name = "NCR No")]
        [StringLength(250)]
        public string? NCRNo { get; set; }


        //NCR Qty
        [Display(Name = "NCR Qty")]
        public float NCRQty { get; set; }


        //Batch No
        [Display(Name = "Batch No")]
        [StringLength(250)]
        public string? BatchNo { get; set; }


        //Process
        [Display(Name = "Process")]
        public int? ProcessId { get; set; }

        public string? processName { get; set; }

        [Display(Name = "No Dimension Entry")]
        public bool NoDimensionEntry { get; set; }


        //Display Names
        public string InspectionType => IsRandom == true ? "Random Inspection" : "Individual Inspection";
        public String? SupplierName=String.Empty;
        public String? CustomerName = String.Empty;
        public String? VendorName = String.Empty;
        public String? ItemCode = String.Empty;
        public String? ItemName = String.Empty;
        public string? ItemUOM { get; set; }
        public string? SheetName { get; set; }
        public String? StaffName = String.Empty;

    }
}
