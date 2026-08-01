using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_.ToolCrib;
using V.SMART.Shared.ViewModels.InventoryViewModel.ToolCribViewModels;


namespace V.SMART.Shared.Mappings.Inventory_Stock.ToolCribProfile
{
    public class ToolCribReturnProfile : Profile
    {
        public ToolCribReturnProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ToolCribReturn, ToolCribReturnVM>()


                // Currency
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ToolCribReturnSubVMs, opt => opt.MapFrom(src => src.ToolCribReturnSubs));

            // ===================== VM -> Entity =====================
            CreateMap<ToolCribReturnVM, ToolCribReturn>()
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.ToolCribReturnSubs, opt => opt.Ignore());
        }
    }


}
