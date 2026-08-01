using AutoMapper;
using V.SMART.Shared.Data.Master.Inventory_module;
using V.SMART.Shared.Data.SalesAndLabour.SalesEnquiry;
using V.SMART.Shared.ViewModels.MasterViewModel.InventoryViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesEnquiryVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    public class AssemblyDefProfile : Profile
    {
        public AssemblyDefProfile()
        {
            // Entity -> VM
            CreateMap<AssmblyDef, AssemblyDefVM>()
                .ForMember(dest => dest.AssemblyItemCode, opt => opt.MapFrom(src => src.AssemblyItem != null ? src.AssemblyItem.ItemCode : string.Empty))
                .ForMember(dest => dest.AssemblyItemName, opt => opt.MapFrom(src => src.AssemblyItem != null ? src.AssemblyItem.ItemName : string.Empty))


                .ForMember(dest => dest.PartItemCode, opt => opt.MapFrom(src => src.PartItem != null ? src.PartItem.ItemCode : string.Empty))
                .ForMember(dest => dest.PartItemName, opt => opt.MapFrom(src => src.PartItem != null ? src.PartItem.ItemName : string.Empty))
                .ForMember(dest => dest.UOM, opt => opt.MapFrom(src => src.PartItem != null ? src.PartItem.MeasureUnit : string.Empty))
                .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => src.PartItem != null ? src.PartItem.BtOtorMfg : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.PartItem != null ? src.PartItem.Category.CategoryName : string.Empty))

                .ForMember(dest => dest.CostCenterName, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))

                .ForMember(dest => dest.RackNo, opt => opt.MapFrom(src => src.PartItem != null ? src.PartItem.RackNo : string.Empty))

                .ForMember(dest => dest.IsLabourMaterial, opt => opt.MapFrom(src => src.IsLabourMaterial))

                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty));


            // VM -> Entity
            CreateMap<AssemblyDefVM, AssmblyDef>()
                .ForMember(dest => dest.AssemblyItem, opt => opt.Ignore())
                .ForMember(dest => dest.PartItem, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.Process, opt => opt.Ignore());


        }
    }
}
