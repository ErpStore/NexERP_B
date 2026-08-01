using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry
{
    public class EnquiryPurchaseSub
    {
        [Key]
        public int EnquirySubId { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int SlNo { get; set; }

        [Required]
        public int EnquiryId { get; set; }

        [ForeignKey(nameof(EnquiryId))]
        public virtual EnquiryPurchase EnquiryPurchase { get; set; } = null!;

        [Required]
        public int ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item Item { get; set; } = null!;

        [Required]
        [Precision(18, 3)]
        public decimal? Qty { get; set; }

        [Precision(18, 3)]
        public decimal? BalQty { get; set; }

        [Precision(18, 4)]
        [Range(0, double.MaxValue)]
        public decimal UnitPrice { get; set; }

        [StringLength(250)]
        public string? Remark { get; set; }

        public int? CostCenterId { get; set; }

        [ForeignKey(nameof(CostCenterId))]
        public virtual CostCenter? CostCenter { get; set; }

        public int? RefMReqSubId { get; set; }
        [ForeignKey(nameof(RefMReqSubId))]
        public virtual MaterialReqSub? MaterialReqSub { get; set; }


        public DateTime? DueDate { get; set; }

        public bool ItemCancel { get; set; } = false;

        [StringLength(500)]
        public string? ItemCancelReason { get; set; }

    }

}
