using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.SalesAndLabour.Leads;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILeadservice
{
    public interface ILeadService
    {
        Task<List<User>> GetUsersBySelectedStateId(int stateId);
        Task<Leads?> GetLeadsAsync(int LeadId);
        Task<bool> IsDuplicateLeadsNameAsync(string LeadName, int? LeadId = null);
        Task<Leads> CreateleadsAsync(Leads leads);
        Task<bool> DeleteLeadsAsync(int LeadId);
        Task<bool> DeleteContactByIdAsync(int Id);
        Task<bool> DeleteContactbyLeadIdAsync(int LeadId);
        Task<List<CustomerVM>> GetCustomersAsync();
        Task<IEnumerable<Leads>> GetAllLeadsAsync();
        Task<IEnumerable<User>> GetAllUsersAsync();
        Task<IEnumerable<State>> GetAllStatesAsync();
        Task<IEnumerable<Currency>> GetAllCurrencyAsync();
        Task<IEnumerable<Leads>> GetAllLoadLeadsAsync();
        Task<IEnumerable<ContactPerson>> GetContactPersonsByLeadAsync(int LeadId);
        Task<Customer> GetCustomerByNameAsync(string? LeadName);
        Task CreateOrUpdateContactsAsync(int leadId, string leadName, List<ContactPerson> contacts);
        Task<Leads> CreateCustomerAsync(Leads leads);
        Task<bool> CreateCurrencyAsync(Currency currency);

    }
}
