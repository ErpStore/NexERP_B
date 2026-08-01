using AutoMapper;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    public class VendorMapping : Profile
    {
        public VendorMapping()
        {
            // ===================== Entity -> VM =====================
            CreateMap<Vendor, VendorVM>()

                // Category
                .ForMember(dest => dest.CurrencyName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty));

            // ===================== VM -> Entity =====================
            CreateMap<VendorVM, Vendor>()
                .ForMember(dest => dest.Currency, opt => opt.Ignore())
                .ForMember(dest => dest.VendorContacts, opt => opt.Ignore())
                .ForMember(dest => dest.VendorInDirects, opt => opt.Ignore())
                .ForMember(dest => dest.ItemSubs, opt => opt.Ignore());
        }
    }
}
