using V.SMART.Shared.Data.Inspection.IncomingInspection;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Production.ProductionReturnGrnAssy
{
    public class ProductionReturnAssySub
    {
        [Key]
        public int ReturnSubId { get; set; }

        public int ReturnId { get; set; }
        [ForeignKey(nameof(ReturnId))]
        public virtual ProductionReturnAssy? ProductionReturnAssy { get; set; }

        [Required]
        public int SlNo { get; set; }
        public int? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [Precision(18, 3)]
        public decimal? QtyReturned { get; set; }
        
        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Precision(18, 3)]  
        public decimal UnitPrice { get; set; }

        public int? RefJobOrderId { get; set; }
        [ForeignKey(nameof(RefJobOrderId))]
        public virtual JobOrder? JobOrder { get; set; }

        public int? RefIssueSubId { get; set; }
        [ForeignKey(nameof(RefIssueSubId))]

        public virtual ProductionIssueAssySub? ProductionIssueAssySub { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }

        [MaxLength(250)]
        public string? Remarks { get; set; }


        public int? InspectId { get; set; }
        [ForeignKey(nameof(InspectId))]
        public virtual IncomingInspection? IncomingInspection { get; set; }


    }
}
