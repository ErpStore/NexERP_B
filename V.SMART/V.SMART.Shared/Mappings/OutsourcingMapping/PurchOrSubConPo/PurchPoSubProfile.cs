using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchOrSubConPo
{
    public class PurchPoSubProfile : Profile
    {
        public PurchPoSubProfile()
        {
            // Entity -> VM
            CreateMap<PurchPoSub, PurchPoSubVM>()
                .ForMember(dest => dest.SelectedRouteCard, opt => opt.Ignore())
                .ForMember(dest => dest.RouteCards, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))
                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty))
                .ForMember(dest => dest.RefRcNo, opt => opt.MapFrom(src => src.RouteCardS != null ? src.RouteCardS.RCNo + src.RouteCardS.Suffix : string.Empty))

                 .ForMember(dest => dest.RefMReqNo,
                        opt => opt.MapFrom(src =>
                            src.MaterialReqSub != null
                                ? src.MaterialReqSub.MaterialReq.MReqNo + "" + src.MaterialReqSub.MaterialReq.Suffix
                                : string.Empty))

                .ForMember(dest => dest.RefMReqDate,
                    opt => opt.MapFrom(src =>
                        src.MaterialReqSub != null
                            ? src.MaterialReqSub.MaterialReq.MreqDate
                            : (DateTime?)null))

                 .ForMember(dest => dest.RefQuoteNo,
                    opt => opt.MapFrom(src =>
                        src.PurchaseQuoteSub != null
                            ? src.PurchaseQuoteSub.PurchaseQuote.QuoteNo + "" + src.PurchaseQuoteSub.PurchaseQuote.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefQuoteDate,
                    opt => opt.MapFrom(src =>
                        src.PurchaseQuoteSub != null
                            ? src.PurchaseQuoteSub.PurchaseQuote.QuoteDate
                            : (DateTime?)null));

            // VM -> Entity
            CreateMap<PurchPoSubVM, PurchPoSub>()
                 .ForMember(dest => dest.Item, opt => opt.Ignore())
                 .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                 .ForMember(dest => dest.Process, opt => opt.Ignore())
                 .ForMember(dest => dest.PurchaseQuoteSub, opt => opt.Ignore())
                 .ForMember(dest => dest.RouteCardS, opt => opt.Ignore())
                 .ForMember(dest => dest.RouteCardSub, opt => opt.Ignore())
                 // IMPORTANT
                 .ForMember(dest => dest.PurchPo, opt => opt.Ignore())
                 .ForMember(dest => dest.MaterialReqSub, opt => opt.Ignore());
        }
    }
}
