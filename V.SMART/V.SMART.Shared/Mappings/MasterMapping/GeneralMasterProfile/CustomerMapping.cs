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
            // Entity -> VM
            CreateMap<Customer, CustomerVM>()
                
                .ForMember(dest => dest.CurrName, opt => opt.MapFrom(src => src.Currency != null ? src.Currency.CurrName : string.Empty));

            // VM -> Entity
            CreateMap<CustomerVM, Customer>()
                .ForMember(dest => dest.CustomerIndirects, opt => opt.Ignore())
                .ForMember(dest => dest.ContactPersons, opt => opt.Ignore())
                .ForMember(dest => dest.ItemSubs, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Condition(src => src.CreatedDate != null))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(_ => DateTime.Now));
        }
    }
}
