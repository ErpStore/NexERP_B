using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.Inventory_Stock.SCNGenProfile
{
    public class SCNGenProfile : Profile
    {
        public SCNGenProfile()
        {
            // ===================== Entity → ViewModel =====================
            CreateMap<SCNGen, SCNGenVM>()
                // Store
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))

                // Assembly (Main Item)
                .ForMember(dest => dest.AssyItemCode, opt => opt.MapFrom(src => src.AssyItem != null ? src.AssyItem.ItemCode : string.Empty))
                .ForMember(dest => dest.AssyItemName, opt => opt.MapFrom(src => src.AssyItem != null ? src.AssyItem.ItemName : string.Empty))
                // Sub Assembly
                .ForMember(dest => dest.SubAssyItemCode,
                    opt => opt.MapFrom(src => src.SubAssyItem != null ? src.SubAssyItem.ItemCode : string.Empty))
                .ForMember(dest => dest.SubAssyItemName,
                    opt => opt.MapFrom(src => src.SubAssyItem != null ? src.SubAssyItem.ItemName : string.Empty))

                // Sub Assembly 2
                .ForMember(dest => dest.SubAssyItemCode2,
                    opt => opt.MapFrom(src => src.SubAssyItem2 != null ? src.SubAssyItem2.ItemCode : string.Empty))
                .ForMember(dest => dest.SubAssyItemName2,
                    opt => opt.MapFrom(src => src.SubAssyItem2 != null ? src.SubAssyItem2.ItemName : string.Empty))

                // Sub Assembly 3
                .ForMember(dest => dest.SubAssyItemCode3,
                    opt => opt.MapFrom(src => src.SubAssyItem3 != null ? src.SubAssyItem3.ItemCode : string.Empty))
                .ForMember(dest => dest.SubAssyItemName3,
                    opt => opt.MapFrom(src => src.SubAssyItem3 != null ? src.SubAssyItem3.ItemName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.SCNGenSubVMs, opt => opt.MapFrom(src => src.SCNGenSubs));

            // ===================== ViewModel → Entity =====================
            CreateMap<SCNGenVM, SCNGen>()
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.AssyItem, opt => opt.Ignore())
                .ForMember(dest => dest.SubAssyItem, opt => opt.Ignore())
                .ForMember(dest => dest.SCNGenSubs, opt => opt.Ignore());
        }

    }
}
