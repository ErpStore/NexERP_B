using V.SMART.Shared.Data.Master.General;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Inventory
{
    public class ItemProductAssign
    {
        [Key]
        public int Id { get; set; }

        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item Item { get; set; }

        [Required(ErrorMessage = "Customer selection is required.")]
        public int? CustId { get; set; }

        [ForeignKey(nameof(CustId))]
        public Customer Customer { get; set; }

        [Required(ErrorMessage = "Product Name is required.")]
        [StringLength(200, ErrorMessage = "Product Name cannot exceed 200 characters.")]
        public string ProductName { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [RegularExpression(@"^\d{4,8}$", ErrorMessage = "HSN Code must be 4 to 8 digits.")]
        public string? HSNCode { get; set; }


        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }


        // Local use only (not mapped to DB)
        [NotMapped]
        public bool IsEditable { get; set; } = false;
    }

}
