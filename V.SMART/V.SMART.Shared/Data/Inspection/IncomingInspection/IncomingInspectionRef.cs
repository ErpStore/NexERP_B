using V.SMART.Shared.Data.Master.Inventory;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Inspection.IncomingInspection
{
    public class IncomingInspectionRef
    {
        [Key]
        public int Id { get; set; } = 0;

        public int ItemID { get; set; } = 0;
        [ForeignKey(nameof(ItemID))]
        public virtual Item? Item { get; set; }

        public int ProcessId { get; set; }

        [Column(TypeName = "NVARCHAR(MAX)")]
        public string? InsRefData { get; set; }

        public short TotRows { get; set; } = 0;
    }
}
