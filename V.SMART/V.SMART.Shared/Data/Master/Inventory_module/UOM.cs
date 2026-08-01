using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Inventory
{
    public class UOM
    {
        [Key]
        public string UnitCode { get; set; }

        public string? UnitDescription { get; set; }
        public bool IsSystemDefined { get; set; } = false;
    }
}
