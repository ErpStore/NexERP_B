using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.SubContractSCN;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.SubContractViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.SubContractSCNMapping
{
    public class SubContractSCNProfile : Profile
    {
        public SubContractSCNProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<SubConSCN, SubConSCNVM>()

                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.vendor != null ? src.vendor.VendorName : null))
                .ForMember(dest => dest.VendorAddress, opt => opt.MapFrom(src => src.vendor != null ? src.vendor.VendorAddress : string.Empty))

                .ForMember(dest => dest.VendorGSTNo, opt => opt.MapFrom(src => src.vendor != null ? src.vendor.GSTNo : string.Empty))
                .ForMember(dest => dest.VendorContactNo, opt => opt.MapFrom(src => src.vendor != null ? src.vendor.ContactNo : string.Empty))

                .ForMember(dest => dest.AddStoreName, opt => opt.MapFrom(src => src.AddStore != null ? src.AddStore.StoreName : null))

                .ForMember(dest => dest.IssueStoreName, opt => opt.MapFrom(src => src.IssueStore != null ? src.IssueStore.StoreName : null))

                .ForMember(dest => dest.NoOfItems, opt => opt.MapFrom(src => src.SubConSCNSubs.Count))

                // Child Collection
                .ForMember(dest => dest.SubConSCNSubVMs, opt => opt.MapFrom(src => src.SubConSCNSubs));

            // ===================== VM -> Entity =====================
            CreateMap<SubConSCNVM, SubConSCN>()
                .ForMember(dest => dest.vendor, opt => opt.Ignore())
                .ForMember(dest => dest.AddStore, opt => opt.Ignore())
                .ForMember(dest => dest.IssueStore, opt => opt.Ignore())
                .ForMember(dest => dest.SubConSCNSubs, opt => opt.Ignore());
        }
    }
}
