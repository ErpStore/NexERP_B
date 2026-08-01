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
    public class ProductionReturnCompMapping : Profile
    {
        public ProductionReturnCompMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionReturnComp, ProductionReturnCompVM>()

                .ForMember(dest => dest.AddStoreName, opt => opt.MapFrom(src => src.AddStore != null ? src.AddStore.StoreName : string.Empty))

                .ForMember(dest => dest.CustName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ProductionReturnCompSubVMs, opt => opt.MapFrom(src => src.ProductionReturnCompSubs))
                .ForMember(dest => dest.ProductionReturnCompTrackVMs, opt => opt.MapFrom(src => src.ProductionReturnCompTracks));

            // ===================== VM -> Entity =====================
            CreateMap<ProductionReturnCompVM, ProductionReturnComp>()
                .ForMember(dest => dest.AddStore, opt => opt.Ignore())

                .ForMember(dest => dest.ProductionReturnCompSubs, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionReturnCompTracks, opt => opt.Ignore());


        }
    }
}
