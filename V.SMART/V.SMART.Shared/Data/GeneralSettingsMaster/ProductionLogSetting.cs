using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.GeneralSettingsMaster
{
    public class ProductionLogSetting
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Production Stop Type is required.")]
        [StringLength(50, ErrorMessage = "ProdStopType cannot exceed 50 characters.")]
        public string ProdStopType { get; set; } = string.Empty;


        [Range(0, 100, ErrorMessage = "Efficiency Percentage must be between 0 and 100.")]
        public int EfficiencyPerc { get; set; } // 100 = No loss, 0 = Full loss for this stop type

        [StringLength(255)]
        public string? Definition { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
