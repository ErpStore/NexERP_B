using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.PerformaInvoice;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.PerformaInvoiceVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.PerformaInvoiceProfile
{
    public class PerformaInvProfile : Profile
    {
        public PerformaInvProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<PerformaInv, PerformaInvVM>()
                // Customer
                .ForMember(dest => dest.CustName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
                .ForMember(dest => dest.CustAddress, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))
                .ForMember(dest => dest.CustGst, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))
                .ForMember(dest => dest.CustContactNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))

                // Consignee / Shipping
                .ForMember(dest => dest.ShippingName, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.AltCustName : string.Empty))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.Consinee != null ?
                    string.Join(" ", new[] { src.Consinee.AltCustAddr1, src.Consinee.AltCustAddr2 }.Where(a => !string.IsNullOrEmpty(a)))
                    : string.Empty))
                .ForMember(dest => dest.ShippingGstin, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.GSTNo : string.Empty))
                .ForMember(dest => dest.ShippingContactNo, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.ContactNo : string.Empty))

                // Currency
                .ForMember(dest => dest.CurrName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty))
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : string.Empty))

                // Bank
                .ForMember(dest => dest.BankName, opt => opt.MapFrom(src => src.Banks != null ? src.Banks.BankName : string.Empty))

                // Sub-items
                .ForMember(dest => dest.PerformaInvSubVMs, opt => opt.MapFrom(src => src.PerformaInvSubs));

            // ===================== VM -> Entity =====================
            CreateMap<PerformaInvVM, PerformaInv>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Consinee, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Banks, opt => opt.Ignore())
                .ForMember(dest => dest.PerformaInvSubs, opt => opt.Ignore());
        }
    }
}
