using AutoMapper;
using V.SMART.Shared.Data.Inspection.FinalInspection;
using V.SMART.Shared.Data.Inspection.MasterInspection;
using V.SMART.Shared.ViewModels.InspectionViewModel.FinalInspectionVM;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.InspectionMap.FinalInspectionMap
{
    public class FinalInspectionMap:Profile
    {
        public FinalInspectionMap()
        {
            //====================== Entity -> VM =====================
            CreateMap<FinalInspection, FinalInspectionVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : String.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : String.Empty))
                .ForMember(dest => dest.ItemUOM, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : String.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : String.Empty));

            //====================== VM -> Entity =====================
            CreateMap<FinalInspectionVM, FinalInspection>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore()); 
        }
    }
}
