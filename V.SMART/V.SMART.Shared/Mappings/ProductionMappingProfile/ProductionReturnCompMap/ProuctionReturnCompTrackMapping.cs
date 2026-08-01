using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionReturnCompMap
{
    public class ProuctionReturnCompTrackMapping : Profile
    {
        public ProuctionReturnCompTrackMapping()
        {
            // VM → Entity
            CreateMap<ProductionReturnCompTrackVM, ProductionReturnCompTrack>()
                .ForMember(dest => dest.ProductionReturnCompSub, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionIssueCompSub, opt => opt.Ignore())
                .ForMember(dest => dest.InComingItem, opt => opt.Ignore())
                .ForMember(dest => dest.OutgoingItem, opt => opt.Ignore())
                .ForMember(dest => dest.RouteCardSub, opt => opt.Ignore());

            // Entity → VM
            CreateMap<ProductionReturnCompTrack, ProductionReturnCompTrackVM>()
                .ForMember(dest => dest.RefRcNo, opt => opt.MapFrom(src => src.RouteCardSub != null ? src.RouteCardSub.RouteCard.RCNo : null));
        }
    }
}
