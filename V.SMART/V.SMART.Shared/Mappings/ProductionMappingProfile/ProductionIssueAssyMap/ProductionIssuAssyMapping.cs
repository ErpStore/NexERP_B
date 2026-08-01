using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.Pages.ProductionModule_pages;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionIssueAssyMap
{
    public class ProductionIssAssyTrackMapping : Profile
    {
        public ProductionIssAssyTrackMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionIssueAssy, ProductionIssueAssyVM>()
                .ForMember(dest => dest.StoreIssName, opt => opt.MapFrom(src => src.StoreIssue != null ? src.StoreIssue.StoreName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ProductionIssueAssySubVMs, opt => opt.MapFrom(src => src.ProductionIssueAssySubs));

            // ===================== VM -> Entity =====================
            CreateMap<ProductionIssueAssyVM, ProductionIssueAssy>()
                .ForMember(dest => dest.StoreIssue, opt => opt.Ignore())

                .ForMember(dest => dest.ProductionIssueAssySubs, opt => opt.Ignore());
                

        }
    }
}
