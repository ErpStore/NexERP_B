using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.SubContractDC;
using V.SMART.Shared.Data.OutSourcing.SubContractGRN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.SubContractGRNMapping
{
    public class SubConGRNProfile : Profile
    {
        public SubConGRNProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<SubConGRN, SubConGRNVM>()
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.vendor != null ? src.vendor.VendorName : null))

                 .ForMember(dest => dest.VendorAddress,
                     opt => opt.MapFrom(src => src.vendor != null ? src.vendor.VendorAddress : string.Empty))
                 .ForMember(dest => dest.VendorGstNo,
                     opt => opt.MapFrom(src => src.vendor != null ? src.vendor.GSTNo : string.Empty))
                 .ForMember(dest => dest.VendorContactNo,
                 opt => opt.MapFrom(src => src.vendor != null ? src.vendor.ContactNo : string.Empty))
                .ForMember(dest => dest.StoreName, opt => opt.MapFrom(src => src.AddStore != null ? src.AddStore.StoreName : null))

                .ForMember(dest => dest.NoOfItems, opt => opt.MapFrom(src => src.SubConGRNSubs.Count))
                  .ForMember(dest => dest.IsReturn,
                             opt => opt.MapFrom(src => src.Return))
                // Child Collection
                .ForMember(dest => dest.SubConGRNSubVMs, opt => opt.MapFrom(src => src.SubConGRNSubs))
                .ForMember(dest => dest.SubConGRNTrackVMs, opt => opt.MapFrom(src => src.SubConGRNTracks));

            // ===================== VM -> Entity =====================
            CreateMap<SubConGRNVM, SubConGRN>()
                .ForMember(dest => dest.vendor, opt => opt.Ignore())
                .ForMember(dest => dest.AddStore, opt => opt.Ignore())
                .ForMember(dest => dest.SubConGRNSubs, opt => opt.Ignore())
                .ForMember(dest => dest.SubConGRNTracks, opt => opt.Ignore());
        }
    }
}
