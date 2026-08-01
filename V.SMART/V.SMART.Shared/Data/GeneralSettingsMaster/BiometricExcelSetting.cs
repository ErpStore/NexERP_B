using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.GeneralSettingsMaster
{
    public class BiometricExcelSetting
    {
        [Key]
        public int Id { get; set; }
        public int MonthDateRow { get; set; }
        public int MonthDateColumn { get; set; }
        public string? StatusName { get; set; }

        public string? InTimeName { get; set; }
        public string? OutTimeName { get; set; }
        public string? TotalWorkedHrs { get; set; }

        public int InTimeNameStartRow { get; set; }
        public int InTimeDataStartCol { get; set; }
        public int EmpCodeIndexFromInTime { get; set; }

        public int StatusNameIndexFromInTime { get; set; }
        public int OutTimeIndexFromInTime { get; set; }
        public int TotalWorkHrsIndexFromInTime { get; set; }
        public int EmpIdColIndex { get; set; }
        public int HeaderInTimeStartCol { get; set; }

        public string? P { get; set; }
        public string? H { get; set; }
        public string? A { get; set; }
        public string? L { get; set; }
        public string? W { get; set; }
        public string? M { get; set; }
        public string? N { get; set; }

        public string? O { get; set; }

        //public string? PrefixBfrID { get; set; }
        //public string? SplCharAfterID { get; set; }
        public string? OTName { get; set; }
        public int OTNameIndexFromInTime { get; set; }
    }
}
