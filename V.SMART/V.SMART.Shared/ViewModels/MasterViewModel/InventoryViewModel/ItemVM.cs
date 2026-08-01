using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.ViewModels
{
    public class ItemVM : IValidatableObject
    {
        public bool IsChecked { get; set; }
        public int ItemId { get; set; }

        // Basic Details
        [Required(ErrorMessage = "Item Code / Part No. is required.")]
        [StringLength(250, ErrorMessage = "Item Code cannot exceed 250 characters.")]
        public string ItemCode { get; set; }

        [Required(ErrorMessage = "Item Name / Description is required.")]
        [StringLength(250, ErrorMessage = "Item Name cannot exceed 250 characters.")]
        public string ItemName { get; set; }

        [StringLength(500, ErrorMessage = "Specification cannot exceed 500 characters.")]
        public string? Specification { get; set; }

        [StringLength(50, ErrorMessage = "Drawing No cannot exceed 50 characters.")]
        public string? DrawingNo { get; set; }

        public string? Barcode { get; set; }
        public string? QRCode { get; set; }
        public string? ImagePath { get; set; }

        [Required(ErrorMessage = "Please select Item Type (Brought-Out or Manufacturing).")]
        public string BtOtorMfg { get; set; }

        public string? Remarks { get; set; }



        // Category
        [Required(ErrorMessage = "Please select a Category.")]
        public int? CategoryCode { get; set; }

        public string? CategoryName { get; set; }
        public bool Hide { get; set; }

        public bool IslabItem { get; set; } = false;

        // Units
        [Required(ErrorMessage = "Measure Unit is required.")]
        public string MeasureUnit { get; set; }

        public string? PurchaseUnit { get; set; }

        [Range(0.0001, double.MaxValue, ErrorMessage = "Unit Convert must be greater than 0.")]
        public decimal? UnitConvert { get; set; }


        // Inventory
        public string? RackNo { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "ROL cannot be negative.")]
        public decimal? ROL { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "MOL cannot be negative.")]
        public decimal? MOL { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Lead Time must be a positive number.")]
        public int? LeadTime { get; set; }

        [Precision(18, 3)]
        public decimal PartWeight { get; set; }

        public string? BinLocation { get; set; }
        public bool ConsiderAsRm { get; set; }

        // Physical Properties
        public string? Make { get; set; }

        public int? RmId { get; set; }
        public string? RawMaterialName { get; set; }

        public string? Shape { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Density cannot be negative.")]
        public decimal Density { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Length cannot be negative.")]
        public decimal Length { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Width cannot be negative.")]
        public decimal Width { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Height cannot be negative.")]
        public decimal Height { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Thickness cannot be negative.")]
        public decimal Thickness { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Wall Thickness cannot be negative.")]
        public decimal WallThickness { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Outer Diameter cannot be negative.")]
        public decimal OuterDia { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Inner Diameter cannot be negative.")]
        public decimal InnerDia { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue, ErrorMessage = "Weight cannot be negative.")]
        public decimal Weight { get; set; }

        public string? BatchNo { get; set; }

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Total Area must be a non-negative number.")]
        public decimal TotalArea { get; set; }

        // Rates
        [Precision(18, 4)]
        [Range(0, double.MaxValue, ErrorMessage = "Rate cannot be negative.")]
        public decimal Rate { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue, ErrorMessage = "Alternate Rate cannot be negative.")]
        public decimal AltRate { get; set; }


        // Tax Codes
        [Required(ErrorMessage = "HSN Code is required.")]
        [RegularExpression(@"^\d{4,8}$", ErrorMessage = "HSN Code must be 4 to 8 digits.")]
        public string HSNCode { get; set; }

        [Required(ErrorMessage = "SAC Code is required.")]
        [RegularExpression(@"^\d{4,8}$", ErrorMessage = "SAC Code must be 4 to 8 digits.")]
        public string SACCode { get; set; }

        public string? IsServcS { get; set; }
        public string? IsServcL { get; set; }

        [Range(0, 100, ErrorMessage = "CGST must be between 0 and 100%.")]
        public decimal CGST { get; set; }

        [Range(0, 100, ErrorMessage = "SGST must be between 0 and 100%.")]
        public decimal SGST { get; set; }

        [Range(0, 100, ErrorMessage = "IGST must be between 0 and 100%.")]
        public decimal IGST { get; set; }


        // Revision
        public string? Rev { get; set; }
        public int? RevItemId { get; set; }


        // Audit Fields
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }

        public List<CustomerVM> SelectedCustomers { get; set; } = new();

        ///sns <summary>
        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "LabourPrice must be a non-negative value.")]
        public decimal LabourPricePerHour { get; set; } = 0;

        [Precision(18, 3)]
      
        public decimal CycleTime { get; set; } = 60;

        [Precision(18, 3)]
       
        public decimal LabourTimeMinutes { get; set; } = 0;


        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "LabourPrice must be a non-negative value.")]
        public decimal LabourPrice { get; set; } = 0;

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "AssemblyPrice must be a non-negative value.")]
        public decimal AssemblyPrice { get; set; } = 0;
        public bool IsLabourItem { get; set; } = false;

        [Range(0, (double)decimal.MaxValue, ErrorMessage = "HandlingCharges must be between 0 and 100%.")]
        public decimal HandlingCharges { get; set; } = 0;

        public decimal? LotSize { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Delivery Days cannot be negative.")]
        public int? DeliveryDays { get; set; }

        public string? AssignedCustomerName { get; set; }

        public byte[]? DrawingImage { get; set; }

        /// </summary>


        // Child Collections
        public List<ItemCustomerAssignVM> ItemCustomerAssignVMs { get; set; } = new List<ItemCustomerAssignVM>();


        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!string.IsNullOrWhiteSpace(MeasureUnit) &&
                !string.IsNullOrWhiteSpace(PurchaseUnit) &&
                MeasureUnit != PurchaseUnit)
            {
                if (!UnitConvert.HasValue || UnitConvert <= 0)
                {
                    yield return new ValidationResult(
                        "Unit conversion value must be greater than 0 when Measure Unit and Purchase Unit are different.",
                        new[] { nameof(UnitConvert) }
                    );
                }

                if (AltRate <= 0.001m)
                {
                    yield return new ValidationResult(
                        "Alternate Rate must be greater than 0.001 when Measure Unit and Purchase Unit are different.",
                        new[] { nameof(AltRate) }
                    );
                }
            }
        }


    }

}
