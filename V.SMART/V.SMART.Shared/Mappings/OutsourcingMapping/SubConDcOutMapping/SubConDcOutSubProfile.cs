using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.SubConDcOutMapping
{
    public class SubConDcOutSubProfile : Profile
    {
        public SubConDcOutSubProfile()
        {
            // Entity -> VM
            CreateMap<SubConDcOutSub, SubConDcOutSubVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty))

                .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.MachineName : string.Empty))

                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))

                .ForMember(dest => dest.RefPoNo,
                    opt => opt.MapFrom(src =>
                        src.PurchPoSub != null
                            ? src.PurchPoSub.PurchPo.PONo + "" + src.PurchPoSub.PurchPo.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefPoDate,
                    opt => opt.MapFrom(src =>
                        src.PurchPoSub != null
                            ? src.PurchPoSub.PurchPo.PODate
                            : (DateTime?)null))

                 .ForMember(dest => dest.RefRcNo,
                    opt => opt.MapFrom(src =>
                        src.ComponentRouteCardSub != null
                            ? src.ComponentRouteCardSub.RouteCard.RCNo + "" + src.ComponentRouteCardSub.RouteCard.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefRcDate,
                    opt => opt.MapFrom(src =>
                        src.ComponentRouteCardSub != null
                            ? src.ComponentRouteCardSub.RouteCard.RCDate
                            : (DateTime?)null));


            // VM -> Entity
            CreateMap<SubConDcOutSubVM, SubConDcOutSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentRouteCardSub, opt => opt.Ignore());
        }
    }
}
