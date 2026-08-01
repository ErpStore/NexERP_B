using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour_Module.SalesQuotation;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;

namespace V.SMART.Shared.Mappings.SalesMapping.MfgQuotation
{
    public class MfgQuoteProfile : Profile
    {
        public MfgQuoteProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<MfgQuote, MfgQuoteVM>()
                .ForMember(dest => dest.CustName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
                .ForMember(dest => dest.CustAddress, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))
                .ForMember(dest => dest.ContactNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))
                .ForMember(dest => dest.GSTNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))
                .ForMember(dest => dest.PANNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.PANNo : string.Empty))

                // Shipping / Consignee
                .ForMember(dest => dest.ShippingName, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.AltCustName : string.Empty))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.AltCustAddr1 + src.Consinee.AltCustAddr2 : string.Empty))
                .ForMember(dest => dest.ShippingGstin, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.GSTNo : string.Empty))
                .ForMember(dest => dest.ShippingContactNo, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.ContactNo : string.Empty))

                // Currency
                .ForMember(dest => dest.CurrName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty))
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : string.Empty))

                // Terms & Conditions
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.TermsAndConditions != null ? src.TermsAndConditions.Details : string.Empty))

                // Child Collection
                .ForMember(dest => dest.MfgQuoteSubVM, opt => opt.MapFrom(src => src.MfgQuoteSub));

            // ===================== VM -> Entity =====================
            CreateMap<MfgQuoteVM, MfgQuote>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore()) // handled separately
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Consinee, opt => opt.Ignore())
                .ForMember(dest => dest.TermsAndConditions, opt => opt.Ignore())
                .ForMember(dest => dest.MfgQuoteSub, opt => opt.Ignore());
        }
    }
}
