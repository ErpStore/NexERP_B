using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.Purchase_Invoice;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseInvoiceVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchaseInvoice_Mapping
{
    public class PurchaseInvoiceProfile:Profile
    {
        public PurchaseInvoiceProfile() 
        {
            CreateMap<PurchaseInvoice, PurchaseInvoiceVM>()

                // Vendor Details
                .ForMember(dest => dest.VendorName,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : string.Empty))
                .ForMember(dest => dest.VendorAddress,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorAddress : string.Empty))
                .ForMember(dest => dest.VendorGstNo,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.GSTNo : string.Empty))
                .ForMember(dest => dest.VendorContactNo,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.ContactNo : string.Empty))

                // Consignee / Shipping
                .ForMember(dest => dest.ShippingName, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.AltVendorName : string.Empty))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.Consinee != null ?
                    string.Join(" ", new[] { src.Consinee.AltVendorAddr1, src.Consinee.AltVendorAddr2 }.Where(a => !string.IsNullOrEmpty(a)))
                    : string.Empty))
                .ForMember(dest => dest.ShippingGstin, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.GSTNo : string.Empty))
                .ForMember(dest => dest.ShippingContactNo, opt => opt.MapFrom(src => src.Consinee != null ? src.Consinee.ContactNo : string.Empty))

                 .ForMember(dest => dest.CurrName,
                    opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty))

                 .ForMember(dest => dest.Symbol,
                    opt => opt.MapFrom(src => src.Currency != null ? src.Currency.Symbol : string.Empty))

                .ForMember(dest => dest.PurchaseInvoiceSubVMs, opt => opt.MapFrom(src => src.PurchaseInvoiceSubs));

            // ============== ViewModel → Entity Mapping ==============
            CreateMap<PurchaseInvoiceVM, PurchaseInvoice>()
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseInvoiceSubs, opt => opt.Ignore());
        }

    }
}
