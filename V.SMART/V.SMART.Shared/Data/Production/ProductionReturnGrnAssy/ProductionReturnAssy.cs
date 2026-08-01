using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Production.ProductionReturnGrnAssy
{
    public class ProductionReturnAssy
    {
        [Key]
        public int ReturnId { get; set; }

        public bool IsNoInspection {get; set; } = false;    
        public bool Rejection { get; set; } = false;
        public bool Return { get; set; } = false;

        [Required, MaxLength(50)]
        public string ReturnNo { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Suffix { get; set; } = string.Empty;

        [Required]
        public DateTime ReturnDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime ReturnDateNow { get; set; } = DateTime.Now;

        [MaxLength(250)]
        public string? MainRemark { get; set; }

        public string ReturnBy { get; set; } = string.Empty;
        public int? AddStoreId { get; set; }
        [ForeignKey(nameof(AddStoreId))]
        public virtual Store? AddStore { get; set; }

        public bool ReturnTally { get; set; } = false;

        public virtual ICollection<ProductionReturnAssySub>? ProductionReturnAssySubs { get; set; }
        public virtual ICollection<ProductionReturnAssyTrack>? ProductionReturnAssyTracks { get; set; }


        [MaxLength(50)]
        public string? CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }

        [MaxLength(50)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

    }
}
