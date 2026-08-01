using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_.MaterialIssueNote;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.ViewModels.InventoryViewModel.MaterialIssueNoteVM;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.Inventory_Stock_.StockIssueRequest;
using V.SMART.Shared.ViewModels.InventoryViewModel.StockIssueRequestVM;

namespace V.SMART.Shared.Mappings.Inventory_Stock.MaterialIssueProfile
{
    public class StockIssueRequsetProfile : Profile
    {
        public StockIssueRequsetProfile()
        {
            // ===================== Entity → ViewModel =====================
            CreateMap<StockIssueRequest, StockIssueRequestVM>()
                // Store Mapping
                .ForMember(dest => dest.StoreIssName,
                    opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))
                .ForMember(dest => dest.StoreIssId,
                    opt => opt.MapFrom(src => src.StoreIssId))

                // Assembly (Main Item)
                .ForMember(dest => dest.AssyItemCode,
                    opt => opt.MapFrom(src => src.AssyItem != null ? src.AssyItem.ItemCode : string.Empty))
                .ForMember(dest => dest.AssyItemName,
                    opt => opt.MapFrom(src => src.AssyItem != null ? src.AssyItem.ItemName : string.Empty))

                // Sub Assembly (if any)
                .ForMember(dest => dest.SubAssyItemCode,
                    opt => opt.MapFrom(src => src.SubAssyId != null && src.SubAssyItem != null ? src.SubAssyItem.ItemCode : string.Empty))
                .ForMember(dest => dest.SubAssyItemName,
                    opt => opt.MapFrom(src => src.SubAssyId != null && src.SubAssyItem != null ? src.SubAssyItem.ItemName : string.Empty))

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


                // NoOfItems is computed automatically in VM (Count)
                .ForMember(dest => dest.NoOfItems, opt => opt.Ignore())

                // Child collection mapping
                .ForMember(dest => dest.StockIssueRequestSubVMs,
                    opt => opt.MapFrom(src => src.StockIssueRequestSubs));

            // ===================== ViewModel → Entity =====================
            CreateMap<StockIssueRequestVM, StockIssueRequest>()
                .ForMember(dest => dest.StoreIssId,
                    opt => opt.MapFrom(src => src.StoreIssId))
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.AssyItem, opt => opt.Ignore())
                .ForMember(dest => dest.SubAssyItem, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.StockIssueRequestSubs, opt => opt.Ignore()); // handled separately
        }

    }
}
