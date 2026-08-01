using AutoMapper;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    public class StaffChildMapping : Profile
    {
        public StaffChildMapping()
        {
            // Education
            CreateMap<StaffEducation, StaffEducationVM>().ReverseMap();

            // Emergency
            CreateMap<StaffEmergency, StaffEmergencyVM>().ReverseMap();

            CreateMap<StaffFamilyDetails, StaffFamilyVM>().ReverseMap();
        }
    }
}
