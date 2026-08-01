using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.Credit_Note;
using V.SMART.Shared.Data.SalesAndLabour.SalesInvoice;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.CreditNote_VM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.MfgInvVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.SalesMapping.CreditNote_Profile
{
    public class CreditNoteProfile:Profile
    {
        public CreditNoteProfile()
        {
            CreateMap<CreditNote, CreditNoteVM>()
           .ForMember(dest => dest.CustName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
           .ForMember(dest => dest.CustAddress, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))
           .ForMember(dest => dest.GSTNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))
           .ForMember(dest => dest.ContactNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))

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

           .ForMember(dest => dest.CreditNoteSubVMs, opt => opt.MapFrom(src => src.CreditNoteSubs));

            // ===================== VM -> Entity =====================
            CreateMap<CreditNoteVM, CreditNote>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Consinee, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.CreditNoteSubs, opt => opt.Ignore());
        }
    }
}
