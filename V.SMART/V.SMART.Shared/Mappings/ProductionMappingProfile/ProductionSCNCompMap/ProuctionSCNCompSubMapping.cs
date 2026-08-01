using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionSCNCompMap
{
    public class ProuctionSCNCompSubMapping : Profile
    {
        public ProuctionSCNCompSubMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionSCNCompSub, ProductionSCNCompSubVM>()

                 .ForMember(dest => dest.RefReturnNo,
                    opt => opt.MapFrom(src =>
                        src.ProductionReturnCompSubs != null
                            ? src.ProductionReturnCompSubs.ProductionReturnComp.ReturnNo + "" + src.ProductionReturnCompSubs.ProductionReturnComp.Suffix
                            : string.Empty))
                .ForMember(dest => dest.RefReturnDate,
                    opt => opt.MapFrom(src =>
                        src.ProductionReturnCompSubs != null
                            ? src.ProductionReturnCompSubs.ProductionReturnComp.ReturnDate
                            : (DateTime?)null))

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))


                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty));


            // ===================== VM -> Entity =====================
            CreateMap<ProductionSCNCompSubVM, ProductionSCNCompSub>()
                .ForMember(dest => dest.ProductionReturnCompSubs, opt => opt.Ignore())
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionSCNComp, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());


        }
    }
}
