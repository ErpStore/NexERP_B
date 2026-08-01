using AutoMapper;
using V.SMART.Shared.Data.Production.DailyProductionLog;
using V.SMART.Shared.ViewModels.ProductionViewModel.ProductionLogViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ProductionMappingProfile.ProductionLogMap
{
    public class ProuctionLogMapping : Profile
    {
        public ProuctionLogMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ProductionLog, ProductionLogVM>()

                .ForMember(dest => dest.RCNo,
                    opt => opt.MapFrom(src =>
                        src.RouteCard != null
                            ? $"{src.RouteCard.RCNo} {src.RouteCard.Suffix}"
                            : string.Empty))

                .ForMember(dest => dest.InspectNo,
                    opt => opt.MapFrom(src =>
                        src.IncomingInspection != null
                            ? $"{src.IncomingInspection.InspectNo}{src.IncomingInspection.Suffix}"
                            : string.Empty))

                .ForMember(dest => dest.InspectDate,
                    opt => opt.MapFrom(src => src.IncomingInspection != null
                        ? src.IncomingInspection.InspectDate
                        : (DateTime?)null))



                .ForMember(dest => dest.ProcessName, opt => opt.MapFrom(src => src.Process != null ? src.Process.ProcessName : string.Empty))

                .ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src => src.Shift != null ? src.Shift.ShiftName : string.Empty))

                .ForMember(dest => dest.MachineName, opt => opt.MapFrom(src => src.Machine != null ? src.Machine.MachineName : string.Empty))

                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))

                .ForMember(dest => dest.ItemOutCode, opt => opt.MapFrom(src => src.ItemOut != null ? src.ItemOut.ItemCode : string.Empty))

                .ForMember(dest => dest.ItemInCode, opt => opt.MapFrom(src => src.ItemIn != null ? src.ItemIn.ItemCode : string.Empty))

                .ForMember(dest => dest.RewProcessName, opt => opt.MapFrom(src => src.ReworkProcess != null ? src.ReworkProcess.ProcessName : string.Empty))

                .ForMember(dest => dest.IssueStoreName, opt => opt.MapFrom(src => src.IssueStore != null ? src.IssueStore.StoreName : string.Empty))

                .ForMember(dest => dest.AddStoreName, opt => opt.MapFrom(src => src.AddStore != null ? src.AddStore.StoreName : string.Empty))

                .ForMember(dest => dest.CostCenterName, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ProductionLogSubVMs, opt => opt.MapFrom(src => src.ProductionLogSubs));

            // ===================== VM -> Entity =====================
            CreateMap<ProductionLogVM, ProductionLog>()
                .ForMember(dest => dest.RouteCard, opt => opt.Ignore())
                .ForMember(dest => dest.RouteCardProcess, opt => opt.Ignore())
                .ForMember(dest => dest.Process, opt => opt.Ignore())
                .ForMember(dest => dest.Shift, opt => opt.Ignore())
                .ForMember(dest => dest.Machine, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.ItemOut, opt => opt.Ignore())
                .ForMember(dest => dest.ItemIn, opt => opt.Ignore())
                .ForMember(dest => dest.ReworkProcess, opt => opt.Ignore())
                .ForMember(dest => dest.IssueStore, opt => opt.Ignore())
                .ForMember(dest => dest.AddStore, opt => opt.Ignore())
                .ForMember(dest => dest.CostCenter, opt => opt.Ignore())
                .ForMember(dest => dest.ProductionLogSubs, opt => opt.Ignore());


        }
    }
}
