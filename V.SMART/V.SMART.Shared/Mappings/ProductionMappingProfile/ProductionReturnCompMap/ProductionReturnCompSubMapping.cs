using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionReturnCompMap
{
    public class ProductionReturnCompSubMapping : Profile
    {
        public ProductionReturnCompSubMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionReturnCompSub, ProductionReturnCompSubVM>()

                 .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.MfgPo.PONo + "" + src.MfgPoSub.MfgPo.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src =>
                        src.MfgPoSub != null
                            ? src.MfgPoSub.MfgPo.PODate
                            : (DateTime?)null))


                 .ForMember(dest => dest.RefIssueNo,
                    opt => opt.MapFrom(src =>
                        src.ProductionIssueCompSub != null
                            ? src.ProductionIssueCompSub.ProductionIssueComp.IssueNo + "" + src.ProductionIssueCompSub.ProductionIssueComp.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefIssueDate,
                    opt => opt.MapFrom(src =>
                        src.ProductionIssueCompSub != null
                            ? src.ProductionIssueCompSub.ProductionIssueComp.IssueDate
                            : (DateTime?)null))


                 .ForMember(dest => dest.RefRcNo,
                    opt => opt.MapFrom(src =>
                        src.RouteCardSubs != null
                            ? src.RouteCardSubs.RouteCard.RCNo + "" + src.RouteCardSubs.RouteCard.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefRcDate,
                    opt => opt.MapFrom(src =>
                        src.RouteCardSubs != null
                            ? src.RouteCardSubs.RouteCard.RCDate
                            : (DateTime?)null))

                .ForMember(dest => dest.InspectNo,
                    opt => opt.MapFrom(src =>
                        src.IncomingInspection != null
                            ? $"{src.IncomingInspection.InspectNo}{src.IncomingInspection.Suffix}"
                            : string.Empty))

                .ForMember(dest => dest.InspectDate,
                    opt => opt.MapFrom(src => src.IncomingInspection != null
                        ? src.IncomingInspection.InspectDate
                        : (DateTime?)null))


                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))

                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty))

                .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.MachineName : string.Empty));


            // ===================== VM -> Entity =====================
            CreateMap<ProductionReturnCompSubVM, ProductionReturnCompSub>()
                .ForMember(dest => dest.ProductionReturnComp, opt => opt.Ignore())
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionIssueCompSub, opt => opt.Ignore())
                .ForMember(dest => dest.RouteCardSubs, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());


        }
    }
}
