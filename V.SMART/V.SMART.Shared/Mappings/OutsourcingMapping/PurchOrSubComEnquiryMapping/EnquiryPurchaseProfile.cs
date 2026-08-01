using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchOrSubComEnquiryMapping
{
    public class EnquiryPurchaseProfile : Profile
    {
        public EnquiryPurchaseProfile()
        {
            CreateMap<EnquiryPurchase, EnquiryPurchaseVM>()
               // Child Lists
               .ForMember(dest => dest.EnquiryPurchaseSubVMs,
                   opt => opt.MapFrom(src => src.EnquiryPurchaseSub))
               .ForMember(dest => dest.EnquiryPurchaseVendorAssignVMs,
                   opt => opt.MapFrom(src => src.EnquiryPurchaseVendorAssigns));

            // ------------------------------------------------------------
            // VIEWMODEL → ENTITY
            // ------------------------------------------------------------
            CreateMap<EnquiryPurchaseVM, EnquiryPurchase>()
                .ForMember(dest => dest.EnquiryPurchaseSub, opt => opt.Ignore())
                .ForMember(dest => dest.EnquiryPurchaseVendorAssigns, opt => opt.Ignore());
        }
    }
}
