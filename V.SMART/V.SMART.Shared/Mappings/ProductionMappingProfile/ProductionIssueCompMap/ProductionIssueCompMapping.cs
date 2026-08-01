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
    public class ProductionIssueCompMapping : Profile
    {
        public ProductionIssueCompMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionIssueComp, ProductionIssueCompVM>()
                .ForMember(dest => dest.StoreIssName, opt => opt.MapFrom(src => src.StoreIssue != null ? src.StoreIssue.StoreName : string.Empty))

                .ForMember(dest => dest.CustName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ProductionIssueCompSubVMs, opt => opt.MapFrom(src => src.ProductionIssueCompSubs));

            // ===================== VM -> Entity =====================
            CreateMap<ProductionIssueCompVM, ProductionIssueComp>()

                .ForMember(dest => dest.StoreIssue, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionIssueCompSubs, opt => opt.Ignore());


        }
    }
}
