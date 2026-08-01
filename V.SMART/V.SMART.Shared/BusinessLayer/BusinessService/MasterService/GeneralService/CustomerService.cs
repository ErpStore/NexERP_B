using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using V.SMART.Shared.ViewModels.MfgAndlabourViewModel.QuotationVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MudBlazor.Icons;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.GeneralService
{
    public class CustomerService : ICustomerService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public CustomerService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
                            CurrentUserService userService, ICommonService commonService,
                            ForeignKeyUsageChecker foreignKeyUsageChecker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logs = loggingService;
            _userService = userService;
            _commonService = commonService;
            _fkChecker = foreignKeyUsageChecker;
        }


        public async Task<(List<CustomerVM> customerVMs, int TotalCount)>SearchWithDynamicFilterAsync(int pageNumber,int pageSize,Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Customers
                    .GetQueryable()
                    .Include(x => x.Currency)
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = CustomerFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.CustId)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<CustomerVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex,"Error in SearchWithDynamicFilterAsync (Customer)");
                throw new InvalidOperationException("Failed to load customer list.", ex);
            }
        }

        public static class CustomerFilterBuilder
        {
            public static IQueryable<Customer> ApplyFilter(
                IQueryable<Customer> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "Customer":
                        return query.Where(x =>
                            x.CustName != null &&
                            EF.Functions.Like(x.CustName, $"%{val}%"));

                    case "CreatedBy":
                        return query.Where(x =>
                            x.CreatedBy != null &&
                            EF.Functions.Like(x.CreatedBy, $"%{val}%"));

                    case "FromDate":
                        if (DateTime.TryParse(val, out var fromDate))
                            return query.Where(x =>
                                x.CreatedDate >= fromDate.Date);
                        return query;

                    case "ToDate":
                        if (DateTime.TryParse(val, out var toDate))
                            return query.Where(x =>
                                x.CreatedDate <= toDate.Date
                                    .AddDays(1)
                                    .AddTicks(-1));
                        return query;

                    case "Status":
                        return ApplyStatusFilter(query, val);

                    default:
                        return query;
                }
            }

            private static IQueryable<Customer> ApplyStatusFilter(
                IQueryable<Customer> query,
                string status)
            {
                return status switch
                {
                    "Active" => query.Where(x => !x.Inactive),
                    "In Active" => query.Where(x => x.Inactive),
                    _ => query
                };
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteCustomerAsync(int custId)
        {
            try
            {
                var customer = await _unitOfWork.Customers
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.CustId == custId);

                if (customer == null)
                    return (false, "Customer not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Customer>(custId);

                if (usedIn != null)
                    return (false, $"Cannot delete Customer '{customer.CustName}' because it is used in {usedIn} Screen.");

                if (customer.Inactive)
                    return (false, "Customer is inactive. You cannot delete this record.");

                return (true, $"Customer '{customer.CustId}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Customer delete validation failed: {custId}");
                return (false, "Unexpected error occurred while validating Customer.");
            }
        }


        public async Task<bool> DeleteCustomerByCustIdAsync(int custId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the customer with its sub-items
                var customer = await _unitOfWork.Customers
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.CustId == custId);

                if (customer == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the customer
                await _unitOfWork.Customers.DeleteAsync(customer);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Customer List",
                    action: $"Deleted Customer: {customer.CustName}",
                    additionalInfo: $"Customer Id: {customer.CustId}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Customer: {custId}");
                throw;
            }
        }


        public async Task<List<CustomerVM>> GetCustomerListAsync(string topic, DateTime fromDate, DateTime toDate)
        {
            try
            {
                IQueryable<Customer> query;

                // ================= SALES + LABOUR =================
                if (topic == "Sales")
                {
                    query =
                    (
                        from c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                        join si in _unitOfWork.MfgInvs.GetQueryable().AsNoTracking()
                            on c.CustId equals si.CustId
                        where si.InvDate >= fromDate
                              && si.InvDate < toDate.AddDays(1)
                        select c
                    )
                    .Union
                    (
                        from c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                        join li in _unitOfWork.LabInvs.GetQueryable().AsNoTracking()
                            on c.CustId equals li.CustId
                        where li.LabInvDate >= fromDate
                              && li.LabInvDate < toDate.AddDays(1)
                        select c
                    );
                }

                // ================= EXPORTS =================
                else if (topic == "Exports")
                {
                    query =
                    (
                        from c in _unitOfWork.Customers.GetQueryable().AsNoTracking()
                        join ei in _unitOfWork.ExpInvs.GetQueryable().AsNoTracking()
                            on c.CustId equals ei.CustId
                        where ei.ExpInvDate >= fromDate
                              && ei.ExpInvDate < toDate.AddDays(1)
                        select c
                    );
                }
                else
                {
                    return new List<CustomerVM>();
                }

                var customers = await query
                    .Distinct()
                    .OrderBy(x => x.CustName)
                    .ToListAsync();

                return _mapper.Map<List<CustomerVM>>(customers);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error loading customer list");
                throw;
            }
        }



    }
}
