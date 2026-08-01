using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.SubConDcOutMapping
{
    public class SubConDcOutProfile : Profile
    {
        public SubConDcOutProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<SubConDcOut, SubConDcOutVM>()
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : null))
                .ForMember(dest => dest.VendorAddress, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorAddress : null))
                .ForMember(dest => dest.VendorContactNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.ContactNo : null))
                .ForMember(dest => dest.VendorGstNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.GSTNo : null))

                //Consignee
                .ForMember(dest => dest.ShippingName,
                    opt => opt.MapFrom(src => src.Consignee != null
                        ? src.Consignee.AltVendorName
                        : null))

                .ForMember(dest => dest.ShippingAddress,
                    opt => opt.MapFrom(src => src.Consignee != null
                        ? $"{src.Consignee.AltVendorAddr1} {src.Consignee.AltVendorAddr2}".Trim()
                        : null))

                .ForMember(dest => dest.ShippingContactNo,
                    opt => opt.MapFrom(src => src.Consignee != null
                        ? src.Consignee.ContactNo
                        : null))

                .ForMember(dest => dest.ShippingGstin,
                    opt => opt.MapFrom(src => src.Consignee != null
                        ? src.Consignee.GSTNo
                        : null))

                .ForMember(dest => dest.StoreIssueName, opt => opt.MapFrom(src => src.StoreIssue != null ? src.StoreIssue.StoreName : null))

                .ForMember(dest => dest.NoOfItems, opt => opt.MapFrom(src => src.SubConDcOutSubs.Count))


                // Child Collection
                .ForMember(dest => dest.SubConDcOutSubVMs, opt => opt.MapFrom(src => src.SubConDcOutSubs));

            // ===================== VM -> Entity =====================
            CreateMap<SubConDcOutVM, SubConDcOut>()
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.Consignee, opt => opt.Ignore())
                .ForMember(dest => dest.StoreIssue, opt => opt.Ignore())
                .ForMember(dest => dest.SubConDcOutSubs, opt => opt.Ignore());
        }
    }
}
