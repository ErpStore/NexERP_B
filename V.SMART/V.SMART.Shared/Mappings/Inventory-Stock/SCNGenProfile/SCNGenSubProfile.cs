using AutoMapper;
using V.SMART.Shared.Data.Inventory_Stock_.StoreTransferNote;
using V.SMART.Shared.ViewModels.InventoryViewModel.SCNGenViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.Inventory_Stock.SCNGenProfile
{
    public class SCNGenSubProfile : Profile
    {
        public SCNGenSubProfile()
        {
            // ===================== Entity → ViewModel =====================
            CreateMap<SCNGenSub, SCNGenSubVM>()
                // Item
                .ForMember(dest => dest.SelectedItemVMs, opt => opt.Ignore())
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))

                //CostCenter
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))
                .ForMember(dest => dest.IsEditable, opt => opt.MapFrom(src => false));

            // ===================== ViewModel → Entity =====================
            CreateMap<SCNGenSubVM, SCNGenSub>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.AssemblyDef, opt => opt.Ignore())
                .ForMember(dest => dest.SCNGen, opt => opt.Ignore());
        }
    }
}
