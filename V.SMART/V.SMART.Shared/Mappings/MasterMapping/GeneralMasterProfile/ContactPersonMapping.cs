using AutoMapper;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.GeneralMasterProfile
{
    public class ContactPersonMapping : Profile
    {
        public ContactPersonMapping()
        {
            // Entity -> VM
            CreateMap<ContactPerson, ContactPersonVM>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty));

            // VM -> Entity
            // M2-D02-01 / BR-CUST-017: ContactPersonVM -> ContactPerson did not exist, so the
            // ContactPersons <-> ContactPersonVMs collection mapping added to CustomerMapping
            // had no element map to use. Added here, its natural home.
            CreateMap<ContactPersonVM, ContactPerson>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Lead, opt => opt.Ignore());

            CreateMap<CustomerIndirectVM, CustomerIndirect>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore());
        }
    }
}
