using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchOrSubComEnquiryMapping
{
    public class EnquiryPurchaseVendorAssignProfile : Profile
    {
        public EnquiryPurchaseVendorAssignProfile()
        {
            // ------------------------------------------------------------
            // ENTITY → VIEWMODEL
            // ------------------------------------------------------------
            CreateMap<EnquiryPurchaseVendorAssign, EnquiryPurchaseVendorAssignVM>()

                 //Optional: include vendor info if needed in VM
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : null))
                .ForMember(dest => dest.VendorAddress, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorAddress : null))
                .ForMember(dest => dest.VendorGSTNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.GSTNo : null))
                .ForMember(dest => dest.VendorPhoneNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.ContactNo : null))
                ;

            // ------------------------------------------------------------
            // VIEWMODEL → ENTITY
            // ------------------------------------------------------------
            CreateMap<EnquiryPurchaseVendorAssignVM, EnquiryPurchaseVendorAssign>()
                .ForMember(dest => dest.Enquiry, opt => opt.Ignore())
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.EnquiryPurchaseSub, opt => opt.Ignore());
        }
    }

}
