using V.SMART.Shared.Data.Inspection.FinalInspection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Data.Master.MasterScreeenManagement_Module
{
    public class InspectionSettings
    {
            [Key]
            public int Id{ get; set; }
            public string? SettingName { get; set; }
            public int SamplesPerSheet { get; set; }
    }
}
