using AutoMapper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.Data.OutSourcing.Debit_Note;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.DebitNote_VM;

namespace IQSMART.Shared.Mappings.OutsourcingMapping.DebitNote_Profile
{
    public class DebitNoteProfile:Profile
    {
        public DebitNoteProfile()
        {
            CreateMap<DebitNote, DebitNoteVM>()
           .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : string.Empty))
           .ForMember(dest => dest.VendorAddress, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorAddress : string.Empty))
           .ForMember(dest => dest.VendorGstNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.GSTNo : string.Empty))
           .ForMember(dest => dest.VendorContactNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.ContactNo : string.Empty))
           .ForMember(dest => dest.PAN, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.PANNo : string.Empty))

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

           .ForMember(dest => dest.DebitNoteSubVMs, opt => opt.MapFrom(src => src.DebitNoteSubs));

            // ===================== VM -> Entity =====================
            CreateMap<DebitNoteVM, DebitNote>()
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.Consinee, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.DebitNoteSubs, opt => opt.Ignore());
        }
    }
}
