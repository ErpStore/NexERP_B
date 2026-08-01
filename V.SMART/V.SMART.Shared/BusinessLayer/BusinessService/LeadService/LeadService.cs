using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ILeadservice;
using V.SMART.Shared.Data.Master.Accounts;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Data.SalesAndLabour.Leads;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.LeadService
{
    public class LeadService : ILeadService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;

        public LeadService(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService, ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
        }


        public async Task<List<User>> GetUsersBySelectedStateId(int stateId)
        {
            string stateStr = stateId.ToString();

            return await _unitOfWork.Users.GetQueryable()
                .Where(u =>
                    EF.Functions.Like(u.StateCodesCsv, $"{stateStr}") ||
                    EF.Functions.Like(u.StateCodesCsv, $"{stateStr},%") ||
                    EF.Functions.Like(u.StateCodesCsv, $"%,{stateStr},%") ||
                    EF.Functions.Like(u.StateCodesCsv, $"%,{stateStr}")
                )
                .ToListAsync();
        }

        public async Task<Leads?> GetLeadsAsync(int LeadId)
        {
            try
            {
                return await _unitOfWork.Leads.GetAsync(LeadId);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get GetLeadsAsync with LeadId {LeadId}");
                throw;
            }
        }
        public async Task<IEnumerable<ContactPerson>> GetContactPersonsByLeadAsync(int leadId)
        {
            try
            {
                var contacts = await _unitOfWork.ContactPersons.FindAsync(x => x.LeadId == leadId);

                return contacts?.ToList() ?? new List<ContactPerson>();

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get contact persons for LeadId: {leadId}");
                throw;
            }
        }

        public async Task<Customer> GetCustomerByNameAsync(string LeadName)
        {
            try
            {
                return await _unitOfWork.Customers.GetQueryable().Where(c => c.CustName == LeadName).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get GetCustomerByNameAsync for LeadId: {LeadName}");
                throw;
            }
        }

        public async Task<List<CustomerVM>> GetCustomersAsync()
        {
            return await _commonService.GetAllActiveCustomersAsync();
        }

        public async Task<IEnumerable<Leads>> GetAllLeadsAsync()
        {
            try
            {
                return await _unitOfWork.Leads.GetAllAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get GetAllLeadsAsync with Leads");
                throw;
            }
        }
        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            try
            {
                return await _unitOfWork.Users.GetAllAsync();
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to Load user in leads GetAllUsersAsync Method");
                throw;
            }
        }
        public async Task<IEnumerable<State>> GetAllStatesAsync()
        {
            return await _commonService.GetStatesAsync();
        }
        public async Task<IEnumerable<Currency>> GetAllCurrencyAsync()
        {
            return await _commonService.GetCurrenciesAsync();
        }

        public async Task<IEnumerable<Leads>> GetAllLoadLeadsAsync()
        {
            try
            {
                var currentUserName = await _currentUserService.GetUsernameAsync();
                var Currentuser = (await _unitOfWork.Users.GetAllAsync())
                    .FirstOrDefault(u => u.UserName == currentUserName);

                var userStatecode = Currentuser?.StateCodesCsv?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList() ?? new List<string>();

                var leads = (await _unitOfWork.Leads.GetAllAsync())
                     .Where(l => userStatecode.Contains(l.StateId.ToString()))
                     .OrderByDescending(s => s.LeadId)
                     .ToList();
                return leads;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to get GetAllLoadLeadsAsync with Leads");
                throw;
            }
        }
        public async Task<bool> IsDuplicateLeadsNameAsync(string LeadName, int? LeadId = null)
        {
            try
            {
                return await _unitOfWork.Leads.ExistsByNameAsync("LeadName", LeadName?.Trim(), "LeadId", LeadId == 0 ? null : LeadId);

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error checking duplicate Leads name");
                throw;
            }
        }
        public async Task<bool> DeleteContactByIdAsync(int Id)
        {
            try
            {
                await _unitOfWork.ContactPersons.DeleteAsync(Id);
                await _unitOfWork.SaveAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting contact for leads with Id: {Id}");
                return false;
            }
        }
        public async Task<bool> DeleteContactbyLeadIdAsync(int LeadId)
        {
            try
            {
                var oldContacts = await _unitOfWork.ContactPersons.FindAsync(x => x.LeadId == LeadId);
                foreach (var ct in oldContacts)
                {
                    await _unitOfWork.ContactPersons.DeleteAsync(ct.Id);
                }
                await _unitOfWork.SaveAsync();

                return true;

            }
            catch (Exception ex)
            {

                await _loggingService.LogDeveloperError(ex, $"Error deleting leads contact with Id: {LeadId}");
                return false;
            }

        }
        public async Task<Leads> CreateleadsAsync(Leads leads)
        {
            try
            {
                if (await IsDuplicateLeadsNameAsync(leads.LeadName, leads.LeadId == 0 ? null : leads.LeadId))
                {
                    await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), _currentUserService.MachineName, _currentUserService.IpAddress, "Category", $"Failed to create duplicate Leads: {leads.LeadName}", "");
                    throw new InvalidOperationException("Leads name already exists.");
                }
                if (leads.LeadId == 0)
                {
                    leads.CreatedBy = await _currentUserService.GetUsernameAsync();
                    leads.CreatedDate = DateTime.Now;
                    await _unitOfWork.Leads.CreateAsync(leads);
                    await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), Environment.MachineName, _currentUserService.IpAddress, "Leads", $"Created category: {leads.LeadName}", "");
                }
                else
                {
                    leads.ModifiedBy = await _currentUserService.GetUsernameAsync();
                    leads.ModifiedDate = DateTime.Now;
                    await _unitOfWork.Leads.UpdateAsync(leads);
                    await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), Environment.MachineName, _currentUserService.IpAddress, "Leads", $"Updated category: {leads.LeadName}", "");
                }
                await _unitOfWork.SaveAsync();
                return leads;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to create CreateleadsAsync with leadName: {leads.LeadName}");
                throw;
            }
        }
        public async Task<Leads> CreateCustomerAsync(Leads Leads)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var UserName = await _currentUserService.GetUsernameAsync();

                Customer newCustomer = new Customer
                {
                    CustName = Leads.LeadName,
                    CustAddr = Leads.Address,
                    PinCode = Leads.PinCode,
                    BusiType = Leads.BusiType,
                    SupTyp = Leads.SupType,
                    GSTNo = Leads.GSTNo,
                    PANNo = Leads.PANNo,
                    CurrId = Leads.CurrId,
                    Currency = Leads.Currency,
                    StateName = Leads.StateName,
                    Location = Leads.City,
                    Distance = 1,
                    StateId = Leads.StateId,
                    Email = Leads.Email,
                    Remark = Leads.Remarks,
                    ContactNo = Leads.PhoneNo,
                    Inactive = false,
                    CreatedBy = Leads.CreatedBy,
                    CreatedDate = Leads.CreatedDate,
                };

                await _unitOfWork.Customers.CreateAsync(newCustomer);
                await _unitOfWork.SaveAsync();

                int newCustomerId = newCustomer.CustId;

                var contactPersons = await _unitOfWork.ContactPersons.FindAsync(x => x.LeadId == Leads.LeadId);
                foreach (var contact in contactPersons)
                {
                    contact.CustId = newCustomerId;
                    await _unitOfWork.ContactPersons.UpdateAsync(contact);
                }

                Leads.LeadTally = true;
                await _unitOfWork.Leads.UpdateAsync(Leads);

                await _unitOfWork.SaveAsync();

                await transaction.CommitAsync();

                await _loggingService.LogUserAction(
                    UserName,
                    Environment.MachineName,
                    _currentUserService.IpAddress,
                    "Category",
                    $"Created Customer: {Leads.LeadName}",
                    ""
                );

                return Leads;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(); 
                await _loggingService.LogDeveloperError(ex, $"Failed to create Customer: {Leads.LeadName}");
                throw;
            }
        }



        public async Task CreateOrUpdateContactsAsync(int leadId, string leadName, List<ContactPerson> contacts)
        {
            try
            {
                var existingCustomer = await _unitOfWork.Customers
               .GetQueryable().Where(c => c.CustName == leadName.Trim()).FirstOrDefaultAsync();
                bool isCustomer = existingCustomer != null;

                var oldContacts = await _unitOfWork.ContactPersons.FindAsync(x => x.LeadId == leadId);
                foreach (var ct in oldContacts)
                {
                    await _unitOfWork.ContactPersons.DeleteAsync(ct.Id);
                }
                await _unitOfWork.SaveAsync();


                foreach (var contact in contacts)
                {
                    contact.Id = 0;

                    if (isCustomer)
                    {
                        contact.CustId = existingCustomer.CustId;
                    }
                    contact.LeadId = leadId;

                    await _unitOfWork.ContactPersons.CreateAsync(contact);
                }

                await _unitOfWork.SaveAsync();
                await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), Environment.MachineName, _currentUserService.IpAddress, "Contacts", $"Create/Updated Contacts: {leadName}", "");

            }
            catch (Exception ex)
            {

                await _loggingService.LogDeveloperError(ex, $"Failed to Create / Update ContactPerson: {leadName}");
            }
        }

        public async Task<bool> DeleteLeadsAsync(int DeleteLeadID)
        {
            try
            {
                var success = await _unitOfWork.Leads.DeleteAsync(DeleteLeadID);

                if (success)
                {
                    var oldContacts = await _unitOfWork.ContactPersons.FindAsync(x => x.LeadId == DeleteLeadID);
                    if (oldContacts != null)
                    {
                        foreach (var ct in oldContacts)
                        {
                            await _unitOfWork.ContactPersons.DeleteAsync(ct.Id);
                        }
                        await _unitOfWork.SaveAsync();
                    }

                    await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), Environment.MachineName, _currentUserService.IpAddress, "Contact", $"Delete Contact In Leads: {oldContacts}", "");

                    await _unitOfWork.SaveAsync();
                    await _loggingService.LogUserAction(await _currentUserService.GetUsernameAsync(), Environment.MachineName, _currentUserService.IpAddress, "Leads", $"Delete from Leads: {DeleteLeadID}", "");
                    return true;

                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Leads with ID" + $" {DeleteLeadID}");
                return false;
            }
        }

        public async Task<bool> CreateCurrencyAsync(Currency currency)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(currency.CurrName))
                    return false;

                bool isDuplicate = await _unitOfWork.Currencyis
                    .ExistsByNameAsync("CurrName", currency.CurrName, "CurrId", currency.CurrId == 0 ? null : currency.CurrId);

                if (isDuplicate)
                {
                    return false;
                }

                currency.CreatedBy = await _currentUserService.GetUsernameAsync();
                currency.CreatedDate = DateTime.Now;

                await _unitOfWork.Currencyis.CreateAsync(currency);
                await _unitOfWork.SaveAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Failed to create currency: {currency.CurrName}");
                throw;
            }
        }
    }
}
