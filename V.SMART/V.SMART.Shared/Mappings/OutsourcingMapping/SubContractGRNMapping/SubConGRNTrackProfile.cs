using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using V.SMART.Shared.Data.Production.ProductionComponent;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProuctionCompViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.SubContractGRNMapping
{
    public class SubConGRNTrackProfile : Profile
    {
        public SubConGRNTrackProfile()
        {
            // VM → Entity
            CreateMap<SubConGRNTrackVM, SubConGRNTrack>()

                .ForMember(dest => dest.SubConGRNSub, opt => opt.Ignore())
                .ForMember(dest => dest.RouteCardSub, opt => opt.Ignore())
                .ForMember(dest => dest.SubConDcOutSub, opt => opt.Ignore())
                .ForMember(dest => dest.InComingItem, opt => opt.Ignore())
                .ForMember(dest => dest.OutgoingItem, opt => opt.Ignore());

            // Entity → VM
            CreateMap<ProductionReturnCompTrack, ProductionReturnCompTrackVM>()
                .ForMember(dest => dest.RefRcNo, opt => opt.MapFrom(src => src.RouteCardSub != null ? src.RouteCardSub.RouteCard.RCNo : null));
        }
    }
}
