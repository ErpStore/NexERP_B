using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.ViewModels.MaintenanceViewModel.CalibrationHistoryAndMaintenanceVM
{
    public class CalibrationDueModelVM
    {
        public int SerialNo { get; set; }
        public string ItemName { get; set; }
        public string UniqueId { get; set; }
        public DateTime DueDate { get; set; }
        public int DaysLeft { get; set; }
        public string DueStatus { get; set; }
    }
}
