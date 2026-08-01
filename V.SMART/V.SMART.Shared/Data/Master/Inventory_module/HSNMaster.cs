using System.ComponentModel.DataAnnotations;

namespace V.SMART.Shared.Data.Master.Inventory
{
    public class HSNMaster
    {
        [Key]
        public int Slno { get; set; }

        [Required]
        public string HSNCategoryCode { get; set; }

        [Required]
        public string HSNcode { get; set; }

        public bool Required { get; set; }

        public string? HSNCategory { get; set; }
    }
}
