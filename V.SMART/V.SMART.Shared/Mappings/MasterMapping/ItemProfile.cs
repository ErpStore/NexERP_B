using AutoMapper;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    public class ItemProfile : Profile
    {
        public ItemProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<Item, ItemVM>()

                // Category
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.CategoryName : string.Empty))

                .ForMember(dest => dest.RawMaterialName, opt => opt.MapFrom(src => src.RawMaterial != null ? src.RawMaterial.RMName : string.Empty));



            // ===================== VM -> Entity =====================
            CreateMap<ItemVM, Item>()
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.RawMaterial, opt => opt.Ignore())
                .ForMember(dest => dest.UOM, opt => opt.Ignore());
        }
    }
}
