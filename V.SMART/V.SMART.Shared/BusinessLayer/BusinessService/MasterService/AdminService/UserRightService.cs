using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.IMasterServices.IAdminService;
using V.SMART.Shared.Data.Master.Admin;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace V.SMART.Shared.BusinessLayer.BusinessService.MasterService.AdminService
{
    public class UserRightService : IUserRightService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;

        public UserRightService(IUnitOfWork unitOfWork, ILoggingService loggingService, 
                                CurrentUserService currentUserService,ICommonService commonService)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
        }


        public async Task SyncRightsForUserAsync(int userId)
        {
            try
            {

                if (!await _unitOfWork.Users.AnyAsync(x => x.UserId == userId))
                {
                    await _loggingService.LogDeveloperInfo($"[UserRights] User not found: {userId}");
                    return;
                }

                var screens = await _unitOfWork.Screens.GetQueryable()
                    .Select(s => new { s.Id, s.ScreenCode })
                    .ToListAsync();

                var existing = await _unitOfWork.UserRights.GetQueryable()
                    .Where(x => x.UserId == userId)
                    .Select(x => x.ScreenId)
                    .ToListAsync();

                var missing = screens
                    .Where(s => !existing.Contains(s.Id))
                    .ToList();

                if (!missing.Any())
                {
                    await _loggingService.LogDeveloperInfo($"[UserRights] No missing rights for UserId: {userId}");
                    return;
                }

                var rights = missing.Select(s => new UserRight
                {
                    UserId = userId,
                    ScreenId = s.Id,

                    CanView = true,
                    CanCreate = true,
                    CanEdit = true,
                    CanDelete = true,
                    IsHide = false,

                    CreatedDate = DateTime.Now,
                    CreatedBy = "System"
                }).ToList();

                await _unitOfWork.UserRights.CreateRangeAsync(rights);
                await _unitOfWork.SaveAsync();

                await _loggingService.LogDeveloperInfo($"[UserRights] {rights.Count} rights created for UserId: {userId}");
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex,$"[UserRights] Error while syncing rights for UserId: {userId}");
                throw;
            }
        }




    }
}
