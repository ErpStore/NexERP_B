using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchaseGRNMapping
{
    public class PurchaseGrnProfile : Profile
    {
        public PurchaseGrnProfile()
        {
            // ============== Entity → ViewModel Mapping ==============
            CreateMap<PurchaseGRN, PurchaseGRNVM>()
                // Vendor Details
                .ForMember(dest => dest.VendorName,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : string.Empty))
                .ForMember(dest => dest.VendorAddress,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorAddress : string.Empty))
                .ForMember(dest => dest.VendorGstNo,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.GSTNo : string.Empty))
                .ForMember(dest => dest.VendorContactNo,
                    opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.ContactNo : string.Empty))

                // Store Details
                .ForMember(dest => dest.StoreName,
                    opt => opt.MapFrom(src => src.Store != null ? src.Store.StoreName : string.Empty))

                // Child Collection
                .ForMember(dest => dest.PurchaseGRNSubVMs, opt => opt.MapFrom(src => src.PurchaseGRNSubs));

            // ============== ViewModel → Entity Mapping ==============
            CreateMap<PurchaseGRNVM, PurchaseGRN>()
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.Store, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseGRNSubs, opt => opt.Ignore());
        }
    }
}
