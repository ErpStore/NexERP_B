using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.HumanResourceMaster_Module
{
    public class DailyLeaveRecord
    {
        [Key]
        public int DailyLeaveRecordID { get; set; }
        public DateTime Date { get; set; }
        public bool IsWeekend { get; set; }
        public string DayType { get; set; }
        public int LeaveApplicationID { get; set; }

    }

}
