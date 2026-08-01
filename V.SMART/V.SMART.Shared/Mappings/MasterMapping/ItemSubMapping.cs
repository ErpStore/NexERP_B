using AutoMapper;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    internal class ItemSubMapping : Profile
    {
        public ItemSubMapping()
        {
            // ===================== Entity -> VM =====================

            CreateMap<ItemSub, ItemSubVM>()
                .ForMember(dest => dest.ItemName,
                    opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.VendorName,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : string.Empty))
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty));

            CreateMap<ItemSubVM, ItemSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore());

        }
    }
}
