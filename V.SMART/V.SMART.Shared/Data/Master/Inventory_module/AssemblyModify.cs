using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.Inventory_module
{
    public class AssemblyModify
    {
        [Key]
        public int AssyModifyId { get; set; }

        [Required]
        public int RecNo { get; set; }

        public int? AssyId { get; set; }

        [ForeignKey(nameof(AssyId))]
        public virtual Item? Assy { get; set; }
        public int? ItemId { get; set; }

        [ForeignKey(nameof(ItemId))]
        public virtual Item? Item { get; set; }

        [Precision(18,3)]
        public decimal UtilityQty { get; set; }
        public string? Remark { get; set; }

        public bool Cancel { get; set; }
        public int? ProcessId { get; set; }

        [ForeignKey(nameof(ProcessId))]
        public Process? Process { get; set; }

        [StringLength(100)]
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

    }
}
