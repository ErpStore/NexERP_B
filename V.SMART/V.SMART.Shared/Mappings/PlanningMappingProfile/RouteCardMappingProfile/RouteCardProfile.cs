using AutoMapper;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;

namespace V.SMART.Shared.Mappings.PlanningMappingProfile.RouteCardMappingProfile
{
    public class RouteCardProfile : Profile
    {
        public RouteCardProfile()
        {
            // ===================== Entity → VM =====================
            CreateMap<RouteCard, RouteCardVM>()

             // ---------------- BASIC ----------------
             .ForMember(dest => dest.CustName,
                 opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

             .ForMember(dest => dest.RefPoNo,
                 opt => opt.MapFrom(src => src.MfgPo != null
                     ? (src.MfgPo.PONo + src.MfgPo.Suffix)
                     : string.Empty))

             .ForMember(dest => dest.RefPoDate,
                 opt => opt.MapFrom(src => src.MfgPo != null ? src.MfgPo.PODate : (DateTime?)null))


             // ---------------- Rc Release NO -------------

             .ForMember(dest => dest.RefRcReleaseNo,
                 opt => opt.MapFrom(src => src.RcRelease != null
                     ? (src.RcRelease.RcReleaseNo + src.RcRelease.Suffix)
                     : string.Empty))

             .ForMember(dest => dest.RefRcReleaseDate,
                 opt => opt.MapFrom(src => src.RcRelease != null ? src.RcRelease.RcReleaseDate : (DateTime?)null))


             // ---------------- ITEM CODES ----------------
             .ForMember(dest => dest.AssyCode,
                 opt => opt.MapFrom(src => src.AssemblyItem != null ? src.AssemblyItem.ItemCode : string.Empty))

             .ForMember(dest => dest.SubAssyCode,
                 opt => opt.MapFrom(src => src.SubAssemblyItem != null ? src.SubAssemblyItem.ItemCode : string.Empty))

             .ForMember(dest => dest.SubAssyCode2,
                 opt => opt.MapFrom(src => src.SubAssemblyItem2 != null ? src.SubAssemblyItem2.ItemCode : string.Empty))

             .ForMember(dest => dest.SubAssyCode3,
                 opt => opt.MapFrom(src => src.SubAssemblyItem3 != null ? src.SubAssemblyItem3.ItemCode : string.Empty))

             .ForMember(dest => dest.CompItemCode,
                 opt => opt.MapFrom(src => src.ComponentItem != null ? src.ComponentItem.ItemCode : string.Empty))

             .ForMember(dest => dest.RMCode,
                 opt => opt.MapFrom(src => src.RawMaterialItem != null ? src.RawMaterialItem.ItemCode : string.Empty))


             // ---------------- COST CENTER ----------------
             .ForMember(dest => dest.ProjectNo,
                 opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))


             // ---------------- CHILD LIST ----------------
             .ForMember(dest => dest.RouteCardSubVMs,
                 opt => opt.MapFrom(src => src.RouteCardSubs));


            // ===================== VM → Entity =====================
            CreateMap<RouteCardVM, RouteCard>()

                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPo, opt => opt.Ignore())

                .ForMember(dest => dest.AssemblyItem, opt => opt.Ignore())
                .ForMember(dest => dest.SubAssemblyItem, opt => opt.Ignore())
                .ForMember(dest => dest.SubAssemblyItem2, opt => opt.Ignore())
                .ForMember(dest => dest.SubAssemblyItem3, opt => opt.Ignore())

                .ForMember(dest => dest.ComponentItem, opt => opt.Ignore())
                .ForMember(dest => dest.RawMaterialItem, opt => opt.Ignore())

                .ForMember(dest => dest.IssueStore, opt => opt.Ignore())
                .ForMember(dest => dest.AddStore, opt => opt.Ignore())

                .ForMember(dest => dest.RouteCardSubs, opt => opt.Ignore());
        }
    }

}
