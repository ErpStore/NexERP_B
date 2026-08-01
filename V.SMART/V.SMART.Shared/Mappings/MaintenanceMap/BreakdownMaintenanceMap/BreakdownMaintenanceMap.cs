using AutoMapper;
using V.SMART.Shared.Data.Maintenance.BreakdownMaintenance;
using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.BreakdownMaintenanceVM;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MaintenanceMap.BreakdownMaintenanceMap
{
    public class BreakdownMaintenanceMap:Profile
    {
        public BreakdownMaintenanceMap() {

            //====================== Entity -> VM =====================
            CreateMap<BreakdownMaintenance, BreakdownMaintenanceVM>()
                .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.MachineName : String.Empty));

            //====================== VM -> Entity =====================
            CreateMap<BreakdownMaintenanceVM, BreakdownMaintenance>()
                .ForMember(dest => dest.Machine, opt => opt.Ignore());
        
        }
    }
}
