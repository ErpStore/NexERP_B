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
    public class ProductionSCNCompMapping : Profile
    {
        public ProductionSCNCompMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionSCNComp, ProductionSCNCompVM>()
                .ForMember(dest => dest.AddStoreName, opt => opt.MapFrom(src => src.AddStore != null ? src.AddStore.StoreName : string.Empty))
                .ForMember(dest => dest.IssueStoreName, opt => opt.MapFrom(src => src.IssueStore != null ? src.IssueStore.StoreName : string.Empty))

                .ForMember(dest => dest.ProductionSCNCompSubVMs, opt => opt.MapFrom(src => src.ProductionSCNCompSubs));

            // ===================== VM -> Entity =====================
            CreateMap<ProductionSCNCompVM, ProductionSCNComp>()
                .ForMember(dest => dest.AddStore, opt => opt.Ignore())
                .ForMember(dest => dest.IssueStore, opt => opt.Ignore())

                .ForMember(dest => dest.ProductionSCNCompSubs, opt => opt.Ignore());

        }
    }
}
