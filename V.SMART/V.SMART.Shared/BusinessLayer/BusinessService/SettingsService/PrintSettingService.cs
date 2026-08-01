using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService;
using V.SMART.Shared.BusinessLayer.BusinessService.IBusinessService.ISettingsService;
using V.SMART.Shared.Data.GeneralSettingsMaster;
using V.SMART.Shared.Data.Master.MasterScreeenManagement;
using V.SMART.Shared.Repository.IRepository;
using V.SMART.Shared.Services;
using V.SMART.Shared.ViewModels;

namespace V.SMART.Shared.BusinessLayer.BusinessService.SettingsService
{
    public class PrintSettingService : IPrintSettingService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILoggingService _loggingService;
        private readonly CurrentUserService _currentUserService;
        private readonly ICommonService _commonService;
        private readonly IMapper _mapper;

        public PrintSettingService(
            IUnitOfWork unitOfWork,
            ILoggingService loggingService,
            CurrentUserService currentUserService,
            ICommonService commonService,
            IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _loggingService = loggingService;
            _currentUserService = currentUserService;
            _commonService = commonService;
            _mapper = mapper;
        }

        public async Task<List<Screens?>> GetScreenDetailsAsync()
        {
            try
            {
                var screens = await _unitOfWork.Screens.GetAllAsync();
                return screens
                    .Where(s => s.IsPrintRequired)
                    .OrderBy(s => s.ScreenName)
                    .ToList();

            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(ex, "Error while  GetScreenDetailsAsync in PrintSettingService .");
                return null;
            }
        }
        public async Task<IEnumerable<PrintSetting>> GetPrintSettingByScreenName(string screenName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(screenName))
                    return Enumerable.Empty<PrintSetting>();

                var entities = await _unitOfWork.PrintSettings
                    .GetQueryable()
                    .Where(q => q.ScreenName == screenName)
                    .ToListAsync();

                return entities;
            }
            catch (Exception ex)
            {
                await _loggingService.LogDeveloperError(
                    ex,
                    "Error while fetching Print Settings in GetPrintSettingByScreenName."
                );
                return Enumerable.Empty<PrintSetting>();
            }
        }

        public async Task<IEnumerable<PrintSetting>> UpsertPrintSettingAsync(IEnumerable<PrintSetting> settings)
        {
            var now = DateTime.Now;
            var currentUser = await _currentUserService.GetUsernameAsync();

            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                foreach (var setting in settings)
                {
                    if (setting.Id == 0)
                    {
                        setting.CreatedBy = currentUser;
                        setting.CreatedDate = now;
                        setting.CopyName.ToUpper();
                        await _unitOfWork.PrintSettings.CreateAsync(setting);
                    }
                    else
                    {
                        setting.ModifiedBy = currentUser;
                        setting.ModifiedDate = now;
                        setting.CopyName.ToUpper();
                        await _unitOfWork.PrintSettings.UpdateAsync(setting);
                    }
                }

                await _unitOfWork.SaveAsync();
                await transaction.CommitAsync();

                return settings;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                await _loggingService.LogDeveloperError(ex, "Error in UpsertPrintSettingAsync");
                throw;
            }
        }
        public async Task ToDeleteByIdAsync(int Id)
        {
            try
            {

                // Delete from DB
                await _unitOfWork.PrintSettings.DeleteAsync(Id);
                await _unitOfWork.SaveAsync();

                // Log action
                await _loggingService.LogUserAction(
                    await _currentUserService.GetUsernameAsync(),
                    _currentUserService.MachineName,
                    _currentUserService.IpAddress,
                    "Print Settings",
                    $"Deleted Id: {Id}",
                    $"Deleted Id: {Id}"
                );
            }
            catch (Exception ex)
            {

                await _loggingService.LogDeveloperError(
                    ex,
                    "Error while Deleting Print Settings in ToDeleteByIdAsync."
                );
            }
        }

        public async Task<List<ScreensVM>> GetScreenPrintSettings()
        {
            try
            {
                var usedScreens = (await _unitOfWork.PrintSettings.GetQueryable()
                                .Select(x => x.ScreenName.Trim().ToLower())
                                .ToListAsync())
                                .ToHashSet();

                return await _unitOfWork.Screens.GetQueryable()
                    .Select(x => new ScreensVM
                    {
                        Id = x.Id,
                        ScreenCode = x.ScreenCode,
                        ScreenName = x.ScreenName,
                        IsPrintRequired = x.IsPrintRequired,
                        IsLocked = usedScreens.Contains(x.ScreenName.Trim().ToLower())
                    })
                    .OrderBy(x => x.ScreenCode)
                    .ToListAsync();

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task SaveScreenPrintSettings(List<ScreensVM> list)
        {
            foreach (var item in list)
            {
                var entity = await _unitOfWork.Screens
                    .FirstOrDefaultAsync(x => x.Id == item.Id);

                if (entity != null)
                {
                    entity.IsPrintRequired = item.IsPrintRequired ? true : false;
                }
            }

            await _unitOfWork.SaveAsync();
        }

    }
}
