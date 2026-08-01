using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.HumanResource.OfferLetter;
using V.SMART.Shared.ViewModels.HumanResourceViewModel.OfferLetter;

namespace V.SMART.Shared.Mappings.HumanResource_Profile.OfferLetter_Profile
{
    public class OfferLetterSubProfile:Profile
    {
        public OfferLetterSubProfile()
        {
           // VM->Entity


            CreateMap<OfferLetterSubVM, OfferLetterSub>()
                .ForMember(dest => dest.OfferId, opt => opt.Ignore());

        }
    }
}
