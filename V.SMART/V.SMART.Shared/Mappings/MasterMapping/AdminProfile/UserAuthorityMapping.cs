using AutoMapper;
using V.SMART.Shared.Data.Master.Admin_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.AdminViewmodel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.AdminProfile
{
    public class UserAuthorityMapping : Profile
    {
        public UserAuthorityMapping()
        {
            // Entity -> VM
            CreateMap<UserAuthority, UserAuthorityVM>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.UserName : string.Empty));


            // VM -> Entity
            CreateMap<UserAuthorityVM, UserAuthority>()
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }
    }
}
