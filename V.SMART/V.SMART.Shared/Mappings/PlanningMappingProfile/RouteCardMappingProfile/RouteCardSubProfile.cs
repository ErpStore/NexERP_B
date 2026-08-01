
using AutoMapper;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;

namespace V.SMART.Shared.Mappings.PlanningMappingProfile.RouteCardMappingProfile
{
    public class RouteCardSubProfile : Profile
    {
        public RouteCardSubProfile()
        {
            // ================== Entity → VM ==================
            CreateMap<RouteCardSub, RouteCardSubVM>()

                .ForMember(dest => dest.ProcessName,
                    opt => opt.MapFrom(src => src.Process.ProcessName))

                .ForMember(dest => dest.ItemCodeIn,
                    opt => opt.MapFrom(src => src.IncomingItem.ItemCode))

                .ForMember(dest => dest.ItemCodeOut,
                    opt => opt.MapFrom(src => src.OutgoingItem.ItemCode))

                .ForMember(dest => dest.MachineName,
                    opt => opt.MapFrom(src => src.Machine.MachineName))

                .ForMember(dest => dest.VendorName,
                    opt => opt.MapFrom(src => src.Vendor.VendorName));




            // ================== VM → Entity ==================
            CreateMap<RouteCardSubVM, RouteCardSub>()

                // Ignore navigation properties – EF loads them
                .ForMember(dest => dest.Process, opt => opt.Ignore())
                .ForMember(dest => dest.IncomingItem, opt => opt.Ignore())
                .ForMember(dest => dest.OutgoingItem, opt => opt.Ignore())
                .ForMember(dest => dest.Machine, opt => opt.Ignore())
                .ForMember(dest => dest.Vendor, opt => opt.Ignore());
        }
    }

}
