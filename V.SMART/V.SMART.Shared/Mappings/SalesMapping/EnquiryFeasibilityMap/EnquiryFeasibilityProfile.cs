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
    public class EnquiryFeasibilityProfile : Profile
    {
        public EnquiryFeasibilityProfile()
        {
            // Entity -> VM
            CreateMap<EnquiryFeasibility, EnquiryFeasibilityVM>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
                .ForMember(dest => dest.CustomerAddress, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))
                .ForMember(dest => dest.CustomerPhoneNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))
                .ForMember(dest => dest.CustomerGSTNO, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))
                .ForMember(dest => dest.EnquiryFeasibilitySubVM, opt => opt.MapFrom(src => src.EnquiryFeasibilitySub));

            // VM -> Entity
            CreateMap<EnquiryFeasibilityVM, EnquiryFeasibility>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.EnquiryFeasibilitySub, opt => opt.Ignore()); // handled manually
        }
    }

}
