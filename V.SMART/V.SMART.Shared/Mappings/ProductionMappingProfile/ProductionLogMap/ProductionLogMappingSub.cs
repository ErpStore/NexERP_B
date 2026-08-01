using AutoMapper;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using V.SMART.Shared.Data.Production.ProductionIssueWOAssy;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionIssueWOAssyVM;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionLogMap
{
    public class ProductionLogMappingSub : Profile
    {
        public ProductionLogMappingSub()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionLogSub, ProductionLogSubVM>()
                .ForMember(dest => dest.ProuctionStopType, opt => opt.MapFrom(src => src.ProductionStopageSettings != null ? src.ProductionStopageSettings.ProdStopType : string.Empty))

                .ForMember(dest => dest.EfficienctyPerc,
                    opt => opt.MapFrom(src => src.ProductionStopageSettings != null ? src.ProductionStopageSettings.EfficiencyPerc : 0))

                .ForMember(dest => dest.Defination, opt => opt.MapFrom(src => src.ProductionStopageSettings != null ? src.ProductionStopageSettings.Definition : string.Empty));

            // ===================== VM -> Entity =====================
            CreateMap<ProductionLogSubVM, ProductionLogSub>()
                .ForMember(dest => dest.ProductionStopageSettings, opt => opt.Ignore());
        }
    }
}
