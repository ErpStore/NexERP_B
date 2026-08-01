using AutoMapper;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    public class CompMasterProfile : Profile
    {
        public CompMasterProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<CompMaster, CompMasterVM>()

                // Category
                .ForMember(dest => dest.CompItemCode, opt => opt.MapFrom(src => src.ComponentItem != null ? src.ComponentItem.ItemCode : string.Empty))
                .ForMember(dest => dest.CompItemName, opt => opt.MapFrom(src => src.ComponentItem != null ? src.ComponentItem.ItemName : string.Empty))
                
                .ForMember(dest => dest.RMCode, opt => opt.MapFrom(src => src.RawMaterial != null ? src.RawMaterial.ItemCode : string.Empty))
                .ForMember(dest => dest.RMName, opt => opt.MapFrom(src => src.RawMaterial != null ? src.RawMaterial.ItemName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ProcessFlowCharts, opt => opt.MapFrom(src => src.ProcessFlowCharts));

            // ===================== VM -> Entity =====================
            CreateMap<CompMasterVM, CompMaster>()
                .ForMember(dest => dest.RawMaterial, opt => opt.Ignore())
                .ForMember(dest => dest.ComponentItem, opt => opt.Ignore())
                .ForMember(dest => dest.ProcessFlowCharts, opt => opt.Ignore());
        }
    }
}
