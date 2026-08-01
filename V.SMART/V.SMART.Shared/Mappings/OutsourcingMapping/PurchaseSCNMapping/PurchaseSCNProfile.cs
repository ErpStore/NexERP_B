using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseGRN;
using V.SMART.Shared.Data.OutSourcing.PurchaseSCN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseGRNVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchaseSCNVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchaseSCNMapping
{
    public class PurchaseSCNProfile:Profile
    {
        public PurchaseSCNProfile()
        {
            // ============== Entity → ViewModel Mapping ==============
            CreateMap<PurchaseSCN, PurchaseSCNVM>()
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
                .ForMember(dest => dest.AddStoreName,
                    opt => opt.MapFrom(src => src.StoreAdd != null ? src.StoreAdd.StoreName : string.Empty))
                .ForMember(dest => dest.IssueStoreName,
                    opt => opt.MapFrom(src => src.StoreIssue != null ? src.StoreIssue.StoreName : string.Empty))
                // Child Collection
                .ForMember(dest => dest.PurchaseSCNSubVMs, opt => opt.MapFrom(src => src.PurchaseSCNSubs));

            // ============== ViewModel → Entity Mapping ==============
            CreateMap<PurchaseSCNVM, PurchaseSCN>()
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.StoreAdd, opt => opt.Ignore())
                .ForMember(dest => dest.StoreIssue, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseSCNSubs, opt => opt.Ignore());
        }
    }
}
