using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.AppointmentLetter;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AppointmentLetterVM;

namespace V.SMART.Shared.Mappings.HumanResource_Profile.AppointmetLetter_Profile
{
    public class AppointmentLetterProfile:Profile
    {
        public AppointmentLetterProfile()
        {
            CreateMap<AppointmentLetter, AppointmentLetterVM>()
                .ForMember(dest => dest.Candidatename,
                    opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.CandidateName : string.Empty))
                .ForMember(dest => dest.Address,
                    opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.Address : string.Empty))
                .ForMember(dest => dest.Position,
                    opt => opt.MapFrom(src => src.Candidate != null ? src.Candidate.Position : string.Empty))
                .ForMember(dest => dest.AppointmentLetterSubVM,
                    opt => opt.MapFrom(src => src.AppointmetLetterSubs));

            CreateMap<AppointmentLetterVM, AppointmentLetter>()
                .ForMember(dest => dest.Candidate, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmetLetterSubs, opt => opt.Ignore());

            // Add this
            CreateMap<AppointmetLetterSub, AppointmentLetterSubVM>()
                .ReverseMap();
        }
    }
}
