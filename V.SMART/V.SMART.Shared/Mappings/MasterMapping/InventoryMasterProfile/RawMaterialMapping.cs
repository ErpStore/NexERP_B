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
    public class RawMaterialMapping : Profile
    {
        public RawMaterialMapping()
        {
            // Entity -> VM
            CreateMap<RawMaterial, RawMaterialVM>();

            // VM -> Entity
            CreateMap<RawMaterialVM, RawMaterial>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Condition(src => src.CreatedDate != null))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(_ => DateTime.Now));
        }
    }
}
