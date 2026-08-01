using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionSCNAssembly;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionSCNAssyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionSCNAssyMap
{
    public class ProductionSCNAssyMapping : Profile
    {
        public ProductionSCNAssyMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionSCNAssy, ProductionSCNAssyVM>()
                .ForMember(dest => dest.AddStoreName, opt => opt.MapFrom(src => src.AddStore != null ? src.AddStore.StoreName : string.Empty))
                .ForMember(dest => dest.IssueStoreName, opt => opt.MapFrom(src => src.IssueStore != null ? src.IssueStore.StoreName : string.Empty))

                .ForMember(dest => dest.ProductionSCNAssySubVMs, opt => opt.MapFrom(src => src.ProductionSCNAssySubs));

            // ===================== VM -> Entity =====================
            CreateMap<ProductionSCNAssyVM, ProductionSCNAssy>()
                .ForMember(dest => dest.AddStore, opt => opt.Ignore())
                .ForMember(dest => dest.IssueStore, opt => opt.Ignore())

                .ForMember(dest => dest.ProductionSCNAssySubs, opt => opt.Ignore());

        }
    }
}
