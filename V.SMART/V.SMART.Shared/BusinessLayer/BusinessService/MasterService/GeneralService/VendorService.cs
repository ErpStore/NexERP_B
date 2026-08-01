using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IGeneralService;
using V.SMART.Shared.Data.Master.General;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.GeneralService
{
    public class VendorService : IVendorService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public VendorService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
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

        public async Task<(List<VendorVM> vendorVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Vendors
                    .GetQueryable()
                    .Include(x => x.Currency)
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = VenodrFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.VendorCode)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<VendorVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Vendor)");
                throw new InvalidOperationException("Failed to load Vendor list.", ex);
            }
        }

        public static class VenodrFilterBuilder
        {
            public static IQueryable<Vendor> ApplyFilter(
                IQueryable<Vendor> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "Vendor":
                        return query.Where(x =>
                            x.VendorName != null &&
                            EF.Functions.Like(x.VendorName, $"%{val}%"));

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

            private static IQueryable<Vendor> ApplyStatusFilter(
                IQueryable<Vendor> query,
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

        public async Task<(bool CanDelete, string Message)> CanDeleteVendorAsync(int vendorId)
        {
            try
            {
                var vendor = await _unitOfWork.Vendors
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.VendorCode == vendorId);

                if (vendor == null)
                    return (false, "Vendor not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Vendor>(vendorId);

                if (usedIn != null)
                    return (false, $"Cannot delete Vendor '{vendor.VendorName}' because it is used in {usedIn} Screen.");

                if (vendor.Inactive)
                    return (false, "Vendor is inactive. You cannot delete this record.");

                return (true, $"Vendor '{vendor.VendorCode}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Customer delete validation failed: {vendorId}");
                return (false, "Unexpected error occurred while validating Vendor.");
            }
        }

        public async Task<bool> DeleteVendorByVendorIdAsync(int vendorId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Get the customer with its sub-items
                var vendor = await _unitOfWork.Vendors
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.VendorCode == vendorId);

                if (vendor == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the customer
                await _unitOfWork.Vendors.DeleteAsync(vendor);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Venor List",
                    action: $"Deleted Vendor Name: {vendor.VendorName}",
                    additionalInfo: $"Vendor Id: {vendor.VendorCode}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Vendor: {vendorId}");
                throw;
            }
        }



        public async Task<List<VendorVM>> GetVendorListAsync(string topic, DateTime fromDate, DateTime toDate)
        {
            try
            {
                IQueryable<Vendor> query;

                // ================= PURCHASE + SUBCONTRACT =================
                if (topic == "Purchase")
                {
                    query =
                    (
                        from v in _unitOfWork.Vendors.GetQueryable().AsNoTracking()
                        join pi in _unitOfWork.PurchaseInvoices.GetQueryable().AsNoTracking()
                            on v.VendorCode equals pi.VendorCode
                        where pi.InvDate >= fromDate
                              && pi.InvDate < toDate.AddDays(1)
                        select v
                    )
                    .Union
                    (
                        from v in _unitOfWork.Vendors.GetQueryable().AsNoTracking()
                        join si in _unitOfWork.SubConInvs.GetQueryable().AsNoTracking()
                            on v.VendorCode equals si.VendorCode
                        where si.InvDate >= fromDate
                              && si.InvDate < toDate.AddDays(1)
                        select v
                    );
                }
                else
                {
                    return new List<VendorVM>();
                }

                var vendors = await query
                    .Distinct()
                    .OrderBy(x => x.VendorName)
                    .ToListAsync();

                return _mapper.Map<List<VendorVM>>(vendors);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error loading vendor list");
                throw;
            }
        }


    }
}

