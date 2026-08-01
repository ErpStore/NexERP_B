using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer;
using V.SMART.Shared.Data.Inventory_Stock_.MaterialIssueNote;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.ViewModels.InventoryViewModel.MaterialIssueNoteVM;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using V.SMART.Shared.ViewModels.InventoryViewModel.StoreInterTransferViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.Inventory_Stock.StoreInterTransProfile
{
    public class StoreInterTransferSubProfile : Profile
    {
        public StoreInterTransferSubProfile()
        {
            // ===================== Entity → ViewModel =====================
            CreateMap<StoreInterTransSub, StoreInterTransSubVM>()
            .ForMember(dest => dest.SelectedItemVMs, opt => opt.Ignore())
            .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : ""))
            .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : ""))
            .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : ""));

            CreateMap<StoreInterTransSubVM, StoreInterTransSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.StoreInterTrans, opt => opt.Ignore());
        }
    }
}
