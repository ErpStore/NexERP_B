using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour.SalesPo
{
    public class MfgPoSub
    {
        [Key]
        public int PoSubId { get; set; }

        [Required]
        public int PoId { get; set; }
        [ForeignKey(nameof(PoId))]
        public virtual MfgPo MfgPo { get; set; }

        [Required]
        public int SlNo { get; set; }
        public int? LineNo { get; set; }

        [Required]
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; }

        [StringLength(500)]
        public string? ItemSpecification { get; set; }

        public int? RefQuoteSubId { get; set; }
        [ForeignKey(nameof(RefQuoteSubId))]
        public virtual MfgQuoteSub MfgQuoteSub { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal Qty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal BalQty { get; set; }

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

        [Required]
        public DateTime DueDate { get; set; } = DateTime.Now;

        [Required]
        public DateTime PoDate { get; set; } = DateTime.Now;


        public string? WoNO { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal IndentBalQty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal WOBalQty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal PerformaBalQty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal RouteCardBalQty { get; set; }

        [Precision(18, 3)]
        [Range(0, double.MaxValue)]
        public decimal ProdCompBalQty { get; set; }

        public bool ItemCancel { get; set; } = false;
        public string? ItemCancelReason { get; set; }

        public bool IsAnnexure { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter CostCenter { get; set; }



        [StringLength(500)]
        public string? Remark { get; set; }
    }

}
