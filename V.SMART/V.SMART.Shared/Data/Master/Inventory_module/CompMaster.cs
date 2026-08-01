using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Inventory_module
{
    public class CompMaster
    {
        [Key]
        public int Id { get; set; }
        public int? CompItemId { get; set; }
        [ForeignKey(nameof(CompItemId))]
        public Item? ComponentItem { get; set; }

        [Required(ErrorMessage = "Please select Raw-Material")]
        public int? RMId { get; set; }

        [ForeignKey(nameof(RMId))]
        public Item? RawMaterial { get; set; }

        [Precision(18, 3)]
        [Required(ErrorMessage = "Please Enter Raw-Material Weight")]
        public decimal Weight { get; set; }

        public decimal RMCuttingSize { get; set; }
        public string? OtherDet { get; set; }
        public bool LabWORM { get; set; } = false;
        public bool IsDefaultRM { get; set; }

        [Precision(18, 3)]
        public decimal EBLength { get; set; }

        [Precision(18, 3)]
        public decimal EBWidth { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        [StringLength(100)]
        public string? ModifiedBy { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<ProcessFlowChart> ProcessFlowCharts { get; set; } = new List<ProcessFlowChart>();

    }
}
