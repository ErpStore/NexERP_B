using AutoMapper;
using FastReport.Utils;
using V.SMART.Shared.Data.Inspection.MasterInspection;
using V.SMART.Shared.ViewModels.InspectionViewModel.MasterInspectionVM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.InspectionMap.MasterInspectionMap
{
    public class MasterInspectionMap : Profile
    {
        public MasterInspectionMap()
        {
            //====================== Entity -> VM =====================
            CreateMap<MasterInspection, MasterInspectionVM >()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : String.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item!=null ? src.Item.ItemName : String.Empty))
                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : String.Empty));

            //====================== VM -> Entity =====================
            CreateMap<MasterInspectionVM, MasterInspection>()
                .ForMember(dest => dest.Item, opt => opt.Ignore())
                .ForMember(dest => dest.Process, opt => opt.Ignore());  
        }
    }
}
