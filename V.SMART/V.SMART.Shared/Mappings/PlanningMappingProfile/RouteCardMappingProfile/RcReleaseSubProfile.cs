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
    public class RcReleaseSubProfile : Profile
    {
        public RcReleaseSubProfile()
        {
            // ================== Entity → VM ==================
            CreateMap<RouteCardReleaseSub, RouteCardReleaseSubVM>()

                .ForMember(dest => dest.ComponentItemCode,
                    opt => opt.MapFrom(src => src.Item.ItemCode))

                .ForMember(dest => dest.ComponentItemName,
                    opt => opt.MapFrom(src => src.Item.ItemName))

                .ForMember(dest => dest.ComponentItemUOM,
                    opt => opt.MapFrom(src => src.Item.MeasureUnit))


                //Cost Center to Project No mapping (if needed in VM)
                .ForMember(dest => dest.ProjectNo,
                    opt => opt.MapFrom(src => src.CostCenter.ProjectNo));


                    

            // ================== VM → Entity ==================
            CreateMap<RouteCardReleaseSubVM, RouteCardReleaseSub>()

                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore());
        }
    }
}
