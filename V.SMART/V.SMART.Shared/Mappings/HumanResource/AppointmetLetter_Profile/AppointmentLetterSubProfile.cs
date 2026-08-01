using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.AppointmentLetterVM;

namespace V.SMART.Shared.Mappings.HumanResource_Profile.AppointmetLetter_Profile
{
    public class AppointmentLetterSubProfile :Profile
    {
        public AppointmentLetterSubProfile()
        {
            CreateMap<AppointmentLetterSubVM, AppointmentLetterSubVM>()
                .ForMember(dest => dest.AppointmentId, opt => opt.Ignore());
        }
    }
}
