using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Inventory_module
{
    public class ItemSub 
    {
        public int Id { get; set; }


        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; } 


        public int? VendorId { get; set; }

        [ForeignKey(nameof(VendorId))]
        public Vendor Vendor { get; set; }

        public int? CustomerId { get; set; }

        [ForeignKey(nameof(CustomerId))]
        public Customer Customer { get; set; }

        [Precision(18, 2)]
        [Range(0.01, double.MaxValue, ErrorMessage = "Rate must be greater than 0.01")]
        public decimal Rate { get; set; }

        [Required(ErrorMessage = "Please Select Customer Or Vendor")]
        public int ClassId { get; set; }

        [NotMapped]
        public bool IsEditable { get; set; } = false;


        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
