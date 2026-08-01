using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.ContractReview;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ContractReviewVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.ContractReviewProfile
{
    public class ContractReviewSubProfile:Profile
    {
        public ContractReviewSubProfile() 
        {
            // ============== Entity → ViewModel Mapping ==============
            CreateMap<ContractReviewSub, ContractReviewSubVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.Specification, opt => opt.MapFrom(src => src.Item != null ? src.Item.Specification : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.HSNCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Item != null ? src.Item.Weight : (decimal?)null))


            //Category
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Item != null ? src.Item.Category.CategoryName : string.Empty));

            // ============== ViewModel → Entity Mapping ==============
            CreateMap<ContractReviewSubVM, ContractReviewSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore());
        }
    }
}
