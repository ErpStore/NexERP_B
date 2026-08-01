using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel
{
    public class ProductionLogSubVM
    {
        public long LogSubId { get; set; }

        [Required(ErrorMessage ="LogId is Not Mapped")]
        public long LogId { get; set; }
        [ForeignKey(nameof(LogId))]
        public virtual ProductionLog? ProuctionLog { get; set; }

        [Required(ErrorMessage ="Stopage Name Selection Required")]
        public int LogSettingId { get; set; }
        public virtual ProductionLogSetting? ProductionStopageSettings { get; set; }

        public string? ProuctionStopType { get; set; }
        public int? EfficienctyPerc { get; set; }
        public string? Defination { get; set; }

        [Required(ErrorMessage = "Please enter stoppage time.")]
        [Range(1, 1440, ErrorMessage = "Stoppage time must be between 1 and 1440 minutes.")]
        public int? StopageTime { get; set; }

    }
}
