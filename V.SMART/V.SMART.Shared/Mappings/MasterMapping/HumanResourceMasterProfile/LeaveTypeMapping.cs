using AutoMapper;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.HumanResourceMasterProfile
{
    public class LeaveTypeMapping : Profile
    {
        public LeaveTypeMapping()
        {
            // Entity -> VM
            CreateMap<LeaveType, LeaveTypeVM> ();

            // VM -> Entity
            CreateMap<LeaveTypeVM, LeaveType>();
        }
    }
}
