using V.SMART.Shared.Data.GeneralSettingsMaster;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Production.DailyProductionLog
{
    public class ProductionLogSub
    {
        [Key]
        public long LogSubId { get; set; }

        [Required]
        public long LogId { get; set; }
        [ForeignKey(nameof(LogId))]
        public virtual ProductionLog? ProuctionLog { get; set; }

        [Required]
        public int LogSettingId { get; set; }
        public virtual ProductionLogSetting? ProductionStopageSettings { get; set; }

        public TimeSpan? StopageTime { get; set; }
    }
}
