using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using V.SMART.Shared.Data.OutSourcing.SubContractSCN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.SubContractSCNMapping
{
    public class SubContractSCNSubProfile : Profile
    {
        public SubContractSCNSubProfile()
        {
            // Entity -> VM
            CreateMap<SubConSCNSub, SubConSCNSubVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))


                .ForMember(dest => dest.RefGRNNo,
                    opt => opt.MapFrom(src =>
                        src.SubConGRNSub != null
                            ? src.SubConGRNSub.SubConGRNs.GRNNo + "" + src.SubConGRNSub.SubConGRNs.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefGRNDate,
                    opt => opt.MapFrom(src =>
                        src.SubConGRNSub != null
                            ? src.SubConGRNSub.SubConGRNs.GRNDate
                            : (DateTime?)null))



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
                        src.RouteCardSub != null
                            ? src.RouteCardSub.RouteCard.RCNo + "" + src.RouteCardSub.RouteCard.Suffix
                            : string.Empty))

                .ForMember(dest => dest.RefRcDate,
                    opt => opt.MapFrom(src =>
                        src.RouteCardSub != null
                            ? src.RouteCardSub.RouteCard.RCDate
                            : (DateTime?)null))

            .ForMember(dest => dest.Process,
                    opt => opt.MapFrom(src =>
                        src.RouteCardSub != null
                            ? src.RouteCardSub.Process.ProcessName 
                            : string.Empty));


            // VM -> Entity
            CreateMap<SubConSCNSubVM, SubConSCNSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.SubConGRNSub, opt => opt.Ignore())
                .ForMember(dest => dest.PurchPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.RouteCardSub, opt => opt.Ignore());
        }
    }

}
