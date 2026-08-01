using AutoMapper;
using V.SMART.Shared.Data.Maintenance.MaintenanceSchedule;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.MaintenanceScheduleVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MaintenanceMap.MaintenanceScheduleMap
{
    public class MaintenanceScheduleMap:Profile
    {
        public MaintenanceScheduleMap()
        {
            //====================== Entity -> VM =====================
            CreateMap<MaintenanceSchedule, MaintenanceScheduleVM>()
                .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.MachineName : String.Empty));

            //====================== VM -> Entity =====================
            CreateMap<MaintenanceScheduleVM, MaintenanceSchedule>()
                .ForMember(dest => dest.Machine, opt => opt.Ignore());
        }

              
    }
}
