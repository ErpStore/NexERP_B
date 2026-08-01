using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using V.SMART.Shared.Data.OutSourcing.Debit_Note;

namespace V.SMART.Shared.Data.OutSourcing.Purchase_Invoice
{
    public class PurchaseInvoiceSub
    {
        [Key]
        public int InvSubId { get; set; }                       // PK

        [Required]
        public int InvId { get; set; }
        [ForeignKey(nameof(InvId))]
        public PurchaseInvoice PurchaseInvoice { get; set; }// FK → PurchaseInvoice

        [Required]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select ItemCode.")]
        public int ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual PurchPoSub? PurchPoSub { get; set; }

        public int? RefSCNSubId { get; set; }
        [ForeignKey(nameof(RefSCNSubId))]
        public virtual PurchaseSCNSub? PurchaseSCNSub { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

        // -------- Quantities --------

        [Required(ErrorMessage = "Qty is Required")]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0.")]
        [Precision(18, 3)]
        public decimal Qty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Precision(18, 3)]
        public decimal RejectQty { get; set; }

        [Precision(18, 3)]
        public decimal ReworkQty { get; set; }

        [Precision(18, 3)]
        public decimal DbBalQty { get; set; }

        // -------- Pricing --------
        [Required(ErrorMessage = "UnitPrice is Required")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "UnitPrice must be greater than 0.")]
        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }
       

        // -------- Line Discounts --------
        [Precision(18, 3)]
        public decimal? LineDiscountPercent { get; set; }

        [Precision(18, 3)]
        public decimal? LineDiscountAmount { get; set; }

        [Precision(18, 3)]
        public decimal LineBasicAmount { get; set; }

        // -------- Line GST --------
        [Precision(18, 3)]
        public decimal? LineCGSTRate { get; set; }

        [Precision(18, 3)]
        public decimal? LineSGSTRate { get; set; }

        [Precision(18, 3)]
        public decimal? LineIGSTRate { get; set; }

        // -------- Remark --------
        [MaxLength(300)]
        public string? Remarks { get; set; }

        public bool ItemCancel { get; set; } = false;
        [MaxLength(200)]
        public string? CancelItemReason { get; set; }//CostId

        public ICollection<DebitNoteSub> DebitNoteSubs { get; set; }
    }
}
