using AutoMapper;
using V.SMART.Shared.Data.SalesAndLabour.SalesDC;
using V.SMART.Shared.Data.SalesAndLabour.SalesPo;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.ManufacturingPoVM;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.SalesDCVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.ManufacturingDcProfile
{
    public class MfgDcProfile : Profile
    {
        public MfgDcProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<MfgDc, MfgDcVM>()
                // Customer
                .ForMember(dest => dest.CustName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
                .ForMember(dest => dest.CustAddress, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustAddr : string.Empty))
                .ForMember(dest => dest.ContactNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.ContactNo : string.Empty))
                .ForMember(dest => dest.CustGstNo, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.GSTNo : string.Empty))

                // Consignee / Shipping
                .ForMember(dest => dest.ShippingName, opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.AltCustName : string.Empty))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.Consignee != null
                    ? string.Join(" ", new[] { src.Consignee.AltCustAddr1, src.Consignee.AltCustAddr2 }.Where(a => !string.IsNullOrEmpty(a)))
                    : string.Empty))
                .ForMember(dest => dest.ShippingGstNo, opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.GSTNo : string.Empty))
                .ForMember(dest => dest.ShippingPhone, opt => opt.MapFrom(src => src.Consignee != null ? src.Consignee.ContactNo : string.Empty))

                // Issue Store
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))

                // Vendor
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : string.Empty))
                .ForMember(dest => dest.VendorAddress, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorAddress : string.Empty))
                .ForMember(dest => dest.VendorGstNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.GSTNo : string.Empty))
                .ForMember(dest => dest.VendorContactNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.ContactNo : string.Empty))

                // Child Collection
                .ForMember(dest => dest.MfgDcSubVMs, opt => opt.MapFrom(src => src.MfgDcSubs))

                // Flatten other fields as-is
                .ForMember(dest => dest.MfgDcSubVMs, opt => opt.MapFrom(src => src.MfgDcSubs));

            // ===================== VM -> Entity =====================
            CreateMap<MfgDcVM, MfgDc>()
                // Ignore navigation properties to avoid EF Core tracking issues
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Consignee, opt => opt.Ignore())
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.MfgDcSubs, opt => opt.Ignore());

        }
    }
}
