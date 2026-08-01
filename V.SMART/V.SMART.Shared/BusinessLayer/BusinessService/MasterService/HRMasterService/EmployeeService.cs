using AutoMapper;
using AutoMapper.QueryableExtensions;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IHRMasterservice;
using V.SMART.Shared.Data.Master.HumanResourceMaster_Module.V.SMART.Shared.Data.Master.HumanResourceMaster_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.HumanResourceViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.HRMasterService
{
    public class EmployeeService : IEmployeeService
    {

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public EmployeeService(IUnitOfWork unitOfWork, ILoggingService loggingService, CurrentUserService currentUserService, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<(List<StaffVM> staffVMs, int TotalCount)> SearchWithDynamicFilterAsync(
             int pageNumber,
             int pageSize,
             Dictionary<string, object>? filters)
        {
            try
            {
                IQueryable<Staff> query = _unitOfWork.Staffs
                    .GetQueryable()
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Count > 0)
                    query = DynamicWhereBuilder.ApplyFilters(query, filters);

                // Total Count
                var totalCount = await query.CountAsync();

                // Paging + Projection
                var items = await query
                    .OrderByDescending(x => x.StaffID)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ProjectTo<StaffVM>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while searching staff with dynamic filter");
                return (new List<StaffVM>(), 0);
            }
        }

        public static class DynamicWhereBuilder
        {
            public static IQueryable<Staff> ApplyFilters(
                IQueryable<Staff> query,
                Dictionary<string, object> filters)
            {
                try
                {
                    string? employeeCode = GetString(filters, "EmployeeCode");
                    string? employeeName = GetString(filters, "EmployeeName");
                    string? department = GetString(filters, "Department");
                    string? designation = GetString(filters, "Designation");
                    string? createdBy = GetString(filters, "CreatedBy");
                    string? status = GetString(filters, "Active");

                    DateTime? fromDate = GetDate(filters, "FromDate");
                    DateTime? toDate = GetDate(filters, "ToDate");

                    // Employee Code
                    if (!string.IsNullOrWhiteSpace(employeeCode))
                        query = query.Where(x => x.StaffRefId != null &&
                            EF.Functions.Like(x.StaffRefId, employeeCode + "%"));

                    // Employee Name
                    if (!string.IsNullOrWhiteSpace(employeeName))
                        query = query.Where(x => x.StaffName != null &&
                            EF.Functions.Like(x.StaffName, employeeName + "%"));

                    // Department
                    if (!string.IsNullOrWhiteSpace(department))
                        query = query.Where(x => x.Department != null &&
                            EF.Functions.Like(x.Department, department + "%"));

                    // Designation
                    if (!string.IsNullOrWhiteSpace(designation))
                        query = query.Where(x => x.Designation != null &&
                            EF.Functions.Like(x.Designation, designation + "%"));

                    // Created By
                    if (!string.IsNullOrWhiteSpace(createdBy))
                        query = query.Where(x => x.CreatedBy != null &&
                            EF.Functions.Like(x.CreatedBy, createdBy + "%"));

                    // Status
                    if (!string.IsNullOrWhiteSpace(status))
                    {
                        var normalized = status.Trim().ToLower();

                        if (normalized == "active")
                            query = query.Where(x => x.Status == "Active");

                        else if (normalized == "inactive")
                            query = query.Where(x => x.Status == "InActive");
                    }

                    // Date Range
                    if (fromDate.HasValue)
                        query = query.Where(x => x.CreatedDate >= fromDate.Value.Date);

                    if (toDate.HasValue)
                        query = query.Where(x => x.CreatedDate < toDate.Value.Date.AddDays(1));

                    return query;
                }
                catch
                {
                    // NEVER crash filtering
                    return query;
                }
            }

            // ---------------- Helper Methods ----------------

            private static string? GetString(Dictionary<string, object> filters, string key)
            {
                if (!filters.ContainsKey(key) || filters[key] == null)
                    return null;

                var value = filters[key].ToString()?.Trim();
                return string.IsNullOrWhiteSpace(value) ? null : value;
            }

            private static DateTime? GetDate(Dictionary<string, object> filters, string key)
            {
                if (!filters.ContainsKey(key) || filters[key] == null)
                    return null;

                return DateTime.TryParse(filters[key].ToString(), out var dt)
                    ? dt
                    : null;
            }
        }


        public async Task<(bool Success, string Message)> DeleteEmployeeAsync(int staffId)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                var staff = await _unitOfWork.Staffs.GetQueryable()
                    .FirstOrDefaultAsync(s => s.StaffID == staffId);

                if (staff == null)
                    return (false, "Employee not found.");

                var (canDelete, validationMsg) = await CanDeleteEmployeeAsync(staffId);
                if (!canDelete)
                    return (false, validationMsg);

                string employeeName = staff.StaffName ?? "Unknown";

                var user = await _unitOfWork.Users.GetQueryable()
                    .FirstOrDefaultAsync(u => u.StaffId == staffId);

                if (user != null)
                {
                    var rights = await _unitOfWork.UserRights.GetQueryable()
                        .Where(r => r.UserId == user.UserId)
                        .ToListAsync();

                    foreach (var r in rights)
                    {
                        await _unitOfWork.UserRights.DeleteAsync(r);
                    }
                    await _unitOfWork.SaveAsync();


                    await _loggingService.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "UserRights",
                        "Delete",
                        $"Deleted rights for user {user.UserName}"
                    );

                    await _unitOfWork.Users.DeleteAsync(user);
                    await _unitOfWork.SaveAsync();

                    await _loggingService.LogUserAction(
                        await _currentUserService.GetUsernameAsync(),
                        _currentUserService.MachineName,
                        _currentUserService.IpAddress,
                        "User",
                        "Delete",
                        $"Deleted user {user.UserName}"
                    );
                }


                staff.Status = "InActive";
                await _unitOfWork.Staffs.UpdateAsync(staff);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    await _currentUserService.GetUsernameAsync(),
                    _currentUserService.MachineName,
                    _currentUserService.IpAddress,
                    "Employee",
                    "Delete",
                    $"Deleted employee {employeeName}"
                );

                // 5️⃣ Commit
                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                return (true, "Employee deleted successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                await _loggingService.LogDeveloperError(ex, $"DeleteEmployeeAsync Failed StaffID {staffId}");

                return (false, "Error occurred while deleting employee.");
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteEmployeeAsync(int staffId)
        {
            try
            {
                var staff = await _unitOfWork.Staffs
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.StaffID == staffId);

                if (staff == null)
                    return (false, "Employee not found. Deletion not allowed.");

                bool hasUser = await _unitOfWork.Users
                    .GetQueryable()
                    .AnyAsync(u => u.StaffId == staff.StaffID);
                if (hasUser)
                    return (false, "Cannot delete this Employee because a linked User exists in User Master.");


                bool hasLeaveApp = await _unitOfWork.LeaveApplications.GetQueryable()
                    .AnyAsync(u => u.EmployeeID == staff.StaffID);
                if (hasLeaveApp)
                    return (false, "Cannot delete this Employee because Leave Applications are recorded. Please remove or archive leave records first.");


                bool hasLeaveBal = await _unitOfWork.EmployeeLeaveBalances.GetQueryable()
                    .AnyAsync(u => u.EmployeeID == staff.StaffID);
                if (hasLeaveBal)
                    return (false, "Cannot delete this Employee because Leave Balance exists. Clear the employee leave balance before deletion.");


                var status = staff.Status?.Trim();

                if (string.Equals(status, "InActive", StringComparison.OrdinalIgnoreCase))
                    return (false, "InActive employee cannot be deleted.");

                return (true, "Employee can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error in CanDeleteEmployeeAsync for StaffID: {staffId}");
                throw new Exception("Error while checking Employee delete eligibility.", ex);
            }
        }


    }
}
