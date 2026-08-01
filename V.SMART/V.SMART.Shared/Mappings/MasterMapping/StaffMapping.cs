using AutoMapper;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    public class StaffMapping : Profile
    {
        public StaffMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<Staff, StaffVM>()
                .ForMember(dest => dest.Educations, opt => opt.MapFrom(src => src.StaffEducations))
                .ForMember(dest => dest.Emergencies, opt => opt.MapFrom(src => src.StaffEmergencies))
                .ForMember(dest => dest.Families, opt => opt.MapFrom(src => src.StaffFamilyDetails))
                .ReverseMap()

                // Ignore navigation tables while saving (handled manually)
                .ForMember(dest => dest.StaffEducations, opt => opt.Ignore())
                .ForMember(dest => dest.StaffEmergencies, opt => opt.Ignore())
                .ForMember(dest => dest.StaffFamilyDetails, opt => opt.Ignore());
        }
    }
}
