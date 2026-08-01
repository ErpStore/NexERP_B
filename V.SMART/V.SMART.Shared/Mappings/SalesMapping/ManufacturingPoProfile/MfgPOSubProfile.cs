using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.ManufacturingPoProfile
{
    public class MfgPOSubProfile : Profile
    {
        public MfgPOSubProfile()
        {
            // Entity -> VM
            CreateMap<MfgPoSub, MfgPoSubVM>()

                .ForMember(dest => dest.RefQuoteNo,
                    opt => opt.MapFrom(src =>
                        src.MfgQuoteSub != null
                            ? src.MfgQuoteSub.MfgQuote.QuoteNo + "" + src.MfgQuoteSub.MfgQuote.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefQuoteDate,
                    opt => opt.MapFrom(src =>
                        src.MfgQuoteSub != null
                            ? src.MfgQuoteSub.MfgQuote.QuoteDate
                            : (DateTime?)null))

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.LabourCost, opt => opt.MapFrom(src => src.Item!.AssemblyPrice))
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));

            // VM -> Entity
            CreateMap<MfgPoSubVM, MfgPoSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.MfgQuoteSub, opt => opt.Ignore());
        }
    }

}
