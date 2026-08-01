using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using FastReport.Utils;
using V.SMART.Shared.Data.Maintenance.BreakdownMaintenance;
using V.SMART.Shared.Data.Maintenance.CalibrationHistoryAndMaintenance;
using V.SMART.Shared.ViewModels.MaintenanceViewModel.BreakdownMaintenanceVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MaintenanceMap.CalibrationHistoryMaintenanceMap
{
    public class CalibrationHistoryMap : Profile
    {

        public CalibrationHistoryMap()
        {
            //Calibration History Mapping

            //Entity => VM
            CreateMap<CalibrationHistory, CalibrationHistoryVM>()

                //Instrument Id Mapping 
                .ForMember(dest => dest.InstrumentId,
                    opt => opt.MapFrom(src => src.InstrumentId))

                //Instrument Name
                .ForMember(dest => dest.InstrumentName,
                            opt => opt.MapFrom(src => src.Instrument != null && src.Instrument.Item != null
                                    ? src.Instrument.Item.ItemCode : string.Empty))

                //Instrument Unique Id
                .ForMember(dest => dest.UniqueId,
                            opt => opt.MapFrom(src => src.Instrument != null && src.Instrument.UniqueId != null ? src.Instrument.UniqueId : string.Empty))

                //Instrument Frequency
                .ForMember(d => d.Frequency,
                             o => o.MapFrom(s => s.Instrument.Frequency != null && s.Instrument != null ? s.Instrument.Frequency : string.Empty))

                //Instrument Range
                .ForMember(d => d.Range,
                             o => o.MapFrom(s => s.Instrument != null && s.Instrument.Range != null ? s.Instrument.Range : string.Empty))

                //Instrument Least Count 
                .ForMember(d => d.LeastCount,
                             o => o.MapFrom(s => s.Instrument != null && s.Instrument.LeastCount !=null ? s.Instrument.LeastCount : string.Empty)); 

            // VM => Entity
            CreateMap<CalibrationHistoryVM, CalibrationHistory>()
                .ForMember(dest => dest.Instrument, opt => opt.Ignore());



            //Instrument Details Mapping

            //Entity => VM
            CreateMap<InstrumentDetails, InstrumentDetailsVM>()

                //Item Id
                     .ForMember(dest => dest.ItemId,
                         opt => opt.MapFrom(src => src.ItemId))
                //ItemName
                     .ForMember(dest => dest.ItemName,
                         opt => opt.MapFrom(src =>
                             src.Item != null ? src.Item.ItemName : string.Empty))
               //ItemCode
                     .ForMember(dest => dest.ItemCode,
                         opt => opt.MapFrom(src =>
                             src.Item != null ? src.Item.ItemCode : string.Empty));


            // VM=>Entity
            CreateMap<InstrumentDetailsVM, InstrumentDetails>()
                     .ForMember(dest => dest.Item, opt => opt.Ignore());

        }

    }
}
