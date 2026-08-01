
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;

namespace V.SMART.Shared.Data.CashFlow.ServiceBills
{
    public class ServiceBillsSub
    {
        [Key]
        public int ServiceBillSubId { get; set; }


        // -------------------- Quote --------------------
        [Required]
        public int ServiceId { get; set; }
        [ForeignKey(nameof(ServiceId))]
        public ServiceBills? ServiceBills { get; set; }

        // -------------------- Line Info --------------------
        [Required]
        public int SlNo { get; set; }


        public string? LineId { get; set; }
        // -------------------- Item --------------------
        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }


        // -------------------- Quantity --------------------
        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal Qty { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,3)")]
        public decimal BalQty { get; set; }


        // -------------------- Pricing --------------------
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal LineGross { get; set; }

        // -------------------- Discount --------------------
        [Column(TypeName = "decimal(18,3)")]
        public decimal? LineDiscountPercent { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LineDiscountAmount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal LineBasicAmount { get; set; }

        // -------------------- GST --------------------
        [Column(TypeName = "decimal(18,4)")]
        public decimal? LineCGSTRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LineSGSTRate { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        public decimal? LineIGSTRate { get; set; }


        //-------------------- Reference PO --------------------
        public int? RefMfgPoSubId { get; set; }
        [ForeignKey(nameof(RefMfgPoSubId))]
        public virtual MfgPoSub? MfgPoSub { get; set; }


        public int? RefPurchPoSubId { get; set; }
        [ForeignKey(nameof(RefPurchPoSubId))]
        public virtual PurchPoSub? PurchPoSub { get; set; }

        public string? RefPoNo { get; set; }
        public DateTime? RefPoDate { get; set; }
        public DateTime? PoDueDate { get; set; }
        public string TypeOfPo { get; set; } = string.Empty;


        // -------------------- Cost Center --------------------
        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public CostCenter? CostCenter { get; set; }

        // -------------------- Cancel --------------------
        [Required]
        public bool ItemCancel { get; set; }
        public string? ItemCancelReason { get; set; }
        public string? Remarks { get; set; }
    }
}
