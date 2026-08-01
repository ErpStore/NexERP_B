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
    public class UserMapping : Profile
    {
        public UserMapping()
        {
            // ===============================
            // Entity → ViewModel
            // ===============================
            CreateMap<User, UserVM>()
                .ForMember(dest => dest.StateCodes,
                    opt => opt.MapFrom(src =>
                        string.IsNullOrWhiteSpace(src.StateCodesCsv)
                            ? new List<int>()
                            : src.StateCodesCsv
                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse)
                                .ToList()
                    ));


            // ===============================
            // ViewModel → Entity
            // ===============================
            CreateMap<UserVM, User>()
                // Convert List<int> → CSV
                .ForMember(dest => dest.StateCodesCsv,
                    opt => opt.MapFrom(src =>
                        src.StateCodes != null && src.StateCodes.Any()
                            ? string.Join(",", src.StateCodes)
                            : null
                    ))

                // Navigation ignore
                .ForMember(dest => dest.Staff, opt => opt.Ignore())
                .ForMember(dest => dest.UserRights, opt => opt.Ignore())
                .ForMember(dest => dest.UserAuthorities, opt => opt.Ignore());

        }
    }
}
