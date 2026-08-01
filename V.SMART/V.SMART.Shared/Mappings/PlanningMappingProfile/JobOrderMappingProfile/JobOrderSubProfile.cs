using AutoMapper;
using V.SMART.Shared.Data.Planning.AssyJobOrder;
using V.SMART.Shared.ViewModels.PlanningViewModel.JobOrderViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.PlanningMappingProfile.JobOrderMappingProfile
{
    public class JobOrderSubProfile : Profile
    {
        public JobOrderSubProfile()
        {
            // ================== Entity -> VM ==================
            CreateMap<JobOrderSub, JobOrderSubVM>()
                .ForMember(dest => dest.ItemCode,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.Measureunit,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))

                .ForMember(dest => dest.SelectedItem, opt => opt.Ignore());

            // ================== VM -> Entity ==================
            CreateMap<JobOrderSubVM, JobOrderSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore());

        }
    }
}
