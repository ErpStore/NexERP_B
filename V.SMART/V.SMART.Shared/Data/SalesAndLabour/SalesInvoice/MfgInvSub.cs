using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
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

namespace V.SMART.Shared.Data.SalesAndLabour.SalesInvoice
{
    public class MfgInvSub
    {
        [Key]
        public int InvSubId { get; set; }

        public int InvId { get; set; }
        [ForeignKey(nameof(InvId))]
        public virtual MfgInv MfgInv { get; set; }

        public int SlNo { get; set; }

        public int? RefDcSubId { get; set; }
        [ForeignKey(nameof(RefDcSubId))]
        public MfgDcSub? MfgDcSub { get; set; }


        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public MfgPoSub? MfgPoSub { get; set; }


        [Required(ErrorMessage = "Please select ItemCode")]
        public int? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }


        [Required(ErrorMessage = "Please Enter the Quantity")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Precision(18, 3)]
        public decimal Qty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Precision(18, 3)]
        public decimal? CrBalQty { get; set; }

        [Required(ErrorMessage = "Please Enter the Price")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        [Precision(18, 4)]
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

        public bool ItemCancel { get; set; } = false;

        [StringLength(150)]
        public string? ItemCancelReason { get; set; }

        [StringLength(200)]
        public string? Remarks { get; set; }

        [StringLength(200)]
        public string? RcSubIds { get; set; }

        public ICollection<CreditNoteSub> CreditNoteSubs { get; set; }
    }
}
