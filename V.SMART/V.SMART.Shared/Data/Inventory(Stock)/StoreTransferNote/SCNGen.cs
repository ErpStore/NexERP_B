using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote
{
    using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
    using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class SCNGen
    {
        [Key]
        public int SCNGenId { get; set; }

        [Required]
        public string SCNGenNo { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public String Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime SCNGenDate { get; set; } = DateTime.Now;

        [Required]
        public int StoreId { get; set; }

        [ForeignKey(nameof(StoreId))]
        public Store? Store { get; set; }

        public bool IsWithBom { get; set; } = false;

        public int? AssyId { get; set; }

        [ForeignKey(nameof(AssyId))]
        public Item? AssyItem { get; set; }

        public int? SubAssyId { get; set; }

        [ForeignKey(nameof(SubAssyId))]
        public Item? SubAssyItem { get; set; }


        public int? SubAssyId2 { get; set; }

        [ForeignKey(nameof(SubAssyId2))]
        public Item? SubAssyItem2 { get; set; }


        public int? SubAssyId3 { get; set; }

        [ForeignKey(nameof(SubAssyId3))]
        public Item? SubAssyItem3 { get; set; }



        [Required]
        [Precision(18, 3)]
        public decimal ReqQty { get; set; }

        [StringLength(500, ErrorMessage = "Main Remark cannot exceed 500 characters.")]
        public string? MainRemark { get; set; }

        [Required]
        public int NoOfItems { get; set; }


        [Required]
        public string CreatedBy { get; set; } = string.Empty;
        [Required]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(100, ErrorMessage = "Modified By cannot exceed 50 characters.")]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<SCNGenSub> SCNGenSubs { get; set; } = new List<SCNGenSub>();
    }


}
