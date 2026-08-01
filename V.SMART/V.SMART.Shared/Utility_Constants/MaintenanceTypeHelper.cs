using V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Utility_Constants
{
    public static class MaintenanceTypeHelper
    {
        public static IReadOnlyList<ScheduleNameVM> GetMaintenanceTypes()
        {
            return new List<ScheduleNameVM>
            {
                new() { Id = 1, Name = "Daily" },
                new() { Id = 2, Name = "Weekly" },
                new() { Id = 3, Name = "Fortnightly" },
                new() { Id = 4, Name = "Monthly" },
                new() { Id = 5, Name = "Quarterly" },
                new() { Id = 6, Name = "Bi-Annual" },
                new() { Id = 7, Name = "Annual" }
            };
        }

    }
}
