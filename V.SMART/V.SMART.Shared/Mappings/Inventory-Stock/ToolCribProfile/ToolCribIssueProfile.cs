using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.Inventory_Stock.ToolCribProfile
{
    public class ToolCribIssueProfile : Profile
    {
        public ToolCribIssueProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ToolCribIssue, ToolCribIssueVM>()

                // Category
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty))

                // Store
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ToolCribIssueSubVMs, opt => opt.MapFrom(src => src.ToolCribIssueSubs));

            // ===================== VM -> Entity =====================
            CreateMap<ToolCribIssueVM, ToolCribIssue>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.ToolCribIssueSubs, opt => opt.Ignore());
        }
    }


}
