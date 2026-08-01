namespace V.SMART.Shared.Data.Master.Inventory
{
    using V.SMART.Shared.Data.Master.Inventory_module;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Item : IValidatableObject
    {
        [Key]
        public int ItemId { get; set; }


        [Required]
        [MaxLength(250)]
        public string ItemCode { get; set; }

        [Required]
        [StringLength(250)]
        public string ItemName { get; set; }


        [StringLength(500)]
        public string? Specification { get; set; }

        [StringLength(50)]
        public string? DrawingNo { get; set; }

        public string? Barcode { get; set; }
        public string? QRCode { get; set; }
        public string? ImagePath { get; set; }

        [Required]
        public string BtOtorMfg { get; set; }

        [StringLength(1000)]
        public string? Remarks { get; set; }


        [Required(ErrorMessage = "Please select a Category")]
        public int? CategoryCode { get; set; }

        [ForeignKey(nameof(CategoryCode))]
        public Category? Category { get; set; }

        public bool IslabItem { get; set; }   = false;
        public bool Hide { get; set; }

        [Required(ErrorMessage = "Measure Unit is required.")]
        public string MeasureUnit { get; set; }

        [ForeignKey(nameof(MeasureUnit))]
        public UOM? UOM { get; set; }

        public string? PurchaseUnit { get; set; }

        [Precision(18, 4)]

        [Range(0.0001, double.MaxValue, ErrorMessage = "Unit conversion factor must be greater than 0.")]
        public decimal? UnitConvert { get; set; }


        // Inventory

        [MaxLength(50)]
        public string? RackNo { get; set; }

        [Precision(18, 3)]
        public decimal? ROL { get; set; }

        [Precision(18, 3)]
        public decimal? MOL { get; set; }


        [Range(0, int.MaxValue, ErrorMessage = "Lead Time must be a non-negative number.")]
        public int? leadTime { get; set; }


        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Part Weight must be a non-negative number.")]
        public decimal PartWeight { get; set; }

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Total Area must be a non-negative number.")]
        public decimal TotalArea { get; set; }

        public string? BinLocation { get; set; }
        public bool ConsiderAsRm { get; set; }


        // Physical Properties
        [MaxLength(100)]
        public string? Make { get; set; }

        public int? RmId { get; set; }

        [ForeignKey(nameof(RmId))]
        public RawMaterial? RawMaterial { get; set; }


        public string? Shape { get; set; }

        [Precision(18, 3)]
        public decimal Density { get; set; }

        [Precision(18, 3)]
        public decimal Length { get; set; }

        [Precision(18, 3)]
        public decimal Width { get; set; }

        [Precision(18, 3)]
        public decimal Height { get; set; }

        [Precision(18, 3)]
        public decimal Thickness { get; set; }

        [Precision(18, 3)]
        public decimal WallThickness { get; set; }

        [Precision(18, 3)]
        public decimal OuterDia { get; set; }

        [Precision(18, 3)]
        public decimal InnerDia { get; set; }

        [Precision(18, 3)]
        public decimal Weight { get; set; }
        public string? BatchNo { get; set; }

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Rate must be a non-negative value.")]
        public decimal Rate { get; set; }


        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "Alternate Rate must be a non-negative value.")]
        public decimal AltRate { get; set; }

        [Required(ErrorMessage = "HSNCode is Required")]
        [RegularExpression(@"^\d{4,8}$", ErrorMessage = "HSN Code must be 4 to 8 digits.")]
        public string HSNCode { get; set; }

        [Required(ErrorMessage = "SACCode is Required")]
        [RegularExpression(@"^\d{4,8}$", ErrorMessage = "SAC Code must be 4 to 8 digits.")]
        public string SACCode { get; set; }

        public string? IsServcS { get; set; }
        public string? IsServcL { get; set; }

        [Precision(5, 3)]
        [Range(0, 100, ErrorMessage = "CGST must be between 0 and 100")]
        public decimal CGST { get; set; }

        [Precision(5, 3)]
        [Range(0, 100, ErrorMessage = "SGST must be between 0 and 100")]
        public decimal SGST { get; set; }

        [Precision(5, 3)]
        [Range(0, 100, ErrorMessage = "IGST must be between 0 and 100")]
        public decimal IGST { get; set; }

        // Revision
        public string? Rev { get; set; }
        public int? RevItemId { get; set; }

        [ForeignKey(nameof(RevItemId))]
        public Item? RevItem { get; set; }

        /// SNS<summary>
        [Precision(18, 3)]
    
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "LabourPricePerHour must be a non-negative value.")]
        public decimal LabourPricePerHour { get; set; } = 0;

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "CycleTime must be a non-negative value.")]
        public decimal CycleTime { get; set; } = 60;

        [Precision(18, 3)]
      
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "LabourTimeMinutes must be a non-negative value.")]
        public decimal LabourTimeMinutes { get; set; } = 0;


        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "LabourPrice must be a non-negative value.")]
        public decimal LabourPrice { get; set; } = 0;

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "AssemblyPrice must be a non-negative value.")]
        public decimal AssemblyPrice { get; set; } = 0;
        public bool IsLabourItem { get; set; } = false;

    
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "HandlingCharges must be a non-negative value.")]
        public decimal HandlingCharges { get; set; } = 0;

        [Precision(18, 3)]
        [Range(0, (double)decimal.MaxValue, ErrorMessage = "LotSize must be a non-negative value.")]
        public decimal? LotSize { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Delivery Days cannot be negative.")]
        public int? DeliveryDays { get; set; }


        /// </summary>


        // Audit
        public DateTime? CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }



        //Child Collections
        public ICollection<ItemCustomerAssign> ItemCustomerAssigns { get; set; } = new List<ItemCustomerAssign>();



        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Ensure both units are provided first
            if (!string.IsNullOrWhiteSpace(MeasureUnit) &&
                !string.IsNullOrWhiteSpace(PurchaseUnit) &&
                MeasureUnit != PurchaseUnit)
            {
                // UnitConvert required when units differ
                if (!UnitConvert.HasValue || UnitConvert == 0)
                {
                    yield return new ValidationResult("Unit conversion value is required when Measure Unit and Purchase Unit are different.",
                        new[] { nameof(UnitConvert) }
                    );
                }

                // AltRate must also be required
                if (AltRate <= 0)
                {
                    yield return new ValidationResult("Alternate Rate must be greater than 0.001 when Measure Unit and Purchase Unit are different.",
                        new[] { nameof(AltRate) }
                    );
                }
            }
        }
    }
}
