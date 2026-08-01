using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices;
using V.SMART.Shared.Data.Master.MasterScreeenManagement_Module;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels.MasterViewModel.GeneralViewModel;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService
{
    public class TermsAndConditionsService : ITermsAndConditionsService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILoggingService _logs;
        private readonly CurrentUserService _userService;
        private readonly ICommonService _commonService;
        private readonly ForeignKeyUsageChecker _fkChecker;

        public TermsAndConditionsService(IUnitOfWork unitOfWork, IMapper mapper, ILoggingService loggingService,
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

        public async Task<IEnumerable<TermsAndConditions>> GetAllAsync()
        {
            return await _unitOfWork.TermsAndConditions.GetAllAsync();
        }

        public async Task<TermsAndConditions> GetByIdAsync(int id)
        {
            return await _unitOfWork.TermsAndConditions.GetAsync(id);
        }

        public async Task<TermsAndConditions> CreateAsync(TermsAndConditions model)
        {
            try
            {
                model.CreatedDate = DateTime.UtcNow;
                model.CreatedBy = await _userService.GetUsernameAsync(); // TODO: Replace with CurrentUserService
                await _unitOfWork.TermsAndConditions.CreateAsync(model);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(await _userService.GetUsernameAsync(),
                    _userService.MachineName, 
                    _userService.IpAddress, "Terms and Conditions", "Created", $"Created T&C: {model.Title}");
                return model;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while creating Terms & Conditions");
                throw;
            }
        }

        public async Task<TermsAndConditions> UpdateAsync(TermsAndConditions model)
        {
            try
            {
                model.ModifiedDate = DateTime.UtcNow;
                model.ModifiedBy = "System"; // TODO: Replace with CurrentUserService
                await _unitOfWork.TermsAndConditions.UpdateAsync(model);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(await _userService.GetUsernameAsync(),
                   _userService.MachineName,
                   _userService.IpAddress, "Terms and Conditions", "Updated", $"Updated T&C: {model.Title}");
                return model;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while updating Terms & Conditions");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var obj = await _unitOfWork.TermsAndConditions.GetAsync(id);
                if (obj == null) return false;

                await _unitOfWork.TermsAndConditions.DeleteAsync(id);
                await _unitOfWork.SaveAsync();

                await _logs.LogUserAction(await _userService.GetUsernameAsync(),
                   _userService.MachineName,
                   _userService.IpAddress, "Terms and Conditions", "Deleted", $"Deleted T&C: {obj.Title}");
                return true;
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error while deleting Terms & Conditions");
                throw;
            }
        }


        public async Task<(List<TermsAndConditionsVM> termsAndConditionsVMs, int TotalCount)> SearchWithDynamicFilterAsync(int pageNumber, int pageSize, Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.TermsAndConditions
                    .GetQueryable()
                    .AsNoTracking();

                // Apply Filters
                if (filters != null && filters.Any())
                {
                    foreach (var filter in filters)
                    {
                        query = TeremsAndConditionsFilterBuilder
                            .ApplyFilter(query, filter.Key, filter.Value);
                    }
                }

                var totalCount = await query.CountAsync();

                var list = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var vmList = _mapper.Map<List<TermsAndConditionsVM>>(list);

                return (vmList, totalCount);
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync (Terms And Conditions)");
                throw new InvalidOperationException("Failed to load Machine list.", ex);
            }
        }

        public static class TeremsAndConditionsFilterBuilder
        {
            public static IQueryable<TermsAndConditions> ApplyFilter(
                IQueryable<TermsAndConditions> query,
                string field,
                object value)
            {
                if (value == null) return query;

                var val = value.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(val))
                    return query;

                switch (field)
                {
                    case "Title":
                        return query.Where(x =>
                            x.Title != null &&
                            EF.Functions.Like(x.Title, $"%{val}%"));

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

                    default:
                        return query;
                }
            }
        }


        public async Task<(bool CanDelete, string Message)> CanDeleteTermsAsync(int termsId)
        {
            try
            {
                var terms = await _unitOfWork.TermsAndConditions
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == termsId);

                if (terms == null)
                    return (false, "Terms And Conditions not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<TermsAndConditions>(termsId);

                if (usedIn != null)
                    return (false, $"Cannot delete Terms And Conditions '{terms.Title}' because it is used in {usedIn} Screen.");

                if (!terms.IsActive)
                    return (false, $"Terms And Conditions '{terms.Title}' is already marked as inactive and cannot be deleted again.");

                return (true, $"Terms And Conditions '{terms.Title}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _logs.LogDeveloperError(ex, $"Terms And Conditions delete validation failed: {termsId}");
                return (false, "Unexpected error occurred while validating Terms And Conditions.");
            }
        }

        public async Task<bool> DeleteTermsAndConditionsByTermsIdAsync(int id)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                var termsAndConditions = await _unitOfWork.TermsAndConditions
                    .GetQueryable()
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (termsAndConditions == null)
                {
                    return false;
                }

                var changes = new StringBuilder();

                // Delete the Machine
                await _unitOfWork.TermsAndConditions.DeleteAsync(termsAndConditions);

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                // Log the user action
                await _logs.LogUserAction(
                    UserName: await _userService.GetUsernameAsync(),
                    Machine: _userService.MachineName,
                    IP_Address: _userService.IpAddress,
                    screen: "Terms And Conditions List",
                    action: $"Deleted Terms: {termsAndConditions.Title}",
                    additionalInfo: $"Terms Id: {termsAndConditions.Id}\n{changes}"
                );

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _logs.LogDeveloperError(ex, $"Failed to delete Terms And Conditions: {id}");
                throw;
            }
        }











    }
}
