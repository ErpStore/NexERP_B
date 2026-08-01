using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.Master.Inventory;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace V.SMART.Shared.Data.Master.Inventory_module
{
    public class ProcessFlowChart
    {
        [Key]
        public int SubId { get; set; }

        public int Id { get; set; }
        [ForeignKey(nameof(Id))]
        public CompMaster CompMaster { get; set; }

        public int CompItemId { get; set; }

        [ForeignKey(nameof(CompItemId))]
        public Item? ComponentItem { get; set; }

        public int RMId { get; set; }

        [ForeignKey(nameof(RMId))]
        public Item? RawMaterialItem { get; set; }

        [Required]
        public int SlNo { get; set; }

        [Required(ErrorMessage = "Please select the Process.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid Process.")]
        public int ProcId { get; set; }
        [ForeignKey("ProcId")]
        public virtual Process? Process { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "RM Weight must be a non-negative number.")]
        [Precision(18, 2)]
        public decimal RmWt { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Component Weight must be a non-negative number.")]
        [Precision(18, 2)]
        public decimal CompWt { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Scrap Weight must be a non-negative number.")]
        [Precision(18, 2)]
        public decimal ScrapWt { get; set; }

        public int? ScrapEndBit { get; set; }

        [ForeignKey(nameof(ScrapEndBit))]
        public RawMaterial? RawMaterial { get; set; }

        public int? MachId { get; set; }
        [ForeignKey(nameof(MachId))]
        public Machine? Machine { get; set; }

        public int? VendorCode { get; set; }
        [ForeignKey(nameof(VendorCode))]
        public Vendor? Vendor { get; set; }

        public string? Remarks { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "UnitPrice must be a non-negative number.")]
        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }
        public string? Program { get; set; }


        [Required(ErrorMessage = "Please select Cycle Time")]
        public TimeSpan CycleTime { get; set; }
        public TimeSpan SettingTime { get; set; }


        [Required(ErrorMessage = "Please select Sub Process.")]
        [Range(1, int.MaxValue, ErrorMessage = "Sub Process must start from 1.")]
        public int SubProcess { get; set; }
        public bool UseSameSubProcess { get; set; } = false;

        // Optional: GroupKey for SubProcess logic
        public string? GroupKey { get; set; }

        public bool BOM { get; set; } = false;
        public bool Inspection { get; set; } = false;
        public bool ProcessSkip { get; set; } = false;





    }
}
