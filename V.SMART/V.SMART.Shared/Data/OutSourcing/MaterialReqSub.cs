using DocumentFormat.OpenXml.Office.Y2022.FeaturePropertyBag;
using V.SMART.Shared.Data.Master.Accounts_Module;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.OutSourcing
{
    public class MaterialReqSub
    {
        [Key]
        public int MreqSubId { get; set; }

        public int MreqId { get; set; }
        [ForeignKey(nameof(MreqId))]
        public virtual MaterialReq MaterialReq { get; set; } = null!;

        public int SlNo { get; set; }

        public int? ItemId { get; set; }
        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        public int? CustId { get; set; }
        [ForeignKey(nameof(CustId))]
        public virtual Customer? Customer { get; set; }


        [Precision(18, 3)]
        public decimal PurchQty { get; set; }

        [Precision(18, 3)]
        public decimal BalQty { get; set; }

        [Precision(18, 4)]
        public decimal UnitPrice { get; set; }

        public DateTime DueDate { get; set; } = DateTime.Now;

        public bool ItemCancel { get; set; } = false;
        [StringLength(250)]
        public string? ItemCancelReason { get; set; }

        public int? VendorCode { get; set; }
        [ForeignKey(nameof(VendorCode))]
        public virtual Vendor? Vendor { get; set; }

        public int? CostId { get; set; }
        [ForeignKey(nameof(CostId))]
        public virtual CostCenter? CostCenter { get; set; }

        public int? RefPoSubId { get; set; }
        [ForeignKey(nameof(RefPoSubId))]
        public virtual MfgPoSub? MfgPoSub { get; set; }

        public int? RefPoId { get; set; }
        [ForeignKey(nameof(RefPoId))]
        public virtual MfgPo? MfgPo { get; set; }

        [Precision(18, 4)]
        public decimal TotalGross { get; set; }

        [StringLength(500)]
        public string? Remark { get; set; }

        public int? RefBomId { get; set; }
        [ForeignKey(nameof(RefBomId))]
        public virtual AssmblyDef? AssmblyDef { get; set; }

        public int? MainAssyId { get; set; }

        [Precision(18, 3)]
        public decimal MfgPoIndentQty { get; set; }
    }

}
