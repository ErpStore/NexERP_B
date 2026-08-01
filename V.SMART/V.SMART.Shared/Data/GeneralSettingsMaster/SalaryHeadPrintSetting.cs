using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.GeneralSettingsMaster
{
    public class SalaryHeadPrintSetting
    {
        [Key]
        public int Id { get; set; }

        public string FieldName { get; set; } = string.Empty;

        public string PrintName { get; set; } = string.Empty;

        public bool IsPrint { get; set; }

        public int Type { get; set; }

        public int? StrRow { get; set; }

        public int? StrCol { get; set; }

        public int? VertCol { get; set; }

        public int? VertRow { get; set; }
    }

}
