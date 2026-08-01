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
    public class GroupingMapping : Profile
    {
        public GroupingMapping()
        {
            // Entity -> VM
            CreateMap<Grouping, GroupingVM>();

            // VM -> Entity
            CreateMap<GroupingVM, Grouping>()
                .ForMember(dest => dest.GroupingSubs, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Condition(src => src.CreatedDate != null))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(_ => DateTime.Now));
        }
    }
}
