using AutoMapper;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IInventoryService;
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
    public class HSNService :IHSNService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public HSNService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<(List<HSNMaster> hSNs, int TotalCount)>
            SearchWithDynamicFilterAsync(int pageNumber, int pageSize,
            Dictionary<string, object>? filters)
        {
            var query = _unitOfWork.HSNMasters.GetQueryable();

            if (filters != null && filters.Any())
            {
                foreach (var f in filters)
                {
                    query = DynamicWhereBuilder.ApplyFilter(query, f.Key, f.Value);
                }
            }

            var total = await query.CountAsync();

            var list = await query
                .OrderByDescending(x => x.Slno)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (list, total);
        }

        public static class DynamicWhereBuilder
        {
            public static IQueryable<HSNMaster> ApplyFilter(
                IQueryable<HSNMaster> query, string field, object value)
            {
                if (value == null) return query;

                switch (field)
                {
                    case "HSNcode":
                        return query.Where(x => x.HSNcode.Contains(value.ToString()));

                    case "HSNCategory":
                        return query.Where(x => x.HSNCategory.Contains(value.ToString()));
                }

                return query;
            }
        }


        public async Task<bool> DeleteHSNAsync(int slNo)
        {
            try
            {
                var hSNMaster = await _unitOfWork.HSNMasters.GetAsync(slNo);

                if (hSNMaster == null)
                    return false;

                await _unitOfWork.HSNMasters.DeleteAsync(hSNMaster);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogUserAction(
                    UserName: await _currentUserService.GetUsernameAsync(),
                    Machine: _currentUserService.MachineName,
                    IP_Address: _currentUserService.IpAddress,
                    screen: "HSNMaster List",
                    action: $"Deleted HSNCode: {hSNMaster.HSNcode}",
                    additionalInfo: $"HSN: {hSNMaster.HSNcode}"
                );
                return true;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, $"Error deleting Store with Id {slNo}");
                return false;
            }
        }


    }
}
