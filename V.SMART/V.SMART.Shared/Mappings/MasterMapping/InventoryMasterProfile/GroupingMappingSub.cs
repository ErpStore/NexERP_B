using AutoMapper;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.InventoryMasterProfile
{
    public class GroupingMappingSub : Profile
    {
        public GroupingMappingSub()
        {

            // ===================== Entity -> VM =====================

            CreateMap<GroupingSub, GroupingSubVM>()

                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty))
                .ForMember(dest => dest.FactorName, opt => opt.MapFrom(src => src.Factor != null ? src.Factor.Factors : string.Empty));


            // ===================== VM -> Entity =====================

            CreateMap<GroupingSubVM, GroupingSub>()
                            .ForMember(dest => dest.Process, opt => opt.Ignore())
                            .ForMember(dest => dest.Factors, opt => opt.Ignore())
                            .ForMember(dest => dest.Grouping, opt => opt.Ignore());
        }
    }
}
