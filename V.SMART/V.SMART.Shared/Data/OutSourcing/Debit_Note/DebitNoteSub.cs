
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.OutSourcing.SubContractInvoice;

namespace V.SMART.Shared.Data.OutSourcing.Debit_Note
{
    public class DebitNoteSub
    {
        [Key]
        public int DbSubId { get; set; }

        public int DbId { get; set; }
        [ForeignKey(nameof(DbId))]
        public virtual DebitNote DebitNote { get; set; }

        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select ItemCode")]
        public int? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [Required(ErrorMessage = "Please Enter the Quantity")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Precision(18, 3)]
        public decimal Qty { get; set; }

        [Required(ErrorMessage = "Please Enter the Unit Price")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Unit Price must be greater than 0.")]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }


        [Precision(18, 3)]
        public decimal RejQty { get; set; }

        [Precision(18, 3)]
        public decimal RewQty { get; set; }


        [Precision(18, 3)]
        public decimal CrDrQty { get; set; }

        [Precision(18, 3)]
        public decimal CrDrUnitPrice { get; set; }


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

        [StringLength(200)]
        public string? Remarks { get; set; }

        public int? RefPurchInvSubId { get; set; }
        [ForeignKey(nameof(RefPurchInvSubId))]
        public PurchaseInvoiceSub? PurchaseInvoiceSub { get; set; }


        public int? RefSubConInvSubId { get; set; }
        [ForeignKey(nameof(RefSubConInvSubId))]
        public SubConInvSub? SubConInvSub { get; set; }

        public int? RefPoSubId { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

    }
}
