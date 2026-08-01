using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice
{
    public class PerformaInvSub
    {
        [Key]
        public int InvSubId { get; set; }

        [Required]
        public int InvId { get; set; }
        [ForeignKey(nameof(InvId))]
        public virtual PerformaInv PerformaInv { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required]
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual MfgPoSub MfgPoSub { get; set; }

        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal Qty { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [Precision(18, 4)]
        public decimal LineGross { get; set; }

        [Precision(18, 3)]
        public decimal? LineDiscountPercent { get; set; }

        [Precision(18, 4)]
        public decimal? LineDiscountAmount { get; set; }

        [Precision(18, 4)]
        public decimal LineBasicAmount { get; set; }

        [Precision(18, 4)]
        public decimal? LineCGSTRate { get; set; }

        [Precision(18, 4)]
        public decimal? LineSGSTRate { get; set; }

        [Precision(18, 4)]
        public decimal? LineIGSTRate { get; set; }

        public DateTime DueDate { get; set; } = DateTime.Now;

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter CostCenter { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

    }

}
