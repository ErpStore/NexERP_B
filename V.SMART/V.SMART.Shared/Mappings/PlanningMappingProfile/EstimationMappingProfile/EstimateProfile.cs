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
    public class EstimateProfile : Profile
    {
        public EstimateProfile() 
        {
            // ===================== Entity -> VM =====================
            CreateMap<Estimate, EstimateVM>()
                .ForMember(dest => dest.Custname, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                 .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                 .ForMember(dest => dest.EnquiryNo, opt => opt.MapFrom(src => src.EnquirySalesSub.EnquirySales!= null ? src.EnquirySalesSub.EnquirySales.EnquiryNo : string.Empty))
                  .ForMember(dest => dest.EstimateSubVM, opt => opt.MapFrom(src => src.EstimateSub));

            // ===================== VM -> Entity =====================
            CreateMap<EstimateVM, Estimate>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())// handled separately
                  .ForMember(dest => dest.EnquirySalesSub, opt => opt.Ignore())
                  .ForMember(dest => dest.EstimateSub, opt => opt.Ignore());



        }

    }
}
