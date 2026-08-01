using AutoMapper;
using V.SMART.Shared.Data.Inspection.FinalInspection;
using V.SMART.Shared.Data.Inspection.IncomingInspection;
using V.SMART.Shared.ViewModels.InspectionViewModel.IncomingInspectionVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.InspectionMap.IncomingInspetionMap
{
    public class IncomingInspectionMap:Profile
    {
        public IncomingInspectionMap()
        {
            //====================== Entity->VM =====================
            CreateMap<IncomingInspection, IncomingInspectionVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : String.Empty))
                .ForMember(dest => dest.ItemUOM, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : String.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : String.Empty))
                .ForMember(dest => dest.processName, opt => opt.MapFrom(src => src.process != null ? src.process.ProcessName : String.Empty));

            //====================== VM -> Entity =====================
            CreateMap<IncomingInspectionVM, IncomingInspection>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.process, opt => opt.Ignore());

        }
    }
}
