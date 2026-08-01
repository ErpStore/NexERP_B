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
    public class ToolCribIssueSubProfile : Profile
    {
        public ToolCribIssueSubProfile()
        {
            // Entity -> VM
            CreateMap<ToolCribIssueSub, ToolCribIssueSubVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty));

            // VM -> Entity
            CreateMap<ToolCribIssueSubVM, ToolCribIssueSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore());
        }
    }


}
