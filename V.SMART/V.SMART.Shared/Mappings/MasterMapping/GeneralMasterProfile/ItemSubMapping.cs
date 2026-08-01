using AutoMapper;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.GeneralMasterProfile
{
    public class ItemSubMapping : Profile
    {
        public ItemSubMapping()
        {
            // Entity -> VM
            CreateMap<ItemSub, ItemSubVM>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty));

            // VM -> Entity
            CreateMap<ItemSubVM, ItemSub>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore());
        }
    }
}
