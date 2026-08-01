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
    public class ProcessFlowProfile : Profile
    {
        public ProcessFlowProfile()
        {
            // ===================== Entity -> VM =====================

            CreateMap<ProcessFlowChart, ProcessFlowChartVM>()

                .ForMember(dest => dest.CompItemCode, opt => opt.MapFrom(src => src.ComponentItem != null ? src.ComponentItem.ItemCode : string.Empty))
                .ForMember(dest => dest.CompItemName, opt => opt.MapFrom(src => src.ComponentItem != null ? src.ComponentItem.ItemName : string.Empty))

                .ForMember(dest => dest.RMCode, opt => opt.MapFrom(src => src.RawMaterial != null ? src.RawMaterialItem.ItemCode : string.Empty))
                .ForMember(dest => dest.RMName, opt => opt.MapFrom(src => src.RawMaterial != null ? src.RawMaterialItem.ItemName : string.Empty))

                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty))

                .ForMember(dest => dest.ScrapMaterialName, opt => opt.MapFrom(src => src.RawMaterial != null ? src.RawMaterial.RMName : string.Empty))

                .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.MachineName : string.Empty))

                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : string.Empty));

            // ===================== VM -> Entity =====================

            CreateMap<ProcessFlowChartVM, ProcessFlowChart>()
                .ForMember(dest => dest.ComponentItem, opt => opt.Ignore())
                .ForMember(dest => dest.RawMaterialItem, opt => opt.Ignore())
                .ForMember(dest => dest.RawMaterial, opt => opt.Ignore())
                .ForMember(dest => dest.Machine, opt => opt.Ignore())
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.Process, opt => opt.Ignore());
        }
    }
}
