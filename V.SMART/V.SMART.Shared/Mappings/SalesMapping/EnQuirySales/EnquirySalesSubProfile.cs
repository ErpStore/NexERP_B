using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.EnQuirySales
{
    public class EnquirySalesSubProfile : Profile
    {
        public EnquirySalesSubProfile()
        {
            // Entity -> VM
            CreateMap<EnquirySalesSub, EnquirySalesSubVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HsnCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));

            // VM -> Entity
            CreateMap<EnquirySalesSubVM, EnquirySalesSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());
        }


    }
}
