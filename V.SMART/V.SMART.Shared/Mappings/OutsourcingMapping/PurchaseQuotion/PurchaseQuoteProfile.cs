using AutoMapper;
using V.SMART.Shared.Data.PurchaseAndSubcontract.Purchase_Quote;
using V.SMART.Shared.ViewModels.PurchAndSubConViewModel.Purch_QuotationVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.PurchaseQuotion
{
    public class PurchaseQuoteProfile : Profile
    {
        public PurchaseQuoteProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<PurchaseQuote, PurchaseQuoteVM>()
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : string.Empty))
                .ForMember(dest => dest.VendorAddress, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorAddress : string.Empty))
                .ForMember(dest => dest.ContactNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.ContactNo : string.Empty))
                .ForMember(dest => dest.GSTNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.GSTNo : string.Empty))
                .ForMember(dest => dest.PANNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.PANNo : string.Empty))

                // Shipping / Consignee
                .ForMember(dest => dest.ShippingName, opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.AltVendorName : string.Empty))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.AltVendorAddr1 + src.Consignee.AltVendorAddr2 : string.Empty))
                .ForMember(dest => dest.ShippingGstin, opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.GSTNo : string.Empty))
                .ForMember(dest => dest.ShippingContactNo, opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.ContactNo : string.Empty))

                // Currency
                .ForMember(dest => dest.CurrName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty))
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : string.Empty))

                // Terms & Conditions
                .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.TermsAndConditions != null ? src.TermsAndConditions.Details : string.Empty))

                // Child Collection
                .ForMember(dest => dest.PurchaseQuoteSubVM, opt => opt.MapFrom(src => src.PurchaseQuoteSub));

            // ===================== VM -> Entity =====================
            CreateMap<PurchaseQuoteVM, PurchaseQuote>()
                .ForMember(dest => dest.Vendor, opt => opt.Ignore()) // handled separately
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.Consignee, opt => opt.Ignore())
                .ForMember(dest => dest.TermsAndConditions, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseQuoteSub, opt => opt.Ignore());
        }
    }

}
