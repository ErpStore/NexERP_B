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
            CreateMap<CustomerIndirectVM, CustomerIndirect>()
                .ForMember(dest => dest.Customer, opt => opt.Ignore());
        }
    }
}
