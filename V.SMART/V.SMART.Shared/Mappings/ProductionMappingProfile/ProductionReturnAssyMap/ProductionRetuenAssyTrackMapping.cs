using AutoMapper;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.Data.Production.ProductionReturnGrnAssy;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionReturnAssyViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionReturnAssyMap
{
    public class ProductionRetuenAssyTrackMapping : Profile
    {
        public ProductionRetuenAssyTrackMapping()
        {
            // VM → Entity
            CreateMap<ProductionReturnAssyTrackVM, ProductionReturnAssyTrack>()
                .ForMember(dest => dest.ProductionReturnAssySub, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionIssueAssySub, opt => opt.Ignore())
                .ForMember(dest => dest.AssyItem, opt => opt.Ignore())
                .ForMember(dest => dest.PartItem, opt => opt.Ignore())
                .ForMember(dest => dest.JobOrder, opt => opt.Ignore());

            // Entity → VM
            CreateMap<ProductionReturnAssyTrack, ProductionReturnAssyTrackVM>()
                .ForMember(dest => dest.RefJobNo, opt => opt.MapFrom(src => src.JobOrder != null ? src.JobOrder.JobNo : null));
        }
    }
}
