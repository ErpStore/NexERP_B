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
    public class ContractReviewProfile:Profile
    {
        public ContractReviewProfile()
        {
            // ============== Entity → ViewModel Mapping ==============
            CreateMap<ContractReview, ContractReviewVM>()

                .ForMember(dest => dest.PoNo,
                    opt => opt.MapFrom(src =>
                        src.MfgPo != null
                            ? src.MfgPo.PONo + "" + src.MfgPo.Suffix
                            : string.Empty))

                .ForMember(dest => dest.CustName,
                    opt => opt.MapFrom(src => src.MfgPo != null ? src.MfgPo.Customer.CustName : string.Empty))

                .ForMember(dest => dest.ContractReviewSubVMS, opt => opt.MapFrom(src => src.ContractReviewSubs));


            // ============== ViewModel → Entity Mapping ==============
            CreateMap<ContractReviewVM, ContractReview>()
                .ForMember(dest => dest.MfgPo, opt => opt.Ignore())
                .ForMember(dest => dest.ContractReviewSubs, opt => opt.Ignore());
        }
    }
}
