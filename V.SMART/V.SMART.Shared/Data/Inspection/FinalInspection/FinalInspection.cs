using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inspection.FinalInspection
{
    [Table("FinalInspection")]
    public class FinalInspection
    {
        // Base properties from both tables
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Auto-generated primary key

        //Inspection Number
        [Required(ErrorMessage = "Inspection Number is required")]
        [StringLength(250, ErrorMessage = "Inspection Number cannot exceed 250 characters")]
        [Display(Name = "Inspection Number")]
        public string? InspectNo { get; set; }

        //Suffix
        [Required]
        public string Suffix { get; set; }

        //Inspection Date
        [Required(ErrorMessage = "Inspection Date is required")]
        [Display(Name = "Inspection Date")]
        public DateTime InspectDate { get; set; }



        //customer ID
        [Required(ErrorMessage = "Customer ID is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Customer ID must be a positive number")]
        [Display(Name = "Customer ID")]
        public int? CustId { get; set; }
        [ForeignKey("CustId")]
        public virtual Customer Customer { get; set; }

        //Item ID   
        [Required(ErrorMessage = "Item ID is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Item ID must be a positive number")]
        [Display(Name = "Item ID")]
        public int? ItemId { get; set; }
        [ForeignKey("ItemId")]
        public virtual Item Item { get; set; }

        //Quantity
        [Required(ErrorMessage = "Quantity is required")]
        [Range(0, float.MaxValue, ErrorMessage = "Quantity must be a positive number")]
        [Display(Name = "Quantity")]
        public Decimal? Qty { get; set; }
        public Decimal? BalQty { get; set; }

        //Inspected By
        [StringLength(250, ErrorMessage = "Inspected By cannot exceed 250 characters")]
        [Display(Name = "Inspected By")]
        public String? InspectBy { get; set; }


        //Description
        [StringLength(250, ErrorMessage = "Description cannot exceed 250 characters")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // PONo
        [StringLength(250, ErrorMessage = "PONo cannot exceed 250 characters")]
        [Display(Name = "PONo")]
        public string? PONo { get; set; } = String.Empty;


        //Remarks
        [StringLength(250, ErrorMessage = "Remarks cannot exceed 250 characters")]
        [Display(Name = "Remarks")]
        public string? Remarks { get; set; }

        //Inspection Data
        [Column(TypeName = "NVARCHAR(MAX)")]
        [Display(Name = "Inspection Data")]
        public string? InspectData { get; set; }


        //Total Rows
        [Display(Name = "Total Rows")]
        [Range(0, int.MaxValue, ErrorMessage = "Total Rows must be a positive number")]
        public int? TotRows { get; set; }


        // DCTYPE
        [Display(Name = "DC Type")]
        [StringLength(250, ErrorMessage = "DC Type cannot exceed 250 characters")]
        public String? DcType { get; set; }
        
        
        //DC Number
        [StringLength(250, ErrorMessage = "DC Number cannot exceed 250 characters")]
        [Display(Name = "DC Number")]
        public string? DCNo { get; set; }

        //DC Date
        [StringLength(250, ErrorMessage = "DC Date cannot exceed 250 characters")]
        [Display(Name = "DC Date")]
        public DateTime DcDate { get; set; }

        public int? RefMfgDcSubID { get; set; }
        [ForeignKey(nameof(RefMfgDcSubID))]
        public virtual MfgDcSub? MfgDcSub { get; set; }

        public string? RefLabDcID {  get; set; }

        //Cost Center
        [Display(Name = "Cost Center Name")]
        [StringLength(250, ErrorMessage = "CostCenter cannot exceed 250 characters")]
        public String? CostCenter { get; set; }


        //Inpection Date (Actual)
        [Display(Name = "Inspection Date (Actual)")]
        public DateTime? InsDateNow { get; set; }

        //Accepted Quantity
        [Display(Name = "Accepted Quantity")]
        [Range(0, float.MaxValue, ErrorMessage = "Accepted Quantity must be a positive number")]
        public Decimal? AcceptQty { get; set; }

        //NCR Date
        [Display(Name = "NCR Date")]
        public DateTime? NCrDate { get; set; }

        [StringLength(250, ErrorMessage = "NCR Number cannot exceed 250 characters")]
        [Display(Name = "NCR Number")]

        // NCR Number
        public string NCRNo { get; set; }

        [Display(Name = "NCR Quantity")]
        [Range(0, float.MaxValue, ErrorMessage = "NCR Quantity must be a positive number")]

        // NCR Quantity
        public Decimal? NCRQty { get; set; }

        [Display(Name = "Rejected Quantity")]
        [Range(0, float.MaxValue, ErrorMessage = "Rejected Quantity must be a positive number")]

        // Rejected Quantity
        public Decimal? RejQty { get; set; }

        // Rework Quantity
        [Display(Name = "Rework Quantity")]
        [Range(0, float.MaxValue, ErrorMessage = "Rework Quantity must be a positive number")]
        public Decimal? ReWork { get; set; }

        // Batch Number
        [StringLength(250, ErrorMessage = "Batch Number cannot exceed 250 characters")]
        [Display(Name = "Batch Number")]
        public string? BatchNo { get; set; }

        // Sheet SlNo.
        [Required(ErrorMessage = "Sheet  Sl is required")]
        [Display(Name = "Sheet SlNo.")]
        public int? SheetSlNo { get; set; } 

        // Screen differentiation property
        [Required(ErrorMessage = "Screen type is required")]
        [Display(Name = "Is Random Inspection?")]
        public bool IsRandom { get; set; }


        // Audit Fields
        [Display(Name = "Created By")]
        [StringLength(40, ErrorMessage = "Created By cannot exceed 40 characters")]
        public string? CreatedBy { get; set; }


        [Display(Name = "Created Date")]
        public DateTime? CreatedDate { get; set; }


        [Display(Name = "Modified By")]
        [StringLength(40, ErrorMessage = "Modified By cannot exceed 40 characters")]
        public string? ModifiedBy { get; set; }


        [Display(Name = "Modified Date")]
        public DateTime? ModifiedDate { get; set; }

    }
}
