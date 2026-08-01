using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IInventoryService;
using V.SMART.Shared.Data.Master.Inventory;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.InventoryService
{
    public class FactorService : IFactorservice
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ForeignKeyUsageChecker _fkChecker;
        public FactorService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            IMapper mapper,
            ForeignKeyUsageChecker fkChecker)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
            _fkChecker = fkChecker;
        }

        public async Task<(List<Factor> factors, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters)
        {
            try
            {
                var query = _unitOfWork.Factors.GetQueryable();

                if (filters != null)
                {
                    foreach (var f in filters)
                    {
                        query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                    }
                }

                var total = await query.CountAsync();

                var entities = await query
                    .OrderByDescending(x => x.Id)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (entities, total);
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error in SearchWithDynamicFilterAsync");
                throw;
            }
        }


        public static class DynamicWhereBuilder
        {
            public static IQueryable<Factor> ApplyFilter(
                IQueryable<Factor> query, string field, object value)
            {
                if (value == null) return query;

                switch (field)
                {
                    case "FactorName":
                        return query.Where(x => x.Factors.Contains(value.ToString()));

                    case "CreatedBy":
                        return query.Where(x => x.CreatedBy.Contains(value.ToString()));

                    case "FromDate":
                        return query.Where(x => x.CreatedDate >= DateTime.Parse(value.ToString()));

                    case "ToDate":
                        return query.Where(x => x.CreatedDate <= DateTime.Parse(value.ToString()));
                }

                return query;
            }
        }

        public async Task<(bool CanDelete, string Message)> CanDeleteFactorsAsync(int factorId)
        {
            try
            {
                var factor = await _unitOfWork.Factors
                    .GetQueryable()
                    .FirstOrDefaultAsync(s => s.Id == factorId);

                if (factor == null)
                    return (false, "Factor not found or already removed.");

                var usedIn = await _fkChecker.GetUsageTableAsync<Factor>(factorId);

                if (usedIn != null)
                    return (false, $"Cannot delete Factor '{factor.Factors}' because it is used in {usedIn}.");

                return (true, $"Factors '{factor.Factors}' can be safely deleted.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Factors delete validation failed: {factorId}");
                return (false, "Unexpected error occurred while validating Factors.");
            }
        }

        public async Task<bool> DeleteFactorAsync(int factorId)
        {
            try
            {
                var factors = await _unitOfWork.Factors.GetAsync(factorId);

                if (factors == null)
                    return false;

                await _unitOfWork.Factors.DeleteAsync(factors);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "Factors List",
                    action: $"Deleted Factors: {factors.Factors}",
                    additionalInfo: $"Process Name: {factors.Factors}"
                );
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Factors with Id {factorId}");
                return false;
            }
        }







    }
}
