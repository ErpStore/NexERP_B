using AutoMapper;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.InventoryMasterProfile
{
    public class ItemCustomerAssignMapping : Profile
    {
        public ItemCustomerAssignMapping()
        {

            // ===================== Entity -> VM =====================

            CreateMap<ItemCustomerAssign, ItemCustomerAssignVM>()

                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty));


            // ===================== VM -> Entity =====================

            CreateMap<ItemCustomerAssignVM, ItemCustomerAssign>()
                            .ForMember(dest => dest.Customer, opt => opt.Ignore())
                            .ForMember(dest => dest.Item, opt => opt.Ignore());
        }
    }
}
