using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionIssueCompMap
{
    public class ProductionIssueCompSubMapping : Profile
    {
        public ProductionIssueCompSubMapping()
        {

            // ===================== Entity -> VM =====================
            CreateMap<ProductionIssueCompSub, ProductionIssueCompSubVM>()

                .ForMember(dest => dest.RcNo,
                    opt => opt.MapFrom(src =>
                        src.ComponentRouteCardSub != null
                            ? src.ComponentRouteCardSub.RouteCard.RCNo + "" + src.ComponentRouteCardSub.RouteCard.Suffix
                            : string.Empty))

               .ForMember(dest => dest.IsBOM,
                    opt => opt.MapFrom(src =>
                        src.ComponentRouteCardSub != null &&
                        src.ComponentRouteCardSub.IsBOM))

                .ForMember(dest => dest.RcDate,
                    opt => opt.MapFrom(src =>
                        src.ComponentRouteCardSub.RouteCard != null
                            ? src.ComponentRouteCardSub.RouteCard.RCDate
                            : (DateTime?)null))

                .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.MfgPo.PONo + "" + src.MfgPoSub.MfgPo.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub.MfgPo != null
                            ? src.MfgPoSub.MfgPo.PODate
                            : (DateTime?)null))

                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty))

                .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.MachineName : string.Empty))

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));


            // ===================== VM -> Entity =====================
            CreateMap<ProductionIssueCompSubVM, ProductionIssueCompSub>()
                .ForMember(dest => dest.ProductionIssueComp, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentRouteCardSub, opt => opt.Ignore())
                .ForMember(dest => dest.Process, opt => opt.Ignore())
                .ForMember(dest => dest.Machine, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());


        }
    }
}
