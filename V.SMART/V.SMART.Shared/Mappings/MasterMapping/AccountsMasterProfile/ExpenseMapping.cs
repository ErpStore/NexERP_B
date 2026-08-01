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
    public class ExpenseMapping : Profile
    {
        public ExpenseMapping()
        {
            // Entity -> VM
            CreateMap<Expense, ExpenseVM>();

            // VM -> Entity
            CreateMap<ExpenseVM, Expense>();
        }
    }
}
