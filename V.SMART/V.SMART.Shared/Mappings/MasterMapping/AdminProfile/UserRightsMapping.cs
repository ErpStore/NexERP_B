using AutoMapper;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.AdminProfile
{
    public class UserRightsMapping : Profile
    {
        public UserRightsMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<UserRight, UserRightVM>()

                // User
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty))

                .ForMember(dest => dest.ScreenName, opt => opt.MapFrom(src => src.Screens != null ? src.Screens.ScreenName : string.Empty));

            CreateMap<UserRightVM, UserRight>()

                // Ignore navigation tables while saving (handled manually)
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Screens, opt => opt.Ignore());
        }
    }
}
