using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_.InterStoreTransfer;
using V.SMART.Shared.ViewModels.InventoryViewModel.StoreInterTransferViewModel;

namespace V.SMART.Shared.Mappings.Inventory_Stock.StoreInterTransProfile
{
    public class StoreIntertransferProfile : Profile
    {
        public StoreIntertransferProfile()
        {
            // ===================== Entity → ViewModel =====================
            CreateMap<StoreInterTrans, StoreInterTransVM>()
            .ForMember(dest => dest.StoreInterTransSubVMs,
                opt => opt.MapFrom(src => src.StoreInterTransSubs))

            .ForMember(dest => dest.NoOfItems,
                opt => opt.MapFrom(src => src.StoreInterTransSubs.Count))

            .ForMember(dest => dest.StoreIdTo,
                opt => opt.MapFrom(src => src.StoreAddId))
            .ForMember(dest => dest.StoreIdFrom,
                opt => opt.MapFrom(src => src.StoreIssueId))

            .ForMember(dest => dest.StoreIssName,
                opt => opt.MapFrom(src => src.StoreIssue != null ? src.StoreIssue.StoreName : ""))

            .ForMember(dest => dest.StoreAddName,
                opt => opt.MapFrom(src => src.StoreAdd != null ? src.StoreAdd.StoreName : ""));

            CreateMap<StoreInterTransVM, StoreInterTrans>()
                .ForMember(dest => dest.StoreAddId, opt => opt.MapFrom(src => src.StoreIdTo))
                .ForMember(dest => dest.StoreIssueId, opt => opt.MapFrom(src => src.StoreIdFrom))
                .ForMember(dest => dest.StoreInterTransSubs, opt => opt.Ignore())
                .ForMember(dest => dest.NoOfItems, opt => opt.Ignore());
        }
    }
}
