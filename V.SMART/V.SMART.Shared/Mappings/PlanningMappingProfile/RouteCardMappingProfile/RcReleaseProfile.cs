using AutoMapper;
using V.SMART.Shared.Data.Planning.ComponentRouteCard;
using V.SMART.Shared.ViewModels.PlanningViewModel.RouteCardViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.PlanningMappingProfile.RouteCardMappingProfile
{
    public class RcReleaseProfile : Profile
    {
        public RcReleaseProfile()
        {
            // ===================== Entity → VM =====================
            CreateMap<RouteCardRelease, RouteCardReleaseVM>()

             .ForMember(dest => dest.CustomerName,
                 opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

             .ForMember(dest => dest.PoNumber,opt => opt.MapFrom(src => src.MfgPo != null
                                 ? (src.MfgPo.PONo + src.MfgPo.Suffix)
                                 : string.Empty))

             .ForMember(dest => dest.AssemblyItemCode, opt => opt.MapFrom(src => src.Assembly != null ? src.Assembly.ItemCode : string.Empty))

             // ---------------- CHILD LIST ----------------
             .ForMember(dest => dest.RouteCardReleaseSubs,
                 opt => opt.MapFrom(src => src.RouteCardReleaseSubs));


            // ===================== VM → Entity =====================
            CreateMap<RouteCardReleaseVM, RouteCardRelease>()

                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPo, opt => opt.Ignore())
                .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                .ForMember(dest => dest.Assembly, opt => opt.Ignore())
                .ForMember(dest => dest.RouteCardReleaseSubs, opt => opt.Ignore());
        }
    }
}
