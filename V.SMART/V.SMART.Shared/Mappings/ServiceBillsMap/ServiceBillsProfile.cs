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
    public class ServiceBillsProfile :Profile
    {
        public ServiceBillsProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<ServiceBills, ServiceBillsVM>()

                // Currency
                .ForMember(dest => dest.CurrName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty))
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : string.Empty))

                // Child Collection
                .ForMember(dest => dest.ServiceBillsSub, opt => opt.MapFrom(src => src.ServiceBillSub));

            // ===================== VM -> Entity =====================
            CreateMap<ServiceBillsVM, ServiceBills>()
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.ServiceBillSub, opt => opt.Ignore());
        }
    }
}
