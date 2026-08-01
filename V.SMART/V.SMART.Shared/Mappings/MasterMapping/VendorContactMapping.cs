using AutoMapper;
using V.SMART.Shared.Data.Master.General_Module;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping
{
    public class VendorContactMapping: Profile
    {
        public VendorContactMapping()
        {
            // ===================== Entity -> VM =====================

            CreateMap<VendorContact, VendorContactVM>()
            .ForMember(dest => dest.VendorName,
                opt => opt.MapFrom(src => src.Vendor != null ? src.Vendor.VendorName : string.Empty));

            CreateMap<VendorContactVM, VendorContact>()
                .ForMember(dest => dest.Vendor, opt => opt.Ignore());

        }
    }
}
