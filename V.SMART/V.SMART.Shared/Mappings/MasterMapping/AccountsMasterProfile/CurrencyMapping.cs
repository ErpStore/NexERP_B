using AutoMapper;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.ViewModels.MasterViewModel.AccountsViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.Mappings.MasterMapping.AccountsMasterProfile
{
    public class CurrencyMapping : Profile
    {
        public CurrencyMapping()
        {
            // Entity -> VM
            CreateMap<Currency, CurrencyVM>();

            // VM -> Entity
            CreateMap<CurrencyVM, Currency>();
        }
    }
}
