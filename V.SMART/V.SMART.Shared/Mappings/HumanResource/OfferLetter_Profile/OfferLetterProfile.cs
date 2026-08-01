using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.OfferLetter;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.OfferLetter;

namespace V.SMART.Shared.Mappings.HumanResource_Profile.OfferLetter_Profile
{
    public class OfferLetterProfile:Profile
    {
        public OfferLetterProfile()
        {
            // Entity -> ViewModel
            //CreateMap<OfferLetter, OfferLetterVM>()

            //    .ForMember(dest => dest.Candidatename, opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.CandidateName : string.Empty))
            //    .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.Address : string.Empty))
            //    .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.Position : string.Empty))
            //    // Child Collection
            //    .ForMember(dest => dest.OfferLetterSubVMs, opt => opt.MapFrom(src => src.OfferLetterSubs));




            //// ViewModel -> Entity
            //CreateMap<OfferLetterVM, OfferLetter>()
            //    .ForMember(dest => dest.Candidate, opt => opt.Ignore())
            //    .ForMember(dest => dest.OfferLetterSubs, opt => opt.Ignore());
        
                CreateMap<OfferLetter, OfferLetterVM>()
                    .ForMember(dest => dest.Candidatename,
                        opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.CandidateName : string.Empty))
                    .ForMember(dest => dest.Address,
                        opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.Address : string.Empty))
                    .ForMember(dest => dest.Position,
                        opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.Position : string.Empty))
                    .ForMember(dest => dest.OfferLetterSubVMs,
                        opt => opt.MapFrom(src => src.OfferLetterSubs));

                CreateMap<OfferLetterVM, OfferLetter>()
                    .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                    .ForMember(dest => dest.OfferLetterSubs, opt => opt.Ignore());

                // Add this
                CreateMap<OfferLetterSub, OfferLetterSubVM>()
                    .ReverseMap();
           
        }
    }
}
