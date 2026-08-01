using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Inventory
{
    [Table("Stores")]
    public class Store
    {
        [Key]
        public int StoreId { get; set; }

        [Required(ErrorMessage = "Store Name is required.")]
        [MaxLength(250)]
        public string StoreName { get; set; }

        [MaxLength(250)]
        public string? StoreDesc { get; set; }

        public bool CanIssue { get; set; }

        public bool CanAdd { get; set; }

        public bool IsSystemDefined { get; set; } = false;

        [MaxLength(50)]
        public string? CreatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }
}
