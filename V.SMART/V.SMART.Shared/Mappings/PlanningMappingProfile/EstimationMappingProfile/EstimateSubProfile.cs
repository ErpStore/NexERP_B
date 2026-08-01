using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Planning.Estimaton;
using V.SMART.Shared.ViewModels.PlanningViewModel.EstimationViewModel;

namespace V.SMART.Shared.Mappings.PlanningMappingProfile.EstimationMappingProfile
{
    public class EstimateSubProfile : Profile
    {
        public EstimateSubProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<EstimateSub, EstimateSubVM>()
                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty));
                

            // ===================== VM -> Entity =====================
            CreateMap<EstimateSubVM, EstimateSub>()
                .ForMember(dest => dest.Process, opt => opt.Ignore()); // handled separately
        }

    }
}
