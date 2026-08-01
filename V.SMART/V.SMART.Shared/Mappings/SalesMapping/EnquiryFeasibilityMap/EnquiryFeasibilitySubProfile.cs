using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.Fesibility;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.FeasibilityVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.EnquiryFeasibilityMap
{
    public class EnquiryFeasibilitySubProfile : Profile
    {
        public EnquiryFeasibilitySubProfile()
        {
            // Entity -> VM
            CreateMap<EnquiryFeasibilitySub, EnquiryFeasibilitySubVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty));

            // VM -> Entity
            CreateMap<EnquiryFeasibilitySubVM, EnquiryFeasibilitySub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore());
        }
    }

}
