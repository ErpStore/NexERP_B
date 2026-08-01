using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionReturnAssyMap
{
    public class ProductionReturnAssyMapping : Profile
    {
        public ProductionReturnAssyMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionReturnAssy, ProductionReturnAssyVM>()
                .ForMember(dest => dest.AddStoreName, opt => opt.MapFrom(src => src.AddStore != null ? src.AddStore.StoreName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ProductionReturnAssySubVMs, opt => opt.MapFrom(src => src.ProductionReturnAssySubs))
                .ForMember(dest => dest.ProductionReturnAssyTrackVMs, opt => opt.MapFrom(src => src.ProductionReturnAssyTracks));

            // ===================== VM -> Entity =====================
            CreateMap<ProductionReturnAssyVM, ProductionReturnAssy>()
                .ForMember(dest => dest.AddStore, opt => opt.Ignore())

                .ForMember(dest => dest.ProductionReturnAssySubs, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionReturnAssyTracks, opt => opt.Ignore());


        }
    }
}
