using AutoMapper;
using V.SMART.Shared.Data.OutSourcing.PurchaseEnquiry;
using V.SMART.Shared.Data.OutSourcing.PurchasePo;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchOrSubConEnquiryVM;
using V.SMART.Shared.ViewModels.OutSourcingViewModel.PurchPoVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.OutsourcingMapping.PurchOrSubConPo
{
    public class PurchPoProfile : Profile
    {
        public PurchPoProfile()
        {
            // ===================== Entity -> VM =====================
            CreateMap<PurchPo, PurchPoVM>()
                .ForMember(dest => dest.VendorName, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : null))
                .ForMember(dest => dest.VendorAddress, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorAddress : null))
                .ForMember(dest => dest.VendorPhoneNo, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.ContactNo : null))
                .ForMember(dest => dest.VendorGSTNO, opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.GSTNo : null))
                .ForMember(dest => dest.NoOfItems, opt => opt.MapFrom(src => src.PurchPoSubs.Count))

                // Child Collection
                .ForMember(dest => dest.PurchPoSubVMs, opt => opt.MapFrom(src => src.PurchPoSubs));

            // ===================== VM -> Entity =====================
            CreateMap<PurchPoVM, PurchPo>()
                .ForMember(dest => dest.Vendor, opt => opt.Ignore())
                .ForMember(dest => dest.Consinee, opt => opt.Ignore())
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.PurchPoSubs, opt => opt.Ignore());
        }
    }
}
