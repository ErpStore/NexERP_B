using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.Data.SalesAndLabour.LabourDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.Utility_Constants;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.SalesAndLabour_Module
{
    public class LabInvSub
    {
        [Key]
        public int LabInvSubId { get; set; }

        public int LabInvId { get; set; }
        [ForeignKey(nameof(LabInvId))]
        public LabInv LabInv { get; set; }

        public int SlNo { get; set; }
        public string? RFQ { get; set; }
        public int? RefDcSubId { get; set; }

        public LabourDcOutgoingSub? LabourDcOutgoingSub { get; set; }


        [Required(ErrorMessage = "Please select ItemCode")]
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public Item? Item { get; set; }

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

        public bool ItemCancel { get; set; } = false;
        public string? CancelItemReason { get; set; }


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

        public string? Remarks { get; set; }

        public ICollection<CreditNoteSub> CreditNoteSubs { get; set; }
    }

}
