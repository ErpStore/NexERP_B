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
    internal class CustomerMapping : Profile
    {
        public CustomerMapping()
        {
            // M2-D02-01 / BR-CUST-017: the child collections were unmapped in both directions,
            // so CustomerVM could not round-trip a customer's consignees or contact persons.
            // Nothing consumed CustomerIndirectVMs / ContactPersonVMs before this change
            // (Confirmed: a search over V.SMART/ found hits only in CustomerVM.cs), so mapping
            // them adds behaviour to a dead path rather than altering an existing one.

            // Entity -> VM
            CreateMap<Customer, CustomerVM>()

                .ForMember(dest => dest.CurrName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty))
                .ForMember(dest => dest.CustomerIndirectVMs, opt => opt.MapFrom(src => src.CustomerIndirects))
                .ForMember(dest => dest.ContactPersonVMs, opt => opt.MapFrom(src => src.ContactPersons));

            // VM -> Entity
            CreateMap<CustomerVM, Customer>()
                .ForMember(dest => dest.CustomerIndirects, opt => opt.MapFrom(src => src.CustomerIndirectVMs))
                .ForMember(dest => dest.ContactPersons, opt => opt.MapFrom(src => src.ContactPersonVMs))
                .ForMember(dest => dest.ItemSubs, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Condition(src => src.CreatedDate != null))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(_ => DateTime.Now));
        }
    }
}
