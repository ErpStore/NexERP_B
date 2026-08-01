using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.CashFlow.ServiceBills;
using V.SMART.Shared.ViewModels.CashFlowViewModel.ServiceBillViewModel;

namespace IQSMART.Shared.Mappings.CashFlowMap.ServiceBillsMap
{
    public class ServiceBillsSubProfile :Profile
    {
        public ServiceBillsSubProfile()
        {

            // ===================== Entity -> VM =====================

            CreateMap<ServiceBillsSub, ServiceBillsSubVM>()
                .ForMember(dest => dest.ItemCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemCode : string.Empty))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.Item != null ? src.Item.ItemName : string.Empty))
                .ForMember(dest => dest.HsnCode, opt => opt.MapFrom(src => src.Item != null ? src.Item.HSNCode : string.Empty))
                .ForMember(dest => dest.MeasureUnit, opt => opt.MapFrom(src => src.Item != null ? src.Item.MeasureUnit : string.Empty))
                .ForMember(dest => dest.ProjectNo, opt => opt.MapFrom(src => src.CostCenter != null ? src.CostCenter.ProjectNo : string.Empty))
                .ForMember(dest => dest.IsEditable, opt => opt.MapFrom(src => false)); // default false unless UI overrides

            // ===================== VM -> Entity =====================
            CreateMap<ServiceBillsSubVM, ServiceBillsSub>()
                            .ForMember(dest => dest.Item, opt => opt.Ignore())
                            .ForMember(dest => dest.MfgPoSub, opt => opt.Ignore())
                            .ForMember(dest => dest.CostCenter, opt => opt.Ignore());    // handled by CostId
                           
        }
    }
}
